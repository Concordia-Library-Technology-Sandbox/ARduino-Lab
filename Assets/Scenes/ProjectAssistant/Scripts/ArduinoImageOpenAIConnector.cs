using System;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.Networking;

namespace QuestCameraKit.OpenAI
{
    public enum OpenAIVisionModel
    {
        Gpt4O,
        Gpt4OMini
    }

    public static class OpenAIVisionModelExtensions
    {
        public static string ToModelString(this OpenAIVisionModel model)
        {
            return model switch
            {
                OpenAIVisionModel.Gpt4O => "gpt-4o",
                OpenAIVisionModel.Gpt4OMini => "gpt-4o-mini",
                _ => "gpt-4o"
            };
        }
    }

    public class ArduinoImageOpenAIConnector : MonoBehaviour
    {
        [Header("OpenAI Settings")]
        [Tooltip("Your OpenAI API key. For prototyping only; do not hardcode in production.")]
        public string apiKey = "YOUR_API_KEY";

        [SerializeField] private OpenAIVisionModel selectedModel = OpenAIVisionModel.Gpt4O;

        [Header("Events")]
        [Tooltip("Invoked with the JSON string returned by the model.")]
        public UnityEvent<string> onJsonReceived;

        /// <summary>
        /// Call this with a Texture2D from your camera/passthrough.
        /// </summary>
        public void AnalyzeArduinoComponents(Texture2D image)
        {
            if (image == null)
            {
                Debug.LogError("AnalyzeArduinoComponents: image is null.");
                return;
            }

            StartCoroutine(SendImageRequest(image));
        }

        private IEnumerator SendImageRequest(Texture2D image)
        {
            // Prepare image (resize and encode to JPG)
            Texture2D processedImage;
            if (image.width == 512 && image.height == 512 && image.format == TextureFormat.RGBA32)
            {
                processedImage = image;
            }
            else
            {
                processedImage = ResizeTexture(image, 512, 512);
            }

            byte[] imageBytes = processedImage.EncodeToJPG(90);
            if (imageBytes == null || imageBytes.Length == 0)
            {
                Debug.LogError("Failed to encode image to JPG. Make sure the texture is readable.");
                yield break;
            }

            string base64Image = Convert.ToBase64String(imageBytes);

            // Build the user message content with text + image_url
            // Prompt: detect only these components and return strict JSON.
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

            string contentJson =
                "{" +
                    "\"type\":\"text\",\"text\":\"" + EscapeJson(command) + "\"" +
                "}," +
                "{" +
                    "\"type\":\"image_url\",\"image_url\":{" +
                        "\"url\":\"data:image/jpeg;base64," + base64Image + "\"" +
                    "}" +
                "}";

            string payloadJson =
                "{" +
                    $"\"model\":\"{selectedModel.ToModelString()}\"," +
                    "\"messages\":[{" +
                        "\"role\":\"user\"," +
                        "\"content\":[" + contentJson + "]" +
                    "}]," +
                    "\"max_tokens\":300" +
                "}";

            using UnityWebRequest request = new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(payloadJson);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            Debug.Log("Sending OpenAI vision request...");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error sending request: {request.error} (Response code: {request.responseCode})");
                Debug.LogError(request.downloadHandler.text);
            }
            else
            {
                string raw = request.downloadHandler.text;
                Debug.Log("Raw OpenAI response: " + raw);

                // Very simple JSON extraction using string search or a lightweight parser library.
                // The important part is: choices[0].message.content contains the JSON we asked for.
                // Here we just use a quick manual extraction assuming the format is stable.
                try
                {
                    // If you have OVRSimpleJSON, you can use that instead:
                    // var json = OVRSimpleJSON.JSON.Parse(raw);
                    // string content = json["choices"][0]["message"]["content"].Value;

                    string content = ExtractContentField(raw);
                    Debug.Log("OpenAI JSON content: " + content);

                    onJsonReceived?.Invoke(content);
                }
                catch (Exception ex)
                {
                    Debug.LogError("Failed to extract JSON content: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Resizes or converts the given texture to a 512x512 RGBA32 texture.
        /// </summary>
        private Texture2D ResizeTexture(Texture2D source, int targetWidth, int targetHeight)
        {
            RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight);
            rt.filterMode = FilterMode.Bilinear;
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;
            Graphics.Blit(source, rt);

            Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
            result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
            result.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);
            return result;
        }

        /// <summary>
        /// Escapes quotes for embedding text safely inside JSON strings.
        /// </summary>
        private string EscapeJson(string input)
        {
            return input.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        /// <summary>
        /// Very simple extraction of choices[0].message.content as a string.
        /// If you already use OVRSimpleJSON, replace this with proper parsing.
        /// </summary>
        private string ExtractContentField(string rawJson)
        {
            // Recommended: use OVRSimpleJSON instead of this manual helper:
            // var json = OVRSimpleJSON.JSON.Parse(rawJson);
            // return json["choices"][0]["message"]["content"].Value;

            int contentIndex = rawJson.IndexOf("\"content\":", StringComparison.Ordinal);
            if (contentIndex < 0)
                throw new Exception("content field not found");

            int firstQuote = rawJson.IndexOf('\"', contentIndex + 10);
            int lastQuote = rawJson.LastIndexOf('\"');

            if (firstQuote < 0 || lastQuote <= firstQuote)
                throw new Exception("content string quotes not found");

            string content = rawJson.Substring(firstQuote + 1, lastQuote - firstQuote - 1);

            // Unescape basic JSON escapes
            content = content.Replace("\\n", "\n").Replace("\\\"", "\"");
            return content;
        }
    }
}
