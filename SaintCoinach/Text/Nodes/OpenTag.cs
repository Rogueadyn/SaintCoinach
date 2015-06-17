using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaintCoinach.Text.Nodes {
    public class OpenTag : INode, IExpressionNode {
        private readonly TagType _Tag;
        private readonly ArgumentCollection _Arguments;

        public TagType Tag { get { return _Tag; } }
        NodeFlags INode.Flags { get { return NodeFlags.IsExpression; } }
        public IEnumerable<INode> Arguments { get { return _Arguments; } }

        public OpenTag(TagType tag, params INode[] arguments) : this(tag, (IEnumerable<INode>)arguments) { }
        public OpenTag(TagType tag, IEnumerable<INode> arguments) {
            _Tag = tag;
            _Arguments = new ArgumentCollection(arguments);
        }

        public bool Equals(INode other) {
            var n = other as OpenTag;
            if (n == null)
                return false;

            return (_Tag == n._Tag && _Arguments.Equals(n._Arguments));
        }
        public int CompareTo(INode other) {
            var n = other as OpenTag;
            if (n == null)
                return 1;

            if (_Tag != n._Tag)
                return ((byte)_Tag).CompareTo(n._Tag);

            return _Arguments.CompareTo(n._Arguments);
        }

        public override string ToString() {
            var sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }
        public void ToString(StringBuilder builder) {
            builder.Append(StringTokens.TagOpen);
            builder.Append(Tag);

            _Arguments.ToString(builder);

            builder.Append(StringTokens.TagClose);
        }

        #region IExpressionNode Members

        public IExpression Evaluate(IEvaluationFunctionProvider provider, EvaluationParameters parameters) {
            return new Expressions.OpenTag(Tag, Arguments.Select(_ => _.TryEvaluate(provider, parameters)));
        }

        #endregion
    }
}
