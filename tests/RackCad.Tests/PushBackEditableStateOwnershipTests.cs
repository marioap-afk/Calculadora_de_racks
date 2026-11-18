using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using RackCad.Application.Systems.PushBack;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T1b — CT-PBT1B-11: the ownership map. Every leaf of the 17 envelopes of the corpus, in its three
    /// document levels, has an owner; the rest of the contract is the table itself (D-PBT1-08).
    /// </summary>
    public sealed class PushBackEditableStateOwnershipTests
    {
        private static IEnumerable<string> FixtureNames()
        {
            var dir = System.IO.Path.Combine(PilotKit.RepoRoot().FullName, "tests", "RackCad.Tests", "Fixtures", "I64");
            return System.IO.Directory.GetFiles(dir, "pb-*.json")
                .Select(System.IO.Path.GetFileNameWithoutExtension)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToList();
        }

        /// <summary>Every leaf path of a JSON document, as <c>$.A.B[3].C</c>; an empty container is a leaf too.</summary>
        private static void Leaves(JsonElement element, string path, List<string> into)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    if (!element.EnumerateObject().Any()) { into.Add(path); return; }
                    foreach (var property in element.EnumerateObject()) Leaves(property.Value, path + "." + property.Name, into);
                    return;
                case JsonValueKind.Array:
                    if (element.GetArrayLength() == 0) { into.Add(path); return; }
                    var index = 0;
                    foreach (var item in element.EnumerateArray()) Leaves(item, path + "[" + index++ + "]", into);
                    return;
                default:
                    into.Add(path);
                    return;
            }
        }

        private static List<string> LeavesOf(string json)
        {
            var into = new List<string>();
            using (var document = JsonDocument.Parse(json))
            {
                Leaves(document.RootElement, "$", into);
            }

            return into;
        }

        [Fact]
        public void CT_PBT1B_11_EveryFieldOfTheThreeDocumentLevelsHasAnOwner()
        {
            var log = new List<string>();
            var expected = new List<string>();

            log.Add("normalize " + PushBackEditableStateOwnership.Normalize("$.Structure.Fronts[3].Cells[0]"));
            expected.Add("normalize $.Structure.Fronts[*].Cells[*]");

            foreach (var name in FixtureNames())
            {
                var envelopeText = PilotKit.Text(name);
                var envelope = LeavesOf(envelopeText);
                string design;
                using (var document = JsonDocument.Parse(envelopeText)) design = document.RootElement.GetProperty("Design").GetString();
                var project = LeavesOf(design);
                string payload;
                using (var document = JsonDocument.Parse(design)) payload = document.RootElement.GetProperty("PushBack").GetRawText();
                var payloadLeaves = LeavesOf(payload);

                var envelopeBad = envelope.Where(path =>
                {
                    var owner = PushBackEditableStateOwnership.Of(PushBackDocumentLevel.Envelope, path);
                    var wanted = path == "$.Name" ? PushBackFieldOwner.Editor : path == "$.Design" ? PushBackFieldOwner.Container : PushBackFieldOwner.Envelope;
                    return owner != wanted;
                }).ToList();
                var projectBad = project.Where(path =>
                {
                    var owner = PushBackEditableStateOwnership.Of(PushBackDocumentLevel.Project, path);
                    var wanted = path == "$.PushBack" || path.StartsWith("$.PushBack.", StringComparison.Ordinal) || path.StartsWith("$.PushBack[", StringComparison.Ordinal)
                        ? PushBackFieldOwner.Container
                        : PushBackFieldOwner.Envelope;
                    return owner != wanted;
                }).ToList();
                var payloadBad = payloadLeaves.Where(path =>
                {
                    var owner = PushBackEditableStateOwnership.Of(PushBackDocumentLevel.Payload, path);
                    var wanted = path == "$.SchemaVersion" ? PushBackFieldOwner.Envelope : PushBackFieldOwner.Editor;
                    return owner != wanted;
                }).ToList();

                log.Add(name + " envelope sin-clasificar-o-distinta=" + envelopeBad.Count + " hojas>0=" + (envelope.Count > 0));
                log.Add(name + " proyecto sin-clasificar-o-distinta=" + projectBad.Count + " hojas>0=" + (project.Count > 0));
                log.Add(name + " carga sin-clasificar-o-distinta=" + payloadBad.Count + " hojas>0=" + (payloadLeaves.Count > 0));
                expected.Add(name + " envelope sin-clasificar-o-distinta=0 hojas>0=True");
                expected.Add(name + " proyecto sin-clasificar-o-distinta=0 hojas>0=True");
                expected.Add(name + " carga sin-clasificar-o-distinta=0 hojas>0=True");
            }

            foreach (var level in new[] { PushBackDocumentLevel.Envelope, PushBackDocumentLevel.Project, PushBackDocumentLevel.Payload })
            {
                log.Add(level + " propiedad-desconocida=" + PushBackEditableStateOwnership.Of(level, "$.PropiedadQueNadieDeclaro"));
                expected.Add(level + " propiedad-desconocida=" + PushBackFieldOwner.Envelope);
            }

            var table = PushBackEditableStateOwnership.Table;
            log.Add("filas>0=" + (table.Count > 0));
            log.Add("toda-fila-con-fuente=" + (table.Count > 0 && table.All(row => !string.IsNullOrWhiteSpace(row.Source) && !Regex.IsMatch(row.Source, @":\d"))));
            expected.Add("filas>0=True");
            expected.Add("toda-fila-con-fuente=True");

            Assert.Equal(expected, log);
        }
    }
}
