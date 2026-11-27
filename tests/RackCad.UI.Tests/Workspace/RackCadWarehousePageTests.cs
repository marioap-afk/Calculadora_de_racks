#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using RackCad.Application.Geometry;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.UI.VentanaRackCad;
using RackCad.UI.Workspace;
using Xunit;
using static RackCad.UI.Tests.SelectiveWindowFixtures;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// T5 al (I-64 A-14, line D; prototype «almacen», questions 1 to 11 accepted): the page «Almacén» of the RackCad window, RACKLAYOUT and
    /// RACKRELLENAR without a modal, over a door that reads the planta and the site and places (<see cref="IRackCadWarehousePort"/>). The read
    /// and the placement are requests of the door like the others (ONE in flight for the window, numbered, «Dejar de esperar», the answer only
    /// for its request and its drawing); the page reads only after a gesture, computes the result and the scheme at once, places nothing
    /// until «Colocar», remembers its values per user and never vetoes closing the drawing.
    /// </summary>
    public class RackCadWarehousePageTests
    {
        private static readonly SessionId Session = new(7);

        /// <summary>A door that reads the planta and the site and places, and reads a Selectivo (for a draft with changes, question 4).</summary>
        private sealed class WarehouseDoor : IRackCadSelectiveDoorPort, IRackCadWarehousePort
        {
            public List<WarehouseReadRequest> Reads { get; } = new();

            public List<WarehousePlaceRequest> Places { get; } = new();

            public List<RackDoorRequest> SelectiveReads { get; } = new();

            public List<long> Abandoned { get; } = new();

            public RackCadClassicResult Answer { get; set; } = RackCadClassicResult.Started();

            public event Action<RackDesignRead>? DesignRead { add { } remove { } }

            public event Action<RackDesignWritten>? DesignWritten { add { } remove { } }

            public event Action<long>? RequestStarted;

            public event Action<SelectiveDesignRead>? SelectiveRead;

            public event Action<WarehouseReading>? WarehouseRead;

            public event Action<WarehousePlaced>? WarehousePlaced;

            public RackCadClassicResult RequestDesign(RackDoorRequest request) => RackCadClassicResult.Started();

            public RackCadClassicResult RequestDynamicUpdate(RackDoorWrite write) => RackCadClassicResult.Started();

            public RackCadClassicResult RequestSelectiveDesign(RackDoorRequest request)
            {
                SelectiveReads.Add(request);
                return RackCadClassicResult.Started();
            }

            public RackCadClassicResult RequestSelectiveUpdate(SelectiveDoorWrite write) => RackCadClassicResult.Started();

            public RackCadClassicResult RequestWarehouseRead(WarehouseReadRequest request)
            {
                Reads.Add(request);
                return Answer;
            }

            public RackCadClassicResult RequestWarehousePlace(WarehousePlaceRequest request)
            {
                Places.Add(request);
                return Answer;
            }

            public void Abandon(long requestId) => Abandoned.Add(requestId);

            public void Start(long requestId) => RequestStarted?.Invoke(requestId);

            public void Read(WarehouseReading reading) => WarehouseRead?.Invoke(reading);

            public void Placed(WarehousePlaced placed) => WarehousePlaced?.Invoke(placed);

            public void AnswerLastSelectiveRead() => SelectiveRead?.Invoke(SelectiveWindowFixtures.Read(SelectiveReads.Last().RequestId, SelectiveReads.Last().RackId));

            // T6 rl: RLY and RR.
            public event Action<WarehousePageAsk>? WarehousePageAsked;

            public event Action<WarehousePageAsk, WarehouseReading>? WarehouseShortcutRead;

            /// <summary>What RLY or RR read inside themselves, for what the page asked.</summary>
            public List<WarehouseReadRequest> ShortcutReads { get; } = new();

            // RLY or RR as the command runs them (OpenWarehousePage): ask the page, read what it asked inside the command, hand the reading.
            public WarehousePageAsk Shortcut(WarehouseLayoutMode mode, string? designated = null, string? note = null,
                Func<WarehouseReadRequest, WarehouseReading>? read = null)
            {
                var ask = new WarehousePageAsk(mode, designated!, note!);
                WarehousePageAsked?.Invoke(ask);
                if (ask.Taken && ask.Read != null)
                {
                    ShortcutReads.Add(ask.Read);
                    WarehouseShortcutRead?.Invoke(ask, (read ?? (r => RackCadWarehousePageTests.Reading(r)))(ask.Read));
                }

                return ask;
            }
        }

        /// <summary>The settings of the user, in memory: what the page saved last.</summary>
        private sealed class SettingsDouble : IRackCadSettingsAccess, IRackCadWarehouseSettings
        {
            public string BlockLibraryPath => string.Empty;

            public bool BlockLibraryOverridden => false;

            public bool BlockLibraryExists => false;

            public string DesignLibraryPath => string.Empty;

            public void SetBlockLibraryPath(string path)
            {
            }

            public WarehouseLayoutValues? WarehouseValues { get; set; }

            public int Saves { get; private set; }

            public void SetWarehouseValues(WarehouseLayoutValues values)
            {
                WarehouseValues = values;
                Saves++;
            }
        }

        private sealed class Rig
        {
            public Rig(bool withDoor = true, WarehouseLayoutValues? saved = null)
            {
                Settings.WarehouseValues = saved;
                Host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7) };
                Summary = new WorkspaceBrowserSummaryPortDouble
                {
                    ListResponder = (_, _) => BrowserRackList.Ready(new[]
                    {
                        new BrowserRackListEntry(RackId, "Selectivo A", "selective", new[] { "frontal", "lateral", "planta" }),
                        new BrowserRackListEntry("SEL-2", "Selectivo B", "selective", new[] { "planta" }),
                        new BrowserRackListEntry("PB-1", "Push Back 1", "pushback", new[] { "lateral", "planta" }),
                        new BrowserRackListEntry("CAN-1", "Cantilever 1", "cantilever", new[] { "frontal", "lateral" }),
                    }),
                };
                Panel = new WorkspacePanelViewModel(Host, _ => { }, new WorkspaceNavigationPortDouble(), Summary, new WorkspacePilotEditPortDouble());
                Window = new RackCadWindowViewModel(Panel, null, Settings, withDoor ? Door : null, UiSupport.LoadCatalogSafe);
                Window.Refresh();
            }

            public WorkspacePortDouble Host { get; }

            public WorkspaceBrowserSummaryPortDouble Summary { get; }

            public WorkspacePanelViewModel Panel { get; }

            public WarehouseDoor Door { get; } = new();

            public SettingsDouble Settings { get; } = new();

            public RackCadWindowViewModel Window { get; }

            public WarehousePageViewModel Page => Window.Warehouse;

            public WarehouseReadRequest LastRead => Door.Reads.Last();

            public void Present(long session = 7, WorkspaceActionBlock block = WorkspaceActionBlock.None)
            {
                Host.Snapshot = new WorkspaceHostSnapshot(WorkspaceHostStatus.Ready, new SessionId(session), true,
                    new WorkspaceActionAvailability(block == WorkspaceActionBlock.None, false, block), SelectionContext.None, false, false);
                Window.Refresh();
            }

            // RACKALMACENLEER: it takes the last read, answers with what it read and ends (the drawing at rest again).
            public WarehouseReading AnswerRead(Func<WarehouseReadRequest, WarehouseReading>? reading = null)
            {
                var request = LastRead;
                Door.Start(request.RequestId);
                var answer = (reading ?? (r => Reading(r)))(request);
                Door.Read(answer);
                Present();
                return answer;
            }

            // A rack selected in AutoCAD (the selection is followed by default).
            public void Select(string rack)
            {
                Host.Snapshot = WorkspacePanelFixtures.Ready(session: 7, selection: SelectionContext.One(rack));
                Window.Refresh();
            }

            // The section opened with a click, and its first read answered.
            public WarehouseReading Open()
            {
                Window.ShowPage(RackCadWindowPage.Almacen);
                return AnswerRead();
            }

            // «Editar aquí» of the Selectivo of the drawing and a change in its draft (not written).
            public SelectiveEditorViewModel EditSelective()
            {
                Window.OpenRack(RackId);
                Window.EditSelectiveHere();
                Door.AnswerLastSelectiveRead();
                var editor = Window.ActiveSelectiveEditor!;
                Write(Field(editor.CellFields, "cHeight"), "72");
                Assert.True(editor.IsDirty);
                return editor;
            }
        }

        // ------------------------------------------------------------------ what AutoCAD reads in the tests

        private static readonly Dictionary<string, string> Names = new()
        {
            [RackId] = "Selectivo A", ["SEL-2"] = "Selectivo B", ["PB-1"] = "Push Back 1",
        };

        private static WarehouseSeedFacts Seed(string rackId, double originX = 100, double originY = 50, double footprintX = 48, double footprintY = 96) =>
            new(rackId, Names.TryGetValue(rackId, out var name) ? name : rackId, "selective", "2A1", "1F0", originX, originY, footprintX, footprintY, 0, 0,
                0, 1, 1, 1, "H1", true, true, false);

        // The L of the prototype (the nave of the tests of the fill ×2.4) with one column; MUROS has no outline.
        private static WarehouseSiteFacts Site(string? layer) =>
            string.Equals(layer, "MUROS", StringComparison.Ordinal)
                ? new WarehouseSiteFacts("MUROS", "No encontré una polilínea CERRADA en la capa 'MUROS' que sirva de contorno del sitio.", null, Array.Empty<WarehouseObstacleFact>(), 0)
                : new WarehouseSiteFacts(layer ?? "RACKCAD_SITIO", null,
                    new[] { new Point2D(0, 0), new Point2D(1200, 0), new Point2D(1200, 480), new Point2D(720, 480), new Point2D(720, 960), new Point2D(0, 960) },
                    new[] { new WarehouseObstacleFact(444, 420, 12, 12, "columna 1") }, 0);

        private static WarehouseReading Reading(WarehouseReadRequest request, int plantas = 1, WarehouseSeedFacts? seed = null) =>
            new(request.RequestId, null, request.SeedRackId, Names.TryGetValue(request.SeedRackId, out var name) ? name : request.SeedRackId, plantas,
                seed ?? Seed(request.SeedRackId), null, Site(request.Layer), new[] { "0", "MUROS", "RACKCAD_LAYOUT", "RACKCAD_SITIO" });

        // ------------------------------------------------------------------ reading after a gesture

        [Fact]
        public void TheSectionIsBetweenTheBomAndTheLibraryAndItsFirstClickReadsThePlantaOfTheSeedOnce()
        {
            var rig = new Rig();
            Assert.Equal(new[] { "Nuevo", "Variables", "BOM y lista de racks", "Almacén", "Biblioteca de diseños", "Configuración" },
                rig.Window.Sections.Select(s => s.Label).ToArray());
            Assert.Empty(rig.Door.Reads); // nothing leaves the window without a gesture

            rig.Window.ShowPage(RackCadWindowPage.Almacen);
            Assert.True(rig.Window.IsAlmacenPage);
            var read = Assert.Single(rig.Door.Reads);
            Assert.Equal((RackId, "RACKCAD_SITIO", (string?)null), (read.SeedRackId, read.Layer, read.PreferredSeedHandle));
            Assert.Equal(Session, read.Session);
            Assert.True(rig.Window.IsWaitingForAutoCad);
            Assert.Equal("Esperando a que AutoCAD lea la planta y el sitio. Si no responde (por ejemplo, porque había texto a medio escribir en la línea de órdenes), pulsa «Dejar de esperar».", rig.Window.WaitingText);
            Assert.Equal("Leyendo el dibujo", rig.Page.ChipText);
            Assert.False(rig.Page.CanPlace);

            rig.AnswerRead();
            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.Equal("Lectura al día", rig.Page.ChipText);
            Assert.Equal("Huella leída: 48 × 96 in (a lo profundo × a lo largo). 1 planta de este rack en el dibujo.", rig.Page.SeedLine);
            Assert.Equal("Colocar 3 racks", rig.Page.PlaceText); // 4 × 1 of the classic window, the seed included
            Assert.True(rig.Page.CanPlace);

            // Showing the page again, changing drawing and coming back send nothing.
            rig.Window.ShowPage(RackCadWindowPage.Bom);
            rig.Window.ShowPage(RackCadWindowPage.Almacen);
            rig.Present(session: 8);
            rig.Present();
            Assert.Single(rig.Door.Reads);
            Assert.Equal("Puede no estar al día", rig.Page.ChipText); // coming back to a drawing
        }

        [Fact]
        public void TheSeedsAreTheRacksWithPlantaAndANewSeedOrLayerIsReadAsAGesture()
        {
            var rig = new Rig();
            rig.Open();

            Assert.Equal(new[] { "Selectivo A · Selectivo", "Selectivo B · Selectivo", "Push Back 1 · Push Back" }, rig.Page.SeedOptions.Select(o => o.Label));
            Assert.Equal("Sin planta, no se ofrecen: Cantilever 1. Inserta su planta con «Insertar vista…» para usarlos.", rig.Page.NoPlantaText);

            rig.Page.ChooseSeed("PB-1");
            Assert.Equal(2, rig.Door.Reads.Count);
            Assert.Equal("PB-1", rig.LastRead.SeedRackId);
            Assert.Equal(WarehouseLayoutRules.NotReadText, rig.Page.SeedLine); // the reading of another seed is no reading of this one
            rig.AnswerRead();
            Assert.Equal("PB-1", rig.Page.SelectedSeedId);

            // «Volver a leer» keeps the planta it read (when nothing is selected in AutoCAD).
            rig.Page.Read();
            Assert.Equal("2A1", rig.LastRead.PreferredSeedHandle);
            rig.AnswerRead();

            rig.Page.ShowFill();
            rig.Page.ChooseLayer("MUROS");
            Assert.Equal(4, rig.Door.Reads.Count);
            Assert.Equal("MUROS", rig.LastRead.Layer);
            Assert.Equal(WarehouseLayoutRules.SiteNotReadText("MUROS"), rig.Page.SiteError);
            rig.AnswerRead();
            Assert.Equal("No encontré una polilínea CERRADA en la capa 'MUROS' que sirva de contorno del sitio.", rig.Page.SiteError);
            Assert.Equal(WarehouseLayoutRules.NoOutlineText, rig.Page.PlaceBlockedText);
            Assert.True(rig.Page.FillTabHasProblems);
            Assert.Equal(new[] { "0", "MUROS", "RACKCAD_LAYOUT", "RACKCAD_SITIO" }, rig.Page.LayerOptions);
        }

        [Fact]
        public void ASeedWithManyPlantasOrADraftWithChangesIsSaidAndNotBlocked()
        {
            var rig = new Rig();
            var editor = rig.EditSelective();
            rig.Window.ShowPage(RackCadWindowPage.Almacen);
            rig.AnswerRead(r => Reading(r, plantas: 3));

            Assert.Equal("Selectivo A · Selectivo · con cambios sin escribir", rig.Page.SeedOptions[0].Label);
            Assert.Equal("Selectivo A tiene cambios sin escribir en la ventana: el layout copia lo que hay en el dibujo. Si quieres los cambios, escríbelos antes con «Actualizar» en su editor.", rig.Page.DraftWarning);
            Assert.Equal("Ya hay 3 plantas de Selectivo A en el dibujo. La semilla es la que esté seleccionada en AutoCAD. Repetir el layout desde la misma semilla deja la rejilla nueva encima de la anterior.", rig.Page.MultiWarning);
            Assert.True(rig.Page.CanPlace); // question 4: not blocked

            // Several plantas and none selected: the reading has no seed and says so; «Colocar» is off with that reason.
            rig.Page.Read();
            rig.AnswerRead(r => Reading(r, plantas: 3) with { Seed = null, SeedProblem = "Hay 3 plantas de Selectivo A en el dibujo y ninguna está seleccionada: selecciona en AutoCAD la que sirve de semilla y pulsa «Volver a leer»." });
            Assert.Equal("Hay 3 plantas de Selectivo A en el dibujo y ninguna está seleccionada: selecciona en AutoCAD la que sirve de semilla y pulsa «Volver a leer».", rig.Page.SeedProblem);
            Assert.Equal(rig.Page.SeedProblem, rig.Page.PlaceBlockedText);
            Assert.False(rig.Page.CanPlace);
            GC.KeepAlive(editor);
        }

        // ------------------------------------------------------------------ the result, at once

        [Fact]
        public void TheResultAndTheSchemeFollowEveryValueAtOnceAndEachTabSaysItsProblems()
        {
            var rig = new Rig();
            rig.Open();

            rig.Page.RowsText = "3";
            rig.Page.ColsText = "4";
            Assert.Equal("Colocar 11 racks", rig.Page.PlaceText);
            Assert.Equal(new[] { "Racks nuevos", "En la rejilla", "Ocupa", "Encaje" }, rig.Page.Stats.Select(s => s.Label));
            Assert.Equal(new[] { "11", "3 × 4 = 12", "336 × 672 in", "Sin verificar" }, rig.Page.Stats.Select(s => s.Value));
            Assert.Equal(3, rig.Page.SchemeStrips.Count);
            Assert.Equal(new[] { "A4", "A3", "A2", "A1" }, rig.Page.SchemeStrips[0].Cells.Select(c => c.Text)); // Y goes up: the first column at the bottom
            Assert.Equal("Seed", rig.Page.SchemeStrips[0].Cells.Last().Kind);
            Assert.Equal(new[] { "4", "3", "2", "1" }, rig.Page.SchemeNumbers.Select(n => n.Text));
            Assert.Equal("Esquema de la rejilla: 3 filas por 4 columnas; A1 es la semilla y hay 11 racks nuevos.", rig.Page.SchemeName);
            Assert.Empty(rig.Door.Places); // nothing is written

            // An invalid value is said in its field, marks its tab and turns «Colocar» off with the reason.
            rig.Page.ColsText = "0";
            Assert.Equal(WarehouseLayoutRules.IntegerText, rig.Page.ColsError);
            Assert.True(rig.Page.GridTabHasProblems);
            Assert.False(rig.Page.FillTabHasProblems);
            Assert.Equal(WarehouseLayoutRules.GridFixText, rig.Page.PlaceBlockedText);
            Assert.Equal("Colocar", rig.Page.PlaceText);
            Assert.Equal("Corrige los valores marcados para ver el esquema.", rig.Page.NoSchemeText);
            rig.Page.ColsText = "1";
            rig.Page.RowsText = "1";
            Assert.Equal(WarehouseLayoutRules.OneRackText, rig.Page.GridError);
            rig.Page.RowsText = "40";
            rig.Page.ColsText = "25";
            Assert.Equal("El esquema se dibuja hasta 30 filas × 24 columnas; esta rejilla tiene 40 × 25. El recuento y los problemas valen igual.", rig.Page.NoSchemeText);
            rig.Page.RowsText = "4";
            rig.Page.ColsText = "1";

            // The gap counts only with its switch; with back to back the rotated try is off and says why (dolor 7).
            rig.Page.BackGapText = "-1";
            Assert.Equal(string.Empty, rig.Page.BackGapError);
            Assert.False(rig.Page.BackGapEnabled);
            rig.Page.BackToBack = true;
            Assert.Equal(WarehouseLayoutRules.NonNegativeText, rig.Page.BackGapError);
            Assert.True(rig.Page.BackGapEnabled);
            Assert.False(rig.Page.TryRotatedEnabled);
            Assert.False(rig.Page.TryRotatedShown);
            Assert.StartsWith("Con espalda con espalda solo se prueba la orientación natural", rig.Page.RotatedHint, StringComparison.Ordinal);
            rig.Page.BackGapText = "6";

            // «Rellenar un área»: the auto-fill over the site, with the candidates outside and on the column.
            rig.Page.BackToBack = false;
            rig.Page.ShowFill();
            Assert.Equal(new[] { "Caben", "Orientación", "Rejilla probada", "Descartadas" }, rig.Page.Stats.Select(s => s.Label));
            var fill = rig.Page.Assessment.Fill!;
            Assert.Equal(fill.Kept.ToString(System.Globalization.CultureInfo.InvariantCulture), rig.Page.Stats[0].Value);
            Assert.Equal("Colocar " + fill.Kept + " racks", rig.Page.PlaceText);
            Assert.Contains(rig.Page.SchemeStrips.SelectMany(s => s.Cells), c => c.Kind == "Off");
            Assert.Equal(new[] { "New", "Off", "Col" }, rig.Page.Legend.Select(l => l.Kind));
            Assert.Equal("La planta de Selectivo A está dentro del área a rellenar: el relleno no la tiene en cuenta y una copia puede quedar encima. Muévela o bórrala después.", rig.Page.InsideWarning);
            Assert.Equal("Siempre enlazadas: un solo bloque, editar una edita todas y el BOM las cuenta como copias del mismo rack.", rig.Page.CopyText);
        }

        // ------------------------------------------------------------------ «Colocar»

        [Fact]
        public void ColocarIsOneRequestOfTheDoorWithTheMarkOfTheReadingAndItsAnswerBringsTheNewReading()
        {
            var rig = new Rig();
            var reading = rig.Open();
            rig.Page.ChooseIndependent();
            rig.Page.Place();

            var place = Assert.Single(rig.Door.Places);
            Assert.Equal((WarehouseLayoutMode.Grid, RackId, "2A1", false), (place.Mode, place.SeedRackId, place.SeedHandle, place.PlaceAnyway));
            Assert.Equal(WarehouseLayoutRules.Fingerprint(reading, WarehouseLayoutMode.Grid), place.Fingerprint);
            Assert.True(place.Values.Independent);
            Assert.Equal(rig.Page.Values, place.Values);
            Assert.True(rig.Page.IsWriting);
            Assert.Equal("Escribiendo", rig.Page.ChipText);
            Assert.False(rig.Page.CanPlace);
            Assert.False(rig.Page.IsEditable);
            Assert.True(rig.Page.CanStopWriting);
            Assert.Equal("AutoCAD coloca 3 racks independientes de Selectivo A y 4 etiquetas en la capa RACKCAD_LAYOUT (RACKALMACENCOLOCAR, petición n.º "
                         + place.RequestId + "). Antes vuelve a leer la planta: si cambió, no coloca nada.", rig.Page.WritingText);
            Assert.False(rig.Window.CanTakeToWarehouse); // the page is placing

            // The command takes it: no more «Dejar de esperar»; a command of its own is no change of the drawing.
            rig.Door.Start(place.RequestId);
            Assert.False(rig.Page.CanStopWriting);
            Assert.False(rig.Window.CanStopWaiting);
            Assert.Equal("La orden ya corre en AutoCAD: se termina o se cancela allí, y la respuesta siempre llega.", rig.Page.WritingNote);
            rig.Present(block: WorkspaceActionBlock.CommandActive);

            var after = Reading(rig.LastRead, plantas: 1) with { RequestId = place.RequestId };
            rig.Door.Placed(new WarehousePlaced(place.RequestId, true, "Layout colocado: 3 racks nuevos (4 × 1, independientes) y 4 etiquetas en la capa RACKCAD_LAYOUT. Un «U» en AutoCAD lo deshace.", 3, after));
            rig.Present(block: WorkspaceActionBlock.CommandActive);
            rig.Present();
            Assert.False(rig.Page.IsWriting);
            Assert.False(rig.Page.MessageIsError);
            Assert.StartsWith("Layout colocado: 3 racks nuevos", rig.Page.Message, StringComparison.Ordinal);
            Assert.Equal("Lectura al día", rig.Page.ChipText);
            Assert.Single(rig.Door.Reads); // no read is asked inside the answer of the write (F-2)
            Assert.Equal(rig.Page.Values, rig.Settings.WarehouseValues); // the values used are remembered
        }

        [Fact]
        public void ADrawingThatChangedPlacesNothingAndThePageTakesTheNewReading()
        {
            var rig = new Rig();
            rig.Open();
            rig.Page.Place();
            var place = rig.Door.Places.Single();
            rig.Door.Start(place.RequestId);

            var moved = Reading(rig.LastRead, seed: Seed(RackId, originX: 400)) with { RequestId = place.RequestId };
            rig.Door.Placed(new WarehousePlaced(place.RequestId, false, WarehouseLayoutRules.ChangedText(WarehouseLayoutMode.Grid, "Selectivo A"), 0, moved));
            rig.Present();

            Assert.True(rig.Page.MessageIsError);
            Assert.Equal("No se colocó nada: la planta de Selectivo A cambió en el dibujo desde la lectura. La página la volvió a leer: revisa el esquema y vuelve a pulsar «Colocar».", rig.Page.Message);
            Assert.Equal(400, rig.Page.Assessment.Grid!.Plan.Cells[0].X); // the scheme is the one of the new reading
            Assert.True(rig.Page.CanPlace);
        }

        [Fact]
        public void AGridThatLeavesItsBuildingAsksInLineBeforePlacing()
        {
            var rig = new Rig();
            rig.Open();
            rig.Page.ColsText = "2";
            rig.Page.BuildingXText = "400";
            Assert.True(rig.Page.FitOpen); // a side without the other opens «Verificar encaje» with its error
            Assert.Equal(WarehouseLayoutRules.BothSidesText, rig.Page.BuildingYError);
            rig.Page.BuildingYText = "200";
            Assert.Equal("Edificio 400 × 200 in", rig.Page.FitSummary);
            Assert.Equal("5 se salen", rig.Page.Stats.Single(s => s.Label == "Encaje").Value);

            rig.Page.Place();
            Assert.Empty(rig.Door.Places);
            Assert.True(rig.Page.ShowFitQuestion);
            Assert.Equal("Se salen del edificio de 400 × 200 in: A2, B2, C2, D1 y D2. ¿Colocar la rejilla de todos modos?", rig.Page.FitQuestionText);

            rig.Page.KeepAdjusting();
            Assert.False(rig.Page.ShowFitQuestion);
            rig.Page.Place();
            rig.Page.PlaceAnyway();
            Assert.True(Assert.Single(rig.Door.Places).PlaceAnyway);
        }

        // ------------------------------------------------------------------ the door of the window

        [Fact]
        public void OneRequestWaitsForTheWholeWindowAndOnlyItsAnswerCounts()
        {
            var rig = new Rig();
            rig.Window.OpenRack(RackId);
            rig.Window.EditSelectiveHere(); // a read of a Selectivo waits
            rig.Window.ShowPage(RackCadWindowPage.Almacen);
            Assert.Empty(rig.Door.Reads);
            Assert.Equal("AutoCAD todavía no respondió a la petición anterior de la ventana: espera a que termine o pulsa «Dejar de esperar».", rig.Page.Message);
            Assert.False(rig.Page.CanRead);
            Assert.Equal("La ventana espera otra respuesta de AutoCAD: espera a que termine.", rig.Page.PlaceBlockedText);
            rig.Door.AnswerLastSelectiveRead();

            rig.Page.Read();
            var read = rig.Door.Reads.Single();
            rig.Door.Read(Reading(read) with { RequestId = read.RequestId + 50 }); // another number
            Assert.True(rig.Page.IsReading);
            Assert.Null(rig.Page.CurrentReading);

            // An answer that arrives with another drawing active is not used.
            rig.Host.Snapshot = WorkspacePanelFixtures.Ready(session: 8);
            rig.Door.Read(Reading(read));
            Assert.False(rig.Page.IsReading);
            Assert.StartsWith("La lectura n.º " + read.RequestId + " no es del dibujo para el que se pidió", rig.Page.Message, StringComparison.Ordinal);
            rig.Present();
            Assert.Null(rig.Page.CurrentReading);
            Assert.False(rig.Window.IsWaitingForAutoCad);
        }

        [Fact]
        public void DejarDeEsperarDropsTheRequestUntilTheCommandTakesIt()
        {
            var rig = new Rig();
            rig.Window.ShowPage(RackCadWindowPage.Almacen);
            var read = rig.Door.Reads.Single();
            rig.Window.StopWaiting();
            Assert.Equal(new[] { read.RequestId }, rig.Door.Abandoned);
            Assert.False(rig.Page.IsReading);
            rig.Door.Read(Reading(read)); // a late answer does nothing
            Assert.Null(rig.Page.CurrentReading);

            rig.Page.Read();
            rig.AnswerRead();
            rig.Page.Place();
            var place = rig.Door.Places.Single();
            rig.Page.StopWaiting(); // the button of the banner
            Assert.Contains(place.RequestId, rig.Door.Abandoned);
            Assert.False(rig.Page.IsWriting);
            Assert.Equal("Se dejó de esperar a AutoCAD: si la orden llega a correr, no colocará nada.", rig.Page.Message);

            rig.Page.Place();
            var second = rig.Door.Places.Last();
            rig.Door.Start(second.RequestId);
            rig.Window.StopWaiting();
            Assert.DoesNotContain(second.RequestId, rig.Door.Abandoned);
            Assert.True(rig.Page.IsWriting);
            Assert.Equal("AutoCAD ya está con la petición de «Almacén»: termina o cancela la orden en AutoCAD; la ventana recibirá su respuesta.", rig.Window.ActionMessage);
        }

        [Fact]
        public void AFailedHandOverIsSaidAndAClosedDrawingDropsItsRequest()
        {
            var rig = new Rig();
            rig.Door.Answer = RackCadClassicResult.Without(RackCadClassicStatus.Busy, "Hay un comando en curso.");
            rig.Window.ShowPage(RackCadWindowPage.Almacen);
            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.False(rig.Page.IsReading);
            Assert.Equal("Hay un comando en curso en AutoCAD: termínalo o cancélalo (Esc) y vuelve a intentarlo.", rig.Page.Message);
            Assert.True(rig.Page.MessageIsError);

            rig.Door.Answer = RackCadClassicResult.Started();
            rig.Page.Read();
            Assert.True(rig.Window.IsWaitingForAutoCad);
            rig.Summary.Dead.Add(Session);
            rig.Present(session: 8);
            Assert.False(rig.Window.IsWaitingForAutoCad);
            Assert.Contains(rig.Door.Reads.Last().RequestId, rig.Door.Abandoned);
        }

        [Fact]
        public void AReadingMayBeOutOfDateAfterACommandThatIsNotItsOwn()
        {
            var rig = new Rig();
            rig.Window.ShowPage(RackCadWindowPage.Almacen);
            var read = rig.Door.Reads.Single();
            rig.Door.Start(read.RequestId);
            rig.Present(block: WorkspaceActionBlock.CommandActive); // the read itself runs
            rig.Door.Read(Reading(read));
            rig.Present(block: WorkspaceActionBlock.CommandActive);
            Assert.Equal("Lectura al día", rig.Page.ChipText);
            Assert.False(rig.Page.ShowStale);

            rig.Present();
            rig.Present(block: WorkspaceActionBlock.CommandActive); // a MOVE of the user
            Assert.Equal("Puede no estar al día", rig.Page.ChipText);
            Assert.True(rig.Page.ShowStale);
            Assert.True(rig.Page.CanPlace); // «Colocar» reads again before it writes (question 5)
            rig.Present();
            Assert.Single(rig.Door.Reads); // and nothing is read on its own
        }

        // ------------------------------------------------------------------ the values (question 6)

        [Fact]
        public void TheValuesAreRememberedPerUserAndRestablecerGoesBackToTheClassicOnes()
        {
            var saved = WarehouseLayoutValues.Defaults with { Rows = "6", Cols = "2", Layer = "MUROS", BackToBack = true };
            var rig = new Rig(saved: saved);
            Assert.Equal(0, rig.Settings.Saves); // nothing is read or written before the page is shown

            rig.Open();
            Assert.Equal(("6", "2", true), (rig.Page.RowsText, rig.Page.ColsText, rig.Page.BackToBack));
            Assert.Equal("MUROS", rig.LastRead.Layer);
            Assert.True(rig.Page.CanReset);

            // Correction 6 of the review: a value typed is not saved on every key; it is when its box loses the focus (CommitValues).
            var saves = rig.Settings.Saves;
            rig.Page.AisleRowsText = "1";
            rig.Page.AisleRowsText = "12";
            rig.Page.AisleRowsText = "120";
            Assert.Equal(saves, rig.Settings.Saves);
            Assert.Equal("96", rig.Settings.WarehouseValues!.AisleRows);
            rig.Page.CommitValues();
            Assert.Equal(saves + 1, rig.Settings.Saves);
            Assert.Equal("120", rig.Settings.WarehouseValues!.AisleRows);
            rig.Page.CommitValues();
            Assert.Equal(saves + 1, rig.Settings.Saves); // nothing new to save

            rig.Page.Reset();
            Assert.Equal(WarehouseLayoutValues.Defaults, rig.Page.Values);
            Assert.Equal(WarehouseLayoutValues.Defaults, rig.Settings.WarehouseValues);
            Assert.False(rig.Page.CanReset);
            Assert.Equal("Valores restablecidos: los de las ventanas clásicas.", rig.Page.Message);
            Assert.Equal(2, rig.Door.Reads.Count); // the layer changed: its site is read again
            Assert.Equal("RACKCAD_SITIO", rig.LastRead.Layer);

            // «Almacén» has nothing unwritten: the drawing closes.
            Assert.Null(rig.Window.UnwrittenWorkOf(Session));
        }

        [Fact]
        public void WithoutTheDoorOfThePageThePageSaysSoAndSendsNothing()
        {
            var rig = new Rig(withDoor: false);
            rig.Window.ShowPage(RackCadWindowPage.Almacen);
            Assert.Equal("Aquí no se puede leer el dibujo.", rig.Page.Message);
            Assert.Equal("Aquí no se puede leer ni escribir el dibujo.", rig.Page.PlaceBlockedText);
            Assert.False(rig.Window.ShowTakeToWarehouse);
        }

        // ------------------------------------------------------------------ «Llevar al almacén…» (question 1)

        [Fact]
        public void LlevarAlAlmacenOpensTheSectionWithTheRackAsTheSeedAndReadsIt()
        {
            var rig = new Rig();
            rig.Window.OpenRack("CAN-1");
            Assert.True(rig.Window.ShowTakeToWarehouse);
            Assert.False(rig.Window.CanTakeToWarehouse);
            Assert.Equal("Sin vista en planta: insértala con «Insertar vista…» (o RACKEDITAR) para usar este rack como semilla.", rig.Window.TakeToWarehouseReason);

            rig.Window.OpenRack("PB-1");
            Assert.True(rig.Window.CanTakeToWarehouse);
            Assert.Equal(string.Empty, rig.Window.TakeToWarehouseReason);
            rig.Window.TakeActiveRackToWarehouse();
            Assert.True(rig.Window.IsAlmacenPage);
            Assert.True(rig.Page.IsGridTab);
            Assert.Equal("PB-1", Assert.Single(rig.Door.Reads).SeedRackId);
            rig.AnswerRead();

            // Again from the page of the rack, with a reading already: a gesture reads again.
            rig.Window.OpenRack("PB-1");
            rig.Window.TakeActiveRackToWarehouse();
            Assert.Equal(2, rig.Door.Reads.Count);
        }

        // ------------------------------------------------------------------ the corrections of the review

        // Correction 1: AutoCAD placed but the reading after it failed: the page says it was placed and «Colocar» waits for «Volver a leer»,
        // so a second «Colocar» cannot put another grid on top.
        [Fact]
        public void APlacementWhoseReadingAfterFailedSaysItWasPlacedAndAsksToReadAgain()
        {
            var rig = new Rig();
            rig.Open();
            rig.Page.Place();
            var place = rig.Door.Places.Single();
            rig.Door.Start(place.RequestId);
            rig.Door.Placed(new WarehousePlaced(place.RequestId, true, "Layout colocado: 3 racks nuevos (4 × 1, enlazados) y 4 etiquetas en la capa RACKCAD_LAYOUT. Un «U» en AutoCAD lo deshace.", 3,
                WarehouseReading.Failed(place.RequestId, RackId, "No se pudo volver a leer el dibujo después de colocar: fallo.")));
            rig.Present();

            Assert.False(rig.Page.MessageIsError);
            Assert.StartsWith("Layout colocado: 3 racks nuevos", rig.Page.Message, StringComparison.Ordinal);
            Assert.EndsWith(WarehousePageViewModel.MustRereadText, rig.Page.Message, StringComparison.Ordinal);
            Assert.False(rig.Page.CanPlace);
            Assert.Equal(WarehousePageViewModel.MustRereadText, rig.Page.PlaceBlockedText);
            Assert.Equal("Puede no estar al día", rig.Page.ChipText);
            Assert.True(rig.Page.CanRead);

            rig.Page.Read();
            rig.AnswerRead(r => Reading(r, plantas: 4));
            Assert.True(rig.Page.CanPlace);
            Assert.Equal("Lectura al día", rig.Page.ChipText);
            Assert.StartsWith("Ya hay 4 plantas de Selectivo A", rig.Page.MultiWarning, StringComparison.Ordinal);
        }

        // Correction 2: on the layer of the site, the references placed would be read as obstacles next time: said before «Colocar».
        [Fact]
        public void TheCurrentLayerOnTheLayerOfTheSiteIsSaidBeforePlacing()
        {
            var rig = new Rig();
            rig.Window.ShowPage(RackCadWindowPage.Almacen);
            rig.AnswerRead(r => Reading(r) with { CurrentLayer = "RACKCAD_SITIO" });
            Assert.Equal(string.Empty, rig.Page.CurrentLayerWarning); // «Rejilla» does not read the site
            rig.Page.ShowFill();
            Assert.StartsWith("La capa actual del dibujo es «RACKCAD_SITIO», la del sitio:", rig.Page.CurrentLayerWarning, StringComparison.Ordinal);
            Assert.True(rig.Page.CanPlace); // the computation of today: it warns, it does not block

            rig.Page.Read();
            rig.AnswerRead(r => Reading(r) with { CurrentLayer = "0" });
            Assert.Equal(string.Empty, rig.Page.CurrentLayerWarning);
        }

        // Correction 6: the values typed are remembered when the page is left too; a switch or a choice at once.
        [Fact]
        public void TheValuesTypedAreRememberedWhenThePageIsLeftAndASwitchAtOnce()
        {
            var rig = new Rig();
            rig.Open();
            var saves = rig.Settings.Saves;
            rig.Page.RowsText = "7";
            Assert.Equal(saves, rig.Settings.Saves);
            rig.Window.ShowPage(RackCadWindowPage.Bom);
            Assert.Equal(saves + 1, rig.Settings.Saves);
            Assert.Equal("7", rig.Settings.WarehouseValues!.Rows);

            rig.Window.ShowPage(RackCadWindowPage.Almacen);
            rig.Page.BackToBack = true;
            Assert.Equal(saves + 2, rig.Settings.Saves);
            rig.Page.ChooseIndependent();
            Assert.Equal(saves + 3, rig.Settings.Saves);
            Assert.True(rig.Settings.WarehouseValues!.Independent);
        }

        // Correction 7: while the page reads, «Llevar al almacén…» waits and says why; a seed chosen meanwhile is not lost in silence.
        [Fact]
        public void LlevarAlAlmacenWaitsForTheReadOfThePageAndAChoiceDuringItIsSaid()
        {
            var rig = new Rig();
            rig.Window.ShowPage(RackCadWindowPage.Almacen); // reads Selectivo A
            rig.Window.OpenRack("PB-1");
            Assert.False(rig.Window.CanTakeToWarehouse);
            Assert.Equal("«Almacén» espera a AutoCAD (lee la planta o coloca): espera a que termine.", rig.Window.TakeToWarehouseReason);
            rig.Window.ShowPage(RackCadWindowPage.Almacen);
            rig.Page.ChooseSeed("SEL-2");
            Assert.Single(rig.Door.Reads);
            Assert.Equal("AutoCAD todavía está leyendo la planta: cuando termine, pulsa «Volver a leer» para leer lo que elegiste.", rig.Page.Message);

            rig.AnswerRead();
            rig.Window.OpenRack("PB-1");
            Assert.True(rig.Window.CanTakeToWarehouse);
            Assert.Equal(string.Empty, rig.Window.TakeToWarehouseReason);
        }

        // Correction 8: a layer of AutoCAD compares without case: the list shows the one of the drawing chosen, and choosing it reads nothing.
        [Fact]
        public void TheLayerChosenIsTheOneOfTheDrawingWhateverItsCase()
        {
            var rig = new Rig();
            rig.Window.ShowPage(RackCadWindowPage.Almacen);
            rig.AnswerRead(r => Reading(r) with { Layers = new[] { "0", "Rackcad_Sitio" }, Site = Site("Rackcad_Sitio") });
            Assert.Equal(new[] { "0", "Rackcad_Sitio" }, rig.Page.LayerOptions);
            Assert.Equal("Rackcad_Sitio", rig.Page.SelectedLayer);
            rig.Page.ShowFill();
            Assert.Equal(string.Empty, rig.Page.SiteError);
            rig.Page.ChooseLayer("Rackcad_Sitio");
            Assert.Single(rig.Door.Reads);
        }

        // Correction 9: in AutoCAD the command starts (CommandWillStart) BEFORE it says it took the request. The reading before it may be
        // marked meanwhile; the answer brings one up to date, and only a command after the rest counts again.
        [Fact]
        public void InTheOrderOfAutoCadTheOwnReadAndTheOwnPlacementLeaveTheReadingUpToDate()
        {
            var rig = new Rig();
            rig.Open();

            rig.Page.Read();
            var read = rig.LastRead;
            rig.Present(block: WorkspaceActionBlock.CommandActive); // RACKALMACENLEER begins
            rig.Door.Start(read.RequestId);
            rig.Present(block: WorkspaceActionBlock.CommandActive);
            rig.Door.Read(Reading(read));
            rig.Present(block: WorkspaceActionBlock.CommandActive); // still RACKALMACENLEER, after its answer
            rig.Present(); // CommandEnded
            Assert.Equal("Lectura al día", rig.Page.ChipText);

            rig.Page.Place();
            var place = rig.Door.Places.Single();
            rig.Present(block: WorkspaceActionBlock.CommandActive); // RACKALMACENCOLOCAR begins
            rig.Door.Start(place.RequestId);
            rig.Present(block: WorkspaceActionBlock.CommandActive);
            rig.Door.Placed(new WarehousePlaced(place.RequestId, true, "Layout colocado.", 3, Reading(read, plantas: 4) with { RequestId = place.RequestId }));
            rig.Present(block: WorkspaceActionBlock.CommandActive);
            rig.Present();
            Assert.Equal("Lectura al día", rig.Page.ChipText);
            Assert.True(rig.Page.CanPlace);

            rig.Present(block: WorkspaceActionBlock.CommandActive); // a command of the user
            Assert.Equal("Puede no estar al día", rig.Page.ChipText);
        }

        // AL-S15: selecting the planta in AutoCAD is part of the page (question 3): the window stays on «Almacén» and sends nothing.
        [Fact]
        public void ASelectionInAutoCadKeepsThePageOfAlmacenAndSendsNothing()
        {
            var rig = new Rig();
            rig.Open();
            rig.Select("PB-1");
            Assert.Equal("PB-1", rig.Panel.Browser.ActiveRackId); // the side column follows the selection
            Assert.True(rig.Window.IsAlmacenPage);
            Assert.Single(rig.Door.Reads);
            Assert.Equal(RackId, rig.Page.SelectedSeedId);

            // Elsewhere the selection still shows the page of the rack.
            rig.Window.ShowPage(RackCadWindowPage.Bom);
            rig.Select("SEL-2");
            Assert.True(rig.Window.IsRackPage);
        }

        // ------------------------------------------------------------------ T6 rl: RLY and RR open the page (AL-S14)

        // RLY with a rack selected in AutoCAD: «Almacén» › «Rejilla» with that rack as the seed, read inside the shortcut (the gesture): no
        // request of the door, and the rest of the shortcut is the page's own command.
        [Fact]
        public void RlyOpensRejillaWithTheSelectedRackAsTheSeedReadInsideItsCommandAndSendsNothing()
        {
            var rig = new Rig();
            rig.Select("PB-1"); // the selection, followed: the page of the rack
            Assert.True(rig.Window.IsRackPage);

            var ask = rig.Door.Shortcut(WarehouseLayoutMode.Grid, designated: "PB-1");
            Assert.True(ask.Taken);
            Assert.Null(ask.Note);
            var read = Assert.Single(rig.Door.ShortcutReads);
            Assert.Equal((0L, Session, "PB-1", "RACKCAD_SITIO", (string?)null), (read.RequestId, read.Session, read.SeedRackId, read.Layer, read.PreferredSeedHandle));
            Assert.True(rig.Window.IsAlmacenPage);
            Assert.True(rig.Page.IsGridTab);
            Assert.Equal("PB-1", rig.Page.SelectedSeedId);
            Assert.Equal("Lectura al día", rig.Page.ChipText);
            Assert.Equal("Colocar 3 racks", rig.Page.PlaceText);
            Assert.True(rig.Page.CanPlace);
            Assert.Empty(rig.Door.Reads); // no RACKALMACENLEER: the read was the shortcut's own
            Assert.False(rig.Window.IsWaitingForAutoCad);

            rig.Present(block: WorkspaceActionBlock.CommandActive); // the rest of RLY
            Assert.Equal("Lectura al día", rig.Page.ChipText);
            rig.Present();
            rig.Select("PB-1"); // AL-S15: the selection keeps «Almacén»
            Assert.True(rig.Window.IsAlmacenPage);
            Assert.Equal("Lectura al día", rig.Page.ChipText);
            Assert.Empty(rig.Door.Reads);

            rig.Present(block: WorkspaceActionBlock.CommandActive); // a command of the user, after it
            Assert.Equal("Puede no estar al día", rig.Page.ChipText);
        }

        // RR: «Rellenar un área», with the layer of the values remembered (the page asks it, the shortcut reads its site). The rack selected
        // replaces the seed the page had.
        [Fact]
        public void RrOpensRellenarUnAreaWithTheSelectedRackAndTheLayerOfTheValues()
        {
            var rig = new Rig(saved: WarehouseLayoutValues.Defaults with { Layer = "MUROS" });
            rig.Open(); // by hand: Selectivo A
            Assert.Equal(RackId, rig.Page.SelectedSeedId);
            rig.Window.ShowPage(RackCadWindowPage.Nuevo);
            rig.Present(block: WorkspaceActionBlock.CommandActive); // RR runs
            rig.Door.Shortcut(WarehouseLayoutMode.Fill, designated: "SEL-2");

            var read = Assert.Single(rig.Door.ShortcutReads);
            Assert.Equal(("SEL-2", "MUROS", (string?)null), (read.SeedRackId, read.Layer, read.PreferredSeedHandle));
            Assert.True(rig.Window.IsAlmacenPage);
            Assert.True(rig.Page.IsFillTab);
            Assert.Equal("SEL-2", rig.Page.SelectedSeedId);
            Assert.Equal("No encontré una polilínea CERRADA en la capa 'MUROS' que sirva de contorno del sitio.", rig.Page.SiteError);
            Assert.Single(rig.Door.Reads); // only the one of the click on the section
        }

        // Without a selection, the shortcut reads the seed the page takes by hand (as the first click on the section), and later the one the
        // page has, with the planta it read before.
        [Fact]
        public void WithoutASelectionTheShortcutReadsTheSeedThePageTakesByHand()
        {
            var rig = new Rig();
            rig.Present(block: WorkspaceActionBlock.CommandActive);
            rig.Door.Shortcut(WarehouseLayoutMode.Grid);
            Assert.Equal(RackId, rig.Door.ShortcutReads.Single().SeedRackId); // the first rack with planta of the column
            Assert.Equal("Lectura al día", rig.Page.ChipText);
            rig.Present();

            rig.Page.ChooseSeed("PB-1");
            rig.AnswerRead();
            rig.Window.ShowPage(RackCadWindowPage.Nuevo);
            rig.Present(block: WorkspaceActionBlock.CommandActive);
            rig.Door.Shortcut(WarehouseLayoutMode.Fill);
            var again = rig.Door.ShortcutReads.Last();
            Assert.Equal(("PB-1", "2A1"), (again.SeedRackId, again.PreferredSeedHandle));
            Assert.True(rig.Window.IsAlmacenPage);
            Assert.True(rig.Page.IsFillTab);
            Assert.Single(rig.Door.Reads); // only the one of «Rack semilla»
        }

        // Several racks selected: none is taken; the page keeps its seed and says why.
        [Fact]
        public void SeveralRacksSelectedTakeNoneAndThePageSaysSo()
        {
            var rig = new Rig();
            var designated = WarehouseLayoutRules.DesignatedSeed(
                new[] { new WarehouseSelectedRack("SEL-2", "Selectivo B", true), new WarehouseSelectedRack("PB-1", "Push Back 1", false) },
                WarehouseLayoutMode.Grid, out var note);
            Assert.Null(designated);

            var ask = rig.Door.Shortcut(WarehouseLayoutMode.Grid, designated, note);
            Assert.Equal(RackId, rig.Door.ShortcutReads.Single().SeedRackId);
            Assert.Null(ask.Note);
            Assert.Equal(note, rig.Page.Message);
            Assert.False(rig.Page.MessageIsError);
        }

        // While the page waits for AutoCAD (a read or a placement), the shortcut reads nothing, keeps the seed of the request and says why, as
        // «Llevar al almacén…» (correction 7); the answer that waits still counts.
        [Fact]
        public void WhileThePageWaitsForAutoCadTheShortcutReadsNothingAndSaysWhy()
        {
            var rig = new Rig();
            rig.Window.ShowPage(RackCadWindowPage.Almacen); // RACKALMACENLEER of Selectivo A waits
            rig.Window.ShowPage(RackCadWindowPage.Nuevo);

            var ask = rig.Door.Shortcut(WarehouseLayoutMode.Fill, designated: "PB-1");
            Assert.True(ask.Taken);
            Assert.Null(ask.Read);
            Assert.Equal(WarehouseLayoutRules.ShortcutWaitText(WarehouseLayoutMode.Fill), ask.Note);
            Assert.Equal(ask.Note, rig.Page.Message);
            Assert.True(rig.Window.IsAlmacenPage);
            Assert.Equal(RackId, rig.Page.SelectedSeedId);
            Assert.Single(rig.Door.Reads);
            Assert.Empty(rig.Door.ShortcutReads);

            rig.AnswerRead();
            Assert.Equal("Lectura al día", rig.Page.ChipText);
            Assert.Equal(RackId, rig.Page.SelectedSeedId);
        }

        // The read that waits is of another drawing: the shortcut in this one reads nothing either, and the page, shown by the shortcut, tries
        // no read of its own (its reason stays, not the refusal of the door).
        [Fact]
        public void WhileARequestOfThePageWaitsInAnotherDrawingTheShortcutSaysWhyAndThePageTriesNothing()
        {
            var rig = new Rig();
            rig.Window.ShowPage(RackCadWindowPage.Almacen); // RACKALMACENLEER of drawing 7 waits
            rig.Window.ShowPage(RackCadWindowPage.Nuevo);
            rig.Host.Snapshot = WorkspacePanelFixtures.Ready(session: 8); // RLY in drawing 8

            var ask = rig.Door.Shortcut(WarehouseLayoutMode.Grid, designated: "PB-1");
            Assert.Null(ask.Read);
            Assert.True(rig.Window.IsAlmacenPage);
            Assert.Equal(WarehouseLayoutRules.ShortcutWaitText(WarehouseLayoutMode.Grid), rig.Page.Message);
            Assert.Single(rig.Door.Reads);
        }

        // A rack selected without planta, or a read that failed: the page opens with it and says why, and nothing is placed or sent.
        [Fact]
        public void AShortcutWhoseReadingHasNoSeedOrFailedOpensThePageAndSaysWhy()
        {
            var rig = new Rig();
            const string noPlanta = "Cantilever 1 no tiene vista en planta: insértala con «Insertar vista…» (o RACKEDITAR) para usarlo como semilla.";
            rig.Door.Shortcut(WarehouseLayoutMode.Grid, designated: "CAN-1",
                read: r => new WarehouseReading(r.RequestId, null, r.SeedRackId, "Cantilever 1", 0, null, noPlanta, Site(r.Layer), new[] { "0" }));
            Assert.Equal("CAN-1", rig.Page.SelectedSeedId);
            Assert.Contains(rig.Page.SeedOptions, o => o.RackId == "CAN-1" && o.Label == "Cantilever 1"); // by the name of its reading
            Assert.Equal(noPlanta, rig.Page.SeedProblem);
            Assert.False(rig.Page.CanPlace);

            rig.Door.Shortcut(WarehouseLayoutMode.Grid, designated: "SEL-2",
                read: r => WarehouseReading.Failed(r.RequestId, r.SeedRackId, "No se pudo leer el dibujo: fallo."));
            Assert.Equal("No se pudo leer", rig.Page.ChipText);
            Assert.Equal("No se pudo leer el dibujo: fallo.", rig.Page.SeedProblem);
            Assert.Empty(rig.Door.Reads);
            Assert.Empty(rig.Door.Places);
        }

        // A window without the door of the page does not take the shortcut: the command says it could not open the page.
        [Fact]
        public void AWindowWithoutTheDoorOfThePageDoesNotTakeTheShortcut()
        {
            var rig = new Rig(withDoor: false);
            var ask = rig.Door.Shortcut(WarehouseLayoutMode.Grid, designated: "PB-1");
            Assert.False(ask.Taken);
            Assert.False(rig.Window.IsAlmacenPage);
            Assert.Empty(rig.Door.ShortcutReads);
        }

        // ------------------------------------------------------------------ the views, with real WPF

        private sealed class BindingErrors : TraceListener
        {
            public List<string> Messages { get; } = new();

            public override void Write(string? message)
            {
            }

            public override void WriteLine(string? message)
            {
                if (message != null) Messages.Add(message);
            }
        }

        private static void WithBindingErrors(Action body)
        {
            var errors = new BindingErrors();
            PresentationTraceSources.Refresh();
            var source = PresentationTraceSources.DataBindingSource;
            var level = source.Switch.Level;
            source.Listeners.Add(errors);
            source.Switch.Level = SourceLevels.Error;
            try
            {
                body();
            }
            finally
            {
                source.Listeners.Remove(errors);
                source.Switch.Level = level;
            }

            Assert.Empty(errors.Messages);
        }

        private static void Lay(FrameworkElement view, double width = 1300)
        {
            view.Measure(new Size(width, 1600));
            view.Arrange(new Rect(0, 0, width, 1600));
            view.UpdateLayout();
        }

        private static void Click(ButtonBase button) => button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

        private static string Name(UIElement element) => UIElementAutomationPeer.CreatePeerForElement(element).GetName();

        private static bool Shown(DependencyObject element)
        {
            for (var current = element; current != null; current = System.Windows.Media.VisualTreeHelper.GetParent(current))
                if (current is UIElement ui && ui.Visibility != Visibility.Visible) return false;
            return true;
        }

        private static IEnumerable<T> Descendants<T>(DependencyObject root) where T : DependencyObject
        {
            var count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < count; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
                if (child is T match) yield return match;
                foreach (var nested in Descendants<T>(child)) yield return nested;
            }
        }

        [Fact]
        public void ThePageBuildsWithNoBindingErrorAndItsControlsDoWhatTheySay()
        {
            StaTestRunner.Run(() => WithBindingErrors(() =>
            {
                var rig = new Rig();
                var view = new WarehousePageView(rig.Page);
                Lay(view);
                Assert.Equal("Sin leer", view.ChipText.Text);
                Assert.False(view.PlaceButton.IsEnabled);

                rig.Window.ShowPage(RackCadWindowPage.Almacen);
                rig.AnswerRead();
                Lay(view);
                Assert.Equal("Lectura al día", view.ChipText.Text);
                Assert.Equal("Colocar 3 racks", view.PlaceButton.Content);
                Assert.True(view.PlaceButton.IsEnabled);
                Assert.Equal(RackId, view.SeedCombo.SelectedValue);
                Assert.Equal(Visibility.Visible, view.GridSection.Visibility);
                Assert.Equal(Visibility.Collapsed, view.SiteSection.Visibility);

                // A value typed in its box: the button and the scheme follow at once.
                view.ColsBox.Text = "2";
                Lay(view);
                Assert.Equal("Colocar 7 racks", view.PlaceButton.Content);
                var cells = Descendants<Grid>(view.SchemeStripList).Where(g => g.ToolTip is string).ToList();
                Assert.Equal(8, cells.Count);
                Assert.Equal("A1: semilla, ya está en el dibujo", System.Windows.Automation.AutomationProperties.GetName(cells.Single(c => (string)c.ToolTip == "A1: semilla, ya está en el dibujo")));
                Assert.Equal(cells.Count, cells.Select(c => System.Windows.Automation.AutomationProperties.GetName(c)).Distinct().Count());
                Assert.Equal("Esquema de la rejilla: 4 filas por 2 columnas; A1 es la semilla y hay 7 racks nuevos.", System.Windows.Automation.AutomationProperties.GetName(view.SchemePanel));

                // An error in its field; the tab is marked.
                view.RowsBox.Text = "x";
                Lay(view);
                Assert.Equal(Visibility.Visible, view.GridTabProblems.Visibility);
                Assert.False(view.PlaceButton.IsEnabled);
                Assert.Equal(WarehouseLayoutRules.GridFixText, view.PlaceBlockedText.Text);
                view.RowsBox.Text = "4";

                // The rack chosen stays chosen when the lists are built again (a new reading, a presentation of the host).
                rig.Present();
                Lay(view);
                Assert.Equal(RackId, view.SeedCombo.SelectedValue);
                Assert.Single(rig.Door.Reads);

                // A rack chosen in the list is a gesture: its planta is read.
                view.SeedCombo.SelectedValue = "SEL-2";
                Assert.Equal("SEL-2", rig.LastRead.SeedRackId);
                rig.AnswerRead();

                // The fill: its section, its layer and the rotated try.
                Click(view.FillTabButton);
                Lay(view);
                Assert.True(rig.Page.IsFillTab);
                Assert.Equal(Visibility.Visible, view.SiteSection.Visibility);
                Assert.Equal("RACKCAD_SITIO", view.LayerCombo.SelectedItem);
                view.LayerCombo.SelectedItem = "MUROS";
                Assert.Equal("MUROS", rig.LastRead.Layer);
                rig.AnswerRead();
                Lay(view);
                Assert.Equal("MUROS", view.LayerCombo.SelectedItem);
                Assert.Equal(Visibility.Visible, view.SiteErrorText.Visibility);
                view.LayerCombo.SelectedItem = "RACKCAD_SITIO";
                rig.AnswerRead();
                Lay(view);
                Assert.True(view.PlaceButton.IsEnabled);
                Assert.Contains(Descendants<Grid>(view.SchemeStripList), g => g.ToolTip is string tip && tip.EndsWith(": fuera del contorno", StringComparison.Ordinal));

                // «Colocar» from its button, and the banner of the write with its «Dejar de esperar».
                Click(view.PlaceButton);
                Lay(view);
                Assert.Single(rig.Door.Places);
                Assert.Equal(Visibility.Visible, view.WritingBanner.Visibility);
                Assert.Equal("Dejar de esperar a AutoCAD (Almacén)", Name(view.StopWritingButton));
                Assert.False(view.RowsBox.IsEnabled);
                Click(view.StopWritingButton);
                Lay(view);
                Assert.Equal(Visibility.Collapsed, view.WritingBanner.Visibility);
                Assert.Equal(Visibility.Visible, view.MessageBanner.Visibility);
                Click(view.DismissMessageButton);
                Lay(view);
                Assert.Equal(Visibility.Collapsed, view.MessageBanner.Visibility);

                // «Restablecer valores», and every button of the page with a name of its own.
                Click(view.GridTabButton);
                Click(view.IndependentButton);
                Assert.True(rig.Page.IsIndependent);
                Lay(view);
                Assert.True(view.ResetButton.IsEnabled);
                var names = Descendants<Button>(view).Where(Shown).Select(Name).ToList();
                Assert.True(names.Count >= 8, string.Join(" | ", names));
                Assert.Equal(names.Count, names.Distinct().Count());
                Click(view.ResetButton);
                Assert.Equal(WarehouseLayoutValues.Defaults, rig.Page.Values);
            }));
        }

        [Fact]
        public void TheFitQuestionAndTheStaleNoticeHaveTheirButtons()
        {
            StaTestRunner.Run(() => WithBindingErrors(() =>
            {
                var rig = new Rig();
                rig.Open();
                var view = new WarehousePageView(rig.Page);
                Click(view.FitToggleButton);
                Lay(view);
                Assert.Equal(Visibility.Visible, view.FitPanel.Visibility);
                view.ColsBox.Text = "2";
                view.BuildingXBox.Text = "400";
                view.BuildingYBox.Text = "200";
                Click(view.PlaceButton);
                Lay(view);
                Assert.Equal(Visibility.Visible, view.FitQuestionBanner.Visibility);
                Click(view.PlaceAnywayButton);
                Assert.True(Assert.Single(rig.Door.Places).PlaceAnyway);
                rig.Window.StopWaiting();

                rig.Present(block: WorkspaceActionBlock.CommandActive);
                rig.Present();
                Lay(view);
                Assert.Equal(Visibility.Visible, view.StaleBanner.Visibility);
                Click(view.ReadNowButton);
                Assert.Equal(2, rig.Door.Reads.Count);
            }));
        }

        [Fact]
        public void TheHeaderOfVerificarEncajeHasItsSummaryAtTheRightAndAFieldSavesWhenItLosesTheFocus()
        {
            StaTestRunner.Run(() => WithBindingErrors(() =>
            {
                var rig = new Rig();
                rig.Window.ShowPage(RackCadWindowPage.Almacen);
                rig.AnswerRead(r => Reading(r) with { Layers = new[] { "0", "Rackcad_Sitio" }, Site = Site("Rackcad_Sitio") });
                var view = new WarehousePageView(rig.Page);
                Lay(view);

                // Correction 5: the content stretches; the summary sits at the right edge of the button, the title at the left.
                var button = view.FitToggleButton;
                var summaryRight = view.FitSummaryText.TranslatePoint(new Point(view.FitSummaryText.ActualWidth, 0), button).X;
                var chevronLeft = view.FitChevronText.TranslatePoint(new Point(0, 0), button).X;
                Assert.True(button.ActualWidth > 300, button.ActualWidth.ToString(System.Globalization.CultureInfo.InvariantCulture));
                Assert.True(summaryRight > button.ActualWidth - 20, summaryRight + " of " + button.ActualWidth);
                Assert.True(chevronLeft < 20, chevronLeft.ToString(System.Globalization.CultureInfo.InvariantCulture));
                Assert.Equal("▸", view.FitChevronText.Text);
                Click(button);
                Lay(view);
                Assert.Equal("▾", view.FitChevronText.Text);

                // Correction 6: typing does not save; the box losing the focus does.
                var saves = rig.Settings.Saves;
                view.RowsBox.Text = "5";
                Assert.Equal(saves, rig.Settings.Saves);
                view.RowsBox.RaiseEvent(new System.Windows.Input.KeyboardFocusChangedEventArgs(System.Windows.Input.Keyboard.PrimaryDevice, 0, view.RowsBox, null)
                {
                    RoutedEvent = System.Windows.Input.Keyboard.LostKeyboardFocusEvent,
                });
                Assert.Equal(saves + 1, rig.Settings.Saves);
                Assert.Equal("5", rig.Settings.WarehouseValues!.Rows);

                // Correction 8: the list shows the layer of the drawing chosen, whatever the case of the value.
                Click(view.FillTabButton);
                Lay(view);
                Assert.Equal("Rackcad_Sitio", view.LayerCombo.SelectedItem);
                Assert.Single(rig.Door.Reads);

                // Correction 2: the warning of the current layer has its place in «Sitio».
                Assert.Equal(Visibility.Collapsed, view.CurrentLayerWarningText.Visibility);
            }));
        }

        [Fact]
        public void OnANarrowPageTheResultGoesUnderTheValues()
        {
            StaTestRunner.Run(() => WithBindingErrors(() =>
            {
                var rig = new Rig();
                rig.Open();
                var view = new WarehousePageView(rig.Page);
                Lay(view);
                Assert.False(view.ResultBelow);
                Lay(view, 600);
                Assert.True(view.ResultBelow);
                Assert.Equal((1, 0), (Grid.GetRow(view.ResultBorder), Grid.GetColumn(view.ResultBorder)));
                Lay(view);
                Assert.False(view.ResultBelow);
            }));
        }

        [Fact]
        public void TheWindowHostsThePageAndThePageOfARackOffersLlevarAlAlmacen()
        {
            StaTestRunner.Run(() => WithBindingErrors(() =>
            {
                var rig = new Rig();
                var view = new RackCadWindowView(rig.Window);
                Lay(view);
                Assert.IsType<WarehousePageView>(view.WarehousePage.Content);
                Assert.Same(rig.Page, ((WarehousePageView)view.WarehousePage.Content).ViewModel);
                Assert.Equal(Visibility.Collapsed, view.WarehousePage.Visibility);
                Assert.Equal(Visibility.Collapsed, view.TakeToWarehousePanel.Visibility);

                // The section in the column, between the BOM and the library.
                var sections = Descendants<Button>(view.SectionList).Where(Shown).Select(Name).ToList();
                Assert.Equal(new[] { "Nuevo", "Variables", "BOM y lista de racks", "Almacén", "Biblioteca de diseños", "Configuración" }, sections);

                rig.Window.OpenRack("CAN-1");
                Lay(view);
                Assert.Equal(Visibility.Visible, view.TakeToWarehousePanel.Visibility);
                Assert.False(view.TakeToWarehouseButton.IsEnabled);
                Assert.Equal(Visibility.Visible, view.TakeToWarehouseReason.Visibility);

                rig.Window.OpenRack("PB-1");
                Lay(view);
                Assert.True(view.TakeToWarehouseButton.IsEnabled);
                Assert.Equal("Llevar este rack al almacén", Name(view.TakeToWarehouseButton));
                Click(view.TakeToWarehouseButton);
                Lay(view);
                Assert.True(rig.Window.IsAlmacenPage);
                Assert.Equal(Visibility.Visible, view.WarehousePage.Visibility);
                Assert.Equal("PB-1", Assert.Single(rig.Door.Reads).SeedRackId);
                Assert.DoesNotContain(Descendants<TextBlock>(view.WarehousePage).Select(t => t.Text), t => t.StartsWith("RackCad.", StringComparison.Ordinal)); // F-1
            }));
        }
    }
}
