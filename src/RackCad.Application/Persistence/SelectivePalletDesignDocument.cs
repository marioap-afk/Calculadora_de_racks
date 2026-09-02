using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// Serializable snapshot of a pallet-driven selective design (the whole state of the advanced editor)
    /// plus its identity (<see cref="Id"/> + <see cref="Name"/>). This is what gets embedded in the drawing
    /// so a rack can be reopened and edited later. Round-trips through <see cref="SelectivePalletDesignStore"/>.
    /// </summary>
    public sealed class SelectivePalletDesignDocument
    {
        /// <summary>Schema version this build writes; a file with a higher MAJOR is rejected (see <see cref="SchemaGuard"/>).</summary>
        public const string CurrentSchemaVersion = "1.0";

        public string SchemaVersion { get; set; } = CurrentSchemaVersion;

        /// <summary>Stable identity of the rack (GUID string). Kept across edits; assigned by the caller.</summary>
        public string Id { get; set; }

        /// <summary>Client-facing name ("Rack A"); may be empty (an auto-name is used then).</summary>
        public string Name { get; set; }

        public string PostId { get; set; }
        public double PostPeralte { get; set; }
        public double PalletTolerance { get; set; }
        public double VerticalClearance { get; set; }
        public double FloorBeamRise { get; set; }
        public double PalletDepth { get; set; }

        /// <summary>Number of fondos (cabecera-lines in depth). Nullable so legacy documents (no field) load as a single fondo.</summary>
        public int? DepthCount { get; set; }

        /// <summary>Separations (in) between consecutive fondos, one per gap (DepthCount-1). Empty = defaults apply.</summary>
        public List<double> SeparatorLengths { get; set; } = new List<double>();

        /// <summary>Per-fondo pallet depth for fondos 1..N-1 (&lt;=0 = inherit fondo 0's PalletDepth). Empty = all share PalletDepth.</summary>
        public List<double> ExtraFondoDepths { get; set; } = new List<double>();

        /// <summary>Optional custom cabecera-depth per fondo (index k; &lt;=0 = derived by the rule). Empty = every fondo derived.</summary>
        public List<double> CabeceraFondoOverrides { get; set; } = new List<double>();

        public List<SelectiveBayDocument> Bays { get; set; } = new List<SelectiveBayDocument>();

        /// <summary>Per-fondo level matrices for fondos 1..N-1 (each a list of bays). Empty = every fondo shares fondo 0's <see cref="Bays"/>.</summary>
        public List<List<SelectiveBayDocument>> ExtraFondoBays { get; set; } = new List<List<SelectiveBayDocument>>();

        /// <summary>Per-post cabeceras of FONDO 0 (one per post; null = run default), each embedded as a frame
        /// document. This is the legacy field and keeps its legacy meaning exactly: a document written before I-43
        /// carries only this, and it describes fondo 0 alone.</summary>
        public List<RackFrameProjectDocument> PostCabeceras { get; set; } = new List<RackFrameProjectDocument>();

        /// <summary>
        /// Per-post cabeceras of the fondos AFTER fondo 0 (I-43): entry <c>k-1</c> is fondo <c>k</c>. NULLABLE and
        /// additive — a legacy document has no such field, which deserializes to null and means "every extra fondo is
        /// standard". That is precisely what those drawings showed, so nothing is migrated and no existing rack changes
        /// when it is reopened. Rows may be null, short, or hold nulls; all three mean "standard" at that post.
        /// </summary>
        public List<List<RackFrameProjectDocument>> ExtraFondoPostCabeceras { get; set; }

        /// <summary>Per-post PERALTE overrides (one per post; &lt;= 0 = inherit <see cref="PostPeralte"/>).</summary>
        public List<double> PostPeraltes { get; set; } = new List<double>();

        /// <summary>Drawing toggles. DrawBasePlate is nullable so legacy designs (no field) keep drawing the plate.</summary>
        public bool? DrawBasePlate { get; set; }
        public bool NumberFronts { get; set; }
        public bool NumberLevels { get; set; }
        public bool DrawRackName { get; set; }
        public bool DrawPallets { get; set; }

        /// <summary>Annotation text scale (1 = default). Nullable so legacy designs (no field) keep scale 1.</summary>
        public double? AnnotationScale { get; set; }

        /// <summary>Dimension detail (0=None..3=Detailed). Nullable so legacy designs (no field) keep dimensions off.</summary>
        public int? Dimensions { get; set; }

        /// <summary>Chosen AutoCAD dimension style name (null/empty = automatic).</summary>
        public string DimensionStyle { get; set; }

        /// <summary>Selected safety accessories (id + quantity). Null/empty for legacy designs (no field).</summary>
        public List<SafetySelectionDocument> SafetySelections { get; set; }

        public static SelectivePalletDesignDocument From(SelectivePalletDesign design, string id, string name)
        {
            if (design == null)
            {
                throw new ArgumentNullException(nameof(design));
            }

            var document = new SelectivePalletDesignDocument
            {
                Id = id,
                Name = name,
                PostId = design.PostId,
                PostPeralte = design.PostPeralte,
                PalletTolerance = design.PalletTolerance,
                VerticalClearance = design.VerticalClearance,
                FloorBeamRise = design.FloorBeamRise,
                PalletDepth = design.PalletDepth,
                DepthCount = design.DepthCount,
                SeparatorLengths = design.SeparatorLengths.ToList(),
                ExtraFondoDepths = design.ExtraFondoDepths.ToList(),
                CabeceraFondoOverrides = design.CabeceraFondoOverrides.ToList()
            };

            foreach (var bay in design.Bays)
            {
                document.Bays.Add(SelectiveBayDocument.From(bay));
            }

            foreach (var fondo in design.ExtraFondoBays)
            {
                var fondoDoc = new List<SelectiveBayDocument>();
                if (fondo != null)
                {
                    foreach (var bay in fondo)
                    {
                        fondoDoc.Add(SelectiveBayDocument.From(bay));
                    }
                }

                document.ExtraFondoBays.Add(fondoDoc);
            }

            foreach (var cabecera in design.PostCabeceras)
            {
                document.PostCabeceras.Add(cabecera == null ? null : RackFrameProjectDocument.FromConfiguration(cabecera));
            }

            // The extra fondos' rows are written ONLY when at least one of them carries something. Writing an empty
            // structure into every design would churn the JSON of every single-fondo rack for no information (I-43).
            if (design.ExtraFondoPostCabeceras.Any(row => row != null && row.Any(cabecera => cabecera != null)))
            {
                document.ExtraFondoPostCabeceras = new List<List<RackFrameProjectDocument>>();
                foreach (var row in design.ExtraFondoPostCabeceras)
                {
                    document.ExtraFondoPostCabeceras.Add(row == null
                        ? null
                        : row.Select(cabecera => cabecera == null ? null : RackFrameProjectDocument.FromConfiguration(cabecera)).ToList());
                }
            }

            document.PostPeraltes = design.PostPeraltes.ToList();
            document.DrawBasePlate = design.DrawBasePlate;
            document.NumberFronts = design.NumberFronts;
            document.NumberLevels = design.NumberLevels;
            document.DrawRackName = design.DrawRackName;
            document.DrawPallets = design.DrawPallets;
            document.AnnotationScale = design.AnnotationScale;
            document.Dimensions = (int)design.Dimensions;
            document.DimensionStyle = design.DimensionStyle;
            document.SafetySelections = design.SafetySelections
                .Where(s => s != null)
                .Select(SafetySelectionDocument.From)
                .ToList();

            return document;
        }

        public SelectivePalletDesign ToDomain()
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId,
                PostPeralte = PostPeralte,
                PalletTolerance = PalletTolerance,
                VerticalClearance = VerticalClearance,
                FloorBeamRise = FloorBeamRise,
                PalletDepth = PalletDepth > 0.0 ? PalletDepth : SelectiveRackDefaults.DefaultPalletDepth, // legacy docs had no fondo
                DepthCount = DepthCount.HasValue && DepthCount.Value > 0 ? DepthCount.Value : 1 // legacy docs = single fondo
            };

            foreach (var separator in SeparatorLengths ?? Enumerable.Empty<double>())
            {
                design.SeparatorLengths.Add(separator);
            }

            foreach (var fondoDepth in ExtraFondoDepths ?? Enumerable.Empty<double>())
            {
                design.ExtraFondoDepths.Add(fondoDepth);
            }

            foreach (var cabeceraOverride in CabeceraFondoOverrides ?? Enumerable.Empty<double>())
            {
                design.CabeceraFondoOverrides.Add(cabeceraOverride);
            }

            foreach (var bay in Bays ?? Enumerable.Empty<SelectiveBayDocument>())
            {
                design.Bays.Add(bay.ToDomain());
            }

            foreach (var fondo in ExtraFondoBays ?? Enumerable.Empty<List<SelectiveBayDocument>>())
            {
                var fondoDesign = new List<SelectiveBayDesign>();
                foreach (var bay in fondo ?? Enumerable.Empty<SelectiveBayDocument>())
                {
                    fondoDesign.Add(bay.ToDomain());
                }

                design.ExtraFondoBays.Add(fondoDesign);
            }

            foreach (var cabecera in PostCabeceras ?? Enumerable.Empty<RackFrameProjectDocument>())
            {
                design.PostCabeceras.Add(cabecera?.ToConfiguration());
            }

            // Absent field (a legacy document) = no extra rows at all = every fondo after 0 is standard. There is no
            // migration: propagating fondo 0's customs to the other fondos would change drawings that already exist.
            foreach (var row in ExtraFondoPostCabeceras ?? Enumerable.Empty<List<RackFrameProjectDocument>>())
            {
                design.ExtraFondoPostCabeceras.Add(row == null
                    ? new List<RackFrameConfiguration>()
                    : row.Select(cabecera => cabecera?.ToConfiguration()).ToList());
            }

            foreach (var peralte in PostPeraltes ?? Enumerable.Empty<double>())
            {
                design.PostPeraltes.Add(peralte);
            }

            design.DrawBasePlate = DrawBasePlate ?? true; // legacy designs (no field) keep drawing the plate
            design.NumberFronts = NumberFronts;
            design.NumberLevels = NumberLevels;
            design.DrawRackName = DrawRackName;
            design.DrawPallets = DrawPallets;
            design.AnnotationScale = AnnotationScale.HasValue && AnnotationScale.Value > 0.0 ? AnnotationScale.Value : 1.0;
            design.Dimensions = ToDimensionDetail(Dimensions);
            design.DimensionStyle = string.IsNullOrWhiteSpace(DimensionStyle) ? null : DimensionStyle.Trim();
            foreach (var safety in SafetySelections ?? Enumerable.Empty<SafetySelectionDocument>())
            {
                if (safety != null && !string.IsNullOrWhiteSpace(safety.ElementId))
                {
                    design.SafetySelections.Add(safety.ToDomain());
                }
            }

            return design;
        }

        /// <summary>Map the persisted int to <see cref="SafetySide"/>; null/out-of-range (legacy) defaults to Both.</summary>
        /// <summary>Map the persisted int to <see cref="DimensionDetail"/>, clamping out-of-range/legacy values to None.</summary>
        private static DimensionDetail ToDimensionDetail(int? value)
        {
            if (!value.HasValue) return DimensionDetail.None; // legacy docs (no field) draw no dimensions
            return value.Value >= (int)DimensionDetail.None && value.Value <= (int)DimensionDetail.Detailed
                ? (DimensionDetail)value.Value
                : DimensionDetail.None;
        }
    }

    /// <summary>One frente (bay) column: its "larguero a piso" flag, optional height override, medio-frente tramos, and level cells.</summary>
    public sealed class SelectiveBayDocument
    {
        public bool FloorBeam { get; set; }
        public double? HeightOverride { get; set; }

        /// <summary>"Medio frente" generalizado: N tramos (each length + loaded); the last length is calculated. Empty = full bay.</summary>
        public List<SelectiveSegmentDocument> Segments { get; set; } = new List<SelectiveSegmentDocument>();

        /// <summary>LEGACY (pre-N-way) single medio-frente length (in). Read-only fallback: when <see cref="Segments"/> is
        /// empty and this is &gt; 0, it maps to a loaded tramo + an empty calculated remainder. Newer docs use Segments.</summary>
        public double MedioFrenteLength { get; set; }

        public List<SelectiveCellDocument> Levels { get; set; } = new List<SelectiveCellDocument>();

        public static SelectiveBayDocument From(SelectiveBayDesign bay)
        {
            var document = new SelectiveBayDocument
            {
                FloorBeam = bay.FloorBeam,
                HeightOverride = bay.HeightOverride
            };

            foreach (var segment in bay.Segments)
            {
                document.Segments.Add(new SelectiveSegmentDocument { Length = segment.Length, Loaded = segment.Loaded });
            }

            foreach (var cell in bay.Levels)
            {
                document.Levels.Add(SelectiveCellDocument.From(cell));
            }

            return document;
        }

        public SelectiveBayDesign ToDomain()
        {
            var bay = new SelectiveBayDesign
            {
                FloorBeam = FloorBeam,
                HeightOverride = HeightOverride
            };

            if (Segments != null && Segments.Count > 0)
            {
                foreach (var segment in Segments)
                {
                    bay.Segments.Add(new SelectiveSegment { Length = segment.Length, Loaded = segment.Loaded });
                }
            }
            else if (MedioFrenteLength > 0.0)
            {
                // Legacy single medio frente: a custom LOADED tramo + an empty CALCULATED remainder (the classic ½frente).
                bay.Segments.Add(new SelectiveSegment { Length = MedioFrenteLength, Loaded = true });
                bay.Segments.Add(new SelectiveSegment { Length = 0.0, Loaded = false });
            }

            foreach (var cell in Levels ?? Enumerable.Empty<SelectiveCellDocument>())
            {
                bay.Levels.Add(cell.ToDomain());
            }

            return bay;
        }
    }

    /// <summary>One serialized medio-frente tramo: a larguero length + whether it carries largueros (the last tramo's length is calculated).</summary>
    public sealed class SelectiveSegmentDocument
    {
        public double Length { get; set; }
        public bool Loaded { get; set; } = true;
    }

    /// <summary>One selected safety accessory: its catalog id, quantity, the default side, and per-post side overrides.
    /// Side is nullable so legacy designs (no field) default to Both; PostSides is null/empty when there are none.</summary>
    public sealed class SafetySelectionDocument
    {
        public string ElementId { get; set; }
        public int Quantity { get; set; }
        public int? Side { get; set; }
        public List<PostSideDocument> PostSides { get; set; }

        /// <summary>TOPE-only: shared central tope vs one per fondo (nullable = legacy → shared), the SAQUE, whether it draws
        /// in the frontal, and the skipped cells.</summary>
        public bool? TopeShared { get; set; }
        public double? TopeSaque { get; set; }
        public bool? TopeFrontal { get; set; }
        public int? TopeFondo { get; set; }
        public List<GridCellDocument> TopeOffCells { get; set; }
        public double? DesviadorLongitud { get; set; }
        public double? DesviadorPrimerNivelAltura { get; set; }
        public List<GridCellDocument> DesviadorOffCells { get; set; }
        public List<PostDefenseDocument> DefensaPosts { get; set; }
        public List<GridCellDocument> GuiaEntradaOffCells { get; set; }
        public bool? ParrillaFrontal { get; set; }
        public bool? ParrillaLateral { get; set; }
        public double? ParrillaFrente { get; set; }
        public int? ParrillaCantidad { get; set; }
        public List<GridCellDocument> ParrillaOffCells { get; set; }

        /// <summary>
        /// I-42 (S1B) — BOTA: la colocacion general elegida (nulo = la resuelve el sistema) y los postes con
        /// decision propia. Aditivo: un documento anterior no los trae y se lee por su <see cref="Side"/> historico,
        /// cuya intencion es la misma (Izquierda = entrada/salida, Derecha = posterior).
        /// </summary>
        public int? BotaPlacement { get; set; }

        public List<BootPostDocument> BotaPosts { get; set; }

        /// <summary>
        /// I-42 (S1E) — BOTA del LADO B de un rack compuesto: su propia colocacion general y sus propios postes.
        /// Aditivo y nulo por omision: un documento anterior no los trae, y entonces su unica configuracion se lee
        /// como la del lado A —que es como se dibujaba— y el lado B no pide nada.
        /// </summary>
        public int? BotaBPlacement { get; set; }

        public List<BootPostDocument> BotaBPosts { get; set; }

        /// <summary>
        /// I-42 (S1E) — true cuando este documento declara las botas POR LADO. Ausente = documento anterior, cuya
        /// unica configuracion se lee como la del lado A y deja el lado B sin pedir nada, que es como se dibujaba.
        /// </summary>
        public bool? BotaSidesDeclared { get; set; }

        /// <summary>
        /// I-42 (S1G) — el TIPO de pieza de cada lado. Ausente = ese lado nunca lo eligio y hereda el del documento
        /// (<see cref="ElementId"/>), que es como se guardaba antes: un documento anterior abre con la misma pieza
        /// en los dos lados y dibuja exactamente lo que dibujaba.
        /// </summary>
        public string BotaPieceId { get; set; }

        public string BotaBPieceId { get; set; }

        /// <summary>
        /// I-42 (A1/B1) — el lado que el USUARIO eligio, antes de que la restriccion de extremo de un sistema lo
        /// colapse. Ausente = documento anterior, que se lee por <see cref="Side"/> como siempre.
        ///
        /// <para>
        /// Sin el, un rack compuesto guardado SIN abrir «Elementos de seguridad» persistia el lado ya colapsado
        /// —«Izquierda»— y al reabrirlo parecia un documento antiguo con una decision global explicita: el lado B
        /// perdia su configuracion automatica y sus botas desaparecian del dibujo y del BOM.
        /// </para>
        /// </summary>
        public int? AuthoredSide { get; set; }

        /// <summary>
        /// I-42 (A1/H8) — los postes cuya entrada en <see cref="PostSides"/> la escribio una regla DERIVADA (los
        /// pasillos de carga que el rack tiene ahora), con el valor que el usuario tenia ahi. No se persiste el
        /// derivado: se persiste lo que el usuario habia decidido, para que degradar un compuesto a un solo sentido
        /// no deje un lado rancio que mueva el desviador al extremo alto.
        /// </summary>
        public List<PostSideDocument> DerivedAisles { get; set; }

        /// <summary>Shared explicit mapping used by every rack system that composes the safety subsystem. The wire
        /// format is a FLAT record (unchanged, and shared with the dynamic path); each family flattens its own DTO
        /// (I-22, E7 — <see cref="TopeSelectionDocument"/> and siblings) into these flat properties and reads it back
        /// out, so on-disk names, nullability, effective order and legacy fallbacks are byte-for-byte the prior wire.</summary>
        public static SafetySelectionDocument From(SelectiveSafetySelection selection)
        {
            var document = new SafetySelectionDocument
            {
                ElementId = selection.ElementId,
                Quantity = selection.Quantity,
                Side = (int)selection.Side,
                AuthoredSide = selection.AuthoredSide.HasValue ? (int)selection.AuthoredSide.Value : (int?)null,
                PostSides = AuthoredPostSides(selection)
            };

            TopeSelectionDocument.From(selection.Tope).WriteInto(document);
            DesviadorSelectionDocument.From(selection.Desviador).WriteInto(document);
            DefensaSelectionDocument.From(selection.Defensa).WriteInto(document);
            GuiaSelectionDocument.From(selection.Guia).WriteInto(document);
            ParrillaSelectionDocument.From(selection.Parrilla).WriteInto(document);
            document.BotaPlacement = selection.Bota.Placement.HasValue ? (int)selection.Bota.Placement.Value : (int?)null;
            document.BotaPosts = BootPosts(selection.Bota);
            document.BotaBPlacement = selection.BotaB.Placement.HasValue
                ? (int)selection.BotaB.Placement.Value
                : (int?)null;
            document.BotaBPosts = BootPosts(selection.BotaB);
            document.BotaSidesDeclared = selection.BootSidesDeclared ? true : (bool?)null;
            document.BotaPieceId = selection.Bota.PieceId;
            document.BotaBPieceId = selection.BotaB.PieceId;
            return document;
        }

        /// <summary>
        /// La matriz por poste TAL Y COMO EL USUARIO LA DEJO: las entradas que una regla derivada escribio se
        /// devuelven a su valor autorado, y las que esa regla creo no se guardan.
        /// </summary>
        private static List<PostSideDocument> AuthoredPostSides(SelectiveSafetySelection selection)
        {
            var derived = selection.DerivedAisles.Where(entry => entry != null).ToList();
            var result = new List<PostSideDocument>();
            foreach (var post in selection.PostSides.Where(entry => entry != null))
            {
                var overwritten = derived.FirstOrDefault(entry => entry.PostIndex == post.PostIndex);
                if (overwritten == null)
                {
                    result.Add(new PostSideDocument { PostIndex = post.PostIndex, Side = (int)post.Side });
                    continue;
                }

                if (overwritten.Authored.HasValue)
                {
                    result.Add(new PostSideDocument
                    {
                        PostIndex = post.PostIndex,
                        Side = (int)overwritten.Authored.Value,
                    });
                }
            }

            return result;
        }

        /// <summary>Los postes con decision propia de un lado, o NULL cuando no hay ninguno.</summary>
        private static List<BootPostDocument> BootPosts(SelectiveBotaConfig config)
            => config == null || config.Posts.Count == 0
                ? null
                : config.Posts.Where(post => post != null)
                    .Select(post => new BootPostDocument { PostIndex = post.PostIndex, Placement = (int)post.Placement })
                    .ToList();

        /// <summary>Version-tolerant domain mapping. Each family reconstructs its own config subtype from its DTO, with
        /// the exact legacy fallback for a missing field (I-22, E7).</summary>
        public SelectiveSafetySelection ToDomain()
        {
            var selection = new SelectiveSafetySelection
            {
                ElementId = ElementId,
                Quantity = Quantity,
                Side = SafetyDocumentMapping.ToSafetySide(Side),
                AuthoredSide = AuthoredSide.HasValue
                    ? SafetyDocumentMapping.ToSafetySide(AuthoredSide.Value)
                    : (SafetySide?)null,
                Tope = TopeSelectionDocument.ReadFrom(this).ToDomain(),
                Desviador = DesviadorSelectionDocument.ReadFrom(this).ToDomain(),
                Defensa = DefensaSelectionDocument.ReadFrom(this).ToDomain(),
                Guia = GuiaSelectionDocument.ReadFrom(this).ToDomain(),
                Parrilla = ParrillaSelectionDocument.ReadFrom(this).ToDomain()
            };

            foreach (var post in PostSides ?? Enumerable.Empty<PostSideDocument>())
            {
                if (post != null && post.PostIndex >= 0)
                {
                    selection.PostSides.Add(new SafetyPostSide { PostIndex = post.PostIndex, Side = SafetyDocumentMapping.ToSafetySide(post.Side) });
                }
            }

            // I-42 (S1B): la colocacion de la bota. Ausente = documento anterior, que se lee por su lado historico.
            if (BotaPlacement.HasValue && Enum.IsDefined(typeof(BootPlacement), BotaPlacement.Value))
            {
                selection.Bota.Placement = (BootPlacement)BotaPlacement.Value;
            }

            BootPostDocumentMapping.Read(BotaPosts, selection.Bota);

            // I-42 (S1E): y la del lado B, si el documento la trae. Ausente = documento anterior a S1E.
            if (BotaBPlacement.HasValue && Enum.IsDefined(typeof(BootPlacement), BotaBPlacement.Value))
            {
                selection.BotaB.Placement = (BootPlacement)BotaBPlacement.Value;
            }

            BootPostDocumentMapping.Read(BotaBPosts, selection.BotaB);
            selection.BootSidesDeclared = BotaSidesDeclared ?? false;
            selection.Bota.PieceId = BotaPieceId;
            selection.BotaB.PieceId = BotaBPieceId;

            return selection;
        }

    }

    /// <summary>I-42 (S1B) — la colocacion de bota que UN poste declara por su cuenta.</summary>
    public static class BootPostDocumentMapping
    {
        /// <summary>Vuelca los postes de un lado en su configuracion, ignorando lo que no sea un valor valido.</summary>
        public static void Read(IEnumerable<BootPostDocument> posts, SelectiveBotaConfig config)
        {
            foreach (var post in posts ?? Enumerable.Empty<BootPostDocument>())
            {
                if (post != null && post.PostIndex >= 0 && Enum.IsDefined(typeof(BootPlacement), post.Placement))
                {
                    config.Posts.Add(new BootPostPlacement
                    {
                        PostIndex = post.PostIndex,
                        Placement = (BootPlacement)post.Placement,
                    });
                }
            }
        }
    }

    public sealed class BootPostDocument
    {
        public int PostIndex { get; set; }

        public int Placement { get; set; }
    }

    /// <summary>A serialized (frente, level) cell — a tope cell that is turned off.</summary>
    public sealed class GridCellDocument
    {
        public int Frente { get; set; }
        public int Level { get; set; }
    }

    /// <summary>A per-post side override for a safety selection (post index → side int).</summary>
    public sealed class PostSideDocument
    {
        public int PostIndex { get; set; }
        public int? Side { get; set; }
    }

    /// <summary>A persisted per-post forklift-defense length; zero explicitly disables that post.</summary>
    public sealed class PostDefenseDocument
    {
        public int PostIndex { get; set; }
        public double? ExitLength { get; set; }
        public double? EntranceLength { get; set; }

        /// <summary>
        /// PB-010 (I-32) — this end follows the automatic 12"/36" rule instead of the stored length. NULL is the legacy
        /// value every earlier document carries and reads as FALSE (an explicit override), so a saved rack keeps the
        /// exact lengths it was saved with.
        /// </summary>
        public bool? ExitAuto { get; set; }

        /// <summary>PB-010 — same, for the other end. NULL reads as an explicit override (legacy).</summary>
        public bool? EntranceAuto { get; set; }
    }

    /// <summary>One matrix cell (a level of a frente): pallet, count, beam, and the optional manual overrides.</summary>
    public sealed class SelectiveCellDocument
    {
        public double Frente { get; set; }
        public double Alto { get; set; }
        public int PalletCount { get; set; }
        public string BeamId { get; set; }
        public double BeamPeralte { get; set; }
        public double? BeamLengthOverride { get; set; }
        public double? ClearOverride { get; set; }

        public static SelectiveCellDocument From(SelectiveCell cell)
        {
            return new SelectiveCellDocument
            {
                Frente = cell.Pallet?.Frente ?? 0.0,
                Alto = cell.Pallet?.Alto ?? 0.0,
                PalletCount = cell.PalletCount,
                BeamId = cell.BeamId,
                BeamPeralte = cell.BeamPeralte,
                BeamLengthOverride = cell.BeamLengthOverride,
                ClearOverride = cell.ClearOverride
            };
        }

        public SelectiveCell ToDomain()
        {
            return new SelectiveCell
            {
                Pallet = new Tarima { Frente = Frente, Alto = Alto },
                PalletCount = PalletCount,
                BeamId = BeamId,
                BeamPeralte = BeamPeralte,
                BeamLengthOverride = BeamLengthOverride,
                ClearOverride = ClearOverride
            };
        }
    }
}
