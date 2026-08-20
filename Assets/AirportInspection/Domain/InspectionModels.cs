using System;
using System.Collections.Generic;

namespace AirportInspection.Domain
{
    public enum MaterialKind { Organic, Inorganic, Mixed }
    public enum DecisionAction { Pass, Confiscate, Report }

    [Serializable]
    public sealed class ItemDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string CategoryId { get; }
        public MaterialKind Material { get; }
        public float X { get; }
        public float Y { get; }
        public float Width { get; }
        public float Height { get; }
        public float Rotation { get; }

        public ItemDefinition(
            string id, string displayName, string categoryId, MaterialKind material,
            float x, float y, float width, float height, float rotation)
        {
            Id = id;
            DisplayName = displayName;
            CategoryId = categoryId;
            Material = material;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Rotation = rotation;
        }
    }

    [Serializable]
    public sealed class RuleDefinition
    {
        public string Id { get; }
        public string CategoryId { get; }
        public DecisionAction RequiredAction { get; }
        public string DisplayText { get; }

        public RuleDefinition(string id, string categoryId, DecisionAction requiredAction, string displayText)
        {
            Id = id;
            CategoryId = categoryId;
            RequiredAction = requiredAction;
            DisplayText = displayText;
        }
    }

    [Serializable]
    public sealed class BagCase
    {
        public string Id { get; }
        public string PassengerCode { get; }
        public IReadOnlyList<ItemDefinition> Items { get; }

        public BagCase(string id, string passengerCode, IReadOnlyList<ItemDefinition> items)
        {
            Id = id;
            PassengerCode = passengerCode;
            Items = items;
        }
    }

    public sealed class EvaluationResult
    {
        public bool IsCorrect { get; }
        public DecisionAction SelectedAction { get; }
        public DecisionAction ExpectedAction { get; }
        public string RuleId { get; }
        public string RuleText { get; }
        public string EvidenceItemId { get; }
        public string EvidenceItemName { get; }

        public EvaluationResult(
            bool isCorrect, DecisionAction selectedAction, DecisionAction expectedAction,
            string ruleId, string ruleText, string evidenceItemId, string evidenceItemName)
        {
            IsCorrect = isCorrect;
            SelectedAction = selectedAction;
            ExpectedAction = expectedAction;
            RuleId = ruleId;
            RuleText = ruleText;
            EvidenceItemId = evidenceItemId;
            EvidenceItemName = evidenceItemName;
        }
    }
}
