using UnityEngine;
using UnityEngine.Localization.Settings;

namespace ContentWarningShop.Localisation
{
    /// <summary>
    /// Contains the locale identifiers that are supported by the game and guaranteed to resolve.
    /// </summary>
    public static class LocaleKeys
    {
        public const string English = "en";
        public const string Swedish = "sv";
        public const string French = "fr";
        public const string German = "de";
        public const string Italian = "it";
        public const string Portuguese = "pt-br";
        public const string Spanish = "es";
        public const string Ukrainian = "uk";
        public const string ChineseSimplified = "zh-hans";
        public const string ChineseTraditional = "zh-hant";
        public const string Japanese = "ja";
        public const string Korean = "ko";
        public const string Russian = "ru";
    }
    public static class ShopLocalisation
    {
        private static readonly Dictionary<UnityEngine.Localization.Locale, Dictionary<string, string>> _localeStrings = new();

        public const string TooltipsSuffix = "_ToolTips";
        /// <summary>
        /// Represents the Left Mouse Button glyph.
        /// </summary>
        public const string UseGlyph = "{key_use}";
        /// <summary>
        /// Represents the Right Mouse Button glyph.
        /// </summary>
        public const string Use2Glyph = "{key_use2}";
        /// <summary>
        /// Represents the R key glyph (default, can be rebound by the player).
        /// </summary>
        public const string SelfieGlyph = "{key_selfie}";
        /// <summary>
        /// Represents the Mouse Wheel glyph.
        /// </summary>
        public const string ZoomGlyph = "{key_zoom}";

        static ShopLocalisation()
        {
            foreach (var loc in LocalizationSettings.AvailableLocales.Locales)
            {
                if (_localeStrings.ContainsKey(loc) == false)
                {
                    _localeStrings.Add(loc, new Dictionary<string, string>());
                }
            }
            Debug.Log($"ShopLocalisation loaded {_localeStrings.Count} locales.");
        }

        /// <summary>
        /// Gets the currently used locale.
        /// </summary>
        /// <returns></returns>
        public static UnityEngine.Localization.Locale GetCurrentLocale()
        {
            return LocalizationSettings.SelectedLocale;
        }

        /// <summary>
        /// Gets a locale with the specified name. See <see cref="LocaleKeys"/> for locale keys that are guaranteed to exist.
        /// </summary>
        /// <param name="locId"></param>
        /// <param name="locale"></param>
        /// <returns><see langword="true"/> if a locale with the given name was found; otherwise <see langword="false"/>.</returns>
        public static bool TryGetLocale(string locId, out UnityEngine.Localization.Locale locale)
        {
            UnityEngine.Localization.Locale loc = null;
            foreach (var sLoc in _localeStrings.Keys)
            {
                if (sLoc.LocaleName.ToLower().Contains(locId))
                {
                    loc = sLoc;
                    break;
                }
            }
            locale = loc;
            return loc != null;
        }

        /// <summary>
        /// Registers a localised string with the given key to the selected locale.
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="key"></param>
        /// <param name="str"></param>
        public static void AddLocaleString(this UnityEngine.Localization.Locale loc, string key, string str)
        {
            if (_localeStrings.ContainsKey(loc) == false)
            {
                return;
            }
            if (_localeStrings[loc].ContainsKey(key) == false)
            {
                _localeStrings[loc].Add(key, str);
            }
            else
            {
                _localeStrings[loc][key] = str;
            }
        }

        /// <summary>
        /// Gets the localised string associated with the given key based on the current locale.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="res"></param>
        /// <returns><see langword="true"/> if the key was found; otherwise <see langword="false"/>.</returns>
        public static bool TryGetLocaleString(string key, out string res)
        {
            var currLoc = GetCurrentLocale();
            var found = _localeStrings[currLoc].TryGetValue(key, out string str);
            res = str;
            return found;
        }
    }
}
