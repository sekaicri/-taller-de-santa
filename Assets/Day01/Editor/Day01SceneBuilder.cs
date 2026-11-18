using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Day01.Editor
{
    /// <summary>
    /// Builds the Day 01 scene: a floor, one grabbable gift and the XR rig.
    /// </summary>
    public static class Day01SceneBuilder
    {
        const string DAY_FOLDER = "Assets/Day01";
        const string SCENE_PATH = "Assets/Day01/Day01_Grab.unity";
        const string SAMPLES_FOLDER = "Assets/Samples";
        const string XR_ORIGIN_PREFAB = "XR Origin (XR Rig)";
        const string DEVICE_SIMULATOR_PREFAB = "XR Device Simulator";
        const string INTERACTION_MANAGER_NAME = "XR Interaction Manager";
        const string LIGHT_NAME = "Directional Light";
        const string FLOOR_NAME = "Floor";
        const string GIFT_NAME = "Regalo";
        const string READY_MESSAGE = "Day 01 scene ready: press Play.";
        const float GIFT_SCALE = 0.2f;
        const float GIFT_HEIGHT = 1f;
        const float GIFT_DISTANCE = 0.6f;
        static readonly Vector3 LIGHT_ROTATION = new Vector3(50f, -30f, 0f);

        // Builds the full Day 01 scene and saves it as an asset.
        [MenuItem("VR Class/Day 01/Build Grab Scene")]
        public static void BuildScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EnsureFolder(DAY_FOLDER);

            var light = new GameObject(LIGHT_NAME).AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(LIGHT_ROTATION);

            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = FLOOR_NAME;

            var gift = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gift.name = GIFT_NAME;
            gift.transform.SetPositionAndRotation(new Vector3(0f, GIFT_HEIGHT, GIFT_DISTANCE), Quaternion.identity);
            gift.transform.localScale = Vector3.one * GIFT_SCALE;
            gift.AddComponent<Rigidbody>();
            gift.AddComponent<XRGrabInteractable>();

            CreateInteractionManager();
            AddSamplePrefab(XR_ORIGIN_PREFAB);
            AddSamplePrefab(DEVICE_SIMULATOR_PREFAB);

            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            Debug.Log(READY_MESSAGE);
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
                PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));
                return;
            }
            Debug.LogWarning($"Prefab '{prefabName}' not found: import the XRI samples first.");
        }
    }
}
