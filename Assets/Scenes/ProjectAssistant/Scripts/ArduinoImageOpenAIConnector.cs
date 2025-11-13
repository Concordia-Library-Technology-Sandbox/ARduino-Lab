using System;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.Networking;

namespace PassthroughCameraSamples.StartScene
{
    public enum OpenAIVisionModel
    {
        Gpt41Mini
    }

    public static class OpenAIVisionModelExtensions
    {
        public static string ToModelString(this OpenAIVisionModel model)
        {
            return model switch
            {
                OpenAIVisionModel.Gpt41Mini => "gpt-4.1-mini",
                _ => "gpt-4o"
            };
        }
    }

    [Serializable]
    public class OpenAIRequestHeader
    {
        public string model;
        public int max_tokens;
        public OpenAIMessageShell[] messages;
    }

    [Serializable]
    public class OpenAIMessageShell
    {
        public string role;
        // content omitted because we inject it manually
    }

    public class ArduinoImageOpenAIConnector : MonoBehaviour
    {
        public string apiKey;
        [SerializeField] private OpenAIVisionModel selectedModel = OpenAIVisionModel.Gpt41Mini;
        public UnityEvent<string> onJsonReceived;

        private void OnEnable()
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                apiKey = LoadApiKey();
            }
        }

        private string LoadApiKey()
        {
            TextAsset keyFile = Resources.Load<TextAsset>("secrets/api_key");
            if (keyFile == null)
            {
                Debug.LogError("API key not found at Assets/Resources/secrets/api_key.txt");
                return "";
            }
            return keyFile.text.Trim();
        }

        public void AnalyzeArduinoComponents(Texture2D image)
        {
            if (image == null)
            {
                Debug.LogError("Image is null.");
                return;
            }
            StartCoroutine(SendImageRequest(image));
        }

        private IEnumerator SendImageRequest(Texture2D image)
        {
            // Encode image
            Texture2D resized = ResizeTexture(image, 512, 512);
            string base64 = Convert.ToBase64String(resized.EncodeToJPG(90));

            string command =
                "You are an Arduino lab assistant. " +
                "In this image, detect ONLY the following components and count how many of each are clearly visible:\n" +
                "- arduino\n" +
                "- breadboard\n" +
                "- DC_Motor\n" +
                "- diode\n" +
                "- flex_sensor\n" +
                "- led\n" +
                "- lcd_screen\n" +
                "- photo_resistor\n" +
                "- potentiometer\n" +
                "- push_button\n" +
                "- relay\n" +
                "- servo_motor\n" +
                "- soft_potentiometer\n" +
                "- temp_sensor\n" +
                "- transistor\n" +
                "- integrated_circuit\n" +
                "- piezo_buzzer\n\n" +
                "Return STRICT JSON with this exact schema:\n" +
                "{ \"components\": [ { \"item\": string, \"quantity\": integer } ] }\n" +
                "The \"item\" field must use exactly one of the names from the list above. " +
                "Only include components that are present with quantity > 0. " +
                "Do not include any explanation or extra text, only valid JSON.";

            // Build SAFE top-level JSON (without content)
            OpenAIRequestHeader shell = new OpenAIRequestHeader
            {
                model = selectedModel.ToModelString(),
                max_tokens = 300,
                messages = new[]
                {
                    new OpenAIMessageShell { role = "user" }
                }
            };

            string shellJson = JsonUtility.ToJson(shell);

            // Build content array manually (correct JSON)
            string contentJson =
                "\"content\":[" +
                "{ \"type\":\"text\", \"text\":" + EscapeJSON(command) + " }," +
                "{ \"type\":\"image_url\", \"image_url\":{ \"url\":\"data:image/jpeg;base64," + base64 + "\" } }" +
                "]";

            // Inject content into the shell JSON
            string finalJson = shellJson.Replace(
                "\"role\":\"user\"",
                "\"role\":\"user\"," + contentJson
            );

            Debug.Log("OpenAI Request Payload: " + finalJson);

            // Send request
            using UnityWebRequest req =
                new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST");

            byte[] body = Encoding.UTF8.GetBytes(finalJson);
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("OpenAI Error: " + req.downloadHandler.text);
            }
            else
            {
                string raw = req.downloadHandler.text;
                Debug.Log("Raw response: " + raw);
                onJsonReceived?.Invoke(raw);
            }
        }

        private string EscapeJSON(string s)
        {
            return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n") + "\"";
        }

        private Texture2D ResizeTexture(Texture2D src, int w, int h)
        {
            RenderTexture rt = RenderTexture.GetTemporary(w, h);
            RenderTexture.active = rt;
            Graphics.Blit(src, rt);

            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);
            return tex;
        }
    }
}
