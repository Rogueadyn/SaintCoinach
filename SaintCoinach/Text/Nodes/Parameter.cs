using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaintCoinach.Text.Nodes {
    public class Parameter : INode, IExpressionNode {
        private readonly DecodeExpressionType _ParameterType;
        private readonly INode _ParameterIndex;

        TagType INode.Tag { get { return TagType.None; } }
        NodeFlags INode.Flags { get { return NodeFlags.IsExpression; } }
        public DecodeExpressionType ParameterType { get { return _ParameterType; } }
        public INode ParameterIndex { get { return _ParameterIndex; } }

        public Parameter(DecodeExpressionType parameterType, INode parameterIndex) {
            if (parameterIndex == null)
                throw new ArgumentNullException("parameterIndex");
            _ParameterType = parameterType;
            _ParameterIndex = parameterIndex;
        }

        public bool Equals(INode other) {
            var n = other as Parameter;
            if (n == null)
                return false;

            return (_ParameterIndex.Equals(n._ParameterIndex) && _ParameterType == n._ParameterType);
        }
        public int CompareTo(INode other) {
            var n = other as Parameter;
            if (n == null)
                return 1;

            if (_ParameterType != n._ParameterType)
                return ((byte)_ParameterType).CompareTo((byte)n._ParameterType);

            return _ParameterIndex.CompareTo(n._ParameterIndex);
        }

        public override string ToString() {
            var sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }
        public void ToString(StringBuilder builder) {
            builder.Append(ParameterType);
            builder.Append(StringTokens.ArgumentsOpen);
            ParameterIndex.ToString(builder);
            builder.Append(StringTokens.ArgumentsClose);
        }

        #region IExpressionNode Members

        public IExpression Evaluate(IEvaluationFunctionProvider provider, EvaluationParameters parameters) {
            var evalIndex = ParameterIndex.TryEvaluate(provider, parameters);
            var index = provider.ToInteger(evalIndex);
            return new Expressions.GenericExpression(parameters[this.ParameterType, index]);
        }

        #endregion
    }
}
