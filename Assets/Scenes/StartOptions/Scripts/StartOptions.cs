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
    public class StartOptions : MonoBehaviour
    {
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
                                    LoadScene(0);
                                });

                
                _ = uiBuilder.AddLabel("Start", DebugUIBuilder.DEBUG_PANE_CENTER, 40);

                //Buttons
                _ = uiBuilder.AddButton("Project Assistant", () => loadDescription(uiBuilder, 1), -1, DebugUIBuilder.DEBUG_PANE_CENTER);
                _ = uiBuilder.AddButton("Project Ideas", () => loadDescription(uiBuilder, 2), -1, DebugUIBuilder.DEBUG_PANE_CENTER);

                uiBuilder.Show();
            }
        }

        private void loadDescription(DebugUIBuilder uiBuilder, int option = 1)
        {
            string title = "";
            string description = "";
            int sceneNumber = -1;
            if (option == 1)
            {
                title = "Project Assistant";
                description = "Enter this mode to scan and recognize your Arduino components directly using your Meta Quest 3 cameras. The system identifies the parts you wish to use, suggests possible projects, and provides step-by-step AR overlays to guide you through each process. We recommend using the SparkFun RedBoard kit for the best experience.";
                sceneNumber = 4;
            }
            else
            {
                title = "Project Ideas";
                description = "Browse a collection of project ideas built around the SparkFun RedBoard kit, as featured in the kit manual. Each project includes a detailed description and a list of required components, allowing you to recreate circuits or expand on them with your own modifications. It’s the perfect place to get inspired and explore what’s possible with the components you already have.";
                sceneNumber = 3;
            }

            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_RIGHT);
            _ = uiBuilder.AddLabel(title, DebugUIBuilder.DEBUG_PANE_RIGHT, 40);
            uiBuilder.LoadImage($"start-options/{option}.png", DebugUIBuilder.DEBUG_PANE_RIGHT, 300);
            _ = uiBuilder.AddParagraph(description, DebugUIBuilder.DEBUG_PANE_RIGHT, 20);
            _ = uiBuilder.AddButton("Select", () => LoadScene(sceneNumber), -1, DebugUIBuilder.DEBUG_PANE_RIGHT);
            uiBuilder.Show();
        }

        private void LoadScene(int idx)
        {
           
            DebugUIBuilder.Instance.Hide();
            Debug.Log("Load scene: " + idx);
            UnityEngine.SceneManagement.SceneManager.LoadScene(idx);
        }
    }
    
}


