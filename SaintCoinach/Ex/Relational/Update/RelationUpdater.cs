using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using SaintCoinach.Ex.Relational.Definition;
using SaintCoinach.Ex.Relational.Update.Changes;
using SaintCoinach.IO;

namespace SaintCoinach.Ex.Relational.Update {
    public class RelationUpdater {
        #region Static

        private const Language UsedLanguage = Language.Japanese;

        #endregion

        #region Fields

        private readonly RelationalExCollection _Previous;
        private readonly IProgress<UpdateProgress> _Progress;
        private readonly RelationalExCollection _Updated;

        #endregion

        #region Properties

        public RelationDefinition Previous { get; private set; }
        public RelationDefinition Updated { get; private set; }

        #endregion

        #region Constructors

        public RelationUpdater(PackCollection previousPacks,
                               RelationDefinition previousDefinition,
                               PackCollection updatedPacks,
                               string updatedVersion,
                               IProgress<UpdateProgress> progress) {
            var optimize = (IntPtr.Size > 4);   // Not enough memory to keep everything cached when running on 32-bit

            _Progress = progress ?? new NullProgress();

            _Previous = new RelationalExCollection(previousPacks);
            _Previous.Optimize = optimize;
            Previous = previousDefinition;

            _Updated = new RelationalExCollection(updatedPacks);
            _Updated.Optimize = optimize;
            Updated = new RelationDefinition {
                Version = updatedVersion
            };

            _Previous.ActiveLanguage = UsedLanguage;
            _Updated.ActiveLanguage = UsedLanguage;
        }

        #endregion

        #region Update

        private readonly ReaderWriterLockSlim _ProgressStatusLock = new ReaderWriterLockSlim();
        private UpdateProgress _ProgressStatus;
        private readonly ReaderWriterLockSlim _ChangeLogLock = new ReaderWriterLockSlim();
        private List<IChange> _ChangeLog;

        void UpdateSheet(SheetUpdater updater) {
            var name = updater.PreviousDefinition.Name;
            ReportFileStart(name);

            var changes = updater.Update();

            _ChangeLogLock.EnterWriteLock();
            try {
                _ChangeLog.AddRange(changes);
            } finally {
                _ChangeLogLock.ExitWriteLock();
            }

            ReportFileEnd(name);

            GC.Collect();
        }
        void CompareSheet(SheetComparer comparer) {
            var name = comparer.PreviousDefinition.Name;
            ReportFileStart(name);

            var changes = comparer.Compare();

            _ChangeLogLock.EnterWriteLock();
            try {
                _ChangeLog.AddRange(changes);
            } finally {
                _ChangeLogLock.ExitWriteLock();
            }

            ReportFileEnd(name);

            GC.Collect();
        }
        public IEnumerable<IChange> Update(bool detectDataChanges, int threadCount = -1) {
            if (threadCount <= 0)
                threadCount = Math.Max(1, Environment.ProcessorCount - 1);

            _ChangeLog = new List<IChange>();
            _ProgressStatus = new UpdateProgress {
                CurrentOperation = "Structure",
                CurrentStep = 0,
                CurrentFiles = new List<string>(),
                TotalSteps = (detectDataChanges ? 2 : 1) * Previous.SheetDefinitions.Count
            };
            {
                var processor = new Processor<SheetUpdater>(UpdateSheet);

                foreach (var prevSheetDef in Previous.SheetDefinitions) {
                    if (!_Updated.SheetExists(prevSheetDef.Name)) {
                        _ChangeLog.Add(new SheetRemoved(prevSheetDef.Name));
                        continue;
                    }

                    var prevSheet = _Previous.GetSheet(prevSheetDef.Name);

                    var updatedSheet = _Updated.GetSheet(prevSheetDef.Name);
                    var updatedSheetDef = Updated.GetOrCreateSheet(prevSheetDef.Name);

                    var sheetUpdater = new SheetUpdater(prevSheet, prevSheetDef, updatedSheet, updatedSheetDef);

                    processor.Items.Add(sheetUpdater);
                }
                
                UpdateProgress last = new UpdateProgress(_ProgressStatus);
                _Progress.Report(last);

                processor.Start(threadCount);
                while (processor.IsAlive) {
                    UpdateProgress copy;
                    _ProgressStatusLock.EnterReadLock();
                    try {
                        copy = new UpdateProgress(_ProgressStatus);
                    } finally {
                        _ProgressStatusLock.ExitReadLock();
                    }
                    if (copy.CurrentStep != last.CurrentStep || copy.CurrentFiles.Any(f => !last.CurrentFiles.Contains(f)) || last.CurrentFiles.Any(f => !copy.CurrentFiles.Contains(f)))
                        _Progress.Report(copy);
                    last = copy;

                    Thread.Sleep(100);
                }

                Updated.Compile();
            }

            if (detectDataChanges) {
                _ProgressStatus.CurrentOperation = "Data";

                var processor = new Processor<SheetComparer>(CompareSheet);
                
                foreach (var prevSheetDef in Previous.SheetDefinitions) {
                    if (!_Updated.SheetExists(prevSheetDef.Name)) {
                        _ChangeLog.Add(new SheetRemoved(prevSheetDef.Name));
                        continue;
                    }

                    var prevSheet = _Previous.GetSheet(prevSheetDef.Name);

                    var updatedSheet = _Updated.GetSheet(prevSheetDef.Name);
                    var updatedSheetDef = Updated.GetOrCreateSheet(prevSheetDef.Name);

                    var sheetComparer = new SheetComparer(prevSheet, prevSheetDef, updatedSheet, updatedSheetDef);

                    processor.Items.Add(sheetComparer);
                }

                UpdateProgress last = new UpdateProgress(_ProgressStatus);
                _Progress.Report(last);

                processor.Start(threadCount);
                while (processor.IsAlive) {
                    UpdateProgress copy;
                    _ProgressStatusLock.EnterReadLock();
                    try {
                        copy = new UpdateProgress(_ProgressStatus);
                    } finally {
                        _ProgressStatusLock.ExitReadLock();
                    }
                    if (copy.CurrentStep != last.CurrentStep || copy.CurrentFiles.Any(f => !last.CurrentFiles.Contains(f)) || last.CurrentFiles.Any(f => !copy.CurrentFiles.Contains(f)))
                        _Progress.Report(copy);
                    last = copy;

                    Thread.Sleep(100);
                }
            }

            _ProgressStatus.CurrentStep = _ProgressStatus.TotalSteps;
            _ProgressStatus.CurrentOperation = "Finished";
            _ProgressStatus.CurrentFiles.Clear();
            _Progress.Report(_ProgressStatus);

            return _ChangeLog;
        }
        internal void ReportFileStart(string file) {
            _ProgressStatusLock.EnterWriteLock();
            try {
                _ProgressStatus.CurrentFiles.Add(file);
            } finally {
                _ProgressStatusLock.ExitWriteLock();
            }
        }
        internal void ReportFileEnd(string file) {
            _ProgressStatusLock.EnterWriteLock();
            try {
                _ProgressStatus.CurrentFiles.Remove(file);
                ++_ProgressStatus.CurrentStep;
            } finally {
                _ProgressStatusLock.ExitWriteLock();
            }
        }

        #endregion

        class Processor<T> {
            #region Fields & Properties
            public bool IsAlive { get { return _Threads.Any(t => t.IsAlive); } }
            public readonly List<T> Items = new List<T>();

            private readonly Action<T> Action;
            private int _NextIndex = 0;
            private ProcessorThread[] _Threads;
            #endregion

            public Processor(Action<T> action) {
                Action = action;
            }

            bool GetNext(out T value) {
                value = default(T);
                lock(Items) {
                    if (_NextIndex >= Items.Count)
                        return false;
                    value = Items[_NextIndex++];
                }
                return true;
            }

            public void Start(int threadCount) {
                _Threads = new ProcessorThread[threadCount];
                for (var i = 0; i < threadCount; ++i) {
                    _Threads[i] = new ProcessorThread(this);
                    _Threads[i].Start();
                }
            }

            class ProcessorThread {
                public volatile bool IsAlive = false;
                readonly Processor<T> _Owner;

                public ProcessorThread(Processor<T> owner) {
                    _Owner = owner;
                }

                public void Start() {
                    IsAlive = true;
                    var t = new Thread(DoWork);
                    t.Name = "Updater thread";
                    t.IsBackground = true;
                    t.Start();
                }

                void DoWork() {
                    try {
                        T next;
                        while (_Owner.GetNext(out next))
                            _Owner.Action(next);
                    } finally {
                        IsAlive = false;
                    }
                }
            }
        }

        private class NullProgress : IProgress<UpdateProgress> {
            #region IProgress<UpdateProgress> Members

            public void Report(UpdateProgress value) { }

            #endregion
        }
    }
}
