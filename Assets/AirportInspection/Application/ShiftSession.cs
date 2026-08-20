using System;
using System.Collections.Generic;
using AirportInspection.Domain;

namespace AirportInspection.Application
{
    public sealed class ShiftSession
    {
        private readonly IReadOnlyList<BagCase> cases;
        private readonly IReadOnlyList<RuleDefinition> rules;
        private readonly InspectionEvaluator evaluator;

        public int CaseIndex { get; private set; }
        public int CorrectCount { get; private set; }
        public bool HasPendingResult { get; private set; }
        public bool IsComplete => CaseIndex >= cases.Count;
        public int TotalCases => cases.Count;
        public BagCase CurrentCase => IsComplete ? null : cases[CaseIndex];
        public IReadOnlyList<RuleDefinition> Rules => rules;

        public ShiftSession(
            IReadOnlyList<BagCase> cases,
            IReadOnlyList<RuleDefinition> rules,
            InspectionEvaluator evaluator)
        {
            this.cases = cases;
            this.rules = rules;
            this.evaluator = evaluator;
        }

        public EvaluationResult Decide(DecisionAction action)
        {
            if (IsComplete) throw new InvalidOperationException("勤務は完了しています。");
            if (HasPendingResult) throw new InvalidOperationException("次の荷物へ進んでください。");

            var result = evaluator.Evaluate(CurrentCase, rules, action);
            if (result.IsCorrect) CorrectCount++;
            HasPendingResult = true;
            return result;
        }

        public void Advance()
        {
            if (!HasPendingResult) throw new InvalidOperationException("判定前には進めません。");
            CaseIndex++;
            HasPendingResult = false;
        }
    }
}
