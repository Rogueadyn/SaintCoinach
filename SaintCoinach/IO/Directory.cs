using System;
using System.Collections.Generic;

namespace SaintCoinach.IO {
    /// <summary>
    ///     Directory inside a SqPack.
    /// </summary>
    public partial class Directory : IPackSource {
        #region Fields

        private readonly Dictionary<string, uint> _FileNameMap = new Dictionary<string, uint>();
        private readonly Dictionary<uint, PackReference<File>> _Files = new Dictionary<uint, PackReference<File>>();
        private string _Path;

        #endregion

        #region Properties

        public Pack Pack { get; private set; }
        public IndexDirectory Index { get; private set; }

        public string Path {
            get { return _Path ?? string.Join("/", Pack.ToString(), Index.Key.ToString("X8")); }
            internal set { _Path = value; }
        }

        #endregion

        #region Constructors

        #region Constructor

        public Directory(Pack pack, IndexDirectory index) {
            Pack = pack;
            Index = index;
        }

        #endregion

        #endregion

        public override string ToString() {
            return Path;
        }

        #region Get

        public bool FileExists(string name) {
            uint hash;
            if (Pack.Optimize) {
                if (!_FileNameMap.TryGetValue(name, out hash))
                    _FileNameMap.Add(name, hash = Hash.Compute(name));
            } else
                hash = Hash.Compute(name);
            return FileExists(hash);
        }

        public bool FileExists(uint fileKey) {
            return Index.Files.ContainsKey(fileKey);
        }

        public File GetFile(string name) {
            uint hash;
            if (Pack.Optimize) {
                if (!_FileNameMap.TryGetValue(name, out hash))
                    _FileNameMap.Add(name, hash = Hash.Compute(name));
            } else
                hash = Hash.Compute(name);

            var file = GetFile(hash);
            file.Path = string.Format("{0}/{1}", this.Path, name);
            return file;
        }

        public File GetFile(uint key) {
            PackReference<File> fileRef;
            File file;
            var hasRef = _Files.TryGetValue(key, out fileRef);
            if (hasRef && fileRef.TryGetTarget(out file))
                return file;

            var index = Index.Files[key];
            file = FileFactory.Get(this.Pack, index);
            if (hasRef)
                fileRef.SetTarget(file);
            else
                _Files.Add(key, new PackReference<File>(Pack, file));
            return file;
        }

        public bool TryGetFile(string name, out File file) {
            uint hash;
            if (Pack.Optimize) {
                if (!_FileNameMap.TryGetValue(name, out hash))
                    _FileNameMap.Add(name, hash = Hash.Compute(name));
            } else
                hash = Hash.Compute(name);

            var result = TryGetFile(hash, out file);
            if (result)
                file.Path = string.Format("{0}/{1}", this.Path, name);
            return result;
        }

        public bool TryGetFile(uint key, out File file) {
            PackReference<File> fileRef;
            var hasRef = _Files.TryGetValue(key, out fileRef);
            if (hasRef && fileRef.TryGetTarget(out file))
                return true;

            IndexFile index;
            if (Index.Files.TryGetValue(key, out index)) {
                file = FileFactory.Get(this.Pack, index);
                if (hasRef)
                    fileRef.SetTarget(file);
                else
                    _Files.Add(key, new PackReference<File>(Pack, file));
                return true;
            }

            file = null;
            return false;
        }

        #endregion
    }
}
