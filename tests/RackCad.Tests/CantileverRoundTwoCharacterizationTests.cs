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

        /// <summary>
        /// SHA-256 of the content, with the LINE ENDINGS normalized first.
        ///
        /// <c>Utf8JsonWriter</c> indents with the platform's newline on .NET 8, so a JSON pin taken on Windows
        /// does not match the same JSON on the Linux runner. That is a defect of the PIN and not of the product:
        /// the bytes that differ are whitespace nobody persists meaning in. Normalizing keeps the pin portable
        /// and still catches every real change of content — a renamed key, a lost field, a moved value.
        /// </summary>
        private static string Sha(string content) =>
            Convert.ToHexString(SHA256.HashData(
                Encoding.UTF8.GetBytes(content.Replace("\r\n", "\n").Replace("\r", "\n"))));

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
            // ESTOS DOS SE MOVIERON en la correccion de columna/base, y por una causa fisica declarada: el
            // dueno retiro los dos margenes de troquel, asi que cabe un agujero entero mas por fila. La firma
            // de la linea lleva dentro las cuentas de troqueles —204 -> 210 en la sencilla, 276 -> 282 en la
            // doble— y por eso se mueve.
            //
            // Los CUATRO pines de vista de abajo NO se movieron: excluyen los troqueles a proposito, asi que
            // demuestran que el cambio toco agujeros y NADA MAS. Tampoco se movieron los dos del BOM: un
            // troquel no es una linea de BOM.
            // Anteriores: linea 35F179AA…, linea-doble E05B3F68…
            // MOVIDOS OTRA VEZ en la correccion del offset de troquel, y por causa fisica declarada: el dueno
            // bajo la distancia de la orilla al centro del troquel de 1.5 a 1.0 in y precisó que se mide
            // «desde el exterior de la placa hacia el centro de la columna». Las dos filas pasan de x = ±2.48
            // —(7.96/2) − 1.5, desde la COLUMNA— a x = ±2.245 —(6.49/2) − 1.0, desde la placa posterior—. La
            // firma de la linea lleva dentro las coordenadas de cada troquel, y la persistencia lleva el
            // propio 1.0 en el JSON.
            //
            // Los CUATRO pines de vista NO se movieron: excluyen los troqueles a proposito, y mover una fila
            // en X no cambia cuantas curvas dibuja ninguna vista. Los dos del BOM tampoco: un troquel no es
            // una linea de BOM.
            // Anteriores: linea 58202A8B…, linea-doble 57A55555…, persistencia 24358728…
            // MOVIDOS UNA TERCERA VEZ en la ronda 3, por los defaults del BRAZO que el dueno aprobo: la
            // pendiente por omision pasa de 0 a 7/16 por 12 in y el margen vertical de la placa pasa a ser un
            // default de 2 in. La fixture de referencia no fijaba pendiente, asi que ahora sus brazos SUBEN.
            //
            // `planta` se movio con ellos y es la consecuencia fisica exacta: un brazo inclinado recorre menos
            // horizontal que su longitud de corte, asi que su huella en planta se acorta por el coseno.
            //
            // `frontal` y `frontal-doble` NO se movieron, y eso es la comprobacion de que el aplanado de la
            // frontal hace lo que dice: deshace la pendiente nueva y devuelve exactamente el perfil que se
            // veia cuando la pendiente era cero. `lateral` tampoco, porque cuenta puntos y no coordenadas.
            // Los dos del BOM tampoco: la pendiente no cambia la longitud que se ordena.
            // Anteriores: linea 6D3BB5C6…, linea-doble 2038786A…, planta 9E9611FA…, persistencia 9F89BB74…
            // MOVIDOS UNA CUARTA VEZ en la ronda 3, con el SEPARADOR al alma. El dueno declaro que el
            // arriostramiento longitudinal se atornilla al alma y no al patin, asi que el separador pasa entre
            // los dos patines y topa contra ella: su claro deja de medirse de cara a cara de patin —96 − 7.96
            // = 88.04 in— y pasa a medirse de alma a alma —96 − 0.29 = 95.71 in—.
            //
            // Y esta vez SE MUEVEN TAMBIEN LOS DOS DEL BOM, que no se habian movido en toda la iniciativa: la
            // longitud de un separador SI es una linea de pedido. Es la senal de que este cambio toca el
            // producto y no solo el dibujo.
            // Anteriores: linea E3AAA3BC…, linea-doble 2A9EF89B…, bom 6088C538…, bom-doble 608B790C…
            ["linea"] = "04BFD336A2EC1F31E8ECF973FE23A0083F4A7C86FA072A2E7B5B10435C54803A",
            ["linea-doble"] = "A7690B79B05895FD41D86F1031EADB160CB2EC96E9A7EEEF83B14287C14D51F5",
            ["bom"] = "F8AB3351A2D4F7C2F59888AAD54DE47DE73E100DE36C7F07788CFDE8B18072EC",
            ["bom-doble"] = "C17AD69D9A435B20B99D7DF3121D107C910C6D361D68D21261C230CCBF5D0F63",
            // TRES DE ESTOS CUATRO SE MOVIERON en la correccion de columna/base, por la causa fisica del
            // motivo 2: se le preguntaba a la CAMARA si miraba a lo largo del eje Z DEL MUNDO, cuando lo que
            // decide si una seccion conserva su forma es el eje del MIEMBRO. Una camara cenital conserva la
            // forma de una columna de pie y no la de una base tumbada, y solo su colocacion las distingue.
            //
            // - `planta`: la base pasa de UNA linea de 6.49 in con longitud CERO a su huella real de
            //   6.49 x 48 in —sus dos caras y sus generatrices—, y los brazos de igual modo a 4 x 36 in.
            // - `frontal` y `frontal-doble`: el caso contrario. La base apunta hacia la camara, asi que ahora
            //   se dibuja como la seccion que es y no como dos caras extremas superpuestas: una entidad
            //   duplicada menos por base.
            // - `lateral` NO se movio, y es correcto: alli ninguna seccion cambia de regimen. Que las placas
            //   ganen su espesor tampoco mueve esta firma, porque cuenta PUNTOS y no coordenadas —el casco de
            //   un rectangulo visto de canto sigue siendo cuatro esquinas cerradas—; ese espesor lo fija
            //   CantileverLateralViewTests, midiendolo.
            // Anteriores: frontal 1A12F749…, planta EFB547E6…, frontal-doble 7D74B31D…
            // MOVIDOS en el punto 7 de la ronda 3, con la REPRESENTACION FISICA del tensor. El dueno reviso
            // la convencion de ADR-0027 D7 —OWNER_REVISED_CANTILEVER_BRACE_VISUAL_REPRESENTATION—: el eje
            // sigue siendo el datum y la geometria visible pasa a tener ancho. El cuerpo cold rolled pasa de
            // una polilinea ABIERTA de 2 puntos a una banda CERRADA de 4, y cada adaptador de un cuadrado de 4
            // puntos a una L de 6 mas sus dos cartabones de 3.
            //
            // `lateral` NO se movio: un intervalo no dibuja tensores en la lateral de una estacion. Los dos
            // del BOM tampoco, y es la comprobacion de que esto fue un cambio de DIBUJO: la longitud nominal,
            // el diametro y la cuenta de adaptadores y cartabones no se tocaron.
            // Anteriores: frontal D17564B6…, planta 7DA73B45…, frontal-doble 34E02F20…
            ["frontal"] = "C7A22C781A99209CF2C67188A5C6F9CD28DF6903CAE33D05CF90A3A1A35D28CC",
            // Movido en la ronda 3 con la pendiente por omision del brazo: su huella en planta se acorta.
            ["planta"] = "632DD853DA564DA9580CFE90A5E2B276034000950633DB97B407DD6F2D564B22",
            ["lateral"] = "E26334E504741413419C6E777934B06209AE989F6202B37C8C33E149E5BB4343",
            ["frontal-doble"] = "C7EE670CF2960D5F6A575C5D5F2C1E6371791259747600B9F56D6006B0FFCD82",

            // MOVIDO A PROPOSITO en la ronda 2, y es el UNICO pin que se movio por contenido.
            // `BaseFollowsColumn` es intencion nueva del diseno y se persiste, asi que el JSON gana una clave.
            // El pin anterior, 874066E2…, es el de antes de que la base pudiera seguir a la columna. Los ocho
            // pines de resolucion, BOM y planes fisicos NO se movieron: la regla es del editor y no cambia lo
            // que la linea resuelve, y hay una prueba que lo comprueba.
            //
            // Movido una SEGUNDA vez —de E7146CB3… a este— al normalizar los saltos de linea antes de hashear:
            // la CI corre en Linux, `Utf8JsonWriter` indenta con el salto de la plataforma, y el pin tomado en
            // Windows no podia coincidir. Era un defecto del pin, no del producto; el contenido no cambio.
            // Movido una TERCERA vez, en la correccion de columna y base: el JSON dejo de escribir
            // `ColumnBottomPlateEndOffset` y `ColumnTopPunchOffset`, que el dueno retiro. Dos claves menos.
            // Anterior: C8D5A3C8…
            // Movido una CUARTA vez, con el offset de 1.0: el JSON persiste el propio numero.
            // Movido una QUINTA vez en la ronda 3: el JSON persiste la pendiente y el margen nuevos.
            ["persistencia"] = "C80E01490F21E980FF8BCAF69E5271EE3CC043AC32D8228477B361BC42E10065"
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
            // La medida del motivo 5 del rechazo: los troqueles EXISTEN resueltos —210 en la línea sencilla,
            // 282 en la doble, tras retirar los dos márgenes— y las placas de separador también. Lo que fallaba no era el modelo: era que la
            // representación no los pedía. Este número es el que la corrección tiene que hacer visible.
            var single = Assembler().Build(Reference());
            var doble = Assembler().Build(Reference(CantileverStationFaceMode.Double));

            Assert.Equal(210, single.Line.Stations.Sum(s => s.Station.Punches.Count));
            Assert.Equal(282, doble.Line.Stations.Sum(s => s.Station.Punches.Count));
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
