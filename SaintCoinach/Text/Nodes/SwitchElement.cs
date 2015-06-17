using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaintCoinach.Text.Nodes {
    public class SwitchElement : INode, IExpressionNode {
        private readonly TagType _Tag;
        private readonly INode _CaseSwitch;
        private readonly ReadOnlyDictionary<int, INode> _Cases;

        public TagType Tag { get { return _Tag; } }
        NodeFlags INode.Flags { get { return NodeFlags.IsExpression; } }
        public INode CaseSwitch { get { return _CaseSwitch; } }
        public IReadOnlyDictionary<int, INode> Cases { get { return _Cases; } }

        public SwitchElement(TagType tag, INode caseSwitch, IDictionary<int, INode> cases) {
            if (caseSwitch == null)
                throw new ArgumentNullException("caseSwitch");
            if (cases == null)
                throw new ArgumentNullException("cases");
            _Tag = tag;
            _CaseSwitch = caseSwitch;
            _Cases = new ReadOnlyDictionary<int, INode>(cases);
        }

        public bool Equals(INode other) {
            var n = other as SwitchElement;
            if (n == null)
                return false;

            if (_Tag != n._Tag)
                return false;
            if (!_CaseSwitch.Equals(n._CaseSwitch))
                return false;

            if (_Cases.Count != n._Cases.Count)
                return false;

            var otherQueue = n._Cases.Values.ToList();
            foreach(var kvp in _Cases) {
                INode otherCase;
                if (!n._Cases.TryGetValue(kvp.Key, out otherCase))
                    return false;

                if (!kvp.Value.Equals(otherCase))
                    return false;

                otherQueue.Remove(otherCase);
            }

            return (otherQueue.Count == 0);
        }
        public int CompareTo(INode other) {
            var n = other as SwitchElement;
            if (n == null)
                return 1;
            
            if (_Tag != n._Tag)
                return ((byte)_Tag).CompareTo((byte)n._Tag);

            var switchCmp = _CaseSwitch.CompareTo(n._CaseSwitch);
            if (switchCmp != 0)
                return switchCmp;

            if (_Cases.Count != n._Cases.Count)
                return _Cases.Count.CompareTo(n._Cases.Count);

            var otherQueue = n._Cases.Values.ToList();
            foreach (var kvp in _Cases) {
                INode otherCase;
                if (!n._Cases.TryGetValue(kvp.Key, out otherCase))
                    return 1;

                var cmp = kvp.Value.CompareTo(otherCase);
                if (cmp != 0)
                    return cmp;

                otherQueue.Remove(otherCase);
            }

            if (otherQueue.Count > 0)
                return -1;
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
            CaseSwitch.ToString(builder);
            builder.Append(StringTokens.ArgumentsClose);
            builder.Append(StringTokens.TagClose);

            foreach(var caseValue in Cases){
                builder.Append(StringTokens.TagOpen);
                builder.Append(StringTokens.CaseTagName);
                builder.Append(StringTokens.ArgumentsOpen);
                builder.Append(caseValue.Key);
                builder.Append(StringTokens.ArgumentsClose);
                builder.Append(StringTokens.TagClose);

                caseValue.Value.ToString(builder);

                builder.Append(StringTokens.TagOpen);
                builder.Append(StringTokens.ElementClose);
                builder.Append(StringTokens.CaseTagName);
                builder.Append(StringTokens.TagClose);
            }

            builder.Append(StringTokens.TagOpen);
            builder.Append(StringTokens.ElementClose);
            builder.Append(Tag);
            builder.Append(StringTokens.TagClose);
        }

        #region IExpressionNode Members

        public IExpression Evaluate(IEvaluationFunctionProvider provider, EvaluationParameters parameters) {
            var evalSwitch = CaseSwitch.TryEvaluate(provider, parameters);
            var asInt = provider.ToInteger(evalSwitch);

            INode caseNode;
            if (!_Cases.TryGetValue(asInt, out caseNode))
                throw new InvalidOperationException();
            return caseNode.TryEvaluate(provider, parameters);
        }

        #endregion
    }
}
