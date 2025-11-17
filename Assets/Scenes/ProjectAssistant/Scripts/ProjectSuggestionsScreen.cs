// Copyright (c) Meta Platforms
// Modified for ARduino Lab

using System;
using System.Collections.Generic;
using Meta.XR.Samples;
using PassthroughCameraSamples.StartScene;
using UnityEngine;
using UnityEngine.SceneManagement;
using OVRSimpleJSON;

namespace PassthroughCameraSamples.SelectProject
{
    public class ProjectSuggestionsScreen : MonoBehaviour
    {
        [SerializeField] private ArduinoImageOpenAIConnector openAIConnector;

        private DebugUIBuilder uiBuilder;

        // Project list (center panel)
        private List<JSONNode> projectList = new List<JSONNode>();

        // Component pagination (RIGHT PANEL ONLY)
        private int componentPage = 0;
        private const int componentPageSize = 3;
        private JSONArray currentComponents;
        private string currentTitle;
        private string currentDescription;

        [SerializeField] private RollingAnimationLoader rollingLoader;


        private void Start()
        {
            uiBuilder = DebugUIBuilder.Instance;

            // Listener BEFORE generating
            openAIConnector.onJsonReceived.AddListener(OnProjectsJsonReceived);

            // Request project ideas from OpenAI
            openAIConnector.GenerateProjects(StaticClass.generateCompoundStringOfComponents());

            // Initial Loading UI
            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_CENTER);

            rollingLoader.LoadRollingAnimation(DebugUIBuilder.DEBUG_PANE_CENTER);

            _ = uiBuilder.AddLabel("Generating Projects...", DebugUIBuilder.DEBUG_PANE_CENTER, 40);

            uiBuilder.Show();
        }

        private void OnProjectsJsonReceived(string json)
        {
            Debug.Log("Received project suggestions JSON: " + json);

            try
            {
                JSONNode root = JSON.Parse(json);
                string rawContent = root["choices"][0]["message"]["content"];

                string cleaned = rawContent
                                .Replace("```json", "")
                                .Replace("```", "")
                                .Trim();

                JSONNode result = JSON.Parse(cleaned);
                JSONArray projects = result["projects"].AsArray;

                projectList.Clear();
                foreach (var p in projects)
                    projectList.Add(p.Value);

                ShowProjectButtons();
            }
            catch (Exception e)
            {
                Debug.LogError("Error parsing project suggestions JSON: " + e.Message);
            }
        }

        // ==============================
        //   PROJECT BUTTONS (NO PAGING)
        // ==============================
        private void ShowProjectButtons()
        {
            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_CENTER);

            // Back button
            uiBuilder.LoadComponentImage(
                uiBuilder,
                "icons/back-btn.png",
                DebugUIBuilder.DEBUG_PANE_CENTER,
                () => LoadScene(1)
            );

            _ = uiBuilder.AddLabel("Project Suggestions", DebugUIBuilder.DEBUG_PANE_CENTER, 48);

            for (int i = 0; i < projectList.Count; i++)
            {
                var project = projectList[i];

                string title = project["title"];
                string description = project["description"];
                JSONArray components = project["components"].AsArray;

                // Each project gets a button
                _ = uiBuilder.AddButton(
                    $"{i + 1}. {title}",
                    () => LoadSidePanel(title, description, components),
                    -1,
                    DebugUIBuilder.DEBUG_PANE_CENTER
                );

                _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_CENTER);
            }

            uiBuilder.Show();
        }

   
        private void LoadSidePanel(string title, string description, JSONArray components)
        {
            currentTitle = title;
            currentDescription = description;
            currentComponents = components;
            componentPage = 0;

            ShowComponentPage();
        }

        private void ShowComponentPage()
        {
            uiBuilder.Clear(DebugUIBuilder.DEBUG_PANE_RIGHT);

            // Title + description
            _ = uiBuilder.AddParagraph(currentTitle, DebugUIBuilder.DEBUG_PANE_RIGHT, 40);
            _ = uiBuilder.AddParagraph(currentDescription, DebugUIBuilder.DEBUG_PANE_RIGHT, 20);

            _ = uiBuilder.AddButton("Select Project", () =>
            {
                StaticClass.Reset();
                StaticClass.projectTitle = currentTitle;
                StaticClass.projectDescription = currentDescription;
                
                for (int i = 0; i < currentComponents.Count; i++)
                {
                    string item = currentComponents[i]["item"];
                    int qty = currentComponents[i]["quantity"];

                    StaticClass.AddComponentQuantity(item, qty);
                }


                LoadScene(9);
            }, -1, DebugUIBuilder.DEBUG_PANE_RIGHT);

            _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_RIGHT);

            // Pagination math
            int total = currentComponents.Count;
            int totalPages = Mathf.CeilToInt(total / (float)componentPageSize);

            int start = componentPage * componentPageSize;
            int end = Mathf.Min(start + componentPageSize, total);

            if (total > componentPageSize)
            {
                _ = uiBuilder.AddLabel(
                    $"Page {componentPage + 1} / {totalPages}",
                    DebugUIBuilder.DEBUG_PANE_RIGHT,
                    30
                );

                _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_RIGHT);
            }

            // COMPONENTS FOR THIS PAGE
            for (int i = start; i < end; i++)
            {
                JSONNode comp = currentComponents[i];
                string item = comp["item"];
                int qty = comp["quantity"];

                uiBuilder.LoadComponentImage(
                    uiBuilder,
                    $"2dmod/{item}.jpg",
                    DebugUIBuilder.DEBUG_PANE_RIGHT,
                    () => {}
                );

                _ = uiBuilder.AddLabel(
                    $"{item} (x{qty})",
                    DebugUIBuilder.DEBUG_PANE_RIGHT,
                    22
                );

                _ = uiBuilder.AddDivider(DebugUIBuilder.DEBUG_PANE_RIGHT);
            }

            // PAGINATION BUTTONS
            if (componentPage > 0)
            {
                _ = uiBuilder.AddButton("← Previous Page", () =>
                {
                    componentPage--;
                    ShowComponentPage();
                }, -1, DebugUIBuilder.DEBUG_PANE_RIGHT);
            }

            if ((componentPage + 1) * componentPageSize < total)
            {
                _ = uiBuilder.AddButton("Next Page →", () =>
                {
                    componentPage++;
                    ShowComponentPage();
                }, -1, DebugUIBuilder.DEBUG_PANE_RIGHT);
            }

            uiBuilder.Show();
        }

          private void LoadScene(int idx)
        {
            DebugUIBuilder.Instance.Hide();
            SceneManager.LoadScene(idx);
        }
    }
}
