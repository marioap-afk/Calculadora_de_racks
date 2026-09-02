using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 — DECISION FISICA DEL DUEÑO: los dos lados comparten la retícula TRANSVERSAL (lineas de postes, ancho,
    /// BFR) pero NO la altura.
    ///
    /// <para>
    /// Una cabecera es una pieza LONGITUDINAL y pertenece a un lado. Con 4 niveles en A y 2 en B, las cabeceras de A
    /// miden lo que A pide y las de B lo que pide B, y las dos lineas de la interfaz —terminal de A e inicial de B—
    /// pueden tener alturas distintas. La ronda anterior resolvia los dos lados con <c>max(alturaA, alturaB)</c>:
    /// subir un nivel en A estiraba los postes de B.
    /// </para>
    /// </summary>
    public class PushBackCompositeHeightTests
    {
        private const int Fronts = 4;

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackEditorInputs Inputs()
        {
            var inputs = PushBackEditorInputs.NewDesign();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            return inputs;
        }

        private static PushBackCompositeEditorState State(int levelsA, int levelsB)
        {
            var state = new PushBackCompositeEditorState();
            // Como la VENTANA: los dos lados nacen con los defaults de un Push Back nuevo (PB-012: primer nivel a 4",
            // no los 6" del dinamico compartido). Saltarselo en el lado A daba una diferencia de altura de fixture
            // que no existe en el producto.
            state.SideA.LoadNew();
            state.SetSideBPresent(true);
            state.SideB.LoadNew();
            state.SetSlotCount(Fronts);
            // I-42: declarar la CAPACIDAD del lado B ya no lo declara PRESENTE en ningun frente.
            // Este fixture quiere el rack compuesto ENTERO, asi que lo declara frente a frente.
            for (var declared = 0; declared < state.SlotCount; declared++)
            {
                state.SetSlotPresent(PushBackSide.B, declared, true);
            }


            SetLevels(state, PushBackSide.A, levelsA);
            SetLevels(state, PushBackSide.B, levelsB);
            return state;
        }

        private static void SetLevels(PushBackCompositeEditorState state, PushBackSide side, int levels)
        {
            var matrix = state.Of(side).Structure;
            for (var front = 0; front < matrix.Count; front++)
            {
                state.Of(side).AdjustLevels(front, levels - matrix.Fronts[front].LoadLevels);
            }
        }

        private static PushBackSystem Resolve(PushBackCompositeEditorState state)
            => new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog).System;

        /// <summary>La altura de las cabeceras de un lado, leida de los modulos que le pertenecen por identidad.</summary>
        private static IReadOnlyList<double> HeaderHeights(PushBackSystem system, PushBackSide side)
        {
            var prefix = PushBackCompositeStructure.SideBModulePrefix;
            return system.Structure.Modules
                .Where(module => module.IsHeader && module.AssociatedFrameConfiguration != null)
                .Where(module => (module.ModuleId ?? string.Empty).StartsWith(prefix, StringComparison.Ordinal)
                                 == (side == PushBackSide.B))
                .Select(module => Math.Round(module.AssociatedFrameConfiguration.Height, 4))
                .ToList();
        }

        // ================= U: alturas independientes ============================================================

        [Fact]
        public void EachSideKeepsItsOwnHeight_WhenTheLevelsDiffer()
        {
            var system = Resolve(State(levelsA: 4, levelsB: 2));

            var heightsA = HeaderHeights(system, PushBackSide.A);
            var heightsB = HeaderHeights(system, PushBackSide.B);

            Assert.NotEmpty(heightsA);
            Assert.NotEmpty(heightsB);

            // Cada lado es COHERENTE consigo mismo...
            Assert.Single(heightsA.Distinct());
            Assert.Single(heightsB.Distinct());

            // ...y B es MAS BAJO que A, porque pide menos niveles. No se estira hasta la altura de A.
            Assert.True(
                heightsB[0] < heightsA[0] - 1.0,
                "el lado B (2 niveles) no puede medir lo mismo que el A (4): " + heightsB[0] + " vs " + heightsA[0]);
        }

        /// <summary>
        /// PRUEBA VINCULANTE: subir los niveles de A no puede mover NI UNA pieza de B. Se comparan las alturas de las
        /// cabeceras de B antes y despues, una por una y por identidad.
        /// </summary>
        [Fact]
        public void RaisingTheLevelsOfA_LeavesEveryPieceOfB_Untouched()
        {
            var before = Resolve(State(levelsA: 4, levelsB: 2));
            var beforeB = HeaderHeights(before, PushBackSide.B);
            var beforeIds = ModuleIds(before, PushBackSide.B);

            var raised = State(levelsA: 5, levelsB: 2);
            var after = Resolve(raised);
            var afterB = HeaderHeights(after, PushBackSide.B);

            // El lado A SI crece.
            Assert.True(HeaderHeights(after, PushBackSide.A)[0] > HeaderHeights(before, PushBackSide.A)[0] + 1.0);

            // Y el lado B no se mueve: ni su altura, ni su identidad de modulos.
            Assert.Equal(beforeB, afterB);
            Assert.Equal(beforeIds, ModuleIds(after, PushBackSide.B));
        }

        /// <summary>Y al reves: subir B no toca A.</summary>
        [Fact]
        public void RaisingTheLevelsOfB_LeavesEveryPieceOfA_Untouched()
        {
            var before = Resolve(State(levelsA: 4, levelsB: 2));
            var beforeA = HeaderHeights(before, PushBackSide.A);
            var beforeIds = ModuleIds(before, PushBackSide.A);

            var after = Resolve(State(levelsA: 4, levelsB: 3));

            Assert.True(HeaderHeights(after, PushBackSide.B)[0] > HeaderHeights(before, PushBackSide.B)[0] + 1.0);
            Assert.Equal(beforeA, HeaderHeights(after, PushBackSide.A));
            Assert.Equal(beforeIds, ModuleIds(after, PushBackSide.A));
        }

        private static IReadOnlyList<string> ModuleIds(PushBackSystem system, PushBackSide side)
        {
            var prefix = PushBackCompositeStructure.SideBModulePrefix;
            return system.Structure.Modules
                .Where(module => (module.ModuleId ?? string.Empty).StartsWith(prefix, StringComparison.Ordinal)
                                 == (side == PushBackSide.B))
                .Select(module => module.ModuleId)
                .ToList();
        }

        /// <summary>
        /// La interfaz central tiene DOS lineas fisicas —la terminal de A y la inicial de B— y pueden medir distinto.
        /// Es la consecuencia visible de que la altura sea de cada lado.
        /// </summary>
        [Fact]
        public void TheTwoInterfaceLines_CanHaveDifferentHeights()
        {
            var system = Resolve(State(levelsA: 4, levelsB: 2));
            var headers = system.Structure.Modules
                .Where(module => module.IsHeader && module.AssociatedFrameConfiguration != null)
                .ToList();

            var prefix = PushBackCompositeStructure.SideBModulePrefix;
            var lastOfA = headers.Last(module => !(module.ModuleId ?? string.Empty).StartsWith(prefix, StringComparison.Ordinal));
            var firstOfB = headers.First(module => (module.ModuleId ?? string.Empty).StartsWith(prefix, StringComparison.Ordinal));

            Assert.True(
                lastOfA.AssociatedFrameConfiguration.Height > firstOfB.AssociatedFrameConfiguration.Height + 1.0,
                "la linea terminal de A y la inicial de B son DOS piezas: pueden medir distinto");
        }

        /// <summary>
        /// El DIBUJO lo refleja: ninguna pieza del lado B alcanza la altura del lado A. Se mide sobre el lateral
        /// real, no sobre el modelo, porque es donde el dueño lo ve.
        /// </summary>
        [Fact]
        public void TheDrawing_HasNoPieceOfB_ReachingTheHeightOfA()
        {
            var system = Resolve(State(levelsA: 4, levelsB: 2));
            var instances = new PushBackSystemLateralBuilder().Build(system, Catalog).Flatten().Instances;

            var heightA = HeaderHeights(system, PushBackSide.A)[0];
            var heightB = HeaderHeights(system, PushBackSide.B)[0];
            Assert.True(heightB < heightA - 1.0);

            // La mitad de B del rack: desde el arranque de su primer modulo hacia el final.
            var prefix = PushBackCompositeStructure.SideBModulePrefix;
            var sideBStart = system.Structure.Modules
                .First(module => (module.ModuleId ?? string.Empty).StartsWith(prefix, StringComparison.Ordinal))
                .StartX;

            var structural = instances
                .Where(instance => instance.Role == HeaderBlockRole.Post
                                   || instance.Role == HeaderBlockRole.Diagonal
                                   || instance.Role == HeaderBlockRole.Horizontal)
                .Where(instance => instance.Insertion.X > sideBStart + 1e-6)
                .ToList();

            Assert.NotEmpty(structural);
            Assert.All(structural, instance => Assert.True(
                instance.Insertion.Y < heightA - 1.0,
                "ninguna pieza de la mitad de B puede llegar a la altura del lado A"));
        }

        // ================= Q: I-40 con alturas independientes ==================================================

        /// <summary>
        /// Una cabecera PERSONALIZADA de un lado no se toca porque el otro cambie de niveles. La identidad
        /// (<c>ModuleId</c>) y la configuracion viajan por lado, asi que la personalizacion de A no puede aterrizar
        /// en B ni al reves — que es exactamente lo que la reconciliacion de I-40 promete.
        /// </summary>
        [Fact]
        public void CustomHeaders_SurviveALevelChangeOnTheOtherSide()
        {
            var state = State(levelsA: 4, levelsB: 2);
            var resolver = new PushBackResolver(Catalog);

            // Se resuelve UNA vez y se toma el snapshot: es el camino real —el usuario abre el rack y despues
            // personaliza una cabecera—, y es donde la secuencia compuesta de modulos ya existe con sus identidades.
            var design = resolver.Snapshot(
                new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog).System);

            // Se personaliza UNA cabecera de cada lado, con alturas distintas y reconocibles.
            var prefix = PushBackCompositeStructure.SideBModulePrefix;
            var headers = design.Structure.Modules
                .Where(module => module.Kind == DynamicRackModuleKind.HeaderStart
                                 || module.Kind == DynamicRackModuleKind.HeaderIntermediate
                                 || module.Kind == DynamicRackModuleKind.HeaderEnd)
                .ToList();
            Assert.True(headers.Count >= 2, "hacen falta cabeceras en los dos lados");

            var headerA = headers.First(module => !(module.ModuleId ?? string.Empty).StartsWith(prefix, StringComparison.Ordinal));
            var headerB = headers.First(module => (module.ModuleId ?? string.Empty).StartsWith(prefix, StringComparison.Ordinal));
            Customize(headerA, 321.0);
            Customize(headerB, 123.0);

            var before = resolver.Resolve(design);
            Assert.Equal(321.0, HeightOf(before, headerA.ModuleId), 4);
            Assert.Equal(123.0, HeightOf(before, headerB.ModuleId), 4);

            // Cambiar los NIVELES del lado A no toca la cabecera personalizada de B, ni su identidad.
            design.Structure.LoadLevels = 5;
            foreach (var front in design.Structure.Fronts)
            {
                front.LoadLevels = 5;
            }

            var after = resolver.Resolve(design);
            Assert.Equal(123.0, HeightOf(after, headerB.ModuleId), 4);
            Assert.Equal(321.0, HeightOf(after, headerA.ModuleId), 4);
            Assert.Contains(after.Structure.Modules, module => module.ModuleId == headerB.ModuleId);
        }

        private static void Customize(DynamicRackModuleDesign module, double height)
        {
            module.UseCalculatedHeaderConfiguration = false;
            module.HeaderConfiguration = new RackCad.Domain.RackFrames.RackFrameConfiguration { Height = height };
        }

        private static double HeightOf(PushBackSystem system, string moduleId)
            => system.Structure.Modules
                .Where(module => module.ModuleId == moduleId && module.AssociatedFrameConfiguration != null)
                .Select(module => module.AssociatedFrameConfiguration.Height)
                .DefaultIfEmpty(-1.0)
                .First();

        /// <summary>Con los DOS lados en los mismos niveles, las alturas coinciden: la regla no inventa diferencias.</summary>
        [Fact]
        public void EqualLevels_StillProduceEqualHeights()
        {
            var system = Resolve(State(levelsA: 3, levelsB: 3));
            Assert.Equal(HeaderHeights(system, PushBackSide.A)[0], HeaderHeights(system, PushBackSide.B)[0], 4);
        }
    }
}
