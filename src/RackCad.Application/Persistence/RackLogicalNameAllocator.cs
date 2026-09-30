using System;
using System.Collections.Generic;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// I-60: the automatic logical name of a NEW rack. Inert scaffold of the Freeze: the behaviour arrives with the GREEN commit.
    /// </summary>
    public static class RackLogicalNameAllocator
    {
        public static string PrefixOf(RackSystemKind kind) => throw new NotImplementedException("I-60 scaffold");

        public static bool TryReadNumber(string name, string prefix, out string number) => throw new NotImplementedException("I-60 scaffold");

        public static string Next(RackSystemKind kind, IEnumerable<string> existingLogicalNames) => throw new NotImplementedException("I-60 scaffold");

        public static bool IsUnassigned(RackSystemKind kind, string requestedName) => throw new NotImplementedException("I-60 scaffold");

        public static string ForNewRack(RackSystemKind kind, string requestedName, IEnumerable<string> existingLogicalNames)
            => throw new NotImplementedException("I-60 scaffold");
    }
}
