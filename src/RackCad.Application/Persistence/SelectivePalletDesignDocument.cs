using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// Serializable snapshot of a pallet-driven selective design (the whole state of the advanced editor)
    /// plus its identity (<see cref="Id"/> + <see cref="Name"/>). This is what gets embedded in the drawing
    /// so a rack can be reopened and edited later. Round-trips through <see cref="SelectivePalletDesignStore"/>.
    /// </summary>
    public sealed class SelectivePalletDesignDocument
    {
        public string SchemaVersion { get; set; } = "1.0";

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

        /// <summary>Per-post cabeceras (one per post; null = run default), each embedded as a frame document.</summary>
        public List<RackFrameProjectDocument> PostCabeceras { get; set; } = new List<RackFrameProjectDocument>();

        /// <summary>Per-post PERALTE overrides (one per post; &lt;= 0 = inherit <see cref="PostPeralte"/>).</summary>
        public List<double> PostPeraltes { get; set; } = new List<double>();

        /// <summary>Drawing toggles. DrawBasePlate is nullable so legacy designs (no field) keep drawing the plate.</summary>
        public bool? DrawBasePlate { get; set; }
        public bool NumberFronts { get; set; }
        public bool NumberLevels { get; set; }
        public bool DrawRackName { get; set; }

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

            document.PostPeraltes = design.PostPeraltes.ToList();
            document.DrawBasePlate = design.DrawBasePlate;
            document.NumberFronts = design.NumberFronts;
            document.NumberLevels = design.NumberLevels;
            document.DrawRackName = design.DrawRackName;
            document.AnnotationScale = design.AnnotationScale;
            document.Dimensions = (int)design.Dimensions;
            document.DimensionStyle = design.DimensionStyle;
            document.SafetySelections = design.SafetySelections
                .Select(s => new SafetySelectionDocument
                {
                    ElementId = s.ElementId,
                    Quantity = s.Quantity,
                    Side = (int)s.Side,
                    PostSides = s.PostSides.Where(p => p != null).Select(p => new PostSideDocument { PostIndex = p.PostIndex, Side = (int)p.Side }).ToList(),
                    TopeShared = s.TopeShared,
                    TopeSaque = s.TopeSaque,
                    TopeFrontal = s.TopeFrontal,
                    TopeOffCells = s.TopeOffCells.Where(c => c != null).Select(c => new GridCellDocument { Frente = c.Frente, Level = c.Level }).ToList()
                }).ToList();

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

            foreach (var peralte in PostPeraltes ?? Enumerable.Empty<double>())
            {
                design.PostPeraltes.Add(peralte);
            }

            design.DrawBasePlate = DrawBasePlate ?? true; // legacy designs (no field) keep drawing the plate
            design.NumberFronts = NumberFronts;
            design.NumberLevels = NumberLevels;
            design.DrawRackName = DrawRackName;
            design.AnnotationScale = AnnotationScale.HasValue && AnnotationScale.Value > 0.0 ? AnnotationScale.Value : 1.0;
            design.Dimensions = ToDimensionDetail(Dimensions);
            design.DimensionStyle = string.IsNullOrWhiteSpace(DimensionStyle) ? null : DimensionStyle.Trim();
            foreach (var safety in SafetySelections ?? Enumerable.Empty<SafetySelectionDocument>())
            {
                if (safety != null && !string.IsNullOrWhiteSpace(safety.ElementId))
                {
                    var selection = new SelectiveSafetySelection
                    {
                        ElementId = safety.ElementId,
                        Quantity = safety.Quantity,
                        Side = ToSafetySide(safety.Side),
                        TopeShared = safety.TopeShared ?? true, // legacy docs (no field) default to shared
                        TopeSaque = safety.TopeSaque.HasValue && safety.TopeSaque.Value > 0.0 ? safety.TopeSaque.Value : 3.0,
                        TopeFrontal = safety.TopeFrontal ?? false
                    };
                    foreach (var post in safety.PostSides ?? Enumerable.Empty<PostSideDocument>())
                    {
                        if (post != null && post.PostIndex >= 0)
                        {
                            selection.PostSides.Add(new SafetyPostSide { PostIndex = post.PostIndex, Side = ToSafetySide(post.Side) });
                        }
                    }

                    foreach (var cell in safety.TopeOffCells ?? Enumerable.Empty<GridCellDocument>())
                    {
                        if (cell != null && cell.Frente >= 0 && cell.Level >= 0)
                        {
                            selection.TopeOffCells.Add(new SelectiveGridCell { Frente = cell.Frente, Level = cell.Level });
                        }
                    }

                    design.SafetySelections.Add(selection);
                }
            }

            return design;
        }

        /// <summary>Map the persisted int to <see cref="SafetySide"/>; null/out-of-range (legacy) defaults to Both.</summary>
        private static SafetySide ToSafetySide(int? value)
        {
            if (!value.HasValue) return SafetySide.Both; // legacy designs (no field) kept the default
            return value.Value >= (int)SafetySide.None && value.Value <= (int)SafetySide.Both
                ? (SafetySide)value.Value
                : SafetySide.Both;
        }

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
        public List<GridCellDocument> TopeOffCells { get; set; }
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
