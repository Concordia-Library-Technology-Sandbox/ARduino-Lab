// Author: Gabriel Armas

using System;
using System.Collections.Generic;
using Meta.XR.Samples;
using PassthroughCameraSamples.StartScene;
using UnityEngine;

namespace PassthroughCameraSamples.SelectProject
{
    /// <summary>
    /// Allows the user to manually add or remove Arduino components 
    /// from their inventory using paginated UI buttons (LEFT panel only).
    /// </summary>
    public class ManualComponentSelection : MonoBehaviour
    {
        // Must fully qualify because Unity also has a class named Component
        private List<PassthroughCameraSamples.StartScene.Component> components;

        private int currentPage = 0;
        private const int pageSize = 2;

        private DebugUIBuilder uiBuilder;


        // ----------------------------------------------------------------------
        // LIFECYCLE
        // ----------------------------------------------------------------------

        private void Start()
        {
            uiBuilder = DebugUIBuilder.Instance;

            // Always draw back button
            uiBuilder.LoadComponentImage(
                uiBuilder,
                "icons/back-btn.png",
                DebugUIBuilder.DEBUG_PANE_LEFT,
                () => LoadScene(5)
            );

            // Load component list into local reference
            if (StaticClass.Components?.components != null)
            {
                components = StaticClass.Components.components;
                ShowPage();
            }

            uiBuilder.Show();
        }


        // ----------------------------------------------------------------------
        // PAGE RENDERING
        // ----------------------------------------------------------------------

        private void ShowPage()
        {
            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_LEFT);

            // Rebuild static UI header
            uiBuilder.LoadComponentImage(
                uiBuilder,
                "icons/back-btn.png",
                DebugUIBuilder.DEBUG_PANE_LEFT,
                () => LoadScene(5)
            );

            _ = uiBuilder.AddLabel("Add Manually", DebugUIBuilder.DEBUG_PANE_LEFT, 50);
            _ = uiBuilder.AddParagraph(
                "Add components manually to your project.",
                DebugUIBuilder.DEBUG_PANE_LEFT,
                22
            );

            _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_LEFT);

            // Page indicator
            int totalPages = Mathf.CeilToInt((float)components.Count / pageSize);
            _ = uiBuilder.AddLabel(
                $"Page {currentPage + 1} / {totalPages}",
                DebugUIBuilder.DEBUG_PANE_LEFT,
                32
            );

            _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_LEFT);


            // ------------------------------------------------------------------
            // COMPONENT LIST (Paginated)
            // ------------------------------------------------------------------

            int startIndex = currentPage * pageSize;
            int endIndex = Mathf.Min(startIndex + pageSize, components.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                int index = i;

                // MUST fully qualify the component type to avoid ambiguity
                PassthroughCameraSamples.StartScene.Component component = components[index];

                string displayName =
                    $"{FormatComponentName(component.item)} (x{component.quantity})";

                // Thumbnail
                uiBuilder.LoadComponentImage(
                    uiBuilder,
                    "2dmod/" + component.item + ".jpg",
                    DebugUIBuilder.DEBUG_PANE_LEFT,
                    () => { }
                );

                // Label
                _ = uiBuilder.AddLabel(displayName, DebugUIBuilder.DEBUG_PANE_LEFT, 23);


                // -------------------------------
                // QUANTITY MODIFY BUTTONS
                // -------------------------------

                _ = uiBuilder.AddButton("(+) Add 1", () =>
                {
                    component.quantity++;
                    StaticClass.Components.components[index].quantity = component.quantity;
                    ShowPage(); // refresh page
                }, -1, DebugUIBuilder.DEBUG_PANE_LEFT);

                _ = uiBuilder.AddButton("(-) Remove 1", () =>
                {
                    component.quantity = Mathf.Max(0, component.quantity - 1);
                    StaticClass.Components.components[index].quantity = component.quantity;
                    ShowPage();
                }, -1, DebugUIBuilder.DEBUG_PANE_LEFT);

                _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_LEFT);
            }


            // ------------------------------------------------------------------
            // PAGINATION BUTTONS
            // ------------------------------------------------------------------

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


        // ----------------------------------------------------------------------
        // UTILITIES
        // ----------------------------------------------------------------------

        private string FormatComponentName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            return char.ToUpper(name[0]) +
                   name.Substring(1)
                       .Replace("_", " ")
                       .Replace("(", " (")
                       .Trim();
        }

        private void LoadScene(int idx)
        {
            DebugUIBuilder.Instance.Hide();
            UnityEngine.SceneManagement.SceneManager.LoadScene(idx);
        }
    }
}
