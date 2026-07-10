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

            return design;
        }
    }

    /// <summary>One frente (bay) column: its "larguero a piso" flag, optional height override, and level cells.</summary>
    public sealed class SelectiveBayDocument
    {
        public bool FloorBeam { get; set; }
        public double? HeightOverride { get; set; }
        public List<SelectiveCellDocument> Levels { get; set; } = new List<SelectiveCellDocument>();

        public static SelectiveBayDocument From(SelectiveBayDesign bay)
        {
            var document = new SelectiveBayDocument
            {
                FloorBeam = bay.FloorBeam,
                HeightOverride = bay.HeightOverride
            };

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

            foreach (var cell in Levels ?? Enumerable.Empty<SelectiveCellDocument>())
            {
                bay.Levels.Add(cell.ToDomain());
            }

            return bay;
        }
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
