using System.Collections.Generic;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Domain.Systems.PushBack
{
    /// <summary>
    /// Resolved Push Back system: the shared dynamic structure (fronts, modules, separators, derived posts, and the
    /// per-front load-beam elevations already computed with the 7/16"/ft slope and the 2" troquel snap) PLUS Push
    /// Back's own resolved bits — the high-end (rear) beam id and its PERALTE resolved PER FRONT AND LEVEL, the rear
    /// pallet-stop configuration, and the entrance-side safety selections with entrance GUIDES removed (Push Back has
    /// none). Longitudinal coordinates and elevations live on the composed <see cref="DynamicRackSystem"/>, so drawing
    /// and BOM consume one resolved model without recomputing the structure.
    /// </summary>
    public sealed class PushBackSystem
    {
        /// <summary>The resolved shared structure (identical to the dynamic system's resolved aggregate). Never null.</summary>
        public DynamicRackSystem Structure { get; set; } = new DynamicRackSystem();

        /// <summary>Resolved high-end (rear) load beam id (<c>LARGUERO_ESCALON_TROQUEL_REDONDO</c>).</summary>
        public string HighEndBeamCatalogId { get; set; } = PushBackDefaults.HighEndBeamCatalogId;

        /// <summary>Resolved high-end (rear) beam PERALTE by level, one entry per <see cref="DynamicRackSystem.Fronts"/> (aligned by index).</summary>
        public IList<PushBackResolvedFront> HighEndBeams { get; } = new List<PushBackResolvedFront>();

        /// <summary>Resolved rear pallet-stop configuration (active by default; drawing and BOM consume an independent copy).</summary>
        public PushBackRearTopeConfig RearTope { get; set; } = new PushBackRearTopeConfig();

        /// <summary>I-42 (ronda 7E) — el tipo de defensa resuelto del lado A. Ver <see cref="PushBackDesign.DefensePieceId"/>.</summary>
        public string DefensePieceId { get; set; }

        /// <summary>
        /// Resolved entrance-side safety selections. GUIA (entrance guides) are EXCLUDED — Push Back has no entrance
        /// guides, so a guide never reaches the plan, the BOM or a snapshot. Drawing/BOM consume independent copies.
        /// </summary>
        public IList<SelectiveSafetySelection> SafetySelections { get; } = new List<SelectiveSafetySelection>();

        /// <summary>
        /// I-42 — la parte COMPUESTA del rack: los dos lados, la interfaz central (gap y separador) y la rejilla de
        /// celdas con su topologia. NULL es el rack de un solo sentido, que es lo que resuelve cualquier documento
        /// anterior a I-42; en ese caso <see cref="HighEndBeams"/> y <see cref="RearTope"/> siguen siendo, como
        /// siempre, la configuracion del unico lado.
        /// </summary>
        public PushBackCompositeSystem Composite { get; set; }

        /// <summary>True cuando el rack tiene DOS lados fisicos resueltos.</summary>
        public bool IsComposite => Composite != null && Composite.SideB != null && Composite.SideB.IsPresent;

        /// <summary>
        /// Los valores Push Back resueltos de un lado. El lado A responde SIEMPRE <see cref="HighEndBeams"/> — la
        /// misma lista, no una copia—, de modo que no puede existir una segunda autoridad para el lado que ya tenia
        /// el rack antes de I-42.
        /// </summary>
        public IList<PushBackResolvedFront> ResolvedFronts(PushBackSide side)
            => side == PushBackSide.A
                ? HighEndBeams
                : Composite?.SideB?.ResolvedFronts ?? new List<PushBackResolvedFront>();

        /// <summary>La rejilla de topes de un lado. El lado A responde la del rack, que es la de siempre.</summary>
        public PushBackRearTopeConfig RearTopeOf(PushBackSide side)
            => side == PushBackSide.A
                ? RearTope
                : Composite?.SideB?.RearTope ?? new PushBackRearTopeConfig();

        /// <summary>La proyeccion de una ranura transversal en un lado, o null si esa ranura no existe alli.</summary>
        public DynamicRackFront FrontOf(PushBackSide side, int frontIndex)
        {
            if (Composite != null)
            {
                return Composite.Of(side).Front(frontIndex);
            }

            var fronts = Structure?.Fronts;
            return side == PushBackSide.A && fronts != null && frontIndex >= 0 && frontIndex < fronts.Count
                ? fronts[frontIndex]
                : null;
        }

        /// <summary>
        /// I-42 — el fondo EFECTIVO de la celda de un LADO. Es la misma regla de I-41 (ADR-0030), ahora aplicada
        /// dentro de cada lado: <c>override de la celda ?? fondo por defecto del frente</c>. El lado A responde
        /// exactamente lo que respondia antes de I-42.
        /// </summary>
        public int EffectivePalletsDeepAt(PushBackSide side, int frontIndex, int level)
        {
            if (side == PushBackSide.A && Composite == null)
            {
                return EffectivePalletsDeepAt(frontIndex, level);
            }

            var resolved = Composite?.Of(side)?.Resolved(frontIndex);
            if (resolved != null && level >= 0 && level < resolved.PalletsDeep.Count && resolved.PalletsDeep[level] >= 2)
            {
                return resolved.PalletsDeep[level];
            }

            var front = FrontOf(side, frontIndex);
            return front != null ? System.Math.Max(2, front.PalletsDeep) : 2;
        }

        /// <summary>I-42 — si la celda de un LADO dibuja su tarima (I-41/PB-016). Fuera del BOM, como siempre.</summary>
        public bool DrawPalletAt(PushBackSide side, int frontIndex, int level)
        {
            if (side == PushBackSide.A && Composite == null)
            {
                return DrawPalletAt(frontIndex, level);
            }

            var resolved = Composite?.Of(side)?.Resolved(frontIndex);
            return resolved != null && level >= 0 && level < resolved.DrawPallets.Count && resolved.DrawPallets[level];
        }

        /// <summary>I-42 — el peralte del larguero posterior de la celda de un LADO.</summary>
        public double HighEndBeamPeralteAt(PushBackSide side, int frontIndex, int level)
        {
            if (side == PushBackSide.A && Composite == null)
            {
                return HighEndBeamPeralteAt(frontIndex, level);
            }

            var resolved = Composite?.Of(side)?.Resolved(frontIndex);
            if (resolved != null)
            {
                var peraltes = resolved.HighEndBeamPeraltes;
                if (level >= 0 && level < peraltes.Count)
                {
                    return peraltes[level];
                }

                if (peraltes.Count > 0)
                {
                    return peraltes[peraltes.Count - 1];
                }
            }

            return PushBackDefaults.HighEndBeamDefaultPeralte;
        }

        /// <summary>Client-facing rack name (supplied by the DWG envelope at drawing time); mirrors the structure's name.</summary>
        public string Name
        {
            get => Structure?.Name;
            set { if (Structure != null) { Structure.Name = value; } }
        }

        /// <summary>Convenience pass-through: the resolved transverse fronts of the shared structure.</summary>
        public IList<DynamicRackFront> Fronts => Structure?.Fronts;

        /// <summary>Convenience pass-through: the shared low-end complete IN/OUT beam id (same as the dynamic system).</summary>
        public string InOutBeamCatalogId => Structure?.InOutBeamCatalogId;

        /// <summary>Convenience pass-through: total longitudinal length of the shared structure.</summary>
        public double TotalLength => Structure?.TotalLength ?? 0.0;

        /// <summary>
        /// The resolved high-end beam PERALTE at (<paramref name="frontIndex"/>, <paramref name="level"/> — 0-based).
        /// Falls back to the last resolved level, then the explicit 3.5 default, so a caller never reads a hole.
        /// </summary>
        public double HighEndBeamPeralteAt(int frontIndex, int level)
        {
            if (frontIndex >= 0 && frontIndex < HighEndBeams.Count)
            {
                var peraltes = HighEndBeams[frontIndex].HighEndBeamPeraltes;
                if (level >= 0 && level < peraltes.Count)
                {
                    return peraltes[level];
                }

                if (peraltes.Count > 0)
                {
                    return peraltes[peraltes.Count - 1];
                }
            }

            return PushBackDefaults.HighEndBeamDefaultPeralte;
        }

        /// <summary>
        /// I-41 (PB-015) — the EFFECTIVE fondo of the cell at (<paramref name="frontIndex"/>, <paramref name="level"/> —
        /// 0-based): the number of pallet positions that cell occupies, already resolved by the single precedence rule
        /// <c>override ?? default</c> and clamped to the front's envelope. Geometry, views and BOM read THIS, never the
        /// front's structural <c>PalletsDeep</c> (which is only the envelope the structure is sized by).
        /// <para>
        /// Falls back to the front's structural depth when the cell was never resolved, so a caller never reads a hole
        /// and a legacy rack answers exactly what it always answered.
        /// </para>
        /// </summary>
        public int EffectivePalletsDeepAt(int frontIndex, int level)
        {
            if (frontIndex >= 0 && frontIndex < HighEndBeams.Count)
            {
                var deeps = HighEndBeams[frontIndex].PalletsDeep;
                if (level >= 0 && level < deeps.Count && deeps[level] >= 2)
                {
                    return deeps[level];
                }
            }

            var fronts = Structure?.Fronts;
            if (fronts != null && frontIndex >= 0 && frontIndex < fronts.Count)
            {
                return System.Math.Max(2, fronts[frontIndex].PalletsDeep);
            }

            return 2;
        }

        /// <summary>
        /// I-41 (PB-015) — the front's DEFAULT fondo, i.e. what a level of that front inherits when it carries no
        /// override. Zero/absent falls back to the front's structural depth (a legacy rack, where the two coincide).
        /// </summary>
        public int DefaultPalletsDeepAt(int frontIndex)
        {
            if (frontIndex >= 0 && frontIndex < HighEndBeams.Count && HighEndBeams[frontIndex].DefaultPalletsDeep >= 2)
            {
                return HighEndBeams[frontIndex].DefaultPalletsDeep;
            }

            var fronts = Structure?.Fronts;
            return fronts != null && frontIndex >= 0 && frontIndex < fronts.Count
                ? System.Math.Max(2, fronts[frontIndex].PalletsDeep)
                : 2;
        }

        /// <summary>
        /// I-41 (PB-016) — whether the cell at (<paramref name="frontIndex"/>, <paramref name="level"/> — 0-based) draws
        /// its pallet. Anything unresolved answers FALSE, which is the legacy Push Back drawing (no pallets at all).
        /// </summary>
        public bool DrawPalletAt(int frontIndex, int level)
        {
            if (frontIndex < 0 || frontIndex >= HighEndBeams.Count)
            {
                return false;
            }

            var flags = HighEndBeams[frontIndex].DrawPallets;
            return level >= 0 && level < flags.Count && flags[level];
        }
    }

    /// <summary>
    /// Resolved Push Back per-front values by load level (index 0 = level 1): the high-end (rear) beam PERALTE, and —
    /// I-41 — the EFFECTIVE fondo of each cell plus its pallet-drawing flag. The three lists stay aligned by level with
    /// the front's effective load levels; <see cref="DefaultPalletsDeep"/> is the front-wide fondo a level inherits.
    /// </summary>
    public sealed class PushBackResolvedFront
    {
        /// <summary>
        /// I-42 — si la ranura transversal existe en el lado al que pertenece esta entrada. False es el caso
        /// «A=3 y B=4»: la cuarta ranura de A no aporta celda, cama, larguero ni tope. Un rack de un solo sentido
        /// resuelve SIEMPRE true, que es lo que era implicito antes de I-42.
        /// </summary>
        public bool IsPresent { get; set; } = true;

        public IList<double> HighEndBeamPeraltes { get; } = new List<double>();

        /// <summary>I-41 (PB-015): the front's DEFAULT fondo — what a level with no override inherits.</summary>
        public int DefaultPalletsDeep { get; set; }

        /// <summary>I-41 (PB-015): the EFFECTIVE fondo of each level, already resolved and clamped to the envelope.</summary>
        public IList<int> PalletsDeep { get; } = new List<int>();

        /// <summary>I-41 (PB-016): whether each level draws its pallet (false = the legacy Push Back drawing).</summary>
        public IList<bool> DrawPallets { get; } = new List<bool>();
    }
}
