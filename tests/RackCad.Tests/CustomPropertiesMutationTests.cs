using System;
using System.Linq;
using System.Reflection;
using RackCad.Application.CustomProperties;
using RackCad.Application.Persistence;
using Xunit;
using static RackCad.Tests.CustomPropertiesTestKit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-54 G3 — MUTACIONES PURAS de propiedades personalizadas: T-MUT-01..06 y T-MUT-09 de la Proposal V5 §12.4, bajo
    /// ADR-0039 (aceptado) §1 y §9 (solo la parte pura: el ejecutor, la autoridad por <c>RackId</c> y el commit son G5).
    ///
    /// <para>
    /// Una mutacion recibe la LECTURA —no un documento suelto— y un intent dirigido por identidad. Asi el precondicionado
    /// de escritura es estructural: sin una lectura <c>Absent</c> o <c>Readable</c> no hay documento resultante. Y el
    /// documento leido nunca se modifica: la mutacion devuelve otro.
    /// </para>
    /// <para>
    /// Fuera de este archivo, por gate: la instantanea obsoleta (T-MUT-07), el plan y la atomicidad logica (T-MUT-08) y
    /// el commit de unificacion (T-MUT-10) necesitan la autoridad y el commit de G5.
    /// </para>
    /// </summary>
    public class CustomPropertiesMutationTests
    {
        private static CustomPropertiesReadResult TresEntradas(string version = "1.0")
            => ReadableResult(Doc(
                version,
                Entries(
                    Entry(IdA, "Cliente", "ACME", ",\"EA\":{\"a\":1}"),
                    Entry(IdB, "Obra", "Nave 3", ",\"EB\":[true,2.50]"),
                    Entry(IdC, "Revision", "B", ",\"EC\":\"c\"")),
                ",\"Raiz\":{\"r\":1}"));

        private static void AssertEntradaIntacta(CustomPropertyEntryDocument expected, CustomPropertyEntryDocument actual)
        {
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Value, actual.Value);
            AssertExtensionIgual(expected, actual);
        }

        private static void AssertExtensionIgual(CustomPropertyEntryDocument expected, CustomPropertyEntryDocument actual)
        {
            if (expected.ExtensionData == null)
            {
                Assert.Null(actual.ExtensionData);
                return;
            }

            Assert.Equal(expected.ExtensionData.Keys, actual.ExtensionData.Keys);

            foreach (var key in expected.ExtensionData.Keys)
            {
                Assert.Equal(expected.ExtensionData[key].GetRawText(), actual.ExtensionData[key].GetRawText());
            }
        }

        // ================================================================ T-MUT-01 crear

        [Fact]
        public void TMut01_Crear_IdNuevoEnMinusculasD_AlFinal_NombreNfcRecortado_ValorExacto()
        {
            var current = TresEntradas();

            var created = Apply(current, CustomPropertiesIntent.Create("  " + AreaNfd + " ", "  Norte  "));

            Assert.True(created.Succeeded);
            Assert.Equal(CustomPropertiesRejection.None, created.Rejection);
            Assert.Null(created.Error);
            Assert.False(created.CreatedId.IsEmpty);
            Assert.DoesNotContain(created.CreatedId, new[] { Id(IdA), Id(IdB), Id(IdC) });
            Assert.Equal(4, created.Document.Entries.Count);

            for (var i = 0; i < 3; i++)
            {
                AssertEntradaIntacta(current.Document.Entries[i], created.Document.Entries[i]);
            }

            var nueva = created.Document.Entries[3];

            Assert.Equal(created.CreatedId, Id(nueva.Id));
            Assert.Equal(AreaNfc, nueva.Name);
            Assert.Equal("  Norte  ", nueva.Value);
            Assert.Null(nueva.ExtensionData);

            var serialized = Serialize(created.Document);

            Assert.Contains("\"Id\":\"" + created.CreatedId + "\"", serialized, StringComparison.Ordinal);
            Assert.Equal(created.CreatedId.ToString().ToLowerInvariant(), created.CreatedId.ToString());
            Assert.Equal(created.CreatedId, Id(Readable(serialized).Entries[3].Id));
        }

        [Fact]
        public void TMut01_Crear_DesdeAusente_NaceLaColeccionEnLaVersionActual()
        {
            var created = Apply(AbsentResult(), CustomPropertiesIntent.Create("Cliente", "ACME"));

            Assert.True(created.Succeeded);
            Assert.Equal(CustomPropertiesDocument.CurrentSchemaVersion, created.Document.SchemaVersion);
            Assert.Equal("Cliente", Assert.Single(created.Document.Entries).Name);
            Assert.Null(created.Document.ExtensionData);
        }

        [Fact]
        public void TMut01_Crear_NoModificaLaLecturaDeOrigen()
        {
            var current = TresEntradas();
            var antes = Serialize(current.Document);

            var created = Apply(current, CustomPropertiesIntent.Create("Ubicacion", "Monterrey"));

            Assert.True(created.Succeeded);
            Assert.NotSame(current.Document, created.Document);
            Assert.Equal(3, current.Document.Entries.Count);
            Assert.Equal(antes, Serialize(current.Document));
        }

        [Fact]
        public void TMut01_Crear_RespetaLosLimites_YUnRechazoNoProduceDocumento()
        {
            var demasiadoLargo = Apply(TresEntradas(), CustomPropertiesIntent.Create(new string('n', 81), "v"));
            var colision = Apply(TresEntradas(), CustomPropertiesIntent.Create("cliente", "v"));

            Assert.False(demasiadoLargo.Succeeded);
            Assert.Equal(CustomPropertiesRejection.NameTooLong, demasiadoLargo.Rejection);
            Assert.Null(demasiadoLargo.Document);
            Assert.True(demasiadoLargo.CreatedId.IsEmpty);
            Assert.False(colision.Succeeded);
            Assert.Equal(CustomPropertiesRejection.NameCollision, colision.Rejection);
        }

        [Fact]
        public void TMut01_DosCreaciones_DanIdsDistintos()
        {
            var primera = Apply(AbsentResult(), CustomPropertiesIntent.Create("Cliente", "ACME"));
            var segunda = Apply(ReadableResult(Serialize(primera.Document)), CustomPropertiesIntent.Create("Obra", "Nave 3"));

            Assert.True(segunda.Succeeded);
            Assert.NotEqual(primera.CreatedId, segunda.CreatedId);
            Assert.Equal(2, segunda.Document.Entries.Count);
        }

        // ================================================================ T-MUT-02 renombrar

        [Fact]
        public void TMut02_Renombrar_SoloCambiaElNombre_ValorYExtensionDeLaEntradaExactos()
        {
            var current = TresEntradas();

            var renamed = Apply(current, CustomPropertiesIntent.Rename(Id(IdB), "  Proyecto "));

            Assert.True(renamed.Succeeded);
            Assert.True(renamed.CreatedId.IsEmpty);
            AssertEntradaIntacta(current.Document.Entries[0], renamed.Document.Entries[0]);
            AssertEntradaIntacta(current.Document.Entries[2], renamed.Document.Entries[2]);

            var entrada = renamed.Document.Entries[1];

            Assert.Equal(Id(IdB), Id(entrada.Id));
            Assert.Equal("Proyecto", entrada.Name);
            Assert.Equal("Nave 3", entrada.Value);
            AssertExtensionIgual(current.Document.Entries[1], entrada);
            Assert.Equal("{\"r\":1}", renamed.Document.ExtensionData["Raiz"].GetRawText());
        }

        /// <summary>C-6: el id se compara por valor; su representacion persistida es la forma D en minusculas.</summary>
        [Fact]
        public void TMut02_Renombrar_UnIdLeidoEnMayusculasSeLocalizaPorValor_YSePersisteEnMinusculas()
        {
            var current = ReadableResult(Doc("1.0", Entries(
                Entry(IdA.ToUpperInvariant(), "Cliente", "ACME"),
                Entry(IdB, "Obra", "Nave 3"))));

            var renamed = Apply(current, CustomPropertiesIntent.Rename(Id(IdA), "Razon social"));

            Assert.True(renamed.Succeeded);
            Assert.Equal(Id(IdA), Id(renamed.Document.Entries[0].Id));

            var serialized = Serialize(renamed.Document);
            var reread = Readable(serialized);

            Assert.Contains("\"Id\":\"" + IdA + "\"", serialized, StringComparison.Ordinal);
            Assert.Equal("Razon social", reread.Entries[0].Name);
            Assert.Equal(Id(IdA), Id(reread.Entries[0].Id));
        }

        [Fact]
        public void TMut02_Renombrar_CambiandoSoloMayusculas_SePermite()
        {
            var renamed = Apply(TresEntradas(), CustomPropertiesIntent.Rename(Id(IdA), "CLIENTE"));

            Assert.True(renamed.Succeeded);
            Assert.Equal("CLIENTE", renamed.Document.Entries[0].Name);
        }

        [Fact]
        public void TMut02_Renombrar_ConElNombreDeOtraEntrada_SeRechaza()
        {
            var current = TresEntradas();

            var renamed = Apply(current, CustomPropertiesIntent.Rename(Id(IdA), " obra "));

            Assert.False(renamed.Succeeded);
            Assert.Equal(CustomPropertiesRejection.NameCollision, renamed.Rejection);
            Assert.Null(renamed.Document);
            Assert.Equal("Cliente", current.Document.Entries[0].Name);
        }

        // ================================================================ T-MUT-03 cambiar valor

        [Fact]
        public void TMut03_CambiarValor_SoloCambiaElValor_NombreYExtensionDeLaEntradaExactos()
        {
            var current = TresEntradas();

            var changed = Apply(current, CustomPropertiesIntent.ChangeValue(Id(IdC), "C"));

            Assert.True(changed.Succeeded);
            AssertEntradaIntacta(current.Document.Entries[0], changed.Document.Entries[0]);
            AssertEntradaIntacta(current.Document.Entries[1], changed.Document.Entries[1]);

            var entrada = changed.Document.Entries[2];

            Assert.Equal(IdC, entrada.Id);
            Assert.Equal("Revision", entrada.Name);
            Assert.Equal("C", entrada.Value);
            AssertExtensionIgual(current.Document.Entries[2], entrada);
        }

        [Fact]
        public void TMut03_CambiarValor_GuardaElValorTalComoLlega_SinNormalizarNiRecortar()
        {
            var valor = "  " + AreaNfd + "\r\n";

            var changed = Apply(TresEntradas(), CustomPropertiesIntent.ChangeValue(Id(IdA), valor));

            Assert.True(changed.Succeeded);
            Assert.Equal(valor, changed.Document.Entries[0].Value);
            Assert.Equal(valor, Readable(Serialize(changed.Document)).Entries[0].Value);
        }

        // ================================================================ T-MUT-04 eliminar

        [Fact]
        public void TMut04_Eliminar_QuitaSoloEsaEntradaYSuExtension_ElRestoExacto()
        {
            var current = TresEntradas();

            var deleted = Apply(current, CustomPropertiesIntent.Delete(Id(IdB)));

            Assert.True(deleted.Succeeded);
            Assert.Equal(2, deleted.Document.Entries.Count);
            AssertEntradaIntacta(current.Document.Entries[0], deleted.Document.Entries[0]);
            AssertEntradaIntacta(current.Document.Entries[2], deleted.Document.Entries[1]);
            Assert.Equal("{\"r\":1}", deleted.Document.ExtensionData["Raiz"].GetRawText());

            var serialized = Serialize(deleted.Document);

            Assert.DoesNotContain(IdB, serialized, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("\"EB\"", serialized, StringComparison.Ordinal);
        }

        // ================================================================ T-MUT-05 vaciar

        [Fact]
        public void TMut05_EliminarLaUltima_DejaEntriesVacio_ConservaRaizYVersion_YElContenedorSigue()
        {
            var current = ReadableResult(Doc("1.3", Entries(Entry(IdA, "Cliente", "ACME")), ",\"Raiz\":{\"r\":1}"));

            var emptied = Apply(current, CustomPropertiesIntent.Delete(Id(IdA)));

            Assert.True(emptied.Succeeded);
            Assert.NotNull(emptied.Document.Entries);
            Assert.Empty(emptied.Document.Entries);
            Assert.Equal("1.3", emptied.Document.SchemaVersion);

            var serialized = Serialize(emptied.Document);

            Assert.Equal("{\"SchemaVersion\":\"1.3\",\"Entries\":[],\"Raiz\":{\"r\":1}}", serialized);

            var reread = ReadText(serialized);

            Assert.Equal(CustomPropertiesReadOutcome.Readable, reread.Outcome);
            Assert.Empty(reread.Document.Entries);
        }

        // ================================================================ T-MUT-06 extension de entrada en un 1.7

        [Fact]
        public void TMut06_EnUnDocumento17_RenombrarYCambiarValor_ConservanLaExtensionDeLaEntradaYLaVersion()
        {
            var current = ReadableResult(Doc(
                "1.7",
                Entries(Entry(IdA, "Cliente", "ACME", ",\"Moneda\":\"MXN\",\"Formato\":{\"decimales\":2}"))));

            var renamed = Apply(current, CustomPropertiesIntent.Rename(Id(IdA), "Razon social"));

            Assert.True(renamed.Succeeded);

            var trasRenombrar = ReadableResult(Serialize(renamed.Document));
            var changed = Apply(trasRenombrar, CustomPropertiesIntent.ChangeValue(Id(IdA), "ACME SA"));

            Assert.True(changed.Succeeded);

            var final = Readable(Serialize(changed.Document));
            var entrada = Assert.Single(final.Entries);

            Assert.Equal("1.7", final.SchemaVersion);
            Assert.Equal("Razon social", entrada.Name);
            Assert.Equal("ACME SA", entrada.Value);
            AssertExtensionIgual(current.Document.Entries[0], entrada);
        }

        // ================================================================ T-MUT-09 intent por id

        [Fact]
        public void TMut09_UnIdInexistente_SeRechazaAlRenombrarCambiarYEliminar()
        {
            var current = TresEntradas();
            var ajeno = CustomPropertyId.New();

            foreach (var intent in new[]
            {
                CustomPropertiesIntent.Rename(ajeno, "Otro"),
                CustomPropertiesIntent.ChangeValue(ajeno, "Otro"),
                CustomPropertiesIntent.Delete(ajeno),
            })
            {
                var result = Apply(current, intent);

                Assert.False(result.Succeeded);
                Assert.Equal(CustomPropertiesRejection.EntryNotFound, result.Rejection);
                Assert.Null(result.Document);
                Assert.False(string.IsNullOrWhiteSpace(result.Error));
            }

            Assert.Equal(3, current.Document.Entries.Count);
        }

        /// <summary>
        /// El nombre nunca localiza: renombrar un id inexistente con el nombre de una entrada existente no la toca, y con
        /// dos entradas del mismo nombre solo cambia la del id pedido.
        /// </summary>
        [Fact]
        public void TMut09_ElNombreNuncaLocalizaUnaEntrada()
        {
            var inexistente = Apply(TresEntradas(), CustomPropertiesIntent.Rename(CustomPropertyId.New(), "Cliente"));

            Assert.Equal(CustomPropertiesRejection.EntryNotFound, inexistente.Rejection);

            var repetidos = ReadableResult(Doc("1.0", Entries(Entry(IdA, "Cliente", "uno"), Entry(IdB, "Cliente", "dos"))));
            var changed = Apply(repetidos, CustomPropertiesIntent.ChangeValue(Id(IdB), "DOS"));

            Assert.True(changed.Succeeded);
            Assert.Equal("uno", changed.Document.Entries[0].Value);
            Assert.Equal("DOS", changed.Document.Entries[1].Value);
        }

        [Fact]
        public void TMut09_LosIntentsSobreUnaEntradaExistente_ExigenUnId()
        {
            Assert.Throws<ArgumentException>(() => CustomPropertiesIntent.Rename(default(CustomPropertyId), "Otro"));
            Assert.Throws<ArgumentException>(() => CustomPropertiesIntent.ChangeValue(default(CustomPropertyId), "Otro"));
            Assert.Throws<ArgumentException>(() => CustomPropertiesIntent.Delete(default(CustomPropertyId)));

            // Solo crear se dirige sin id: ninguna fabrica publica localiza una entrada existente con un string.
            var porTexto = typeof(CustomPropertiesIntent)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(method => method.GetParameters().All(parameter => parameter.ParameterType == typeof(string)))
                .Select(method => method.Name)
                .ToList();

            Assert.Equal(new[] { nameof(CustomPropertiesIntent.Create) }, porTexto);
        }
    }
}
