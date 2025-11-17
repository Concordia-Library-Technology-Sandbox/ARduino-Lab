// Copyright (c)
// Rewritten to use Newtonsoft.JSON instead of OVRSimpleJSON.

using System;
using System.Collections.Generic;
using Meta.XR.Samples;
using PassthroughCameraSamples.StartScene;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.IO;

namespace PassthroughCameraSamples.SelectProject
{
    public class InstructionOverlaysScreen : MonoBehaviour
    {
        [SerializeField] private ArduinoImageOpenAIConnector openAIConnector;
        [SerializeField] private RollingAnimationLoader rollingLoader;


        private DebugUIBuilder uiBuilder;
        private int instructionIndex = 0;
        private JArray instructionsArray;

        // NEW FIELDS FOR CODE PREVIEW
        private bool codeExpanded = false;
        private const int CodePreviewLength = 250;

        private void Start()
        {
            var passthroughScenes = new List<Tuple<int, string>>();
            var n = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;

            for (var sceneIndex = 1; sceneIndex < n; ++sceneIndex)
            {
                var path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(sceneIndex);
                passthroughScenes.Add(new Tuple<int, string>(sceneIndex, path));
            }

            uiBuilder = DebugUIBuilder.Instance;

            if (passthroughScenes.Count > 0)
            {

                rollingLoader.LoadRollingAnimation(DebugUIBuilder.DEBUG_PANE_CENTER);

                _ = uiBuilder.AddLabel("Generating Instructions...", DebugUIBuilder.DEBUG_PANE_CENTER, 33);

                openAIConnector.onJsonReceived.AddListener(OnInstructionsJsonReceived);

                string title = StaticClass.projectTitle;
                string description = StaticClass.projectDescription;
                string components = StaticClass.generateCompoundStringOfComponents();

                openAIConnector.GenerateProjectInstructions(title, description, components);

                uiBuilder.Show();
            }
        }

        private void DebugLong(string tag, string message)
        {
            int max = 800;
            for (int i = 0; i < message.Length; i += max)
            {
                string chunk = message.Substring(i, Mathf.Min(max, message.Length - i));
                Debug.Log($"{tag} ({i}): {chunk}");
            }
        }

        private void SaveJsonToFile(string json)
        {
            try
            {
                string path = Path.Combine(Application.persistentDataPath, "instructions_debug.json");
                File.WriteAllText(path, json);
                Debug.Log("✔ Full JSON saved to: " + path);
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to write JSON file: " + ex);
            }
        }

        private void OnInstructionsJsonReceived(string json)
        {
            try
            {
                JObject root = JObject.Parse(json);
                string content = root["choices"][0]["message"]["content"].ToString();

                DebugLong("RAW_CONTENT", content);

                JObject instructionsRoot = JObject.Parse(content);
                instructionsArray = (JArray)instructionsRoot["instructions"];

                if (instructionsArray == null || instructionsArray.Count == 0)
                {
                    Debug.LogError("Instructions array missing or empty.");
                    return;
                }

                instructionIndex = 0;
                DrawStepUI();
            }
            catch (Exception e)
            {
                Debug.LogError("ERROR parsing instructions JSON: " + e);
            }
        }

        private void DrawStepUI()
        {
            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_CENTER);
            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_LEFT);
            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_RIGHT);

            JObject step = (JObject)instructionsArray[instructionIndex];
            string content = step["text"]?.ToString() ?? "";
            string imagePrompt = step["image_prompt"]?.ToString() ?? "";

            string code = null;

            // NOTE: your JSON for "code" changed — update as needed
            if (step["code"] != null && step["code"].Type != JTokenType.Null)
            {
                // NEW FORMAT EXPECTED:
                //   "code": { "snippet": "..." }
                code = step["code"]["snippet"]?.ToString();
            }

            _ = uiBuilder.AddLabel($"Step {instructionIndex + 1} of {instructionsArray.Count}", DebugUIBuilder.DEBUG_PANE_CENTER, 40);
            _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_CENTER);

            _ = uiBuilder.AddParagraph(content, DebugUIBuilder.DEBUG_PANE_CENTER, 30);

            //-----------------------------------
            //   CODE SNIPPET REGION
            //-----------------------------------
            if (!string.IsNullOrEmpty(code))
            {
                string displayCode = code;

                if (!codeExpanded && code.Length > CodePreviewLength)
                {
                    displayCode = code.Substring(0, CodePreviewLength) + "...";
                }

                // Expand / Collapse button
                string expandLabel = codeExpanded ? "Collapse Code ▲" : "Expand Code ▼";

                _ = uiBuilder.AddButton(expandLabel, () =>
                {
                    codeExpanded = !codeExpanded;
                    DrawStepUI();
                }, -1, DebugUIBuilder.DEBUG_PANE_CENTER);

                // Show the code in the RIGHT pane
                uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_RIGHT);

                _ = uiBuilder.AddLabel("Code Snippet", DebugUIBuilder.DEBUG_PANE_RIGHT, 40);
                _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_RIGHT);
                _ = uiBuilder.AddParagraph(displayCode, DebugUIBuilder.DEBUG_PANE_RIGHT, 26);

                // Better close button location: BELOW code block
                _ = uiBuilder.AddButton("Close Code", () =>
                {
                    uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_RIGHT);
                }, -1, DebugUIBuilder.DEBUG_PANE_RIGHT);
            }

            //-----------------------------------
            //   NAVIGATION BUTTONS
            //-----------------------------------

            _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_CENTER);

            if (instructionIndex > 0)
            {
                _ = uiBuilder.AddButton("◀ Previous", () =>
                {
                    codeExpanded = false;
                    instructionIndex--;
                    DrawStepUI();
                }, -1, DebugUIBuilder.DEBUG_PANE_CENTER);
            }

            if (instructionIndex < instructionsArray.Count - 1)
            {
                _ = uiBuilder.AddButton("Next ▶", () =>
                {
                    codeExpanded = false;
                    instructionIndex++;
                    DrawStepUI();
                }, -1, DebugUIBuilder.DEBUG_PANE_CENTER);
            }

            uiBuilder.Show();
        }

        private void LoadScene(int idx)
        {
            DebugUIBuilder.Instance.Hide();
            UnityEngine.SceneManagement.SceneManager.LoadScene(idx);
        }
    }
}
