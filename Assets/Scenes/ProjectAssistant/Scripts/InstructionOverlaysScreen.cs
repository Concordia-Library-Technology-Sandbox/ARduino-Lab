// Copyright (c) Meta Platforms, Inc. and affiliates.
// Original Source code from Oculus Starter Samples (https://github.com/oculus-samples/Unity-StarterSamples)

using System;
using System.Collections.Generic;
using System.IO;
using Meta.XR.Samples;
using PassthroughCameraSamples.StartScene; 
using UnityEngine;

namespace PassthroughCameraSamples.SelectProject

{
    // Create menu of all scenes included in the build.
    public class InstructionOverlaysScreen : MonoBehaviour
    {
        [SerializeField] private ArduinoImageOpenAIConnector openAIConnector;

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

            var uiBuilder = DebugUIBuilder.Instance;
            if (passthroughScenes.Count > 0)
            {
                uiBuilder.LoadComponentImage(uiBuilder, "icons/back-btn.png", DebugUIBuilder.DEBUG_PANE_CENTER, () =>
                                {
                                    LoadScene(1);
                                });

                _ = uiBuilder.AddLabel("Instructions", DebugUIBuilder.DEBUG_PANE_CENTER, 50);

                openAIConnector.onJsonReceived.AddListener(OnInstructionsJsonReceived);
                
                // Send request to OpenAI for instructions generation
                string title = StaticClass.projectTitle;
                string description = StaticClass.projectDescription;
                string components = StaticClass.generateCompoundStringOfComponents();
                
                Debug.Log("Requesting instructions for project: " + title + description + components); 
                openAIConnector.GenerateProjectInstructions(title, description, components);

                _ = uiBuilder.AddButton("Start", () => {
                    StaticClass.RestartInventory = true;
                    LoadScene(5); 
                     }, -1, DebugUIBuilder.DEBUG_PANE_CENTER);
                
                uiBuilder.Show();
            }
        }

        private void OnInstructionsJsonReceived(string json)
        {
            Debug.Log("Received instructions JSON: " + json);

            try
            {
                var instructionsData = OVRSimpleJSON.JSON.Parse(json);
                string instructionsText = instructionsData["instructions"];

                var uiBuilder = DebugUIBuilder.Instance;
                _ = uiBuilder.AddParagraph(instructionsText, DebugUIBuilder.DEBUG_PANE_CENTER, 20);
            }
            catch (Exception e)
            {
                Debug.LogError("Error parsing instructions JSON: " + e.Message);
            }
        }


        private void LoadScene(int idx)
        {
            DebugUIBuilder.Instance.Hide();
            Debug.Log("Load scene: " + idx);
            UnityEngine.SceneManagement.SceneManager.LoadScene(idx);
        }
    }
    
}


