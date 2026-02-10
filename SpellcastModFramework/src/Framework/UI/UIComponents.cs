using UnityEngine;
using UnityEngine.UI;
using System;

namespace SpellcastModFramework.UI
{
    public static class UIComponents
    {
        public static GameObject CreatePanel(Transform parent, string name, float width, float height)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            var rect = panel.AddComponent<RectTransform>();
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
