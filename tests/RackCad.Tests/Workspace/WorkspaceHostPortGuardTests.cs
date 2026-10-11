#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using RackCad.Application.Workspace;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F1-T3A-HOST-CORE: INV-F1-T3A-03 (keys and values only, every request carries its SessionId) and
// INV-F1-T3A-04 (in F1 the port exposes no write), by reflection and by source.
public class WorkspaceHostPortGuardTests
{
    private const string PortPath = "src/RackCad.Application/Workspace/IWorkspaceHostPort.cs";
    private const string PortImplementationPath = "src/RackCad.Plugin/Workspace/WorkspaceHostPort.cs";

    /// <summary>
    /// A name that reads like a write or a state change of the drawing, the selection, the view, the stored data or a file.
    /// The F1 port has none of them.
    /// </summary>
    private static readonly Regex WriteLikeName = new(
        @"^(?:Set|Write|Save|Store|Persist|Commit|Apply|Update|Delete|Remove|Erase|Add|Create|Insert|Select|Zoom|Pan|Regen|Highlight|"
        + @"Navigate|Execute|Run|Send|Import|Export|Move|Modify|Edit|Draw|Redraw|Rename|Publish|Open|Close|Load|Reset|Clear|Enable|Disable|"
        + @"Toggle|Mark|Reconcile|Refresh|Start|Stop)",
        RegexOptions.CultureInvariant);

    private static readonly Regex ReadLikeName = new(@"^(?:Get|Authorize|Is|Can|Try)", RegexOptions.CultureInvariant);

    private static IReadOnlyList<MethodInfo> PortMethods() =>
        new[] { typeof(IWorkspaceHostPort) }.Concat(typeof(IWorkspaceHostPort).GetInterfaces())
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            .ToList();

    private static IEnumerable<Type> TypesOf(Type type)
    {
        yield return type;
        if (type.IsGenericType)
        {
            foreach (var argument in type.GetGenericArguments())
                foreach (var inner in TypesOf(argument)) yield return inner;
        }

        if (type.HasElementType && type.GetElementType() is { } element)
        {
            foreach (var inner in TypesOf(element)) yield return inner;
        }
    }

    [Fact]
    public void ThePortDeclaresOperationsAndNothingElse()
    {
        var port = typeof(IWorkspaceHostPort);

        Assert.True(port.IsInterface);
        Assert.Empty(port.GetEvents());
        Assert.Empty(port.GetFields());
        Assert.Empty(port.GetNestedTypes());
        Assert.NotEmpty(PortMethods());
    }

    [Fact]
    public void ThePortExposesNoPropertyThatCouldBeWritten()
    {
        Assert.All(typeof(IWorkspaceHostPort).GetProperties(), p => Assert.False(p.CanWrite, p.Name + " can be written"));
    }

    [Fact]
    public void EveryOperationOfThePortIsAReadOrTheAuthorizationAndNoneWritesAnything()
    {
        foreach (var method in PortMethods())
        {
            Assert.DoesNotMatch(WriteLikeName, method.Name);
            Assert.Matches(ReadLikeName, method.Name);
        }
    }

    [Fact]
    public void ThePortHasNoGenericOperationNorACallbackThatCouldRunWriteCode()
    {
        foreach (var method in PortMethods())
        {
            Assert.False(method.IsGenericMethod, method.Name);
            Assert.DoesNotContain(method.GetParameters(), p => typeof(Delegate).IsAssignableFrom(p.ParameterType));
            Assert.False(typeof(Delegate).IsAssignableFrom(method.ReturnType), method.Name);
        }
    }

    [Fact]
    public void EveryRequestOfThePortCarriesItsSessionIdentityAsItsFirstArgument()
    {
        var withArguments = PortMethods().Where(m => m.GetParameters().Length > 0).ToList();

        Assert.NotEmpty(withArguments);
        Assert.Contains(withArguments, m => m.Name == "Authorize");
        Assert.Contains(withArguments, m => m.Name == "GetSelectionContext");
        foreach (var method in withArguments)
        {
            Assert.Equal(typeof(WorkspaceRequest), method.GetParameters()[0].ParameterType);
            Assert.Single(method.GetParameters());
        }
    }

    [Fact]
    public void TheWorkspaceRequestCarriesTheOriginSessionId()
    {
        var origin = typeof(WorkspaceRequest).GetProperty("Origin");

        Assert.NotNull(origin);
        Assert.Equal(typeof(SessionId), origin!.PropertyType);
    }

    [Fact]
    public void OnlyKeysAndValuesOfTheApplicationLayerCrossThePort()
    {
        foreach (var method in PortMethods())
        {
            var types = method.GetParameters().Select(p => p.ParameterType).Append(method.ReturnType).SelectMany(TypesOf).Distinct();
            foreach (var type in types)
            {
                var ns = type.Namespace ?? string.Empty;
                Assert.True(
                    ns == "System" || ns.StartsWith("System.Collections", StringComparison.Ordinal) || ns == "RackCad.Application.Workspace",
                    method.Name + " exposes " + type.FullName);
                Assert.DoesNotContain("Autodesk", type.Assembly.GetName().Name ?? string.Empty);
            }
        }
    }

    [Fact]
    public void TheSnapshotTheUiReceivesHoldsOnlyValuesAndSessionKeys()
    {
        foreach (var type in new[] { typeof(WorkspaceHostSnapshot), typeof(WorkspaceSelectionResponse), typeof(WorkspaceMeasurementReport), typeof(WorkspaceMeasurement), typeof(SelectionContext) })
        {
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                foreach (var inner in TypesOf(property.PropertyType))
                {
                    var ns = inner.Namespace ?? string.Empty;
                    Assert.True(
                        ns == "System" || ns.StartsWith("System.Collections", StringComparison.Ordinal) || ns == "RackCad.Application.Workspace",
                        type.Name + "." + property.Name + " exposes " + inner.FullName);
                }
            }
        }
    }

    [Theory]
    [InlineData("SetSelection")]
    [InlineData("SetImpliedSelection")]
    [InlineData("ZoomToRack")]
    [InlineData("WriteRack")]
    [InlineData("SaveDraft")]
    [InlineData("ApplyContext")]
    [InlineData("UpdateRack")]
    [InlineData("SelectRack")]
    [InlineData("CommitDraft")]
    [InlineData("NavigateTo")]
    [InlineData("PersistLayout")]
    [InlineData("ExportFile")]
    [InlineData("DeleteRack")]
    [InlineData("StartCommand")]
    [InlineData("RefreshView")]
    public void TheWriteDetectorFlagsEachWriteLikeOperationName(string name)
    {
        Assert.Matches(WriteLikeName, name);
    }

    [Theory]
    [InlineData("GetSnapshot")]
    [InlineData("Authorize")]
    [InlineData("GetSelectionContext")]
    [InlineData("GetMeasurementReport")]
    public void TheWriteDetectorDoesNotFlagTheRealPortOperations(string name)
    {
        Assert.DoesNotMatch(WriteLikeName, name);
        Assert.Matches(ReadLikeName, name);
    }

    // ------------------------------------------------------------------ the source of the port

    [Fact]
    public void ThePortIsDeclaredInOneNewFileOfApplicationAndNowhereElse()
    {
        Assert.True(Src.Exists(PortPath));
        var declarations = Src.AllProduct().Where(f => Regex.IsMatch(f.Code, @"\binterface\s+IWorkspaceHostPort\b")).Select(f => f.Path).ToList();

        Assert.Equal(new[] { PortPath }, declarations);
    }

    [Fact]
    public void ThePortSourceNamesNoAutoCadNorWpfType()
    {
        var code = Src.Code(PortPath);

        Assert.DoesNotMatch(
            new Regex(@"\b(?:Document|Database|Editor|Transaction|DBObject|ObjectId|Entity|BlockReference|SelectionSet|Autodesk|PaletteSet|UserControl)\b|System\.Windows"),
            code);
    }

    [Fact]
    public void ThePortSourceDeclaresNoWriteLikeOperation()
    {
        var code = Src.Code(PortPath);

        var names = Regex.Matches(code, @"^\s*(?!public\b|namespace\b|using\b)[\w<>?,\[\]]+\s+(\w+)\s*\(", RegexOptions.Multiline)
            .Select(m => m.Groups[1].Value)
            .ToList();

        Assert.NotEmpty(names);
        Assert.All(names, n => Assert.DoesNotMatch(WriteLikeName, n));
    }

    [Fact]
    public void ThePortIsImplementedInThePluginAndOnlyThere()
    {
        Assert.True(Src.Exists(PortImplementationPath));
        var implementers = Src.AllProduct()
            .Where(f => Regex.IsMatch(f.Code, @"class\s+\w+\s*:\s*[^{]*\bIWorkspaceHostPort\b"))
            .Select(f => f.Path)
            .ToList();

        Assert.Equal(new[] { PortImplementationPath }, implementers);
    }

    [Fact]
    public void ThePluginImplementationOnlyDelegatesToTheHostCore()
    {
        var code = Src.Code(PortImplementationPath);

        Assert.Equal(4, Src.Count(code, @"_core\."));
        Assert.DoesNotMatch(new Regex(@"\b(?:Autodesk|AcApplication|RackLog|StartTransaction|LockDocument|GetObject|SelectImplied)\b"), code);
        foreach (var name in new[] { "GetSnapshot", "Authorize", "GetSelectionContext", "GetMeasurementReport" })
            Assert.Matches(new Regex(@"public\s+\w+\s+" + name + @"\s*\([^)]*\)\s*=>\s*_core\." + name + @"\("), code);
    }

    [Fact]
    public void ThePluginImplementationExposesNoAutoCadTypeInItsPublicApi()
    {
        var code = Src.Code(PortImplementationPath);

        foreach (var line in code.Split('\n').Where(l => Regex.IsMatch(l, @"^\s*(?:public|protected)\b")))
            Assert.DoesNotMatch(new Regex(@"\b(?:Document|Database|Editor|ObjectId|Entity|Transaction|DBObject|Autodesk)\b"), line);
    }
}
