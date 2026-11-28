using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Domain.RackFrames;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>The categories of «Cambios respecto al estándar» and of the boxes of «Restaurar estándar…» (T4 cb, CB-05 and CB-06).</summary>
    public enum CabeceraChangeCategory
    {
        Horizontals = 0,
        Panels = 1,
        Posts = 2,
        Plates = 3,
        Celosia = 4,
    }

    /// <summary>One difference with the standard: the piece («H3», «P2», «Poste izquierdo»…) and what changed, in Spanish.</summary>
    public sealed record CabeceraChange(CabeceraChangeCategory Category, string Piece, string Text)
    {
        public string Line => Piece + ": " + Text;
    }

    /// <summary>What a write of the header editor did: applied (with a notice for the line of messages) or refused (with its reason).</summary>
    public sealed record CabeceraEdit(bool Applied, string Error, string Notice)
    {
        public static CabeceraEdit Refuse(string error) => new CabeceraEdit(false, error, null);

        public static CabeceraEdit Done(string notice = null) => new CabeceraEdit(true, null, notice);
    }

    public enum CabeceraHeightSeverity
    {
        None = 0,
        Informative = 1,
        Severe = 2,
    }

    /// <summary>The height of the cabecera against the rack that holds it (CB-09): below its top load level is severe.</summary>
    public sealed record CabeceraHeightNote(CabeceraHeightSeverity Severity, string Text)
    {
        public static readonly CabeceraHeightNote None = new CabeceraHeightNote(CabeceraHeightSeverity.None, string.Empty);
    }

    /// <summary>
    /// Where the header editor works (CB-01, CB-08): a cabecera of its own (a rack «Cabecera N» of the list), or the cabecera of a post
    /// or a module inside the editor of its rack, where the rack imposes the depth (and, in Dinámico, the peralte) and the editor only
    /// writes the draft of that rack. A locked value is shown read only with where it comes from.
    /// </summary>
    public sealed record CabeceraDraftContext(
        bool Embedded,
        double? LockedDepth = null,
        string DepthOrigin = null,
        double? LockedPostPeralte = null,
        double RackPostPeralte = 0.0,
        string PostPeralteOrigin = null,
        Func<double, CabeceraHeightNote> HeightReview = null,
        string Where = null)
    {
        public static readonly CabeceraDraftContext Standalone = new CabeceraDraftContext(false);
    }

    /// <summary>
    /// The draft of the header editor of the RackCad window (I-64, fourth batch, part cb; prototype «cabecera», CB-01..CB-13): ONE screen
    /// over a working copy of a <see cref="RackFrameConfiguration"/>, with no WPF and no AutoCAD. It replaces the two modes of the classic
    /// configurator (dolor 1): the template and the height regenerate (and say first what is lost), the post, the depth and the peralte are
    /// written without regenerating, and every other write edits the frame where it is.
    /// <para>
    /// The frame is kept NORMALIZED after every write, with the rules of the classic configurator: the horizontals ordered by elevation and
    /// numbered H1…Hn, one panel between each two consecutive horizontals (P1…Pn−1), and the height of the post = the top horizontal + the
    /// remate (<see cref="RackFrameConfigurationFactory.PostTopRemate"/>). A panel belongs to its LOWER horizontal: moving horizontals keeps
    /// each panel with the horizontal under it, splitting a panel gives both halves its values, a panel added on top is a standard one.
    /// </para>
    /// <para>
    /// «Cambios respecto al estándar» (CB-06) compares with the template (CB-05: the height it was generated with, the post and the depth
    /// of now), never with what was open; «Restaurar estándar…» goes back to that template by categories; «Descartar» goes back to what was
    /// open. «Cara de montaje» (CB-13, note of the Owner: the celosía always goes in the centre) is not written here: the datum stays in the
    /// domain (<see cref="BracingPanel.MountingFace"/>, <see cref="FrameHorizontal.MountingFace"/>) as it is.
    /// </para>
    /// </summary>
    public sealed class CabeceraDraftEditor
    {
        private const double Tolerance = 0.01;

        /// <summary>
        /// The smallest clear of a panel the editor writes (correction 5 of the review): the minimum of «Dividir en dos» — each half has at
        /// least this — and of «Claro», «Agregar panel arriba», «Elevación», «Desplazar» and «Duplicar arriba». A frame the template
        /// generated with a shorter closing panel is kept as it is; only a write is refused.
        /// </summary>
        public const double MinimumClear = 4.0;
        private const string TemplatePrefix = "TEMPLATE-";

        private static readonly RackFrameProjectStore Store = new RackFrameProjectStore();

        private readonly RackCatalog _catalog;
        private readonly IReadOnlyList<RackFrameTemplate> _templates;
        private RackFrameConfiguration _config;
        private string _openedJson;
        private string _openedTemplateId;
        private double _openedGeneratedHeight;
        private string _templateId;
        private double _generatedHeight;

        public CabeceraDraftEditor(RackFrameConfiguration opened, RackCatalog catalog, IReadOnlyList<RackFrameTemplate> templates, CabeceraDraftContext context)
        {
            if (opened == null) throw new ArgumentNullException(nameof(opened));
            _catalog = catalog ?? new RackCatalog();
            _templates = (templates ?? Array.Empty<RackFrameTemplate>()).Where(t => t != null).ToList();
            if (_templates.Count == 0) _templates = RackFrameTemplateCatalog.All;
            Context = context ?? CabeceraDraftContext.Standalone;

            _config = Store.DeepCopy(opened);
            ImposeContext(_config);
            Normalize(_config, null);
            _templateId = TemplateIdOf(_config);
            _generatedHeight = HeightOf(_config);
            _openedJson = Serialize(_config);
            _openedTemplateId = _templateId;
            _openedGeneratedHeight = _generatedHeight;
        }

        public CabeceraDraftContext Context { get; }

        public RackCatalog Catalog => _catalog;

        public IReadOnlyList<RackFrameTemplate> Templates => _templates;

        /// <summary>The working frame. Callers read it; every write goes through the methods of this editor.</summary>
        public RackFrameConfiguration Configuration => _config;

        public string TemplateId => _templateId;

        public RackFrameTemplate Template => FindTemplate(_templateId) ?? _templates[0];

        /// <summary>The height the standard of «Cambios respecto al estándar» is generated with (CB-05): the one of the last regeneration.</summary>
        public double GeneratedHeight => _generatedHeight;

        /// <summary>The height of the post: the top horizontal + the remate (CB-03).</summary>
        public double Height => HeightOf(_config);

        public double TopElevation => _config.Horizontals.Count == 0 ? 0.0 : _config.Horizontals.Max(h => h.Elevation);

        public int PanelCount => _config.BracingPanels.Count;

        public int HorizontalCount => _config.Horizontals.Count;

        public IReadOnlyList<FrameHorizontal> Horizontals => _config.Horizontals.ToList();

        public IReadOnlyList<BracingPanel> Panels => _config.BracingPanels.ToList();

        public PostAssembly Side(PostSide side) => side == PostSide.Left ? _config.LeftPost : _config.RightPost;

        public BasePlatePlacement Plate(PostSide side) => side == PostSide.Left ? _config.LeftBasePlate : _config.RightBasePlate;

        /// <summary>The two posts and plates are the same (CB-07: «Igual en los dos postes»).</summary>
        public bool SidesAreEqual => SameSide(_config.LeftPost, _config.RightPost) && SamePlate(_config.LeftBasePlate, _config.RightBasePlate);

        /// <summary>The draft differs from what was opened, read or written last.</summary>
        public bool IsDirty => !string.Equals(Serialize(_config), _openedJson, StringComparison.Ordinal);

        /// <summary>The JSON of the working frame, as the project store writes it.</summary>
        public string Json => Serialize(_config);

        /// <summary>An independent copy of the working frame, for whoever writes it (the rack that holds it, or the door).</summary>
        public RackFrameConfiguration Snapshot() => Store.DeepCopy(_config);

        /// <summary>«Descartar»: back to what was opened, read or written last.</summary>
        public void Discard()
        {
            _config = Store.Deserialize(_openedJson);
            _templateId = _openedTemplateId;
            _generatedHeight = _openedGeneratedHeight;
        }

        /// <summary>
        /// What was written becomes what «Descartar» goes back to (a written standalone cabecera): the JSON that was SENT to AutoCAD
        /// (<paramref name="writtenJson"/>), which is what the drawing has now; without it, the working frame.
        /// </summary>
        public void MarkWritten(string writtenJson = null)
        {
            _openedJson = string.IsNullOrWhiteSpace(writtenJson) ? Serialize(_config) : writtenJson;
            _openedTemplateId = _templateId;
            _openedGeneratedHeight = _generatedHeight;
        }

        /// <summary>T7 cab: what «Descartar» goes back to (what was opened, read or written last), as the JSON of the project store.</summary>
        public string BaselineJson => _openedJson;

        /// <summary>
        /// T7 cab («Duplicar» of a cabecera of «Nuevo», the rule of the other drafts): the working frame becomes <paramref name="edited"/>, kept
        /// normalized, while what «Descartar» goes back to stays what was opened (the base of the original). The template is the one the
        /// frame names and the height it was generated with is its own height (CB4-03: the model keeps no other).
        /// </summary>
        public void TakeAsEdit(RackFrameConfiguration edited)
        {
            if (edited == null) throw new ArgumentNullException(nameof(edited));
            var next = Store.DeepCopy(edited);
            ImposeContext(next);
            Normalize(next, null);
            _config = next;
            _templateId = TemplateIdOf(_config);
            _generatedHeight = HeightOf(_config);
        }

        // ------------------------------------------------------------------ the standard and its differences

        /// <summary>The frame the template gives with the height it was generated with, the post and the depth of now; null if it cannot be built.</summary>
        public RackFrameConfiguration Standard() => Build(_templateId, _generatedHeight);

        /// <summary>«Cambios respecto al estándar» (CB-06), by piece and in Spanish. With at least one the cabecera is «Personalizada».</summary>
        public IReadOnlyList<CabeceraChange> Changes()
        {
            var standard = Standard();
            return standard == null ? Array.Empty<CabeceraChange>() : Compare(_config, standard);
        }

        /// <summary>Whether the panel <paramref name="index"/> differs from the standard panel with its same elevations.</summary>
        public bool PanelDiffers(int index)
        {
            var standard = Standard();
            if (standard == null || index < 0 || index >= _config.BracingPanels.Count) return false;
            var panel = _config.BracingPanels[index];
            var std = StandardPanelAt(standard, panel.StartElevation, panel.EndElevation);
            return std == null || !SamePanel(panel, std);
        }

        /// <summary>Whether the horizontal <paramref name="index"/> differs from the standard one (by position, or by elevation if the count changed).</summary>
        public bool HorizontalDiffers(int index)
        {
            var standard = Standard();
            if (standard == null || index < 0 || index >= _config.Horizontals.Count) return false;
            var own = _config.Horizontals[index];
            var std = standard.Horizontals.Count == _config.Horizontals.Count
                ? standard.Horizontals[index]
                : standard.Horizontals.FirstOrDefault(h => Near(h.Elevation, own.Elevation));
            return std == null || !Near(std.Elevation, own.Elevation) || !Same(std.ProfileId, own.ProfileId) || std.Quantity != own.Quantity;
        }

        /// <summary>
        /// «Comprobación» (CB-14 of the inventory: every notice at once): what the model says about itself, in Spanish, and the height
        /// against the rack that holds it. A notice does not prevent writing.
        /// </summary>
        public IReadOnlyList<string> Notices()
        {
            var list = new List<string>();
            var ordered = _config.Horizontals.OrderBy(h => h.Elevation).ToList();
            for (var i = 1; i < ordered.Count; i++)
            {
                if (Math.Abs(ordered[i].Elevation - ordered[i - 1].Elevation) <= Tolerance)
                {
                    list.Add("Elevación duplicada o muy cercana en horizontales: " + Inches(ordered[i].Elevation) + "; el panel entre las dos no tiene altura.");
                    break;
                }
            }

            var paso = _config.PasoTroquel > 0.0 ? _config.PasoTroquel : 2.0;
            var expected = Math.Max(0.0, (_config.CelosiaStartTroquel - 1) * paso);
            if (ordered.Count > 0 && Math.Abs(ordered[0].Elevation - expected) > Tolerance)
            {
                list.Add("La horizontal inferior debería estar en el troquel de inicio (" + Inches(expected) + "); está en " + Inches(ordered[0].Elevation) + ".");
            }

            var height = Height;
            foreach (var (post, name) in new[] { (_config.LeftPost, "izquierdo"), (_config.RightPost, "derecho") })
            {
                if (post != null && post.HasReinforcement && post.ReinforcementHeight > height + 1e-6)
                {
                    list.Add("El refuerzo del poste " + name + " (" + Inches(post.ReinforcementHeight) + ") supera la altura del marco (" + Inches(height) + ").");
                }
            }

            // The ids the catalog does not know: the rule of the classic configurator (the others above say the same with accents).
            foreach (var warning in FrameModelValidator.Validate(_config, _catalog, Tolerance))
            {
                if (warning.StartsWith("Perfil", StringComparison.Ordinal) || warning.IndexOf("catalogo", StringComparison.OrdinalIgnoreCase) >= 0
                    || warning.IndexOf("catálogo", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    list.Add(warning);
                }
            }

            var note = HeightNote();
            if (note.Severity != CabeceraHeightSeverity.None && !string.IsNullOrWhiteSpace(note.Text)) list.Add(note.Text);
            return list;
        }

        /// <summary>The height of the cabecera against the rack that holds it (CB-09); none outside a rack.</summary>
        public CabeceraHeightNote HeightNote()
        {
            if (Context.HeightReview == null) return CabeceraHeightNote.None;
            try
            {
                return Context.HeightReview(Height) ?? CabeceraHeightNote.None;
            }
            catch (InvalidOperationException)
            {
                return CabeceraHeightNote.None;
            }
        }

        // ------------------------------------------------------------------ «Lo esencial» (tab «Cabecera»)

        public CabeceraEdit SetName(string name)
        {
            if (Context.Embedded) return CabeceraEdit.Refuse("La cabecera de un poste no tiene nombre propio.");
            _config.Name = (name ?? string.Empty).Trim();
            return CabeceraEdit.Done();
        }

        /// <summary>
        /// What «Plantilla» with <paramref name="templateId"/> would lose (empty = nothing is lost): the changes of now, the «Notas» of the
        /// horizontals whose elevation does not stay (the others are kept), and a «Cara de montaje» that is not the front one (correction 3
        /// of the review: the regeneration puts every piece on the front face, and the window does not offer the face, CB-13).
        /// </summary>
        public IReadOnlyList<CabeceraChange> RegenerationLossesForTemplate(string templateId)
        {
            var template = FindTemplate(templateId);
            if (template == null || string.Equals(template.Id, _templateId, StringComparison.OrdinalIgnoreCase)) return Array.Empty<CabeceraChange>();
            return RegenerationLosses(template.Id, Height > 0.0 ? Height : template.DefaultHeight);
        }

        /// <summary>What «Alto» <paramref name="height"/> would lose, with the same rules as <see cref="RegenerationLossesForTemplate"/>.</summary>
        public IReadOnlyList<CabeceraChange> RegenerationLossesForHeight(double height)
        {
            if (!(height > 0.0) || Math.Abs(height - Height) < Tolerance) return Array.Empty<CabeceraChange>();
            return RegenerationLosses(_templateId, height);
        }

        private IReadOnlyList<CabeceraChange> RegenerationLosses(string templateId, double height)
        {
            var list = Changes().ToList();
            list.AddRange(ReplacementLosses(Build(templateId, height), horizontals: true, panels: true));
            return list;
        }

        // What replacing the horizontals (and their panels) and the panels with those of «next» loses beyond the changes respecto al
        // estándar: the notes of a horizontal whose elevation does not stay in «next» (one that stays keeps its notes), a face of mounting
        // that is not the front one (the template is always on the front face), and, when only the horizontals are replaced, the values of a
        // panel whose interval does not stay (it takes the values of the template). Correcciones 3 y 4 de la revisión.
        private IEnumerable<CabeceraChange> ReplacementLosses(RackFrameConfiguration next, bool horizontals, bool panels)
        {
            var hs = _config.Horizontals;
            for (var i = 0; horizontals && i < hs.Count; i++)
            {
                var h = hs[i];
                var stays = next != null && next.Horizontals.Any(o => Near(o.Elevation, h.Elevation));
                if (!string.IsNullOrWhiteSpace(h.Notes) && !stays)
                    yield return new CabeceraChange(CabeceraChangeCategory.Horizontals, "H" + (i + 1), "nota «" + h.Notes.Trim() + "» (su elevación no queda)");
                if (h.MountingFace != FrameSide.Front)
                    yield return new CabeceraChange(CabeceraChangeCategory.Horizontals, "H" + (i + 1), FaceLabel(h.MountingFace) + " → cara frontal");
            }

            for (var i = 0; i < _config.BracingPanels.Count; i++)
            {
                var p = _config.BracingPanels[i];
                var intervalStays = next != null && StandardPanelAt(next, p.StartElevation, p.EndElevation) != null;
                var replaced = panels || (horizontals && !intervalStays);
                if (!replaced) continue;
                if (!panels && next != null)
                {
                    // The panel of the template that holds the start of its tramo takes its place; with its same values nothing is lost.
                    var successor = next.BracingPanels.FirstOrDefault(o => o.StartElevation <= p.StartElevation + Tolerance && p.StartElevation < o.EndElevation - Tolerance);
                    if (successor == null || !SamePanel(p, successor))
                        yield return new CabeceraChange(CabeceraChangeCategory.Panels, "P" + (i + 1),
                            "su tramo de " + Num(p.StartElevation) + " a " + Inches(p.EndElevation) + " no queda; pierde " + Lower(ArrangementLabel(p.Arrangement))
                            + " y toma los valores de la plantilla");
                }

                if (p.MountingFace != FrameSide.Front)
                    yield return new CabeceraChange(CabeceraChangeCategory.Panels, "P" + (i + 1), FaceLabel(p.MountingFace) + " → cara frontal");
            }
        }

        private static string ClearText => "El claro debe ser de al menos " + Inches(MinimumClear) + " (el mínimo de un panel).";

        private static string FaceLabel(FrameSide face)
            => face == FrameSide.Back ? "cara posterior" : face == FrameSide.Both ? "ambas caras" : "cara frontal";

        // The notes of the horizontals that keep their elevation in the frame that replaces them (correction 3 of the review).
        private static void KeepNotes(IEnumerable<FrameHorizontal> before, RackFrameConfiguration next)
        {
            foreach (var old in before.Where(h => !string.IsNullOrWhiteSpace(h.Notes)))
            {
                var same = next.Horizontals.FirstOrDefault(h => Near(h.Elevation, old.Elevation) && string.IsNullOrWhiteSpace(h.Notes));
                if (same != null) same.Notes = old.Notes;
            }
        }

        /// <summary>
        /// The template (CB-02): regenerates the frame from it with the height of now, the post and the depth; the name and the peralte stay.
        /// The window asks first with <see cref="RegenerationLosses"/> when there are own changes.
        /// </summary>
        public CabeceraEdit SetTemplate(string templateId)
        {
            var template = FindTemplate(templateId);
            if (template == null) return CabeceraEdit.Refuse("Esa plantilla no está en el catálogo.");
            if (string.Equals(template.Id, _templateId, StringComparison.OrdinalIgnoreCase)) return CabeceraEdit.Done();
            var height = Height > 0.0 ? Height : template.DefaultHeight;
            return Regenerate(template.Id, height, "Cabecera regenerada desde «" + Text(template.Name) + "» a " + Inches(height) + ".");
        }

        /// <summary>«Alto» (CB-03): the height of the post. Writing another one regenerates from the template (and the window asks first).</summary>
        public CabeceraEdit SetHeight(double height)
        {
            if (!(height > 0.0) || double.IsNaN(height) || double.IsInfinity(height)) return CabeceraEdit.Refuse("El alto debe ser un número mayor que cero.");
            if (Math.Abs(height - Height) < Tolerance) return CabeceraEdit.Done();
            return Regenerate(_templateId, height, "Cabecera regenerada desde «" + Text(Template.Name) + "» a " + Inches(height) + ".");
        }

        /// <summary>«Poste»: the profile of the two posts, without regenerating; a reinforcement of the same profile follows it.</summary>
        public CabeceraEdit SetPostProfile(string postId)
        {
            var id = (postId ?? string.Empty).Trim();
            if (id.Length == 0) return CabeceraEdit.Refuse("Elige un poste del catálogo.");
            foreach (var post in new[] { _config.LeftPost, _config.RightPost })
            {
                if (post == null) continue;
                var old = post.PostCatalogId;
                post.PostCatalogId = id;
                post.Description = DescriptionOf(id, post.Description);
                if (Same(post.ReinforcementCatalogId, old)) post.ReinforcementCatalogId = id;
            }

            return CabeceraEdit.Done();
        }

        /// <summary>«Profundidad» (today «Fondo»): from the front face to the back one. Read only when the rack imposes it (CB-08).</summary>
        public CabeceraEdit SetDepth(double depth)
        {
            if (Context.LockedDepth.HasValue) return CabeceraEdit.Refuse("La profundidad la da el rack: " + Text(Context.DepthOrigin));
            if (!(depth > 0.0) || double.IsNaN(depth) || double.IsInfinity(depth)) return CabeceraEdit.Refuse("La profundidad debe ser un número mayor que cero.");
            _config.Depth = depth;
            return CabeceraEdit.Done();
        }

        /// <summary>
        /// «Peralte del poste»: null = the one of the profile (a cabecera of its own) or the one of the rack (inside a Selectivo, CB-08). Read
        /// only when the rack imposes it (Dinámico).
        /// </summary>
        public CabeceraEdit SetPostPeralte(double? peralte)
        {
            if (Context.LockedPostPeralte.HasValue) return CabeceraEdit.Refuse("El peralte lo da el rack: " + Text(Context.PostPeralteOrigin));
            if (peralte.HasValue && (!(peralte.Value > 0.0) || double.IsNaN(peralte.Value) || double.IsInfinity(peralte.Value)))
                return CabeceraEdit.Refuse("El peralte debe ser un número mayor que cero (vacío = " + (Context.Embedded ? "el del rack" : "el del perfil") + ").");
            _config.PostPeralte = peralte ?? (Context.Embedded ? Context.RackPostPeralte : 0.0);
            return CabeceraEdit.Done();
        }

        /// <summary>The peralte the field shows: empty when it is the one of the rack (embedded) or of the profile (0).</summary>
        public double? OwnPostPeralte()
        {
            var value = _config.PostPeralte;
            if (!(value > 0.0)) return null;
            if (Context.Embedded && Context.RackPostPeralte > 0.0 && Math.Abs(value - Context.RackPostPeralte) < 1e-6) return null;
            return value;
        }

        // ------------------------------------------------------------------ «Alzado»: panels

        public CabeceraEdit SetPanelArrangement(IEnumerable<int> panels, BracingPattern arrangement)
            => EditPanels(panels, p => p.Arrangement = arrangement);

        public CabeceraEdit SetPanelDirection(IEnumerable<int> panels, DiagonalDirection direction)
            => EditPanels(panels, p => p.DiagonalDirection = direction);

        public CabeceraEdit SetPanelProfile(IEnumerable<int> panels, string profileId)
            => string.IsNullOrWhiteSpace(profileId) ? CabeceraEdit.Refuse("Elige un perfil del catálogo.") : EditPanels(panels, p => p.DiagonalProfileId = profileId.Trim());

        public CabeceraEdit SetPanelStartPoint(IEnumerable<int> panels, string pointId)
            => string.IsNullOrWhiteSpace(pointId) ? CabeceraEdit.Refuse("Elige un punto.") : EditPanels(panels, p => p.StartConnectionPointId = pointId.Trim());

        public CabeceraEdit SetPanelEndPoint(IEnumerable<int> panels, string pointId)
            => string.IsNullOrWhiteSpace(pointId) ? CabeceraEdit.Refuse("Elige un punto.") : EditPanels(panels, p => p.EndConnectionPointId = pointId.Trim());

        /// <summary>
        /// «Claro» of the panels (CB-04, note of the Owner: «falta una opción para cambiar el claro del panel»). The clear of a panel is not a
        /// datum of the model: it is the distance between its two horizontals (<see cref="BracingPanel.ClearHeight"/>). Writing it moves the
        /// upper horizontal of each chosen panel and everything above it by the difference, from the lowest chosen panel up, so the other
        /// panels keep their clear and the post grows or shrinks (the remate stays on the top horizontal).
        /// </summary>
        public CabeceraEdit SetPanelClear(IEnumerable<int> panels, double clear)
        {
            if (double.IsNaN(clear) || double.IsInfinity(clear) || clear < MinimumClear) return CabeceraEdit.Refuse(ClearText);
            var chosen = Valid(panels, _config.BracingPanels.Count);
            if (chosen.Count == 0) return CabeceraEdit.Refuse("Elige uno o más paneles.");
            var ordered = Ordered(_config);
            foreach (var index in chosen.OrderBy(i => i))
            {
                var lower = ordered[index];
                var upper = ordered[index + 1];
                var delta = clear - (upper.Elevation - lower.Elevation);
                if (Math.Abs(delta) < 1e-9) continue;
                for (var k = index + 1; k < ordered.Count; k++) ordered[k].Elevation = Round(ordered[k].Elevation + delta);
            }

            Normalize(_config, null);
            return CabeceraEdit.Done(chosen.Count == 1
                ? "El claro de P" + (chosen[0] + 1) + " es ahora " + Inches(clear) + "; lo de arriba se movió con él. La altura del poste es " + Inches(Height) + "."
                : "El claro de " + chosen.Count + " paneles es ahora " + Inches(clear) + "; lo de arriba se movió con ellos. La altura del poste es " + Inches(Height) + ".");
        }

        /// <summary>«Dividir en dos»: a horizontal in the middle of the panel; both halves keep its values.</summary>
        public CabeceraEdit SplitPanel(int index)
        {
            if (index < 0 || index >= _config.BracingPanels.Count) return CabeceraEdit.Refuse("Elige un solo panel.");
            var ordered = Ordered(_config);
            var lower = ordered[index];
            var upper = ordered[index + 1];
            if (upper.Elevation - lower.Elevation < 2.0 * MinimumClear)
                return CabeceraEdit.Refuse("El panel es demasiado bajo para dividirlo: cada mitad debe tener al menos " + Inches(MinimumClear) + ".");
            var middle = Round((lower.Elevation + upper.Elevation) / 2.0);
            var panel = _config.BracingPanels[index];
            var added = NewHorizontal(middle, lower.ProfileId);
            _config.Horizontals.Add(added);
            Normalize(_config, new Dictionary<FrameHorizontal, BracingPanel> { [added] = CopyPanel(panel) });
            return CabeceraEdit.Done("P" + (index + 1) + " dividido en dos de " + Inches((upper.Elevation - lower.Elevation) / 2.0) + ", con una horizontal nueva a " + Inches(middle) + ".");
        }

        /// <summary>«Combinar con el de arriba»: the horizontal they share goes; the panel keeps the values of the lower one.</summary>
        public CabeceraEdit CombineWithAbove(int index)
        {
            if (index < 0 || index >= _config.BracingPanels.Count - 1 || _config.Horizontals.Count <= 2) return CabeceraEdit.Refuse("Elige un panel que tenga otro encima.");
            var ordered = Ordered(_config);
            var gone = ordered[index + 1];
            _config.Horizontals.Remove(gone);
            Normalize(_config, null);
            return CabeceraEdit.Done("P" + (index + 1) + " y P" + (index + 2) + " combinados: se quitó la horizontal a " + Inches(gone.Elevation) + ".");
        }

        /// <summary>«Volver al estándar» of the chosen panels: the values the template gives a panel with the same elevations.</summary>
        public CabeceraEdit PanelsToStandard(IEnumerable<int> panels)
        {
            var standard = Standard();
            if (standard == null) return CabeceraEdit.Refuse("No se pudo calcular el estándar.");
            var changed = 0;
            foreach (var index in Valid(panels, _config.BracingPanels.Count))
            {
                var panel = _config.BracingPanels[index];
                var std = StandardPanelAt(standard, panel.StartElevation, panel.EndElevation);
                if (std == null || SamePanel(panel, std)) continue;
                CopyPanelValues(std, panel);
                changed++;
            }

            return changed == 0 ? CabeceraEdit.Refuse("Ninguno de los paneles elegidos tiene un panel estándar con sus mismas elevaciones.")
                : CabeceraEdit.Done("Los paneles elegidos volvieron a como los da la plantilla.");
        }

        /// <summary>«Agregar panel arriba» with its clear (CB-12; 44 in by default): a horizontal over the top one and a standard panel under it.</summary>
        public CabeceraEdit AddPanelOnTop(double clear)
        {
            if (double.IsNaN(clear) || double.IsInfinity(clear) || clear < MinimumClear) return CabeceraEdit.Refuse("El claro del panel nuevo debe ser de al menos " + Inches(MinimumClear) + " (el mínimo de un panel).");
            var top = Ordered(_config).LastOrDefault();
            var elevation = Round((top?.Elevation ?? 0.0) + clear);
            _config.Horizontals.Add(NewHorizontal(elevation, top?.ProfileId ?? DefaultHorizontalProfile()));
            Normalize(_config, null);
            return CabeceraEdit.Done("Panel de " + Inches(clear) + " agregado arriba, con diagonal simple. La altura del poste pasa a " + Inches(Height) + ".");
        }

        // ------------------------------------------------------------------ «Alzado»: horizontals

        /// <summary>«Elevación» of ONE horizontal; moving the top one changes the height of the post without regenerating (CB-03).</summary>
        public CabeceraEdit SetElevation(int index, double elevation)
        {
            if (index < 0 || index >= _config.Horizontals.Count) return CabeceraEdit.Refuse("Elige una sola horizontal.");
            if (!(elevation >= 0.0) || double.IsNaN(elevation) || double.IsInfinity(elevation)) return CabeceraEdit.Refuse("La elevación debe ser un número de 0 o más.");
            var own = _config.Horizontals[index];
            var near = _config.Horizontals.FirstOrDefault(h => !ReferenceEquals(h, own) && Math.Abs(h.Elevation - elevation) < MinimumClear - 1e-9);
            if (near != null)
                return CabeceraEdit.Refuse("Quedaría a menos de " + Inches(MinimumClear) + " de " + near.Id + " (a " + Inches(near.Elevation) + "): no se movió, para no dejar un panel sin altura.");
            own.Elevation = Round(elevation);
            Normalize(_config, null);
            return CabeceraEdit.Done();
        }

        /// <summary>«Desplazar» the chosen horizontals together (today «Desplazar elevación»); none goes under 0.</summary>
        public CabeceraEdit ShiftHorizontals(IEnumerable<int> horizontals, double delta)
        {
            if (double.IsNaN(delta) || double.IsInfinity(delta) || Math.Abs(delta) < 1e-9) return CabeceraEdit.Refuse("Escribe un desplazamiento distinto de cero, por ejemplo +6 o −6.");
            var chosen = Valid(horizontals, _config.Horizontals.Count).Select(i => _config.Horizontals[i]).ToList();
            if (chosen.Count == 0) return CabeceraEdit.Refuse("Elige una o más horizontales.");
            var moved = chosen.Select(h => Round(Math.Max(0.0, h.Elevation + delta))).ToList();
            var others = _config.Horizontals.Except(chosen).Select(h => h.Elevation).ToList();

            // The moved ones keep their distances among them (two of them could only meet at 0); against the others, the minimum clear.
            var sorted = moved.OrderBy(e => e).ToList();
            for (var i = 1; i < sorted.Count; i++)
            {
                if (Math.Abs(sorted[i] - sorted[i - 1]) <= Tolerance) return CabeceraEdit.Refuse("Dos horizontales quedarían a la misma elevación: no se movió nada.");
            }

            if (moved.Any(m => others.Any(o => Math.Abs(m - o) < MinimumClear - 1e-9)))
                return CabeceraEdit.Refuse("Una horizontal quedaría a menos de " + Inches(MinimumClear) + " de otra: no se movió nada.");

            for (var i = 0; i < chosen.Count; i++) chosen[i].Elevation = moved[i];
            Normalize(_config, null);
            return CabeceraEdit.Done("Se " + (chosen.Count == 1 ? "movió 1 horizontal " : "movieron " + chosen.Count + " horizontales ") + (delta > 0 ? "+" : "") + Num(delta) + " in.");
        }

        public CabeceraEdit SetHorizontalProfile(IEnumerable<int> horizontals, string profileId)
            => string.IsNullOrWhiteSpace(profileId) ? CabeceraEdit.Refuse("Elige un perfil del catálogo.") : EditHorizontals(horizontals, h => h.ProfileId = profileId.Trim());

        /// <summary>«Cantidad»: 2 = a double horizontal (another travesaño one troquel higher).</summary>
        public CabeceraEdit SetHorizontalQuantity(IEnumerable<int> horizontals, int quantity)
            => quantity < 1 ? CabeceraEdit.Refuse("La cantidad debe ser un entero de 1 o más.") : EditHorizontals(horizontals, h => h.Quantity = quantity);

        /// <summary>«Notas»: informative only (they change neither the drawing nor the BOM).</summary>
        public CabeceraEdit SetHorizontalNotes(IEnumerable<int> horizontals, string notes)
            => EditHorizontals(horizontals, h => h.Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim());

        /// <summary>«Duplicar arriba»: a copy of ONE horizontal one standard clear higher.</summary>
        public CabeceraEdit DuplicateAbove(int index)
        {
            if (index < 0 || index >= _config.Horizontals.Count) return CabeceraEdit.Refuse("Elige una sola horizontal.");
            var source = _config.Horizontals[index];
            var clear = _config.PanelClear > 0.0 ? _config.PanelClear : 44.0;
            var elevation = Round(source.Elevation + clear);
            if (_config.Horizontals.Any(h => Math.Abs(h.Elevation - elevation) < MinimumClear - 1e-9))
                return CabeceraEdit.Refuse("Ya hay una horizontal a menos de " + Inches(MinimumClear) + " de " + Inches(elevation) + ": no se duplicó.");
            var ordered = Ordered(_config);
            var below = ordered.LastOrDefault(h => h.Elevation < elevation);
            var inherited = below == null ? null : _config.BracingPanels.FirstOrDefault(p => p.LowerHorizontalId == below.Id);
            var copy = NewHorizontal(elevation, source.ProfileId);
            copy.Quantity = source.Quantity;
            copy.MountingFace = source.MountingFace;
            copy.Notes = source.Notes;
            _config.Horizontals.Add(copy);
            Normalize(_config, inherited == null ? null : new Dictionary<FrameHorizontal, BracingPanel> { [copy] = CopyPanel(inherited) });
            return CabeceraEdit.Done("H" + (index + 1) + " duplicada " + Inches(clear) + " más arriba, a " + Inches(elevation) + ".");
        }

        /// <summary>«Eliminar» the chosen horizontals: at least two stay; the panels around join.</summary>
        public CabeceraEdit DeleteHorizontals(IEnumerable<int> horizontals)
        {
            var chosen = Valid(horizontals, _config.Horizontals.Count).Select(i => _config.Horizontals[i]).ToList();
            if (chosen.Count == 0) return CabeceraEdit.Refuse("Elige una o más horizontales.");
            if (_config.Horizontals.Count - chosen.Count < 2) return CabeceraEdit.Refuse("Deben quedar al menos dos horizontales.");
            foreach (var h in chosen) _config.Horizontals.Remove(h);
            Normalize(_config, null);
            return CabeceraEdit.Done(chosen.Count == 1 ? "Se eliminó 1 horizontal; los paneles de alrededor se unieron." : "Se eliminaron " + chosen.Count + " horizontales; los paneles de alrededor se unieron.");
        }

        // ------------------------------------------------------------------ «Postes y placas»

        public CabeceraEdit SetSidePost(IEnumerable<PostSide> sides, string postId)
            => string.IsNullOrWhiteSpace(postId) ? CabeceraEdit.Refuse("Elige un poste del catálogo.")
                : EditSides(sides, (post, plate) => { post.PostCatalogId = postId.Trim(); post.Description = DescriptionOf(postId.Trim(), post.Description); });

        public CabeceraEdit SetSideDescription(IEnumerable<PostSide> sides, string description)
            => EditSides(sides, (post, plate) => post.Description = (description ?? string.Empty).Trim());

        public CabeceraEdit SetSideReinforced(IEnumerable<PostSide> sides, bool reinforced)
            => EditSides(sides, (post, plate) =>
            {
                post.HasReinforcement = reinforced;
                if (reinforced && string.IsNullOrWhiteSpace(post.ReinforcementCatalogId)) post.ReinforcementCatalogId = post.PostCatalogId;
            });

        public CabeceraEdit SetSideReinforcementType(IEnumerable<PostSide> sides, string postId)
            => string.IsNullOrWhiteSpace(postId) ? CabeceraEdit.Refuse("Elige un poste del catálogo (un refuerzo es un poste).")
                : EditSides(sides, (post, plate) => post.ReinforcementCatalogId = postId.Trim());

        public CabeceraEdit SetSideReinforcementHeight(IEnumerable<PostSide> sides, double height)
            => !(height >= 0.0) || double.IsNaN(height) || double.IsInfinity(height) ? CabeceraEdit.Refuse("La altura del refuerzo debe ser un número de 0 o más.")
                : EditSides(sides, (post, plate) => post.ReinforcementHeight = height);

        public CabeceraEdit SetSidePlate(IEnumerable<PostSide> sides, string plateId)
            => string.IsNullOrWhiteSpace(plateId) ? CabeceraEdit.Refuse("Elige una placa del catálogo.")
                : EditSides(sides, (post, plate) =>
                {
                    plate.PlateCatalogId = plateId.Trim();
                    var entry = _catalog.BasePlates.FindBasePlate(plateId.Trim());
                    if (entry != null) plate.Description = entry.Description ?? plate.Description;
                });

        /// <summary>«Peralte de la placa»: null = the one of the post.</summary>
        public CabeceraEdit SetSidePlatePeralte(IEnumerable<PostSide> sides, double? peralte)
            => peralte.HasValue && (!(peralte.Value > 0.0) || double.IsNaN(peralte.Value) || double.IsInfinity(peralte.Value))
                ? CabeceraEdit.Refuse("El peralte de la placa debe ser un número mayor que cero (vacío = el del poste).")
                : EditSides(sides, (post, plate) => plate.PeralteOverride = peralte);

        public CabeceraEdit SetSidePlatePoint(IEnumerable<PostSide> sides, string pointId)
            => string.IsNullOrWhiteSpace(pointId) ? CabeceraEdit.Refuse("Elige un punto.") : EditSides(sides, (post, plate) => plate.ConnectionPointId = pointId.Trim());

        /// <summary>«Igual en los dos postes» turned on with different posts: the right one takes what the left one has.</summary>
        public CabeceraEdit MakeSidesEqual()
        {
            if (SidesAreEqual) return CabeceraEdit.Done();
            CopySide(_config.LeftPost, _config.RightPost, PostSide.Right);
            CopyPlate(_config.LeftBasePlate, _config.RightBasePlate, PostSide.Right);
            return CabeceraEdit.Done("El poste derecho toma lo del izquierdo.");
        }

        // ------------------------------------------------------------------ «Avanzado»: celosía

        public CabeceraEdit SetCelosiaStart(int troquel)
        {
            if (troquel < 1) return CabeceraEdit.Refuse("El inicio de la celosía debe ser un entero de 1 o más.");
            _config.CelosiaStartTroquel = troquel;
            return CabeceraEdit.Done();
        }

        public CabeceraEdit SetDiagonalStartOffset(int troqueles)
        {
            if (troqueles < 0) return CabeceraEdit.Refuse("La separación debe ser un entero de 0 o más.");
            _config.DiagonalStartOffsetTroqueles = troqueles;
            return CabeceraEdit.Done();
        }

        public CabeceraEdit SetDiagonalEndOffset(int troqueles)
        {
            if (troqueles < 0) return CabeceraEdit.Refuse("La separación debe ser un entero de 0 o más.");
            _config.DiagonalEndOffsetTroqueles = troqueles;
            return CabeceraEdit.Done();
        }

        // ------------------------------------------------------------------ «Restaurar estándar…»

        /// <summary>
        /// The lines of «Ver lo que se pierde»: the changes of the categories marked and, beyond them (correction 4 of the review), what the
        /// restoration replaces without being a change of those categories: with «Horizontales», the panels whose tramo does not stay (when
        /// «Paneles» is not marked, they take the values of the template), the notes of the horizontals whose elevation does not stay, and a
        /// face that is not the front one.
        /// </summary>
        public IReadOnlyList<CabeceraChange> RestoreLosses(IReadOnlyCollection<CabeceraChangeCategory> categories)
        {
            if (categories == null || categories.Count == 0) return Array.Empty<CabeceraChange>();
            var list = Changes().Where(c => categories.Contains(c.Category)).ToList();
            var horizontals = categories.Contains(CabeceraChangeCategory.Horizontals);
            var panels = categories.Contains(CabeceraChangeCategory.Panels);
            if (horizontals || panels) list.AddRange(ReplacementLosses(Standard(), horizontals, panels));
            return list;
        }

        /// <summary>
        /// «Restaurar estándar…» (CB-05) of the categories marked: back to the template with the height it was generated with, the post and the
        /// depth of now. The horizontals bring their panels (a panel kept by «Paneles» unmarked keeps its values where its elevations remain).
        /// </summary>
        public CabeceraEdit Restore(IReadOnlyCollection<CabeceraChangeCategory> categories)
        {
            var standard = Standard();
            if (standard == null) return CabeceraEdit.Refuse("No se pudo calcular el estándar.");
            if (categories == null || categories.Count == 0) return CabeceraEdit.Refuse("Marca lo que quieres restaurar.");

            if (categories.Contains(CabeceraChangeCategory.Horizontals))
            {
                var keepPanels = !categories.Contains(CabeceraChangeCategory.Panels);
                var oldPanels = _config.BracingPanels.Select(CopyPanel).ToList();
                var oldHorizontals = _config.Horizontals.Select(CopyHorizontal).ToList();
                _config.Horizontals.Clear();
                foreach (var h in standard.Horizontals) _config.Horizontals.Add(CopyHorizontal(h));
                KeepNotes(oldHorizontals, _config); // correction 3: the notes stay where their elevation stays
                Normalize(_config, null);
                for (var i = 0; i < _config.BracingPanels.Count; i++)
                {
                    var panel = _config.BracingPanels[i];
                    var std = StandardPanelAt(standard, panel.StartElevation, panel.EndElevation);
                    var old = keepPanels ? oldPanels.FirstOrDefault(p => Near(p.StartElevation, panel.StartElevation) && Near(p.EndElevation, panel.EndElevation)) : null;
                    var source = old ?? std;
                    if (source != null) CopyPanelValues(source, panel);
                }
            }
            else if (categories.Contains(CabeceraChangeCategory.Panels))
            {
                foreach (var panel in _config.BracingPanels)
                {
                    var std = StandardPanelAt(standard, panel.StartElevation, panel.EndElevation);
                    CopyPanelValues(std ?? DefaultPanel(), panel);
                }
            }

            if (categories.Contains(CabeceraChangeCategory.Posts))
            {
                CopySide(standard.LeftPost, _config.LeftPost, PostSide.Left);
                CopySide(standard.RightPost, _config.RightPost, PostSide.Right);
            }

            if (categories.Contains(CabeceraChangeCategory.Plates))
            {
                CopyPlate(standard.LeftBasePlate, _config.LeftBasePlate, PostSide.Left);
                CopyPlate(standard.RightBasePlate, _config.RightBasePlate, PostSide.Right);
            }

            if (categories.Contains(CabeceraChangeCategory.Celosia))
            {
                _config.CelosiaStartTroquel = standard.CelosiaStartTroquel;
                _config.DiagonalStartOffsetTroqueles = standard.DiagonalStartOffsetTroqueles;
                _config.DiagonalEndOffsetTroqueles = standard.DiagonalEndOffsetTroqueles;
            }

            Normalize(_config, null);
            return CabeceraEdit.Done("Estándar restaurado en lo marcado.");
        }

        // ------------------------------------------------------------------ labels (Spanish, CB-06 of the inventory: dolor 6)

        public static string ArrangementLabel(BracingPattern arrangement)
        {
            switch (arrangement)
            {
                case BracingPattern.NoBracing: return "Sin celosía";
                case BracingPattern.SingleDiagonal: return "Diagonal simple";
                case BracingPattern.DoubleDiagonal: return "Diagonal doble";
                case BracingPattern.XBracing: return "Celosía en X";
                case BracingPattern.KBracing: return "Celosía en K";
                default: return "Personalizado";
            }
        }

        public static string DirectionLabel(DiagonalDirection direction)
        {
            switch (direction)
            {
                case DiagonalDirection.UpRight: return "Sube a la derecha";
                case DiagonalDirection.UpLeft: return "Sube a la izquierda";
                default: return "Auto (alterna)";
            }
        }

        public static string CategoryLabel(CabeceraChangeCategory category)
        {
            switch (category)
            {
                case CabeceraChangeCategory.Horizontals: return "Horizontales";
                case CabeceraChangeCategory.Panels: return "Paneles";
                case CabeceraChangeCategory.Posts: return "Postes y refuerzos";
                case CabeceraChangeCategory.Plates: return "Placas";
                default: return "Celosía";
            }
        }

        /// <summary>The label of a catalog id (its display name, else its description, else the id).</summary>
        public string LabelOf(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return "—";
            var text = _catalog.DescribeId(id);
            return string.IsNullOrWhiteSpace(text) ? id : text.Trim();
        }

        public static string Num(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);

        public static string Inches(double value) => Num(value) + " in";

        // ------------------------------------------------------------------ internals

        private CabeceraEdit Regenerate(string templateId, double height, string notice)
        {
            var built = Build(templateId, height);
            if (built == null) return CabeceraEdit.Refuse("No se pudo generar la cabecera con esa plantilla, ese alto y esa profundidad.");
            built.Name = _config.Name;
            built.PostPeralte = _config.PostPeralte;
            KeepNotes(_config.Horizontals, built); // correction 3 of the review: the notes stay where their elevation stays
            _config = built;
            Normalize(_config, null);
            _templateId = TemplateIdOf(_config) ?? templateId;
            _generatedHeight = height;
            return CabeceraEdit.Done(notice);
        }

        private RackFrameConfiguration Build(string templateId, double height)
        {
            var template = FindTemplate(templateId) ?? _templates[0];
            var depth = _config.Depth > 0.0 ? _config.Depth : template.DefaultDepth;
            if (!(height > 0.0) || !(depth > 0.0)) return null;
            try
            {
                var built = new RackFrameConfigurationFactory(_catalog).Build(template, _config.LeftPost?.PostCatalogId, height, depth);
                built.PostPeralte = _config.PostPeralte;
                ImposeContext(built);
                Normalize(built, null);
                return built;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private void ImposeContext(RackFrameConfiguration config)
        {
            if (Context.LockedDepth.HasValue && Context.LockedDepth.Value > 0.0) config.Depth = Context.LockedDepth.Value;
            if (Context.LockedPostPeralte.HasValue) config.PostPeralte = Context.LockedPostPeralte.Value;
            else if (Context.Embedded && !(config.PostPeralte > 0.0) && Context.RackPostPeralte > 0.0) config.PostPeralte = Context.RackPostPeralte;
        }

        private RackFrameTemplate FindTemplate(string id)
            => string.IsNullOrWhiteSpace(id) ? null : _templates.FirstOrDefault(t => string.Equals(t.Id, id.Trim(), StringComparison.OrdinalIgnoreCase));

        private string TemplateIdOf(RackFrameConfiguration config)
        {
            var baseline = config?.StandardBaselineId ?? string.Empty;
            var id = baseline.StartsWith(TemplatePrefix, StringComparison.OrdinalIgnoreCase) ? baseline.Substring(TemplatePrefix.Length) : baseline;
            var found = FindTemplate(id);
            return (found ?? RackFrameTemplateCatalog.FindById(RackFrameTemplateCatalog.StandardTemplateId) ?? _templates[0]).Id;
        }

        private static double HeightOf(RackFrameConfiguration config)
        {
            var top = config.Horizontals.Count == 0 ? 0.0 : config.Horizontals.Max(h => h.Elevation);
            return top > 0.0 ? Round(top + RackFrameConfigurationFactory.PostTopRemate) : config.Height;
        }

        private static List<FrameHorizontal> Ordered(RackFrameConfiguration config)
            => config.Horizontals.OrderBy(h => h.Elevation).ThenBy(h => h.Number).ToList();

        // The rules of the classic configurator (NormalizeHorizontalsAndPanels + SyncHeightToHorizontals), with one difference: a panel
        // belongs to its LOWER horizontal (the prototype), so moving or adding horizontals never moves the values of a panel elsewhere.
        private void Normalize(RackFrameConfiguration config, IDictionary<FrameHorizontal, BracingPanel> inherited)
        {
            var oldOrdered = Ordered(config);
            var byLower = new Dictionary<FrameHorizontal, BracingPanel>();
            var byId = oldOrdered.GroupBy(h => h.Id ?? string.Empty).ToDictionary(g => g.Key, g => g.First());
            foreach (var panel in config.BracingPanels)
            {
                if (panel?.LowerHorizontalId != null && byId.TryGetValue(panel.LowerHorizontalId, out var lower) && !byLower.ContainsKey(lower))
                    byLower[lower] = CopyPanel(panel);
            }

            var ordered = Ordered(config);
            config.Horizontals.Clear();
            for (var i = 0; i < ordered.Count; i++)
            {
                ordered[i].Id = "H" + (i + 1).ToString(CultureInfo.InvariantCulture);
                ordered[i].Number = i + 1;
                config.Horizontals.Add(ordered[i]);
            }

            config.BracingPanels.Clear();
            for (var i = 0; i < ordered.Count - 1; i++)
            {
                var lower = ordered[i];
                var upper = ordered[i + 1];
                BracingPanel source = null;
                if (inherited != null && inherited.TryGetValue(lower, out var inherit)) source = inherit;
                else if (byLower.TryGetValue(lower, out var own)) source = own;
                var panel = source != null ? CopyPanel(source) : DefaultPanel();
                panel.Number = i + 1;
                panel.PanelId = "P" + (i + 1).ToString(CultureInfo.InvariantCulture);
                panel.LowerHorizontalId = lower.Id;
                panel.UpperHorizontalId = upper.Id;
                panel.StartElevation = lower.Elevation;
                panel.EndElevation = upper.Elevation;
                config.BracingPanels.Add(panel);
            }

            var top = ordered.Count == 0 ? 0.0 : ordered[ordered.Count - 1].Elevation;
            if (top > 0.0) config.Height = Round(top + RackFrameConfigurationFactory.PostTopRemate);
        }

        private BracingPanel DefaultPanel() => new BracingPanel
        {
            Arrangement = BracingPattern.SingleDiagonal,
            MountingFace = FrameSide.Front,
            DiagonalDirection = DiagonalDirection.AutoAlternating,
            DiagonalProfileId = DefaultDiagonalProfile(),
            StartConnectionPointId = _catalog.Defaults?.BraceStartConnectionPoint,
            EndConnectionPointId = _catalog.Defaults?.BraceEndConnectionPoint,
            IsStandard = false,
            IsException = true,
        };

        private string DefaultDiagonalProfile()
            => _config?.BracingPanels.Select(p => p.DiagonalProfileId).FirstOrDefault(id => !string.IsNullOrWhiteSpace(id)) ?? _catalog.Defaults?.DiagonalProfile;

        private string DefaultHorizontalProfile()
            => _config?.Horizontals.Select(h => h.ProfileId).FirstOrDefault(id => !string.IsNullOrWhiteSpace(id)) ?? _catalog.Defaults?.HorizontalProfile;

        private static FrameHorizontal NewHorizontal(double elevation, string profileId) => new FrameHorizontal
        {
            Elevation = elevation,
            ProfileId = profileId,
            Quantity = 1,
            MountingFace = FrameSide.Front,
            State = FrameComponentState.Manual,
            IsStandard = false,
        };

        private static FrameHorizontal CopyHorizontal(FrameHorizontal source) => new FrameHorizontal
        {
            Id = source.Id,
            Number = source.Number,
            Elevation = source.Elevation,
            ProfileId = source.ProfileId,
            Quantity = source.Quantity,
            MountingFace = source.MountingFace,
            State = source.State,
            Notes = source.Notes,
            IsStandard = source.IsStandard,
        };

        private static BracingPanel CopyPanel(BracingPanel source)
        {
            var copy = new BracingPanel
            {
                PanelId = source.PanelId,
                Number = source.Number,
                LowerHorizontalId = source.LowerHorizontalId,
                UpperHorizontalId = source.UpperHorizontalId,
                StartElevation = source.StartElevation,
                EndElevation = source.EndElevation,
            };
            CopyPanelValues(source, copy);
            return copy;
        }

        private static void CopyPanelValues(BracingPanel source, BracingPanel target)
        {
            target.Arrangement = source.Arrangement;
            target.MountingFace = source.MountingFace;
            target.DiagonalProfileId = source.DiagonalProfileId;
            target.DiagonalDirection = source.DiagonalDirection;
            target.StartConnectionPointId = source.StartConnectionPointId;
            target.EndConnectionPointId = source.EndConnectionPointId;
            target.IsStandard = source.IsStandard;
            target.IsException = source.IsException;
        }

        private static void CopySide(PostAssembly source, PostAssembly target, PostSide side)
        {
            if (source == null || target == null) return;
            target.Side = side;
            target.PostCatalogId = source.PostCatalogId;
            target.Description = source.Description;
            target.HasReinforcement = source.HasReinforcement;
            target.ReinforcementCatalogId = source.ReinforcementCatalogId;
            target.ReinforcementHeight = source.ReinforcementHeight;
        }

        private static void CopyPlate(BasePlatePlacement source, BasePlatePlacement target, PostSide side)
        {
            if (source == null || target == null) return;
            target.PostSide = side;
            target.PlateCatalogId = source.PlateCatalogId;
            target.Description = source.Description;
            target.ConnectionPointId = source.ConnectionPointId;
            target.PeralteOverride = source.PeralteOverride;
        }

        private CabeceraEdit EditPanels(IEnumerable<int> panels, Action<BracingPanel> write)
        {
            var chosen = Valid(panels, _config.BracingPanels.Count);
            if (chosen.Count == 0) return CabeceraEdit.Refuse("Elige uno o más paneles.");
            foreach (var index in chosen) write(_config.BracingPanels[index]);
            return CabeceraEdit.Done();
        }

        private CabeceraEdit EditHorizontals(IEnumerable<int> horizontals, Action<FrameHorizontal> write)
        {
            var chosen = Valid(horizontals, _config.Horizontals.Count);
            if (chosen.Count == 0) return CabeceraEdit.Refuse("Elige una o más horizontales.");
            foreach (var index in chosen) write(_config.Horizontals[index]);
            return CabeceraEdit.Done();
        }

        private CabeceraEdit EditSides(IEnumerable<PostSide> sides, Action<PostAssembly, BasePlatePlacement> write)
        {
            var chosen = (sides ?? Enumerable.Empty<PostSide>()).Distinct().ToList();
            if (chosen.Count == 0) return CabeceraEdit.Refuse("Elige un poste.");
            foreach (var side in chosen)
            {
                if (side == PostSide.Left)
                {
                    _config.LeftPost ??= new PostAssembly { Side = PostSide.Left };
                    _config.LeftBasePlate ??= new BasePlatePlacement { PostSide = PostSide.Left };
                    write(_config.LeftPost, _config.LeftBasePlate);
                }
                else
                {
                    _config.RightPost ??= new PostAssembly { Side = PostSide.Right };
                    _config.RightBasePlate ??= new BasePlatePlacement { PostSide = PostSide.Right };
                    write(_config.RightPost, _config.RightBasePlate);
                }
            }

            return CabeceraEdit.Done();
        }

        private static List<int> Valid(IEnumerable<int> indices, int count)
            => (indices ?? Enumerable.Empty<int>()).Where(i => i >= 0 && i < count).Distinct().OrderBy(i => i).ToList();

        private string DescriptionOf(string postId, string fallback)
        {
            var profile = _catalog.PostProfiles.FindProfile(postId);
            return string.IsNullOrWhiteSpace(profile?.Description) ? (string.IsNullOrWhiteSpace(fallback) ? postId : fallback) : profile.Description.Trim();
        }

        private static BracingPanel StandardPanelAt(RackFrameConfiguration standard, double start, double end)
            => standard.BracingPanels.FirstOrDefault(p => Near(p.StartElevation, start) && Near(p.EndElevation, end));

        private static bool SamePanel(BracingPanel a, BracingPanel b)
            => a.Arrangement == b.Arrangement && a.DiagonalDirection == b.DiagonalDirection && Same(a.DiagonalProfileId, b.DiagonalProfileId)
               && Same(a.StartConnectionPointId, b.StartConnectionPointId) && Same(a.EndConnectionPointId, b.EndConnectionPointId);

        private static bool SameSide(PostAssembly a, PostAssembly b)
            => a == null || b == null ? a == b
                : Same(a.PostCatalogId, b.PostCatalogId) && string.Equals(a.Description ?? string.Empty, b.Description ?? string.Empty, StringComparison.Ordinal)
                  && a.HasReinforcement == b.HasReinforcement && Same(a.ReinforcementCatalogId, b.ReinforcementCatalogId)
                  && Math.Abs(a.ReinforcementHeight - b.ReinforcementHeight) < 1e-9;

        private static bool SamePlate(BasePlatePlacement a, BasePlatePlacement b)
            => a == null || b == null ? a == b
                : Same(a.PlateCatalogId, b.PlateCatalogId) && Same(a.ConnectionPointId, b.ConnectionPointId) && Nullable.Equals(a.PeralteOverride, b.PeralteOverride);

        private IReadOnlyList<CabeceraChange> Compare(RackFrameConfiguration own, RackFrameConfiguration standard)
        {
            var list = new List<CabeceraChange>();
            var hs = own.Horizontals;
            var shs = standard.Horizontals;

            void HorizontalProps(FrameHorizontal h, FrameHorizontal o, string piece)
            {
                if (!Same(h.ProfileId, o.ProfileId)) list.Add(new CabeceraChange(CabeceraChangeCategory.Horizontals, piece, "perfil " + LabelOf(o.ProfileId) + " → " + LabelOf(h.ProfileId)));
                if (h.Quantity != o.Quantity) list.Add(new CabeceraChange(CabeceraChangeCategory.Horizontals, piece, "cantidad " + o.Quantity + " → " + h.Quantity));
            }

            if (hs.Count == shs.Count)
            {
                for (var i = 0; i < hs.Count; i++)
                {
                    var piece = "H" + (i + 1);
                    if (!Near(hs[i].Elevation, shs[i].Elevation))
                        list.Add(new CabeceraChange(CabeceraChangeCategory.Horizontals, piece, "elevación " + Num(shs[i].Elevation) + " → " + Num(hs[i].Elevation) + " in"));
                    HorizontalProps(hs[i], shs[i], piece);
                }
            }
            else
            {
                for (var i = 0; i < hs.Count; i++)
                {
                    var match = shs.FirstOrDefault(o => Near(o.Elevation, hs[i].Elevation));
                    if (match == null) list.Add(new CabeceraChange(CabeceraChangeCategory.Horizontals, "H" + (i + 1), "agregada a " + Inches(hs[i].Elevation)));
                    else HorizontalProps(hs[i], match, "H" + (i + 1));
                }

                foreach (var o in shs.Where(o => !hs.Any(h => Near(h.Elevation, o.Elevation))))
                    list.Add(new CabeceraChange(CabeceraChangeCategory.Horizontals, "Horizontal", "la del estándar a " + Inches(o.Elevation) + " ya no está"));
            }

            for (var i = 0; i < own.BracingPanels.Count; i++)
            {
                var p = own.BracingPanels[i];
                var piece = "P" + (i + 1);
                var o = StandardPanelAt(standard, p.StartElevation, p.EndElevation);
                if (o == null)
                {
                    list.Add(new CabeceraChange(CabeceraChangeCategory.Panels, piece, "panel nuevo de " + Inches(p.EndElevation - p.StartElevation) + " (" + Lower(ArrangementLabel(p.Arrangement)) + ")"));
                    continue;
                }

                if (p.Arrangement != o.Arrangement) list.Add(new CabeceraChange(CabeceraChangeCategory.Panels, piece, Lower(ArrangementLabel(o.Arrangement)) + " → " + Lower(ArrangementLabel(p.Arrangement))));
                if (p.DiagonalDirection != o.DiagonalDirection) list.Add(new CabeceraChange(CabeceraChangeCategory.Panels, piece, "dirección " + Lower(DirectionLabel(o.DiagonalDirection)) + " → " + Lower(DirectionLabel(p.DiagonalDirection))));
                if (!Same(p.DiagonalProfileId, o.DiagonalProfileId)) list.Add(new CabeceraChange(CabeceraChangeCategory.Panels, piece, "perfil de la diagonal " + LabelOf(p.DiagonalProfileId)));
                if (!Same(p.StartConnectionPointId, o.StartConnectionPointId)) list.Add(new CabeceraChange(CabeceraChangeCategory.Panels, piece, "punto de inicio " + LabelOf(p.StartConnectionPointId)));
                if (!Same(p.EndConnectionPointId, o.EndConnectionPointId)) list.Add(new CabeceraChange(CabeceraChangeCategory.Panels, piece, "punto de fin " + LabelOf(p.EndConnectionPointId)));
            }

            foreach (var (a, o, name, plate, oplate, plateName) in new[]
            {
                (own.LeftPost, standard.LeftPost, "Poste izquierdo", own.LeftBasePlate, standard.LeftBasePlate, "Placa izquierda"),
                (own.RightPost, standard.RightPost, "Poste derecho", own.RightBasePlate, standard.RightBasePlate, "Placa derecha"),
            })
            {
                if (a != null && o != null)
                {
                    if (!Same(a.PostCatalogId, o.PostCatalogId)) list.Add(new CabeceraChange(CabeceraChangeCategory.Posts, name, "perfil " + LabelOf(a.PostCatalogId)));
                    if (!string.Equals((a.Description ?? string.Empty).Trim(), (o.Description ?? string.Empty).Trim(), StringComparison.Ordinal))
                        list.Add(new CabeceraChange(CabeceraChangeCategory.Posts, name, "descripción «" + (a.Description ?? string.Empty).Trim() + "»"));
                    if (a.HasReinforcement != o.HasReinforcement)
                        list.Add(new CabeceraChange(CabeceraChangeCategory.Posts, name, "refuerzo " + (o.HasReinforcement ? "sí" : "no") + " → " + (a.HasReinforcement ? "sí, " + Inches(a.ReinforcementHeight) : "no")));
                    else if (a.HasReinforcement && Math.Abs(a.ReinforcementHeight - o.ReinforcementHeight) > 1e-9)
                        list.Add(new CabeceraChange(CabeceraChangeCategory.Posts, name, "altura del refuerzo " + Num(o.ReinforcementHeight) + " → " + Inches(a.ReinforcementHeight)));
                    if (a.HasReinforcement && o.HasReinforcement && !Same(a.ReinforcementCatalogId, o.ReinforcementCatalogId))
                        list.Add(new CabeceraChange(CabeceraChangeCategory.Posts, name, "tipo de refuerzo " + LabelOf(a.ReinforcementCatalogId)));
                }

                if (plate != null && oplate != null)
                {
                    if (!Same(plate.PlateCatalogId, oplate.PlateCatalogId)) list.Add(new CabeceraChange(CabeceraChangeCategory.Plates, plateName, "placa " + LabelOf(plate.PlateCatalogId)));
                    if (!Nullable.Equals(plate.PeralteOverride, oplate.PeralteOverride))
                        list.Add(new CabeceraChange(CabeceraChangeCategory.Plates, plateName, "peralte " + (oplate.PeralteOverride.HasValue ? Inches(oplate.PeralteOverride.Value) : "del poste") + " → " + (plate.PeralteOverride.HasValue ? Inches(plate.PeralteOverride.Value) : "del poste")));
                    if (!Same(plate.ConnectionPointId, oplate.ConnectionPointId)) list.Add(new CabeceraChange(CabeceraChangeCategory.Plates, plateName, "punto de montaje " + LabelOf(plate.ConnectionPointId)));
                }
            }

            if (own.CelosiaStartTroquel != standard.CelosiaStartTroquel)
                list.Add(new CabeceraChange(CabeceraChangeCategory.Celosia, "Celosía", "inicio (troquel) " + standard.CelosiaStartTroquel + " → " + own.CelosiaStartTroquel));
            if (own.DiagonalStartOffsetTroqueles != standard.DiagonalStartOffsetTroqueles)
                list.Add(new CabeceraChange(CabeceraChangeCategory.Celosia, "Celosía", "separación de la diagonal abajo " + standard.DiagonalStartOffsetTroqueles + " → " + own.DiagonalStartOffsetTroqueles));
            if (own.DiagonalEndOffsetTroqueles != standard.DiagonalEndOffsetTroqueles)
                list.Add(new CabeceraChange(CabeceraChangeCategory.Celosia, "Celosía", "separación de la diagonal arriba " + standard.DiagonalEndOffsetTroqueles + " → " + own.DiagonalEndOffsetTroqueles));
            return list;
        }

        private static string Serialize(RackFrameConfiguration config) => Store.Serialize(config);

        private static bool Same(string a, string b) => string.Equals((a ?? string.Empty).Trim(), (b ?? string.Empty).Trim(), StringComparison.OrdinalIgnoreCase);

        private static bool Near(double a, double b) => Math.Abs(a - b) < Tolerance;

        private static double Round(double value) => Math.Round(value, 4);

        private static string Lower(string text) => string.IsNullOrEmpty(text) ? text : char.ToLowerInvariant(text[0]) + text.Substring(1);

        private static string Text(string value) => value ?? string.Empty;
    }
}
