using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.FlowBed;
using Xunit;

namespace RackCad.Tests;

/// <summary>
/// T6 at (I-64, sexta tanda): the plan of «Actualizar todo» in Application, without AutoCAD (the A-14, C-05): who goes and who prevents it
/// (condition 4, adapted), in which order (Variables first, decision Q-02), all or nothing over a double of the host (a refusal of the
/// re-read, a failure of a write, a cancellation or a confirmation that fails leave zero writes, or say nobody knows), the re-read of every
/// draft inside the one transaction BEFORE any write (condition 1), and the list where every draft has its cause or «No aplicado: falló otro
/// rack del conjunto». The command itself is AutoCAD code: its shape is fixed by its source at the end.
/// </summary>
public sealed class UpdateAllRunTests
{
    private static readonly SessionId Session = new(7);

    // ------------------------------------------------------------------ the drafts

    private static UpdateAllItem Dynamic(string rackId, string name = null)
        => new(UpdateAllSystem.Dynamic, rackId, name ?? rackId, Dynamic: new RackDoorWrite(1, Session, rackId, name ?? rackId, new DynamicRackDesign(), new DynamicRackSystem()));

    private static UpdateAllItem Cama(string rackId)
        => new(UpdateAllSystem.Cama, rackId, rackId, Cama: new CamaDoorWrite(1, Session, rackId, rackId, new FlowBedConfiguration()));

    private static UpdateAllItem Cabecera(string rackId)
        => new(UpdateAllSystem.Cabecera, rackId, rackId, Cabecera: new CabeceraDoorWrite(1, Session, rackId, rackId, new RackFrameConfiguration()));

    private static UpdateAllItem Variables()
        => new(UpdateAllSystem.Variables, null, "Variables", Variables: new VariablesDoorWrite(1, Session, "f", null, "p", null));

    private static UpdateAllWrite Write(params UpdateAllItem[] items) => new(1, Session, items);

    /// <summary>The host as the composition sees it, recording every call in order.</summary>
    private sealed class Host : IUpdateAllUnitPort
    {
        public List<string> Calls { get; } = new();

        public HashSet<string> Refuse { get; } = new(StringComparer.OrdinalIgnoreCase);

        public HashSet<string> Fail { get; } = new(StringComparer.OrdinalIgnoreCase);

        public HashSet<string> Throw { get; } = new(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, string> Warn { get; } = new(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyList<string> VariablesRedraw { get; set; } = Array.Empty<string>();

        public bool CommitThrows { get; set; }

        public bool BeginThrows { get; set; }

        public bool FinishThrows { get; set; }

        public int CancelAfterApplies { get; set; } = -1;

        /// <summary>The blocks of the library a draft needs that the drawing lacks, until they are imported (review of c4985a2e, I-2).</summary>
        public Dictionary<string, string[]> Lacking { get; } = new(StringComparer.OrdinalIgnoreCase);

        public string ImportError { get; set; }

        /// <summary>The import «works» but the drawing still lacks them (the library could not give them).</summary>
        public bool ImportGivesNothing { get; set; }

        public int Applies { get; private set; }

        public bool Open { get; private set; }

        public string ImportOutside(IReadOnlyCollection<string> blocks)
        {
            Assert.False(Open, "the library is imported with no transaction open");
            Calls.Add("import " + string.Join(",", blocks));
            if (ImportError != null) return ImportError;
            if (!ImportGivesNothing) Lacking.Clear();
            return null;
        }

        private static string Key(UpdateAllItem item) => item.RackId ?? "Variables";

        public void Begin()
        {
            if (BeginThrows) throw new InvalidOperationException("sin dibujo");
            Open = true;
            Calls.Add("begin");
        }

        public UpdateAllCheck Preflight(UpdateAllItem item)
        {
            Assert.True(Open, "the re-read is inside the one transaction");
            Calls.Add("check " + Key(item));
            if (Throw.Contains("check " + Key(item))) throw new InvalidOperationException("lectura rota");
            if (Refuse.Contains(Key(item))) return UpdateAllCheck.Refused("El rack ya no está en el dibujo");
            return UpdateAllCheck.Ready(Key(item), item.System == UpdateAllSystem.Variables ? VariablesRedraw : null);
        }

        public bool CancelRequested() => CancelAfterApplies >= 0 && Applies >= CancelAfterApplies;

        public UpdateAllApplied Apply(UpdateAllItem item, UpdateAllCheck check)
        {
            Assert.True(Open, "the writes are inside the one transaction");
            Assert.Equal(Key(item), check.State); // what its re-read kept
            Calls.Add("apply " + Key(item));
            Applies++;
            if (Throw.Contains("apply " + Key(item))) throw new InvalidOperationException("eInvalidInput");
            if (Fail.Contains(Key(item))) return UpdateAllApplied.Failed("AutoCAD no redibujó ninguna vista del rack, así que el borrador sigue sin escribir");
            if (item.System == UpdateAllSystem.Variables)
                return new UpdateAllApplied(true, null, VariablesAnswer: VariablesDoorRules.AnswerForWrite(1, true, VariablesRedraw, 2, false, null));
            Warn.TryGetValue(Key(item), out var warning);
            Lacking.TryGetValue(Key(item), out var lacking);
            return new UpdateAllApplied(true, null, warning, RackDoorRules.AnswerForApply(1, item.RackId, 2, 0), PendingImports: lacking);
        }

        public void Commit()
        {
            Calls.Add("commit");
            if (CommitThrows) throw new InvalidOperationException("eLockViolation");
            Open = false;
        }

        public void Rollback()
        {
            Calls.Add("rollback");
            Open = false;
        }

        public void Progress(string step, int done, int total, UpdateAllItem item)
        {
        }

        public void Finish(bool committed)
        {
            Calls.Add("finish " + (committed ? "committed" : "discarded"));
            if (FinishThrows) throw new InvalidOperationException("regen");
        }

        public int Writes => Calls.Count(c => c.StartsWith("apply ", StringComparison.Ordinal));
    }

    // ------------------------------------------------------------------ elegibility (condition 4, adapted)

    private static UpdateAllCandidate Candidate(UpdateAllSystem system, string rackId, string name, bool dirty = true, bool writing = false, bool isNew = false,
        string blocked = null, bool stale = false) => new(system, rackId, name, dirty, writing, isNew, blocked, stale);

    [Fact]
    public void ModifiedDraftsGoCleanOnesAndNewOnesDoNotAndVariablesGoFirstThenTheSystemsOfTheListThenTheName()
    {
        var result = UpdateAllEligibility.Decide(new[]
        {
            Candidate(UpdateAllSystem.Cama, "C-1", "Cama 1"),
            Candidate(UpdateAllSystem.Dynamic, "D-2", "DIN 10"),
            Candidate(UpdateAllSystem.Dynamic, "D-1", "DIN 2"),
            Candidate(UpdateAllSystem.Selective, "S-1", "Selectivo A", dirty: false),
            Candidate(UpdateAllSystem.PushBack, "P-9", "Borrador 1", isNew: true),
            Candidate(UpdateAllSystem.PushBack, "P-1", "PB 1"),
            Candidate(UpdateAllSystem.Variables, null, "Variables"),
            Candidate(UpdateAllSystem.Cabecera, "K-1", "Cabecera 1", dirty: false),
            Candidate(UpdateAllSystem.Cantilever, "L-1", "Cantilever 1"),
        });

        Assert.True(result.CanRun);
        Assert.Equal(6, result.Count);
        Assert.Empty(result.Blockers);
        Assert.Equal(new[] { "Variables", "PB 1", "DIN 10", "DIN 2", "Cantilever 1", "Cama 1" }, result.Included.Select(c => c.Name).ToArray());
        Assert.Equal(string.Empty, result.BlockedText);
        Assert.False(result.NeedsConfirmation);
        Assert.Equal("Actualizar todo (6)", UpdateAllText.Label(result.Count));
    }

    [Fact]
    public void NothingWithChangesSaysSoAndADraftThatIsWritingOrBlockedPreventsItWithItsCause()
    {
        var empty = UpdateAllEligibility.Decide(new[] { Candidate(UpdateAllSystem.Dynamic, "D-1", "DIN 1", dirty: false) });
        Assert.False(empty.CanRun);
        Assert.Equal(UpdateAllText.NothingToWrite, empty.BlockedText);

        var blocked = UpdateAllEligibility.Decide(new[]
        {
            Candidate(UpdateAllSystem.Dynamic, "D-1", "DIN 1"),
            Candidate(UpdateAllSystem.Cama, "C-1", "Cama 1", blocked: "Corrige el valor marcado para actualizar."),
            Candidate(UpdateAllSystem.PushBack, "P-1", "PB 1", dirty: false, writing: true),
        });
        Assert.False(blocked.CanRun);
        Assert.Equal(1, blocked.Count);
        Assert.Equal(new[] { "PB 1", "Cama 1" }, blocked.Blockers.Select(b => b.Candidate.Name).ToArray());
        Assert.Equal("«Actualizar todo» no puede escribir mientras estos borradores no se corrijan: «PB 1»: AutoCAD lo está escribiendo; espera a que termine.; "
                     + "«Cama 1»: Corrige el valor marcado para actualizar. No se escribiría nada.", blocked.BlockedText);
    }

    [Fact]
    public void ADraftMarkedPossiblyStaleIsIncludedAndAsksFirst()
    {
        var result = UpdateAllEligibility.Decide(new[]
        {
            Candidate(UpdateAllSystem.Dynamic, "D-1", "DIN 1", stale: true),
            Candidate(UpdateAllSystem.Dynamic, "D-2", "DIN 2"),
            Candidate(UpdateAllSystem.Dynamic, "D-3", "DIN 3", dirty: false, stale: true),
        });

        Assert.True(result.CanRun);
        Assert.Equal(2, result.Count);
        Assert.True(result.NeedsConfirmation);
        Assert.Equal("DIN 1", Assert.Single(result.PossiblyStale).Name);
    }

    // ------------------------------------------------------------------ all or nothing

    [Fact]
    public void EveryDraftIsReReadInsideTheOneTransactionBeforeAnyWriteThenWrittenInOrderAndConfirmedOnce()
    {
        var host = new Host();
        var answer = UpdateAllRun.Execute(Write(Variables(), Dynamic("D-1", "DIN 1"), Cama("C-1")), host);

        Assert.Equal(new[] { "begin", "check Variables", "check D-1", "check C-1", "apply Variables", "apply D-1", "apply C-1", "commit", "finish committed" }, host.Calls);
        Assert.True(answer.Committed);
        Assert.Equal("AutoCAD escribió 3 borradores en una sola operación. Un «U» en AutoCAD lo deshace todo.", answer.Message);
        Assert.All(answer.Items, r => Assert.Equal(UpdateAllOutcome.Applied, r.Outcome));
        var dynamic = answer.Items[1];
        Assert.True(dynamic.Written.Written);
        Assert.Equal("«Actualizar todo»: AutoCAD redibujó 2 vistas del rack con el borrador; la línea de órdenes da el recuento por tipo de vista.", dynamic.Written.Message);
        Assert.True(answer.Items[0].VariablesWritten.Written);
    }

    [Fact]
    public void ARefusalOfTheReReadWritesNothingAndEveryRefusalIsNamedWithItsCause()
    {
        var host = new Host();
        host.Refuse.Add("C-1");
        host.Throw.Add("check K-1");
        var answer = UpdateAllRun.Execute(Write(Dynamic("D-1", "DIN 1"), Cama("C-1"), Cabecera("K-1"), Dynamic("D-2", "DIN 2")), host);

        Assert.Equal(0, host.Writes);
        Assert.DoesNotContain("commit", host.Calls);
        Assert.Equal(new[] { "check D-1", "check C-1", "check K-1", "check D-2" }, host.Calls.Where(c => c.StartsWith("check", StringComparison.Ordinal)).ToArray());
        Assert.Equal("finish discarded", host.Calls.Last());
        Assert.False(answer.Committed);
        Assert.Equal("No se escribió nada: no se pueden escribir «C-1» y «K-1». La lista dice por qué.", answer.Message);
        Assert.Equal(new[] { UpdateAllOutcome.NotAppliedOtherFailed, UpdateAllOutcome.Failed, UpdateAllOutcome.Failed, UpdateAllOutcome.NotAppliedOtherFailed },
            answer.Items.Select(r => r.Outcome).ToArray());
        Assert.Equal("El rack ya no está en el dibujo", answer.Items[1].Cause);
        Assert.Equal("No se pudo comprobar: lectura rota", answer.Items[2].Cause);
        Assert.Equal(UpdateAllText.OtherFailed, answer.Items[0].Cause);
        Assert.All(answer.Items, r => Assert.False(r.Written.Written));
        Assert.Equal("No aplicado: falló otro rack del conjunto. El borrador sigue sin escribir.", answer.Items[3].Written.Message);
        Assert.Equal("«Actualizar todo» no escribió nada: este borrador no se pudo escribir. El rack ya no está en el dibujo. El borrador sigue sin escribir.",
            answer.Items[1].Written.Message);
    }

    [Fact]
    public void AWriteThatFailsDiscardsTheOnesBeforeItAndNoneAfterItIsWritten()
    {
        foreach (var how in new[] { "fail", "throw" })
        {
            var host = new Host();
            if (how == "fail") host.Fail.Add("C-1");
            else host.Throw.Add("apply C-1");
            var answer = UpdateAllRun.Execute(Write(Dynamic("D-1", "DIN 1"), Cama("C-1"), Dynamic("D-2", "DIN 2")), host);

            Assert.Equal(new[] { "apply D-1", "apply C-1" }, host.Calls.Where(c => c.StartsWith("apply", StringComparison.Ordinal)).ToArray());
            Assert.Contains("rollback", host.Calls);
            Assert.DoesNotContain("commit", host.Calls);
            Assert.False(answer.Committed);
            Assert.Equal(new[] { UpdateAllOutcome.NotAppliedOtherFailed, UpdateAllOutcome.Failed, UpdateAllOutcome.NotAppliedOtherFailed },
                answer.Items.Select(r => r.Outcome).ToArray());
            Assert.All(answer.Items, r => Assert.False(r.IsWritten));
            Assert.StartsWith("No se escribió nada: falló «C-1». ", answer.Message);
            Assert.Equal(how == "fail" ? "AutoCAD no redibujó ninguna vista del rack, así que el borrador sigue sin escribir" : "No se pudo escribir: eInvalidInput",
                answer.Items[1].Cause);
        }
    }

    [Fact]
    public void AConfirmationThatFailsLeavesEveryDraftUnknownToBeReadAgain()
    {
        var host = new Host { CommitThrows = true };
        var answer = UpdateAllRun.Execute(Write(Dynamic("D-1"), Cama("C-1")), host);

        Assert.False(answer.Committed);
        Assert.All(answer.Items, r => Assert.Equal(UpdateAllOutcome.Unknown, r.Outcome));
        Assert.All(answer.Items, r => Assert.False(r.Written.Written));
        Assert.Contains("Vuelve a leerlo del dibujo", answer.Items[0].Written.Message);
        Assert.StartsWith("No se sabe qué escribió AutoCAD: falló la confirmación de «Actualizar todo» (eLockViolation).", answer.Message);
        Assert.Equal(new[] { "commit", "rollback", "finish discarded" }, host.Calls.Skip(host.Calls.Count - 3).ToArray());
    }

    [Fact]
    public void ACancellationIsOnlyHeardBeforeTheConfirmationAndWritesNothing()
    {
        var before = new Host { CancelAfterApplies = 0 };
        var answer = UpdateAllRun.Execute(Write(Dynamic("D-1"), Cama("C-1")), before);
        Assert.Equal(0, before.Writes);
        Assert.All(answer.Items, r => Assert.Equal(UpdateAllOutcome.Cancelled, r.Outcome));
        Assert.Equal("No se escribió nada: se canceló «Actualizar todo» antes de confirmar.", answer.Message);

        var between = new Host { CancelAfterApplies = 1 };
        answer = UpdateAllRun.Execute(Write(Dynamic("D-1"), Cama("C-1")), between);
        Assert.Equal(1, between.Writes);
        Assert.Contains("rollback", between.Calls);
        Assert.DoesNotContain("commit", between.Calls);
        Assert.All(answer.Items, r => Assert.False(r.IsWritten));

        var after = new Host { CancelAfterApplies = 2 };
        answer = UpdateAllRun.Execute(Write(Dynamic("D-1"), Cama("C-1")), after);
        Assert.False(answer.Committed); // the last chance is before the confirmation
        Assert.DoesNotContain("commit", after.Calls);
    }

    [Fact]
    public void VariablesThatWouldRedrawARackWithADraftOfTheSetWriteNothingAndNameBoth()
    {
        var host = new Host { VariablesRedraw = new[] { "d-1" } };
        var answer = UpdateAllRun.Execute(Write(Variables(), Dynamic("D-1", "DIN 1"), Cama("C-1")), host);

        Assert.Equal(0, host.Writes);
        Assert.False(answer.Committed);
        Assert.Equal(UpdateAllOutcome.Failed, answer.Items[0].Outcome);
        Assert.Equal("redibujaría «DIN 1», que también tiene un borrador sin escribir: escribe o descarta antes uno de los dos.", answer.Items[0].Cause);
        Assert.Equal(UpdateAllOutcome.Failed, answer.Items[1].Outcome);
        Assert.Equal("«Variables» también lo redibuja: escribe o descarta antes uno de los dos.", answer.Items[1].Cause);
        Assert.Equal(UpdateAllOutcome.NotAppliedOtherFailed, answer.Items[2].Outcome);
        Assert.False(answer.Items[0].VariablesWritten.Written);
    }

    [Fact]
    public void ANoticeIsWrittenWithWarningsAndAFailureAfterTheConfirmationNeverUndoesIt()
    {
        var host = new Host { FinishThrows = true };
        host.Warn["C-1"] = "AutoCAD no redibujó 1 vista de 2; la línea de órdenes dice cuáles.";
        var answer = UpdateAllRun.Execute(Write(Dynamic("D-1"), Cama("C-1")), host);

        Assert.True(answer.Committed);
        Assert.Equal(UpdateAllOutcome.AppliedWithWarnings, answer.Items[1].Outcome);
        Assert.True(answer.Items[1].IsWritten);
        Assert.EndsWith("AutoCAD no redibujó 1 vista de 2; la línea de órdenes dice cuáles.", answer.Items[1].Written.Message);
        Assert.Equal("AutoCAD escribió 2 borradores en una sola operación (1 con avisos). Un «U» en AutoCAD lo deshace todo. Después, AutoCAD avisó: regen.", answer.Message);
    }

    [Fact]
    public void ARequestThatCannotStartOrIsNotWellFormedWritesNothing()
    {
        var host = new Host { BeginThrows = true };
        var answer = UpdateAllRun.Execute(Write(Dynamic("D-1")), host);
        Assert.False(answer.Committed);
        Assert.Empty(host.Calls);
        Assert.Equal("No se escribió nada: AutoCAD no pudo empezar la operación: sin dibujo.", answer.Message);

        // A new rack, «Insertar vista…» or two writes in one item are not drafts of «Actualizar todo».
        var inserting = new UpdateAllItem(UpdateAllSystem.Dynamic, "D-1", "DIN 1",
            Dynamic: new RackDoorWrite(1, Session, "D-1", "DIN 1", new DynamicRackDesign(), new DynamicRackSystem(), NewRack: true));
        var twice = Dynamic("D-2") with { Cama = new CamaDoorWrite(1, Session, "D-2", "x", new FlowBedConfiguration()) };
        var wrongSystem = Dynamic("D-3") with { System = UpdateAllSystem.Cama };
        foreach (var item in new[] { inserting, twice, wrongSystem })
        {
            Assert.False(item.IsWellFormed);
            var other = new Host();
            Assert.False(UpdateAllRun.Execute(Write(item), other).Committed);
            Assert.Empty(other.Calls);
        }

        var none = new Host();
        var nothing = UpdateAllRun.Execute(Write(), none);
        Assert.False(nothing.Committed);
        Assert.Empty(none.Calls);
        Assert.Equal("No se escribió nada: " + UpdateAllText.NothingToWrite, nothing.Message);
    }

    // ------------------------------------------------------------------ corrections of the review of c4985a2e

    // B-1: a view that was not redrawn (the helpers swallow its failure), views that are not the ones that must remain, or views that do not
    // carry one design after the write: the rack is not written, so the set is not confirmed.
    [Fact]
    public void ARackHalfWrittenIsNotWrittenInsideTheSet()
    {
        Assert.Null(UpdateAllVerification.AfterRackWrite(3, 3, 0, 3, true, null));
        Assert.Null(UpdateAllVerification.AfterRackWrite(3, 2, 1, 2, true, null));

        Assert.Equal("AutoCAD no redibujó 1 vista de las 3 del rack (la línea de órdenes dice cuál): el rack quedaría a medias.",
            UpdateAllVerification.AfterRackWrite(3, 2, 0, 3, true, null));
        Assert.Equal("AutoCAD no redibujó 2 vistas de las 2 del rack (la línea de órdenes dice cuál): el rack quedaría a medias.",
            UpdateAllVerification.AfterRackWrite(2, 0, 0, 2, true, null));
        Assert.Equal("Releído tras escribirlo, el rack tiene 3 vistas y debían quedar 2: el rack quedaría a medias.",
            UpdateAllVerification.AfterRackWrite(3, 2, 1, 3, true, null));
        Assert.Equal("Releído tras escribirlo, sus vistas no llevan el mismo diseño: el rack quedaría a medias. La vista lateral es distinta.",
            UpdateAllVerification.AfterRackWrite(3, 3, 0, 3, false, "La vista lateral es distinta."));
        Assert.Equal("Releído tras escribirlo, sus vistas no llevan el mismo diseño: el rack quedaría a medias.",
            UpdateAllVerification.AfterRackWrite(1, 1, 0, 1, false, null));
        Assert.Equal("Releído tras escribirlo, el rack tiene 0 vistas y debían quedar 1: el rack quedaría a medias.",
            UpdateAllVerification.AfterRackWrite(1, 1, 0, 0, false, null));
    }

    // I-2: nothing is imported inside the transaction. A pass whose drafts need blocks the drawing lacks is discarded, they are imported with
    // no transaction open, and the set is re-read and written again; only that second pass is confirmed.
    [Fact]
    public void BlocksTheDrawingLacksAreImportedOutsideTheTransactionAndTheSetIsWrittenAgain()
    {
        var host = new Host();
        host.Lacking["C-1"] = new[] { "BOTA_B" };
        host.Lacking["D-1"] = new[] { "TOPE_A", "BOTA_B" };
        var answer = UpdateAllRun.Execute(Write(Dynamic("D-1", "DIN 1"), Cama("C-1")), host);

        Assert.Equal(new[]
        {
            "begin", "check D-1", "check C-1", "apply D-1", "apply C-1", "rollback", "import BOTA_B,TOPE_A",
            "begin", "check D-1", "check C-1", "apply D-1", "apply C-1", "commit", "finish committed",
        }, host.Calls);
        Assert.True(answer.Committed);
        Assert.All(answer.Items, r => Assert.Equal(UpdateAllOutcome.Applied, r.Outcome));
    }

    [Fact]
    public void AnImportThatFailsOrBlocksStillLackingAfterItWriteNothingAndNameTheDraftsThatNeedThem()
    {
        var failing = new Host { ImportError = "la importación falló (el registro de RackCad dice por qué)." };
        failing.Lacking["C-1"] = new[] { "BOTA_B" };
        var answer = UpdateAllRun.Execute(Write(Dynamic("D-1", "DIN 1"), Cama("C-1")), failing);
        Assert.False(answer.Committed);
        Assert.DoesNotContain("commit", failing.Calls);
        Assert.Equal(1, failing.Calls.Count(c => c == "begin"));
        Assert.Equal(new[] { UpdateAllOutcome.NotAppliedOtherFailed, UpdateAllOutcome.Failed }, answer.Items.Select(r => r.Outcome).ToArray());
        Assert.Equal("No se pudieron importar los bloques de la biblioteca que necesita (BOTA_B): la importación falló (el registro de RackCad dice por qué).",
            answer.Items[1].Cause);
        Assert.StartsWith("No se escribió nada: falló «C-1». ", answer.Message);

        var lacking = new Host { ImportGivesNothing = true };
        lacking.Lacking["D-1"] = new[] { "TOPE_A" };
        answer = UpdateAllRun.Execute(Write(Dynamic("D-1", "DIN 1"), Cama("C-1")), lacking);
        Assert.False(answer.Committed);
        Assert.Equal(2, lacking.Calls.Count(c => c == "begin"));
        Assert.Single(lacking.Calls, c => c.StartsWith("import", StringComparison.Ordinal));
        Assert.DoesNotContain("commit", lacking.Calls);
        Assert.Equal(new[] { UpdateAllOutcome.Failed, UpdateAllOutcome.NotAppliedOtherFailed }, answer.Items.Select(r => r.Outcome).ToArray());
        Assert.Equal("Necesita bloques de la biblioteca que siguen sin estar en el dibujo tras importarlos (TOPE_A).", answer.Items[0].Cause);
    }

    // ------------------------------------------------------------------ the command, by its source (AutoCAD code)

    private static string Source(params string[] relative)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln"))) dir = dir.Parent;
        Assert.NotNull(dir);
        return File.ReadAllText(Path.Combine(new[] { dir.FullName }.Concat(relative).ToArray())).Replace("\r\n", "\n");
    }

    [Fact]
    public void TheCommandIsOneCommandWithUndoThatRunsTheCompositionInOneOuterTransactionWithTheApplyOfEverySystem()
    {
        var command = WorkspaceHostSource.CodeOnly(Source("src", "RackCad.Plugin", "VentanaRackCad", "RackCadUpdateAllCommands.cs"));

        // One command, with its undo group (one «U»), that takes the request of the door and answers it.
        Assert.Contains("[CommandMethod(\"RACKACTUALIZARTODO\")]", command, StringComparison.Ordinal);
        Assert.Single(System.Text.RegularExpressions.Regex.Matches(command, @"\[CommandMethod\("));
        Assert.DoesNotContain("NoUndoMarker", command, StringComparison.Ordinal);
        Assert.Contains("RackCadRackDoorPort.TakePendingUpdateAll()", command, StringComparison.Ordinal);
        Assert.Contains("UpdateAllRun.Execute(pending.Write, units)", command, StringComparison.Ordinal);
        Assert.Contains("door?.AnswerUpdateAll(answer)", command, StringComparison.Ordinal);
        Assert.Contains("catch (System.Exception ex)", command, StringComparison.Ordinal);

        // ONE outer transaction, opened by the composition (Begin) and closed by it (Commit / Rollback); the regeneration once, at the end.
        Assert.Contains("_transaction = RackUpdateAllTransaction.Begin(_document);", command, StringComparison.Ordinal);
        Assert.Single(System.Text.RegularExpressions.Regex.Matches(command, @"RackUpdateAllTransaction\.Begin\("));
        Assert.Single(System.Text.RegularExpressions.Regex.Matches(command, @"\.Regen\(\)"));
        Assert.True(command.IndexOf("public void Finish(", StringComparison.Ordinal) < command.IndexOf(".Regen()", StringComparison.Ordinal));
        Assert.Contains("HostApplicationServices.Current.UserBreak()", command, StringComparison.Ordinal);

        // The re-read of every system is the one of its APLICAR command, and the write calls the same apply function.
        foreach (var check in new[]
                 {
                     "RackCommandSupport.FindRackBlocks(_document, rackId)", "RackSiblingDesigns.Compare(rackId, Views(blocks))",
                     "RackDoorRules.PlanSelectiveApply(", "PushBackDoorRules.PlanApply(", "CantileverDoorRules.PlanApply(", "CamaDoorRules.PlanApply(",
                     "CabeceraDoorRules.PlanApply(", "VariablesDoorRules.PlanWrite(",
                 })
            Assert.Contains(check, command, StringComparison.Ordinal);
        foreach (var apply in new[]
                 {
                     "RackDinamicoCommands.ApplyDynamicEdit(", "RackSelectivoCommands.ApplySelectiveEdit(", "RackPushBackWindowDrawing.Apply(",
                     "RackCantileverCommands.ApplyCantileverEdit(", "RackCamaCommands.ApplyCamaUpdate(", "RackCabeceraCommands.ApplyCabeceraUpdate(",
                     "ProjectVariableMutationExecutor.Execute(", "CustomPropertiesExecutor.ExecuteProjectDraft(", "RackPropertiesDoorData.WithRackProperties(",
                 })
            Assert.Single(System.Text.RegularExpressions.Regex.Matches(command, System.Text.RegularExpressions.Regex.Escape(apply)));

        // T7 cab: a cabecera of its own writes its tab «Propiedades» too, as RACKCABECERAAPLICAR (it had none until the séptima tanda).
        Assert.Matches(@"RackCabeceraCommands\.ApplyCabeceraUpdate\(_document, plan\.Project, write\.Cabecera, write\.RackId, name\);\s*return RackWritten\(write\.RackId, views, redrawn, erased, write\.Properties,", command);

        // Push Back is written in place only: no views to insert inside the set.
        Assert.Contains("RackPushBackWindowDrawing.Apply(_document, blockId, embed, plan.Project, write.Design, system, write.RackId, null)", command, StringComparison.Ordinal);

        // An import of the block library that failed inside the transaction is a failure of the set (not a piece left out in silence).
        Assert.Contains("BlockLibraryImporter.ImportFailures", command, StringComparison.Ordinal);
        var importer = Source("src", "RackCad.Plugin", "Drawing", "BlockLibraryImporter.cs");
        Assert.Contains("System.Threading.Interlocked.Increment(ref _importFailures);", importer, StringComparison.Ordinal);

        // The transaction lives outside the folder of the door, which opens none.
        var transaction = WorkspaceHostSource.CodeOnly(Source("src", "RackCad.Plugin", "Systems", "Shared", "RackUpdateAllTransaction.cs"));
        Assert.Single(System.Text.RegularExpressions.Regex.Matches(transaction, @"StartTransaction\(\)"));
        Assert.Single(System.Text.RegularExpressions.Regex.Matches(transaction, @"_transaction\.Commit\(\)"));
        Assert.Contains("_transaction.Abort()", transaction, StringComparison.Ordinal);
        Assert.DoesNotContain("StartOpenCloseTransaction", transaction, StringComparison.Ordinal);
    }

    // The corrections of the review of c4985a2e that live in AutoCAD code, by their source.
    [Fact]
    public void TheSetReReadsEveryRackAfterItsWriteImportsOutsideItsTransactionAndPurgesAfterTheCommit()
    {
        var command = WorkspaceHostSource.CodeOnly(Source("src", "RackCad.Plugin", "VentanaRackCad", "RackCadUpdateAllCommands.cs"));

        // B-1: after the apply and the tab «Propiedades», the rack is read again inside the transaction and checked by the pure rule.
        var written = command.Substring(command.IndexOf("private UpdateAllApplied RackWritten(", StringComparison.Ordinal));
        written = written.Substring(0, written.IndexOf("\n        }", StringComparison.Ordinal));
        Assert.True(written.IndexOf("RackPropertiesDoorData.WithRackProperties(", StringComparison.Ordinal)
                    < written.IndexOf("RackCommandSupport.FindRackBlocks(_document, rackId)", StringComparison.Ordinal));
        Assert.True(written.IndexOf("RackCommandSupport.FindRackBlocks(_document, rackId)", StringComparison.Ordinal)
                    < written.IndexOf("UpdateAllVerification.AfterRackWrite(", StringComparison.Ordinal));
        Assert.Contains("RackSiblingDesigns.Compare(rackId, Views(after))", written, StringComparison.Ordinal);
        Assert.Equal(6, System.Text.RegularExpressions.Regex.Matches(command, @"return RackWritten\(write\.RackId, views, ").Count);

        // I-2: the scope opens with the transaction and is always left; the import is outside (no transaction open); the purge after the commit.
        Assert.Contains("_transaction = RackUpdateAllTransaction.Begin(_document);\n            RackUpdateAllScope.Enter();", command.Replace("\r\n", "\n"), StringComparison.Ordinal);
        var dispose = command.Substring(command.IndexOf("public void Dispose()", StringComparison.Ordinal));
        Assert.Contains("RackUpdateAllScope.Exit();", dispose.Substring(0, dispose.IndexOf('}')), StringComparison.Ordinal);
        var import = command.Substring(command.IndexOf("public string ImportOutside(", StringComparison.Ordinal));
        import = import.Substring(0, import.IndexOf("\n        }", StringComparison.Ordinal));
        Assert.True(import.IndexOf("RackUpdateAllTransaction.AnyOpen(_document)", StringComparison.Ordinal)
                    < import.IndexOf("BlockLibraryImporter.EnsureBlocks(_document.Database, blocks)", StringComparison.Ordinal));
        var finish = command.Substring(command.IndexOf("public void Finish(bool committed)", StringComparison.Ordinal));
        finish = finish.Substring(0, finish.IndexOf("\n        }", StringComparison.Ordinal));
        Assert.True(finish.IndexOf("RackUpdateAllScope.Exit();", StringComparison.Ordinal)
                    < finish.IndexOf("LateralHeaderDrawer.PurgeUnreferenced(_document.Database, purge)", StringComparison.Ordinal));
        Assert.Contains("if (committed && purge.Count > 0)", finish, StringComparison.Ordinal);

        var importer = WorkspaceHostSource.CodeOnly(Source("src", "RackCad.Plugin", "Drawing", "BlockLibraryImporter.cs"));
        var ensure = importer.Substring(importer.IndexOf("public static int EnsureForPlan(", StringComparison.Ordinal));
        Assert.True(ensure.IndexOf("RackUpdateAllScope.IsActive", StringComparison.Ordinal) < ensure.IndexOf("LibraryBlockAvailabilityFlow.Observe(", StringComparison.Ordinal));
        var observe = importer.Substring(importer.IndexOf("private static void ObserveForTheSet(", StringComparison.Ordinal));
        observe = observe.Substring(0, observe.IndexOf("\n        }", StringComparison.Ordinal));
        Assert.DoesNotContain("WblockCloneObjects", observe, StringComparison.Ordinal);

        var drawer = WorkspaceHostSource.CodeOnly(Source("src", "RackCad.Plugin", "Drawing", "LateralHeaderDrawer.cs"));
        var purge = drawer.Substring(drawer.IndexOf("internal static void PurgeUnreferenced(", StringComparison.Ordinal));
        Assert.True(purge.IndexOf("RackUpdateAllScope.DeferPurge(candidates);", StringComparison.Ordinal) < purge.IndexOf("db.Purge(ids)", StringComparison.Ordinal));

        // Minor 3: the blocks neither the drawing nor the library has are a notice of the draft in every system (Apply, not only the cama).
        var apply = command.Substring(command.IndexOf("public UpdateAllApplied Apply(", StringComparison.Ordinal));
        apply = apply.Substring(0, apply.IndexOf("\n        }", StringComparison.Ordinal));
        Assert.Contains("RackUpdateAllScope.BeginDraft();", apply, StringComparison.Ordinal);
        Assert.Contains("RackUpdateAllScope.DraftMissing", apply, StringComparison.Ordinal);
        Assert.DoesNotContain("HasMissingBlocks", command, StringComparison.Ordinal);

        // Minor 4: a set that is not confirmed says on the command line that the lines of the drawing halves were undone.
        Assert.Contains("\"lo que dicen las líneas anteriores se deshizo. \" + answer.Message", command, StringComparison.Ordinal);

        // I-2: UpgradeOpen only on an object that is not open for write already. I-1: the erasures count once the transaction is confirmed.
        Assert.Contains("if (!def.IsWriteEnabled) def.UpgradeOpen();", Source("src", "RackCad.Plugin", "Drawing", "RackBlockRenamer.cs"), StringComparison.Ordinal);
        Assert.Contains("if (!dictionary.IsWriteEnabled) dictionary.UpgradeOpen();", Source("src", "RackCad.Plugin", "CustomPropertiesData.cs"), StringComparison.Ordinal);
        var support = Source("src", "RackCad.Plugin", "RackCommandSupport.cs");
        Assert.Contains("                        transaction.Commit();\n                        erased = erasing;", support, StringComparison.Ordinal);
        Assert.DoesNotContain("erased++", support, StringComparison.Ordinal);
    }

    [Fact]
    public void TheDoorKeepsTheRequestBeforeHandingItOverAndStopWaitingDropsIt()
    {
        var port = WorkspaceHostSource.CodeOnly(Source("src", "RackCad.Plugin", "VentanaRackCad", "RackCadRackDoorPort.UpdateAll.cs"));
        Assert.Contains("internal const string UpdateAllCommand = \"RACKACTUALIZARTODO\";", port, StringComparison.Ordinal);
        Assert.Contains("return Send(UpdateAllCommand + \" \",", port, StringComparison.Ordinal);
        Assert.Contains("() => _pendingUpdateAll = null);", port, StringComparison.Ordinal);
        Assert.DoesNotContain("SendStringToExecute", port, StringComparison.Ordinal);
        var door = Source("src", "RackCad.Plugin", "VentanaRackCad", "RackCadRackDoorPort.cs");
        Assert.Contains("AbandonUpdateAll(requestId); // T6 at", door, StringComparison.Ordinal);
    }
}
