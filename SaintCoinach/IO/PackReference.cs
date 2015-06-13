using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaintCoinach.IO {
    public sealed class PackReference<T> where T : class {
        #region Fields
        private readonly WeakReference<T> _WeakRef;
        private T _StrongRef;
        private readonly Pack _Pack;
        #endregion

        #region Constructor
        public PackReference(Pack pack, T file) {
            _WeakRef = new WeakReference<T>(file);
            _Pack = pack;
            if (pack.Optimize)
                _StrongRef = file;
        }
        #endregion

        #region TryGet
        public bool TryGetTarget(out T target) {
            var useStrong = _Pack.Optimize;

            if (_StrongRef != null) {
                target = _StrongRef;
                if (!useStrong)
                    _StrongRef = null;

                return true;
            }

            var result = _WeakRef.TryGetTarget(out target);
            if (result && useStrong)
                _StrongRef = target;
            return result;
        }
        public void SetTarget(T target) {
            _WeakRef.SetTarget(target);
            if (_Pack.Optimize)
                _StrongRef = target;
            else
                _StrongRef = null;
        }
        #endregion
    }
}
