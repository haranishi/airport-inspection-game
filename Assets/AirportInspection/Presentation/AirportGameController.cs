using System;
using System.Collections.Generic;
using AirportInspection.Application;
using AirportInspection.Domain;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AirportInspection.Presentation
{
    public sealed class AirportGameController : MonoBehaviour
    {
        private static readonly Color Ink = Hex("DCECF3");
        private static readonly Color Muted = Hex("8BA5AF");
        private static readonly Color Screen = Hex("07161D");
        private static readonly Color Panel = Hex("102832");
        private static readonly Color Accent = Hex("44C2C9");
        private static readonly Color Danger = Hex("E45A61");

        private readonly Dictionary<string, GameObject> itemViews = new Dictionary<string, GameObject>();
        private ShiftSession session;
        private RectTransform xrayContent;
        private Text caseText;
        private Text progressText;
        private Text filterText;
        private Text openListText;
        private GameObject openPanel;
        private GameObject modal;
        private Text modalTitle;
        private Text modalBody;
        private Text modalButtonText;
        private Button modalButton;
        private MaterialKind? filter;
        private int bagRotation;
        private bool bagOpened;
        private bool shiftStarted;
        private Action modalAction;
        private Font font;

        private void Awake()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            session = new ShiftSession(
                VerticalSliceContent.CreateCases(),
                VerticalSliceContent.CreateRules(),
                new InspectionEvaluator());
            BuildInterface();
            ShowBriefing();
        }

        private void Update()
        {
            if (!shiftStarted || modal.activeSelf)
            {
                if (modal.activeSelf && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)))
                    modalAction?.Invoke();
                return;
            }

            if (Input.GetKeyDown(KeyCode.R)) RotateBag();
            if (Input.GetKeyDown(KeyCode.F)) CycleFilter();
            if (Input.GetKeyDown(KeyCode.O)) ToggleOpenBag();
            if (Input.GetKeyDown(KeyCode.Alpha1)) Decide(DecisionAction.Pass);
            if (Input.GetKeyDown(KeyCode.Alpha2)) Decide(DecisionAction.Confiscate);
            if (Input.GetKeyDown(KeyCode.Alpha3)) Decide(DecisionAction.Report);
        }

        private void BuildInterface()
        {
            var canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = .5f;

            var background = PanelObject("Background", canvasObject.transform, Hex("061015"));
            Stretch(background.GetComponent<RectTransform>(), 0, 0, 0, 0);

            TextObject("Title", background.transform, "NORTHGATE / BAGGAGE CONTROL", 25, FontStyle.Bold,
                new Vector2(28, -18), new Vector2(730, 38), TextAnchor.MiddleLeft, Accent);
            progressText = TextObject("Progress", background.transform, "", 17, FontStyle.Bold,
                new Vector2(-330, -22), new Vector2(300, 30), TextAnchor.MiddleRight, Ink, true);
            caseText = TextObject("Case", background.transform, "", 18, FontStyle.Normal,
                new Vector2(-24, -22), new Vector2(280, 30), TextAnchor.MiddleRight, Muted, true);

            var rules = PanelObject("Rules", background.transform, Panel);
            SetRect(rules.GetComponent<RectTransform>(), new Vector2(24, 78), new Vector2(270, 540));
            TextObject("RulesTitle", rules.transform, "本日の規則", 22, FontStyle.Bold,
                new Vector2(16, -14), new Vector2(220, 34), TextAnchor.MiddleLeft, Accent);
            TextObject("RulesBody", rules.transform,
                "R-NOVA\n加圧式ノヴァ液 → 没収\n\nR-CORE\n赤色共鳴核 → 警備通報\n\nそれ以外 → 通過\n\n※ 禁止品は架空のものです。", 17, FontStyle.Normal,
                new Vector2(16, -64), new Vector2(220, 260), TextAnchor.UpperLeft, Ink);
            TextObject("Shortcuts", rules.transform,
                "R  回転\nF  材質フィルター\nO  開披\n1 / 2 / 3  判定", 15, FontStyle.Normal,
                new Vector2(16, -386), new Vector2(220, 120), TextAnchor.UpperLeft, Muted);

            var xray = PanelObject("XrayMonitor", background.transform, Hex("0B2028"));
            SetRect(xray.GetComponent<RectTransform>(), new Vector2(300, 78), new Vector2(650, 470));
            var xrayViewport = PanelObject("XrayViewport", xray.transform, Screen);
            SetRect(xrayViewport.GetComponent<RectTransform>(), new Vector2(22, 56), new Vector2(606, 376));
            xrayContent = xrayViewport.GetComponent<RectTransform>();
            TextObject("MonitorLabel", xray.transform, "X-RAY / 架空材質スキャン", 16, FontStyle.Bold,
                new Vector2(20, -12), new Vector2(380, 30), TextAnchor.MiddleLeft, Accent);
            filterText = TextObject("Filter", xray.transform, "材質: 全表示", 15, FontStyle.Bold,
                new Vector2(-210, -14), new Vector2(190, 28), TextAnchor.MiddleRight, Ink, true);

            var controls = PanelObject("Controls", background.transform, Panel);
            SetRect(controls.GetComponent<RectTransform>(), new Vector2(964, 78), new Vector2(292, 540));
            TextObject("ControlsTitle", controls.transform, "検査操作", 22, FontStyle.Bold,
                new Vector2(18, -14), new Vector2(240, 34), TextAnchor.MiddleLeft, Accent);
            ButtonObject("Rotate", controls.transform, "荷物を90°回転  [R]", new Vector2(18, -66), new Vector2(256, 48), RotateBag);
            ButtonObject("Filter", controls.transform, "材質フィルター  [F]", new Vector2(18, -122), new Vector2(256, 48), CycleFilter);
            ButtonObject("Open", controls.transform, "開披して確認  [O]", new Vector2(18, -178), new Vector2(256, 48), ToggleOpenBag);
            TextObject("DecisionTitle", controls.transform, "最終判定", 18, FontStyle.Bold,
                new Vector2(18, -252), new Vector2(240, 30), TextAnchor.MiddleLeft, Ink);
            ButtonObject("Pass", controls.transform, "1  通過", new Vector2(18, -294), new Vector2(256, 48), () => Decide(DecisionAction.Pass), Hex("246A61"));
            ButtonObject("Confiscate", controls.transform, "2  没収", new Vector2(18, -350), new Vector2(256, 48), () => Decide(DecisionAction.Confiscate), Hex("9A7333"));
            ButtonObject("Report", controls.transform, "3  通報", new Vector2(18, -406), new Vector2(256, 48), () => Decide(DecisionAction.Report), Hex("8E3B43"));

            openPanel = PanelObject("OpenedBag", background.transform, Hex("173540"));
            SetRect(openPanel.GetComponent<RectTransform>(), new Vector2(300, 558), new Vector2(650, 92));
            TextObject("OpenedTitle", openPanel.transform, "開披結果", 16, FontStyle.Bold,
                new Vector2(16, -10), new Vector2(120, 26), TextAnchor.MiddleLeft, Accent);
            openListText = TextObject("OpenedList", openPanel.transform, "", 17, FontStyle.Normal,
                new Vector2(142, -10), new Vector2(486, 62), TextAnchor.UpperLeft, Ink);
            openPanel.SetActive(false);

            modal = PanelObject("ModalShade", canvasObject.transform, new Color(0, 0, 0, .78f));
            Stretch(modal.GetComponent<RectTransform>(), 0, 0, 0, 0);
            var card = PanelObject("ModalCard", modal.transform, Hex("102832"));
            Center(card.GetComponent<RectTransform>(), new Vector2(650, 360));
            modalTitle = TextObject("ModalTitle", card.transform, "", 28, FontStyle.Bold,
                new Vector2(34, -28), new Vector2(582, 44), TextAnchor.MiddleLeft, Accent);
            modalBody = TextObject("ModalBody", card.transform, "", 19, FontStyle.Normal,
                new Vector2(34, -92), new Vector2(582, 176), TextAnchor.UpperLeft, Ink);
            modalButton = ButtonObject("ModalButton", card.transform, "", new Vector2(170, -286), new Vector2(310, 52), InvokeModal, Accent);
            modalButtonText = modalButton.GetComponentInChildren<Text>();

            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                eventSystem.transform.SetParent(transform, false);
            }
        }

        private void ShowBriefing()
        {
            modal.SetActive(true);
            modalTitle.text = "勤務ブリーフィング / DAY 1";
            modalBody.text = "6件の荷物を検査してください。\n\nX線の形・材質を確認し、必要なら開披します。最終判断は「通過」「没収」「通報」のいずれかです。判定後には、該当物と規則を必ず表示します。";
            modalButtonText.text = "勤務を開始  [Enter]";
            modalAction = () =>
            {
                shiftStarted = true;
                modal.SetActive(false);
                RenderCase();
            };
        }

        private void RenderCase()
        {
            filter = null;
            bagRotation = 0;
            bagOpened = false;
            openPanel.SetActive(false);
            itemViews.Clear();
            foreach (Transform child in xrayContent)
                Destroy(child.gameObject);

            var bag = session.CurrentCase;
            progressText.text = $"検査 {session.CaseIndex + 1} / {session.TotalCases}";
            caseText.text = $"{bag.PassengerCode}  /  BAG {bag.Id}";
            foreach (var item in bag.Items)
                itemViews[item.Id] = CreateXrayItem(item);
            UpdateFilterVisuals();
        }

        private GameObject CreateXrayItem(ItemDefinition item)
        {
            var view = PanelObject(item.Id, xrayContent, MaterialColor(item.Material));
            var rect = view.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            ApplyItemTransform(rect, item);
            var outline = view.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, .8f);
            outline.effectDistance = new Vector2(3, -3);
            var mark = item.Material == MaterialKind.Organic ? "O //" : item.Material == MaterialKind.Inorganic ? "I ##" : "M XX";
            TextObject("Pattern", view.transform, mark, 15, FontStyle.Bold,
                Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, new Color(1, 1, 1, .82f), false, true);
            return view;
        }

        private void RotateBag()
        {
            if (!CanInspect()) return;
            bagRotation = (bagRotation + 90) % 360;
            foreach (var item in session.CurrentCase.Items)
                ApplyItemTransform(itemViews[item.Id].GetComponent<RectTransform>(), item);
        }

        private void ApplyItemTransform(RectTransform rect, ItemDefinition item)
        {
            var x = item.X;
            var y = item.Y;
            if (bagRotation == 90) { var oldX = x; x = -y; y = oldX; }
            else if (bagRotation == 180) { x = -x; y = -y; }
            else if (bagRotation == 270) { var oldX = x; x = y; y = -oldX; }

            rect.anchoredPosition = new Vector2(x * 500f, y * 300f);
            var swap = bagRotation == 90 || bagRotation == 270;
            rect.sizeDelta = swap
                ? new Vector2(item.Height * 500f, item.Width * 300f)
                : new Vector2(item.Width * 500f, item.Height * 300f);
            rect.localEulerAngles = new Vector3(0, 0, item.Rotation + bagRotation);
        }

        private void CycleFilter()
        {
            if (!CanInspect()) return;
            if (!filter.HasValue) filter = MaterialKind.Organic;
            else if (filter == MaterialKind.Organic) filter = MaterialKind.Inorganic;
            else if (filter == MaterialKind.Inorganic) filter = MaterialKind.Mixed;
            else filter = null;
            UpdateFilterVisuals();
        }

        private void UpdateFilterVisuals()
        {
            filterText.text = "材質: " + (!filter.HasValue ? "全表示" : MaterialLabel(filter.Value));
            foreach (var item in session.CurrentCase.Items)
            {
                var image = itemViews[item.Id].GetComponent<Image>();
                var color = MaterialColor(item.Material);
                color.a = !filter.HasValue || filter.Value == item.Material ? .9f : .10f;
                image.color = color;
            }
        }

        private void ToggleOpenBag()
        {
            if (!CanInspect()) return;
            bagOpened = !bagOpened;
            openPanel.SetActive(bagOpened);
            if (!bagOpened) return;
            var names = new List<string>();
            foreach (var item in session.CurrentCase.Items)
                names.Add($"{item.DisplayName} [{MaterialLabel(item.Material)}]");
            openListText.text = string.Join("  /  ", names);
        }

        private void Decide(DecisionAction action)
        {
            if (!CanInspect()) return;
            var result = session.Decide(action);
            if (!string.IsNullOrEmpty(result.EvidenceItemId) && itemViews.TryGetValue(result.EvidenceItemId, out var view))
            {
                var outline = view.GetComponent<Outline>();
                outline.effectColor = Danger;
                outline.effectDistance = new Vector2(7, -7);
            }

            modal.SetActive(true);
            modalTitle.text = result.IsCorrect ? "判定一致" : "判定を再確認";
            modalTitle.color = result.IsCorrect ? Accent : Danger;
            modalBody.text =
                $"あなたの判定：{ActionLabel(result.SelectedAction)}\n" +
                $"必要な措置：{ActionLabel(result.ExpectedAction)}\n\n" +
                $"根拠物：{result.EvidenceItemName}\n{result.RuleText}\n\n" +
                (result.IsCorrect ? "根拠と判断が一致しました。" : "赤枠の物と規則を確認して、次の検査へ進んでください。");
            modalButtonText.text = session.CaseIndex + 1 >= session.TotalCases ? "勤務結果へ  [Enter]" : "次の荷物  [Enter]";
            modalAction = Advance;
        }

        private void Advance()
        {
            session.Advance();
            if (session.IsComplete)
            {
                modal.SetActive(true);
                modalTitle.color = Accent;
                modalTitle.text = "勤務完了";
                modalBody.text = $"正解 {session.CorrectCount} / {session.TotalCases}\n\n1勤務を開始から評価まで完走しました。\n再実行すると同じ6ケースで操作と根拠表示を検証できます。";
                modalButtonText.text = "もう一度遊ぶ";
                modalAction = Restart;
                return;
            }
            modal.SetActive(false);
            RenderCase();
        }

        private void Restart()
        {
            session = new ShiftSession(VerticalSliceContent.CreateCases(), VerticalSliceContent.CreateRules(), new InspectionEvaluator());
            modal.SetActive(false);
            RenderCase();
        }

        private bool CanInspect() => shiftStarted && !modal.activeSelf && !session.IsComplete && !session.HasPendingResult;
        private void InvokeModal() => modalAction?.Invoke();

        private static string MaterialLabel(MaterialKind kind)
        {
            switch (kind)
            {
                case MaterialKind.Organic: return "有機 O//";
                case MaterialKind.Inorganic: return "無機 I##";
                default: return "混合 MXX";
            }
        }

        private static string ActionLabel(DecisionAction action)
        {
            switch (action)
            {
                case DecisionAction.Confiscate: return "没収";
                case DecisionAction.Report: return "警備通報";
                default: return "通過";
            }
        }

        private static Color MaterialColor(MaterialKind kind)
        {
            switch (kind)
            {
                case MaterialKind.Organic: return Hex("E59B45");
                case MaterialKind.Inorganic: return Hex("4BA6E8");
                default: return Hex("58C779");
            }
        }

        private GameObject PanelObject(string name, Transform parent, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);
            obj.GetComponent<Image>().color = color;
            return obj;
        }

        private Text TextObject(
            string name, Transform parent, string value, int size, FontStyle style,
            Vector2 position, Vector2 dimensions, TextAnchor anchor, Color color,
            bool rightAnchored = false, bool stretch = false)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Text));
            obj.transform.SetParent(parent, false);
            var text = obj.GetComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = anchor;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            var rect = obj.GetComponent<RectTransform>();
            if (stretch)
            {
                Stretch(rect, 4, 4, 4, 4);
            }
            else
            {
                rect.anchorMin = rect.anchorMax = rightAnchored ? new Vector2(1, 1) : new Vector2(0, 1);
                rect.pivot = rightAnchored ? new Vector2(1, 1) : new Vector2(0, 1);
                rect.anchoredPosition = position;
                rect.sizeDelta = dimensions;
            }
            return text;
        }

        private Button ButtonObject(string name, Transform parent, string label, Vector2 position, Vector2 size, Action action, Color? color = null)
        {
            var obj = PanelObject(name, parent, color ?? Hex("244A56"));
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var button = obj.AddComponent<Button>();
            button.targetGraphic = obj.GetComponent<Image>();
            button.onClick.AddListener(() => action());
            TextObject("Label", obj.transform, label, 17, FontStyle.Bold, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Ink, false, true);
            return button;
        }

        private static void SetRect(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void Center(RectTransform rect, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;
        }

        private static void Stretch(RectTransform rect, float left, float right, float top, float bottom)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var color);
            return color;
        }
    }
}
