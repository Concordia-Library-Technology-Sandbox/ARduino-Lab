// Copyright (c) Meta Platforms
// Modified for ARduino Lab by Gabriel Armas

using System;
using UnityEngine;
using Meta.XR.Samples;

namespace PassthroughCameraSamples.StartScene
{
    /// <summary>
    /// Main landing menu for ARduino Lab.
    /// Displays the app logo, main buttons, and a random friendly tip.
    /// </summary>
    public class StartMenu : MonoBehaviour
    {
        private void Start()
        {
            BuildUI();
        }

        /// <summary>
        /// Builds all UI elements using DebugUIBuilder:
        /// - Logo + headline
        /// - Start / Tutorial / About buttons
        /// - Random tip (right panel)
        /// </summary>
        private void BuildUI()
        {
            var uiBuilder = DebugUIBuilder.Instance;

            // App logo
            _ = uiBuilder.AddAppLogo(DebugUIBuilder.DEBUG_PANE_CENTER);

            // Subtitle
            _ = uiBuilder.AddLabel(
                "Exploring Arduino with Computer Vision, LLMs and Augmented Reality.",
                DebugUIBuilder.DEBUG_PANE_CENTER,
                fontSize: 20
            );

            // Buttons
            _ = uiBuilder.AddButton("Start", () => LoadScene(1), -1, DebugUIBuilder.DEBUG_PANE_CENTER);

            _ = uiBuilder.AddButton("Video Tutorial", () =>
            {
                Application.OpenURL("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
            }, -1, DebugUIBuilder.DEBUG_PANE_CENTER);

            _ = uiBuilder.AddButton("About", () => LoadScene(2), -1, DebugUIBuilder.DEBUG_PANE_CENTER);

            // Footer signature
            _ = uiBuilder.AddLabel(
                "ARduino Lab Beta\nBy Gabriel Armas",
                DebugUIBuilder.DEBUG_PANE_CENTER,
                fontSize: 20
            );

            // Load a random friendly tip in the RIGHT pane
            LoadRandomTip(uiBuilder);

            // Display UI
            uiBuilder.Show();
        }

        /// <summary>
        /// Loads a random tip from Resources/tips.json.
        /// </summary>
        private void LoadRandomTip(DebugUIBuilder uiBuilder)
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("tips");
            if (jsonFile == null) return;

            TipList tipsObj = JsonUtility.FromJson<TipList>(jsonFile.text);
            if (tipsObj == null || tipsObj.tips.Count == 0) return;

            var random = new System.Random();
            int index = random.Next(tipsObj.tips.Count);

            _ = uiBuilder.AddLabel(
                $"Friendly Tip: {tipsObj.tips[index].text}",
                DebugUIBuilder.DEBUG_PANE_RIGHT,
                fontSize: 20
            );
        }

        /// <summary>
        /// Loads a scene by index. Resets project ID for some scenes.
        /// </summary>
        private void LoadScene(int idx)
        {
            if (idx == 1 || idx == 3)
                StaticClass.projectid = -1;

            DebugUIBuilder.Instance.Hide();
            UnityEngine.SceneManagement.SceneManager.LoadScene(idx);
        }
    }
}
