using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using RackCad.Application.Views.Redraw;

namespace RackCad.Plugin.Systems.Shared
{
    internal sealed class AutoCadSiblingRedrawPort : ISiblingRedrawPort<SiblingRedrawUnit>
    {
        private readonly Document document;
        private readonly Func<RackSiblingMember, RackSiblingUnitPreparation<SiblingRedrawUnit>> prepare;

        internal AutoCadSiblingRedrawPort(
            Document document,
            Func<RackSiblingMember, RackSiblingUnitPreparation<SiblingRedrawUnit>> prepare)
        {
            this.document = document ?? throw new ArgumentNullException(nameof(document));
            this.prepare = prepare ?? throw new ArgumentNullException(nameof(prepare));
        }

        public RackSiblingUnitPreparation<SiblingRedrawUnit> Prepare(RackSiblingMember member)
        {
            using (document.LockDocument())
            {
                return prepare(member);
            }
        }

        public RackSiblingMutationResult Mutate(IReadOnlyList<SiblingRedrawUnit> units)
        {
            using (document.LockDocument())
            {
                return SiblingRedrawTransaction.Mutate(document, units);
            }
        }

        public void Post(RackSiblingRedrawPlan<SiblingRedrawUnit> plan, RackSiblingMutationResult committed)
        {
            using (document.LockDocument())
            {
                var units = new List<SiblingRedrawUnit>(plan.RedrawUnits.Count + plan.EraseUnits.Count);
                foreach (var item in plan.RedrawUnits) units.Add(item.Unit);
                foreach (var item in plan.EraseUnits) units.Add(item.Unit);
                SiblingRedrawTransaction.Post(document, units);
            }
        }
    }
}
