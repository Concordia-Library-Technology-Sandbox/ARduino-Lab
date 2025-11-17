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
        GPTModel,
        GPTModel5
    }

    public static class OpenAIVisionModelExtensions
    {
        public static string ToModelString(this OpenAIVisionModel model)
        {
            return model switch
            {
                OpenAIVisionModel.GPTModel => "chatgpt-4o-latest",
                OpenAIVisionModel.GPTModel5=> "gpt-4.1-mini",        
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
        [SerializeField] private OpenAIVisionModel selectedModel = OpenAIVisionModel.GPTModel;
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
                "In this image,detect ONLY the following components and count how many of each are clearly visible:\n" +
                "- arduino\n" +
                "- breadboard\n" +
                "- dc_motor\n" +
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
                max_tokens = 500,
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


        public void GenerateProjects(string componentsCompoundString)
        {
            StartCoroutine(SendProjectGenerationRequest(componentsCompoundString));
        }

        private IEnumerator SendProjectGenerationRequest(string componentsCompoundString)
        {
        string prompt =
        "You are an Arduino project generator. " +
        "Using ONLY the following available components:\n" +
        componentsCompoundString +
        "\nGenerate several creative project ideas, even if they reuse the same components in different ways. " +
        "If only one or two meaningful projects are possible, return only those. " +
        "Do not invent components.\n\n" +

        "For each project, provide:\n" +
        "• A short, creative title\n" +
        "• A brief description\n" +
        "• A list of components required (each with item and quantity)\n\n" +

        "Return ONLY STRICT JSON in this exact schema:\n" +
        "{ \"projects\": [ " +
        "{ \"title\": string, \"description\": string, \"components\": [ " +
        "{ \"item\": string, \"quantity\": number } ] } ] }";




            // Build request JSON
            OpenAIRequestHeader request = new OpenAIRequestHeader
            {
                model = selectedModel.ToModelString(),
                max_tokens = 1000,
                messages = new[]
                {
                    new OpenAIMessageShell
                    {
                        role = "user"
                    }
                }
            };

            string requestJson = JsonUtility.ToJson(request);
            string contentJson =
                "\"content\": " + EscapeJSON(prompt);

            string finalJson = requestJson.Replace(
                "\"role\":\"user\"",
                "\"role\":\"user\"," + contentJson
            );

            Debug.Log("OpenAI Project Generation Request Payload: " + finalJson);

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
                Debug.Log("Raw project generation response: " + raw);
                onJsonReceived?.Invoke(raw);
            }
        }

        public void GenerateProjectInstructions(string title, string description, string components)
        {
            StartCoroutine(SendInstructionGenerationRequest(title, description, components));
        }


        private IEnumerator SendInstructionGenerationRequest(string title, string description, string components)
        {
            string prompt =
            "You are an Arduino instructor.\n" +
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
            "     \\\"language\\\": \\\"arduino\\\",\n" +
            "     \\\"snippet\\\": \\\"<actual code>\\\"\n" +
            "   }\n" +
            "   Otherwise set code to null.\n" +
            "6. Each step MUST include an image_prompt describing EXACTLY what should be drawn.\n" +
            "   Image prompt rules:\n" +
            "   - Keep it short and simple.\n" +
            "   - No photography terms.\n" +
            "   - Focus on showing connections (Arduino pins, breadboard, components).\n" +
            "   - No references to JSON, steps, or the instructions.\n\n" +

            "IMPORTANT JSON VALIDITY RULES:\n" +
            "- The response MUST NOT contain Markdown code fences (no ```json or ```).\n" +
            "- JSON keys and string delimiters MUST use double quotes.\n" +
            "- INSIDE all string values (for 'text', 'snippet', and 'image_prompt'), you MUST use ONLY single quotes 'like this'.\n" +
            "- NEVER include double quotes inside any string field.\n" +
            "- If referencing a phrase, write it as: 'Flex Value' instead of \"Flex Value\".\n" +
            "- The final JSON MUST be valid and parseable without needing post-processing.\n\n" +

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



            // Build request JSON
            OpenAIRequestHeader request = new OpenAIRequestHeader
            {
                model = selectedModel.ToModelString(),
                max_tokens = 15000,
                messages = new[]
                {
                    new OpenAIMessageShell { role = "user" }
                }
            };

            string requestJson = JsonUtility.ToJson(request);

            string contentJson = "\"content\": " + EscapeJSON(prompt);

            string finalJson = requestJson.Replace(
                "\"role\":\"user\"",
                "\"role\":\"user\"," + contentJson
            );

            Debug.Log("OpenAI Instruction Generation Request Payload: " + finalJson);

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
                Debug.Log("Raw project instruction response: " + raw);
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
