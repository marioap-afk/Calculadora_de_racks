using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Systems.Cantilever;
using RackCad.Domain.Systems.Shared;

namespace RackCad.UI.Editor
{
    /// <summary>Which Cantilever component a stand-alone insertion draws.</summary>
    public enum CantileverComponentKind
    {
        ColumnBase = 0,
        Arm = 1,
        Separator = 2,
        Brace = 3
    }

    /// <summary>
    /// The request to draw ONE Cantilever component on its own, outside any line.
    ///
    /// <para><b>It is not a rack, and the drawing must not pretend it is.</b> It follows the <c>RACKSECCION</c>
    /// precedent (I-36C), which is the repository's only pattern for «put a catalogued piece in the drawing»: no
    /// system kind on the block, no design payload, no envelope, no round-trip. So it deliberately does NOT carry
    /// a <see cref="Application.Persistence.RackEmbedDocument"/>, it is never stamped with
    /// <c>KindCantilever</c>, and <c>RACKLISTA</c> and <c>RACKEDITAR</c> will not see it. Promising an edit that
    /// does not exist would be worse than not offering the insertion.</para>
    ///
    /// <para><b>Its identity is its own.</b> A component gets a fresh GUID that is never the line's: the two are
    /// different things, and reusing the line's id would make a loose column claim to be the rack.</para>
    ///
    /// <para>It travels as a UI→host request like every other insertion, and it carries the SAME
    /// <see cref="CantileverViewPlan"/> objects the preview drew — not a description the host would have to
    /// project again. That is what makes «preview == bloque» true by construction rather than by agreement.</para>
    /// </summary>
    public sealed class CantileverComponentInsertionRequest : RackInsertionRequest
    {
        /// <summary>
        /// The DISPATCH kind, and nothing more.
        ///
        /// It says which Plugin class handles the request so it can travel through the vigent insertion seam —
        /// the menu and the command both carry <see cref="RackInsertionRequest"/>. It is NOT what gets stamped on
        /// the drawing: this request writes no envelope at all, so no block of it ever carries
        /// <c>RackEmbedDocument.KindCantilever</c>, and neither <c>RACKLISTA</c> nor <c>RACKEDITAR</c> will treat
        /// it as a line. A source guard pins that distinction.
        /// </summary>
        public override RackSystemKind Kind => RackSystemKind.Cantilever;

        public CantileverComponentInsertionRequest(
            CantileverComponentKind component,
            IReadOnlyList<CantileverViewPlan> views,
            string designation,
            Func<Guid> newIdFactory = null)
        {
            if (views == null || views.Count == 0)
            {
                throw new ArgumentException("Una insercion de componente necesita al menos una vista.", nameof(views));
            }

            Component = component;
            Views = views;
            Designation = designation ?? string.Empty;

            // A FRESH id, minted here and never taken from the line.
            ComponentId = (newIdFactory ?? Guid.NewGuid)();
        }

        public CantileverComponentKind Component { get; }

        /// <summary>The plans the preview drew. The host materialises exactly these.</summary>
        public IReadOnlyList<CantileverViewPlan> Views { get; }

        /// <summary>A short human label — the section designation, normally. Never a key.</summary>
        public string Designation { get; }

        /// <summary>The component's OWN identity. Never the line's.</summary>
        public Guid ComponentId { get; }

        /// <summary>The Spanish noun of the component, for the block name and the message.</summary>
        public string Noun
        {
            get
            {
                switch (Component)
                {
                    case CantileverComponentKind.ColumnBase: return "COLUMNA_BASE";
                    case CantileverComponentKind.Arm: return "BRAZO";
                    case CantileverComponentKind.Separator: return "SEPARADOR";
                    case CantileverComponentKind.Brace: return "TENSOR";
                    default: return Component.ToString().ToUpperInvariant();
                }
            }
        }

        /// <summary>
        /// The deterministic block name of one of its views.
        ///
        /// It carries the component kind, the designation, the view and the first eight digits of the component's
        /// own GUID — enough for a human to tell two loose columns apart in the block table, and enough to say
        /// «RackCad drew this». It is a LABEL and never a key: nothing resolves a block by it.
        /// </summary>
        public string BlockName(CantileverViewPlan plan) => Sanitize(string.Format(
            CultureInfo.InvariantCulture,
            "RACKCAD_CANTILEVER_COMPONENTE_{0}_{1}_{2}_{3}",
            Noun,
            string.IsNullOrWhiteSpace(Designation) ? "SIN_SECCION" : Designation,
            plan.View.ToString().ToUpperInvariant(),
            ComponentId.ToString("N").Substring(0, 8).ToUpperInvariant()));

        private static string Sanitize(string name)
        {
            var builder = new System.Text.StringBuilder(name.Length);

            foreach (var ch in name)
            {
                builder.Append(
                    (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z') ||
                    (ch >= '0' && ch <= '9') || ch == '_' || ch == '-'
                        ? ch
                        : '_');
            }

            return builder.ToString();
        }

        /// <summary>What the user reads afterwards.</summary>
        public string Describe() => string.Format(
            CultureInfo.InvariantCulture,
            "{0} · {1} · {2} vista(s) · {3} contornos",
            Noun,
            string.IsNullOrWhiteSpace(Designation) ? "(sin sección)" : Designation,
            Views.Count,
            Views.Sum(v => v.Curves.Count));
    }
}
