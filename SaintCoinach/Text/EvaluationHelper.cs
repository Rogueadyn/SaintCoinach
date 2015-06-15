﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaintCoinach.Text {
    using Nodes;

    static class EvaluationHelper {
        public static IExpression TryEvaluate(this INode self, IEvaluationFunctionProvider provider, EvaluationParameters parameters) {
            if (self == null)
                return null;
            if (self.Flags.HasFlag(NodeFlags.IsExpression))
                return ((IExpressionNode)self).Evaluate(provider, parameters);
            return new Expressions.GenericExpression(self);
        }
    }
}
