// Copyright (c) Meta Platforms, Inc. and affiliates.
// Original Source code from Oculus Starter Samples (https://github.com/oculus-samples/Unity-StarterSamples)

using System;
using System.Collections.Generic;
using System.IO;
using Meta.XR.Samples;
using UnityEngine;

namespace PassthroughCameraSamples.StartScene
{
    // Create menu of all scenes included in the build.
    public class StartMenu : MonoBehaviour
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

                _ = uiBuilder.AddConcordiaLogo(DebugUIBuilder.DEBUG_PANE_CENTER);

                _ = uiBuilder.AddLabel("ARduino Lab", DebugUIBuilder.DEBUG_PANE_CENTER, 60);

                _ = uiBuilder.AddLabel("Hands-On Arduino tutorials, enhanced with augmented reality & computer vision.", DebugUIBuilder.DEBUG_PANE_CENTER, 20);

                //Buttons
                _ = uiBuilder.AddButton("Suggest project", () => LoadScene(1), -1, DebugUIBuilder.DEBUG_PANE_CENTER);
                _ = uiBuilder.AddButton("Select project", () => LoadScene(2), -1, DebugUIBuilder.DEBUG_PANE_CENTER);
                _ = uiBuilder.AddButton("Settings", () => LoadScene(3), -1, DebugUIBuilder.DEBUG_PANE_CENTER);

                _ = uiBuilder.AddLabel("Developed by Gabriel Armas Aranibar\nConcordia University – Technology Sandbox\n Version 1.0", DebugUIBuilder.DEBUG_PANE_CENTER, 20);
                
            }

            
            //tips
            
            TextAsset jsonFile = Resources.Load<TextAsset>("tips");

            if (jsonFile != null){
                var tipsobj = JsonUtility.FromJson<TipList>(jsonFile.text);
                System.Random random = new System.Random();
                int index = random.Next(0, tipsobj.tips.Count);
                _ = uiBuilder.AddLabel($"Tips: {tipsobj.tips[index].text}", DebugUIBuilder.DEBUG_PANE_RIGHT, 25);
            }

            uiBuilder.Show();
        }

        private void LoadScene(int idx)
        {
            if (idx == 1)
            {
                StaticClass.projectid = -1;
                DynamicProjectStaticClass.components = new List<string>();
            }
            DebugUIBuilder.Instance.Hide();
            Debug.Log("Load scene: " + idx);
            UnityEngine.SceneManagement.SceneManager.LoadScene(idx);
        }
    }
}
