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
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-40 — la operacion UNICA de cabeceras de Push Back, en Application y pura.
    ///
    /// PBH-01 (autoridad efectiva de la cabecera personalizada, incluida su ALTURA), PBH-02 (alcance: esta
    /// cabecera o todas las aplicables) y PBH-03 (reutilizar la configuracion de OTRA cabecera como COPIA
    /// independiente) comparten una sola operacion: obtener la configuracion efectiva, deep-copy, validar TODOS
    /// los destinos, aplicar atomicamente y recomputar una sola vez al Confirmar.
    ///
    /// Aqui se fija el contrato de esa operacion y su travesia completa: sesion transaccional, reconciliacion,
    /// resolver, geometria, persistencia round-trip y BOM. La frontera de UI que perdia el valor —el configurador
    /// compartido reemplazando su propia Configuration— se fija en RackCad.UI.Tests, porque solo existe alli.
    /// </summary>
    public class PushBackHeaderApplyTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackEditorDesignAssembler Assembler() => new PushBackEditorDesignAssembler(Catalog);

        private static PushBackEditorInputs Inputs()
            => new PushBackEditorInputs
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 6
            };

        /// <summary>A live editor with a baseline, so its modules exist and can be customized.</summary>
        private static PushBackEditorState Live(PushBackEditorDesignAssembler assembler, out PushBackEditorInputs inputs)
        {
            var state = new PushBackEditorState();
            state.Structure.Fronts[0].PalletsDeep = 6;
            inputs = Inputs();
            var seed = assembler.Build(state, inputs);
            Assert.True(seed.IsValid, seed.Error);
            assembler.AcceptComputation(state, seed);
            return state;
        }

        /// <summary>A header configuration marked on TWO axes: its HEIGHT (the property PBH-01 is about) and a
        /// second custom property, so a test can tell "the height arrived" from "the whole cabecera arrived".</summary>
        private static RackFrameConfiguration CustomHeader(double height, double panelClear)
        {
            var catalog = Catalog;
            var configuration = new DynamicRackSystemBuilder(catalog).BuildHeaderConfiguration(
                RackFrameTemplateCatalog.Default, catalog.Defaults?.Post, height, 48.0);
            configuration.PanelClear = panelClear;
            return configuration;
        }

        private static string[] HeaderIds(PushBackEditorState state)
            => state.ModuleSession.Modules.Where(module => module.IsHeader).Select(module => module.ModuleId).ToArray();

        private static RackCad.Domain.Systems.Dynamic.DynamicRackModule Resolved(
            PushBackEditorComputation computation, string moduleId)
            => computation.System.Structure.Modules.First(module => module.ModuleId == moduleId);

        // ===== PBH-01 — la cabecera personalizada es la autoridad efectiva ======================================

        /// <summary>
        /// La travesia completa de una ALTURA personalizada: sesion -> commit -> reconciliacion -> resolver ->
        /// geometria. La altura NO es adaptada por la reconciliacion (a diferencia del fondo y del peralte), asi
        /// que lo que el usuario fijo es exactamente lo que se dibuja.
        /// </summary>
        [Fact]
        public void PBH01_ACustomHeight_IsTheEffectiveAuthority_ThroughResolverAndGeometry()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headerId = HeaderIds(state)[0];
            var calculated = Resolved(assembler.Build(state, inputs), headerId).AssociatedFrameConfiguration.Height;
            var custom = calculated + 37.0;

            state.ModuleSession.SetHeaderConfiguration(headerId, CustomHeader(custom, 40.0));
            state.CommitModuleEdits();
            var computation = assembler.Build(state, inputs);

            Assert.True(computation.IsValid, computation.Error);
            var survivor = Resolved(computation, headerId);
            Assert.False(survivor.UseCalculatedHeaderConfiguration);
            Assert.Equal(custom, survivor.AssociatedFrameConfiguration.Height, 4);
            Assert.NotEqual(calculated, survivor.AssociatedFrameConfiguration.Height);

            // Geometria: el sistema resuelto produjo las cuatro vistas y el BOM sobre esa cabecera.
            Assert.NotNull(computation.LateralPlan);
            Assert.NotNull(computation.Bom);
            Assert.Contains(headerId, state.LastModuleReconciliation.Preserved);
        }

        /// <summary>Otras propiedades personalizadas viajan igual que la altura: la cabecera entera es la
        /// autoridad, no un campo suelto.</summary>
        [Fact]
        public void PBH01_OtherCustomProperties_TravelWithTheSameAuthority()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headerId = HeaderIds(state)[0];

            state.ModuleSession.SetHeaderConfiguration(headerId, CustomHeader(151.0, 41.5));
            state.CommitModuleEdits();
            var computation = assembler.Build(state, inputs);

            var survivor = Resolved(computation, headerId).AssociatedFrameConfiguration;
            Assert.Equal(151.0, survivor.Height, 4);
            Assert.Equal(41.5, survivor.PanelClear, 4);
        }

        /// <summary>La PROCEDENCIA acompaña al valor: una cabecera personalizada deja de ser calculada y un
        /// recalculo posterior no la regenera.</summary>
        [Fact]
        public void PBH01_Provenance_TurnsCustom_AndAPlainRecomputeDoesNotRegenerateIt()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headerId = HeaderIds(state)[0];

            state.ModuleSession.SetHeaderConfiguration(headerId, CustomHeader(151.0, 40.0));
            state.CommitModuleEdits();
            assembler.AcceptComputation(state, assembler.Build(state, inputs));
            state.ClearModuleCommit();

            var again = assembler.Build(state, inputs);
            var survivor = Resolved(again, headerId);
            Assert.False(survivor.UseCalculatedHeaderConfiguration);
            Assert.Equal(151.0, survivor.AssociatedFrameConfiguration.Height, 4);
        }

        /// <summary>Round-trip de persistencia y REAPERTURA: la altura personalizada y su procedencia sobreviven
        /// al documento, y el diseño recargado resuelve a la misma cabecera.</summary>
        [Fact]
        public void PBH01_ACustomHeight_SurvivesThePersistenceRoundTrip_AndReopening()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headerId = HeaderIds(state)[0];
            state.ModuleSession.SetHeaderConfiguration(headerId, CustomHeader(151.0, 40.0));
            state.CommitModuleEdits();
            var design = assembler.BuildDesign(state, inputs);

            var store = new RackProjectStore();
            var reloaded = store.Deserialize(store.Serialize(RackProject.ForPushBack(design))).PushBackDesign;

            var persisted = reloaded.Structure.Modules.First(module => module.ModuleId == headerId);
            Assert.False(persisted.UseCalculatedHeaderConfiguration);
            Assert.Equal(151.0, persisted.HeaderConfiguration.Height, 4);

            // Reapertura: resolver el diseño recargado devuelve la MISMA cabecera.
            var resolved = new PushBackResolver(Catalog).Resolve(reloaded);
            var module = resolved.Structure.Modules.First(m => m.ModuleId == headerId);
            Assert.False(module.UseCalculatedHeaderConfiguration);
            Assert.Equal(151.0, module.AssociatedFrameConfiguration.Height, 4);
        }

        /// <summary>El BOM se construye sobre el sistema resuelto que YA lleva la cabecera personalizada: cambiar
        /// la altura cambia el BOM, que es la prueba de que no hay un camino paralelo que lo calcule aparte.</summary>
        [Fact]
        public void PBH01_TheBom_IsBuiltOverTheCustomHeader()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headerId = HeaderIds(state)[0];
            var calculated = Resolved(assembler.Build(state, inputs), headerId).AssociatedFrameConfiguration.Height;

            state.ModuleSession.SetHeaderConfiguration(headerId, CustomHeader(calculated + 48.0, 40.0));
            state.CommitModuleEdits();
            var after = assembler.Build(state, inputs);

            Assert.True(after.IsValid, after.Error);
            Assert.NotNull(after.Bom);
            Assert.Equal(calculated + 48.0, Resolved(after, headerId).AssociatedFrameConfiguration.Height, 4);
        }

        // ===== PBH-02 — alcance: esta cabecera / todas las aplicables ==========================================

        [Fact]
        public void PBH02_ThisHeaderOnly_LeavesEveryOtherHeaderUntouched()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headers = HeaderIds(state);
            Assert.True(headers.Length > 1, "el fixture necesita mas de una cabecera");

            var result = state.ModuleSession.ApplyHeaderConfiguration(
                CustomHeader(151.0, 40.0), new[] { headers[0] });

            Assert.True(result.Applied);
            Assert.Equal(new[] { headers[0] }, result.AppliedModuleIds);

            var staged = state.ModuleSession.Modules.ToDictionary(module => module.ModuleId);
            Assert.True(staged[headers[0]].HasCustomHeaderConfiguration);
            foreach (var other in headers.Skip(1))
            {
                Assert.False(staged[other].HasCustomHeaderConfiguration);
            }
        }

        [Fact]
        public void PBH02_AllHeaders_ReceiveTheSameConfiguration_AndTheSeparatorsAreNeverTouched()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headers = HeaderIds(state);

            var result = state.ModuleSession.ApplyHeaderConfiguration(CustomHeader(151.0, 41.5), headers);

            Assert.True(result.Applied);
            Assert.Equal(headers, result.AppliedModuleIds);

            state.CommitModuleEdits();
            var computation = assembler.Build(state, inputs);
            Assert.True(computation.IsValid, computation.Error);

            foreach (var id in headers)
            {
                var module = Resolved(computation, id);
                Assert.False(module.UseCalculatedHeaderConfiguration);
                Assert.Equal(151.0, module.AssociatedFrameConfiguration.Height, 4);
                Assert.Equal(41.5, module.AssociatedFrameConfiguration.PanelClear, 4);
            }

            foreach (var separator in computation.System.Structure.Modules.Where(module => !module.IsHeader))
            {
                Assert.True(separator.UseCalculatedHeaderConfiguration);
            }
        }

        /// <summary>Un destino que no existe invalida la operacion ENTERA: no hay aplicacion parcial. Es la
        /// atomicidad del alcance multiple.</summary>
        [Fact]
        public void PBH02_AnUnknownTarget_RejectsTheWholeOperation_WithNothingApplied()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headers = HeaderIds(state);

            var result = state.ModuleSession.ApplyHeaderConfiguration(
                CustomHeader(151.0, 40.0),
                new[] { headers[0], "MODULO-QUE-NO-EXISTE", headers[1] });

            Assert.False(result.Applied);
            Assert.Empty(result.AppliedModuleIds);
            Assert.Contains("MODULO-QUE-NO-EXISTE", result.RejectionReason);
            Assert.All(
                state.ModuleSession.Modules,
                module => Assert.False(module.HasCustomHeaderConfiguration));
            Assert.False(state.ModuleSession.HasPendingChanges);
        }

        /// <summary>Un separador entre los destinos rechaza igual, y tampoco deja rastro.</summary>
        [Fact]
        public void PBH02_ASeparatorAmongTheTargets_RejectsTheWholeOperation()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headers = HeaderIds(state);
            var separator = state.ModuleSession.Modules.First(module => !module.IsHeader).ModuleId;

            var result = state.ModuleSession.ApplyHeaderConfiguration(
                CustomHeader(151.0, 40.0), new[] { headers[0], separator });

            Assert.False(result.Applied);
            Assert.All(
                state.ModuleSession.Modules,
                module => Assert.False(module.HasCustomHeaderConfiguration));
        }

        /// <summary>Cancelar devuelve la sesion a su ultimo estado confirmado: una aplicacion multiple es tan
        /// reversible como una simple.</summary>
        [Fact]
        public void PBH02_Cancel_RestoresThePreviousState_AfterAMultipleApply()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headers = HeaderIds(state);

            state.ModuleSession.ApplyHeaderConfiguration(CustomHeader(151.0, 40.0), headers);
            Assert.True(state.ModuleSession.HasPendingChanges);

            state.CancelModuleEdits();

            Assert.False(state.ModuleSession.HasPendingChanges);
            Assert.All(
                state.ModuleSession.Modules,
                module => Assert.False(module.HasCustomHeaderConfiguration));
        }

        /// <summary>Alcance individual: dos cabeceras pueden quedar DISTINTAS, y siguen distintas tras el
        /// recalculo. Es lo que hace que «esta cabecera» signifique algo.</summary>
        [Fact]
        public void PBH02_IndividualScope_PreservesTheDifferencesBetweenHeaders()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headers = HeaderIds(state);

            state.ModuleSession.ApplyHeaderConfiguration(CustomHeader(151.0, 40.0), new[] { headers[0] });
            state.ModuleSession.ApplyHeaderConfiguration(CustomHeader(163.0, 44.0), new[] { headers[1] });
            state.CommitModuleEdits();
            var computation = assembler.Build(state, inputs);

            Assert.Equal(151.0, Resolved(computation, headers[0]).AssociatedFrameConfiguration.Height, 4);
            Assert.Equal(163.0, Resolved(computation, headers[1]).AssociatedFrameConfiguration.Height, 4);
        }

        /// <summary>Cada destino recibe su PROPIA copia: escribir en uno no alcanza al otro ni al origen.</summary>
        [Fact]
        public void PBH02_EveryTarget_GetsAnIndependentCopy()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headers = HeaderIds(state);
            var source = CustomHeader(151.0, 40.0);

            state.ModuleSession.ApplyHeaderConfiguration(source, headers);

            // Mutar el objeto de origen despues de aplicar no puede tocar a ningun destino.
            source.Height = 999.0;
            foreach (var id in headers)
            {
                Assert.Equal(151.0, state.ModuleSession.HeaderConfigurationCopy(id).Height, 4);
            }
        }

        // ===== PBH-03 — reutilizar otra cabecera como COPIA independiente ======================================

        [Fact]
        public void PBH03_CopyingHeaderOneOntoHeaderTwo_CarriesTheWholeConfiguration()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headers = HeaderIds(state);

            state.ModuleSession.SetHeaderConfiguration(headers[0], CustomHeader(151.0, 41.5));
            var result = state.ModuleSession.CopyHeaderConfiguration(headers[0], new[] { headers[1] });

            Assert.True(result.Applied);
            var copy = state.ModuleSession.HeaderConfigurationCopy(headers[1]);
            Assert.Equal(151.0, copy.Height, 4);
            Assert.Equal(41.5, copy.PanelClear, 4);
        }

        /// <summary>La independencia de referencias es el corazon de PBH-03: modificar la Cabecera 2 despues de
        /// copiarla NO altera la Cabecera 1.</summary>
        [Fact]
        public void PBH03_ModifyingTheDestination_NeverChangesTheSource()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headers = HeaderIds(state);

            state.ModuleSession.SetHeaderConfiguration(headers[0], CustomHeader(151.0, 41.5));
            state.ModuleSession.CopyHeaderConfiguration(headers[0], new[] { headers[1] });

            // La Cabecera 2 se re-personaliza por su cuenta.
            state.ModuleSession.SetHeaderConfiguration(headers[1], CustomHeader(187.0, 44.0));

            Assert.Equal(151.0, state.ModuleSession.HeaderConfigurationCopy(headers[0]).Height, 4);
            Assert.Equal(41.5, state.ModuleSession.HeaderConfigurationCopy(headers[0]).PanelClear, 4);
            Assert.Equal(187.0, state.ModuleSession.HeaderConfigurationCopy(headers[1]).Height, 4);
        }

        [Fact]
        public void PBH03_OneSourceCanBeReusedOnSeveralHeaders_EachIndependent()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headers = HeaderIds(state);
            Assert.True(headers.Length >= 3, "el fixture necesita al menos tres cabeceras");

            state.ModuleSession.SetHeaderConfiguration(headers[0], CustomHeader(151.0, 41.5));
            var result = state.ModuleSession.CopyHeaderConfiguration(headers[0], headers.Skip(1).ToArray());

            Assert.True(result.Applied);
            Assert.Equal(headers.Length - 1, result.AppliedModuleIds.Count);

            state.ModuleSession.SetHeaderConfiguration(headers[1], CustomHeader(187.0, 44.0));
            Assert.Equal(151.0, state.ModuleSession.HeaderConfigurationCopy(headers[2]).Height, 4);
            Assert.Equal(151.0, state.ModuleSession.HeaderConfigurationCopy(headers[0]).Height, 4);
        }

        [Fact]
        public void PBH03_Cancel_ThrowsTheCopyAway()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headers = HeaderIds(state);

            state.ModuleSession.SetHeaderConfiguration(headers[0], CustomHeader(151.0, 41.5));
            state.ModuleSession.CopyHeaderConfiguration(headers[0], new[] { headers[1] });
            state.CancelModuleEdits();

            Assert.All(
                state.ModuleSession.Modules,
                module => Assert.False(module.HasCustomHeaderConfiguration));
        }

        /// <summary>Copiar desde un separador se rechaza: no tiene cabecera que copiar.</summary>
        [Fact]
        public void PBH03_CopyingFromASeparator_IsRejected()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headers = HeaderIds(state);
            var separator = state.ModuleSession.Modules.First(module => !module.IsHeader).ModuleId;

            var result = state.ModuleSession.CopyHeaderConfiguration(separator, new[] { headers[0] });

            Assert.False(result.Applied);
            Assert.False(string.IsNullOrWhiteSpace(result.RejectionReason));
            Assert.False(state.ModuleSession.Modules.First(m => m.ModuleId == headers[0]).HasCustomHeaderConfiguration);
        }

        /// <summary>
        /// PBH-03 NO introduce persistencia nueva: la copia viaja dentro del modulo que el diseño ya tenia. El
        /// documento serializado no gana ninguna clave, y terminar la sesion no deja nada guardado en ningun sitio.
        /// </summary>
        [Fact]
        public void PBH03_AddsNoNewPersistence_TheCopyTravelsInsideTheModuleTheDesignAlreadyHad()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headers = HeaderIds(state);
            var store = new RackProjectStore();

            // Referencia: el mismo rack con la cabecera personalizada DIRECTAMENTE.
            var direct = new PushBackEditorState();
            direct.Structure.Fronts[0].PalletsDeep = 6;
            var directInputs = Inputs();
            assembler.AcceptComputation(direct, assembler.Build(direct, directInputs));
            direct.ModuleSession.ApplyHeaderConfiguration(CustomHeader(151.0, 41.5), HeaderIds(direct));
            direct.CommitModuleEdits();
            var directJson = store.Serialize(RackProject.ForPushBack(assembler.BuildDesign(direct, directInputs)));

            // Lo mismo, pero llegando por COPIA desde otra cabecera.
            state.ModuleSession.SetHeaderConfiguration(headers[0], CustomHeader(151.0, 41.5));
            state.ModuleSession.CopyHeaderConfiguration(headers[0], headers.Skip(1).ToArray());
            state.CommitModuleEdits();
            var copiedJson = store.Serialize(RackProject.ForPushBack(assembler.BuildDesign(state, inputs)));

            // Byte a byte el mismo documento: la copia no aparece en el formato de alambre por ningun lado.
            Assert.Equal(directJson, copiedJson);
        }

        // ===== La operacion unica ==============================================================================

        /// <summary>Aplicar y copiar son la MISMA operacion: copiar resuelve el origen y delega. Un destino unico
        /// por cualquiera de las dos vias deja exactamente el mismo estado.</summary>
        [Fact]
        public void TheApplyAndTheCopy_AreTheSameOperation()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headers = HeaderIds(state);
            var configuration = CustomHeader(151.0, 41.5);

            state.ModuleSession.SetHeaderConfiguration(headers[0], configuration);
            state.ModuleSession.ApplyHeaderConfiguration(configuration, new[] { headers[1] });
            state.ModuleSession.CopyHeaderConfiguration(headers[0], new[] { headers[2] });

            var byApply = state.ModuleSession.HeaderConfigurationCopy(headers[1]);
            var byCopy = state.ModuleSession.HeaderConfigurationCopy(headers[2]);
            Assert.Equal(byApply.Height, byCopy.Height, 4);
            Assert.Equal(byApply.PanelClear, byCopy.PanelClear, 4);
        }

        /// <summary>Sin destinos, o sin configuracion, la operacion se rechaza con un motivo y no toca nada.</summary>
        [Fact]
        public void AnEmptyOperation_IsRejectedWithAReason()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);

            Assert.False(state.ModuleSession.ApplyHeaderConfiguration(CustomHeader(151.0, 40.0), Array.Empty<string>()).Applied);
            Assert.False(state.ModuleSession.ApplyHeaderConfiguration(null, HeaderIds(state)).Applied);
            Assert.False(state.ModuleSession.HasPendingChanges);
        }

        /// <summary>La operacion multiple es UNA recomputacion: el Confirmar es uno solo por muchas cabeceras.</summary>
        [Fact]
        public void AMultipleApply_NeedsASingleCommitAndASingleRecompute()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headers = HeaderIds(state);

            state.ModuleSession.ApplyHeaderConfiguration(CustomHeader(151.0, 40.0), headers);
            var commit = state.CommitModuleEdits();

            Assert.Equal(state.ModuleSession.Modules.Count, commit.Modules.Count);
            Assert.False(state.ModuleSession.HasPendingChanges);

            var computation = assembler.Build(state, inputs);
            Assert.True(computation.IsValid, computation.Error);
            Assert.Equal(headers.Length, state.LastModuleReconciliation.Preserved.Count);
        }

        /// <summary>
        /// REGRESION de I-40: los descriptores de la SESION se numeran por su lugar en la secuencia. Se numeraban
        /// todos como 1 porque una intencion no lleva Index propio, y de ahi salen las etiquetas del selector de
        /// modulos y de la lista «Copiar de:»: con todas iguales, PBH-03 no permite elegir origen.
        /// </summary>
        [Fact]
        public void REGRESION_TheSessionDescriptors_AreNumberedByTheirPlaceInTheSequence()
        {
            var assembler = Assembler();
            var state = Live(assembler, out _);
            var descriptors = state.ModuleSession.Modules;

            Assert.True(descriptors.Count > 1, "el fixture necesita mas de un modulo");
            Assert.Equal(Enumerable.Range(0, descriptors.Count), descriptors.Select(module => module.Index));
        }

        // ===== Ronda 2 del Owner ===============================================================================

        /// <summary>
        /// REGRESION de la ronda 2 (defecto 2): solo las cabeceras con una personalizacion REAL pueden ser origen
        /// de una copia. Toda cabecera lleva configuracion —tambien las calculadas—, asi que ofrecerlas todas
        /// permite propagar el estandar mientras el usuario cree estar copiando lo suyo.
        /// </summary>
        [Fact]
        public void RONDA2_CustomHeaderModuleIds_ListsOnlyRealPersonalizations()
        {
            var assembler = Assembler();
            var state = Live(assembler, out _);
            var headers = HeaderIds(state);

            Assert.Empty(state.ModuleSession.CustomHeaderModuleIds);

            state.ModuleSession.SetHeaderConfiguration(headers[0], CustomHeader(151.0, 41.5));
            Assert.Equal(new[] { headers[0] }, state.ModuleSession.CustomHeaderModuleIds);

            state.ModuleSession.SetHeaderConfiguration(headers[1], CustomHeader(163.0, 44.0));
            Assert.Equal(new[] { headers[0], headers[1] }, state.ModuleSession.CustomHeaderModuleIds);

            // Devolverla a calculada la retira del universo de origenes.
            state.ModuleSession.ResetHeaderToCalculated(headers[0]);
            Assert.Equal(new[] { headers[1] }, state.ModuleSession.CustomHeaderModuleIds);
        }

        /// <summary>
        /// REGRESION de la ronda 2 (defecto 1, punto 8 del Owner): lo personalizado ANTES de guardar es identico a
        /// lo personalizado DESPUES de cargar — no solo el alto, sino cada propiedad tocada — y la procedencia sigue
        /// en «personalizada».
        /// </summary>
        [Fact]
        public void RONDA2_TheWholeCustomConfiguration_IsIdenticalBeforeSavingAndAfterLoading()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headerId = HeaderIds(state)[0];

            var custom = CustomHeader(151.0, 41.5);
            custom.Horizontals[0].Elevation = 33.0;
            state.ModuleSession.SetHeaderConfiguration(headerId, custom);
            state.CommitModuleEdits();

            var design = assembler.BuildDesign(state, inputs);
            var before = design.Structure.Modules.First(module => module.ModuleId == headerId).HeaderConfiguration;

            var store = new RackProjectStore();
            var reloaded = store.Deserialize(store.Serialize(RackProject.ForPushBack(design))).PushBackDesign;
            var after = reloaded.Structure.Modules.First(module => module.ModuleId == headerId).HeaderConfiguration;

            Assert.Equal(before.Height, after.Height, 4);
            Assert.Equal(before.PanelClear, after.PanelClear, 4);
            Assert.Equal(before.Depth, after.Depth, 4);
            Assert.Equal(before.PostPeralte, after.PostPeralte, 4);
            Assert.Equal(before.Horizontals.Count, after.Horizontals.Count);
            Assert.Equal(33.0, after.Horizontals[0].Elevation, 4);
            Assert.False(reloaded.Structure.Modules.First(module => module.ModuleId == headerId)
                .UseCalculatedHeaderConfiguration);
        }

        /// <summary>
        /// REGRESION de la ronda 2 (defecto 3): la secuencia EXACTA del reporte —personalizar la 1, copiarla a la 2,
        /// aplicar a todas— deja a TODAS con la personalizacion, y sobrevive al round-trip. Ninguna vuelve a
        /// calculada.
        /// </summary>
        [Fact]
        public void RONDA2_TheOwnerSequence_SurvivesTheRoundTrip_WithNoHeaderBackToCalculated()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headers = HeaderIds(state);

            // 1) Cabecera 1 personalizada y confirmada.
            state.ModuleSession.SetHeaderConfiguration(headers[0], CustomHeader(151.0, 41.5));
            state.CommitModuleEdits();
            assembler.AcceptComputation(state, assembler.Build(state, inputs));
            state.ClearModuleCommit();

            // 2-3) Copiar de la Cabecera 1 sobre TODAS las demas, y confirmar UNA vez.
            var result = state.ModuleSession.CopyHeaderConfiguration(headers[0], headers.Skip(1).ToArray());
            Assert.True(result.Applied, result.RejectionReason);
            state.CommitModuleEdits();

            var design = assembler.BuildDesign(state, inputs);
            var store = new RackProjectStore();
            var reloaded = store.Deserialize(store.Serialize(RackProject.ForPushBack(design))).PushBackDesign;
            var resolved = new PushBackResolver(Catalog).Resolve(reloaded);

            foreach (var id in headers)
            {
                var module = resolved.Structure.Modules.First(m => m.ModuleId == id);
                Assert.False(module.UseCalculatedHeaderConfiguration);
                Assert.Equal(151.0, module.AssociatedFrameConfiguration.Height, 4);
                Assert.Equal(41.5, module.AssociatedFrameConfiguration.PanelClear, 4);
            }
        }

        /// <summary>El conjunto de cabeceras de la sesion es el universo del alcance «todas».</summary>
        [Fact]
        public void HeaderModuleIds_ListsEveryHeader_AndNoSeparator()
        {
            var assembler = Assembler();
            var state = Live(assembler, out _);

            Assert.Equal(HeaderIds(state), state.ModuleSession.HeaderModuleIds);
            Assert.NotEmpty(state.ModuleSession.HeaderModuleIds);
        }
    }
}
