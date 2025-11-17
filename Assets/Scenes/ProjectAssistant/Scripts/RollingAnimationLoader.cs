// Author: Gabriel Armas

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PassthroughCameraSamples.StartScene;

/// <summary>
/// Displays a rolling frame-by-frame animation inside a DebugUIBuilder pane.
/// Uses sprites stored in Resources/rolling/1.png ... 30.png.
/// The animation loops indefinitely by swapping UI Image sprites.
/// </summary>
public class RollingAnimationLoader : MonoBehaviour
{
    // UI elements created dynamically through DebugUIBuilder.LoadImage()
    private Image uiImage;           
    private RectTransform uiRect;

    // Number of animation frames expected in Resources/rolling/
    private const int FrameCount = 30;


    // ----------------------------------------------------------------------
    // PUBLIC API
    // ----------------------------------------------------------------------

    /// <summary>
    /// Creates a loading animation inside the specified DebugUIBuilder pane.
    /// </summary>
    /// <param name="targetPane">Pane index (e.g., DebugUIBuilder.DEBUG_PANE_CENTER)</param>
    /// <param name="maxDisplayWidth">Max width for the displayed image</param>
    public void LoadRollingAnimation(int targetPane, int maxDisplayWidth = 400)
    {
        StartCoroutine(RollingAnimationCoroutine(0.02f, targetPane, maxDisplayWidth));
    }


    // ----------------------------------------------------------------------
    // INTERNAL ANIMATION COROUTINE
    // ----------------------------------------------------------------------

    /// <summary>
    /// Loads all rolling animation sprites, inserts an initial UI image,
    /// retrieves its Image component via reflection, and swaps its sprite
    /// every frame to animate the loader.
    /// </summary>
    private IEnumerator RollingAnimationCoroutine(float frameDelay, int targetPane, int maxDisplayWidth)
    {
        DebugUIBuilder ui = DebugUIBuilder.Instance;

        // ----------------------------------------------------------
        // STEP 1 — Load animation frames from Resources
        // ----------------------------------------------------------
        List<Sprite> frames = new List<Sprite>();

        for (int i = 1; i <= FrameCount; i++)
        {
            string path = $"rolling/{i}.png";
            Sprite sprite = ui.LoadSpriteFromResources(path);

            if (sprite != null)
                frames.Add(sprite);
            else
                Debug.LogError($"❌ RollingAnimationLoader: Could not load frame '{path}'");
        }

        if (frames.Count == 0)
        {
            ui.AddLabel("[ERROR: Rolling Animation Frames NOT FOUND]", targetPane);
            yield break;
        }

        // ----------------------------------------------------------
        // STEP 2 — Add the first image using DebugUIBuilder's API
        //          (LoadImage does NOT return the UI element)
        // ----------------------------------------------------------
        ui.LoadImage("rolling/1.png", targetPane, maxDisplayWidth);


        // ----------------------------------------------------------
        // STEP 3 — Retrieve the UI element that was just inserted
        //          DebugUIBuilder stores elements in a *private* list.
        //          We use reflection to fetch the last added element.
        // ----------------------------------------------------------

        // Access DebugUIBuilder.m_insertedElements (private)
        var insertedField = typeof(DebugUIBuilder)
            .GetField("m_insertedElements",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

        if (insertedField == null)
        {
            Debug.LogError("❌ Reflection failed: m_insertedElements not found.");
            yield break;
        }

        var elements =
            insertedField.GetValue(ui) as List<RectTransform>[];

        if (elements == null ||
            elements.Length <= targetPane ||
            elements[targetPane].Count == 0)
        {
            Debug.LogError("❌ RollingAnimationLoader: Could not retrieve inserted UI elements.");
            yield break;
        }

        // Retrieve last UI element (the image we just added)
        uiRect = elements[targetPane][elements[targetPane].Count - 1];
        uiImage = uiRect.GetComponent<Image>();

        if (uiImage == null)
        {
            Debug.LogError("❌ RollingAnimationLoader: Loaded UI element has no Image component.");
            yield break;
        }

        // ----------------------------------------------------------
        // STEP 4 — Loop animation by swapping sprites
        // ----------------------------------------------------------

        int index = 0;

        while (true)
        {
            uiImage.sprite = frames[index];
            index = (index + 1) % frames.Count;

            yield return new WaitForSeconds(frameDelay);
        }
    }
}
