#nullable enable
using System;
using System.Collections.Generic;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using Xunit;

namespace RackCad.Tests;

// I-64 A-14, line D: decision 11 of the Owner on the prototype of Dinámico («Que avise antes de cerrar») and his authorization to change
// the rule of the close (2026-11-24). The close advisor of the window wraps the one of the host: the host decides for its drafts, and a
// drawing whose window has work not written does not close either, with a notice on the command line.
public class RackCadWindowCloseAdvisorTests
{
    private static readonly SessionId S1 = new(1);

    private sealed class Inner : IWorkspaceLifecycleAdvisor
    {
        public CloseDecision Answer { get; set; } = CloseDecision.Allow;

        public Exception? Fault { get; set; }

        public List<SessionId> Asked { get; } = new();

        public List<(SessionId Session, bool Intact)> Destroyed { get; } = new();

        public CloseDecision AdviseClose(SessionId session)
        {
            Asked.Add(session);
            if (Fault != null) throw Fault;
            return Answer;
        }

        public void SessionDestroyed(SessionId session, bool closeSequenceIntact) => Destroyed.Add((session, closeSequenceIntact));
    }

    private sealed class Rig
    {
        public Rig()
        {
            Advisor = new RackCadWindowCloseAdvisor(Inner, Lines.Add, (context, ex) => Reports.Add(context + ": " + ex.Message));
        }

        public Inner Inner { get; } = new();

        public List<string> Lines { get; } = new();

        public List<string> Reports { get; } = new();

        public RackCadWindowCloseAdvisor Advisor { get; }
    }

    [Fact]
    public void WithoutWorkInTheWindowTheDecisionIsTheOneOfTheHost()
    {
        var rig = new Rig();
        Assert.Equal(CloseDecision.Allow, rig.Advisor.AdviseClose(S1));

        rig.Advisor.SetWindowWork(_ => null);
        Assert.Equal(CloseDecision.Allow, rig.Advisor.AdviseClose(S1));

        rig.Inner.Answer = CloseDecision.Veto;
        Assert.Equal(CloseDecision.Veto, rig.Advisor.AdviseClose(S1));

        Assert.Empty(rig.Lines);
        Assert.Equal(new[] { S1, S1, S1 }, rig.Inner.Asked);
    }

    [Fact]
    public void WorkNotWrittenInTheWindowVetoesTheCloseAndSaysWhichRacksOnTheCommandLine()
    {
        var rig = new Rig();
        rig.Advisor.SetWindowWork(session => session == S1 ? "DIN 1, DIN 2" : null);

        Assert.Equal(CloseDecision.Veto, rig.Advisor.AdviseClose(S1));
        Assert.Equal(
            // Selectivo E1-B: the text names no system, so it holds for the drafts of Dinámico and of Selectivo.
            "RackCad: el dibujo no se cerró: la ventana RackCad tiene cambios sin escribir en DIN 1, DIN 2. Escríbelos con «Actualizar» en su editor (un rack nuevo de la ventana, con «Insertar en el dibujo…») o descártalos, y vuelve a cerrar.",
            Assert.Single(rig.Lines));

        // The host is still asked, so its own notice is written when it vetoes too.
        Assert.Equal(new[] { S1 }, rig.Inner.Asked);
        Assert.Equal(CloseDecision.Allow, rig.Advisor.AdviseClose(new SessionId(2)));
    }

    [Fact]
    public void AFailureOfTheHostOrOfTheWindowIsAVeto()
    {
        var rig = new Rig();
        rig.Inner.Fault = new InvalidOperationException("host");
        Assert.Equal(CloseDecision.Veto, rig.Advisor.AdviseClose(S1));

        rig.Inner.Fault = null;
        rig.Advisor.SetWindowWork(_ => throw new InvalidOperationException("window"));
        Assert.Equal(CloseDecision.Veto, rig.Advisor.AdviseClose(S1));
        Assert.Equal("RackCad: el dibujo no se cerró: no se pudo comprobar si la ventana RackCad tiene cambios sin escribir. Vuelve a intentarlo.", Assert.Single(rig.Lines));
        Assert.Equal(2, rig.Reports.Count);
    }

    [Fact]
    public void ANoticeThatCannotBeWrittenDoesNotChangeTheVeto()
    {
        var inner = new Inner();
        var advisor = new RackCadWindowCloseAdvisor(inner, _ => throw new InvalidOperationException("no command line"), null);
        advisor.SetWindowWork(_ => "DIN 1");

        Assert.Equal(CloseDecision.Veto, advisor.AdviseClose(S1));
    }

    [Fact]
    public void TheDestructionOfASessionIsTheHostsAlone()
    {
        var rig = new Rig();
        rig.Advisor.SetWindowWork(_ => "DIN 1");

        rig.Advisor.SessionDestroyed(S1, closeSequenceIntact: true);

        Assert.Equal(new[] { (S1, true) }, rig.Inner.Destroyed);
        Assert.Empty(rig.Lines);
    }

    // M-08 of the review of the entrega 1 of Selectivo: the host of the palette installs the work of the WHOLE window (Dinámico and
    // Selectivo, since Selectivo E1-B). UnwrittenDynamicWorkOf is kept with its behavior, so going back to it would leave every test of the
    // window green while a Selectivo not written stops vetoing the close; this guard fixes the line on the source (the Plugin cannot be
    // loaded here).
    [Fact]
    public void ThePaletteHostInstallsTheWorkOfTheWholeWindowInTheAdvisor()
    {
        var host = WorkspaceHostSource.Code("src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs");

        Assert.Equal(1, WorkspaceHostSource.Count(host, System.Text.RegularExpressions.Regex.Escape("_controller.WindowCloseAdvisor.SetWindowWork(_window.UnwrittenWorkOf);")));
        Assert.Equal(1, WorkspaceHostSource.Count(host, @"\.SetWindowWork\("));
        Assert.DoesNotContain("UnwrittenDynamicWorkOf", host, StringComparison.Ordinal);
    }

    [Fact]
    public void TheAdvisorNeedsTheHostAndTheCommandLine()
    {
        Assert.Throws<ArgumentNullException>(() => new RackCadWindowCloseAdvisor(null!, _ => { }, null));
        Assert.Throws<ArgumentNullException>(() => new RackCadWindowCloseAdvisor(new Inner(), null!, null));
    }
}
