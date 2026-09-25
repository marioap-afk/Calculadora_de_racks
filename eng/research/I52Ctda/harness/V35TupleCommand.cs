using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using I52Ctda.ControlPlane.V35;

// tuple-v35 <repo> <package-root> <source-sha>: NEW BUILD_MACHINE_TOOLCHAIN_TUPLE of the R3 canonical build. The hash
// method is the V34 one: SHA-256 over UTF-8 compact JSON with keys sorted ordinally at every level.
internal static class V35TupleCommand
{
    public static int Run(string repo, string packageRoot, string sourceSha)
    {
        string root = Path.GetFullPath(packageRoot);
        string run = Path.Combine(root, "run");
        string evidence = Path.Combine(root, "evidence");
        JsonObject toolchain = JsonNode.Parse(File.ReadAllText(Path.Combine(evidence, "toolchain.json")))!.AsObject();
        var tuple = new JsonObject
        {
            ["kind"] = "BUILD_MACHINE_TOOLCHAIN_TUPLE",
            ["milestone"] = "I-52 R3 build side (V35 frozen contract)",
            ["v35FreezePackageHash"] = V35Freeze.PackageHash,
            ["v35FreezeSha"] = V35Freeze.FreezeSha,
            ["sourceSha"] = sourceSha,
            ["authority"] = new JsonObject
            {
                ["catalogBlob"] = V35Freeze.GitBlobId(Path.Combine(repo, V35Authority.CatalogPath)),
                ["matrixBlob"] = V35Freeze.GitBlobId(Path.Combine(repo, V35Authority.MatrixPath)),
                ["oracleBlob"] = V35Freeze.GitBlobId(Path.Combine(repo, V35Authority.OraclePath)),
                ["runtimePlanManifest"] = Describe(Path.Combine(repo, V35PlanCompiler.PlanManifestPath)),
                ["generated"] = new JsonArray(new[] { V35PlanCompiler.NativeIdsPath, V35PlanCompiler.NativePlanPath, V35PlanCompiler.NativeAuthorityPath }
                    .Select(p => (JsonNode)Describe(Path.Combine(repo, p))).ToArray()),
            },
            ["toolchain"] = toolchain.DeepClone(),
            ["nativeHelper"] = Native(Path.Combine(run, "I52CtdaNative.arx"), Path.Combine(evidence, "I52CtdaNative")),
            ["payload"] = Native(Path.Combine(run, "I52CtdaPayload.arx"), Path.Combine(evidence, "I52CtdaPayload")),
            ["managedObserver"] = Managed(Path.Combine(run, "I52Ctda.ManagedObserver.dll")),
            ["managedHarness"] = new JsonArray(Directory.GetFiles(Path.Combine(root, "harness"), "I52Ctda.*.dll").OrderBy(f => f, StringComparer.Ordinal).Select(f => (JsonNode)Managed(f)).ToArray()),
            ["linkedLibraries"] = new JsonObject
            {
                ["nativeHelper"] = Libraries(Path.Combine(evidence, "I52CtdaNative.link.read.tlog")),
                ["payload"] = Libraries(Path.Combine(evidence, "I52CtdaPayload.link.read.tlog")),
            },
            ["buildLogs"] = new JsonArray(Directory.GetFiles(Path.Combine(root, "logs")).OrderBy(f => f, StringComparer.Ordinal).Select(f => (JsonNode)Describe(f, root)).ToArray()),
        };
        string compact = Sorted(tuple)!.ToJsonString();
        string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(compact)));
        var document = new JsonObject
        {
            ["buildMachineToolchainTuple"] = Sorted(tuple),
            ["buildTupleHash"] = hash,
            ["buildTupleHashMethod"] = "SHA-256 over UTF-8 compact JSON of buildMachineToolchainTuple with keys sorted ordinally at every level",
        };
        Directory.CreateDirectory(Path.Combine(root, "tuple"));
        File.WriteAllText(Path.Combine(root, "tuple", "build-machine-toolchain-tuple.json"), document.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n", new UTF8Encoding(false));
        Console.WriteLine("NEW_BUILD_MACHINE_TOOLCHAIN_TUPLE_HASH=" + hash);
        return 0;
    }

    private static JsonNode? Sorted(JsonNode? node) => node switch
    {
        JsonObject o => new JsonObject(o.OrderBy(p => p.Key, StringComparer.Ordinal).Select(p => KeyValuePair.Create(p.Key, Sorted(p.Value)))),
        JsonArray a => new JsonArray(a.Select(Sorted).ToArray()),
        null => null,
        _ => node.DeepClone(),
    };

    private static JsonObject Describe(string path, string? relativeTo = null)
    {
        var file = new FileInfo(path);
        return new JsonObject
        {
            ["path"] = relativeTo is null ? file.FullName : Path.GetRelativePath(relativeTo, file.FullName).Replace('\\', '/'),
            ["bytes"] = file.Length,
            ["sha256"] = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))),
        };
    }

    // PE identity: machine, sections from the header and the dumpbin import/export lists captured by build-r3.ps1.
    private static JsonObject Native(string path, string dumpPrefix)
    {
        JsonObject d = Describe(path);
        byte[] bytes = File.ReadAllBytes(path);
        int pe = BitConverter.ToInt32(bytes, 0x3C);
        ushort machine = BitConverter.ToUInt16(bytes, pe + 4);
        d["machine"] = machine == 0x8664 ? "x64 (0x8664)" : $"0x{machine:X4}";
        d["fileVersion"] = FileVersionInfo.GetVersionInfo(path).FileVersion ?? "";
        d["pdb"] = Describe(Path.ChangeExtension(path, ".pdb"));
        string imports = File.ReadAllText(dumpPrefix + ".imports.txt");
        d["importedDlls"] = new JsonArray(imports.Split('\n').Select(l => l.Trim()).Where(l => l.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) && !l.Contains(' '))
            .Select(l => l.ToLowerInvariant()).Distinct().Order(StringComparer.Ordinal).Select(l => (JsonNode)l).ToArray());
        string exports = File.ReadAllText(dumpPrefix + ".exports.txt");
        d["exports"] = new JsonArray(System.Text.RegularExpressions.Regex.Matches(exports, @"^\s+\d+\s+[0-9A-F]+\s+[0-9A-F]{8}\s+(\S+)", System.Text.RegularExpressions.RegexOptions.Multiline)
            .Select(m => m.Groups[1].Value).Order(StringComparer.Ordinal).Select(x => (JsonNode)x).ToArray());
        d["dumpbinImportsSha256"] = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(imports)));
        return d;
    }

    private static JsonObject Managed(string path)
    {
        JsonObject d = Describe(path);
        FileVersionInfo info = FileVersionInfo.GetVersionInfo(path);
        d["informationalVersion"] = info.ProductVersion ?? "";
        d["fileVersion"] = info.FileVersion ?? "";
        string runtimeConfig = Path.ChangeExtension(path, ".deps.json");
        d["targetFramework"] = File.Exists(runtimeConfig)
            ? JsonNode.Parse(File.ReadAllText(runtimeConfig))!["runtimeTarget"]!["name"]!.GetValue<string>() : "UNRECORDED";
        return d;
    }

    // Every library the linker read (MSBuild link.read tlog), with its identity.
    private static JsonArray Libraries(string tlog)
    {
        var result = new JsonArray();
        foreach (string line in File.ReadAllLines(tlog, Encoding.Unicode).Select(l => l.Trim()).Where(l => l.EndsWith(".LIB", StringComparison.OrdinalIgnoreCase)).Distinct().Order(StringComparer.Ordinal))
            result.Add(File.Exists(line) ? Describe(line) : new JsonObject { ["path"] = line, ["availability"] = "NOT_AVAILABLE" });
        return result;
    }
}
