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
        private DebugUIBuilder uiBuilder;
        private List<Component> nonZeroComponents = new List<Component>();
        private int currentPage = 0;
        private const int pageSize = 5;

        private void Start()
        {
            // Collect scenes (kept from your original code in case you use it elsewhere)
            var passthroughScenes = new List<Tuple<int, string>>();
            var n = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
            for (var sceneIndex = 1; sceneIndex < n; ++sceneIndex)
            {
                var path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(sceneIndex);
                passthroughScenes.Add(new Tuple<int, string>(sceneIndex, path));
            }

            uiBuilder = DebugUIBuilder.Instance;

            // Handle inventory reset
            bool restartInventory = StaticClass.RestartInventory;
            if (restartInventory)
            {
                StaticClass.Components = CreateEmptyComponentList();
                StaticClass.RestartInventory = false;
            }

            // Build RIGHT pane once (not affected by left-pane pagination redraws)
            _ = uiBuilder.AddButton("Generate Projects", () => LoadScene(6), -1, DebugUIBuilder.DEBUG_PANE_RIGHT);

            // Precompute list to paginate
            if (StaticClass.Components != null && StaticClass.Components.components != null)
            {
                nonZeroComponents = StaticClass.Components.components.FindAll(c => c.quantity > 0);
            }
            else
            {
                nonZeroComponents = new List<Component>();
            }

            // First draw
            ShowPage();

            uiBuilder.Show();
        }

        private void ShowPage()
        {
            // Clear only the LEFT pane; keep the RIGHT pane intact
            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_LEFT);

            // Back button + header (always present)
            uiBuilder.LoadComponentImage(uiBuilder, "icons/back-btn.png", DebugUIBuilder.DEBUG_PANE_LEFT, () =>
            {
                LoadScene(4);
            });

            _ = uiBuilder.AddLabel("My Components", DebugUIBuilder.DEBUG_PANE_LEFT, 50);

            // Top actions (always visible)
            _ = uiBuilder.AddButton("Scan Using Camera", () => LoadScene(6), -1, DebugUIBuilder.DEBUG_PANE_LEFT);
            _ = uiBuilder.AddButton("Add Manually", () => LoadScene(6), -1, DebugUIBuilder.DEBUG_PANE_LEFT);

            if (nonZeroComponents == null || nonZeroComponents.Count == 0)
            {
                _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_LEFT);
                _ = uiBuilder.AddLabel("No components added.", DebugUIBuilder.DEBUG_PANE_LEFT, 28);
                return;
            }

            // If more than a page, show page indicator; else hide it entirely
            int total = nonZeroComponents.Count;
            int totalPages = Mathf.CeilToInt((float)total / pageSize);
            bool showPagination = total > pageSize;

            if (showPagination)
            {
                _ = uiBuilder.AddLabel($"Page {currentPage + 1} / {totalPages}", DebugUIBuilder.DEBUG_PANE_LEFT, 40);
            }

            int startIndex = currentPage * pageSize;
            int endIndex = Mathf.Min(startIndex + pageSize, total);

            for (int i = startIndex; i < endIndex; i++)
            {
                var comp = nonZeroComponents[i];


                string display = $"{FormatComponentName(comp.item)} (x{comp.quantity})";
                _ = uiBuilder.AddLabel(display, DebugUIBuilder.DEBUG_PANE_LEFT, 28);

                uiBuilder.LoadComponentImage(uiBuilder, "2dmod/" + comp.item + ".jpg", DebugUIBuilder.DEBUG_PANE_LEFT, () => { });

                _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_LEFT);

            }

            // Pagination buttons only if needed
            if (showPagination)
            {
                if (currentPage > 0)
                {
                    _ = uiBuilder.AddButton("← Previous Page", () =>
                    {
                        currentPage--;
                        ShowPage();
                    }, -1, DebugUIBuilder.DEBUG_PANE_LEFT);
                }

                if ((currentPage + 1) * pageSize < total)
                {
                    _ = uiBuilder.AddButton("Next Page →", () =>
                    {
                        currentPage++;
                        ShowPage();
                    }, -1, DebugUIBuilder.DEBUG_PANE_LEFT);
                }
            }
        }

        private string FormatComponentName(string componentName)
        {
            return char.ToUpper(componentName[0]) + componentName.Substring(1)
                .Replace("_", " ")
                .Replace("(", " (")
                .Trim();
        }

        private ComponentList CreateEmptyComponentList()
        {
            return new ComponentList
            {
                components = new List<Component>
                {
                    new Component { item = "arduino", quantity = 0 },
                    new Component { item = "breadboard", quantity = 0 },
                    new Component { item = "DC_Motor", quantity = 0 },
                    new Component { item = "diode", quantity = 0 },
                    new Component { item = "flex_sensor", quantity = 0 },
                    new Component { item = "led", quantity = 0 },
                    new Component { item = "lcd_screen", quantity = 0 },
                    new Component { item = "photo_resistor", quantity = 0 },
                    new Component { item = "potentiometer", quantity = 0 },
                    new Component { item = "push_button", quantity = 0 },
                    new Component { item = "relay", quantity = 0 },
                    new Component { item = "servo_motor", quantity = 0 },
                    new Component { item = "soft_potentiometer", quantity = 0 },
                    new Component { item = "temp_sensor", quantity = 0 },
                    new Component { item = "transistor", quantity = 0 },
                    new Component { item = "integrated_circuit", quantity = 0 },
                    new Component { item = "piezo_buzzer", quantity = 0 }
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
