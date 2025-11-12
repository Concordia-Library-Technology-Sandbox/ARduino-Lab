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
    public class AboutMenu : MonoBehaviour
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

                    uiBuilder.LoadComponentImage(uiBuilder, "icons/back-btn.png", DebugUIBuilder.DEBUG_PANE_LEFT, () =>
                    {
                                    LoadScene(0);
                                });

                _ = uiBuilder.AddLabel("About ARduino Lab", DebugUIBuilder.DEBUG_PANE_LEFT, 40);

                _ = uiBuilder.AddAppLogo(DebugUIBuilder.DEBUG_PANE_LEFT);

                _ = uiBuilder.AddParagraph("ARduino Lab is a personal project and proof of concept built to explore the Meta Passthrough API, which enables developers to access the Meta Quest cameras. The goal of the project is to make it easier for beginners to learn about Arduino components and simple electronics projects through an engaging, hands-on experience. Arduino Lab received funding through the EL Grant and has been supported and developed at the Concordia University Technology Sandbox.", DebugUIBuilder.DEBUG_PANE_LEFT, 17);

                _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_LEFT);

                _ = uiBuilder.AddLabel("About The Developer", DebugUIBuilder.DEBUG_PANE_LEFT, 40);

                uiBuilder.LoadImage("abouts/gabriel_armas.png", DebugUIBuilder.DEBUG_PANE_LEFT, 200);

                _ = uiBuilder.AddParagraph("Hello! My name is Gabriel, and I am a final-year CS undergraduate student at Concordia University in Montreal, Canada. My interests are in computer vision, augmented reality, and robotics, where I have gained hands-on experience through internships and research projects. I am passionate about developing innovative solutions, and aspire to pursue academic research in the future.", DebugUIBuilder.DEBUG_PANE_LEFT, 17);

                _ = uiBuilder.AddParagraph("github.com/gabxap\nlinkedin.com/in/gabriel-armas", DebugUIBuilder.DEBUG_PANE_LEFT, 20);

                // GALLERY 


                uiBuilder.LoadImage("abouts/1.jpg", DebugUIBuilder.DEBUG_PANE_RIGHT,1000);
                _ = uiBuilder.AddLabel("Concordia University Technology Sandbox.", DebugUIBuilder.DEBUG_PANE_RIGHT, 20);


                uiBuilder.Show();
            }
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


