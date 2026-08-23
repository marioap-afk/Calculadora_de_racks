using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-40, ronda 6 — OWNER DECISION: la FRONTAL y la POSTERIOR son VISTAS DE CORTE, no envolventes de altura.
    ///
    /// <para>
    /// La frontal es el corte por la PRIMERA cabecera longitudinal del rack (el extremo bajo, el del pasillo) y la
    /// posterior el corte por la ULTIMA. Mirando el rack de frente, la cabecera que se tiene delante es esa y
    /// ninguna otra: que exista una cabecera mas alta en otra posicion longitudinal NO puede dominar la vista.
    /// </para>
    ///
    /// <para>
    /// El eje de la vista es el LONGITUDINAL (que cabecera se tiene delante). El <c>postIndex</c> sigue siendo la
    /// LINEA fisica, y cada poste que la vista dibuja pertenece a su linea: los dos ejes no se mezclan.
    /// </para>
    /// </summary>
    public class PushBackFrontalRearCutTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackEditorDesignAssembler Assembler() => new PushBackEditorDesignAssembler(Catalog);

        private static PushBackEditorInputs Inputs()
            => new PushBackEditorInputs
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 6
            };

        /// <summary>A live rack with TWO fronts: three physical lines and several longitudinal cabeceras.</summary>
        private static PushBackEditorState Live(PushBackEditorDesignAssembler assembler, out PushBackEditorInputs inputs)
        {
            var state = new PushBackEditorState();
            state.SetFrontCount(2);

            // Fondo UNIFORME: asi las tres lineas cubren el mismo rango longitudinal y las tres tienen delante la
            // misma primera cabecera y detras la misma ultima. Con fondos distintos cada linea acaba en la SUYA,
            // que tambien es correcto, pero no es lo que estos casos miden.
            foreach (var front in state.Structure.Fronts)
            {
                front.PalletsDeep = 6;
            }
            inputs = Inputs();
            var seed = assembler.Build(state, inputs);
            Assert.True(seed.IsValid, seed.Error);
            assembler.AcceptComputation(state, seed);
            return state;
        }

        private static RackFrameConfiguration Header(double height)
        {
            var catalog = Catalog;
            return new DynamicRackSystemBuilder(catalog).BuildHeaderConfiguration(
                RackFrameTemplateCatalog.Default, catalog.Defaults?.Post, height, 48.0);
        }

        /// <summary>The header modules of the rack, in longitudinal order (first = aisle end).</summary>
        private static string[] HeaderIds(PushBackEditorState state)
            => state.ModuleSession.Modules.Where(module => module.IsHeader).Select(module => module.ModuleId).ToArray();

        /// <summary>
        /// Give a longitudinal cabecera its own height ON EVERY LINE, so the fixture varies only along the axis the
        /// frontal/rear views select on.
        /// </summary>
        private static void SetHeightEverywhere(
            PushBackEditorState state, string moduleId, double height, IReadOnlyList<int> lines)
        {
            state.ModuleSession.ApplyHeaderConfigurationToInstances(Header(height), new[] { moduleId }, lines);
        }

        private static IReadOnlyList<int> Lines(PushBackEditorComputation computation)
            => DynamicFrontActivation.PresentBoundaries(computation.System.Structure);

        /// <summary>The LONGITUD of the posts a frontal/rear plan draws — one per physical line.</summary>
        private static double[] PostLengths(HeaderRunPlan plan)
            => plan == null
                ? Array.Empty<double>()
                : plan.Flatten().Instances
                    .Where(instance => instance.Role == HeaderBlockRole.Post
                                       && instance.DynamicParameters.ContainsKey(SelectiveRackDefaults.LengthParam))
                    .Select(instance => instance.DynamicParameters[SelectiveRackDefaults.LengthParam])
                    .ToArray();

        private static PushBackEditorComputation WithHeights(
            double first, double middle, double last, out string[] headers, out PushBackEditorState state)
        {
            var assembler = Assembler();
            state = Live(assembler, out var inputs);
            headers = HeaderIds(state);
            Assert.True(headers.Length >= 3, "el fixture necesita al menos tres cabeceras longitudinales");

            var lines = DynamicFrontActivation.PresentBoundaries(
                state.WorkingBaseline.Structure);

            SetHeightEverywhere(state, headers[0], first, lines);
            SetHeightEverywhere(state, headers[1], middle, lines);
            SetHeightEverywhere(state, headers[headers.Length - 1], last, lines);
            state.CommitModuleEdits();

            var computation = assembler.Build(state, inputs);
            Assert.True(computation.IsValid, computation.Error);
            return computation;
        }

        // ===== CASO A — frontal ALTA, posterior BAJA, intermedia mas alta que las dos =========================

        /// <summary>
        /// REGRESION del caso A. La frontal muestra la PRIMERA cabecera (180") y la posterior la ULTIMA (120"),
        /// aunque exista una intermedia de 200". Contra <c>04f76cf</c> las dos mostraban 200: la altura se resolvia
        /// con un <c>Max()</c> sobre todas las cabeceras de la linea — una ENVOLVENTE, no un corte.
        /// </summary>
        [Fact]
        public void CasoA_REGRESION_TheFrontalShowsTheFirstHeader_AndTheRearTheLast()
        {
            var computation = WithHeights(180.0, 200.0, 120.0, out _, out _);
            var builder = new PushBackSystemFrontalBuilder();

            var frontal = PostLengths(builder.BuildPlan(computation.System, Catalog, PushBackFrontalEnd.EntradaSalida));
            var posterior = PostLengths(builder.BuildPlan(computation.System, Catalog, PushBackFrontalEnd.Posterior));

            Assert.NotEmpty(frontal);
            Assert.NotEmpty(posterior);
            Assert.All(frontal, length => Assert.Equal(180.0, length, 4));
            Assert.All(posterior, length => Assert.Equal(120.0, length, 4));
            Assert.DoesNotContain(frontal, length => Math.Abs(length - 200.0) < 1e-6);
            Assert.DoesNotContain(posterior, length => Math.Abs(length - 200.0) < 1e-6);
        }

        // ===== CASO B — frontal BAJA con una intermedia ALTA ==================================================

        [Fact]
        public void CasoB_REGRESION_ALowFrontHeader_IsNotRaisedByATallerIntermediateOne()
        {
            var computation = WithHeights(120.0, 220.0, 150.0, out _, out _);
            var frontal = PostLengths(new PushBackSystemFrontalBuilder()
                .BuildPlan(computation.System, Catalog, PushBackFrontalEnd.EntradaSalida));

            Assert.NotEmpty(frontal);
            Assert.All(frontal, length => Assert.Equal(120.0, length, 4));
        }

        // ===== CASO C — posterior BAJA con una anterior ALTA ==================================================

        [Fact]
        public void CasoC_REGRESION_ALowRearHeader_IsNotRaisedByATallerPreviousOne()
        {
            var computation = WithHeights(150.0, 210.0, 110.0, out _, out _);
            var posterior = PostLengths(new PushBackSystemFrontalBuilder()
                .BuildPlan(computation.System, Catalog, PushBackFrontalEnd.Posterior));

            Assert.NotEmpty(posterior);
            Assert.All(posterior, length => Assert.Equal(110.0, length, 4));
        }

        // ===== CASO D — personalizacion POR INSTANCIA =========================================================

        /// <summary>
        /// CASO D. La MISMA cabecera longitudinal con alturas distintas en la linea frontal y en la posterior: cada
        /// vista dibuja el poste de SU linea con SU altura. Los dos ejes conviven —longitudinal para elegir que
        /// cabecera se ve, linea para elegir que poste—, y ninguno colapsa al otro.
        /// </summary>
        [Fact]
        public void CasoD_EachLineKeepsItsOwnHeight_InsideTheSameView()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headers = HeaderIds(state);
            var lines = DynamicFrontActivation.PresentBoundaries(state.WorkingBaseline.Structure);
            Assert.True(lines.Count >= 3);

            // La PRIMERA cabecera, con alturas distintas por linea.
            state.ModuleSession.ApplyHeaderConfigurationToInstances(Header(175.0), new[] { headers[0] }, new[] { lines[0] });
            state.ModuleSession.ApplyHeaderConfigurationToInstances(Header(125.0), new[] { headers[0] }, new[] { lines[2] });
            state.CommitModuleEdits();

            var computation = assembler.Build(state, inputs);
            Assert.True(computation.IsValid, computation.Error);

            var frontal = PostLengths(new PushBackSystemFrontalBuilder()
                .BuildPlan(computation.System, Catalog, PushBackFrontalEnd.EntradaSalida));

            // La vista frontal dibuja un poste por LINEA: el de la linea 1 mide 175 y el de la 3 mide 125.
            Assert.Contains(frontal, length => Math.Abs(length - 175.0) < 1e-6);
            Assert.Contains(frontal, length => Math.Abs(length - 125.0) < 1e-6);
        }

        // ===== CASO F — todas x todas =========================================================================

        [Fact]
        public void CasoF_WhenEveryHeaderIsTheSame_TheTwoViewsAgree_BecauseTheyPhysicallyAre()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headers = HeaderIds(state);
            var lines = DynamicFrontActivation.PresentBoundaries(state.WorkingBaseline.Structure);

            state.ModuleSession.ApplyHeaderConfigurationToInstances(Header(190.0), headers, lines);
            state.CommitModuleEdits();
            var computation = assembler.Build(state, inputs);

            var builder = new PushBackSystemFrontalBuilder();
            var frontal = PostLengths(builder.BuildPlan(computation.System, Catalog, PushBackFrontalEnd.EntradaSalida));
            var posterior = PostLengths(builder.BuildPlan(computation.System, Catalog, PushBackFrontalEnd.Posterior));

            Assert.All(frontal, length => Assert.Equal(190.0, length, 4));
            Assert.All(posterior, length => Assert.Equal(190.0, length, 4));
        }

        // ===== CASO G — una cabecera INTERMEDIA no toca las dos vistas ========================================

        /// <summary>CASO G. Cambiar solo una cabecera intermedia mueve su corte lateral y deja la frontal y la
        /// posterior exactamente como estaban.</summary>
        [Fact]
        public void CasoG_ChangingAnIntermediateHeader_LeavesTheFrontalAndRearUntouched()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headers = HeaderIds(state);
            var lines = DynamicFrontActivation.PresentBoundaries(state.WorkingBaseline.Structure);
            var builder = new PushBackSystemFrontalBuilder();

            var before = assembler.Build(state, inputs);
            var frontalBefore = PostLengths(builder.BuildPlan(before.System, Catalog, PushBackFrontalEnd.EntradaSalida));
            var posteriorBefore = PostLengths(builder.BuildPlan(before.System, Catalog, PushBackFrontalEnd.Posterior));
            // El corte por LINEA es el que lee la configuracion fisica de cada modulo; el lateral del sistema
            // completo dibuja la del modulo, que un override por linea no toca.
            var lateralBefore = string.Join("//", before.LateralCortes.Select(corte => Signature(corte.Plan)));

            state.ModuleSession.ApplyHeaderConfigurationToInstances(Header(233.0), new[] { headers[1] }, lines);
            state.CommitModuleEdits();
            var after = assembler.Build(state, inputs);

            Assert.NotEqual(
                lateralBefore,
                string.Join("//", after.LateralCortes.Select(corte => Signature(corte.Plan))));   // los cortes SI cambian
            Assert.Equal(frontalBefore, PostLengths(builder.BuildPlan(after.System, Catalog, PushBackFrontalEnd.EntradaSalida)));
            Assert.Equal(posteriorBefore, PostLengths(builder.BuildPlan(after.System, Catalog, PushBackFrontalEnd.Posterior)));
        }

        // ===== CASO E — round-trip ============================================================================

        [Fact]
        public void CasoE_TheTwoCuts_SurviveThePersistenceRoundTripAndReopening()
        {
            var assembler = Assembler();
            var computation = WithHeights(180.0, 200.0, 120.0, out _, out var state);
            var design = assembler.BuildDesign(state, Inputs());

            var store = new RackProjectStore();
            var reloaded = store.Deserialize(store.Serialize(RackProject.ForPushBack(design))).PushBackDesign;
            var resolved = new PushBackResolver(Catalog).Resolve(reloaded);

            var builder = new PushBackSystemFrontalBuilder();
            Assert.All(
                PostLengths(builder.BuildPlan(resolved, Catalog, PushBackFrontalEnd.EntradaSalida)),
                length => Assert.Equal(180.0, length, 4));
            Assert.All(
                PostLengths(builder.BuildPlan(resolved, Catalog, PushBackFrontalEnd.Posterior)),
                length => Assert.Equal(120.0, length, 4));
        }

        private static string Signature(HeaderRunPlan plan)
            => plan == null
                ? string.Empty
                : string.Join("|", plan.Flatten().Instances.Select(instance => string.Join(
                    ",",
                    instance.Role,
                    instance.PieceId,
                    instance.Insertion.X.ToString("0.###"),
                    instance.Insertion.Y.ToString("0.###"),
                    string.Join(";", instance.DynamicParameters.OrderBy(entry => entry.Key)
                        .Select(entry => entry.Key + "=" + entry.Value.ToString("0.###"))))));
    }
}
