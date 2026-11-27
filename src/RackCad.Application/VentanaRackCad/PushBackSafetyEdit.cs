using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>
    /// The copy of the safety of a Push Back draft that «Extremos y seguridad» of the RackCad window edits (T5 pb, PB-S03): the safety list of
    /// the rack and the type of defense of each side, which lives in the state of the side (I-42 ronda 7E). It carries what the sections of the
    /// classic dialog read to seed themselves: the bota RESOLVED by the last valid recomposition (it knows the automatic of each side), the
    /// posts and whether the rack has a side B. The caller replaces the safety of its draft with the copy and recomposes.
    /// </summary>
    public sealed class PushBackSafetyCopy
    {
        public PushBackSafetyCopy(IEnumerable<SelectiveSafetySelection> safety, string defensePieceA, string defensePieceB, bool twoSided, int postCount,
            SelectiveSafetySelection resolvedBota, IReadOnlyList<bool> facesA = null, IReadOnlyList<bool> facesB = null)
        {
            Safety = (safety ?? Enumerable.Empty<SelectiveSafetySelection>()).Select(s => s?.DeepCopy()).ToList();
            DefensePieceA = defensePieceA;
            DefensePieceB = defensePieceB;
            TwoSided = twoSided;
            PostCount = Math.Max(1, postCount);
            ResolvedBota = resolvedBota?.DeepCopy();
            _facesA = facesA?.ToList();
            _facesB = facesB?.ToList();
        }

        private readonly List<bool> _facesA;
        private readonly List<bool> _facesB;

        /// <summary>
        /// Whether the side has a face of attack at that line (<see cref="PushBackDefenseSides.FacesOf"/> on the resolved structure: the grid of
        /// the side offers only those rows). True when it is not known, as the classic grid (fail-open).
        /// </summary>
        public bool HasFace(PushBackSide side, int post)
        {
            var faces = side == PushBackSide.B ? _facesB : _facesA;
            return faces == null || post < 0 || post >= faces.Count || faces[post];
        }

        /// <summary>The safety list of the rack (a deep copy: editing it touches nothing of the draft).</summary>
        public List<SelectiveSafetySelection> Safety { get; }

        /// <summary>The type of defense of side A (a catalog id, «(ninguno)» or null: the one of the selection).</summary>
        public string DefensePieceA { get; set; }

        /// <summary>The type of defense of side B; written only when the rack has a side B, as the classic dialog.</summary>
        public string DefensePieceB { get; set; }

        public bool TwoSided { get; }

        /// <summary>The posts of the rack, as the classic dialog counts them: max(2, fronts + 1).</summary>
        public int PostCount { get; }

        /// <summary>The bota of the last valid recomposition, which the classic section of each side seeds from; null when there is none.</summary>
        public SelectiveSafetySelection ResolvedBota { get; }

        public IReadOnlyList<PushBackSide> Sides => TwoSided ? new[] { PushBackSide.A, PushBackSide.B } : new[] { PushBackSide.A };
    }

    /// <summary>
    /// The bota of ONE side as the classic section holds it (<c>PushBackBootSection</c>, I-42 S1E/S1G): its type (a catalog id or «(ninguno)»),
    /// its general (chosen, or the automatic one shown) and the posts with a decision of their own.
    /// </summary>
    public sealed class PushBackBootSideDraft
    {
        internal PushBackBootSideDraft(PushBackSide side)
        {
            Side = side;
        }

        public PushBackSide Side { get; }

        /// <summary>The type of the side, as its list leaves it: a catalog id or <see cref="PushBackDefaults.NonePieceId"/>.</summary>
        public string PieceId { get; internal set; }

        /// <summary>True when somebody chose the general; false = the side keeps inheriting its automatic one.</summary>
        public bool Explicit { get; internal set; }

        /// <summary>The general the list shows: the one chosen, or the automatic one when nobody chose.</summary>
        public BootPlacement Placement { get; internal set; }

        public List<BootPostPlacement> Posts { get; } = new List<BootPostPlacement>();

        public bool IsNone => PushBackDefenseSides.IsNone(PieceId);

        /// <summary>The decision of a post of its own; null when it takes the general.</summary>
        public BootPlacement? OwnAt(int post) => Posts.FirstOrDefault(p => p != null && p.PostIndex == post)?.Placement;

        /// <summary>What the classic host writes for the side when the dialog is accepted (<c>PushBackBootSection.ToConfig</c>).</summary>
        public SelectiveBotaConfig ToConfig()
        {
            var config = new SelectiveBotaConfig { PieceId = PieceId, Placement = Explicit ? Placement : (BootPlacement?)null };
            foreach (var post in Posts.Where(p => p != null && p.PostIndex >= 0))
            {
                config.Posts.Add(new BootPostPlacement { PostIndex = post.PostIndex, Placement = post.Placement });
            }

            return config;
        }
    }

    /// <summary>
    /// The defense of ONE side as the classic section holds it (<c>PushBackDefenseSection</c>, I-42 ronda 7D/7E): its type and its working copy
    /// of the records by post. In a rack of one way the section owns both ends (the historic grid of «Entrada/Salida» and «Posterior»).
    /// </summary>
    public sealed class PushBackDefenseSideDraft
    {
        internal PushBackDefenseSideDraft(PushBackSide side, bool ownsBothEnds, int postCount)
        {
            Side = side;
            OwnsBothEnds = ownsBothEnds;
            PostCount = Math.Max(1, postCount);
        }

        public PushBackSide Side { get; }

        public bool OwnsBothEnds { get; }

        public int PostCount { get; }

        /// <summary>The type of the side, as its list leaves it: a catalog id or <see cref="PushBackDefaults.NonePieceId"/>.</summary>
        public string PieceId { get; internal set; }

        public List<SafetyPostDefense> Posts { get; internal set; } = new List<SafetyPostDefense>();

        public bool IsNone => PushBackDefenseSides.IsNone(PieceId);
    }

    /// <summary>
    /// The edits of the safety of a Push Back in the RackCad window (I-64, quinta tanda, part «pb», PB-S03; SG-09: Push Back adopts the table and
    /// the inspector of the fourth batch WITHOUT touching its safety authority). Each edit writes what the classic dialog «Elementos de
    /// seguridad» of Push Back writes for the same choice (<c>RackPushBackSystemWindow.Safety_Click</c>), family by family:
    /// <list type="bullet">
    /// <item>Botas: the two sections by side (<c>BuildBootSection</c> and <c>ApplyBootSections</c>): the selection is rebuilt with its carrier,
    /// the configuration of each side whole and <see cref="SelectiveSafetySelection.BootSidesDeclared"/>.</item>
    /// <item>Defensa: the two sections by side (<c>BuildDefenseSection</c>, <c>ApplyDefenseSections</c> and <c>EnsureDefenseSelection</c>): the
    /// type of each side in its state, and the records by post merged face by face (<see cref="PushBackDefenseSides.Merge"/>).</item>
    /// <item>Protector lateral: the per-post window and the «Aceptar» of the dialog (<c>SafetyPerPostWindow</c>, <c>OnOk</c>): the posts in
    /// their order, no general side.</item>
    /// <item>Desviador: the grid (<c>SafetyDesviadorGridWindow</c>): its cells by post (PB-002), the dormant ones of a blank front kept, its
    /// measures validated, the side it returns (Left: no selector, PB-003). The variant of an existing one is the row of the dialog, without
    /// the grid.</item>
    /// </list>
    /// What differs is declared in construido.md: an edit writes ONE family (the classic «Aceptar» rebuilds them all), the order of the list
    /// stays, and a family left with nothing keeps its variant (C2-07). Pure: no WPF and no AutoCAD.
    /// </summary>
    public static class PushBackSafetyEdit
    {
        /// <summary>The four placements of a bota, in the order of the classic list («Ninguno», «Entrada/Salida», «Posterior», «Ambas»).</summary>
        public static readonly IReadOnlyList<BootPlacement> Placements = new[] { BootPlacement.None, BootPlacement.EntryExit, BootPlacement.Rear, BootPlacement.Both };

        public static SelectiveSafetySelection Of(IEnumerable<SelectiveSafetySelection> safety, RackCatalog catalog, string type)
            => SelectiveSafetyFamilies.SelectedOfType(safety, catalog?.SafetyElements, type);

        public static IReadOnlyList<SafetyElementCatalogEntry> Variants(RackCatalog catalog, string type)
            => SelectiveSafetyFamilies.VariantsOfType(catalog?.SafetyElements, type);

        /// <summary>
        /// What the list of a type of the classic sections leaves selected for an id (<c>CatalogCombo.SetCatalogEntries</c> with the «Ninguno»
        /// placeholder): the catalog id when it is one of its variants (case and spaces as the catalog has them), else «(ninguno)».
        /// </summary>
        public static string TypeListId(RackCatalog catalog, string type, string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return PushBackDefaults.NonePieceId;
            var trimmed = id.Trim();
            if (string.Equals(trimmed, PushBackDefaults.NonePieceId, StringComparison.OrdinalIgnoreCase)) return PushBackDefaults.NonePieceId;
            var variant = Variants(catalog, type).FirstOrDefault(v => string.Equals(v.Id.Trim(), trimmed, StringComparison.OrdinalIgnoreCase));
            return variant == null ? PushBackDefaults.NonePieceId : variant.Id.Trim();
        }

        // ================================================================== the botas, by side (I-42 S1E/S1G)

        /// <summary>
        /// The section of a side as the classic dialog builds it (<c>BuildBootSection</c> and the constructor of <c>PushBackBootSection</c>): it
        /// seeds from the RESOLVED bota when there is one (it carries the automatic of the side), its general is the chosen one or the
        /// automatic, its posts are its own in range plus, on side A, the historic matrix by post where it has none of its own.
        /// </summary>
        public static PushBackBootSideDraft BootSide(PushBackSafetyCopy copy, RackCatalog catalog, PushBackSide side)
        {
            if (copy == null) throw new ArgumentNullException(nameof(copy));
            var selection = copy.ResolvedBota ?? Of(copy.Safety, catalog, SelectiveSafetyDefaults.BotaType);
            var config = selection == null ? null : side == PushBackSide.B ? selection.BotaB : selection.Bota;
            var source = config?.DeepCopy() ?? new SelectiveBotaConfig();
            var draft = new PushBackBootSideDraft(side)
            {
                Explicit = source.Placement.HasValue,
                Placement = source.Placement ?? source.Automatic ?? BootPlacement.EntryExit,
            };

            var posts = copy.PostCount;
            foreach (var post in source.Posts.Where(p => p != null && p.PostIndex >= 0 && p.PostIndex < posts))
            {
                draft.Posts.Add(new BootPostPlacement { PostIndex = post.PostIndex, Placement = post.Placement });
            }

            if (side == PushBackSide.A)
            {
                foreach (var legacy in selection?.PostSides ?? Enumerable.Empty<SafetyPostSide>())
                {
                    if (legacy != null && legacy.PostIndex >= 0 && legacy.PostIndex < posts && !draft.Posts.Any(p => p.PostIndex == legacy.PostIndex))
                    {
                        draft.Posts.Add(new BootPostPlacement { PostIndex = legacy.PostIndex, Placement = BootPlacements.From(legacy.Side) });
                    }
                }
            }

            var piece = selection?.BootPieceOf(config);
            draft.PieceId = TypeListId(catalog, SelectiveSafetyDefaults.BotaType, PushBackDefenseSides.IsNone(piece) ? PushBackDefaults.NonePieceId : piece);
            return draft;
        }

        /// <summary>
        /// One gesture on the botas of one side, written as the classic «Aceptar» writes the family (<c>ApplyBootSections</c>): the sections of
        /// every side are built, the gesture changes the one of <paramref name="side"/>, and the selection is rebuilt with its carrier (the first
        /// type a side chose, else the first variant of the catalog), Quantity 1, no general side, the configuration of each side whole, an
        /// empty side B in a rack of one way, and the intent declared by side. It replaces every selection of the family, in the place of the
        /// first one. Returns false when the catalog has no bota (there is nothing to configure).
        /// </summary>
        public static bool WriteBoots(PushBackSafetyCopy copy, RackCatalog catalog, PushBackSide side, Action<PushBackBootSideDraft> gesture)
        {
            if (copy == null) throw new ArgumentNullException(nameof(copy));
            if (side == PushBackSide.B && !copy.TwoSided) throw new InvalidOperationException("Este rack no tiene lado B.");
            var a = BootSide(copy, catalog, PushBackSide.A);
            var b = copy.TwoSided ? BootSide(copy, catalog, PushBackSide.B) : null;
            gesture?.Invoke(side == PushBackSide.B ? b : a);

            var carrier = new[] { a, b }.Where(s => s != null && !s.IsNone && !string.IsNullOrWhiteSpace(s.PieceId)).Select(s => s.PieceId).FirstOrDefault()
                          ?? Variants(catalog, SelectiveSafetyDefaults.BotaType).FirstOrDefault()?.Id
                          ?? Of(copy.Safety, catalog, SelectiveSafetyDefaults.BotaType)?.ElementId;
            if (carrier == null) return false;

            var rebuilt = new SelectiveSafetySelection { ElementId = carrier, Quantity = 1, Side = SafetySide.None };
            rebuilt.Bota = a.ToConfig();
            rebuilt.BotaB = b != null ? b.ToConfig() : new SelectiveBotaConfig();
            rebuilt.BootSidesDeclared = true;
            ReplaceFamily(copy.Safety, catalog, SelectiveSafetyDefaults.BotaType, rebuilt);
            return true;
        }

        /// <summary>The type of the botas of a side: a variant, or «(ninguno)» (the side keeps its placement and its posts, dormant).</summary>
        public static bool SetBootPiece(PushBackSafetyCopy copy, RackCatalog catalog, PushBackSide side, string pieceId)
            => WriteBoots(copy, catalog, side, s => s.PieceId = TypeListId(catalog, SelectiveSafetyDefaults.BotaType, pieceId));

        /// <summary>
        /// The general placement of a side, as its list: choosing the value it already shows changes nothing (the list does not change), so a
        /// side that inherits its automatic keeps inheriting it; another value is chosen and stays.
        /// </summary>
        public static bool SetBootGeneral(PushBackSafetyCopy copy, RackCatalog catalog, PushBackSide side, BootPlacement placement)
            => WriteBoots(copy, catalog, side, s =>
            {
                if (s.Placement == placement) return;
                s.Placement = placement;
                s.Explicit = true;
            });

        /// <summary>
        /// The bota of one post of a side, as the per-post grid of the classic section (<c>SafetyPerPostWindow</c>): a placement of its own, or
        /// null to take the general again; the grid returns the posts with a decision, in their order.
        /// </summary>
        public static bool SetBootPost(PushBackSafetyCopy copy, RackCatalog catalog, PushBackSide side, int post, BootPlacement? placement)
            => WriteBoots(copy, catalog, side, s =>
            {
                var posts = s.Posts.Where(p => p.PostIndex != post).ToList();
                if (placement.HasValue && post >= 0 && post < copy.PostCount) posts.Add(new BootPostPlacement { PostIndex = post, Placement = placement.Value });
                s.Posts.Clear();
                s.Posts.AddRange(posts.OrderBy(p => p.PostIndex));
            });

        /// <summary>«Todos los postes al general» of a side: the per-post grid with every post on «(por defecto)».</summary>
        public static bool BootPostsToGeneral(PushBackSafetyCopy copy, RackCatalog catalog, PushBackSide side)
            => WriteBoots(copy, catalog, side, s => s.Posts.Clear());

        // ================================================================== the defense, by side (I-42 ronda 7D/7E)

        /// <summary>The type of defense of a side before its list resolves it (<c>DefensePieceIdOf</c>): the one the side keeps, or the selection's.</summary>
        public static string DefensePieceOf(PushBackSafetyCopy copy, RackCatalog catalog, PushBackSide side)
        {
            var stored = side == PushBackSide.B ? copy.DefensePieceB : copy.DefensePieceA;
            return string.IsNullOrWhiteSpace(stored) ? Of(copy.Safety, catalog, SelectiveSafetyDefaults.DefensaType)?.ElementId : stored;
        }

        /// <summary>The section of a side as the classic dialog builds it: the records of the family in range, and the type of the side.</summary>
        public static PushBackDefenseSideDraft DefenseSide(PushBackSafetyCopy copy, RackCatalog catalog, PushBackSide side)
        {
            if (copy == null) throw new ArgumentNullException(nameof(copy));
            var defensa = Of(copy.Safety, catalog, SelectiveSafetyDefaults.DefensaType);
            var draft = new PushBackDefenseSideDraft(side, !copy.TwoSided, copy.PostCount)
            {
                Posts = PushBackDefenseSides.Copy(defensa?.DefensaPosts).Where(r => r.PostIndex >= 0 && r.PostIndex < copy.PostCount).ToList(),
            };
            var piece = DefensePieceOf(copy, catalog, side);
            draft.PieceId = TypeListId(catalog, SelectiveSafetyDefaults.DefensaType, PushBackDefenseSides.IsNone(piece) ? PushBackDefaults.NonePieceId : piece);
            return draft;
        }

        /// <summary>
        /// One gesture on the defense of one side, written as the classic «Aceptar» (<c>ApplyDefenseSections</c>): the type of each side goes to
        /// its state; the family exists while a side has a type (or, with both in «Ninguno», only as the carrier of an intent by post); it is
        /// rebuilt with Quantity 1, no side, restricted to the low end; a rack of one way takes the records of its section, a composite merges
        /// each face from its own section, and a line the rack no longer has leaves no record. It replaces the family in the place of the first.
        /// </summary>
        public static void WriteDefense(PushBackSafetyCopy copy, RackCatalog catalog, PushBackSide side, Action<PushBackDefenseSideDraft> gesture)
        {
            if (copy == null) throw new ArgumentNullException(nameof(copy));
            if (side == PushBackSide.B && !copy.TwoSided) throw new InvalidOperationException("Este rack no tiene lado B.");
            var carrierId = Of(copy.Safety, catalog, SelectiveSafetyDefaults.DefensaType)?.ElementId;
            var a = DefenseSide(copy, catalog, PushBackSide.A);
            var b = copy.TwoSided ? DefenseSide(copy, catalog, PushBackSide.B) : null;
            gesture?.Invoke(side == PushBackSide.B ? b : a);

            copy.DefensePieceA = a.PieceId;
            if (b != null) copy.DefensePieceB = b.PieceId;

            var chosen = new[] { a, b }.Where(s => s != null && !s.IsNone).Select(s => s.PieceId).FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));
            if (string.IsNullOrWhiteSpace(chosen))
            {
                var hasIntent = a.Posts.Count > 0 || (b?.Posts.Count ?? 0) > 0;
                if (!hasIntent || string.IsNullOrWhiteSpace(carrierId))
                {
                    ReplaceFamily(copy.Safety, catalog, SelectiveSafetyDefaults.DefensaType, null);
                    return;
                }

                chosen = carrierId;
            }

            var rebuilt = new SelectiveSafetySelection { Quantity = 1, Side = SafetySide.None, ElementId = chosen };
            PushBackSafetyAuthority.RestrictToLowEnd(rebuilt);
            List<SafetyPostDefense> records;
            if (b == null)
            {
                records = PushBackDefenseSides.Copy(a.Posts);
            }
            else
            {
                records = PushBackDefenseSides.Merge(PushBackDefenseSides.Merge(new List<SafetyPostDefense>(), a.Posts, PushBackSide.A), b.Posts, PushBackSide.B);
                records.RemoveAll(r => r.PostIndex < 0 || r.PostIndex >= a.PostCount);
            }

            foreach (var record in records) rebuilt.DefensaPosts.Add(record);
            ReplaceFamily(copy.Safety, catalog, SelectiveSafetyDefaults.DefensaType, rebuilt);
        }

        /// <summary>The type of defense of a side: a variant, or «(ninguno)» (its records by post stay, dormant).</summary>
        public static void SetDefensePiece(PushBackSafetyCopy copy, RackCatalog catalog, PushBackSide side, string pieceId)
            => WriteDefense(copy, catalog, side, s => s.PieceId = TypeListId(catalog, SelectiveSafetyDefaults.DefensaType, pieceId));

        /// <summary>
        /// Whether a decision on the defense of a post has somewhere to be written (review of 01e76902, point 3): a side has a type, or the
        /// family is there to carry the intent by post with every side in «Ninguno». With no type and no family the classic «Aceptar» writes
        /// nothing for the posts (<c>ApplyDefenseSections</c> has no carrier), so the window offers no box to click there.
        /// </summary>
        public static bool DefenseHoldsPosts(PushBackSafetyCopy copy, RackCatalog catalog)
        {
            if (copy == null) throw new ArgumentNullException(nameof(copy));
            return copy.Sides.Any(side => !DefenseSide(copy, catalog, side).IsNone)
                   || !string.IsNullOrWhiteSpace(Of(copy.Safety, catalog, SelectiveSafetyDefaults.DefensaType)?.ElementId);
        }

        /// <summary>
        /// The box of one end of the defense of a post (the casilla of the classic grid): on is a length of its own, the one the rule gives that
        /// post (12 in at an edge post, 36 in at an intermediate one), as the classic grid proposes it; off is no defense at that end.
        /// <paramref name="farEnd"/> is the «Posterior» of a rack of one way; in a composite each side has its own face (A the near one).
        /// </summary>
        public static void SetDefenseOn(PushBackSafetyCopy copy, RackCatalog catalog, PushBackSide side, int post, bool farEnd, bool on)
            => SetDefenseEnd(copy, catalog, side, post, farEnd, on ? Proposed(post, copy.PostCount) : 0.0, auto: false);

        /// <summary>The length of one end: null (an empty box) is the automatic one (the «Auto» of the classic grid); a number is the post's own.</summary>
        public static void SetDefenseLength(PushBackSafetyCopy copy, RackCatalog catalog, PushBackSide side, int post, bool farEnd, double? length)
            => SetDefenseEnd(copy, catalog, side, post, farEnd, length ?? 0.0, auto: !length.HasValue);

        /// <summary>«Todas automáticas» of a side: its grid with every «Auto» marked (in a rack of one way, both ends of every post).</summary>
        public static void DefenseAllAutomatic(PushBackSafetyCopy copy, RackCatalog catalog, PushBackSide side)
            => WriteDefense(copy, catalog, side, s =>
            {
                if (s.OwnsBothEnds)
                {
                    s.Posts = new List<SafetyPostDefense>();
                    return;
                }

                // Declared: written on the records of the side itself. The face grid of the classic drops a row that became automatic at both
                // ends BEFORE it merges it, so there its «Auto» is lost when the other face is automatic too (the length stays).
                var result = PushBackDefenseSides.Copy(s.Posts);
                foreach (var record in result.Where(r => copy.HasFace(s.Side, r.PostIndex))) PushBackDefenseSides.Set(record, s.Side, 0.0, auto: true);
                result.RemoveAll(r => r.ExitAuto && r.EntranceAuto);
                s.Posts = result.OrderBy(r => r.PostIndex).ToList();
            });

        /// <summary>The length the classic grid proposes when an end is turned on: the automatic one of that post in a rack of one way.</summary>
        public static double Proposed(int post, int postCount) => DynamicForkliftDefensePlan.At(null, post, postCount).ExitLength;

        // One end of one post, as the OK of the grid of the side: every other row goes through the grid as it shows it (an automatic end keeps
        // no length; a length is read back from its box, «0.##»), rows that are automatic at both ends are no record, in the order of the posts.
        private static void SetDefenseEnd(PushBackSafetyCopy copy, RackCatalog catalog, PushBackSide side, int post, bool farEnd, double length, bool auto)
            => WriteDefense(copy, catalog, side, s =>
            {
                if (post < 0 || post >= s.PostCount) return;
                if (s.OwnsBothEnds)
                {
                    var result = PushBackDefenseSides.Copy(s.Posts);
                    foreach (var record in result)
                    {
                        Normalize(record, farEnd: false);
                        Normalize(record, farEnd: true);
                    }

                    var target = result.FirstOrDefault(r => r.PostIndex == post);
                    if (target == null)
                    {
                        target = new SafetyPostDefense { PostIndex = post, ExitAuto = true, EntranceAuto = true };
                        result.Add(target);
                    }

                    if (farEnd) { target.EntranceLength = length; target.EntranceAuto = auto; }
                    else { target.ExitLength = length; target.ExitAuto = auto; }
                    result.RemoveAll(r => r.ExitAuto && r.EntranceAuto);
                    s.Posts = result.OrderBy(r => r.PostIndex).ToList();
                    return;
                }

                // A composite: the grid of ONE face (the near one for A, the far one for B). Its result keeps the other face as it was, and a
                // line where the side has no face of attack is a row it does not offer (nothing to decide there). Declared: the result is
                // written on the records of the side itself; the classic merges it after dropping the rows automatic at both ends, so its
                // «Auto» on a face whose other face is automatic is lost there (the length stays). Every other gesture writes the same.
                if (!copy.HasFace(s.Side, post)) return;
                var face = PushBackDefenseSides.IsFarEnd(s.Side);
                var faceResult = PushBackDefenseSides.Copy(s.Posts);
                foreach (var record in faceResult.Where(r => copy.HasFace(s.Side, r.PostIndex))) Normalize(record, face);
                var row = faceResult.FirstOrDefault(r => r.PostIndex == post);
                if (row == null)
                {
                    row = new SafetyPostDefense { PostIndex = post, ExitAuto = true, EntranceAuto = true };
                    faceResult.Add(row);
                }

                PushBackDefenseSides.Set(row, s.Side, length, auto);
                faceResult.RemoveAll(r => r.ExitAuto && r.EntranceAuto);
                s.Posts = faceResult.OrderBy(r => r.PostIndex).ToList();
            });

        // The round trip of one end through the classic grid: an automatic end keeps no length, a length is what its box shows («0.##»).
        private static void Normalize(SafetyPostDefense record, bool farEnd)
        {
            var auto = farEnd ? record.EntranceAuto : record.ExitAuto;
            var length = farEnd ? record.EntranceLength : record.ExitLength;
            var value = auto || length <= 0.0 ? 0.0 : double.Parse(length.ToString("0.##", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
            if (farEnd) record.EntranceLength = value;
            else record.ExitLength = value;
        }

        // ================================================================== the protector lateral (the per-post window of the dialog)

        /// <summary>
        /// The protector lateral of one post, as the per-post window of the classic dialog and its «Aceptar» (<c>EditPerPost</c>,
        /// <c>SafetyPerPostWindow</c>, <c>OnOk</c>): opened on a lateral with no post chosen it shows the two ends (the guide of the first post to
        /// the left, of the last one to the right); the post takes its own value («No lleva» is written, so the grid stays chosen by post); the
        /// window returns the posts in their order, and «Aceptar» writes the family again with no general side. Returns true when the lateral
        /// followed the rule of the ends before (the caller says that it no longer does).
        /// </summary>
        public static bool SetLateralPost(List<SelectiveSafetySelection> safety, RackCatalog catalog, int post, SafetySide side, int postCount)
        {
            var lateral = Of(safety, catalog, SelectiveSafetyDefaults.LateralType);
            if (lateral == null || post < 0 || post >= Math.Max(1, postCount)) return false;
            var followedTheRule = LateralFollowsRule(lateral);
            var current = lateral.PostSides.Where(p => p != null).ToList();
            if (current.Count == 0)
            {
                current.Add(new SafetyPostSide { PostIndex = 0, Side = SafetySide.Left });
                if (postCount > 1) current.Add(new SafetyPostSide { PostIndex = postCount - 1, Side = SafetySide.Right });
            }

            // The window has one row per post: a row with a value of its own is returned, in the order of the posts.
            var posts = Enumerable.Range(0, Math.Max(1, postCount))
                .Select(p => p == post ? new SafetyPostSide { PostIndex = p, Side = side } : current.FirstOrDefault(c => c.PostIndex == p))
                .Where(p => p != null)
                .Select(p => new SafetyPostSide { PostIndex = p.PostIndex, Side = p.Side })
                .ToList();
            Replace(safety, lateral, RebuiltLateral(lateral.ElementId, posts));
            return followedTheRule;
        }

        /// <summary>«Volver a la regla de los extremos»: the per-post window with every post on «(por defecto)»; the family written again.</summary>
        public static void LateralToRule(List<SelectiveSafetySelection> safety, RackCatalog catalog)
        {
            var lateral = Of(safety, catalog, SelectiveSafetyDefaults.LateralType);
            if (lateral != null) Replace(safety, lateral, RebuiltLateral(lateral.ElementId, Array.Empty<SafetyPostSide>()));
        }

        /// <summary>The protector lateral follows the rule of the ends (no post chosen, no general side), recomputed with the fronts.</summary>
        public static bool LateralFollowsRule(SelectiveSafetySelection lateral)
            => lateral != null && lateral.Side == SafetySide.None && !lateral.PostSides.Any(p => p != null);

        // What «Aceptar» writes for the row of the protector lateral: its variant, no general side, one, its posts.
        private static SelectiveSafetySelection RebuiltLateral(string elementId, IEnumerable<SafetyPostSide> posts)
        {
            var selection = new SelectiveSafetySelection { ElementId = elementId, Side = SafetySide.None, Quantity = 1 };
            foreach (var post in posts.Where(p => p != null && p.PostIndex >= 0)) selection.PostSides.Add(new SafetyPostSide { PostIndex = post.PostIndex, Side = post.Side });
            return selection;
        }

        // ================================================================== the desviador (its grid, PB-002 and PB-003)

        /// <summary>
        /// The side the classic grid of the desviador of Push Back returns, whatever the desviador had: the dialog opens it with
        /// <c>showDesviadorSide: false</c> (PB-003), so the grid selects its <c>LowEndSide</c> (Left) and <c>BuildResult</c> returns it
        /// (<c>SafetyDesviadorGridWindow</c>). Review of 01e76902, point 2.
        /// </summary>
        public const SafetySide DesviadorGridSide = SafetySide.Left;

        /// <summary>
        /// One gesture on the desviador, written as the «Aceptar» of its classic grid and of the dialog (<c>SafetyDesviadorGridWindow</c>,
        /// <c>OnOk</c>): the grid opens with the cells of the desviador (one column per post, its levels), the gesture changes it, and the family is
        /// written again with its variant, one, the side the grid of Push Back returns (<see cref="DesviadorGridSide"/>: it has no selector,
        /// PB-003), the longitud and the first level as the grid shows them (an invalid one is the default 18 in), the reading by post it had,
        /// and the off cells in the order of the grid (post by post, level by level) followed by the ones kept on a column with no level (a
        /// blank front, I-33); a cell beyond the levels of its post goes.
        /// </summary>
        public static void WriteDesviador(List<SelectiveSafetySelection> safety, RackCatalog catalog, IReadOnlyList<int> levelsPerPost,
            Action<SelectiveSafetySelection> gesture)
        {
            var desviador = Of(safety, catalog, SelectiveSafetyDefaults.DesviadorType);
            if (desviador == null) return;
            var grid = desviador.DeepCopy();
            if (!SelectiveDesviadorPlan.IsValidEvenAbove8(grid.DesviadorLongitud)) grid.DesviadorLongitud = SelectiveSafetyDefaults.DesviadorLongitud;
            if (!SelectiveDesviadorPlan.IsValidEvenAbove8(grid.DesviadorPrimerNivelAltura)) grid.DesviadorPrimerNivelAltura = SelectiveSafetyDefaults.DesviadorPrimerNivelAltura;
            var levels = levelsPerPost ?? Array.Empty<int>();
            var stored = grid.DesviadorOffCells.Where(c => c != null).Select(c => new SelectiveGridCell { Frente = c.Frente, Level = c.Level }).ToList();
            gesture?.Invoke(grid);

            var off = new HashSet<(int, int)>(grid.DesviadorOffCells.Where(c => c != null).Select(c => (c.Frente, c.Level)));
            var live = new List<SelectiveGridCell>();
            for (var post = 0; post < levels.Count; post++)
            {
                for (var level = 0; level < levels[post]; level++)
                {
                    if (off.Contains((post, level))) live.Add(new SelectiveGridCell { Frente = post, Level = level });
                }
            }

            var rebuilt = new SelectiveSafetySelection
            {
                ElementId = grid.ElementId,
                Quantity = 1,
                Side = DesviadorGridSide,
                DesviadorLongitud = grid.DesviadorLongitud,
                DesviadorPrimerNivelAltura = grid.DesviadorPrimerNivelAltura,
            };
            rebuilt.Desviador.CellsByPost = grid.Desviador.CellsByPost;
            foreach (var cell in SafetyDormantCells.Merge(live, stored, levels)) rebuilt.DesviadorOffCells.Add(new SelectiveGridCell { Frente = cell.Frente, Level = cell.Level });
            Replace(safety, desviador, rebuilt);
        }

        /// <summary>One cell (post, level) of the desviador on or off, through its grid.</summary>
        public static void SetDesviador(List<SelectiveSafetySelection> safety, RackCatalog catalog, IReadOnlyList<int> levelsPerPost, int post, int level, bool on)
            => WriteDesviador(safety, catalog, levelsPerPost, d => RackCadSafetyEdit.SetDesviador(d, post, level, on));

        /// <summary>«En todos los niveles» / «En ninguno» of the grid: every cell it shows.</summary>
        public static void SetDesviadorAll(List<SelectiveSafetySelection> safety, RackCatalog catalog, IReadOnlyList<int> levelsPerPost, bool on)
        {
            var levels = levelsPerPost ?? Array.Empty<int>();
            WriteDesviador(safety, catalog, levels, d => RackCadSafetyEdit.SetDesviadorAll(d,
                Enumerable.Range(0, levels.Count).SelectMany(p => Enumerable.Range(0, Math.Max(0, levels[p])).Select(l => (p, l))), on));
        }

        /// <summary>A measure of the desviador (its longitud or its first level), through its grid.</summary>
        public static void SetDesviadorMeasure(List<SelectiveSafetySelection> safety, RackCatalog catalog, IReadOnlyList<int> levelsPerPost,
            Action<SelectiveSafetySelection> measure)
            => WriteDesviador(safety, catalog, levelsPerPost, measure);

        // ================================================================== the variant of the protector lateral and of the desviador

        /// <summary>
        /// The variant of the protector lateral or of the desviador, as the list of the classic dialog: a family already chosen keeps its row and
        /// is written again with the other variant (the desviador as the «Aceptar» without «Configurar…»: <see cref="DesviadorRow"/>, not through
        /// its grid); a new one comes as «Aceptar» writes it with nothing else changed (the lateral with the rule of the ends; the desviador
        /// through its grid, on every cell, 18 in, its first level at 18 in, on <see cref="DesviadorGridSide"/>). «Sin variante» is the shared
        /// <see cref="RackCadSafetyEdit.Remove"/> (the «Ninguno» of the list). Returns the selection afterwards.
        /// </summary>
        public static SelectiveSafetySelection SetVariant(List<SelectiveSafetySelection> safety, RackCatalog catalog, string type, string elementId,
            int postCount, IReadOnlyList<int> levelsPerPost)
        {
            var variant = Variants(catalog, type).FirstOrDefault(v => string.Equals(v.Id, (elementId ?? string.Empty).Trim(), StringComparison.OrdinalIgnoreCase))
                          ?? throw new ArgumentException("«" + elementId + "» no es una variante de " + type + " en el catálogo.", nameof(elementId));
            var isDesviador = SelectiveSafetyDefaults.IsType(type, SelectiveSafetyDefaults.DesviadorType);
            var current = Of(safety, catalog, type);
            if (current != null && isDesviador)
            {
                // Review of 01e76902, point 1: only the variant changes; its measures and its cells are written as the row holds them.
                Replace(safety, current, DesviadorRow(current, variant.Id));
                return Of(safety, catalog, type);
            }

            if (current == null)
            {
                current = RackCadSafetyEdit.Fresh(type, variant.Id, RackCadSafetySystem.PushBack, postCount);
                safety.Add(current);
            }
            else
            {
                current.ElementId = variant.Id;
            }

            if (SelectiveSafetyDefaults.IsType(type, SelectiveSafetyDefaults.LateralType))
            {
                Replace(safety, current, RebuiltLateral(current.ElementId, current.PostSides));
            }
            else if (isDesviador)
            {
                WriteDesviador(safety, catalog, levelsPerPost, null);
            }

            return Of(safety, catalog, type);
        }

        /// <summary>
        /// What the «Aceptar» of the classic dialog writes for the row of an existing desviador whose list changed and whose grid was not
        /// opened (<c>SelectiveSafetyWindow</c>: the row is seeded from the selection, and its level counts come from
        /// <c>SelectiveDesviadorPlan.Build(null, …)</c>, empty, so <c>AllCellsOff</c> is false and the row stays whole): the other variant, one,
        /// the side it had (Left or Right, else both), the longitud and the first level as they are (not validated), the reading by post and every
        /// off cell, also one beyond the levels of its post. Only the variant changes in what the row shows.
        /// </summary>
        public static SelectiveSafetySelection DesviadorRow(SelectiveSafetySelection existing, string elementId)
        {
            if (existing == null) throw new ArgumentNullException(nameof(existing));
            var row = new SelectiveSafetySelection
            {
                ElementId = elementId,
                Quantity = 1,
                Side = existing.Side == SafetySide.Left || existing.Side == SafetySide.Right ? existing.Side : SafetySide.Both,
                DesviadorLongitud = existing.DesviadorLongitud,
                DesviadorPrimerNivelAltura = existing.DesviadorPrimerNivelAltura,
            };
            row.Desviador.CellsByPost = existing.Desviador.CellsByPost;
            foreach (var cell in existing.DesviadorOffCells.Where(c => c != null)) row.DesviadorOffCells.Add(new SelectiveGridCell { Frente = cell.Frente, Level = cell.Level });
            return row;
        }

        // The selection takes the place of the one it replaces.
        private static void Replace(List<SelectiveSafetySelection> safety, SelectiveSafetySelection old, SelectiveSafetySelection replacement)
        {
            var index = safety.IndexOf(old);
            if (index < 0) safety.Add(replacement);
            else safety[index] = replacement;
        }

        // ================================================================== helpers

        // Every selection of a family goes; the new one (when there is one) takes the place of the first.
        private static void ReplaceFamily(List<SelectiveSafetySelection> safety, RackCatalog catalog, string type, SelectiveSafetySelection replacement)
        {
            var ids = new HashSet<string>(Variants(catalog, type).Select(v => v.Id), StringComparer.OrdinalIgnoreCase);
            var index = safety.FindIndex(s => s != null && !string.IsNullOrWhiteSpace(s.ElementId) && ids.Contains(s.ElementId));
            safety.RemoveAll(s => s != null && !string.IsNullOrWhiteSpace(s.ElementId) && ids.Contains(s.ElementId));
            if (replacement == null) return;
            if (index < 0 || index > safety.Count) safety.Add(replacement);
            else safety.Insert(index, replacement);
        }
    }
}
