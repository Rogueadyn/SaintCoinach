using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaintCoinach.IO {
    public partial class Index2Source : IPackSource {
        #region Fields

        private readonly Dictionary<uint, PackReference<File>> _Files =
            new Dictionary<uint, PackReference<File>>();

        private readonly Dictionary<string, uint> _FilePathMap = new Dictionary<string, uint>();

        private readonly Pack _Pack;

        private readonly Index2 _Index;

        #endregion

        #region Properties
        public Index2 Index { get { return _Index; } }
        public Pack Pack { get { return _Pack; } }
        #endregion

        #region Constructor
        internal Index2Source(Pack pack, Index2 index) {
            _Pack = pack;
            _Index = index;
        }
        #endregion

        #region IPackSource Members

        public bool FileExists(string path) {
            uint hash;
            if (Pack.Optimize) {
                if (!_FilePathMap.TryGetValue(path, out hash))
                    _FilePathMap.Add(path, hash = Hash.Compute(path));
            } else
                hash = Hash.Compute(path);
            return FileExists(hash);
        }

        public bool FileExists(uint hash) {
            return Index.Files.ContainsKey(hash);
        }

        public bool TryGetFile(string path, out File value) {
            uint hash;
            if (Pack.Optimize) {
                if (!_FilePathMap.TryGetValue(path, out hash))
                    _FilePathMap.Add(path, hash = Hash.Compute(path));
            } else
                hash = Hash.Compute(path);
            var result = TryGetFile(hash, out value);
            if (result)
                value.Path = path;
            return result;
        }

        public bool TryGetFile(uint hash, out File value) {
            PackReference<File> fileRef;
            var hasRef = _Files.TryGetValue(hash, out fileRef);
            if (hasRef && fileRef.TryGetTarget(out value))
                return true;

            Index2File index;
            if (Index.Files.TryGetValue(hash, out index)) {
                value = FileFactory.Get(Pack, index);
                if (hasRef)
                    fileRef.SetTarget(value);
                else
                    _Files.Add(hash, new PackReference<File>(Pack, value));
                return true;
            }

            value = null;
            return false;
        }

        public File GetFile(string path) {
            uint hash;
            if (Pack.Optimize) {
                if (!_FilePathMap.TryGetValue(path, out hash))
                    _FilePathMap.Add(path, hash = Hash.Compute(path));
            } else
                hash = Hash.Compute(path);
            var f = GetFile(hash);
            f.Path = path;
            return f;
        }

        public File GetFile(uint hash) {
            File file;
            PackReference<File> fileRef;
            var hasRef = _Files.TryGetValue(hash, out fileRef);
            if (hasRef && fileRef.TryGetTarget(out file))
                return file;

            var index = Index.Files[hash];
            file = FileFactory.Get(Pack, index);
            if (hasRef)
                fileRef.SetTarget(file);
            else
                _Files.Add(hash, new PackReference<File>(Pack, file));
            return file;
        }

        #endregion
    }
}
