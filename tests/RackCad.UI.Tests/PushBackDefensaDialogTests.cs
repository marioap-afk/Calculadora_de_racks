using System.Linq;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using RackCad.UI;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// PB-008 / PB-010 (I-32) — the forklift-defence dialog Push Back opens.
    ///
    /// PB-008: Push Back is LIFO, so the low end is where loading AND unloading happen. Naming its two ends "Salida"
    /// and "Entrada" described a rack it is not; they are "Entrada/Salida" and "Posterior". The physical mapping does
    /// not move — the low end is still the one the plan calls the exit — only the words the user reads.
    ///
    /// PB-010: an "Auto" box per end, so the 12"/36" rule is recomputed when a post stops being an edge.
    /// </summary>
    public sealed class PushBackDefensaDialogTests
    {
        [Fact]
        public void PushBack_NamesTheEndsEntradaSalidaAndPosterior()
        {
            var r = StaTestRunner.Run(() =>
            {
                var pushBack = new SafetyDefensaGridWindow(
                    "Defensa de montacargas", 3, null, lowEndOnly: true, autoPerEnd: true);
                var dynamicOne = new SafetyDefensaGridWindow("Defensa de montacargas", 3, null);

                return (
                    pbLow: SafetyDialogTestSupport.HasText(pushBack, "Entrada/Salida"),
                    pbHigh: SafetyDialogTestSupport.HasText(pushBack, "Posterior"),
                    pbOldLow: SafetyDialogTestSupport.HasText(pushBack, "Salida"),
                    pbOldHigh: SafetyDialogTestSupport.HasText(pushBack, "Entrada"),
                    dynLow: SafetyDialogTestSupport.HasText(dynamicOne, "Salida"),
                    dynHigh: SafetyDialogTestSupport.HasText(dynamicOne, "Entrada"),
                    dynAuto: SafetyDialogTestSupport.HasText(dynamicOne, "Auto"),
                    pbAuto: SafetyDialogTestSupport.HasText(pushBack, "Auto"));
            });

            Assert.True(r.pbLow);
            Assert.True(r.pbHigh);
            Assert.False(r.pbOldLow);    // the bare "Salida"/"Entrada" headers are gone in Push Back
            Assert.False(r.pbOldHigh);
            Assert.True(r.pbAuto);       // PB-010: the per-end Auto is offered

            // The default path (Dinámico) keeps its own two names and gains no Auto column.
            Assert.True(r.dynLow);
            Assert.True(r.dynHigh);
            Assert.False(r.dynAuto);
        }

        /// <summary>
        /// A brand-new Push Back rack: every post is fully automatic at the low end and OFF at the rear, so the dialog
        /// stores NO record — "no record" is exactly what makes the plan recompute 12/36 as fronts are added.
        /// </summary>
        [Fact]
        public void PushBack_ANewRackStoresNoRecord_AndTheRearStartsOff()
        {
            var r = StaTestRunner.Run(() =>
            {
                var dialog = new SafetyDefensaGridWindow(
                    "Defensa de montacargas", 4, null, lowEndOnly: true, autoPerEnd: true);
                dialog.BuildResultForTest();
                return dialog.Result.ToList();
            });

            Assert.Empty(r);
        }

        /// <summary>
        /// The Dinámico path is byte-identical in DATA too: with no Auto columns it never emits an Auto flag, so its
        /// records keep meaning exactly what they always meant.
        /// </summary>
        [Fact]
        public void Dynamic_NeverEmitsAutoFlags()
        {
            var r = StaTestRunner.Run(() =>
            {
                var current = new[] { new SafetyPostDefense { PostIndex = 1, ExitLength = 18.0, EntranceLength = 24.0 } };
                var dialog = new SafetyDefensaGridWindow("Defensa de montacargas", 3, current);
                dialog.BuildResultForTest();
                return dialog.Result.ToList();
            });

            Assert.Single(r);
            Assert.Equal(18.0, r[0].ExitLength, 6);
            Assert.Equal(24.0, r[0].EntranceLength, 6);
            Assert.False(r[0].ExitAuto);
            Assert.False(r[0].EntranceAuto);
        }

        // ---- PB-010: el diálogo NO puede deducir la procedencia comparando números ----

        /// <summary>
        /// El defecto está en la UI, no en el helper puro: <see cref="DynamicForkliftDefensePlan"/> resuelve
        /// correctamente un registro manual (se comprueba aquí mismo, y pasa desde antes), pero el diálogo lo
        /// DESCARTABA al aceptar porque su número coincidía con el automático del momento.
        ///
        /// Caso: un registro legacy/manual del poste 1 de un rack de 2 postes, 12"/0" con ambos Auto en false. El
        /// poste 1 es orilla, así que el automático de ese instante también vale 12": el diálogo concluía «esto es el
        /// default» y no guardaba nada. Al crecer a 3 postes ese mismo poste pasa a intermedio y, sin registro, el
        /// plan recalcula 36" — se pierde una longitud que el usuario había fijado.
        /// </summary>
        [Fact]
        public void PushBack_AManualRecordThatMatchesTheDefault_IsStillStored()
        {
            var legacy = new[]
            {
                new SafetyPostDefense
                {
                    PostIndex = 1,
                    ExitLength = 12.0,
                    EntranceLength = 0.0,
                    ExitAuto = false,
                    EntranceAuto = false,
                },
            };

            // El helper puro YA hace lo correcto con ese registro: 12" sigue siendo 12" con 3 postes.
            Assert.Equal(12.0, DynamicForkliftDefensePlan.At(legacy, 1, 2).ExitLength, 6);
            Assert.Equal(12.0, DynamicForkliftDefensePlan.At(legacy, 1, 3).ExitLength, 6);

            var stored = StaTestRunner.Run(() =>
            {
                var dialog = new SafetyDefensaGridWindow(
                    "Defensa de montacargas", 2, legacy, lowEndOnly: true, autoPerEnd: true);
                dialog.BuildResultForTest();   // ruta real de Aceptar, sin tocar un solo control
                return dialog.Result.ToList();
            });

            // Lo que el diálogo devuelve tiene que seguir siendo el registro manual, intacto.
            Assert.Single(stored);
            Assert.Equal(1, stored[0].PostIndex);
            Assert.Equal(12.0, stored[0].ExitLength, 6);
            Assert.Equal(0.0, stored[0].EntranceLength, 6);
            Assert.False(stored[0].ExitAuto);
            Assert.False(stored[0].EntranceAuto);

            // Y con ese resultado, crecer a 3 postes NO puede recalcular a 36".
            Assert.Equal(12.0, DynamicForkliftDefensePlan.At(stored, 1, 3).ExitLength, 6);
        }

        /// <summary>
        /// La cara contraria del mismo defecto: un poste INTERMEDIO cuya longitud manual coincide con el automático
        /// de 36" también se perdía, y al reducir el rack pasaba a orilla y se recalculaba a 12".
        /// </summary>
        [Fact]
        public void PushBack_AManualIntermediateThatMatchesTheDefault_KeepsItsLengthWhenItBecomesAnEdge()
        {
            var manual = new[]
            {
                new SafetyPostDefense
                {
                    PostIndex = 1,
                    ExitLength = 36.0,
                    EntranceLength = 0.0,
                    ExitAuto = false,
                    EntranceAuto = false,
                },
            };

            Assert.Equal(36.0, DynamicForkliftDefensePlan.At(manual, 1, 3).ExitLength, 6);

            var stored = StaTestRunner.Run(() =>
            {
                var dialog = new SafetyDefensaGridWindow(
                    "Defensa de montacargas", 3, manual, lowEndOnly: true, autoPerEnd: true);
                dialog.BuildResultForTest();
                return dialog.Result.ToList();
            });

            Assert.Single(stored);
            Assert.Equal(36.0, stored[0].ExitLength, 6);
            Assert.False(stored[0].ExitAuto);

            // Reducir a 2 postes convierte ese poste en orilla: la longitud manual tiene que sobrevivir.
            Assert.Equal(36.0, DynamicForkliftDefensePlan.At(stored, 1, 2).ExitLength, 6);
        }

        /// <summary>
        /// Un extremo Auto y el otro manual sigue guardándose (es un registro mixto), y ambos extremos en Auto
        /// siguen sin guardar nada — que es lo que hace que el plan recalcule.
        /// </summary>
        [Fact]
        public void PushBack_MixedEnds_AreStored_AndFullyAutomaticOnesAreNot()
        {
            var mixed = new[]
            {
                new SafetyPostDefense { PostIndex = 1, ExitAuto = true, EntranceLength = 20.0, EntranceAuto = false },
                new SafetyPostDefense { PostIndex = 2, ExitAuto = true, EntranceAuto = true },
            };

            var stored = StaTestRunner.Run(() =>
            {
                var dialog = new SafetyDefensaGridWindow(
                    "Defensa de montacargas", 4, mixed, lowEndOnly: true, autoPerEnd: true);
                dialog.BuildResultForTest();
                return dialog.Result.ToList();
            });

            var mixedRow = Assert.Single(stored.Where(row => row.PostIndex == 1));
            Assert.True(mixedRow.ExitAuto);
            Assert.False(mixedRow.EntranceAuto);
            Assert.Equal(20.0, mixedRow.EntranceLength, 6);

            Assert.DoesNotContain(stored, row => row.PostIndex == 2);   // fully automatic => no record
        }

        /// <summary>
        /// Aislamiento: en el camino del Dinámico (sin Auto) se conserva la heurística histórica — un poste cuyos dos
        /// extremos coinciden con el automático NO se guarda. Cambiar eso sería cambiar el Dinámico.
        /// </summary>
        [Fact]
        public void Dynamic_StillDropsARecordThatEqualsItsDefault()
        {
            var atDefault = new[] { new SafetyPostDefense { PostIndex = 1, ExitLength = 36.0, EntranceLength = 36.0 } };

            var stored = StaTestRunner.Run(() =>
            {
                var dialog = new SafetyDefensaGridWindow("Defensa de montacargas", 3, atDefault);
                dialog.BuildResultForTest();
                return dialog.Result.ToList();
            });

            Assert.Empty(stored);
        }
    }
}
