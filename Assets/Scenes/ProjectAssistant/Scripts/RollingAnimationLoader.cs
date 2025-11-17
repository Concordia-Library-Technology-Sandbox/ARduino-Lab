using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PassthroughCameraSamples.StartScene;

public class RollingAnimationLoader : MonoBehaviour
{
    private Image uiImage;           // The UI image created by LoadImage
    private RectTransform uiRect;   // The RectTransform from LoadImage

    

    public void LoadRollingAnimation(int targetPane, int maxDisplayWidth = 400)
    {
        StartCoroutine(RollingAnimationCoroutine(0.02f, targetPane, maxDisplayWidth));
    }

    private IEnumerator RollingAnimationCoroutine(float frameDelay, int targetPane, int maxDisplayWidth)
    {
        DebugUIBuilder ui = DebugUIBuilder.Instance;

        // Load all frames using YOUR LoadImage() naming convention
        List<Sprite> frames = new List<Sprite>();

        for (int i = 1; i <= 30; i++)
        {
            string path = $"rolling/{i}.png";

            Sprite sprite = ui.LoadSpriteFromResources(path);

            if (sprite != null)
                frames.Add(sprite);
            else
                Debug.LogError($"Could not load frame: {path}");
        }

        if (frames.Count == 0)
        {
            ui.AddLabel("[Rolling Animation Frames NOT FOUND]", targetPane);
            yield break;
        }

        //
        // ⭐ STEP 1 — Use YOUR LoadImage() to create the initial image.
        //
        ui.LoadImage("rolling/1.png", targetPane, maxDisplayWidth);

        //
        // ⭐ STEP 2 — Get the RectTransform of the LAST added UI element.
        // (Because LoadImage() doesn't return anything)
        //
        var elements = typeof(DebugUIBuilder)
            .GetField("m_insertedElements", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.GetValue(ui) as List<RectTransform>[];

        uiRect = elements[targetPane][elements[targetPane].Count - 1];
        uiImage = uiRect.GetComponent<Image>();


        //
        // ⭐ STEP 3 — Animate by swapping sprites
        //
        int idx = 0;

        while (true)
        {
            uiImage.sprite = frames[idx];
            idx = (idx + 1) % frames.Count;

            yield return new WaitForSeconds(frameDelay);
        }
    }
}
