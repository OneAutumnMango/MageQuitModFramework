using UnityEngine;
using System;

namespace SpellcastModFramework.UI
{
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
