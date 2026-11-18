using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Day02.Editor
{
    /// <summary>
    /// Builds the Day 02 scene: gifts that glow on hover, open on activate and show sparkles.
    /// </summary>
    public static class Day02SceneBuilder
    {
        const string DAY_FOLDER = "Assets/Day02";
        const string SCENE_PATH = "Assets/Day02/Day02_Grab.unity";
        const string MATERIALS_FOLDER = "Assets/Day02/Materials";
        const string SAMPLES_FOLDER = "Assets/Samples";
        const string XR_ORIGIN_PREFAB = "XR Origin (XR Rig)";
        const string DEVICE_SIMULATOR_PREFAB = "XR Device Simulator";
        const string INTERACTION_MANAGER_NAME = "XR Interaction Manager";
        const string LIGHT_NAME = "Directional Light";
        const string FLOOR_NAME = "Floor";
        const string GIFT_PREFIX = "Regalo ";
        const string LID_NAME = "Tapa";
        const string GLOW_NAME = "Brillo";
        const string SURPRISE_NAME = "Sorpresa";
        const string MATERIAL_VERDE = "Verde";
        const string MATERIAL_ROJO = "Rojo";
        const string MATERIAL_CHISPAS = "Chispas";
        const string URP_LIT_SHADER = "Universal Render Pipeline/Lit";
        const string URP_PARTICLE_SHADER = "Universal Render Pipeline/Particles/Unlit";
        const string LEGACY_PARTICLE_SHADER = "Particles/Standard Unlit";
        const string OPEN_SOUND_PATH = "Assets/Audio/victory.mp3";
        const string READY_MESSAGE = "Day 02 scene ready: hover a gift to see it glow, grab it (turns red), release it (turns green), then pull the trigger to open it with sound and sparkles.";

        const float GIFT_SPACING = 0.4f;
        const float GIFT_HEIGHT = 1f;
        const float GIFT_DISTANCE = 0.6f;
        const float GIFT_SCALE = 0.2f;
        const float LID_HEIGHT = 0.55f;
        const float LID_SCALE = 1.1f;
        const float LID_THICKNESS = 0.1f;
        const float GLOW_HEIGHT = 0.8f;
        const float GLOW_INTENSITY = 2f;
        const float GLOW_RANGE = 1.5f;
        const float SURPRISE_HEIGHT = 0.6f;
        const float PARTICLE_LIFETIME = 1.5f;
        const float PARTICLE_SPEED = 0.6f;
        const float PARTICLE_SIZE = 0.05f;
        const float PARTICLE_GRAVITY = 0.05f;
        const float PARTICLE_RATE = 25f;
        const float PARTICLE_RADIUS = 0.3f;

        const string SIMPLE_BOX_NAME = "Caja Simple";
        const float SIMPLE_BOX_SCALE = 0.25f;
        static readonly Vector3 SIMPLE_BOX_POSITION = new Vector3(0f, 1f, 1.1f);
        // Regalo 1 sticks to the hand, Regalo 2 follows with a delay, Regalo 3 has weight and collides.
        static readonly XRBaseInteractable.MovementType[] MOVEMENTS =
        {
            XRBaseInteractable.MovementType.Instantaneous,
            XRBaseInteractable.MovementType.Kinematic,
            XRBaseInteractable.MovementType.VelocityTracking
        };
        static readonly Vector3 LIGHT_ROTATION = new Vector3(50f, -30f, 0f);
        static readonly Color VERDE_COLOR = new Color(0.1f, 0.7f, 0.15f);
        static readonly Color ROJO_COLOR = new Color(0.8f, 0.1f, 0.1f);
        static readonly Color GLOW_COLOR = new Color(1f, 0.85f, 0.4f);
        static readonly Color SPARK_COLOR = new Color(1f, 0.9f, 0.55f, 1f);
        static readonly float[] GIFT_X_POSITIONS = { -GIFT_SPACING, 0f, GIFT_SPACING };

        static GiftCounter _counter;

        const string ANIMATIONS_FOLDER = "Assets/Day02/Animations";
        const string OPEN_TRIGGER = "Abrir";
        const string CLOSE_TRIGGER = "Cerrar";
        const float LID_OPEN_ANGLE = -110f;
        const float LID_OPEN_LIFT = 0.15f;
        const float LID_ANIMATION_SECONDS = 0.5f;

        static AnimatorController _lidController;

        // Builds the full Day 02 scene with one gift per X position and saves it.
        [MenuItem("VR Class/Day 02/Build Gift Scene")]
        public static void BuildScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EnsureFolder(DAY_FOLDER); EnsureFolder(MATERIALS_FOLDER);

            var light = new GameObject(LIGHT_NAME).AddComponent<Light>();
            light.type = LightType.Directional; light.transform.rotation = Quaternion.Euler(LIGHT_ROTATION);
            GameObject.CreatePrimitive(PrimitiveType.Plane).name = FLOOR_NAME;

            var lidMaterial = CreateMaterial(MATERIAL_ROJO, ROJO_COLOR);
            var bodyMaterial = CreateMaterial(MATERIAL_VERDE, VERDE_COLOR);
            var sparkMaterial = CreateSparkMaterial();

            _lidController = null;
            _counter = new GameObject("Contador").AddComponent<GiftCounter>();
            for (var index = 0; index < GIFT_X_POSITIONS.Length; index++)
                BuildGift(GIFT_PREFIX + (index + 1), GIFT_X_POSITIONS[index], MOVEMENTS[index], lidMaterial, bodyMaterial, sparkMaterial);
            BuildSimpleBox(lidMaterial, bodyMaterial);

            CreateInteractionManager();
            AddSamplePrefab(XR_ORIGIN_PREFAB); AddSamplePrefab(DEVICE_SIMULATOR_PREFAB);

            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            Debug.Log(READY_MESSAGE);
        }

        // Builds one gift with its lid, glow light and surprise particles.
        static void BuildGift(string name, float x, XRBaseInteractable.MovementType movement, Material lidMaterial, Material bodyMaterial, Material sparkMaterial)
        {
            var gift = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gift.name = name; gift.transform.position = new Vector3(x, GIFT_HEIGHT, GIFT_DISTANCE);
            gift.transform.localScale = Vector3.one * GIFT_SCALE;
            gift.AddComponent<Rigidbody>(); gift.GetComponent<MeshRenderer>().sharedMaterial = bodyMaterial;
            var grab = gift.AddComponent<XRGrabInteractable>(); grab.throwOnDetach = true;
            grab.movementType = movement;

            var lid = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lid.name = LID_NAME; lid.transform.SetParent(gift.transform, false);
            lid.transform.localPosition = new Vector3(0f, LID_HEIGHT, 0f);
            lid.transform.localScale = new Vector3(LID_SCALE, LID_THICKNESS, LID_SCALE);
            lid.GetComponent<MeshRenderer>().sharedMaterial = lidMaterial;
            Object.DestroyImmediate(lid.GetComponent<BoxCollider>());

            var glow = new GameObject(GLOW_NAME);
            glow.transform.SetParent(gift.transform, false); glow.transform.localPosition = new Vector3(0f, GLOW_HEIGHT, 0f);
            var glowLight = glow.AddComponent<Light>();
            glowLight.type = LightType.Point; glowLight.color = GLOW_COLOR;
            glowLight.intensity = GLOW_INTENSITY; glowLight.range = GLOW_RANGE;
            glow.SetActive(false);

            var surprise = new GameObject(SURPRISE_NAME);
            surprise.transform.SetParent(gift.transform, false); surprise.transform.localPosition = new Vector3(0f, SURPRISE_HEIGHT, 0f);
            var particles = surprise.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.loop = true; main.startLifetime = PARTICLE_LIFETIME; main.startSpeed = PARTICLE_SPEED;
            main.startSize = PARTICLE_SIZE; main.startColor = SPARK_COLOR;
            main.gravityModifier = PARTICLE_GRAVITY; main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Local;
            var emission = particles.emission; emission.enabled = true; emission.rateOverTime = PARTICLE_RATE;
            var shape = particles.shape; shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere; shape.radius = PARTICLE_RADIUS;
            surprise.GetComponent<ParticleSystemRenderer>().sharedMaterial = sparkMaterial;
            surprise.SetActive(false);

            UnityEventTools.AddBoolPersistentListener(grab.hoverEntered, glow.SetActive, true);
            UnityEventTools.AddBoolPersistentListener(grab.hoverExited, glow.SetActive, false);
            // The lid opens with an animation (Animator trigger) instead of disappearing.
            var lidAnimator = lid.AddComponent<Animator>();
            lidAnimator.runtimeAnimatorController = GetLidController();
            UnityEventTools.AddStringPersistentListener(grab.activated, lidAnimator.SetTrigger, OPEN_TRIGGER);
            UnityEventTools.AddBoolPersistentListener(grab.activated, surprise.SetActive, true);

            // Sound: one more thing that happens when the gift is activated.
            var audio = gift.AddComponent<AudioSource>();
            audio.playOnAwake = false; audio.spatialBlend = 1f;
            audio.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(OPEN_SOUND_PATH);
            if (audio.clip == null) Debug.LogWarning($"Sound '{OPEN_SOUND_PATH}' not found: copy victory.mp3 into Assets/Audio and build again.");
            UnityEventTools.AddVoidPersistentListener(grab.activated, audio.Play);
            UnityEventTools.AddVoidPersistentListener(grab.activated, _counter.Open);

            // The gift closes itself a few seconds after being opened.
            var autoClose = gift.AddComponent<GiftAutoClose>();
            autoClose.Lid = lid; autoClose.Surprise = surprise; autoClose.LidAnimator = lidAnimator;
            UnityEventTools.AddVoidPersistentListener(grab.activated, autoClose.CloseLater);

            // Select: the gift turns red while held and green again when released.
            var body = gift.GetComponent<MeshRenderer>();
            var setMaterial = (UnityAction<Material>)System.Delegate.CreateDelegate(
                typeof(UnityAction<Material>), body, typeof(Renderer).GetProperty("sharedMaterial").GetSetMethod());
            UnityEventTools.AddObjectPersistentListener(grab.selectEntered, setMaterial, lidMaterial);
            UnityEventTools.AddObjectPersistentListener(grab.selectExited, setMaterial, bodyMaterial);
        }

        // Builds a box that can be pointed at and pressed but never picked up (XR Simple Interactable).
        static void BuildSimpleBox(Material hoverMaterial, Material idleMaterial)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = SIMPLE_BOX_NAME; box.transform.position = SIMPLE_BOX_POSITION;
            box.transform.localScale = Vector3.one * SIMPLE_BOX_SCALE;
            var renderer = box.GetComponent<MeshRenderer>(); renderer.sharedMaterial = idleMaterial;
            var simple = box.AddComponent<XRSimpleInteractable>();

            var audio = box.AddComponent<AudioSource>();
            audio.playOnAwake = false; audio.spatialBlend = 1f;
            audio.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(OPEN_SOUND_PATH);

            var setMaterial = (UnityAction<Material>)System.Delegate.CreateDelegate(
                typeof(UnityAction<Material>), renderer, typeof(Renderer).GetProperty("sharedMaterial").GetSetMethod());
            UnityEventTools.AddObjectPersistentListener(simple.hoverEntered, setMaterial, hoverMaterial);
            UnityEventTools.AddObjectPersistentListener(simple.hoverExited, setMaterial, idleMaterial);
            UnityEventTools.AddVoidPersistentListener(simple.selectEntered, audio.Play);
        }

        // Creates (or updates) a lit material asset with the given colour.
        static Material CreateMaterial(string name, Color color)
        {
            var path = MATERIALS_FOLDER + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find(URP_LIT_SHADER));
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(material); AssetDatabase.SaveAssets();
            return material;
        }

        // Creates the transparent material used by the surprise particles.
        static Material CreateSparkMaterial()
        {
            var path = MATERIALS_FOLDER + "/" + MATERIAL_CHISPAS + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            var shader = Shader.Find(URP_PARTICLE_SHADER);
            if (shader == null)
            {
                Debug.LogWarning("URP particle shader not found: using Particles/Standard Unlit for Chispas.");
                shader = Shader.Find(LEGACY_PARTICLE_SHADER);
            }
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path); AssetDatabase.SaveAssets();
            return material;
        }

        // Adds an XR Interaction Manager when the scene does not have one yet.
        // Creates (once per build) the Animator Controller with Cerrada, TapaAbrir and TapaCerrar and its triggers.
        static AnimatorController GetLidController()
        {
            if (_lidController != null) return _lidController;
            EnsureFolder(ANIMATIONS_FOLDER);
            var openClip = CreateLidClip("TapaAbrir", 0f, LID_OPEN_ANGLE, 0f, LID_OPEN_LIFT);
            var closeClip = CreateLidClip("TapaCerrar", LID_OPEN_ANGLE, 0f, LID_OPEN_LIFT, 0f);

            var path = ANIMATIONS_FOLDER + "/Tapa.controller";
            AssetDatabase.DeleteAsset(path);
            var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.AddParameter(OPEN_TRIGGER, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(CLOSE_TRIGGER, AnimatorControllerParameterType.Trigger);

            var machine = controller.layers[0].stateMachine;
            var closed = machine.AddState("Cerrada");
            var opening = machine.AddState("TapaAbrir"); opening.motion = openClip;
            var closing = machine.AddState("TapaCerrar"); closing.motion = closeClip;
            machine.defaultState = closed;
            AddTransition(closed, opening, OPEN_TRIGGER);
            AddTransition(closing, opening, OPEN_TRIGGER);
            AddTransition(opening, closing, CLOSE_TRIGGER);
            AssetDatabase.SaveAssets();
            _lidController = controller;
            return controller;
        }

        static void AddTransition(AnimatorState from, AnimatorState to, string trigger)
        {
            var transition = from.AddTransition(to);
            transition.hasExitTime = false; transition.duration = 0f;
            transition.AddCondition(AnimatorConditionMode.If, 0f, trigger);
        }

        // A clip that rotates the lid around X and lifts it a little.
        static AnimationClip CreateLidClip(string name, float fromAngle, float toAngle, float fromLift, float toLift)
        {
            var clip = new AnimationClip { name = name };
            clip.SetCurve("", typeof(Transform), "localEulerAnglesRaw.x", AnimationCurve.EaseInOut(0f, fromAngle, LID_ANIMATION_SECONDS, toAngle));
            clip.SetCurve("", typeof(Transform), "localPosition.y", AnimationCurve.EaseInOut(0f, LID_HEIGHT + fromLift, LID_ANIMATION_SECONDS, LID_HEIGHT + toLift));
            var path = ANIMATIONS_FOLDER + "/" + name + ".anim";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(clip, path);
            return clip;
        }

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
