// Copyright (c) Meta Platforms, Inc. and affiliates.
// Original Source code from Oculus Starter Samples (https://github.com/oculus-samples/Unity-StarterSamples)

using System;
using System.Collections.Generic;
using System.IO;
using Meta.XR.Samples;
using PassthroughCameraSamples.StartScene; 
using UnityEngine;

namespace PassthroughCameraSamples.StartScene 

{
    // Create menu of all scenes included in the build.
    public class ComponentsScreen : MonoBehaviour
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
                                    LoadScene(4);
                                });


                StaticClass.Components = CreateEmptyComponentList();
                


                _ = uiBuilder.AddLabel("Components", DebugUIBuilder.DEBUG_PANE_LEFT, 50);

                _ = uiBuilder.AddButton("Scan Components", () => LoadScene(6), -1, DebugUIBuilder.DEBUG_PANE_LEFT);


                _ = uiBuilder.AddButton("Add Manually", () => LoadScene(6), -1, DebugUIBuilder.DEBUG_PANE_LEFT);
                _ = uiBuilder.AddButton("Generate Projects", () => LoadScene(6), -1, DebugUIBuilder.DEBUG_PANE_LEFT);
                
                uiBuilder.Show();
            }
        }


        private ComponentList CreateEmptyComponentList()
        {
            return new ComponentList
            {
                components = new List<Component>
                {
                    new Component { item = "led", quantity = 0 },
                    new Component { item = "breadboard", quantity = 0 },
                    new Component { item = "arduino", quantity = 0 },
                    new Component { item = "potentiometer", quantity = 0 },
                    new Component { item = "photo_resistor", quantity = 0 },
                    new Component { item = "temp_sensor", quantity = 0 },
                    new Component { item = "servo_motor", quantity = 0 },
                    new Component { item = "flex_sensor", quantity = 0 },
                    new Component { item = "soft_potentiometer", quantity = 0 },
                    new Component { item = "piezo_buzzer", quantity = 0 },
                    new Component { item = "DC_Motor", quantity = 0 },
                    new Component { item = "Diode (1N4148)", quantity = 0 },
                    new Component { item = "transistor", quantity = 0 },
                    new Component { item = "relay", quantity = 0 },
                    new Component { item = "integrated_circuit", quantity = 0 },
                    new Component { item = "lcd_screen", quantity = 0 },
                    new Component { item = "push_button", quantity = 0 }
                }
            };
        }


        private void LoadScene(int idx)
        {
            if (idx == 1)
            {
                StaticClass.projectid = -1;
            }
            DebugUIBuilder.Instance.Hide();
            Debug.Log("Load scene: " + idx);
            UnityEngine.SceneManagement.SceneManager.LoadScene(idx);
        }
    }
    
}


