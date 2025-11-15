// Copyright (c) Meta Platforms
// Modified for ARduino Lab

using System;
using System.Collections.Generic;
using Meta.XR.Samples;
using PassthroughCameraSamples.StartScene;
using UnityEngine;
using OVRSimpleJSON;

namespace PassthroughCameraSamples.SelectProject
{
    public class CameraScreen : MonoBehaviour
    {
        [SerializeField] private WebCamTextureManager webCamTextureManager;
        [SerializeField] private ArduinoImageOpenAIConnector openAIConnector;

        private DebugUIBuilder uiBuilder;

        // Pagination
        private List<JSONNode> detectedComponents = new List<JSONNode>();
        private int currentPage = 0;
        private const int pageSize = 3;

        private void Start()
        {
            uiBuilder = DebugUIBuilder.Instance;
            createBaseUi();
        }

        private void createBaseUi()
        {
            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_LEFT);

            // Back Navigation
            uiBuilder.LoadComponentImage(uiBuilder, "icons/back-btn.png",
                DebugUIBuilder.DEBUG_PANE_LEFT, () => LoadScene(5));

            uiBuilder.LoadImage("icons/camera.png", DebugUIBuilder.DEBUG_PANE_LEFT, 110);

            // Title
            _ = uiBuilder.AddLabel("Scan Using Camera", DebugUIBuilder.DEBUG_PANE_LEFT, 50);

            _ = uiBuilder.AddParagraph(
            "Use your camera to scan Arduino components and detect what's on your workspace.",
                DebugUIBuilder.DEBUG_PANE_LEFT, 22);

            // Main Action
            _ = uiBuilder.AddButton("Scan Components", () => takePicture(),
                -1, DebugUIBuilder.DEBUG_PANE_LEFT);

            uiBuilder.Show();
        }

        private void takePicture()
        {
            Debug.Log("Taking picture from webcam texture...");
            WebCamTexture tex = webCamTextureManager.WebCamTexture;

            if (tex == null || tex.width <= 16 || tex.height <= 16)
                return;

            Texture2D frame = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
            frame.SetPixels32(tex.GetPixels32());
            frame.Apply();

            Debug.Log("Picture captured. Sending to OpenAI...");

            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_LEFT);
            createBaseUi();
            _ = uiBuilder.AddLabel("Analyzing...", DebugUIBuilder.DEBUG_PANE_LEFT, 45);

            openAIConnector.AnalyzeArduinoComponents(frame);
            openAIConnector.onJsonReceived.AddListener(OnComponentsJsonReceived);
        }

        private void OnComponentsJsonReceived(string json)
        {
            Debug.Log("Received JSON from OpenAI.");

            try
            {
                JSONNode root = JSON.Parse(json);
                string rawContent = root["choices"][0]["message"]["content"];

                string cleaned = rawContent
                    .Replace("```json", "")
                    .Replace("```", "")
                    .Trim();

                JSONNode result = JSON.Parse(cleaned);
                JSONArray components = result["components"].AsArray;

                detectedComponents.Clear();
                foreach (var c in components)
                    detectedComponents.Add(c.Value);

                currentPage = 0;
                ShowPage();
            }
            catch (Exception ex)
            {
                Debug.LogError("JSON parsing failed: " + ex.Message);
            }
        }

        private void ShowPage()
        {
            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_LEFT);
            createBaseUi();

            if (detectedComponents.Count == 0)
            {
                _ = uiBuilder.AddLabel("No components detected.", DebugUIBuilder.DEBUG_PANE_LEFT, 30);
                return;
            }

            _ = uiBuilder.AddButton("Add to Inventory", () =>
            {
                Debug.Log("Inventory add confirmed.");

                foreach (var comp in detectedComponents)
                {
                    string item = comp["item"];
                    int qty = comp["quantity"];

                    StaticClass.AddComponentQuantity(item, qty);
                }

                LoadScene(5);
           }, -1, DebugUIBuilder.DEBUG_PANE_LEFT);

            // Section Title
            _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_LEFT);

            // Pagination info
            int total = detectedComponents.Count;
            int totalPages = Mathf.CeilToInt((float)total / pageSize);
            bool paginated = total > pageSize;

            if (paginated)
            {
                _ = uiBuilder.AddLabel($"Page {currentPage + 1} / {totalPages}",
                                       DebugUIBuilder.DEBUG_PANE_LEFT, 32);
                _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_LEFT);
            }

            // Show page items
            int start = currentPage * pageSize;
            int end = Mathf.Min(start + pageSize, total);

            for (int i = start; i < end; i++)
            {
                var comp = detectedComponents[i];

                string item = comp["item"];
                int qty = comp["quantity"];

                string label = $"{FormatComponentName(item)} (x{qty})";

                // Thumbnail
                uiBuilder.LoadComponentImage(uiBuilder, $"2dmod/{item}.jpg",
                                             DebugUIBuilder.DEBUG_PANE_LEFT, () => { });

                _ = uiBuilder.AddLabel(label, DebugUIBuilder.DEBUG_PANE_LEFT, 23);

                _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_LEFT);
            }

            // Pagination Buttons
            if (paginated)
            {
                if (currentPage > 0)
                    _ = uiBuilder.AddButton("← Previous", () =>
                    {
                        currentPage--;
                        ShowPage();
                    }, -1, DebugUIBuilder.DEBUG_PANE_LEFT);

                if ((currentPage + 1) * pageSize < total)
                    _ = uiBuilder.AddButton("Next →", () =>
                    {
                        currentPage++;
                        ShowPage();
                    }, -1, DebugUIBuilder.DEBUG_PANE_LEFT);
            }

            uiBuilder.Show();
        }

        private string FormatComponentName(string componentName)
        {
            return char.ToUpper(componentName[0]) + componentName.Substring(1)
                .Replace("_", " ")
                .Replace("(", " (")
                .Trim();
        }

        private void LoadScene(int idx)
        {
            DebugUIBuilder.Instance.Hide();
            UnityEngine.SceneManagement.SceneManager.LoadScene(idx);
        }
    }
}
