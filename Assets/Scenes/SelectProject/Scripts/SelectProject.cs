using System;
using System.Collections.Generic;
using System.IO;
using Meta.XR.Samples;
using PassthroughCameraSamples.StartScene; // Add this namespace for DebugUIBuilder
using UnityEngine;

namespace PassthroughCameraSamples.Select

{
    // Create menu of all scenes included in the build.
    public class SelectProjectMenu : MonoBehaviour
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


                _ = uiBuilder.AddLabel("Select Project", DebugUIBuilder.DEBUG_PANE_LEFT, 50);


                // Load projects from JSON file
                TextAsset jsonFile = Resources.Load<TextAsset>("projects");
                if (jsonFile != null)
                {
                    var projects = JsonUtility.FromJson<ProjectList>(jsonFile.text);
                    int index = 0;

                    foreach (var project in projects.projects)
                    {
                        int selectedIndex = index;
                        _ = uiBuilder.AddButton($"#{index + 1} {project.name}", () =>
                        {

                            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_CENTER);
                            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_RIGHT);

                            _ = uiBuilder.AddLabel($"#{selectedIndex + 1} {project.name}", DebugUIBuilder.DEBUG_PANE_CENTER, 40);
                            _ = uiBuilder.AddParagraph($"{project.description}", DebugUIBuilder.DEBUG_PANE_CENTER, 25);
                            _ = uiBuilder.AddLabel($"Components", DebugUIBuilder.DEBUG_PANE_RIGHT, 40);
                            _ = uiBuilder.AddLabel($"Press on a component image to view its 3D model", DebugUIBuilder.DEBUG_PANE_RIGHT, 25);

                            for (int i = 0; i < project.components.Count; i++)
                            {
                                var component = project.components[i];

                                var plural = component.quantity > 1 ? "s" : "";

                                if (component.item == "resistance")
                                {
                                    _ = uiBuilder.AddParagraph($"{component.quantity} {component.item}{plural} {component.value}Ω", DebugUIBuilder.DEBUG_PANE_RIGHT, 20);
                                }
                                else
                                {
                                    _ = uiBuilder.AddParagraph($"{component.quantity} {component.item}{plural}", DebugUIBuilder.DEBUG_PANE_RIGHT, 20);
                                }
                                LoadComponentImage(component.item + ".jpg", DebugUIBuilder.DEBUG_PANE_RIGHT, () =>
                                {

                                    GameObject prefab = Resources.Load<GameObject>($"3DI/{component.item}");
                                    if (prefab != null)
                                    {
                                        Instantiate(prefab, new Vector3(0, 1, 0.35f), Quaternion.identity);
                                    }
                                    else
                                    {
                                        _ = uiBuilder.AddLabel("Failed to Load", DebugUIBuilder.DEBUG_PANE_CENTER, 25);
                                    }



                                });

                                if (i < project.components.Count - 1)
                                {
                                    _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_RIGHT);
                                }

                            }

                            var componentsArray = new List<(string name, int quantity, int value)>();

                            for (int i = 0; i < project.components.Count; i++)
                            {
                                componentsArray.Add((project.components[i].item, project.components[i].quantity, project.components[i].value));
                            }

                            _ = uiBuilder.AddButton("Select", () => { LoadProjectScene(selectedIndex, componentsArray); }, -1, DebugUIBuilder.DEBUG_PANE_CENTER);
                            uiBuilder.Show();

                        }, -1, DebugUIBuilder.DEBUG_PANE_LEFT);
                        index++;
                    }


                }

            }
            uiBuilder.Show();
        }

   


        /*
        private void Start()
        {
            var generalScenes = new List<Tuple<int, string>>();
            var passthroughScenes = new List<Tuple<int, string>>();
            var proControllerScenes = new List<Tuple<int, string>>();

            var n = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
            for (var sceneIndex = 1; sceneIndex < n; ++sceneIndex)
            {
                var path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(sceneIndex);

                if (path.Contains("Passthrough"))
                {
                    passthroughScenes.Add(new Tuple<int, string>(sceneIndex, path));
                }

            }

            var uiBuilder = DebugUIBuilder.Instance;
            if (passthroughScenes.Count > 0)
            {


                _ = uiBuilder.AddLabel("Select Project", DebugUIBuilder.DEBUG_PANE_LEFT, 50);


                // Load projects from JSON file
                TextAsset jsonFile = Resources.Load<TextAsset>("projects");
                if (jsonFile != null)
                {
                    var projects = JsonUtility.FromJson<ProjectList>(jsonFile.text);
                    int index = 0;

                    foreach (var project in projects.projects)
                    {
                        int selectedIndex = index;
                        _ = uiBuilder.AddButton($"#{index + 1} {project.name}", () =>
                        {

                            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_CENTER);
                            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_RIGHT);

                            _ = uiBuilder.AddLabel($"#{selectedIndex + 1} {project.name}", DebugUIBuilder.DEBUG_PANE_CENTER, 40);
                            _ = uiBuilder.AddParagraph($"{project.description}", DebugUIBuilder.DEBUG_PANE_CENTER, 25);
                            _ = uiBuilder.AddLabel($"Components", DebugUIBuilder.DEBUG_PANE_RIGHT, 40);
                            _ = uiBuilder.AddLabel($"Press on a component image to view its 3D model", DebugUIBuilder.DEBUG_PANE_RIGHT, 25);

                            for (int i = 0; i < project.components.Count; i++)
                            {
                                var component = project.components[i];

                                var plural = component.quantity > 1 ? "s" : "";

                                if (component.item == "resistance")
                                {
                                    _ = uiBuilder.AddParagraph($"{component.quantity} {component.item}{plural} {component.value}Ω", DebugUIBuilder.DEBUG_PANE_RIGHT, 20);
                                }
                                else
                                {
                                    _ = uiBuilder.AddParagraph($"{component.quantity} {component.item}{plural}", DebugUIBuilder.DEBUG_PANE_RIGHT, 20);
                                }
                                LoadComponentImage(component.item + ".jpg", DebugUIBuilder.DEBUG_PANE_RIGHT, () =>
                                {

                                    GameObject prefab = Resources.Load<GameObject>($"3DI/{component.item}");
                                    if (prefab != null)
                                    {
                                        Instantiate(prefab, new Vector3(0.3f, 1, 0.35f), Quaternion.identity);
                                    }
                                    else
                                    {
                                        _ = uiBuilder.AddLabel("Failed to Load", DebugUIBuilder.DEBUG_PANE_CENTER, 25);
                                    }



                                });

                                if (i < project.components.Count - 1)
                                {
                                    _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_RIGHT);
                                }

                            }

                            var componentsArray = new List<(string name, int quantity, int value)>();

                            for (int i = 0; i < project.components.Count; i++)
                            {
                                componentsArray.Add((project.components[i].item, project.components[i].quantity, project.components[i].value));
                            }

                            _ = uiBuilder.AddButton("Select", () => { LoadProjectScene(selectedIndex, componentsArray); }, -1, DebugUIBuilder.DEBUG_PANE_CENTER);
                            uiBuilder.Show();

                        }, -1, DebugUIBuilder.DEBUG_PANE_LEFT);
                        index++;
                    }


                }

            }


            uiBuilder.Show();
        }

*/
        public void LoadComponentImage(string imageName, int targetPane, Action onClick)
                {
                    // Load from Resources folder
                    Sprite sprite = DebugUIBuilder.Instance.LoadSpriteFromResources(imageName);
                    if (sprite != null)
                    {
                        DebugUIBuilder.Instance.AddComponentImage(sprite, targetPane, onClick);
                    }
                    else
                    {
                        Debug.LogError($"Failed to load image: {imageName}");
                        // Add a placeholder or error message
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

            UnityEngine.SceneManagement.SceneManager.LoadScene(4);
        }
    }
    
    }


