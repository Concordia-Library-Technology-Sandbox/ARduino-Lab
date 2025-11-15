// Copyright (c) Meta Platforms, Inc. and affiliates.
// Original Source code from Oculus Starter Samples (https://github.com/oculus-samples/Unity-StarterSamples)

using System;
using System.Collections.Generic;
using System.IO;
using Meta.XR.Samples;
using PassthroughCameraSamples.StartScene; 
using UnityEngine;
using OVRSimpleJSON;

namespace PassthroughCameraSamples.SelectProject

{
    // Create menu of all scenes included in the build.
    public class CameraScreen : MonoBehaviour
    {

        [SerializeField] private WebCamTextureManager webCamTextureManager;
        [SerializeField] private ArduinoImageOpenAIConnector openAIConnector;

        private DebugUIBuilder uiBuilder;


        private void Start()
        {
            var generalScenes = new List<Tuple<int, string>>();
            var passthroughScenes = new List<Tuple<int, string>>();
            var proControllerScenes = new List<Tuple<int, string>>();

            var n = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
            for (var sceneIndex = 1; sceneIndex < n; ++sceneIndex)
            {
                var path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(sceneIndex);
                passthroughScenes.Add(new Tuple<int, string>(sceneIndex, path));
            }

            uiBuilder = DebugUIBuilder.Instance;
            if (passthroughScenes.Count > 0)
            {
                createBaseUi();
            }
        }

        private void createBaseUi()
        {
         uiBuilder.LoadComponentImage(uiBuilder, "icons/back-btn.png", DebugUIBuilder.DEBUG_PANE_LEFT, () =>
                                {
                                    LoadScene(5);
                                });
        _ = uiBuilder.AddLabel("Scan Using Camera", DebugUIBuilder.DEBUG_PANE_LEFT, 50);

        _ = uiBuilder.AddButton("Take Picture", () => takePicture(), -1, DebugUIBuilder.DEBUG_PANE_LEFT);   

        uiBuilder.Show();

        }


        private void takePicture()
        {

            Debug.Log("Taking picture from webcam texture...");
            WebCamTexture webCamTexture = webCamTextureManager.WebCamTexture;

            if (webCamTexture == null || webCamTexture.width <= 16 || webCamTexture.height <= 16)
            {
                return;
            }


            // Capture frame
            Texture2D frame = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGBA32, false);
            frame.SetPixels32(webCamTexture.GetPixels32());
            frame.Apply();

            Debug.Log("Captured camera frame. Sending to OpenAI...");

            // Show status to user in LEFT pane
            if (uiBuilder != null)
            {
                _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_LEFT);
                _ = uiBuilder.AddLabel("Captured camera frame. Sending to OpenAI...", DebugUIBuilder.DEBUG_PANE_LEFT, 28);
            }

            // Send to your OpenAI vision connector
            openAIConnector.AnalyzeArduinoComponents(frame);

            // Listen for JSON
            openAIConnector.onJsonReceived.AddListener(OnComponentsJsonReceived);
        }

        private void OnComponentsJsonReceived(string json)
        {
            Debug.Log("Received JSON from OpenAI: " + json);

            try
            {
                
                JSONNode root = JSON.Parse(json);

                string rawContent = root["choices"][0]["message"]["content"];

                string cleaned = rawContent
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();


                JSONNode compJson = JSON.Parse(cleaned);
                
                var components = compJson["components"];
                
                Debug.Log("Parsed components from JSON. gabe");

                uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_LEFT);

                createBaseUi();
                foreach (var comp in components.AsArray)
                {
                    string item = comp.Value["item"];
                    int quantity = comp.Value["quantity"];

                    Debug.Log(item + " = " + quantity);

                    string display = $"{FormatComponentName(item)} (x{quantity})";
                    _ = uiBuilder.AddLabel(display, DebugUIBuilder.DEBUG_PANE_LEFT, 28);

                    uiBuilder.LoadComponentImage(uiBuilder, "2dmod/" + item + ".jpg", DebugUIBuilder.DEBUG_PANE_LEFT, () => { });

                    _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_LEFT);
                }

                _ = uiBuilder.AddButton("Add to Inventory", () => {}, -1, DebugUIBuilder.DEBUG_PANE_LEFT);

                uiBuilder.Show();



                /*

                _ = uiBuilder.AddLabel($"Components detected: {parsed.components.Count}", DebugUIBuilder.DEBUG_PANE_LEFT, 28);
                if (parsed != null && parsed.components != null)
                {
                    // Merge counts with StaticClass.Components
                    foreach (var comp in parsed.components)
                    {
                        var localComp = StaticClass.Components.components.Find(c => c.item == comp.item);
                        if (localComp != null)
                        {
                            localComp.quantity += comp.quantity;
                        }
                    }

                    // Refresh non-zero list
                    //nonZeroComponents = StaticClass.Components.components.FindAll(c => c.quantity > 0);

                    // Refresh left pane
                
                    //ShowPage();
               
                }
                else
                {
                    Debug.LogError("Parsed JSON was null or invalid");
                }
                */
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to parse component JSON: " + ex.Message);
            }
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
            Debug.Log("Load scene: " + idx);
            UnityEngine.SceneManagement.SceneManager.LoadScene(idx);
        }
    }
    
}


