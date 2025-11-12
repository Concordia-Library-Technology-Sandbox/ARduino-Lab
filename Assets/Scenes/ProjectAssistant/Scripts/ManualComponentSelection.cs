using System;
using System.Collections.Generic;
using Meta.XR.Samples;
using PassthroughCameraSamples.StartScene;
using UnityEngine;

namespace PassthroughCameraSamples.SelectProject
{
    public class ManualComponentSelection : MonoBehaviour
    {
        private List<PassthroughCameraSamples.StartScene.Component> components;
        private int currentPage = 0;
        private const int pageSize = 3;
        private DebugUIBuilder uiBuilder;

        private void Start()
        {
            uiBuilder = DebugUIBuilder.Instance;

            uiBuilder.LoadComponentImage(uiBuilder, "icons/back-btn.png", DebugUIBuilder.DEBUG_PANE_LEFT, () =>
            {
                LoadScene(5);
            });

            _ = uiBuilder.AddLabel("Add Components Manually", DebugUIBuilder.DEBUG_PANE_LEFT, 50);
            _ = uiBuilder.AddLabel("Select components to add to your project:", DebugUIBuilder.DEBUG_PANE_LEFT, 30);

            if (StaticClass.Components?.components != null)
            {
                components = StaticClass.Components.components;
                ShowPage();
            }

            uiBuilder.Show();
        }

        private void ShowPage()
        {
            // Clear UI and redraw same layout
            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_LEFT);

            uiBuilder.LoadComponentImage(uiBuilder, "icons/back-btn.png", DebugUIBuilder.DEBUG_PANE_LEFT, () =>
            {
                LoadScene(5);
            });

            _ = uiBuilder.AddLabel($"Page {currentPage + 1} / {Mathf.CeilToInt((float)components.Count / pageSize)}",
                                   DebugUIBuilder.DEBUG_PANE_LEFT, 40);

            int startIndex = currentPage * pageSize;
            int endIndex = Mathf.Min(startIndex + pageSize, components.Count);

            // Show components (3 per page)
            for (int i = startIndex; i < endIndex; i++)
            {
                int idx = i;
                var component = components[idx];
                string displayText = $"{FormatComponentName(component.item)} (x{component.quantity})";

                // Label + image
                _ = uiBuilder.AddLabel(displayText, DebugUIBuilder.DEBUG_PANE_LEFT, 30);
                uiBuilder.LoadComponentImage(uiBuilder, "2dmod/" + component.item + ".jpg", DebugUIBuilder.DEBUG_PANE_LEFT, () => { });

                // Add + and - buttons to modify the static list
                _ = uiBuilder.AddButton($"(+) Add 1", () =>
                {
                    component.quantity++;
                    StaticClass.Components.components[idx].quantity = components[idx].quantity;
                            ShowPage(); // refresh
                }, -1, DebugUIBuilder.DEBUG_PANE_LEFT);

                _ = uiBuilder.AddButton($"(-) Remove 1", () =>
                {
                    component.quantity = Mathf.Max(0, component.quantity - 1);
                    StaticClass.Components.components[idx].quantity = components[idx].quantity;
                    ShowPage(); // refresh
                }, -1, DebugUIBuilder.DEBUG_PANE_LEFT);

                _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_LEFT);
            }

            // Pagination
            if (currentPage > 0)
            {
                _ = uiBuilder.AddButton("← Previous Page", () =>
                {
                    currentPage--;
                    ShowPage();
                }, -1, DebugUIBuilder.DEBUG_PANE_LEFT);
            }

            if ((currentPage + 1) * pageSize < components.Count)
            {
                _ = uiBuilder.AddButton("Next Page →", () =>
                {
                    currentPage++;
                    ShowPage();
                }, -1, DebugUIBuilder.DEBUG_PANE_LEFT);
            }
        }

        private string FormatComponentName(string componentName)
        {
            return char.ToUpper(componentName[0]) + componentName.Substring(1).Replace("_", " ").Replace("(", " (").Trim();
        }

        private void LoadScene(int idx)
        {
            DebugUIBuilder.Instance.Hide();
            UnityEngine.SceneManagement.SceneManager.LoadScene(idx);
        }
    }
}
