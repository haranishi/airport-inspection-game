using AirportInspection.Application;
using AirportInspection.Domain;
using NUnit.Framework;

namespace AirportInspection.Tests
{
    public sealed class InspectionEvaluatorTests
    {
        private InspectionEvaluator evaluator;

        [SetUp]
        public void SetUp() => evaluator = new InspectionEvaluator();

        [Test]
        public void NormalBag_RequiresPass()
        {
            var bag = VerticalSliceContent.CreateCases()[0];
            var result = evaluator.Evaluate(bag, VerticalSliceContent.CreateRules(), DecisionAction.Pass);
            Assert.That(result.IsCorrect, Is.True);
            Assert.That(result.ExpectedAction, Is.EqualTo(DecisionAction.Pass));
            Assert.That(result.RuleId, Is.EqualTo("R-CLEAR"));
        }

        [Test]
        public void RestrictedLiquid_RequiresConfiscationWithMatchingEvidence()
        {
            var bag = VerticalSliceContent.CreateCases()[3];
            var result = evaluator.Evaluate(bag, VerticalSliceContent.CreateRules(), DecisionAction.Pass);
            Assert.That(result.IsCorrect, Is.False);
            Assert.That(result.ExpectedAction, Is.EqualTo(DecisionAction.Confiscate));
            Assert.That(result.EvidenceItemId, Is.EqualTo("nova-liquid"));
            Assert.That(result.RuleId, Is.EqualTo("R-NOVA"));
        }

        [Test]
        public void CriticalArtifact_RequiresReport()
        {
            var bag = VerticalSliceContent.CreateCases()[5];
            var result = evaluator.Evaluate(bag, VerticalSliceContent.CreateRules(), DecisionAction.Report);
            Assert.That(result.IsCorrect, Is.True);
            Assert.That(result.ExpectedAction, Is.EqualTo(DecisionAction.Report));
            Assert.That(result.EvidenceItemId, Is.EqualTo("resonance-core"));
        }

        [Test]
        public void ReportRule_WinsOverConfiscationRule()
        {
            var source = VerticalSliceContent.CreateCases();
            var mixedBag = new BagCase("T-1", "TEST", new[] { source[3].Items[1], source[5].Items[1] });
            var result = evaluator.Evaluate(mixedBag, VerticalSliceContent.CreateRules(), DecisionAction.Report);
            Assert.That(result.ExpectedAction, Is.EqualTo(DecisionAction.Report));
            Assert.That(result.RuleId, Is.EqualTo("R-CORE"));
        }

        [Test]
        public void Shift_RejectsAdvanceBeforeDecisionAndDoubleDecision()
        {
            var shift = new ShiftSession(VerticalSliceContent.CreateCases(), VerticalSliceContent.CreateRules(), evaluator);
            Assert.Throws<System.InvalidOperationException>(() => shift.Advance());
            shift.Decide(DecisionAction.Pass);
            Assert.Throws<System.InvalidOperationException>(() => shift.Decide(DecisionAction.Pass));
            shift.Advance();
            Assert.That(shift.CaseIndex, Is.EqualTo(1));
        }

        [Test]
        public void SixCases_CanCompleteAndScoreAllCorrect()
        {
            var shift = new ShiftSession(VerticalSliceContent.CreateCases(), VerticalSliceContent.CreateRules(), evaluator);
            var choices = new[]
            {
                DecisionAction.Pass, DecisionAction.Pass, DecisionAction.Pass,
                DecisionAction.Confiscate, DecisionAction.Pass, DecisionAction.Report
            };
            foreach (var choice in choices)
            {
                shift.Decide(choice);
                shift.Advance();
            }
            Assert.That(shift.IsComplete, Is.True);
            Assert.That(shift.CorrectCount, Is.EqualTo(6));
        }
    }
}
