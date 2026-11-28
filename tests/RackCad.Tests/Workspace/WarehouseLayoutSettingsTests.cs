#nullable enable
using System;
using System.IO;
using System.Text.Json;
using RackCad.Application.Settings;
using Xunit;

namespace RackCad.Tests;

/// <summary>
/// T5 al, correction 6 of the review: the values of the page «Almacén» live in the settings of the user (question 6). A field of another
/// version of RackCad survives a load and a save (<see cref="UserSettings.ExtensionData"/>, the pattern of I-11), and a section
/// «WarehouseLayout» that this build cannot read only loses that section (the values of the classic windows): the rest of the settings
/// load and the file is never quarantined for it. The tests use the internal path overloads, never the real %APPDATA%, and the log of the
/// section that cannot be read goes to a temporary folder.
/// </summary>
[Collection("RackLog")]
public class WarehouseLayoutSettingsTests
{
    private static string TempPath() => Path.Combine(Path.GetTempPath(), "rackcad-settings-al-" + Guid.NewGuid().ToString("N") + ".json");

    private static void Clean(string path)
    {
        foreach (var p in new[] { path, path + ".bad" })
            if (File.Exists(p)) File.Delete(p);
    }

    [Fact]
    public void AFieldOfAnotherVersionSurvivesALoadAndASaveOfTheValuesOfTheWarehouse()
    {
        var path = TempPath();
        try
        {
            File.WriteAllText(path, "{ \"BlockLibraryPath\": \"C:/lib/blocks.dwg\", \"CampoDelFuturo\": { \"a\": 1, \"b\": [true, \"x\"] }, \"OtraCosa\": 7 }");

            var settings = UserSettingsStore.Load(path);
            Assert.Equal("C:/lib/blocks.dwg", settings.BlockLibraryPath);
            settings.WarehouseLayout = new WarehouseLayoutPreferences { Rows = "6", BackToBack = true, Layer = "MUROS" };
            UserSettingsStore.Save(settings, path);

            using var written = JsonDocument.Parse(File.ReadAllText(path));
            var root = written.RootElement;
            Assert.Equal(1, root.GetProperty("CampoDelFuturo").GetProperty("a").GetInt32());
            Assert.Equal("x", root.GetProperty("CampoDelFuturo").GetProperty("b")[1].GetString());
            Assert.Equal(7, root.GetProperty("OtraCosa").GetInt32());
            Assert.Equal("6", root.GetProperty("WarehouseLayout").GetProperty("Rows").GetString());

            var again = UserSettingsStore.Load(path);
            Assert.Equal(("6", true, "MUROS"), (again.WarehouseLayout!.Rows, again.WarehouseLayout.BackToBack, again.WarehouseLayout.Layer));
            Assert.Equal("C:/lib/blocks.dwg", again.BlockLibraryPath);
        }
        finally
        {
            Clean(path);
        }
    }

    [Theory]
    [InlineData("{ \"Rows\": 5 }")]                       // a number where the page writes a text
    [InlineData("{ \"BackToBack\": \"sí\" }")]             // a text where it writes a switch
    [InlineData("\"4 filas\"")]                           // no object at all
    [InlineData("[ 1, 2 ]")]
    public void AWarehouseSectionThatCannotBeReadOnlyLosesThatSectionAndNeverQuarantinesTheFile(string section)
    {
        var path = TempPath();
        using var log = LogCapture.Begin();
        try
        {
            File.WriteAllText(path, "{ \"BlockLibraryPath\": \"C:/lib/blocks.dwg\", \"SelectiveTargetFondos\": \"0,2\", \"WarehouseLayout\": " + section
                                    + ", \"OpenRackCadOnStartup\": true }");

            var settings = UserSettingsStore.Load(path);

            Assert.Null(settings.WarehouseLayout); // the values of the classic windows
            Assert.Equal(("C:/lib/blocks.dwg", "0,2", true), (settings.BlockLibraryPath, settings.SelectiveTargetFondos, settings.OpenRackCadOnStartup));
            Assert.True(File.Exists(path));
            Assert.False(File.Exists(path + ".bad"));
            Assert.Contains("WarehouseLayout", log.Text);
        }
        finally
        {
            Clean(path);
        }
    }

    [Fact]
    public void AFileThatIsNoJsonIsStillQuarantinedAsBefore()
    {
        var path = TempPath();
        using var log = LogCapture.Begin();
        try
        {
            File.WriteAllText(path, "{ \"WarehouseLayout\": { \"Rows\": \"4\" }, esto no es json ][");
            var settings = UserSettingsStore.Load(path);
            Assert.Null(settings.WarehouseLayout);
            Assert.True(File.Exists(path + ".bad")); // I-03: a file that is not JSON keeps its quarantine
        }
        finally
        {
            Clean(path);
        }
    }
}
