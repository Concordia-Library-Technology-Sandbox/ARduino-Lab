// Author: Gabriel Armas

using System;
using System.Collections.Generic;
using Meta.XR.Samples;
using PassthroughCameraSamples.StartScene;
using UnityEngine;
using OVRSimpleJSON;

namespace PassthroughCameraSamples.SelectProject
{
    /// <summary>
    /// Handles scanning Arduino components using the Quest camera + OpenAI Vision.
    /// LEFT panel shows:
    /// - camera scan options
    /// - detected components (paginated)
    /// - option to add results to the inventory.
    /// </summary>
    public class CameraScreen : MonoBehaviour
    {
        [Header("Component References")]
        [SerializeField] private WebCamTextureManager webCamTextureManager;
        [SerializeField] private ArduinoImageOpenAIConnector openAIConnector;

        [SerializeField] private RollingAnimationLoader rollingLoader;


        private DebugUIBuilder uiBuilder;

        // Pagination state
        private readonly List<JSONNode> detectedComponents = new List<JSONNode>();
        private int currentPage = 0;
        private const int pageSize = 3;


        // ----------------------------------------------------------------------
        // LIFECYCLE
        // ----------------------------------------------------------------------

        private void Start()
        {
            uiBuilder = DebugUIBuilder.Instance;
            BuildBaseUI();
        }


        // ----------------------------------------------------------------------
        // UI BUILDING — BASE LAYOUT
        // ----------------------------------------------------------------------

        /// <summary>
        /// Creates the top-level UI elements visible before any scanning occurs.
        /// </summary>
        private void BuildBaseUI()
        {
            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_LEFT);

            // Back button
            uiBuilder.LoadComponentImage(
                uiBuilder,
                "icons/back-btn.png",
                DebugUIBuilder.DEBUG_PANE_LEFT,
                () => LoadScene(5)
            );

            // Camera icon
            uiBuilder.LoadImage("icons/camera.png",
                DebugUIBuilder.DEBUG_PANE_LEFT,
                110);

            // Title
            _ = uiBuilder.AddLabel("Scan Using Camera",
                DebugUIBuilder.DEBUG_PANE_LEFT,
                50);

            // Helper text
            _ = uiBuilder.AddParagraph(
                "Use your camera to scan Arduino components and detect what's on your workspace.",
                DebugUIBuilder.DEBUG_PANE_LEFT,
                22
            );

            // Scan button
            _ = uiBuilder.AddButton("Scan Components",
                () => CaptureImageAndAnalyze(),
                -1,
                DebugUIBuilder.DEBUG_PANE_LEFT
            );

            uiBuilder.Show();
        }


        // ----------------------------------------------------------------------
        // CAMERA CAPTURE + OPENAI REQUEST
        // ----------------------------------------------------------------------

        /// <summary>
        /// Captures a frame from the webcam texture and sends it to OpenAI Vision.
        /// </summary>
        private void CaptureImageAndAnalyze()
        {
            Debug.Log("Capturing frame from WebCamTextureManager...");

            WebCamTexture tex = webCamTextureManager.WebCamTexture;

            if (tex == null || tex.width <= 16 || tex.height <= 16)
            {
                Debug.LogError("Webcam texture is invalid or not initialized.");
                return;
            }

            // Copy pixel data
            Texture2D frame = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
            frame.SetPixels32(tex.GetPixels32());
            frame.Apply();

            Debug.Log("Frame captured. Sending to OpenAI Vision...");

            // Rebuild UI with loading message
            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_LEFT);
            BuildBaseUI();
            
            rollingLoader.LoadRollingAnimation(DebugUIBuilder.DEBUG_PANE_LEFT);

            _ = uiBuilder.AddLabel("Analyzing...", DebugUIBuilder.DEBUG_PANE_LEFT, 45);

            // Attach listener (avoid duplicates)
            openAIConnector.onJsonReceived.RemoveListener(OnComponentsJsonReceived);
            openAIConnector.onJsonReceived.AddListener(OnComponentsJsonReceived);

            openAIConnector.AnalyzeArduinoComponents(frame);
        }


        // ----------------------------------------------------------------------
        // OPENAI JSON RECEIVING + PARSING
        // ----------------------------------------------------------------------

        /// <summary>
        /// Handles JSON returned by OpenAI and extracts component detection results.
        /// </summary>
        private void OnComponentsJsonReceived(string json)
        {
            Debug.Log("OpenAI Vision JSON received.");

            try
            {
                JSONNode root = JSON.Parse(json);
                string rawContent = root["choices"][0]["message"]["content"];

                // Remove markdown fences if present
                string cleaned = rawContent
                    .Replace("```json", "")
                    .Replace("```", "")
                    .Trim();

                JSONNode result = JSON.Parse(cleaned);
                JSONArray arr = result["components"].AsArray;

                detectedComponents.Clear();
                foreach (var c in arr)
                    detectedComponents.Add(c.Value);

                currentPage = 0;
                ShowDetectedComponentsPage();
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to parse component JSON: " + ex.Message);
            }
        }


        // ----------------------------------------------------------------------
        // PAGINATED UI — DETECTED COMPONENTS
        // ----------------------------------------------------------------------

        /// <summary>
        /// Displays the detected components with pagination in the LEFT panel.
        /// </summary>
        private void ShowDetectedComponentsPage()
        {
            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_LEFT);
            BuildBaseUI(); // rebuild persistent UI elements

            if (detectedComponents.Count == 0)
            {
                _ = uiBuilder.AddLabel("No components detected.",
                    DebugUIBuilder.DEBUG_PANE_LEFT,
                    30);
                return;
            }

            // Add-to-inventory button
            _ = uiBuilder.AddButton("Add to Inventory", () =>
            {
                Debug.Log("Adding detected components to inventory...");

                foreach (var comp in detectedComponents)
                {
                    string item = comp["item"];
                    int qty = comp["quantity"];

                    StaticClass.AddComponentQuantity(item, qty);
                }

                LoadScene(5);

            }, -1, DebugUIBuilder.DEBUG_PANE_LEFT);

            _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_LEFT);

            // Pagination details
            int total = detectedComponents.Count;
            int totalPages = Mathf.CeilToInt(total / (float)pageSize);
            bool paginated = total > pageSize;

            if (paginated)
            {
                _ = uiBuilder.AddLabel(
                    $"Page {currentPage + 1} / {totalPages}",
                    DebugUIBuilder.DEBUG_PANE_LEFT,
                    32
                );
                _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_LEFT);
            }

            // Items for current page
            int start = currentPage * pageSize;
            int end = Mathf.Min(start + pageSize, total);

            for (int i = start; i < end; i++)
            {
                JSONNode comp = detectedComponents[i];

                string item = comp["item"];
                int qty = comp["quantity"];

                string prettyName = FormatComponentName(item);
                string label = $"{prettyName} (x{qty})";

                // Thumbnail
                uiBuilder.LoadComponentImage(
                    uiBuilder,
                    $"2dmod/{item}.jpg",
                    DebugUIBuilder.DEBUG_PANE_LEFT,
                    () => { } // no click action
                );

                _ = uiBuilder.AddLabel(label,
                    DebugUIBuilder.DEBUG_PANE_LEFT,
                    23);

                _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_LEFT);
            }

            // Pagination Controls
            if (paginated)
            {
                if (currentPage > 0)
                    _ = uiBuilder.AddButton("← Previous", () =>
                    {
                        currentPage--;
                        ShowDetectedComponentsPage();
                    }, -1, DebugUIBuilder.DEBUG_PANE_LEFT);

                if ((currentPage + 1) * pageSize < total)
                    _ = uiBuilder.AddButton("Next →", () =>
                    {
                        currentPage++;
                        ShowDetectedComponentsPage();
                    }, -1, DebugUIBuilder.DEBUG_PANE_LEFT);
            }

            uiBuilder.Show();
        }


        // ----------------------------------------------------------------------
        // UTILITIES
        // ----------------------------------------------------------------------

        /// <summary>
        /// Converts item keys such as "photo_resistor" into "Photo resistor".
        /// </summary>
        private string FormatComponentName(string componentName)
        {
            if (string.IsNullOrEmpty(componentName))
                return componentName;

            return char.ToUpper(componentName[0]) +
                   componentName.Substring(1)
                        .Replace("_", " ")
                        .Trim();
        }

        /// <summary>
        /// Loads a Unity scene by build index, hiding DebugUI first.
        /// </summary>
        private void LoadScene(int idx)
        {
            DebugUIBuilder.Instance.Hide();
            UnityEngine.SceneManagement.SceneManager.LoadScene(idx);
        }
    }
}
