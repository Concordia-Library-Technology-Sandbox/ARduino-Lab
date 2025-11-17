// Author: Gabriel Armas

using System;
using System.Collections.Generic;
using System.IO;
using Meta.XR.Samples;
using PassthroughCameraSamples.StartScene;
using UnityEngine;

namespace PassthroughCameraSamples.SelectProject
{
    /// <summary>
    /// Displays the "About" screen for ARduino Lab.
    /// Includes project description, developer info, and a photo gallery.
    /// </summary>
    public class AboutMenu : MonoBehaviour
    {
        private DebugUIBuilder uiBuilder;

        private void Start()
        {
            uiBuilder = DebugUIBuilder.Instance;

            BuildUI();
            uiBuilder.Show();
        }

        // ----------------------------------------------------------------------
        // UI CONSTRUCTION
        // ----------------------------------------------------------------------

        /// <summary>
        /// Constructs the full left and right pane UI for the About screen.
        /// </summary>
        private void BuildUI()
        {
            // ==========================
            // LEFT PANE — MAIN CONTENT
            // ==========================

            // Back button
            uiBuilder.LoadComponentImage(
                uiBuilder,
                "icons/back-btn.png",
                DebugUIBuilder.DEBUG_PANE_LEFT,
                () => LoadScene(0)
            );

            // Title
            _ = uiBuilder.AddLabel("About ARduino Lab", DebugUIBuilder.DEBUG_PANE_LEFT, 40);

            // App logo
            _ = uiBuilder.AddAppLogo(DebugUIBuilder.DEBUG_PANE_LEFT);

            // Project description
            _ = uiBuilder.AddParagraph(
                "ARduino Lab is a personal project and proof of concept built to explore "
                + "the Meta Passthrough API, which enables developers to access the Meta Quest cameras. "
                + "The goal of the project is to make it easier for beginners to learn about Arduino components "
                + "and simple electronics projects through an engaging, hands-on experience. "
                + "Arduino Lab received funding through the EL Grant and has been supported and developed at the "
                + "Concordia University Library Technology Sandbox.",
                DebugUIBuilder.DEBUG_PANE_LEFT,
                17
            );

            _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_LEFT);

            // Developer section
            _ = uiBuilder.AddLabel("About The Developer", DebugUIBuilder.DEBUG_PANE_LEFT, 40);

            // Developer photo
            uiBuilder.LoadImage("abouts/gabriel_armas.png", DebugUIBuilder.DEBUG_PANE_LEFT, 200);

            // Developer bio
            _ = uiBuilder.AddParagraph(
                "Hello! My name is Gabriel, and I am a final-year CS undergraduate student "
                + "at Concordia University in Montreal, Canada. My interests include computer vision, "
                + "augmented reality, and robotics, where I have gained hands-on experience through "
                + "internships and research projects. I am passionate about developing innovative solutions "
                + "and aspire to pursue academic research in the future.",
                DebugUIBuilder.DEBUG_PANE_LEFT,
                17
            );

            // Social links
            _ = uiBuilder.AddParagraph(
                "github.com/gabxap\nlinkedin.com/in/gabriel-armas",
                DebugUIBuilder.DEBUG_PANE_LEFT,
                20
            );


            // ==========================
            // RIGHT PANE — PHOTO GALLERY
            // ==========================

            uiBuilder.LoadImage("abouts/1.jpg", DebugUIBuilder.DEBUG_PANE_RIGHT, 1000);

            _ = uiBuilder.AddLabel(
                "Concordia University Library Technology Sandbox.",
                DebugUIBuilder.DEBUG_PANE_RIGHT,
                18
            );
        }


        // ----------------------------------------------------------------------
        // SCENE MANAGEMENT
        // ----------------------------------------------------------------------

        /// <summary>
        /// Loads a Unity scene by build index.
        /// Hides the debug UI first.
        /// </summary>
        private void LoadScene(int idx)
        {
            DebugUIBuilder.Instance.Hide();
            Debug.Log("Load scene: " + idx);
            UnityEngine.SceneManagement.SceneManager.LoadScene(idx);
        }
    }
}
