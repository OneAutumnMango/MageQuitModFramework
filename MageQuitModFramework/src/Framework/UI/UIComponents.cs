using UnityEngine;
using System;
using MageQuitModFramework.Loading;

namespace MageQuitModFramework.UI
{
    public static class UIComponents
    {
        public static bool DrawModuleToggle(string modName, string moduleName, ModuleManager moduleManager, float buttonWidth = 200)
        {
            bool isLoaded = moduleManager.IsModuleLoaded(moduleName);
            string buttonText = isLoaded ? $"Unload {moduleName}" : $"Load {moduleName}";

            if (GUILayout.Button(buttonText, GUILayout.Width(buttonWidth)))
            {
                if (isLoaded)
                    moduleManager.UnloadModule(moduleName);
                else
                    moduleManager.LoadModule(moduleName);
                return true;
            }
            return false;
        }

        public static void Section(string title, Action content)
        {
            GUILayout.Label(title);
            GUILayout.Space(10);
            content?.Invoke();
            GUILayout.Space(10);
        }

        public static (string value, bool clicked) TextFieldWithButton(string label, string currentValue, string buttonText, out bool buttonClicked, float textWidth = 200, float buttonWidth = 100)
        {
            if (!string.IsNullOrEmpty(label))
                GUILayout.Label(label);
            
            GUILayout.BeginHorizontal();
            string result = GUILayout.TextField(currentValue, GUILayout.Width(textWidth));
            buttonClicked = GUILayout.Button(buttonText, GUILayout.Width(buttonWidth));
            GUILayout.EndHorizontal();
            
            return (result, buttonClicked);
        }

        public static bool Button(string text, float width = 200, GUIStyle style = null)
        {
            return style == null 
                ? GUILayout.Button(text, GUILayout.Width(width))
                : GUILayout.Button(text, style, GUILayout.Width(width));
        }

        public static void Label(string text, GUIStyle style = null)
        {
            if (style == null)
                GUILayout.Label(text);
            else
                GUILayout.Label(text, style);
        }

        public static string TextField(string text, float width = 200)
        {
            return GUILayout.TextField(text, GUILayout.Width(width));
        }

        public static void Space(float pixels = 10)
        {
            GUILayout.Space(pixels);
        }

        public static void BeginHorizontal()
        {
            GUILayout.BeginHorizontal();
        }

        public static void EndHorizontal()
        {
            GUILayout.EndHorizontal();
        }

        public static void BeginVertical()
        {
            GUILayout.BeginVertical();
        }

        public static void EndVertical()
        {
            GUILayout.EndVertical();
        }

        public static void Box(string text, float width, float height)
        {
            GUILayout.Box(text, GUILayout.Width(width), GUILayout.Height(height));
        }
    }
}
