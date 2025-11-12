using System;
using System.Collections.Generic;
using Meta.XR.Samples;
using PassthroughCameraSamples.StartScene; 
using UnityEngine;

namespace PassthroughCameraSamples.SelectProject
{
    public class ManualComponentSelection : MonoBehaviour
    {
        private void Start()
        {
            var passthroughScenes = new List<Tuple<int, string>>();
            var n = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
            
            for (var sceneIndex = 1; sceneIndex < n; ++sceneIndex)
            {
                var path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(sceneIndex);
                passthroughScenes.Add(new Tuple<int, string>(sceneIndex, path));
            }
            
            var uiBuilder = DebugUIBuilder.Instance;
            
            if (passthroughScenes.Count > 0)
            {
                uiBuilder.LoadComponentImage(uiBuilder, "icons/back-btn.png", DebugUIBuilder.DEBUG_PANE_LEFT, () =>
                {
                    LoadScene(5);
                });
                
                _ = uiBuilder.AddLabel("Add Components Manually", DebugUIBuilder.DEBUG_PANE_LEFT, 40);
                _ = uiBuilder.AddLabel("Select components to add to your project:", DebugUIBuilder.DEBUG_PANE_LEFT, 30);
                
                // Add a label for each component in StaticClass
                if (StaticClass.Components != null && StaticClass.Components.components != null)
                {
                    foreach (var component in StaticClass.Components.components)
                    {
                        string displayText = $"{FormatComponentName(component.item)}: {component.quantity}";
                        _ = uiBuilder.AddLabel(displayText, DebugUIBuilder.DEBUG_PANE_LEFT);
                    }
                }
                
                uiBuilder.Show();
            }
        }
        
        private string FormatComponentName(string componentName)
        {
            // Convert snake_case to Title Case for better readability
            return componentName.Replace("_", " ")
                .Replace("(", " (")
                .Trim();
        }
        
        private void LoadScene(int idx)
        {
            DebugUIBuilder.Instance.Hide();
            Debug.Log("Load scene: " + idx);
            UnityEngine.SceneManagement.SceneManager.LoadScene(idx);
        }
    }
}