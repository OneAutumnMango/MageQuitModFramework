using UnityEngine;
using System;
using MageQuitModFramework.Modding;

namespace MageQuitModFramework.UI
{
    /// <summary>
    /// Reusable IMGUI components for building mod UI interfaces.
    /// Provides styled buttons, labels, text fields, and layout helpers.
    /// </summary>
    public static class UIComponents
    {
        /// <summary>
        /// Draws a toggle button for loading/unloading a module.
        /// </summary>
        /// <param name="modName">The name of the mod owning the module</param>
        /// <param name="moduleName">The name of the module to toggle</param>
        /// <param name="moduleManager">The module manager controlling the module</param>
        /// <param name="buttonWidth">Width of the button in pixels (default 300)</param>
        /// <returns>True if the button was clicked and module state changed, false otherwise</returns>
        public static bool DrawModuleToggle(string modName, string moduleName, ModuleManager moduleManager, float buttonWidth = 300)
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

        /// <summary>
        /// Creates a labeled section with optional content callback.
        /// </summary>
        /// <param name="title">The section title text</param>
        /// <param name="content">Optional callback to render section content</param>
        public static void Section(string title, Action content)
        {
            GUILayout.Label(title);
            GUILayout.Space(10);
            content?.Invoke();
            GUILayout.Space(10);
        }

        /// <summary>
        /// Draws a text field with an adjacent button.
        /// </summary>
        /// <param name="label">Optional label displayed above the field</param>
        /// <param name="currentValue">Current text value of the field</param>
        /// <param name="buttonText">Text displayed on the button</param>
        /// <param name="buttonClicked">Output parameter indicating if button was clicked</param>
        /// <param name="textWidth">Width of the text field in pixels (default 200)</param>
        /// <param name="buttonWidth">Width of the button in pixels (default 100)</param>
        /// <returns>Tuple containing the updated text value and button click state</returns>
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

        /// <summary>
        /// Draws a button with optional custom style.
        /// </summary>
        /// <param name="text">Button label text</param>
        /// <param name="width">Button width in pixels (default 200)</param>
        /// <param name="style">Optional custom GUIStyle (uses default if null)</param>
        /// <returns>True if button was clicked, false otherwise</returns>
        public static bool Button(string text, float width = 200, GUIStyle style = null)
        {
            return style == null
                ? GUILayout.Button(text, GUILayout.Width(width))
                : GUILayout.Button(text, style, GUILayout.Width(width));
        }

        /// <summary>
        /// Draws a text label with optional custom style.
        /// </summary>
        /// <param name="text">Label text content</param>
        /// <param name="style">Optional custom GUIStyle (uses default if null)</param>
        public static void Label(string text, GUIStyle style = null)
        {
            if (style == null)
                GUILayout.Label(text);
            else
                GUILayout.Label(text, style);
        }

        /// <summary>
        /// Draws a text input field.
        /// </summary>
        /// <param name="text">Current text value</param>
        /// <param name="width">Field width in pixels (default 200)</param>
        /// <returns>Updated text value</returns>
        public static string TextField(string text, float width = 200)
        {
            return GUILayout.TextField(text, GUILayout.Width(width));
        }

        /// <summary>
        /// Draws a toggle with optional label in a single horizontal row.
        /// </summary>
        /// <param name="label">Label text shown to the left of the toggle</param>
        /// <param name="value">Current toggle value</param>
        /// <param name="labelWidth">Label width in pixels (default 200)</param>
        /// <returns>Updated toggle value</returns>
        public static bool Toggle(string label, bool value, float labelWidth = 200)
        {
            GUILayout.BeginHorizontal();
            if (!string.IsNullOrEmpty(label))
                GUILayout.Label(label, GUILayout.Width(labelWidth));
            bool result = GUILayout.Toggle(value, "");
            GUILayout.EndHorizontal();
            return result;
        }

        /// <summary>
        /// Adds vertical spacing in the layout.
        /// </summary>
        /// <param name="pixels">Amount of vertical space in pixels (default 10)</param>
        public static void Space(float pixels = 10)
        {
            GUILayout.Space(pixels);
        }

        /// <summary>
        /// Begins a horizontal layout group.
        /// </summary>
        public static void BeginHorizontal()
        {
            GUILayout.BeginHorizontal();
        }

        /// <summary>
        /// Ends the current horizontal layout group.
        /// </summary>
        public static void EndHorizontal()
        {
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Begins a vertical layout group.
        /// </summary>
        public static void BeginVertical()
        {
            GUILayout.BeginVertical();
        }

        /// <summary>
        /// Ends the current vertical layout group.
        /// </summary>
        public static void EndVertical()
        {
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Draws a box with specified dimensions.
        /// </summary>
        /// <param name="text">Text content for the box</param>
        /// <param name="width">Width in pixels</param>
        /// <param name="height">Height in pixels</param>
        public static void Box(string text, float width, float height)
        {
            GUILayout.Box(text, GUILayout.Width(width), GUILayout.Height(height));
        }
    }
}
