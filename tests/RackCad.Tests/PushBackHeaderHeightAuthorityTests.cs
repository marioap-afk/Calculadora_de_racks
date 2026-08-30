using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (ronda 6B) — UNA LÍNEA FÍSICA, UNA ALTURA DE CABECERA.
    ///
    /// <para>
    /// El dueño reportó que las alturas de las cabeceras no coinciden entre la lateral y las frontales. Medido: en
    /// un rack COMPUESTO el corte lateral dibujaba el poste de 132" —el mismo que el BOM compra— y el corte frontal
    /// lo dibujaba de 120". La causa es que el frontal de un lado se construye sobre el sistema LOCAL de ese lado,
    /// que es un modelo de trabajo con sus propias alturas resueltas, mientras la pieza que se fabrica pertenece a
    /// la estructura COMPUESTA.
    /// </para>
    /// <para>
    /// Estas pruebas fijan el contrato: la misma línea física responde la misma altura en todas las vistas y en el
    /// BOM, esa altura sale sólo de los frentes ADYACENTES a la línea, y un frente profundo remoto no la toca.
    /// </para>
    /// </summary>
    public class PushBackHeaderHeightAuthorityTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackEditorInputs Inputs()
        {
            var inputs = PushBackEditorInputs.NewDesign();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            foreach (var selection in new PushBackSafetyAuthority(Catalog).Defaults())
            {
                inputs.SafetySelections.Add(selection);
            }

            return inputs;
        }

        private static PushBackSystem SingleSided(int levels, params int[] depths)
        {
            var state = new PushBackEditorState();
            var inputs = state.LoadNew();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            inputs.PalletsDeep = depths[0];
            state.SetFrontCount(depths.Length);
            for (var index = 0; index < depths.Length; index++)
            {
                state.Structure.Fronts[index].LoadLevels = levels;
                state.Structure.Fronts[index].PalletsDeep = depths[index];
                state.AdjustLevels(index, 0);
            }

            var computation = new PushBackEditorDesignAssembler(Catalog).Build(state, inputs);
            Assert.True(computation.IsValid, computation.Error);
            return computation.System;
        }

        private static PushBackSystem Composite(
            PushBackCellTopology topology, PushBackRunDirection direction, int slots,
            int levelsA = 2, int levelsB = 2, int deepA = 5, int deepB = 5,
            bool sideB = true, int? blankSlotA = null, int? blankSlotB = null)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSlotCount(slots);
            state.SetSideBPresent(sideB);
            if (sideB)
            {
                for (var slot = 0; slot < slots; slot++)
                {
                    state.SetSlotPresent(PushBackSide.B, slot, true);
                }
            }

            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                if (side == PushBackSide.B && !sideB) { continue; }
                var levels = side == PushBackSide.A ? levelsA : levelsB;
                var deep = side == PushBackSide.A ? deepA : deepB;
                var editor = state.Of(side);
                for (var index = 0; index < editor.Structure.Count; index++)
                {
                    editor.AdjustLevels(index, levels - editor.Structure.Fronts[index].LoadLevels);
                    editor.Structure.Fronts[index].PalletsDeep = deep;
                }
            }

            state.SetDefaults(topology, direction);
            if (blankSlotA.HasValue) { Assert.True(state.SetSlotPresent(PushBackSide.A, blankSlotA.Value, false)); }
            if (blankSlotB.HasValue) { Assert.True(state.SetSlotPresent(PushBackSide.B, blankSlotB.Value, false)); }

            var computation = new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog);
            Assert.NotNull(computation.System);
            return computation.System;
        }

        // ---- lecturas -----------------------------------------------------------------------------------------

        private static IReadOnlyList<double> PostHeights(IEnumerable<HeaderBlockInstance> instances)
            => instances.Where(instance => instance.Role == HeaderBlockRole.Post)
                .OrderBy(instance => instance.Insertion.X)
                .Select(instance => Math.Round(
                    instance.DynamicParameters.TryGetValue(SelectiveRackDefaults.LengthParam, out var value) ? value : -1.0,
                    3))
                .ToList();

        /// <summary>
        /// La altura que el CORTE LATERAL dibuja en una linea fisica, DENTRO del tramo de profundidad de un lado.
        ///
        /// <para>
        /// I-42 (ronda 6D) SUSTITUYE la version anterior de este ayudante, que exigia una sola altura por corte. Una
        /// linea atraviesa los dos lados y sus demandas son INDEPENDIENTES: el corte dibuja las cabeceras de A a la
        /// altura de A y las de B a la de B. Lo que sigue siendo cierto —y lo que se comprueba— es que la MISMA
        /// pieza fisica mide lo mismo en todas las vistas.
        /// </para>
        /// </summary>
        private static double LateralHeight(PushBackSystem system, int line, PushBackSide side = PushBackSide.A)
        {
            var view = system.IsComposite ? system.Composite?.Of(side) : null;
            var minX = view == null ? double.NegativeInfinity : Math.Min(view.OuterX, view.InnerX);
            var maxX = view == null ? double.PositiveInfinity : Math.Max(view.OuterX, view.InnerX);
            // En la linea INTERIOR los dos lados tienen su propio poste, uno contra otro: se excluye de la ventana
            // para que la lectura de un lado no recoja el del otro.
            var inner = view?.InnerX;
            var heights = new PushBackSystemLateralBuilder().Build(system, Catalog, line).Flatten().Instances
                .Where(instance => instance.Role == HeaderBlockRole.Post)
                .Where(instance => instance.Insertion.X >= minX - 1e-6 && instance.Insertion.X <= maxX + 1e-6)
                .Where(instance => !inner.HasValue || Math.Abs(instance.Insertion.X - inner.Value) > 1e-6)
                .Select(instance => Math.Round(
                    instance.DynamicParameters.TryGetValue(SelectiveRackDefaults.LengthParam, out var value) ? value : -1.0, 3))
                .Distinct().ToList();
            Assert.Single(heights);
            return heights[0];
        }

        /// <summary>
        /// Las alturas que un corte dibuja en una linea, sin distinguir lado: vacio si esa linea no lleva ninguna
        /// cabecera. Un lado EN BLANCO en una ranura no tiene pieza en su linea exterior, y preguntar por la altura
        /// de ese lado ahi no significa nada.
        /// </summary>
        private static IReadOnlyList<double> LateralHeights(PushBackSystem system, int line)
            => new PushBackSystemLateralBuilder().Build(system, Catalog, line).Flatten().Instances
                .Where(instance => instance.Role == HeaderBlockRole.Post)
                .Select(instance => Math.Round(
                    instance.DynamicParameters.TryGetValue(SelectiveRackDefaults.LengthParam, out var value) ? value : -1.0, 3))
                .Distinct().OrderBy(value => value).ToList();

        /// <summary>La altura de un lado en una linea, o null si ese lado no tiene cabecera ahi.</summary>
        private static double? LateralHeightOrNull(PushBackSystem system, int line, PushBackSide side)
        {
            var view = system.IsComposite ? system.Composite?.Of(side) : null;
            var minX = view == null ? double.NegativeInfinity : Math.Min(view.OuterX, view.InnerX);
            var maxX = view == null ? double.PositiveInfinity : Math.Max(view.OuterX, view.InnerX);
            var inner = view?.InnerX;
            var heights = new PushBackSystemLateralBuilder().Build(system, Catalog, line).Flatten().Instances
                .Where(instance => instance.Role == HeaderBlockRole.Post)
                .Where(instance => instance.Insertion.X >= minX - 1e-6 && instance.Insertion.X <= maxX + 1e-6)
                .Where(instance => !inner.HasValue || Math.Abs(instance.Insertion.X - inner.Value) > 1e-6)
                .Select(instance => Math.Round(
                    instance.DynamicParameters.TryGetValue(SelectiveRackDefaults.LengthParam, out var value) ? value : -1.0, 3))
                .Distinct().ToList();
            return heights.Count == 1 ? heights[0] : (double?)null;
        }

        /// <summary>
        /// Las líneas COMPUESTAS que un lado posee, en el orden en que su corte frontal las dibuja: cada ranura
        /// presente aporta la línea de su izquierda y la de su derecha.
        /// </summary>
        private static IReadOnlyList<int> OwnedLines(PushBackSystem system, PushBackSide side)
        {
            var view = system.Composite?.Of(side);
            if (view == null || !view.IsPresent)
            {
                return new List<int>();
            }

            var lines = new SortedSet<int>();
            for (var slot = 0; slot < view.LocalIndexBySlot.Count; slot++)
            {
                if (view.LocalIndexBySlot[slot] < 0) { continue; }
                lines.Add(slot);
                lines.Add(slot + 1);
            }

            return lines.ToList();
        }

        private static IReadOnlyList<double> FrontalHeights(
            PushBackSystem system, PushBackFrontalEnd end, PushBackSide side)
            => PostHeights(new PushBackSystemFrontalBuilder().BuildPlan(system, Catalog, end, side).Flatten().Instances);

        /// <summary>
        /// Los postes que un corte frontal dibuja, con la LINEA FISICA de cada uno. La linea se identifica por la
        /// posicion transversal del poste dentro de la reticula sobre la que ese corte se construye, asi que no se
        /// supone ningun emparejamiento por orden: se lee la que el dibujo usa.
        /// </summary>
        private static IReadOnlyList<(int Line, double Height)> FrontalPostsByLine(
            PushBackSystem system, PushBackFrontalEnd end, PushBackSide side)
        {
            var view = system.IsComposite ? system.Composite?.Of(side) : null;
            var model = system.IsComposite ? view?.Local : system;
            if (model == null || (system.IsComposite && !view.IsPresent))
            {
                return new List<(int, double)>();
            }

            // En el compuesto la ranura fisica y el frente local van 1:1 (un blanco conserva su ranura), asi que la
            // linea local ES la compuesta. Se comprueba en vez de suponerse.
            if (system.IsComposite)
            {
                Assert.Equal(system.Structure.Fronts.Count, model.Structure.Fronts.Count);
            }

            var positions = DynamicFrontGeometry.Compute(model.Structure, Catalog).PostPositions;
            var result = new List<(int, double)>();
            foreach (var instance in new PushBackSystemFrontalBuilder().BuildPlan(system, Catalog, end, side)
                         .Flatten().Instances.Where(i => i.Role == HeaderBlockRole.Post))
            {
                var line = -1;
                for (var index = 0; index < positions.Count; index++)
                {
                    if (Math.Abs(positions[index] - instance.Insertion.X) < 1e-6) { line = index; break; }
                }

                Assert.True(line >= 0, $"poste frontal en X={instance.Insertion.X:0.###} fuera de la reticula");
                result.Add((line, Math.Round(
                    instance.DynamicParameters.TryGetValue(SelectiveRackDefaults.LengthParam, out var value) ? value : -1.0, 3)));
            }

            return result;
        }

        // ---- A) la misma cabecera física mide lo mismo en todas las vistas -------------------------------------

        public static IEnumerable<object[]> Scenarios() => new[]
        {
            new object[] { "un solo sentido 5/8/6/9" },
            new object[] { "un solo sentido 5/5" },
            new object[] { "compuesto solo A" },
            new object[] { "compuesto solo B" },
            new object[] { "compuesto encontradas" },
            new object[] { "compuesto encontradas niveles 3/2" },
            new object[] { "compuesto encontradas fondos 8/4" },
            new object[] { "compuesto corrida A->B" },
            new object[] { "compuesto corrida B->A" },
            new object[] { "compuesto blank A slot0" },
            new object[] { "compuesto blank B slot1" },
            new object[] { "compuesto sin lado B" }
        };

        private static PushBackSystem Scenario(string label)
        {
            switch (label)
            {
                case "un solo sentido 5/8/6/9": return SingleSided(2, 5, 8, 6, 9);
                case "un solo sentido 5/5": return SingleSided(2, 5, 5);
                case "compuesto solo A": return Composite(PushBackCellTopology.SoloA, PushBackRunDirection.AToB, 2);
                case "compuesto solo B": return Composite(PushBackCellTopology.SoloB, PushBackRunDirection.AToB, 2);
                case "compuesto encontradas": return Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 2);
                case "compuesto encontradas niveles 3/2": return Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 2, levelsA: 3);
                case "compuesto encontradas fondos 8/4": return Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 2, deepA: 8, deepB: 4);
                case "compuesto corrida A->B": return Composite(PushBackCellTopology.Corrida, PushBackRunDirection.AToB, 2);
                case "compuesto corrida B->A": return Composite(PushBackCellTopology.Corrida, PushBackRunDirection.BToA, 2);
                case "compuesto blank A slot0": return Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 3, blankSlotA: 0);
                case "compuesto blank B slot1": return Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 3, blankSlotB: 1);
                default: return Composite(PushBackCellTopology.SoloA, PushBackRunDirection.AToB, 2, sideB: false);
            }
        }

        /// <summary>
        /// LA PRUEBA CENTRAL: para cada línea física, la altura del poste que dibuja el corte LATERAL y la que
        /// dibujan los DOS cortes frontales de cada lado son la misma. Antes de 6B un rack compuesto respondía 132"
        /// en el lateral y 120" en la frontal — un pie comercial de diferencia en la misma pieza.
        /// </summary>
        [Theory]
        [MemberData(nameof(Scenarios))]
        public void SamePhysicalHeader_HasSameHeightInAllViews(string label)
        {
            var system = Scenario(label);
            var sides = system.IsComposite
                ? new[] { PushBackSide.A, PushBackSide.B }
                : new[] { PushBackSide.A };

            var checkedPosts = 0;
            foreach (var side in sides)
            {
                foreach (var end in new[] { PushBackFrontalEnd.EntradaSalida, PushBackFrontalEnd.Posterior })
                {
                    foreach (var post in FrontalPostsByLine(system, end, side))
                    {
                        var lateral = LateralHeight(system, post.Line, side);
                        Assert.True(
                            Math.Abs(lateral - post.Height) < 1e-6,
                            $"{label} [{side}/{end}] línea {post.Line}: el lateral dibuja {lateral:0.###}\" "
                                + $"y la frontal {post.Height:0.###}\"");
                        checkedPosts++;
                    }
                }
            }

            Assert.True(checkedPosts > 0, $"{label}: ningun poste frontal comprobado");
        }

        /// <summary>
        /// J) …y esa altura es la que el BOM compra: toda altura dibujada aparece como longitud de cabecera en el
        /// BOM, y ninguna longitud del BOM es ajena al dibujo.
        /// </summary>
        [Theory]
        [MemberData(nameof(Scenarios))]
        public void BomHeaderHeight_EqualsResolvedPhysicalHeaderHeight(string label)
        {
            var system = Scenario(label);
            var sides = system.IsComposite
                ? new[] { PushBackSide.A, PushBackSide.B }
                : new[] { PushBackSide.A };
            var drawn = Enumerable.Range(0, system.Structure.Fronts.Count + 1)
                .Where(line => DynamicFrontActivation.BoundaryExists(system.Structure, line))
                .SelectMany(line => sides
                    .Select(side => LateralHeightOrNull(system, line, side))
                    .Where(height => height.HasValue)
                    .Select(height => height.Value))
                .Distinct()
                .OrderBy(value => value)
                .ToList();
            var bought = PushBackBomBuilder.Build(system, Catalog).Components
                .Where(component => component.Category.IndexOf("abecera", StringComparison.Ordinal) >= 0)
                .Select(component => Math.Round(component.Length, 3))
                .Distinct()
                .OrderBy(value => value)
                .ToList();

            Assert.NotEmpty(drawn);
            Assert.Equal(drawn, bought);
        }

        // ---- B/C) envolvente LOCAL: sólo los frentes adyacentes -------------------------------------------------

        /// <summary>
        /// La altura de una línea sale de los frentes que FÍSICAMENTE la tocan. Medido sobre 5/8/6/9: las tres
        /// primeras líneas valen 120" y sólo las dos que tocan el frente de 9 fondos suben a 132".
        /// </summary>
        [Fact]
        public void HeaderLineHeight_UsesOnlyAdjacentFronts()
        {
            var system = SingleSided(2, 5, 8, 6, 9);
            var heights = Enumerable.Range(0, system.Structure.Fronts.Count + 1)
                .Select(line => LateralHeight(system, line))
                .ToList();

            Assert.Equal(new[] { 120.0, 120.0, 120.0, 132.0, 132.0 }, heights);
        }

        /// <summary>
        /// C) UN FRENTE PROFUNDO REMOTO NO SUBE UNA CABECERA AJENA. Se compara el mismo rack con y sin el frente
        /// de 9 fondos al final: las líneas que no lo tocan miden exactamente lo mismo en los dos.
        /// </summary>
        [Fact]
        public void RemoteDeepFront_DoesNotIncreaseUnrelatedHeader()
        {
            var withRemote = SingleSided(2, 5, 8, 6, 9);
            var without = SingleSided(2, 5, 8, 6);

            // Las lineas 0..2 no tocan al frente remoto en NINGUNO de los dos racks: son las mismas piezas.
            for (var line = 0; line <= 2; line++)
            {
                Assert.Equal(LateralHeight(without, line), LateralHeight(withRemote, line), 6);
            }

            // Y la que SI lo toca sube, que es lo que hace la prueba no vacia: la linea 3 es la exterior del frente
            // de 6 fondos cuando no hay nada detras, y la compartida con el de 9 cuando lo hay.
            Assert.Equal(120.0, LateralHeight(without, 3), 6);
            Assert.Equal(132.0, LateralHeight(withRemote, 3), 6);
        }

        /// <summary>
        /// D) Una demanda alta del lado A no sube una línea en la que A no participa. Con el lado A en blanco en la
        /// ranura 0, su línea exterior conserva la altura que tendría sin ese lado.
        /// </summary>
        [Fact]
        public void SideAHeight_DoesNotLeakToIndependentSideB()
        {
            // El lado A tiene TRES niveles pero esta EN BLANCO en la ranura 0, asi que no carga la linea 0. Su
            // demanda no puede subirla: se compara con el mismo rack cuyo lado A tiene dos niveles.
            var tallA = Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 3, levelsA: 3, blankSlotA: 0);
            var plainA = Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 3, levelsA: 2, blankSlotA: 0);

            // En la linea 0 el lado A no tiene cabecera —su ranura esta en blanco—, asi que lo que se dibuja ahi
            // es del otro lado y no puede moverse por subir A.
            Assert.Null(LateralHeightOrNull(tallA, 0, PushBackSide.A));
            Assert.Equal(LateralHeights(plainA, 0), LateralHeights(tallA, 0));
            // ...y donde A SI carga, su demanda manda: la prueba no es vacia.
            Assert.True(LateralHeight(tallA, 2) > LateralHeight(plainA, 2));
        }

        /// <summary>
        /// E) Una línea la cargan los dos lados, y cada uno resuelve la envolvente de SUS adyacencias reales.
        ///
        /// <para>
        /// I-42 (ronda 6D) SUSTITUYE la version anterior de esta prueba, que exigia un solo valor para toda la
        /// linea. El contrato del dueño es que A y B tienen alturas independientes: subir A no alarga una cabecera
        /// que pertenece solo a B. Lo que se conserva —y se comprueba aqui— es que cada lado recoge su propia
        /// demanda y que sus dos frontales coinciden con SU lateral.
        /// </para>
        /// </summary>
        [Fact]
        public void SharedPhysicalHeader_UsesRequiredLocalEnvelope()
        {
            var system = Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 2, levelsA: 3, levelsB: 2);
            var symmetric = Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 2, levelsA: 2, levelsB: 2);

            // El lado A recoge su demanda; el lado B conserva la suya, la misma que en el rack simetrico.
            Assert.True(
                LateralHeight(system, 1, PushBackSide.A) > LateralHeight(symmetric, 1, PushBackSide.A),
                "el lado A no recogió su propia demanda");
            Assert.Equal(
                LateralHeight(symmetric, 1, PushBackSide.B),
                LateralHeight(system, 1, PushBackSide.B),
                6);

            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                var lateral = LateralHeight(system, 1, side);
                foreach (var end in new[] { PushBackFrontalEnd.EntradaSalida, PushBackFrontalEnd.Posterior })
                {
                    var frontal = FrontalHeights(system, end, side);
                    if (frontal.Count == 0) { continue; }
                    Assert.All(frontal, height => Assert.Equal(lateral, height, 6));
                }
            }
        }

        // ---- H/I) blancos ---------------------------------------------------------------------------------------

        /// <summary>
        /// H) Un blanco conserva su ranura física, así que las líneas que hacen falta para llegar al siguiente
        /// grupo siguen existiendo, con su altura propia.
        /// </summary>
        [Fact]
        public void BlankSlots_PreserveRequiredHeaderLines()
        {
            var system = Composite(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, 3, blankSlotA: 0);

            Assert.Equal(3, system.Structure.Fronts.Count);   // el blanco NO compacta la retícula
            for (var line = 0; line <= 3; line++)
            {
                Assert.True(DynamicFrontActivation.BoundaryExists(system.Structure, line), $"falta la línea {line}");
                // La linea se dibuja: lleva al menos una cabecera, del lado que la carga.
                Assert.NotEmpty(LateralHeights(system, line));
                Assert.All(LateralHeights(system, line), height => Assert.True(height > 0.0));
            }
        }

        /// <summary>
        /// I) Dos blancos consecutivos comparten una frontera que no sirve a ningún frente: ahí no se dibuja
        /// cabecera. Es la regla de I-33, que esta ronda no puede romper.
        /// </summary>
        [Fact]
        public void TwoConsecutiveBlankSlots_DoNotCreateUselessMiddleHeader()
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSlotCount(4);
            state.SetSideBPresent(true);
            for (var slot = 0; slot < 4; slot++)
            {
                state.SetSlotPresent(PushBackSide.B, slot, true);
            }

            state.SetDefaults(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB);
            Assert.True(state.SetSlotPresent(PushBackSide.A, 1, false));
            Assert.True(state.SetSlotPresent(PushBackSide.B, 1, false));
            Assert.True(state.SetSlotPresent(PushBackSide.A, 2, false));
            Assert.True(state.SetSlotPresent(PushBackSide.B, 2, false));

            var system = new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog).System;
            Assert.NotNull(system);

            // La frontera COMPARTIDA por las dos ranuras en blanco (línea 2) no existe; las demás sí.
            Assert.False(DynamicFrontActivation.BoundaryExists(system.Structure, 2));
            Assert.True(DynamicFrontActivation.BoundaryExists(system.Structure, 0));
            Assert.True(DynamicFrontActivation.BoundaryExists(system.Structure, 4));
        }

        // ---- F/G) I-40: overrides y Restore ---------------------------------------------------------------------

        /// <summary>
        /// F) Un override manual de altura sobrevive a la resolución compuesta y lo consumen las dos vistas — que
        /// es la razón por la que la altura tiene que salir de UNA función y no de dos.
        /// </summary>
        [Fact]
        public void HeaderOverride_PreservedAcrossCompositeResolution()
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSlotCount(2);
            state.SetSideBPresent(true);
            state.SetSlotPresent(PushBackSide.B, 0, true);
            state.SetSlotPresent(PushBackSide.B, 1, true);
            state.SetDefaults(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB);

            var inputs = Inputs();
            var baseline = new PushBackCompositeEditorAssembler(Catalog).Build(state, inputs, Catalog).System;
            var derived = LateralHeight(baseline, 1);

            inputs.ManualHeaderHeightOverride = derived + 24.0;
            var overridden = new PushBackCompositeEditorAssembler(Catalog).Build(state, inputs, Catalog).System;

            Assert.Equal(derived + 24.0, LateralHeight(overridden, 1), 6);
            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                var frontal = FrontalHeights(overridden, PushBackFrontalEnd.EntradaSalida, side);
                Assert.NotEmpty(frontal);
                Assert.All(frontal, height => Assert.Equal(derived + 24.0, height, 6));
            }
        }

        /// <summary>
        /// G) Restore borra el override y devuelve la PROPUESTA ACTUAL, no el valor que el override tenía: una
        /// propuesta derivada no es un override efectivo.
        /// </summary>
        [Fact]
        public void RestoreHeader_ReturnsCurrentDerivedProposal()
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSlotCount(2);
            state.SetSideBPresent(true);
            state.SetSlotPresent(PushBackSide.B, 0, true);
            state.SetSlotPresent(PushBackSide.B, 1, true);
            state.SetDefaults(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB);

            var inputs = Inputs();
            var derived = LateralHeight(new PushBackCompositeEditorAssembler(Catalog).Build(state, inputs, Catalog).System, 1);

            inputs.ManualHeaderHeightOverride = derived + 24.0;
            Assert.Equal(derived + 24.0,
                LateralHeight(new PushBackCompositeEditorAssembler(Catalog).Build(state, inputs, Catalog).System, 1), 6);

            inputs.ManualHeaderHeightOverride = null;   // Restore
            Assert.Equal(derived,
                LateralHeight(new PushBackCompositeEditorAssembler(Catalog).Build(state, inputs, Catalog).System, 1), 6);
        }
    }
}
