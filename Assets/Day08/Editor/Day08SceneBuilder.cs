using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Day08.Editor
{
    /// <summary>
    /// Builds the Day 08 scene: a cosy workshop with fireplace light, winter fog and falling snow.
    /// </summary>
    public static class Day08SceneBuilder
    {
        const string DAY_FOLDER = "Assets/Day08";
        const string SCENE_PATH = "Assets/Day08/Day08_Polish.unity";
        const string MATERIALS_FOLDER = "Assets/Day08/Materials";
        const string SAMPLES_FOLDER = "Assets/Samples";
        const string XR_ORIGIN_PREFAB = "XR Origin (XR Rig)";
        const string DEVICE_SIMULATOR_PREFAB = "XR Device Simulator";
        const string INTERACTION_MANAGER_NAME = "XR Interaction Manager";
        const string LIGHT_NAME = "Directional Light";
        const string FLOOR_NAME = "Floor";
        const string BACK_WALL_NAME = "Pared Fondo";
        const string LEFT_WALL_NAME = "Pared Izquierda";
        const string RIGHT_WALL_NAME = "Pared Derecha";
        const string TABLE_NAME = "Mesa";
        const string GIFT_RED_NAME = "Regalo Rojo";
        const string GIFT_GREEN_NAME = "Regalo Verde";
        const string FIREPLACE_NAME = "Chimenea";
        const string FIRE_NAME = "Fuego";
        const string FIRE_LIGHT_NAME = "Luz de chimenea";
        const string FIRE_BOUNCE_LIGHT_NAME = "Luz de chimenea (rebote)";
        const string SNOW_NAME = "Nieve";
        const string MATERIAL_NIEVE = "Nieve";
        const string MATERIAL_MADERA = "Madera";
        const string MATERIAL_PIEDRA = "Piedra";
        const string MATERIAL_ROJO = "Rojo";
        const string MATERIAL_VERDE = "Verde";
        const string MATERIAL_FUEGO = "Fuego";
        const string MATERIAL_COPO = "Copo";
        const string MATERIAL_CIELO = "Cielo";
        const string URP_LIT_SHADER = "Universal Render Pipeline/Lit";
        const string URP_PARTICLE_SHADER = "Universal Render Pipeline/Particles/Unlit";
        const string LEGACY_PARTICLE_SHADER = "Particles/Standard Unlit";
        const string SKYBOX_SHADER = "Skybox/Procedural";
        const string FALLBACK_SHADER = "Standard";
        const string READY_MESSAGE = "Day 08 scene ready: chimenea calida, niebla invernal y nieve ligera. Ajusta densidades a tu gusto.";

        const float FLOOR_SCALE = 2f;
        const float ROOM_HEIGHT = 1.5f;
        const float ROOM_HALF_WIDTH = 5f;
        const float ROOM_DEPTH = 8f;
        const float ROOM_BACK_Z = 4f;
        const float WALL_THICKNESS = 0.2f;
        const float FOG_DENSITY = 0.025f;
        const float DIRECTIONAL_INTENSITY = 0.8f;
        const float TABLE_HEIGHT = 0.72f;
        const float TABLE_DISTANCE = 1.4f;
        const float TABLE_WIDTH = 1.6f;
        const float TABLE_THICKNESS = 0.08f;
        const float TABLE_DEPTH = 0.9f;
        const float GIFT_SCALE = 0.18f;
        const float GIFT_HEIGHT = 0.85f;
        const float GIFT_SPACING = 0.3f;
        const float FIREPLACE_HEIGHT = 0.9f;
        const float FIREPLACE_Z = 3.6f;
        const float FIREPLACE_WIDTH = 2.2f;
        const float FIREPLACE_TALL = 1.8f;
        const float FIREPLACE_DEPTH = 0.5f;
        const float FIRE_HEIGHT = 0.45f;
        const float FIRE_Z = 3.3f;
        const float FIRE_WIDTH = 0.9f;
        const float FIRE_TALL = 0.5f;
        const float FIRE_DEPTH = 0.1f;
        const float FIRE_LIGHT_HEIGHT = 0.8f;
        const float FIRE_LIGHT_Z = 2.9f;
        const float FIRE_LIGHT_INTENSITY = 4f;
        const float FIRE_LIGHT_RANGE = 5f;
        const float FIRE_BOUNCE_HEIGHT = 0.3f;
        const float FIRE_BOUNCE_Z = 2.4f;
        const float FIRE_BOUNCE_INTENSITY = 1.5f;
        const float FIRE_BOUNCE_RANGE = 3f;
        const float SNOW_HEIGHT = 3.5f;
        const float SNOW_DISTANCE = 1.5f;
        const float SNOW_LIFETIME = 8f;
        const float SNOW_SPEED = 0.2f;
        const float SNOW_SIZE_MIN = 0.04f;
        const float SNOW_SIZE_MAX = 0.08f;
        const float SNOW_GRAVITY = 0.008f;
        const float SNOW_RATE = 300f;
        const float SNOW_NOISE_STRENGTH = 0.4f;
        const float SNOW_NOISE_FREQUENCY = 0.4f;
        const string SNOWFLAKE_TEXTURE = "Assets/Day08/Copo.png";
        const int SNOWFLAKE_SIZE = 64;
        const float SNOW_ALPHA = 0.9f;
        const int SNOW_MAX_PARTICLES = 3000;
        const float SKY_ATMOSPHERE = 0.7f;
        const float SKY_EXPOSURE = 1.1f;

        static readonly Vector3 LIGHT_ROTATION = new Vector3(35f, -40f, 0f);
        static readonly Vector3 SNOW_SHAPE_SCALE = new Vector3(9f, 7f, 0.1f);
        static readonly Vector3 SNOW_SHAPE_ROTATION = new Vector3(90f, 0f, 0f);
        static readonly Color LIGHT_COLOR = new Color(0.75f, 0.82f, 1f);
        static readonly Color FOG_COLOR = new Color(0.55f, 0.62f, 0.75f);
        static readonly Color AMBIENT_COLOR = new Color(0.35f, 0.4f, 0.5f);
        static readonly Color NIEVE_COLOR = new Color(0.92f, 0.94f, 1f);
        static readonly Color MADERA_COLOR = new Color(0.45f, 0.28f, 0.15f);
        static readonly Color PIEDRA_COLOR = new Color(0.35f, 0.34f, 0.33f);
        static readonly Color FIRE_BASE_COLOR = new Color(0.2f, 0.08f, 0.03f);
        static readonly Color FIRE_EMISSION_COLOR = new Color(1f, 0.42f, 0.1f) * 3f;
        static readonly Color FIRE_LIGHT_COLOR = new Color(1f, 0.55f, 0.25f);
        static readonly Color FIRE_BOUNCE_COLOR = new Color(1f, 0.45f, 0.2f);
        static readonly Color SNOW_COLOR = new Color(1f, 1f, 1f, SNOW_ALPHA);
        static readonly Color SKY_TINT = new Color(0.4f, 0.5f, 0.7f);
        static readonly Color SKY_GROUND = new Color(0.2f, 0.18f, 0.2f);

        static readonly string[] GIFT_NAMES = { GIFT_RED_NAME, GIFT_GREEN_NAME };
        static readonly string[] GIFT_MATERIAL_NAMES = { MATERIAL_ROJO, MATERIAL_VERDE };
        static readonly float[] GIFT_X_POSITIONS = { -GIFT_SPACING, GIFT_SPACING };
        static readonly Color[] GIFT_COLORS =
        {
            new Color(0.8f, 0.1f, 0.1f),
            new Color(0.1f, 0.7f, 0.15f),
        };

        // Builds the Day 08 scene: the whole game from Day 07 plus the decoration.
        [MenuItem("VR Class/Day 08/Build Polish Scene")]
        public static void BuildScene() =>
            Day03.Editor.Day03SceneBuilder.BuildWorkshop(SCENE_PATH, () => VrClass.Tools.WorkshopStages.Apply(8));

        // Decorates the packing workshop: winter atmosphere, walls, stove with chimney, tree, snowman, gifts, snow and music.
        public static void DecorateWorkshop()
        {
            EnsureFolder(DAY_FOLDER); EnsureFolder(MATERIALS_FOLDER);
            ConfigureAtmosphere();
            foreach (var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (light.type != LightType.Directional) continue;   // the main light, not the warning lamp
                light.transform.rotation = Quaternion.Euler(LIGHT_ROTATION);
                light.color = LIGHT_COLOR;
                light.intensity = DIRECTIONAL_INTENSITY;
            }

            var piedra = CreateMaterial(MATERIAL_PIEDRA, PIEDRA_COLOR);
            var fuego = CreateFireMaterial(MATERIAL_FUEGO, FIRE_EMISSION_COLOR);
            AddBox(BACK_WALL_NAME, new Vector3(0f, ROOM_HEIGHT, ROOM_BACK_Z), new Vector3(ROOM_HALF_WIDTH * 2f, ROOM_HEIGHT * 2f, WALL_THICKNESS), piedra);
            AddBox(LEFT_WALL_NAME, new Vector3(-ROOM_HALF_WIDTH, ROOM_HEIGHT, 0f), new Vector3(WALL_THICKNESS, ROOM_HEIGHT * 2f, ROOM_DEPTH), piedra);
            AddBox(RIGHT_WALL_NAME, new Vector3(ROOM_HALF_WIDTH, ROOM_HEIGHT, 0f), new Vector3(WALL_THICKNESS, ROOM_HEIGHT * 2f, ROOM_DEPTH), piedra);

            var polyart = BuildPolyartFireplace();
            if (!polyart) { BuildFireplace(fuego); MoveFireplaceAside(); }
            BuildFireLight(FIRE_LIGHT_NAME, new Vector3(STOVE_X, FIRE_LIGHT_HEIGHT, FIRE_LIGHT_Z), FIRE_LIGHT_COLOR, FIRE_LIGHT_INTENSITY, FIRE_LIGHT_RANGE);
            BuildFireLight(FIRE_BOUNCE_LIGHT_NAME, new Vector3(STOVE_X, FIRE_BOUNCE_HEIGHT, FIRE_BOUNCE_Z), FIRE_BOUNCE_COLOR, FIRE_BOUNCE_INTENSITY, FIRE_BOUNCE_RANGE);
            BuildWallDetails();
            BuildGarland();
            BuildSnow();
            if (polyart) BuildPolyartDecor();
            if (polyart) BuildCorner();
            BuildSledAndMachine();
            DecorateChristmas(polyart);
            AddMusic();
        }

        // ---- Stylized Christmas Environment (Polyart Studio): Assets/Polyart/PolyartStudio ----
        const string STOVE_PREFAB = "PF_Farm_KitchenStove_01";
        const string CHIMNEY_MODEL = "SM_Farm_StoveChimney";
        const float STOVE_HEIGHT = 1.3f;
        const float CHIMNEY_HEIGHT = 1.6f;
        const float STOVE_X = 3.2f;   // the signs hang in front of the middle of the back wall, so the stove stands to one side
        static readonly Vector3 STOVE_BASE = new Vector3(STOVE_X, 0f, 3.4f);
        static readonly Vector3 FACING_ROOM = new Vector3(0f, 180f, 0f);

        // Places the stove and its chimney pipe against the back wall. Returns false when the pack is not imported.
        static bool BuildPolyartFireplace()
        {
            var group = new GameObject(FIREPLACE_NAME).transform;
            var stove = VrClass.Tools.RealAssets.Place(STOVE_PREFAB, STOVE_BASE, FACING_ROOM, STOVE_HEIGHT, group);
            if (stove == null) { Object.DestroyImmediate(group.gameObject); return false; }
            VrClass.Tools.RealAssets.Place(CHIMNEY_MODEL, new Vector3(STOVE_BASE.x, STOVE_HEIGHT, STOVE_BASE.z + 0.1f), FACING_ROOM, CHIMNEY_HEIGHT, group);
            return true;
        }

        // Tree with gifts, wreath on the wall and a snowman, all from the Polyart pack.
        static void BuildPolyartDecor()
        {
            var decor = new GameObject("Entorno navideno (Polyart)").transform;
            void Put(string prefab, Vector3 position, Vector3 euler, float height) =>
                VrClass.Tools.RealAssets.Place(prefab, position, euler, height, decor);

            Put("PF_ChristmasTree", new Vector3(-3f, 0f, 3f), Vector3.zero, 2.2f);
            Put("PF_Christmas_GiftBox_01", new Vector3(-2.4f, 0f, 2.5f), new Vector3(0f, 20f, 0f), 0.35f);
            Put("PF_Christmas_GiftBox_02", new Vector3(-3.5f, 0f, 2.4f), new Vector3(0f, -15f, 0f), 0.3f);
            Put("PF_Christmas_GiftBox_03", new Vector3(-2.9f, 0f, 2.1f), new Vector3(0f, 40f, 0f), 0.4f);
            Put("PF_Christmas_Snowman", new Vector3(4f, 0f, 1.2f), new Vector3(0f, 200f, 0f), 1.1f);
            Put("PF_ChristmasWreath", new Vector3(0f, 2.5f, 3.85f), FACING_ROOM, 0.45f);   // above the signs
        }

        // Builds a brick fireplace (hearth, pillars, lintel, mantle, chimney stack, logs and flames) against the back wall.
        static void BuildFireplace(Material fuego)
        {
            var ladrillo = CreateMaterial("Ladrillo", new Color(0.55f, 0.22f, 0.16f));
            var hollin = CreateMaterial("Hollin", new Color(0.06f, 0.05f, 0.05f));
            var madera = CreateMaterial(MATERIAL_MADERA, MADERA_COLOR);
            var llama = CreateFireMaterial("Llama", new Color(1f, 0.75f, 0.2f) * 3f);

            var group = new GameObject(FIREPLACE_NAME).transform;
            AddPart(group, "Hogar", PrimitiveType.Cube, new Vector3(0f, 0.05f, 3.45f), new Vector3(2.6f, 0.1f, 1f), ladrillo);
            AddPart(group, "Pilar Izquierdo", PrimitiveType.Cube, new Vector3(-0.9f, 0.75f, 3.55f), new Vector3(0.5f, 1.3f, 0.7f), ladrillo);
            AddPart(group, "Pilar Derecho", PrimitiveType.Cube, new Vector3(0.9f, 0.75f, 3.55f), new Vector3(0.5f, 1.3f, 0.7f), ladrillo);
            AddPart(group, "Dintel", PrimitiveType.Cube, new Vector3(0f, 1.575f, 3.55f), new Vector3(2.3f, 0.35f, 0.7f), ladrillo);
            AddPart(group, "Repisa", PrimitiveType.Cube, new Vector3(0f, 1.8f, 3.45f), new Vector3(2.8f, 0.1f, 0.9f), madera);
            AddPart(group, "Conducto", PrimitiveType.Cube, new Vector3(0f, 2.425f, 3.6f), new Vector3(1.4f, 1.15f, 0.6f), ladrillo);
            AddPart(group, "Fondo del hogar", PrimitiveType.Cube, new Vector3(0f, 0.75f, 3.84f), new Vector3(1.3f, 1.3f, 0.05f), hollin);

            var logA = AddPart(group, "Leno 1", PrimitiveType.Cylinder, new Vector3(0f, 0.22f, 3.5f), new Vector3(0.14f, 0.45f, 0.14f), madera);
            logA.transform.rotation = Quaternion.Euler(0f, 8f, 90f);
            var logB = AddPart(group, "Leno 2", PrimitiveType.Cylinder, new Vector3(0f, 0.22f, 3.7f), new Vector3(0.14f, 0.45f, 0.14f), madera);
            logB.transform.rotation = Quaternion.Euler(0f, -8f, 90f);

            AddPart(group, FIRE_NAME, PrimitiveType.Cube, new Vector3(0f, 0.13f, 3.6f), new Vector3(0.9f, 0.12f, 0.4f), fuego);
            AddPart(group, "Llama 1", PrimitiveType.Sphere, new Vector3(-0.2f, 0.5f, 3.6f), new Vector3(0.22f, 0.5f, 0.22f), llama);
            AddPart(group, "Llama 2", PrimitiveType.Sphere, new Vector3(0.05f, 0.6f, 3.6f), new Vector3(0.26f, 0.65f, 0.26f), llama);
            AddPart(group, "Llama 3", PrimitiveType.Sphere, new Vector3(0.25f, 0.47f, 3.6f), new Vector3(0.2f, 0.42f, 0.2f), llama);
        }

        // The brick fireplace is built in the middle of the back wall: slide it next to the stove position.
        static void MoveFireplaceAside()
        {
            var group = GameObject.Find(FIREPLACE_NAME);
            if (group != null) group.transform.position += new Vector3(STOVE_X, 0f, 0f);
        }

        // Adds a decorative primitive (no collider) under a parent.
        static GameObject AddPart(Transform parent, string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material)
        {
            var part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.position = position;
            part.transform.localScale = scale;
            part.GetComponent<MeshRenderer>().sharedMaterial = material;
            Object.DestroyImmediate(part.GetComponent<Collider>());
            part.transform.SetParent(parent, true);
            return part;
        }

        // Adds the looping Christmas music (Assets/Audio/christmas-loop.mp3) to the scene when the file exists.
        static void AddMusic()
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/christmas-loop.mp3");
            if (clip == null)
            {
                Debug.Log("Musica navidena: copia christmas-loop.mp3 a Assets/Audio/ para que suene en la escena.");
                return;
            }
            var source = new GameObject("Musica navidena").AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.playOnAwake = true;
            source.spatialBlend = 0f;
            source.volume = 0.35f;
        }

        // Places the Christmas Presents models (when imported) on the mantle, hearth and table.
        static void DecorateChristmas(bool polyart)
        {
            var decor = new GameObject("Decoracion navidena").transform;
            var placed = 0;
            void Put(string prefab, Vector3 position, Vector3 euler, float height)
            {
                if (VrClass.Tools.RealAssets.Place(prefab, position, euler, height, decor) != null) placed++;
            }

            // Con el pack Polyart la estrella va en la punta del arbol y lo demas en el suelo junto a la estufa.
            var mantleTop = polyart ? STOVE_HEIGHT : 1.85f;
            if (polyart) Put("Star", new Vector3(-3f, 2.2f, 3f), Vector3.zero, 0.3f);
            else Put("Star", new Vector3(0f, mantleTop, 3.45f), new Vector3(0f, 180f, 0f), 0.32f);
            var caneHeight = polyart ? 0f : mantleTop;
            Put("Christmas-Ball", new Vector3(STOVE_X - 0.25f, mantleTop, 3.45f), Vector3.zero, 0.16f);
            Put("Christmas-Ball", new Vector3(STOVE_X + 0.25f, mantleTop, 3.45f), Vector3.zero, 0.16f);
            Put("Candy-Cane", new Vector3(STOVE_X - 1f, caneHeight, 3.45f), new Vector3(0f, 180f, 0f), 0.4f);
            Put("Candy-Cane", new Vector3(STOVE_X + 1f, caneHeight, 3.45f), new Vector3(0f, 180f, 0f), 0.4f);
            Put("Teddybear", new Vector3(4.4f, 0f, 3.0f), new Vector3(0f, 200f, 0f), 0.55f);
            Put("Christmas-Ball", new Vector3(2f, 0f, 3.1f), Vector3.zero, 0.2f);
            Put("Present", new Vector3(3.4f, 0f, 0.9f), new Vector3(0f, 30f, 0f), 0.3f);
            Put("Present", new Vector3(4.4f, 0f, 0.8f), new Vector3(0f, -20f, 0f), 0.35f);
            if (polyart)
            {
                Put("Christmas-Ball", new Vector3(-4.2f, 0.75f, 0.4f), Vector3.zero, 0.16f);
                Put("Present", new Vector3(-4.2f, 0.75f, 0.9f), new Vector3(0f, 15f, 0f), 0.2f);
            }

            if (placed == 0)
                Object.DestroyImmediate(decor.gameObject);
        }

        // Sets the winter fog, ambient light and procedural sky.
        static void ConfigureAtmosphere()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = FOG_DENSITY;
            RenderSettings.fogColor = FOG_COLOR;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;   // Environment Lighting > Source: Color
            RenderSettings.ambientLight = AMBIENT_COLOR;

            var sky = CreateSky();
            if (sky != null) RenderSettings.skybox = sky;
        }

        // Builds one grabbable gift on the table.
        static void BuildGift(string name, float x, Material material)
        {
            var gift = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gift.name = name;
            gift.transform.position = new Vector3(x, GIFT_HEIGHT, TABLE_DISTANCE);
            gift.transform.localScale = Vector3.one * GIFT_SCALE;
            gift.GetComponent<MeshRenderer>().sharedMaterial = material;
            gift.AddComponent<Rigidbody>();
            gift.AddComponent<XRGrabInteractable>();
        }

        // Builds one warm point light for the fireplace.
        static void BuildFireLight(string name, Vector3 position, Color color, float intensity, float range)
        {
            var light = new GameObject(name).AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.transform.position = position;
        }

        const string WOOD_DARK_NAME = "Madera_Oscura";
        static readonly Color WOOD_DARK_COLOR = new Color(0.28f, 0.17f, 0.09f);
        const float WAINSCOT_HEIGHT = 0.8f;
        const float BEAM_HEIGHT = 2.85f;
        const float GARLAND_HEIGHT = 2.7f;
        const float GARLAND_BULB = 0.12f;
        const float GARLAND_GLOW = 2f;
        static readonly string[] BULB_NAMES = { "Luz roja", "Luz verde", "Luz dorada", "Luz azul" };
        static readonly Color[] BULB_COLORS =
        {
            new Color(1f, 0.16f, 0.16f),
            new Color(0.2f, 0.85f, 0.3f),
            new Color(1f, 0.78f, 0.2f),
            new Color(0.3f, 0.6f, 1f),
        };

        // Dark wood skirting boards along the three walls and three beams across the ceiling (no colliders).
        static void BuildWallDetails()
        {
            var wood = CreateMaterial(WOOD_DARK_NAME, WOOD_DARK_COLOR);
            var group = new GameObject("Detalles de pared").transform;
            var skirtingY = WAINSCOT_HEIGHT / 2f;
            AddPart(group, "Zocalo Fondo", PrimitiveType.Cube, new Vector3(0f, skirtingY, 3.87f), new Vector3(9.6f, WAINSCOT_HEIGHT, 0.06f), wood);
            AddPart(group, "Zocalo Izquierdo", PrimitiveType.Cube, new Vector3(-4.87f, skirtingY, 0f), new Vector3(0.06f, WAINSCOT_HEIGHT, 7.8f), wood);
            AddPart(group, "Zocalo Derecho", PrimitiveType.Cube, new Vector3(4.87f, skirtingY, 0f), new Vector3(0.06f, WAINSCOT_HEIGHT, 7.8f), wood);
            for (var i = 0; i < 3; i++)
                AddPart(group, "Viga " + (i + 1), PrimitiveType.Cube, new Vector3(0f, BEAM_HEIGHT, -2f + 2f * i), new Vector3(9.8f, 0.2f, 0.25f), wood);
        }

        // A string of coloured bulbs (red, green, gold, blue, again) along the back wall and the two side walls.
        static void BuildGarland()
        {
            var bulbs = new Material[BULB_NAMES.Length];
            for (var i = 0; i < bulbs.Length; i++) bulbs[i] = CreateBulbMaterial(BULB_NAMES[i], BULB_COLORS[i]);

            var group = new GameObject("Guirnalda").transform;
            for (var i = 0; i < 13; i++)
                AddPart(group, "Bombilla fondo " + i, PrimitiveType.Sphere, new Vector3(-4.5f + 0.75f * i, GARLAND_HEIGHT, 3.8f), Vector3.one * GARLAND_BULB, bulbs[i % bulbs.Length]);
            for (var i = 0; i < 5; i++)
            {
                var z = -3f + 1.5f * i;
                AddPart(group, "Bombilla izquierda " + i, PrimitiveType.Sphere, new Vector3(-4.8f, GARLAND_HEIGHT, z), Vector3.one * GARLAND_BULB, bulbs[i % bulbs.Length]);
                AddPart(group, "Bombilla derecha " + i, PrimitiveType.Sphere, new Vector3(4.8f, GARLAND_HEIGHT, z), Vector3.one * GARLAND_BULB, bulbs[i % bulbs.Length]);
            }
        }

        // A lit material for the bulbs: the colour is also the glow (Emission, intensity 2).
        static Material CreateBulbMaterial(string name, Color color)
        {
            var material = CreateMaterial(name, color);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * GARLAND_GLOW);
            EditorUtility.SetDirty(material); AssetDatabase.SaveAssets();
            return material;
        }

        // ---- Santa's sleigh (the delivery crate) and the toy machine (the factory): two FBX models from Sketchfab, both CC BY 4.0 ----
        const string SLEIGH_MODEL = "santas_sleigh";
        const string MACHINE_MODEL = "machine_industry_2";
        const string MACHINE_TEXTURE = "Assets/Day08/Models/Textures/Maquina.png";
        const string MACHINE_MATERIAL = "Assets/Day08/Materials/Maquina.mat";
        const string CRATE_OBJECT = "Caja (Socket)";
        static readonly Vector3 SLEIGH_POSITION = new Vector3(1.465f, 0.151f, 0.942f);
        static readonly Vector3 SLEIGH_ROTATION = new Vector3(0.05f, 89.4f, -2.98f);
        const float SLEIGH_SCALE = 0.929f;
        static readonly Vector3 MACHINE_POSITION = new Vector3(-1.837f, 0.909f, 2.624f);
        const float MACHINE_SCALE = 0.305f;
        static readonly Vector3 CRATE_NEW_POSITION = new Vector3(1.465f, 1.147f, 0.443f);   // inside the sleigh's bag

        // The sleigh holds the crate (the crate keeps working, but it is not drawn) and the machine stands at the start of the belt.
        // Without the files in Assets/Day08/Models, nothing changes.
        static void BuildSledAndMachine()
        {
            var group = new GameObject("Trineo y maquina").transform;
            var sleigh = PlaceModel(SLEIGH_MODEL, SLEIGH_POSITION, SLEIGH_ROTATION, SLEIGH_SCALE, group);
            PrepareMachineModel();
            var machine = PlaceModel(MACHINE_MODEL, MACHINE_POSITION, Vector3.zero, MACHINE_SCALE, group);
            if (sleigh != null)
            {
                var crate = GameObject.Find(CRATE_OBJECT);
                if (crate != null)
                {
                    crate.transform.position = CRATE_NEW_POSITION;
                    foreach (var part in crate.GetComponentsInChildren<MeshRenderer>()) part.enabled = false;
                }
            }
            if (sleigh == null && machine == null)
            {
                Debug.Log("Trineo y maquina: copia santas_sleigh.fbx y machine_industry_2.fbx a Assets/Day08/Models.");
                Object.DestroyImmediate(group.gameObject);
            }
        }

        // The machine FBX: its texture goes in a URP Lit material (the only material of the model).
        static void PrepareMachineModel()
        {
            var path = AssetDatabase.GetAssetPath(FindModel(MACHINE_MODEL));
            var importer = string.IsNullOrEmpty(path) ? null : AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) return;

            var material = AssetDatabase.LoadAssetAtPath<Material>(MACHINE_MATERIAL);
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(MACHINE_TEXTURE);
            if (material == null && texture != null)
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(MACHINE_MATERIAL));
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.SetTexture("_BaseMap", texture);
                material.SetFloat("_Smoothness", 0.4f);
                AssetDatabase.CreateAsset(material, MACHINE_MATERIAL);
            }
            bool changed = false;
            if (material != null)
            {
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), "Maquina"), material);
                changed = true;
            }
            if (changed) importer.SaveAndReimport();
        }

        static GameObject FindModel(string fileName)
        {
            foreach (var guid in AssetDatabase.FindAssets(fileName))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileNameWithoutExtension(path) != fileName || !path.EndsWith(".fbx")) continue;
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (model != null) return model;
            }
            return null;
        }

        // Puts a model (found by file name) in the scene with the given position, rotation and scale, and without colliders.
        static GameObject PlaceModel(string fileName, Vector3 position, Vector3 euler, float scale, Transform parent)
        {
            var model = FindModel(fileName);
            if (model == null) return null;
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
            foreach (var collider in instance.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(collider);
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = position;
            instance.transform.localEulerAngles = euler;
            instance.transform.localScale = Vector3.one * scale;
            return instance;
        }

        // A cosy corner with the pack's table and chair, and a few gifts and ornaments on and around them.
        static void BuildCorner()
        {
            var corner = new GameObject("Rincon acogedor").transform;
            void Put(string prefab, Vector3 position, Vector3 euler, float height) =>
                VrClass.Tools.RealAssets.Place(prefab, position, euler, height, corner);

            Put("PF_Farm_KitchenTable_01", new Vector3(-4.2f, 0f, 0.6f), new Vector3(0f, 90f, 0f), 0.75f);
            Put("PF_Farm_WoodChair_01", new Vector3(4.2f, 0f, 2.3f), new Vector3(0f, -40f, 0f), 0.9f);
        }

        // Builds a light snow particle system above the workshop.
        static void BuildSnow()
        {
            var snowObject = new GameObject(SNOW_NAME);
            snowObject.transform.position = new Vector3(0f, SNOW_HEIGHT, SNOW_DISTANCE);
            var particles = snowObject.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = SNOW_LIFETIME;
            main.startSpeed = SNOW_SPEED;
            main.startSize = new ParticleSystem.MinMaxCurve(SNOW_SIZE_MIN, SNOW_SIZE_MAX);   // every flake has its own size
            main.startColor = SNOW_COLOR;
            main.gravityModifier = SNOW_GRAVITY;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = SNOW_MAX_PARTICLES;

            var emission = particles.emission;
            emission.enabled = true;
            emission.rateOverTime = SNOW_RATE;

            var shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = SNOW_SHAPE_SCALE;
            shape.rotation = SNOW_SHAPE_ROTATION;

            // The flakes drift a little and shrink as they fall, so they melt before reaching the floor.
            var noise = particles.noise;
            noise.enabled = true;
            noise.strength = SNOW_NOISE_STRENGTH;
            noise.frequency = SNOW_NOISE_FREQUENCY;
            var size = particles.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

            snowObject.GetComponent<ParticleSystemRenderer>().sharedMaterial = CreateParticleMaterial(MATERIAL_COPO, Color.white);
        }

        // Adds a cube with the given position, scale and material.
        static void AddBox(string name, Vector3 position, Vector3 scale, Material material)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.position = position;
            box.transform.localScale = scale;
            box.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        // Creates (or updates) a lit material asset with the given colour.
        static Material CreateMaterial(string name, Color color)
        {
            var path = MATERIALS_FOLDER + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                var shader = Shader.Find(URP_LIT_SHADER);
                if (shader == null) shader = Shader.Find(FALLBACK_SHADER);
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(material); AssetDatabase.SaveAssets();
            return material;
        }

        // Creates (or updates) a lit material that also emits light.
        static Material CreateFireMaterial(string name, Color emissionColor)
        {
            var material = CreateMaterial(name, FIRE_BASE_COLOR);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emissionColor);
            EditorUtility.SetDirty(material); AssetDatabase.SaveAssets();
            return material;
        }

        // The snowflake picture: a white round dot with a soft edge (64 x 64 px). It is saved as a PNG when it is not in the project yet.
        static Texture2D EnsureSnowflakeTexture()
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(SNOWFLAKE_TEXTURE);
            if (texture != null) return texture;

            var picture = new Texture2D(SNOWFLAKE_SIZE, SNOWFLAKE_SIZE, TextureFormat.RGBA32, false);
            var radius = SNOWFLAKE_SIZE / 2f;
            for (var y = 0; y < SNOWFLAKE_SIZE; y++)
            {
                for (var x = 0; x < SNOWFLAKE_SIZE; x++)
                {
                    var distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(radius, radius)) / radius;
                    picture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01((1f - distance) * 8f)));
                }
            }
            System.IO.File.WriteAllBytes(SNOWFLAKE_TEXTURE, picture.EncodeToPNG());
            Object.DestroyImmediate(picture);
            AssetDatabase.ImportAsset(SNOWFLAKE_TEXTURE);
            var importer = (TextureImporter)AssetImporter.GetAtPath(SNOWFLAKE_TEXTURE);
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(SNOWFLAKE_TEXTURE);
        }

        // Creates the URP particle material used by the snowflakes: the round picture, with Alpha Clipping so only the dot is drawn.
        static Material CreateParticleMaterial(string name, Color color)
        {
            var path = MATERIALS_FOLDER + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                var shader = Shader.Find(URP_PARTICLE_SHADER);
                if (shader == null)
                {
                    Debug.LogWarning("URP particle shader not found: using Particles/Standard Unlit for " + name + ".");
                    shader = Shader.Find(LEGACY_PARTICLE_SHADER);
                }
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", color);
            material.SetTexture("_BaseMap", EnsureSnowflakeTexture());
            material.SetFloat("_AlphaClip", 1f);
            material.SetFloat("_Cutoff", 0.5f);
            material.EnableKeyword("_ALPHATEST_ON");
            material.renderQueue = 2450;
            EditorUtility.SetDirty(material); AssetDatabase.SaveAssets();
            return material;
        }

        // Creates the procedural skybox material used by the scene.
        static Material CreateSky()
        {
            var path = MATERIALS_FOLDER + "/" + MATERIAL_CIELO + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;

            var shader = Shader.Find(SKYBOX_SHADER);
            if (shader == null)
            {
                Debug.LogWarning("Skybox/Procedural not found: keeping the default skybox.");
                return null;
            }

            material = new Material(shader);
            material.SetColor("_SkyTint", SKY_TINT);
            material.SetColor("_GroundColor", SKY_GROUND);
            material.SetFloat("_AtmosphereThickness", SKY_ATMOSPHERE);
            material.SetFloat("_Exposure", SKY_EXPOSURE);
            AssetDatabase.CreateAsset(material, path); AssetDatabase.SaveAssets();
            return material;
        }

        // Adds an XR Interaction Manager when the scene does not have one yet.
        static void CreateInteractionManager()
        {
            if (Object.FindFirstObjectByType<XRInteractionManager>() != null) return;
            new GameObject(INTERACTION_MANAGER_NAME).AddComponent<XRInteractionManager>();
        }

        // Creates a project folder when it does not exist yet.
        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
        }

        // Instantiates a prefab from the imported XRI samples.
        static void AddSamplePrefab(string prefabName)
        {
            foreach (var guid in AssetDatabase.FindAssets(prefabName + " t:Prefab", new[] { SAMPLES_FOLDER }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileNameWithoutExtension(path) != prefabName) continue;
                PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path)); return;
            }
            Debug.LogWarning($"Prefab '{prefabName}' not found: import the XRI samples first.");
        }
    }
}
