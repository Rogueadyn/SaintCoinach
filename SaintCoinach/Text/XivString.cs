using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaintCoinach.Text {
    using Nodes;

    public class XivString : INode, IExpressionNode, INodeWithChildren, IComparable, IComparable<XivString>, IComparable<string>, IEquatable<XivString>, IEquatable<string> {
        public static readonly XivString Empty = new XivString(new INode[0]);

        private readonly INode[] _Children;
        private bool? _IsStatic;
        private string _String;

        TagType INode.Tag { get { return TagType.None; } }
        NodeFlags INode.Flags { get { return NodeFlags.HasChildren | NodeFlags.IsExpression; } }
        public IEnumerable<INode> Children { get { return _Children; } }
        public int ChildrenCount { get { return _Children.Length; } }

        public bool IsStatic {
            get {
                if (_Children.Length == 0)
                    return true;
                if (!_IsStatic.HasValue)
                    _IsStatic = _Children.All(node => node.Flags.HasFlag(NodeFlags.IsStatic));

                return _IsStatic.Value;
            }
        }
        public bool IsEmpty {
            get {
                if (_Children.Length == 0)
                    return true;
                // TODO: This can be made more efficient (as in, without the need to convert to a string), but that's going to take lots of work.
                return string.IsNullOrWhiteSpace(ToString());
            }
        }

        public XivString(params INode[] children) : this((IEnumerable<INode>)children) { }
        public XivString(IEnumerable<INode> children) {
            _Children = children.ToArray();
        }

        public override string ToString() {
            if (_String == null) {
                var sb = new StringBuilder();
                ToString(sb);
                _String = sb.ToString();
            }
            return _String;
        }
        public void ToString(StringBuilder builder) {
            foreach (var part in Children)
                part.ToString(builder);
        }

        public static implicit operator string (XivString xivString) {
            return xivString.ToString();
        }

        public override int GetHashCode() {
            return ToString().GetHashCode();
        }

        #region Equality
        public static bool Equals(XivString left, XivString right) {
            var lNull = object.ReferenceEquals(null, left);
            var rNull = object.ReferenceEquals(null, right);
            if (lNull != rNull)
                return false;
            if (lNull == true)
                return true;

            if (left._Children.Length != right._Children.Length)
                return false;

            for (var i = 0; i < left.ChildrenCount; ++i) {
                var lc = left._Children[i];
                var rc = right._Children[i];
                if (!lc.Equals(rc))
                    return false;
            }
            return true;
        }

        bool IEquatable<INode>.Equals(INode other) {
            if (other is XivString)
                return Equals(this, (XivString)other);
            return false;
        }

        public override bool Equals(object obj) {
            if (obj is XivString)
                return Equals(this, (XivString)obj);
            if (obj is string)
                return Equals(this.ToString(), (string)obj);
            return false;
        }

        #region Operators

        public static bool operator ==(XivString left, XivString right) {
            var lNull = object.ReferenceEquals(null, left);
            var rNull = object.ReferenceEquals(null, right);
            if (lNull && rNull)
                return true;
            if (lNull != rNull)
                return false;
            return (Equals(left, right));
        }
        public static bool operator !=(XivString left, XivString right) {
            var lNull = object.ReferenceEquals(null, left);
            var rNull = object.ReferenceEquals(null, right);
            if (lNull && rNull)
                return false;
            if (lNull != rNull)
                return true;
            return !(Equals(left, right));
        }
        public static bool operator ==(XivString left, string right) {
            var lNull = object.ReferenceEquals(null, left);
            var rNull = object.ReferenceEquals(null, right);
            if (lNull && rNull)
                return true;
            if (lNull != rNull)
                return false;
            return (left.ToString() == right);
        }
        public static bool operator !=(XivString left, string right) {
            var lNull = object.ReferenceEquals(null, left);
            var rNull = object.ReferenceEquals(null, right);
            if (lNull && rNull)
                return false;
            if (lNull != rNull)
                return true;
            return (left.ToString() != right);
        }
        #endregion

        #region IEquatable<XivString> Members

        public bool Equals(XivString other) {
            return Equals(this, other);
        }

        #endregion

        #region IEquatable<string> Members

        public bool Equals(string other) {
            return string.Equals(this.ToString(), other);
        }

        #endregion

        #endregion

        #region Comparison

        public static int Compare(XivString left, XivString right) {
            var lNull = object.ReferenceEquals(null, left);
            var rNull = object.ReferenceEquals(null, right);
            if (lNull != rNull)
                return lNull.CompareTo(rNull);
            if (!lNull)
                return 0;

            if (left._Children.Length != right._Children.Length)
                return left._Children.Length.CompareTo(right._Children.Length);

            for (var i = 0; i < left.ChildrenCount; ++i) {
                var lc = left._Children[i];
                var rc = right._Children[i];

                var cmp = lc.CompareTo(rc);
                if (cmp != 0)
                    return cmp;
            }
            return 0;
        }

        int IComparable<INode>.CompareTo(INode other) {
            if (other is XivString)
                return Compare(this, (XivString)other);
            return 1;
        }

        #region IComparable Members

        public int CompareTo(object obj) {
            if (obj is XivString)
                return CompareTo((XivString)obj);
            if (obj is string)
                return CompareTo((string)obj);
            if (obj == null)
                return 1;
            return -1;
        }

        #endregion

        #region IComparable<XivString> Members

        public int CompareTo(XivString other) {
            return Compare(this, other);
        }

        #endregion

        #region IComparable<string> Members

        public int CompareTo(string other) {
            return string.Compare(this.ToString(), other);
        }

        #endregion

        #endregion

        #region IExpressionNode Members

        public IExpression Evaluate(IEvaluationFunctionProvider provider, EvaluationParameters parameters) {
            return new Expressions.ExpressionCollection(Children.Select(c => c.TryEvaluate(provider, parameters)));
        }

        #endregion
    }
}
