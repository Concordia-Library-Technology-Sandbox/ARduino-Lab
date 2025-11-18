// Author: Gabriel Armas

using System;
using System.Collections.Generic;
using System.IO;
using Meta.XR.Samples;
using PassthroughCameraSamples.StartScene;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace PassthroughCameraSamples.SelectProject
{
    /// <summary>
    /// Displays step-by-step instructions returned from OpenAI.
    /// Handles:
    /// - Loading animation while waiting for response
    /// - Step navigation
    /// - Code snippet expansion/collapsing
    /// - Clean UI separation across panes
    /// </summary>
    public class InstructionOverlaysScreen : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private ArduinoImageOpenAIConnector openAIConnector;
        [SerializeField] private RollingAnimationLoader rollingLoader;

        private DebugUIBuilder uiBuilder;

        // Parsed instructions JSON array
        private JArray instructionsArray;

        // Current step index
        private int instructionIndex = 0;

        // Code preview controls
        private bool codeExpanded = false;
        private const int CodePreviewLength = 250;

        private Texture2D[] textures = null;


        // ----------------------------------------------------------------------
        // LIFECYCLE
        // ----------------------------------------------------------------------

        private void Start()
        {
            uiBuilder = DebugUIBuilder.Instance;

            // Show loading indicator immediately
            rollingLoader.LoadRollingAnimation(DebugUIBuilder.DEBUG_PANE_CENTER);
            _ = uiBuilder.AddLabel("Generating Instructions...", DebugUIBuilder.DEBUG_PANE_CENTER, 33);

            // Subscribe to JSON callback from OpenAI
            openAIConnector.onJsonReceived.AddListener(OnInstructionsJsonReceived);

            // Build prompt and send request
            string title = StaticClass.projectTitle;
            string description = StaticClass.projectDescription;
            string components = StaticClass.generateCompoundStringOfComponents();

            openAIConnector.GenerateProjectInstructions(title, description, components);

            uiBuilder.Show();
        }


        // ----------------------------------------------------------------------
        // JSON PARSING
        // ----------------------------------------------------------------------

        /// <summary>
        /// Handles JSON returned by OpenAI. Extracts the instruction array.
        /// </summary>
        private void OnInstructionsJsonReceived(string json)
        {
            try
            {

                SaveJsonToFile(json);
                JObject root = JObject.Parse(json);
                string content = root["choices"]?[0]?["message"]?["content"]?.ToString();

                if (string.IsNullOrEmpty(content))
                {
                    Debug.LogError("OpenAI content empty or missing.");
                    return;
                }

                content = content.Trim();
                if (content.StartsWith("```json"))
                {
                    content = content.Substring(7); // Remove ```json
                }
                else if (content.StartsWith("```"))
                {
                    content = content.Substring(3); // Remove ```
                }
                
                if (content.EndsWith("```"))
                {
                    content = content.Substring(0, content.Length - 3); // Remove trailing ```
                }
                
                content = content.Trim(); //
                // Content should already be valid JSON due to strict schema rules
                JObject parsed = JObject.Parse(content);
                instructionsArray = (JArray)parsed["instructions"];

                if (instructionsArray == null || instructionsArray.Count == 0)
                {
                    Debug.LogError("Instructions array missing or empty.");
                    return;
                }

                instructionIndex = 0;

                // Initialize textures array
                textures = new Texture2D[instructionsArray.Count];



                DrawStepUI();
            }
            catch (Exception e)
            {
                Debug.LogError("Error parsing instructions JSON: " + e);
            }
        }


        // ----------------------------------------------------------------------
        // UI RENDERING — MAIN STEP UI
        // ----------------------------------------------------------------------

        private Sprite TextureToSprite(Texture2D tex)
        {
            return Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                100f   // pixels per unit
            );
        }

        /// <summary>
        /// Draws the current step: text, optional code snippet, and navigation.
        /// </summary>
        private void DrawStepUI()
        {
            // Reset full UI layout for this refresh
            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_CENTER);
            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_LEFT);
            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_RIGHT);

            JObject step = (JObject)instructionsArray[instructionIndex];

            string text = step["text"]?.ToString() ?? "";
            JToken codeToken = step["code"];
            string code = (codeToken != null && codeToken.Type != JTokenType.Null) 
                ? codeToken["snippet"]?.ToString() 
                : null;
            string imagePrompt = step["image_prompt"]?.ToString() ?? "";

            // -----------------------------------------------------
            // CENTER PANE — Step Title & Description
            // -----------------------------------------------------

            _ = uiBuilder.AddLabel(
                $"Step {instructionIndex + 1} of {instructionsArray.Count}",
                DebugUIBuilder.DEBUG_PANE_CENTER,
                40
            );

            _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_CENTER);

            _ = uiBuilder.AddParagraph(text,
                DebugUIBuilder.DEBUG_PANE_CENTER,
                30);
            

            Texture2D tex = textures[instructionIndex];

            if(tex != null)
            {
                    Sprite stepSprite = TextureToSprite(tex);
                    uiBuilder.AddImage(stepSprite, DebugUIBuilder.DEBUG_PANE_CENTER, 450);
            }
            else
            {
                rollingLoader.LoadRollingAnimation(DebugUIBuilder.DEBUG_PANE_CENTER);
                _ = uiBuilder.AddLabel("Generating Illustration...", DebugUIBuilder.DEBUG_PANE_CENTER, 25);
                GenerateDescriptionImage(imagePrompt, instructionIndex);
            }


            // -----------------------------------------------------
            // RIGHT PANE — Code Snippet (Optional)
            // -----------------------------------------------------

            if (!string.IsNullOrEmpty(code))
            {
                // Expandable code preview
                string preview = code;

                if (!codeExpanded && code.Length > CodePreviewLength)
                    preview = code.Substring(0, CodePreviewLength) + "...";

                // Expand/Collapse button
                string expandLabel = codeExpanded ? "Collapse Code ▲" : "Expand Code ▼";

                _ = uiBuilder.AddButton(expandLabel, () =>
                {
                    codeExpanded = !codeExpanded;
                    DrawStepUI(); // refresh UI
                }, -1, DebugUIBuilder.DEBUG_PANE_CENTER);

                // Clear right pane + inject code display
                uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_RIGHT);

                _ = uiBuilder.AddLabel("Code Snippet", DebugUIBuilder.DEBUG_PANE_RIGHT, 40);
                _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_RIGHT);

                _ = uiBuilder.AddParagraph(preview, DebugUIBuilder.DEBUG_PANE_RIGHT, 26);

                // Button to close code panel
                _ = uiBuilder.AddButton("Close Code", () =>
                {
                    uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_RIGHT);
                }, -1, DebugUIBuilder.DEBUG_PANE_RIGHT);
            }


            // -----------------------------------------------------
            // NAVIGATION BUTTONS
            // -----------------------------------------------------

            _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_CENTER);

            // Previous step
            if (instructionIndex > 0)
            {
                _ = uiBuilder.AddButton("◀ Previous", () =>
                {
                    codeExpanded = false;
                    instructionIndex--;
                    DrawStepUI();
                }, -1, DebugUIBuilder.DEBUG_PANE_CENTER);
            }

            // Next step
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

        private void GenerateDescriptionImage(string prompt, int index)
        {
            StartCoroutine(openAIConnector.SendImageGenerationRequest(prompt, (Texture2D tex) =>
            {
                if (tex == null)
                {
                    Debug.LogError("Image generation failed for step " + index);
                    return;
                }

                // Store texture for this step
                textures[index] = tex;

                Debug.Log("Image generated for step " + index + " — size: " + tex.width + "x" + tex.height);

                // refresh UI if this is the currently visible step
                if (instructionIndex == index)
                    DrawStepUI();
            }));
        }



        // ----------------------------------------------------------------------
        // SCENE LOADING
        // ----------------------------------------------------------------------

        /// <summary>
        /// Helper for scene navigation.
        /// </summary>
        private void LoadScene(int idx)
        {
            DebugUIBuilder.Instance.Hide();
            UnityEngine.SceneManagement.SceneManager.LoadScene(idx);
        }

         // ----------------------------------------------------------------------
        // DEBUG UTILITIES
        // ----------------------------------------------------------------------

        /// <summary>
        /// Prints long strings into multiple smaller blocks so Unity Console won’t truncate them.
        /// </summary>
        private void DebugLong(string tag, string message)
        {
            int size = 800;
            for (int i = 0; i < message.Length; i += size)
            {
                string chunk = message.Substring(i, Mathf.Min(size, message.Length - i));
                Debug.Log($"{tag} ({i}): {chunk}");
            }
        }

        /// <summary>
        /// Saves full JSON to a file for debugging.
        /// </summary>
        private void SaveJsonToFile(string json)
        {
            try
            {
                string path = Path.Combine(Application.persistentDataPath, "instructions_debug.json");
                File.WriteAllText(path, json);
                Debug.Log("✔ Instructions JSON saved: " + path);
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to save JSON: " + ex.Message);
            }
        }
    }
}
