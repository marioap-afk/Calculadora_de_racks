using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.Shared;

namespace RackCad.Application.Views.Placement
{
    /// <summary>
    /// The selection the pure plan receives plus the notices about what was set aside. Entities that are not blocks,
    /// are outside Model Space or carry no RackCad data are ignored with a notice (as I-51 does); every other
    /// non-selectable member stays in the selection so the plan fails the whole operation and lists it (OD-3).
    /// </summary>
    public sealed class RackProjectionSelectionSnapshot
    {
        internal RackProjectionSelectionSnapshot(RackPhysicalSelection selection, IReadOnlyList<string> notices)
        {
            Selection = selection;
            Notices = notices;
        }

        public RackPhysicalSelection Selection { get; }
        public IReadOnlyList<string> Notices { get; }
        public bool HasMembers => Selection.Members.Count > 0;
    }

    public static class RackProjectionSelectionFilter
    {
        public static RackProjectionSelectionSnapshot Build(
            IReadOnlyList<RackPhysicalReferenceSnapshot> references,
            IReadOnlyList<RackPhysicalDefinitionSnapshot> definitions,
            Func<string, RackPhysicalKindDisposition> classifyKind)
        {
            if (references == null) throw new ArgumentNullException(nameof(references));
            if (definitions == null) throw new ArgumentNullException(nameof(definitions));
            if (classifyKind == null) throw new ArgumentNullException(nameof(classifyKind));

            var all = RackPhysicalSelection.Classify(references, definitions, classifyKind);
            var ignored = new HashSet<string>(StringComparer.Ordinal);
            var notBlock = 0;
            var outside = 0;
            var withoutData = 0;
            foreach (var member in all.Members)
            {
                switch (member.Disposition)
                {
                    case RackPhysicalMemberDisposition.NotBlock:
                        notBlock++;
                        ignored.Add(member.PhysicalKey);
                        break;
                    case RackPhysicalMemberDisposition.WrongSpace:
                        outside++;
                        ignored.Add(member.PhysicalKey);
                        break;
                    case RackPhysicalMemberDisposition.MissingRackData:
                        withoutData++;
                        ignored.Add(member.PhysicalKey);
                        break;
                }
            }

            var kept = ignored.Count == 0
                ? all
                : RackPhysicalSelection.Classify(
                    references.Where(reference => !ignored.Contains(reference.PhysicalKey)).ToList(),
                    definitions,
                    classifyKind);

            var notices = new List<string>();
            if (notBlock > 0)
                notices.Add("RackCad: se ignoraron " + notBlock + " objeto(s) que no son bloques.");
            if (outside > 0)
                notices.Add("RackCad: se ignoraron " + outside + " referencia(s) fuera del espacio modelo.");
            if (withoutData > 0)
                notices.Add("RackCad: se ignoraron " + withoutData + " bloque(s) sin datos de RackCad.");

            return new RackProjectionSelectionSnapshot(kept, notices);
        }
    }
}
