using System;
using System.Security.Cryptography;
using System.Text;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems.Shared;
using Xunit;
using static RackCad.Tests.DimensionViewScenarios;

namespace RackCad.Tests
{
    /// <summary>
    /// I-50, T-11 y T-12 (G5, guarda) — un rack LEGACY (sin política) se escribe EXACTAMENTE igual que antes de que
    /// existiera el campo. Los pines se capturaron sobre <c>11c04db</c>, ANTES de declarar <c>DimensionViews</c> en los
    /// diseños y en los DTO: si uno se mueve, el campo nuevo cambió el JSON de un rack que no lo usa.
    ///
    /// <para>
    /// Cada pin es la longitud y el SHA-256 del JSON que producen los stores reales: el documento embebido del
    /// Selectivo y el proyecto (<c>RackProjectStore</c>) del Dinámico y de Push Back. NO se re-fijan por conveniencia.
    /// </para>
    ///
    /// <para>
    /// El proyecto se escribe indentado con el salto de línea de la plataforma (CR LF en Windows, LF en ubuntu), así
    /// que el pin se toma con los saltos normalizados a LF y vale igual en las dos. La normalización no oculta nada:
    /// dentro de una cadena JSON un CR va escapado, de modo que CR LF solo puede ser espacio entre tokens.
    /// </para>
    /// </summary>
    public class DimensionViewsLegacyJsonTests
    {
        private static string Pin(string json)
        {
            var lf = json.Replace("\r\n", "\n", StringComparison.Ordinal);
            return lf.Length + ":" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(lf)));
        }

        private static void AssertPin(string expected, string json)
        {
            var actual = Pin(json);
            Assert.True(string.Equals(expected, actual, StringComparison.Ordinal), "pin del JSON legacy movido; actual: " + actual);
        }

        [Fact]
        public void T11_TheSelectiveLegacyDocument_IsWrittenByteForByteAsBefore()
        {
            var design = SelectiveDesign(DimensionDetail.Standard, twoFondos: true);
            var json = new SelectivePalletDesignStore().Serialize(SelectivePalletDesignDocument.From(design, "I50-ID", RackName));

            Assert.DoesNotContain("DimensionViews", json, StringComparison.OrdinalIgnoreCase);
            AssertPin(SelectiveLegacyPin, json);
        }

        [Fact]
        public void T12_TheDynamicLegacyProject_IsWrittenByteForByteAsBefore()
        {
            var design = DynamicDesign(DimensionDetail.Standard, Catalog);
            var json = new RackProjectStore().Serialize(RackProject.ForDynamic(design));

            Assert.DoesNotContain("DimensionViews", json, StringComparison.OrdinalIgnoreCase);
            AssertPin(DynamicLegacyPin, json);
        }

        [Fact]
        public void T12_ThePushBackLegacyProject_IsWrittenByteForByteAsBefore()
        {
            var design = PushBackSingleSidedDesign(DimensionDetail.Standard);
            var json = new RackProjectStore().Serialize(RackProject.ForPushBack(design));

            Assert.DoesNotContain("DimensionViews", json, StringComparison.OrdinalIgnoreCase);
            AssertPin(PushBackLegacyPin, json);
        }

        // Capturados sobre 11c04db, antes de declarar el campo. El JSON del Selectivo es compacto y no tiene saltos. Los
        // del Dinámico y Push Back se capturaron en Windows con CR LF (6481:5B5972CC… y 3289:F0728285…); estos son los
        // mismos JSON en LF, 227 y 120 saltos menos, tal como los escribió ubuntu en el CI de 94220fb.
        private const string SelectiveLegacyPin = "2708:B21C3A100CA6EDBDE3BD4924E86D2AF602EABEB8227CB8FA7B0C2D78187FF4DB";
        private const string DynamicLegacyPin = "6254:D0FFE8DB6110E36996F1A7B726AD06294FB8D0B3C6E3DD02B51AA226D6F6D68C";
        private const string PushBackLegacyPin = "3169:15BC9518A5354A19C05D15A4A6177576E04AB225E5BB2743195F064AA16EC11E";
    }
}
