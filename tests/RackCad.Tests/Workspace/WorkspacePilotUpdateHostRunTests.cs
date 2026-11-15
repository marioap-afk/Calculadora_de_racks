#nullable enable
using System;
using System.Linq;
using System.Reflection;
using RackCad.Application.Systems.PushBack.Pilot;
using Xunit;

namespace RackCad.Tests;

// I-64 F4-T4C1-HOST (D-T4C-04, D-T4C-17; INV-F4T4C-02, 04, 05, 06, 07 and 19): the runner of ONE request of the update of the pilot, in Core,
// with the doubles of the primitives, of the target and of the run. Nothing here loads AutoCAD. Every expectation that something throws, or
// does not, uses Record.Exception, so a behavior that is missing fails by ASSERTION and never by an escaped exception.
public class WorkspacePilotUpdateHostRunTests
{
    private static int At(System.Collections.Generic.List<string> events, string name) => events.IndexOf(name);

    // ------------------------------------------------------------------ the order of the steps (test 1, 5, 7)

    [Fact]
    public void TheRightDocumentIsAdmittedAndEachStepRunsOnceInTheOrderOfThePackage()
    {
        var kit = new PilotHostKit();

        Assert.True(kit.Request(out var failure), "the request was refused: " + failure);

        Assert.Equal(1, kit.Primitives.Enters);
        Assert.Equal(1, kit.Primitives.Opens);
        Assert.Equal(1, kit.Target.Locks);
        Assert.Equal(1, kit.Target.CreateWriters);
        Assert.Equal(1, kit.Run.Executes);
        Assert.Same(kit.Target.Writer, kit.Run.Port);
        Assert.Equal(
            new[] { "Enter", "TryOpenTarget", "Admit", "Lock", "Begin", "CreateWriter", "Execute", "Retire", "Unlock", "Completed" },
            kit.Events.ToArray());
    }

    [Fact]
    public void TheLockIsTakenOnceOnTheTargetResolvedByTheAdmittedIdentityAndBeforeTheOwnMutationTheWriterAndTheUpdate()
    {
        var kit = new PilotHostKit();
        kit.Request(out _);

        Assert.Equal(1, kit.Count("Lock"));
        Assert.True(At(kit.Events, "TryOpenTarget") < At(kit.Events, "Admit"));
        Assert.True(At(kit.Events, "Admit") < At(kit.Events, "Lock"));
        Assert.True(At(kit.Events, "Lock") < At(kit.Events, "Begin"));
        Assert.True(At(kit.Events, "Begin") < At(kit.Events, "CreateWriter"));
        Assert.True(At(kit.Events, "CreateWriter") < At(kit.Events, "Execute"));
        // The lock is released last, after the writer was retired.
        Assert.True(At(kit.Events, "Execute") < At(kit.Events, "Retire"));
        Assert.True(At(kit.Events, "Retire") < At(kit.Events, "Unlock"));
        Assert.Equal(1, kit.Target.LastLock!.Disposed);
    }

    [Fact]
    public void TheWriterIsRetiredOnceAfterTheUpdateAndItIsTheOneThatWasCreated()
    {
        var kit = new PilotHostKit();
        kit.Request(out _);

        Assert.Equal(1, kit.Target.Retires);
        Assert.Same(kit.Target.Writer, kit.Target.Retired);
        Assert.True(At(kit.Events, "Execute") < At(kit.Events, "Retire"));
    }

    [Fact]
    public void TheWriterIsCreatedOnceEvenWhenTheHostRunsTheCallbackAgainAndTheSecondOneOnlyLogs()
    {
        var kit = new PilotHostKit();
        kit.Primitives.RepeatCallback = true;

        Assert.True(kit.Request(out _));

        Assert.Equal(1, kit.Target.CreateWriters);
        Assert.Equal(1, kit.Run.Executes);
        Assert.Equal(1, kit.Target.Locks);
        Assert.Equal(1, kit.Target.Retires);
        Assert.Contains(kit.Logged, l => l.Context.Contains("OnCallback", StringComparison.Ordinal));
    }

    // ------------------------------------------------------------------ the document (tests 2 and 3)

    [Fact]
    public void ADifferentOrAClosedDocumentIsRejectedBeforeTheWriterIsCreatedAndBeforeTheLock()
    {
        foreach (var active in new long?[] { 999, null })
        {
            var kit = new PilotHostKit();
            kit.Primitives.ActiveNativeId = active;

            Assert.True(kit.Request(out _));

            Assert.Equal(0, kit.Target.Locks);
            Assert.Equal(0, kit.Target.CreateWriters);
            Assert.Equal(0, kit.Run.Executes);
            Assert.Equal(0, kit.Run.AdmitCalls);
            var notRun = Assert.Single(kit.Run.NotRuns);
            Assert.Equal(PushBackPilotHostFailure.Rejected, notRun.Kind);
            Assert.Equal(PushBackPilotUpdateRun.DocumentMismatchReason, notRun.Cause);
        }
    }

    [Fact]
    public void ARecycledIdentityFindsTheNewDocumentAndTheAdmissionOfTheRunStopsItBeforeTheLock()
    {
        var kit = new PilotHostKit();
        var recycled = new PilotHostTargetDouble(kit.Events, new PilotUpdateScopeDouble());
        kit.Primitives.TargetNativeId = 55;
        kit.Primitives.RecycledTarget = recycled;
        kit.Primitives.ActiveNativeId = PilotHostKit.DocumentId;
        kit.Primitives.Target = kit.Target;
        // The document of the identity admitted (100) is now another one: the session of the run is no longer valid for it.
        kit.Run.Admits = false;
        kit.Run.AdmitReason = "the witness is not the current one";

        Assert.True(kit.Request(out _));

        Assert.Equal(1, kit.Run.AdmitCalls);
        Assert.Equal(0, recycled.Locks);
        Assert.Equal(0, kit.Target.Locks);
        var notRun = Assert.Single(kit.Run.NotRuns);
        Assert.Equal(PushBackPilotHostFailure.Rejected, notRun.Kind);
        Assert.Equal("the witness is not the current one", notRun.Cause);
        Assert.Equal(0, kit.Run.Begins);
    }

    [Fact]
    public void ARefusedAdmissionIsTheTerminalCallWithItsReasonAndNothingElseRuns()
    {
        var kit = new PilotHostKit();
        kit.Run.Admits = false;
        kit.Run.AdmitReason = "modal";

        Assert.True(kit.Request(out _));

        Assert.Equal(0, kit.Target.Locks);
        Assert.Equal(0, kit.Run.Begins);
        Assert.Equal(0, kit.Run.Executes);
        Assert.Equal(1, kit.Run.Terminals);
        Assert.Equal(("modal"), Assert.Single(kit.Run.NotRuns).Cause);
    }

    // ------------------------------------------------------------------ the command context (tests 4 and 10)

    [Fact]
    public void TheCommandContextIsEnteredOncePerRequestAndNeverByTheCallback()
    {
        var kit = new PilotHostKit();
        kit.Primitives.Mode = PilotHostMode.Deferred;

        Assert.True(kit.Request(out _));
        Assert.Equal(1, kit.Primitives.Enters);
        Assert.Equal(0, kit.Run.AdmitCalls);

        kit.Primitives.RunDeferred();
        Assert.Equal(1, kit.Primitives.Enters);
        Assert.Equal(1, kit.Run.Executes);
    }

    [Fact]
    public void ARequestThatThrowsBeforeTheCallbackIsFalseWithItsCauseAndMakesNoTerminalCall()
    {
        var kit = new PilotHostKit();
        kit.Primitives.ThrowOnEnter = true;

        var requested = kit.Request(out var failure);

        Assert.False(requested);
        Assert.StartsWith(PushBackPilotHostTexts.HostRequestFailedPrefix, failure, StringComparison.Ordinal);
        Assert.Contains("enter boom", failure, StringComparison.Ordinal);
        Assert.Equal(0, kit.Run.Terminals);
        Assert.Empty(kit.Run.NotRuns);
        Assert.Equal(0, kit.Run.Executes);
        Assert.Equal(0, kit.Target.Locks);
    }

    [Fact]
    public void AFailedRequestEndsTheRunnerSoACallbackThatWasQueuedAnywayDoesNothing()
    {
        var kit = new PilotHostKit();
        kit.Primitives.ThrowAfterRegistering = true;

        var requested = kit.Request(out var failure);

        Assert.False(requested);
        Assert.StartsWith(PushBackPilotHostTexts.HostRequestFailedPrefix, failure, StringComparison.Ordinal);
        Assert.NotNull(kit.Primitives.Callback);
        Assert.Null(Record.Exception(() => kit.Primitives.Callback!()));
        Assert.Equal(0, kit.Run.AdmitCalls);
        Assert.Equal(0, kit.Run.Terminals);
        Assert.Equal(0, kit.Target.Locks);
    }

    [Fact]
    public void ANativeRefusalNeverRunsTheCallbackNorCompletesAndTheRunnerMakesNoCall()
    {
        var kit = new PilotHostKit();
        kit.Primitives.Mode = PilotHostMode.Never;

        Assert.True(kit.Request(out var failure), "the request was refused: " + failure);

        Assert.Equal(1, kit.Primitives.Enters);
        Assert.Equal(0, kit.Run.AdmitCalls);
        Assert.Equal(0, kit.Run.Terminals);
        Assert.Equal(0, kit.Run.Completions);
    }

    [Fact]
    public void ARequestThatThrowsAfterASynchronousCallbackThatWroteIsTrueAndNeverMakesANotRun()
    {
        var kit = new PilotHostKit();
        kit.Primitives.ThrowAfterSynchronousCallback = true;

        var requested = kit.Request(out _);

        Assert.True(requested);
        Assert.Equal(1, kit.Run.Executes);
        Assert.Empty(kit.Run.NotRuns);
        Assert.Contains(kit.Logged, l => l.Context.Contains("Request", StringComparison.Ordinal));
    }

    // ------------------------------------------------------------------ the failures of the host before the update (tests 8 and 11)

    [Fact]
    public void ALockThatThrowsNeverReachesTheOwnMutationTheWriterOrTheUpdateAndIsAFailedWithItsPrefix()
    {
        var kit = new PilotHostKit();
        kit.Target.ThrowOnLock = true;

        Assert.True(kit.Request(out _));

        Assert.Equal(0, kit.Run.Begins);
        Assert.Equal(0, kit.Target.CreateWriters);
        Assert.Equal(0, kit.Run.Executes);
        Assert.Equal(0, kit.Target.Retires);
        var notRun = Assert.Single(kit.Run.NotRuns);
        Assert.Equal(PushBackPilotHostFailure.Failed, notRun.Kind);
        Assert.StartsWith(PushBackPilotHostTexts.LockFailedPrefix, notRun.Cause, StringComparison.Ordinal);
        Assert.Equal(0, kit.Count("Unlock"));
    }

    [Fact]
    public void AnOwnMutationThatThrowsIsAFailedBeforeTheUpdateAndTheLockIsStillReleased()
    {
        var kit = new PilotHostKit();
        kit.Run.ThrowAt.Add("Begin");

        Assert.True(kit.Request(out _));

        Assert.Equal(0, kit.Target.CreateWriters);
        Assert.Equal(0, kit.Run.Executes);
        Assert.Equal(PushBackPilotHostFailure.Failed, Assert.Single(kit.Run.NotRuns).Kind);
        Assert.Equal(1, kit.Target.LastLock!.Disposed);
        Assert.Equal(0, kit.Target.Retires);
    }

    [Fact]
    public void AWriterThatCannotBeCreatedIsAFailedAndRetireIsNeverCalledOnANullWriter()
    {
        var kit = new PilotHostKit();
        kit.Target.ThrowOnCreateWriter = true;

        Assert.True(kit.Request(out _));

        Assert.Equal(0, kit.Run.Executes);
        Assert.Equal(0, kit.Target.Retires);
        Assert.Null(kit.Target.Retired);
        Assert.Equal(PushBackPilotHostFailure.Failed, Assert.Single(kit.Run.NotRuns).Kind);
        Assert.Equal(1, kit.Target.LastLock!.Disposed);
    }

    [Fact]
    public void AnUpdateThatThrowsStillRetiresTheWriterOnceAndReleasesTheLockAndNeverBecomesANotRun()
    {
        var kit = new PilotHostKit();
        kit.Run.ThrowAt.Add("Execute");

        Assert.True(kit.Request(out _));

        Assert.Equal(1, kit.Run.Executes);
        Assert.Equal(1, kit.Target.Retires);
        Assert.Equal(1, kit.Target.LastLock!.Disposed);
        Assert.Empty(kit.Run.NotRuns);
    }

    [Fact]
    public void AnAdmissionOrAnOpenThatThrowsIsTheTerminalFailedCallAndNothingElseRuns()
    {
        var admit = new PilotHostKit();
        admit.Run.ThrowAt.Add("Admit");
        Assert.True(admit.Request(out _));
        Assert.Equal(0, admit.Target.Locks);
        Assert.Equal(PushBackPilotHostFailure.Failed, Assert.Single(admit.Run.NotRuns).Kind);

        var open = new PilotHostKit();
        open.Primitives.ThrowOnOpen = true;
        Assert.True(open.Request(out _));
        Assert.Equal(0, open.Run.AdmitCalls);
        Assert.Equal(PushBackPilotHostFailure.Failed, Assert.Single(open.Run.NotRuns).Kind);
    }

    // ------------------------------------------------------------------ the terminal latch (N-01, M-10, R-02)

    [Fact]
    public void AfterATrueAdmissionExactlyOneTerminalCallIsMadeAlsoWhenTheRetirementOrTheUnlockThrow()
    {
        foreach (var (retire, unlock, begin) in new[] { (true, false, false), (false, true, false), (true, true, false), (false, true, true) })
        {
            var kit = new PilotHostKit();
            kit.Target.ThrowOnRetire = retire;
            kit.Target.ThrowOnUnlock = unlock;
            if (begin) kit.Run.ThrowAt.Add("Begin");

            Assert.True(kit.Request(out _));

            Assert.Equal(1, kit.Run.AdmitCalls);
            Assert.Equal(1, kit.Run.Terminals);
        }
    }

    [Fact]
    public void TheCallbackNeverThrowsWhenTheRunTheNotRunOrTheLogThrow()
    {
        var kit = new PilotHostKit();
        kit.Primitives.Mode = PilotHostMode.Deferred;
        kit.Run.ThrowAt.Add("Execute");
        kit.Run.ThrowAt.Add("NotRun");
        kit.Run.ThrowAt.Add("Completed");
        Assert.True(kit.Request(out _));

        Assert.Null(Record.Exception(() => kit.Primitives.RunCallback()));
        Assert.Null(Record.Exception(() => kit.Primitives.Complete()));

        var refused = new PilotHostKit();
        refused.Primitives.Mode = PilotHostMode.Deferred;
        refused.Run.Admits = false;
        refused.Run.ThrowAt.Add("NotRun");
        var logThrows = 0;
        Assert.True(PushBackPilotHostRunner.Request(PilotHostKit.DocumentId, refused.Run, refused.Primitives, (_, _) => { logThrows++; throw new InvalidOperationException("log boom"); }, out _));
        Assert.Null(Record.Exception(() => refused.Primitives.RunCallback()));
        Assert.True(logThrows > 0);
    }

    [Fact]
    public void TheCompletionIsPassedOnOnceAndASecondOneDoesNothing()
    {
        var kit = new PilotHostKit();
        kit.Primitives.Mode = PilotHostMode.Deferred;
        Assert.True(kit.Request(out _));
        kit.Primitives.RunCallback();

        kit.Primitives.Complete();
        kit.Primitives.Complete();

        Assert.Equal(1, kit.Run.Completions);
    }

    [Fact]
    public void ACompletionThatArrivesWithoutACallbackIsStillPassedOnOnceAndTheLateCallbackDoesNothing()
    {
        var kit = new PilotHostKit();
        kit.Primitives.Mode = PilotHostMode.Deferred;
        Assert.True(kit.Request(out _));

        kit.Primitives.Complete();
        kit.Primitives.RunCallback();

        Assert.Equal(1, kit.Run.Completions);
        Assert.Equal(0, kit.Run.Executes);
        Assert.Equal(0, kit.Run.AdmitCalls);
        Assert.Equal(0, kit.Target.Locks);
    }

    [Fact]
    public void ADeferredRunExecutesOnlyWhenTheCallbackArrivesAndCompletesLater()
    {
        var kit = new PilotHostKit();
        kit.Primitives.Mode = PilotHostMode.Deferred;
        Assert.True(kit.Request(out _));
        Assert.Equal(new[] { "Enter" }, kit.Events.ToArray());

        kit.Primitives.RunCallback();
        Assert.Equal(1, kit.Run.Executes);
        Assert.Equal(0, kit.Run.Completions);

        kit.Primitives.Complete();
        Assert.Equal(1, kit.Run.Completions);
    }

    // ------------------------------------------------------------------ the shape of the runner and of its texts

    [Fact]
    public void TheRunnerIsCreatedOnlyByItsStaticRequestAndNoLongLivedTypeCanBuildIt()
    {
        var type = typeof(PushBackPilotHostRunner);

        Assert.Empty(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        Assert.NotEmpty(type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.True(type.IsSealed);
        Assert.NotNull(type.GetMethod("Request", BindingFlags.Public | BindingFlags.Static));
        Assert.Empty(type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(f => f.FieldType == type));
    }

    [Fact]
    public void TheHostTextsHaveTheExactWordingThatTheSessionFixedAtTheEmission()
    {
        Assert.Equal("Hay una actualización en curso: espera a que termine.", PushBackPilotHostTexts.UpdateInFlightReason);
        Assert.Equal("La actualización no llegó a ejecutarse; no se escribió nada en el dibujo.", PushBackPilotHostTexts.HostNotRunReason);
        Assert.Equal("No se pudo pedir el contexto de comando: ", PushBackPilotHostTexts.HostRequestFailedPrefix);
        Assert.Equal("No se pudo bloquear el documento: ", PushBackPilotHostTexts.LockFailedPrefix);
        Assert.Equal("La actualización no está disponible en esta sesión.", PushBackPilotHostTexts.HostUnavailableReason);
        Assert.Equal("El borrador no aceptó el cierre de la actualización.", PushBackPilotHostTexts.UpdateNotClosedReason);
    }

    [Fact]
    public void TheThreeTextsOfI01AreConstantsOfTheRunWithTheWordingThatTheHostHadBefore()
    {
        Assert.Equal("La actualización no llegó a ejecutarse.", PushBackPilotUpdateRun.UpdateNotRunReason);
        Assert.Equal("No hay un documento activo para actualizar.", PushBackPilotUpdateRun.NoActiveDocumentReason);
        Assert.Equal("Hay una ventana de RackCad abierta: ciérrala antes de actualizar.", PushBackPilotUpdateRun.ModalOpenReason);
    }

    [Fact]
    public void TheContractsOfTheRunnerHaveTheOperationsOfThePackage()
    {
        Assert.Equal(
            new[] { "Admit", "Begin", "Completed", "Execute", "NotRun" },
            typeof(IPushBackPilotHostRun).GetMethods().Select(m => m.Name).OrderBy(n => n, StringComparer.Ordinal).ToArray());
        Assert.Equal(new[] { "CreateWriter", "Lock", "Retire" }, typeof(IPushBackPilotHostTarget).GetMethods().Select(m => m.Name).OrderBy(n => n, StringComparer.Ordinal).ToArray());
        Assert.Equal(new[] { "EnterCommandContext", "TryOpenTarget" }, typeof(IPushBackPilotHostPrimitives).GetMethods().Select(m => m.Name).OrderBy(n => n, StringComparer.Ordinal).ToArray());
        Assert.Equal(new[] { "TryRequest" }, typeof(IPushBackPilotUpdateHost).GetMethods().Select(m => m.Name).ToArray());
        Assert.Equal(new[] { "Rejected", "Failed" }, Enum.GetNames<PushBackPilotHostFailure>());
    }
}
