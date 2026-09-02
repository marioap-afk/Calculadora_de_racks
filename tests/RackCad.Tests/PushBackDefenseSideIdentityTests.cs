using System.Linq;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (ronda 7D) — la IDENTIDAD «lado + linea» de una intencion de defensa, probada por si misma.
    ///
    /// <para>
    /// El contrato: en un compuesto <c>A/Pn</c> y <c>B/Pn</c> comparten la linea y son dos intenciones distintas,
    /// porque son dos caras de ataque con su propio pasillo. Escribir una nunca puede tocar la otra.
    /// </para>
    /// </summary>
    public sealed class PushBackDefenseSideIdentityTests
    {
        private static SafetyPostDefense Blank(int post)
            => new SafetyPostDefense { PostIndex = post, ExitAuto = true, EntranceAuto = true };

        [Fact]
        public void SideA_IsTheNearEnd_AndSideB_TheFarEnd()
        {
            Assert.False(PushBackDefenseSides.IsFarEnd(PushBackSide.A));
            Assert.True(PushBackDefenseSides.IsFarEnd(PushBackSide.B));
        }

        [Fact]
        public void WritingOneSide_LeavesTheOtherExactlyAsItWas()
        {
            var record = Blank(0);
            PushBackDefenseSides.Set(record, PushBackSide.A, 36.0, auto: false);

            Assert.Equal(36.0, PushBackDefenseSides.LengthOf(record, PushBackSide.A), 9);
            Assert.False(PushBackDefenseSides.AutoOf(record, PushBackSide.A));
            Assert.True(PushBackDefenseSides.AutoOf(record, PushBackSide.B));
            Assert.Equal(0.0, PushBackDefenseSides.LengthOf(record, PushBackSide.B), 9);
        }

        [Fact]
        public void TheTwoSidesOfOneLine_HoldOppositeDecisions()
        {
            var record = Blank(1);
            PushBackDefenseSides.Set(record, PushBackSide.A, 0.0, auto: false);    // A apagado
            PushBackDefenseSides.Set(record, PushBackSide.B, 36.0, auto: false);   // B encendido

            Assert.Equal(0.0, PushBackDefenseSides.LengthOf(record, PushBackSide.A), 9);
            Assert.Equal(36.0, PushBackDefenseSides.LengthOf(record, PushBackSide.B), 9);
        }

        /// <summary>La fusion de un lado no toca el otro, aunque el registro no existiera antes.</summary>
        [Fact]
        public void Merge_WritesOnlyItsOwnSide()
        {
            var target = new[] { Blank(0) }.ToList();
            PushBackDefenseSides.Set(target[0], PushBackSide.B, 24.0, auto: false);

            var edited = new[] { Blank(0) }.ToList();
            PushBackDefenseSides.Set(edited[0], PushBackSide.A, 0.0, auto: false);

            var merged = PushBackDefenseSides.Merge(target, edited, PushBackSide.A);
            var record = Assert.Single(merged);

            Assert.Equal(0.0, PushBackDefenseSides.LengthOf(record, PushBackSide.A), 9);
            Assert.False(PushBackDefenseSides.AutoOf(record, PushBackSide.A));
            Assert.Equal(24.0, PushBackDefenseSides.LengthOf(record, PushBackSide.B), 9);
        }

        /// <summary>
        /// Un registro NUEVO nace con la otra cara AUTOMATICA: crearlo para decidir un lado no puede convertir el
        /// otro en un cero explicito, que es «sin registro» leido al reves.
        /// </summary>
        [Fact]
        public void Merge_KeepsTheUntouchedSideAutomatic()
        {
            var edited = new[] { Blank(2) }.ToList();
            PushBackDefenseSides.Set(edited[0], PushBackSide.A, 0.0, auto: false);

            var record = Assert.Single(PushBackDefenseSides.Merge(null, edited, PushBackSide.A));

            Assert.True(PushBackDefenseSides.AutoOf(record, PushBackSide.B));
        }

        /// <summary>Una fila que volvio a ser automatica en las dos caras no es ninguna decision: se retira.</summary>
        [Fact]
        public void Merge_DropsARowThatIsAutomaticOnBothSides()
        {
            var target = new[] { Blank(0) }.ToList();
            PushBackDefenseSides.Set(target[0], PushBackSide.A, 0.0, auto: false);

            var edited = new[] { Blank(0) }.ToList();   // vuelve a automatico
            Assert.Empty(PushBackDefenseSides.Merge(target, edited, PushBackSide.A));
        }

        [Fact]
        public void Merge_DoesNotShareObjectsWithItsInputs()
        {
            var target = new[] { Blank(0) }.ToList();
            PushBackDefenseSides.Set(target[0], PushBackSide.A, 12.0, auto: false);

            var merged = PushBackDefenseSides.Merge(target, null, PushBackSide.B);

            Assert.NotSame(target[0], Assert.Single(merged));
        }

        /// <summary>La longitud resuelta de cada lado es la de SU extremo.</summary>
        [Fact]
        public void Resolved_ReadsTheEndOfItsOwnSide()
        {
            var setting = new DynamicForkliftDefenseSetting(exitLength: 12.0, entranceLength: 36.0);

            Assert.Equal(12.0, PushBackDefenseSides.Resolved(setting, PushBackSide.A), 9);
            Assert.Equal(36.0, PushBackDefenseSides.Resolved(setting, PushBackSide.B), 9);
        }

        // ==================== I-42 (ronda 7E): el TIPO, por lado ====================

        /// <summary>
        /// El contrato admite UN TIPO DISTINTO POR LADO. Hoy el catalogo ofrece una sola defensa, y por eso esta
        /// prueba usa dos ids de prueba: lo que fija es que el modelo no supone que solo haya uno.
        /// </summary>
        [Fact]
        public void TheTwoFaces_CanCarryDifferentPieces()
        {
            var selection = new SelectiveSafetySelection { ElementId = "DEFENSA_HISTORICA" };
            PushBackDefenseSides.DeclareFaces(selection, "DEFENSA_X", "DEFENSA_Y");

            Assert.Equal("DEFENSA_X", selection.ElementIdForFace(farEnd: false));
            Assert.Equal("DEFENSA_Y", selection.ElementIdForFace(farEnd: true));
        }

        /// <summary>Una cara sin declarar HEREDA la pieza de la seleccion: el comportamiento historico, intacto.</summary>
        [Fact]
        public void AnUndeclaredFace_InheritsTheSelectionPiece()
        {
            var selection = new SelectiveSafetySelection { ElementId = "DEFENSA_HISTORICA" };

            Assert.Equal("DEFENSA_HISTORICA", selection.ElementIdForFace(farEnd: false));
            Assert.Equal("DEFENSA_HISTORICA", selection.ElementIdForFace(farEnd: true));

            // Y declarar NULL —«este lado nunca eligio»— sigue siendo heredar, no apagar.
            PushBackDefenseSides.DeclareFaces(selection, null, null);
            Assert.Equal("DEFENSA_HISTORICA", selection.ElementIdForFace(farEnd: false));
            Assert.Equal("DEFENSA_HISTORICA", selection.ElementIdForFace(farEnd: true));
        }

        /// <summary>«Ninguno» NO es una pieza: esa cara no resuelve ningun id, asi que no hay bloque ni BOM.</summary>
        [Fact]
        public void ANoneFace_ResolvesToNoPieceAtAll()
        {
            var selection = new SelectiveSafetySelection { ElementId = "DEFENSA_HISTORICA" };
            PushBackDefenseSides.DeclareFaces(selection, PushBackDefaults.NonePieceId, "DEFENSA_Y");

            Assert.Null(selection.ElementIdForFace(farEnd: false));
            Assert.Equal("DEFENSA_Y", selection.ElementIdForFace(farEnd: true));
            Assert.NotEqual(PushBackDefaults.NonePieceId, selection.ElementIdForFace(farEnd: false));
        }

        [Fact]
        public void BothNone_ResolveToNoPiece()
        {
            var selection = new SelectiveSafetySelection { ElementId = "DEFENSA_HISTORICA" };
            PushBackDefenseSides.DeclareFaces(selection, PushBackDefaults.NonePieceId, PushBackDefaults.NonePieceId);

            Assert.Null(selection.ElementIdForFace(farEnd: false));
            Assert.Null(selection.ElementIdForFace(farEnd: true));
        }

        [Fact]
        public void IsNone_RecognisesTheSentinel_AndNothingElse()
        {
            Assert.True(PushBackDefenseSides.IsNone(PushBackDefaults.NonePieceId));
            Assert.True(PushBackDefenseSides.IsNone(" (NINGUNO) "));
            Assert.False(PushBackDefenseSides.IsNone(null));
            Assert.False(PushBackDefenseSides.IsNone("DEFENSA_MONTACARGAS"));
        }

        /// <summary>Las caras viajan en la copia profunda: si no, el resolver las perderia al cruzar a la estructura.</summary>
        [Fact]
        public void TheDeclaredFaces_SurviveDeepCopy()
        {
            var selection = new SelectiveSafetySelection { ElementId = "DEFENSA_HISTORICA" };
            PushBackDefenseSides.DeclareFaces(selection, PushBackDefaults.NonePieceId, "DEFENSA_Y");

            var copy = selection.DeepCopy();

            Assert.Null(copy.ElementIdForFace(farEnd: false));
            Assert.Equal("DEFENSA_Y", copy.ElementIdForFace(farEnd: true));
        }

        /// <summary>
        /// Sin estructura resuelta la aplicabilidad es FAIL-OPEN: una superficie de edicion no deshabilita una fila
        /// por una lectura que todavia no existe. La fisica vuelve a filtrar al dibujar.
        /// </summary>
        [Fact]
        public void FacesOf_FailsOpen_WithoutAResolvedStructure()
        {
            Assert.Equal(new[] { true, true, true }, PushBackDefenseSides.FacesOf(null, 3, PushBackSide.A));
            Assert.Equal(new[] { true, true, true }, PushBackDefenseSides.FacesOf(null, 3, PushBackSide.B));
        }
    }
}
