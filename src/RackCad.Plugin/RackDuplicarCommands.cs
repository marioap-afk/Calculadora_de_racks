using System;
using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Persistence;
using RackCad.Plugin.KindHandlers;
using RackCad.Plugin.Systems.Shared;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin
{
    /// <summary>RACKDUPLICAR: COPY-style duplication of a rack — base point + repeated destination points, each copy
    /// an INDEPENDENT rack (fresh GUID + numbered "- copia" name). Plus its short alias.</summary>
    public sealed class RackDuplicarCommands
    {
        [CommandMethod("RD")]  public void AliasRackDuplicar() => RackDuplicar();          // RACKDUPLICAR

        /// <summary>
        /// Duplicate a rack like AutoCAD's COPY: pick the rack, pick a BASE point, then click destination points —
        /// each click places an independent copy (its own GUID and name, so RACKEDITAR touches only that copy).
        /// Multiple mode by default (keep clicking; Enter/Esc ends); the [Unica] keyword switches to a single copy.
        /// The copy CLONES the clicked view-block's drawn geometry (nested ARRAY defs shared, payload re-stamped),
        /// so it is exact and works the same for the five rack types (selective, dynamic, Push Back, cabecera, cama).
        /// </summary>
        [CommandMethod("RACKDUPLICAR")]
        public void RackDuplicar()
        {
            try
            {
                var document = AcApplication.DocumentManager.MdiActiveDocument;
                if (document == null)
                {
                    return;
                }

                var editor = document.Editor;

                if (!PickDuplicateSource(document, out var embed, out var source))
                {
                    return;
                }

                if (embed == null || string.IsNullOrEmpty(embed.Design))
                {
                    editor.WriteMessage("\nRackCad: ese bloque no tiene datos de rack para duplicar.");
                    return;
                }

                // An unrecognized kind cannot be re-stamped safely (its inner identity is unknown): report the
                // historic visible error and abort BEFORE placing any copy, so no copy carries a possibly-
                // inconsistent identity. Case-insensitive, matching the restamp; the five embedded kinds resolve.
                if (!KindHandlerDispatch.TryResolveIgnoreCase(editor, embed.Kind, out _))
                {
                    return;
                }

                var basePrompt = new PromptPointOptions("\nPunto base: ");
                var baseResult = editor.GetPoint(basePrompt);
                if (baseResult.Status != PromptStatus.OK)
                {
                    return;
                }

                var basePoint = baseResult.Value;
                var baseName = string.IsNullOrWhiteSpace(embed.Name) ? "Rack" : embed.Name.Trim();
                var multiple = true; // like COPY: keep placing until Enter/Esc
                var placed = 0;

                while (true)
                {
                    var options = new PromptPointOptions("\nPunto de destino o")
                    {
                        UseBasePoint = true,
                        BasePoint = basePoint,
                        AllowNone = true, // Enter ends the command
                        AppendKeywordsToMessage = true
                    };
                    options.Keywords.Add("Unica");
                    options.Keywords.Add("Multiple");

                    var destination = editor.GetPoint(options);

                    if (destination.Status == PromptStatus.Keyword)
                    {
                        multiple = string.Equals(destination.StringResult, "Multiple", StringComparison.OrdinalIgnoreCase);
                        editor.WriteMessage(multiple
                            ? "\nRackCad: modo copia múltiple (Enter para terminar)."
                            : "\nRackCad: modo copia única (el siguiente punto coloca una copia y termina).");
                        continue;
                    }

                    if (destination.Status != PromptStatus.OK)
                    {
                        break; // Enter or Esc
                    }

                    placed++;
                    var copyName = placed == 1
                        ? baseName + " - copia"
                        : baseName + " - copia " + placed.ToString(CultureInfo.InvariantCulture);

                    // PREPARE (I-47 G14, I-51 G4): la transformacion que puede fallar se decide AQUI, antes de abrir
                    // la transaccion de escritura. Si el re-estampado no sale, no se clona nada: una copia con la
                    // identidad vieja dentro y una nueva fuera es irreversible, y solo se descubre cuando alguien la
                    // abre y la guarda. El fallo sale como siempre, hacia el catch del comando.
                    var restamped = RackEnvelopeRestamp.RestampEnvelope(source.Payload, copyName);

                    if (!restamped.IsSuccess)
                    {
                        throw new InvalidOperationException(restamped.Error);
                    }

                    // GetPoint returns CURRENT-UCS coordinates but BlockReference.Position is WCS: transform the
                    // displacement (a vector — only the rotational part applies) or a rotated UCS lands copies wrong.
                    var displacement = (destination.Value - basePoint).TransformBy(editor.CurrentUserCoordinateSystem);
                    var position = source.Position + displacement;

                    PlaceIndependentCopy(document, source, restamped.DesignJson, copyName, position, embed.Name);
                    editor.WriteMessage("\nRackCad: copia '" + copyName + "' colocada.");

                    if (!multiple)
                    {
                        break;
                    }
                }

                if (placed > 0)
                {
                    editor.WriteMessage(string.Format(CultureInfo.InvariantCulture,
                        "\nRackCad: {0} copia(s) independiente(s) de '{1}'.", placed, baseName));
                }
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
            }
        }

        /// <summary>Everything a duplication needs from the clicked reference, read in ONE transaction.</summary>
        private struct DuplicateSource
        {
            public ObjectId DefinitionId;
            public Point3d Position;
            public double Rotation;
            public Scale3d Scale;
            public ObjectId LayerId; // COPY preserves the entity's layer; so do we
            public string Payload;
        }

        /// <summary>Pick a rack block reference and snapshot it. False only when the user cancels the selection; a
        /// picked-but-non-rack block returns true with a null <paramref name="embed"/> so the caller reports it.</summary>
        private static bool PickDuplicateSource(Document document, out RackEmbedDocument embed, out DuplicateSource source)
        {
            embed = null;
            source = default;

            var options = new PromptEntityOptions("\nSelecciona un rack para duplicar: ");
            options.SetRejectMessage("\nEse objeto no es un rack.");
            options.AddAllowedClass(typeof(BlockReference), exactMatch: false);

            var selection = document.Editor.GetEntity(options);
            if (selection.Status != PromptStatus.OK)
            {
                return false;
            }

            source = InDocumentTransaction.Run(document, transaction =>
            {
                var reference = (BlockReference)transaction.GetObject(selection.ObjectId, OpenMode.ForRead);
                var snapshot = new DuplicateSource
                {
                    DefinitionId = reference.BlockTableRecord,
                    Position = reference.Position,
                    Rotation = reference.Rotation,
                    Scale = reference.ScaleFactors,
                    LayerId = reference.LayerId
                };
                snapshot.Payload = RackBlockData.Read(transaction, snapshot.DefinitionId);
                return snapshot;
            });

            embed = new RackEmbedStore().Deserialize(source.Payload);
            return true;
        }

        /// <summary>One independent copy — the MUTATE half: clone the view-block's definition (nested ARRAY defs
        /// shared, drawn name label renamed — Layout's helpers) with a payload the caller ALREADY prepared and
        /// checked, and reference it at the destination with the source's own rotation/mirror/layer. Nothing that
        /// can fail for a semantic reason is decided in here (I-47 G14, I-51 G4).</summary>
        private static void PlaceIndependentCopy(Document document, DuplicateSource source, string preparedPayload, string copyName, Point3d position, string sourceName)
        {
            var database = document.Database;

            InDocumentTransaction.Run(document, transaction =>
            {
                var definitionId = RackCloner.CloneDefinition(database, transaction, source.DefinitionId, copyName, preparedPayload, sourceName, copyName);

                var modelSpace = (BlockTableRecord)transaction.GetObject(
                    SymbolUtilityServices.GetBlockModelSpaceId(database), OpenMode.ForWrite);
                var reference = new BlockReference(position, definitionId)
                {
                    Rotation = source.Rotation,
                    ScaleFactors = source.Scale,
                    LayerId = source.LayerId
                };
                modelSpace.AppendEntity(reference);
                transaction.AddNewlyCreatedDBObject(reference, true);
            });
        }
    }
}
