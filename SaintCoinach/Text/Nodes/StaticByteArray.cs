using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaintCoinach.Text.Nodes {
    public class StaticByteArray : INode, IStaticNode {
        private readonly byte[] _Value;

        TagType INode.Tag { get { return TagType.None; } }
        NodeFlags INode.Flags { get { return NodeFlags.IsStatic; } }
        public byte[] Value { get { return _Value; } }

        public StaticByteArray(byte[] value) {
            _Value = value.ToArray();
        }

        public bool Equals(INode other) {
            var n = other as StaticByteArray;
            if (n == null)
                return false;
            if (_Value.Length != n._Value.Length)
                return false;

            for(var i = 0; i < _Value.Length; ++i) {
                if (_Value[i] != n._Value[i])
                    return false;
            }
            return true;
        }
        public int CompareTo(INode other) {
            var n = other as StaticByteArray;
            if (n == null)
                return 1;

            if (_Value.Length != n._Value.Length)
                return _Value.Length.CompareTo(n._Value.Length);

            for(var i = 0; i < _Value.Length; ++i) {
                var cmp = _Value[i].CompareTo(n._Value[i]);
                if (cmp != 0)
                    return cmp;
            }

            return 0;
        }

        public override string ToString() {
            var sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }
        public void ToString(StringBuilder builder) {
            for (var i = 0; i < Value.Length; ++i)
                builder.Append(Value[i].ToString("X2"));
        }

        #region IDecodeNodeStatic Members

        object IStaticNode.Value {
            get { return ToString(); }
        }

        #endregion
    }
}
