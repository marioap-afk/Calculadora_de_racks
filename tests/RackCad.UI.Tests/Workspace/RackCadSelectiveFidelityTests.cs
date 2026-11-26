#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using RackCad.Application.Bom;
using RackCad.Application.Systems.Larguero;
using RackCad.Application.Systems.Selective;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Selective;
using RackCad.UI.VentanaRackCad;
using Xunit;
using static RackCad.UI.Tests.SelectiveWindowFixtures;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 A-14, line D, Selectivo, third wave (β): the editor of the window is the draft the Owner approved (the review of fidelity of the
    /// entrega 1, its ten «A»). «Rack» › «Lo esencial» in its three groups writes the WHOLE rack by the same path as «Todo» of the matrix
    /// with «Todos los fondos»; the table «Fondos» writes each fondo; the inspector has «Celda · Frente · Nivel · Todo» and «Destino»; the
    /// automatic and calculated values are beside the manual ones; «Calculado» counts with the BOM; «Altura por poste» and the legend are
    /// under the matrix; the table of the posts has the height. And the accessibility of the matrix and of the repeated controls.
    /// </summary>
    public class RackCadSelectiveFidelityTests
    {
        private static string N(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);

        // ------------------------------------------------------------------ «Rack» › «Lo esencial»

        [Fact]
        public void TheEssentialsAreTheThreeGroupsOfTheDraftAndThePostIsInPostAndAnnotations()
        {
            var vm = SelectiveEditor();

            Assert.Equal(new[] { "Estructura", "Tarima", "Larguero y holguras" }, vm.EssentialGroups.Select(g => g.Title).ToArray());
            string Labels(int group) => string.Join("|", vm.EssentialGroups[group].Fields.Select(f => f is DynamicFieldItem d ? d.Label : ((SelectiveChoiceItem)f).Label));
            Assert.Equal("Frentes (bahías)|Niveles|Fondos|Tarimas por nivel", Labels(0));
            Assert.Equal("Ancho|Alto|Profundidad|Profundidad de cabecera", Labels(1)); // one fondo: the depth of the cabecera stays here
            Assert.Equal("Larguero|Peralte del larguero|Holgura horizontal|Holgura vertical", Labels(2));
            Assert.Equal("Escribe en todo el rack. Para un frente, un nivel o una celda, usa «Matriz».", vm.EssentialsHelp);
            Assert.Equal("1 = sencillo; de 2 a 4, doble profundidad.", Field(vm.Essentials, "fondos").Hint);
            Assert.Equal("A los lados de cada tarima.", Field(vm.Essentials, "tolerance").Hint);
            Assert.Equal("Sobre cada tarima.", Field(vm.Essentials, "clearance").Hint);
            Assert.Equal("Auto: 42 in (la de la tarima − 6 in).", Field(vm.Essentials, "cabecera").Hint);
            // The fronts differ in levels, pallets and widths: «Varía», with an empty box.
            Assert.Equal(("", "Varía"), (Field(vm.Essentials, "rLevels").Text, Field(vm.Essentials, "rLevels").Hint));
            Assert.Equal("Varía", Field(vm.Essentials, "rWidth").Hint);
            Assert.Equal("2", Field(vm.Essentials, "rCount").Text);

            Assert.Equal(new[] { "post", "peralte", "scale", "dims", "dimStyle" },
                vm.AnnotationFields.Select(f => f is DynamicFieldItem d ? d.Key : ((SelectiveChoiceItem)f).Key).ToArray());
            Assert.Equal("Poste 3 in · cotas: Ninguna · sin tarimas", vm.AnnotationSummary);
            vm.DrawPallets = true;
            Assert.EndsWith("· con tarimas", vm.AnnotationSummary);
        }

        [Fact]
        public void AnEssentialWritesTheWholeRackAsTodoOfTheMatrixWithAllTheFondosWhateverTheViewIs()
        {
            // The draft: «Escribe en todo el rack, en todos los fondos»; the same write as «Todo» of the matrix with «Todos los fondos».
            var matrix = SelectiveEditor(TwoFondos());
            matrix.SelectAll();
            Write(Field(matrix.CellFields, "cHeight"), "72");
            Write(Field(matrix.CellFields, "cCount"), "3");
            Write(Field(matrix.FrontFields, "fLevels"), "4");

            var rack = SelectiveEditor(TwoFondos());
            rack.FondoScopeIndex = 2; // the view of the matrix on fondo 2 does not reduce «Lo esencial»
            Write(Field(rack.Essentials, "rHeight"), "72");
            Assert.Equal(string.Empty, rack.Message); // every cell that exists was written: nothing «omitted» to say
            Write(Field(rack.Essentials, "rCount"), "3");
            Write(Field(rack.Essentials, "rLevels"), "4");

            Assert.Equal(Json(matrix.Draft.Design), Json(rack.Draft.Design));
            Assert.Equal(2, rack.FondoScopeIndex);
            Assert.Equal("72", Field(rack.Essentials, "rHeight").Text);
            Assert.Equal("4", Field(rack.Essentials, "rLevels").Text);
            Assert.Equal(new[] { 4, 4, 4 }, new[] { rack.Draft.LevelCount(0, 0), rack.Draft.LevelCount(1, 0), rack.Draft.LevelCount(1, 2) });

            // The larguero and its peralte of the whole rack: the same as the matrix too.
            matrix.SelectAll();
            Choice(matrix.CellFields, "cPeralte").SelectedValue = "5";
            Choice(rack.Essentials, "rPeralte").SelectedValue = "5";
            Assert.Equal(Json(matrix.Draft.Design), Json(rack.Draft.Design));
            Assert.All(new[] { rack.Draft.Cell(0, 0, 0), rack.Draft.Cell(1, 2, 3) }, c => Assert.Equal(5.0, c!.BeamPeralte));

            // A refused value writes nothing and stays in its field.
            var before = Json(rack.Draft.Design);
            Write(Field(rack.Essentials, "rWidth"), "0");
            Assert.Equal("El ancho de tarima debe ser un número mayor que cero.", Field(rack.Essentials, "rWidth").Error);
            Assert.Equal(before, Json(rack.Draft.Design));
        }

        [Fact]
        public void TheTableOfTheFondosWritesEachFondoAndSaysItsState()
        {
            var vm = SelectiveEditor(TwoFondos());
            Assert.Equal(2, vm.FondoRows.Count);
            Assert.Equal("Escribe en todo el rack, en todos los fondos. Para un frente, un nivel o una celda, usa «Matriz».", vm.EssentialsHelp);
            Assert.DoesNotContain(vm.Essentials, f => f is DynamicFieldItem d && d.Key == "cabecera"); // it goes by fondo, in the table
            var (first, last) = (vm.FondoRows[0], vm.FondoRows[1]);
            Assert.Equal(("Fondo 1", false), (first.Title, first.IsLast));
            Assert.True(last.IsLast);
            Assert.Null(last.Separator);
            Assert.Equal("Fondo 2: profundidad de tarima", last.Depth.Label); // the name of the box says the row
            Assert.Equal(("48", "40"), (first.Depth.Text, last.Depth.Text));
            Assert.Equal("Auto: 42", first.Cabecera.Hint);
            Assert.Equal("Auto: 34", last.Cabecera.Hint);

            Write(last.Fronts, "4");
            Assert.Equal((2, 4), (vm.Draft.FrontCount(0), vm.Draft.FrontCount(1)));
            Write(first.Cabecera, "40");
            Assert.Equal((40.0, (double?)null), (vm.Draft.Fondo(0)!.CabeceraDepthOverride, vm.Draft.Fondo(1)!.CabeceraDepthOverride));
            Assert.Equal(string.Empty, vm.FondoRows[0].Cabecera.Hint);
            Write(vm.FondoRows[0].Cabecera, string.Empty);
            Assert.Null(vm.Draft.Fondo(0)!.CabeceraDepthOverride);

            // A refused value: the state of its row says it, and nothing is written.
            var before = Json(vm.Draft.Design);
            Write(vm.FondoRows[1].Depth, "-3");
            Assert.True(vm.FondoRows[1].HasProblem);
            Assert.Equal("1 problema: La profundidad de tarima debe ser un número mayor que cero.", vm.FondoRows[1].State);
            Assert.Equal("Correcto", vm.FondoRows[0].State);
            Assert.Equal(before, Json(vm.Draft.Design));
            Write(vm.FondoRows[1].Depth, "40");
            Assert.Equal("Correcto", vm.FondoRows[1].State);

            // One fondo: no table.
            Write(Field(vm.Essentials, "fondos"), "1");
            Assert.Empty(vm.FondoRows);
            Assert.Empty(vm.SeparatorFields);
            Assert.Contains(vm.Essentials, f => f is DynamicFieldItem d && d.Key == "cabecera");
        }

        // ------------------------------------------------------------------ «Calculado»

        [Fact]
        public void TheCalculatedValuesAreThoseOfTheDraftAndTheCountsAreTheBom()
        {
            var vm = SelectiveEditor();
            var system = vm.Draft.System!;
            string Value(string label) => vm.Calculated.Single(c => c.Label == label).Value;

            // F1: 40 × 2 + 4 × 3 = 92; F2: 44 × 2 + 4 × 3 = 100 (the resolver).
            Assert.Equal("92–100 in", Value("Largo del larguero"));
            Assert.Equal("≈ " + N(system.Height) + " in", Value("Altura de cabecera"));
            // F1 has «Larguero a piso»: N2 → N3 is levels[2] − levels[1]; F2 has two levels: nothing to add.
            Assert.Equal(N(system.Bays[0].Levels[2].Y - system.Bays[0].Levels[1].Y) + " in", Value("Separación N2 → N3"));
            Assert.Equal("42 in", Value("Profundidad de cabecera"));

            // The counts are those of the BOM, never counted again: 3 cabeceras, 8 largueros (F1: 3 levels with larguero, F2: 1; two each).
            var bom = SelectiveBomBuilder.Build(system, vm.Draft.Catalog).Components;
            int Sum(string category) => bom.Where(c => c.Category == category).Sum(c => c.Quantity);
            Assert.Equal(Sum(BomBuilder.Cabecera).ToString(CultureInfo.InvariantCulture), Value("Cabeceras"));
            Assert.Equal(Sum(LargueroBomBuilder.Larguero).ToString(CultureInfo.InvariantCulture), Value("Largueros"));
            Assert.Equal(("3", "8"), (Value("Cabeceras"), Value("Largueros")));
            Assert.Equal("10", Value("Posiciones de tarima")); // 5 cells × 2 pallets
            Assert.DoesNotContain(vm.Calculated, c => c.Label == "Separadores"); // one fondo
            Assert.Equal("Una por poste.", vm.Calculated.Single(c => c.Label == "Cabeceras").Hint);

            // Two fondos: the separators of the BOM too.
            var two = SelectiveEditor(TwoFondos());
            var twoBom = SelectiveBomBuilder.Build(two.Draft.System!, two.Draft.Catalog).Components;
            var separadores = twoBom.Where(c => c.Category == SelectiveBomBuilder.Separador).Sum(c => c.Quantity);
            Assert.True(separadores > 0);
            Assert.Equal(separadores.ToString(CultureInfo.InvariantCulture), two.Calculated.Single(c => c.Label == "Separadores").Value);
            Assert.Equal(new SelectiveBomCounts(twoBom.Where(c => c.Category == BomBuilder.Cabecera).Sum(c => c.Quantity),
                twoBom.Where(c => c.Category == LargueroBomBuilder.Larguero).Sum(c => c.Quantity), separadores), SelectiveBomCounts.Of(two.Draft.System!, two.Draft.Catalog));
            Assert.Equal("Tarima − 6 in, en el fondo 1.", two.Calculated.Single(c => c.Label == "Profundidad de cabecera").Hint);
        }

        [Fact]
        public void TheCalculatedValuesFollowEveryRecomposition()
        {
            var vm = SelectiveEditor();
            var first = vm.Calculated;
            Assert.Same(first, vm.Calculated); // read once per recomposition
            vm.SelectFront(1);
            Write(Field(vm.FrontFields, "fLevels"), "3");
            Assert.NotSame(first, vm.Calculated);
            Assert.Equal("12", vm.Calculated.Single(c => c.Label == "Posiciones de tarima").Value); // 6 cells × 2 pallets
        }

        // ------------------------------------------------------------------ the inspector: scope, «Destino», groups

        [Fact]
        public void TheScopeOfTheInspectorIsCellFrontLevelOrAllWithItsDestination()
        {
            var vm = SelectiveEditor();
            Assert.Equal(SelectiveEditScope.Cell, vm.Scope);
            Assert.Equal("Frente 1 · Nivel 1", vm.InspectorTitle);
            Assert.Equal("Destino: 1 celda · frente 1.", vm.TargetText);
            Assert.Equal(new[] { "Celda", "Frente", "Nivel", "Todo" }, vm.ScopeOptions.Select(o => o.Label).ToArray());
            Assert.Equal(new[] { true, false, false, false }, vm.ScopeOptions.Select(o => o.IsOn).ToArray());

            vm.SetScope(SelectiveEditScope.Front);
            Assert.Equal(new[] { P(0, 0), P(0, 1), P(0, 2) }, vm.SelectedPositions);
            Assert.Equal(("Frente 1", "Destino: 3 celdas · frente 1."), (vm.InspectorTitle, vm.TargetText));
            Assert.True(vm.ScopeOptions[1].IsOn);

            vm.ClickCell(1, 0, control: false, shift: false);
            vm.SetScope(SelectiveEditScope.Level);
            Assert.Equal(new[] { P(0, 0), P(1, 0) }, vm.SelectedPositions);
            Assert.Equal(("Nivel 1", "Destino: 2 celdas · frentes 1, 2."), (vm.InspectorTitle, vm.TargetText));

            vm.SetScope(SelectiveEditScope.All);
            Assert.Equal(5, vm.SelectedPositions.Count);
            Assert.Equal("Todo el rack", vm.InspectorTitle);

            // Several cells by Ctrl+clic: «Celdas (n)»; «Celda» keeps the main one.
            vm.ClickCell(0, 1, control: false, shift: false);
            vm.ClickCell(1, 1, control: true, shift: false);
            Assert.Equal(("2 celdas", "Celdas (2)"), (vm.InspectorTitle, vm.ScopeOptions[0].Label));
            vm.SetScope(SelectiveEditScope.Cell);
            Assert.Equal(new[] { P(1, 1) }, vm.SelectedPositions);
            Assert.Equal("Celda", vm.ScopeOptions[0].Label);

            // The buttons of the matrix set the scope too.
            vm.SelectFront(0);
            Assert.Equal(SelectiveEditScope.Front, vm.Scope);
            vm.SelectLevel(1);
            Assert.Equal((SelectiveEditScope.Level, "Nivel 2"), (vm.Scope, vm.InspectorTitle));
            vm.SelectAll();
            Assert.Equal(SelectiveEditScope.All, vm.Scope);
        }

        [Fact]
        public void TheDestinationSaysTheFondosItReachesAndTodoOfOneFondoSaysSo()
        {
            var vm = SelectiveEditor(TwoFondos());
            Assert.Equal("Destino: 2 celdas en los fondos 1 y 2 · frente 1.", vm.TargetText);
            vm.ClickCell(2, 0, control: false, shift: false);
            Assert.Equal("Destino: 1 celda en el fondo 2 · frente 3.", vm.TargetText);
            vm.SelectFront(1);
            Assert.Equal("Frente 2 · fondos 1 y 2", vm.FrontTitle);
            Assert.Equal("niveles: varía", vm.FrontSummary);

            vm.FondoScopeIndex = 2;
            vm.SetScope(SelectiveEditScope.All);
            Assert.Equal("Todo el fondo 2", vm.InspectorTitle);
            Assert.Equal("Destino: 9 celdas en el fondo 2 · frentes 1, 2, 3.", vm.TargetText);
            Assert.Equal(new[] { false, false, true }, vm.FondoViews.Select(o => o.IsOn).ToArray());
            Assert.Equal("Fondos de la vista: Fondo 2", vm.FondoViews[2].AccessibleText);
        }

        [Fact]
        public void TheInspectorIsInTheGroupsOfTheDraftWithTheirSummaries()
        {
            var vm = SelectiveEditor();
            Assert.Equal(new[] { "cFront", "cHeight", "cCount" }, vm.PalletFields.Select(f => ((DynamicFieldItem)f).Key).ToArray());
            Assert.Equal(new[] { "cBeam", "cPeralte" }, vm.BeamFields.Select(f => ((SelectiveChoiceItem)f).Key).ToArray());
            Assert.Equal(new[] { "cLength", "cClear" }, vm.ManualFields.Select(f => ((DynamicFieldItem)f).Key).ToArray());
            Assert.Same(vm.CellFields[3], vm.BeamFields[0]); // the same items
            Assert.Equal(("Peralte 4 in", "Automático"), (vm.BeamSummary, vm.ManualSummary));
            Assert.Equal(("Frente 1", "3 niveles · larguero a piso"), (vm.FrontTitle, vm.FrontSummary));
            Assert.Equal(new[] { "Largo de larguero", "Claro debajo" }, vm.ManualFields.Select(f => ((DynamicFieldItem)f).Label).ToArray());
            Assert.Equal("Altura manual de cabecera", Field(vm.FrontFields, "fHeight").Label);
            Assert.Equal("Cantidad exacta; N1 es el piso.", Field(vm.FrontFields, "fLevels").Hint);

            Write(Field(vm.CellFields, "cLength"), "120");
            Assert.Equal("1 celda con valor manual", vm.ManualSummary);
            vm.ClickCell(1, 0, control: false, shift: false);
            Assert.Equal("Sin larguero (piso)", vm.BeamSummary);
        }

        // ------------------------------------------------------------------ the automatic values beside the manual ones

        [Fact]
        public void TheAutomaticLengthAndTheCalculatedHeightAreBesideTheManualOnes()
        {
            var vm = SelectiveEditor();
            var system = vm.Draft.System!;
            var width = system.Bays[0].BeamLength;
            var height = system.Bays[0].Height;

            Assert.Equal("Automático: " + N(width) + " in. El mayor del frente fija su ancho.", Field(vm.CellFields, "cLength").Hint);
            Assert.Equal("Calculada: ≈ " + N(height) + " in. Manda en los dos postes del frente.", Field(vm.FrontFields, "fHeight").Hint);

            // Two fronts of different widths: no single number.
            vm.SelectLevel(1);
            Assert.Equal("Automático. El mayor del frente fija su ancho.", Field(vm.CellFields, "cLength").Hint);
            Assert.Equal("Calculada. Manda en los dos postes del frente.", Field(vm.FrontFields, "fHeight").Hint);

            vm.ClickCell(0, 0, control: false, shift: false);
            Write(Field(vm.CellFields, "cLength"), "120");
            Assert.Equal(120.0, vm.Draft.System!.Bays[0].BeamLength);
            Assert.Equal("Vacío = automático (" + N(width) + " in). El mayor del frente fija su ancho.", Field(vm.CellFields, "cLength").Hint);
            Write(Field(vm.FrontFields, "fHeight"), "200");
            Assert.Equal(200.0, vm.Draft.System!.Bays[0].Height);
            Assert.Equal("Vacía = calculada (≈ " + N(height) + " in). Manda en los dos postes del frente.", Field(vm.FrontFields, "fHeight").Hint);
        }

        // ------------------------------------------------------------------ under the matrix: «Altura por poste» and the legend

        [Fact]
        public void TheHeightOfEveryPostIsUnderTheMatrixByFondoOfTheView()
        {
            var vm = SelectiveEditor();
            var system = vm.Draft.System!;
            var block = Assert.Single(vm.PostHeights);
            Assert.False(block.HasTitle);
            Assert.Equal(new[] { "P1", "P2", "P3" }, block.Chips.Select(c => c.Id).ToArray());
            for (var post = 0; post < 3; post++)
            {
                var h = SelectivePostGeometry.PostHeight(system.Bays, post, SelectivePostGeometry.FallbackHeight(system.Bays, system.Height));
                Assert.Equal("≈ " + N(h) + " in", block.Chips[post].Value);
            }

            Assert.Equal("Poste 1: " + block.Chips[0].Value, block.Chips[0].AccessibleText);
            Assert.DoesNotContain(block.Chips, c => c.IsManual);

            // A manual height of F2 governs its two posts.
            vm.SelectFront(1);
            Write(Field(vm.FrontFields, "fHeight"), "300");
            Assert.Equal(new[] { false, true, true }, vm.PostHeights[0].Chips.Select(c => c.IsManual).ToArray());
            Assert.Equal("≈ 300 in", vm.PostHeights[0].Chips[2].Value);
            Assert.EndsWith(", con una altura manual", vm.PostHeights[0].Chips[2].AccessibleText);

            // Two fondos: one block per fondo of the view.
            var two = SelectiveEditor(TwoFondos());
            Assert.Equal(new[] { "Fondo 1", "Fondo 2" }, two.PostHeights.Select(b => b.Title).ToArray());
            Assert.Equal(new[] { 3, 4 }, two.PostHeights.Select(b => b.Chips.Count).ToArray());
            Assert.StartsWith("Poste 1 del fondo 2:", two.PostHeights[1].Chips[0].AccessibleText);
            two.FondoScopeIndex = 2;
            Assert.Equal("Fondo 2", Assert.Single(two.PostHeights).Title);
        }

        [Fact]
        public void TheCellsCarryTheMarksOfTheLegendModifiedDistinctAndManual()
        {
            var vm = SelectiveEditor();
            SelectiveCellItem Cell(int front, int level) => vm.Rows.SelectMany(r => r.Cells).Single(c => c.FrontIndex == front && c.LevelIndex == level);

            Assert.DoesNotContain(vm.Rows.SelectMany(r => r.Cells), c => c.IsModified);
            // F1 has three cells of 40 × 60; F2 two of 44 × 55: those are distinct from the rest.
            Assert.False(Cell(0, 0).IsDistinct);
            Assert.True(Cell(1, 0).IsDistinct);

            Write(Field(vm.CellFields, "cHeight"), "72");
            Assert.True(Cell(0, 0).IsModified);
            Assert.False(Cell(0, 0).IsDistinct); // modified says modified
            Assert.False(Cell(0, 1).IsModified);
            Write(Field(vm.CellFields, "cLength"), "120");
            Assert.True(Cell(0, 0).IsManual);
            Assert.Contains("con un valor manual", Cell(0, 0).AccessibleText);

            // The floor larguero of the front is a change of its cells.
            vm.SelectFront(1);
            vm.SelectionFloorBeam = true;
            Assert.True(Cell(1, 0).IsModified);
            Assert.True(Cell(1, 1).IsModified);

            // Written: what was written is the baseline of the marks.
            vm.MarkWritten("Escrito.", Json(vm.Draft.Design), vm.Name);
            Assert.DoesNotContain(vm.Rows.SelectMany(r => r.Cells), c => c.IsModified);
            Write(Field(vm.FrontFields, "fLevels"), "3"); // a new cell is a change
            Assert.True(Cell(1, 2).IsModified);
            vm.Discard();
            Assert.DoesNotContain(vm.Rows.SelectMany(r => r.Cells), c => c.IsModified);
        }

        // ------------------------------------------------------------------ «Avanzado»

        [Fact]
        public void TheTableOfThePostsHasTheirHeightAndItsSummary()
        {
            var vm = SelectiveEditor();
            Assert.Equal("Altura", vm.PostHeightHeader);
            Assert.Equal("3 postes · 1 peralte propio · 0 cabeceras personalizadas", vm.PostsSummary);
            Assert.Equal(vm.PostHeights[0].Chips.Select(c => c.Value).ToArray(), vm.PostRows.Select(r => r.Height).ToArray());
            Assert.Equal(new[] { "P1: peralte propio", "P2: peralte propio", "P3: peralte propio" }, vm.PostRows.Select(r => r.Field.Label).ToArray());
            Assert.Equal(("", "Del rack: 3"), (vm.PostRows[0].Field.Hint, vm.PostRows[1].Field.Hint));
            Assert.Equal("Ningún frente está partido.", vm.HalfFrontsText);

            var two = SelectiveEditor(TwoFondos());
            Assert.Equal("Altura (por fondo)", two.PostHeightHeader);
            Assert.Contains(" / ", two.PostRows[0].Height);
            Assert.DoesNotContain(" / ", two.PostRows[3].Height); // P4 is only in fondo 2
        }

        // ------------------------------------------------------------------ accessibility (sprint\a11y.md)

        [Fact]
        public void EveryCellAndEveryRepeatedControlHasAUniqueNameWithItsPosition()
        {
            var vm = SelectiveEditor();
            var cells = vm.Rows.SelectMany(r => r.Cells).ToList();
            Assert.Equal(cells.Count, cells.Select(c => c.AccessibleText).Distinct().Count());
            var f2n2 = cells.Single(c => c.FrontIndex == 1 && c.LevelIndex == 1);
            Assert.Equal("Frente 2 · Nivel 2: 44×55; distinta del resto", f2n2.AccessibleText);
            Assert.StartsWith("Frente 2 · Nivel 1: 44×55; en el piso, sin larguero", cells.Single(c => c.FrontIndex == 1 && c.LevelIndex == 0).AccessibleText);
            Assert.Equal("Frente 2 · Nivel 3: no existe en los fondos de la vista", cells.Single(c => c.FrontIndex == 1 && c.LevelIndex == 2).AccessibleText);
            Assert.Equal(new[] { "Nivel 3: seleccionar la fila", "Nivel 2: seleccionar la fila", "Nivel 1, el piso: seleccionar la fila" },
                vm.Rows.Select(r => r.AccessibleText).ToArray());

            vm.SelectFront(0);
            vm.StartSegments();
            vm.AddSegment();
            var rows = vm.SegmentRows;
            Assert.Equal(new[] { "Tramo 1: longitud", "Tramo 2: longitud", "Tramo 3: longitud" }, rows.Select(r => r.LengthName).ToArray());
            Assert.Equal(rows.Count, rows.Select(r => r.LoadedName).Distinct().Count());
            Assert.Equal(rows.Count, rows.Select(r => r.RemoveName).Distinct().Count());
            Assert.Equal("Quitar el tramo 2", rows[1].RemoveName);
            Assert.Equal(vm.PostRows.Count, vm.PostRows.Select(r => r.Field.Label).Distinct().Count());
        }

        // ------------------------------------------------------------------ the view

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

        private static void Lay(FrameworkElement view, double width = 1200, double height = 900)
        {
            view.Measure(new Size(width, height));
            view.Arrange(new Rect(0, 0, width, height));
            view.UpdateLayout();
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

        private static void Click(Button button) => button.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));

        private static string Name(UIElement element) => UIElementAutomationPeer.CreatePeerForElement(element).GetName();

        [Fact]
        public void TheViewHasTheRackOfTheDraftTheScopeButtonsTheLegendAndUniqueNamesWithNoBindingError()
        {
            StaTestRunner.Run(() =>
            {
                var errors = new BindingErrors();
                PresentationTraceSources.Refresh();
                var source = PresentationTraceSources.DataBindingSource;
                var level = source.Switch.Level;
                source.Listeners.Add(errors);
                source.Switch.Level = SourceLevels.Error;
                try
                {
                    var vm = SelectiveEditor(TwoFondos());
                    var view = new SelectiveEditorView(vm);
                    Lay(view);

                    // Matriz: the segmented fondos, the scope of the inspector, the legend and the heights of the posts.
                    var fondoButtons = Descendants<Button>(view.FondoViewList).ToList();
                    Click(fondoButtons[2]);
                    Assert.Equal(2, vm.FondoScopeIndex);
                    Lay(view);
                    Click(Descendants<Button>(view.FondoViewList).First());
                    Assert.Equal(0, vm.FondoScopeIndex);
                    Lay(view);
                    var scopeButtons = Descendants<Button>(view.ScopeList).ToList();
                    Assert.Equal(new[] { "Alcance de la edición: Celda", "Alcance de la edición: Frente", "Alcance de la edición: Nivel", "Alcance de la edición: Todo" },
                        scopeButtons.Select(Name).ToArray());
                    Click(scopeButtons[3]);
                    Assert.Equal(SelectiveEditScope.All, vm.Scope);
                    Lay(view);
                    Assert.Equal("Todo el rack", view.InspectorTitle.Text);
                    Assert.StartsWith("Destino: ", view.TargetText.Text);
                    Assert.Contains(Descendants<TextBlock>(view.MatrixLegend), t => t.Text == "Modificada en el borrador");
                    Assert.Contains(Descendants<TextBlock>(view.MatrixLegend), t => t.Text == "Los fondos no coinciden");
                    var chips = Descendants<TextBlock>(view.PostHeightList).Select(t => System.Windows.Automation.AutomationProperties.GetName(t))
                        .Where(n => n.StartsWith("Poste ", StringComparison.Ordinal)).ToList();
                    Assert.Equal(7, chips.Count); // 3 posts of fondo 1 and 4 of fondo 2
                    Assert.Contains("Poste 4 del fondo 2: ", chips.Select(n => n.Substring(0, Math.Min(n.Length, 21))));

                    // The names of the cells carry their position and are unique; the floor larguero box is named as it reads.
                    var cellNames = Descendants<Button>(view.MatrixRows).Where(b => b.DataContext is SelectiveCellItem).Select(Name).ToList();
                    Assert.Equal(9, cellNames.Count);
                    Assert.Equal(cellNames.Count, cellNames.Distinct().Count());
                    Assert.Contains("Frente 3 · Nivel 1: 42×60", cellNames.Select(n => n.Split(';')[0]));
                    Assert.Equal(vm.SelectionFloorBeamLabel, Name(view.FloorBeamCheck));
                    Assert.Equal(view.FloorBeamCheck.Content, Name(view.FloorBeamCheck));

                    // Rack: the three groups, the table «Fondos» with its boxes named by row, and the hints of «Calculado».
                    view.Tabs.SelectedItem = view.RackTab;
                    view.PostExpander.IsExpanded = true;
                    Lay(view);
                    Assert.Contains(Descendants<TextBlock>(view.EssentialsList), t => t.Text == "Larguero y holguras");
                    var fondoBoxes = Descendants<TextBox>(view.FondoRowList).Select(Name).ToList();
                    Assert.Contains("Fondo 1: separación al fondo siguiente", fondoBoxes);
                    Assert.Contains("Fondo 2: profundidad de cabecera", fondoBoxes);
                    Assert.DoesNotContain("Fondo 2: separación al fondo siguiente", fondoBoxes);
                    Assert.Contains(Descendants<TextBlock>(view.FondoRowList), t => t.Text == "Último");
                    Assert.Contains(Descendants<TextBlock>(view.CalculatedList), t => t.Text == "Una por poste." || t.Text.StartsWith("En los 2 fondos", StringComparison.Ordinal));
                    Assert.Contains(Descendants<ContentPresenter>(view.AnnotationList), c => c.Content is SelectiveChoiceItem choice && choice.Key == "post");

                    // Avanzado: the column of the heights, and the boxes named by post.
                    view.Tabs.SelectedItem = view.AdvancedTab;
                    Lay(view);
                    Assert.Equal("Altura (por fondo)", view.PostHeightHeaderText.Text);
                    Assert.Equal(new[] { "P1: peralte propio", "P2: peralte propio", "P3: peralte propio", "P4: peralte propio" },
                        Descendants<TextBox>(view.PostRowList).Select(Name).ToArray());
                    view.Tabs.SelectedItem = view.MatrixTab;
                    Lay(view);

                    // The tramos of a front: their boxes, boxes and buttons named by tramo.
                    vm.SelectFront(0);
                    vm.StartSegments();
                    Lay(view);
                    var tramoNames = Descendants<FrameworkElement>(view.SegmentList).OfType<UIElement>()
                        .Where(e => e is TextBox || e is CheckBox || e is Button).Select(Name).ToList();
                    Assert.Contains("Tramo 1: longitud", tramoNames);
                    Assert.Contains("Tramo 2 con largueros", tramoNames);
                    Assert.Contains("Quitar el tramo 1", tramoNames);
                    Assert.Equal(tramoNames.Count, tramoNames.Distinct().Count());
                }
                finally
                {
                    source.Listeners.Remove(errors);
                    source.Switch.Level = level;
                }

                Assert.Empty(errors.Messages);
            });
        }
    }
}
