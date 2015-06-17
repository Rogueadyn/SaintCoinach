using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaintCoinach.Text.Nodes {
    public class IfElement : INode, IConditionalNode {
        private readonly TagType _Tag;
        private readonly INode _Condition;
        private readonly INode _TrueValue;
        private readonly INode _FalseValue;

        public TagType Tag { get { return _Tag; } }
        NodeFlags INode.Flags { get { return NodeFlags.IsExpression | NodeFlags.IsConditional; } }
        public INode Condition { get { return _Condition; } }
        public INode TrueValue { get { return _TrueValue; } }
        public INode FalseValue { get { return _FalseValue; } }

        public IfElement(TagType tag, INode condition, INode trueValue, INode falseValue) {
            if (condition == null)
                throw new ArgumentNullException("condition");
            if (trueValue == null)
                throw new ArgumentNullException("trueValue");
            _Tag = tag;
            _Condition = condition;
            _TrueValue = trueValue;
            _FalseValue = falseValue;
        }

        public bool Equals(INode other) {
            var n = other as IfElement;
            if (n == null)
                return false;
            if (_Tag != n._Tag)
                return false;

            if (!_Condition.Equals(n._Condition))
                return false;

            if (!_TrueValue.Equals(n._TrueValue))
                return false;

            {
                var lNull = object.ReferenceEquals(_FalseValue, null);
                var rNull = object.ReferenceEquals(n._FalseValue, null);

                if (lNull != rNull)
                    return false;

                if (!lNull && !_FalseValue.Equals(n._FalseValue))
                    return false;
            }

            return true;
        }
        public int CompareTo(INode other) {
            var n = other as IfElement;
            if (n == null)
                return 1;

            if (_Tag != n._Tag)
                return ((byte)_Tag).CompareTo((byte)n._Tag);

            var cndCmp = _Condition.CompareTo(n._Condition);
            if (cndCmp != 0)
                return cndCmp;

            var trueCmp = _TrueValue.CompareTo(n._TrueValue);
            if (trueCmp != 0)
                return trueCmp;


            {
                var lNull = object.ReferenceEquals(_FalseValue, null);
                var rNull = object.ReferenceEquals(n._FalseValue, null);

                if (lNull != rNull)
                    return lNull.CompareTo(rNull);
                if(!lNull) {
                    var cmp = _FalseValue.CompareTo(n._FalseValue);
                    if (cmp != 0)
                        return cmp;
                }
            }

            return 0;
        }

        public override string ToString() {
            var sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }
        public void ToString(StringBuilder builder) {
            builder.Append(StringTokens.TagOpen);
            builder.Append(Tag);
            builder.Append(StringTokens.ArgumentsOpen);
            Condition.ToString(builder);
            builder.Append(StringTokens.ArgumentsClose);
            builder.Append(StringTokens.TagClose);

            if (TrueValue != null)
                TrueValue.ToString(builder);

            if (FalseValue != null) {
                builder.Append(StringTokens.ElseTag);
                FalseValue.ToString(builder);
            }

            builder.Append(StringTokens.TagOpen);
            builder.Append(StringTokens.ElementClose);
            builder.Append(Tag);
            builder.Append(StringTokens.TagClose);
        }

        #region IExpressionNode Members

        public IExpression Evaluate(IEvaluationFunctionProvider provider, EvaluationParameters parameters) {
            var evalCond = Condition.TryEvaluate(provider, parameters);
            if (provider.ToBoolean(evalCond))
                return TrueValue.TryEvaluate(provider, parameters);
            return FalseValue.TryEvaluate(provider, parameters);
        }

        #endregion
    }
}
