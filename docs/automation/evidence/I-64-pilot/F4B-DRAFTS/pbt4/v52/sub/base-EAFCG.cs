using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using RackCad.Application.Persistence;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T4b — the script of «Saltar y duplicar» (CT-PBT4B-20; D-PBT4-11), in execution. It is a GUARD: it passes on the skeleton
    /// and on the implementation, because the copies, the kit and the Skip travel in the RED, and it discriminates through its negative
    /// self-tests, run over text injected in memory (never written). EVERY text is normalized to <c>\n</c> before it is compared or hashed
    /// (the repository works with <c>core.autocrlf=true</c>; I-01).
    ///
    /// <para>
    /// (1) Each of the 21 original files is its base but for the lines of attribute that carry their Skip. (2) Each test of the list, but
    /// CT-PBT4A-06, has a test of the same name in its copy. (3) In the nine classes of C-0, each mechanical copy is the text of its method in
    /// the base with the closed map M. (4) Each copy and the kit have the sha256 of the contract. (5) No copy names the kit of the pilot.
    /// (6) The full read that the kit derives is the one that the reader gives. (7) The four Theory split by rows are, together, the original.
    /// </para>
    /// </summary>
    public sealed class PushBackEditAllFullCopyGuardTests
    {
        private static string Root => PilotKit.RepoRoot().FullName;

        // The base of each original: the sha256 of its text in BaseSha with the line ends as \n (section 6.2.1 of the package), and its number of Skip.
        private static readonly (string Path, string BaseSha, int Skips)[] Origins =
        {
            ("tests/RackCad.Tests/PushBackFullReadGuardTests.cs",
             "97dff70cf57d866895eaae78ebde98e4350135743d747018c7de0a0a4758eb75", 2),
            ("tests/RackCad.Tests/PushBackFullReadPathTests.cs",
             "f6611c3ad90d0e8d481b6a2b56788b53d37f3f0f08af4f1ae5d3cb81783ab28f", 1),
            ("tests/RackCad.Tests/PushBackWritePathGuardTests.cs",
             "996adc5d4ddfc80462e7ade23ef44a8f18fabd0fdbf814b88f2505f437af65b6", 1),
            ("tests/RackCad.Tests/Workspace/WorkspaceAssociativeBookkeepingTests.cs",
             "07573661e65b0a4658796a92bf90fd33e79c667a3ccf2f4615588519899bf3f1", 10),
            ("tests/RackCad.Tests/Workspace/WorkspaceCommandAdvisorTests.cs",
             "27f26a28a760225bed676f8ca0d52fb1fd4b64d1d601f64bfdfb1b83f6786a66", 54),
            ("tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs",
             "da9d341f6b544d3d85eaafdd01cffe7796fa5367a8ead1e2f0071e2110e5cab4", 7),
            ("tests/RackCad.Tests/Workspace/WorkspaceHostHardeningGuardTests.cs",
             "f52072b53b819edd270e05ec38c987a54aa6772fbe09c18aee71afb4f4067b80", 1),
            ("tests/RackCad.Tests/Workspace/WorkspaceEditIntentHostTests.cs",
             "df1dd9e118ce78162b304052f64f356cfac30d8c7fd9e6a8ea1eb6f680887b0b", 62),
            ("tests/RackCad.Tests/Workspace/WorkspaceLifecycleGuardTests.cs",
             "4044c8678c60a0e05cef7eb6fa8d6b8161651f4dbf17f8b1ced04498b3975997", 2),
            ("tests/RackCad.Tests/Workspace/WorkspaceLifecycleHostTests.cs",
             "1808ac7a22c33938b575063e3b7a049450f187b493c5b09b4464ddc37acfc5f0", 69),
            ("tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs",
             "1ef3b59be954177391a0a0c5df07d2daa65728bb90d6f4d20dd46e8531903d35", 5),
            ("tests/RackCad.Tests/Workspace/WorkspacePilotEditHostTests.cs",
             "43afc8f34423a9ee00bc830c3b78473cebc71f08a3f3bdb4551f1e751148400b", 59),
            ("tests/RackCad.Tests/Workspace/WorkspacePilotPrepareTests.cs",
             "d85407d0961082f01066c61a6cd6e21e95038f001572660d00f7414737390ade", 67),
            ("tests/RackCad.Tests/Workspace/WorkspacePilotReconciliationGuardTests.cs",
             "48dead3420e3ac272bb05e43732bae50d8e47eccacf09bb8f37578acc5d35494", 1),
            ("tests/RackCad.Tests/Workspace/WorkspacePilotReconciliationHostTests.cs",
             "e6557a8032ef69f37f6b4a91f1c918510a19d13f64d5bff61912b1b91f3a0aac", 28),
            ("tests/RackCad.Tests/Workspace/WorkspacePilotUpdateGuardTests.cs",
             "e7db8e72cb4bfcd182f686b907128c8428f59fd14ec812365fd1b10a5dc089fc", 1),
            ("tests/RackCad.Tests/Workspace/WorkspacePilotUpdateHostTests.cs",
             "d602daf657aae175f9972052e1594034959c0706fd2466644f740d981bfa18fe", 35),
            ("tests/RackCad.Tests/Workspace/WorkspacePilotUpdateIntegrationGuardTests.cs",
             "d5c075112c4eee1e47ebdcae641e80c78d10cb4fda4a5b70eac8833bddccafb2", 2),
            ("tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRequestTests.cs",
             "cabfb232768fe69e1e7b686722fa87d05efe97b44474c1b4b8a411db598b130e", 44),
            ("tests/RackCad.UI.Tests/Workspace/WorkspacePilotReconciliationTests.cs",
             "1cedca91c973b0dfe8825e002bfcab4fc883d71219bf7fc4612f19dc4ea8300a", 21),
            ("tests/RackCad.UI.Tests/Workspace/WorkspacePilotReconciliationViewTests.cs",
             "e8db638a72da2a517cba2ea66a762a79b25ddd75621e9d3e95c07ef97a2bd568", 6),
        };

        // The sha256 of each copy: its text with the line ends as LF (CopySha of the contract).
        private static readonly Dictionary<string, string> CopySha = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["tests/RackCad.Tests/PushBackFullReadGuardFull.cs"] =
                "ee09002b03cc3231c76ccfe4900919ee1241ffc85ea5d42d8d83278cbba3aa35",
            ["tests/RackCad.Tests/PushBackFullReadPathFull.cs"] =
                "8d4cfb2eebdbcfc0615243368feca221a4f4834bef704abb4e097528d9039bc7",
            ["tests/RackCad.Tests/PushBackWritePathGuardFull.cs"] =
                "38cb158d8464ff303eaadddf9e1722e49057618d39bb83b9ee32150fd3ea5a3c",
            ["tests/RackCad.Tests/Workspace/WorkspaceAssociativeBookkeepingFull.cs"] =
                "5d54905f0a3f76ba786dce1d0adaa89eee3fe50c867197e575d50b07cf958a01",
            ["tests/RackCad.Tests/Workspace/WorkspaceCommandAdvisorFull.cs"] =
                "af0929f95082d0bd896ed11afbdc2f1cdc6f10749e34ab678193955fcf924522",
            ["tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardFull.cs"] =
                "a6c90caec73f6f440b2228e60279f478372409cce0bdce8504c7cc29d5da76ed",
            ["tests/RackCad.Tests/Workspace/WorkspaceEditIntentHostFull.cs"] =
                "c269eac4774b6f5bf5ee20c7a9a2a02097b15060c6e6c3b8d5dd32d9f316ad9e",
            ["tests/RackCad.Tests/Workspace/WorkspaceHostHardeningGuardFull.cs"] =
                "f82806835c6da858abda885bbfc8296b28c7952d64038e72eebbabd5cb27d9ea",
            ["tests/RackCad.Tests/Workspace/WorkspaceLifecycleGuardFull.cs"] =
                "f4523b11a73c223ece1bbd930a578f9be637cb38af8a2066b110a060bfd1dac7",
            ["tests/RackCad.Tests/Workspace/WorkspaceLifecycleHostFull.cs"] =
                "91aa5c15a095ddcdafcba74d73c2e0a78d013d8bfe53a4d4f3af6708cf075c16",
            ["tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardFull.cs"] =
                "cfa15cd304f3167ba3140d286a0729dd05423697f3f344e7bdaa5679f8528b8a",
            ["tests/RackCad.Tests/Workspace/WorkspacePilotEditHostFull.cs"] =
                "7fed50bac4d5bc4bfe440f75c6eac37647142c0538f42278cb9612c5f9fdc95c",
            ["tests/RackCad.Tests/Workspace/WorkspacePilotPrepareFull.cs"] =
                "31554def2b2fbb040d1e440b8987ce6e2057be3027bb5447306f4da2862eb5f8",
            ["tests/RackCad.Tests/Workspace/WorkspacePilotReconciliationGuardFull.cs"] =
                "b07cf90219fa182786e2a731c6a5e6fbc06cabe4480bd811418e4b8ab78f7bfc",
            ["tests/RackCad.Tests/Workspace/WorkspacePilotReconciliationHostFull.cs"] =
                "83980964cfd66ae88dc0d93f2f6e9e6e9d3bac5a60ce99698039230d96f0c97d",
            ["tests/RackCad.Tests/Workspace/WorkspacePilotUpdateGuardFull.cs"] =
                "30a47920ea337b6620472a2614a3cad2c8a699e3238259fb0741b348535911c4",
            ["tests/RackCad.Tests/Workspace/WorkspacePilotUpdateHostFull.cs"] =
                "4ba5042576e0ca28771120ad322c18f7021b870db49999b5ce66a430f398b4f0",
            ["tests/RackCad.Tests/Workspace/WorkspacePilotUpdateIntegrationGuardFull.cs"] =
                "889f5d86a7097db0cecd9e6bb216e5c587d7c5e934eea8d37f048f667b0cd383",
            ["tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRequestFull.cs"] =
                "e81cc9d319544140c31850bb8f876d884f19a3b0f77d2ade3184b25209ca0fbb",
            ["tests/RackCad.UI.Tests/Workspace/WorkspacePilotReconciliationFull.cs"] =
                "985f9b1ff90da56e7093a6675196f7586436842eedea643991bc1c9d6d3c502d",
            ["tests/RackCad.UI.Tests/Workspace/WorkspacePilotReconciliationViewFull.cs"] =
                "54953f62130a7dc3f4f57b43039bbffd291ae628957766e90e427d1f9f792bdf",
        };

        private const string KitPath = "tests/RackCad.Tests/Workspace/WorkspaceFullReadKit.cs";
        private const string KitSha = "b4ed29f92838e5761faf1afaba1f33e58ba82a56541900f52db8c5ecb6239b9a";


        // The nine classes of C-0 (the mechanical copies and the equivalents written by hand), and the numbers of the map of the copy v5.
        private static readonly Dictionary<string, (int Tests, int Mechanical, int Hand)> NineCounts = new Dictionary<string, (int, int, int)>(StringComparer.Ordinal)
        {
            ["WorkspaceAssociativeBookkeeping"] = (10, 10, 0),
            ["WorkspaceCommandAdvisor"] = (54, 54, 0),
            ["WorkspaceEditIntentHost"] = (62, 57, 5),
            ["WorkspaceLifecycleHost"] = (69, 68, 1),
            ["WorkspacePilotEditHost"] = (59, 53, 6),
            ["WorkspacePilotPrepare"] = (67, 46, 21),
            ["WorkspacePilotReconciliationHost"] = (28, 17, 11),
            ["WorkspacePilotUpdateHost"] = (35, 12, 23),
            ["WorkspacePilotUpdateRequest"] = (44, 39, 5),
        };

        // The tests of the other twelve originals that have a copy (the guards and the two of the interface); the CT of the census has none.
        private static readonly Dictionary<string, int> OtherCounts = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["PushBackFullReadGuard"] = 1,
            ["PushBackFullReadPath"] = 1,
            ["PushBackWritePathGuard"] = 1,
            ["WorkspaceEditIntentGuard"] = 7,
            ["WorkspaceHostHardeningGuard"] = 1,
            ["WorkspaceLifecycleGuard"] = 2,
            ["WorkspacePilotEditGuard"] = 5,
            ["WorkspacePilotReconciliationGuard"] = 1,
            ["WorkspacePilotUpdateGuard"] = 1,
            ["WorkspacePilotUpdateIntegrationGuard"] = 2,
            ["WorkspacePilotReconciliation"] = 21,
            ["WorkspacePilotReconciliationView"] = 6,
        };

        // M-00: the types of the nine classes that are copied with the suffix Full (the others of the nine have none).
        private static readonly Dictionary<string, string[]> TypesOfTheCopy = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["WorkspaceCommandAdvisor"] = new[] { "CommandAdvisorPortDouble" },
            ["WorkspaceLifecycleHost"] = new[] { "LifecycleScriptedAdvisor" },
            ["WorkspacePilotEditHost"] = new[] { "PilotEditThrowingComparer", "PilotEditThrowingProbe", "PilotEditAuditedReader", "PilotEditOutcomeProbe" },
        };

        // M-01 to M-08, closed and in this order.
        private static readonly (string Pattern, string Replacement)[] MapM =
        {
            (@"\bEditIntentFakeReader\b", "FullEditIntentFakeReader"),
            (@"\bEditIntentKit\b", "FullEditIntentKit"),
            (@"\bPrepareKit\b", "FullPrepareKit"),
            (@"\bPushBackPilotDraftState\b", "PushBackFullDraftState"),
            (@"\bPushBackPilotStateComparer\b", "PushBackFullDraftStateComparer"),
            (@"\bPushBackPilotFieldAdapter\b", "PushBackFullFieldAdapter"),
            (@"\.Inputs\.Annotations!?\.AnnotationScale\b", ".Editable.AnnotationScale"),
            (@"\.Inputs\.PalletTolerance\b", ".Editable.PalletTolerance"),
        };

        // The four Theory that the partition of the RED splits by rows (D-S5): the class, the method and whether their rows are a MemberData.
        private static readonly (string Stem, string Method, bool Member)[] Splits =
        {
            ("WorkspaceLifecycleHost", "EachOperationThatCanChangeADraftEndsTheMarkSoTheSessionIsRetiredConservatively", false),
            ("WorkspacePilotEditGuard", "TheHostGuardTripsOnAPublicOperationAnOrderOfBasesOrAnAdapterThatIsAFieldOfInstance", false),
            ("WorkspacePilotPrepare", "ConfirmAppliesOnlyToCleanDirtyAndConflictAndTheOthersReadNothing", true),
            ("WorkspacePilotPrepare", "KeepAppliesToConflictAndToDirtyMarkedStaleApplyingWaitsAndTheOthersAreNotNeededAllWithoutReading", true),
        };

        private static readonly Dictionary<string, (int Red, int Green)> SplitRows = new Dictionary<string, (int, int)>(StringComparer.Ordinal)
        {
            ["WorkspaceLifecycleHost.EachOperationThatCanChangeADraftEndsTheMarkSoTheSessionIsRetiredConservatively"] = (3, 7),
            ["WorkspacePilotEditGuard.TheHostGuardTripsOnAPublicOperationAnOrderOfBasesOrAnAdapterThatIsAFieldOfInstance"] = (1, 3),
            ["WorkspacePilotPrepare.ConfirmAppliesOnlyToCleanDirtyAndConflictAndTheOthersReadNothing"] = (7, 1),
            ["WorkspacePilotPrepare.KeepAppliesToConflictAndToDirtyMarkedStaleApplyingWaitsAndTheOthersAreNotNeededAllWithoutReading"] = (7, 1),
        };

        private static readonly Regex SkipLine = new Regex(
            @"^([ \t]*)\[(Fact|Theory)\(Skip = ""I-64 F4B PB-T4b: [^""]*""\)\]", RegexOptions.Multiline);

        private static readonly Regex KitOfThePilot = new Regex(
            @"\b(?:EditIntentFakeReader|EditIntentKit|PrepareKit|PushBackPilotDraftState|PushBackPilotStateComparer|PushBackPilotFieldAdapter|PushBackFullReadRig)\b");

        // ---------------------------------------------------------------------------------------------- helpers

        /// <summary>The «EXCEPCION» rule: a step that throws becomes a line, and the expected lines never contain one.</summary>
        private static T Paso<T>(List<string> log, Func<T> step)
        {
            try
            {
                return step();
            }
            catch (Exception error)
            {
                log.Add("EXCEPCION " + error.GetType().Name + ": " + error.Message);
                return default(T);
            }
        }

        private static string Normalize(string text) => text.Replace("\r\n", "\n");

        private static string ReadText(string relative) => Normalize(File.ReadAllText(Path.Combine(Root, relative)));

        private static string Sha(string text)
        {
            using (var sha = SHA256.Create())
            {
                return string.Concat(sha.ComputeHash(new UTF8Encoding(false).GetBytes(text)).Select(b => b.ToString("x2")));
            }
        }

        // I-64 F4B PB-T4c (owner-pbt4c-list-3-decision.md): the one Skip of PB-T4c in an original, the one of WorkspacePilotEditGuardTests:155,
        // goes back to its [Fact] too, by its exact line; the count of the Skip of PB-T4b and the expected lines do not change.
        private const string EditAllSkipLine = "    [Fact(Skip = \"I-64 F4B PB-T4c: sustituida por su equivalente en PushBackEditAllPortEquivalenceGuardTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 4).\")]";

        private static string Restore(string text) => SkipLine.Replace(text, "$1[$2]").Replace(EditAllSkipLine + "\n", "    [Fact]\n");

        private static string Stem(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path);
            return name.EndsWith("Tests", StringComparison.Ordinal) ? name.Substring(0, name.Length - "Tests".Length) : name;
        }

        private static string CopyPathOf(string path) => path.Substring(0, path.Length - "Tests.cs".Length) + "Full.cs";

        private static string ApplyMap(string stem, string text)
        {
            foreach (var (pattern, replacement) in MapM) text = Regex.Replace(text, pattern, replacement);
            if (TypesOfTheCopy.TryGetValue(stem, out var types))
            {
                foreach (var type in types) text = Regex.Replace(text, @"\b" + Regex.Escape(type) + @"\b", type + "Full");
            }

            return text;
        }

        // ---------------------------------------------------------------------------------------------- the lexicon and the members

        private static void Blank(StringBuilder builder, string code, int from, int to)
        {
            for (var x = from; x < to; x++) builder.Append(code[x] == '\n' ? '\n' : ' ');
        }

        /// <summary>The text with the comments and the inside of the strings and of the characters blanked, without moving a character.</summary>
        private static string Mask(string code)
        {
            var builder = new StringBuilder(code.Length);
            var i = 0;
            var n = code.Length;
            while (i < n)
            {
                var c = code[i];
                if (c == '/' && i + 1 < n && code[i + 1] == '/')
                {
                    var end = code.IndexOf('\n', i);
                    if (end < 0) end = n;
                    Blank(builder, code, i, end);
                    i = end;
                    continue;
                }

                if (c == '/' && i + 1 < n && code[i + 1] == '*')
                {
                    var end = code.IndexOf("*/", i + 2, StringComparison.Ordinal);
                    end = end < 0 ? n : end + 2;
                    Blank(builder, code, i, end);
                    i = end;
                    continue;
                }

                var k = i;
                var verbatim = false;
                var interpolated = false;
                while (k < n && (code[k] == '$' || code[k] == '@'))
                {
                    if (code[k] == '@') verbatim = true;
                    else interpolated = true;
                    k++;
                }

                if (k < n && code[k] == '"' && (k > i || i == 0 || !(char.IsLetterOrDigit(code[i - 1]) || code[i - 1] == '_')))
                {
                    var j = k + 1;
                    var depth = 0;
                    var closed = false;
                    while (j < n)
                    {
                        var ch = code[j];
                        if (interpolated && ch == '{')
                        {
                            if (depth == 0 && j + 1 < n && code[j + 1] == '{')
                            {
                                j += 2;
                                continue;
                            }

                            depth++;
                        }
                        else if (interpolated && ch == '}' && depth > 0)
                        {
                            depth--;
                        }
                        else if (depth == 0)
                        {
                            if (verbatim && ch == '"')
                            {
                                if (j + 1 < n && code[j + 1] == '"')
                                {
                                    j += 2;
                                    continue;
                                }

                                closed = true;
                                break;
                            }

                            if (!verbatim && ch == '\\')
                            {
                                j += 2;
                                continue;
                            }

                            if (!verbatim && ch == '"')
                            {
                                closed = true;
                                break;
                            }

                            if (!verbatim && ch == '\n') break;
                        }

                        j++;
                    }

                    j = Math.Min(j, n);
                    builder.Append(code, i, k + 1 - i);
                    Blank(builder, code, k + 1, j);
                    if (closed && j < n)
                    {
                        builder.Append('"');
                        j++;
                    }

                    i = j;
                    continue;
                }

                if (c == '\'')
                {
                    var j = i + 1;
                    if (j < n && code[j] == '\\')
                    {
                        j += 2;
                        while (j < n && code[j] != '\'' && code[j] != '\n') j++;
                    }
                    else
                    {
                        j++;
                    }

                    if (j < n && code[j] == '\'')
                    {
                        builder.Append('\'');
                        Blank(builder, code, i + 1, j);
                        builder.Append('\'');
                        i = j + 1;
                        continue;
                    }
                }

                builder.Append(c);
                i++;
            }

            return builder.ToString();
        }

        private sealed class Method
        {
            public string Name { get; set; }

            public int Start { get; set; }

            /// <summary>The attribute lines above the declaration.</summary>
            public string Header { get; set; }

            /// <summary>From the first line of attribute to the closing brace, without the line end.</summary>
            public string Text { get; set; }
        }

        private static readonly Regex Declaration = new Regex(@"\bpublic\s+(?:async\s+)?(?:void|Task)\s+(\w+)\s*\(");

        /// <summary>The tests of a text ([Fact] and [Theory]) with their attribute lines and their text, from the first attribute to the closing brace.</summary>
        private static List<Method> Methods(string text)
        {
            var masked = Mask(text);
            var lines = masked.Split('\n');
            var starts = new int[lines.Length];
            for (var line = 1; line < lines.Length; line++) starts[line] = starts[line - 1] + lines[line - 1].Length + 1;

            var result = new List<Method>();
            foreach (Match match in Declaration.Matches(masked))
            {
                var at = Array.BinarySearch(starts, match.Index);
                if (at < 0) at = ~at - 1;
                var first = at - 1;
                while (first >= 0 && lines[first].TrimStart().StartsWith("[", StringComparison.Ordinal)) first--;
                first++;
                if (first == at) continue;
                var attributes = string.Join("\n", lines.Skip(first).Take(at - first));
                if (!Regex.IsMatch(attributes, @"\[(?:Fact|Theory)\b")) continue;

                var depth = 1;
                var p = match.Index + match.Length;
                while (p < masked.Length && depth > 0)
                {
                    if (masked[p] == '(') depth++;
                    else if (masked[p] == ')') depth--;
                    p++;
                }

                while (p < masked.Length && char.IsWhiteSpace(masked[p])) p++;
                int close;
                if (p < masked.Length && masked[p] == '{')
                {
                    var braces = 0;
                    close = p;
                    for (; close < masked.Length; close++)
                    {
                        if (masked[close] == '{') braces++;
                        else if (masked[close] == '}' && --braces == 0) break;
                    }
                }
                else
                {
                    close = masked.IndexOf(';', p);
                }

                var lineEnd = text.IndexOf('\n', Math.Min(close, text.Length - 1));
                if (lineEnd < 0) lineEnd = text.Length;
                var start = starts[first];
                result.Add(new Method
                {
                    Name = match.Groups[1].Value,
                    Start = start,
                    Header = text.Substring(start, starts[at] - start),
                    Text = text.Substring(start, lineEnd - start),
                });
            }

            return result;
        }

        /// <summary>The tests of an original that carry the Skip of PB-T4b, with its reason.</summary>
        private static List<(string Name, string Reason)> Listed(string original)
            => Methods(original)
                .Select(method => (method.Name, Match: Regex.Match(method.Header, @"Skip = ""(I-64 F4B PB-T4b: [^""]*)""")))
                .Where(item => item.Match.Success)
                .Select(item => (item.Name, item.Match.Groups[1].Value))
                .ToList();

        /// <summary>CT-PBT4A-06 is replaced by CT-PBT4B-13 and has no copy; every other test of the list has one.</summary>
        private static bool HasCopy(string reason) => !reason.Contains("CT-PBT4B-13");

        // ---------------------------------------------------------------------------------------------- the lines

        private static string OriginalLine((string Path, string BaseSha, int Skips) origin, string text)
            => "original " + Stem(origin.Path) + " igual-a-la-base=" + (Sha(Restore(text)) == origin.BaseSha) + " skip=" + SkipLine.Matches(text).Count;

        private static string CopyLine((string Path, string BaseSha, int Skips) origin, string original, string copy)
        {
            var stem = Stem(origin.Path);
            var copySha = CopySha[CopyPathOf(origin.Path)];
            var listed = Listed(original).Where(item => HasCopy(item.Reason)).ToList();
            var copyMethods = Methods(copy);
            if (!NineCounts.ContainsKey(stem))
            {
                var present = listed.Count(item => copyMethods.Any(method => method.Name == item.Name));
                return "copia " + stem + " pruebas=" + present + " sha256=" + (Sha(copy) == copySha);
            }

            var baseMethods = Methods(Restore(original)).GroupBy(method => method.Name).ToDictionary(group => group.Key, group => group.First());
            var tests = 0;
            var mechanical = 0;
            var hand = 0;
            var equal = true;
            foreach (var item in listed)
            {
                var found = copyMethods.Where(method => method.Name == item.Name).ToList();
                if (found.Count == 0) continue;
                tests++;
                if (item.Reason.Contains("su equivalente escrito a mano en"))
                {
                    hand++;
                    continue;
                }

                mechanical++;
                equal = equal && found.Count == 1 && baseMethods.ContainsKey(item.Name) && found[0].Text == ApplyMap(stem, baseMethods[item.Name].Text);
            }

            return "copia " + stem + " pruebas=" + tests + " mecanicas=" + mechanical + " a-mano=" + hand + " igual-al-mapa=" + equal + " sha256=" + (Sha(copy) == copySha);
        }

        private static List<string> RowsOf(Method method)
            => method.Header.Split('\n').Where(line => line.TrimStart().StartsWith("[InlineData(", StringComparison.Ordinal)).ToList();

        private static List<string> MemberRows(string typeName, string member)
        {
            var type = typeof(WorkspaceFullCopyGuardTests).Assembly.GetType(typeName);
            var method = type == null ? null : type.GetMethod(member, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var rows = method == null ? null : method.Invoke(null, null) as System.Collections.IEnumerable;
            return rows == null ? new List<string> { "<sin filas>" } : rows.Cast<object[]>().Select(row => Convert.ToString(row[0])).ToList();
        }

        private static string SplitLine((string Stem, string Method, bool Member) split, string original, string copy)
        {
            var label = split.Stem + "." + split.Method;
            var redAt = copy.IndexOf("class FullPanelCopy" + split.Stem + "RedTests", StringComparison.Ordinal);
            var greenAt = copy.IndexOf("class FullPanelCopy" + split.Stem + "GreenTests", StringComparison.Ordinal);
            var methods = Methods(copy).Where(method => method.Name == split.Method).ToList();
            var inRed = methods.Where(method => redAt >= 0 && method.Start > redAt && (greenAt < 0 || method.Start < greenAt)).ToList();
            var inGreen = methods.Where(method => greenAt >= 0 && method.Start > greenAt).ToList();
            if (redAt < 0 || greenAt < redAt || inRed.Count != 1 || inGreen.Count != 1) return "particion-por-filas " + label + " sin-las-dos-mitades";

            List<string> redRows;
            List<string> greenRows;
            List<string> originalRows;
            if (split.Member)
            {
                redRows = MemberRows("RackCad.Tests.FullPanelCopy" + split.Stem + "RedTests", "StatusNamesRed");
                greenRows = MemberRows("RackCad.Tests.FullPanelCopy" + split.Stem + "GreenTests", "StatusNamesGreen");
                originalRows = MemberRows("RackCad.Tests." + split.Stem + "Tests", "StatusNames");
            }
            else
            {
                redRows = RowsOf(inRed[0]);
                greenRows = RowsOf(inGreen[0]);
                var source = Methods(Restore(original)).First(method => method.Name == split.Method);
                originalRows = RowsOf(source).Select(row => ApplyMap(split.Stem, row)).ToList();
            }

            var together = redRows.Concat(greenRows).OrderBy(row => row, StringComparer.Ordinal).ToList();
            var same = together.SequenceEqual(originalRows.OrderBy(row => row, StringComparer.Ordinal), StringComparer.Ordinal) && together.Distinct().Count() == together.Count;
            return "particion-por-filas " + label + " red=" + redRows.Count + " green=" + greenRows.Count + " filas-del-original=" + same;
        }

        private static (string Path, string BaseSha, int Skips) OriginOf(string stem) => Origins.First(origin => Stem(origin.Path) == stem);

        // ------------------------------------------------------------------------------------------------- CT-20

        [Fact]
        public void CT_PBT4B_20_TheScriptOfSkipAndDuplicateGivesTheCopiesTheKitAndTheSkipsOfTheClosedList()
        {
            var log = new List<string>();
            var expected = new List<string>();
            var originals = new Dictionary<string, string>(StringComparer.Ordinal);
            var copies = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var origin in Origins)
            {
                originals[origin.Path] = Paso(log, () => ReadText(origin.Path)) ?? string.Empty;
                copies[origin.Path] = Paso(log, () => ReadText(CopyPathOf(origin.Path))) ?? string.Empty;
            }

            // (1) The 21 originals.
            foreach (var origin in Origins)
            {
                log.Add(OriginalLine(origin, originals[origin.Path]));
                expected.Add("original " + Stem(origin.Path) + " igual-a-la-base=True skip=" + origin.Skips);
            }

            // (2), (3) and (4): the nine classes of C-0 and then the twelve copies of the guards and of the interface.
            foreach (var origin in Origins.Where(origin => NineCounts.ContainsKey(Stem(origin.Path))))
            {
                log.Add(Paso(log, () => CopyLine(origin, originals[origin.Path], copies[origin.Path])));
                var counts = NineCounts[Stem(origin.Path)];
                expected.Add("copia " + Stem(origin.Path) + " pruebas=" + counts.Tests + " mecanicas=" + counts.Mechanical + " a-mano=" + counts.Hand + " igual-al-mapa=True sha256=True");
            }

            foreach (var origin in Origins.Where(origin => !NineCounts.ContainsKey(Stem(origin.Path))))
            {
                log.Add(Paso(log, () => CopyLine(origin, originals[origin.Path], copies[origin.Path])));
                expected.Add("copia " + Stem(origin.Path) + " pruebas=" + OtherCounts[Stem(origin.Path)] + " sha256=True");
            }

            var kit = Paso(log, () => ReadText(KitPath)) ?? string.Empty;
            log.Add("kit sha256=" + (Sha(kit) == KitSha));
            expected.Add("kit sha256=True");

            // (7) The four Theory split by rows.
            foreach (var split in Splits)
            {
                var origin = OriginOf(split.Stem);
                log.Add(Paso(log, () => SplitLine(split, originals[origin.Path], copies[origin.Path])));
                var rows = SplitRows[split.Stem + "." + split.Method];
                expected.Add("particion-por-filas " + split.Stem + "." + split.Method + " red=" + rows.Red + " green=" + rows.Green + " filas-del-original=True");
            }

            // (6) M-05: the full read that the kit derives is the one that the reader gives, over five scans.
            var scans = Paso(log, () => new[]
            {
                FullEditIntentKit.TwoViews(FullEditIntentKit.RackA),
                FullPrepareKit.RealScan(),
                IntentKit.Scan(new RackEmbedStore().Deserialize(PilotKit.Text("pb-20-ab")).Id, "H1", IntentKit.Member("H1", PilotKit.Text("pb-20-ab"))),
                IntentKit.Scan(
                    IntentKit.Rack,
                    "A1",
                    IntentKit.Member("A1", IntentKit.Rewrite(IntentKit.BaseText(), e => e.Design = I58F1Fixtures.Set(I58F1Fixtures.Rich("pushback"), "PushBack.Future", "1")))),
                IntentKit.Scan(FullEditIntentKit.RackA, "ZZ", IntentKit.Member("A1", IntentKit.Variant("pb-01-base", FullEditIntentKit.RackA))),
            });
            log.Add("kit FullOf-igual-al-lector=" + (scans != null && scans.All(scan => FullHandKit.FullOfMatchesTheReader(scan))) + " escaneos=" + (scans == null ? 0 : scans.Length));
            expected.Add("kit FullOf-igual-al-lector=True escaneos=5");

            // (5) No copy of the nine classes of C-0 names the kit of the pilot (the guards name the state of the pilot in the text they inject).
            log.Add("sin-nombres-del-kit-del-piloto=" + !Origins.Where(origin => NineCounts.ContainsKey(Stem(origin.Path))).Any(origin => KitOfThePilot.IsMatch(copies[origin.Path])));
            expected.Add("sin-nombres-del-kit-del-piloto=True");

            // The self-tests, over text injected in memory: each makes the guard see another line than the real one.
            var sample = OriginOf("WorkspaceAssociativeBookkeeping");
            var original = originals[sample.Path];
            var copy = copies[sample.Path];
            var realOriginal = OriginalLine(sample, original);
            var realCopy = Paso(log, () => CopyLine(sample, original, copy));
            log.Add("autoprueba-linea-de-mas=" + (OriginalLine(sample, original + "\n// una linea de mas\n") == realOriginal ? "pasa" : "falla"));
            var changedAssertion = new Regex(@"Assert\.Equal\(").Replace(copy, "Assert.NotEqual(", 1);
            log.Add("autoprueba-asercion-cambiada=" + (Paso(log, () => CopyLine(sample, original, changedAssertion)) == realCopy ? "pasa" : "falla"));
            var extraSkip = original + "\n    [Fact(Skip = \"I-64 F4B PB-T4b: otra\")]\n    public void Otra() { }\n";
            log.Add("autoprueba-skip-de-mas=" + (OriginalLine(sample, extraSkip) == realOriginal ? "pasa" : "falla"));
            var firstName = Listed(original).First().Name;
            var withoutCopy = new Regex(@"\bpublic void " + Regex.Escape(firstName) + @"\(").Replace(copy, "public void Renombrada(", 1);
            log.Add("autoprueba-sin-copia=" + (Paso(log, () => CopyLine(sample, original, withoutCopy)) == realCopy ? "pasa" : "falla"));
            expected.Add("autoprueba-linea-de-mas=falla");
            expected.Add("autoprueba-asercion-cambiada=falla");
            expected.Add("autoprueba-skip-de-mas=falla");
            expected.Add("autoprueba-sin-copia=falla");

            Assert.Equal(string.Join("\n", expected), string.Join("\n", log));
        }
    }
}
