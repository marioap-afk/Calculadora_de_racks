using System;
using System.Windows;

namespace RackCad.UI.Shell
{
    /// <summary>
    /// Lazy, single-load access to the shared <c>Themes/AppStyles.xaml</c> dictionary from code, so shell pieces
    /// created OUTSIDE a window's visual tree (a standalone action button, a test fixture) still resolve the shared
    /// styles and tokens. AppStyles.xaml is a compiled resource of RackCad.UI, so the pack URI always resolves at
    /// runtime. <see cref="Get{T}"/> returns a fallback when a key is ABSENT (an optional lookup); <see cref="Require{T}"/>
    /// throws on an absent/mistyped key. Neither hides a real dictionary-LOAD failure: that exception propagates.
    /// </summary>
    internal static class ShellResources
    {
        private static readonly object Gate = new object();
        private static ResourceDictionary dictionary;

        private static ResourceDictionary Dictionary
        {
            get
            {
                if (dictionary != null)
                {
                    return dictionary;
                }

                lock (Gate)
                {
                    if (dictionary == null)
                    {
                        dictionary = new ResourceDictionary
                        {
                            Source = new Uri("/RackCad.UI;component/Themes/AppStyles.xaml", UriKind.Relative)
                        };
                    }
                }

                return dictionary;
            }
        }

        /// <summary>
        /// The shared dictionary itself, so a lookless shell can merge it into its OWN resources and make the
        /// <c>DynamicResource</c> tokens of its template resolve for EVERY consumer.
        ///
        /// <para>A <c>DynamicResource</c> with a plain string key searches the element tree and then
        /// <c>Application.Resources</c>; it does NOT fall back to the assembly theme dictionary, even though that is
        /// where the template itself came from. So a shell whose template says
        /// <c>Margin="{DynamicResource ShellZoneSpacing}"</c> renders with a ZERO margin unless the consuming window
        /// happens to have merged AppStyles — which a window built in code has no reason to do. Merging here removes
        /// that dependency on the consumer. All six styles of AppStyles are keyed, so nothing implicit leaks into a
        /// consumer's tree.</para>
        /// </summary>
        public static ResourceDictionary Shared => Dictionary;

        /// <summary>The shared resource for <paramref name="key"/> cast to <typeparamref name="T"/>, or
        /// <paramref name="fallback"/> when the key is ABSENT or of another type. A real dictionary-load failure is NOT
        /// swallowed — it propagates, so a broken AppStyles surfaces instead of every lookup silently returning defaults.</summary>
        public static T Get<T>(string key, T fallback = default)
        {
            var dict = Dictionary; // a genuine load failure propagates rather than being hidden behind the fallback
            return dict.Contains(key) && dict[key] is T value ? value : fallback;
        }

        /// <summary>The shared resource for <paramref name="key"/> cast to <typeparamref name="T"/>. Unlike
        /// <see cref="Get{T}"/> this substitutes NO silent default: an absent key, a wrong type, or a dictionary that
        /// fails to load all throw, so a broken normative token (e.g. a <c>ShellStatus*Brush</c>) surfaces immediately.</summary>
        public static T Require<T>(string key)
        {
            var dict = Dictionary; // a genuine load failure propagates
            if (!dict.Contains(key))
            {
                throw new InvalidOperationException(
                    $"Required shell resource '{key}' is missing from Themes/AppStyles.xaml.");
            }

            if (dict[key] is T value)
            {
                return value;
            }

            throw new InvalidOperationException(
                $"Shell resource '{key}' in Themes/AppStyles.xaml is '{dict[key]?.GetType().Name ?? "null"}', expected '{typeof(T).Name}'.");
        }
    }
}
