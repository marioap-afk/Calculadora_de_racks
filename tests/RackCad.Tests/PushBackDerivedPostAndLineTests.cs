using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-40, CUARTA entrega del Owner: la ALTURA DEL POSTE DERIVADO y la edicion POR LINEA DE CABECERAS.
    ///
    /// <para><b>Poste derivado.</b> Es el poste que nace de dos separadores CONSECUTIVOS. Su LONGITUD nunca estuvo
    /// modelada: <c>DynamicSystemLateralBuilder.AddDerivedPost</c> le pasaba <c>context.Height</c> —la altura de la
    /// cabecera— al parametro dinamico LONGITUD del bloque. Es decir, la HEREDABA. Su REFUERZO si era editable
    /// (<c>DerivedPostReinforcementHeight</c>), y por eso el campo nuevo es su hermano exacto: mismo tipo, mismo
    /// sitio, misma nulabilidad, mismo significado del vacio.</para>
    ///
    /// <para><b>Linea de cabeceras.</b> Un rack tiene DOS cosas que se llaman cabecera: el MODULO longitudinal
    /// —una entrada de <c>Modules</c>, la secuencia de fondo compartida por todo el rack— y la LINEA fisica —la
    /// linea transversal de postes, la que el lateral dibuja como un CORTE—. Cada modulo se materializa una vez en
    /// cada linea que lo cubre, asi que UNA <c>DynamicRackModuleDesign</c> ES todas las instancias de esa cabecera:
    /// por eso el modelo no podia expresar «esta linea distinta de aquella».</para>
    /// </summary>
    public class PushBackDerivedPostAndLineTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackEditorDesignAssembler Assembler() => new PushBackEditorDesignAssembler(Catalog);

        private static PushBackEditorInputs Inputs()
            => new PushBackEditorInputs
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 6
            };

        /// <summary>A live rack with TWO fronts, so it has at least three physical LINES (postes 0, 1 y 2).</summary>
        private static PushBackEditorState Live(PushBackEditorDesignAssembler assembler, out PushBackEditorInputs inputs)
        {
            var state = new PushBackEditorState();
            state.SetFrontCount(2);
            state.Structure.Fronts[0].PalletsDeep = 6;
            inputs = Inputs();
            var seed = assembler.Build(state, inputs);
            Assert.True(seed.IsValid, seed.Error);
            assembler.AcceptComputation(state, seed);
            return state;
        }

        private static RackFrameConfiguration CustomHeader(double height, double panelClear)
        {
            var catalog = Catalog;
            var configuration = new DynamicRackSystemBuilder(catalog).BuildHeaderConfiguration(
                RackFrameTemplateCatalog.Default, catalog.Defaults?.Post, height, 48.0);
            configuration.PanelClear = panelClear;
            return configuration;
        }

        private static string[] HeaderIds(PushBackEditorState state)
            => state.ModuleSession.Modules.Where(m => m.IsHeader).Select(m => m.ModuleId).ToArray();

        /// <summary>The LONGITUD the lateral plan gives to the derived posts of a rack.</summary>
        private static double[] DerivedPostLengths(PushBackEditorComputation computation)
        {
            var system = computation.System.Structure;
            var derivedOffsets = system.GetDerivedPostOffsets().ToList();
            if (derivedOffsets.Count == 0)
            {
                return Array.Empty<double>();
            }

            return computation.LateralPlan.Flatten().Instances
                .Where(instance => instance.Role == HeaderBlockRole.Post
                                   && instance.DynamicParameters.ContainsKey(SelectiveRackDefaults.LengthParam)
                                   && derivedOffsets.Any(offset => Math.Abs(instance.ConnectionAnchor.X - offset) < 1e-6))
                .Select(instance => instance.DynamicParameters[SelectiveRackDefaults.LengthParam])
                .ToArray();
        }

        // ===== A — ALTURA DEL POSTE DERIVADO ===================================================================

        /// <summary>Sin valor explicito el poste derivado HEREDA la altura de la cabecera: exactamente lo que hacia
        /// antes de que el campo existiera, que es lo que garantiza que un rack antiguo se dibuje igual.</summary>
        [Fact]
        public void A_WithNoValue_TheDerivedPostInheritsTheHeaderHeight()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);

            inputs.DerivedPostHeight = null;
            var computation = assembler.Build(state, inputs);

            Assert.True(computation.IsValid, computation.Error);
            var lengths = DerivedPostLengths(computation);
            Assert.NotEmpty(lengths);

            var headerHeight = computation.System.Structure.Modules
                .First(module => module.IsHeader && module.AssociatedFrameConfiguration != null)
                .AssociatedFrameConfiguration.Height;
            Assert.All(lengths, length => Assert.Equal(headerHeight, length, 4));
        }

        /// <summary>Un valor manual llega al parametro dinamico LONGITUD del bloque del poste derivado.</summary>
        [Fact]
        public void A_AManualValue_ReachesTheBlockLength()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var inherited = DerivedPostLengths(assembler.Build(state, inputs)).First();
            var manual = inherited + 19.0;

            inputs.DerivedPostHeight = manual;
            var computation = assembler.Build(state, inputs);

            Assert.True(computation.IsValid, computation.Error);
            Assert.Equal(manual, computation.System.Structure.DerivedPostHeight);
            Assert.All(DerivedPostLengths(computation), length => Assert.Equal(manual, length, 4));
        }

        /// <summary>El refuerzo sigue siendo un valor INDEPENDIENTE: fijar la altura del poste no lo arrastra.</summary>
        [Fact]
        public void A_TheReinforcementHeight_StaysIndependent()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);

            inputs.DerivedPostHeight = 100.0;
            inputs.DerivedPostReinforced = true;
            inputs.DerivedPostReinforcementHeight = 40.0;
            var computation = assembler.Build(state, inputs);

            Assert.True(computation.IsValid, computation.Error);
            Assert.Equal(100.0, computation.System.Structure.DerivedPostHeight);
            Assert.Equal(40.0, computation.System.Structure.DerivedPostReinforcementHeight);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-5.0)]
        public void A_ANonPositiveValue_IsRejectedWithAVisibleError(double invalid)
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);

            inputs.DerivedPostHeight = invalid;
            var computation = assembler.Build(state, inputs);

            Assert.False(computation.IsValid);
            Assert.False(string.IsNullOrWhiteSpace(computation.Error));
        }

        [Fact]
        public void A_ItSurvivesThePersistenceRoundTrip_AndReopening()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            inputs.DerivedPostHeight = 137.0;
            var design = assembler.BuildDesign(state, inputs);

            var store = new RackProjectStore();
            var reloaded = store.Deserialize(store.Serialize(RackProject.ForPushBack(design))).PushBackDesign;
            Assert.Equal(137.0, reloaded.Structure.DerivedPostHeight);

            var reopened = new PushBackEditorState();
            var recovered = reopened.LoadFromDesign(reloaded, assembler.Resolver);
            Assert.Equal(137.0, recovered.DerivedPostHeight);
        }

        /// <summary>Un documento ANTERIOR no lleva el campo, y eso significa exactamente «hereda»: nada cambia.</summary>
        [Fact]
        public void A_ALegacyDocumentWithoutTheField_KeepsTheHistoricalBehaviour()
        {
            var document = new DynamicRackSystemDocument();
            Assert.Null(document.DerivedPostHeight);
            Assert.Null(document.ToDomain().DerivedPostHeight);
            Assert.Null(document.ToDesign().DerivedPostHeight);
        }

        /// <summary>«Restaurar parametros globales» lo devuelve al calculo, como a sus cuatro hermanos.</summary>
        [Fact]
        public void A_TheRackWideRestore_ReturnsItToTheInheritedHeight()
        {
            var inputs = Inputs();
            inputs.DerivedPostHeight = 137.0;
            PushBackAdvancedRackParameters.Reset(inputs);
            Assert.Null(inputs.DerivedPostHeight);
        }

        /// <summary>El BOM cotiza el poste derivado a la altura manual, no a la derivada de la linea.</summary>
        [Fact]
        public void A_TheBom_QuotesTheDerivedPostAtTheManualHeight()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var before = assembler.Build(state, inputs);

            inputs.DerivedPostHeight = 137.0;
            var after = assembler.Build(state, inputs);

            Assert.True(after.IsValid, after.Error);
            Assert.NotNull(after.Bom);
            Assert.NotEqual(
                string.Join("|", before.Bom.Components.Select(c => c.Description + ":" + c.Quantity)),
                string.Join("|", after.Bom.Components.Select(c => c.Description + ":" + c.Quantity)));
        }

        // ===== B — LINEA DE CABECERAS ==========================================================================

        /// <summary>
        /// El hecho que define el requisito: una LINEA es un poste transversal, y el rack tiene varias. Las lineas
        /// que existen son las fronteras que I-33 conserva, las mismas que el lateral dibuja como cortes.
        /// </summary>
        [Fact]
        public void B_ARackHasSeveralPhysicalLines_AndTheyAreItsPosts()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var computation = assembler.Build(state, inputs);

            var lines = DynamicFrontActivation.PresentBoundaries(computation.System.Structure);
            Assert.Equal(3, lines.Count);   // dos frentes ⇒ tres lineas de postes

            var cortes = computation.LateralCortes;
            Assert.Equal(lines.Count, cortes.Count);
            Assert.Equal(lines, cortes.Select(corte => corte.PostIndex).ToList());
        }

        /// <summary>
        /// El corazon de B: una LINEA puede llevar una configuracion distinta de la de otra, y la geometria lo
        /// refleja. Sin override, todas las lineas usan la del modulo — que es lo que ocurria siempre.
        /// </summary>
        [Fact]
        public void B_OneLineCanCarryADifferentConfiguration_AndTheGeometryShowsIt()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headers = HeaderIds(state);

            // Linea A = poste 0. Linea B = poste 1, sin tocar.
            var result = state.ModuleSession.ApplyHeaderConfigurationToLine(
                CustomHeader(187.0, 41.5), 0, headers);
            Assert.True(result.Applied, result.RejectionReason);
            state.CommitModuleEdits();

            var computation = assembler.Build(state, inputs);
            Assert.True(computation.IsValid, computation.Error);

            var lineA = computation.LateralCortes.First(corte => corte.PostIndex == 0);
            var lineB = computation.LateralCortes.First(corte => corte.PostIndex == 1);
            Assert.NotEqual(Signature(lineA.Plan), Signature(lineB.Plan));

            // Y la autoridad unica lo dice explicitamente para cada linea.
            var module = computation.System.Structure.Modules.First(m => m.ModuleId == headers[0]);
            Assert.Equal(
                187.0,
                DynamicFrontGeometry.HeaderConfigurationAtPost(computation.System.Structure, module, Catalog, 0).Height,
                4);
            Assert.NotEqual(
                187.0,
                DynamicFrontGeometry.HeaderConfigurationAtPost(computation.System.Structure, module, Catalog, 1).Height);
        }

        /// <summary>Aplicar a una linea NO toca las demas, y cada destino recibe su propia copia.</summary>
        [Fact]
        public void B_ApplyingToOneLine_LeavesEveryOtherLineUntouched_WithIndependentCopies()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headers = HeaderIds(state);
            var source = CustomHeader(187.0, 41.5);

            state.ModuleSession.ApplyHeaderConfigurationToLine(source, 0, headers);

            source.Height = 999.0;   // mutar el origen despues no puede alcanzar a ningun destino
            foreach (var id in headers)
            {
                Assert.Equal(187.0, state.ModuleSession.HeaderConfigurationCopy(id, 0).Height, 4);
                Assert.False(state.ModuleSession.HasLineOverride(id, 1));
            }

            Assert.Equal(new[] { 0 }, state.ModuleSession.OverriddenLines);
        }

        /// <summary>La operacion por linea es ATOMICA: un destino invalido no deja nada aplicado.</summary>
        [Fact]
        public void B_AnInvalidTarget_RejectsTheWholeLineOperation()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headers = HeaderIds(state);

            var result = state.ModuleSession.ApplyHeaderConfigurationToLine(
                CustomHeader(187.0, 41.5), 0, new[] { headers[0], "NO-EXISTE" });

            Assert.False(result.Applied);
            Assert.Empty(state.ModuleSession.OverriddenLines);
            Assert.False(state.ModuleSession.HasPendingChanges);
        }

        /// <summary>Cancelar revierte tambien lo que se hizo por linea.</summary>
        [Fact]
        public void B_Cancel_RevertsTheLineOperation()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headers = HeaderIds(state);

            state.ModuleSession.ApplyHeaderConfigurationToLine(CustomHeader(187.0, 41.5), 0, headers);
            Assert.True(state.ModuleSession.HasPendingChanges);

            state.CancelModuleEdits();

            Assert.Empty(state.ModuleSession.OverriddenLines);
            Assert.False(state.ModuleSession.HasPendingChanges);
        }

        /// <summary>Las diferencias por linea sobreviven al round-trip de persistencia y a la REAPERTURA.</summary>
        [Fact]
        public void B_TheLineDifferences_SurviveSaveLoadAndReopening()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headers = HeaderIds(state);

            state.ModuleSession.ApplyHeaderConfigurationToLine(CustomHeader(187.0, 41.5), 0, headers);
            state.CommitModuleEdits();
            var design = assembler.BuildDesign(state, inputs);

            var store = new RackProjectStore();
            var reloaded = store.Deserialize(store.Serialize(RackProject.ForPushBack(design))).PushBackDesign;
            Assert.NotEmpty(reloaded.Structure.HeaderLineOverrides);

            var reopened = new PushBackEditorState();
            reopened.LoadFromDesign(reloaded, assembler.Resolver);

            foreach (var id in headers)
            {
                Assert.Equal(187.0, reopened.ModuleSession.HeaderConfigurationCopy(id, 0).Height, 4);
                Assert.Equal(41.5, reopened.ModuleSession.HeaderConfigurationCopy(id, 0).PanelClear, 4);
                Assert.False(reopened.ModuleSession.HasLineOverride(id, 1));
            }
        }

        /// <summary>«Todas las cabeceras» deja el rack UNIFORME: escribe el modulo y retira las excepciones de linea,
        /// porque si no seguirian ganando justo despues de pedir que todas fueran iguales.</summary>
        [Fact]
        public void B_ApplyingToAll_MakesTheRackUniformAgain()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headers = HeaderIds(state);

            state.ModuleSession.ApplyHeaderConfigurationToLine(CustomHeader(187.0, 41.5), 0, headers);
            state.ModuleSession.ApplyHeaderConfiguration(CustomHeader(163.0, 44.0), headers);
            state.ModuleSession.ClearLineOverrides(headers);
            state.CommitModuleEdits();

            var computation = assembler.Build(state, inputs);
            Assert.Empty(computation.System.Structure.HeaderLineOverrides);

            foreach (var corte in computation.LateralCortes)
            {
                foreach (var id in headers)
                {
                    var module = computation.System.Structure.Modules.First(m => m.ModuleId == id);
                    Assert.Equal(
                        163.0,
                        DynamicFrontGeometry.HeaderConfigurationAtPost(
                            computation.System.Structure, module, Catalog, corte.PostIndex).Height,
                        4);
                }
            }
        }

        /// <summary>El BOM lee la MISMA autoridad, asi que cotiza lo que cada linea dibuja.</summary>
        [Fact]
        public void B_TheBom_FollowsTheLineDifferences()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headers = HeaderIds(state);
            var before = assembler.Build(state, inputs);

            state.ModuleSession.ApplyHeaderConfigurationToLine(CustomHeader(187.0, 41.5), 0, headers);
            state.CommitModuleEdits();
            var after = assembler.Build(state, inputs);

            Assert.True(after.IsValid, after.Error);
            Assert.NotEqual(
                string.Join("|", before.Bom.Components.Select(c => c.Description + ":" + c.Quantity)),
                string.Join("|", after.Bom.Components.Select(c => c.Description + ":" + c.Quantity)));
        }

        /// <summary>
        /// COMPATIBILIDAD: un documento anterior no lleva overrides, y sin ellos la autoridad por linea se comporta
        /// exactamente como siempre. Es lo que protege al Dinamico, que nunca los escribe.
        /// </summary>
        [Fact]
        public void B_WithNoOverrides_TheAuthorityBehavesExactlyAsBefore()
        {
            var document = new DynamicRackSystemDocument();
            Assert.Null(document.HeaderLineOverrides);
            Assert.Empty(document.ToDomain().HeaderLineOverrides);
            Assert.Empty(document.ToDesign().HeaderLineOverrides);

            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headerId = HeaderIds(state)[0];
            state.ModuleSession.SetHeaderConfiguration(headerId, CustomHeader(151.0, 41.5));
            state.CommitModuleEdits();
            var computation = assembler.Build(state, inputs);

            Assert.Empty(computation.System.Structure.HeaderLineOverrides);
            var module = computation.System.Structure.Modules.First(m => m.ModuleId == headerId);
            foreach (var corte in computation.LateralCortes)
            {
                Assert.Equal(
                    151.0,
                    DynamicFrontGeometry.HeaderConfigurationAtPost(
                        computation.System.Structure, module, Catalog, corte.PostIndex).Height,
                    4);
            }
        }

        /// <summary>Un override que apunta a un modulo que el rack reconstruido ya no tiene se descarta, por la
        /// misma razon por la que la reconciliacion descarta una personalizacion sin modulo.</summary>
        [Fact]
        public void B_AnOverrideOfAModuleThatNoLongerExists_IsDropped()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headers = HeaderIds(state);

            state.ModuleSession.ApplyHeaderConfigurationToLine(CustomHeader(187.0, 41.5), 0, headers);
            var commit = state.CommitModuleEdits();
            Assert.NotEmpty(commit.LineOverrides);

            // Un reset del rack no lleva ninguna: eso es lo que lo hace un reset.
            state.ModuleSession.RequestStandardRestore();
            state.CommitModuleEdits();
            var rebuilt = assembler.Build(state, inputs);

            Assert.Empty(rebuilt.System.Structure.HeaderLineOverrides);
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
                    string.Join(";", instance.DynamicParameters.OrderBy(p => p.Key)
                        .Select(p => p.Key + "=" + p.Value.ToString("0.###"))))));
    }
}
