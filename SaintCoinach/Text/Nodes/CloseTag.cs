using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaintCoinach.Text.Nodes {
    public class CloseTag : IExpressionNode {
        private readonly TagType _Tag;

        public TagType Tag { get { return _Tag; } }
        NodeFlags INode.Flags { get { return NodeFlags.IsExpression | NodeFlags.IsStatic; } }

        public CloseTag(TagType tag) {
            _Tag = tag;
        }

        public bool Equals(INode other) {
            var n = other as CloseTag;
            return (n != null && n._Tag == _Tag);
        }
        public int CompareTo(INode other) {
            var n = other as CloseTag;
            if (n == null)
                return 1;
            if (_Tag != n._Tag)
                return ((byte)_Tag).CompareTo((byte)n._Tag);
            return 0;
        }

        public override string ToString() {
            var sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }
        public void ToString(StringBuilder builder) {
            builder.Append(StringTokens.TagOpen);
            builder.Append(StringTokens.ElementClose);
            builder.Append(Tag);
            builder.Append(StringTokens.TagClose);
        }

        #region IExpressionNode Members

        public IExpression Evaluate(IEvaluationFunctionProvider provider, EvaluationParameters parameters) {
            return new Expressions.CloseTag(Tag);
        }

        #endregion
    }
}
