using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace I52Ctda.ControlPlane;

public sealed record FileIdentity(string Path, string Availability, long? Bytes, string? Sha256, string? Version);
public sealed record TupleIdentity(string Status, IReadOnlyDictionary<string, object> Fields, string DeterministicHash);

public static class ExactTupleCollector
{
    public static TupleIdentity Collect(string repository, string? nativeHelper, string implementationSha)
    {
        string sdk = @"D:\Downloads\CDROM1";
        string acad = @"C:\Program Files\Autodesk\AutoCAD 2025";
        var fields = new SortedDictionary<string, object>(StringComparer.Ordinal)
        {
            ["implementationSha"] = implementationSha,
            ["acad"] = Describe(Path.Combine(acad, "acad.exe")),
            ["AcCoreMgd"] = Describe(Path.Combine(acad, "AcCoreMgd.dll")),
            ["AcDbMgd"] = Describe(Path.Combine(acad, "AcDbMgd.dll")),
            ["AcMgd"] = Describe(Path.Combine(acad, "AcMgd.dll")),
            ["objectArxSdk"] = "25.0.58.0/x64/v143",
            ["headers"] = new[] { "dbmain.h", "dbtrans.h", "dbObject.h", "aced.h", "acdocman.h", "rxdlinkr.h", "acedads.h", "rxregsvc.h" }.Select(x => Describe(Path.Combine(sdk, "inc", x))).ToArray(),
            ["libraries"] = new[] { "rxapi.lib", "acad.lib", "accore.lib", "acdb25.lib" }.Select(x => Describe(Path.Combine(sdk, "lib-x64", x))).ToArray(),
            ["nativeHelper"] = nativeHelper is null ? new FileIdentity("", "NATIVE_TOOLCHAIN_BLOCKED", null, null, null) : Describe(nativeHelper),
            ["npm"] = Describe(Path.Combine(repository, "docs/initiatives/I-52-native-probe-matrix-v34.md")),
            ["nec"] = Describe(Path.Combine(repository, "docs/initiatives/I-52-native-event-catalog-v34.md")),
            ["fec"] = Describe(Path.Combine(repository, "docs/initiatives/I-52-fixture-execution-contract-v34.md"))
        };
        string canonical = JsonSerializer.Serialize(fields);
        string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
        string status = nativeHelper is null ? "INCOMPLETE_NATIVE_TOOLCHAIN_BLOCKED" : "COMPLETE_FOR_PRE_RUN_REVIEW";
        return new(status, fields, hash);
    }

    private static FileIdentity Describe(string path)
    {
        var file = new FileInfo(path);
        if (!file.Exists) return new(path, "NOT_AVAILABLE", null, null, null);
        return new(file.FullName, "AVAILABLE", file.Length, Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file.FullName))), FileVersionInfo.GetVersionInfo(file.FullName).FileVersion);
    }
}
