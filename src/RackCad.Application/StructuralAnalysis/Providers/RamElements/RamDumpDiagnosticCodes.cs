namespace RackCad.Application.StructuralAnalysis.Providers.RamElements
{
    /// <summary>The stable diagnostic codes of the dump script and of the readers of the dump, the read-back and the manifest. A code is never renamed or reused.</summary>
    public static class RamDumpDiagnosticCodes
    {
        public const string DumpJsonMalformed = "RAM-DUMP-JSON-MALFORMED";
        public const string DumpSchemaMismatch = "RAM-DUMP-SCHEMA-MISMATCH";
        public const string DumpDuplicateProperty = "RAM-DUMP-DUPLICATE-PROPERTY";
        public const string DumpUnknownProperty = "RAM-DUMP-UNKNOWN-PROPERTY";
        public const string DumpMissingProperty = "RAM-DUMP-MISSING-PROPERTY";
        public const string DumpWrongType = "RAM-DUMP-WRONG-TYPE";
        public const string DumpValueInvalid = "RAM-DUMP-VALUE-INVALID";
        public const string DumpFailed = "RAM-DUMP-FAILED";
        public const string DumpUnitPair = "RAM-DUMP-UNIT-PAIR";
        public const string DumpUnitsInconsistent = "RAM-DUMP-UNITS-INCONSISTENT";
        public const string DumpIdDuplicate = "RAM-DUMP-ID-DUPLICATE";
        public const string DumpReference = "RAM-DUMP-REFERENCE";
        public const string DumpLoadDirection = "RAM-DUMP-LOAD-DIRECTION";
        public const string DumpMemberGeometry = "RAM-DUMP-MEMBER-GEOMETRY";

        public const string ReadbackJsonMalformed = "RAM-READBACK-JSON-MALFORMED";
        public const string ReadbackSchemaMismatch = "RAM-READBACK-SCHEMA-MISMATCH";
        public const string ReadbackDuplicateProperty = "RAM-READBACK-DUPLICATE-PROPERTY";
        public const string ReadbackUnknownProperty = "RAM-READBACK-UNKNOWN-PROPERTY";
        public const string ReadbackMissingProperty = "RAM-READBACK-MISSING-PROPERTY";
        public const string ReadbackWrongType = "RAM-READBACK-WRONG-TYPE";
        public const string ReadbackValueInvalid = "RAM-READBACK-VALUE-INVALID";
        public const string ReadbackStatusUnknown = "RAM-READBACK-STATUS-UNKNOWN";
        public const string ReadbackInconsistent = "RAM-READBACK-INCONSISTENT";

        public const string ManifestJsonMalformed = "RAM-MANIFEST-JSON-MALFORMED";
        public const string ManifestSchemaMismatch = "RAM-MANIFEST-SCHEMA-MISMATCH";
        public const string ManifestDuplicateProperty = "RAM-MANIFEST-DUPLICATE-PROPERTY";
        public const string ManifestUnknownProperty = "RAM-MANIFEST-UNKNOWN-PROPERTY";
        public const string ManifestMissingProperty = "RAM-MANIFEST-MISSING-PROPERTY";
        public const string ManifestWrongType = "RAM-MANIFEST-WRONG-TYPE";
        public const string ManifestValueInvalid = "RAM-MANIFEST-VALUE-INVALID";
    }
}
