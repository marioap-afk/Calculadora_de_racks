using System;

namespace RackCad.Application.StructuralAnalysis
{
    public enum StructuralDiagnosticSeverity
    {
        /// <summary>Informative: the model is still produced.</summary>
        NonBlocking = 1,

        /// <summary>No model is produced.</summary>
        Blocking = 2,
    }

    /// <summary>
    /// One finding of building the neutral model: a stable code, a severity, the outcome it implies and its subject
    /// (I-65 D-08, CD-I65-F1-04). A blocking diagnostic implies a non-Valid outcome; a non-blocking one implies Valid.
    /// </summary>
    public sealed class StructuralDiagnostic
    {
        public StructuralDiagnostic(
            string code, StructuralDiagnosticSeverity severity, StructuralExportOutcome impliedOutcome, string subject)
        {
            throw new NotImplementedException();
        }

        public string Code => throw new NotImplementedException();

        public StructuralDiagnosticSeverity Severity => throw new NotImplementedException();

        public StructuralExportOutcome ImpliedOutcome => throw new NotImplementedException();

        public string Subject => throw new NotImplementedException();

        public bool IsBlocking => throw new NotImplementedException();

        public static StructuralDiagnostic Blocking(string code, StructuralExportOutcome outcome, string subject) =>
            throw new NotImplementedException();

        public static StructuralDiagnostic NonBlocking(string code, string subject) => throw new NotImplementedException();
    }

    /// <summary>The stable diagnostic codes. A code is never renamed or reused.</summary>
    public static class StructuralDiagnosticCodes
    {
        public const string LoadsJsonMalformed = "LOADS-JSON-MALFORMED";
        public const string LoadsSchemaMismatch = "LOADS-SCHEMA-MISMATCH";
        public const string LoadsDuplicateProperty = "LOADS-DUPLICATE-PROPERTY";
        public const string LoadsUnknownProperty = "LOADS-UNKNOWN-PROPERTY";
        public const string LoadsMissingProperty = "LOADS-MISSING-PROPERTY";
        public const string LoadsWrongType = "LOADS-WRONG-TYPE";
        public const string LoadsEmpty = "LOADS-EMPTY";
        public const string LoadsIdEmpty = "LOADS-ID-EMPTY";
        public const string LoadsIdDuplicate = "LOADS-ID-DUPLICATE";
        public const string LoadsTargetInvalid = "LOADS-TARGET-INVALID";
        public const string LoadsPositionBasis = "LOADS-POSITION-BASIS";
        public const string LoadsPositionRange = "LOADS-POSITION-RANGE";
        public const string LoadsDirection = "LOADS-DIRECTION";
        public const string LoadsMagnitude = "LOADS-MAGNITUDE";
        public const string LoadsUnitMissing = "LOADS-UNIT-MISSING";
        public const string LoadsUnitUnknown = "LOADS-UNIT-UNKNOWN";
        public const string SourceUnavailable = "SOURCE-UNAVAILABLE";
        public const string ModelReference = "MODEL-REFERENCE";
        public const string ModelDuplicateId = "MODEL-DUPLICATE-ID";
        public const string MemberGeometry = "MEMBER-GEOMETRY";
        public const string SectionUnknown = "SECTION-UNKNOWN";
        public const string SectionFamilyUnsupported = "SECTION-FAMILY-UNSUPPORTED";
        public const string SectionUnitSystem = "SECTION-UNIT-SYSTEM";
        public const string SectionPropertyAbsent = "SECTION-PROPERTY-ABSENT";
        public const string MaterialRowMissing = "MATERIAL-ROW-MISSING";
        public const string MaterialRowUnverified = "MATERIAL-ROW-UNVERIFIED";
        public const string ReleaseUnsupported = "RELEASE-UNSUPPORTED";
        public const string LoadCountExceeded = "LOAD-COUNT-EXCEEDED";
        public const string LoadTargetMember = "LOAD-TARGET-MEMBER";
        public const string LoadPositionRange = "LOAD-POSITION-RANGE";
    }
}
