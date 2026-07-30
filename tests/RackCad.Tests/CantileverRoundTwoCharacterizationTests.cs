using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.StructuralSections;
using RackCad.Application.Systems.Cantilever;
using RackCad.Domain.Systems.Cantilever;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-37D ronda 2 — CARACTERIZACIÓN PREVIA al refactor visual.
    ///
    /// El dueño rechazó la ronda 1 por seis motivos, y **los seis son de arquitectura visual y de flujo**:
    /// ninguno dice que la línea resuelva mal. Así que antes de mover una sola ventana, esta suite fija lo
    /// que el refactor NO puede cambiar: las resoluciones, el BOM, la persistencia, la identidad y el
    /// contenido FÍSICO de los tres planes.
    ///
    /// <para><b>Qué se fija y qué no.</b> Las firmas de vista se toman <b>excluyendo los troqueles</b>. No es
    /// un descuido: el motivo 5 del rechazo es justamente que los troqueles y las placas no se dibujan, así
    /// que la ronda 2 los AÑADE. Un pin que incluyera el conjunto completo se rompería por la corrección y
    /// no distinguiría «arreglé lo que había que arreglar» de «rompí lo que no debía tocar». Excluyéndolos,
    /// estos pines dicen exactamente eso: todo lo demás siguió igual.</para>
    ///
    /// <para>La <b>distribución visual rechazada no se congela</b> en ningún golden. Aquí no hay una sola
    /// aserción sobre nombres de controles, orden de campos ni layout: todo es modelo puro.</para>
    /// </summary>
    public class CantileverRoundTwoCharacterizationTests
    {
        private const string ColumnW = "AISC-W-W10X33";
        private const string BaseW = "AISC-W-W12X26";
        private const string ArmHss = "AISC-HSS-RECT-HSS4X4X_250";

        private static readonly StructuralSectionCatalog Catalog =
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        private static CantileverLineEditorAssembler Assembler() => new CantileverLineEditorAssembler(Catalog);

        /// <summary>The reference line: three stations, two levels, the approved defaults. Fixed GUID so the
        /// persistence pin is reproducible.</summary>
        internal static CantileverLineDesign Reference(
            CantileverStationFaceMode face = CantileverStationFaceMode.Single)
        {
            var design = new CantileverLineDesign
            {
                Id = Guid.Parse("11111111-2222-3333-4444-555555555555"),
                Name = "Caracterizacion",
                StationCount = 3,
                ColumnCentreSpacing = 96.0,
                StationTopology = new CantileverLineStationTopologyDesign
                {
                    FaceMode = face,
                    LevelCount = 2,
                    RequestedClearHeight = 24.0,
                    ColumnBaseTemplate = new CantileverStationColumnBaseTemplateDesign
                    {
                        ColumnSectionId = ColumnW,
                        Base = new CantileverBaseDesign { SectionId = BaseW, Length = 48.0 }
                    }
                },
                DefaultArmTemplate = new CantileverArmTemplateDesign
                {
                    Body = new CantileverArmBodyDesign { SectionId = ArmHss, CutLength = 36.0 },
                    MountingPlate = new CantileverArmMountingPlateTemplateDesign
                    {
                        VerticalPunchCount = 2,
                        VerticalEndOffset = 1.5
                    }
                }
            };

            var punches = design.StationTopology.ColumnBaseTemplate.Connection.Punches;
            punches.ColumnBottomPlateEndOffset = 1.5;
            punches.ColumnTopPunchOffset = 4.0;

            return design;
        }

        private static string Sha(string content) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content)));

        /// <summary>The physical content of a view, WITHOUT punches. See the class remarks.</summary>
        private static string ViewSignatureWithoutPunches(CantileverViewPlan plan) =>
            plan.View + ";s=" + plan.StationIndex + ";" + string.Join("|", plan.Curves
                .Where(c => c.Kind != CantileverViewPieceKind.Punch)
                .Select(c => c.Kind + ":" + c.PieceId.Value + ":" + c.Points.Count + ":" + (c.IsClosed ? "C" : "O")));

        private static string BomSignature(BillOfMaterials bom) =>
            string.Join("\n", bom.Components
                .Select(c => FormattableString.Invariant($"{c.Category}|{c.ProfileId}|{c.Length:0.####}|{c.Quantity}|{c.Pieces.Count}"))
                .OrderBy(s => s, StringComparer.Ordinal));

        private static CantileverViewPlan View(
            CantileverLineEditorComputation computation, CantileverViewKind view) =>
            computation.Views.Single(v => v.View == view);

        // ---- Pines FIJOS (SHA-256). Se regeneran SOLO ante un cambio intencionado y explicado -------------
        private static readonly IReadOnlyDictionary<string, string> Expected = new Dictionary<string, string>
        {
            // Tomados sobre 5a0d188, la punta que el dueño validó y rechazó. La ronda 2 es un refactor de
            // ARQUITECTURA VISUAL: si cualquiera de estos se mueve, el refactor cambió el producto y no sólo
            // su interfaz.
            ["linea"] = "35F179AA24C647488E76237C3A0F4D6D437F5D56A439B10B6E6A27A1F063F081",
            ["linea-doble"] = "E05B3F68FD4C6EA6E6FC30A9D7F2FAD14B252043EDB3B01077C428AF4BC22251",
            ["bom"] = "6088C538512ACDCA52D0C5DB5D0AD1DE180686CE13752D33F674F19C6DFA66F3",
            ["bom-doble"] = "608B790C28618CD013D8B807F937E3EF73FCF4BAF9B77D076E0CFE866FA5A272",
            ["frontal"] = "1A12F7495D9DBCE6FF5FD14B3ABD18F4EB10A78870397F8229C2636B2274629C",
            ["planta"] = "EFB547E6A9B591386307E424A02028F70898DCBE36112BB5BB3C32D7DE5F4266",
            ["lateral"] = "E26334E504741413419C6E777934B06209AE989F6202B37C8C33E149E5BB4343",
            ["frontal-doble"] = "7D74B31D65273817192792F2871DCB321227EAE8BE8BD2BFA41A14394A1B53AE",
            ["persistencia"] = "874066E2CEFE63A74AB03467B31D0B870D4774BBD2DAEDA50E084C06A910B952"
        };

        // ---- 1. Las resoluciones ------------------------------------------------------------------------

        [Fact]
        public void LaLineaResuelveExactamenteIgual()
        {
            var single = Assembler().Build(Reference());
            var doble = Assembler().Build(Reference(CantileverStationFaceMode.Double));

            Assert.Equal(Expected["linea"], Sha(single.Line.Signature()));
            Assert.Equal(Expected["linea-doble"], Sha(doble.Line.Signature()));
        }

        [Fact]
        public void ElBomEsElMismo()
        {
            var single = Assembler().Build(Reference());
            var doble = Assembler().Build(Reference(CantileverStationFaceMode.Double));

            Assert.Equal(Expected["bom"], Sha(BomSignature(single.Bom)));
            Assert.Equal(Expected["bom-doble"], Sha(BomSignature(doble.Bom)));

            // Y sus cuatro componentes siguen siendo cuatro, con estas cantidades.
            Assert.Equal(4, single.Bom.Components.Count);
            Assert.Equal(3, single.Bom.Components.Single(c => c.Category == "Columna con base").Quantity);
            Assert.Equal(6, single.Bom.Components.Single(c => c.Category == "Brazo").Quantity);
            Assert.Equal(12, doble.Bom.Components.Single(c => c.Category == "Brazo").Quantity);
        }

        // ---- 2. Los planes físicos, sin los troqueles que esta ronda añade -------------------------------

        [Fact]
        public void LosTresPlanesConservanSuContenidoFisico()
        {
            var single = Assembler().Build(Reference());

            Assert.Equal(Expected["frontal"], Sha(ViewSignatureWithoutPunches(View(single, CantileverViewKind.Frontal))));
            Assert.Equal(Expected["planta"], Sha(ViewSignatureWithoutPunches(View(single, CantileverViewKind.Planta))));
            Assert.Equal(Expected["lateral"], Sha(ViewSignatureWithoutPunches(View(single, CantileverViewKind.Lateral))));
        }

        [Fact]
        public void LaGondolaDobleConservaSuContenidoFisico()
        {
            var doble = Assembler().Build(Reference(CantileverStationFaceMode.Double));

            Assert.Equal(
                Expected["frontal-doble"],
                Sha(ViewSignatureWithoutPunches(View(doble, CantileverViewKind.Frontal))));
        }

        [Fact]
        public void LaLateralSigueSiendoDeUnaEstacionYSinArriostramiento()
        {
            var single = Assembler().Build(Reference());
            var lateral = View(single, CantileverViewKind.Lateral);

            Assert.Equal(0, lateral.StationIndex);
            Assert.Empty(lateral.Of(CantileverViewPieceKind.Separator));
            Assert.Empty(lateral.Of(CantileverViewPieceKind.Brace));
        }

        // ---- 3. El defecto 5, medido ---------------------------------------------------------------------

        [Fact]
        public void ElModeloYaTieneResueltosTodosSusTroqueles()
        {
            // La medida del motivo 5 del rechazo: los troqueles EXISTEN resueltos —204 en la línea sencilla,
            // 276 en la doble— y las placas de separador también. Lo que fallaba no era el modelo: era que la
            // representación no los pedía. Este número es el que la corrección tiene que hacer visible.
            var single = Assembler().Build(Reference());
            var doble = Assembler().Build(Reference(CantileverStationFaceMode.Double));

            Assert.Equal(204, single.Line.Stations.Sum(s => s.Station.Punches.Count));
            Assert.Equal(276, doble.Line.Stations.Sum(s => s.Station.Punches.Count));
            Assert.Equal(8, single.Line.SeparatorColumnPlates.Count);
            Assert.All(single.Line.SeparatorColumnPlates, p => Assert.True(p.Punch.Diameter > 0.0));
        }

        // ---- 4. Persistencia e identidad ------------------------------------------------------------------

        [Fact]
        public void LaPersistenciaEsLaMismaYElRoundTripEsDeterminista()
        {
            var store = new RackProjectStore();
            var json = store.Serialize(RackProject.ForCantilever(Reference()));

            Assert.Equal(Expected["persistencia"], Sha(json));

            var reloaded = store.Deserialize(json);
            var again = store.Serialize(RackProject.ForCantilever(reloaded.CantileverLineDesign));

            Assert.Equal(json, again);
            Assert.Equal(Reference().Id, reloaded.CantileverLineDesign.Id);
            Assert.Equal("Caracterizacion", reloaded.CantileverLineDesign.Name);
        }

        [Fact]
        public void LaIdentidadEsUnaPorLineaYDuplicarAcunaUnaNueva()
        {
            var design = Reference();
            var copy = design.DuplicateWithNewIdentity();

            Assert.NotEqual(design.Id, copy.Id);
            Assert.Equal(design.StationCount, copy.StationCount);

            // Y una re-resolución no toca la identidad.
            var computation = Assembler().Build(design);
            Assert.Equal(design.Id, computation.Line.Id);
        }

        // ---- 5. La matriz: una operación, un cambio agregado ----------------------------------------------

        [Fact]
        public void UnaOperacionDeMatrizProduceUnSoloCambioAgregado()
        {
            var design = Reference();
            var matrix = new CantileverLineArmMatrix(design);
            var arm = design.DefaultArmTemplate.DeepCopy();
            arm.Body.CutLength = 60.0;

            var change = matrix.Apply(
                CantileverLineApplyScope.Station, new CantileverLineCell(1, 0, CantileverArmSide.PositiveY), arm);

            Assert.Equal(2, change.Count);            // la estación entera: dos niveles × un lado
            Assert.Equal(2, change.Changed.Count);
            Assert.False(change.IsNoOp);
            Assert.Equal(2, matrix.OverrideCount);

            // Repetirla no mueve nada: el editor no vuelve a regenerar.
            var again = matrix.Apply(
                CantileverLineApplyScope.Station, new CantileverLineCell(1, 0, CantileverArmSide.PositiveY), arm);

            Assert.True(again.IsNoOp);
            Assert.Equal(2, matrix.OverrideCount);
        }

        [Fact]
        public void UnBrazoIgualAlDeOmisionNoSeGuardaComoExcepcion()
        {
            var design = Reference();
            var matrix = new CantileverLineArmMatrix(design);

            var change = matrix.Apply(
                CantileverLineApplyScope.Line,
                new CantileverLineCell(0, 0, CantileverArmSide.PositiveY),
                design.DefaultArmTemplate.DeepCopy());

            Assert.True(change.IsNoOp);
            Assert.Equal(0, matrix.OverrideCount);
            Assert.Empty(design.ArmCellOverrides);
        }

        [Fact]
        public void LosCuatroAlcancesAlcanzanExactamenteLoQueDicen()
        {
            var design = Reference(CantileverStationFaceMode.Double);
            var matrix = new CantileverLineArmMatrix(design);
            var anchor = new CantileverLineCell(1, 0, CantileverArmSide.PositiveY);

            Assert.Single(matrix.InScope(CantileverLineApplyScope.Cell, anchor));
            Assert.Equal(4, matrix.InScope(CantileverLineApplyScope.Station, anchor).Count);  // 2 niveles × 2 lados
            Assert.Equal(6, matrix.InScope(CantileverLineApplyScope.Level, anchor).Count);    // 3 estaciones × 2 lados
            Assert.Equal(6, matrix.InScope(CantileverLineApplyScope.Side, anchor).Count);     // 3 estaciones × 2 niveles
            Assert.Equal(12, matrix.InScope(CantileverLineApplyScope.Line, anchor).Count);
        }

        // ---- 6. La selección de secciones vigente ---------------------------------------------------------

        [Fact]
        public void LasSeccionesSeEligenPorIdExactoYElCatalogoLasOfrecePorFamilia()
        {
            var assembler = Assembler();

            Assert.Contains(assembler.SectionsOf(StructuralSectionFamily.W), s => s.SectionId.Value == ColumnW);
            Assert.Contains(assembler.SectionsOf(StructuralSectionFamily.W), s => s.SectionId.Value == BaseW);
            Assert.Contains(assembler.SectionsOf(StructuralSectionFamily.Channel), s => s.SectionId.Value == "AISC-C-C4X4_5");

            // Un id que no existe NO se resuelve por parecido: bloquea.
            var design = Reference();
            design.StationTopology.ColumnBaseTemplate.ColumnSectionId = "W10X33"; // designación, no id

            Assert.False(assembler.Build(design).IsValid);
        }
    }
}
