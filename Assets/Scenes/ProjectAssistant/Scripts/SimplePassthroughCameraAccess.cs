// Copyright (c) Meta Platforms, Inc. and affiliates.
// Original Source code from Oculus Starter Samples (https://github.com/oculus-samples/Unity-StarterSamples)
using System;
using System.Collections.Generic;
using System.IO;
using Meta.XR.Samples;
using PassthroughCameraSamples.StartScene;
using UnityEngine;
using TMPro; 
using System.Collections;         
using UnityEngine.UI;


namespace PassthroughCameraSamples.StartScene
{
    // Create menu of all scenes included in the build.
    public class SimplePassthroughCameraAccess : MonoBehaviour


    {
        [SerializeField] private WebCamTextureManager webCamTextureManager;

        [SerializeField] private RawImage webCamImage;
        


        private IEnumerator Start()
        {
            while(webCamTextureManager.WebCamTexture == null)
            {
                yield return null;
            }


            webCamImage.texture = webCamTextureManager.WebCamTexture;
        }


        private void LoadScene(int idx)
        {
            DebugUIBuilder.Instance.Hide();
            Debug.Log("Load scene: " + idx);
            UnityEngine.SceneManagement.SceneManager.LoadScene(idx);
        }
    }
    
}