using System;
using System.IO;
using RackCad.Application.Catalogs;
using RackCad.Application.Settings;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// What the section "Configuración" of the RackCad window reads and changes of the settings of RackCad (I-64 A-14, phase 1): the block
    /// library, with the same rules as the classic menu, and the folder of the design library. An interface so the window can be tested
    /// without touching the settings of the machine.
    /// </summary>
    public interface IRackCadSettingsAccess
    {
        /// <summary>The block library that RackCad uses now (the saved one or the default one next to the catalogs).</summary>
        string BlockLibraryPath { get; }

        /// <summary>True when the path was chosen by the user and saved.</summary>
        bool BlockLibraryOverridden { get; }

        /// <summary>True when the file of <see cref="BlockLibraryPath"/> exists.</summary>
        bool BlockLibraryExists { get; }

        /// <summary>The folder of the design library.</summary>
        string DesignLibraryPath { get; }

        /// <summary>Saves a chosen block library; null goes back to the default one.</summary>
        void SetBlockLibraryPath(string path);
    }

    /// <summary>The settings of the machine, through the same stores the classic menu uses. Every read loads the settings again.</summary>
    public sealed partial class RackCadSettingsAccess : IRackCadSettingsAccess
    {
        public string BlockLibraryPath => BlockLibraryLocator.ResolvePath() ?? string.Empty;

        public bool BlockLibraryOverridden => !string.IsNullOrWhiteSpace(UserSettingsStore.Load().BlockLibraryPath);

        public bool BlockLibraryExists
        {
            get
            {
                var path = BlockLibraryLocator.ResolvePath();
                return !string.IsNullOrWhiteSpace(path) && File.Exists(path);
            }
        }

        public string DesignLibraryPath => UserSettingsStore.ResolveDesignLibraryPath(UserSettingsStore.Load()) ?? string.Empty;

        public void SetBlockLibraryPath(string path)
        {
            var settings = UserSettingsStore.Load();
            settings.BlockLibraryPath = string.IsNullOrWhiteSpace(path) ? null : path;
            UserSettingsStore.Save(settings);
        }
    }
}
