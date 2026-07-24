using System;
using System.Windows;

namespace RackCad.UI.Shell
{
    /// <summary>
    /// Lazy, single-load access to the shared <c>Themes/AppStyles.xaml</c> dictionary from code, so shell pieces
    /// created OUTSIDE a window's visual tree (a standalone action button, a test fixture) still resolve the shared
    /// styles and tokens. AppStyles.xaml is a compiled resource of RackCad.UI, so the pack URI always resolves at
    /// runtime; a lookup miss returns the fallback instead of throwing.
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

        /// <summary>The shared resource for <paramref name="key"/> cast to <typeparamref name="T"/>, or
        /// <paramref name="fallback"/> when it is absent or of another type. Never throws.</summary>
        public static T Get<T>(string key, T fallback = default)
        {
            try
            {
                return Dictionary.Contains(key) && Dictionary[key] is T value ? value : fallback;
            }
            catch
            {
                return fallback;
            }
        }
    }
}
