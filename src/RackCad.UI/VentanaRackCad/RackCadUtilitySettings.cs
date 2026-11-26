using System;
using System.IO;
using System.Reflection;
using RackCad.Application.Catalogs;
using RackCad.Application.Diagnostics;
using RackCad.Application.Settings;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// «Acerca de RackCad» (I-64 A-14, fourth batch, part «ut», UT-12): read only, for support — the version of the plugin, the folder of
    /// the catalogs it reads and the folder of the log of errors.
    /// </summary>
    public sealed record RackCadAboutInfo(string Version, string CatalogsFolder, string LogsFolder);

    /// <summary>
    /// What «Configuración» of the window changes beyond the block library (part «ut»): the folder of the design library (UT-02, «Cambiar…»
    /// and «Restablecer»), «Abrir RackCad al iniciar AutoCAD» (UT-01) and the facts of «Acerca de RackCad» (UT-12). An interface of its own,
    /// next to <see cref="IRackCadSettingsAccess"/>, so a settings access that does not have it leaves those controls disabled.
    /// </summary>
    public interface IRackCadUtilitySettings
    {
        /// <summary>True when the folder of the design library was chosen by the user and saved.</summary>
        bool DesignLibraryOverridden { get; }

        /// <summary>Saves a chosen folder of the design library; null goes back to the default one (%AppData%\RackCad\Designs).</summary>
        void SetDesignLibraryPath(string path);

        /// <summary>«Abrir RackCad al iniciar AutoCAD».</summary>
        bool OpenOnStartup { get; }

        void SetOpenOnStartup(bool open);

        /// <summary>The file of the settings of this user in this machine.</summary>
        string SettingsFilePath { get; }

        RackCadAboutInfo About { get; }
    }

    /// <summary>The settings of the machine for the utilities of the window, through the same store; every read loads the settings again.</summary>
    public sealed partial class RackCadSettingsAccess : IRackCadUtilitySettings
    {
        public bool DesignLibraryOverridden => !string.IsNullOrWhiteSpace(UserSettingsStore.Load().DesignLibraryPath);

        public void SetDesignLibraryPath(string path)
        {
            var settings = UserSettingsStore.Load();
            settings.DesignLibraryPath = string.IsNullOrWhiteSpace(path) ? null : path.Trim();
            UserSettingsStore.Save(settings);
        }

        public bool OpenOnStartup => UserSettingsStore.Load().OpenRackCadOnStartup;

        public void SetOpenOnStartup(bool open)
        {
            var settings = UserSettingsStore.Load();
            settings.OpenRackCadOnStartup = open;
            UserSettingsStore.Save(settings);
        }

        public string SettingsFilePath => UserSettingsStore.SettingsPath;

        public RackCadAboutInfo About => new RackCadAboutInfo(VersionText(), Safe(CatalogDirectory.Resolve), Safe(() => RackLog.LogDirectory));

        // The informational version is «x.y.z+<sha>» (Directory.Build.targets): shown as «x.y.z (sha corto)».
        private static string VersionText()
        {
            var informational = typeof(UserSettings).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            if (string.IsNullOrWhiteSpace(informational)) return typeof(UserSettings).Assembly.GetName().Version?.ToString(3) ?? string.Empty;
            var plus = informational.IndexOf('+');
            if (plus < 0) return informational;
            var sha = informational.Substring(plus + 1);
            return informational.Substring(0, plus) + (sha.Length == 0 ? string.Empty : " (" + (sha.Length > 8 ? sha.Substring(0, 8) : sha) + ")");
        }

        private static string Safe(Func<string> read)
        {
            try
            {
                return read() ?? string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
