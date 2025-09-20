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
    public class DynamicMenu : MonoBehaviour
    {
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

                DynamicProjectStaticClass.OnComponentsChanged += HandleComponentsChanged;



                // Initial UI render
                HandleComponentsChanged(DynamicProjectStaticClass.components);

                

            }

        }


        private void OnDestroy()
        {
            DynamicProjectStaticClass.OnComponentsChanged -= HandleComponentsChanged;
        }

            

       

        public void HandleComponentsChanged(List<string> updatedComponents)
        {
            Debug.Log("ObjectsFound changed!");
            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_LEFT);

            _ = uiBuilder.AddButton("btn", () => {}, -1, DebugUIBuilder.DEBUG_PANE_LEFT);

            
            _ = uiBuilder.AddLabel($"Components List", DebugUIBuilder.DEBUG_PANE_LEFT, 40);

            if(updatedComponents.Count == 0)
            {
                _ = uiBuilder.AddLabel($"No components found, start showing your components!", DebugUIBuilder.DEBUG_PANE_LEFT, 20);
            }


            for (int i = 0; i < updatedComponents.Count; i++)
            {
                var componentName = updatedComponents[i];

                _ = uiBuilder.AddLabel($"{componentName}", DebugUIBuilder.DEBUG_PANE_LEFT, 20);

                LoadComponentImage(componentName + ".jpg", DebugUIBuilder.DEBUG_PANE_LEFT, () =>
                {

                });

                if (i < updatedComponents.Count - 1)
                {
                    _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_LEFT);
                }

            }

            uiBuilder.Show();
        }


        public void LoadComponentImage(string imageName, int targetPane, Action onClick)
        {
            Sprite sprite = DebugUIBuilder.Instance.LoadSpriteFromResources(imageName);
            if (sprite != null)
            {
                DebugUIBuilder.Instance.AddComponentImage(sprite, targetPane, onClick);
            }
            else
            {
                Debug.LogError($"Failed to load image: {imageName}");
                _ = DebugUIBuilder.Instance.AddLabel("[Image Not Found]", targetPane);
            }
        }


        private void LoadProjectScene(int projectid, List<(string name, int quantity, int value)> components)
        {
            //Debug.Log("Load scene: " + idx);
            StaticClass.projectid = projectid;
            StaticClass.components = components.ToArray();
            bool[] initialObjectFound = new bool[components.Count];
            for (int i = 0; i < components.Count; i++)
            {
                initialObjectFound[i] = false;
            }

            StaticClass.ObjectsFound = initialObjectFound; 

            UnityEngine.SceneManagement.SceneManager.LoadScene(1);
        }
    
    
    }
}


