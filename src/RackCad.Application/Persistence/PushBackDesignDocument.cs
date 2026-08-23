using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// Versioned, self-contained persistence DTO for a Push Back design (I-18a). It REUSES the dynamic document mapping
    /// (<see cref="DynamicRackSystemDocument"/>) for the shared structure and adds Push Back's own fields, including the
    /// high-end (rear) beam PERALTE PER FRONT AND LEVEL. It follows the FLAT, self-versioned pattern (own
    /// <see cref="SchemaVersion"/> + <c>[JsonExtensionData]</c>) like <see cref="FlowBedDocument"/>: a legacy JSON with no
    /// SchemaVersion loads via the fallback; unknown fields survive a load/save; and, when re-written from a loaded
    /// source via <see cref="FromDomain(PushBackDesign, PushBackDesignDocument)"/>, the schema version is never silently
    /// downgraded (a supported higher same-major minor is preserved) and the source's unknown fields are carried forward.
    /// </summary>
    public sealed class PushBackDesignDocument
    {
        public const string CurrentSchemaVersion = "1.0";

        public string SchemaVersion { get; set; } = CurrentSchemaVersion;

        /// <summary>The shared structural intent, mapped verbatim by the dynamic document (nullable-fallback tolerant).</summary>
        public DynamicRackSystemDocument Structure { get; set; }

        /// <summary>Per-front high-end (rear) beam peraltes by level; aligned by index with the structure's fronts.</summary>
        public List<PushBackFrontDocument> Fronts { get; set; }

        /// <summary>LEGACY rack-wide high-end beam peralte fallback (before per-cell); null falls back to the 3.5 default.</summary>
        public double? LegacyHighEndBeamPeralte { get; set; }

        /// <summary>Rear-tope SAQUE (in); null falls back to the domain default.</summary>
        public double? RearTopeSaque { get; set; }

        /// <summary>
        /// PB-005 (I-32) — the chosen catalog TOPE variant. NULL is the legacy value every earlier document carries and
        /// means "the system default", so an existing rack keeps drawing exactly the piece it always drew.
        /// </summary>
        public string RearTopePieceId { get; set; }

        /// <summary>Rear-tope DEACTIVATIONS only (front, level). Active-by-default is implicit: an absent cell is active.</summary>
        public List<PushBackCellDocument> RearTopeOffCells { get; set; }

        /// <summary>JSON fields this build does not know about, preserved verbatim across a load/save (I-11, D3).</summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement> ExtensionData { get; set; }

        public static PushBackDesignDocument FromDomain(PushBackDesign design) => FromDomain(design, null);

        /// <summary>Whether a front config carries anything worth writing (peraltes, default fondo, an override or a pallet).</summary>
        private static bool HasContent(PushBackFrontConfig front)
            => front != null
               && (front.HighEndBeamPeraltes.Count > 0
                   || front.DefaultPalletsDeep.HasValue
                   || front.PalletsDeepOverrides.Any(value => value.HasValue)
                   || front.DrawPallets.Any(value => value.GetValueOrDefault(false)));

        /// <summary>
        /// Maps a design to a document, INHERITING the persistence metadata of a previously loaded <paramref name="source"/>
        /// (its unknown fields + a non-downgraded schema version), so a Document→Domain→Document re-save preserves both.
        /// </summary>
        public static PushBackDesignDocument FromDomain(PushBackDesign design, PushBackDesignDocument source)
        {
            var document = new PushBackDesignDocument
            {
                SchemaVersion = SchemaVersionPolicy.ResolveWriteVersion(source?.SchemaVersion, CurrentSchemaVersion),
                ExtensionData = source?.ExtensionData
            };

            if (design == null)
            {
                return document;
            }

            document.Structure = DynamicRackSystemDocument.From(design.Structure ?? new DynamicRackDesign());
            document.LegacyHighEndBeamPeralte = design.LegacyHighEndBeamPeralte > 0.0
                ? design.LegacyHighEndBeamPeralte
                : (double?)null;
            document.RearTopeSaque = design.RearTope != null && design.RearTope.Saque > 0.0
                ? design.RearTope.Saque
                : (double?)null;
            // PB-005: only a real choice is written. A blank id stays absent from the file, so a rack that never chose
            // a variant is byte-identical to what previous builds wrote.
            document.RearTopePieceId = string.IsNullOrWhiteSpace(design.RearTope?.PieceId)
                ? null
                : design.RearTope.PieceId.Trim();

            // I-41: la entrada por frente se escribe si aporta ALGO — peraltes, fondo por defecto, algun override de
            // fondo o alguna tarima. Un rack anterior a I-41 sigue decidiendose solo por los peraltes, y los tres
            // campos nuevos quedan ausentes del archivo.
            if (design.Fronts != null && design.Fronts.Any(HasContent))
            {
                document.Fronts = design.Fronts
                    .Select(front => new PushBackFrontDocument
                    {
                        HighEndBeamPeraltes = front?.HighEndBeamPeraltes.ToList() ?? new List<double?>(),
                        DefaultPalletsDeep = front?.DefaultPalletsDeep,
                        // Listas ausentes cuando no hay nada que decir: asi un rack sin overrides ni tarimas produce
                        // exactamente el JSON que producian las versiones anteriores.
                        PalletsDeepOverrides = front != null && front.PalletsDeepOverrides.Any(value => value.HasValue)
                            ? front.PalletsDeepOverrides.ToList()
                            : null,
                        DrawPallets = front != null && front.DrawPallets.Any(value => value.GetValueOrDefault(false))
                            ? front.DrawPallets.ToList()
                            : null
                    })
                    .ToList();
            }

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
                LegacyHighEndBeamPeralte = LegacyHighEndBeamPeralte ?? PushBackDefaults.HighEndBeamDefaultPeralte
            };

            if (Fronts != null)
            {
                foreach (var frontDocument in Fronts)
                {
                    // I-41: los tres campos nuevos son nullable y su ausencia ES el fallback legacy — fondo por
                    // defecto = el estructural (lo resuelve el resolver), sin overrides, y sin ninguna tarima.
                    var config = new PushBackFrontConfig { DefaultPalletsDeep = frontDocument?.DefaultPalletsDeep };
                    if (frontDocument?.HighEndBeamPeraltes != null)
                    {
                        foreach (var peralte in frontDocument.HighEndBeamPeraltes)
                        {
                            config.HighEndBeamPeraltes.Add(peralte);
                        }
                    }

                    if (frontDocument?.PalletsDeepOverrides != null)
                    {
                        foreach (var deep in frontDocument.PalletsDeepOverrides)
                        {
                            config.PalletsDeepOverrides.Add(deep);
                        }
                    }

                    if (frontDocument?.DrawPallets != null)
                    {
                        foreach (var draw in frontDocument.DrawPallets)
                        {
                            config.DrawPallets.Add(draw);
                        }
                    }

                    design.Fronts.Add(config);
                }
            }

            design.RearTope.Saque = RearTopeSaque ?? PushBackDefaults.RearTopeSaque;
            design.RearTope.PieceId = string.IsNullOrWhiteSpace(RearTopePieceId) ? null : RearTopePieceId.Trim();
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

    /// <summary>
    /// Per-front Push Back document: the high-end (rear) beam peraltes by level (null = inherit the fallback) and,
    /// since I-41, the front's default fondo plus the per-level fondo override and pallet flag. The three I-41 fields
    /// are nullable and ABSENT in every document written before the initiative; their absence is the legacy fallback
    /// (default = the structural fondo, no override, no pallet), so an old rack loads and draws exactly as before.
    /// </summary>
    public sealed class PushBackFrontDocument
    {
        public List<double?> HighEndBeamPeraltes { get; set; }

        /// <summary>I-41 (PB-015): fondo POR DEFECTO del frente. Null = el fondo estructural (documento legacy).</summary>
        public int? DefaultPalletsDeep { get; set; }

        /// <summary>I-41 (PB-015): override de fondo por nivel. Null (lista o entrada) = hereda el default.</summary>
        public List<int?> PalletsDeepOverrides { get; set; }

        /// <summary>I-41 (PB-016): tarima por nivel. Null (lista o entrada) = false, el default legacy.</summary>
        public List<bool?> DrawPallets { get; set; }
    }

    /// <summary>One (front, level) cell in a Push Back document (a rear-tope deactivation).</summary>
    public sealed class PushBackCellDocument
    {
        public int Frente { get; set; }
        public int Level { get; set; }
    }
}
