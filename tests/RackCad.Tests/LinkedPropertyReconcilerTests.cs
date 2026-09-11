using System.Collections.Generic;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-48 gate G4C — el RECONCILER: estado final declarado -> authored + effective.
    ///
    /// <para>
    /// El editor no entrega una secuencia. Una sesion donde el usuario vincula, desvincula y vuelve a vincular
    /// persistiria tres documentos intermedios, y el segundo congela un literal que nadie pidio. Lo que el
    /// usuario expreso es DONDE TERMINO, asi que es lo que viaja.
    /// </para>
    /// <para>
    /// Los oraculos se declaran de forma INDEPENDIENTE del editor: se afirma sobre
    /// <c>authored.PropertyValues</c>, sobre <c>authored.VerticalClearance</c> y sobre
    /// <c>effective.VerticalClearance</c> con numeros escritos a mano, no comparando el resultado con el estado
    /// que lo produjo. Comparar el reconciler con el editor probaria que coinciden, no que aciertan.
    /// </para>
    /// </summary>
    public class LinkedPropertyReconcilerTests
    {
        private const string RackId = "3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44";
        private const string IdX = "11111111-1111-1111-1111-111111111111";
        private const string IdY = "22222222-2222-2222-2222-222222222222";
        private const string Token = ProjectPropertyIds.SelectiveVerticalClearanceToken;

        private static VariableId Id(string guid) => VariableId.Parse(guid);

        private static PropertyId Clearance => ProjectPropertyIds.SelectiveVerticalClearance;

        private static SelectivePalletDesign Diseno(double clearance)
        {
            var design = new SelectivePalletDesign { VerticalClearance = clearance, PalletDepth = 48.0 };
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

        /// <summary>El authored inicial: literal <paramref name="literal"/>, vinculado a <paramref name="boundTo"/> si se da.</summary>
        private static SelectivePalletDesignDocument Authored(double literal, string boundTo = null)
        {
            var doc = SelectivePalletDesignDocument.From(Diseno(literal), RackId, "Rack A");

            if (boundTo != null)
            {
                doc.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>
                {
                    [Token] = SelectivePropertyValueDocument.ToProjectVariable(boundTo),
                };
                doc.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;
            }

            return doc;
        }

        private static ProjectVariablesReadResult Registro()
        {
            var document = ProjectVariablesDocument.CreateNew();
            document.Variables = new List<ProjectVariableDocument>
            {
                Entry(IdX, "Holgura General", 10.0),
                Entry(IdY, "Holgura Estrecha", 12.0),
            };
            return ProjectVariablesReadResult.Readable(document);
        }

        private static ProjectVariableDocument Entry(string guid, string name, double value)
            => new ProjectVariableDocument
            {
                VariableId = guid,
                Name = name,
                Type = VariableType.Length.ToString(),
                Definition = new ProjectVariableDefinitionDocument { Kind = "literal", Value = value },
            };

        private static Dictionary<PropertyId, LinkedPropertyEditState> Final(LinkedPropertyEditState state)
            => new Dictionary<PropertyId, LinkedPropertyEditState> { [Clearance] = state };

        private static LinkedPropertyReconciliation Reconcile(
            SelectivePalletDesignDocument authored,
            double editedClearance,
            LinkedPropertyEditState finalState,
            ProjectVariablesReadResult read = null)
            => LinkedPropertyReconciler.Reconcile(
                authored, Diseno(editedClearance), Final(finalState), read ?? Registro());

        // ================================================================ 17: Literal -> Literal

        [Fact]
        public void CASO_17_LITERAL_A_LITERAL_DEJA_EL_NUMERO_NUEVO_Y_NINGUN_VINCULO()
        {
            var result = Reconcile(Authored(4.0), editedClearance: 9.0, LinkedPropertyEditState.Literal(9.0));

            Assert.True(result.IsSuccess);
            Assert.Equal(9.0, result.Authored.VerticalClearance);
            Assert.False(result.Authored.TryGetBinding(Clearance, out _));
            Assert.Equal(9.0, result.Effective.VerticalClearance);
        }

        // ================================================================ 18: Literal -> Reference

        [Fact]
        public void CASO_18_LITERAL_A_REFERENCIA_CONGELA_EL_LITERAL_COMPROMETIDO_Y_APLICA_EL_VALOR_DE_LA_VARIABLE()
        {
            // El estado final declara: gobierna X, y detras queda congelado el 4.
            var result = Reconcile(
                Authored(4.0), editedClearance: 4.0, LinkedPropertyEditState.Reference(4.0, Id(IdX)));

            Assert.True(result.IsSuccess);

            // authored: el literal congelado es 4, y el vinculo apunta a X.
            Assert.Equal(4.0, result.Authored.VerticalClearance);
            Assert.True(result.Authored.TryGetBinding(Clearance, out var bound));
            Assert.Equal(Id(IdX), bound);

            // effective: manda la variable, que vale 10.
            Assert.Equal(10.0, result.Effective.VerticalClearance);
        }

        [Fact]
        public void CASO_18_BIS_SI_EL_LITERAL_COMPROMETIDO_ERA_OTRO_SE_CONGELA_ESE_Y_NO_EL_INICIAL()
        {
            // 20.13 caso 2 visto desde el reconciler: el authored empezo en 4, el usuario comprometio 7 y
            // despues vinculo. Lo que se congela es 7.
            var result = Reconcile(
                Authored(4.0), editedClearance: 7.0, LinkedPropertyEditState.Reference(7.0, Id(IdX)));

            Assert.True(result.IsSuccess);
            Assert.Equal(7.0, result.Authored.VerticalClearance);
            Assert.Equal(10.0, result.Effective.VerticalClearance);
        }

        // ================================================================ 19: Reference -> Reference distinta

        [Fact]
        public void CASO_19_DE_UNA_REFERENCIA_A_OTRA_CONSERVA_EL_CONGELADO_Y_CAMBIA_EL_VINCULO()
        {
            var result = Reconcile(
                Authored(4.0, IdX), editedClearance: 10.0, LinkedPropertyEditState.Reference(4.0, Id(IdY)));

            Assert.True(result.IsSuccess);

            // El congelado sigue siendo 4, aunque el control mostrase 10 (el efectivo de X).
            Assert.Equal(4.0, result.Authored.VerticalClearance);
            Assert.True(result.Authored.TryGetBinding(Clearance, out var bound));
            Assert.Equal(Id(IdY), bound);

            // Y ahora gobierna Y, que vale 12.
            Assert.Equal(12.0, result.Effective.VerticalClearance);
        }

        // ================================================================ 20: Reference -> la MISMA

        [Fact]
        public void CASO_20_DE_UNA_REFERENCIA_A_LA_MISMA_ES_IDEMPOTENTE()
        {
            var inicial = Authored(4.0, IdX);

            var result = Reconcile(
                inicial, editedClearance: 10.0, LinkedPropertyEditState.Reference(4.0, Id(IdX)));

            Assert.True(result.IsSuccess);
            Assert.Equal(4.0, result.Authored.VerticalClearance);
            Assert.True(result.Authored.TryGetBinding(Clearance, out var bound));
            Assert.Equal(Id(IdX), bound);
            Assert.Equal(10.0, result.Effective.VerticalClearance);

            // Y el documento de entrada no se mutó: reconciliar es puro.
            Assert.Equal(4.0, inicial.VerticalClearance);
        }

        // ================================================================ 21: Reference -> Literal

        [Fact]
        public void CASO_21_DE_REFERENCIA_A_LITERAL_QUITA_EL_VINCULO_Y_EL_NUMERO_GOBIERNA()
        {
            var result = Reconcile(
                Authored(4.0, IdX), editedClearance: 7.0, LinkedPropertyEditState.Literal(7.0));

            Assert.True(result.IsSuccess);
            Assert.Equal(7.0, result.Authored.VerticalClearance);
            Assert.False(result.Authored.TryGetBinding(Clearance, out _));
            Assert.Equal(7.0, result.Effective.VerticalClearance);
        }

        // ================================================================ 22: fail-closed

        [Fact]
        public void CASO_22_UNA_PROPIEDAD_AJENA_IRRESOLUBLE_DEJA_CERO_RESULTADO()
        {
            // Otra propiedad del rack lleva un token que esta version no conoce. El rack no puede producir un
            // efectivo COMPLETO, asi que no se devuelve nada: ni authored a medias, ni fallback al literal.
            var authored = Authored(4.0);
            authored.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>
            {
                ["selective.noExiste"] = SelectivePropertyValueDocument.ToProjectVariable(IdX),
            };
            authored.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;

            var result = Reconcile(authored, editedClearance: 9.0, LinkedPropertyEditState.Literal(9.0));

            Assert.False(result.IsSuccess);
            Assert.Null(result.Authored);
            Assert.Null(result.Effective);
            Assert.Equal(LinkedPropertyReconcileOutcome.NotResolvable, result.Outcome);
        }

        [Fact]
        public void UN_REGISTRO_ILEGIBLE_DEJA_CERO_RESULTADO()
        {
            var result = Reconcile(
                Authored(4.0),
                editedClearance: 9.0,
                LinkedPropertyEditState.Literal(9.0),
                ProjectVariablesReadResult.Unreadable("registro corrupto"));

            Assert.False(result.IsSuccess);
            Assert.Null(result.Authored);
            Assert.Equal(LinkedPropertyReconcileOutcome.RegistryNotUsable, result.Outcome);
        }

        [Fact]
        public void UN_REGISTRO_CON_VariableId_DUPLICADO_DEJA_CERO_RESULTADO()
        {
            var document = ProjectVariablesDocument.CreateNew();
            document.Variables = new List<ProjectVariableDocument>
            {
                Entry(IdX, "Primera", 10.0),
                Entry(IdX, "Segunda", 20.0),
            };

            var result = Reconcile(
                Authored(4.0),
                editedClearance: 4.0,
                LinkedPropertyEditState.Reference(4.0, Id(IdX)),
                ProjectVariablesReadResult.Readable(document));

            Assert.False(result.IsSuccess);
            Assert.Equal(LinkedPropertyReconcileOutcome.RegistryNotUsable, result.Outcome);
        }

        [Fact]
        public void VINCULAR_A_UNA_VARIABLE_INEXISTENTE_DEJA_CERO_RESULTADO_Y_NO_CREA_UN_VINCULO_ROTO()
        {
            var ausente = "99999999-8888-7777-6666-555555555555";

            var result = Reconcile(
                Authored(4.0), editedClearance: 4.0, LinkedPropertyEditState.Reference(4.0, Id(ausente)));

            Assert.False(result.IsSuccess);
            Assert.Null(result.Authored);
        }

        [Fact]
        public void UNA_PROPIEDAD_DESCONOCIDA_EN_EL_ESTADO_FINAL_SE_RECHAZA()
        {
            var final = new Dictionary<PropertyId, LinkedPropertyEditState>
            {
                [PropertyId.Parse("selective.noExiste")] = LinkedPropertyEditState.Literal(9.0),
            };

            var result = LinkedPropertyReconciler.Reconcile(
                Authored(4.0), Diseno(9.0), final, Registro());

            Assert.False(result.IsSuccess);
            Assert.Equal(LinkedPropertyReconcileOutcome.InvalidFinalState, result.Outcome);
        }

        // ================================================================ la autoridad del campo vinculado

        [Fact]
        public void EL_DISENO_EDITADO_NO_PUEDE_PISAR_EL_LITERAL_CONGELADO_DE_UNA_PROPIEDAD_VINCULADA()
        {
            // Este es el riesgo de BuildDesign: el control muestra el EFECTIVO (10), asi que el diseno que la
            // ventana construye trae 10 en ese campo. Si ese 10 llegara al documento, guardar un rack vinculado
            // y no tocado reescribiria su literal congelado con el valor de la variable -y el dia que se
            // desvincule gobernaria 10 en lugar del 4 que el usuario congelo.
            var result = Reconcile(
                Authored(4.0, IdX), editedClearance: 10.0, LinkedPropertyEditState.Reference(4.0, Id(IdX)));

            Assert.True(result.IsSuccess);
            Assert.Equal(4.0, result.Authored.VerticalClearance);
        }

        [Fact]
        public void EL_ESTADO_FINAL_Y_NO_EL_CARRIER_DECIDE_EL_LITERAL_CONGELADO()
        {
            // DISCRIMINADOR. El carrier heredado (WithDesign) ya conserva el literal congelado de la holgura
            // cuando hay vinculo, asi que casi cualquier escenario pasaria por dos razones distintas y no se
            // sabria cual. Este no: el usuario estaba vinculado a X con 4 congelado, comprometio 7 con Enter y
            // volvio a seleccionar X, asi que el estado final es Reference(7, X).
            //
            // Si el authored lo decidiera el carrier, quedaria 4. Solo queda 7 si la autoridad de ESA propiedad
            // es el estado final declarado.
            var result = Reconcile(
                Authored(4.0, IdX), editedClearance: 10.0, LinkedPropertyEditState.Reference(7.0, Id(IdX)));

            Assert.True(result.IsSuccess);
            Assert.Equal(7.0, result.Authored.VerticalClearance);
            Assert.True(result.Authored.TryGetBinding(Clearance, out var bound));
            Assert.Equal(Id(IdX), bound);

            // Y el efectivo lo sigue dando la variable, no el congelado.
            Assert.Equal(10.0, result.Effective.VerticalClearance);
        }

        [Fact]
        public void EL_DISENO_EDITADO_SI_GOBIERNA_LAS_PROPIEDADES_NO_VINCULABLES()
        {
            // La autoridad del estado final es SOLO para su propiedad. El resto del diseno sigue siendo del
            // editor normal.
            var editado = Diseno(9.0);
            editado.PalletTolerance = 5.5;
            editado.PostPeralte = 3.25;

            var result = LinkedPropertyReconciler.Reconcile(
                Authored(4.0), editado, Final(LinkedPropertyEditState.Literal(9.0)), Registro());

            Assert.True(result.IsSuccess);
            Assert.Equal(5.5, result.Authored.PalletTolerance);
            Assert.Equal(3.25, result.Authored.PostPeralte);
            Assert.Equal(5.5, result.Effective.PalletTolerance);
        }

        [Fact]
        public void RECONCILIAR_CONSERVA_EL_SCHEMA_PROMOVIDO_Y_LA_IDENTIDAD()
        {
            var inicial = Authored(4.0, IdX);

            var result = Reconcile(inicial, 10.0, LinkedPropertyEditState.Reference(4.0, Id(IdY)));

            Assert.True(result.IsSuccess);
            Assert.Equal(SelectivePalletDesignDocument.PromotedSchemaVersion, result.Authored.SchemaVersion);
            Assert.Equal(RackId, result.Authored.Id);
        }

        [Fact]
        public void SIN_ESTADOS_FINALES_RECONCILIAR_ES_EL_CAMINO_NORMAL_DEL_EDITOR()
        {
            // Un rack sin ninguna propiedad vinculable editada: el reconciler no puede ser un camino aparte que
            // haya que recordar activar.
            var result = LinkedPropertyReconciler.Reconcile(
                Authored(4.0),
                Diseno(9.0),
                new Dictionary<PropertyId, LinkedPropertyEditState>(),
                Registro());

            Assert.True(result.IsSuccess);
            Assert.Equal(9.0, result.Authored.VerticalClearance);
            Assert.Equal(9.0, result.Effective.VerticalClearance);
        }
    }
}
