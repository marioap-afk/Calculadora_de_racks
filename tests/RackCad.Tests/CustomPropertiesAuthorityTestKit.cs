using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RackCad.Application.CustomProperties;
using RackCad.Application.Persistence;
using Xunit;
using static RackCad.Tests.CustomPropertiesTestKit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-54 G5 — utilidades de las pruebas de autoridad, workspace, preflight y commit (Proposal V5 D-09, D-10, D-18 y
    /// D-22; §12.3 y §12.4).
    ///
    /// <para>
    /// La proyeccion plana se construye como la construira el Plugin: cada sobre se lee de su texto JSON con el
    /// <see cref="RackEmbedStore"/> de produccion, y una definicion no interpretable llega sin sobre. Ningun tipo de
    /// AutoCAD cruza a estas pruebas.
    /// </para>
    /// </summary>
    internal static class CustomPropertiesAuthorityTestKit
    {
        internal const string RackA = "a1a1a1a1-1111-4111-8111-aaaaaaaaaaaa";

        internal const string RackB = "b2b2b2b2-2222-4222-8222-bbbbbbbbbbbb";

        /// <summary>Los kinds que este build conoce, sin distinguir mayusculas, como hara el registro de handlers.</summary>
        internal static readonly Func<string, bool> KnownKinds = kind =>
            new[] { "selective", "dynamic", "cabecera", "cama", "pushback", "cantilever" }
                .Contains(kind, StringComparer.OrdinalIgnoreCase);

        /// <summary>El texto JSON de un sobre con los miembros de BASE y, si se da, el miembro <c>CustomProperties</c> crudo.</summary>
        internal static string EnvelopeText(
            string rackId,
            string properties,
            string kind = "selective",
            string view = "frontal",
            int section = -1,
            string extra = "")
        {
            var builder = new StringBuilder();
            builder.Append("{\"SchemaVersion\":\"1.0\",\"Kind\":").Append(kind == null ? "null" : JsonString(kind));
            builder.Append(",\"View\":").Append(view == null ? "null" : JsonString(view));
            builder.Append(",\"Section\":").Append(section);
            builder.Append(",\"Id\":").Append(rackId == null ? "null" : JsonString(rackId));
            builder.Append(",\"Name\":\"Rack A\",\"Design\":\"{}\"");

            if (properties != null)
            {
                builder.Append(",\"CustomProperties\":").Append(properties);
            }

            builder.Append(extra).Append('}');
            return builder.ToString();
        }

        /// <summary>Una definicion interpretable: su sobre sale del texto por el store de produccion.</summary>
        internal static RackCustomPropertiesDefinition View(
            string handle,
            string rackId = RackA,
            string properties = null,
            string kind = "selective",
            string view = "frontal",
            int section = -1,
            bool placed = true,
            bool dependent = false,
            string extra = "")
            => Definition(handle, EnvelopeText(rackId, properties, kind, view, section, extra), placed, dependent);

        /// <summary>Una definicion a partir de un texto de sobre dado, que tiene que ser interpretable.</summary>
        internal static RackCustomPropertiesDefinition Definition(string handle, string envelopeText, bool placed = true, bool dependent = false)
        {
            var envelope = new RackEmbedStore().Deserialize(envelopeText);
            Assert.NotNull(envelope);
            return new RackCustomPropertiesDefinition(handle, "RACK_" + handle, placed, dependent, envelope);
        }

        /// <summary>Una definicion con payload RackCad que este build no interpreta: llega sin sobre.</summary>
        internal static RackCustomPropertiesDefinition Uninterpretable(string handle, bool placed = true, bool dependent = false)
            => new RackCustomPropertiesDefinition(handle, "RACK_" + handle, placed, dependent, null);

        internal static RackCustomPropertiesSelection Pick(string handle, bool fromExternalReference = false)
            => new RackCustomPropertiesSelection(handle, fromExternalReference);

        internal static RackCustomPropertiesAuthorityResult Authority(string selected, params RackCustomPropertiesDefinition[] definitions)
            => RackCustomPropertiesAuthority.Evaluate(definitions, Pick(selected), KnownKinds);

        internal static RackCustomPropertiesAuthorityResult Authority(
            IEnumerable<RackCustomPropertiesDefinition> definitions, string selected, Func<string, bool> isKnownKind = null)
            => RackCustomPropertiesAuthority.Evaluate(definitions, Pick(selected), isKnownKind ?? KnownKinds);

        /// <summary>Una coleccion 1.0 con estas entradas, en forma de texto JSON.</summary>
        internal static string Props(params string[] entries) => Doc("1.0", Entries(entries));

        /// <summary>Una coleccion de la version dada con estas entradas.</summary>
        internal static string PropsAt(string version, params string[] entries) => Doc(version, Entries(entries));

        internal static string EmptyAt(string version) => Doc(version, "[]");

        /// <summary>Una huella comparable de todo lo que la autoridad decide, para comprobar que no depende del orden del barrido.</summary>
        internal static string Signature(RackCustomPropertiesAuthorityResult result)
        {
            var builder = new StringBuilder();
            builder.Append(result.Outcome).Append('|').Append(result.RackId).Append('|').Append(result.Kind).Append('|');
            builder.Append(result.WriteVersion).Append('|').Append(result.Error).Append('|');

            foreach (var member in result.Members)
            {
                builder.Append(member.Handle).Append(':').Append(member.Collection.Outcome).Append(':').Append(member.CanonicalForm).Append(';');
            }

            builder.Append('|');

            foreach (var definition in result.UninterpretableDefinitions)
            {
                builder.Append(definition.Handle).Append(':').Append(definition.BlockName).Append(':').Append(definition.IsPlaced).Append(';');
            }

            builder.Append('|');

            foreach (var option in result.UnifyOptions)
            {
                builder.Append(option.SourceHandle).Append(':').Append(option.IsAvailable).Append(':')
                    .Append(string.Join(",", option.BlockingHandles)).Append(';');
            }

            builder.Append('|');

            if (result.Collection != null)
            {
                builder.Append(CustomPropertiesCanonicalForm.Of(result.Collection)).Append('@').Append(result.Collection.Document.SchemaVersion);
            }

            return builder.ToString();
        }

        /// <summary>Todas las permutaciones de una lista corta: el barrido del Plugin puede venir en cualquier orden.</summary>
        internal static IEnumerable<IReadOnlyList<T>> Permutations<T>(IReadOnlyList<T> items)
        {
            if (items.Count <= 1)
            {
                yield return items.ToList();
                yield break;
            }

            for (var i = 0; i < items.Count; i++)
            {
                var rest = items.Where((_, index) => index != i).ToList();

                foreach (var tail in Permutations(rest))
                {
                    var permutation = new List<T> { items[i] };
                    permutation.AddRange(tail);
                    yield return permutation;
                }
            }
        }

        /// <summary>
        /// La proyeccion tal como quedaria tras ejecutar un plan: cada definicion planificada lleva el sobre de su payload.
        /// Sirve para comprobar el estado logico final (INV-05) sin el Plugin.
        /// </summary>
        internal static IReadOnlyList<RackCustomPropertiesDefinition> AfterPlan(
            IEnumerable<RackCustomPropertiesDefinition> fresh, CustomPropertiesCommitResult commit)
        {
            var payloads = commit.Plan.ToDictionary(entry => entry.Handle, entry => entry.Payload, StringComparer.Ordinal);

            return fresh
                .Select(definition => payloads.TryGetValue(definition.Handle, out var payload)
                    ? new RackCustomPropertiesDefinition(
                        definition.Handle, definition.BlockName, definition.IsPlaced, definition.IsDependent, new RackEmbedStore().Deserialize(payload))
                    : definition)
                .ToList();
        }
    }
}
