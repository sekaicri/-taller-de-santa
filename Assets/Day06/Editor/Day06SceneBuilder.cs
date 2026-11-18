using UnityEditor;

namespace Day06.Editor
{
    /// <summary>
    /// Builds the Day 06 scene: the packing workshop from Day 03 plus this day's stage (see VrClass.Tools.WorkshopStages).
    /// </summary>
    public static class Day06SceneBuilder
    {
        const string SCENE_PATH = "Assets/Day06/Day06_Feedback.unity";

        // Builds the Day 06 scene and saves it as an asset.
        [MenuItem("VR Class/Day 06/Build Feedback Scene")]
        public static void BuildScene() =>
            Day03.Editor.Day03SceneBuilder.BuildWorkshop(SCENE_PATH, () => VrClass.Tools.WorkshopStages.Apply(6));
    }
}
