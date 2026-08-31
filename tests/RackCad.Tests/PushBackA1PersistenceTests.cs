using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (A1) — LA FRONTERA DE PERSISTENCIA DE LA SEGURIDAD.
    ///
    /// <para>
    /// Dos defectos alcanzables en produccion, los dos por la misma razon: un valor DERIVADO viajaba al documento
    /// como si fuera una decision del usuario.
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <b>B1.</b> Un rack compuesto se puede guardar sin abrir «Elementos de seguridad». La autoridad colapsa el
    /// lado al extremo bajo —«Ambas» se vuelve «Izquierda»— y ESO era lo unico que se guardaba: al reabrir, el
    /// documento parecia antiguo con una decision global explicita y el lado B perdia sus botas.
    /// </item>
    /// <item>
    /// <b>H8.</b> Los PASILLOS de carga que el rack tiene ahora se escribian en la matriz por poste, que tambien es
    /// intencion persistible: degradar un compuesto a un solo sentido dejaba un «Derecha» rancio que mandaba el
    /// desviador al extremo alto.
    /// </item>
    /// </list>
    /// </summary>
    public class PushBackA1PersistenceTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static string BootId => Catalog.SafetyElements
            .First(entry => SelectiveSafetyDefaults.IsType(entry.Type, SelectiveSafetyDefaults.BotaType)).Id;

        private static string DiverterId => Catalog.SafetyElements
            .First(entry => SelectiveSafetyDefaults.IsType(entry.Type, SelectiveSafetyDefaults.DesviadorType)).Id;

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

        private static PushBackCompositeEditorState State(bool composite, int slots = 3)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSlotCount(slots);
            if (composite)
            {
                state.SetSideBPresent(true);
                state.SideB.LoadNew();
                state.SetSlotCount(slots);
                for (var slot = 0; slot < slots; slot++)
                {
                    state.SetSlotPresent(PushBackSide.B, slot, true);
                }
            }

            return state;
        }

        /// <summary>El diseño que un rack recien montado guardaria, SIN pasar por «Elementos de seguridad».</summary>
        private static PushBackDesign Untouched(bool composite)
            => new PushBackCompositeEditorAssembler(Catalog).Build(State(composite), Inputs(), Catalog).Design;

        /// <summary>El viaje real al documento y de vuelta, con el JSON de por medio.</summary>
        private static PushBackDesign RoundTrip(PushBackDesign design)
            => JsonSerializer
                .Deserialize<PushBackDesignDocument>(JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(design)))
                .ToDomain();

        private static IReadOnlyList<ResolvedBoot> Boots(PushBackDesign design)
            => PushBackBootPlan.Resolve(new PushBackResolver(Catalog).Resolve(design), Catalog);

        private static IReadOnlyList<HeaderBlockInstance> Diverters(PushBackSystem system, PushBackFrontalEnd end)
            => new PushBackSystemFrontalBuilder().BuildPlan(system, Catalog, end).Flatten().Instances
                .Where(instance => string.Equals(instance.PieceId, DiverterId, StringComparison.OrdinalIgnoreCase))
                .ToList();

        // ==================================================================== B1

        /// <summary>
        /// B1 — un compuesto que nadie toco sobrevive el viaje al documento con las botas de LOS DOS lados. Antes
        /// volvia con el lado colapsado y las de B desaparecian del dibujo y del BOM.
        /// </summary>
        [Fact]
        public void UntouchedComposite_SurvivesDocumentRoundTrip_WithSideBBoots()
        {
            var design = Untouched(composite: true);
            var before = Boots(design);
            var after = Boots(RoundTrip(design));

            Assert.NotEmpty(before);
            Assert.Contains(before, boot => boot.Side == PushBackSide.B);
            Assert.Equal(
                before.Select(boot => boot.Identity).OrderBy(value => value, StringComparer.Ordinal),
                after.Select(boot => boot.Identity).OrderBy(value => value, StringComparer.Ordinal));
        }

        /// <summary>Y el rack de un solo sentido sigue viajando exactamente igual que siempre.</summary>
        [Fact]
        public void UntouchedSimple_SurvivesDocumentRoundTrip()
        {
            var design = Untouched(composite: false);

            Assert.Equal(
                Boots(design).Select(boot => boot.Identity).OrderBy(value => value, StringComparer.Ordinal),
                Boots(RoundTrip(design)).Select(boot => boot.Identity).OrderBy(value => value, StringComparer.Ordinal));
        }

        /// <summary>
        /// BITE A — volver a guardar el lado COLAPSADO como si fuera del usuario rompe justo esto: el documento
        /// resultante se lee como una decision global y el lado B se queda sin botas.
        /// </summary>
        [Fact]
        public void Bite_PersistingTheCollapsedSide_LosesSideBBoots()
        {
            var document = PushBackDesignDocument.FromDomain(Untouched(composite: true));
            var restored = document.ToDomain();
            var boot = restored.Structure.SafetySelections
                .Single(selection => string.Equals(selection.ElementId, BootId, StringComparison.OrdinalIgnoreCase));

            Assert.NotNull(boot.AuthoredSide);                       // se guarda lo que el usuario tenia…
            Assert.Equal(SafetySide.Both, boot.AuthoredSide.Value);   // …que es el valor sin colapsar

            boot.AuthoredSide = null;   // lo que hacia el documento anterior a A1
            Assert.Equal(SafetySide.Left, boot.Side);
            Assert.Empty(
                PushBackBootPlan.Resolve(new PushBackResolver(Catalog).Resolve(restored), Catalog)
                    .Where(resolved => resolved.Side == PushBackSide.B));
        }

        // ==================================================================== H8

        /// <summary>
        /// H8 — los pasillos de carga que el rack tiene AHORA no se guardan como intencion del usuario: el
        /// documento conserva la matriz tal y como el usuario la dejo.
        /// </summary>
        [Fact]
        public void DerivedLoadingAisles_AreNotPersistedAsUserIntent()
        {
            var design = Untouched(composite: true);
            var system = new PushBackResolver(Catalog).Resolve(design);
            var live = system.SafetySelections
                .Single(selection => string.Equals(selection.ElementId, DiverterId, StringComparison.OrdinalIgnoreCase));

            Assert.NotEmpty(live.PostSides);        // el dibujo SI ve los pasillos…
            Assert.NotEmpty(live.DerivedAisles);    // …declarados como derivados

            var snapshot = new PushBackResolver(Catalog).Snapshot(system);
            var stored = RoundTrip(snapshot).Structure.SafetySelections
                .Single(selection => string.Equals(selection.ElementId, DiverterId, StringComparison.OrdinalIgnoreCase));

            Assert.Empty(stored.PostSides);         // …y el documento no los guarda
        }

        /// <summary>
        /// Y por eso degradar un compuesto a un solo sentido no deja un lado rancio: el desviador sigue en el
        /// extremo BAJO, que es el unico pasillo que le queda al rack.
        /// </summary>
        [Fact]
        public void CompositeToSimple_DoesNotMoveDiverterToHigh()
        {
            var composite = new PushBackResolver(Catalog).Resolve(Untouched(composite: true));
            var stored = RoundTrip(new PushBackResolver(Catalog).Snapshot(composite));

            // El mismo documento, ya sin lado B: es la degradacion que el usuario hace en la ventana.
            stored.SideB = null;
            stored.Composite = null;
            var simple = new PushBackResolver(Catalog).Resolve(stored);

            Assert.NotEmpty(Diverters(simple, PushBackFrontalEnd.EntradaSalida));
            Assert.Empty(Diverters(simple, PushBackFrontalEnd.Posterior));
        }
    }
}
