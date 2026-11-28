using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RackCad.Application.Persistence;
using RackCad.Application.Workspace;

// I-64 Smoke-1 fixture harness (offline, no AutoCAD). It uses ONLY the Candidate's own RackCad.Application code:
//   make <outDir>   -> writes the two envelopes produced by RackEmbedStore.Serialize (UTF-8, no BOM, no newline)
//   check <file>... -> for each envelope file: Deserialize, the DefinitionFact the reader would build, and
//                      SelectionContext.Classify for that single selected reference (no other entities)
internal static class Program
{
    private const string DiagnosticId = "6f4e1c2a-64d0-4b1e-9a30-5f1c0d2e7b41";

    private static int Main(string[] args)
    {
        var utf8 = new UTF8Encoding(false);
        var store = new RackEmbedStore();
        if (args.Length == 2 && args[0] == "make")
        {
            var noIdentity = new RackEmbedDocument
            {
                Kind = RackEmbedDocument.KindSelective,
                View = RackEmbedDocument.ViewFrontal,
                Section = -1,
                Id = "",
                Name = "I-64 SMOKE-1 FIXTURE PR-13 SIN IDENTIDAD",
                Design = "{}"
            };
            var diagnostic = new RackEmbedDocument
            {
                SchemaVersion = "2.0",
                Kind = RackEmbedDocument.KindSelective,
                View = RackEmbedDocument.ViewFrontal,
                Section = -1,
                Id = DiagnosticId,
                Name = "I-64 SMOKE-1 FIXTURE PR-12 DIAGNOSTICO",
                Design = "{}"
            };
            File.WriteAllText(Path.Combine(args[1], "envelope-pr13-sin-identidad.json"), store.Serialize(noIdentity), utf8);
            File.WriteAllText(Path.Combine(args[1], "envelope-pr12-diagnostico.json"), store.Serialize(diagnostic), utf8);
            Console.WriteLine("made");
            return 0;
        }

        if (args.Length >= 2 && args[0] == "check")
        {
            for (var i = 1; i < args.Length; i++)
            {
                var json = File.ReadAllText(args[i], utf8);
                // Same steps as WorkspaceSelectionReader.ReadDefinition after RackBlockData.Read returned the text.
                DefinitionFact fact;
                if (string.IsNullOrWhiteSpace(json))
                {
                    Console.WriteLine(Path.GetFileName(args[i]) + "\tNOT_A_RACK");
                    continue;
                }

                var envelope = store.Deserialize(json);
                fact = envelope == null ? new DefinitionFact(false, null) : new DefinitionFact(true, envelope.Id);
                var context = SelectionContext.Classify(new List<DefinitionFact> { fact }, 0);
                Console.WriteLine(string.Join("\t", Path.GetFileName(args[i]),
                    "Deserialized=" + (envelope != null),
                    "IsInterpretable=" + fact.IsInterpretable,
                    "EnvelopeId=[" + (fact.EnvelopeId ?? "null") + "]",
                    "Kind=" + context.Kind,
                    "RackId=[" + (context.RackId ?? "null") + "]",
                    "RackCount=" + context.RackCount,
                    "OtherCount=" + context.OtherCount));
            }

            return 0;
        }

        Console.WriteLine("usage: make <outDir> | check <file>...");
        return 2;
    }
}
