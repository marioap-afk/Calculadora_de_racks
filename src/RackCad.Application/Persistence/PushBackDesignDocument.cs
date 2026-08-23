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

        /// <summary>
        /// I-42 — la configuracion funcional del LADO B. NULL es el rack de un solo sentido: todo documento anterior
        /// a I-42 lo trae ausente y por eso carga y dibuja exactamente igual, sin pedir reconfiguracion.
        /// <para>
        /// Se OMITE del JSON cuando es nula: sin eso, un rack anterior a I-42 empezaria a escribir un campo nuevo y
        /// su archivo dejaria de ser byte-identico al que producian las versiones anteriores.
        /// </para>
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PushBackSideDocument SideB { get; set; }

        /// <summary>
        /// I-42 — la intencion de la INTERFAZ: gap, separador central, topologia por celda y overrides de estructura
        /// por lado. NULL es el legacy (gap 0, sin separador, un solo lado activo y sin override).
        /// <para>Se OMITE del JSON cuando es nula, por la misma razon que <see cref="SideB"/>.</para>
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PushBackCompositeDocument Composite { get; set; }

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

            document.SideB = PushBackSideDocument.From(design.SideB);
            document.Composite = PushBackCompositeDocument.From(design.Composite);

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

            design.SideB = SideB?.ToDomain();
            design.Composite = Composite?.ToDomain();

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

    /// <summary>
    /// I-42 - documento del LADO B: solo su configuracion FUNCIONAL. La estructura fisica (postes, cabeceras,
    /// separadores, postes derivados, overrides de linea de I-40, anotaciones y seguridad) NO se repite aqui: es
    /// propiedad unica del rack y viaja en <see cref="PushBackDesignDocument.Structure"/>. Repetirla haria posible
    /// que el archivo describiera dos estructuras distintas para un mismo rack fisico.
    /// </summary>
    public sealed class PushBackSideDocument
    {
        public bool? IsPresent { get; set; }
        public int? LoadLevels { get; set; }
        public double? FirstLevelHeight { get; set; }
        public double? LegacyHighEndBeamPeralte { get; set; }

        /// <summary>Frentes del lado por ranura transversal. Una entrada NULA = la ranura no existe en este lado.</summary>
        public List<DynamicRackFrontDocument> Fronts { get; set; }

        /// <summary>Configuracion Push Back por ranura (peralte posterior, fondo por celda, tarima).</summary>
        public List<PushBackFrontDocument> FrontConfigs { get; set; }

        public double? RearTopeSaque { get; set; }
        public string RearTopePieceId { get; set; }
        public List<PushBackCellDocument> RearTopeOffCells { get; set; }

        public static PushBackSideDocument From(PushBackSideDesign side)
        {
            if (side == null)
            {
                return null;
            }

            var document = new PushBackSideDocument
            {
                IsPresent = side.IsPresent,
                LoadLevels = side.LoadLevels,
                FirstLevelHeight = side.FirstLevelHeight,
                LegacyHighEndBeamPeralte = side.LegacyHighEndBeamPeralte > 0.0
                    ? side.LegacyHighEndBeamPeralte
                    : (double?)null,
                RearTopeSaque = side.RearTope != null && side.RearTope.Saque > 0.0
                    ? side.RearTope.Saque
                    : (double?)null,
                RearTopePieceId = string.IsNullOrWhiteSpace(side.RearTope?.PieceId) ? null : side.RearTope.PieceId.Trim()
            };

            if (side.Fronts.Count > 0)
            {
                // La entrada nula se CONSERVA: es la que dice "esta ranura no existe en este lado", y perderla
                // desalinearia las ranuras al recargar.
                document.Fronts = side.Fronts
                    .Select(front => front == null ? null : DynamicRackFrontDocument.From(front))
                    .ToList();
            }

            if (side.FrontConfigs.Any(config => config != null))
            {
                document.FrontConfigs = side.FrontConfigs
                    .Select(config => config == null
                        ? null
                        : new PushBackFrontDocument
                        {
                            HighEndBeamPeraltes = config.HighEndBeamPeraltes.ToList(),
                            DefaultPalletsDeep = config.DefaultPalletsDeep,
                            PalletsDeepOverrides = config.PalletsDeepOverrides.Any(value => value.HasValue)
                                ? config.PalletsDeepOverrides.ToList()
                                : null,
                            DrawPallets = config.DrawPallets.Any(value => value.GetValueOrDefault(false))
                                ? config.DrawPallets.ToList()
                                : null
                        })
                    .ToList();
            }

            if (side.RearTope != null && side.RearTope.OffCells.Count > 0)
            {
                document.RearTopeOffCells = side.RearTope.OffCells
                    .Where(cell => cell != null)
                    .Select(cell => new PushBackCellDocument { Frente = cell.Frente, Level = cell.Level })
                    .ToList();
            }

            return document;
        }

        public PushBackSideDesign ToDomain()
        {
            var side = new PushBackSideDesign
            {
                IsPresent = IsPresent ?? true,
                LoadLevels = LoadLevels ?? DynamicRackDefaults.DefaultLoadLevels,
                FirstLevelHeight = FirstLevelHeight ?? PushBackDefaults.DefaultFirstLevelHeight,
                LegacyHighEndBeamPeralte = LegacyHighEndBeamPeralte ?? PushBackDefaults.HighEndBeamDefaultPeralte
            };

            if (Fronts != null)
            {
                foreach (var front in Fronts)
                {
                    side.Fronts.Add(front?.ToDesign(side.LoadLevels, DynamicRackDefaults.DefaultPalletsDeep));
                }
            }

            if (FrontConfigs != null)
            {
                foreach (var config in FrontConfigs)
                {
                    if (config == null)
                    {
                        side.FrontConfigs.Add(null);
                        continue;
                    }

                    var restored = new PushBackFrontConfig { DefaultPalletsDeep = config.DefaultPalletsDeep };
                    foreach (var peralte in config.HighEndBeamPeraltes ?? new List<double?>())
                    {
                        restored.HighEndBeamPeraltes.Add(peralte);
                    }

                    foreach (var deep in config.PalletsDeepOverrides ?? new List<int?>())
                    {
                        restored.PalletsDeepOverrides.Add(deep);
                    }

                    foreach (var draw in config.DrawPallets ?? new List<bool?>())
                    {
                        restored.DrawPallets.Add(draw);
                    }

                    side.FrontConfigs.Add(restored);
                }
            }

            side.RearTope.Saque = RearTopeSaque ?? PushBackDefaults.RearTopeSaque;
            side.RearTope.PieceId = string.IsNullOrWhiteSpace(RearTopePieceId) ? null : RearTopePieceId.Trim();
            if (RearTopeOffCells != null)
            {
                foreach (var cell in RearTopeOffCells)
                {
                    if (cell != null)
                    {
                        side.RearTope.OffCells.Add(new SelectiveGridCell { Frente = cell.Frente, Level = cell.Level });
                    }
                }
            }

            return side;
        }
    }

    /// <summary>
    /// I-42 - documento de la INTERFAZ compuesta: gap, separador central, overrides de estructura por lado y las
    /// celdas cuya topologia se APARTA del valor por defecto. Como en el tope posterior, solo se escribe la
    /// excepcion: una rejilla positiva completa nunca llega al archivo.
    /// </summary>
    public sealed class PushBackCompositeDocument
    {
        public double? Gap { get; set; }
        public bool? CentralSeparator { get; set; }
        public int? StructureOverrideA { get; set; }
        public int? StructureOverrideB { get; set; }
        public string DefaultTopology { get; set; }
        public string DefaultDirection { get; set; }
        public List<PushBackTopologyCellDocument> Topologies { get; set; }

        public static PushBackCompositeDocument From(PushBackCompositeDesign composite)
        {
            if (composite == null)
            {
                return null;
            }

            var document = new PushBackCompositeDocument
            {
                Gap = composite.Gap > 0.0 ? composite.Gap : (double?)null,
                CentralSeparator = composite.CentralSeparator ? true : (bool?)null,
                StructureOverrideA = composite.StructureOverrideA,
                StructureOverrideB = composite.StructureOverrideB,
                DefaultTopology = composite.DefaultTopology.ToString(),
                DefaultDirection = composite.DefaultDirection.ToString()
            };

            if (composite.Topologies.Any(cell => cell != null))
            {
                document.Topologies = composite.Topologies
                    .Where(cell => cell != null)
                    .Select(cell => new PushBackTopologyCellDocument
                    {
                        Frente = cell.Frente,
                        Level = cell.Level,
                        Topology = cell.Topology.ToString(),
                        Direction = cell.Direction.ToString()
                    })
                    .ToList();
            }

            return document;
        }

        public PushBackCompositeDesign ToDomain()
        {
            var composite = new PushBackCompositeDesign
            {
                Gap = Gap.HasValue && Gap.Value > 0.0 ? Gap.Value : 0.0,
                CentralSeparator = CentralSeparator ?? false,
                StructureOverrideA = StructureOverrideA,
                StructureOverrideB = StructureOverrideB,
                DefaultTopology = ParseTopology(DefaultTopology, PushBackCellTopology.Encontradas),
                DefaultDirection = ParseDirection(DefaultDirection, PushBackRunDirection.AToB)
            };

            if (Topologies != null)
            {
                foreach (var cell in Topologies)
                {
                    if (cell == null)
                    {
                        continue;
                    }

                    composite.Topologies.Add(new PushBackTopologyCell
                    {
                        Frente = cell.Frente,
                        Level = cell.Level,
                        Topology = ParseTopology(cell.Topology, composite.DefaultTopology),
                        Direction = ParseDirection(cell.Direction, composite.DefaultDirection)
                    });
                }
            }

            return composite;
        }

        /// <summary>
        /// Un nombre desconocido -de una version futura o de un archivo tocado a mano- NO aborta la carga: cae al
        /// valor por defecto declarado. La alternativa seria dejar el rack sin abrir por una celda.
        /// </summary>
        private static PushBackCellTopology ParseTopology(string value, PushBackCellTopology fallback)
            => System.Enum.TryParse(value, ignoreCase: true, out PushBackCellTopology parsed) ? parsed : fallback;

        private static PushBackRunDirection ParseDirection(string value, PushBackRunDirection fallback)
            => System.Enum.TryParse(value, ignoreCase: true, out PushBackRunDirection parsed) ? parsed : fallback;
    }

    /// <summary>I-42 - una celda (ranura, nivel) con topologia y sentido explicitos.</summary>
    public sealed class PushBackTopologyCellDocument
    {
        public int Frente { get; set; }
        public int Level { get; set; }
        public string Topology { get; set; }
        public string Direction { get; set; }
    }

    /// <summary>One (front, level) cell in a Push Back document (a rear-tope deactivation).</summary>
    public sealed class PushBackCellDocument
    {
        public int Frente { get; set; }
        public int Level { get; set; }
    }
}
