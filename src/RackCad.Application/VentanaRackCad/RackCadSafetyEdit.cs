using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>The systems whose safety the RackCad window edits without the classic dialog (T4 seg; T5 pb adds Push Back, SG-09).</summary>
    public enum RackCadSafetySystem
    {
        Selective,
        Dynamic,

        /// <summary>T5 pb (PB-S03, SG-09): Push Back adopts the table and the inspector, with its own authority (<see cref="PushBackSafetyEdit"/>).</summary>
        PushBack,
    }

    /// <summary>The two ends of a Dinámico forklift defense: the exit (salida, the low end) and the entrance (entrada, the high end).</summary>
    public enum RackCadDefenseEnd
    {
        Exit,
        Entrance,
    }

    /// <summary>
    /// One end of the defense of one post, as the window shows it (SG-03 of the Owner): with a defense or not, its own length when one was
    /// written (null = the automatic one, which follows the post: 12 in at an edge post and 36 in at an intermediate one), the automatic
    /// length of that post now, and the length the drawing resolves.
    /// </summary>
    public sealed record RackCadDefenseEndState(bool On, double? OwnLength, double Automatic, double Resolved)
    {
        public bool IsAutomatic => On && !OwnLength.HasValue;
    }

    /// <summary>
    /// The edits of «Extremos y seguridad» of the RackCad window on a COPY of the safety of a draft (I-64, cuarta tanda, part «seg»; the
    /// prototype «seguridad», SG-01 to SG-08): the variant of a family, with the defaults of the classic dialog when it is chosen; the bota of
    /// each post; the protector lateral of each post (and the rule of the ends of Dinámico); the levels of the desviador of each post; and the
    /// two ends of the defense of each post of a Dinámico. Pure: no WPF and no AutoCAD. The caller replaces the safety of its draft with the
    /// copy and recomposes.
    /// <para>
    /// The data is the one the classic dialog writes (<c>SelectiveSafetyWindow.OnOk</c>): the bota keeps its side AND its placement
    /// (<see cref="SelectiveSafetySelection.Side"/> / <see cref="SelectiveSafetySelection.PostSides"/> and
    /// <see cref="SelectiveBotaConfig.Placement"/> / <see cref="SelectiveBotaConfig.Posts"/>, the same ordinals), a protector lateral is a per-post
    /// list, a desviador and the grids keep their off cells. What differs is declared: an edit here works IN PLACE on the selection (its other
    /// fields stay), and a family whose cells are all off keeps its variant (C2-07), where the classic «Aceptar» drops it.
    /// </para>
    /// </summary>
    public static class RackCadSafetyEdit
    {
        /// <summary>The families the window edits for each system, in the order of the prototype.</summary>
        public static IReadOnlyList<string> FamiliesOf(RackCadSafetySystem system)
            => system == RackCadSafetySystem.PushBack // T5 pb: the families Push Back admits (PushBackSafetyAuthority); its rear tope has its own card
                ? new[] { SelectiveSafetyDefaults.BotaType, SelectiveSafetyDefaults.LateralType, SelectiveSafetyDefaults.DesviadorType, SelectiveSafetyDefaults.DefensaType }
                : system == RackCadSafetySystem.Dynamic
                ? new[] { SelectiveSafetyDefaults.GuiaType, SelectiveSafetyDefaults.BotaType, SelectiveSafetyDefaults.LateralType, SelectiveSafetyDefaults.DesviadorType, SelectiveSafetyDefaults.DefensaType }
                : new[] { SelectiveSafetyDefaults.TopeType, SelectiveSafetyDefaults.ParrillaType, SelectiveSafetyDefaults.BotaType, SelectiveSafetyDefaults.LateralType, SelectiveSafetyDefaults.DesviadorType };

        /// <summary>The variants of a family in the catalog, in its order (seguridad.csv).</summary>
        public static IReadOnlyList<SafetyElementCatalogEntry> Variants(RackCatalog catalog, string type)
            => SelectiveSafetyFamilies.VariantsOfType(catalog?.SafetyElements, type);

        /// <summary>The selection of a family (the first one in the document, as every reader takes it); null when none is chosen.</summary>
        public static SelectiveSafetySelection Selected(IEnumerable<SelectiveSafetySelection> safety, RackCatalog catalog, string type)
            => SelectiveSafetyFamilies.SelectedOfType(safety, catalog?.SafetyElements, type);

        // ------------------------------------------------------------------ the variant (SG-01, SG-06)

        /// <summary>
        /// The variant of a family: an empty id takes the family out (every selection of it, as the «Ninguno» of the classic dialog), a family
        /// already chosen changes its variant and keeps its measures and its posts, and a new one comes with the defaults of the classic
        /// dialog (<see cref="Fresh"/>). Returns the selection of the family afterwards (null when it was taken out); throws when the id is
        /// not a variant of that family in the catalog.
        /// </summary>
        public static SelectiveSafetySelection SetVariant(List<SelectiveSafetySelection> safety, RackCatalog catalog, string type, string elementId,
            RackCadSafetySystem system, int postCount)
        {
            if (safety == null) throw new ArgumentNullException(nameof(safety));
            if (string.IsNullOrWhiteSpace(elementId))
            {
                Remove(safety, catalog, type);
                return null;
            }

            var variant = Variants(catalog, type).FirstOrDefault(v => string.Equals(v.Id, elementId.Trim(), StringComparison.OrdinalIgnoreCase));
            if (variant == null) throw new ArgumentException("«" + elementId + "» no es una variante de " + type + " en el catálogo.", nameof(elementId));

            var current = Selected(safety, catalog, type);
            if (current != null)
            {
                current.ElementId = variant.Id;
                return current;
            }

            var fresh = Fresh(type, variant.Id, system, postCount);
            safety.Add(fresh);
            return fresh;
        }

        /// <summary>Takes a family out of the safety: every selection of its variants. Returns how many went.</summary>
        public static int Remove(List<SelectiveSafetySelection> safety, RackCatalog catalog, string type)
        {
            var ids = new HashSet<string>(Variants(catalog, type).Select(v => v.Id), StringComparer.OrdinalIgnoreCase);
            return safety?.RemoveAll(s => s != null && !string.IsNullOrWhiteSpace(s.ElementId) && ids.Contains(s.ElementId)) ?? 0;
        }

        /// <summary>
        /// A family just chosen, with the defaults of the classic dialog when it is accepted without changing anything (the prototype: «elegir
        /// la variante basta», dolor 3 of the inventory): the bota on both sides of every post; the protector lateral of a Selectivo on the two
        /// posts of the ends (the guide of the first one to the left and of the last one to the right, <c>OrillaDefaults</c>) and the one of a
        /// Dinámico with the rule of the ends (no post chosen); the desviador on both faces, 18 in long, its first level 18 in above the first
        /// troquel, in every level; the tope shared, in the central fondo, with a saque of 3 in, not in the frontal, in every larguero; the
        /// parrilla in the frontal and the lateral, one per tarima, in every larguero; the guide in every cell; the defense automatic at
        /// every post.
        /// </summary>
        public static SelectiveSafetySelection Fresh(string type, string elementId, RackCadSafetySystem system, int postCount)
        {
            var selection = new SelectiveSafetySelection { ElementId = elementId, Quantity = 1, Side = SafetySide.None };
            if (Is(type, SelectiveSafetyDefaults.BotaType))
            {
                selection.Side = SafetySide.Both;
                selection.Bota.Placement = BootPlacements.From(SafetySide.Both);
            }
            else if (Is(type, SelectiveSafetyDefaults.LateralType))
            {
                if (system == RackCadSafetySystem.Selective) LateralToEnds(selection, postCount);
            }
            else if (Is(type, SelectiveSafetyDefaults.DesviadorType) || Is(type, SelectiveSafetyDefaults.TopeType) || Is(type, SelectiveSafetyDefaults.ParrillaType))
            {
                selection.Side = SafetySide.Both;

                // A new desviador of a Dinámico is read by post (decision of the Owner of 2026-11-26), as a new Dinámico.
                if (system == RackCadSafetySystem.Dynamic && Is(type, SelectiveSafetyDefaults.DesviadorType)) selection.Desviador.CellsByPost = true;
            }

            return selection;
        }

        // ------------------------------------------------------------------ the bota of each post

        /// <summary>The general side of the bota: the one every post without its own takes (as the classic combo reads it).</summary>
        public static SafetySide BootGeneral(SelectiveSafetySelection bota)
            => bota == null ? SafetySide.None : BootPlacements.To(bota.Bota.Placement ?? BootPlacements.From(bota.Side));

        /// <summary>The side a post has of its own; null when it takes the general one.</summary>
        public static SafetySide? BootOwn(SelectiveSafetySelection bota, int post)
        {
            if (bota == null) return null;
            if (bota.Bota.HasOwnAt(post)) return BootPlacements.To(bota.Bota.At(post).Value);
            var legacy = bota.PostSides.FirstOrDefault(p => p != null && p.PostIndex == post);
            return legacy?.Side;
        }

        /// <summary>How many posts have a bota of their own.</summary>
        public static int BootOwnCount(SelectiveSafetySelection bota, int postCount)
            => bota == null ? 0 : Enumerable.Range(0, Math.Max(0, postCount)).Count(p => BootOwn(bota, p).HasValue);

        /// <summary>
        /// The general side of the bota: its side and, when the selection keeps one, its placement (as the classic «Aceptar» writes them). A
        /// selection read with no placement (a document before I-42) keeps it empty: with no placement the drawing reads the side, so the
        /// result is the same and going back to the side read leaves the draft as the drawing has it. The classic «Aceptar» would write
        /// the placement too (declared difference).
        /// </summary>
        public static void SetBootGeneral(SelectiveSafetySelection bota, SafetySide side)
        {
            if (bota == null) return;
            bota.Side = side;
            if (bota.Bota.Placement.HasValue) bota.Bota.Placement = BootPlacements.From(side);
        }

        /// <summary>The bota of one post: a side of its own, or null to take the general one again.</summary>
        public static void SetBootPost(SelectiveSafetySelection bota, int post, SafetySide? side)
        {
            if (bota == null || post < 0) return;
            foreach (var entry in bota.PostSides.Where(p => p != null && p.PostIndex == post).ToList()) bota.PostSides.Remove(entry);
            foreach (var entry in bota.Bota.Posts.Where(p => p != null && p.PostIndex == post).ToList()) bota.Bota.Posts.Remove(entry);
            if (!side.HasValue) return;
            bota.PostSides.Add(new SafetyPostSide { PostIndex = post, Side = side.Value });
            bota.Bota.Posts.Add(new BootPostPlacement { PostIndex = post, Placement = BootPlacements.From(side.Value) });
        }

        /// <summary>«Todos los postes al general».</summary>
        public static void BootPostsToGeneral(SelectiveSafetySelection bota)
        {
            if (bota == null) return;
            bota.PostSides.Clear();
            bota.Bota.Posts.Clear();
        }

        // ------------------------------------------------------------------ the protector lateral of each post

        /// <summary>
        /// The protector lateral of a Dinámico follows the rule of the ends (no post chosen and no general side,
        /// <see cref="DynamicLateralGuardPlan"/>): it is recomputed when fronts are added or removed. A Selectivo has no such rule.
        /// </summary>
        public static bool LateralFollowsRule(SelectiveSafetySelection lateral, RackCadSafetySystem system)
            => system == RackCadSafetySystem.Dynamic && lateral != null && lateral.Side == SafetySide.None && !lateral.PostSides.Any(p => p != null);

        /// <summary>The side of the protector lateral at a post, as the drawing reads it.</summary>
        public static SafetySide LateralAt(SelectiveSafetySelection lateral, int post, int postCount, RackCadSafetySystem system)
        {
            if (lateral == null) return SafetySide.None;
            return system == RackCadSafetySystem.Dynamic ? DynamicLateralGuardPlan.SideAt(lateral, post, postCount) : lateral.SideForPost(post);
        }

        /// <summary>
        /// The protector lateral of one post. In a Dinámico that follows the rule of the ends, the rule is first written post by post (its
        /// two ends), so only the post touched changes: the result is «Elegido por poste» and no longer follows the fronts (SG-04). Returns
        /// true when that happened, so the caller says it.
        /// </summary>
        public static bool SetLateralPost(SelectiveSafetySelection lateral, int post, SafetySide side, int postCount, RackCadSafetySystem system)
        {
            if (lateral == null || post < 0) return false;
            var leftTheRule = false;
            if (LateralFollowsRule(lateral, system))
            {
                // Read the whole rule first: once a post is written the selection is no longer adaptive.
                var rule = Enumerable.Range(0, Math.Max(1, postCount)).Select(p => (Post: p, Side: DynamicLateralGuardPlan.SideAt(lateral, p, postCount))).ToList();
                foreach (var (p, ruleSide) in rule.Where(r => r.Side != SafetySide.None)) lateral.PostSides.Add(new SafetyPostSide { PostIndex = p, Side = ruleSide });
                leftTheRule = true;
            }

            foreach (var entry in lateral.PostSides.Where(p => p != null && p.PostIndex == post).ToList()) lateral.PostSides.Remove(entry);

            // «No lleva» in a Selectivo without a general side is the absence of the post (the «(por defecto)» of the classic per-post
            // editor); in a Dinámico it is written, so the grid stays chosen by post even when every post is emptied.
            if (side != SafetySide.None || lateral.Side != SafetySide.None || system == RackCadSafetySystem.Dynamic)
            {
                lateral.PostSides.Add(new SafetyPostSide { PostIndex = post, Side = side });
            }

            return leftTheRule;
        }

        /// <summary>«Volver a la regla de los extremos» of a Dinámico: no post chosen and no general side.</summary>
        public static void LateralToRule(SelectiveSafetySelection lateral)
        {
            if (lateral == null) return;
            lateral.PostSides.Clear();
            lateral.Side = SafetySide.None;
        }

        /// <summary>«Solo en los extremos» of a Selectivo: the first post with the guide to the left, the last one to the right (OrillaDefaults).</summary>
        public static void LateralToEnds(SelectiveSafetySelection lateral, int postCount)
        {
            if (lateral == null) return;
            lateral.PostSides.Clear();
            lateral.Side = SafetySide.None;
            lateral.PostSides.Add(new SafetyPostSide { PostIndex = 0, Side = SafetySide.Left });
            if (postCount > 1) lateral.PostSides.Add(new SafetyPostSide { PostIndex = postCount - 1, Side = SafetySide.Right });
        }

        // ------------------------------------------------------------------ the levels of the desviador of each post

        /// <summary>The desviador is at that (column, level) of its grid: the column is the key the drawing reads for the post.</summary>
        public static bool DesviadorOn(SelectiveSafetySelection desviador, int key, int level)
            => desviador != null && !desviador.DesviadorOffCells.Any(c => c != null && c.Frente == key && c.Level == level);

        /// <summary>The desviador on or off at one (column, level).</summary>
        public static void SetDesviador(SelectiveSafetySelection desviador, int key, int level, bool on)
        {
            if (desviador == null) return;
            foreach (var cell in desviador.DesviadorOffCells.Where(c => c != null && c.Frente == key && c.Level == level).ToList()) desviador.DesviadorOffCells.Remove(cell);
            if (!on) desviador.DesviadorOffCells.Add(new SelectiveGridCell { Frente = key, Level = level });
        }

        /// <summary>
        /// I-64, cuarta tanda (decision of the Owner of 2026-11-26, «Sí, corrígelo en Dinámico»): a desviador of a Dinámico still read the
        /// historic way (its last post reads the column of the post before it) passes to the reading by post, KEEPING what it draws: the
        /// last post takes the cells of the column it read, and the cells it had of its own, which nothing read, go. Called before an edit
        /// of the desviador in the window, so an existing rack draws what it drew until its desviador is edited, and from then on every
        /// post governs its own. Returns true when it passed. <paramref name="postCount"/> is the posts of the rack (fronts + 1).
        /// </summary>
        public static bool DesviadorToPosts(SelectiveSafetySelection desviador, int postCount)
        {
            if (desviador == null || desviador.Desviador.CellsByPost || desviador.DesviadorCellsAreByPost) return false;
            var last = postCount - 1;
            if (last >= 1)
            {
                var read = desviador.DesviadorOffCells.Where(c => c != null && c.Frente == last - 1).Select(c => c.Level).Distinct().ToList();
                foreach (var cell in desviador.DesviadorOffCells.Where(c => c != null && c.Frente == last).ToList()) desviador.DesviadorOffCells.Remove(cell);
                foreach (var level in read) desviador.DesviadorOffCells.Add(new SelectiveGridCell { Frente = last, Level = level });
            }

            desviador.Desviador.CellsByPost = true;
            return true;
        }

        /// <summary>«En todos los niveles» / «En ninguno»: only the cells the table shows, so a dormant cell of a blank front stays as it was.</summary>
        public static void SetDesviadorAll(SelectiveSafetySelection desviador, IEnumerable<(int Key, int Level)> cells, bool on)
        {
            foreach (var (key, level) in (cells ?? Enumerable.Empty<(int, int)>()).Distinct()) SetDesviador(desviador, key, level, on);
        }

        // ------------------------------------------------------------------ the defense of a Dinámico (SG-03)

        /// <summary>One end of the defense of one post: an empty length is the automatic one; a written one is the post's own.</summary>
        public static RackCadDefenseEndState DefenseAt(SelectiveSafetySelection defensa, int post, RackCadDefenseEnd end, int postCount)
        {
            var automatic = DynamicForkliftDefensePlan.At(null, post, postCount).ExitLength;
            if (defensa == null) return new RackCadDefenseEndState(false, null, automatic, 0.0);
            var setting = DynamicForkliftDefensePlan.ForSelection(defensa, post, postCount);
            var resolved = end == RackCadDefenseEnd.Exit ? setting.ExitLength : setting.EntranceLength;
            var record = defensa.DefensaPosts.FirstOrDefault(p => p != null && p.PostIndex == post);
            if (record == null || (end == RackCadDefenseEnd.Exit ? record.ExitAuto : record.EntranceAuto))
            {
                return new RackCadDefenseEndState(resolved > 0.0, null, automatic, resolved);
            }

            var length = end == RackCadDefenseEnd.Exit ? record.ExitLength : record.EntranceLength;
            return length > 0.0
                ? new RackCadDefenseEndState(true, length, automatic, resolved)
                : new RackCadDefenseEndState(false, null, automatic, resolved);
        }

        /// <summary>The ends with a length of their own or with no defense (what «Todas automáticas» gives back).</summary>
        public static int DefenseOwnEnds(SelectiveSafetySelection defensa)
            => defensa == null ? 0 : defensa.DefensaPosts.Where(p => p != null).Sum(p => (p.ExitAuto ? 0 : 1) + (p.EntranceAuto ? 0 : 1));

        /// <summary>The box of one end: on is the automatic length (a length can be written after); off is no defense at that end.</summary>
        public static void SetDefenseOn(SelectiveSafetySelection defensa, int post, RackCadDefenseEnd end, bool on)
            => EditDefense(defensa, post, record =>
            {
                if (end == RackCadDefenseEnd.Exit) { record.ExitAuto = on; record.ExitLength = 0.0; }
                else { record.EntranceAuto = on; record.EntranceLength = 0.0; }
            });

        /// <summary>
        /// The length of one end: null (an empty box) is the automatic one, recomputed when fronts are added or removed; a number is the
        /// post's own and stays even when it equals the automatic one, as Push Back does (PB-010). A declared change of the classic dialog of
        /// Dinámico, which drops a row equal to its automatic values.
        /// </summary>
        public static void SetDefenseLength(SelectiveSafetySelection defensa, int post, RackCadDefenseEnd end, double? length)
            => EditDefense(defensa, post, record =>
            {
                var auto = !length.HasValue;
                var value = length ?? 0.0;
                if (end == RackCadDefenseEnd.Exit) { record.ExitAuto = auto; record.ExitLength = value; }
                else { record.EntranceAuto = auto; record.EntranceLength = value; }
            });

        /// <summary>«Todas automáticas»: no record, so the plan computes both ends of every post.</summary>
        public static void DefenseAllAutomatic(SelectiveSafetySelection defensa) => defensa?.DefensaPosts.Clear();

        // A post with no record is automatic at both ends; a record automatic at both ends is no record (the classic and the plan agree).
        private static void EditDefense(SelectiveSafetySelection defensa, int post, Action<SafetyPostDefense> change)
        {
            if (defensa == null || post < 0) return;
            var record = defensa.DefensaPosts.FirstOrDefault(p => p != null && p.PostIndex == post);
            if (record == null)
            {
                record = new SafetyPostDefense { PostIndex = post, ExitAuto = true, EntranceAuto = true };
                defensa.DefensaPosts.Add(record);
            }

            change(record);
            if (record.ExitAuto && record.EntranceAuto) defensa.DefensaPosts.Remove(record);
        }

        private static bool Is(string actual, string expected) => SelectiveSafetyDefaults.IsType(actual, expected);
    }
}
