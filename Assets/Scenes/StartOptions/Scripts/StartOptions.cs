// Copyright (c) Meta Platforms
// Modified for ARduino Lab by Gabriel Armas
// Original source: Oculus Starter Samples (Unity Starter Samples)

using System;
using UnityEngine;
using Meta.XR.Samples;
using PassthroughCameraSamples.StartScene;

namespace PassthroughCameraSamples.SelectProject
{
    /// <summary>
    /// Menu for choosing between:
    /// - Project Assistant (component detection + AR instructions)
    /// - Project Ideas (browse suggested projects)
    /// Produces a dynamic description panel on the RIGHT pane.
    /// </summary>
    public class StartOptions : MonoBehaviour
    {
        private void Start()
        {
            BuildUI();
        }

        /// <summary>
        /// Builds the initial UI in the center pane:
        /// - Back button
        /// - "Start" label
        /// - Options: Project Assistant / Project Ideas
        /// </summary>
        private void BuildUI()
        {
            var uiBuilder = DebugUIBuilder.Instance;

            //
            // BACK BUTTON (icon)
            //
            uiBuilder.LoadComponentImage(
                uiBuilder,
                "icons/back-btn.png",
                DebugUIBuilder.DEBUG_PANE_CENTER,
                () => LoadScene(0)
            );

            //
            // TITLE
            //
            _ = uiBuilder.AddLabel("Start", DebugUIBuilder.DEBUG_PANE_CENTER, 50);

            //
            // MAIN MENU BUTTONS
            //

            _ = uiBuilder.AddButton(
                "Project Assistant",
                () => ShowDescription(uiBuilder, 1),
                -1,
                DebugUIBuilder.DEBUG_PANE_CENTER
            );

            _ = uiBuilder.AddButton(
                "Project Ideas",
                () => ShowDescription(uiBuilder, 2),
                -1,
                DebugUIBuilder.DEBUG_PANE_CENTER
            );

            // Display UI
            uiBuilder.Show();
        }

        /// <summary>
        /// Displays a dynamic description panel on the RIGHT side
        /// depending on the selected option (1 = assistant, 2 = ideas).
        /// </summary>
        private void ShowDescription(DebugUIBuilder uiBuilder, int option)
        {
            string title;
            string description;
            int sceneNumber;

            //
            // CONFIGURE TEXT + SCENE BASED ON SELECTED OPTION
            //
            if (option == 1)
            {
                title = "Project Assistant";
                description =
                    "Enter this mode to scan and recognize your Arduino components directly using your Meta Quest 3 cameras. " +
                    "The system identifies the parts you wish to use, suggests possible projects, and provides step-by-step AR overlays " +
                    "to guide you through each process. We recommend using the SparkFun RedBoard kit for the best experience.";
                sceneNumber = 4;
            }
            else
            {
                title = "Project Ideas";
                description =
                    "Browse a collection of project ideas built around the SparkFun RedBoard kit, as featured in the kit manual. " +
                    "Each project includes a detailed description and a list of required components, allowing you to recreate circuits " +
                    "or expand on them with your own modifications. It’s the perfect place to get inspired and explore what’s possible " +
                    "with the components you already have.";
                sceneNumber = 3;
            }

            //
            // CLEAR RIGHT PANE AND RENDER NEW CONTENT
            //
            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_RIGHT);

            // Title
            _ = uiBuilder.AddLabel(title, DebugUIBuilder.DEBUG_PANE_RIGHT, 40);

            // Image (optional)
            uiBuilder.LoadImage(
                $"start-options/{option}.png",
                DebugUIBuilder.DEBUG_PANE_RIGHT,
                300
            );

            // Paragraph description
            _ = uiBuilder.AddParagraph(description, DebugUIBuilder.DEBUG_PANE_RIGHT, 20);

            // Select button → loads the appropriate scene
            _ = uiBuilder.AddButton("Select", () => LoadScene(sceneNumber), -1, DebugUIBuilder.DEBUG_PANE_RIGHT);

            uiBuilder.Show();
        }

        /// <summary>
        /// Loads a Unity scene by index and hides the current UI.
        /// </summary>
        private void LoadScene(int idx)
        {
            DebugUIBuilder.Instance.Hide();
            Debug.Log("Loading scene: " + idx);

            UnityEngine.SceneManagement.SceneManager.LoadScene(idx);
        }
    }
}
