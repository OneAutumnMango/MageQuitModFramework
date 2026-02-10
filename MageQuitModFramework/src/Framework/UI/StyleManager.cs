using UnityEngine;
using System;

namespace MageQuitModFramework.UI
{
    public static class StyleManager
    {
        private static GUIStyle _green, _red;
        private static GUIStyle _white, _purple, _gold;
        private static bool _initialized = false;

        public static Color BackgroundColor => new Color(0.1f, 0.1f, 0.1f, 0.9f);
        public static Color PanelColor => new Color(0.2f, 0.2f, 0.2f, 0.95f);

        public static GUIStyle Green
        {
            get
            {
                EnsureInitialized();
                return _green;
            }
        }

        public static GUIStyle Red
        {
            get
            {
                EnsureInitialized();
                return _red;
            }
        }

        public static GUIStyle White
        {
            get
            {
                EnsureInitialized();
                return _white;
            }
        }

        public static GUIStyle Purple
        {
            get
            {
                EnsureInitialized();
                return _purple;
            }
        }

        public static GUIStyle Gold
        {
            get
            {
                EnsureInitialized();
                return _gold;
            }
        }

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
