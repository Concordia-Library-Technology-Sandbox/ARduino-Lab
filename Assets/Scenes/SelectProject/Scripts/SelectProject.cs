using System;
using System.Collections.Generic;
using System.IO;
using Meta.XR.Samples;
using PassthroughCameraSamples.StartScene;
using UnityEngine;

namespace PassthroughCameraSamples.Select
{
    /// <summary>
    /// Displays a list of Arduino project ideas using DebugUIBuilder.
    /// LEFT  PANE  → Project list
    /// CENTER PANE → Selected project image + description
    /// RIGHT PANE  → Component list with pagination + 3D model loading
    /// </summary>
    public class SelectProjectMenu : MonoBehaviour
    {
        private DebugUIBuilder uiBuilder;

        // Pagination for component list
        private int currentPage = 0;
        private const int pageSize = 3;

        // Loaded from Resources/projects.json
        private ProjectList projects;

        private void Start()
        {
            uiBuilder = DebugUIBuilder.Instance;

            BuildHeader();
            LoadProjects();

            uiBuilder.Show();
        }

        // ----------------------------------------------------------------------
        // UI INITIALIZATION
        // ----------------------------------------------------------------------

        /// <summary>
        /// Builds the header of the LEFT pane: back button + main title.
        /// </summary>
        private void BuildHeader()
        {
            // Back button
            uiBuilder.LoadComponentImage(
                uiBuilder,
                "icons/back-btn.png",
                DebugUIBuilder.DEBUG_PANE_LEFT,
                () => LoadScene(1)
            );

            // Screen title
            _ = uiBuilder.AddLabel("Project Ideas", DebugUIBuilder.DEBUG_PANE_LEFT, 50);
        }

        /// <summary>
        /// Loads the project list JSON file and either draws the list or shows an error.
        /// </summary>
        private void LoadProjects()
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("projects");
            if (jsonFile == null)
            {
                _ = uiBuilder.AddLabel("projects.json not found.", DebugUIBuilder.DEBUG_PANE_LEFT, 40);
                return;
            }

            projects = JsonUtility.FromJson<ProjectList>(jsonFile.text);

            if (projects?.projects == null || projects.projects.Count == 0)
            {
                _ = uiBuilder.AddLabel("No project files found.", DebugUIBuilder.DEBUG_PANE_LEFT, 40);
            }
            else
            {
                DrawProjectList();
            }
        }

        // ----------------------------------------------------------------------
        // LEFT PANE — PROJECT LIST
        // ----------------------------------------------------------------------

        /// <summary>
        /// Displays a button for each project in the LEFT pane.
        /// </summary>
        private void DrawProjectList()
        {
            for (int i = 0; i < projects.projects.Count; i++)
            {
                int index = i; // capture for button closure

                _ = uiBuilder.AddButton(
                    $"{index + 1}. {projects.projects[index].name}",
                    () => ShowProjectDetails(index),
                    -1,
                    DebugUIBuilder.DEBUG_PANE_LEFT
                );
            }
        }

        // ----------------------------------------------------------------------
        // CENTER + RIGHT PANE — PROJECT DETAILS
        // ----------------------------------------------------------------------

        /// <summary>
        /// Called when the user selects a project. Clears center & right panes,
        /// displays project info, and draws the first component page.
        /// </summary>
        private void ShowProjectDetails(int selectedIndex)
        {
            currentPage = 0; // reset pagination

            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_CENTER);
            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_RIGHT);

            var project = projects.projects[selectedIndex];

            //
            // ---- CENTER PANE ----
            //

            // Project title
            _ = uiBuilder.AddLabel(
                $"{selectedIndex + 1}. {project.name}",
                DebugUIBuilder.DEBUG_PANE_CENTER,
                40
            );

            // Project preview image (optional)
            uiBuilder.LoadImage(
                $"projects-imgs/{selectedIndex + 1}.png",
                DebugUIBuilder.DEBUG_PANE_CENTER,
                200
            );

            // Description text
            _ = uiBuilder.AddParagraph(
                project.description,
                DebugUIBuilder.DEBUG_PANE_CENTER,
                23
            );

            // Draw component list (right pane)
            DrawComponentPage(project, selectedIndex);
        }

        // ----------------------------------------------------------------------
        // RIGHT PANE — COMPONENT PAGINATION
        // ----------------------------------------------------------------------

        /// <summary>
        /// Draws a page of components for the selected project, including images,
        /// clickable 3D model previews, and pagination buttons.
        /// </summary>
        private void DrawComponentPage(Project project, int selectedIndex)
        {
            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_RIGHT);

            //
            // HEADERS
            //
            _ = uiBuilder.AddLabel("Components", DebugUIBuilder.DEBUG_PANE_RIGHT, 40);
            _ = uiBuilder.AddLabel("Tap image to see 3D model", DebugUIBuilder.DEBUG_PANE_RIGHT, 25);

            int total = project.components.Count;
            int totalPages = Mathf.CeilToInt(total / (float)pageSize);

            //
            // PAGE INDICATOR
            //
            if (total > pageSize)
            {
                _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_RIGHT);
                _ = uiBuilder.AddLabel(
                    $"Page {currentPage + 1} / {totalPages}",
                    DebugUIBuilder.DEBUG_PANE_RIGHT,
                    30
                );
                _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_RIGHT);
            }

            //
            // COMPONENT SLICE
            //
            int startIndex = currentPage * pageSize;
            int endIndex = Mathf.Min(startIndex + pageSize, total);

            for (int i = startIndex; i < endIndex; i++)
            {
                var component = project.components[i];

                // Build label for component (special formatting for resistors)
                string labelText = component.item == "resistance"
                    ? $"{component.item} {component.value}Ω (x{component.quantity})"
                    : $"{component.item} (x{component.quantity})";

                _ = uiBuilder.AddParagraph(labelText, DebugUIBuilder.DEBUG_PANE_RIGHT, 23);

                //
                // CLICKABLE 2D -> 3D MODEL
                //
                uiBuilder.LoadComponentImage(
                    uiBuilder,
                    $"2dmod/{component.item}.jpg",
                    DebugUIBuilder.DEBUG_PANE_RIGHT,
                    () =>
                    {
                        GameObject prefab = Resources.Load<GameObject>($"3DI/{component.item}");
                        if (prefab != null)
                        {
                            Instantiate(prefab, new Vector3(0, 1, 0.35f), Quaternion.identity);
                        }
                        else
                        {
                            _ = uiBuilder.AddLabel("Failed to load 3D model", DebugUIBuilder.DEBUG_PANE_CENTER, 25);
                        }
                    }
                );

                if (i < endIndex - 1)
                {
                    _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_RIGHT);
                }
            }

            _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_RIGHT);

            //
            // PAGINATION BUTTONS
            //
            if (currentPage > 0)
            {
                _ = uiBuilder.AddButton(
                    "← Previous",
                    () =>
                    {
                        currentPage--;
                        DrawComponentPage(project, selectedIndex);
                    },
                    -1,
                    DebugUIBuilder.DEBUG_PANE_RIGHT
                );
            }

            if ((currentPage + 1) * pageSize < total)
            {
                _ = uiBuilder.AddButton(
                    "Next →",
                    () =>
                    {
                        currentPage++;
                        DrawComponentPage(project, selectedIndex);
                    },
                    -1,
                    DebugUIBuilder.DEBUG_PANE_RIGHT
                );
            }

            uiBuilder.Show();
        }

        // ----------------------------------------------------------------------
        // SCENE LOADING
        // ----------------------------------------------------------------------

        /// <summary>
        /// Hides the debug UI and loads a Unity scene by index.
        /// </summary>
        private void LoadScene(int idx)
        {
            DebugUIBuilder.Instance.Hide();
            UnityEngine.SceneManagement.SceneManager.LoadScene(idx);
        }
    }
}
