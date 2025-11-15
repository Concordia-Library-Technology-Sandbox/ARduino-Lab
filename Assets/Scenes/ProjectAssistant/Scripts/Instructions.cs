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
    public class Instructions : MonoBehaviour
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
                                    LoadScene(1);
                                });
                uiBuilder.LoadImage("icons/instructions.png", DebugUIBuilder.DEBUG_PANE_CENTER, 180);

                _ = uiBuilder.AddLabel("Instructions", DebugUIBuilder.DEBUG_PANE_CENTER, 50);



               _ = uiBuilder.AddParagraph(
                "Welcome to the ARduino Project Assistant!\n\n" +
                "Use your Meta Quest 3 cameras to recognize Arduino components and build projects with AR overlays.\n\n" +
                "Before you start:\n" +
                "• Have your components ready on a flat, well-lit surface.\n" +
                "• The SparkFun RedBoard kit works best.\n\n" +
                "Steps:\n" +
                "1. Click Scan Components.\n" +
                "2. View the detected list.\n" +
                "3. Follow the AR instructions to build your project.",
                DebugUIBuilder.DEBUG_PANE_CENTER, 30);


                _ = uiBuilder.AddButton("Start", () => {
                    StaticClass.RestartInventory = true;
                    LoadScene(5); 
                     }, -1, DebugUIBuilder.DEBUG_PANE_CENTER);
                
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


