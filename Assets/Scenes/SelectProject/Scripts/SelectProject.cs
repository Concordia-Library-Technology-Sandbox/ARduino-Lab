using System;
using System.Collections.Generic;
using System.IO;
using Meta.XR.Samples;
using PassthroughCameraSamples.StartScene;
using UnityEngine;

namespace PassthroughCameraSamples.Select
{
    public class SelectProjectMenu : MonoBehaviour
    {
        private DebugUIBuilder uiBuilder;

        private int currentPage = 0;           // pagination for components
        private const int pageSize = 3;        // components per page

        private ProjectList projects;          // loaded from JSON

        private void Start()
        {
            uiBuilder = DebugUIBuilder.Instance;

            //
            // BACK BUTTON + HEADER
            //
            uiBuilder.LoadComponentImage(
                uiBuilder, "icons/back-btn.png",
                DebugUIBuilder.DEBUG_PANE_LEFT,
                () => { LoadScene(1); }
            );

            _ = uiBuilder.AddLabel("Project Ideas", DebugUIBuilder.DEBUG_PANE_LEFT, 50);

            //
            // Load JSON file
            //
            TextAsset jsonFile = Resources.Load<TextAsset>("projects");
            if (jsonFile != null)
            {
                projects = JsonUtility.FromJson<ProjectList>(jsonFile.text);

                if (projects.projects == null || projects.projects.Count == 0)
                {
                    _ = uiBuilder.AddLabel("No project files found.", DebugUIBuilder.DEBUG_PANE_LEFT, 40);
                }
                else
                {
                    DrawProjectList();
                }
            }

            uiBuilder.Show();
        }

        //
        // LEFT PANE — PROJECT LIST
        //
        private void DrawProjectList()
        {
            for (int i = 0; i < projects.projects.Count; i++)
            {
                int index = i;

                _ = uiBuilder.AddButton($"{index + 1}. {projects.projects[i].name}",
                    () => { ShowProjectDetails(index); },
                    -1,
                    DebugUIBuilder.DEBUG_PANE_LEFT
                );
            }
        }

        //
        // CENTER + RIGHT PANE — PROJECT DETAILS (WITH COMPONENT PAGINATION)
        //
        private void ShowProjectDetails(int selectedIndex)
        {
            currentPage = 0; // reset pagination on new project

            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_CENTER);
            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_RIGHT);

            var project = projects.projects[selectedIndex];

            //
            // CENTER PANE
            //
            _ = uiBuilder.AddLabel($"{selectedIndex + 1}. {project.name}", DebugUIBuilder.DEBUG_PANE_CENTER, 40);

            uiBuilder.LoadImage($"projects-imgs/{selectedIndex + 1}.png",
                DebugUIBuilder.DEBUG_PANE_CENTER, 200);

            _ = uiBuilder.AddParagraph(project.description,
                DebugUIBuilder.DEBUG_PANE_CENTER, 23);

            DrawComponentPage(project, selectedIndex);
        }



        //
        // RIGHT PANE — COMPONENTS PAGINATION
        //
        private void DrawComponentPage(Project project, int selectedIndex)
        {
            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_RIGHT);

            _ = uiBuilder.AddLabel("Components", DebugUIBuilder.DEBUG_PANE_RIGHT, 40);
            _ = uiBuilder.AddLabel("Tap image to see 3D model", DebugUIBuilder.DEBUG_PANE_RIGHT, 25);

            int total = project.components.Count;
            int totalPages = Mathf.CeilToInt((float)total / pageSize);

            // Page Indicator
            if (total > pageSize)
            {
                _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_RIGHT);
                _ = uiBuilder.AddLabel($"Page {currentPage + 1} / {totalPages}",
                    DebugUIBuilder.DEBUG_PANE_RIGHT, 30);
                _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_RIGHT);
            }

            // Current slice
            int startIndex = currentPage * pageSize;
            int endIndex = Mathf.Min(startIndex + pageSize, total);

            for (int i = startIndex; i < endIndex; i++)
            {
                var c = project.components[i];

                // Text
                string txt = c.item == "resistance"
                    ? $"{c.item} {c.value}Ω (x{c.quantity})"
                    : $"{c.item} (x{c.quantity})";

                _ = uiBuilder.AddParagraph(txt,
                    DebugUIBuilder.DEBUG_PANE_RIGHT, 23);

                // Image + 3D loading
                uiBuilder.LoadComponentImage(
                    uiBuilder,
                    "2dmod/" + c.item + ".jpg",
                    DebugUIBuilder.DEBUG_PANE_RIGHT,
                    () =>
                    {
                        GameObject prefab = Resources.Load<GameObject>($"3DI/{c.item}");
                        if (prefab != null)
                        {
                            Instantiate(prefab, new Vector3(0, 1, 0.35f), Quaternion.identity);
                        }
                        else
                        {
                            _ = uiBuilder.AddLabel("Failed to Load 3D", DebugUIBuilder.DEBUG_PANE_CENTER, 25);
                        }
                    }
                );

                if (i < endIndex - 1)
                    _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_RIGHT);
            }

            
            _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_RIGHT);//
            // PAGINATION BUTTONS
            //
            if (currentPage > 0)
            {
                _ = uiBuilder.AddButton("← Previous", () =>
                {
                    currentPage--;
                    DrawComponentPage(project, selectedIndex);
                }, -1, DebugUIBuilder.DEBUG_PANE_RIGHT);
            }

            if ((currentPage + 1) * pageSize < total)
            {
                _ = uiBuilder.AddButton("Next →", () =>
                {
                    currentPage++;
                    DrawComponentPage(project, selectedIndex);
                }, -1, DebugUIBuilder.DEBUG_PANE_RIGHT);
            }

            uiBuilder.Show();
        }

        //
        // SCENE LOADING WRAPPER
        //
        private void LoadScene(int idx)
        {
            DebugUIBuilder.Instance.Hide();
            UnityEngine.SceneManagement.SceneManager.LoadScene(idx);
        }
    }
}
