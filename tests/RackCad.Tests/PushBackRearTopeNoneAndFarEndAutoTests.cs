using System.Linq;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (ronda 7C) — las dos autoridades que la correccion de la UI necesitaba, probadas por si mismas.
    ///
    /// <para><b>«Ninguno».</b> <see cref="PushBackRearTopeConfig.Draws"/> es LA pregunta fisica —la que hacen el
    /// dibujo y el BOM—, y separa dos alcances que conviven: la mascara POR CELDA, que es del usuario, y la decision
    /// de OBJETIVO, que dice que ahi no va tope. Elegir «Ninguno» no borra la mascara, asi que es reversible.</para>
    ///
    /// <para><b>El automatico lejano.</b> <c>RestrictToLowEnd</c> borraba la marca de automatico del extremo lejano
    /// de todo registro. Se justificaba en que «un automatico lejano volveria a 12/36», y eso dejo de ser cierto en
    /// cuanto PB-009 llego al plan: hoy <see cref="DynamicForkliftDefensePlan"/> resuelve ese automatico a CERO por
    /// la marca <c>LowEndOnly</c>. Borrarlo no defendia de nada y, en un rack compuesto —donde ese extremo es un
    /// pasillo de verdad—, lo convertia en un cero explicito que ya no volvia.</para>
    /// </summary>
    public sealed class PushBackRearTopeNoneAndFarEndAutoTests
    {
        // ==================== «Ninguno» ====================

        [Fact]
        public void None_IsAnExplicitValue_NotABlank()
        {
            Assert.False(new PushBackRearTopeConfig().IsNone);
            Assert.False(new PushBackRearTopeConfig { PieceId = null }.IsNone);
            Assert.False(new PushBackRearTopeConfig { PieceId = "LARGUERO_ESCALON_TOPE_DE_3" }.IsNone);
            Assert.True(new PushBackRearTopeConfig { PieceId = PushBackRearTopeConfig.NonePieceId }.IsNone);
            Assert.True(new PushBackRearTopeConfig { PieceId = " (NINGUNO) " }.IsNone);
        }

        [Fact]
        public void None_StopsEveryCellFromDrawing()
        {
            var config = new PushBackRearTopeConfig { PieceId = PushBackRearTopeConfig.NonePieceId };
            Assert.False(config.Draws(0, 0));
            Assert.False(config.Draws(2, 1));
        }

        /// <summary>Sin «Ninguno», <c>Draws</c> es exactamente la mascara de siempre.</summary>
        [Fact]
        public void WithoutNone_DrawsIsTheCellMask()
        {
            var config = new PushBackRearTopeConfig();
            config.Disable(1, 0);

            Assert.True(config.Draws(0, 0));
            Assert.False(config.Draws(1, 0));
            Assert.Equal(config.At(0, 0), config.Draws(0, 0));
            Assert.Equal(config.At(1, 0), config.Draws(1, 0));
        }

        /// <summary>«Ninguno» NO borra la mascara: es lo que hace reversible la decision.</summary>
        [Fact]
        public void None_DoesNotEraseThePerCellMask()
        {
            var config = new PushBackRearTopeConfig { PieceId = PushBackRearTopeConfig.NonePieceId };
            config.Disable(1, 0);

            Assert.False(config.At(1, 0));
            Assert.True(config.At(0, 0));   // la mascara sigue diciendo lo suyo aunque nada se dibuje

            config.PieceId = null;          // de vuelta a una pieza
            Assert.True(config.Draws(0, 0));
            Assert.False(config.Draws(1, 0));
        }

        /// <summary>Viaja en la copia profunda, como el SAQUE y las celdas.</summary>
        [Fact]
        public void None_SurvivesDeepCopy()
        {
            var config = new PushBackRearTopeConfig { PieceId = PushBackRearTopeConfig.NonePieceId };
            config.Disable(0, 1);
            var copy = config.DeepCopy();

            Assert.True(copy.IsNone);
            Assert.False(copy.At(0, 1));
        }

        // ==================== el automatico del extremo lejano ====================

        /// <summary>
        /// La autoridad conserva la marca de automatico del extremo lejano. Antes la borraba, y con ella la unica
        /// forma de que ese extremo volviera a resolverse solo.
        /// </summary>
        [Fact]
        public void RestrictToLowEnd_KeepsTheFarEndAutomatic()
        {
            var selection = new SelectiveSafetySelection { ElementId = "DEFENSA_MONTACARGAS" };
            selection.DefensaPosts.Add(new SafetyPostDefense
            {
                PostIndex = 1,
                ExitLength = 0.0,
                ExitAuto = false,
                EntranceLength = 0.0,
                EntranceAuto = true,
            });

            PushBackSafetyAuthority.RestrictToLowEnd(selection);

            Assert.True(selection.LowEndOnly);
            Assert.True(selection.DefensaPosts.Single().EntranceAuto);
        }

        /// <summary>
        /// Y conservarla no reabre el extremo lejano de un rack de UN SENTIDO: el plan ya lo resuelve a cero por la
        /// marca <c>LowEndOnly</c>, que es lo que PB-009 introdujo. Esta es la defensa que el borrado imitaba.
        /// </summary>
        [Fact]
        public void AFarEndAutomatic_StillResolvesToZero_InASingleEndedRack()
        {
            var selection = new SelectiveSafetySelection { ElementId = "DEFENSA_MONTACARGAS" };
            selection.DefensaPosts.Add(new SafetyPostDefense
            {
                PostIndex = 1,
                ExitLength = 0.0,
                ExitAuto = false,
                EntranceLength = 0.0,
                EntranceAuto = true,
            });
            PushBackSafetyAuthority.RestrictToLowEnd(selection);

            var setting = DynamicForkliftDefensePlan.ForSelection(selection, 1, 4);

            Assert.False(setting.DrawsExit);       // apagado a mano
            Assert.False(setting.DrawsEntrance);   // y el lejano sigue apagado por la regla, no por el borrado
        }

        /// <summary>
        /// En un rack COMPUESTO, donde esa linea tiene de verdad su segunda cara de carga (6D), el automatico
        /// lejano vuelve a valer: apagar el extremo bajo de un poste ya no apaga tambien el pasillo del otro lado.
        /// </summary>
        [Fact]
        public void AFarEndAutomatic_ComesBack_WhenTheLineHasASecondLoadFace()
        {
            var selection = new SelectiveSafetySelection { ElementId = "DEFENSA_MONTACARGAS" };
            selection.DefensaPosts.Add(new SafetyPostDefense
            {
                PostIndex = 1,
                ExitLength = 0.0,
                ExitAuto = false,
                EntranceLength = 0.0,
                EntranceAuto = true,
            });
            PushBackSafetyAuthority.RestrictToLowEnd(selection);
            selection.BothEndsAreLoadFaces = true;

            var setting = DynamicForkliftDefensePlan.ForSelection(selection, 1, 4);

            Assert.False(setting.DrawsExit);
            Assert.Equal(DynamicForkliftDefensePlan.IntermediateLength, setting.EntranceLength, 9);
        }

        /// <summary>Un extremo lejano que el usuario fijo A MANO se sigue honrando tal cual: no es una prohibicion.</summary>
        [Fact]
        public void AnExplicitFarEnd_IsStillHonoured()
        {
            var selection = new SelectiveSafetySelection { ElementId = "DEFENSA_MONTACARGAS" };
            selection.DefensaPosts.Add(new SafetyPostDefense
            {
                PostIndex = 1,
                ExitAuto = true,
                EntranceLength = 20.0,
                EntranceAuto = false,
            });
            PushBackSafetyAuthority.RestrictToLowEnd(selection);

            var setting = DynamicForkliftDefensePlan.ForSelection(selection, 1, 4);

            Assert.Equal(20.0, setting.EntranceLength, 9);
        }
    }
}
