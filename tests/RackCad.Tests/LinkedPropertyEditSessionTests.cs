using System.Collections.Generic;
using RackCad.Application.ProjectVariables;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-48 gate G4C — la SEMANTICA de edicion de una propiedad vinculable, sin WPF.
    ///
    /// <para>
    /// Todo lo que las ocho revisiones discutieron se decide aqui: Enter, Escape, LostFocus, el <c>=</c>
    /// pelado, la negativa a auto-seleccionar un unico candidato y el congelado de 20.13. Si esas reglas
    /// vivieran en el control, harian falta un message pump y una ventana real para ejercitarlas, y una regla
    /// que no se puede ejercitar es una regla que se desvia.
    /// </para>
    /// <para>
    /// Las dos asimetrias que estas pruebas fijan:
    /// </para>
    /// <list type="number">
    /// <item><b><c>LostFocus</c> NUNCA cambia la fuente</b> (V3-R06). Puede comprometer un literal sobre un
    /// literal, porque eso conserva la fuente; no puede crear, sustituir ni retirar una referencia.</item>
    /// <item><b>Una referencia se compromete SOLO por seleccion explicita</b> (V3-R08). El texto filtra. Enter
    /// sin candidato no resuelve: ni por nombre, ni tomando «el unico», ni por mejor coincidencia.</item>
    /// </list>
    /// <para>
    /// Y la regla de 20.13, que es UNA sola: <c>Link</c> congela el ultimo literal COMPROMETIDO, no el ultimo
    /// texto escrito. Los dos casos del contrato se prueban por separado porque difieren solo en si hubo commit
    /// en medio.
    /// </para>
    /// </summary>
    public class LinkedPropertyEditSessionTests
    {
        private const string IdX = "11111111-1111-1111-1111-111111111111";
        private const string IdY = "22222222-2222-2222-2222-222222222222";

        private static VariableId Id(string guid) => VariableId.Parse(guid);

        private static LinkedPropertyOption Option(string guid, string name, double value)
            => new LinkedPropertyOption(Id(guid), name, VariableType.Length, value);

        private static IReadOnlyList<LinkedPropertyOption> DosOpciones()
            => new[] { Option(IdX, "Holgura General", 10.0), Option(IdY, "Holgura Estrecha", 12.0) };

        private static LinkedPropertyEditSession Literal(double value, IReadOnlyList<LinkedPropertyOption> options = null)
            => new LinkedPropertyEditSession(LinkedPropertyEditState.Literal(value), options ?? DosOpciones());

        private static LinkedPropertyEditSession Vinculada(
            double frozen, string guid, IReadOnlyList<LinkedPropertyOption> options = null)
            => new LinkedPropertyEditSession(
                LinkedPropertyEditState.Reference(frozen, Id(guid)), options ?? DosOpciones());

        // ================================================================ 20.13, las dos variantes

        [Fact]
        public void CASO_1_UN_LITERAL_TECLEADO_Y_NO_COMPROMETIDO_NO_ES_EL_QUE_SE_CONGELA()
        {
            // inicial 4 · el usuario escribe 7 · NO lo compromete · selecciona Reference(X) → congela 4.
            var session = Literal(4.0);

            session.Type("7");

            // Mientras no se compromete, el estado comprometido NO se mueve.
            Assert.Equal(4.0, session.Committed.CommittedLiteral);

            Assert.True(session.TrySelect(Id(IdX), out _));

            Assert.True(session.Committed.IsReference);
            Assert.Equal(Id(IdX), session.Committed.Source.VariableId);
            Assert.Equal(4.0, session.Committed.CommittedLiteral);
        }

        [Fact]
        public void CASO_2_UN_LITERAL_COMPROMETIDO_CON_ENTER_SI_ES_EL_QUE_SE_CONGELA()
        {
            // La frontera es COMMIT vs DRAFT, no el orden temporal: una sola regla, sin caso especial.
            var session = Literal(4.0);

            session.Type("7");
            Assert.True(session.TryCommitByEnter(out _));
            Assert.Equal(7.0, session.Committed.CommittedLiteral);

            Assert.True(session.TrySelect(Id(IdX), out _));

            Assert.True(session.Committed.IsReference);
            Assert.Equal(7.0, session.Committed.CommittedLiteral);
        }

        [Fact]
        public void CASO_2_BIS_UN_LITERAL_COMPROMETIDO_POR_LOSTFOCUS_TAMBIEN_SE_CONGELA()
        {
            // LostFocus SI puede comprometer Literal -> Literal, porque no cambia la fuente.
            var session = Literal(4.0);

            session.Type("7");
            Assert.True(session.TryCommitByLostFocus(out _));
            Assert.Equal(7.0, session.Committed.CommittedLiteral);

            Assert.True(session.TrySelect(Id(IdX), out _));
            Assert.Equal(7.0, session.Committed.CommittedLiteral);
        }

        // ================================================================ literal -> literal

        [Fact]
        public void UN_DRAFT_LITERAL_NO_MUEVE_EL_COMPROMETIDO_HASTA_QUE_SE_COMPROMETE()
        {
            var session = Literal(4.0);

            session.Type("7");

            Assert.Equal(LinkedPropertyDraftKind.DraftLiteral, session.Draft);
            Assert.True(session.IsDirty);
            Assert.False(session.DraftChangesSource);
            Assert.Equal(4.0, session.Committed.CommittedLiteral);
        }

        [Fact]
        public void UN_TEXTO_INVALIDO_NO_MUTA_NADA_Y_SIGUE_PENDIENTE()
        {
            var session = Literal(4.0);

            session.Type("abc");

            Assert.Equal(LinkedPropertyDraftKind.InvalidDraft, session.Draft);
            Assert.False(session.TryCommitByEnter(out var error));
            Assert.NotNull(error);
            Assert.Equal(4.0, session.Committed.CommittedLiteral);
            Assert.True(session.IsDirty);
        }

        // ================================================================ reference -> literal

        [Fact]
        public void CASO_3_SOBRE_UNA_REFERENCIA_UN_DRAFT_LITERAL_Y_LOSTFOCUS_NO_DESVINCULA()
        {
            // La regla general: LostFocus puede comprometer solo si NO cambia Source.
            var session = Vinculada(4.0, IdX);

            session.Type("7");

            Assert.True(session.DraftChangesSource);
            Assert.False(session.TryCommitByLostFocus(out var error));
            Assert.NotNull(error);

            // Sigue gobernada por X, y el draft sigue pendiente en lugar de perderse en silencio.
            Assert.True(session.Committed.IsReference);
            Assert.Equal(Id(IdX), session.Committed.Source.VariableId);
            Assert.Equal(LinkedPropertyDraftKind.DraftLiteral, session.Draft);
            Assert.True(session.IsDirty);
        }

        [Fact]
        public void CASO_4_SOBRE_UNA_REFERENCIA_UN_DRAFT_LITERAL_CON_ENTER_SI_DESVINCULA()
        {
            // Enter explicito es la unica via, y no hace falta un boton Unlink previo.
            var session = Vinculada(4.0, IdX);

            session.Type("7");
            Assert.True(session.TryCommitByEnter(out _));

            Assert.False(session.Committed.IsReference);
            Assert.Equal(LinkedPropertySourceKind.Literal, session.Committed.Source.Kind);
            Assert.Equal(7.0, session.Committed.CommittedLiteral);
            Assert.False(session.IsDirty);
        }

        // ================================================================ Escape

        [Fact]
        public void CASO_5_ESCAPE_SOBRE_UNA_REFERENCIA_RESTAURA_LA_REFERENCIA_Y_SU_LITERAL_CONGELADO()
        {
            var session = Vinculada(4.0, IdX);

            session.Type("7");
            session.Cancel();

            Assert.True(session.Committed.IsReference);
            Assert.Equal(Id(IdX), session.Committed.Source.VariableId);
            Assert.Equal(4.0, session.Committed.CommittedLiteral);
            Assert.Equal(LinkedPropertyDraftKind.Committed, session.Draft);
            Assert.False(session.IsDirty);
            Assert.Equal("=Holgura General", session.Text);
        }

        [Fact]
        public void ESCAPE_SOBRE_UN_LITERAL_RESTAURA_EL_NUMERO()
        {
            var session = Literal(6.0);

            session.Type("abc");
            session.Cancel();

            Assert.Equal(6.0, session.Committed.CommittedLiteral);
            Assert.Equal("6", session.Text);
            Assert.False(session.IsDirty);
        }

        [Fact]
        public void ESCAPE_DESDE_UNA_CONSULTA_DE_REFERENCIA_RESTAURA_LA_REFERENCIA_ANTERIOR()
        {
            var session = Vinculada(4.0, IdX);

            session.Type("=otr");
            session.Cancel();

            Assert.Equal(Id(IdX), session.Committed.Source.VariableId);
            Assert.Equal(4.0, session.Committed.CommittedLiteral);
        }

        // ================================================================ el "=" pelado y el filtro

        [Theory]
        [InlineData("=")]
        [InlineData("=H")]
        [InlineData("=Holg")]
        [InlineData("=Holgura General")]
        public void CASO_6_UNA_CONSULTA_DE_REFERENCIA_QUEDA_PENDIENTE_Y_NO_COMPROMETE_NINGUNA_REFERENCIA(string texto)
        {
            var session = Literal(4.0);

            session.Type(texto);

            Assert.Equal(LinkedPropertyDraftKind.DraftReferenceQuery, session.Draft);
            Assert.True(session.IsQuerying);
            Assert.True(session.DraftChangesSource);

            // Ni siquiera con el nombre EXACTO: el texto filtra, no resuelve.
            Assert.False(session.Committed.IsReference);
            Assert.Equal(4.0, session.Committed.CommittedLiteral);
        }

        [Fact]
        public void CASO_7_UN_FILTRO_CON_UN_UNICO_CANDIDATO_NO_LO_SELECCIONA()
        {
            var session = Literal(4.0);

            session.Type("=Estrecha");

            // Hay exactamente un candidato...
            Assert.Single(session.Candidates);
            Assert.Equal(Id(IdY), session.Candidates[0].VariableId);

            // ...y aun asi Enter no lo confirma: quedarse solo uno no es una eleccion humana.
            Assert.False(session.TryCommitByEnter(out var error));
            Assert.NotNull(error);
            Assert.False(session.Committed.IsReference);
        }

        [Fact]
        public void CASO_8_ENTER_SIN_CANDIDATO_SELECCIONADO_NO_RESUELVE_POR_TEXTO()
        {
            var session = Literal(4.0);

            session.Type("=Holgura General");

            Assert.False(session.TryCommitByEnter(out _));
            Assert.False(session.Committed.IsReference);
            Assert.Equal(4.0, session.Committed.CommittedLiteral);
        }

        [Fact]
        public void EL_FILTRO_ES_UN_SUBSTRING_INSENSIBLE_A_CAJA_Y_NO_UNA_RESOLUCION()
        {
            var session = Literal(4.0);

            session.Type("=holgura");
            Assert.Equal(2, session.Candidates.Count);

            session.Type("=");
            Assert.Equal(2, session.Candidates.Count);

            session.Type("=nada de esto existe");
            Assert.Empty(session.Candidates);
        }

        // ================================================================ seleccion explicita

        [Fact]
        public void CASO_9_UNA_SELECCION_EXPLICITA_COMPROMETE_EL_VariableId_EXACTO()
        {
            var session = Literal(4.0);

            session.Type("=holgura");
            Assert.True(session.TrySelect(Id(IdY), out _));

            Assert.Equal(Id(IdY), session.Committed.Source.VariableId);
            Assert.Equal("=Holgura Estrecha", session.Text);
        }

        [Fact]
        public void CASO_10_LA_SELECCION_NO_NECESITA_QUE_EL_FILTRO_LA_CONTENGA_PORQUE_LLEGA_POR_IDENTIDAD()
        {
            // Da igual como llego el usuario al candidato -raton o teclado-: lo que viaja es el id.
            var session = Literal(4.0);

            session.Type("=");
            Assert.True(session.TrySelect(Id(IdX), out _));

            Assert.Equal(Id(IdX), session.Committed.Source.VariableId);
        }

        [Fact]
        public void SELECCIONAR_UNA_VARIABLE_QUE_NO_ESTA_OFRECIDA_FALLA_SIN_MUTAR()
        {
            // La compatibilidad la decidio Application al construir las opciones; el editor no la re-decide,
            // pero tampoco acepta una identidad que nadie le ofrecio.
            var session = Literal(4.0, new[] { Option(IdX, "Holgura General", 10.0) });

            Assert.False(session.TrySelect(Id(IdY), out var error));
            Assert.NotNull(error);
            Assert.False(session.Committed.IsReference);
        }

        // ================================================================ reference -> reference

        [Fact]
        public void DE_UNA_REFERENCIA_A_OTRA_SE_CONSERVA_EL_LITERAL_CONGELADO()
        {
            var session = Vinculada(4.0, IdX);

            Assert.True(session.TrySelect(Id(IdY), out _));

            Assert.Equal(Id(IdY), session.Committed.Source.VariableId);
            Assert.Equal(4.0, session.Committed.CommittedLiteral);
        }

        [Fact]
        public void DE_UNA_REFERENCIA_A_LA_MISMA_ES_IDEMPOTENTE()
        {
            var session = Vinculada(4.0, IdX);
            var antes = session.Committed;

            Assert.True(session.TrySelect(Id(IdX), out _));

            Assert.Equal(antes, session.Committed);
            Assert.Equal(4.0, session.Committed.CommittedLiteral);
            Assert.False(session.IsDirty);
        }

        [Fact]
        public void DE_UNA_REFERENCIA_A_OTRA_CON_UN_LITERAL_COMPROMETIDO_EN_MEDIO_CONGELA_EL_NUEVO()
        {
            // Reference(X) -> Literal(7) con Enter -> Reference(Y): el congelado es 7, no 4.
            var session = Vinculada(4.0, IdX);

            session.Type("7");
            Assert.True(session.TryCommitByEnter(out _));
            Assert.True(session.TrySelect(Id(IdY), out _));

            Assert.Equal(Id(IdY), session.Committed.Source.VariableId);
            Assert.Equal(7.0, session.Committed.CommittedLiteral);
        }

        // ================================================================ nombres duplicados

        [Fact]
        public void CASO_11_DOS_HOMONIMAS_CONSERVAN_IDENTIDADES_DISTINTAS_Y_SE_DISTINGUEN_EN_PANTALLA()
        {
            // Mismo nombre Y mismo valor: ni el nombre ni el nombre-mas-valor bastan para que una persona
            // elija, asi que el fragmento de identidad es OBLIGATORIO (V2 R-06).
            var homonimas = new[] { Option(IdX, "Holgura General", 10.0), Option(IdY, "Holgura General", 10.0) };
            var session = Literal(4.0, homonimas);

            session.Type("=Holgura General");

            Assert.Equal(2, session.Candidates.Count);
            Assert.NotEqual(session.Candidates[0].VariableId, session.Candidates[1].VariableId);
            Assert.NotEqual(session.Candidates[0].DisplayText, session.Candidates[1].DisplayText);
            Assert.Contains(session.Candidates[0].Disambiguator, session.Candidates[0].DisplayText);

            // Y seleccionar la SEGUNDA compromete la segunda identidad, no la primera ni la del nombre.
            Assert.True(session.TrySelect(Id(IdY), out _));
            Assert.Equal(Id(IdY), session.Committed.Source.VariableId);
        }

        // ================================================================ Stage / Apply (C4, puro)

        [Fact]
        public void UN_EDITOR_LIMPIO_NO_APORTA_NI_BLOQUEA()
        {
            Assert.Equal(LinkedPropertyStageOutcome.Clean, Literal(4.0).TryStage().Outcome);
        }

        [Fact]
        public void UN_DRAFT_QUE_CONSERVA_LA_FUENTE_SE_PUEDE_ESTACIONAR_Y_APLICAR()
        {
            var session = Literal(4.0);
            session.Type("7");

            var stage = session.TryStage();

            Assert.Equal(LinkedPropertyStageOutcome.Ready, stage.Outcome);

            // Stage NO muta.
            Assert.Equal(4.0, session.Committed.CommittedLiteral);

            session.ApplyStaged();
            Assert.Equal(7.0, session.Committed.CommittedLiteral);
        }

        [Fact]
        public void UN_DRAFT_QUE_CAMBIA_LA_FUENTE_BLOQUEA_LA_FRONTERA_Y_NO_SE_CONVIERTE_SOLO()
        {
            var session = Vinculada(4.0, IdX);
            session.Type("7");

            var stage = session.TryStage();

            Assert.Equal(LinkedPropertyStageOutcome.Blocked, stage.Outcome);
            Assert.False(stage.CanProceed);
            Assert.Contains("Enter", stage.Error);
            Assert.Contains("Escape", stage.Error);

            // Y aplicar despues de un Stage bloqueado no puede convertir la fuente por la puerta de atras.
            session.ApplyStaged();
            Assert.True(session.Committed.IsReference);
            Assert.Equal(Id(IdX), session.Committed.Source.VariableId);
        }

        [Fact]
        public void UNA_CONSULTA_DE_REFERENCIA_PENDIENTE_TAMBIEN_BLOQUEA_LA_FRONTERA()
        {
            var session = Literal(4.0);
            session.Type("=Holg");

            Assert.Equal(LinkedPropertyStageOutcome.Blocked, session.TryStage().Outcome);
            Assert.False(session.Committed.IsReference);
        }

        [Fact]
        public void UN_DRAFT_INVALIDO_BLOQUEA_LA_FRONTERA()
        {
            var session = Literal(4.0);
            session.Type("abc");

            var stage = session.TryStage();

            Assert.Equal(LinkedPropertyStageOutcome.Blocked, stage.Outcome);
            Assert.NotNull(stage.Error);
        }

        // ================================================================ presentacion del comprometido

        [Fact]
        public void UN_LITERAL_SE_MUESTRA_COMO_NUMERO_Y_UNA_REFERENCIA_COMO_IGUAL_MAS_NOMBRE()
        {
            Assert.Equal("6", Literal(6.0).Text);
            Assert.Equal("=Holgura General", Vinculada(4.0, IdX).Text);
        }

        [Fact]
        public void UNA_REFERENCIA_A_UNA_VARIABLE_QUE_NO_ESTA_OFRECIDA_SE_MUESTRA_POR_IDENTIDAD()
        {
            // No puede pasar en produccion -abrir ya exige que resuelva- pero el control no debe inventar un
            // nombre ni ensenar un hueco si pasara.
            var session = new LinkedPropertyEditSession(
                LinkedPropertyEditState.Reference(4.0, Id(IdY)), new[] { Option(IdX, "Holgura General", 10.0) });

            Assert.Contains(IdY, session.Text);
            Assert.StartsWith("=", session.Text);
        }
    }
}
