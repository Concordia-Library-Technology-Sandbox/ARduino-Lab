// Copyright (c) Meta Platforms, Inc. and affiliates.
// Original Source code from Oculus Starter Samples (https://github.com/oculus-samples/Unity-StarterSamples)

using System;
using System.Collections.Generic;
using System.IO;
using Meta.XR.Samples;
using PassthroughCameraSamples.StartScene; // Add this namespace for DebugUIBuilder
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
                LoadAboutImage("abouts/about_img_1.jpg", DebugUIBuilder.DEBUG_PANE_RIGHT,  null);

                _ = uiBuilder.AddLabel("About ARduino Lab", DebugUIBuilder.DEBUG_PANE_CENTER, 40);

                _ = uiBuilder.AddParagraph("ARduino Lab is a personal project and proof of concept built to explore the Meta Passthrough API, which enables developers to access the Meta Quest cameras. The goal of the project is to make it easier for beginners to learn about Arduino components and simple electronics projects through an engaging, hands-on experience. Arduino Lab received funding through the EL Grant and has been supported and developed at the Concordia University Technology Sandbox.", DebugUIBuilder.DEBUG_PANE_CENTER, 20);

                _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_CENTER);

                _ = uiBuilder.AddLabel("About Gabriel", DebugUIBuilder.DEBUG_PANE_CENTER, 40);

                _ = uiBuilder.AddParagraph("Hello! My name is Gabriel, and I am a final-year CS undergraduate student at Concordia University in Montreal, Canada. My interests are in computer vision, augmented reality, and robotics, where I have gained hands-on experience through internships and research projects. I am passionate about developing innovative solutions, and aspire to pursue academic research in the future.", DebugUIBuilder.DEBUG_PANE_CENTER, 20);

                _ = uiBuilder.AddParagraph("github.com/gabxap\nlinkedin.com/in/gabriel-armas", DebugUIBuilder.DEBUG_PANE_CENTER, 20);

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

           public void LoadAboutImage(string imageName, int targetPane, Action onClick)
                {
                    // Load from Resources folder
                    Sprite sprite = DebugUIBuilder.Instance.LoadSpriteFromResources(imageName);
                    if (sprite != null)
                    {
                        DebugUIBuilder.Instance.AddAboutImage(sprite, targetPane, onClick);
                    }
                    else
                    {
                        Debug.LogError($"Failed to load image: {imageName}");
                        // Add a placeholder or error message
                        _ = DebugUIBuilder.Instance.AddLabel("[Image Not Found]", targetPane);
                    }
                }

    }
    
}


