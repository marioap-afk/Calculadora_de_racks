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

// I-64 F1-T3A-HOST-CORE: INV-F1-T3A-19, 20 and 21 (5). The scope correction released two product files of T2 for exact,
// additive deltas. These guards fix the shape of the deltas and everything that must stay as T2 left it.
public class WorkspaceBridgeFacadeSourceGuardTests
{
    private const string BridgePath = "src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs";
    private const string CorePath = "src/RackCad.Application/Workspace/EventBridgeCore.cs";

    private static string Bridge() => Src.Code(BridgePath);

    private static string Core() => Src.Code(CorePath);

    private static string Squash(string text) => Regex.Replace(text, @"\s+", string.Empty);

    // ------------------------------------------------------------------ one core, one registry

    [Fact]
    public void TheBridgeHoldsExactlyOneEventBridgeCoreAndNoSecondOne()
    {
        Assert.Equal(1, Src.Count(Bridge(), @"\bnew\s+EventBridgeCore\s*\("));
        var builders = Src.AllProduct().Where(f => Regex.IsMatch(f.Code, @"\bnew\s+EventBridgeCore\s*\(")).Select(f => f.Path).ToList();
        Assert.Equal(new[] { BridgePath }, builders);
        Assert.Equal(1, Src.Count(Bridge(), @"\bEventBridgeCore\s+_core\b"));
    }

    [Fact]
    public void NoWorkspaceSessionRegistryIsCreatedOutsideTheCoreAndTheBridgeNamesNone()
    {
        Assert.DoesNotContain("WorkspaceSessionRegistry", Bridge());
        foreach (var (path, code) in Src.PluginFiles())
            Assert.DoesNotMatch(new Regex(@"WorkspaceSessionRegistry"), code);

        // The registry is named only by its own file and by the core, which owns exactly one.
        var users = Src.AllProduct().Where(f => Regex.IsMatch(f.Code, @"\bWorkspaceSessionRegistry\b")).Select(f => f.Path).OrderBy(p => p, StringComparer.Ordinal).ToList();
        Assert.Equal(new[] { CorePath, "src/RackCad.Application/Workspace/WorkspaceSessionRegistry.cs" }, users);
        Assert.Equal(1, Src.Count(Core(), @"\bWorkspaceSessionRegistry\s+_registry\s*=\s*new\b"));
        Assert.Equal(1, Src.Count(Core(), @"\bWorkspaceSessionRegistry\b"));
    }

    // ------------------------------------------------------------------ the four thin facades of the bridge

    [Fact]
    public void TheBridgeExposesTheActiveSessionByDelegatingToTheCore()
    {
        Assert.Matches(@"public\s+SessionId\?\s+ActiveSession\s*=>\s*_core\.ActiveSession\s*;", Bridge());
    }

    [Fact]
    public void TheBridgeExposesTryGetSessionByDelegatingToTheCore()
    {
        var body = Src.BodyOf(Bridge(), @"public\s+bool\s+TryGetSession\s*\(\s*SessionId\s+\w+\s*,\s*out\s+WorkspaceSession\s+\w+\s*\)");

        Assert.Contains("_core.TryGetSession(", body);
        Assert.DoesNotMatch(new Regex(@"GetOrCreate|new\s+WorkspaceSession|Destroy"), body);
    }

    [Fact]
    public void TheBridgeExposesAuthorizeByDelegatingToTheCore()
    {
        Assert.Matches(
            @"public\s+RequestDecision\s+Authorize\s*\(\s*WorkspaceRequest\s+(\w+)\s*,\s*long\s+(\w+)\s*\)\s*=>\s*_core\.Authorize\s*\(\s*\1\s*,\s*\2\s*\)\s*;",
            Bridge());
    }

    [Fact]
    public void TheBridgeExposesRequestReconciliationByDelegatingToTheCoreAndNothingElse()
    {
        var body = Src.BodyOf(Bridge(), @"public\s+void\s+RequestReconciliation\s*\(\s*\)");

        Assert.Contains("_core.RequestReconciliation()", body);
        Assert.Equal(2, Src.Count(Bridge(), @"\bRequestReconciliation\s*\("));
        var squashed = Squash(body);
        Assert.True(
            squashed == "publicvoidRequestReconciliation()=>_core.RequestReconciliation();"
            || squashed == "publicvoidRequestReconciliation(){_core.RequestReconciliation();}",
            "the facade does more than delegate: " + body);
    }

    [Fact]
    public void TheBridgePublicSurfaceIsTheT2OneAndTheFourFacadesAndNothingElse()
    {
        var names = Bridge().Split('\n')
            .Select(l => Regex.Match(l, @"^\s*public\s+(?:event\s+)?[\w<>?,\s\.]+?\s+(\w+)\s*(?:\(|=>|;|\{)"))
            .Where(m => m.Success)
            .Select(m => m.Groups[1].Value)
            .Distinct()
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            new[] { "ActiveSession", "Authorize", "Dispose", "Drained", "RequestReconciliation", "SetModalRackCadActive", "SetPanelVisible", "Start", "TryGetSession" },
            names);
    }

    [Fact]
    public void TheBridgeAddsNoSubscriptionHandlerOrAttachmentOfItsOwn()
    {
        var code = Bridge();

        Assert.Equal(16, Src.Count(code, @"\+="));
        Assert.Equal(16, Src.Count(code, @"-="));
        Assert.Equal(16, Src.Count(code, @"\bhandler\s*=\s*\("));
        Assert.Equal(1, Src.Count(code, @"private\s+Action\s+Hook\s*\("));
        Assert.Equal(1, Src.Count(code, @"void\s+IHostEventSource\.Attach\s*\("));
        Assert.Equal(1, Src.Count(code, @"void\s+IHostEventSource\.Detach\s*\("));
    }

    [Fact]
    public void TheBridgeReadsAndWritesNothingOfTheDrawing()
    {
        var code = Bridge();

        Assert.DoesNotMatch(
            new Regex(@"StartTransaction|StartOpenCloseTransaction|LockDocument|GetObject|SelectImplied|SetImpliedSelection|GetSelection|RackBlockData|RackEmbedStore|ExtensionDictionary|AppendEntity|\.Commit\s*\(|Highlight|Zoom|Regen|SendStringToExecute"),
            code);
        foreach (var line in code.Split('\n').Where(l => Regex.IsMatch(l, @"\.Database\b")))
            Assert.True(line.Contains("+=") || line.Contains("-="), "the bridge touches the database outside a hookup: " + line.Trim());
    }

    [Fact]
    public void TheOrdinaryHandlersStillOnlyForwardToTheCore()
    {
        var code = Bridge();

        foreach (var line in code.Split('\n').Where(l => Regex.IsMatch(l, @"\bhandler\s*=\s*\(")))
        {
            Assert.DoesNotContain("RequestReconciliation", line);
            Assert.Matches(@"Contain(?:Session)?\(", line);
        }
    }

    [Fact]
    public void TheBridgeMembersOfT2KeepTheirBodies()
    {
        var code = Bridge();

        var start = Src.BodyOf(code, @"public\s+void\s+Start\s*\(\s*\)");
        Assert.Contains("_core.Start();", start);
        Assert.Contains("_core.OnDocumentActivated(", start);
        Assert.DoesNotContain("RequestReconciliation", start);

        var dispose = Src.BodyOf(code, @"public\s+void\s+Dispose\s*\(\s*\)");
        Assert.Contains("_core.Stop();", dispose);
        Assert.DoesNotContain("RequestReconciliation", dispose);

        var idle = Src.BodyOf(code, @"private\s+void\s+OnIdle\s*\(\s*\)");
        Assert.Contains("_core.OnIdle()", idle);
        Assert.Contains("Drained?.Invoke(", idle);

        Assert.Contains("_core.SetPanelVisible(visible)", Src.BodyOf(code, @"public\s+void\s+SetPanelVisible\s*\(\s*bool\s+visible\s*\)"));
        Assert.Contains("_core.OnBeginDocumentClose(a.Session)", code);
    }

    // ------------------------------------------------------------------ the two additive members of the pure core

    [Fact]
    public void TheCoreAuthorizesByDelegatingDirectlyToTheRegistry()
    {
        Assert.Matches(
            @"public\s+RequestDecision\s+Authorize\s*\(\s*WorkspaceRequest\s+(\w+)\s*,\s*long\s+(\w+)\s*\)\s*=>\s*_registry\.Authorize\s*\(\s*\1\s*,\s*\2\s*\)\s*;",
            Core());
    }

    [Fact]
    public void TheCoreRequestReconciliationOnlyEnqueuesTheHintOnTheActiveSessionAndSyncsIdle()
    {
        var body = Src.BodyOf(Core(), @"public\s+void\s+RequestReconciliation\s*\(\s*\)");

        Assert.Contains("HintKind.ImpliedSelectionChanged", body);
        Assert.Contains("EnqueueHint(", body);
        Assert.Contains("SyncIdle()", body);
        Assert.Contains("_active", body);
        Assert.Contains("Degraded", body);
        Assert.DoesNotMatch(
            new Regex(@"TryDrain|ApplyContext|GetOrCreate|OnIdle\s*\(|_source\.Attach|_source\.Detach|_registry\.Destroy|Autodesk|new\s+WorkspaceSession|_started\s*=|_active\s*="),
            body);
    }

    [Fact]
    public void StartAndActivationOfTheCoreDoNotQueueAReconciliation()
    {
        var core = Core();

        foreach (var signature in new[] { @"public\s+void\s+Start\s*\(\s*\)", @"public\s+void\s+OnDocumentActivated\s*\(\s*long\s+\w+\s*,\s*bool\s+\w+\s*\)" })
        {
            var body = Src.BodyOf(core, signature);
            Assert.DoesNotMatch(new Regex(@"EnqueueHint|RequestReconciliation"), body);
        }
    }

    [Fact]
    public void TheCoreAddsExactlyTwoPublicMembersToWhatT2Left()
    {
        var names = typeof(EventBridgeCore)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(m => !(m is MethodBase method && method.IsSpecialName))
            .Select(m => m.Name)
            .Distinct()
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToArray();

        var t2 = new[]
        {
            "ActiveSession", "AttachedSources", "IsDegraded", "IsIdleAttached", "IsStarted", "LiveSessionCount", "OnBeginDocumentClose",
            "OnDocumentActivated", "OnDocumentCreated", "OnDocumentToBeDestroyed", "OnIdle", "OnSourceEvent", "ReportFault",
            "SetModalRackCadActive", "SetPanelVisible", "Start", "Stop", "TryGetSession",
        };
        Assert.Equal(t2.Concat(new[] { "Authorize", "RequestReconciliation" }).OrderBy(n => n, StringComparer.Ordinal).ToArray(), names);
    }

    [Fact]
    public void TheCoreDoesNotExposeTheRegistryNorAnyMutableCollection()
    {
        var exposed = typeof(EventBridgeCore)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            .OfType<PropertyInfo>()
            .Select(p => p.PropertyType)
            .ToList();

        Assert.DoesNotContain(typeof(WorkspaceSessionRegistry), exposed);
        Assert.DoesNotContain(exposed, t => t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(List<>) || t.GetGenericTypeDefinition() == typeof(Dictionary<,>) || t.GetGenericTypeDefinition() == typeof(HashSet<>)));
    }

    [Fact]
    public void TheCoreStillNamesNothingOfAutoCadNorOfThePlugin()
    {
        Assert.DoesNotMatch(new Regex(@"\bAutodesk\.|RackCad\.Plugin|RackCad\.UI|System\.Windows"), Core());
    }
}
