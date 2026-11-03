namespace RackCad.Application.StructuralAnalysis.Providers.RamElements
{
    /// <summary>The stable diagnostic codes of the provider. A code is never renamed or reused.</summary>
    public static class RamElementsDiagnosticCodes
    {
        public const string MapJsonMalformed = "RAM-MAP-JSON-MALFORMED";
        public const string MapSchemaMismatch = "RAM-MAP-SCHEMA-MISMATCH";
        public const string MapDuplicateProperty = "RAM-MAP-DUPLICATE-PROPERTY";
        public const string MapUnknownProperty = "RAM-MAP-UNKNOWN-PROPERTY";
        public const string MapMissingProperty = "RAM-MAP-MISSING-PROPERTY";
        public const string MapWrongType = "RAM-MAP-WRONG-TYPE";
        public const string MapSectionIdInvalid = "RAM-MAP-SECTION-ID-INVALID";
        public const string MapSectionIdDuplicate = "RAM-MAP-SECTION-ID-DUPLICATE";
        public const string MapTextInvalid = "RAM-MAP-TEXT-INVALID";
        public const string MapUnitUnknown = "RAM-MAP-UNIT-UNKNOWN";
        public const string MapValueInvalid = "RAM-MAP-VALUE-INVALID";
        public const string MapStatusInvalid = "RAM-MAP-STATUS-INVALID";
        public const string MapDateInvalid = "RAM-MAP-DATE-INVALID";
        public const string MapVerifiedIncomplete = "RAM-MAP-VERIFIED-INCOMPLETE";

        public const string ModelNotValid = "RAM-MODEL-NOT-VALID";
        public const string SectionRowMissing = "RAM-SECTION-ROW-MISSING";
        public const string SectionRowUnverified = "RAM-SECTION-ROW-UNVERIFIED";
        public const string MaterialMismatch = "RAM-MATERIAL-MISMATCH";
        public const string OrientationUnsupported = "RAM-ORIENTATION-UNSUPPORTED";
        public const string ReleaseUnsupported = "RAM-RELEASE-UNSUPPORTED";
        public const string LoadCountExceeded = "RAM-LOAD-COUNT-EXCEEDED";
        public const string LoadPositionRange = "RAM-LOAD-POSITION-RANGE";
        public const string ModelReference = "RAM-MODEL-REFERENCE";
        public const string SupportDuplicate = "RAM-SUPPORT-DUPLICATE";
        public const string MemberGeometry = "RAM-MEMBER-GEOMETRY";
    }
}
