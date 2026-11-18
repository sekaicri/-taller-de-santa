using UnityEditor;

namespace Day07.Editor
{
    /// <summary>
    /// Builds the Day 07 scene: the packing workshop from Day 03 plus this day's stage (see VrClass.Tools.WorkshopStages).
    /// </summary>
    public static class Day07SceneBuilder
    {
        const string SCENE_PATH = "Assets/Day07/Day07_Logic.unity";

        // Builds the Day 07 scene and saves it as an asset.
        [MenuItem("VR Class/Day 07/Build Game Logic Scene")]
        public static void BuildScene() =>
            Day03.Editor.Day03SceneBuilder.BuildWorkshop(SCENE_PATH, () => VrClass.Tools.WorkshopStages.Apply(7));
    }
}
