using System.Text.Json;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

[assembly: CommandClass(typeof(I52G3AAutoCadProbe.ProbeCommands))]

namespace I52G3AAutoCadProbe;

public sealed class ProbeCommands
{
    private static readonly string[] ModeVariables =
    {
        "REFEDITNAME",
        "BLOCKEDITOR",
        "BLOCKTESTWINDOW",
        "ARRAYEDITSTATE",
        "CMDNAMES",
        "CMDACTIVE",
        "XLOADCTL",
        "INDEXCTL",
        "ACADVER",
        "PRODUCT",
        "PLATFORM",
        "VERNUM",
    };

    [CommandMethod("I52G3APROBE", CommandFlags.Session)]
    public void Run()
    {
        var document = Application.DocumentManager.MdiActiveDocument;
        var output = Environment.GetEnvironmentVariable("I52_G3A_OUTPUT");
        if (string.IsNullOrWhiteSpace(output))
        {
            output = Path.Combine(Path.GetTempPath(), "I-52-g3a-autocad-runtime-probe.json");
        }

        var host = HostApplicationServices.Current;
        var report = new ProbeReport
        {
            CapturedAtUtc = DateTimeOffset.UtcNow,
            ProcessPath = Environment.ProcessPath ?? string.Empty,
            ApplicationVersion = Safe(() => Application.Version.ToString()),
            HostType = host.GetType().FullName ?? host.GetType().Name,
            HostProduct = Safe(() => host.Product),
            HostProgram = Safe(() => host.Program),
            HostMarketVersion = Safe(() => host.releaseMarketVersion),
            HostCompany = Safe(() => host.CompanyName),
            MachineRegistryProductRootKey = Safe(() => host.MachineRegistryProductRootKey),
            UserRegistryProductRootKey = Safe(() => host.UserRegistryProductRootKey),
            DocumentName = document?.Name ?? string.Empty,
            CommandInProgress = document is null ? string.Empty : Safe(() => document.CommandInProgress),
            EditorIsQuiescent = document is null ? "NO_DOCUMENT" : Safe(() => document.Editor.IsQuiescent.ToString()),
            ApplicationIsQuiescent = Safe(() => Application.IsQuiescent.ToString()),
            CurrentLongTransaction = document is null
                ? "NO_DOCUMENT"
                : Safe(() => Application.LongTransactionManager.CurrentLongTransactionFor(document).ToString()),
        };

        Save(output, report);

        foreach (var name in ModeVariables)
        {
            report.ModeSignals.Add(name, Safe(() => Application.GetSystemVariable(name)?.ToString() ?? "<null>"));
            Save(output, report);
        }

        var enumerator = new SystemVariableEnumerator();
        while (enumerator.MoveNext())
        {
            var variable = enumerator.Current;
            report.SystemVariables.Add(new VariableObservation(
                Safe(() => variable.Name),
                Safe(() => variable.IsReadOnly.ToString()),
                Safe(() => variable.Storage.ToString()),
                Safe(() => variable.Value?.ToString() ?? "<null>")));
            Save(output, report);
        }

        if (document is not null)
        {
            report.OverruleProbe = ProbeOverrules();
            Save(output, report);
        }

        Save(output, report);
        document?.Editor.WriteMessage($"\nI-52 G3A probe: {output}\n");
    }

    private static void Save(string output, ProbeReport report)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(output)!);
        File.WriteAllText(output, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static OverruleProbe ProbeOverrules()
    {
        var result = new OverruleProbe();
        using var lineA = new Line(Point3d.Origin, new Point3d(1, 0, 0));
        using var lineB = new Line(new Point3d(0, 1, 0), new Point3d(1, 1, 0));
        using var circle = new Circle(new Point3d(2, 2, 0), Vector3d.ZAxis, 1);
        var lineClass = RXClass.GetClass(typeof(Line));
        var families = new (string Name, Overrule Instance, Type Family)[]
        {
            ("Drawable", new ProbeDrawableOverrule(), typeof(DrawableOverrule)),
        };

        var oldOverruling = Overrule.Overruling;
        try
        {
            Overrule.Overruling = true;
            foreach (var family in families)
            {
                try
                {
                    var familyClass = RXClass.GetClass(family.Family);
                    Overrule.AddOverrule(lineClass, family.Instance, false);
                    result.Families.Add(new OverruleFamilyObservation(
                        family.Name,
                        familyClass?.Name ?? "<null>",
                        Overrule.HasOverrule(lineA, familyClass),
                        Overrule.HasOverrule(lineB, familyClass),
                        Overrule.HasOverrule(circle, familyClass),
                        string.Empty));
                }
                catch (System.Exception ex)
                {
                    result.Families.Add(new OverruleFamilyObservation(
                        family.Name,
                        string.Empty,
                        false,
                        false,
                        false,
                        ex.GetType().Name + ": " + ex.Message));
                }
                finally
                {
                    try
                    {
                        Overrule.RemoveOverrule(lineClass, family.Instance);
                    }
                    catch
                    {
                    }
                }
            }

            var applicable = new ApplicableDrawableOverrule();
            applicable.SetCustomFilter();
            Overrule.AddOverrule(lineClass, applicable, false);
            try
            {
                var drawableClass = RXClass.GetClass(typeof(DrawableOverrule));
                result.CustomApplicabilityLineA = Overrule.HasOverrule(lineA, drawableClass);
                result.CustomApplicabilityLineB = Overrule.HasOverrule(lineB, drawableClass);
                result.CustomApplicabilityCircle = Overrule.HasOverrule(circle, drawableClass);
            }
            finally
            {
                Overrule.RemoveOverrule(lineClass, applicable);
            }
        }
        finally
        {
            Overrule.Overruling = oldOverruling;
        }

        return result;
    }

    private static string Safe(Func<string> read)
    {
        try
        {
            return read();
        }
        catch (System.Exception ex)
        {
            return "ERROR " + ex.GetType().Name + ": " + ex.Message;
        }
    }

    private sealed class ProbeDrawableOverrule : DrawableOverrule { }
    private sealed class ApplicableDrawableOverrule : DrawableOverrule
    {
        public override bool IsApplicable(RXObject subject) =>
            subject is Line line && line.StartPoint == Point3d.Origin;
    }
}

public sealed class ProbeReport
{
    public DateTimeOffset CapturedAtUtc { get; set; }
    public string ProcessPath { get; set; } = string.Empty;
    public string ApplicationVersion { get; set; } = string.Empty;
    public string HostType { get; set; } = string.Empty;
    public string HostProduct { get; set; } = string.Empty;
    public string HostProgram { get; set; } = string.Empty;
    public string HostMarketVersion { get; set; } = string.Empty;
    public string HostCompany { get; set; } = string.Empty;
    public string MachineRegistryProductRootKey { get; set; } = string.Empty;
    public string UserRegistryProductRootKey { get; set; } = string.Empty;
    public string DocumentName { get; set; } = string.Empty;
    public string CommandInProgress { get; set; } = string.Empty;
    public string EditorIsQuiescent { get; set; } = string.Empty;
    public string ApplicationIsQuiescent { get; set; } = string.Empty;
    public string CurrentLongTransaction { get; set; } = string.Empty;
    public Dictionary<string, string> ModeSignals { get; } = new(StringComparer.Ordinal);
    public List<VariableObservation> SystemVariables { get; } = new();
    public OverruleProbe? OverruleProbe { get; set; }
}

public sealed record VariableObservation(string Name, string IsReadOnly, string Storage, string Value);

public sealed class OverruleProbe
{
    public List<OverruleFamilyObservation> Families { get; } = new();
    public bool CustomApplicabilityLineA { get; set; }
    public bool CustomApplicabilityLineB { get; set; }
    public bool CustomApplicabilityCircle { get; set; }
}

public sealed record OverruleFamilyObservation(
    string Family,
    string FamilyRxClass,
    bool LineA,
    bool LineB,
    bool Circle,
    string Error);
