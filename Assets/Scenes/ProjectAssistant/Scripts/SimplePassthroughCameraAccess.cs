// Author: Gabriel Armas
// Modified from Meta Platforms' original Oculus Starter Samples

using System.Collections;
using Meta.XR.Samples;
using PassthroughCameraSamples.StartScene;
using UnityEngine;
using UnityEngine.UI;

namespace PassthroughCameraSamples.StartScene
{
    /// <summary>
    /// Displays the passthrough WebCamTexture inside a RawImage.
    /// This script waits until WebCamTextureManager initializes the WebCamTexture,
    /// then assigns it to the UI for real-time display.
    /// </summary>
    public class SimplePassthroughCameraAccess : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private WebCamTextureManager webCamTextureManager;

        [Tooltip("RawImage UI element where the passthrough camera feed will be shown.")]
        [SerializeField] private RawImage webCamImage;


        // ----------------------------------------------------------------------
        // UNITY LIFECYCLE
        // ----------------------------------------------------------------------

        /// <summary>
        /// Waits until WebCamTextureManager initializes its texture,
        /// then assigns it to the UI RawImage.
        /// </summary>
        private IEnumerator Start()
        {
            // Wait until the passthrough WebCamTexture becomes available
            while (webCamTextureManager.WebCamTexture == null)
            {
                yield return null;
            }

            // Assign the active passthrough camera texture
            webCamImage.texture = webCamTextureManager.WebCamTexture;
        }


        // ----------------------------------------------------------------------
        // SCENE MANAGEMENT
        // ----------------------------------------------------------------------

        /// <summary>
        /// Loads a Unity scene by build index and hides DebugUIBuilder.
        /// </summary>
        private void LoadScene(int idx)
        {
            DebugUIBuilder.Instance.Hide();
            Debug.Log("Load scene: " + idx);
            UnityEngine.SceneManagement.SceneManager.LoadScene(idx);
        }
    }
}
