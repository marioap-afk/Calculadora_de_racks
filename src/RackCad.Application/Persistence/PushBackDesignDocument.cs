using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using RackCad.Domain.Systems;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// Versioned, self-contained persistence DTO for a Push Back design (I-18a). It REUSES the dynamic document
    /// mapping (<see cref="DynamicRackSystemDocument"/>) for the shared structure and adds Push Back's own fields. It
    /// follows the FLAT, self-versioned pattern (own <see cref="SchemaVersion"/> + <c>[JsonExtensionData]</c>) like
    /// <see cref="FlowBedDocument"/>/<see cref="LargueroDocument"/>: a legacy JSON with no SchemaVersion loads via the
    /// fallback, and unknown top-level fields survive a load/save (I-11, D3). Push-back-specific inputs are nullable so
    /// legacy documents fall back to the domain defaults. When Push Back is registered as a system (I-18b) this DTO
    /// gets a typed slot on <see cref="RackProjectDocument"/> and its non-downgrade re-save is wired through the store.
    /// </summary>
    public sealed class PushBackDesignDocument
    {
        public const string CurrentSchemaVersion = "1.0";

        public string SchemaVersion { get; set; } = CurrentSchemaVersion;

        /// <summary>The shared structural intent, mapped verbatim by the dynamic document (nullable-fallback tolerant).</summary>
        public DynamicRackSystemDocument Structure { get; set; }

        /// <summary>High-end (rear) beam PERALTE (in); null falls back to the resolver's explicit 3.5 default.</summary>
        public double? HighEndBeamPeralte { get; set; }

        /// <summary>Rear-tope SAQUE (in); null falls back to the domain default.</summary>
        public double? RearTopeSaque { get; set; }

        /// <summary>Rear-tope DEACTIVATIONS only (front, level). Active-by-default is implicit: an absent cell is active,
        /// so a complete positive list is never persisted.</summary>
        public List<PushBackCellDocument> RearTopeOffCells { get; set; }

        /// <summary>JSON fields this build does not know about, preserved verbatim across a load/save (I-11, D3).</summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement> ExtensionData { get; set; }

        public static PushBackDesignDocument FromDomain(PushBackDesign design)
        {
            if (design == null)
            {
                return new PushBackDesignDocument();
            }

            var document = new PushBackDesignDocument
            {
                Structure = DynamicRackSystemDocument.From(design.Structure ?? new DynamicRackDesign()),
                HighEndBeamPeralte = design.HighEndBeamPeralte > 0.0 ? design.HighEndBeamPeralte : (double?)null,
                RearTopeSaque = design.RearTope != null && design.RearTope.Saque > 0.0
                    ? design.RearTope.Saque
                    : (double?)null
            };

            if (design.RearTope != null && design.RearTope.OffCells.Count > 0)
            {
                document.RearTopeOffCells = design.RearTope.OffCells
                    .Where(cell => cell != null)
                    .Select(cell => new PushBackCellDocument { Frente = cell.Frente, Level = cell.Level })
                    .ToList();
            }

            return document;
        }

        public PushBackDesign ToDomain()
        {
            var design = new PushBackDesign
            {
                Structure = Structure?.ToDesign() ?? new DynamicRackDesign(),
                HighEndBeamPeralte = HighEndBeamPeralte ?? PushBackDefaults.HighEndBeamDefaultPeralte
            };

            design.RearTope.Saque = RearTopeSaque ?? PushBackDefaults.RearTopeSaque;

            if (RearTopeOffCells != null)
            {
                foreach (var cell in RearTopeOffCells)
                {
                    if (cell != null)
                    {
                        design.RearTope.OffCells.Add(new SelectiveGridCell { Frente = cell.Frente, Level = cell.Level });
                    }
                }
            }

            return design;
        }
    }

    /// <summary>One (front, level) cell in a Push Back document (a rear-tope deactivation).</summary>
    public sealed class PushBackCellDocument
    {
        public int Frente { get; set; }
        public int Level { get; set; }
    }
}
