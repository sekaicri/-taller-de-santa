using System.Collections;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace VrClass.Tools
{
    /// <summary>
    /// The stage each course day adds on top of the Day 03 packing table.
    /// Day 4 sign (TextMeshPro + Christmas sprites) with order list, start button and help, Day 6 feedback, Day 7 timer, pause and restart, Day 5 locomotion,
    /// Day 6 feedback, Day 7 order/score/timer, Day 8 decoration.
    /// Apply(day) builds every stage up to that day, so each day's scene contains the previous ones.
    /// </summary>
    public static class WorkshopStages
    {
        const string MATERIALS_FOLDER = "Assets/Common/Materials";
        const string SPRITES_FOLDER = "Assets/Common/UI/Sprites";
        const string PREFABS_FOLDER = "Assets/Common/Prefabs";
        const string GAME_NAME = "Juego de empaque";
        const string CANVAS_NAME = "Canvas del pedido";
        const string START_NAME = "Boton Empezar";

        static readonly Vector3 CANVAS_POSITION = new Vector3(0f, 1.7f, 2.1f);
        const float CANVAS_SCALE = 0.0018f;
        static readonly Vector2 CANVAS_SIZE = new Vector2(1040f, 778f);
        static readonly Color PANEL_COLOR = new Color(0.08f, 0.2f, 0.12f, 0.92f);
        static readonly Color CREAM_TEXT = new Color(1f, 0.96f, 0.8f);
        static readonly Color GOLD_TEXT = new Color(1f, 0.85f, 0.35f);
        static readonly Color BROWN_TEXT = new Color(0.29f, 0.15f, 0.06f);
        static readonly Color BUTTON_COLOR = new Color(0.75f, 0.12f, 0.12f);

        // Day 7: the side panels, the difficulty levels (name, seconds, belt speed), and the stars.
        static readonly Vector2 SIDE_PANEL_SIZE = new Vector2(420f, 275f);
        const float SIDE_PANEL_X = 1.36f;
        const float SIDE_PANEL_Y = 1.5f;
        static readonly string[] LEVEL_NAMES = { "Fácil", "Normal", "Difícil" };
        static readonly float[] LEVEL_TIMES = { 120f, 90f, 60f };
        static readonly float[] LEVEL_SPEEDS = { 0.05f, 0.08f, 0.14f };
        const int STAR_COUNT = 3;

        const int PENALTY = 25;   // Day 6: points lost for a wrong toy
        static readonly string[] ORDER = { "Bola", "Baston", "Osito" };
        static readonly string[] ORDER_LABELS = { "Bola", "Bastón", "Osito" };

        // Builds every stage from Day 4 up to the given day.
        public static void Apply(int day)
        {
            var game = BuildGame(day);
            if (day >= 5) BuildLocomotion(game);
            if (day >= 8) CallDecoration();
            Debug.Log("Stage " + day + " ready: " + game.name + " configured.");
        }

        // Day 8 decoration lives in the Day 08 builder; it is found by name so Days 4-7 compile without that file.
        static void CallDecoration()
        {
            var type = System.Type.GetType("Day08.Editor.Day08SceneBuilder");
            var method = type?.GetMethod("DecorateWorkshop");
            if (method == null) { Debug.LogWarning("Day08SceneBuilder.cs not found: download it to decorate the scene."); return; }
            method.Invoke(null, null);
        }

        // Day 4 (sign, order list, buttons), Day 6 (feedback) and Day 7 (timer, pause and restart).
        // The sign is organized in groups: Fondo > Encabezado, Tarjeta del pedido, Placa de estado, Placa de puntos (Day 6), Botones, Tiempo, Panel de ayuda.
        static PackingGame BuildGame(int day)
        {
            EnsureEventSystem();
            var gameObject = new GameObject(GAME_NAME);
            var game = gameObject.AddComponent<PackingGame>();

            var socket = Object.FindFirstObjectByType<XRSocketInteractor>();
            var spawner = Object.FindFirstObjectByType<Day03.ToySpawner>();
            if (spawner != null) spawner.AutoStart = false;   // the factory starts when the player presses Start
            var clearer = socket == null ? null : socket.GetComponent<Day03.SocketClearer>();
            if (clearer != null) Object.DestroyImmediate(clearer);   // the game now handles the crate
            Wire(game, "_belt", Object.FindFirstObjectByType<Day03.ConveyorBelt>());
            Wire(game, "_spawner", spawner);
            Wire(game, "_socket", socket);
            Wire(game, "_toysToPack", ORDER);
            if (day >= 6)
            {
                // Day 6: a wrong toy is thrown far away, every round asks for a new random order and a wrong toy costs points.
                Wire(game, "_throwRejected", true);
                var picker = gameObject.AddComponent<OrderPicker>();
                Wire(picker, "_game", game);
                Wire(picker, "_toyPool", ORDER);
                Wire(picker, "_orderSize", ORDER.Length);
                var penalty = gameObject.AddComponent<WrongToyPenalty>();
                Wire(penalty, "_game", game);
                Wire(penalty, "_points", PENALTY);
                AddToyFeedback(spawner);
            }

            var canvasObject = new GameObject(CANVAS_NAME, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(TrackedDeviceGraphicRaycaster));
            canvasObject.transform.position = CANVAS_POSITION;
            canvasObject.transform.localScale = Vector3.one * CANVAS_SCALE;
            canvasObject.AddComponent<Billboard>();
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            canvasObject.GetComponent<CanvasScaler>().dynamicPixelsPerUnit = 6f;
            var canvasRect = (RectTransform)canvasObject.transform;
            canvasRect.sizeDelta = CANVAS_SIZE;

            // Christmas frame: the whole sign is built on top of it (the sprites come from UI_Navidad.png).
            var panel = CreateSprite(canvasRect, "Fondo", UiSprites.Get("Marco"), CANVAS_SIZE, Vector2.zero, PANEL_COLOR);

            // Header: gift icon and a red ribbon with the title inside (the help button joins it later).
            // The group hangs from the top edge of the frame; the gift sticks to its left side and the help button to its right side.
            var header = CreateGroup(panel, "Encabezado", new Vector2(840f, 135f), new Vector2(12f, -281f));
            header.anchorMin = header.anchorMax = new Vector2(0.5f, 1f);
            header.anchoredPosition = new Vector2(12f, -281f);
            var gift = CreateSprite(header, "Icono regalo", UiSprites.Get("Regalo"), new Vector2(84f, 86f), new Vector2(42f, 0f), BUTTON_COLOR);
            gift.anchorMin = gift.anchorMax = new Vector2(0f, 0.5f);
            gift.anchoredPosition = new Vector2(42f, 0f);
            var ribbon = CreateSprite(header, "Cinta", UiSprites.Get("Cinta"), new Vector2(507f, 112f), Vector2.zero, BUTTON_COLOR);
            // The banner part of the ribbon sits above its center, so the title is lifted a little and kept on one line.
            var title = CreateText(ribbon, "Titulo", "TALLER DE SANTA", 28, new Vector2(300f, 46f), new Vector2(0f, 16f), TextAlignmentOptions.Center, CREAM_TEXT, FontStyles.Bold);
            title.textWrappingMode = TextWrappingModes.NoWrap;

            BuildOrderCard(panel, game);

            // Status plaque with its text inside: shows the countdown and the messages of the game.
            var plaque = CreateSprite(panel, "Placa de estado", UiSprites.Get("Placa"), new Vector2(430f, 40f), new Vector2(0f, -205f), PANEL_COLOR, true);
            Wire(game, "_statusText", CreateText(plaque, "Texto", "Aprieta Empezar", 28, new Vector2(410f, 36f), Vector2.zero, TextAlignmentOptions.Center, GOLD_TEXT, FontStyles.Bold));

            // Day 6: the live score, between the order card and the status plaque.
            if (day >= 6) BuildScore(panel, game);

            // Buttons group. The Start button hides while a round runs.
            var buttons = CreateGroup(panel, "Botones", new Vector2(840f, 64f), new Vector2(0f, -257f));
            var start = CreateButton(buttons, START_NAME, "EMPEZAR", new Vector2(200f, 42f), Vector2.zero, 26, UiSprites.Get("BotonRojo"), Color.white);
            Wire(game, "_startButton", start.gameObject);
            UnityEventTools.AddVoidPersistentListener(start.onClick, game.Begin);

            var timer = day >= 7 ? BuildTimer(panel, buttons, gameObject, game) : null;
            var resultCard = BuildResult(panel, gameObject, game);
            if (day >= 7) BuildRoundExtras(gameObject, game, timer, resultCard);
            BuildHelp(panel, header, day);   // last, so the open help panel is drawn above everything else
            if (day >= 6) BuildFeedback(gameObject, game);
            return game;
        }

        // Day 6: the live score under the order card ("Puntos: 0", with a green +100 or a red -25 next to it).
        static void BuildScore(RectTransform panel, PackingGame game)
        {
            var plaque = CreateSprite(panel, "Placa de puntos", UiSprites.Get("Placa"), new Vector2(300f, 40f), new Vector2(0f, -143f), PANEL_COLOR, true);
            var label = CreateText(plaque, "Texto", "Puntos: 0", 26, new Vector2(280f, 36f), Vector2.zero, TextAlignmentOptions.Center, GOLD_TEXT, FontStyles.Bold);
            label.textWrappingMode = TextWrappingModes.NoWrap;
            var score = game.gameObject.AddComponent<ScoreDisplay>();
            Wire(score, "_game", game);
            Wire(score, "_label", label);
        }

        // Day 6: the sounds, a warning light and gold sparks above the crate, and the vibration of the controllers.
        static void BuildFeedback(GameObject gameObject, PackingGame game)
        {
            var audio = gameObject.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.volume = 0.7f;
            audio.spatialBlend = 0f;   // the sounds come from the game, not from a place in the room

            var lightObject = new GameObject("Luz de aviso");
            lightObject.transform.position = new Vector3(1.2f, 1.4f, 0.9f);
            var warning = lightObject.AddComponent<Light>();
            warning.type = LightType.Point; warning.range = 3f; warning.intensity = 0f;

            var feedback = gameObject.AddComponent<GameFeedback>();
            Wire(feedback, "_game", game);
            Wire(feedback, "_sound", audio);
            Wire(feedback, "_flash", warning);
            Wire(feedback, "_sparks", BuildSparks());

            var haptics = gameObject.AddComponent<GameHaptics>();
            Wire(haptics, "_game", game);
        }

        // A short burst of gold dots over the crate (it plays when a toy is accepted).
        static ParticleSystem BuildSparks()
        {
            var sparksObject = new GameObject("Chispas");
            sparksObject.transform.position = new Vector3(1.2f, 1.0f, 0.9f);
            var sparks = sparksObject.AddComponent<ParticleSystem>();
            sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);   // a new system may be playing: it must be stopped to change its duration

            var main = sparks.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 0.5f;
            main.startLifetime = 0.9f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.2f, 2.2f);       // each star flies at its own speed
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.16f);       // and has its own size
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main.startColor = new ParticleSystem.MinMaxGradient(Color.white, new Color(1f, 0.95f, 0.7f));   // gold stars, some lighter
            main.gravityModifier = 0.5f;
            main.maxParticles = 40;

            var emission = sparks.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 30) });

            var shape = sparks.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;

            // The stars shrink to nothing while they spin, so they fade away instead of vanishing suddenly.
            var size = sparks.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));
            var spin = sparks.rotationOverLifetime;
            spin.enabled = true;
            spin.z = new ParticleSystem.MinMaxCurve(180f * Mathf.Deg2Rad);

            sparksObject.GetComponent<ParticleSystemRenderer>().sharedMaterial = CreateSparkMaterial();
            return sparks;
        }

        // The material of the sparks: the URP unlit particle shader with the star picture. Alpha Clipping cuts the empty corners of the picture.
        static Material CreateSparkMaterial()
        {
            if (!AssetDatabase.IsValidFolder(MATERIALS_FOLDER)) AssetDatabase.CreateFolder("Assets/Common", "Materials");
            var path = MATERIALS_FOLDER + "/Chispas.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                if (shader == null) shader = Shader.Find("Sprites/Default");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            var star = AssetDatabase.LoadAssetAtPath<Texture2D>(SPRITES_FOLDER + "/Estrella.png");
            if (star != null) material.SetTexture("_BaseMap", star);
            material.SetFloat("_AlphaClip", 1f);
            material.SetFloat("_Cutoff", 0.5f);
            material.EnableKeyword("_ALPHATEST_ON");
            material.renderQueue = 2450;
            EditorUtility.SetDirty(material);
            return material;
        }

        // Day 6: sound and vibration in the hand when a toy is grabbed or wrapped (on every toy template of the factory).
        static void AddToyFeedback(Day03.ToySpawner spawner)
        {
            if (spawner == null) return;
            var templates = new SerializedObject(spawner).FindProperty("_templates");
            for (var i = 0; i < templates.arraySize; i++)
            {
                var template = templates.GetArrayElementAtIndex(i).objectReferenceValue as GameObject;
                if (template != null && template.GetComponent<ToyFeedback>() == null) template.AddComponent<ToyFeedback>();
            }
        }

        // Day 7: time group (bar with its text and a star), the clock script, and the Pause and Restart buttons.
        static GameTimer BuildTimer(RectTransform panel, RectTransform buttons, GameObject gameObject, PackingGame game)
        {
            // Time bar: a green strip whose width shrinks with the remaining time (pivot on the left).
            var time = CreateGroup(panel, "Tiempo");
            var back = CreateSprite(time, "Barra fondo", UiSprites.Get("BarraFondo"), new Vector2(410f, 16f), new Vector2(-12f, -232f), new Color(0f, 0f, 0f, 0.5f), true);
            var bar = CreateSprite(back, "Barra de tiempo", UiSprites.Get("BarraRelleno"), new Vector2(402f, 12f), new Vector2(-201f, 0f), new Color(0.35f, 0.9f, 0.4f), true);
            bar.pivot = new Vector2(0f, 0.5f); bar.anchoredPosition = new Vector2(-201f, 0f);
            var timeText = CreateText(back, "Texto", "", 13, new Vector2(400f, 16f), Vector2.zero, TextAlignmentOptions.Center, Color.white, FontStyles.Bold);
            CreateSprite(time, "Estrella", UiSprites.Get("Estrella"), new Vector2(28f, 26f), new Vector2(236f, -232f), GOLD_TEXT);

            var timer = gameObject.AddComponent<GameTimer>();
            Wire(timer, "_game", game);
            Wire(timer, "_timeText", timeText);
            Wire(timer, "_timeBar", bar);

            var pause = CreateButton(buttons, "Boton Pausa", "PAUSA", new Vector2(130f, 42f), new Vector2(-290f, 0f), 24, UiSprites.Get("BotonAzul"), Color.white);
            var restart = CreateButton(buttons, "Boton Reiniciar", "REINICIAR", new Vector2(130f, 42f), new Vector2(290f, 0f), 20, UiSprites.Get("BotonNaranja"), Color.white);
            var pauseScript = gameObject.AddComponent<GamePause>();
            Wire(pauseScript, "_game", game);
            Wire(pauseScript, "_pauseLabel", pause.GetComponentInChildren<TMP_Text>());
            UnityEventTools.AddVoidPersistentListener(pause.onClick, pauseScript.Toggle);
            UnityEventTools.AddVoidPersistentListener(restart.onClick, pauseScript.Restart);
            return timer;
        }

        // Day 7: stars and "new record" on the result card, the difficulty panel, the streak and record panel.
        static void BuildRoundExtras(GameObject gameObject, PackingGame game, GameTimer timer, RectTransform resultCard)
        {
            // Result card: three stars between the points and the button, and a sign for a new record above the title.
            var stars = new Image[STAR_COUNT];
            for (var i = 0; i < STAR_COUNT; i++)
                stars[i] = CreateSprite(resultCard, "Estrella " + (i + 1), UiSprites.Get("Estrella"), new Vector2(34f, 32f), new Vector2((i - 1) * 44f, -38f), GOLD_TEXT).GetComponent<Image>();
            var rating = gameObject.AddComponent<StarRating>();
            Wire(rating, "_game", game);
            Wire(rating, "_stars", stars);

            var newRecord = CreateText(resultCard, "Nuevo record", "¡NUEVO RÉCORD!", 26, new Vector2(400f, 36f), new Vector2(0f, 128f), TextAlignmentOptions.Center, BUTTON_COLOR, FontStyles.Bold);
            newRecord.gameObject.SetActive(false);

            // Left of the sign: the three difficulty levels.
            var difficultyPanel = CreateSidePanel("Panel de dificultad", new Vector3(-SIDE_PANEL_X, SIDE_PANEL_Y, CANVAS_POSITION.z));
            CreateText(difficultyPanel, "Titulo", "DIFICULTAD", 30, new Vector2(380f, 44f), new Vector2(0f, 90f), TextAlignmentOptions.Center, BROWN_TEXT, FontStyles.Bold);
            var levelButtons = new[]
            {
                CreateButton(difficultyPanel, "Boton Facil", "FÁCIL", new Vector2(120f, 50f), new Vector2(-130f, 15f), 22, UiSprites.Get("BotonAzul"), Color.white),
                CreateButton(difficultyPanel, "Boton Normal", "NORMAL", new Vector2(120f, 50f), new Vector2(0f, 15f), 22, UiSprites.Get("BotonNaranja"), Color.white),
                CreateButton(difficultyPanel, "Boton Dificil", "DIFÍCIL", new Vector2(120f, 50f), new Vector2(130f, 15f), 22, UiSprites.Get("BotonRojo"), Color.white),
            };
            var info = CreateText(difficultyPanel, "Texto", "", 22, new Vector2(380f, 40f), new Vector2(0f, -60f), TextAlignmentOptions.Center, BROWN_TEXT, FontStyles.Normal);
            var picker = gameObject.AddComponent<DifficultyPicker>();
            Wire(picker, "_game", game);
            Wire(picker, "_timer", timer);
            Wire(picker, "_buttons", levelButtons);
            Wire(picker, "_info", info);
            var pickerData = new SerializedObject(picker);
            var levels = pickerData.FindProperty("_levels");
            levels.arraySize = LEVEL_NAMES.Length;
            for (var i = 0; i < LEVEL_NAMES.Length; i++)
            {
                var level = levels.GetArrayElementAtIndex(i);
                level.FindPropertyRelative("name").stringValue = LEVEL_NAMES[i];
                level.FindPropertyRelative("timeLimit").floatValue = LEVEL_TIMES[i];
                level.FindPropertyRelative("beltSpeed").floatValue = LEVEL_SPEEDS[i];
                UnityEventTools.AddIntPersistentListener(levelButtons[i].onClick, picker.Choose, i);
            }
            pickerData.ApplyModifiedPropertiesWithoutUndo();

            // Right of the sign: the streak and the best score.
            var scorePanel = CreateSidePanel("Panel de marcador", new Vector3(SIDE_PANEL_X, SIDE_PANEL_Y, CANVAS_POSITION.z));
            var streakText = CreateText(scorePanel, "Racha", "Racha: x0", 34, new Vector2(380f, 50f), new Vector2(0f, 55f), TextAlignmentOptions.Center, BROWN_TEXT, FontStyles.Bold);
            var recordText = CreateText(scorePanel, "Record", "Récord: 0", 34, new Vector2(380f, 50f), new Vector2(0f, -25f), TextAlignmentOptions.Center, BROWN_TEXT, FontStyles.Bold);
            CreateText(scorePanel, "Texto", "Sin equivocarte, cada juguete vale más", 20, new Vector2(380f, 50f), new Vector2(0f, -85f), TextAlignmentOptions.Center, BROWN_TEXT, FontStyles.Normal);
            var streak = gameObject.AddComponent<ToyStreak>();
            Wire(streak, "_game", game);
            Wire(streak, "_label", streakText);
            var record = gameObject.AddComponent<HighScore>();
            Wire(record, "_game", game);
            Wire(record, "_label", recordText);
            Wire(record, "_newRecordSign", newRecord.gameObject);
        }

        // A small canvas next to the sign: a parchment background that turns toward the player. Returns the background.
        static RectTransform CreateSidePanel(string name, Vector3 position)
        {
            var canvasObject = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(TrackedDeviceGraphicRaycaster));
            canvasObject.transform.position = position;
            canvasObject.transform.localScale = Vector3.one * CANVAS_SCALE;
            canvasObject.AddComponent<Billboard>();
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            canvasObject.GetComponent<CanvasScaler>().dynamicPixelsPerUnit = 6f;
            var canvasRect = (RectTransform)canvasObject.transform;
            canvasRect.sizeDelta = SIDE_PANEL_SIZE;
            return CreateSprite(canvasRect, "Fondo", UiSprites.Get("Ayuda"), SIDE_PANEL_SIZE, Vector2.zero, PANEL_COLOR);
        }

        // An empty group the size of the sign, used only to keep the Hierarchy tidy.
        // It is centered like the sign, so the positions of its children do not change.
        static RectTransform CreateGroup(RectTransform parent, string name) { return CreateGroup(parent, name, CANVAS_SIZE, Vector2.zero); }

        // The same, with its own size and position (the header and the buttons only need the strip they use).
        static RectTransform CreateGroup(RectTransform parent, string name, Vector2 size, Vector2 position)
        {
            var group = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)group.transform;
            rect.SetParent(parent, false);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            return rect;
        }

        // The order card: a scroll with a Vertical Layout Group. OrderList fills it with one row (a prefab) per toy.
        static void BuildOrderCard(RectTransform panel, PackingGame game)
        {
            var card = CreateSprite(panel, "Tarjeta del pedido", UiSprites.Get("Pergamino"), new Vector2(520f, 140f), new Vector2(0f, 51f), PANEL_COLOR);
            card.pivot = new Vector2(0.5f, 1f);   // anchored by its top edge: the card grows downward
            card.anchoredPosition = new Vector2(0f, 51f);

            var column = card.gameObject.AddComponent<VerticalLayoutGroup>();
            column.padding = new RectOffset(80, 60, 20, 20);
            column.spacing = 2f;
            column.childAlignment = TextAnchor.UpperLeft;
            column.childControlWidth = true; column.childControlHeight = true;
            column.childForceExpandWidth = true; column.childForceExpandHeight = false;

            // The card grows with the number of rows.
            var fitter = card.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var emptyBox = UiSprites.Get("CasillaVacia");
            var list = game.gameObject.AddComponent<OrderList>();
            Wire(list, "_game", game);
            Wire(list, "_listRoot", card);
            var rowPrefab = BuildRowPrefab();
            Wire(list, "_rowPrefab", rowPrefab);
            Wire(list, "_emptySprite", emptyBox);
            Wire(list, "_checkedSprite", UiSprites.Get("CasillaMarcada"));

            // Name, text and picture of each toy.
            var toys = new SerializedObject(list).FindProperty("_toys");
            var serialized = toys.serializedObject;
            toys.arraySize = ORDER.Length;
            for (var i = 0; i < ORDER.Length; i++)
            {
                var toy = toys.GetArrayElementAtIndex(i);
                toy.FindPropertyRelative("toyName").stringValue = ORDER[i];
                toy.FindPropertyRelative("label").stringValue = ORDER_LABELS[i];
                toy.FindPropertyRelative("icon").objectReferenceValue = UiSprites.Get(ORDER[i]);
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();

            // Preview rows, so the card looks right in the Editor. OrderList replaces them with real rows when Play starts.
            for (var i = 0; i < ORDER.Length; i++)
            {
                var preview = (OrderRow)PrefabUtility.InstantiatePrefab(rowPrefab, card);
                preview.Show(ORDER_LABELS[i], UiSprites.Get(ORDER[i]));
            }
        }

        // The prefab of one order row: check box, toy picture and name inside a Horizontal Layout Group.
        static OrderRow BuildRowPrefab()
        {
            var row = new GameObject("Fila del pedido", typeof(RectTransform));
            var rowRect = (RectTransform)row.transform;
            var line = row.AddComponent<HorizontalLayoutGroup>();
            line.spacing = 12f;
            line.childAlignment = TextAnchor.MiddleLeft;
            line.childControlWidth = true; line.childControlHeight = true;
            line.childForceExpandWidth = false; line.childForceExpandHeight = false;
            row.AddComponent<LayoutElement>().preferredHeight = 36f;

            var box = CreateSprite(rowRect, "Casilla", UiSprites.Get("CasillaVacia"), new Vector2(32f, 32f), Vector2.zero, Color.white);
            SetSize(box, 32f, 32f);
            var icon = CreateSprite(rowRect, "Icono", null, new Vector2(36f, 36f), Vector2.zero, Color.white);
            icon.GetComponent<Image>().preserveAspect = true;
            SetSize(icon, 36f, 36f);
            var label = CreateText(rowRect, "Nombre", "Juguete", 30, new Vector2(200f, 36f), Vector2.zero, TextAlignmentOptions.MidlineLeft, BROWN_TEXT, FontStyles.Bold);
            label.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

            var component = row.AddComponent<OrderRow>();
            Wire(component, "_check", box.GetComponent<Image>());
            Wire(component, "_icon", icon.GetComponent<Image>());
            Wire(component, "_label", label);

            if (!AssetDatabase.IsValidFolder(PREFABS_FOLDER)) AssetDatabase.CreateFolder("Assets/Common", "Prefabs");
            var prefab = PrefabUtility.SaveAsPrefabAsset(row, PREFABS_FOLDER + "/FilaPedido.prefab");
            Object.DestroyImmediate(row);
            return prefab.GetComponent<OrderRow>();
        }

        // Fixes the size a Layout Group will use for this element.
        static void SetSize(RectTransform element, float width, float height)
        {
            var size = element.gameObject.AddComponent<LayoutElement>();
            size.preferredWidth = width; size.preferredHeight = height;
        }

        // Sets a serialized field (private ones too) the same way the Inspector would.
        static void Wire(Object target, string field, object value)
        {
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(field);
            if (property == null) { Debug.LogWarning("Field " + field + " not found on " + target.GetType().Name); return; }

            if (value == null || value is Object) property.objectReferenceValue = (Object)value;
            else if (value is int number) property.intValue = number;
            else if (value is bool flag) property.boolValue = flag;
            else if (value is float decimalNumber) property.floatValue = decimalNumber;
            else if (value is IList list)
            {
                property.arraySize = list.Count;
                for (var i = 0; i < list.Count; i++)
                {
                    var element = property.GetArrayElementAtIndex(i);
                    if (list[i] is Object item) element.objectReferenceValue = item;
                    else element.stringValue = (string)list[i];
                }
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        // End-of-round card: hidden until the game finishes. Its button starts a new round.
        static RectTransform BuildResult(RectTransform panel, GameObject gameObject, PackingGame game)
        {
            var card = CreateSprite(panel, "Panel de resultado", UiSprites.Get("Ayuda"), new Vector2(560f, 320f), Vector2.zero, new Color(0.05f, 0.12f, 0.08f, 0.97f));
            card.GetComponent<Image>().raycastTarget = true;   // the open card blocks the buttons behind it
            var title = CreateText(card, "Titulo", "¡LO LOGRASTE!", 40, new Vector2(480f, 60f), new Vector2(0f, 70f), TextAlignmentOptions.Center, BROWN_TEXT, FontStyles.Bold);
            var points = CreateText(card, "Puntos", "Puntos: 0", 32, new Vector2(480f, 50f), new Vector2(0f, 5f), TextAlignmentOptions.Center, BROWN_TEXT, FontStyles.Bold);
            var again = CreateButton(card, "Boton Jugar otra vez", "JUGAR OTRA VEZ", new Vector2(320f, 56f), new Vector2(0f, -85f), 26, UiSprites.Get("BotonRojo"), Color.white);
            UnityEventTools.AddVoidPersistentListener(again.onClick, game.Begin);
            card.gameObject.SetActive(false);

            var result = gameObject.AddComponent<ResultPanel>();
            Wire(result, "_game", game);
            Wire(result, "_panel", card.gameObject);
            Wire(result, "_title", title);
            Wire(result, "_points", points);
            return card;
        }

        // Help panel toggled by the "?" button.
        static void BuildHelp(RectTransform panel, RectTransform header, int day)
        {
            var help = CreateSprite(panel, "Panel de ayuda", UiSprites.Get("Ayuda"), new Vector2(600f, 360f), new Vector2(0f, -5f), new Color(0.05f, 0.12f, 0.08f, 0.97f));
            // From Day 5 the player has to walk to the start station first, so the text changes.
            var helpText = day < 5
                ? "COMO SE JUEGA\n1. Aprieta EMPEZAR.\n2. Agarra el juguete que pide el pedido.\n3. Aprieta el gatillo para envolverlo.\n4. Sueltalo en la caja de la derecha."
                : "COMO SE JUEGA\n1. Teletranspórtate a la estación de inicio.\n2. Aprieta EMPEZAR.\n3. Agarra un juguete y aprieta el gatillo.\n4. Suéltalo en la caja de entrega.";
            CreateText(help, "Texto de ayuda", helpText, day < 5 ? 30 : 26, new Vector2(470f, day < 5 ? 230f : 250f), new Vector2(0f, day < 5 ? 20f : 25f), TextAlignmentOptions.TopLeft, BROWN_TEXT, FontStyles.Normal);
            var closeButton = CreateButton(help, "Boton Cerrar", "CERRAR", new Vector2(170f, 52f), new Vector2(0f, -125f), 26, UiSprites.Get("BotonAzul"), Color.white);
            UnityEventTools.AddBoolPersistentListener(closeButton.onClick, help.gameObject.SetActive, false);
            help.GetComponent<Image>().raycastTarget = true;   // the open panel blocks the buttons behind it
            help.gameObject.SetActive(false);

            var helpButton = CreateButton(header, "Boton Ayuda", "?", new Vector2(60f, 62f), new Vector2(-30f, 0f), 38, UiSprites.Get("BotonRedondo"), BROWN_TEXT);
            var helpRect = (RectTransform)helpButton.transform;
            helpRect.anchorMin = helpRect.anchorMax = new Vector2(1f, 0.5f);   // sticks to the right side of the header
            helpRect.anchoredPosition = new Vector2(-30f, 0f);
            UnityEventTools.AddBoolPersistentListener(helpButton.onClick, help.gameObject.SetActive, true);
        }

        // Day 5: teleport over the floor, two teleport stations (start and delivery), snap turning,
        // a comfort vignette, the station sign and the rule "press Start only from the start station".
        static void BuildLocomotion(PackingGame game)
        {
            // The teleport rays only look at the "Teleport" interaction layer, so the floor and the stations must be on it.
            EnsureTeleportLayer();
            var teleportLayer = InteractionLayerMask.GetMask("Teleport");
            var floor = GameObject.Find("Floor");
            TeleportationArea area = null;
            if (floor != null)
            {
                area = floor.GetComponent<TeleportationArea>();
                if (area == null) area = floor.AddComponent<TeleportationArea>();
                area.interactionLayers = teleportLayer;
            }

            var gold = CreateMaterial("Dorado_Estacion", new Color(0.85f, 0.65f, 0.15f));
            var startStation = BuildPad("Estacion de inicio", new Vector3(-1f, 0.02f, 0.1f), gold, teleportLayer);
            var deliveryStation = BuildPad("Estacion de entrega", new Vector3(1.2f, 0.02f, 0.1f), gold, teleportLayer);

            var snap = Object.FindFirstObjectByType<SnapTurnProvider>(FindObjectsInactive.Include);
            if (snap != null) { snap.enabled = true; snap.turnAmount = 45f; }
            else Debug.LogWarning("SnapTurnProvider not found: import the XRI Starter Assets samples first.");
            var continuous = Object.FindFirstObjectByType<ContinuousTurnProvider>(FindObjectsInactive.Include);
            if (continuous != null) continuous.enabled = false;
            var move = Object.FindFirstObjectByType<ContinuousMoveProvider>(FindObjectsInactive.Include);
            if (move != null) { move.enabled = false; move.moveSpeed = 1.5f; }

            // Floor tracking: the virtual floor is the real floor.
            var origin = Object.FindFirstObjectByType<XROrigin>();
            if (origin != null) { origin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor; EditorUtility.SetDirty(origin); }

            BuildVignette(origin);
            if (area != null && startStation != null && deliveryStation != null) BuildStationGate(game, area, startStation, deliveryStation);
        }

        // The tunneling vignette of the XRI Starter Assets: it darkens the edges of the view while the player moves.
        static void BuildVignette(XROrigin origin)
        {
            var guids = AssetDatabase.FindAssets("TunnelingVignette t:Prefab");
            if (origin == null || guids.Length == 0) { Debug.LogWarning("TunnelingVignette prefab not found: import the XRI Starter Assets samples first."); return; }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guids[0]));
            var vignette = (GameObject)PrefabUtility.InstantiatePrefab(prefab, origin.Camera.transform);
            var controller = vignette.GetComponentInChildren<TunnelingVignetteController>();
            if (controller == null) return;

            // One entry for each way of moving: teleport and continuous move.
            var providers = new LocomotionProvider[]
            {
                Object.FindFirstObjectByType<TeleportationProvider>(FindObjectsInactive.Include),
                Object.FindFirstObjectByType<ContinuousMoveProvider>(FindObjectsInactive.Include)
            };
            var serialized = new SerializedObject(controller);
            var list = serialized.FindProperty("m_LocomotionVignetteProviders");
            list.arraySize = providers.Length;
            for (var i = 0; i < providers.Length; i++)
            {
                var entry = list.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("m_LocomotionProvider").objectReferenceValue = providers[i];
                entry.FindPropertyRelative("m_Enabled").boolValue = true;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        // The small sign that says where the player is, and the script that locks the game buttons.
        static void BuildStationGate(PackingGame game, TeleportationArea floor, TeleportationAnchor start, TeleportationAnchor delivery)
        {
            var canvasObject = new GameObject("Cartel de estacion", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasObject.transform.position = new Vector3(0f, 1.1f, 1.5f);   // low, under the game sign, so it never covers it
            canvasObject.transform.localScale = Vector3.one * CANVAS_SCALE;
            canvasObject.AddComponent<Billboard>();
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            canvasObject.GetComponent<CanvasScaler>().dynamicPixelsPerUnit = 6f;
            var canvasRect = (RectTransform)canvasObject.transform;
            canvasRect.sizeDelta = new Vector2(860f, 80f);

            var plaque = CreateSprite(canvasRect, "Placa", UiSprites.Get("Placa"), new Vector2(860f, 80f), Vector2.zero, PANEL_COLOR, true);
            var label = CreateText(plaque, "Texto", "", 36, new Vector2(820f, 70f), Vector2.zero, TextAlignmentOptions.Center, GOLD_TEXT, FontStyles.Bold);

            // The two buttons that start a round: Start and Play again.
            var buttons = new System.Collections.Generic.List<Button>();
            foreach (var name in new[] { START_NAME, "Boton Jugar otra vez" })
            {
                var found = FindByName(name);
                var button = found == null ? null : found.GetComponent<Button>();
                if (button != null) buttons.Add(button);
            }

            var gate = game.gameObject.AddComponent<StationGate>();
            Wire(gate, "_startStation", start);
            Wire(gate, "_deliveryStation", delivery);
            Wire(gate, "_floor", floor);
            Wire(gate, "_gatedButtons", buttons);
            Wire(gate, "_label", label);
            Wire(gate, "_sign", canvasObject);
        }

        // GameObject.Find ignores switched-off objects (the result panel starts off), so the sign is searched by hand.
        static GameObject FindByName(string name)
        {
            foreach (var rect in Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (rect.name == name) return rect.gameObject;
            return null;
        }

        const string LAYERS_ASSET = "Assets/XRI/Settings/Resources/InteractionLayerSettings.asset";
        const string TELEPORT_LAYER = "Teleport";
        const int TELEPORT_LAYER_INDEX = 31;   // the toolkit samples reserve Interaction Layer 31 for teleporting

        // Names Interaction Layer 31 "Teleport" (the same as the Fix button in Project Validation). Without it nothing can be teleported to.
        static void EnsureTeleportLayer()
        {
            if (InteractionLayerMask.NameToLayer(TELEPORT_LAYER) >= 0) return;
            var settings = AssetDatabase.LoadAssetAtPath<ScriptableObject>(LAYERS_ASSET);
            var layer = settings == null ? null : new SerializedObject(settings).FindProperty("m_LayerNames");
            if (settings == null || layer == null) { Debug.LogWarning("Interaction layer settings not found: name Interaction Layer 31 'Teleport' by hand."); return; }
            var serialized = new SerializedObject(settings);
            serialized.FindProperty("m_LayerNames").GetArrayElementAtIndex(TELEPORT_LAYER_INDEX).stringValue = TELEPORT_LAYER;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }

        static TeleportationAnchor BuildPad(string name, Vector3 position, Material material, InteractionLayerMask layer)
        {
            var pad = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pad.name = name; pad.transform.position = position;
            pad.transform.localScale = new Vector3(0.7f, 0.02f, 0.7f);
            pad.GetComponent<MeshRenderer>().sharedMaterial = material;
            var point = new GameObject("Punto de llegada");
            point.transform.position = new Vector3(position.x, 0f, position.z);
            point.transform.SetParent(pad.transform, true);
            var anchor = pad.AddComponent<TeleportationAnchor>();
            anchor.teleportAnchorTransform = point.transform;
            anchor.interactionLayers = layer;
            return anchor;
        }

        static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(XRUIInputModule));
        }

        static RectTransform CreatePanel(RectTransform parent, string name, Vector2 size, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)obj.transform;
            rect.SetParent(parent, false); rect.sizeDelta = size;
            obj.AddComponent<Image>().color = color;
            return rect;
        }

        // An Image with a sprite (or a plain color when the sprite is missing). Sliced sprites keep their corners when resized.
        static RectTransform CreateSprite(RectTransform parent, string name, Sprite sprite, Vector2 size, Vector2 position, Color fallback, bool sliced = false)
        {
            var rect = CreatePanel(parent, name, size, sprite == null ? fallback : Color.white);
            rect.anchoredPosition = position;
            if (sprite == null) return rect;

            var image = rect.GetComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            if (sliced)
            {
                // The multiplier shrinks the borders so they fit the new height.
                image.type = Image.Type.Sliced;
                image.pixelsPerUnitMultiplier = sprite.rect.height / size.y;
            }
            return rect;
        }

        // Creates a TextMeshPro label (uses the default font imported with TMP Essentials).
        static TextMeshProUGUI CreateText(RectTransform parent, string name, string content, int size, Vector2 box, Vector2 position, TextAlignmentOptions alignment, Color color, FontStyles style)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)obj.transform;
            rect.SetParent(parent, false); rect.sizeDelta = box; rect.anchoredPosition = position;
            var text = obj.AddComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = size; text.text = content; text.alignment = alignment; text.color = color; text.fontStyle = style;
            text.textWrappingMode = TextWrappingModes.Normal; text.overflowMode = TextOverflowModes.Overflow;
            text.raycastTarget = false;
            return text;
        }

        // A button with a sprite and a label; it lights up when pointed at and darkens when pressed.
        static Button CreateButton(RectTransform parent, string name, string label, Vector2 size, Vector2 position, int fontSize, Sprite sprite, Color textColor)
        {
            var rect = CreateSprite(parent, name, sprite, size, position, BUTTON_COLOR, true);
            var image = rect.GetComponent<Image>();
            image.raycastTarget = true;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.highlightedColor = new Color(1f, 0.95f, 0.63f);
            colors.pressedColor = new Color(0.73f, 0.73f, 0.73f);
            colors.fadeDuration = 0.1f;
            button.colors = colors;
            CreateText(rect, "Texto", label, fontSize, size, Vector2.zero, TextAlignmentOptions.Center, textColor, FontStyles.Bold);
            return button;
        }

        static Material CreateMaterial(string name, Color color)
        {
            if (!AssetDatabase.IsValidFolder(MATERIALS_FOLDER)) AssetDatabase.CreateFolder("Assets/Common", "Materials");
            var path = MATERIALS_FOLDER + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}
