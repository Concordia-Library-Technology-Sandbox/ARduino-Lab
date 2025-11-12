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

                  uiBuilder.LoadComponentImage(uiBuilder, "icons/back-btn.png", DebugUIBuilder.DEBUG_PANE_LEFT, () =>
                                {
                                    LoadScene(1);



                                });
                _ = uiBuilder.AddLabel("Project Ideas", DebugUIBuilder.DEBUG_PANE_LEFT, 50);


                // Load projects from JSON file
                TextAsset jsonFile = Resources.Load<TextAsset>("projects");
                if (jsonFile != null)
                {
                    var projects = JsonUtility.FromJson<ProjectList>(jsonFile.text);
                    int index = 0;

                    foreach (var project in projects.projects)
                    {
                        int selectedIndex = index;
                        _ = uiBuilder.AddButton($"{index + 1}. {project.name}", () =>
                        {

                            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_CENTER);
                            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_RIGHT);

                            _ = uiBuilder.AddLabel($"{selectedIndex + 1}. {project.name}", DebugUIBuilder.DEBUG_PANE_CENTER, 40);
                            uiBuilder.LoadImage($"projects-imgs/{selectedIndex + 1}.png", DebugUIBuilder.DEBUG_PANE_CENTER, 200);
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
                                uiBuilder.LoadComponentImage(uiBuilder, "2dmod/" +component.item + ".jpg", DebugUIBuilder.DEBUG_PANE_RIGHT, () =>
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

                           //_ = uiBuilder.AddButton("Select", () => { LoadProjectScene(selectedIndex, componentsArray); }, -1, DebugUIBuilder.DEBUG_PANE_CENTER);
                            uiBuilder.Show();

                        }, -1, DebugUIBuilder.DEBUG_PANE_LEFT);
                        index++;
                    }


                }

            }
            uiBuilder.Show();
        }

   

   private void LoadScene(int idx)
        {
            if (idx == 1)
            {
            }
            DebugUIBuilder.Instance.Hide();
            Debug.Log("Load scene: " + idx);
            UnityEngine.SceneManagement.SceneManager.LoadScene(idx);
        }
    }
    
    }


