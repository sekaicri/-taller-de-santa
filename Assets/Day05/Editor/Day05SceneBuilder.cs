using UnityEditor;

namespace Day05.Editor
{
    /// <summary>
    /// Builds the Day 05 scene: the packing workshop from Day 03 plus this day's stage (see VrClass.Tools.WorkshopStages).
    /// </summary>
    public static class Day05SceneBuilder
    {
        const string SCENE_PATH = "Assets/Day05/Day05_Locomotion.unity";

        // Builds the Day 05 scene and saves it as an asset.
        [MenuItem("VR Class/Day 05/Build Locomotion Scene")]
        public static void BuildScene() =>
            Day03.Editor.Day03SceneBuilder.BuildWorkshop(SCENE_PATH, () => VrClass.Tools.WorkshopStages.Apply(5));
    }
}
