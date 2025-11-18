// Author: Gabriel Armas

using System;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.Networking;

namespace PassthroughCameraSamples.StartScene
{
    // -------------------------------------------------------------------------
    // MODEL SELECTION ENUM
    // -------------------------------------------------------------------------

    /// <summary>
    /// Supported OpenAI vision-capable models.
    /// </summary>
    public enum OpenAIVisionModel
    {
        GPTModel,   // gpt-4o
        GPTModel5   // gpt-4.1-mini
    }

    /// <summary>
    /// Converts enum values into their exact OpenAI model string names.
    /// </summary>
    public static class OpenAIVisionModelExtensions
    {
        public static string ToModelString(this OpenAIVisionModel model)
        {
            return model switch
            {
                OpenAIVisionModel.GPTModel => "chatgpt-4o-latest",
                OpenAIVisionModel.GPTModel5 => "gpt-4.1-mini",
                _ => "chatgpt-4o-latest"
            };
        }
    }


    // -------------------------------------------------------------------------
    // REQUEST SCHEMA CLASSES
    // -------------------------------------------------------------------------

    /// <summary>
    /// Base shell request wrapper used for all OpenAI calls.
    /// Content is injected manually because JsonUtility cannot serialize arrays of mixed types.
    /// </summary>
    [Serializable]
    public class OpenAIRequestHeader
    {
        public string model;
        public int max_tokens;
        public OpenAIMessageShell[] messages;
    }

    /// <summary>
    /// Minimal message wrapper used for manual content injection.
    /// </summary>
    [Serializable]
    public class OpenAIMessageShell
    {
        public string role;
        // "content" is added manually later as raw JSON
    }


    // -------------------------------------------------------------------------
    // MAIN CLASS
    // -------------------------------------------------------------------------

    /// <summary>
    /// Handles:
    /// • Sending images to OpenAI for Vision + Text analysis
    /// • Generating project ideas
    /// • Generating step-by-step Arduino instructions
    ///
    /// Emits results through onJsonReceived UnityEvent<string>.
    /// </summary>
    public class ArduinoImageOpenAIConnector : MonoBehaviour
    {
        [Header("OpenAI Settings")]
        [Tooltip("API key is automatically loaded from Resources/secrets/api_key.txt if empty.")]
        public string apiKey;

        [SerializeField] private OpenAIVisionModel selectedModel = OpenAIVisionModel.GPTModel;

        [Header("Output")]
        public UnityEvent<string> onJsonReceived; // fires with raw OpenAI JSON response


        // ---------------------------------------------------------------------
        // LIFECYCLE
        // ---------------------------------------------------------------------

        private void OnEnable()
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                apiKey = LoadApiKey();
            }
        }


        // ---------------------------------------------------------------------
        // API KEY LOADING
        // ---------------------------------------------------------------------

        /// <summary>
        /// Loads API key from Resources/secrets/api_key.txt.
        /// Ensures safe storage that does not get committed to GitHub.
        /// </summary>
        private string LoadApiKey()
        {
            TextAsset keyFile = Resources.Load<TextAsset>("secrets/api_key");
            if (keyFile == null)
            {
                Debug.LogError("API key file not found: Resources/secrets/api_key.txt");
                return "";
            }
            return keyFile.text.Trim();
        }


        // ---------------------------------------------------------------------
        // ARDUINO COMPONENT DETECTION FROM IMAGE
        // ---------------------------------------------------------------------

        /// <summary>
        /// Begins an OpenAI request to detect Arduino components in an image.
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

        /// <summary>
        /// Sends an image + prompt to OpenAI's Vision models
        /// asking for component detection and returns JSON only.
        /// </summary>
        private IEnumerator SendImageRequest(Texture2D image)
        {
            // ---- Encode Image ----
            Texture2D resized = ResizeTexture(image, 512, 512);
            string base64 = Convert.ToBase64String(resized.EncodeToJPG(90));

            // ---- Prompt ----
            string command =
                "You are an Arduino lab assistant. " +
                "In this image, detect ONLY the following components and count how many of each are clearly visible:\n" +
                "- arduino\n- breadboard\n- dc_motor\n- diode\n- flex_sensor\n- led\n- lcd_screen\n" +
                "- photo_resistor\n- potentiometer\n- push_button\n- relay\n- servo_motor\n" +
                "- soft_potentiometer\n- temp_sensor\n- transistor\n- integrated_circuit\n- piezo_buzzer\n\n" +
                "Return STRICT JSON with this exact schema:\n" +
                "{ \"components\": [ { \"item\": string, \"quantity\": integer } ] }\n" +
                "Only include components that are visible with quantity > 0. No extra text.";

            // ---- Build JSON shell ----
            OpenAIRequestHeader shell = new OpenAIRequestHeader
            {
                model = selectedModel.ToModelString(),
                max_tokens = 500,
                messages = new[] { new OpenAIMessageShell { role = "user" } }
            };

            string shellJson = JsonUtility.ToJson(shell);

            // Inject content manually (text + image)
            string contentJson =
                "\"content\":[" +
                "{ \"type\":\"text\", \"text\":" + EscapeJSON(command) + " }," +
                "{ \"type\":\"image_url\", \"image_url\":{ \"url\":\"data:image/jpeg;base64," + base64 + "\" } }" +
                "]";

            string finalJson = shellJson.Replace("\"role\":\"user\"", "\"role\":\"user\"," + contentJson);

            Debug.Log("OpenAI Request Payload: " + finalJson);

            // ---- Send Request ----
            using UnityWebRequest req = new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST");
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(finalJson));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return req.SendWebRequest();

            // ---- Result ----
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


        // ---------------------------------------------------------------------
        // PROJECT GENERATION
        // ---------------------------------------------------------------------

        public void GenerateProjects(string componentsCompoundString)
        {
            StartCoroutine(SendProjectGenerationRequest(componentsCompoundString));
        }

        /// <summary>
        /// Sends a text-only request to OpenAI asking for multiple Arduino project ideas.
        /// Strict JSON schema is required.
        /// </summary>
        private IEnumerator SendProjectGenerationRequest(string componentsCompoundString)
        {
            string prompt =
                "You are an Arduino project generator. " +
                "Using ONLY the following available components:\n" +
                componentsCompoundString +
                "\nGenerate several creative project ideas. Do NOT invent components.\n\n" +
                "Each project must include:\n" +
                "• title\n• description\n• components: [ { item, quantity } ]\n\n" +
                "Return STRICT JSON in this schema:\n" +
                "{ \"projects\": [ { \"title\": string, \"description\": string, \"components\": [...] } ] }";

            // Build minimal message shell
            OpenAIRequestHeader request = new OpenAIRequestHeader
            {
                model = selectedModel.ToModelString(),
                max_tokens = 1000,
                messages = new[] { new OpenAIMessageShell { role = "user" } }
            };

            string requestJson = JsonUtility.ToJson(request);
            string contentJson = "\"content\": " + EscapeJSON(prompt);
            string finalJson = requestJson.Replace("\"role\":\"user\"", "\"role\":\"user\"," + contentJson);

            Debug.Log("OpenAI Project Generation Payload: " + finalJson);

            using UnityWebRequest req =
                new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST");

            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(finalJson));
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
                Debug.Log("Raw project generation response: " + raw);
                onJsonReceived?.Invoke(raw);
            }
        }


        // ---------------------------------------------------------------------
        // INSTRUCTION GENERATION
        // ---------------------------------------------------------------------

        public void GenerateProjectInstructions(string title, string description, string components)
        {
            StartCoroutine(SendInstructionGenerationRequest(title, description, components));
        }

        /// <summary>
        /// Sends a large prompt asking OpenAI to generate full step-by-step
        /// Arduino instructions with optional embedded code blocks.
        /// </summary>
        private IEnumerator SendInstructionGenerationRequest(string title, string description, string components)
        {
            // (Large prompt preserved exactly)

            string prompt = "You are an Arduino instructor.\n" +
            "Generate a very clear step-by-step guide for building the project using the information below.\n" +
            "Each step MUST include:\n" +
            "1. step: number\n" +
            "2. text: beginner-friendly instruction\n" +
            "3. code: null OR a code object (if this step requires Arduino code)\n" +
            "4. image_prompt: a short, simple sentence describing an image that can visually illustrate this step\n\n" +
            "PROJECT TITLE:\n" + title + "\n\n" +
            "PROJECT DESCRIPTION:\n" + description + "\n\n" +
            "AVAILABLE COMPONENTS:\n" + components + "\n\n" +
            "IMPORTANT RULES:\n" +
            "1. Use ONLY the components listed above.\n" +
            "2. Wires and resistors are NOT included in the list. They MUST be added automatically when needed.\n" +
            "   When resistors are required (e.g., for LEDs), instruct the user:\n" +
            "   'Use an appropriate resistor (typically 220Ω–1kΩ depending on LED specifications).'\n" +
            "3. The instructions MUST be extremely clear for beginners.\n" +
            "4. Steps MUST be logical and sequential.\n" +
            "5. A step MAY need code. If code is needed, include:\n" +
            "   {\n" +
            "     \"language\": \"arduino\",\n" +
            "     \"snippet\": \"<actual code>\"\n" +
            "   }\n" +
            "   Otherwise set code to null.\n" +
            "6. Each step MUST include an image_prompt describing EXACTLY what should be drawn.\n" +
            "   Image prompt rules:\n" +
            "   - Keep it short and simple.\n" +
            "   - No photography terms.\n" +
            "   - Focus on showing connections (Arduino pins, breadboard, components).\n" +
            "   - No references to JSON, steps, or the instructions.\n\n" +
            "IMPORTANT JSON VALIDITY RULES:\n" +
            "- JSON must be 100% valid and parseable.\n" +
            "- All JSON keys must use double quotes.\n" +
            "- All JSON string values must use double quotes.\n" +
            "- Inside code snippets, escape internal double quotes with \".\n" +
            "- All newlines must be escaped as \\n.\n" +
            "- DO NOT use single quotes for strings in code.\n\n" +
            "RETURN STRICT JSON USING THIS EXACT SCHEMA:\n" +
            "{\n" +
            "  \"instructions\": [\n" +
            "    {\n" +
            "      \"step\": number,\n" +
            "      \"text\": string,\n" +
            "      \"code\": { \"language\": string, \"snippet\": string } | null,\n" +
            "      \"image_prompt\": string\n" +
            "    }\n" +
            "  ]\n" +
            "}\n";
            
            OpenAIRequestHeader request = new OpenAIRequestHeader
            {
                model = selectedModel.ToModelString(),
                max_tokens = 15000,
                messages = new[] { new OpenAIMessageShell { role = "user" } }
            };

            string requestJson = JsonUtility.ToJson(request);
            string finalJson = requestJson.Replace(
                "\"role\":\"user\"",
                "\"role\":\"user\", \"content\": " + EscapeJSON(prompt)
            );

            Debug.Log("OpenAI Instruction Payload: " + finalJson);

            using UnityWebRequest req =
                new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST");

            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(finalJson));
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
                Debug.Log("Raw instruction response: " + raw);
                onJsonReceived?.Invoke(raw);
            }
        }

        public IEnumerator SendImageGenerationRequest(string prompt, Action<Texture2D> onImageReady = null)
        {
            // ----------------------------
            // 1. Build payload
            // ----------------------------
           

            var payload = new
            {
                model = "gpt-image-1-mini",
                prompt = prompt,  // No escaping needed!
                size = "1024x1024",
            };

            string jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(payload);

            Debug.Log("Image Payload: " + jsonPayload);

            // ----------------------------
            // 2. UnityWebRequest
            // ----------------------------
            using UnityWebRequest req =
                new UnityWebRequest("https://api.openai.com/v1/images/generations", "POST");

            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonPayload));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Image Generation Error: " + req.error);
                Debug.LogError(req.downloadHandler.text);
                yield break;
            }

            // ----------------------------
            // 3. Read JSON and extract base64
            // ----------------------------
            string raw = req.downloadHandler.text;
            Debug.Log("Image Gen Response: " + raw);

            // Example response:
            // { "data": [ { "b64_json": "iVBOR..." } ] }

            string base64 = null;
            try
            {
                var root = Newtonsoft.Json.Linq.JObject.Parse(raw);
                base64 = root["data"][0]["b64_json"].ToString();
            }
            catch (Exception ex)
            {
                Debug.LogError("Could not parse image JSON: " + ex.Message);
                yield break;
            }

            // ----------------------------
            // 4. Convert to Texture2D
            // ----------------------------
            byte[] bytes = Convert.FromBase64String(base64);

            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(bytes);

            Debug.Log("Image decoded successfully!");

            // Return through callback
            onImageReady?.Invoke(tex);
        }



        // ---------------------------------------------------------------------
        // HELPER FUNCTIONS
        // ---------------------------------------------------------------------


        /// <summary>
        /// Escapes text for safe injection into raw JSON fields.
        /// </summary>
        private string EscapeJSON(string s)
        {
            return "\"" +
                   s.Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\n", "\\n") +
                   "\"";
        }

        /// <summary>
        /// Resizes a Texture2D using GPU blitting for speed.
        /// </summary>
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
