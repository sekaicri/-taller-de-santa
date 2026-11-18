using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.Features.MetaQuestSupport;

namespace Day09.Editor
{
    /// <summary>
    /// Prepares the project for a Quest build: scene list, Android player settings and OpenXR.
    /// </summary>
    public static class Day09BuildHelper
    {
        const string QUEST_BUNDLE_ID = "com.tallerdesanta.vrclass";
        const string OPEN_XR_LOADER_TYPE = "UnityEngine.XR.OpenXR.OpenXRLoader";
        const string OPEN_XR_SETTINGS_WARNING = "Day 09: no XR settings asset for Android yet. Abre Project Settings > XR Plug-in Management > Android y marca OpenXR (instala com.unity.xr.management y com.unity.xr.openxr).";
        const string OPEN_XR_ACTIVE_MESSAGE = "Day 09: OpenXR ya esta activo para Android.";
        const string OPEN_XR_ENABLED_MESSAGE = "Day 09: OpenXR activado para Android.";
        const string OPEN_XR_MANUAL_WARNING = "Day 09: no se pudo activar OpenXR por API. Marca la casilla OpenXR en Project Settings > XR Plug-in Management > Android.";
        const string QUEST_FEATURES_MESSAGE = "Day 09: perfil Oculus Touch Controller y Meta Quest Support activados.";
        const string QUEST_FEATURES_WARNING = "Day 09: no se encontraron los ajustes de OpenXR para Android. Activa a mano el perfil Oculus Touch Controller y Meta Quest Support.";
        const string ANDROID_READY_MESSAGE = "Day 09: Android minSdk 29, ARM64, IL2CPP Release, bundle " + QUEST_BUNDLE_ID + ".";
        const string BUILD_READY_MESSAGE = "Day 09 ready: escenas en el build, Android en ARM64/IL2CPP y revision de OpenXR. Abre File > Build Profiles para compilar: este menu nunca inicia la build.";

        static readonly string[] SCENE_PATHS =
        {
            "Assets/Day08/Day08_Polish.unity",
        };

        // Applies the scene list, Android settings and OpenXR check without building.
        [MenuItem("VR Class/Day 09/Configure Quest Build")]
        public static void ConfigureQuestBuild()
        {
            ConfigureSceneList();
            ConfigureAndroidPlayer();
            ConfigureOpenXR();
            Debug.Log(BUILD_READY_MESSAGE);
        }

        // Copies every existing course scene into the build scene list.
        static void ConfigureSceneList()
        {
            var scenes = new List<EditorBuildSettingsScene>();
            foreach (var path in SCENE_PATHS)
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
                {
                    Debug.LogWarning($"Scene not found, skipped: {path}. Build it from its VR Class menu first.");
                    continue;
                }
                scenes.Add(new EditorBuildSettingsScene(path, true));
            }
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log($"Day 09: {scenes.Count} escenas en la lista de build.");
        }

        // Sets the Android player options that Meta Quest needs.
        static void ConfigureAndroidPlayer()
        {
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetIl2CppCompilerConfiguration(NamedBuildTarget.Android, Il2CppCompilerConfiguration.Release);
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, QUEST_BUNDLE_ID);
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            Debug.Log(ANDROID_READY_MESSAGE);
        }

        // Turns on, for Android, the Quest controller profile and Meta Quest Support (without them the controllers do not respond).
        static void EnableQuestFeatures()
        {
            var openXr = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            if (openXr == null) { Debug.LogWarning(QUEST_FEATURES_WARNING); return; }
            foreach (var feature in openXr.GetFeatures<OpenXRFeature>())
                if (feature is OculusTouchControllerProfile || feature is MetaQuestFeature) { feature.enabled = true; EditorUtility.SetDirty(feature); }
            AssetDatabase.SaveAssets();
            Debug.Log(QUEST_FEATURES_MESSAGE);
        }

        // Enables the OpenXR loader for Android when the XR settings exist.
        static void ConfigureOpenXR()
        {
            try
            {
                var settings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
                if (settings == null || settings.Manager == null)
                {
                    Debug.LogWarning(OPEN_XR_SETTINGS_WARNING);
                    return;
                }

                if (XRPackageMetadataStore.IsLoaderAssigned(OPEN_XR_LOADER_TYPE, BuildTargetGroup.Android))
                    Debug.Log(OPEN_XR_ACTIVE_MESSAGE);
                else if (XRPackageMetadataStore.AssignLoader(settings.Manager, OPEN_XR_LOADER_TYPE, BuildTargetGroup.Android))
                    Debug.Log(OPEN_XR_ENABLED_MESSAGE);
                else
                {
                    Debug.LogWarning(OPEN_XR_MANUAL_WARNING);
                    return;
                }
                EnableQuestFeatures();
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning("Day 09: no se pudo revisar OpenXR por API (" + exception.Message + "). Configuralo en Project Settings > XR Plug-in Management.");
            }
        }
    }

    /// <summary>
    /// The XR Device Simulator is only for testing on the PC. In the Quest it would take over the camera and the controllers,
    /// so it is removed from the copy of the scene that goes into the Android build (the scene in the project keeps it).
    /// </summary>
    public class RemoveSimulatorFromQuestBuild : IProcessSceneWithReport
    {
        const string SIMULATOR_NAME = "XR Device Simulator";

        public int callbackOrder => 0;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if (report == null || report.summary.platform != BuildTarget.Android) return;
            foreach (var root in scene.GetRootGameObjects())
            {
                if (!root.name.StartsWith(SIMULATOR_NAME)) continue;
                Debug.Log("Day 09: " + root.name + " quitado de la build de Android.");
                Object.DestroyImmediate(root);
            }
        }
    }
}
