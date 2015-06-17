using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaintCoinach.Text.Nodes {
    public class Comparison : INode, IExpressionNode {
        private readonly INode _Left;
        private readonly DecodeExpressionType _ComparisonType;
        private readonly INode _Right;

        TagType INode.Tag { get { return TagType.None; } }
        NodeFlags INode.Flags { get { return NodeFlags.IsExpression; } }
        public INode Left { get { return _Left; } }
        public DecodeExpressionType ComparisonType { get { return _ComparisonType; } }
        public INode Right { get { return _Right; } }

        public Comparison(DecodeExpressionType comparisonType, INode left, INode right) {
            if (left == null)
                throw new ArgumentNullException("left");
            if (right == null)
                throw new ArgumentNullException("right");
            _ComparisonType = comparisonType;
            _Left = left;
            _Right = right;
        }

        public bool Equals(INode other) {
            var n = other as Comparison;
            if (n == null)
                return false;
            if (n._ComparisonType != _ComparisonType)
                return false;
            return (_Left.Equals(n._Left) && _Right.Equals(n._Right));
        }
        public int CompareTo(INode other) {
            var n = other as Comparison;
            if (n == null)
                return 1;
            if(_ComparisonType != n._ComparisonType)
                return ((byte)_ComparisonType).CompareTo((byte)n._ComparisonType);

            var leftCmp = _Left.CompareTo(n._Left);
            if (leftCmp != 0)
                return leftCmp;
            var rightCmp = _Right.CompareTo(n._Right);
            if (rightCmp != 0)
                return rightCmp;
            return 0;
        }

        public override string ToString() {
            var sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }
        public void ToString(StringBuilder builder) {
            builder.Append(ComparisonType);
            builder.Append(StringTokens.ArgumentsOpen);
            if (Left != null)
                Left.ToString(builder);
            builder.Append(StringTokens.ArgumentsSeperator);
            if (Right != null)
                Right.ToString(builder);
            builder.Append(StringTokens.ArgumentsClose);
        }

        #region IExpressionNode Members

        public IExpression Evaluate(IEvaluationFunctionProvider provider, EvaluationParameters parameters) {
            return new Expressions.GenericExpression(provider.Compare(parameters, ComparisonType, Left, Right));
        }

        #endregion
    }
}
