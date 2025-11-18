// Author: Gabriel Armas

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PassthroughCameraSamples.StartScene;

/// <summary>
/// Displays a frame-by-frame animation (AI stars) inside a DebugUIBuilder pane.
/// </summary>
public class StarsAnimationLoader : MonoBehaviour
{
    private Image uiImage;
    private RectTransform uiRect;

    // Set this to the amount of frames you have in Resources/ai-stars
    private const int FrameCount = 55;

    public void LoadStarsAnimation(int targetPane, int maxDisplayWidth = 400)
    {
        StartCoroutine(StarsAnimationCoroutine(0.04f, targetPane, maxDisplayWidth));
    }

    private IEnumerator StarsAnimationCoroutine(float frameDelay, int targetPane, int maxDisplayWidth)
    {
        DebugUIBuilder ui = DebugUIBuilder.Instance;

        // ----------------------------------------------------------
        // LOAD FRAMES FROM RESOURCES/ai-stars
        // ----------------------------------------------------------
        List<Sprite> frames = new List<Sprite>();

        for (int i = 0; i <= FrameCount; i++)
        {
            string path = $"ai-stars/frame_{i.ToString("D2")}_delay-0.04s.png";
            Sprite sprite = ui.LoadSpriteFromResources(path);

            if (sprite != null)
            {
                frames.Add(sprite);
            }
            else
            {
                Debug.LogWarning($"⚠️ Missing frame: {path}");
            }
        }

        if (frames.Count == 0)
        {
            ui.AddLabel("[ERROR: AI Stars Frames NOT FOUND]", targetPane);
            yield break;
        }

        // ----------------------------------------------------------
        // INSERT FIRST FRAME INTO UI
        // ----------------------------------------------------------


        ui.LoadImage("ai-stars/frame_00_delay-0.04s.png", targetPane, maxDisplayWidth);
        // Access private m_insertedElements list inside DebugUIBuilder
        var insertedField = typeof(DebugUIBuilder).GetField(
            "m_insertedElements",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        var elements = insertedField.GetValue(ui) as List<RectTransform>[];
        uiRect = elements[targetPane][elements[targetPane].Count - 1];
        uiImage = uiRect.GetComponent<Image>();

        if (uiImage == null)
        {
            Debug.LogError("❌ No Image component found on inserted UI element.");
            yield break;
        }

        // ----------------------------------------------------------
        // LOOP ANIMATION USING AI-STARS
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
