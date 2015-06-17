using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaintCoinach.Ex {
    public sealed class ExReference<T> where T : class {
        #region Fields
        private WeakReference<T> _WeakRef;
        private T _StrongRef;
        private bool _IsNull;
        private readonly ExCollection _Collection;
        #endregion

        #region Constructor
        public ExReference(ExCollection collection, T file) {
            _IsNull = (file == null);
            _Collection = collection;
            if (collection._Optimize)
                _StrongRef = file;
            else if(!_IsNull)
                _WeakRef = new WeakReference<T>(file);
        }
        #endregion

        #region TryGet
        public bool TryGetTarget(out T target) {
            if(_IsNull) {
                target = null;
                return true;
            }

            var useStrong = _Collection._Optimize;

            if (_StrongRef != null) {
                target = _StrongRef;
                if (!useStrong) {
                    _WeakRef = new WeakReference<T>(_StrongRef);
                    _StrongRef = null;
                }

                return true;
            }

            var result = _WeakRef.TryGetTarget(out target);
            if (result && useStrong) {
                _StrongRef = target;
                _WeakRef = null;
            }
            return result;
        }
        public void SetTarget(T target) {
            _IsNull = (target == null);

            if (_Collection._Optimize) {
                _StrongRef = target;
                _WeakRef = null;
            } else {
                _StrongRef = null;
                if (_IsNull)
                    _WeakRef = null;
                else {
                    if (_WeakRef == null)
                        _WeakRef = new WeakReference<T>(target);
                    else
                        _WeakRef.SetTarget(target);
                }
            }
        }
        #endregion
    }
}
