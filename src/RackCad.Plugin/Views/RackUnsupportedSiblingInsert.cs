using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Insertion;

namespace RackCad.Plugin.Views
{
    /// <summary>Fail-closed G9b edge for kinds whose AUTH-13 comparator is not yet demonstrated.</summary>
    internal static class RackUnsupportedSiblingInsert
    {
        internal static void Reject(Document document, ObjectId selected, RackEmbedDocument source, string rackId)
        {
            var originalIdentityIsAttributable = string.Equals(source?.Kind, RackEmbedDocument.KindCabecera,
                System.StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(source?.Id);
            var snapshot = RackSiblingScan.Capture(document, selected, rackId, source?.Id,
                originalIdentityIsAttributable, _ => false);
            var properties = RackSiblingCustomPropertiesGate.Evaluate(snapshot.Membership, snapshot.Properties);
            if (!properties.Accepted)
            {
                document.Editor.WriteMessage("\nRackCad: no se inserto ninguna vista: " + properties.Diagnostic);
                return;
            }

            document.Editor.WriteMessage("\nRackCad: no se inserto ninguna vista: "
                + CompareWithFoundationAuthority(source?.Kind));
        }

        private static string CompareWithFoundationAuthority(string kind)
        {
            IRackAuthoredComparatorPort<object, object> comparator;
            switch (kind)
            {
                case RackEmbedDocument.KindDynamic:
                    comparator = RackAuthoredComparatorPorts.Dynamic<object, object>();
                    break;
                case RackEmbedDocument.KindPushBack:
                    comparator = RackAuthoredComparatorPorts.PushBack<object, object>();
                    break;
                case RackEmbedDocument.KindCantilever:
                    comparator = RackAuthoredComparatorPorts.Cantilever<object, object>();
                    break;
                case RackEmbedDocument.KindCabecera:
                    comparator = RackAuthoredComparatorPorts.Cabecera<object, object>();
                    break;
                default:
                    return "AUTHORED_UNREADABLE: tipo sin comparador AUTH-13 demostrado.";
            }

            var comparison = comparator.Compare(null);
            return comparison.Outcome + ": " + comparison.Diagnostic;
        }
    }
}
