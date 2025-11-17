// Author: Gabriel Armas

using System;
using System.Collections.Generic;
using Meta.XR.Samples;
using PassthroughCameraSamples.StartScene;
using UnityEngine;

namespace PassthroughCameraSamples.StartScene
{
    /// <summary>
    /// Displays the user's Arduino inventory.
    /// Supports:
    /// • Pagination for components
    /// • Resetting inventory
    /// • Adding components manually or via camera scan
    /// • Suggesting projects when enough components exist
    /// </summary>
    public class ComponentsScreen : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ArduinoImageOpenAIConnector openAIConnector;
        [SerializeField] private WebCamTextureManager webCamManager;

        private DebugUIBuilder uiBuilder;

        // Inventory model
        private List<Component> nonZeroComponents = new List<Component>();

        // Pagination
        private int currentPage = 0;
        private const int pageSize = 3;


        // ----------------------------------------------------------------------
        // LIFECYCLE
        // ----------------------------------------------------------------------

        private void Start()
        {
            uiBuilder = DebugUIBuilder.Instance;

            HandleInventoryReset();
            BuildNonZeroComponentsList();

            // First UI draw
            DrawPage();

            uiBuilder.Show();
        }


        // ----------------------------------------------------------------------
        // INITIALIZATION HELPERS
        // ----------------------------------------------------------------------

        /// <summary>
        /// Resets the inventory when the user returns from a restart request.
        /// </summary>
        private void HandleInventoryReset()
        {
            if (StaticClass.RestartInventory)
            {
                StaticClass.Components = CreateEmptyComponentList();
                StaticClass.RestartInventory = false;
            }
        }

        /// <summary>
        /// Builds a filtered list of all components that have quantity > 0.
        /// </summary>
        private void BuildNonZeroComponentsList()
        {
            if (StaticClass.Components?.components != null)
                nonZeroComponents = StaticClass.Components.components.FindAll(c => c.quantity > 0);
            else
                nonZeroComponents = new List<Component>();
        }


        // ----------------------------------------------------------------------
        // MAIN UI RENDERING
        // ----------------------------------------------------------------------

        /// <summary>
        /// Draws the inventory UI on the LEFT pane, and the Project Suggestion UI
        /// on the RIGHT pane depending on whether the user has enough components.
        /// </summary>
        private void DrawPage()
        {
            // LEFT panel is always rebuilt
            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_LEFT);

            BuildHeaderSection();
            BuildMainActionsSection();
            DrawComponentsList();

            DrawSuggestionsPanel();
        }


        // ----------------------------------------------------------------------
        // UI SECTIONS (LEFT PANE)
        // ----------------------------------------------------------------------

        /// <summary>
        /// Draws the back button, inventory icon, and header text.
        /// </summary>
        private void BuildHeaderSection()
        {
            uiBuilder.LoadComponentImage(
                uiBuilder, "icons/back-btn.png",
                DebugUIBuilder.DEBUG_PANE_LEFT,
                () => LoadScene(4)
            );

            uiBuilder.LoadImage("icons/inventory.png",
                DebugUIBuilder.DEBUG_PANE_LEFT,
                110);

            _ = uiBuilder.AddLabel("My Inventory",
                DebugUIBuilder.DEBUG_PANE_LEFT,
                50);

            _ = uiBuilder.AddParagraph(
                "View and manage all the Arduino components you've added.",
                DebugUIBuilder.DEBUG_PANE_LEFT,
                22
            );
        }

        /// <summary>
        /// Draws the “Scan Using Camera” and “Add Manually” buttons.
        /// </summary>
        private void BuildMainActionsSection()
        {
            _ = uiBuilder.AddButton("Scan Using Camera",
                () => LoadScene(7),
                -1,
                DebugUIBuilder.DEBUG_PANE_LEFT
            );

            _ = uiBuilder.AddButton("Add Manually",
                () => LoadScene(6),
                -1,
                DebugUIBuilder.DEBUG_PANE_LEFT
            );

            _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_LEFT);
        }

        /// <summary>
        /// Displays components in inventory using pagination.
        /// </summary>
        private void DrawComponentsList()
        {
            if (nonZeroComponents.Count == 0)
            {
                _ = uiBuilder.AddLabel(
                    "No components added.",
                    DebugUIBuilder.DEBUG_PANE_LEFT,
                    28
                );
                return;
            }

            int total = nonZeroComponents.Count;
            int totalPages = Mathf.CeilToInt(total / (float)pageSize);
            bool showPagination = total > pageSize;

            // Pagination header
            if (showPagination)
            {
                _ = uiBuilder.AddLabel(
                    $"Page {currentPage + 1} / {totalPages}",
                    DebugUIBuilder.DEBUG_PANE_LEFT,
                    32
                );
                _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_LEFT);
            }

            // Components on this page
            int start = currentPage * pageSize;
            int end = Mathf.Min(start + pageSize, total);

            for (int i = start; i < end; i++)
            {
                Component comp = nonZeroComponents[i];

                string display = $"{FormatComponentName(comp.item)} (x{comp.quantity})";

                uiBuilder.LoadComponentImage(
                    uiBuilder,
                    $"2dmod/{comp.item}.jpg",
                    DebugUIBuilder.DEBUG_PANE_LEFT,
                    () => { }
                );

                _ = uiBuilder.AddLabel(
                    display,
                    DebugUIBuilder.DEBUG_PANE_LEFT,
                    23
                );

                _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_LEFT);
            }

            DrawPaginationButtons(showPagination, total);
        }


        /// <summary>
        /// Draws pagination controls if needed.
        /// </summary>
        private void DrawPaginationButtons(bool showPagination, int total)
        {
            if (!showPagination) return;

            if (currentPage > 0)
            {
                _ = uiBuilder.AddButton(
                    "← Previous Page",
                    () =>
                    {
                        currentPage--;
                        DrawPage();
                    },
                    -1,
                    DebugUIBuilder.DEBUG_PANE_LEFT
                );
            }

            if ((currentPage + 1) * pageSize < total)
            {
                _ = uiBuilder.AddButton(
                    "Next Page →",
                    () =>
                    {
                        currentPage++;
                        DrawPage();
                    },
                    -1,
                    DebugUIBuilder.DEBUG_PANE_LEFT
                );
            }
        }


        // ----------------------------------------------------------------------
        // RIGHT PANE — PROJECT SUGGESTIONS
        // ----------------------------------------------------------------------

        /// <summary>
        /// Displays the “Suggest Projects” button or a reminder message depending
        /// on how many components exist.
        /// </summary>
        private void DrawSuggestionsPanel()
        {
            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_RIGHT);

            if (nonZeroComponents.Count >= 3)
            {
                _ = uiBuilder.AddButton(
                    "Suggest Projects",
                    () => LoadScene(8),
                    -1,
                    DebugUIBuilder.DEBUG_PANE_RIGHT
                );
            }
            else
            {
                _ = uiBuilder.AddParagraph(
                    "Add at least 3 different components to get project suggestions.",
                    DebugUIBuilder.DEBUG_PANE_RIGHT,
                    22
                );
            }

            uiBuilder.Show();
        }


        // ----------------------------------------------------------------------
        // UTILITIES
        // ----------------------------------------------------------------------

        /// <summary>
        /// Converts identifiers such as "photo_resistor" into "Photo resistor".
        /// </summary>
        private string FormatComponentName(string componentName)
        {
            if (string.IsNullOrEmpty(componentName))
                return "";

            return char.ToUpper(componentName[0]) +
                   componentName.Substring(1)
                       .Replace("_", " ")
                       .Replace("(", " (")
                       .Trim();
        }

        /// <summary>
        /// Creates a fresh empty component list (used for restart inventory feature).
        /// </summary>
        private ComponentList CreateEmptyComponentList()
        {
            return new ComponentList
            {
                components = new List<Component>
                {
                    new Component { item = "arduino", quantity = 0 },
                    new Component { item = "breadboard", quantity = 0 },
                    new Component { item = "dc_motor", quantity = 0 },
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

        /// <summary>
        /// Loads the given scene index and hides DebugUI first.
        /// </summary>
        private void LoadScene(int idx)
        {
            if (idx == 1)
                StaticClass.projectid = -1;

            DebugUIBuilder.Instance.Hide();
            Debug.Log("Load scene: " + idx);
            UnityEngine.SceneManagement.SceneManager.LoadScene(idx);
        }
    }
}
