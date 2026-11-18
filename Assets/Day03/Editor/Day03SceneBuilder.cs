using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Attachment;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Day03.Editor
{
    /// <summary>
    /// Builds the Day 03 scene: the packing table of the Santa workshop.
    /// A conveyor belt carries three toys to the right; each toy is wrapped in green paper with a star
    /// when its trigger is pulled, and then dropped into the delivery crate (a socket) on the right.
    /// </summary>
    public static class Day03SceneBuilder
    {
        const string DAY_FOLDER = "Assets/Day03";
        const string SCENE_PATH = "Assets/Day03/Day03_Sockets.unity";
        const string MATERIALS_FOLDER = "Assets/Day03/Materials";
        const string SAMPLES_FOLDER = "Assets/Samples";
        const string SOUND_PATH = "Assets/Audio/victory.mp3";
        const string XR_ORIGIN_PREFAB = "XR Origin (XR Rig)";
        const string DEVICE_SIMULATOR_PREFAB = "XR Device Simulator";
        const string INTERACTION_MANAGER_NAME = "XR Interaction Manager";
        const string LIGHT_NAME = "Directional Light";
        const string FLOOR_NAME = "Floor";
        const string TABLE_NAME = "Mesa de empaque";
        const string BELT_NAME = "Cinta";
        const string STOPPER_NAME = "Tope";
        const string RAIL_FRONT_NAME = "Baranda Frontal";
        const string RAIL_BACK_NAME = "Baranda Trasera";
        const string CRATE_NAME = "Caja (Socket)";
        const string SOCKET_NAME = "Socket";
        const string BASE_NAME = "Base";
        const string NORTH_WALL_NAME = "Pared Norte";
        const string SOUTH_WALL_NAME = "Pared Sur";
        const string EAST_WALL_NAME = "Pared Este";
        const string WEST_WALL_NAME = "Pared Oeste";
        const string WRAPPER_NAME = "Envoltorio";
        const string STAR_NAME = "Estrella";
        const string STAR_PREFAB = "Star";
        const string GRAB_POINT_NAME = "Punto de agarre";
        const string MATERIAL_NIEVE = "Nieve";
        const string MATERIAL_MADERA = "Madera";
        const string MATERIAL_DORADO = "Dorado";
        const string MATERIAL_CINTA = "Cinta";
        const string MATERIAL_VERDE = "Verde";
        const string MATERIAL_ROJO = "Rojo";
        const string URP_LIT_SHADER = "Universal Render Pipeline/Lit";
        const string FALLBACK_SHADER = "Standard";
        const string READY_MESSAGE = "Day 03 scene ready: toys ride the belt; grab one, pull the trigger to wrap it, drop it in the crate on the right.";

        const float TOY_SCALE = 0.18f;
        const float TOY_HEIGHT = 0.94f;
        const float BELT_SPEED = 0.08f;
        const float WRAP_SCALE = 1.3f;
        const float STAR_HEIGHT = 0.12f;
        const float COIN_DIAMETER = 0.12f;
        const float COIN_THICKNESS = 0.01f;
        const float CRATE_SIZE = 0.5f;
        const float CRATE_WALL_HEIGHT = 0.28f;
        const float CRATE_WALL_THICKNESS = 0.04f;
        const float CRATE_FLOOR_THICKNESS = 0.05f;
        const float SOCKET_HOVER_SCALE = 1.05f;
        const float STRONG_THROW_SCALE = 3f;

        static readonly Vector3 LIGHT_ROTATION = new Vector3(50f, -30f, 0f);
        static readonly Vector3 TABLE_POSITION = new Vector3(0f, 0.75f, 0.9f);
        static readonly Vector3 TABLE_SCALE = new Vector3(3.2f, 0.08f, 0.9f);
        static readonly Vector3 BELT_POSITION = new Vector3(-0.4f, 0.815f, 0.9f);
        static readonly Vector3 BELT_SCALE = new Vector3(2.2f, 0.05f, 0.5f);
        static readonly Vector3 STOPPER_POSITION = new Vector3(0.73f, 0.94f, 0.9f);
        static readonly Vector3 STOPPER_SCALE = new Vector3(0.05f, 0.2f, 0.5f);
        static readonly Vector3 RAIL_FRONT_POSITION = new Vector3(-0.4f, 0.89f, 0.65f);
        static readonly Vector3 RAIL_BACK_POSITION = new Vector3(-0.4f, 0.89f, 1.15f);
        static readonly Vector3 RAIL_SCALE = new Vector3(2.2f, 0.1f, 0.03f);
        static readonly Vector3 CRATE_POSITION = new Vector3(1.2f, 0.815f, 0.9f);
        static readonly Vector3 SOCKET_LOCAL_POSITION = new Vector3(0f, 0.12f, 0f);
        static readonly Vector3 SOCKET_SIZE = new Vector3(0.42f, 0.3f, 0.42f);
        static readonly Vector3 GRAB_POINT_LOCAL_POSITION = new Vector3(0.5f, 0.5f, 0f);
        static readonly Color NIEVE_COLOR = new Color(0.92f, 0.94f, 1f);
        static readonly Color MADERA_COLOR = new Color(0.45f, 0.28f, 0.15f);
        static readonly Color DORADO_COLOR = new Color(0.85f, 0.65f, 0.15f);
        static readonly Color CINTA_COLOR = new Color(0.15f, 0.15f, 0.16f);
        static readonly Color VERDE_COLOR = new Color(0.1f, 0.65f, 0.2f);
        static readonly Color ROJO_COLOR = new Color(0.8f, 0.1f, 0.1f);

        static readonly string[] TOY_NAMES = { "Osito", "Bola", "Baston" };
        static readonly string[] TOY_MATERIAL_NAMES = { MATERIAL_MADERA, MATERIAL_ROJO, MATERIAL_NIEVE };
        static readonly float[] TOY_X_POSITIONS = { -0.4f, -0.8f, -1.2f };

        // Builds the full Day 03 scene and saves it as an asset.
        [MenuItem("VR Class/Day 03/Build Sockets Scene")]
        public static void BuildScene() => BuildWorkshop(SCENE_PATH, null);

        // Builds the packing table and saves it at scenePath. Later days pass 'extend' to add their stage on top.
        public static void BuildWorkshop(string scenePath, System.Action extend)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EnsureFolder(DAY_FOLDER); EnsureFolder(MATERIALS_FOLDER);
            EnsureFolder(System.IO.Path.GetDirectoryName(scenePath).Replace('\\', '/'));

            var light = new GameObject(LIGHT_NAME).AddComponent<Light>();
            light.type = LightType.Directional; light.transform.rotation = Quaternion.Euler(LIGHT_ROTATION);
            light.intensity = LIGHT_INTENSITY;   // lower than the default, so the lamp over the table stands out
            BuildLamp();
            EnsureToyLayer();

            var nieve = CreateMaterial(MATERIAL_NIEVE, NIEVE_COLOR);
            var madera = CreateMaterial(MATERIAL_MADERA, MADERA_COLOR);
            var dorado = CreateMaterial(MATERIAL_DORADO, DORADO_COLOR);
            var cinta = CreateMaterial(MATERIAL_CINTA, CINTA_COLOR);
            var verde = CreateMaterial(MATERIAL_VERDE, VERDE_COLOR);
            var rojo = CreateMaterial(MATERIAL_ROJO, ROJO_COLOR);

            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = FLOOR_NAME;
            floor.GetComponent<MeshRenderer>().sharedMaterial = nieve;

            AddSolid(TABLE_NAME, TABLE_POSITION, TABLE_SCALE, madera);
            var belt = AddSolid(BELT_NAME, BELT_POSITION, BELT_SCALE, cinta);
            belt.AddComponent<ConveyorBelt>().Speed = BELT_SPEED;
            AddSolid(STOPPER_NAME, STOPPER_POSITION, STOPPER_SCALE, madera);
            AddSolid(RAIL_FRONT_NAME, RAIL_FRONT_POSITION, RAIL_SCALE, madera);
            AddSolid(RAIL_BACK_NAME, RAIL_BACK_POSITION, RAIL_SCALE, madera);

            BuildSocketCrate(CRATE_POSITION, dorado);

            var toyMaterials = new[] { madera, rojo, nieve };
            var sound = AssetDatabase.LoadAssetAtPath<AudioClip>(SOUND_PATH);
            if (sound == null) Debug.LogWarning($"Sound '{SOUND_PATH}' not found: copy victory.mp3 into Assets/Audio and build again.");
            for (var index = 0; index < TOY_NAMES.Length; index++)
                BuildToy(TOY_NAMES[index], TOY_X_POSITIONS[index], toyMaterials[index], verde, dorado, sound);

            CreateInteractionManager();
            AddSamplePrefab(XR_ORIGIN_PREFAB); AddSamplePrefab(DEVICE_SIMULATOR_PREFAB);

            VrClass.Tools.RealAssets.UpgradeActiveScene();
            BuildToyFactory();
            extend?.Invoke();
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log(READY_MESSAGE);
        }

        // Adds a cube with a collider at a world position.
        static GameObject AddSolid(string name, Vector3 position, Vector3 scale, Material material)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name; cube.transform.position = position; cube.transform.localScale = scale;
            cube.GetComponent<MeshRenderer>().sharedMaterial = material;
            return cube;
        }

        // Builds one toy: grabbable cube on the belt, with green wrapping and a star that appear when activated.
        static void BuildToy(string name, float x, Material material, Material paper, Material gold, AudioClip sound)
        {
            var toy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            toy.name = name; toy.transform.position = new Vector3(x, TOY_HEIGHT, BELT_POSITION.z);
            toy.transform.localScale = Vector3.one * TOY_SCALE;
            toy.GetComponent<MeshRenderer>().sharedMaterial = material;
            toy.AddComponent<Rigidbody>();
            var grab = toy.AddComponent<XRGrabInteractable>();
            ConfigureHolding(toy, grab);
            if (HasToyLayer) grab.interactionLayers = InteractionLayerMask.GetMask("Default", TOY_LAYER);   // hands see it (Default) and the crate accepts it (Juguete)

            var wrapper = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wrapper.name = WRAPPER_NAME; wrapper.transform.SetParent(toy.transform, false);
            wrapper.transform.localScale = Vector3.one * WRAP_SCALE;
            wrapper.GetComponent<MeshRenderer>().sharedMaterial = paper;
            Object.DestroyImmediate(wrapper.GetComponent<BoxCollider>());
            wrapper.SetActive(false);

            var star = BuildStar(toy.transform, gold);
            star.SetActive(false);

            var audio = toy.AddComponent<AudioSource>();
            audio.playOnAwake = false; audio.spatialBlend = 1f; audio.clip = sound;

            UnityEventTools.AddBoolPersistentListener(grab.activated, wrapper.SetActive, true);
            UnityEventTools.AddBoolPersistentListener(grab.activated, star.SetActive, true);
            UnityEventTools.AddVoidPersistentListener(grab.activated, audio.Play);
        }

        // The star from the Christmas Presents pack, or a gold coin when the pack is not imported.
        static GameObject BuildStar(Transform toy, Material gold)
        {
            var top = toy.position + Vector3.up * (TOY_SCALE * 0.5f * WRAP_SCALE);
            var star = VrClass.Tools.RealAssets.Place(STAR_PREFAB, top, Vector3.zero, STAR_HEIGHT, toy);
            if (star == null)
            {
                star = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                star.transform.position = top + Vector3.up * COIN_THICKNESS;
                star.transform.localScale = new Vector3(COIN_DIAMETER, COIN_THICKNESS, COIN_DIAMETER);
                star.GetComponent<MeshRenderer>().sharedMaterial = gold;
                Object.DestroyImmediate(star.GetComponent<Collider>());
                star.transform.SetParent(toy, true);
            }
            star.name = STAR_NAME;
            return star;
        }

        // Each toy teaches one way an object can be held:
        // Osito: grab point at a corner + near far-attach, Bola: strong throw + far far-attach, Baston: two hands.
        static void ConfigureHolding(GameObject toy, XRGrabInteractable grab)
        {
            grab.throwOnDetach = true;
            if (toy.name == TOY_NAMES[0])
            {
                var point = new GameObject(GRAB_POINT_NAME);
                point.transform.SetParent(toy.transform, false); point.transform.localPosition = GRAB_POINT_LOCAL_POSITION;
                grab.attachTransform = point.transform;
                grab.farAttachMode = InteractableFarAttachMode.Near;
            }
            else if (toy.name == TOY_NAMES[1])
            {
                grab.throwVelocityScale = STRONG_THROW_SCALE;
                grab.farAttachMode = InteractableFarAttachMode.Far;
            }
            else if (toy.name == TOY_NAMES[2])
            {
                grab.selectMode = InteractableSelectMode.Multiple;
            }
        }

        // Builds an open crate with a socket inside and returns that socket.
        static XRSocketInteractor BuildSocketCrate(Vector3 position, Material material)
        {
            var crate = new GameObject(CRATE_NAME);
            crate.transform.position = position;

            var half = CRATE_SIZE * 0.5f;
            var wallHeight = CRATE_WALL_HEIGHT * 0.5f;
            var wallSizeX = new Vector3(CRATE_SIZE, CRATE_WALL_HEIGHT, CRATE_WALL_THICKNESS);
            var wallSizeZ = new Vector3(CRATE_WALL_THICKNESS, CRATE_WALL_HEIGHT, CRATE_SIZE);

            AddBox(crate.transform, BASE_NAME, Vector3.zero, new Vector3(CRATE_SIZE, CRATE_FLOOR_THICKNESS, CRATE_SIZE), material);
            AddBox(crate.transform, NORTH_WALL_NAME, new Vector3(0f, wallHeight, half), wallSizeX, material);
            AddBox(crate.transform, SOUTH_WALL_NAME, new Vector3(0f, wallHeight, -half), wallSizeX, material);
            AddBox(crate.transform, EAST_WALL_NAME, new Vector3(half, wallHeight, 0f), wallSizeZ, material);
            AddBox(crate.transform, WEST_WALL_NAME, new Vector3(-half, wallHeight, 0f), wallSizeZ, material);

            var socketObject = new GameObject(SOCKET_NAME);
            socketObject.transform.SetParent(crate.transform, false);
            socketObject.transform.localPosition = SOCKET_LOCAL_POSITION;
            var socketCollider = socketObject.AddComponent<BoxCollider>();
            socketCollider.isTrigger = true;
            socketCollider.size = SOCKET_SIZE;

            var socket = socketObject.AddComponent<XRSocketInteractor>();
            socket.showInteractableHoverMeshes = true;
            socket.interactableHoverScale = SOCKET_HOVER_SCALE;
            socket.hoverSocketSnapping = false;
            if (HasToyLayer) socket.interactionLayers = InteractionLayerMask.GetMask(TOY_LAYER);   // the crate only accepts toys
            return socket;
        }

        const string LAMP_NAME = "Lampara";
        const float LIGHT_INTENSITY = 0.7f;
        const string TOY_LAYER = "Juguete";
        const int TOY_LAYER_INDEX = 6;   // "User Layer 6" in Project Settings > XR Interaction Toolkit > Interaction Layers
        const string LAYERS_ASSET = "Assets/XRI/Settings/Resources/InteractionLayerSettings.asset";
        static readonly Vector3 LAMP_POSITION = new Vector3(0f, 2f, 0.9f);
        static readonly Color LAMP_COLOR = new Color(1f, 0.914f, 0.753f);

        static bool HasToyLayer => InteractionLayerMask.NameToLayer(TOY_LAYER) >= 0;

        // A spot light above the table, looking down.
        static void BuildLamp()
        {
            var lamp = new GameObject(LAMP_NAME).AddComponent<Light>();
            lamp.type = LightType.Spot;
            lamp.transform.position = LAMP_POSITION; lamp.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            lamp.range = 4f; lamp.spotAngle = 80f; lamp.intensity = 3f; lamp.color = LAMP_COLOR;
        }

        // Names the interaction layer "Juguete" in the toolkit settings (the same thing as typing it in Project Settings).
        static void EnsureToyLayer()
        {
            var settings = AssetDatabase.LoadAssetAtPath<ScriptableObject>(LAYERS_ASSET);
            if (settings == null) { Debug.LogWarning($"Interaction layer settings not found at '{LAYERS_ASSET}': name User Layer 6 'Juguete' by hand (Day 3, Interaction Layers step)."); return; }

            var serialized = new SerializedObject(settings);
            var layer = serialized.FindProperty("m_LayerNames")?.GetArrayElementAtIndex(TOY_LAYER_INDEX);
            if (layer == null) return;
            if (!string.IsNullOrEmpty(layer.stringValue) && layer.stringValue != TOY_LAYER)
            {
                Debug.LogWarning($"User Layer {TOY_LAYER_INDEX} is already named '{layer.stringValue}': the crate will accept any object.");
                return;
            }
            layer.stringValue = TOY_LAYER;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }

        const string TEMPLATES_NAME = "Plantillas";
        const string FACTORY_NAME = "Fabrica de juguetes";
        const string COAL_NAME = "Carbon";
        const string MATERIAL_CARBON = "Carbon";
        static readonly Color CARBON_COLOR = new Color(0.08f, 0.08f, 0.09f);
        static readonly Vector3 FACTORY_POSITION = new Vector3(-1.35f, 0.94f, 0.9f);
        const float COAL_SCALE = 0.16f;

        // The crate empties itself after one second, and a factory puts new toys (and coal) on the belt.
        static void BuildToyFactory()
        {
            var socket = Object.FindFirstObjectByType<XRSocketInteractor>();
            if (socket != null) socket.gameObject.AddComponent<SocketClearer>();

            var templates = new GameObject(TEMPLATES_NAME).transform;
            var list = new System.Collections.Generic.List<GameObject>();
            foreach (var toyName in TOY_NAMES)
            {
                var original = GameObject.Find(toyName);
                if (original == null) continue;
                var copy = Object.Instantiate(original, templates);
                copy.name = toyName; copy.SetActive(false);
                list.Add(copy);
            }

            var coal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            coal.name = COAL_NAME; coal.transform.SetParent(templates, false);
            coal.transform.localScale = Vector3.one * COAL_SCALE;
            coal.GetComponent<MeshRenderer>().sharedMaterial = CreateMaterial(MATERIAL_CARBON, CARBON_COLOR);
            coal.AddComponent<Rigidbody>();
            var coalGrab = coal.AddComponent<XRGrabInteractable>();
            coalGrab.throwOnDetach = true;
            // The coal also has the Juguete layer: the crate accepts it, so the game can say "that is not a toy" and push it out.
            if (HasToyLayer) coalGrab.interactionLayers = InteractionLayerMask.GetMask("Default", TOY_LAYER);
            coal.SetActive(false);
            list.Add(coal);

            var factory = new GameObject(FACTORY_NAME);
            factory.transform.position = FACTORY_POSITION;
            factory.AddComponent<ToySpawner>().Templates = list.ToArray();
        }

        // Adds a cube with the given local transform and material under a parent.
        static void AddBox(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, false);
            box.transform.localPosition = localPosition;
            box.transform.localScale = localScale;
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
