using UnityEditor;

namespace Day04.Editor
{
    /// <summary>
    /// Builds the Day 04 scene: the packing workshop from Day 03 plus this day's stage (see VrClass.Tools.WorkshopStages).
    /// </summary>
    public static class Day04SceneBuilder
    {
        const string SCENE_PATH = "Assets/Day04/Day04_UI.unity";

        // Builds the Day 04 scene and saves it as an asset.
        [MenuItem("VR Class/Day 04/Build UI Scene")]
        public static void BuildScene() =>
            Day03.Editor.Day03SceneBuilder.BuildWorkshop(SCENE_PATH, () => VrClass.Tools.WorkshopStages.Apply(4));
    }
}
