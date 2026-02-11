using UnityEngine;
using System;

namespace MageQuitModFramework.UI
{
    /// <summary>
    /// Provides pre-configured IMGUI styles and colors for consistent UI appearance across mods.
    /// </summary>
    public static class StyleManager
    {
        private static GUIStyle _green, _red;
        private static GUIStyle _white, _purple, _gold;
        private static bool _initialized = false;

        /// <summary>Dark background color for UI panels.</summary>
        public static Color BackgroundColor => new Color(0.1f, 0.1f, 0.1f, 0.9f);
        
        /// <summary>Slightly lighter panel color for nested containers.</summary>
        public static Color PanelColor => new Color(0.2f, 0.2f, 0.2f, 0.95f);

        /// <summary>Green colored button style for positive actions.</summary>
        public static GUIStyle Green
        {
            get
            {
                EnsureInitialized();
                return _green;
            }
        }

        /// <summary>Red colored button style for negative actions or warnings.</summary>
        public static GUIStyle Red
        {
            get
            {
                EnsureInitialized();
                return _red;
            }
        }

        /// <summary>White colored button style for neutral actions.</summary>
        public static GUIStyle White
        {
            get
            {
                EnsureInitialized();
                return _white;
            }
        }

        /// <summary>Purple colored button style for special actions.</summary>
        public static GUIStyle Purple
        {
            get
            {
                EnsureInitialized();
                return _purple;
            }
        }

        /// <summary>Gold colored button style for important or premium actions.</summary>
        public static GUIStyle Gold
        {
            get
            {
                EnsureInitialized();
                return _gold;
            }
        }

        /// <summary>
        /// Ensures all GUIStyles are initialized before use.
        /// Called automatically when accessing style properties.
        /// </summary>
        public static void EnsureInitialized()
        {
            if (_initialized) return;

            Color upColor = new(0.3f, 0.85f, 0.3f);
            Color downColor = new(0.9f, 0.3f, 0.3f);

            _green = new GUIStyle(GUI.skin.button);
            _green.normal.textColor = upColor;
            _green.hover.textColor = upColor;
            _green.active.textColor = upColor;
            _green.focused.textColor = upColor;

            _red = new GUIStyle(GUI.skin.button);
            _red.normal.textColor = downColor;
            _red.hover.textColor = downColor;
            _red.active.textColor = downColor;
            _red.focused.textColor = downColor;

            _white = new GUIStyle(GUI.skin.label);

            _purple = new GUIStyle(GUI.skin.label);
            _purple.normal.textColor = new Color(0.75f, 0.4f, 0.9f);

            _gold = new GUIStyle(GUI.skin.label);
            _gold.normal.textColor = new Color(1.0f, 0.82f, 0.2f);

            _initialized = true;
        }
    }
}
