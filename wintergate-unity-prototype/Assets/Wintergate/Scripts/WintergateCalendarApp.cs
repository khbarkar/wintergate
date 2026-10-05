using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR;

namespace Wintergate
{
    public sealed class WintergateCalendarApp : MonoBehaviour
    {
        private static readonly Color Night = Hex("071019");
        private static readonly Color Panel = Hex("0D1823");
        private static readonly Color Panel2 = Hex("132332");
        private static readonly Color Line = Hex("294054");
        private static readonly Color Gold = Hex("F2BF4B");
        private static readonly Color Teal = Hex("56D6C9");
        private static readonly Color Ink = Hex("EEF2F6");
        private static readonly Color Muted = Hex("A8B2BF");

        private static readonly string[,] Days =
        {
            {"Norse", "The Raven's Receipt"}, {"Fairy tale", "Glass Slipper Firewall"},
            {"Animal kingdom", "Parliament of Owls"}, {"1001 Nights", "The Brass Moon"},
            {"Norse", "Bifrost Maintenance"}, {"Fairy tale", "Three Bears, One Thermostat"},
            {"Animal kingdom", "Beaver Dam Bureaucracy"}, {"1001 Nights", "The Djinn's Fine Print"},
            {"Norse", "Loki's Honest Maze"}, {"Fairy tale", "Rapunzel's Escape Room"},
            {"Animal kingdom", "Octopus Switchboard"}, {"1001 Nights", "Clockwork Caravan"},
            {"Norse", "The Wolf and Sundial"}, {"Fairy tale", "Breadcrumb Protocol"},
            {"Animal kingdom", "Murmuration Station"}, {"1001 Nights", "Library of Names"},
            {"Norse", "Mjolnir Lost Property"}, {"Fairy tale", "Mirror, Mirror, Offline"},
            {"Animal kingdom", "The Coral Court"}, {"1001 Nights", "Forty Doors, One Key"},
            {"Norse", "Nine Realms Night Shift"}, {"Fairy tale", "Dragon Performance Review"},
            {"Animal kingdom", "Migration Control"}, {"All worlds", "The Star at World's End"}
        };

        // Day 1 puzzle: "The Raven's Receipt". Same four-object, three-rule shape validated
        // in the design prototype — ported 1:1 rather than redesigned, since the logic is
        // already proven solvable in exactly three moves.
        private static readonly string[,] PuzzleThings =
        {
            {"receipt", "Overdue Receipt", "must be filed"},
            {"feather", "Raven Feather", "cannot hold LOUD"},
            {"seal", "Lead Seal", "heavy as sin"},
            {"desk", "Auditor's Desk", "auditor sleeps here"}
        };
        private static readonly string[] PuzzleStart = {"LOUD", "FLIGHT", "HEAVY", null};

        private const string ProgressKey = "wintergate-progress";
        private Font _font;
        private RectTransform _calendarGrid;
        private GameObject _codeModal;
        private InputField _codeInput;
        private Text _mainStatus;
        private Text _modalStatus;
        private int _progress;

        private GameObject _puzzlePanel;
        private RectTransform _puzzleGrid;
        private Text _puzzleSay;
        private Text _puzzleMovesText;
        private Dictionary<string, string> _puzzleState;
        private string _puzzleHeld;
        private int _puzzleMoves;
        private bool _puzzleSolved;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureAppExists()
        {
            if (FindFirstObjectByType<WintergateCalendarApp>() == null)
                new GameObject("Wintergate Calendar App").AddComponent<WintergateCalendarApp>();
        }

        private void Awake()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _progress = Mathf.Clamp(PlayerPrefs.GetInt(ProgressKey, 7), 0, 24);
            BuildWorld();
        }

        private void BuildWorld()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                cameraObject.transform.position = new Vector3(0, 1.55f, 0);
                camera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
                cameraObject.AddComponent<QuestHeadTracking>();
                camera.nearClipPlane = 0.05f;
                camera.farClipPlane = 100f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Night;
            }

            // Include inactive instances in the search: a disabled leftover EventSystem would
            // otherwise be invisible to FindFirstObjectByType's default (active-only) search,
            // so we'd skip creating a working one and end up with zero input processing and
            // no error anywhere, since a disabled GameObject just silently does nothing.
            EventSystem existingEventSystem = FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
            if (existingEventSystem == null)
            {
                GameObject eventSystem = new GameObject("Event System");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
                eventSystem.SetActive(true);
            }
            else if (!existingEventSystem.gameObject.activeSelf)
            {
                existingEventSystem.gameObject.SetActive(true);
            }

            GameObject canvasObject = new GameObject("Wintergate Calendar Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = camera;
            canvas.sortingOrder = 10;
            canvasObject.AddComponent<GraphicRaycaster>();

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(1500, 900);
            canvasRect.position = new Vector3(0, 1.55f, 2.8f);
            canvasRect.rotation = Quaternion.Euler(0, 180, 0);
            canvasRect.localScale = new Vector3(-0.00175f, 0.00175f, 0.00175f);

            Image backdrop = canvasObject.AddComponent<Image>();
            backdrop.color = Night;

            RectTransform header = PanelRect(canvasRect, "Header", new Vector2(0, 360), new Vector2(1420, 120), Panel);
            AddText(header, "WINTERGATE", 40, FontStyle.Bold, Gold, TextAnchor.MiddleLeft,
                new Vector2(42, 0), new Vector2(500, 90));
            AddText(header, "A 24-chapter puzzle adventure", 24, FontStyle.Normal, Muted, TextAnchor.MiddleRight,
                new Vector2(820, 0), new Vector2(360, 90));
            Button quitButton = AddButton(header, "Quit", new Vector2(1250, 0), new Vector2(150, 60), Panel2, Muted);
            quitButton.onClick.AddListener(Application.Quit);

            RectTransform intro = PanelRect(canvasRect, "Intro", new Vector2(0, 245), new Vector2(1420, 90), Night);
            AddText(intro, "THE STOLEN NORTH STAR", 22, FontStyle.Bold, Teal, TextAnchor.UpperLeft,
                new Vector2(10, -2), new Vector2(600, 36));
            AddText(intro, "Complete today's chapter to reveal tomorrow's door.", 30, FontStyle.Normal, Ink,
                TextAnchor.MiddleLeft, new Vector2(10, -34), new Vector2(1000, 48));
            Button codeButton = AddButton(intro, "Caretaker code", new Vector2(1090, -5), new Vector2(300, 65), Panel2, Ink);
            codeButton.onClick.AddListener(() => _codeModal.SetActive(true));

            _calendarGrid = PanelRect(canvasRect, "Calendar Grid", new Vector2(0, -50), new Vector2(1420, 500), Night);
            GridLayoutGroup grid = _calendarGrid.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(220, 110);
            grid.spacing = new Vector2(18, 18);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 6;
            grid.childAlignment = TextAnchor.MiddleCenter;
            RebuildDoors();

            _mainStatus = AddText(canvasRect, "Day 08 is available. The Bifrost appears to be experiencing scheduled unscheduled maintenance.",
                23, FontStyle.Italic, Muted, TextAnchor.MiddleLeft, new Vector2(40, -365), new Vector2(1340, 65));

            BuildCodeModal(canvasRect);
            BuildPuzzlePanel(canvasRect);
            canvasObject.AddComponent<QuestCanvasPointer>().TargetCanvas = canvas;
        }

        private void RebuildDoors()
        {
            foreach (Transform child in _calendarGrid) Destroy(child.gameObject);

            for (int index = 0; index < 24; index++)
            {
                int day = index + 1;
                bool done = day <= _progress;
                bool available = day == _progress + 1 || _progress == 24;
                Color background = done ? new Color(0.08f, 0.20f, 0.20f, 1) : available ? Panel2 : Panel;
                Color border = done ? Teal : available ? Gold : Line;
                string marker = done ? "  COMPLETE" : available ? "  AVAILABLE" : "  LOCKED";

                GameObject doorObject = new GameObject($"Day {day:00}", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Button));
                doorObject.transform.SetParent(_calendarGrid, false);
                Image image = doorObject.GetComponent<Image>();
                image.color = background;
                Outline outline = doorObject.GetComponent<Outline>();
                outline.effectColor = border;
                outline.effectDistance = new Vector2(2, -2);
                Button button = doorObject.GetComponent<Button>();
                button.interactable = done || available;
                int capturedDay = day;
                button.onClick.AddListener(() => SelectDay(capturedDay));

                RectTransform rect = doorObject.GetComponent<RectTransform>();
                AddText(rect, $"{day:00}{marker}\n{Days[index, 0]}\n{Days[index, 1]}", 18, FontStyle.Bold,
                    done || available ? Ink : Muted, TextAnchor.MiddleLeft, new Vector2(14, 0), new Vector2(192, 100));
            }
        }

        private void SelectDay(int day)
        {
            if (day == 1)
            {
                OpenPuzzle();
                return;
            }

            if (day <= _progress)
                _mainStatus.text = $"Day {day:00}: {Days[day - 1, 1]} — complete and ready to replay.";
            else
                _mainStatus.text = $"Day {day:00}: {Days[day - 1, 1]} — today's chapter will begin here in the next prototype.";
        }

        private void OpenPuzzle()
        {
            _puzzleState = new Dictionary<string, string>();
            for (int i = 0; i < PuzzleThings.GetLength(0); i++)
                _puzzleState[PuzzleThings[i, 0]] = PuzzleStart[i];

            _puzzleHeld = null;
            _puzzleMoves = 0;
            _puzzleSolved = false;
            _puzzleMovesText.text = "Moves: 0";
            _puzzleSay.text = "Day 01 — The Raven's Receipt. Lend FLIGHT to the receipt so it can be filed before the Bifrost audit closes. Pick a property, then drop it on an empty desk.";
            RenderPuzzleThings();
            _puzzlePanel.SetActive(true);
        }

        private void PuzzleCardClicked(string id)
        {
            if (_puzzleSolved) return;
            if (_puzzleState[id] != null) PuzzlePick(id);
            else PuzzleDrop(id);
        }

        private void PuzzlePick(string id)
        {
            _puzzleHeld = _puzzleHeld == id ? null : id;
            _puzzleSay.text = _puzzleHeld == null
                ? "Released. Pick a property to carry it."
                : $"Carrying {_puzzleState[id]}. Drop it on an empty desk.";
            RenderPuzzleThings();
        }

        private void PuzzleDrop(string id)
        {
            if (_puzzleHeld == null) return;
            string carried = _puzzleState[_puzzleHeld];

            if (id == "desk" && carried == "LOUD")
            {
                _puzzleSay.text = "Rule two: nothing LOUD rests on the auditor's desk. The auditor wakes. Try somewhere quieter.";
                return;
            }
            if (id == "feather" && carried == "LOUD")
            {
                _puzzleSay.text = "Rule three: a raven feather cannot carry LOUD. It can barely carry itself.";
                return;
            }

            _puzzleState[id] = carried;
            _puzzleState[_puzzleHeld] = null;
            _puzzleHeld = null;
            _puzzleMoves++;
            _puzzleMovesText.text = $"Moves: {_puzzleMoves}";

            if (_puzzleState["receipt"] == "FLIGHT")
            {
                _puzzleSolved = true;
                _puzzleSay.text = $"The receipt lifts off and files itself. {_puzzleMoves} move{(_puzzleMoves == 1 ? "" : "s")} — the minimum is three.";
                CompletePuzzleDay(1);
            }
            else
            {
                _puzzleSay.text = "Filed elsewhere. Keep going.";
            }
            RenderPuzzleThings();
        }

        private void CompletePuzzleDay(int day)
        {
            _mainStatus.text = $"Day {day:00}: {Days[day - 1, 1]} — filed. The Bifrost audit grudgingly accepts it.";
            if (day == _progress + 1)
            {
                _progress = Mathf.Min(24, _progress + 1);
                PlayerPrefs.SetInt(ProgressKey, _progress);
                PlayerPrefs.Save();
                RebuildDoors();
            }
        }

        private void BuildPuzzlePanel(RectTransform root)
        {
            _puzzlePanel = PanelRect(root, "Door Puzzle Panel", Vector2.zero, new Vector2(1180, 680), Panel).gameObject;
            _puzzlePanel.AddComponent<Outline>().effectColor = Teal;
            RectTransform panel = _puzzlePanel.GetComponent<RectTransform>();

            AddText(panel, "DOOR ONE", 22, FontStyle.Bold, Teal, TextAnchor.MiddleLeft,
                new Vector2(44, 300), new Vector2(500, 44));
            AddText(panel, "The Raven's Receipt", 38, FontStyle.Bold, Ink, TextAnchor.MiddleLeft,
                new Vector2(44, 252), new Vector2(700, 56));

            _puzzleGrid = PanelRect(panel, "Puzzle Grid", new Vector2(0, 60), new Vector2(1090, 300), Panel);
            GridLayoutGroup grid = _puzzleGrid.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(255, 150);
            grid.spacing = new Vector2(18, 18);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 4;
            grid.childAlignment = TextAnchor.MiddleCenter;

            _puzzleSay = AddText(panel, "", 23, FontStyle.Italic, Muted, TextAnchor.UpperLeft,
                new Vector2(44, -120), new Vector2(1090, 80));
            _puzzleMovesText = AddText(panel, "Moves: 0", 20, FontStyle.Bold, Gold, TextAnchor.MiddleLeft,
                new Vector2(44, -185), new Vector2(300, 40));

            Button resetButton = AddButton(panel, "Reset", new Vector2(760, -185), new Vector2(170, 55), Panel2, Ink);
            resetButton.onClick.AddListener(OpenPuzzle);
            Button closeButton = AddButton(panel, "Return to calendar", new Vector2(950, -185), new Vector2(300, 55), Gold, Night);
            closeButton.onClick.AddListener(() => _puzzlePanel.SetActive(false));

            _puzzlePanel.SetActive(false);
        }

        private void RenderPuzzleThings()
        {
            foreach (Transform child in _puzzleGrid) Destroy(child.gameObject);

            for (int i = 0; i < PuzzleThings.GetLength(0); i++)
            {
                string id = PuzzleThings[i, 0];
                string name = PuzzleThings[i, 1];
                string note = PuzzleThings[i, 2];
                string prop = _puzzleState[id];
                bool isHeld = _puzzleHeld == id;
                bool isDroppable = prop == null && _puzzleHeld != null;

                GameObject card = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Button));
                card.transform.SetParent(_puzzleGrid, false);
                card.GetComponent<Image>().color = isHeld ? new Color(0.14f, 0.26f, 0.26f, 1) : Panel2;
                Outline outline = card.GetComponent<Outline>();
                outline.effectColor = isHeld ? Gold : isDroppable ? Teal : Line;
                outline.effectDistance = new Vector2(2, -2);
                card.GetComponent<Button>().onClick.AddListener(() => PuzzleCardClicked(id));

                string label = prop != null ? $"{(isHeld ? "» " : "")}{prop}" : isDroppable ? "— drop here —" : "— empty —";
                AddText(card.GetComponent<RectTransform>(), $"{name}\n{note}\n\n{label}", 17, FontStyle.Bold,
                    Ink, TextAnchor.UpperLeft, new Vector2(14, -8), new Vector2(230, 130));
            }
        }

        private void BuildCodeModal(RectTransform root)
        {
            _codeModal = PanelRect(root, "Caretaker Code Modal", Vector2.zero, new Vector2(820, 420), Panel).gameObject;
            _codeModal.AddComponent<Outline>().effectColor = Gold;
            RectTransform modal = _codeModal.GetComponent<RectTransform>();
            AddText(modal, "CARETAKER CONSOLE", 23, FontStyle.Bold, Gold, TextAnchor.MiddleLeft,
                new Vector2(40, 150), new Vector2(600, 50));
            AddText(modal, "Emergency keys", 44, FontStyle.Bold, Ink, TextAnchor.MiddleLeft,
                new Vector2(40, 95), new Vector2(600, 60));
            AddText(modal, "Enter YULE-KEY for the next door or RAVEN-24 for every door.", 24, FontStyle.Normal, Muted,
                TextAnchor.MiddleLeft, new Vector2(40, 35), new Vector2(730, 65));

            GameObject inputObject = new GameObject("Code Input", typeof(RectTransform), typeof(Image), typeof(InputField));
            inputObject.transform.SetParent(modal, false);
            RectTransform inputRect = inputObject.GetComponent<RectTransform>();
            inputRect.anchorMin = inputRect.anchorMax = new Vector2(0, 0.5f);
            inputRect.pivot = new Vector2(0, 0.5f);
            inputRect.anchoredPosition = new Vector2(40, -55);
            inputRect.sizeDelta = new Vector2(500, 70);
            inputObject.GetComponent<Image>().color = Night;
            Text inputText = AddText(inputRect, "", 27, FontStyle.Bold, Ink, TextAnchor.MiddleLeft,
                new Vector2(16, 0), new Vector2(460, 65));
            Text placeholder = AddText(inputRect, "ENTER-CODE", 27, FontStyle.Italic, Muted, TextAnchor.MiddleLeft,
                new Vector2(16, 0), new Vector2(460, 65));
            _codeInput = inputObject.GetComponent<InputField>();
            _codeInput.textComponent = inputText;
            _codeInput.placeholder = placeholder;
            _codeInput.characterLimit = 20;

            Button apply = AddButton(modal, "Apply", new Vector2(570, -55), new Vector2(200, 70), Gold, Night);
            apply.onClick.AddListener(ApplyCode);
            Button close = AddButton(modal, "Close", new Vector2(570, -145), new Vector2(200, 55), Panel2, Ink);
            close.onClick.AddListener(() => _codeModal.SetActive(false));
            _modalStatus = AddText(modal, "", 20, FontStyle.Bold, Teal, TextAnchor.MiddleLeft,
                new Vector2(40, -145), new Vector2(500, 55));
            _codeModal.SetActive(false);
        }

        private void ApplyCode()
        {
            string code = _codeInput.text.Trim().ToUpperInvariant();
            if (code == "YULE-KEY")
            {
                _progress = Mathf.Min(24, _progress + 1);
                _modalStatus.text = "The next door clicks open.";
            }
            else if (code == "RAVEN-24")
            {
                _progress = 24;
                _modalStatus.text = "Every door is now available.";
            }
            else
            {
                _modalStatus.text = "The lock politely pretends it did not hear that.";
                return;
            }
            PlayerPrefs.SetInt(ProgressKey, _progress);
            PlayerPrefs.Save();
            RebuildDoors();
        }

        private static RectTransform PanelRect(RectTransform parent, string name, Vector2 position, Vector2 size, Color color)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            gameObject.transform.SetParent(parent, false);
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            gameObject.GetComponent<Image>().color = color;
            return rect;
        }

        private Text AddText(RectTransform parent, string value, int size, FontStyle style, Color color,
            TextAnchor alignment, Vector2 position, Vector2 dimensions)
        {
            GameObject gameObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
            gameObject.transform.SetParent(parent, false);
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0, 0.5f);
            rect.pivot = new Vector2(0, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = dimensions;
            Text text = gameObject.GetComponent<Text>();
            text.font = _font;
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private Button AddButton(RectTransform parent, string label, Vector2 position, Vector2 size, Color background, Color foreground)
        {
            GameObject gameObject = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            gameObject.transform.SetParent(parent, false);
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0, 0.5f);
            rect.pivot = new Vector2(0, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            gameObject.GetComponent<Image>().color = background;
            AddText(rect, label, 23, FontStyle.Bold, foreground, TextAnchor.MiddleCenter, Vector2.zero, size);
            return gameObject.GetComponent<Button>();
        }

        private static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString("#" + value, out Color color);
            return color;
        }
    }

    public sealed class QuestHeadTracking : MonoBehaviour
    {
        private void Update()
        {
            InputDevice head = InputDevices.GetDeviceAtXRNode(XRNode.Head);
            if (head.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position))
            {
                // Desktop/mock runtimes commonly report a zero-height head pose. Keep the
                // calendar at comfortable eye height there while preserving room-scale poses.
                if (position.y < 0.5f) position.y += 1.55f;
                transform.localPosition = position;
            }
            if (head.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation)) transform.localRotation = rotation;
        }
    }

    public sealed class QuestCanvasPointer : MonoBehaviour
    {
        public Canvas TargetCanvas { get; set; }
        private readonly List<RaycastResult> _hits = new List<RaycastResult>();
        private InputDevice _controller;
        private bool _wasPressed;
        private GameObject _hovered;

        private void Update()
        {
            if (!_controller.isValid) _controller = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            if (!_controller.isValid || TargetCanvas == null || Camera.main == null) return;
            if (!_controller.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 origin)) return;
            if (!_controller.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation)) return;

            Ray ray = new Ray(origin, rotation * Vector3.forward);
            Plane plane = new Plane(TargetCanvas.transform.forward, TargetCanvas.transform.position);
            if (!plane.Raycast(ray, out float distance)) return;
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector2 screenPoint = Camera.main.WorldToScreenPoint(hitPoint);
            PointerEventData eventData = new PointerEventData(EventSystem.current) { position = screenPoint };
            _hits.Clear();
            TargetCanvas.GetComponent<GraphicRaycaster>().Raycast(eventData, _hits);
            GameObject current = _hits.Count > 0 ? _hits[0].gameObject : null;

            if (current != _hovered)
            {
                if (_hovered != null) ExecuteEvents.Execute(_hovered, eventData, ExecuteEvents.pointerExitHandler);
                if (current != null) ExecuteEvents.Execute(current, eventData, ExecuteEvents.pointerEnterHandler);
                _hovered = current;
            }

            _controller.TryGetFeatureValue(CommonUsages.triggerButton, out bool pressed);
            if (pressed && !_wasPressed && current != null)
            {
                ExecuteEvents.Execute(current, eventData, ExecuteEvents.pointerDownHandler);
                ExecuteEvents.Execute(current, eventData, ExecuteEvents.pointerUpHandler);
                ExecuteEvents.Execute(current, eventData, ExecuteEvents.pointerClickHandler);
            }
            _wasPressed = pressed;
        }
    }
}
