using System.Collections.Generic;
using AirportInspection.Domain;

namespace AirportInspection.Application
{
    public static class VerticalSliceContent
    {
        public static IReadOnlyList<RuleDefinition> CreateRules()
        {
            return new[]
            {
                new RuleDefinition("R-NOVA", "restricted-liquid", DecisionAction.Confiscate,
                    "規則 R-NOVA：加圧式ノヴァ液は没収対象です。"),
                new RuleDefinition("R-CORE", "critical-artifact", DecisionAction.Report,
                    "規則 R-CORE：赤色共鳴核は警備への通報対象です。")
            };
        }

        public static IReadOnlyList<BagCase> CreateCases()
        {
            return new[]
            {
                Case("A-101", "旅客 014", Shirt(), Book()),
                Case("A-102", "旅客 027", Bottle(), Camera()),
                Case("A-103", "旅客 041", Shoes(), Gift()),
                Case("A-104", "旅客 052", Book(), NovaLiquid()),
                Case("A-105", "旅客 068", Shirt(), Camera(), Shoes()),
                Case("A-106", "旅客 073", Gift(), ResonanceCore())
            };
        }

        private static BagCase Case(string id, string passenger, params ItemDefinition[] items)
        {
            return new BagCase(id, passenger, items);
        }

        private static ItemDefinition Shirt() => Item("shirt", "衣類", "personal", MaterialKind.Organic, -.45f, .30f, .34f, .24f, -12f);
        private static ItemDefinition Book() => Item("book", "本", "personal", MaterialKind.Organic, .23f, .25f, .27f, .35f, 8f);
        private static ItemDefinition Bottle() => Item("bottle", "飲料ボトル", "personal", MaterialKind.Organic, -.25f, -.22f, .15f, .38f, -5f);
        private static ItemDefinition Camera() => Item("camera", "カメラ", "electronics", MaterialKind.Mixed, .35f, .05f, .28f, .24f, 18f);
        private static ItemDefinition Shoes() => Item("shoes", "靴", "personal", MaterialKind.Mixed, -.18f, .18f, .35f, .20f, 25f);
        private static ItemDefinition Gift() => Item("gift", "贈答箱", "personal", MaterialKind.Inorganic, .30f, -.20f, .31f, .31f, 0f);
        private static ItemDefinition NovaLiquid() => Item("nova-liquid", "加圧式ノヴァ液", "restricted-liquid", MaterialKind.Organic, .20f, -.20f, .13f, .40f, 15f);
        private static ItemDefinition ResonanceCore() => Item("resonance-core", "赤色共鳴核", "critical-artifact", MaterialKind.Mixed, -.15f, -.18f, .25f, .25f, 45f);

        private static ItemDefinition Item(
            string id, string name, string category, MaterialKind material,
            float x, float y, float width, float height, float rotation)
        {
            return new ItemDefinition(id, name, category, material, x, y, width, height, rotation);
        }
    }
}
