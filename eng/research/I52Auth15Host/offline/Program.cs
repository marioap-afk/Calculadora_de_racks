using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using I52Auth15.HostHarness;

namespace I52Auth15.Offline
{
    /// <summary>
    /// Stand-in for acad.exe, for OFFLINE launcher characterization. Usage (same shape as the real launch):
    ///   offlineacad.exe "&lt;scratch.dwg&gt;" /nologo /nossm [/p x] /b "&lt;script&gt;"
    /// It runs the script through a mini interpreter (a LISP subset: setq if = getvar setvar open write-line close itoa strcat
    /// command; NETLOAD; QUIT) and, when the harness command is invoked after a NETLOAD, plays the harness with the REAL
    /// FilediaRecord and EvidenceDoc. Behaviour is chosen by environment variables:
    ///   I52_OFFLINE_SCENARIO  pass | fail | unknown | malformed | noevidence | hashmismatch | nonzero-exit | timeout | sleep
    ///                         | live-start-shift | live-end-shift | hv00-throws | forged-pass | forged-fail
    ///                         | deviation-continue | deviation-stop | exception-stop | unknown-stop | controls-leak | abort-throws
    ///   I52_OFFLINE_FILEDIA   the FILEDIA value AutoCAD "starts" with (default 1)
    ///   I52_OFFLINE_STICKY    all = setvar FILEDIA is always ignored; lock0 = once FILEDIA is 0 it cannot be raised (the restore fails)
    /// It is NOT AutoCAD and its output is NOT host evidence.
    /// </summary>
    internal static class Program
    {
        private static int _filedia = 1;
        private static string _sticky = string.Empty;
        private static string _scenario = "pass";
        private static bool _harnessLoaded;
        private static string _out;
        private static readonly Dictionary<string, object> Vars = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        private static int Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--selftest")
            {
                return SelfTest();
            }

            if (args.Length > 1 && args[0] == "--sysvar")
            {
                try
                {
                    Console.WriteLine(MockGetSystemVariable(args[1]));
                    return 0;
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine(ex.Message);
                    return 1;
                }
            }

            _scenario = Environment.GetEnvironmentVariable("I52_OFFLINE_SCENARIO") ?? "pass";
            _sticky = Environment.GetEnvironmentVariable("I52_OFFLINE_STICKY") ?? string.Empty;
            _out = Environment.GetEnvironmentVariable("I52_AUTH15_HV_OUT");

            if (int.TryParse(Environment.GetEnvironmentVariable("I52_OFFLINE_FILEDIA"), out var start))
            {
                _filedia = start;
            }

            if (_scenario == "sleep")
            {
                Thread.Sleep(120000);
                return 0;
            }

            var at = Array.IndexOf(args, "/b");

            if (at < 0 || at + 1 >= args.Length)
            {
                Console.Error.WriteLine("offlineacad: no /b script");
                return 9;
            }

            var lines = File.ReadAllLines(args[at + 1]);
            var exit = 0;

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                if (line.Length == 0)
                {
                    continue;
                }

                if (line.StartsWith("(", StringComparison.Ordinal))
                {
                    try
                    {
                        new Interp().Eval(new Parser(line).ParseOne());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("; error: " + ex.Message); // AutoCAD prints the LISP error and the script goes on
                    }

                    continue;
                }

                if (line.Equals("_.NETLOAD", StringComparison.OrdinalIgnoreCase))
                {
                    var path = i + 1 < lines.Length ? lines[++i].Trim() : string.Empty;

                    if (_scenario == "timeout")
                    {
                        Thread.Sleep(120000); // a modal dialog: NETLOAD never returns, FILEDIA is still 0
                    }

                    _harnessLoaded = File.Exists(path) && path.EndsWith("I52Auth15.HostHarness.dll", StringComparison.OrdinalIgnoreCase);
                    continue;
                }

                if (line.Equals("_.QUIT", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                RunCommand(line);
            }

            if (_scenario == "nonzero-exit")
            {
                exit = 5;
            }

            return exit;
        }

        /// <summary>
        /// What the mock accepts, written down independently of SysVarCatalog on purpose: these are the system variables that ARE valid in
        /// AutoCAD 2025. Anything else behaves like AutoCAD does for an unknown name: it throws eInvalidInput. (RUN-1: PROFILENAME.)
        /// This is a characterization aid; the mock does not define AutoCAD semantics.
        /// </summary>
        private static readonly string[] MockValidSystemVariables = { "ACADVER", "CPROFILE", "FILEDIA" };

        private static string MockGetSystemVariable(string name)
        {
            switch (name)
            {
                case "ACADVER":
                    return "25.0s (LMS Tech) [offline mock]";
                case "CPROFILE":
                    return "<<Mock Profile>>";
                case "FILEDIA":
                    return _filedia.ToString(CultureInfo.InvariantCulture);
                default:
                    throw new InvalidOperationException("eInvalidInput: '" + name + "' is not a system variable");
            }
        }

        private static void RunCommand(string name)
        {
            if (name.Equals("I52AUTH15_HOSTVAL", StringComparison.OrdinalIgnoreCase) && _harnessLoaded)
            {
                PlayHarness();
            }
            else
            {
                Console.WriteLine("Unknown command \"" + name + "\".");
            }
        }

        // ---------------------------------------------------------------- the harness, as far as the launcher can tell

        private static void PlayHarness()
        {
            if (_scenario == "noevidence")
            {
                return;
            }

            var evidencePath = Path.Combine(_out, "hostval-evidence.json");

            if (_scenario == "malformed")
            {
                File.WriteAllText(evidencePath, "{ \"schema\": \"I52-AUTH15-HV/1\", \"cases\": [ ");
                return;
            }

            var root = Directory.GetParent(_out).FullName;
            var sums = File.ReadAllLines(Path.Combine(root, "SHA256SUMS"))
                .Select(l => (Path: l.Substring(l.IndexOf("  ", StringComparison.Ordinal) + 2), Hash: l.Substring(0, l.IndexOf("  ", StringComparison.Ordinal))))
                .ToDictionary(x => x.Path, x => x.Hash);
            var meta = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "TRANSFER-METADATA.json"))).RootElement;
            string M(string key) => meta.GetProperty(key).GetString();

            var doc = new EvidenceDoc();
            doc.Header["offlineRig"] = true; // never mistakable for host evidence
            doc.Header["unit"] = "I-52-AUTH15";
            doc.Header["implementationSha"] = M("implementationSha");
            doc.Header["harnessSha"] = M("harnessSha");
            doc.Header["srcTree"] = M("implementationSrcTree");
            doc.Header["testsTree"] = M("implementationTestsTree");
            doc.Header["harnessSrcTree"] = M("harnessSrcTree");
            doc.Header["harnessTestsTree"] = M("harnessTestsTree");
            doc.Header["treesEqual"] = true;
            doc.Package["sha256SumsDigest"] = Sha(Path.Combine(root, "SHA256SUMS"));

            // HC-7: immutable run identity, recorded BEFORE anything fallible. EXPECTED package hashes here; LOADED facts only later.
            using (var self = System.Diagnostics.Process.GetCurrentProcess())
            {
                doc.Host["pid"] = Environment.ProcessId;
                doc.Host["processName"] = self.ProcessName;
                doc.Host["processStartUtc"] = self.StartTime.ToUniversalTime().ToString("o");
            }

            doc.Host["runFolder"] = Path.Combine(root, "run");
            doc.Host["harnessDllPath"] = Path.Combine(root, "run", "I52Auth15.HostHarness.dll");
            doc.Host["harnessDllSha256"] = sums["run/I52Auth15.HostHarness.dll"];
            doc.Package["expectedPluginSha256"] = sums["run/RackCad.Plugin.dll"];
            doc.Package["expectedApplicationSha256"] = sums["run/RackCad.Application.dll"];
            doc.Package["expectedDomainSha256"] = sums["run/RackCad.Domain.dll"];
            doc.Package["expectedHarnessSha256"] = sums["run/I52Auth15.HostHarness.dll"];
            doc.Host["scratchDocumentPath"] = Path.Combine(_out, "scratch.dwg");
            doc.Host["scratchSha256AtHarnessStart"] = Sha(Path.Combine(_out, "scratch.dwg"));
            doc.Host["scratchSha256DeclaredByLauncher"] = Environment.GetEnvironmentVariable("I52_AUTH15_HV_SCRATCH_SHA256");
            doc.Host["ownerNoTouchDeclaredByLauncher"] = Environment.GetEnvironmentVariable("I52_AUTH15_HV_OWNER_NOTOUCH") == "1";
            doc.Host["earlyIdentityErrors"] = new List<string>();

            // The same start-of-run FILEDIA validation the real harness performs, with the real pure logic.
            int? live = _filedia + (_scenario == "live-start-shift" ? 1 : 0);
            var record = FilediaRecord.Parse(File.Exists(Path.Combine(_out, "filedia.txt")) ? File.ReadAllText(Path.Combine(_out, "filedia.txt")) : string.Empty, out var parseError);
            doc.Host["filediaBefore"] = record.Before;
            doc.Host["filediaDuringNetload"] = record.During;
            doc.Host["filediaAfter"] = record.After;
            doc.Host["filediaLiveAtHarnessStart"] = live;
            var problem = parseError ?? record.Validate(live);
            var stopped = problem != null;

            if (stopped)
            {
                doc.Problems.Add("FILEDIA: " + problem);
                doc.StoppedBy = "FILEDIA not restored: INVALID RUN";
            }

            // HV-00, as the harness does it: every system variable it reads must be one the mock (AutoCAD 2025) knows.
            var sensitive = new HashSet<string> { "HV-01", "HV-02", "HV-03", "HV-05", "HV-08", "HV-10", "HV-12", "HV-13", "HV-14" };
            var hv00 = new CaseRecord("HV-00", "BIND", "characterization") { DbKind = "SIDE-DB" };
            var readNames = new List<string>(SysVarCatalog.Audited);

            if (_scenario == "hv00-throws")
            {
                readNames = new List<string> { "ACADVER", "PROFILENAME" }; // the RUN-1 defect, reproduced
            }

            var failedRead = false;

            if (!stopped)
            {
                foreach (var name in readNames)
                {
                    try
                    {
                        var value = MockGetSystemVariable(name);
                        hv00.Check("system variable " + name + " readable", "a value", value, !string.IsNullOrEmpty(value));

                        if (name == "ACADVER")
                        {
                            doc.Host["acadVersion"] = value;
                        }
                        else if (name == "CPROFILE")
                        {
                            doc.Host["profile"] = value;
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        failedRead = true;
                        hv00.Exception = ex.Message; // RUN-1: the exception escaped the case
                    }
                }

                if (!failedRead)
                {
                    doc.Binding["loadedPluginPath"] = Path.Combine(root, "run", "RackCad.Plugin.dll");
                    doc.Binding["loadedPluginSha256"] = _scenario == "hashmismatch" ? new string('0', 64) : sums["run/RackCad.Plugin.dll"];
                    doc.Binding["loadedApplicationPath"] = Path.Combine(root, "run", "RackCad.Application.dll");
                    doc.Binding["loadedApplicationSha256"] = sums["run/RackCad.Application.dll"];
                    doc.Binding["loadedDomainPath"] = Path.Combine(root, "run", "RackCad.Domain.dll");
                    doc.Binding["loadedDomainSha256"] = sums["run/RackCad.Domain.dll"];
                    doc.Binding["loadedHarnessPath"] = Path.Combine(root, "run", "I52Auth15.HostHarness.dll");
                    doc.Binding["loadedHarnessSha256"] = sums["run/I52Auth15.HostHarness.dll"];
                }
            }

            if (!stopped)
            {
                hv00.Finish(); // when the FILEDIA gate stopped the run, HV-00 is NOT_RUN, exactly as in the real harness
            }

            doc.Cases.Add(hv00);

            if (problem != null)
            {
                doc.StopKind = StopKind.Filedia;
            }
            else if (failedRead)
            {
                stopped = true;
                doc.StopKind = StopKind.Hv00;
                doc.StoppedBy = "HV-00 is not PASS: no AUTH-15 call was made";
            }

            if (!stopped)
            {
                doc.DocumentAuthority["available"] = true;
                AddControls(doc);
            }

            var deviationAt = _scenario == "deviation-continue" || _scenario == "deviation-stop" || _scenario == "exception-stop" || _scenario == "unknown-stop" ? "HV-08" : null;

            for (var i = 1; i < 15; i++)
            {
                var id = "HV-" + i.ToString("D2");
                var c = new CaseRecord(id, "X", "y") { DbKind = sensitive.Contains(id) ? "DOCUMENT-AUTHORITY" : "SIDE-DB" };

                if (!stopped)
                {
                    c.Check("a", true, true);

                    if (i == 5 && (_scenario == "fail" || _scenario == "forged-pass"))
                    {
                        c.Check("boom", false, true);
                        c.Leaks.Add("after the abort: added BT:*D1 = 7A");
                    }

                    if (i == 5 && _scenario == "unknown")
                    {
                        c.Unknown("ambiguous", "x", "cannot tell");
                    }

                    if (id == deviationAt && _scenario == "unknown-stop")
                    {
                        c.Unknown("ambiguous", "x", "cannot tell"); // an UNKNOWN verdict: no failing case and no deviation
                    }
                    else if (id == deviationAt)
                    {
                        c.Check("CHARACTERIZATION HeaderRun \"<>\" surfaces as WriteFailed", "WriteFailed", "IsSuccess=True Failure=None BlockName=", false);
                        c.Deviation = true;
                        doc.Deviations.Add(id);
                    }

                    c.Finish();
                }

                doc.Cases.Add(c);

                if (!stopped && sensitive.Contains(id))
                {
                    var side = new CaseRecord(id, "X", "y") { DbKind = "SIDE-DB-CHARACTERIZATION" };
                    side.Check("a", true, true);
                    side.Finish();
                    doc.SideCharacterizations.Add(side);
                }

                if (!stopped && id == deviationAt && _scenario == "deviation-stop")
                {
                    stopped = true;
                    doc.StopKind = StopKind.Deviation;
                    doc.StoppedBy = id + " asked to stop: continuing would make later evidence untrustworthy";
                }
                else if (!stopped && id == deviationAt && (_scenario == "exception-stop" || _scenario == "unknown-stop"))
                {
                    stopped = true;
                    doc.StopKind = _scenario == "exception-stop" ? StopKind.Exception : StopKind.Deviation;
                    doc.StoppedBy = id + " stopped";
                }
            }

            var liveEnd = _filedia + (_scenario == "live-end-shift" ? 1 : 0);
            doc.Host["filediaLiveAtHarnessEnd"] = liveEnd;

            if (record.Before != null && liveEnd != record.Before)
            {
                doc.Problems.Add("FILEDIA at the end of the run differs from the original");
            }

            doc.Completed = !stopped;
            doc.State = "final";
            doc.Write(evidencePath);

            if (_scenario == "forged-pass")
            {
                // A harness that lied about its own verdict: the cases still say FAIL.
                File.WriteAllText(evidencePath, File.ReadAllText(evidencePath).Replace("\"verdict\": \"FAIL\"", "\"verdict\": \"PASS\""));
            }
            else if (_scenario == "forged-fail")
            {
                // The opposite lie: a FAIL verdict with nothing (no failing case, no deviation) behind it.
                File.WriteAllText(evidencePath, File.ReadAllText(evidencePath).Replace("\"verdict\": \"PASS\"", "\"verdict\": \"FAIL\""));
            }
        }

        /// <summary>The rollback controls as the real harness records them: raw outcomes with a database-kind label. They never gate the verdict.</summary>
        private static void AddControls(EvidenceDoc doc)
        {
            void Control(string id, string kind)
            {
                var r = new CaseRecord(id, "control", "raw outcome") { DbKind = kind };
                r.Check("sanity: the control wrote something inside the transaction", "> 0 differences", "9 differences", true);

                var tx = new TxOutcome
                {
                    Label = id + " abort",
                    AbortAttempted = true,
                    AbortSucceeded = _scenario != "abort-throws" || id != "RB-01",
                    AbortError = _scenario == "abort-throws" && id == "RB-01" ? "eInvalidInput: injected" : null,
                    DisposeAttempted = true,
                    DisposeSucceeded = true,
                    IsDisposedAfter = true,
                    ActiveBefore = 1,
                    ActiveAfter = 0,
                    TopAfter = "null",
                };
                r.Tx.Add(tx);

                foreach (var failure in tx.Failures(abortExpected: true))
                {
                    r.Check("transaction end [" + tx.Label + "]: " + failure, "Abort/Dispose succeed and the transaction is disposed", failure, false);
                }

                if (_scenario == "controls-leak" && id == "RB-01" && kind == "SIDE-DB")
                {
                    r.Leaks.Add("RB-01 after the caller's rollback: added BT:CTRL_RB_BLOCK = 7C");
                    r.Leaks.Add("RB-01 after the caller's rollback: added LT:CTRL_RB_LAYER = 7D");
                    r.Check("RB-01 after the caller's rollback: no leak (SNAP identical to before)", "identical", "added BT:CTRL_RB_BLOCK = 7C; added LT:CTRL_RB_LAYER = 7D", false);
                }
                else
                {
                    r.Check("RB after the caller's rollback: no leak (SNAP identical to before)", "identical", "identical", true);
                }

                r.Finish();
                doc.Controls.Add(r);
            }

            Control("RB-01", "SIDE-DB");
            Control("RB-01V", "SIDE-DB");
            Control("RB-01V", "DOCUMENT-AUTHORITY");
            Control("RB-01D", "DOCUMENT-AUTHORITY");

            foreach (var id in new[] { "RB-02a", "RB-02b", "RB-02c", "RB-03", "RB-05" })
            {
                Control(id, "SIDE-DB");
                Control(id, "DOCUMENT-AUTHORITY");
            }
        }

        private static string Sha(string path)
        {
            using (var s = File.OpenRead(path))
            {
                return Convert.ToHexString(SHA256.HashData(s));
            }
        }

        // ---------------------------------------------------------------- self-test of the pure FILEDIA logic

        private static int SelfTest()
        {
            var failures = 0;

            void Expect(string name, string text, int? live, string expectedFragment)
            {
                var record = FilediaRecord.Parse(text, out var error);
                var actual = error ?? record.Validate(live);
                var ok = expectedFragment == null ? actual == null : actual != null && actual.Contains(expectedFragment);
                Console.WriteLine((ok ? "PASS " : "FAIL ") + name + " -> " + (actual ?? "valid"));
                failures += ok ? 0 : 1;
            }

            Expect("original 1, restored to 1, live 1", "before=1\nduring=0\nafter=1\n", 1, null);
            Expect("original 0 (never assume 1)", "before=0\nduring=0\nafter=0\n", 0, null);
            Expect("original 2", "before=2\nduring=0\nafter=2\n", 2, null);
            Expect("restored to the wrong value (hard-coded 1)", "before=2\nduring=0\nafter=1\n", 1, "differs from the original");
            Expect("during is not 0", "before=1\nduring=1\nafter=1\n", 1, "during NETLOAD");
            Expect("live differs from the original", "before=1\nduring=0\nafter=1\n", 0, "live FILEDIA");
            Expect("live unreadable", "before=1\nduring=0\nafter=1\n", null, "unreadable");
            Expect("after missing", "before=1\nduring=0\n", 1, "does not hold");
            Expect("nothing recorded", string.Empty, 1, "does not hold");
            Expect("repeated key", "before=1\nbefore=1\nduring=0\nafter=1\n", 1, "repeated");
            Expect("unknown key", "before=1\nduring=0\nafter=1\nextra=3\n", 1, "unknown or repeated");
            Expect("garbage line", "before=one\n", 1, "unparseable");
            Expect("CRLF endings", "before=1\r\nduring=0\r\nafter=1\r\n", 1, null);

            // TxOutcome: a rollback is only taken as done when Abort and Dispose succeeded and the transaction is disposed.
            var total = 13;

            void ExpectTx(string name, TxOutcome outcome, bool abortExpected, string fragment)
            {
                var found = outcome.Failures(abortExpected);
                var ok = fragment == null ? found.Count == 0 : found.Any(f => f.Contains(fragment));
                Console.WriteLine((ok ? "PASS " : "FAIL ") + name + " -> " + (found.Count == 0 ? "no failure" : string.Join(" | ", found)));
                failures += ok ? 0 : 1;
                total++;
            }

            TxOutcome Good() => new TxOutcome { Label = "t", AbortAttempted = true, AbortSucceeded = true, DisposeAttempted = true, DisposeSucceeded = true, IsDisposedAfter = true };

            ExpectTx("tx: a clean abort and dispose", Good(), true, null);
            var abortThrows = Good();
            abortThrows.AbortSucceeded = false;
            abortThrows.AbortError = "eInvalidInput: x";
            ExpectTx("tx: Abort threw => not a rollback", abortThrows, true, "Abort threw");
            var notAttempted = Good();
            notAttempted.AbortAttempted = false;
            notAttempted.AbortSucceeded = false;
            ExpectTx("tx: Abort never attempted when expected", notAttempted, true, "not attempted");
            var disposeThrows = Good();
            disposeThrows.DisposeSucceeded = false;
            disposeThrows.DisposeError = "boom";
            ExpectTx("tx: Dispose threw", disposeThrows, true, "Dispose threw");
            var stillAlive = Good();
            stillAlive.IsDisposedAfter = false;
            ExpectTx("tx: not disposed after the end", stillAlive, true, "not disposed");
            var alreadyDisposed = Good();
            alreadyDisposed.IsDisposedBefore = true;
            alreadyDisposed.AbortAttempted = false;
            alreadyDisposed.AbortSucceeded = false;
            ExpectTx("tx: already disposed beforehand needs no Abort", alreadyDisposed, true, null);
            var disposeOnly = Good();
            disposeOnly.AbortAttempted = false;
            disposeOnly.AbortSucceeded = false;
            ExpectTx("tx: dispose-only end (abort not expected) is fine", disposeOnly, false, null);

            Console.WriteLine("SELFTEST " + (total - failures) + "/" + total);
            return failures;
        }

        // ---------------------------------------------------------------- mini LISP (the subset run.scr uses)

        private sealed class Sym
        {
            public Sym(string name) => Name = name;

            public string Name { get; }
        }

        private sealed class Parser
        {
            private readonly string _text;
            private int _at;

            public Parser(string text) => _text = text;

            public object ParseOne()
            {
                var node = Read();
                SkipSpace();

                if (_at != _text.Length)
                {
                    throw new InvalidOperationException("trailing text after the expression: " + _text.Substring(_at));
                }

                return node;
            }

            private void SkipSpace()
            {
                while (_at < _text.Length && char.IsWhiteSpace(_text[_at]))
                {
                    _at++;
                }
            }

            private object Read()
            {
                SkipSpace();

                if (_at >= _text.Length)
                {
                    throw new InvalidOperationException("unbalanced parentheses: premature end");
                }

                var ch = _text[_at];

                if (ch == '(')
                {
                    _at++;
                    var list = new List<object>();

                    while (true)
                    {
                        SkipSpace();

                        if (_at >= _text.Length)
                        {
                            throw new InvalidOperationException("unbalanced parentheses: missing )");
                        }

                        if (_text[_at] == ')')
                        {
                            _at++;
                            return list;
                        }

                        list.Add(Read());
                    }
                }

                if (ch == ')')
                {
                    throw new InvalidOperationException("unbalanced parentheses: unexpected )");
                }

                if (ch == '"')
                {
                    var sb = new StringBuilder();
                    _at++;

                    while (_at < _text.Length && _text[_at] != '"')
                    {
                        sb.Append(_text[_at++]);
                    }

                    if (_at >= _text.Length)
                    {
                        throw new InvalidOperationException("unterminated string");
                    }

                    _at++;
                    return sb.ToString();
                }

                var start = _at;

                while (_at < _text.Length && !char.IsWhiteSpace(_text[_at]) && _text[_at] != '(' && _text[_at] != ')' && _text[_at] != '"')
                {
                    _at++;
                }

                var token = _text.Substring(start, _at - start);
                return long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) ? number : (object)new Sym(token);
            }
        }

        private sealed class Interp
        {
            public object Eval(object node)
            {
                if (node is Sym symbol)
                {
                    return Vars.TryGetValue(symbol.Name, out var value) ? value : null;
                }

                if (!(node is List<object> list))
                {
                    return node;
                }

                if (list.Count == 0 || !(list[0] is Sym head))
                {
                    throw new InvalidOperationException("bad form");
                }

                switch (head.Name.ToLowerInvariant())
                {
                    case "setq":
                        return Vars[((Sym)list[1]).Name] = Eval(list[2]);
                    case "if":
                        var truth = Eval(list[1]);
                        return truth != null && !(truth is bool b && !b) ? Eval(list[2]) : list.Count > 3 ? Eval(list[3]) : null;
                }

                var args = list.Skip(1).Select(Eval).ToList();

                switch (head.Name.ToLowerInvariant())
                {
                    case "=":
                        return args[0] != null && args[1] != null && Equals(args[0], args[1]) ? (object)true : null;
                    case "getvar":
                        return string.Equals((string)args[0], "FILEDIA", StringComparison.OrdinalIgnoreCase) ? (object)(long)_filedia : null;
                    case "setvar":
                        if (string.Equals((string)args[0], "FILEDIA", StringComparison.OrdinalIgnoreCase)
                            && _sticky != "all"
                            && !(_sticky == "lock0" && _filedia == 0 && (long)args[1] != 0))
                        {
                            _filedia = (int)(long)args[1];
                        }

                        return args[1];
                    case "itoa":
                        return ((long)args[0]).ToString(CultureInfo.InvariantCulture);
                    case "strcat":
                        return string.Concat(args.Cast<string>());
                    case "open":
                        var dir = Path.GetDirectoryName((string)args[0]);
                        return dir != null && Directory.Exists(dir)
                            ? new StreamWriter((string)args[0], string.Equals((string)args[1], "a", StringComparison.OrdinalIgnoreCase), new UTF8Encoding(false))
                            : null;
                    case "write-line":
                        ((StreamWriter)args[1]).WriteLine((string)args[0]);
                        ((StreamWriter)args[1]).Flush();
                        return args[0];
                    case "close":
                        ((StreamWriter)args[0]).Dispose();
                        return null;
                    case "command":
                        foreach (var name in args.OfType<string>())
                        {
                            RunCommand(name);
                        }

                        return null;
                    default:
                        throw new InvalidOperationException("no such LISP function: " + head.Name);
                }
            }
        }
    }
}
