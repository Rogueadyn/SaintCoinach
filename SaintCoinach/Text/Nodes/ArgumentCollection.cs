using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaintCoinach.Text.Nodes {
    public class ArgumentCollection : IEnumerable<INode> {
        private readonly INode[] _Items;

        public bool HasItems { get { return _Items != null && _Items.Length > 0; } }
        public IEnumerable<INode> Items { get { return _Items; } }

        public ArgumentCollection(IEnumerable<INode> items) {
            if (items != null)
                _Items = items.ToArray();
            else
                _Items = new INode[0];
        }

        public override string ToString() {
            var sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }
        public void ToString(StringBuilder builder) {
            if (HasItems) {
                builder.Append(StringTokens.ArgumentsOpen);
                for (var i = 0; i < _Items.Length; ++i) {
                    if (i > 0)
                        builder.Append(StringTokens.ArgumentsSeperator);
                    _Items[i].ToString(builder);
                }
                builder.Append(StringTokens.ArgumentsClose);
            }
        }

        public bool Equals(ArgumentCollection other) {
            if (other == null)
                return false;
            if (other._Items.Length != _Items.Length)
                return false;
            for(var i = 0; i < _Items.Length; ++i) {
                var l = _Items[i];
                var r = other._Items[i];
                if (!l.Equals(r))
                    return false;
            }
            return true;
        }
        public int CompareTo(ArgumentCollection other) {
            if (other == null)
                return 1;
            if (_Items.Length > other._Items.Length)
                return -1;
            if (_Items.Length < other._Items.Length)
                return 1;

            for(var i = 0; i < _Items.Length; ++i) {
                var l = _Items[i];
                var r = other._Items[i];
                var cmp = l.CompareTo(r);
                if (cmp != 0)
                    return cmp;
            }
            return 0;
        }

        #region IEnumerable<IXivStringPart> Members

        public IEnumerator<INode> GetEnumerator() {
            return Items.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion
    }
}
