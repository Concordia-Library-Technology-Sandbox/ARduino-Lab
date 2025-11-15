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
    public class ProjectSuggestionsScreen : MonoBehaviour
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
                openAIConnector.generateProjects(StaticClass.generateCompoundStringOfComponents());

                uiBuilder.LoadComponentImage(uiBuilder, "icons/back-btn.png", DebugUIBuilder.DEBUG_PANE_CENTER, () =>
                                {
                                    LoadScene(1);
                                });

                _ = uiBuilder.AddLabel("Project Suggestions", DebugUIBuilder.DEBUG_PANE_CENTER, 50);

                _ = uiBuilder.AddParagraph(
                    "Based on your current components, here are some project suggestions:\n\n" +
                    "1. Blink an LED: Use an LED, resistor, and Arduino to create a simple blinking light.\n" +
                    "2. Temperature Monitor: Combine a temperature sensor with the Arduino to display real-time temperature readings.\n" +
                    "3. Servo Motor Control: Use a servo motor and potentiometer to control the position of the motor shaft.\n\n" +
                    "Select a project to see AR instructions on how to build it!",
                    DebugUIBuilder.DEBUG_PANE_CENTER, 23);                
                
                uiBuilder.Show();
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


