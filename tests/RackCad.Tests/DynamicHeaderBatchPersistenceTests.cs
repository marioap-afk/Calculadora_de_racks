using System.Globalization;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Dynamic;
using Xunit;
using F = RackCad.Tests.DynamicHeaderBatchFixtures;

namespace RackCad.Tests
{
    /// <summary>
    /// I-53 G6 — lo que una distribucion del Dinamico deja en el rack frente a lo que ya existe: los overrides por linea que
    /// el Dinamico no escribe y el BOM por la ruta de <c>RACKBOMTOTAL</c> (Proposal V2 §7.10, §7.13; matriz §13.4: D-23 y
    /// D-24). Sin DTO ni campo nuevo: las copias caen en <c>Modules[].Header</c> + procedencia, ya persistidos.
    /// </summary>
    public class DynamicHeaderBatchPersistenceTests
    {
        private static DynamicRackDesign DesignWithLineOverrides()
        {
            var design = F.Design();
            design.HeaderLineOverrides.Add(new DynamicHeaderLineOverride { PostIndex = 1, ModuleId = "M3", Header = F.Custom(231.0) });
            design.DerivedPostLineOverrides.Add(new DynamicDerivedPostLineOverride { PostIndex = 0, Height = 90.0 });
            return design;
        }

        private static string Lineas(DynamicRackSystem system)
            => string.Join("|", system.HeaderLineOverrides.Select(line =>
                   line.PostIndex.ToString(CultureInfo.InvariantCulture) + "/" + line.ModuleId + "/"
                   + SelectiveHeaderBatchFixtures.Configuration(line.Header)))
               + "#" + string.Join("|", system.DerivedPostLineOverrides.Select(derived =>
                   derived.PostIndex.ToString(CultureInfo.InvariantCulture) + "/" + derived.Height.ToString("R", CultureInfo.InvariantCulture)));

        // ===== D-23 — overrides por linea ================================================================================

        [Fact]
        public void D23_UN_OVERRIDE_POR_LINEA_ENTRANTE_SOBREVIVE_INTACTO_A_DISTRIBUTE_Y_A_EDIT_E_I53_NO_ESCRIBE_NINGUNO()
        {
            var editor = new F.Editor(DesignWithLineOverrides());
            var linea = Assert.Single(editor.System.HeaderLineOverrides);           // premisa: el resolver lo trajo
            var derivado = Assert.Single(editor.System.DerivedPostLineOverrides);
            var cabeceraDeLinea = linea.Header;
            var huella = Lineas(editor.System);

            editor.Customize("M1", 232.0);
            F.Committed(editor.Apply(editor.Prepare(editor.Distribute("M1", F.All()))));
            AssertIntactos();

            F.Committed(editor.Apply(editor.Prepare(F.Request(DynamicHeaderBatchRequest.Edit(F.Custom(233.0, depth: 60.0), "M3")))));
            AssertIntactos();

            // Sin overrides entrantes, ninguna operacion de I-53 crea uno.
            var limpio = new F.Editor(F.Design());
            limpio.Customize("M1", 234.0);
            F.Committed(limpio.Apply(limpio.Prepare(limpio.Distribute("M1", F.All()))));
            F.Committed(limpio.Apply(limpio.Prepare(F.Request(DynamicHeaderBatchRequest.Edit(F.Custom(235.0, depth: 60.0), "M5")))));
            Assert.Empty(limpio.System.HeaderLineOverrides);
            Assert.Empty(limpio.System.DerivedPostLineOverrides);

            void AssertIntactos()
            {
                Assert.Same(linea, Assert.Single(editor.System.HeaderLineOverrides));
                Assert.Same(derivado, Assert.Single(editor.System.DerivedPostLineOverrides));
                Assert.Same(cabeceraDeLinea, linea.Header);
                Assert.Equal(huella, Lineas(editor.System));
            }
        }

        /// <summary>
        /// CARACTERIZACION del comportamiento historico del Dinamico, sin cambiarlo (§7.10): el sistema reconstruido nace sin
        /// overrides por linea y el diseno los toma de el. No afirma que sea correcto, no es una golden de L-2 de Push Back y
        /// no convierte el Dinamico a overrides por linea.
        /// </summary>
        [Fact]
        public void D23_CARACTERIZACION_UNA_RECONSTRUCCION_DEL_DINAMICO_HOY_PIERDE_LOS_OVERRIDES_POR_LINEA()
        {
            var editor = new F.Editor(DesignWithLineOverrides());
            Assert.Single(editor.System.HeaderLineOverrides);
            Assert.Single(editor.System.DerivedPostLineOverrides);

            var result = editor.Rebuild(palletDepth: 40.0);

            Assert.Empty(result.System.HeaderLineOverrides);
            Assert.Empty(result.System.DerivedPostLineOverrides);
            var design = editor.Design();
            Assert.Empty(design.HeaderLineOverrides);
            Assert.Empty(design.DerivedPostLineOverrides);
        }

        // ===== D-24 — BOM del editor = ruta de RACKBOMTOTAL ==============================================================

        /// <summary>La ruta de <c>DynamicKindHandler.BuildBom</c>: store -> registro (Deserialize) -> resolver -> SystemBomBuilder.</summary>
        private static string BomPorRackBomTotal(F.Editor editor)
        {
            var store = new RackProjectStore();
            var project = store.Deserialize(store.Serialize(RackProject.ForDynamic(editor.Design())));
            Assert.NotNull(project?.DynamicDesign);
            return F.BomSignature(new DynamicRackSystemResolver(F.Catalog).Resolve(project.DynamicDesign).System);
        }

        private static RackFrameConfiguration Receta() => F.Custom(241.0, depth: 54.0, height: 150.0);

        [Fact]
        public void D24_GUARDA_EL_BOM_DEL_EDITOR_CON_UNA_PERSONALIZADA_HISTORICA_ES_EL_DE_LA_RUTA_DE_RACKBOMTOTAL()
        {
            var editor = new F.Editor(F.Design());
            editor.Customize("M1", Receta());

            Assert.Equal(F.BomSignature(editor.System), BomPorRackBomTotal(editor));
        }

        [Fact]
        public void D24_EL_BOM_DEL_EDITOR_CON_PERSONALIZADAS_DISTRIBUIDAS_ES_EL_DE_LA_RUTA_DE_RACKBOMTOTAL_CON_MIEMBROS()
        {
            var sinDistribuir = new F.Editor(F.Design());
            sinDistribuir.Customize("M1", Receta());

            var editor = new F.Editor(F.Design());
            editor.Customize("M1", Receta());
            var committed = F.Committed(editor.Apply(editor.Prepare(editor.Distribute("M1", F.All()))));
            Assert.All(F.Ids(committed.Applied), id => Assert.NotEmpty(editor.Module(id).AssociatedFrameConfiguration.Members));

            var bomDelEditor = F.BomSignature(editor.System);
            Assert.NotEqual(F.BomSignature(sinDistribuir.System), bomDelEditor);   // las distribuidas SI se cotizan
            Assert.Equal(bomDelEditor, BomPorRackBomTotal(editor));
        }
    }
}
