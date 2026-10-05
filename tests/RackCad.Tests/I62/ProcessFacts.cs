#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace RackCad.Tests
{
    /// <summary>One process of a snapshot (<c>Get-CimInstance Win32_Process</c>; README §3.2). A null command line is an unreadable one.</summary>
    public sealed record ProcessRow(int Pid, int ParentPid, string Name, string? CommandLine, DateTime CreationUtc);

    /// <summary>One classified process (<c>Processes[].Classification</c>).</summary>
    public sealed record ClassifiedProcess(ProcessRow Process, string Classification);

    /// <summary>
    /// The process check of the relay (AUTOMATION_PLAN 16.4 and agent-execution README §3.2; C-17): the classes own-tree, owner-app,
    /// nominal-exclusion, build-server, participant and unattributable, plus session-descendant and orphan for a Worker subagent, and the STOP of a
    /// foreign participant, an unattributable process or, at the entry of a Worker subagent, a new session descendant or a live orphan (P-02). It works
    /// on a snapshot taken by the session; it never kills, waits for or starts a process.
    /// </summary>
    public static class ProcessFacts
    {
        public static readonly string[] ClosedList = { "claude.exe", "node.exe", "git.exe", "pwsh.exe", "powershell.exe", "bash.exe", "dotnet.exe" };

        private static bool InClosedList(string name) =>
            ClosedList.Contains(name, StringComparer.OrdinalIgnoreCase) || (name.StartsWith("codex", StringComparison.OrdinalIgnoreCase) && name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));

        /// <summary>The command line names the worktree (case-insensitive, with <c>\</c> or <c>/</c>) or carries <c>-C &lt;worktree&gt;</c>.</summary>
        public static bool NamesWorktree(string commandLine, string worktree)
        {
            var line = commandLine.Replace('/', '\\');
            var path = worktree.Replace('/', '\\').TrimEnd('\\');
            return line.Contains(path, StringComparison.OrdinalIgnoreCase);
        }

        private static bool BuildServer(ProcessRow p) =>
            p.Name.Equals("VBCSCompiler.exe", StringComparison.OrdinalIgnoreCase)
            || ((p.Name.Equals("MSBuild.exe", StringComparison.OrdinalIgnoreCase) || p.Name.Equals("dotnet.exe", StringComparison.OrdinalIgnoreCase))
                && p.CommandLine != null && new[] { "/nodemode", "build-server", "VBCSCompiler.dll" }.Any(t => p.CommandLine.Contains(t, StringComparison.OrdinalIgnoreCase)));

        private static HashSet<int> Ancestry(IReadOnlyList<ProcessRow> rows, int selfPid)
        {
            var byPid = rows.GroupBy(r => r.Pid).ToDictionary(g => g.Key, g => g.First());
            var own = new HashSet<int>();
            for (var pid = selfPid; byPid.TryGetValue(pid, out var row) && own.Add(pid); pid = row.ParentPid)
            {
            }

            return own;
        }

        /// <summary>The classes of README §3.2 for the general evaluation (processes outside every class are not recorded).</summary>
        public static List<ClassifiedProcess> Classify(IReadOnlyList<ProcessRow> rows, int selfPid, string worktree)
        {
            var own = Ancestry(rows, selfPid);
            var result = new List<ClassifiedProcess>();
            foreach (var p in rows)
            {
                string? c = null;
                if (own.Contains(p.Pid))
                {
                    c = "own-tree";
                }
                else if (p.Name.Equals("codex-windows-sandbox-service.exe", StringComparison.OrdinalIgnoreCase))
                {
                    c = "nominal-exclusion";
                }
                else if (p.CommandLine != null && NamesWorktree(p.CommandLine, worktree))
                {
                    c = "participant";
                }
                else if (p.CommandLine != null && p.Name.StartsWith("codex", StringComparison.OrdinalIgnoreCase) && p.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    c = "owner-app";
                }
                else if (BuildServer(p))
                {
                    c = "build-server";
                }
                else if (p.CommandLine == null && InClosedList(p.Name))
                {
                    c = "unattributable";
                }

                if (c != null)
                {
                    result.Add(new ClassifiedProcess(p, c));
                }
            }

            return result;
        }

        /// <summary>
        /// For a Worker subagent: the session descendants outside the general classes (recorded at Exit, compared at Entry) and the orphans — processes of
        /// the closed list created inside the cession window whose parent is absent at the Entry or was created after them (PID reused).
        /// </summary>
        public static List<ClassifiedProcess> WorkerClasses(IReadOnlyList<ProcessRow> rows, int sessionPid, int selfPid, string worktree, DateTime cessionStartUtc)
        {
            var general = Classify(rows, selfPid, worktree).Select(x => x.Process.Pid).ToHashSet();
            var byPid = rows.GroupBy(r => r.Pid).ToDictionary(g => g.Key, g => g.First());
            bool DescendsFromSession(ProcessRow p)
            {
                var seen = new HashSet<int>();
                for (var q = p; byPid.TryGetValue(q.ParentPid, out var parent) && seen.Add(q.Pid); q = parent)
                {
                    if (parent.Pid == sessionPid)
                    {
                        return true;
                    }
                }

                return false;
            }

            var result = new List<ClassifiedProcess>();
            foreach (var p in rows.Where(r => !general.Contains(r.Pid) && r.Pid != sessionPid))
            {
                var parentGone = !byPid.TryGetValue(p.ParentPid, out var parent) || parent.CreationUtc > p.CreationUtc;
                if (InClosedList(p.Name) && p.CreationUtc >= cessionStartUtc && parentGone)
                {
                    result.Add(new ClassifiedProcess(p, "orphan"));
                }
                else if (DescendsFromSession(p))
                {
                    result.Add(new ClassifiedProcess(p, "session-descendant"));
                }
            }

            return result;
        }

        /// <summary>
        /// P-02 of 16.4: a participant other than the launched one (and its tree), an unattributable process, or — at the Entry of a Worker subagent — a
        /// session descendant absent at the Exit or a live orphan that is not a build server.
        /// </summary>
        public static List<string> StopCauses(IEnumerable<ClassifiedProcess> general, IEnumerable<int> launchedTree, IEnumerable<ClassifiedProcess>? workerEntry = null,
            IEnumerable<int>? exitDescendants = null)
        {
            var launched = launchedTree.ToHashSet();
            var causes = general.Where(x => (x.Classification == "participant" && !launched.Contains(x.Process.Pid)) || x.Classification == "unattributable")
                .Select(x => "P-02: " + x.Classification + " " + x.Process.Name + " (" + x.Process.Pid + ")").ToList();
            var known = exitDescendants?.ToHashSet() ?? new HashSet<int>();
            foreach (var x in workerEntry ?? Enumerable.Empty<ClassifiedProcess>())
            {
                if ((x.Classification == "session-descendant" && !known.Contains(x.Process.Pid)) || (x.Classification == "orphan" && !BuildServer(x.Process)))
                {
                    causes.Add("P-02: " + x.Classification + " " + x.Process.Name + " (" + x.Process.Pid + ")");
                }
            }

            return causes;
        }
    }
}
