using UnityEngine;
using System;

namespace SpellcastModFramework.UI
{
    public static class StyleManager
    {
        private static GUIStyle _green, _red;
        private static GUIStyle _commonStyle, _rareStyle, _legendaryStyle;
        private static bool _initialized = false;

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

        public static GUIStyle CommonStyle
        {
            get
            {
                EnsureInitialized();
                return _commonStyle;
            }
        }

        public static GUIStyle RareStyle
        {
            get
            {
                EnsureInitialized();
                return _rareStyle;
            }
        }

        public static GUIStyle LegendaryStyle
        {
            get
            {
                EnsureInitialized();
                return _legendaryStyle;
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

            _commonStyle = new GUIStyle(GUI.skin.label);

            _rareStyle = new GUIStyle(GUI.skin.label);
            _rareStyle.normal.textColor = new Color(0.75f, 0.4f, 0.9f);

            _legendaryStyle = new GUIStyle(GUI.skin.label);
            _legendaryStyle.normal.textColor = new Color(1.0f, 0.82f, 0.2f);

            _initialized = true;
        }

        public static GUIStyle GetTierStyle(Framework.Modifiers.Tier tier)
        {
            EnsureInitialized();

            if (tier.Equals(Framework.Modifiers.TierRegistry.Legendary))
                return _legendaryStyle;

            if (tier.Equals(Framework.Modifiers.TierRegistry.Rare))
                return _rareStyle;

            return _commonStyle;
        }
    }
}
