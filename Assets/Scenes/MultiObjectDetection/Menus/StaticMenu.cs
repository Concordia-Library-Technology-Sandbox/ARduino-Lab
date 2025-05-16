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
    public class StaticMenu : MonoBehaviour
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

                StaticClass.OnObjectsFoundChanged -= HandleObjectsFoundChanged;
                StaticClass.OnObjectsFoundChanged += HandleObjectsFoundChanged;

                // Initial UI render
                HandleObjectsFoundChanged(StaticClass.ObjectsFound);

            }

        }


        private void OnDestroy()
        {
            StaticClass.OnObjectsFoundChanged -= HandleObjectsFoundChanged;
        }

        private void HandleObjectsFoundChanged(bool[] newState)
        {
            Debug.Log("ObjectsFound changed!");
            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_LEFT);
            loadComponentMenu(newState);
        }

        public void loadComponentMenu(bool[] currentObjectFoundList)
        {
            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_LEFT);

            _ = uiBuilder.AddLabel($"Components List", DebugUIBuilder.DEBUG_PANE_LEFT, 40);


            for (int i = 0; i < currentObjectFoundList.Length; i++)
            {
                var component = StaticClass.components[i];
                var plural = component.quantity > 1 ? "s" : "";

                if (component.name == "resistance")
                {
                    _ = uiBuilder.AddParagraph($"{component.quantity} {component.name}{plural} {component.value}Ω", DebugUIBuilder.DEBUG_PANE_LEFT, 20);
                }
                else
                {
                    _ = uiBuilder.AddParagraph($"{component.quantity} {component.name}{plural}", DebugUIBuilder.DEBUG_PANE_LEFT, 20);
                }

                LoadComponentImage(component.name + ".jpg", DebugUIBuilder.DEBUG_PANE_LEFT, () =>
                {
                    GameObject prefab = Resources.Load<GameObject>($"3DI/{component.name}");
                    if (prefab != null)
                    {
                        Instantiate(prefab, new Vector3(0.3f, 1, 0.35f), Quaternion.identity);
                    }
                });

                if (currentObjectFoundList[i])
                {
                    _ = uiBuilder.AddLabel($"DETECTED", DebugUIBuilder.DEBUG_PANE_LEFT, 20, Color.green);
                }
                else
                {
                    _ = uiBuilder.AddLabel($"NOT DETECTED", DebugUIBuilder.DEBUG_PANE_LEFT, 20, Color.red);
                }

                if (i < StaticClass.components.Length - 1)
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


