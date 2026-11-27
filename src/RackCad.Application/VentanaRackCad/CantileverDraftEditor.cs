using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Formatting;
using RackCad.Application.Persistence;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.Systems.Cantilever;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>A value of the line, of its column and base, of its separator, of its brace or of its bracing, as the editor of the window writes it.</summary>
    public enum CantileverLineField
    {
        StationCount,
        Spacing,
        LevelCount,
        FaceMode,
        SingleSide,
        HeightMode,
        ManualHeight,
        FirstLevelPunch,
        ClearHeight,
        TopClearFactor,
        PlantaArms,
        PlantaBraces,
        ColumnSection,
        ColumnBottomPlate,
        BaseFollowsColumn,
        BaseSection,
        BaseLength,
        BaseFrontPlate,
        BaseRearPlate,
        BaseGusset,
        PunchDiameter,
        PunchHorizontalEndOffset,
        ConnectionPitch,
        ConnectionPunchesAboveBase,
        RearPlateVerticalEndOffset,
        RegularColumnPitch,
        ColumnBottomPlatePitch,
        SeparatorSection,
        BraceKind,
        BraceDiameter,
        BraceSection,
        PanelCountMode,
        ManualPanelCount,
        PanelHeight,
        CentralGapHeight,
    }

    /// <summary>A value of an arm: the default arm of the line, or the arm in force in the cells of a selection.</summary>
    public enum CantileverArmField
    {
        Section,
        Arrangement,
        CutLength,
        Slope,
        PlateThickness,
        VerticalPunches,
        VerticalMargin,
        EndPlate,
        EndPlateThickness,
        ExtraStopHeight,
    }

    /// <summary>What a field shows for the selection: its text, and whether the cells of the selection have different values («Varía»).</summary>
    public sealed record CantileverFieldText(string Text, bool Varies);

    /// <summary>
    /// One thing the line lacks before it resolves («Para que la línea se resuelva», CT-05): what is missing, and where it is written (the tab
    /// <see cref="CantileverDraftEditor.TabLine"/>… and, in «Componentes», the panel <see cref="CantileverDraftEditor.ComponentColumn"/>…).
    /// </summary>
    public sealed record CantileverMissing(string Message, string Tab, string Component);

    /// <summary>One box of «Restaurar layout…» (CT-07): its key, its label, how many values it restores and what is lost.</summary>
    public sealed record CantileverRestoreBox(string Key, string Label, int Count, IReadOnlyList<string> Losses);

    /// <summary>One row of the advanced sequence as the table writes it: the elevations as text, and whether the panel is braced.</summary>
    /// <remarks>
    /// Correction 1 of the review: a row read from the design keeps its exact elevations (<paramref name="FromValue"/>,
    /// <paramref name="ToValue"/>), and «Aplicar secuencia» uses them while the text is still the one shown for them (<c>Keep</c> of the
    /// classic editor). Otherwise a sequence of the rule whose top is 106.666… in would be written as 106.6667 and pass the top of the column.
    /// </remarks>
    public sealed record CantileverSegmentRow(string From, string To, bool Braced, double? FromValue = null, double? ToValue = null);

    /// <summary>
    /// The copies «Actualizar», «Insertar vista…» and «Insertar en el dibujo…» hand to AutoCAD: a deep copy of the design and the line resolved
    /// from that copy (never the live design of the draft, D-05 of Dinámico), the name (trimmed, as the classic editor sends it) and the JSON
    /// of the copy, the base of «Limpio» once it is written.
    /// </summary>
    public sealed record CantileverDraftSnapshot(CantileverLineDesign Design, CantileverLineAssembly Line, string Name, string DesignJson);

    /// <summary>
    /// T5 ct (I-64, fifth batch; prototype «cantilever», CT-01 to CT-12, accepted by the Owner on 2026-11-26): the draft of ONE Cantilever line of
    /// the RackCad window. It edits a copy of a <see cref="CantileverLineDesign"/> and recomputes it with THE recompute authority of the classic
    /// editor (<see cref="CantileverLineEditorAssembler"/>), so the line, its BOM and its views are the ones the classic editor would give. No
    /// calculation is changed here.
    ///
    /// One way to confirm: every write is checked by its field (a number, its minimum) and, when it is refused, the draft does not change.
    /// A line that does not resolve yet (a new line has no section and no cut, as the classic one: «the line opens BLOCKED») is a valid draft:
    /// <see cref="Missing"/> says what it lacks and where, and nothing is written in the drawing until it resolves. The arms of the cells are
    /// written through the matrix of the line (<see cref="CantileverLineArmMatrix"/>), which does not store an exception equal to the default
    /// arm; «Todo» writes the exception in every cell (CT-06). The draft is «Modificado» while its JSON differs from its base (what was read
    /// or last written; the defaults for a new line).
    /// </summary>
    public sealed class CantileverDraftEditor
    {
        public const string TabLine = "linea";
        public const string TabArms = "brazos";
        public const string TabComponents = "componentes";
        public const string TabBracing = "arriostramiento";

        public const string ComponentColumn = "col";
        public const string ComponentArm = "arm";
        public const string ComponentSeparator = "sep";
        public const string ComponentBrace = "tens";

        /// <summary>In «Línea», the group «Niveles» (the clear between levels, the first punch, the factor).</summary>
        public const string ComponentLevels = "niveles";

        public const string RestoreExceptions = "excepciones";
        public const string RestoreManualHeight = "altura";
        public const string RestoreManualCount = "paneles";
        public const string RestoreAdvanced = "secuencia";

        private readonly CantileverLineEditorAssembler _assembler;
        private readonly ICantileverColumnBaseSectionPolicy _columnBasePolicy;
        private CantileverLineDesign _design;
        private CantileverLineEditorComputation _computation;
        private string _baselineJson;

        // The base as a design too: the store refuses to read a line without its sections (RackDesignValidation.IsUsableCantilever), and a
        // new line has none, so «Descartar» goes back to this copy and never reads the JSON.
        private CantileverLineDesign _baseline;
        private string _name;
        private string _baselineName;

        public CantileverDraftEditor(StructuralSectionCatalog catalogue)
        {
            if (catalogue == null) throw new ArgumentNullException(nameof(catalogue));
            _assembler = new CantileverLineEditorAssembler(catalogue);
            _columnBasePolicy = CantileverCataloguePolicies.ColumnBase(catalogue);
            Load(new CantileverLineDesign(), null);
        }

        /// <summary>A new line with the defaults of the classic editor (<c>LoadNew</c>): no section and no cut, so it does not resolve yet.</summary>
        public static CantileverDraftEditor CreateNew(StructuralSectionCatalog catalogue) => new CantileverDraftEditor(catalogue);

        public StructuralSectionCatalog Catalogue => _assembler.Catalogue;

        public StructuralSectionGeometryFactory GeometryFactory => _assembler.GeometryFactory;

        /// <summary>The design of the draft. Read it; it is changed only through the writes of this editor.</summary>
        public CantileverLineDesign Design => _design;

        /// <summary>The last recomputation of the draft (it may be blocked: then <see cref="Missing"/> says why).</summary>
        public CantileverLineEditorComputation Computation => _computation;

        /// <summary>The line of the draft resolves: it can be written in the drawing.</summary>
        public bool IsResolved => _computation != null && _computation.IsValid;

        /// <summary>
        /// The name of the line as RACKEDITAR shows and writes it (correction 4 of the review): the one of the envelope
        /// (<c>LoadExisting(…, embed.Name, …)</c>), the identity of the session, not the inner name of the design. Renaming writes both, as
        /// the classic editor does (<c>Name_Changed</c>); a design with no name is not given one otherwise.
        /// </summary>
        public string Name => _name;

        /// <summary>The JSON of the draft, as the inner design of the block (a <see cref="RackProject"/> of Cantilever).</summary>
        public string DesignJson => Serialize(_design);

        /// <summary>The base of «Modificado»: what was read from the drawing or last written; the defaults (or the library) for a new line.</summary>
        public string BaselineJson => _baselineJson;

        public bool IsDirty => !string.Equals(DesignJson, _baselineJson, StringComparison.Ordinal) || !string.Equals(_name, _baselineName, StringComparison.Ordinal);

        /// <summary>A copy of the base of «Modificado» (to mark the cells whose arm changed, and for «Duplicar»).</summary>
        public CantileverLineDesign BaselineDesign => _baseline.DeepCopy();

        /// <summary>What the last write said that is not an error (a base the column could not follow), or null.</summary>
        public string Notice { get; private set; }

        /// <summary>
        /// Loads a design (a copy: the caller keeps its own) as the base of «Modificado», with the name of the line (the one of the envelope for a
        /// line of the drawing, of the entry for a design of the library). The inner name of the design is not touched.
        /// </summary>
        public void Load(CantileverLineDesign design, string name)
        {
            _design = (design ?? new CantileverLineDesign()).DeepCopy();
            _name = Trimmed(name);
            _computation = Build(_design);
            _baselineJson = DesignJson;
            _baseline = _design.DeepCopy();
            _baselineName = _name;
            Notice = null;
        }

        /// <summary>«Duplicar» of a draft of «Nuevo»: the copied design taken as an edit over the base this draft was opened with.</summary>
        public bool TakeAsEdit(CantileverLineDesign design)
        {
            if (design == null) return false;
            var copy = design.DeepCopy();
            copy.Id = _design.Id;
            var computation = Build(copy);
            if (computation == null) return false;
            _design = copy;
            _computation = computation;
            return true;
        }

        /// <summary>«Descartar»: back to the base.</summary>
        public void Discard()
        {
            _design = _baseline.DeepCopy();
            _name = _baselineName;
            _computation = Build(_design);
            Notice = null;
        }

        /// <summary>AutoCAD wrote the draft: what was SENT is the base of «Limpio» now.</summary>
        public void MarkWritten(string sentJson)
        {
            if (string.IsNullOrEmpty(sentJson)) return;
            _baselineJson = sentJson;
            CantileverLineDesign sent = null;
            try
            {
                sent = new RackProjectStore().Deserialize(sentJson)?.CantileverLineDesign;
            }
            catch (InvalidOperationException)
            {
                // What was sent was a resolved line, so the store reads it; if not, the base is the draft as it is (it cannot be edited while written).
            }

            _baseline = (sent ?? _design).DeepCopy();
            _baselineName = _name;
        }

        /// <summary>
        /// The copies to write (see <see cref="CantileverDraftSnapshot"/>): the design is copied and resolved again from the copy, so nothing
        /// edited afterwards reaches what AutoCAD writes. Null when the line does not resolve.
        /// </summary>
        public CantileverDraftSnapshot SnapshotForWrite()
        {
            if (!IsResolved) return null;
            var computation = Build(_design.DeepCopy());
            if (computation == null || !computation.IsValid) return null;
            return new CantileverDraftSnapshot(computation.Design, computation.Line, Name, Serialize(computation.Design));
        }

        /// <summary>The BOM of the draft (the BOM of the classic editor, «Lista de materiales»), on a copy; null when the line does not resolve.</summary>
        public BillOfMaterials BomCopy()
        {
            var snapshot = SnapshotForWrite();
            return snapshot == null ? null : CantileverLineBomBuilder.Build(snapshot.Line);
        }

        // ------------------------------------------------------------------ the name

        /// <summary>Renames the line: the name of the line and the inner name of the design, as <c>Name_Changed</c> of the classic editor.</summary>
        public string SetName(string name)
        {
            var next = Trimmed(name);
            var refused = Edit(d =>
            {
                d.Name = next;
                return null;
            });
            if (refused == null) _name = next;
            return refused;
        }

        private static string Trimmed(string name) => string.IsNullOrWhiteSpace(name) ? null : name.Trim();

        // ------------------------------------------------------------------ the values of the line, of its pieces and of its bracing

        /// <summary>What a field of the line shows now, in the units of the editor (in, the first punch counted from 1).</summary>
        public string TextOf(CantileverLineField field)
        {
            var d = _design;
            var t = d.StationTopology ?? new CantileverLineStationTopologyDesign();
            var cb = t.ColumnBaseTemplate ?? new CantileverStationColumnBaseTemplateDesign();
            var p = cb.Connection?.Punches ?? new CantileverPunchParameters();
            var b = d.Bracing ?? new CantileverBracingDesign();
            switch (field)
            {
                case CantileverLineField.StationCount: return Int(d.StationCount);
                case CantileverLineField.Spacing: return Num(d.ColumnCentreSpacing);
                case CantileverLineField.LevelCount: return Int(t.LevelCount);
                case CantileverLineField.FaceMode: return t.FaceMode.ToString();
                case CantileverLineField.SingleSide: return t.SingleSide.ToString();
                case CantileverLineField.HeightMode: return (t.ColumnHeight?.Mode ?? CantileverStationColumnHeightMode.Automatic).ToString();
                case CantileverLineField.ManualHeight: return t.ColumnHeight?.ManualHeight is double h ? Num(h) : string.Empty;
                case CantileverLineField.FirstLevelPunch: return Int(t.FirstLevelPunchIndex + 1);
                case CantileverLineField.ClearHeight: return Num(t.RequestedClearHeight);
                case CantileverLineField.TopClearFactor: return Num(t.TopClearFactor);
                case CantileverLineField.PlantaArms: return Bool(d.PlantaVisibility?.ShowArms ?? false);
                case CantileverLineField.PlantaBraces: return Bool(d.PlantaVisibility?.ShowBraces ?? false);
                case CantileverLineField.ColumnSection: return cb.ColumnSectionId ?? string.Empty;
                case CantileverLineField.ColumnBottomPlate: return Num(cb.ColumnBottomPlate?.Thickness ?? 0.0);
                case CantileverLineField.BaseFollowsColumn: return Bool(cb.BaseFollowsColumn);
                case CantileverLineField.BaseSection: return cb.Base?.SectionId ?? string.Empty;
                case CantileverLineField.BaseLength: return cb.Base == null || !(cb.Base.Length > 0) ? string.Empty : Num(cb.Base.Length);
                case CantileverLineField.BaseFrontPlate: return Num(cb.Base?.FrontPlate?.Thickness ?? 0.0);
                case CantileverLineField.BaseRearPlate: return Num(cb.Base?.RearPlate?.Thickness ?? 0.0);
                case CantileverLineField.BaseGusset: return Num(cb.Base?.Gusset?.Thickness ?? 0.0);
                case CantileverLineField.PunchDiameter: return Num(p.Diameter);
                case CantileverLineField.PunchHorizontalEndOffset: return Num(p.HorizontalEndOffset);
                case CantileverLineField.ConnectionPitch: return Num(p.ConnectionPitch);
                case CantileverLineField.ConnectionPunchesAboveBase: return Int(p.ConnectionPunchesAboveBase);
                case CantileverLineField.RearPlateVerticalEndOffset: return Num(p.RearPlateVerticalEndOffset);
                case CantileverLineField.RegularColumnPitch: return Num(p.RegularColumnPitch);
                case CantileverLineField.ColumnBottomPlatePitch: return Num(p.ColumnBottomPlatePitch);
                case CantileverLineField.SeparatorSection: return b.SeparatorSectionId ?? string.Empty;
                case CantileverLineField.BraceKind: return b.BraceKind.ToString();
                case CantileverLineField.BraceDiameter: return Num(b.ColdRolled?.Diameter ?? 0.0);
                case CantileverLineField.BraceSection: return b.BraceSectionId ?? string.Empty;
                case CantileverLineField.PanelCountMode: return b.PanelCountMode.ToString();
                case CantileverLineField.ManualPanelCount: return b.ManualPanelCount is int n ? Int(n) : string.Empty;
                case CantileverLineField.PanelHeight: return Num(b.BracedPanelHeight);
                case CantileverLineField.CentralGapHeight: return Num(b.CentralEmptySpaceHeight);
                default: throw new ArgumentOutOfRangeException(nameof(field), field, "El campo no tiene regla.");
            }
        }

        /// <summary>
        /// Writes a field of the line (the text the user wrote, or the token of a choice: the name of the enum value, «True»/«False»). The
        /// value is checked by its field and refused with the reason (the draft does not change); null when it was written.
        /// </summary>
        public string Write(CantileverLineField field, string text)
        {
            text = (text ?? string.Empty).Replace('−', '-').Trim();
            switch (field)
            {
                case CantileverLineField.StationCount:
                    return Integer(text, 2, "Mínimo dos estaciones: con una no hay intervalo, y sin intervalo no hay arriostramiento.", (d, v) => d.StationCount = v);
                case CantileverLineField.Spacing:
                    return Positive(text, "La separación entre centros", (d, v) => d.ColumnCentreSpacing = v);
                case CantileverLineField.LevelCount:
                    return Integer(text, 1, "Escribe al menos un nivel.", (d, v) => Topology(d).LevelCount = v);
                case CantileverLineField.FaceMode:
                    return Choice<CantileverStationFaceMode>(text, (d, v) => Topology(d).FaceMode = v);
                case CantileverLineField.SingleSide:
                    if (Topology(_design).FaceMode == CantileverStationFaceMode.Double) return "Con doble cara se usan los dos lados.";
                    return Choice<CantileverArmSide>(text, (d, v) => Topology(d).SingleSide = v);
                case CantileverLineField.HeightMode:
                    return Choice<CantileverStationColumnHeightMode>(text, (d, v) => Height(d).Mode = v);
                case CantileverLineField.ManualHeight:
                    if ((Topology(_design).ColumnHeight?.Mode ?? CantileverStationColumnHeightMode.Automatic) != CantileverStationColumnHeightMode.Manual)
                        return "Solo con altura manual; la que había se conserva.";
                    if (text.Length == 0) return Edit(d => { Height(d).ManualHeight = null; return null; });
                    return Positive(text, "La altura manual", (d, v) => Height(d).ManualHeight = v);
                case CantileverLineField.FirstLevelPunch:
                    return Integer(text, 1, "El troquel del primer nivel se cuenta desde 1.", (d, v) => Topology(d).FirstLevelPunchIndex = v - 1);
                case CantileverLineField.ClearHeight:
                    return NonNegative(text, "El claro entre niveles", (d, v) => Topology(d).RequestedClearHeight = v);
                case CantileverLineField.TopClearFactor:
                    if (!TryNumber(text, out var factor)) return "El factor de claro superior debe ser un número, de 1/3 (0.3333) o más.";
                    // Correction 2 of the review: what the field shows for 1/3 («0.3333») is 1/3, stored exact.
                    if (string.Equals(Num(factor), Num(CantileverStationDefaults.TopClearFactor), StringComparison.Ordinal)) factor = CantileverStationDefaults.TopClearFactor;
                    if (factor < CantileverStationDefaults.TopClearFactor - 1e-9) return "El factor de claro superior no puede ser menor que 1/3 (0.3333).";
                    return Edit(d => { Topology(d).TopClearFactor = factor; return null; });
                case CantileverLineField.PlantaArms:
                    return Flag(text, (d, v) => Planta(d).ShowArms = v);
                case CantileverLineField.PlantaBraces:
                    return Flag(text, (d, v) => Planta(d).ShowBraces = v);
                case CantileverLineField.ColumnSection:
                    return SectionOf(text, ColumnFamilies, "La columna", out var column) ?? Edit(d =>
                    {
                        var state = new CantileverColumnBaseEditorState(ColumnBase(d), _columnBasePolicy);
                        state.SelectColumn(column);
                        Topology(d).ColumnBaseTemplate = state.Accept();
                        Notice = state.Diagnostics.FirstOrDefault()?.Message;
                        return null;
                    });
                case CantileverLineField.ColumnBottomPlate:
                    return Positive(text, "El espesor de la placa inferior", (d, v) => (ColumnBase(d).ColumnBottomPlate ??= new CantileverPlateDesign()).Thickness = v);
                case CantileverLineField.BaseFollowsColumn:
                    if (!TryFlag(text, out var follows)) return "Elige sí o no.";
                    return Edit(d =>
                    {
                        if (!follows)
                        {
                            ColumnBase(d).BaseFollowsColumn = false;
                            return null;
                        }

                        var state = new CantileverColumnBaseEditorState(ColumnBase(d), _columnBasePolicy);
                        state.UseSameSectionAsColumn();
                        if (state.Diagnostics.Count > 0) return state.Diagnostics[0].Message;
                        Topology(d).ColumnBaseTemplate = state.Accept();
                        return null;
                    });
                case CantileverLineField.BaseSection:
                    return SectionOf(text, ColumnFamilies, "La base", out var baseSection) ?? Edit(d =>
                    {
                        var state = new CantileverColumnBaseEditorState(ColumnBase(d), _columnBasePolicy);
                        state.SelectBase(baseSection);
                        Topology(d).ColumnBaseTemplate = state.Accept();
                        return null;
                    });
                case CantileverLineField.BaseLength:
                    return Positive(text, "La longitud de base", (d, v) => Base(d).Length = v);
                case CantileverLineField.BaseFrontPlate:
                    return Positive(text, "El espesor de la placa frontal", (d, v) => (Base(d).FrontPlate ??= new CantileverPlateDesign()).Thickness = v);
                case CantileverLineField.BaseRearPlate:
                    return Positive(text, "El espesor de la placa posterior", (d, v) => (Base(d).RearPlate ??= new CantileverPlateDesign()).Thickness = v);
                case CantileverLineField.BaseGusset:
                    return Positive(text, "El espesor del cartabón", (d, v) => (Base(d).Gusset ??= new CantileverGussetDesign()).Thickness = v);
                case CantileverLineField.PunchDiameter:
                    return Positive(text, "El diámetro del troquel", (d, v) => Punches(d).Diameter = v);
                case CantileverLineField.PunchHorizontalEndOffset:
                    return NonNegative(text, "El margen horizontal a extremo", (d, v) => Punches(d).HorizontalEndOffset = v);
                case CantileverLineField.ConnectionPitch:
                    return Positive(text, "El paso de conexión", (d, v) => Punches(d).ConnectionPitch = v);
                case CantileverLineField.ConnectionPunchesAboveBase:
                    return Integer(text, 0, "Escribe un número entero de troqueles, de 0 o más.", (d, v) => Punches(d).ConnectionPunchesAboveBase = v);
                case CantileverLineField.RearPlateVerticalEndOffset:
                    return NonNegative(text, "El margen vertical de la placa posterior", (d, v) => Punches(d).RearPlateVerticalEndOffset = v);
                case CantileverLineField.RegularColumnPitch:
                    return Positive(text, "El paso regular de columna", (d, v) => Punches(d).RegularColumnPitch = v);
                case CantileverLineField.ColumnBottomPlatePitch:
                    return Positive(text, "El paso de la placa inferior", (d, v) => Punches(d).ColumnBottomPlatePitch = v);
                case CantileverLineField.SeparatorSection:
                    return SectionOf(text, SeparatorFamilies, "El separador", out var separator)
                           ?? Edit(d => { Bracing(d).SeparatorSectionId = separator; return null; });
                case CantileverLineField.BraceKind:
                    return Choice<CantileverBraceBodyKind>(text, (d, v) => Bracing(d).BraceKind = v);
                case CantileverLineField.BraceDiameter:
                    if (Bracing(_design).BraceKind != CantileverBraceBodyKind.ColdRolledRound) return "Solo con tensor de varilla (cold rolled).";
                    return Positive(text, "El diámetro de la varilla", (d, v) => (Bracing(d).ColdRolled ??= new CantileverColdRolledBraceDesign()).Diameter = v);
                case CantileverLineField.BraceSection:
                    if (Bracing(_design).BraceKind != CantileverBraceBodyKind.StructuralSection) return "Solo con tensor estructural (perfil).";
                    return SectionOf(text, BraceFamilies, "El tensor", out var brace) ?? Edit(d => { Bracing(d).BraceSectionId = brace; return null; });
                case CantileverLineField.PanelCountMode:
                    return Choice<CantileverBracedPanelCountMode>(text, (d, v) => Bracing(d).PanelCountMode = v);
                case CantileverLineField.ManualPanelCount:
                    if (Bracing(_design).PanelCountMode != CantileverBracedPanelCountMode.Manual) return "Solo con cantidad manual; la que había se conserva.";
                    if (text.Length == 0) return Edit(d => { Bracing(d).ManualPanelCount = null; return null; });
                    return Integer(text, 1, "Escribe al menos un panel.", (d, v) => Bracing(d).ManualPanelCount = v);
                case CantileverLineField.PanelHeight:
                    return Positive(text, "La altura de panel", (d, v) => Bracing(d).BracedPanelHeight = v);
                case CantileverLineField.CentralGapHeight:
                    return Positive(text, "La altura de espacio central", (d, v) => Bracing(d).CentralEmptySpaceHeight = v);
                default:
                    throw new ArgumentOutOfRangeException(nameof(field), field, "El campo no tiene regla.");
            }
        }

        // ------------------------------------------------------------------ the arms: the matrix of the line and the default arm

        /// <summary>The matrix of the draft (station × level × side), to read its cells. It writes through this editor only.</summary>
        public CantileverLineArmMatrix Matrix => new CantileverLineArmMatrix(_design);

        /// <summary>What a field of an arm shows: of the default arm (no cells), or of the arms in force in the cells, «Varía» when they differ.</summary>
        public CantileverFieldText ArmText(IReadOnlyCollection<CantileverLineCell> cells, CantileverArmField field)
        {
            if (cells == null) return new CantileverFieldText(ArmValue(_design.DefaultArmTemplate ?? new CantileverArmTemplateDesign(), field), false);
            var matrix = Matrix;
            var values = cells.Where(matrix.IsActive).Select(c => ArmValue(matrix.Effective(c), field)).Distinct(StringComparer.Ordinal).ToList();
            if (values.Count == 0) return new CantileverFieldText(string.Empty, false);
            return values.Count == 1 ? new CantileverFieldText(values[0], false) : new CantileverFieldText(string.Empty, true);
        }

        /// <summary>
        /// Writes a field of the arm in force in every cell of <paramref name="cells"/>, as an exception of each cell (one copy per cell; one
        /// equal to the default arm is not stored, the rule of the matrix). With no cells, it writes the default arm of the line, which does
        /// not touch the exceptions (CT-06). Refused with the reason, and the draft unchanged, when one cell refuses it.
        /// </summary>
        public string WriteArm(IReadOnlyCollection<CantileverLineCell> cells, CantileverArmField field, string text)
        {
            text = (text ?? string.Empty).Replace('−', '-').Trim();
            if (cells == null)
                return Edit(d => ApplyArm(d.DefaultArmTemplate ??= new CantileverArmTemplateDesign(), field, text));

            if (cells.Count == 0) return "Elige al menos una celda.";
            return Edit(d =>
            {
                var matrix = new CantileverLineArmMatrix(d);
                foreach (var cell in cells)
                {
                    if (!matrix.IsActive(cell)) continue;
                    var arm = matrix.Effective(cell).DeepCopy();
                    var refused = ApplyArm(arm, field, text);
                    if (refused != null) return refused;
                    matrix.Apply(CantileverLineApplyScope.Cell, cell, arm);
                }

                return null;
            });
        }

        /// <summary>«Volver al brazo por omisión»: the cells of the selection lose their exception.</summary>
        public string RestoreCells(IReadOnlyCollection<CantileverLineCell> cells)
        {
            if (cells == null || cells.Count == 0) return "Elige al menos una celda.";
            return Edit(d =>
            {
                var matrix = new CantileverLineArmMatrix(d);
                foreach (var cell in cells) matrix.Restore(CantileverLineApplyScope.Cell, cell);
                return null;
            });
        }

        // ------------------------------------------------------------------ the advanced sequence of the bracing

        /// <summary>The sequence the rule gives now (of the first interval of the resolved line), or empty when the line does not resolve.</summary>
        public IReadOnlyList<CantileverPanelSegmentDesign> AutomaticSegments =>
            _computation?.Line?.Intervals?.FirstOrDefault()?.Layout?.EffectiveSegments
            ?? (IReadOnlyList<CantileverPanelSegmentDesign>)Array.Empty<CantileverPanelSegmentDesign>();

        public bool IsAdvanced => Bracing(_design).PanelLayoutMode == CantileverPanelLayoutMode.Advanced;

        /// <summary>The tramos the design keeps (written in «Avanzada», also when the rule governs again).</summary>
        public IReadOnlyList<CantileverPanelSegmentDesign> SavedSegments => Bracing(_design).AdvancedPanelSegments ?? new List<CantileverPanelSegmentDesign>();

        /// <summary>Back to «Automática»: the rule governs, and the tramos are kept for a return to «Avanzada» (the classic rule).</summary>
        public string ToAutomatic() => !IsAdvanced ? "La secuencia ya la gobierna la regla estándar." : Edit(d =>
        {
            var state = new CantileverPanelLayoutEditorState(Bracing(d).PanelLayoutMode, Bracing(d).AdvancedPanelSegments);
            state.RestoreAutomatic();
            state.ApplyTo(Bracing(d));
            return null;
        });

        /// <summary>
        /// To «Avanzada»: with <paramref name="recover"/> and tramos kept, those tramos govern again («Recuperar mis tramos», dolor 17); otherwise
        /// the sequence the rule draws now is materialized («Empezar desde la regla»), as the classic editor does.
        /// </summary>
        public string ToAdvanced(bool recover)
        {
            if (IsAdvanced) return "La secuencia ya es avanzada.";
            if (recover)
            {
                if (SavedSegments.Count == 0) return "No hay tramos guardados que recuperar.";
                return Edit(d =>
                {
                    var state = new CantileverPanelLayoutEditorState(CantileverPanelLayoutMode.Advanced, Bracing(d).AdvancedPanelSegments);
                    state.ApplyTo(Bracing(d));
                    return null;
                });
            }

            var automatic = AutomaticSegments;
            return Edit(d =>
            {
                var state = new CantileverPanelLayoutEditorState(Bracing(d).PanelLayoutMode, Bracing(d).AdvancedPanelSegments);
                var result = state.MaterializeAutomatic(automatic);
                if (!result.Applied) return result.Reason;
                state.ApplyTo(Bracing(d));
                return null;
            });
        }

        /// <summary>
        /// «Aplicar secuencia» (CT-10): the rows of the table become the tramos of the advanced sequence. Each row is checked (two numbers,
        /// from 0, the top above the bottom); whether the sequence is a valid bracing is decided by the resolver when the line recomputes, as in
        /// the classic editor, and <see cref="Missing"/> says it.
        /// </summary>
        public string ApplySegments(IReadOnlyList<CantileverSegmentRow> rows)
        {
            if (!IsAdvanced) return "Pasa antes a «Avanzada».";
            if (rows == null || rows.Count == 0) return "Es el único tramo. Una secuencia sin tramos no es un arriostramiento.";
            var segments = new List<CantileverPanelSegmentDesign>(rows.Count);
            for (var i = 0; i < rows.Count; i++)
            {
                if (!Keep(rows[i].From, rows[i].FromValue, out var from) || !Keep(rows[i].To, rows[i].ToValue, out var to) || from < 0.0)
                    return "Tramo " + Int(i + 1) + ": escribe «Desde» y «Hasta» en pulgadas, de 0 o más.";
                if (!(to > from)) return "Tramo " + Int(i + 1) + ": «Hasta» tiene que estar por encima de «Desde».";
                segments.Add(new CantileverPanelSegmentDesign
                {
                    StartElevation = from,
                    EndElevation = to,
                    BracingMode = rows[i].Braced ? CantileverPanelBracingMode.CrossBraced : CantileverPanelBracingMode.None,
                });
            }

            return Edit(d =>
            {
                Bracing(d).PanelLayoutMode = CantileverPanelLayoutMode.Advanced;
                Bracing(d).AdvancedPanelSegments = segments;
                return null;
            });
        }

        /// <summary>The tramos of the design as rows of the table.</summary>
        public static IReadOnlyList<CantileverSegmentRow> RowsOf(IEnumerable<CantileverPanelSegmentDesign> segments) =>
            (segments ?? Enumerable.Empty<CantileverPanelSegmentDesign>()).Where(s => s != null)
            .Select(s => new CantileverSegmentRow(Num(s.StartElevation), Num(s.EndElevation), s.BracingMode != CantileverPanelBracingMode.None,
                s.StartElevation, s.EndElevation)).ToList();

        /// <summary>
        /// <c>Keep</c> of the classic editor: the exact value a row was read with, while its text is still the one shown for it; otherwise the
        /// number the text says.
        /// </summary>
        public static bool Keep(string text, double? original, out double value)
        {
            if (original.HasValue && string.Equals((text ?? string.Empty).Trim(), Num(original.Value), StringComparison.Ordinal))
            {
                value = original.Value;
                return true;
            }

            return TryNumber(text, out value);
        }

        // ------------------------------------------------------------------ «Restaurar layout…» (CT-07)

        /// <summary>The boxes of «Restaurar layout…», each with what it restores; a box with nothing to restore has a count of 0.</summary>
        public IReadOnlyList<CantileverRestoreBox> RestoreBoxes()
        {
            var boxes = new List<CantileverRestoreBox>();
            var matrix = Matrix;
            var exceptions = matrix.Cells.Where(matrix.HasOverride).ToList();
            boxes.Add(new CantileverRestoreBox(RestoreExceptions, "Excepciones de brazo", exceptions.Count,
                exceptions.Select(c => CellTitle(c) + ": vuelve al brazo por omisión (" + ArmSummary(matrix.Effective(c)) + ").").ToList()));

            var height = Topology(_design).ColumnHeight;
            var manualHeight = height?.Mode == CantileverStationColumnHeightMode.Manual;
            boxes.Add(new CantileverRestoreBox(RestoreManualHeight, "Altura manual de columna", manualHeight ? 1 : 0, manualHeight
                ? new[] { "Altura de columna manual" + (height.ManualHeight is double h ? " de " + Num(h) + " in" : string.Empty) + ": vuelve a «Automática»; el valor se conserva." }
                : Array.Empty<string>()));

            var bracing = Bracing(_design);
            var manualCount = bracing.PanelCountMode == CantileverBracedPanelCountMode.Manual;
            boxes.Add(new CantileverRestoreBox(RestoreManualCount, "Cantidad manual de paneles", manualCount ? 1 : 0, manualCount
                ? new[] { "Cantidad manual" + (bracing.ManualPanelCount is int n ? " de " + Int(n) + " paneles" : string.Empty) + ": vuelve a «Automática»; el valor se conserva." }
                : Array.Empty<string>()));

            var advanced = bracing.PanelLayoutMode == CantileverPanelLayoutMode.Advanced;
            var count = (bracing.AdvancedPanelSegments ?? new List<CantileverPanelSegmentDesign>()).Count;
            boxes.Add(new CantileverRestoreBox(RestoreAdvanced, "Secuencia avanzada", advanced ? 1 : 0, advanced
                ? new[] { "Secuencia avanzada de " + Count(count, "tramo", "tramos") + ": vuelve a la regla; los tramos se conservan sin mandar, por si vuelves a «Avanzada»." }
                : Array.Empty<string>()));
            return boxes;
        }

        /// <summary>«Restaurar lo marcado»: each box marked goes back to the rule (the values are kept as dormant data, as the classic editor keeps them).</summary>
        public string Restore(IReadOnlyCollection<string> keys)
        {
            if (keys == null || keys.Count == 0) return "Marca al menos una casilla.";
            return Edit(d =>
            {
                if (keys.Contains(RestoreExceptions)) d.ArmCellOverrides = new List<CantileverArmCellOverride>();
                if (keys.Contains(RestoreManualHeight)) Height(d).Mode = CantileverStationColumnHeightMode.Automatic;
                if (keys.Contains(RestoreManualCount)) Bracing(d).PanelCountMode = CantileverBracedPanelCountMode.Automatic;
                if (keys.Contains(RestoreAdvanced)) Bracing(d).PanelLayoutMode = CantileverPanelLayoutMode.Automatic;
                return null;
            });
        }

        // ------------------------------------------------------------------ what the line lacks (CT-05)

        /// <summary>
        /// «Para que la línea se resuelva»: what the draft lacks, each with the tab (and the panel of «Componentes») where it is written. The pieces
        /// with no approved default come first, as the prototype names them; when none is missing and the line still does not resolve, the
        /// blocking diagnostics of the resolver say why. Empty when the line resolves.
        /// </summary>
        public IReadOnlyList<CantileverMissing> Missing
        {
            get
            {
                var missing = new List<CantileverMissing>();
                if (IsResolved) return missing;
                var t = Topology(_design);
                var cb = t.ColumnBaseTemplate ?? new CantileverStationColumnBaseTemplateDesign();
                var arm = _design.DefaultArmTemplate ?? new CantileverArmTemplateDesign();
                if (string.IsNullOrWhiteSpace(cb.ColumnSectionId)) missing.Add(new CantileverMissing("Falta la sección de columna.", TabComponents, ComponentColumn));
                if (string.IsNullOrWhiteSpace(cb.Base?.SectionId)) missing.Add(new CantileverMissing("Falta la sección de base.", TabComponents, ComponentColumn));
                if (cb.Base == null || !(cb.Base.Length > 0)) missing.Add(new CantileverMissing("Falta la longitud de base: no hay valor por omisión aprobado.", TabComponents, ComponentColumn));
                if (string.IsNullOrWhiteSpace(arm.Body?.SectionId)) missing.Add(new CantileverMissing("Falta la sección del brazo por omisión.", TabComponents, ComponentArm));
                if (arm.Body == null || !(arm.Body.CutLength > 0)) missing.Add(new CantileverMissing("Falta la longitud de corte del brazo por omisión: no hay valor por omisión aprobado.", TabComponents, ComponentArm));
                // The classic editor opens a new line with a clear of 0, which the station refuses («debe ser un numero positivo»).
                if (!(t.RequestedClearHeight > 0)) missing.Add(new CantileverMissing("Falta el claro entre niveles: escribe uno mayor que cero.", TabLine, ComponentLevels));
                var matrix = Matrix;
                foreach (var cell in matrix.Cells.Where(matrix.HasOverride))
                {
                    var own = matrix.Effective(cell);
                    if (string.IsNullOrWhiteSpace(own.Body?.SectionId)) missing.Add(new CantileverMissing(CellTitle(cell) + ": a su brazo le falta la sección.", TabArms, null));
                    else if (!(own.Body.CutLength > 0)) missing.Add(new CantileverMissing(CellTitle(cell) + ": a su brazo le falta la longitud de corte.", TabArms, null));
                }

                var bracing = Bracing(_design);
                if (bracing.BraceKind == CantileverBraceBodyKind.StructuralSection && string.IsNullOrWhiteSpace(bracing.BraceSectionId))
                    missing.Add(new CantileverMissing("Falta la sección del tensor estructural: sin valor por omisión, un tensor estructural sin sección se rechaza.", TabComponents, ComponentBrace));
                if (t.ColumnHeight?.Mode == CantileverStationColumnHeightMode.Manual && !(t.ColumnHeight.ManualHeight > 0))
                    missing.Add(new CantileverMissing("Falta la altura manual: escríbela o vuelve a «Automática».", TabLine, null));
                if (bracing.PanelCountMode == CantileverBracedPanelCountMode.Manual && !(bracing.ManualPanelCount >= 1))
                    missing.Add(new CantileverMissing("Falta la cantidad manual de paneles: escríbela o vuelve a «Automática».", TabBracing, null));
                if (missing.Count > 0) return missing;

                foreach (var diagnostic in _computation?.Diagnostics?.Where(d => d.IsBlocking) ?? Enumerable.Empty<CantileverDiagnostic>())
                {
                    var message = diagnostic.Message;
                    if (missing.Any(m => string.Equals(m.Message, message, StringComparison.Ordinal))) continue;
                    missing.Add(new CantileverMissing(message, TabOf(diagnostic.Code), null));
                }

                if (missing.Count == 0) missing.Add(new CantileverMissing("La línea no se resolvió.", TabLine, null));
                return missing;
            }
        }

        // ------------------------------------------------------------------ texts shared with the editor of the window

        /// <summary>«E3 · N3 +Y»: a cell by its station, level and side (the sides keep their names, CT-02).</summary>
        public static string CellTitle(CantileverLineCell cell) =>
            "E" + Int(cell.StationIndex + 1) + " · N" + Int(cell.LevelIndex + 1) + " " + SideLabel(cell.Side);

        public static string SideLabel(CantileverArmSide side) => side == CantileverArmSide.NegativeY ? "−Y" : "+Y";

        /// <summary>The designation of the catalogue (HSS4X4X1/4, C4X4.5) for an id, or the id when the catalogue does not have it (dolor 6).</summary>
        public string Designation(string sectionId)
        {
            if (string.IsNullOrWhiteSpace(sectionId)) return null;
            return Catalogue.TryGetById(sectionId, out var section) && !string.IsNullOrWhiteSpace(section.DisplayName)
                ? section.DisplayName
                : CantileverColumnBaseEditorState.Designation(sectionId);
        }

        /// <summary>«HSS4X4X1/4 · 36 in · pendiente 0.4375/12 · 2 troqueles»: an arm in one line.</summary>
        public string ArmSummary(CantileverArmTemplateDesign arm)
        {
            if (arm == null) return "sin brazo";
            var cut = arm.Body?.CutLength > 0 ? Num(arm.Body.CutLength) + " in" : "sin longitud";
            return (Designation(arm.Body?.SectionId) ?? "sin sección") + " · " + cut + " · pendiente " + Num(arm.Body?.SlopeRisePer12 ?? 0.0) + "/12 · "
                   + Count(arm.MountingPlate?.VerticalPunchCount ?? 0, "troquel", "troqueles");
        }

        public static string Num(double value) => value.ToString("0.####", CultureInfo.InvariantCulture);

        public static string Int(int value) => value.ToString(CultureInfo.InvariantCulture);

        public static string Count(int count, string one, string many) => Int(count) + " " + (count == 1 ? one : many);

        /// <summary>The families each piece admits (CT-04): W for the column and the base, C for the separator, C or L for a structural brace.</summary>
        public IReadOnlyList<StructuralSectionFamily> ColumnFamilies => _columnBasePolicy.AllowedFamilies;

        public static IReadOnlyList<StructuralSectionFamily> SeparatorFamilies { get; } = new[] { StructuralSectionFamily.Channel };

        public static IReadOnlyList<StructuralSectionFamily> BraceFamilies { get; } = new[] { StructuralSectionFamily.Channel, StructuralSectionFamily.Angle };

        /// <summary>Every family for the arm (a paired arm admits only channels, which the write checks).</summary>
        public static IReadOnlyList<StructuralSectionFamily> ArmFamilies { get; } = Array.Empty<StructuralSectionFamily>();

        // ------------------------------------------------------------------ internals

        // THE one writer: the mutation runs on a copy of the design, which is recomputed; the copy becomes the draft only when nothing refused.
        private string Edit(Func<CantileverLineDesign, string> mutate)
        {
            var next = _design.DeepCopy();
            Notice = null;
            string refused;
            try
            {
                refused = mutate(next);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is FormatException)
            {
                refused = ex.Message;
            }

            if (refused != null) return refused;
            var computation = Build(next);
            if (computation == null) return "No se pudo recalcular la línea con ese valor: no se aplicó.";
            _design = next;
            _computation = computation;
            return null;
        }

        private CantileverLineEditorComputation Build(CantileverLineDesign design)
        {
            try
            {
                return _assembler.Build(design);
            }
            catch (Exception ex) when (!(ex is OutOfMemoryException))
            {
                return null;
            }
        }

        private string ApplyArm(CantileverArmTemplateDesign arm, CantileverArmField field, string text)
        {
            var body = arm.Body ??= new CantileverArmBodyDesign();
            var plate = arm.MountingPlate ??= new CantileverArmMountingPlateTemplateDesign();
            var end = arm.EndPlate ??= new CantileverArmEndPlateDesign();
            switch (field)
            {
                case CantileverArmField.Section:
                {
                    var refused = SectionOf(text, ArmFamilies, "El brazo", out var section);
                    if (refused != null) return refused;
                    if (body.Arrangement != CantileverArmBodyArrangement.Single && !IsChannel(section))
                        return "Con canal doble, la sección tiene que ser un canal (C): pasa antes el arreglo a «Sencillo».";
                    body.SectionId = section;
                    // The classic rule (CantileverArmWindow.SuggestPunchCountFor): choosing a section derives again how many punches the plate takes.
                    if (Catalogue.TryGetById(section, out var definition))
                    {
                        var height = GeometryFactory.Get(definition.SectionId, SectionDetailLevel.Tabulated).Bounds.Height;
                        plate.VerticalPunchCount = CantileverArmPunchCountRule.For(height,
                            plate.VerticalEndOffset ?? CantileverDefaults.ArmMountingPlateVerticalEndOffset, CantileverDefaults.RegularColumnPunchPitch);
                    }

                    return null;
                }
                case CantileverArmField.Arrangement:
                {
                    if (!Enum.TryParse<CantileverArmBodyArrangement>(text, out var arrangement) || !Enum.IsDefined(typeof(CantileverArmBodyArrangement), arrangement))
                        return "Elige un arreglo de la lista.";
                    if (arrangement != CantileverArmBodyArrangement.Single && !string.IsNullOrWhiteSpace(body.SectionId) && !IsChannel(body.SectionId))
                        return "Un canal doble exige una sección de canal (C): elige antes una sección C.";
                    body.Arrangement = arrangement;
                    return null;
                }
                case CantileverArmField.CutLength:
                    if (!TryNumber(text, out var cut) || !(cut > 0)) return "La longitud de corte debe ser un número mayor que cero.";
                    body.CutLength = cut;
                    return null;
                case CantileverArmField.Slope:
                    if (!TryNumber(text, out var slope) || slope < 0) return "La pendiente debe ser un número de 0 o más (subida por 12 in).";
                    body.SlopeRisePer12 = slope;
                    return null;
                case CantileverArmField.PlateThickness:
                    if (!TryNumber(text, out var thickness) || !(thickness > 0)) return "El espesor de la placa de montaje debe ser un número mayor que cero.";
                    plate.Thickness = thickness;
                    return null;
                case CantileverArmField.VerticalPunches:
                    if (!LocalizedNumberParser.TryInteger(text, out var punches) || punches < CantileverArmPunchCountRule.Minimum)
                        return "Escribe un número entero de troqueles verticales, de " + Int(CantileverArmPunchCountRule.Minimum) + " o más.";
                    plate.VerticalPunchCount = punches;
                    return null;
                case CantileverArmField.VerticalMargin:
                    if (!TryNumber(text, out var margin) || margin < 0) return "El margen vertical debe ser un número de 0 o más.";
                    plate.VerticalEndOffset = margin;
                    return null;
                case CantileverArmField.EndPlate:
                {
                    if (!Enum.TryParse<CantileverArmEndPlateMode>(text, out var mode) || !Enum.IsDefined(typeof(CantileverArmEndPlateMode), mode))
                        return "Elige un tipo de placa de extremo de la lista.";
                    end.Mode = mode;
                    if (mode != CantileverArmEndPlateMode.Stop) end.ExtraStopHeight = 0.0;
                    return null;
                }
                case CantileverArmField.EndPlateThickness:
                    if (end.Mode == CantileverArmEndPlateMode.None) return "Sin placa de extremo.";
                    if (!TryNumber(text, out var endThickness) || !(endThickness > 0)) return "El espesor de la placa de extremo debe ser un número mayor que cero.";
                    end.Thickness = endThickness;
                    return null;
                case CantileverArmField.ExtraStopHeight:
                    if (end.Mode != CantileverArmEndPlateMode.Stop) return "Solo con «Placa con tope».";
                    if (!TryNumber(text, out var stop) || stop < 0) return "La altura extra de tope debe ser un número de 0 o más.";
                    end.ExtraStopHeight = stop;
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(field), field, "El campo no tiene regla.");
            }
        }

        private static string ArmValue(CantileverArmTemplateDesign arm, CantileverArmField field)
        {
            arm ??= new CantileverArmTemplateDesign();
            switch (field)
            {
                case CantileverArmField.Section: return arm.Body?.SectionId ?? string.Empty;
                case CantileverArmField.Arrangement: return (arm.Body?.Arrangement ?? CantileverArmBodyArrangement.Single).ToString();
                case CantileverArmField.CutLength: return arm.Body?.CutLength > 0 ? Num(arm.Body.CutLength) : string.Empty;
                case CantileverArmField.Slope: return Num(arm.Body?.SlopeRisePer12 ?? 0.0);
                case CantileverArmField.PlateThickness: return Num(arm.MountingPlate?.Thickness ?? 0.0);
                case CantileverArmField.VerticalPunches: return Int(arm.MountingPlate?.VerticalPunchCount ?? 0);
                case CantileverArmField.VerticalMargin: return arm.MountingPlate?.VerticalEndOffset is double m ? Num(m) : string.Empty;
                case CantileverArmField.EndPlate: return (arm.EndPlate?.Mode ?? CantileverArmEndPlateMode.None).ToString();
                case CantileverArmField.EndPlateThickness: return Num(arm.EndPlate?.Thickness ?? 0.0);
                case CantileverArmField.ExtraStopHeight: return Num(arm.EndPlate?.ExtraStopHeight ?? 0.0);
                default: throw new ArgumentOutOfRangeException(nameof(field), field, "El campo no tiene regla.");
            }
        }

        // A section is written by its exact id (ADR-0024: searching is not resolving): it must be in the catalogue, and of a family the piece admits.
        private string SectionOf(string text, IReadOnlyList<StructuralSectionFamily> families, string piece, out string sectionId)
        {
            sectionId = null;
            if (string.IsNullOrWhiteSpace(text) || !Catalogue.TryGetById(text.Trim(), out var section))
                return piece + ": elige una sección de la lista del catálogo.";
            if (families != null && families.Count > 0 && !families.Contains(section.Family))
                return piece + " admite " + string.Join(" o ", families.Select(StructuralSectionSearch.Label)) + ": «" + section.DisplayName + "» no.";
            sectionId = section.SectionId.Value;
            return null;
        }

        private bool IsChannel(string sectionId) =>
            !string.IsNullOrWhiteSpace(sectionId) && Catalogue.TryGetById(sectionId, out var section) && section.Family == StructuralSectionFamily.Channel;

        private string Integer(string text, int minimum, string refusal, Action<CantileverLineDesign, int> write)
        {
            if (!LocalizedNumberParser.TryInteger(text, out var value) || value < minimum) return refusal;
            return Edit(d => { write(d, value); return null; });
        }

        private string Positive(string text, string what, Action<CantileverLineDesign, double> write)
        {
            if (!TryNumber(text, out var value) || !(value > 0)) return what + " debe ser un número mayor que cero.";
            return Edit(d => { write(d, value); return null; });
        }

        private string NonNegative(string text, string what, Action<CantileverLineDesign, double> write)
        {
            if (!TryNumber(text, out var value) || value < 0) return what + " debe ser un número de 0 o más.";
            return Edit(d => { write(d, value); return null; });
        }

        private string Choice<T>(string text, Action<CantileverLineDesign, T> write) where T : struct, Enum
        {
            if (!Enum.TryParse<T>(text, out var value) || !Enum.IsDefined(typeof(T), value)) return "Elige una opción de la lista.";
            return Edit(d => { write(d, value); return null; });
        }

        private string Flag(string text, Action<CantileverLineDesign, bool> write)
        {
            if (!TryFlag(text, out var value)) return "Elige sí o no.";
            return Edit(d => { write(d, value); return null; });
        }

        private static bool TryFlag(string text, out bool value) => bool.TryParse(text, out value);

        private static bool TryNumber(string text, out double value)
        {
            value = 0;
            return !string.IsNullOrWhiteSpace(text) && LocalizedNumberParser.TryDouble(text.Replace('−', '-').Trim(), out value)
                   && !double.IsNaN(value) && !double.IsInfinity(value);
        }

        private static string Bool(bool value) => value ? bool.TrueString : bool.FalseString;

        private static string TabOf(string code)
        {
            var c = (code ?? string.Empty).ToUpperInvariant();
            if (c.Contains("PANEL") || c.Contains("BRAC") || c.Contains("SEPARATOR") || c.Contains("BRACE")) return TabBracing;
            if (c.Contains("ARM")) return TabArms;
            if (c.Contains("COLUMN") || c.Contains("BASE")) return TabComponents;
            return TabLine;
        }

        private static string Serialize(CantileverLineDesign design) => new RackProjectStore().Serialize(RackProject.ForCantilever(design));

        private static CantileverLineStationTopologyDesign Topology(CantileverLineDesign d) => d.StationTopology ??= new CantileverLineStationTopologyDesign();

        private static CantileverStationColumnHeightDesign Height(CantileverLineDesign d) => Topology(d).ColumnHeight ??= new CantileverStationColumnHeightDesign();

        private static CantileverStationColumnBaseTemplateDesign ColumnBase(CantileverLineDesign d) =>
            Topology(d).ColumnBaseTemplate ??= new CantileverStationColumnBaseTemplateDesign();

        private static CantileverBaseDesign Base(CantileverLineDesign d) => ColumnBase(d).Base ??= new CantileverBaseDesign();

        private static CantileverPunchParameters Punches(CantileverLineDesign d) =>
            (ColumnBase(d).Connection ??= new CantileverColumnBaseConnectionDesign()).Punches ??= new CantileverPunchParameters();

        private static CantileverBracingDesign Bracing(CantileverLineDesign d) => d.Bracing ??= new CantileverBracingDesign();

        private static CantileverPlantaVisibilityDesign Planta(CantileverLineDesign d) => d.PlantaVisibility ??= new CantileverPlantaVisibilityDesign();
    }
}
