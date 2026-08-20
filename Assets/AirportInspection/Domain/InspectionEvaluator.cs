using System.Collections.Generic;

namespace AirportInspection.Domain
{
    public sealed class InspectionEvaluator
    {
        public EvaluationResult Evaluate(
            BagCase bag,
            IReadOnlyList<RuleDefinition> activeRules,
            DecisionAction selectedAction)
        {
            RuleDefinition strongestRule = null;
            ItemDefinition evidence = null;

            foreach (var item in bag.Items)
            {
                foreach (var rule in activeRules)
                {
                    if (item.CategoryId != rule.CategoryId)
                        continue;

                    if (strongestRule == null || Severity(rule.RequiredAction) > Severity(strongestRule.RequiredAction))
                    {
                        strongestRule = rule;
                        evidence = item;
                    }
                }
            }

            var expected = strongestRule == null ? DecisionAction.Pass : strongestRule.RequiredAction;
            var ruleId = strongestRule == null ? "R-CLEAR" : strongestRule.Id;
            var ruleText = strongestRule == null
                ? "禁止対象は確認されませんでした。通過が適切です。"
                : strongestRule.DisplayText;

            return new EvaluationResult(
                selectedAction == expected,
                selectedAction,
                expected,
                ruleId,
                ruleText,
                evidence == null ? string.Empty : evidence.Id,
                evidence == null ? "該当なし" : evidence.DisplayName);
        }

        private static int Severity(DecisionAction action)
        {
            switch (action)
            {
                case DecisionAction.Report: return 2;
                case DecisionAction.Confiscate: return 1;
                default: return 0;
            }
        }
    }
}
