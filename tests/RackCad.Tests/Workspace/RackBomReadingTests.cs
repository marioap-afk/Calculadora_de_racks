using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.Bom;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.Selective;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// T4 bom (I-64 A-14, line D; prototype «bom», BM-01 to BM-11): the reading of the page «BOM y lista de racks» keeps the rules of
    /// RACKBOMTOTAL (I-42, I-47 G13) — one read of the register, the authority over every view, the gate of a blocked rack, a broken link
    /// and a definition that cannot be classified abort the TOTAL, a rack whose views differ or whose design cannot be read stays out with
    /// its reason — and adds what the page needs: the BOM of every rack that can be quoted even when the total is refused (BM-05), only the
    /// racks with copies (BM-06), the words of the side column (BM-07), where each partida of the total comes from (BM-08), the names of the
    /// files next to the drawing (BM-03) and «Cantidad_total» in the CSV of the drawing (BM-04). The command RACKBOMLEER is fixed by its
    /// source: the Plugin is not loaded here (ADR-0003).
    /// </summary>
    public class RackBomReadingTests
    {
        private const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        private const string BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES";

        // ------------------------------------------------------------------ fixtures

        private static BomLine Line(string category, string id, int qty, double length = 0.0, string description = null)
            => new() { Category = category, ProfileId = id, Quantity = qty, Length = length, Description = description ?? category };

        private static BomComponent Comp(string category, double length, int qty, string description, params BomLine[] pieces)
            => new() { Category = category, ProfileId = category, Description = description, Length = length, Quantity = qty, Pieces = pieces.ToList() };

        private static BillOfMaterials Tree(params BomComponent[] components) => new(components.ToList());

        private static RackEmbedDocument Embed(string id, string kind, string name, string view = RackEmbedDocument.ViewFrontal, string design = "{}", int section = -1)
            => new() { Id = id, Kind = kind, Name = name, View = view, Section = section, Design = design };

        private static RackBomViewFact Fact(string definition, RackEmbedDocument embed, int references, string block = null)
            => new(definition, block ?? "RACK_" + definition, embed, references);

        /// <summary>A handler of a kind for the rules: its label, an optional blocked reason and a BOM (or a typed failure) per rack.</summary>
        private sealed class Kind : IRackBomKind
        {
            public Kind(string label) => Label = label;

            public string Label { get; }

            public Dictionary<string, string> Blocked { get; } = new();

            public Dictionary<string, Func<ProjectVariablesDocument, BomBuildResult>> Results { get; } = new();

            public List<ProjectVariablesDocument> RegistersSeen { get; } = new();

            public List<string> Built { get; } = new();

            public string BlockedReason(RackEmbedDocument embed) => Blocked.TryGetValue(embed.Id, out var reason) ? reason : null;

            public BomBuildResult Build(RackEmbedDocument embed, ProjectVariablesDocument variables)
            {
                RegistersSeen.Add(variables);
                Built.Add(embed.Id);
                return Results.TryGetValue(embed.Id, out var result) ? result(variables) : BomBuildResult.Success(Tree(Comp("Larguero", 96, 2, "Larguero 96", Line("Perfil", "L", 1, 96))));
            }
        }

        private static Func<string, IRackBomKind> Kinds(params (string Kind, IRackBomKind Handler)[] kinds)
            => kind => kinds.FirstOrDefault(k => string.Equals(k.Kind, kind, StringComparison.Ordinal)).Handler;

        private static string SelectiveDesign(string id, double clearance)
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = clearance, FloorBeamRise = 4.0,
                PalletDepth = 48.0, DepthCount = 1, DrawBasePlate = true,
            };
            var bay = new SelectiveBayDesign { FloorBeam = true };
            bay.Levels.Add(new SelectiveCell { Pallet = new Tarima { Frente = 42.0, Alto = 60.0 }, PalletCount = 2, BeamId = BeamId, BeamPeralte = 4.0 });
            design.Bays.Add(bay);
            return new SelectivePalletDesignStore().Serialize(SelectivePalletDesignDocument.From(design, id, "Selectivo 1"));
        }

        private static RackBomReading Compose(IReadOnlyList<RackBomViewFact> views, Func<string, IRackBomKind> kinds, ProjectVariablesReadResult registry = null)
            => RackBomReadingRules.Compose(7, @"C:\Proyectos\Planta ejemplo.dwg", "Planta ejemplo", views, registry ?? ProjectVariablesReadResult.Absent(), kinds);

        // ------------------------------------------------------------------ the reading

        [Fact]
        public void EveryRackPlacedIsListedWithItsBomItsCopiesItsViewsAndTheWordsOfTheSideColumn()
        {
            var dynamic = new Kind("Dinámico");
            var push = new Kind("Push Back");
            var cama = new Kind("Cama");
            cama.Results["C-1"] = _ => BomBuildResult.Success(new BillOfMaterials(new List<BomLine> { Line("Rodillo", "R", 22, 40), Line("Riel", "RI", 2, 96) }));

            var reading = Compose(new[]
            {
                Fact("A1", Embed("D-1", RackEmbedDocument.KindDynamic, "Dinámico 1", RackEmbedDocument.ViewLateral), 1),
                Fact("A2", Embed("D-1", RackEmbedDocument.KindDynamic, "Dinámico 1", RackEmbedDocument.ViewPlanta), 3),
                Fact("B1", Embed("P-1", RackEmbedDocument.KindPushBack, "Push Back 1"), 1),
                Fact("C1", Embed("C-1", RackEmbedDocument.KindCama, "Cama 1", RackEmbedDocument.ViewLateral), 4),
                // A rack defined and not placed (drawn and erased, not purged): not listed (BM-06), as the side column and the BOM.
                Fact("Z1", Embed("Z-1", RackEmbedDocument.KindDynamic, "Borrado"), 0),
            }, Kinds((RackEmbedDocument.KindDynamic, dynamic), (RackEmbedDocument.KindPushBack, push), (RackEmbedDocument.KindCama, cama)));

            Assert.True(reading.IsRead);
            Assert.Equal(7, reading.RequestId);
            Assert.Equal(new[] { "D-1", "P-1", "C-1" }, reading.Racks.Select(r => r.RackId));
            var din = reading.Racks[0];
            Assert.Equal(("Dinámico 1", "Dinámico", "lateral, planta", 3), (din.Name, din.KindLabel, din.Views, din.Copies));
            Assert.Equal("Push Back", reading.Racks[1].KindLabel); // not the raw «pushback» of RACKLISTA (BM-07)
            Assert.Equal("Cama", reading.Racks[2].KindLabel);      // not «Cama de rodamiento»
            Assert.All(reading.Racks, r => Assert.Equal(RackBomRackState.Quoted, r.State));
            Assert.Empty(reading.Refusals);
            Assert.Empty(reading.Skipped);

            // The total is the one of RACKBOMTOTAL: the racks quoted, with the label of their handler and their copies.
            Assert.NotNull(reading.Total);
            Assert.Equal(new[] { ("Dinámico 1", "Dinámico", 3), ("Push Back 1", "Push Back", 1), ("Cama 1", "Cama", 4) },
                reading.Total.Racks.Select(r => (r.Name, r.Kind, r.Copies)));
            Assert.Equal(3 + 1 + 4, reading.Total.TotalCopies);
            Assert.DoesNotContain("Z-1", dynamic.Built);
            Assert.Equal("RackCad: BOM leído para la ventana RackCad: 3 racks.", RackBomReadingRules.CommandLineText(reading));
        }

        [Fact]
        public void TheRegisterIsReadOnceAndEveryRackIsQuotedAgainstTheSameOne()
        {
            var kind = new Kind("Selectivo");
            var registry = ProjectVariablesReadResult.Readable(ProjectVariablesDocument.CreateNew());
            Compose(new[]
            {
                Fact("A1", Embed("A", "dynamic", "A"), 1),
                Fact("B1", Embed("B", "dynamic", "B"), 1),
            }, Kinds(("dynamic", kind)), registry);

            Assert.Equal(2, kind.RegistersSeen.Count);
            Assert.All(kind.RegistersSeen, seen => Assert.Same(registry.Document, seen));
        }

        [Fact]
        public void ARegisterThatExistsAndCannotBeReadGivesNoBomAtAll()
        {
            var kind = new Kind("Dinámico");
            var reading = Compose(new[] { Fact("A1", Embed("A", "dynamic", "A"), 1) }, Kinds(("dynamic", kind)),
                ProjectVariablesReadResult.Unreadable("El registro está dañado."));

            Assert.False(reading.IsRead);
            Assert.StartsWith("No hay BOM: el registro de variables del dibujo existe y no se puede leer. El registro está dañado.", reading.Failure);
            Assert.Empty(reading.Racks);
            Assert.Null(reading.Total);
            Assert.Empty(kind.Built);
            Assert.StartsWith("RackCad: no hay BOM del dibujo (ver la ventana RackCad).", RackBomReadingRules.CommandLineText(reading));
        }

        // BM-05: a blocked rack (I-42) aborts the TOTAL, named with the reason of its own authority; the others keep their BOM.
        [Fact]
        public void ABlockedRackAbortsTheTotalAndTheOthersKeepTheirBom()
        {
            var push = new Kind("Push Back");
            push.Blocked["P-1"] = "su cama pide más longitud de la que tiene la estructura.";
            var dynamic = new Kind("Dinámico");

            var reading = Compose(new[]
            {
                Fact("A1", Embed("D-1", "dynamic", "Dinámico 1"), 1),
                Fact("B1", Embed("P-1", "pushback", "Push Back 1"), 1),
            }, Kinds(("dynamic", dynamic), ("pushback", push)));

            Assert.True(reading.IsRead);
            Assert.Null(reading.Total);
            var refusal = Assert.Single(reading.Refusals);
            Assert.Equal(("P-1", "Push Back 1", "su cama pide más longitud de la que tiene la estructura."), (refusal.RackId, refusal.Name, refusal.Reason));
            Assert.Equal(RackBomRackState.Blocked, reading.Racks[1].State);
            Assert.True(reading.Racks[1].BlocksTheTotal);
            Assert.Null(reading.Racks[1].Bom);
            Assert.DoesNotContain("P-1", push.Built); // the gate is asked BEFORE any BOM is built
            Assert.True(reading.Racks[0].IsQuoted);   // BM-05: the table still shows the BOM of every rack that can be quoted
            Assert.Equal("RackCad: BOM leído, sin total del dibujo: 1 rack tiene un problema que impide cotizarlo (ver la ventana RackCad).",
                RackBomReadingRules.CommandLineText(reading));
        }

        // I-47 G13: the design is readable and the VARIABLE is missing, so a total without it would look complete: it aborts the total.
        [Fact]
        public void ABrokenLinkAbortsTheTotalWithThePropertyAndTheVariable()
        {
            var kind = new Kind("Selectivo");
            kind.Results["S-1"] = _ => BomBuildResult.BrokenProjectVariableReference("S-1", "Selectivo 1", "selective.verticalClearance", "var-9", "La variable no existe.");

            var reading = Compose(new[]
            {
                Fact("A1", Embed("S-1", "dynamic", "Selectivo 1"), 2),
                Fact("B1", Embed("S-2", "dynamic", "Selectivo 2"), 1),
            }, Kinds(("dynamic", kind)));

            Assert.Null(reading.Total);
            Assert.Equal(RackBomRackState.BrokenLink, reading.Racks[0].State);
            var refusal = Assert.Single(reading.Refusals);
            Assert.Equal("está vinculado a una variable de proyecto que no se puede resolver (propiedad 'selective.verticalClearance', variable 'var-9'). "
                         + "Repara el vínculo antes de cotizar. La variable no existe.", refusal.Reason);
            Assert.True(reading.Racks[1].IsQuoted);
        }

        // RACKBOMTOTAL aborts on a kind without handler (never a partial BOM): the page refuses the total and names the rack.
        [Fact]
        public void AKindWithNoHandlerAbortsTheTotal()
        {
            var reading = Compose(new[]
            {
                Fact("A1", Embed("X-1", "futuro", "Rack del futuro"), 1),
                Fact("B1", Embed("D-1", "dynamic", "Dinámico 1"), 1),
            }, Kinds(("dynamic", new Kind("Dinámico"))));

            Assert.Null(reading.Total);
            Assert.Equal(RackBomRackState.UnknownKind, reading.Racks[0].State);
            Assert.Equal("futuro", reading.Racks[0].KindLabel);
            Assert.Equal("su tipo («futuro») no lo reconoce esta versión de RackCad: no se puede cotizar.", Assert.Single(reading.Refusals).Reason);
            Assert.True(reading.Racks[1].IsQuoted);
        }

        // I-47 G13: a placed definition that cannot be classified aborts the total (no RackId is invented); one that is not placed is ignored.
        [Fact]
        public void APlacedDefinitionThatCannotBeClassifiedAbortsTheTotalAndOneNotPlacedDoesNot()
        {
            var kinds = Kinds(("dynamic", new Kind("Dinámico")));
            var notPlaced = Compose(new[]
            {
                Fact("A1", Embed("D-1", "dynamic", "Dinámico 1"), 1),
                Fact("9F", null, 0, "RACK_ROTO"),
            }, kinds);
            Assert.NotNull(notPlaced.Total);
            Assert.Empty(notPlaced.Refusals);

            var placed = Compose(new[]
            {
                Fact("A1", Embed("D-1", "dynamic", "Dinámico 1"), 1),
                Fact("9F", new RackEmbedDocument { Id = " ", Kind = "dynamic" }, 2, "RACK_ROTO"),
            }, kinds);
            Assert.Null(placed.Total);
            var refusal = Assert.Single(placed.Refusals);
            Assert.Null(refusal.RackId);
            Assert.Equal("La definición '9F' (bloque «RACK_ROTO»)", refusal.Name);
            Assert.StartsWith("está colocada en el dibujo y lleva datos de RackCad que esta versión no puede interpretar", refusal.Reason);
            Assert.True(Assert.Single(placed.Racks).IsQuoted);
        }

        // I-47 G13: a Selectivo is quoted from one view only once all its views say the same; otherwise it stays OUT, said, and the total is made.
        [Fact]
        public void ASelectivoWhoseViewsDifferStaysOutOfTheTotalWithItsReason()
        {
            var selective = new Kind("Selectivo");
            var reading = Compose(new[]
            {
                Fact("A1", Embed("S-1", RackEmbedDocument.KindSelective, "Selectivo 1", RackEmbedDocument.ViewFrontal, SelectiveDesign("S-1", 6.0)), 1),
                Fact("A2", Embed("S-1", RackEmbedDocument.KindSelective, "Selectivo 1", RackEmbedDocument.ViewPlanta, SelectiveDesign("S-1", 8.0)), 1),
                Fact("B1", Embed("S-2", RackEmbedDocument.KindSelective, "Selectivo 2", RackEmbedDocument.ViewFrontal, SelectiveDesign("S-2", 6.0)), 1),
                Fact("B2", Embed("S-2", RackEmbedDocument.KindSelective, "Selectivo 2", RackEmbedDocument.ViewPlanta, SelectiveDesign("S-2", 6.0)), 1),
            }, Kinds((RackEmbedDocument.KindSelective, selective)));

            Assert.Equal(RackBomRackState.NoSingleDesign, reading.Racks[0].State);
            Assert.StartsWith("no tiene un diseño único entre sus vistas y queda fuera del total.", reading.Racks[0].Reason);
            Assert.Equal("S-1", Assert.Single(reading.Skipped).RackId);
            Assert.Empty(reading.Refusals);
            Assert.True(reading.Racks[1].IsQuoted);
            Assert.Equal(new[] { "Selectivo 2" }, reading.Total.Racks.Select(r => r.Name));
            Assert.DoesNotContain("S-1", selective.Built);
            Assert.Equal("RackCad: BOM leído para la ventana RackCad: 2 racks; 1 queda fuera del total (ver la ventana RackCad).",
                RackBomReadingRules.CommandLineText(reading));
        }

        [Fact]
        public void ARackWhoseDesignCannotBeReadStaysOutOfTheTotalAndAThrowingHandlerToo()
        {
            var kind = new Kind("Dinámico");
            kind.Results["D-1"] = _ => BomBuildResult.UnreadablePayload("D-1", "Dinámico 1", "El diseno embebido no se pudo interpretar.");
            kind.Results["D-2"] = _ => throw new InvalidOperationException("boom");

            var reading = Compose(new[]
            {
                Fact("A1", Embed("D-1", "dynamic", "Dinámico 1"), 1),
                Fact("B1", Embed("D-2", "dynamic", "Dinámico 2"), 1),
                Fact("C1", Embed("D-3", "dynamic", "Dinámico 3"), 1),
            }, Kinds(("dynamic", kind)));

            Assert.Equal(new[] { RackBomRackState.Unreadable, RackBomRackState.Unreadable, RackBomRackState.Quoted }, reading.Racks.Select(r => r.State));
            Assert.Equal("su diseño no se pudo interpretar y queda fuera del total. El diseno embebido no se pudo interpretar.", reading.Racks[0].Reason);
            Assert.EndsWith("boom", reading.Racks[1].Reason);
            Assert.Equal(2, reading.Skipped.Count);
            Assert.Equal(new[] { "Dinámico 3" }, reading.Total.Racks.Select(r => r.Name));
        }

        [Fact]
        public void ADrawingWithNoRackPlacedHasNoRackAndNoTotal()
        {
            var reading = Compose(Array.Empty<RackBomViewFact>(), Kinds());
            Assert.True(reading.IsRead);
            Assert.Empty(reading.Racks);
            Assert.Null(reading.Total);
            Assert.Equal("RackCad: BOM leído para la ventana RackCad: 0 racks.", RackBomReadingRules.CommandLineText(reading));
        }

        [Fact]
        public void TheWordsAndTheOrderOfTheSystemsAreThoseOfTheSideColumn()
        {
            Assert.Equal(new[] { "Selectivo", "Push Back", "Dinámico", "Cantilever", "Cabecera", "Cama" }, RackBomReadingRules.KindOrder.Select(k => k.Label));
            Assert.Equal("Dinámico", RackBomReadingRules.KindLabelOf(" DYNAMIC "));
            Assert.Equal("Sin tipo", RackBomReadingRules.KindLabelOf(null));
            Assert.Equal(1, RackBomReadingRules.KindRank("pushback"));
            Assert.Equal(6, RackBomReadingRules.KindRank("otro"));
        }

        // ------------------------------------------------------------------ BM-08: where each partida of the total comes from

        [Fact]
        public void TheProvenanceOfTheTotalSaysTheRacksAndTheDescriptionsItJoinsWithoutChangingTheSum()
        {
            // Camas of distinct BFR and system: same category, length and recipe (none): the total joins them under the first (I-68).
            var dynamic = new ConsolidatedRackBom { Name = "Dinámico 1", Kind = "Dinámico", Copies = 2, Bom = Tree(Comp("Cama", 144, 6, "Cama · BFR 2\"")) };
            var push = new ConsolidatedRackBom { Name = "Push Back 1", Kind = "Push Back", Copies = 1, Bom = Tree(Comp("Cama", 144, 9, "Cama · BFR 2.5\""), Comp("Larguero", 96, 4, "L 96", Line("Perfil", "L", 1, 96))) };
            var cama = new ConsolidatedRackBom { Name = "Cama 1", Kind = "Cama", Copies = 4, Bom = new BillOfMaterials(new List<BomLine> { Line("Rodillo", "R", 22, 40) }) };
            var total = ConsolidatedBomBuilder.Build(new[] { dynamic, push, cama });

            var provenance = ConsolidatedBomBuilder.Provenance(total);

            Assert.Equal(total.GrandTotal.Components.Count, provenance.Count);
            Assert.Equal(new[] { "Cama", "Larguero", "Cama" }, total.GrandTotal.Components.Select(c => c.Category));
            Assert.Equal(6 * 2 + 9, total.GrandTotal.Components[0].Quantity); // the sum does not change
            Assert.True(provenance[0].JoinsDescriptions);
            Assert.Equal(new[] { "Cama · BFR 2\"", "Cama · BFR 2.5\"" }, provenance[0].Descriptions);
            Assert.Equal(new[] { ("Dinámico 1", 6, 2), ("Push Back 1", 9, 1) }, provenance[0].Sources.Select(s => (s.RackName, s.QuantityPerCopy, s.Copies)));
            Assert.False(provenance[1].JoinsDescriptions);
            Assert.Equal(("Cama 1", 1, 4), (provenance[2].Sources[0].RackName, provenance[2].Sources[0].QuantityPerCopy, provenance[2].Sources[0].Copies));
            Assert.Empty(ConsolidatedBomBuilder.Provenance(null));
        }

        // ------------------------------------------------------------------ BM-03: the files next to the drawing

        [Fact]
        public void TheFilesGoNextToTheDrawingWithTheirAutomaticNames()
        {
            Assert.Equal("Planta ejemplo - BOM.xlsx", RackBomFiles.DrawingFileName("Planta ejemplo", RackBomFiles.Excel));
            Assert.Equal("Planta ejemplo - Selectivo 2 - BOM.csv", RackBomFiles.RackFileName("Planta ejemplo", "Selectivo 2", false, RackBomFiles.Csv));
            Assert.Equal("Planta ejemplo - Selectivo 2 - BOM (borrador).xlsx", RackBomFiles.RackFileName("Planta ejemplo", "Selectivo 2", true, RackBomFiles.Excel));
            Assert.Equal("Planta - PB_1_A - BOM.xlsx", RackBomFiles.RackFileName("Planta", "PB/1:A", false, RackBomFiles.Excel));
            Assert.Equal("Dibujo - Rack - BOM.xlsx", RackBomFiles.RackFileName(" ", null, false, RackBomFiles.Excel));
            Assert.Equal(@"C:\Proyectos", RackBomFiles.FolderOf(@"C:\Proyectos\Planta ejemplo.dwg"));
            Assert.Null(RackBomFiles.FolderOf("Drawing1.dwg"));   // a drawing never saved has no folder
            Assert.Null(RackBomFiles.FolderOf(null));
            Assert.Equal("Planta ejemplo", RackBomFiles.NameOf(@"C:\Proyectos\Planta ejemplo.dwg"));
            Assert.Equal("Drawing1", RackBomFiles.NameOf("Drawing1.dwg"));
        }

        // ------------------------------------------------------------------ BM-04: «Cantidad_total» in the CSV of the drawing

        [Fact]
        public void TheCsvOfTheDrawingSaysTheTotalQuantityAsTheExcel()
        {
            var csv = ConsolidatedBomCsvExporter.ToCsv(ConsolidatedBomBuilder.Build(new[]
            {
                new ConsolidatedRackBom { Name = "A", Kind = "Selectivo", Copies = 3, Bom = Tree(Comp("Larguero", 96, 2, "L 96", Line("Perfil", "L", 1, 96))) },
            }));

            var lines = csv.Split("\r\n");
            Assert.Equal("Rack,Tipo,Copias,Nivel,Categoria,Perfil,Descripcion,Longitud_in,Cantidad,Cantidad_total", lines[0]);
            Assert.Equal("A,Selectivo,3,Componente,Larguero,Larguero,L 96,96,2,6", lines[1]);
            Assert.Equal("A,Selectivo,3,Pieza,Perfil,L,Perfil,96,2,6", lines[2]);
            Assert.Equal("TOTAL DEL DIBUJO,,,Componente,Larguero,Larguero,L 96,96,6,6", lines[3]);
        }

        // ------------------------------------------------------------------ the command and the door, by their source

        private static string Root()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln"))) dir = dir.Parent;
            Assert.NotNull(dir);
            return dir.FullName;
        }

        private static string Code(params string[] parts)
        {
            var source = File.ReadAllText(Path.Combine(new[] { Root() }.Concat(parts).ToArray()));
            var withoutBlocks = Regex.Replace(source, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
            return Regex.Replace(withoutBlocks, @"//[^\n]*", string.Empty);
        }

        private static int Count(string code, string token) => Regex.Matches(code, Regex.Escape(token)).Count;

        [Fact]
        public void RackBomLeerReadsWithNoUndoMarkerOnceAndAnswersWithTheRules()
        {
            var code = Code("src", "RackCad.Plugin", "RackInventarioCommands.Ventana.cs");

            Assert.Contains("[CommandMethod(\"RACKBOMLEER\", CommandFlags.NoUndoMarker | CommandFlags.NoHistory)]", code);
            Assert.Equal(1, Count(code, "ProjectVariablesRegistry.Read("));          // ONE read of the register per reading
            Assert.Equal(1, Count(code, "RackBlockFinder.ScanEnvelopes("));          // ONE pass over the block table
            Assert.Contains("includeReferenceCount: true", code);
            Assert.Contains("RackBomReadingRules.Compose(", code);
            Assert.Contains("KindHandlerRegistry.Default.TryGet(kind, out var handler)", code);
            Assert.Contains("_handler.OutputBlockedReason(embed, _catalog)", code);
            Assert.Contains("_handler.BuildBom(embed, _catalog, variables)", code);

            // The request: taken once, for the drawing it was made for, and answered with its number.
            var body = code.Substring(code.IndexOf("public void RackBomLeer()", StringComparison.Ordinal));
            var steps = new[] { "RackCadRackDoorPort.TakePendingBomRead()", "pending.Document != RackCadRackDoorPort.KeyOf(document)", "ReadBomOfDrawing(document, pending.RequestId", "door?.AnswerBomRead(reading)" };
            var at = -1;
            foreach (var step in steps)
            {
                var next = body.IndexOf(step, at + 1, StringComparison.Ordinal);
                Assert.True(next > at, "out of order or missing: " + step);
                at = next;
            }

            // RACKBOMLEER writes nothing: no write of the drawing, no upgrade of what it reads.
            Assert.DoesNotContain("OpenMode.ForWrite", code);
            Assert.DoesNotContain("UpgradeOpen", code);
        }

        // BM-11: RACKLISTA, RL, RACKBOMTOTAL and RB open the page of the window; the classic flows stay, not registered.
        [Fact]
        public void TheListAndTheBomTotalOpenThePageOfTheWindowAndTheClassicFlowsStayUnregistered()
        {
            var window = Code("src", "RackCad.Plugin", "RackInventarioCommands.Ventana.cs");
            Assert.Matches(@"\[CommandMethod\(""RACKLISTA""\)\]\s*public void RackLista\(\) => OpenBomPage\(RackBomTab\.Racks\);", window);
            Assert.Matches(@"\[CommandMethod\(""RACKBOMTOTAL""\)\]\s*public void RackBomTotal\(\) => OpenBomPage\(RackBomTab\.Total\);", window);
            Assert.Contains("[CommandMethod(\"RL\")]  public void AliasRackLista() => RackLista();", window);
            Assert.Contains("[CommandMethod(\"RB\")]  public void AliasRackBomTotal() => RackBomTotal();", window);
            var open = window.Substring(window.IndexOf("private static void OpenBomPage(", StringComparison.Ordinal));
            Assert.True(open.IndexOf("Workspace.WorkspacePanelCommand.ShowRackCadWindow();", StringComparison.Ordinal)
                        < open.IndexOf("door.AskBomPage(tab);", StringComparison.Ordinal));

            var lista = Code("src", "RackCad.Plugin", "RackInventarioCommands.cs");
            var total = Code("src", "RackCad.Plugin", "RackInventarioCommands.BomTotal.cs");
            Assert.DoesNotContain("[CommandMethod(", lista);
            Assert.DoesNotContain("[CommandMethod(", total);
            Assert.Contains("internal void RackListaClassic()", lista);
            Assert.Contains("internal void RackBomTotalClassic()", total);
        }

        [Fact]
        public void TheDoorOfTheBomHandsTheReadOverAsTheDoorToARackAndAbandonsIt()
        {
            var bom = Code("src", "RackCad.Plugin", "VentanaRackCad", "RackCadRackDoorPort.Bom.cs");
            Assert.Contains("public sealed partial class RackCadRackDoorPort : IRackCadBomPort", bom);
            Assert.Contains("return Send(RackBomReadingRules.CommandName + \" \",", bom);
            Assert.DoesNotContain("SendStringToExecute", bom);
            Assert.Equal("RACKBOMLEER", RackBomReadingRules.CommandName);

            var port = Code("src", "RackCad.Plugin", "VentanaRackCad", "RackCadRackDoorPort.cs");
            var abandon = port.Substring(port.IndexOf("public void Abandon(long requestId)", StringComparison.Ordinal));
            abandon = abandon.Substring(0, abandon.IndexOf("internal static PendingRead TakePendingSelectiveRead()", StringComparison.Ordinal));
            Assert.Contains("AbandonBomRead(requestId);", abandon);
        }
    }
}
