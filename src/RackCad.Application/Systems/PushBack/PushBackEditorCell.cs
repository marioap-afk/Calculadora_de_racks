using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// Editable per-cell (front x level) Push-Back-specific state: the high-end (rear) beam PERALTE and whether the rear
    /// pallet-stop tope is active. Every structural/pallet/beam value Push Back reuses lives on the parallel
    /// <see cref="DynamicEditorCell"/> owned by <see cref="DynamicFrontMatrix"/>; this cell never restates them. A fresh
    /// cell reproduces the Push Back defaults: peralte 3.5 (a RULE, not "the first catalog value") and an active tope.
    /// </summary>
    public sealed class PushBackEditorCell
    {
        /// <summary>High-end (rear) beam PERALTE (in). Default 3.5 — the explicit Push Back rule, never allowed[0].</summary>
        public double HighEndBeamPeralte { get; set; } = PushBackDefaults.HighEndBeamDefaultPeralte;

        /// <summary>Whether the rear pallet-stop tope is drawn for this cell (default active; only deactivations persist).</summary>
        public bool RearTopeEnabled { get; set; } = true;

        /// <summary>
        /// Copy the Push-Back-specific values from an edit buffer (mirrors <see cref="DynamicEditorCell.Apply"/>).
        ///
        /// Owner decision (2026-07-24): the cell SCOPES (Celda/Selección/Nivel/Frente/Todo) must NOT copy
        /// <see cref="RearTopeEnabled"/>. The rear stop is configured EXCLUSIVELY from Seguridad, which writes the
        /// deactivations through the rear-tope config; applying a scope must never silently switch a neighbouring
        /// cell's stop on or off. That is why the tope is absent from <see cref="PushBackEditorValues"/> entirely —
        /// there is no buffer field for it to travel in.
        /// </summary>
        public void Apply(PushBackEditorValues values)
        {
            HighEndBeamPeralte = values.HighEndBeamPeralte;
        }

        public PushBackEditorCell Clone()
            => new PushBackEditorCell { HighEndBeamPeralte = HighEndBeamPeralte, RearTopeEnabled = RearTopeEnabled };

        public static PushBackEditorCell Default() => new PushBackEditorCell();

        /// <summary>
        /// Normalize the peralte against the catalog-allowed high-end values with Push Back's canonical rule: keep the
        /// current value if the catalog allows it, else the explicit 3.5 default if allowed, else the first allowed value.
        /// This is the SAME rule <see cref="PushBackResolver.ResolveHighEndBeamPeralte(double)"/> applies at resolve time,
        /// so a normalized cell resolves unchanged (idempotent). A null/empty list leaves the value untouched.
        /// </summary>
        public void NormalizePeralte(IReadOnlyList<double> allowed)
            => HighEndBeamPeralte = Canonical(HighEndBeamPeralte, allowed);

        /// <summary>
        /// Push Back's canonical high-end peralte rule, EXACTLY coherent with
        /// <see cref="PushBackResolver.ResolveHighEndBeamPeralte(double?, double)"/> called with the 3.5 default as the
        /// legacy fallback: a valid request in the catalog list is kept; a request that is invalid, zero or negative falls
        /// back to the explicit 3.5 default; a null/empty allowed list resolves to 3.5; and, only if 3.5 is somehow absent
        /// from a non-empty list, the first allowed value is used (the resolver's final fallback).
        /// </summary>
        public static double Canonical(double requested, IReadOnlyList<double> allowed)
        {
            if (allowed == null || allowed.Count == 0)
            {
                return PushBackDefaults.HighEndBeamDefaultPeralte;
            }

            bool InList(double value) => allowed.Any(candidate => Math.Abs(candidate - value) < 1e-6);
            if (requested > 0.0 && InList(requested))
            {
                return requested;
            }

            if (InList(PushBackDefaults.HighEndBeamDefaultPeralte))
            {
                return PushBackDefaults.HighEndBeamDefaultPeralte;
            }

            return allowed[0];
        }
    }
}
