using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Day00.Editor
{
    /// <summary>
    /// Teacher-only tool that rebuilds and saves every course scene from Day 01 to Day 08 in order.
    /// </summary>
    public static class BuildAllScenes
    {
        const string MENU_PATH = "VR Class/Build ALL Scenes (Day 01-08)";
        const string DIALOG_TITLE = "Build all scenes";
        const string DIALOG_MESSAGE = "This rebuilds and overwrites the Day 01-08 scenes. Continue?";
        const string DIALOG_CONFIRM = "Build all";
        const string DIALOG_CANCEL = "Cancel";
        const string PROGRESS_TITLE = "Building scenes";
        const string PROGRESS_PREFIX = "Building ";
        const string ERROR_PREFIX = "[BuildAllScenes] ";
        const string ERROR_SUFFIX = " failed: ";
        const string SUMMARY_HEADER = "Build ALL scenes finished.";
        const string SUMMARY_SUCCEEDED = "Succeeded: ";
        const string SUMMARY_FAILED = "Failed: ";
        const string SUMMARY_SCENES = "Saved scenes:";
        const string NONE_LABEL = "none";
        const string UNSAVED_SCENE_LABEL = "(scene not saved)";
        const string LIST_SEPARATOR = ", ";
        const string NEW_LINE = "\n";

        static readonly (string Label, Action Build)[] BUILDERS =
        {
            ("Day 01", Day01.Editor.Day01SceneBuilder.BuildScene),
            ("Day 02", Day02.Editor.Day02SceneBuilder.BuildScene),
            ("Day 03", Day03.Editor.Day03SceneBuilder.BuildScene),
            ("Day 04", Day04.Editor.Day04SceneBuilder.BuildScene),
            ("Day 05", Day05.Editor.Day05SceneBuilder.BuildScene),
            ("Day 06", Day06.Editor.Day06SceneBuilder.BuildScene),
            ("Day 07", Day07.Editor.Day07SceneBuilder.BuildScene),
            ("Day 08", Day08.Editor.Day08SceneBuilder.BuildScene),
        };

        // Asks for confirmation and then runs every builder, one scene per day.
        [MenuItem(MENU_PATH)]
        public static void BuildAll()
        {
            if (!EditorUtility.DisplayDialog(DIALOG_TITLE, DIALOG_MESSAGE, DIALOG_CONFIRM, DIALOG_CANCEL)) return;

            var succeededDays = new List<string>();
            var failedDays = new List<string>();
            var savedScenePaths = new List<string>();

            try
            {
                for (var index = 0; index < BUILDERS.Length; index++)
                {
                    var label = BUILDERS[index].Label;
                    var build = BUILDERS[index].Build;

                    EditorUtility.DisplayProgressBar(PROGRESS_TITLE, PROGRESS_PREFIX + label, (float)(index + 1) / BUILDERS.Length);
                    try
                    {
                        build();
                        succeededDays.Add(label);
                        savedScenePaths.Add(GetActiveScenePath());
                    }
                    catch (Exception exception)
                    {
                        failedDays.Add(label);
                        Debug.LogError(ERROR_PREFIX + label + ERROR_SUFFIX + exception);
                    }
                }

                LogSummary(succeededDays, failedDays, savedScenePaths);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        // Returns the path of the scene that the last builder saved.
        static string GetActiveScenePath()
        {
            var path = EditorSceneManager.GetActiveScene().path;
            return string.IsNullOrEmpty(path) ? UNSAVED_SCENE_LABEL : path;
        }

        // Logs which days worked, which failed and where the scenes were saved.
        static void LogSummary(List<string> succeededDays, List<string> failedDays, List<string> savedScenePaths)
        {
            Debug.Log(SUMMARY_HEADER + NEW_LINE +
                      SUMMARY_SUCCEEDED + FormatList(succeededDays) + NEW_LINE +
                      SUMMARY_FAILED + FormatList(failedDays) + NEW_LINE +
                      SUMMARY_SCENES + NEW_LINE + FormatLines(savedScenePaths));
        }

        // Joins labels on one line, or returns a friendly "none".
        static string FormatList(List<string> labels)
        {
            return labels.Count > 0 ? string.Join(LIST_SEPARATOR, labels) : NONE_LABEL;
        }

        // Joins scene paths one per line, or returns a friendly "none".
        static string FormatLines(List<string> lines)
        {
            return lines.Count > 0 ? string.Join(NEW_LINE, lines) : NONE_LABEL;
        }
    }
}
