using UnityEngine;
using UnityEngine.UI;
using System;
using MageQuitModFramework.Loading;

namespace MageQuitModFramework.UI
{
    public static class UIComponents
    {
        public static GameObject CreatePanel(Transform parent, string name, float width, float height)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            var rect = panel.GetComponent<RectTransform>() ?? panel.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, height);
            var image = panel.AddComponent<Image>();
            image.color = StyleManager.PanelColor;
            return panel;
        }

        public static Transform CreateScrollView(Transform parent, float width, float height)
        {
            var scrollView = new GameObject("ScrollView");
            scrollView.transform.SetParent(parent, false);
            var scrollRect = scrollView.AddComponent<ScrollRect>();
            var rect = scrollView.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, height);
            
            var viewport = CreatePanel(scrollView.transform, "Viewport", width, height);
            var content = CreatePanel(viewport.transform, "Content", width, height);
            
            scrollRect.content = content.GetComponent<RectTransform>();
            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            
            return scrollView.transform;
        }

        public static Text CreateText(Transform parent, string name, string text, int fontSize)
        {
            var textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);
            var rect = textObj.GetComponent<RectTransform>() ?? textObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300, 30);
            
            var textComponent = textObj.AddComponent<Text>();
            textComponent.text = text;
            textComponent.fontSize = fontSize;
            textComponent.color = Color.white;
            textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            textComponent.alignment = TextAnchor.MiddleLeft;
            
            return textComponent;
        }

        public static Button CreateModuleToggleButton(Transform parent, string moduleName, float width = 200, float height = 40)
        {
            var buttonObj = new GameObject($"{moduleName}LoadButton");
            buttonObj.transform.SetParent(parent, false);
            var rectTransform = buttonObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(width, height);
            
            var button = buttonObj.AddComponent<Button>();
            var buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = StyleManager.PanelColor;
            
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            var text = textObj.AddComponent<Text>();
            text.fontSize = 14;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.alignment = TextAnchor.MiddleCenter;
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            void UpdateButtonText()
            {
                text.text = ModuleManager.IsModuleLoaded(moduleName) ? $"Unload {moduleName}" : $"Load {moduleName}";
            }
            
            UpdateButtonText();
            
            button.onClick.AddListener(() =>
            {
                if (ModuleManager.IsModuleLoaded(moduleName))
                    ModuleManager.UnloadModule(moduleName);
                else
                    ModuleManager.LoadModule(moduleName);
                UpdateButtonText();
            });
            
            return button;
        }

        public static InputField CreateInputField(Transform parent, string name, string placeholder, float width = 200, float height = 40)
        {
            var fieldObj = new GameObject(name);
            fieldObj.transform.SetParent(parent, false);
            var rectTransform = fieldObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(width, height);
            
            var image = fieldObj.AddComponent<Image>();
            image.color = StyleManager.PanelColor;
            
            var inputField = fieldObj.AddComponent<InputField>();
            
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(fieldObj.transform, false);
            var text = textObj.AddComponent<Text>();
            text.fontSize = 14;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.supportRichText = false;
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.05f, 0);
            textRect.anchorMax = new Vector2(0.95f, 1);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            var placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(fieldObj.transform, false);
            var placeholderText = placeholderObj.AddComponent<Text>();
            placeholderText.text = placeholder;
            placeholderText.fontSize = 14;
            placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            placeholderText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            placeholderText.fontStyle = FontStyle.Italic;
            var placeholderRect = placeholderObj.GetComponent<RectTransform>();
            placeholderRect.anchorMin = new Vector2(0.05f, 0);
            placeholderRect.anchorMax = new Vector2(0.95f, 1);
            placeholderRect.offsetMin = Vector2.zero;
            placeholderRect.offsetMax = Vector2.zero;
            
            inputField.textComponent = text;
            inputField.placeholder = placeholderText;
            
            return inputField;
        }

        public static Button CreateButton(Transform parent, string name, string buttonText, float width = 200, float height = 40)
        {
            var buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);
            var rectTransform = buttonObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(width, height);
            
            var button = buttonObj.AddComponent<Button>();
            var buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = StyleManager.PanelColor;
            
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            var text = textObj.AddComponent<Text>();
            text.text = buttonText;
            text.fontSize = 14;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.alignment = TextAnchor.MiddleCenter;
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            return button;
        }
    }


    public class ModToggleButton
    {
        public string LoadedText { get; set; }
        public string UnloadedText { get; set; }
        public Rect Position { get; set; }
        public bool IsLoaded { get; set; }

        public event Action OnLoad;
        public event Action OnUnload;

        public ModToggleButton(string loadedText, string unloadedText, Rect position)
        {
            LoadedText = loadedText;
            UnloadedText = unloadedText;
            Position = position;
            IsLoaded = false;
        }

        public void Draw()
        {
            string buttonText = IsLoaded ? LoadedText : UnloadedText;

            if (GUI.Button(Position, buttonText))
            {
                if (IsLoaded)
                    OnUnload?.Invoke();
                else
                    OnLoad?.Invoke();
            }
        }
    }

    public class ConditionalMessage
    {
        public Func<bool> Condition { get; set; }
        public string TrueMessage { get; set; }
        public string FalseMessage { get; set; }
        public GUIStyle TrueStyle { get; set; }
        public GUIStyle FalseStyle { get; set; }
        public Rect Position { get; set; }

        public void Draw()
        {
            bool condition = Condition?.Invoke() ?? false;
            string message = condition ? TrueMessage : FalseMessage;
            GUIStyle style = condition ? TrueStyle : FalseStyle;

            GUI.Label(Position, message, style);
        }
    }
}
