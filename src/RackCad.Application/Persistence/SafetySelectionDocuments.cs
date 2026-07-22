using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.Systems;

namespace RackCad.Application.Persistence
{
    // Real per-subtype DTOs for the selective safety families (I-22, E7 — round 2). Each family has its own
    // documentary type with From/ToDomain (round-trip + legacy fallback) that the tests exercise DIRECTLY. They do
    // NOT introduce nested JSON: the serialized shape stays the FLAT SafetySelectionDocument (shared with the dynamic
    // path). Each DTO flattens itself INTO SafetySelectionDocument (WriteInto) and reads itself back OUT of it
    // (ReadFrom), so on-disk names, nullability, effective order and fallbacks are byte-for-byte the pre-round-2 wire.

    /// <summary>Shared cell/side mapping between the flat safety document and the domain, so every per-family DTO reads
    /// identically to the pre-decomposition code.</summary>
    internal static class SafetyDocumentMapping
    {
        public static List<GridCellDocument> Cells(IEnumerable<SelectiveGridCell> source)
            => (source ?? Enumerable.Empty<SelectiveGridCell>())
                .Where(cell => cell != null)
                .Select(cell => new GridCellDocument { Frente = cell.Frente, Level = cell.Level })
                .ToList();

        public static void AddCells(IEnumerable<GridCellDocument> source, ICollection<SelectiveGridCell> target)
        {
            foreach (var cell in source ?? Enumerable.Empty<GridCellDocument>())
            {
                if (cell != null && cell.Frente >= 0 && cell.Level >= 0)
                {
                    target.Add(new SelectiveGridCell { Frente = cell.Frente, Level = cell.Level });
                }
            }
        }

        public static SafetySide ToSafetySide(int? value)
        {
            if (!value.HasValue) return SafetySide.Both;
            return value.Value >= (int)SafetySide.None && value.Value <= (int)SafetySide.Both
                ? (SafetySide)value.Value
                : SafetySide.Both;
        }
    }

    /// <summary>DTO for the TOPE (larguero tope) family. Flattens into the <c>Tope*</c> fields of the flat document.</summary>
    public sealed class TopeSelectionDocument
    {
        public bool? Shared { get; set; }
        public double? Saque { get; set; }
        public bool? Frontal { get; set; }
        public int? Fondo { get; set; }
        public List<GridCellDocument> OffCells { get; set; }

        public static TopeSelectionDocument From(SelectiveTopeConfig config) => new TopeSelectionDocument
        {
            Shared = config.Shared,
            Saque = config.Saque,
            Frontal = config.Frontal,
            Fondo = config.Fondo,
            OffCells = SafetyDocumentMapping.Cells(config.OffCells)
        };

        public SelectiveTopeConfig ToDomain()
        {
            var config = new SelectiveTopeConfig
            {
                Shared = Shared ?? true,
                Saque = Saque.HasValue && Saque.Value > 0.0 ? Saque.Value : SelectiveSafetyDefaults.TopeSaque,
                Frontal = Frontal ?? false,
                Fondo = Fondo ?? -1
            };
            SafetyDocumentMapping.AddCells(OffCells, config.OffCells);
            return config;
        }

        public void WriteInto(SafetySelectionDocument document)
        {
            document.TopeShared = Shared;
            document.TopeSaque = Saque;
            document.TopeFrontal = Frontal;
            document.TopeFondo = Fondo;
            document.TopeOffCells = OffCells;
        }

        public static TopeSelectionDocument ReadFrom(SafetySelectionDocument document) => new TopeSelectionDocument
        {
            Shared = document.TopeShared,
            Saque = document.TopeSaque,
            Frontal = document.TopeFrontal,
            Fondo = document.TopeFondo,
            OffCells = document.TopeOffCells
        };
    }

    /// <summary>DTO for the DESVIADOR family. Flattens into the <c>Desviador*</c> fields of the flat document.</summary>
    public sealed class DesviadorSelectionDocument
    {
        public double? Longitud { get; set; }
        public double? PrimerNivelAltura { get; set; }
        public List<GridCellDocument> OffCells { get; set; }

        public static DesviadorSelectionDocument From(SelectiveDesviadorConfig config) => new DesviadorSelectionDocument
        {
            Longitud = config.Longitud,
            PrimerNivelAltura = config.PrimerNivelAltura,
            OffCells = SafetyDocumentMapping.Cells(config.OffCells)
        };

        public SelectiveDesviadorConfig ToDomain()
        {
            var config = new SelectiveDesviadorConfig
            {
                Longitud = Longitud.HasValue && Longitud.Value > 0.0 ? Longitud.Value : SelectiveSafetyDefaults.DesviadorLongitud,
                PrimerNivelAltura = PrimerNivelAltura.HasValue && PrimerNivelAltura.Value > 0.0
                    ? PrimerNivelAltura.Value
                    : SelectiveSafetyDefaults.DesviadorPrimerNivelAltura
            };
            SafetyDocumentMapping.AddCells(OffCells, config.OffCells);
            return config;
        }

        public void WriteInto(SafetySelectionDocument document)
        {
            document.DesviadorLongitud = Longitud;
            document.DesviadorPrimerNivelAltura = PrimerNivelAltura;
            document.DesviadorOffCells = OffCells;
        }

        public static DesviadorSelectionDocument ReadFrom(SafetySelectionDocument document) => new DesviadorSelectionDocument
        {
            Longitud = document.DesviadorLongitud,
            PrimerNivelAltura = document.DesviadorPrimerNivelAltura,
            OffCells = document.DesviadorOffCells
        };
    }

    /// <summary>DTO for the DEFENSA family. Flattens into the <c>DefensaPosts</c> field of the flat document.</summary>
    public sealed class DefensaSelectionDocument
    {
        public List<PostDefenseDocument> Posts { get; set; }

        public static DefensaSelectionDocument From(SelectiveDefensaConfig config) => new DefensaSelectionDocument
        {
            Posts = (config.Posts ?? Enumerable.Empty<SafetyPostDefense>())
                .Where(post => post != null)
                .Select(post => new PostDefenseDocument
                {
                    PostIndex = post.PostIndex,
                    ExitLength = post.ExitLength,
                    EntranceLength = post.EntranceLength
                }).ToList()
        };

        public SelectiveDefensaConfig ToDomain()
        {
            var config = new SelectiveDefensaConfig();
            foreach (var post in Posts ?? Enumerable.Empty<PostDefenseDocument>())
            {
                if (post != null && post.PostIndex >= 0)
                {
                    config.Posts.Add(new SafetyPostDefense
                    {
                        PostIndex = post.PostIndex,
                        ExitLength = post.ExitLength.HasValue ? Math.Max(0.0, post.ExitLength.Value) : 0.0,
                        EntranceLength = post.EntranceLength.HasValue ? Math.Max(0.0, post.EntranceLength.Value) : 0.0
                    });
                }
            }

            return config;
        }

        public void WriteInto(SafetySelectionDocument document) => document.DefensaPosts = Posts;

        public static DefensaSelectionDocument ReadFrom(SafetySelectionDocument document) => new DefensaSelectionDocument
        {
            Posts = document.DefensaPosts
        };
    }

    /// <summary>DTO for the GUIA (entrance guide) family. Flattens into the <c>GuiaEntradaOffCells</c> field.</summary>
    public sealed class GuiaSelectionDocument
    {
        public List<GridCellDocument> OffCells { get; set; }

        public static GuiaSelectionDocument From(SelectiveGuiaConfig config) => new GuiaSelectionDocument
        {
            OffCells = SafetyDocumentMapping.Cells(config.OffCells)
        };

        public SelectiveGuiaConfig ToDomain()
        {
            var config = new SelectiveGuiaConfig();
            SafetyDocumentMapping.AddCells(OffCells, config.OffCells);
            return config;
        }

        public void WriteInto(SafetySelectionDocument document) => document.GuiaEntradaOffCells = OffCells;

        public static GuiaSelectionDocument ReadFrom(SafetySelectionDocument document) => new GuiaSelectionDocument
        {
            OffCells = document.GuiaEntradaOffCells
        };
    }

    /// <summary>DTO for the PARRILLA (deck) family. Flattens into the <c>Parrilla*</c> fields of the flat document.</summary>
    public sealed class ParrillaSelectionDocument
    {
        public bool? Frontal { get; set; }
        public bool? Lateral { get; set; }
        public double? Frente { get; set; }
        public int? Cantidad { get; set; }
        public List<GridCellDocument> OffCells { get; set; }

        public static ParrillaSelectionDocument From(SelectiveParrillaConfig config) => new ParrillaSelectionDocument
        {
            Frontal = config.Frontal,
            Lateral = config.Lateral,
            Frente = config.Frente,
            Cantidad = config.Cantidad,
            OffCells = SafetyDocumentMapping.Cells(config.OffCells)
        };

        public SelectiveParrillaConfig ToDomain()
        {
            var config = new SelectiveParrillaConfig
            {
                Frontal = Frontal ?? true,
                Lateral = Lateral ?? true,
                Frente = Frente ?? 0.0,
                Cantidad = Cantidad ?? 0
            };
            SafetyDocumentMapping.AddCells(OffCells, config.OffCells);
            return config;
        }

        public void WriteInto(SafetySelectionDocument document)
        {
            document.ParrillaFrontal = Frontal;
            document.ParrillaLateral = Lateral;
            document.ParrillaFrente = Frente;
            document.ParrillaCantidad = Cantidad;
            document.ParrillaOffCells = OffCells;
        }

        public static ParrillaSelectionDocument ReadFrom(SafetySelectionDocument document) => new ParrillaSelectionDocument
        {
            Frontal = document.ParrillaFrontal,
            Lateral = document.ParrillaLateral,
            Frente = document.ParrillaFrente,
            Cantidad = document.ParrillaCantidad,
            OffCells = document.ParrillaOffCells
        };
    }
}
