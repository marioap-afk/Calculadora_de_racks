using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-48 gate G4E — la PRUEBA REAL de la segunda propiedad vinculable: <c>selective.palletTolerance</c>.
    ///
    /// <para>
    /// Todo lo que I-48 construyo desde G4A existe para que esta activacion sea un cambio de CATALOGO y no de
    /// arquitectura. Por eso lo que se prueba aqui no es que la tolerancia «funcione», sino que atraviesa la
    /// infraestructura generica SIN que nadie haya escrito una rama para ella: el resolver, Link, Unlink,
    /// ChangeValue, FindBroken, Repair y el reconciler tienen que tratarla exactamente igual que a la holgura
    /// vertical, y por el mismo codigo.
    /// </para>
    /// <para>
    /// <b>Los oraculos leen los campos CONCRETOS</b> —<c>document.PalletTolerance</c>,
    /// <c>design.PalletTolerance</c>— y jamas los accesores del descriptor. Preguntarle al descriptor que campo
    /// debia cambiar probaria que el descriptor coincide consigo mismo; lo que hay que decidir aqui es si esta
    /// cableado al campo correcto.
    /// </para>
    /// <para>
    /// Y la no-interferencia se prueba en los DOS sentidos: vincular la tolerancia no puede mover la holgura, y
    /// vincular la holgura no puede mover la tolerancia. Con una sola propiedad registrada esa pregunta no se
    /// podia ni formular.
    /// </para>
    /// </summary>
    public class PalletToleranceRealProofTests
    {
        private const string RackA = "3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44";
        private const string VarX = "11111111-1111-1111-1111-111111111111";
        private const string VarY = "22222222-2222-2222-2222-222222222222";

        private const string ToleranceToken = "selective.palletTolerance";
        private const string ClearanceToken = ProjectPropertyIds.SelectiveVerticalClearanceToken;

        private static VariableId Id(string guid) => VariableId.Parse(guid);

        /// <summary>Por token a proposito: el test compila ANTES de que la constante exista.</summary>
        private static PropertyId Tolerance => PropertyId.Parse(ToleranceToken);

        private static PropertyId Clearance => ProjectPropertyIds.SelectiveVerticalClearance;

        // ---------------------------------------------------------------- fixtures

        /// <summary>Un rack minimo: tolerancia 4, holgura 6, un frente de 48 con una tarima.</summary>
        private static SelectivePalletDesign Diseno(double tolerance = 4.0, double clearance = 6.0)
        {
            var design = new SelectivePalletDesign
            {
                PostId = "POSTE_A",
                PostPeralte = 3.0,
                PalletTolerance = tolerance,
                VerticalClearance = clearance,
                PalletDepth = 48.0,
            };

            var bay = new SelectiveBayDesign();
            bay.Levels.Add(new SelectiveCell
            {
                Pallet = new Tarima { Frente = 48, Alto = 50 },
                PalletCount = 1,
                BeamId = "BEAM_A",
                BeamPeralte = 4.5,
            });
            design.Bays.Add(bay);
            return design;
        }

        /// <summary>El documento guardado, con los vinculos que se le indiquen.</summary>
        private static SelectivePalletDesignDocument Authored(
            double tolerance = 4.0,
            double clearance = 6.0,
            string toleranceBoundTo = null,
            string clearanceBoundTo = null,
            string rackId = RackA)
        {
            var doc = SelectivePalletDesignDocument.From(Diseno(tolerance, clearance), rackId, "Rack A");
            var bindings = new Dictionary<string, SelectivePropertyValueDocument>();

            if (toleranceBoundTo != null)
            {
                bindings[ToleranceToken] = SelectivePropertyValueDocument.ToProjectVariable(toleranceBoundTo);
            }

            if (clearanceBoundTo != null)
            {
                bindings[ClearanceToken] = SelectivePropertyValueDocument.ToProjectVariable(clearanceBoundTo);
            }

            if (bindings.Count > 0)
            {
                doc.PropertyValues = bindings;
                doc.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;
            }

            return doc;
        }

        private static ProjectVariableDocument Entry(string guid, string name, double value)
            => new ProjectVariableDocument
            {
                VariableId = guid,
                Name = name,
                Type = VariableType.Length.ToString(),
                Definition = new ProjectVariableDefinitionDocument { Kind = "literal", Value = value },
            };

        /// <summary>El registro: X vale 9, Y vale 12.</summary>
        private static ProjectVariablesDocument Registro(double x = 9.0, double y = 12.0)
        {
            var document = ProjectVariablesDocument.CreateNew();
            document.Variables = new List<ProjectVariableDocument>
            {
                Entry(VarX, "Tolerancia General", x),
                Entry(VarY, "Otra Longitud", y),
            };
            return document;
        }

        private static ProjectVariableScanEntry Vista(
            SelectivePalletDesignDocument doc, string def = "D1", string rackId = RackA)
            => ProjectVariableScanEntry.Selective(def, rackId, doc);

        private static SelectiveEffectiveResolution Resolve(
            SelectivePalletDesignDocument authored, ProjectVariablesDocument registry)
            => new SelectiveEffectiveDesignResolver().Resolve(authored, registry);

        // ================================================================ 1. el catalogo

        /// <summary>
        /// G4E (1). El catalogo productivo declara AHORA dos propiedades. Es el unico sitio que crece: la
        /// semantica generica ya existia.
        /// </summary>
        [Fact]
        public void EL_CATALOGO_PRODUCTIVO_DECLARA_LAS_DOS_PROPIEDADES()
        {
            Assert.True(ProjectPropertyIds.IsKnown(Clearance));
            Assert.True(ProjectPropertyIds.IsKnown(Tolerance));
        }

        /// <summary>
        /// G4E (2). El ORACLE independiente. Declara por su cuenta que token gobierna que campo, y se compara
        /// contra el catalogo productivo: si el descriptor apuntase al campo equivocado, o si el catalogo
        /// creciera sin que nadie lo decidiera, esta prueba lo dice.
        ///
        /// <para>
        /// La expectativa NO se construye leyendo los accesores del descriptor. Se escribe a mano.
        /// </para>
        /// </summary>
        [Fact]
        public void EL_ORACLE_PRODUCTIVO_COINCIDE_EXACTAMENTE_CON_EL_CATALOGO()
        {
            var oracle = new[] { ClearanceToken, ToleranceToken };

            var catalogo = ProductionTokens();

            Assert.Equal(oracle.OrderBy(t => t, System.StringComparer.Ordinal).ToArray(),
                catalogo.OrderBy(t => t, System.StringComparer.Ordinal).ToArray());
            Assert.Equal(2, catalogo.Count);
        }

        /// <summary>
        /// El oracle de CABLEADO: cada token mueve SU campo. Se comprueba resolviendo de verdad y leyendo el
        /// campo concreto del dominio, nunca el descriptor.
        /// </summary>
        [Theory]
        [InlineData(ToleranceToken, 9.0, 4.0, 6.0)]   // gobierna la tolerancia; la holgura se queda en 6
        [InlineData(ClearanceToken, 9.0, 4.0, 9.0)]   // gobierna la holgura;  la tolerancia se queda en 4
        public void CADA_TOKEN_GOBIERNA_SU_PROPIO_CAMPO(
            string token, double valorVariable, double toleranciaEsperada, double holguraEsperada)
        {
            var authored = Authored(
                tolerance: 4.0,
                clearance: 6.0,
                toleranceBoundTo: token == ToleranceToken ? VarX : null,
                clearanceBoundTo: token == ClearanceToken ? VarX : null);

            var resolution = Resolve(authored, Registro(x: valorVariable));

            Assert.True(resolution.IsSuccess);

            // Oraculos: los campos CONCRETOS del diseno, escritos a mano.
            Assert.Equal(token == ToleranceToken ? valorVariable : toleranciaEsperada,
                resolution.Design.PalletTolerance);
            Assert.Equal(holguraEsperada, resolution.Design.VerticalClearance);
        }

        // ================================================================ 3. resolver efectivo

        [Fact]
        public void LA_TOLERANCIA_VINCULADA_TOMA_EL_VALOR_DE_SU_VARIABLE()
        {
            var resolution = Resolve(
                Authored(tolerance: 4.0, toleranceBoundTo: VarX), Registro(x: 9.0));

            Assert.True(resolution.IsSuccess);
            Assert.Equal(9.0, resolution.Design.PalletTolerance);

            // Y el authored NO se toca: el literal congelado sigue siendo 4.
            Assert.Equal(4.0, Authored(tolerance: 4.0, toleranceBoundTo: VarX).PalletTolerance);
        }

        [Fact]
        public void VINCULAR_LA_HOLGURA_NO_ESCRIBE_LA_TOLERANCIA()
        {
            var resolution = Resolve(
                Authored(tolerance: 4.0, clearance: 6.0, clearanceBoundTo: VarX), Registro(x: 9.0));

            Assert.True(resolution.IsSuccess);
            Assert.Equal(9.0, resolution.Design.VerticalClearance);
            Assert.Equal(4.0, resolution.Design.PalletTolerance);
        }

        [Fact]
        public void LAS_DOS_VINCULADAS_A_VARIABLES_DISTINTAS_RESUELVEN_INDEPENDIENTEMENTE()
        {
            var resolution = Resolve(
                Authored(tolerance: 4.0, clearance: 6.0, toleranceBoundTo: VarX, clearanceBoundTo: VarY),
                Registro(x: 9.0, y: 12.0));

            Assert.True(resolution.IsSuccess);
            Assert.Equal(9.0, resolution.Design.PalletTolerance);
            Assert.Equal(12.0, resolution.Design.VerticalClearance);
        }

        // ================================================================ 4. Link congela SU literal

        [Fact]
        public void LINK_DE_LA_TOLERANCIA_CONGELA_SU_LITERAL_Y_APLICA_EL_VALOR_DE_LA_VARIABLE()
        {
            var registro = Registro(x: 9.0);
            var authored = Authored(tolerance: 4.0, clearance: 6.0);

            var result = ProjectVariableMutationPreflight.Link(
                registro, RackA, Tolerance, Id(VarX), new[] { Vista(authored) });

            Assert.True(result.IsSuccess);

            var rack = Assert.Single(result.Plan.RackMutations);

            // authored: la tolerancia congelada en 4 y el vinculo puesto.
            Assert.Equal(4.0, rack.AuthoredOutput.PalletTolerance);
            Assert.True(rack.AuthoredOutput.TryGetBinding(Tolerance, out var bound));
            Assert.Equal(Id(VarX), bound);

            // effective: manda la variable.
            Assert.Equal(9.0, rack.EffectiveOutput.PalletTolerance);

            // Y la holgura ni se entera.
            Assert.Equal(6.0, rack.AuthoredOutput.VerticalClearance);
            Assert.Equal(6.0, rack.EffectiveOutput.VerticalClearance);
        }

        // ================================================================ 5. Unlink materializa SU efectivo

        [Fact]
        public void UNLINK_DE_LA_TOLERANCIA_MATERIALIZA_SU_PROPIO_EFECTIVO()
        {
            var registro = Registro(x: 9.0);
            var authored = Authored(tolerance: 4.0, clearance: 6.0, toleranceBoundTo: VarX);

            var result = ProjectVariableMutationPreflight.Unlink(
                registro, RackA, Tolerance, new[] { Vista(authored) });

            Assert.True(result.IsSuccess);

            var rack = Assert.Single(result.Plan.RackMutations);

            Assert.False(rack.AuthoredOutput.TryGetBinding(Tolerance, out _));
            Assert.Equal(9.0, rack.AuthoredOutput.PalletTolerance);   // materializa el efectivo que regia
            Assert.Equal(9.0, rack.EffectiveOutput.PalletTolerance);

            Assert.Equal(6.0, rack.AuthoredOutput.VerticalClearance);
        }

        // ================================================================ 6. ChangeValue

        [Fact]
        public void CHANGEVALUE_MUEVE_EL_EFECTIVO_DE_LA_TOLERANCIA_Y_NO_SU_LITERAL_CONGELADO()
        {
            var registro = Registro(x: 9.0);
            var authored = Authored(tolerance: 4.0, toleranceBoundTo: VarX);

            var result = ProjectVariableMutationPreflight.ChangeValue(
                registro, Id(VarX), VariableDefinition.Literal(12.0), new[] { Vista(authored) });

            Assert.True(result.IsSuccess);

            var rack = Assert.Single(result.Plan.RackMutations);

            Assert.Equal(4.0, rack.AuthoredOutput.PalletTolerance);   // el congelado permanece
            Assert.True(rack.AuthoredOutput.TryGetBinding(Tolerance, out _)); // el vinculo permanece
            Assert.Equal(12.0, rack.EffectiveOutput.PalletTolerance);
        }

        // ================================================================ 7. FindBroken

        [Fact]
        public void FINDBROKEN_DIAGNOSTICA_LA_TOLERANCIA_CON_SU_PROPIO_LITERAL()
        {
            // La variable de la tolerancia no existe en este dibujo; la holgura ni siquiera esta vinculada.
            var authored = Authored(tolerance: 4.0, clearance: 6.0, toleranceBoundTo: VarX);
            var sinX = ProjectVariablesDocument.CreateNew();
            sinX.Variables = new List<ProjectVariableDocument> { Entry(VarY, "Otra", 12.0) };

            var workspace = ProjectVariablesWorkspace.Build(
                ProjectVariablesReadResult.Readable(sinX), new[] { Vista(authored) });

            var broken = Assert.Single(workspace.BrokenBindings);

            Assert.Equal(ToleranceToken, broken.PropertyId);
            Assert.Equal(VarX, broken.VariableId);

            // El literal que se ensena es el de LA TOLERANCIA (4), no el de la holgura (6).
            Assert.Equal(4.0, broken.StoredLiteral);
            Assert.True(broken.RackCanRepair);
        }

        // ================================================================ 8. RepairBrokenRack

        [Fact]
        public void REPAIR_QUITA_EL_VINCULO_ROTO_DE_LA_TOLERANCIA_Y_CONSERVA_LA_HOLGURA_SANA()
        {
            // Tolerancia rota (X no existe) + holgura SANA vinculada a Y.
            var authored = Authored(
                tolerance: 4.0, clearance: 6.0, toleranceBoundTo: VarX, clearanceBoundTo: VarY);

            var soloY = ProjectVariablesDocument.CreateNew();
            soloY.Variables = new List<ProjectVariableDocument> { Entry(VarY, "Otra", 12.0) };

            var result = ProjectVariableMutationPreflight.RepairBrokenRack(
                soloY, RackA, new[] { Vista(authored) }, confirmed: true);

            Assert.True(result.IsSuccess);

            var rack = Assert.Single(result.Plan.RackMutations);

            // La rota se retira y su literal almacenado gobierna.
            Assert.False(rack.AuthoredOutput.TryGetBinding(Tolerance, out _));
            Assert.Equal(4.0, rack.AuthoredOutput.PalletTolerance);
            Assert.Equal(4.0, rack.EffectiveOutput.PalletTolerance);

            // La sana se CONSERVA, con su vinculo y su efectivo.
            Assert.True(rack.AuthoredOutput.TryGetBinding(Clearance, out var bound));
            Assert.Equal(Id(VarY), bound);
            Assert.Equal(12.0, rack.EffectiveOutput.VerticalClearance);
        }

        // ================================================================ 9. EditorOpen

        [Fact]
        public void ABRIR_UN_RACK_CON_LA_TOLERANCIA_VINCULADA_USA_SU_EFECTIVO()
        {
            var open = SelectiveEditorOpen.Resolve(
                Authored(tolerance: 4.0, clearance: 6.0, toleranceBoundTo: VarX),
                ProjectVariablesReadResult.Readable(Registro(x: 9.0)));

            Assert.True(open.IsOpen);

            // El editor trabaja sobre el EFECTIVO.
            Assert.Equal(9.0, open.Design.PalletTolerance);
            Assert.Equal(6.0, open.Design.VerticalClearance);
        }

        // ================================================================ 10. reconciler con las dos

        [Fact]
        public void EL_RECONCILER_ESCRIBE_CADA_LITERAL_Y_CADA_VINCULO_POR_SEPARADO()
        {
            var finales = new Dictionary<PropertyId, LinkedPropertyEditState>
            {
                [Tolerance] = LinkedPropertyEditState.Reference(4.0, Id(VarX)),
                [Clearance] = LinkedPropertyEditState.Reference(6.0, Id(VarY)),
            };

            var result = LinkedPropertyReconciler.Reconcile(
                Authored(tolerance: 4.0, clearance: 6.0),
                Diseno(tolerance: 99.0, clearance: 99.0),   // el diseno editado NO manda para estas dos
                finales,
                ProjectVariablesReadResult.Readable(Registro(x: 9.0, y: 12.0)),
                RackA,
                "Rack A");

            Assert.True(result.IsSuccess);

            // Cada literal congelado en su propio campo...
            Assert.Equal(4.0, result.Authored.PalletTolerance);
            Assert.Equal(6.0, result.Authored.VerticalClearance);

            // ...cada vinculo a su propia variable...
            Assert.True(result.Authored.TryGetBinding(Tolerance, out var boundTolerance));
            Assert.True(result.Authored.TryGetBinding(Clearance, out var boundClearance));
            Assert.Equal(Id(VarX), boundTolerance);
            Assert.Equal(Id(VarY), boundClearance);

            // ...y cada efectivo de su propia variable.
            Assert.Equal(9.0, result.Effective.PalletTolerance);
            Assert.Equal(12.0, result.Effective.VerticalClearance);
        }

        [Fact]
        public void EL_RECONCILER_PUEDE_DEJAR_UNA_LITERAL_Y_OTRA_VINCULADA()
        {
            var finales = new Dictionary<PropertyId, LinkedPropertyEditState>
            {
                [Tolerance] = LinkedPropertyEditState.Literal(7.5),
                [Clearance] = LinkedPropertyEditState.Reference(6.0, Id(VarY)),
            };

            var result = LinkedPropertyReconciler.Reconcile(
                Authored(tolerance: 4.0, clearance: 6.0, toleranceBoundTo: VarX),
                Diseno(),
                finales,
                ProjectVariablesReadResult.Readable(Registro(y: 12.0)),
                RackA,
                "Rack A");

            Assert.True(result.IsSuccess);

            Assert.False(result.Authored.TryGetBinding(Tolerance, out _));
            Assert.Equal(7.5, result.Authored.PalletTolerance);
            Assert.Equal(7.5, result.Effective.PalletTolerance);

            Assert.True(result.Authored.TryGetBinding(Clearance, out _));
            Assert.Equal(12.0, result.Effective.VerticalClearance);
        }

        // ================================================================ compatibilidad hacia atras

        /// <summary>
        /// G4E (N). Un documento viejo, sin entrada para la tolerancia, sigue siendo un LITERAL. Registrar el
        /// descriptor no migra nada ni escribe un vinculo por defecto: abrirlo no puede alterarlo.
        /// </summary>
        [Fact]
        public void UN_DOCUMENTO_SIN_ENTRADA_DE_TOLERANCIA_SIGUE_SIENDO_UN_LITERAL()
        {
            var authored = Authored(tolerance: 4.0, clearance: 6.0);

            Assert.Null(authored.PropertyValues);

            var resolution = Resolve(authored, Registro());

            Assert.True(resolution.IsSuccess);
            Assert.Equal(4.0, resolution.Design.PalletTolerance);

            // Y abrir no escribe nada.
            Assert.Null(authored.PropertyValues);
        }

        // ================================================================ R. consumidor real aguas abajo

        /// <summary>
        /// G4E (R). La prueba de que la tolerancia efectiva llega a un consumidor REAL del producto.
        ///
        /// <para>
        /// <c>SelectiveGeometryResolver</c> calcula el largo del larguero como
        /// <c>frente * tarimas + tolerancia * (tarimas + 1)</c>. Con un frente de 48 y una tarima, la tolerancia
        /// 4 da 56 y la tolerancia 9 da 66 — numeros que esta prueba escribe a mano, sin preguntarle nada al
        /// descriptor ni al resolver de variables.
        /// </para>
        /// <para>
        /// Es lo que convierte a la tolerancia en una segunda propiedad REAL y no en un campo de adorno:
        /// gobernarla con una variable cambia geometria que el dibujo y el BOM consumen.
        /// </para>
        /// </summary>
        [Fact]
        public void LA_TOLERANCIA_EFECTIVA_ALCANZA_LA_GEOMETRIA_DEL_LARGUERO()
        {
            var geometry = new SelectiveGeometryResolver();

            // Sin vinculo: gobierna el literal 4 -> 48*1 + 4*2 = 56.
            var literal = geometry.Resolve(Resolve(Authored(tolerance: 4.0), Registro()).Design, null);

            // Vinculada a X = 9 -> 48*1 + 9*2 = 66.
            var vinculada = geometry.Resolve(
                Resolve(Authored(tolerance: 4.0, toleranceBoundTo: VarX), Registro(x: 9.0)).Design, null);

            Assert.Equal(56.0, literal.Bays[0].BeamLength);
            Assert.Equal(66.0, vinculada.Bays[0].BeamLength);
        }

        // ---------------------------------------------------------------- helpers

        /// <summary>
        /// Los tokens que el catalogo productivo declara, leidos por la via publica: una propiedad es conocida
        /// si y solo si el catalogo la declara.
        /// </summary>
        private static IReadOnlyList<string> ProductionTokens()
        {
            var candidatos = new[]
            {
                ClearanceToken,
                ToleranceToken,
                "selective.floorBeamRise",
                "selective.palletDepth",
                "selective.postPeralte",
            };

            return candidatos.Where(token => ProjectPropertyIds.IsKnown(PropertyId.Parse(token))).ToList();
        }
    }
}
