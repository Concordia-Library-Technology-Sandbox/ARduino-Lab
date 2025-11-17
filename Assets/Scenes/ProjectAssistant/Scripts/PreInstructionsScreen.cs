// Modified by Gabriel Armas
// Original Source: Meta Platforms — Oculus Starter Samples
// Refactored for ARduino Lab

using System;
using System.Collections.Generic;
using Meta.XR.Samples;
using PassthroughCameraSamples.StartScene;
using UnityEngine;

namespace PassthroughCameraSamples.SelectProject
{
    /// <summary>
    /// Displays the static instructions page for ARduino Lab.
    /// Acts as a simple informational screen with a clean UI and navigation.
    /// </summary>
    public class Instructions : MonoBehaviour
    {
        private DebugUIBuilder uiBuilder;

        private void Start()
        {
            uiBuilder = DebugUIBuilder.Instance;

            BuildInstructionsUI();

            uiBuilder.Show();
        }

        // ---------------------------------------------------------------------
        // UI BUILDING
        // ---------------------------------------------------------------------

        /// <summary>
        /// Constructs the full instruction screen layout.
        /// </summary>
        private void BuildInstructionsUI()
        {
            // Back button
            uiBuilder.LoadComponentImage(
                uiBuilder,
                "icons/back-btn.png",
                DebugUIBuilder.DEBUG_PANE_CENTER,
                () => LoadScene(1)
            );

            // Page icon
            uiBuilder.LoadImage(
                "icons/instructions.png",
                DebugUIBuilder.DEBUG_PANE_CENTER,
                110
            );

            // Title
            _ = uiBuilder.AddLabel(
                "Instructions",
                DebugUIBuilder.DEBUG_PANE_CENTER,
                50
            );

            // Body text
            _ = uiBuilder.AddParagraph(
                "Welcome to the ARduino Project Assistant!\n\n" +
                "Use your Meta Quest 3 cameras to recognize Arduino components and build projects enhanced with AR overlays.\n\n" +
                "Before you start:\n" +
                "• Place your components on a flat, well-lit surface.\n" +
                "• The SparkFun RedBoard kit works best.\n\n" +
                "Steps:\n" +
                "1. Add your components using camera detection or manual entry.\n" +
                "2. Review the detected component list.\n" +
                "3. Press the 'Generate Projects' button (on the inventory screen) to see personalized project suggestions.\n\n",
                DebugUIBuilder.DEBUG_PANE_CENTER,
                23
            );

            // Start button
            _ = uiBuilder.AddButton(
                "Start",
                () =>
                {
                    StaticClass.RestartInventory = true;
                    LoadScene(5);
                },
                -1,
                DebugUIBuilder.DEBUG_PANE_CENTER
            );
        }

        // ---------------------------------------------------------------------
        // SCENE LOADING
        // ---------------------------------------------------------------------

        /// <summary>
        /// Loads a scene by build index and hides UI before transitioning.
        /// </summary>
        private void LoadScene(int idx)
        {
            DebugUIBuilder.Instance.Hide();
            Debug.Log("Load scene: " + idx);
            UnityEngine.SceneManagement.SceneManager.LoadScene(idx);
        }
    }
}
