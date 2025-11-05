// Copyright (c) Meta Platforms, Inc. and affiliates.
// Original Source code from Oculus Starter Samples (https://github.com/oculus-samples/Unity-StarterSamples)

using System.Collections.Generic;
using Meta.XR.Samples;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;


namespace PassthroughCameraSamples.StartScene
{

    public class DebugUIBuilder : MonoBehaviour
    {
        // room for extension:
        // support update funcs
        // fix bug where it seems to appear at a random offset
        // support remove

        // Convenience consts for clarity when using multiple debug panes.
        // But note that you can an arbitrary number of panes if you add them in the inspector.
        public const int DEBUG_PANE_CENTER = 0;
        public const int DEBUG_PANE_RIGHT = 1;
        public const int DEBUG_PANE_LEFT = 2;

        [SerializeField]
        private RectTransform m_buttonPrefab = null;

        [SerializeField]
        private RectTransform[] m_additionalButtonPrefab = null;

        [SerializeField]
        private RectTransform m_labelPrefab = null;

        [SerializeField]
        private RectTransform m_sliderPrefab = null;

        [SerializeField]
        private RectTransform m_dividerPrefab = null;

        [SerializeField]
        private RectTransform m_togglePrefab = null;

        [SerializeField]
        private RectTransform m_radioPrefab = null;

        [SerializeField]
        private RectTransform m_textPrefab = null;

        [SerializeField]
        private GameObject m_uiHelpersToInstantiate = null;

        [SerializeField]
        private Transform[] m_targetContentPanels = null;

        [SerializeField]
        private RectTransform m_ConcordiaPrefab = null;

        private bool[] m_reEnable;

        [SerializeField]
        private List<GameObject> m_toEnable = null;

        [SerializeField]
        private List<GameObject> m_toDisable = null;

        public static DebugUIBuilder Instance;

        public delegate void OnClick();

        public delegate void OnToggleValueChange(Toggle t);

        public delegate void OnSlider(float f);

        public delegate bool ActiveUpdate();

        public float ElementSpacing = 16.0f;
        public float MarginH = 16.0f;
        public float MarginV = 16.0f;
        private Vector2[] m_insertPositions;
        private List<RectTransform>[] m_insertedElements;
        private Vector3 m_menuOffset;
        private OVRCameraRig m_rig;
        private Dictionary<string, ToggleGroup> m_radioGroups = new();
        private LaserPointer m_lp;
        private LineRenderer m_lr;

        public LaserPointer.LaserBeamBehaviorEnum LaserBeamBehavior = LaserPointer.LaserBeamBehaviorEnum.OnWhenHitTarget;
        public bool IsHorizontal = false;
        public bool UsePanelCentricRelayout = false;


        public void Clear(int targetCanvas)
        {
            if (targetCanvas >= m_targetContentPanels.Length)
            {
            Debug.LogError("Attempted to clear debug panel on canvas " + targetCanvas + ", but only " +
                       m_targetContentPanels.Length +
                       " panels were provided. Fix in the inspector or pass a lower value for target canvas.");
            return;
            }

            var elements = m_insertedElements[targetCanvas];
            foreach (var element in elements)
            {
            Destroy(element.gameObject);
            }

            elements.Clear();
            m_insertPositions[targetCanvas] = new Vector2(MarginH, -MarginV);

            if (gameObject.activeInHierarchy)
            {
            Relayout();
            }
        }

        public void Awake()
        {
            Debug.Assert(Instance == null);
            Instance = this;
            m_menuOffset = transform.position;
            gameObject.SetActive(false);
            m_rig = FindFirstObjectByType<OVRCameraRig>();
            for (var i = 0; i < m_toEnable.Count; ++i)
            {
                m_toEnable[i].SetActive(false);
            }

            m_insertPositions = new Vector2[m_targetContentPanels.Length];
            for (var i = 0; i < m_insertPositions.Length; ++i)
            {
                m_insertPositions[i].x = MarginH;
                m_insertPositions[i].y = -MarginV;
            }

            m_insertedElements = new List<RectTransform>[m_targetContentPanels.Length];
            for (var i = 0; i < m_insertedElements.Length; ++i)
            {
                m_insertedElements[i] = new List<RectTransform>();
            }

            if (m_uiHelpersToInstantiate)
            {
                _ = Instantiate(m_uiHelpersToInstantiate);
            }

            m_lp = FindFirstObjectByType<LaserPointer>();

            if (!m_lp)
            {
                Debug.LogError("Debug UI requires use of a LaserPointer and will not function without it. " +
                            "Add one to your scene, or assign the UIHelpers prefab to the DebugUIBuilder in the inspector.");
                return;
            }

            m_lp.LaserBeamBehavior = LaserBeamBehavior;

            if (!m_toEnable.Contains(m_lp.gameObject))
            {
                m_toEnable.Add(m_lp.gameObject);
            }

            GetComponent<OVRRaycaster>().pointer = m_lp.gameObject;
            m_lp.gameObject.SetActive(false);
        }

        public void Show()
        {
            Relayout();
            gameObject.SetActive(true);
            transform.position = m_rig.transform.TransformPoint(m_menuOffset);
            var newEulerRot = m_rig.transform.rotation.eulerAngles;
            newEulerRot.x = 0.0f;
            newEulerRot.z = 0.0f;
            transform.eulerAngles = newEulerRot;

            if (m_reEnable == null || m_reEnable.Length < m_toDisable.Count) m_reEnable = new bool[m_toDisable.Count];
            m_reEnable.Initialize();
            var len = m_toDisable.Count;
            for (var i = 0; i < len; ++i)
            {
                if (m_toDisable[i])
                {
                    m_reEnable[i] = m_toDisable[i].activeSelf;
                    m_toDisable[i].SetActive(false);
                }
            }

            len = m_toEnable.Count;
            for (var i = 0; i < len; ++i)
            {
                m_toEnable[i].SetActive(true);
            }

            var numPanels = m_targetContentPanels.Length;
            for (var i = 0; i < numPanels; ++i)
            {
                m_targetContentPanels[i].gameObject.SetActive(m_insertedElements[i].Count > 0);
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);

            for (var i = 0; i < m_reEnable.Length; ++i)
            {
                if (m_toDisable[i] && m_reEnable[i])
                {
                    m_toDisable[i].SetActive(true);
                }
            }

            var len = m_toEnable.Count;
            for (var i = 0; i < len; ++i)
            {
                m_toEnable[i].SetActive(false);
            }
        }

        // Currently a slow brute-force method that lays out every element.
        // As this is intended as a debug UI, it might be fine, but there are many simple optimizations we can make.
        private void StackedRelayout()
        {
            for (var panelIdx = 0; panelIdx < m_targetContentPanels.Length; ++panelIdx)
            {
                var canvasRect = m_targetContentPanels[panelIdx].GetComponent<RectTransform>();
                var elems = m_insertedElements[panelIdx];
                var elemCount = elems.Count;
                var x = MarginH;
                var y = -MarginV;
                var maxWidth = 0.0f;
                for (var elemIdx = 0; elemIdx < elemCount; ++elemIdx)
                {
                    var r = elems[elemIdx];
                    r.anchoredPosition = new Vector2(x, y);

                    if (IsHorizontal)
                    {
                        x += r.rect.width + ElementSpacing;
                    }
                    else
                    {
                        y -= r.rect.height + ElementSpacing;
                    }

                    maxWidth = Mathf.Max(r.rect.width + 2 * MarginH, maxWidth);
                }

                canvasRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth);
                canvasRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, -y + MarginV);
            }
        }

        private void PanelCentricRelayout()
        {
            if (!IsHorizontal)
            {
                Debug.Log("Error:Panel Centeric relayout is implemented only for horizontal panels");
                return;
            }

            for (var panelIdx = 0; panelIdx < m_targetContentPanels.Length; ++panelIdx)
            {
                var canvasRect = m_targetContentPanels[panelIdx].GetComponent<RectTransform>();
                var elems = m_insertedElements[panelIdx];
                var elemCount = elems.Count;
                var x = MarginH;
                _ = -MarginV;
                var maxWidth = x;
                for (var elemIdx = 0; elemIdx < elemCount; ++elemIdx)
                {
                    var r = elems[elemIdx];
                    maxWidth += r.rect.width + ElementSpacing;
                }

                maxWidth -= ElementSpacing;
                maxWidth += MarginH;
                var totalmaxWidth = maxWidth;
                x = -0.5f * totalmaxWidth;
                var y = -MarginV;
                //Offset the UI  elements half of total lenght of the panel.
                for (var elemIdx = 0; elemIdx < elemCount; ++elemIdx)
                {
                    var r = elems[elemIdx];
                    if (elemIdx == 0)
                    {
                        x += MarginH;
                    }

                    x += 0.5f * r.rect.width;
                    r.anchoredPosition = new Vector2(x, y);
                    x += r.rect.width * 0.5f + ElementSpacing;
                    maxWidth = Mathf.Max(r.rect.width + 2 * MarginH, maxWidth);
                }

                canvasRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth);
                canvasRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, -y + MarginV);
            }
        }

        private void Relayout()
        {
            if (UsePanelCentricRelayout)
            {
                PanelCentricRelayout();
            }
            else
            {
                StackedRelayout();
            }
        }

        private void AddRect(RectTransform r, int targetCanvas)
        {
            if (targetCanvas > m_targetContentPanels.Length)
            {
                Debug.LogError("Attempted to add debug panel to canvas " + targetCanvas + ", but only " +
                            m_targetContentPanels.Length +
                            " panels were provided. Fix in the inspector or pass a lower value for target canvas.");
                return;
            }

            r.transform.SetParent(m_targetContentPanels[targetCanvas], false);
            m_insertedElements[targetCanvas].Add(r);
            if (gameObject.activeInHierarchy)
            {
                Relayout();
            }
        }

        public RectTransform AddButton(string label, OnClick handler = null, int buttonIndex = -1, int targetCanvas = 0,
            bool highResolutionText = false)
        {
            var buttonRT = (buttonIndex  == -1)
                ? Instantiate(m_buttonPrefab).GetComponent<RectTransform>()
                : Instantiate(m_additionalButtonPrefab[buttonIndex]).GetComponent<RectTransform>();
            var button = buttonRT.GetComponentInChildren<Button>();
            if (handler != null)
                button.onClick.AddListener(delegate { handler(); });


            if (highResolutionText)
            {
                ((TextMeshProUGUI)buttonRT.GetComponentsInChildren(typeof(TextMeshProUGUI), true)[0]).text = label;
            }
            else
            {
                ((Text)buttonRT.GetComponentsInChildren(typeof(Text), true)[0]).text = label;
            }

            AddRect(buttonRT, targetCanvas);
            return buttonRT;
        }

        public RectTransform AddLabel(string label, int targetCanvas = 0, int fontSize = 40, Color color = default)
        {
            color = color == default ? Color.black : color;
            var rt = Instantiate(m_labelPrefab).GetComponent<RectTransform>();
            var textComponent = rt.GetComponent<Text>();
            textComponent.text = label;
            textComponent.fontSize = fontSize;
            textComponent.color = color;
            AddRect(rt, targetCanvas);
            return rt;
        }

        public RectTransform AddParagraph(string text, int targetCanvas = 0, int fontSize = 40)
        {
            var rt = Instantiate(m_labelPrefab).GetComponent<RectTransform>();
            var textComponent = rt.GetComponent<Text>();
            textComponent.text = text;
            textComponent.fontSize = fontSize;
            textComponent.color = Color.black; // Set text color to black
            textComponent.horizontalOverflow = HorizontalWrapMode.Wrap; // Enable text wrapping
            textComponent.verticalOverflow = VerticalWrapMode.Overflow; // Allow vertical overflow

            // Adjust the size of the RectTransform to fit the text
            var preferredHeight = textComponent.preferredHeight + MarginV * 2;
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight);

            AddRect(rt, targetCanvas);
            return rt;
        }


        public RectTransform AddSlider(string label, float min, float max, OnSlider onValueChanged,
            bool wholeNumbersOnly = false, int targetCanvas = 0)
        {
            var rt = Instantiate(m_sliderPrefab);
            var s = rt.GetComponentInChildren<Slider>();
            s.minValue = min;
            s.maxValue = max;
            s.onValueChanged.AddListener(delegate (float f) { onValueChanged(f); });
            s.wholeNumbers = wholeNumbersOnly;
            AddRect(rt, targetCanvas);
            return rt;
        }

        public RectTransform AddDivider(int targetCanvas = 0)
        {
            var rt = Instantiate(m_dividerPrefab);
            AddRect(rt, targetCanvas);
            return rt;
        }

        public RectTransform AddToggle(string label, OnToggleValueChange onValueChanged, int targetCanvas = 0)
        {
            var rt = Instantiate(m_togglePrefab);
            AddRect(rt, targetCanvas);
            var buttonText = rt.GetComponentInChildren<Text>();
            buttonText.text = label;
            var t = rt.GetComponentInChildren<Toggle>();
            t.onValueChanged.AddListener(delegate { onValueChanged(t); });
            return rt;
        }

        public RectTransform AddToggle(string label, OnToggleValueChange onValueChanged, bool defaultValue,
            int targetCanvas = 0)
        {
            var rt = Instantiate(m_togglePrefab);
            AddRect(rt, targetCanvas);
            var buttonText = rt.GetComponentInChildren<Text>();
            buttonText.text = label;
            var t = rt.GetComponentInChildren<Toggle>();
            t.isOn = defaultValue;
            t.onValueChanged.AddListener(delegate { onValueChanged(t); });
            return rt;
        }

        public RectTransform AddRadio(string label, string group, OnToggleValueChange handler, int targetCanvas = 0)
        {
            var rt = Instantiate(m_radioPrefab);
            AddRect(rt, targetCanvas);
            var buttonText = rt.GetComponentInChildren<Text>();
            buttonText.text = label;
            var tb = rt.GetComponentInChildren<Toggle>();
            group ??= "default";
            ToggleGroup tg = null;
            var isFirst = false;
            if (!m_radioGroups.ContainsKey(group))
            {
                tg = tb.gameObject.AddComponent<ToggleGroup>();
                m_radioGroups[group] = tg;
                isFirst = true;
            }
            else
            {
                tg = m_radioGroups[group];
            }

            tb.group = tg;
            tb.isOn = isFirst;
            tb.onValueChanged.AddListener(delegate { handler(tb); });
            return rt;
        }

        public RectTransform AddTextField(string label, int targetCanvas = 0)
        {
            var textRT = Instantiate(m_textPrefab).GetComponent<RectTransform>();
            var inputField = textRT.GetComponentInChildren<InputField>();
            inputField.text = label;
            AddRect(textRT, targetCanvas);
            return textRT;
        }


        public RectTransform AddAppLogo(int targetCanvas = 0)
        {
            var rt = Instantiate(m_ConcordiaPrefab).GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1); // Align to top-center
            rt.anchorMax = new Vector2(0.5f, 1);
            rt.pivot = new Vector2(0.5f, 1); // Set pivot to top-center
            AddRect(rt, targetCanvas);
            return rt;
        }


        public void LoadImage(string imageName, int targetPane,  int maxDisplayWidth = 450)
        {
            // Load from Resources folder
            Sprite sprite = DebugUIBuilder.Instance.LoadSpriteFromResources(imageName);
            if (sprite != null)
            {
                AddImage(sprite, targetPane, maxDisplayWidth);
            }
            else
            {
                Debug.LogError($"Failed to load image: {imageName}");
                // Add a placeholder or error message
                _ = AddLabel("[Image Not Found]", targetPane);
            }

        }
                

        public RectTransform AddImage(Sprite sprite, int targetCanvas = 0, int maxDisplayWidth = 450)
        {
            var imageObject = new GameObject("DebugUIImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var image = imageObject.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;

            var rt = imageObject.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1); // Align to top-center
            rt.anchorMax = new Vector2(0.5f, 1);
            rt.pivot = new Vector2(0.5f, 1); // Set pivot to top-center

            // Compute a smaller display size while preserving aspect ratio.
            var nativeW = sprite.rect.width;
            var nativeH = sprite.rect.height;
            float displayWidth = Mathf.Min(nativeW, maxDisplayWidth);
            float displayHeight = nativeH * (displayWidth / nativeW);

            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, displayWidth);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, displayHeight);

            AddRect(rt, targetCanvas);

            return rt;
        }
     
        public RectTransform AddComponentImage(Sprite sprite, int targetCanvas = 0, Action onClick = null)
        {
            var imageObject = new GameObject("DebugUIImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var image = imageObject.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;

            var button = imageObject.GetComponent<Button>();
            if (onClick != null)
            {
                button.onClick.AddListener(() =>
                {
                    onClick();
                });
            }

            var rt = imageObject.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1); // Align to top-center
            rt.anchorMax = new Vector2(0.5f, 1);
            rt.pivot = new Vector2(0.5f, 1); // Set pivot to top-center

            AddRect(rt, targetCanvas);

            return rt;
        }
        
        public Sprite LoadSpriteFromResources(string imageName)
        {
            // Remove file extensions if present
            string resourcePath = imageName;
            if (resourcePath.Contains("."))
            {
                resourcePath = resourcePath.Substring(0, resourcePath.LastIndexOf('.'));
            }
            
            // Try to load from Resources folder
            Texture2D texture = Resources.Load<Texture2D>(resourcePath);
            if (texture != null)
            {
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
            
            Debug.LogError($"Could not load image from Resources: {resourcePath}");
            return null;
        }
        

        public GameObject Add3DCube(Vector3 position, Vector3 scale, Color color, int targetCanvas = 0)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = position;
            cube.transform.localScale = scale;

            var renderer = cube.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
            }

            // Parent the cube to the target canvas if applicable
            if (targetCanvas < m_targetContentPanels.Length)
            {
                cube.transform.SetParent(m_targetContentPanels[targetCanvas], false);
            }

            return cube;
        }

        public void ToggleLaserPointer(bool isOn)
        {
            if (m_lp)
            {
                m_lp.enabled = isOn;
            }
        }

        
    }
}
