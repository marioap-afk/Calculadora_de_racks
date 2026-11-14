#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Workspace;
using RackCad.Application.Workspace.Drafts;
using Xunit;

namespace RackCad.Tests;

/// <summary>
/// I-64 F4-T3C2 test double of <see cref="IWorkspacePilotEditPort"/> for Core: it records EVERY call with the session that travelled with it.
/// Only the two reads and the discard of the document that the command and the wrapper may use have an answer; any other operation is
/// recorded and then throws, so the oracle of "zero writes of the drawing" is the exact set of operations that were called.
/// </summary>
internal sealed class CommandAdvisorPortDouble : IWorkspacePilotEditPort
{
    public const string RisksOp = "GetRacksAtRisk";
    public const string DiscardDocumentOp = "DiscardDocument";

    public List<(string Operation, SessionId Origin)> Calls { get; } = new();

    /// <summary>The answer of the n-th read of the risk (zero based). The default is one rack at risk and nothing uncertain.</summary>
    public Func<int, PilotRiskView> Risks { get; set; } = _ => new PilotRiskView(new[] { "R-1" }, Array.Empty<string>());

    public Func<WorkspaceRequest, EditDiscardOutcome> Discard { get; set; } =
        _ => EditDiscardOutcome.Discarded(new[] { "R-1" }, Array.Empty<string>());

    public HashSet<string> Operations() => Calls.Select(c => c.Operation).ToHashSet(StringComparer.Ordinal);

    public int Count(string operation) => Calls.Count(c => c.Operation == operation);

    public PilotRiskView GetRacksAtRisk(WorkspaceRequest origin)
    {
        var index = Count(RisksOp);
        Calls.Add((RisksOp, origin.Origin));
        return Risks(index);
    }

    public EditDiscardOutcome DiscardDocument(WorkspaceRequest origin)
    {
        Calls.Add((DiscardDocumentOp, origin.Origin));
        return Discard(origin);
    }

    private static Exception Forbidden(string operation) => new InvalidOperationException("The operation is not allowed here: " + operation);

    private T Refuse<T>(string operation, SessionId origin)
    {
        Calls.Add((operation, origin));
        throw Forbidden(operation);
    }

    public EditIntentOutcome TryBeginEdit(EditIntentRequest request) => Refuse<EditIntentOutcome>("TryBeginEdit", request.Session);

    public EditCheckOutcome TryCheckNow(EditIntentRequest request) => Refuse<EditCheckOutcome>("TryCheckNow", request.Session);

    public EditIntentOutcome TryReload(EditIntentRequest request) => Refuse<EditIntentOutcome>("TryReload", request.Session);

    public EditConfirmOutcome TryConfirmBase(EditIntentRequest request) => Refuse<EditConfirmOutcome>("TryConfirmBase", request.Session);

    public EditIntentOutcome TryKeepMine(EditIntentRequest request) => Refuse<EditIntentOutcome>("TryKeepMine", request.Session);

    public EditIntentOutcome TryDiscardAndReload(EditIntentRequest request) => Refuse<EditIntentOutcome>("TryDiscardAndReload", request.Session);

    public PilotFieldView GetField(WorkspaceRequest origin, string rackId) => Refuse<PilotFieldView>("GetField", origin.Origin);

    public PilotFieldResult SetPendingText(WorkspaceRequest origin, string rackId, string text) =>
        Refuse<PilotFieldResult>("SetPendingText", origin.Origin);

    public PilotFieldResult CommitField(WorkspaceRequest origin, string rackId) => Refuse<PilotFieldResult>("CommitField", origin.Origin);

    public PilotBoundaryResult CrossBoundary(WorkspaceRequest origin, string rackId, PilotBoundaryKind kind) =>
        Refuse<PilotBoundaryResult>("CrossBoundary", origin.Origin);

    public EditDiscardOutcome DiscardRack(WorkspaceRequest origin, string rackId) => Refuse<EditDiscardOutcome>("DiscardRack", origin.Origin);

    public IReadOnlyList<PilotLossView> TakeLossNotices() => Refuse<IReadOnlyList<PilotLossView>>("TakeLossNotices", default);

    public IReadOnlyList<PilotCloseView> TakeCloseNotices() => Refuse<IReadOnlyList<PilotCloseView>>("TakeCloseNotices", default);
}

// I-64 F4-T3C2-COMMAND-ADVISOR (INV-F4T3C2-01 to 06; D-T3C2-01, 02, 04): the lazy access without creating, the pure core of the command
// RACKPANELDESCARTAR and the wrapper of the lifecycle advisor, in Core with doubles and with the real bridge core and host core. Nothing
// reads or writes a drawing. The oracle of INV-F4T3C2-01 is the exact set of operations of the port that were called. Every expectation
// that something throws, or does not, uses Record.Exception, so a behavior that is missing fails by ASSERTION and never by an escaped
// exception. The texts are the ones of the closed tables T-1 and T-2 of the package, repeated here on purpose.
public class WorkspaceCommandAdvisorTests
{
    private static readonly SessionId A = new(7);
    private static readonly SessionId B = new(8);

    private const string C01 = "RackCad: No hay borradores del panel Workspace en esta sesión de AutoCAD; no se descartó nada.";
    private const string C02 = "RackCad: El panel Workspace no sigue ningún documento activo en este momento; no se descartó nada.";
    private const string C03 = "RackCad: No se pudo decidir qué borradores están en riesgo; no se descartó nada.";
    private const string C04 = "RackCad: No hay borradores en riesgo en este documento; no se descartó nada.";
    private const string Q = "¿Descartar los borradores de este documento?";
    private const string C07 = "RackCad: No se descartó nada.";
    private const string C08 = "RackCad: El documento activo cambió; no se descartó nada.";
    private const string C09 = "RackCad: No se pudieron volver a comprobar los borradores en riesgo; no se descartó nada.";
    private const string C10 = "RackCad: Los borradores en riesgo cambiaron; no se descartó nada. Vuelve a ejecutar RACKPANELDESCARTAR para revisarlos.";
    private const string C15 = "RackCad: El panel Workspace no sigue el documento activo en este momento; no se descartó nada.";

    private static PilotRiskView Risk(string[] known, string[]? uncertain = null) => new(known, uncertain ?? Array.Empty<string>());

    // ------------------------------------------------------------------ D-T3C2-01: the access that never creates

    [Fact]
    public void ExistingIsNullBeforeTheFirstCreationAndAskingForItNeverCreatesNorCallsTheFactory()
    {
        var factoryCalls = 0;
        var holder = new WorkspaceLazyInstance<object>(() =>
        {
            factoryCalls++;
            return new object();
        });

        var first = holder.Existing;
        var second = holder.Existing;

        Assert.Null(first);
        Assert.Null(second);
        Assert.Equal(0, factoryCalls);
        Assert.Equal(0, holder.CreateCount);
        Assert.False(holder.IsCreated);
    }

    [Fact]
    public void ExistingIsTheCreatedInstanceAfterwardsAndDoesNotChangeTheCreateCount()
    {
        var holder = new WorkspaceLazyInstance<object>(() => new object());
        var created = holder.GetOrCreate();

        var existing = holder.Existing;

        Assert.Same(created, existing);
        Assert.Same(created, holder.Existing);
        Assert.Equal(1, holder.CreateCount);
        Assert.True(holder.IsCreated);
    }

    [Fact]
    public void ExistingStaysNullWhenTheFactoryFailed()
    {
        var holder = new WorkspaceLazyInstance<object>(() => throw new InvalidOperationException("the factory failed"));

        var thrown = Record.Exception(() => holder.GetOrCreate());

        Assert.IsType<InvalidOperationException>(thrown);
        Assert.Null(holder.Existing);
        Assert.Equal(0, holder.CreateCount);
    }

    // ------------------------------------------------------------------ D-T3C2-02: the pure core of the command

    private sealed class CommandRig
    {
        public CommandAdvisorPortDouble Port { get; } = new();

        public SessionId? Active { get; set; } = A;

        public bool Authorizes { get; set; } = true;

        public Func<string, bool> Answer { get; set; } = _ => true;

        public Action? DuringQuestion { get; set; }

        public Func<Exception?>? WriteFailure { get; set; }

        public List<string> Written { get; } = new();

        public List<string> Questions { get; } = new();

        public List<SessionId> Targets { get; } = new();

        public int ActiveReads { get; private set; }

        /// <summary>The last text that was written; a missing one is an assertion, never an exception of the lookup.</summary>
        public string LastText
        {
            get
            {
                Assert.NotEmpty(Written);
                return Written[Written.Count - 1];
            }
        }

        public Exception? Run(bool withPort = true) => Record.Exception(() => WorkspaceDocumentDiscardCommand.Run(
            withPort ? Port : null,
            () =>
            {
                ActiveReads++;
                return Active;
            },
            session =>
            {
                Targets.Add(session);
                return Authorizes;
            },
            question =>
            {
                Questions.Add(question);
                DuringQuestion?.Invoke();
                return Answer(question);
            },
            text =>
            {
                Written.Add(text);
                var failure = WriteFailure?.Invoke();
                if (failure != null) throw failure;
            }));

        public void AssertOnlyTheTwoOperationsOfTheCommand()
        {
            Assert.Empty(Port.Operations().Except(new[] { CommandAdvisorPortDouble.RisksOp, CommandAdvisorPortDouble.DiscardDocumentOp }));
        }
    }

    [Fact]
    public void WithoutAControllerTheCommandSaysSoAndAsksNothingAtAll()
    {
        var rig = new CommandRig();

        var thrown = rig.Run(withPort: false);

        Assert.Null(thrown);
        Assert.Equal(new[] { C01 }, rig.Written.ToArray());
        Assert.Equal(0, rig.ActiveReads);
        Assert.Empty(rig.Targets);
        Assert.Empty(rig.Questions);
        Assert.Empty(rig.Port.Calls);
    }

    [Fact]
    public void WithoutAnActiveSessionTheCommandSaysSoAndNeitherReadsNorAsksNorDiscards()
    {
        var rig = new CommandRig { Active = null };

        var thrown = rig.Run();

        Assert.Null(thrown);
        Assert.Equal(new[] { C02 }, rig.Written.ToArray());
        Assert.Empty(rig.Targets);
        Assert.Empty(rig.Questions);
        Assert.Empty(rig.Port.Calls);
    }

    [Fact]
    public void AnAmbiguousTargetSaysSoWithoutReadingTheRiskWithoutAskingAndWithoutDiscarding()
    {
        var rig = new CommandRig { Authorizes = false };

        var thrown = rig.Run();

        Assert.Null(thrown);
        Assert.Equal(new[] { C15 }, rig.Written.ToArray());
        Assert.Equal(new[] { A }, rig.Targets.ToArray());
        Assert.Empty(rig.Questions);
        Assert.Equal(0, rig.Port.Count(CommandAdvisorPortDouble.RisksOp));
        Assert.Equal(0, rig.Port.Count(CommandAdvisorPortDouble.DiscardDocumentOp));
        Assert.Empty(rig.Port.Calls);
    }

    [Fact]
    public void AnUnknownFirstReadIsAnExplicitFailureAndNeverNothingAtRisk()
    {
        var rig = new CommandRig();
        rig.Port.Risks = _ => default;

        var thrown = rig.Run();

        Assert.Null(thrown);
        Assert.Equal(new[] { C03 }, rig.Written.ToArray());
        Assert.Empty(rig.Questions);
        Assert.Equal(1, rig.Port.Count(CommandAdvisorPortDouble.RisksOp));
        Assert.Equal(0, rig.Port.Count(CommandAdvisorPortDouble.DiscardDocumentOp));
    }

    [Fact]
    public void AFirstReadThatThrowsCountsAsTheUnknownAndTheExceptionDoesNotEscape()
    {
        var rig = new CommandRig();
        rig.Port.Risks = _ => throw new InvalidOperationException("the read failed");

        var thrown = rig.Run();

        Assert.Null(thrown);
        Assert.Equal(new[] { C03 }, rig.Written.ToArray());
        Assert.Empty(rig.Questions);
        Assert.Equal(0, rig.Port.Count(CommandAdvisorPortDouble.DiscardDocumentOp));
    }

    [Fact]
    public void WhenNothingIsAtRiskTheCommandSaysSoAndAsksNothing()
    {
        var rig = new CommandRig();
        rig.Port.Risks = _ => Risk(Array.Empty<string>());

        var thrown = rig.Run();

        Assert.Null(thrown);
        Assert.Equal(new[] { C04 }, rig.Written.ToArray());
        Assert.Empty(rig.Questions);
        Assert.Equal(new[] { CommandAdvisorPortDouble.RisksOp }, rig.Port.Calls.Select(c => c.Operation).ToArray());
    }

    [Fact]
    public void WithRacksAtRiskTheCommandListsThemByRackIdAsksAndOnYesDiscardsTheWholeDocumentOnce()
    {
        var rig = new CommandRig();
        rig.Port.Risks = _ => Risk(new[] { "R-1", "R-3" });
        rig.Port.Discard = _ => EditDiscardOutcome.Discarded(new[] { "R-1", "R-3", "R-9" }, new[] { "R-1" });

        var thrown = rig.Run();

        Assert.Null(thrown);
        Assert.Equal(
            new[]
            {
                "RackCad: Borradores con trabajo sin aplicar: R-1, R-3.",
                "RackCad: Se descartaron los borradores en riesgo: R-1, R-3. Borradores limpios retirados además: 1.",
            },
            rig.Written.ToArray());
        Assert.Equal(new[] { Q }, rig.Questions.ToArray());
        Assert.Equal(
            new[]
            {
                (CommandAdvisorPortDouble.RisksOp, A),
                (CommandAdvisorPortDouble.RisksOp, A),
                (CommandAdvisorPortDouble.DiscardDocumentOp, A),
            },
            rig.Port.Calls.ToArray());
        rig.AssertOnlyTheTwoOperationsOfTheCommand();
    }

    [Fact]
    public void TheUncertainRacksAreListedApartAndAreDiscardedWithTheRestAfterTheConfirmation()
    {
        var rig = new CommandRig();
        rig.Port.Risks = _ => Risk(new[] { "R-1" }, new[] { "R-2", "R-4" });
        rig.Port.Discard = _ => EditDiscardOutcome.Discarded(new[] { "R-1", "R-2", "R-4", "R-5", "R-6" }, Array.Empty<string>());

        var thrown = rig.Run();

        Assert.Null(thrown);
        Assert.Equal(
            new[]
            {
                "RackCad: Borradores con trabajo sin aplicar: R-1.",
                "RackCad: No se pudo decidir si tienen trabajo sin aplicar: R-2, R-4.",
                "RackCad: Se descartaron los borradores en riesgo: R-1. Sin decidir, también descartados: R-2, R-4. Borradores limpios retirados además: 2.",
            },
            rig.Written.ToArray());
        Assert.Equal(1, rig.Port.Count(CommandAdvisorPortDouble.DiscardDocumentOp));
    }

    [Fact]
    public void WithOnlyUncertainRacksTheCommandListsThemAndTheFinalTextSaysTheDocumentWasDiscarded()
    {
        var rig = new CommandRig();
        rig.Port.Risks = _ => Risk(Array.Empty<string>(), new[] { "R-2" });
        rig.Port.Discard = _ => EditDiscardOutcome.Discarded(new[] { "R-2" }, Array.Empty<string>());

        var thrown = rig.Run();

        Assert.Null(thrown);
        Assert.Equal(
            new[]
            {
                "RackCad: No se pudo decidir si tienen trabajo sin aplicar: R-2.",
                "RackCad: Se descartaron los borradores del documento. Sin decidir, también descartados: R-2. Borradores limpios retirados además: 0.",
            },
            rig.Written.ToArray());
        Assert.Equal(1, rig.Port.Count(CommandAdvisorPortDouble.DiscardDocumentOp));
    }

    [Fact]
    public void AnythingButAYesDiscardsNothing()
    {
        // The command line maps No, Enter, Escape and cancel to a false answer: here they are all the same false (C2-M-8). Enter and
        // Escape themselves are observed in Smoke-2.
        var rig = new CommandRig { Answer = _ => false };
        rig.Port.Risks = _ => Risk(new[] { "R-1" });

        var thrown = rig.Run();

        Assert.Null(thrown);
        Assert.Equal(
            new[] { "RackCad: Borradores con trabajo sin aplicar: R-1.", C07 },
            rig.Written.ToArray());
        Assert.Equal(new[] { Q }, rig.Questions.ToArray());
        Assert.Equal(1, rig.Port.Count(CommandAdvisorPortDouble.RisksOp));
        Assert.Equal(0, rig.Port.Count(CommandAdvisorPortDouble.DiscardDocumentOp));
    }

    [Fact]
    public void NothingIsDiscardedBeforeTheYes()
    {
        var rig = new CommandRig();
        var discardsDuringTheQuestion = -1;
        rig.DuringQuestion = () => discardsDuringTheQuestion = rig.Port.Count(CommandAdvisorPortDouble.DiscardDocumentOp);

        var thrown = rig.Run();

        Assert.Null(thrown);
        Assert.Equal(0, discardsDuringTheQuestion);
        Assert.Equal(1, rig.Port.Count(CommandAdvisorPortDouble.DiscardDocumentOp));
    }

    [Fact]
    public void WhenTheActiveSessionChangesDuringTheQuestionNothingIsDiscardedAndTheOtherSessionIsNeverAsked()
    {
        var rig = new CommandRig();
        rig.DuringQuestion = () => rig.Active = B;

        var thrown = rig.Run();

        Assert.Null(thrown);
        Assert.Equal(C08, rig.LastText);
        Assert.Equal(0, rig.Port.Count(CommandAdvisorPortDouble.DiscardDocumentOp));
        Assert.Equal(1, rig.Port.Count(CommandAdvisorPortDouble.RisksOp));
        Assert.DoesNotContain(B, rig.Targets);
        Assert.DoesNotContain(rig.Port.Calls, c => c.Origin == B);
    }

    [Fact]
    public void WhenTheActiveSessionDisappearsDuringTheQuestionNothingIsDiscarded()
    {
        var rig = new CommandRig();
        rig.DuringQuestion = () => rig.Active = null;

        var thrown = rig.Run();

        Assert.Null(thrown);
        Assert.Equal(C08, rig.LastText);
        Assert.Equal(0, rig.Port.Count(CommandAdvisorPortDouble.DiscardDocumentOp));
    }

    [Fact]
    public void WhenTheSessionStopsAuthorizingAfterTheYesTheTargetIsAmbiguousAndNothingIsReadAgainOrDiscarded()
    {
        var rig = new CommandRig();
        rig.DuringQuestion = () => rig.Authorizes = false;

        var thrown = rig.Run();

        Assert.Null(thrown);
        Assert.Equal(C15, rig.LastText);
        Assert.Equal(new[] { A, A }, rig.Targets.ToArray());
        Assert.Equal(1, rig.Port.Count(CommandAdvisorPortDouble.RisksOp));
        Assert.Equal(0, rig.Port.Count(CommandAdvisorPortDouble.DiscardDocumentOp));
    }

    [Fact]
    public void TheTargetIsOnlyAskedAboutTheSessionThatTheActiveDelegateJustGave()
    {
        var rig = new CommandRig();

        var thrown = rig.Run();

        Assert.Null(thrown);
        Assert.Equal(new[] { A, A }, rig.Targets.ToArray());
        Assert.Equal(2, rig.ActiveReads);
        Assert.All(rig.Port.Calls, c => Assert.Equal(A, c.Origin));
    }

    [Fact]
    public void AnUnknownSecondReadDiscardsNothing()
    {
        var rig = new CommandRig();
        rig.Port.Risks = index => index == 0 ? Risk(new[] { "R-1" }) : default;

        var thrown = rig.Run();

        Assert.Null(thrown);
        Assert.Equal(C09, rig.LastText);
        Assert.Equal(2, rig.Port.Count(CommandAdvisorPortDouble.RisksOp));
        Assert.Equal(0, rig.Port.Count(CommandAdvisorPortDouble.DiscardDocumentOp));
    }

    [Fact]
    public void ASecondReadThatThrowsCountsAsTheUnknownAndDiscardsNothing()
    {
        var rig = new CommandRig();
        rig.Port.Risks = index => index == 0 ? Risk(new[] { "R-1" }) : throw new InvalidOperationException("the second read failed");

        var thrown = rig.Run();

        Assert.Null(thrown);
        Assert.Equal(C09, rig.LastText);
        Assert.Equal(0, rig.Port.Count(CommandAdvisorPortDouble.DiscardDocumentOp));
    }

    public static TheoryData<string, string[], string[], string[], string[]> ChangedRisks() => new()
    {
        { "a rack more", new[] { "R-1" }, Array.Empty<string>(), new[] { "R-1", "R-2" }, Array.Empty<string>() },
        { "a rack less", new[] { "R-1", "R-2" }, Array.Empty<string>(), new[] { "R-1" }, Array.Empty<string>() },
        { "another order", new[] { "R-1", "R-2" }, Array.Empty<string>(), new[] { "R-2", "R-1" }, Array.Empty<string>() },
        { "another case, by ordinal", new[] { "R-1" }, Array.Empty<string>(), new[] { "r-1" }, Array.Empty<string>() },
        { "an uncertain one more", new[] { "R-1" }, Array.Empty<string>(), new[] { "R-1" }, new[] { "R-2" } },
        { "from sure to uncertain", new[] { "R-1" }, Array.Empty<string>(), Array.Empty<string>(), new[] { "R-1" } },
        { "nothing left", new[] { "R-1" }, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>() },
    };

    [Theory]
    [MemberData(nameof(ChangedRisks))]
    public void IfEitherListChangedBetweenTheTwoReadsNothingIsDiscarded(
        string why, string[] firstKnown, string[] firstUncertain, string[] secondKnown, string[] secondUncertain)
    {
        var rig = new CommandRig();
        rig.Port.Risks = index => index == 0 ? Risk(firstKnown, firstUncertain) : Risk(secondKnown, secondUncertain);

        var thrown = rig.Run();

        Assert.Null(thrown);
        Assert.Equal(C10, rig.LastText);
        Assert.Equal(2, rig.Port.Count(CommandAdvisorPortDouble.RisksOp));
        Assert.Equal(0, rig.Port.Count(CommandAdvisorPortDouble.DiscardDocumentOp));
        Assert.False(string.IsNullOrEmpty(why));
    }

    [Fact]
    public void TheDiscardOfTheDocumentWithTheSameListsAfterTheYesIsTheOnlyOperationWithAnEffect()
    {
        var rig = new CommandRig();
        rig.Port.Risks = _ => Risk(new[] { "R-1", "R-2" }, new[] { "R-3" });

        var thrown = rig.Run();

        Assert.Null(thrown);
        Assert.Equal(
            new[] { CommandAdvisorPortDouble.RisksOp, CommandAdvisorPortDouble.RisksOp, CommandAdvisorPortDouble.DiscardDocumentOp },
            rig.Port.Calls.Select(c => c.Operation).ToArray());
        rig.AssertOnlyTheTwoOperationsOfTheCommand();
        Assert.DoesNotContain("DiscardRack", rig.Port.Operations());
    }

    public static TheoryData<string, string> NotDiscardedOutcomes() => new()
    {
        { "Busy|Hay una lectura del piloto en curso.", "RackCad: No se descartaron los borradores del documento: Hay una lectura del piloto en curso." },
        { "Busy| ", "RackCad: No se descartaron los borradores del documento: hay una lectura en curso." },
        { "Busy|", "RackCad: No se descartaron los borradores del documento: hay una lectura en curso." },
        { "Rejected|El documento no es el activo.", "RackCad: No se descartaron los borradores del documento: se rechazó. El documento no es el activo." },
        { "Rejected| ", "RackCad: No se descartaron los borradores del documento: se rechazó. sin detalle." },
        { "Failed|falló el host", "RackCad: No se descartaron los borradores del documento: falló. falló el host" },
        { "Failed| ", "RackCad: No se descartaron los borradores del documento: falló. sin detalle." },
        { "Throws|se cayó el puerto", "RackCad: No se descartaron los borradores del documento: falló. se cayó el puerto" },
        { "Throws| ", "RackCad: No se descartaron los borradores del documento: falló. sin detalle." },
    };

    [Theory]
    [MemberData(nameof(NotDiscardedOutcomes))]
    public void EveryAnswerThatIsNotDiscardedSaysWhyWithTheExactText(string answer, string expected)
    {
        var rig = new CommandRig();
        var parts = answer.Split('|');
        rig.Port.Discard = request => parts[0] switch
        {
            "Busy" => EditDiscardOutcome.Busy(parts[1]),
            "Rejected" => EditDiscardOutcome.Rejected(parts[1]),
            "Failed" => EditDiscardOutcome.Failed(parts[1]),
            _ => throw new InvalidOperationException(parts[1]),
        };

        var thrown = rig.Run();

        Assert.Null(thrown);
        Assert.Equal(expected, rig.LastText);
        Assert.Equal(1, rig.Port.Count(CommandAdvisorPortDouble.DiscardDocumentOp));
    }

    [Fact]
    public void TheDefaultOutcomeOfTheDiscardIsTreatedAsARejection()
    {
        var rig = new CommandRig();
        rig.Port.Discard = _ => default;

        var thrown = rig.Run();

        Assert.Null(thrown);
        Assert.Equal("RackCad: No se descartaron los borradores del documento: se rechazó. sin detalle.", rig.LastText);
    }

    [Fact]
    public void ADelegateThatThrowsIsNotContainedAndNothingWasDiscardedBeforeTheDiscardItself()
    {
        var confirmThrows = new CommandRig { Answer = _ => throw new InvalidOperationException("the question failed") };
        var thrown = confirmThrows.Run();
        Assert.IsType<InvalidOperationException>(thrown);
        Assert.Equal(0, confirmThrows.Port.Count(CommandAdvisorPortDouble.DiscardDocumentOp));

        var writeThrows = new CommandRig { WriteFailure = () => new InvalidOperationException("the writer failed") };
        thrown = writeThrows.Run();
        Assert.IsType<InvalidOperationException>(thrown);
        Assert.Equal(0, writeThrows.Port.Count(CommandAdvisorPortDouble.DiscardDocumentOp));

        // The write of the final text throws AFTER the discard: the discard already happened and only the text is lost.
        var lateWrite = new CommandRig();
        lateWrite.WriteFailure = () => lateWrite.Port.Count(CommandAdvisorPortDouble.DiscardDocumentOp) > 0
            ? new InvalidOperationException("the writer failed late")
            : null;
        thrown = lateWrite.Run();
        Assert.IsType<InvalidOperationException>(thrown);
        Assert.Equal(1, lateWrite.Port.Count(CommandAdvisorPortDouble.DiscardDocumentOp));
    }

    [Fact]
    public void TheDelegatesOfTheSessionAndOfTheTargetAreNotContainedEither()
    {
        var port = new CommandAdvisorPortDouble();
        var written = new List<string>();

        var activeThrows = Record.Exception(() => WorkspaceDocumentDiscardCommand.Run(
            port, () => throw new InvalidOperationException("active"), _ => true, _ => true, written.Add));
        var targetThrows = Record.Exception(() => WorkspaceDocumentDiscardCommand.Run(
            port, () => A, _ => throw new InvalidOperationException("target"), _ => true, written.Add));

        Assert.IsType<InvalidOperationException>(activeThrows);
        Assert.IsType<InvalidOperationException>(targetThrows);
        Assert.Empty(port.Calls);
        Assert.Empty(written);
    }

    [Fact]
    public void ANullDelegateIsRefused()
    {
        var port = new CommandAdvisorPortDouble();

        var active = Record.Exception(() => WorkspaceDocumentDiscardCommand.Run(port, null!, _ => true, _ => true, _ => { }));
        var target = Record.Exception(() => WorkspaceDocumentDiscardCommand.Run(port, () => A, null!, _ => true, _ => { }));
        var confirm = Record.Exception(() => WorkspaceDocumentDiscardCommand.Run(port, () => A, _ => true, null!, _ => { }));
        var write = Record.Exception(() => WorkspaceDocumentDiscardCommand.Run(port, () => A, _ => true, _ => true, null!));

        Assert.IsType<ArgumentNullException>(active);
        Assert.IsType<ArgumentNullException>(target);
        Assert.IsType<ArgumentNullException>(confirm);
        Assert.IsType<ArgumentNullException>(write);
        Assert.Empty(port.Calls);
    }

    // ------------------------------------------------------------------ D-T3C2-04: the wrapper of the advisor, isolated

    private const string DocumentName = "Planta A.dwg";

    private sealed class RelayRig
    {
        public LifecycleScriptedAdvisor Inner { get; } = new();

        public CommandAdvisorPortDouble Port { get; } = new();

        public Func<SessionId, string?> NameOf { get; set; } = _ => @"C:\Dibujos\" + DocumentName;

        public Func<SessionId, bool> IsActive { get; set; } = _ => true;

        public Action<string>? WriteHook { get; set; }

        public Action<string, Exception>? ReportHook { get; set; }

        public bool WithReport { get; set; } = true;

        public List<string> Written { get; } = new();

        public List<(string Context, Exception Error)> Reports { get; } = new();

        public int NameCalls { get; private set; }

        public int ActiveCalls { get; private set; }

        public List<string> Order { get; } = new();

        public WorkspaceCloseAdvisorRelay Build() => new(
            Inner,
            Port,
            id =>
            {
                NameCalls++;
                return NameOf(id);
            },
            id =>
            {
                ActiveCalls++;
                return IsActive(id);
            },
            text =>
            {
                Written.Add(text);
                WriteHook?.Invoke(text);
            },
            WithReport
                ? (context, error) =>
                {
                    Reports.Add((context, error));
                    ReportHook?.Invoke(context, error);
                }
                : null);
    }

    private static string Veto01(string document) =>
        "RackCad: No se cerró " + document + ": puede tener borradores del panel Workspace con trabajo sin aplicar. Descártalos en el panel con \"Descartar borradores de este documento\" o con el comando RACKPANELDESCARTAR, y vuelve a cerrarlo.";

    private static string Veto02(string document) =>
        "RackCad: No se cerró " + document + ": puede tener borradores del panel Workspace con trabajo sin aplicar. Actívalo primero; después descártalos en el panel con \"Descartar borradores de este documento\" o con el comando RACKPANELDESCARTAR, y vuelve a cerrarlo.";

    [Fact]
    public void TheWrapperRefusesANullInnerPortNameActivityOrWriterAndAcceptsANullReport()
    {
        var rig = new RelayRig();
        Func<SessionId, string?> name = _ => null;
        Func<SessionId, bool> active = _ => true;
        Action<string> write = _ => { };

        Assert.IsType<ArgumentNullException>(Record.Exception(() => new WorkspaceCloseAdvisorRelay(null!, rig.Port, name, active, write, null)));
        Assert.IsType<ArgumentNullException>(Record.Exception(() => new WorkspaceCloseAdvisorRelay(rig.Inner, null!, name, active, write, null)));
        Assert.IsType<ArgumentNullException>(Record.Exception(() => new WorkspaceCloseAdvisorRelay(rig.Inner, rig.Port, null!, active, write, null)));
        Assert.IsType<ArgumentNullException>(Record.Exception(() => new WorkspaceCloseAdvisorRelay(rig.Inner, rig.Port, name, null!, write, null)));
        Assert.IsType<ArgumentNullException>(Record.Exception(() => new WorkspaceCloseAdvisorRelay(rig.Inner, rig.Port, name, active, null!, null)));
        Assert.Null(Record.Exception(() => new WorkspaceCloseAdvisorRelay(rig.Inner, rig.Port, name, active, write, null)));
    }

    [Theory]
    [InlineData(CloseDecision.Allow)]
    [InlineData(CloseDecision.Veto)]
    public void TheDecisionOfTheWrapperIsAlwaysTheOneOfTheInnerAdvisorAndItIsAskedOncePerAttempt(CloseDecision answer)
    {
        var rig = new RelayRig();
        rig.Inner.Advise = _ => answer;
        var relay = rig.Build();

        var first = relay.AdviseClose(A);
        var second = relay.AdviseClose(A);

        Assert.Equal(answer, first);
        Assert.Equal(answer, second);
        Assert.Equal(new[] { A, A }, rig.Inner.Advised.ToArray());
        Assert.Equal(answer == CloseDecision.Veto ? 2 : 0, rig.Written.Count);
        Assert.Empty(rig.Port.Calls);
        Assert.Empty(rig.Reports);
    }

    [Fact]
    public void AnInnerAdvisorThatThrowsIsAVetoWithTheNoticeAndTheReportOfThatContext()
    {
        var rig = new RelayRig();
        rig.Inner.Advise = _ => throw new InvalidOperationException("the inner failed");
        var relay = rig.Build();
        var decision = CloseDecision.Allow;

        var thrown = Record.Exception(() => decision = relay.AdviseClose(A));

        Assert.Null(thrown);
        Assert.Equal(CloseDecision.Veto, decision);
        Assert.Single(rig.Inner.Advised);
        var report = Assert.Single(rig.Reports);
        Assert.Equal("WorkspaceCloseAdvisorRelay.AdviseClose", report.Context);
        Assert.Equal("the inner failed", report.Error.Message);
        Assert.Equal(new[] { Veto01("«" + DocumentName + "»") }, rig.Written.ToArray());
    }

    [Fact]
    public void AVetoOfTheActiveDocumentWritesV01AndTheOneOfAnotherDocumentWritesV02()
    {
        var active = new RelayRig();
        active.Inner.Advise = _ => CloseDecision.Veto;
        var other = new RelayRig { IsActive = _ => false };
        other.Inner.Advise = _ => CloseDecision.Veto;

        Assert.Equal(CloseDecision.Veto, active.Build().AdviseClose(A));
        Assert.Equal(CloseDecision.Veto, other.Build().AdviseClose(B));

        Assert.Equal(new[] { Veto01("«" + DocumentName + "»") }, active.Written.ToArray());
        Assert.Equal(new[] { Veto02("«" + DocumentName + "»") }, other.Written.ToArray());
    }

    [Theory]
    [InlineData(@"C:\Dibujos\Planta A.dwg", "Planta A.dwg")]
    [InlineData("C:/Dibujos/Planta A.dwg", "Planta A.dwg")]
    [InlineData(@"D:\Uno/Dos\Tres.dwg", "Tres.dwg")]
    [InlineData("Drawing1.dwg", "Drawing1.dwg")]
    public void TheNameOfTheDocumentIsItsLastSegmentBetweenGuillemets(string full, string shown)
    {
        var rig = new RelayRig { NameOf = _ => full };
        rig.Inner.Advise = _ => CloseDecision.Veto;

        rig.Build().AdviseClose(A);

        Assert.Equal(new[] { Veto01("«" + shown + "»") }, rig.Written.ToArray());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(@"C:\Dibujos\")]
    public void AMissingOrEmptyNameSaysTheDocument(string? name)
    {
        var rig = new RelayRig { NameOf = _ => name };
        rig.Inner.Advise = _ => CloseDecision.Veto;

        rig.Build().AdviseClose(A);

        Assert.Equal(new[] { Veto01("el documento") }, rig.Written.ToArray());
        Assert.Empty(rig.Reports);
    }

    [Fact]
    public void AQueryThatThrowsNeverChangesTheDecisionAndTheNoticeStillSaysTheDocument()
    {
        var rig = new RelayRig
        {
            NameOf = _ => throw new InvalidOperationException("no name"),
            IsActive = _ => throw new InvalidOperationException("no activity"),
        };
        rig.Inner.Advise = _ => CloseDecision.Veto;
        var decision = CloseDecision.Allow;

        var thrown = Record.Exception(() => decision = rig.Build().AdviseClose(A));

        Assert.Null(thrown);
        Assert.Equal(CloseDecision.Veto, decision);
        // A failing activity counts as the active document.
        Assert.Equal(new[] { Veto01("el documento") }, rig.Written.ToArray());
        Assert.Equal(2, rig.Reports.Count);
        Assert.All(rig.Reports, r => Assert.Equal("WorkspaceCloseAdvisorRelay.VetoMessage", r.Context));
        Assert.Equal(new[] { "no name", "no activity" }, rig.Reports.Select(r => r.Error.Message).ToArray());
    }

    [Fact]
    public void AWriterThatThrowsNeverChangesTheDecisionAndIsReportedAsTheVetoMessage()
    {
        var rig = new RelayRig { WriteHook = _ => throw new InvalidOperationException("no command line") };
        rig.Inner.Advise = _ => CloseDecision.Veto;
        var decision = CloseDecision.Allow;

        var thrown = Record.Exception(() => decision = rig.Build().AdviseClose(A));

        Assert.Null(thrown);
        Assert.Equal(CloseDecision.Veto, decision);
        var report = Assert.Single(rig.Reports);
        Assert.Equal("WorkspaceCloseAdvisorRelay.VetoMessage", report.Context);
        Assert.Equal("no command line", report.Error.Message);
    }

    [Fact]
    public void AReportThatThrowsIsIgnoredAndTheWrapperWorksWithoutAReportToo()
    {
        var throwing = new RelayRig { ReportHook = (_, _) => throw new InvalidOperationException("the report failed") };
        throwing.Inner.Advise = _ => throw new InvalidOperationException("the inner failed");
        var silent = new RelayRig { WithReport = false, WriteHook = _ => throw new InvalidOperationException("no command line") };
        silent.Inner.Advise = _ => CloseDecision.Veto;
        var first = CloseDecision.Allow;
        var second = CloseDecision.Allow;

        var thrownFirst = Record.Exception(() => first = throwing.Build().AdviseClose(A));
        var thrownSecond = Record.Exception(() => second = silent.Build().AdviseClose(A));

        Assert.Null(thrownFirst);
        Assert.Null(thrownSecond);
        Assert.Equal(CloseDecision.Veto, first);
        Assert.Equal(CloseDecision.Veto, second);
        Assert.Single(throwing.Written);
    }

    [Fact]
    public void TheWrapperNeverWritesWhenTheInnerAllowsEvenIfTheQueriesWouldFail()
    {
        var rig = new RelayRig { NameOf = _ => throw new InvalidOperationException("never asked") };

        var decision = rig.Build().AdviseClose(A);

        Assert.Equal(CloseDecision.Allow, decision);
        Assert.Empty(rig.Written);
        Assert.Equal(0, rig.NameCalls);
        Assert.Equal(0, rig.ActiveCalls);
        Assert.Empty(rig.Reports);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TheDestructionDelegatesExactlyOnceWithTheIntactFlagAsItCame(bool intact)
    {
        var rig = new RelayRig();
        rig.Port.Risks = _ => Risk(Array.Empty<string>());
        var relay = rig.Build();

        var thrown = Record.Exception(() => relay.SessionDestroyed(A, intact));

        Assert.Null(thrown);
        Assert.Equal(new[] { (A, intact) }, rig.Inner.Destroyed.ToArray());
        Assert.Equal(0, rig.NameCalls);
        Assert.Equal(0, rig.ActiveCalls);
        Assert.Empty(rig.Written);
    }

    [Fact]
    public void TheRiskIsReadBeforeTheDelegationAndTheMessageWaitsWithoutBeingWritten()
    {
        var rig = new RelayRig();
        rig.Port.Risks = _ =>
        {
            rig.Order.Add("risk");
            return Risk(new[] { "R-1", "R-3" });
        };
        rig.Inner.OnDestroyed = (_, _) => rig.Order.Add("inner");
        var relay = rig.Build();

        relay.SessionDestroyed(A, true);

        Assert.Equal(new[] { "risk", "inner" }, rig.Order.ToArray());
        Assert.Empty(rig.Written);
        Assert.Equal(new[] { (CommandAdvisorPortDouble.RisksOp, A) }, rig.Port.Calls.ToArray());
        Assert.Equal(0, rig.NameCalls);
        Assert.Equal(0, rig.ActiveCalls);
        Assert.Equal(
            new[] { "RackCad: Al cerrarse un documento se perdió el trabajo sin aplicar de los borradores del panel Workspace: R-1, R-3." },
            relay.TakeMessages(B).ToArray());
    }

    [Fact]
    public void ALossWithRacksAndUncertainOnesSaysBothAndALossWithOnlyUncertainOnesSaysThat()
    {
        var both = new RelayRig();
        both.Port.Risks = _ => Risk(new[] { "R-1" }, new[] { "R-2", "R-4" });
        var onlyUncertain = new RelayRig();
        onlyUncertain.Port.Risks = _ => Risk(Array.Empty<string>(), new[] { "R-2" });
        var bothRelay = both.Build();
        var uncertainRelay = onlyUncertain.Build();

        bothRelay.SessionDestroyed(A, false);
        uncertainRelay.SessionDestroyed(A, false);

        Assert.Equal(
            new[] { "RackCad: Al cerrarse un documento se perdió el trabajo sin aplicar de los borradores del panel Workspace: R-1. Sin decidir: R-2, R-4." },
            bothRelay.TakeMessages(A).ToArray());
        Assert.Equal(
            new[] { "RackCad: Al cerrarse un documento no se pudo decidir si se perdió trabajo de los borradores del panel Workspace: R-2." },
            uncertainRelay.TakeMessages(A).ToArray());
    }

    [Fact]
    public void AnUnknownRiskIsNeverNothingAtRiskAndIsNeverSilentAndAKnownEmptyRiskSaysNothing()
    {
        var unknown = new RelayRig();
        unknown.Port.Risks = _ => default;
        var empty = new RelayRig();
        empty.Port.Risks = _ => Risk(Array.Empty<string>());
        var unknownRelay = unknown.Build();
        var emptyRelay = empty.Build();

        unknownRelay.SessionDestroyed(A, false);
        emptyRelay.SessionDestroyed(A, true);

        Assert.Equal(
            new[] { "RackCad: Al cerrarse un documento no se pudo decidir si se perdió trabajo de los borradores del panel Workspace." },
            unknownRelay.TakeMessages(A).ToArray());
        Assert.Empty(emptyRelay.TakeMessages(A));
        Assert.Empty(unknown.Reports);
    }

    [Fact]
    public void ARiskReadThatThrowsIsTheUnknownIsReportedAndStillDelegatesExactlyOnce()
    {
        var rig = new RelayRig();
        rig.Port.Risks = _ => throw new InvalidOperationException("the read failed");
        var relay = rig.Build();

        var thrown = Record.Exception(() => relay.SessionDestroyed(A, true));

        Assert.Null(thrown);
        Assert.Single(rig.Inner.Destroyed);
        var report = Assert.Single(rig.Reports);
        Assert.Equal("WorkspaceCloseAdvisorRelay.SessionDestroyed", report.Context);
        Assert.Equal("the read failed", report.Error.Message);
        Assert.Equal(
            new[] { "RackCad: Al cerrarse un documento no se pudo decidir si se perdió trabajo de los borradores del panel Workspace." },
            relay.TakeMessages(A).ToArray());
    }

    [Fact]
    public void ARiskReadThatThrowsWithAReportThatAlsoThrowsStillDelegatesExactlyOnce()
    {
        var rig = new RelayRig { ReportHook = (_, _) => throw new InvalidOperationException("the report failed") };
        rig.Port.Risks = _ => throw new InvalidOperationException("the read failed");
        var relay = rig.Build();

        var thrown = Record.Exception(() => relay.SessionDestroyed(A, false));

        Assert.Null(thrown);
        Assert.Equal(new[] { (A, false) }, rig.Inner.Destroyed.ToArray());
    }

    [Fact]
    public void AnInnerAdvisorThatThrowsWhenTheSessionIsDestroyedIsNotContainedAndTheWorkBeforeItWasDone()
    {
        var rig = new RelayRig();
        rig.Port.Risks = _ => Risk(new[] { "R-1" });
        rig.Inner.OnDestroyed = (_, _) => throw new InvalidOperationException("destroyed boom");
        var relay = rig.Build();

        var thrown = Record.Exception(() => relay.SessionDestroyed(A, true));

        Assert.IsType<InvalidOperationException>(thrown);
        Assert.Equal("destroyed boom", thrown!.Message);
        Assert.Single(rig.Inner.Destroyed);
        Assert.Single(relay.TakeMessages(A));
    }

    [Fact]
    public void TheMessagesWaitForAnActiveSessionAreTakenOnceAndKeepTheirOrder()
    {
        var rig = new RelayRig();
        var reads = 0;
        rig.Port.Risks = _ => Risk(new[] { "R-" + (++reads).ToString() });
        var relay = rig.Build();
        relay.SessionDestroyed(A, false);
        relay.SessionDestroyed(B, false);

        var without = relay.TakeMessages(null);
        var again = relay.TakeMessages(null);
        var taken = relay.TakeMessages(A);
        var afterwards = relay.TakeMessages(A);

        Assert.Empty(without);
        Assert.Empty(again);
        Assert.Equal(2, taken.Count);
        Assert.Contains("R-1.", taken[0]);
        Assert.Contains("R-2.", taken[1]);
        Assert.Empty(afterwards);
        Assert.Empty(rig.Written);
    }

    [Fact]
    public void TheWrapperOnlyEverCallsTheReadOfTheRiskOfThePort()
    {
        var rig = new RelayRig();
        rig.Inner.Advise = _ => CloseDecision.Veto;
        var relay = rig.Build();

        relay.AdviseClose(A);
        relay.SessionDestroyed(A, false);
        relay.TakeMessages(A);

        Assert.Equal(new[] { CommandAdvisorPortDouble.RisksOp }, rig.Port.Operations().ToArray());
        Assert.Single(rig.Port.Calls);
    }

    // ------------------------------------------------------------------ D-T3C2-08: the wrapper over the real bridge core and the real host core

    private sealed class Env
    {
        public Env(bool wire = true)
        {
            Hr = new WorkspaceHostRig().WithDocument(100);
            Hr.Host.OpenPanel();
            Session = Hr.ActiveSession;
            Hr.Idle();
            Hr.Reader.Reads.Clear();
            Hr.Reader.Invalidations.Clear();
            Reader = new EditIntentFakeReader();
            Core.SetEditIntentReader(Reader);
            Port = new CommandAdvisorPortDouble();
            Relay = new WorkspaceCloseAdvisorRelay(
                Core,
                new ForwardingRiskPort(Port, Core),
                id => @"C:\Dibujos\" + DocumentName,
                id => Bridge.ActiveSession == id,
                Written.Add,
                (context, error) => Reports.Add((context, error)));
            if (wire) Bridge.SetLifecycleAdvisor(Relay);
        }

        public WorkspaceHostRig Hr { get; }

        public WorkspaceHostCore Core => Hr.Host;

        public EventBridgeCore Bridge => Hr.Bridge.Core;

        public EditIntentFakeReader Reader { get; }

        public SessionId Session { get; }

        public CommandAdvisorPortDouble Port { get; }

        public WorkspaceCloseAdvisorRelay Relay { get; }

        public List<string> Written { get; } = new();

        public List<(string Context, Exception Error)> Reports { get; } = new();

        public EditIntentRequest Request(string rack = EditIntentKit.RackA) => new(Session, rack, null, null);

        public DraftSession<PushBackPilotDraftState> Pilot(string rack = EditIntentKit.RackA)
        {
            var outcome = Core.TryBeginPilotEdit(Request(rack));
            Assert.Equal(EditIntentStatus.Admitted, outcome.Status);
            var draft = Core.OpenDrafts.Find<PushBackPilotDraftState>(Session, rack);
            Assert.NotNull(draft);
            return draft!;
        }

        public static void MakeDirty(DraftSession<PushBackPilotDraftState> draft) =>
            draft.CurrentDraftState.Inputs.Annotations.AnnotationScale += 100.0;
    }

    // The port that the wrapper receives over the real host: only the read of the risk reaches the host, and every call is recorded.
    private sealed class ForwardingRiskPort : IWorkspacePilotEditPort
    {
        private readonly CommandAdvisorPortDouble _calls;
        private readonly IWorkspacePilotEditPort _host;

        public ForwardingRiskPort(CommandAdvisorPortDouble calls, WorkspaceHostCore host)
        {
            _calls = calls;
            _host = host;
        }

        public PilotRiskView GetRacksAtRisk(WorkspaceRequest origin)
        {
            _calls.Calls.Add((CommandAdvisorPortDouble.RisksOp, origin.Origin));
            return _host.GetRacksAtRisk(origin);
        }

        public EditIntentOutcome TryBeginEdit(EditIntentRequest request) => _calls.TryBeginEdit(request);

        public EditCheckOutcome TryCheckNow(EditIntentRequest request) => _calls.TryCheckNow(request);

        public EditIntentOutcome TryReload(EditIntentRequest request) => _calls.TryReload(request);

        public EditConfirmOutcome TryConfirmBase(EditIntentRequest request) => _calls.TryConfirmBase(request);

        public EditIntentOutcome TryKeepMine(EditIntentRequest request) => _calls.TryKeepMine(request);

        public EditIntentOutcome TryDiscardAndReload(EditIntentRequest request) => _calls.TryDiscardAndReload(request);

        public PilotFieldView GetField(WorkspaceRequest origin, string rackId) => _calls.GetField(origin, rackId);

        public PilotFieldResult SetPendingText(WorkspaceRequest origin, string rackId, string text) => _calls.SetPendingText(origin, rackId, text);

        public PilotFieldResult CommitField(WorkspaceRequest origin, string rackId) => _calls.CommitField(origin, rackId);

        public PilotBoundaryResult CrossBoundary(WorkspaceRequest origin, string rackId, PilotBoundaryKind kind) =>
            _calls.CrossBoundary(origin, rackId, kind);

        public EditDiscardOutcome DiscardRack(WorkspaceRequest origin, string rackId) => _calls.DiscardRack(origin, rackId);

        public EditDiscardOutcome DiscardDocument(WorkspaceRequest origin) => _calls.DiscardDocument(origin);

        public IReadOnlyList<PilotLossView> TakeLossNotices() => _calls.TakeLossNotices();

        public IReadOnlyList<PilotCloseView> TakeCloseNotices() => _calls.TakeCloseNotices();
    }

    [Fact]
    public void OverTheRealBridgeAndHostACleanDocumentIsAllowedAndNothingIsWritten()
    {
        var env = new Env();
        env.Pilot();

        var decision = env.Bridge.OnBeginDocumentClose(env.Session);

        Assert.Equal(CloseDecision.Allow, decision);
        Assert.Empty(env.Written);
        Assert.Empty(env.Reports);
    }

    [Fact]
    public void OverTheRealBridgeAndHostTheDecisionIsTheOneOfTheHostForVetoAndForAllowAndTheDraftIsNeverTouched()
    {
        var env = new Env();
        var draft = env.Pilot();
        Env.MakeDirty(draft);
        var scale = draft.CurrentDraftState.Inputs.Annotations.AnnotationScale;
        var hostAlone = ((IWorkspaceLifecycleAdvisor)env.Core).AdviseClose(env.Session);

        var decision = env.Bridge.OnBeginDocumentClose(env.Session);

        Assert.Equal(CloseDecision.Veto, hostAlone);
        Assert.Equal(CloseDecision.Veto, decision);
        Assert.Equal(new[] { Veto01("«" + DocumentName + "»") }, env.Written.ToArray());
        // The wrapper does not empty live text and discards nothing: the work is exactly where it was.
        Assert.True(draft.IsRegistered);
        Assert.True(draft.IsDirty);
        Assert.Equal(scale, draft.CurrentDraftState.Inputs.Annotations.AnnotationScale);
        Assert.False(env.Bridge.IsDegraded(env.Session));
        Assert.Empty(env.Reports);
        Assert.Empty(env.Hr.Faults);
        Assert.Empty(env.Port.Calls);
    }

    [Fact]
    public void OverTheRealBridgeTheVetoOfAnotherDocumentSaysToActivateItFirst()
    {
        var env = new Env();
        var draft = env.Pilot();
        Env.MakeDirty(draft);
        var other = env.Hr.Activate(200);
        Assert.NotEqual(env.Session, other);
        Assert.Equal(other, env.Bridge.ActiveSession);

        var decision = env.Bridge.OnBeginDocumentClose(env.Session);

        Assert.Equal(CloseDecision.Veto, decision);
        Assert.Equal(new[] { Veto02("«" + DocumentName + "»") }, env.Written.ToArray());
    }

    [Fact]
    public void OverTheRealBridgeAnInnerThatThrowsIsAVetoWithoutDegradingTheSessionAndWithoutAFaultOfTheCore()
    {
        var hr = new WorkspaceHostRig().WithDocument(100);
        hr.Host.OpenPanel();
        var session = hr.ActiveSession;
        var inner = new LifecycleScriptedAdvisor { Advise = _ => throw new InvalidOperationException("advisor boom") };
        var written = new List<string>();
        var reports = new List<(string Context, Exception Error)>();
        var relay = new WorkspaceCloseAdvisorRelay(
            inner, new CommandAdvisorPortDouble(), _ => "Drawing1.dwg", id => hr.Bridge.Core.ActiveSession == id, written.Add, (c, e) => reports.Add((c, e)));
        hr.Bridge.Core.SetLifecycleAdvisor(relay);
        var decision = CloseDecision.Allow;

        var thrown = Record.Exception(() => decision = hr.Bridge.Core.OnBeginDocumentClose(session));

        Assert.Null(thrown);
        Assert.Equal(CloseDecision.Veto, decision);
        Assert.False(hr.Bridge.Core.IsDegraded(session));
        Assert.Empty(hr.Bridge.Faults);
        Assert.Equal("WorkspaceCloseAdvisorRelay.AdviseClose", Assert.Single(reports).Context);
        Assert.Single(written);

        // The advisor is asked anew at the next attempt: a Veto is never remembered.
        inner.Advise = _ => CloseDecision.Allow;
        Assert.Equal(CloseDecision.Allow, hr.Bridge.Core.OnBeginDocumentClose(session));
        Assert.Equal(2, inner.Advised.Count);
    }

    [Fact]
    public void OverTheRealBridgeTheCloseSequenceIntactFlagReachesTheInnerAdvisorAsTheCoreDecidedIt()
    {
        var hr = new WorkspaceHostRig().WithDocument(100);
        hr.Host.OpenPanel();
        var closed = hr.ActiveSession;
        var inner = new LifecycleScriptedAdvisor();
        var relay = new WorkspaceCloseAdvisorRelay(
            inner, new CommandAdvisorPortDouble { Risks = _ => Risk(Array.Empty<string>()) }, _ => null, _ => true, _ => { }, null);
        hr.Bridge.Core.SetLifecycleAdvisor(relay);
        var other = hr.Activate(200);

        Assert.Equal(CloseDecision.Allow, hr.Bridge.Core.OnBeginDocumentClose(closed));
        hr.Bridge.Core.OnDocumentToBeDestroyed(100);
        hr.Bridge.Core.OnDocumentToBeDestroyed(200);

        Assert.Equal(new[] { (closed, true), (other, false) }, inner.Destroyed.ToArray());
    }

    [Fact]
    public void OverTheRealBridgeAndHostALostDocumentIsReadBeforeTheHostRetiresItAndTheMessageWaitsForTheNextActiveSession()
    {
        var env = new Env();
        var draft = env.Pilot();
        Env.MakeDirty(draft);
        var survivor = env.Hr.Activate(200);
        env.Hr.Probe.ActiveDocumentNativeId = 100;
        env.Hr.Bridge.Core.OnDocumentActivated(100, true);
        Assert.Equal(env.Session, env.Bridge.ActiveSession);

        env.Bridge.OnDocumentToBeDestroyed(100);

        // The host noticed the loss itself and the wrapper read the same loss BEFORE the host retired the drafts.
        var notice = Assert.Single(env.Core.LossNotices);
        Assert.Equal(new[] { EditIntentKit.RackA }, notice.LostRackIds.ToArray());
        Assert.False(draft.IsRegistered);
        Assert.Empty(env.Written);
        Assert.Empty(env.Relay.TakeMessages(null));
        Assert.Equal(
            new[] { "RackCad: Al cerrarse un documento se perdió el trabajo sin aplicar de los borradores del panel Workspace: " + EditIntentKit.RackA + "." },
            env.Relay.TakeMessages(survivor).ToArray());
        Assert.Empty(env.Reports);
        Assert.Equal(new[] { CommandAdvisorPortDouble.RisksOp }, env.Port.Operations().ToArray());
    }

    [Fact]
    public void OverTheRealBridgeADestroyedSessionIsNotAskedToTheBridgeNorToTheQueries()
    {
        var env = new Env();
        var nameCalls = 0;
        var activeCalls = 0;
        var relay = new WorkspaceCloseAdvisorRelay(
            env.Core,
            new ForwardingRiskPort(env.Port, env.Core),
            id =>
            {
                nameCalls++;
                return null;
            },
            id =>
            {
                activeCalls++;
                return env.Bridge.ActiveSession == id;
            },
            env.Written.Add,
            null);
        env.Bridge.SetLifecycleAdvisor(relay);
        Env.MakeDirty(env.Pilot());

        env.Bridge.OnDocumentToBeDestroyed(100);

        Assert.Equal(0, nameCalls);
        Assert.Equal(0, activeCalls);
        Assert.Single(env.Core.LossNotices);
        Assert.False(env.Bridge.TryGetSession(env.Session, out _));
    }

    [Fact]
    public void WithoutWiringTheWrapperTheRealBridgeStillDecidesAsTheHostAndWritesNothing()
    {
        var env = new Env(wire: false);
        Env.MakeDirty(env.Pilot());
        env.Bridge.SetLifecycleAdvisor(env.Core);

        var decision = env.Bridge.OnBeginDocumentClose(env.Session);

        Assert.Equal(CloseDecision.Veto, decision);
        Assert.Empty(env.Written);
    }
}
