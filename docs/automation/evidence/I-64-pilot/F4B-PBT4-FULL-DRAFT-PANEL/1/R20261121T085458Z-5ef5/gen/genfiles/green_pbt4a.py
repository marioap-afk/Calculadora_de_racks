"""PB-T4a (v4.3): el texto literal del GREEN que prescribe el paquete, construido en memoria sobre una revisión.

v4.3 (CA-T4a I-01): Draft/PushBackOriginByText.cs no lleva ningún `?` de tipo de referencia (`string Choose`, `pair => pair.Value`),
porque los proyectos tienen Nullable=disable y la pureza de Draft/ prohíbe `#nullable`.

Módulo: green(repo, rev) -> (base, green), dos dict ruta -> texto (LF). Solo `git show` (objetos git).
Cada sustitución tiene que casar exactamente una vez. Los archivos nuevos no están en `base`.
"""
import re
import subprocess

WHC_P = 'src/RackCad.Application/Workspace/WorkspaceHostCore.cs'
WEI_P = 'src/RackCad.Application/Workspace/WorkspaceEditIntent.cs'
NH_P = 'src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs'
RIA_P = 'src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs'
IFACE_P = 'src/RackCad.Application/Workspace/IWorkspaceEditIntentOriginReader.cs'
DRAFT_P = 'src/RackCad.Application/Systems/PushBack/Draft/PushBackOriginByText.cs'


def _show(repo, rev, path):
    p = subprocess.run(['git', '-C', repo, 'show', '%s:%s' % (rev, path)], capture_output=True)
    if p.returncode:
        raise SystemExit('git show %s -> %d' % (path, p.returncode))
    return p.stdout.decode('utf-8').replace('\r\n', '\n')


def _once(text, old, new, what):
    n = text.count(old)
    assert n == 1, '%s: %d coincidencias' % (what, n)
    return text.replace(old, new)


def body_of(code, signature):
    m = re.search(signature, code)
    assert m, 'member not found: ' + signature
    start = m.start()
    open_ = code.find('{', m.end() - 1)
    arrow = code.find('=>', m.end() - 1)
    if arrow >= 0 and (open_ < 0 or arrow < open_):
        return code[start:code.find(';', arrow) + 1]
    depth = 0
    for i in range(open_, len(code)):
        if code[i] == '{':
            depth += 1
        elif code[i] == '}':
            depth -= 1
            if depth == 0:
                return code[start:i + 1]
    raise AssertionError('unbalanced')


OLD_READ = '                var read = _editIntentReader!.ReadIntent(request.Session, request.RackId, request.ChosenReference, request.Witness);\n'
NEW_READ = ('                var read = _editIntentReader is IWorkspaceEditIntentOriginReader originReader\n'
            '                    ? originReader.ReadIntentAtOrigin(request.Session, request.RackId, draft.CurrentDraftState.Origin, request.Witness)\n'
            '                    : _editIntentReader!.ReadIntent(request.Session, request.RackId, request.ChosenReference, request.Witness);\n')
REREADS = ['TryCheckPilotEdit', 'TryReloadPilotEdit', 'TryConfirmPilotBase', 'TryKeepPilotEdit', 'TryDiscardAndReloadPilotEdit']

NH_NEW_METHOD = (
    '            public EditIntentReadResult ReadIntentAtOrigin(SessionId session, string rackId, string originText, EditIntentWitness witness)\n'
    '            {\n'
    '                try\n'
    '                {\n'
    '                    return ReadOnActiveDocument(session, rackId, null, witness, originText);\n'
    '                }\n'
    '                catch (System.Exception ex)\n'
    '                {\n'
    '                    LogFault("WorkspaceNavigationHost.ReadEditIntent", ex);\n'
    '                    return EditIntentReadResult.Failed(ex.Message);\n'
    '                }\n'
    '            }\n'
    '\n')

RIA_CTOR = ('        internal RackPilotIntentReadResult(PushBackPilotIntentResult pilot, PushBackFullIntentResult full, PushBackPilotIntentScan input)\n'
            '        {\n'
            '            Pilot = pilot;\n'
            '            Full = full;\n'
            '            Input = input;\n'
            '            FailureReason = string.Empty;\n'
            '        }\n')
RIA_PROPS = ('        internal PushBackFullIntentResult? Full { get; }\n\n'
             '        internal PushBackPilotIntentScan? Input { get; }\n')
RIA_AT_ORIGIN = (
    '        /// <summary>\n'
    '        /// I-64 F4B PB-T4a (D-PBT4-03): the same read at the origin of an open draft, chosen by its text over the scan that\n'
    '        /// <see cref="Read"/> already took, by the pure reread of the full model (PushBackOriginByText.ReadAt). It reads nothing\n'
    '        /// more of the drawing. Without exactly one other mutable member with that text, the read stays as it was. An exception of\n'
    '        /// the pure reread is not contained here, unlike <see cref="Read"/>: it reaches the reader of the host, which answers Failed.\n'
    '        /// </summary>\n'
    '        internal static RackPilotIntentReadResult AtOrigin(RackPilotIntentReadResult read, string originText, RackCatalog catalog)\n'
    '        {\n'
    '            if (!read.Succeeded || read.Input == null) return read;\n'
    '            var at = PushBackOriginByText.ReadAt(read.Input, originText, catalog);\n'
    '            return at == null ? read : new RackPilotIntentReadResult(at.Value.Pilot, at.Value.Full, at.Value.Scan);\n'
    '        }\n'
    '\n')

WEI_CTOR_OLD = ('        private EditIntentReadResult(EditIntentReadStatus status, string? reason, PushBackPilotIntentResult? pilotRead)\n'
                '        {\n'
                '            Status = status;\n'
                '            Reason = reason;\n'
                '            PilotRead = pilotRead;\n'
                '        }\n')
WEI_CTOR_NEW = ('        private EditIntentReadResult(EditIntentReadStatus status, string? reason, PushBackPilotIntentResult? pilotRead, PushBackFullIntentResult? fullRead, RackCatalog? catalog)\n'
                '        {\n'
                '            Status = status;\n'
                '            Reason = reason;\n'
                '            PilotRead = pilotRead;\n'
                '            FullRead = fullRead;\n'
                '            Catalog = catalog;\n'
                '        }\n')
WEI_PROP_OLD = '        public PushBackPilotIntentResult? PilotRead { get; }\n'
WEI_PROP_NEW = ('        public PushBackPilotIntentResult? PilotRead { get; }\n\n'
                '        /// <summary>I-64 F4B PB-T4a: the read of the full model over the same scan; null unless the read came from <see cref="FromReads"/>.</summary>\n'
                '        public PushBackFullIntentResult? FullRead { get; }\n\n'
                '        /// <summary>I-64 F4B PB-T4a: the catalog of the read; null unless the read came from <see cref="FromReads"/>.</summary>\n'
                '        public RackCatalog? Catalog { get; }\n')
WEI_FROMPILOT_OLD = '            return new EditIntentReadResult(EditIntentReadStatus.Read, null, pilotRead);\n        }\n'
WEI_FROMPILOT_NEW = ('            return new EditIntentReadResult(EditIntentReadStatus.Read, null, pilotRead, null, null);\n        }\n\n'
                     '        public static EditIntentReadResult FromReads(PushBackPilotIntentResult pilotRead, PushBackFullIntentResult fullRead, RackCatalog catalog)\n'
                     '        {\n'
                     '            if (pilotRead == null) throw new ArgumentNullException(nameof(pilotRead));\n'
                     '            if (fullRead == null) throw new ArgumentNullException(nameof(fullRead));\n'
                     '            if (catalog == null) throw new ArgumentNullException(nameof(catalog));\n'
                     '            return new EditIntentReadResult(EditIntentReadStatus.Read, null, pilotRead, fullRead, catalog);\n'
                     '        }\n')

IFACE = ('#nullable enable\n'
         'namespace RackCad.Application.Workspace\n'
         '{\n'
         '    /// <summary>\n'
         '    /// I-64 F4B PB-T4a (D-PBT4-03, A-T3-8) — the reader of the edit intention that can read again at the origin of an open draft,\n'
         '    /// found by its text. It is optional: a reader without it is read as today.\n'
         '    /// </summary>\n'
         '    public interface IWorkspaceEditIntentOriginReader\n'
         '    {\n'
         '        EditIntentReadResult ReadIntentAtOrigin(SessionId session, string rackId, string originText, EditIntentWitness? witness);\n'
         '    }\n'
         '}\n')

DRAFT = ('using System;\n'
         'using System.Linq;\n'
         'using RackCad.Application.Catalogs;\n'
         'using RackCad.Application.Systems.PushBack.Pilot;\n'
         'using RackCad.Application.Workspace.Drafts;\n'
         '\n'
         'namespace RackCad.Application.Systems.PushBack.Draft\n'
         '{\n'
         '    /// <summary>\n'
         '    /// I-64 F4B PB-T4a (D-PBT4-03, A-T3-8) — the origin of an open draft, found by its text: the key of the ONLY mutable member\n'
         '    /// whose raw text is, byte for byte, the origin of the draft (the rule of the run, UR:640-650), the same scan at that key, and\n'
         '    /// the two pure reads over it (the pilot and the full model), as PushBackFullDraftReader does.\n'
         '    /// </summary>\n'
         '    public static class PushBackOriginByText\n'
         '    {\n'
         '        public static string Choose(DraftStaleBase staleBase, string originText)\n'
         '        {\n'
         '            if (staleBase == null || originText == null) return null;\n'
         '            var matches = staleBase.Entries.Values\n'
         '                .Where(entry => entry.Membership == DraftMembership.Mutable && string.Equals(entry.EnvelopeText, originText, StringComparison.Ordinal))\n'
         '                .Select(entry => entry.Key)\n'
         '                .ToList();\n'
         '            return matches.Count == 1 ? matches[0] : null;\n'
         '        }\n'
         '\n'
         '        public static PushBackPilotIntentScan AtKey(PushBackPilotIntentScan scan, string originKey)\n'
         '        {\n'
         '            if (scan == null) throw new ArgumentNullException(nameof(scan));\n'
         '            return new PushBackPilotIntentScan(\n'
         '                scan.RackId,\n'
         '                originKey,\n'
         '                scan.Facts,\n'
         '                scan.RawEnvelopes.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal),\n'
         '                scan.CompletenessEvidence,\n'
         '                scan.PreviewWitness);\n'
         '        }\n'
         '\n'
         '        public static (PushBackPilotIntentScan Scan, PushBackPilotIntentResult Pilot, PushBackFullIntentResult Full)? ReadAt(PushBackPilotIntentScan scan, string originText, RackCatalog catalog)\n'
         '        {\n'
         '            if (scan == null) throw new ArgumentNullException(nameof(scan));\n'
         '            var key = Choose(PushBackPilotIntentReader.BuildStaleBase(scan), originText);\n'
         '            if (key == null || key == scan.OriginKey) return null;\n'
         '            var input = AtKey(scan, key);\n'
         '            return (input, PushBackPilotIntentReader.Read(input, catalog), PushBackFullDraftReader.Read(input, catalog));\n'
         '        }\n'
         '    }\n'
         '}\n')


def green(repo, rev):
    base = {p: _show(repo, rev, p) for p in (WHC_P, WEI_P, NH_P, RIA_P)}
    whc = base[WHC_P]
    for name in REREADS:
        sig = re.search(r'public\s+\w+\s+' + name + r'\s*\(\s*EditIntentRequest\s+request\s*\)', whc)
        body = body_of(whc, re.escape(sig.group(0)))
        assert body.count(OLD_READ) == 1, name
        assert 'var draft = _drafts.Find<PushBackPilotDraftState>(request.Session, request.RackId);' in body, name
        assert body.index('var draft = _drafts.Find<') < body.index(OLD_READ), name
        whc = whc.replace(body, body.replace(OLD_READ, NEW_READ))
    assert whc.count(OLD_READ) == 1 and whc.count('ReadIntentAtOrigin(') == 5

    nh = base[NH_P]
    nh = _once(nh, 'private sealed class PilotIntentReader : IWorkspaceEditIntentReader\n',
               'private sealed class PilotIntentReader : IWorkspaceEditIntentReader, IWorkspaceEditIntentOriginReader\n', 'NH-1')
    nh = _once(nh, '                    return ReadOnActiveDocument(session, rackId, chosenReference, witness);\n',
               '                    return ReadOnActiveDocument(session, rackId, chosenReference, witness, null);\n', 'NH-2')
    nh = _once(nh, '            private EditIntentReadResult ReadOnActiveDocument(SessionId session, string rackId, long? chosenReference, EditIntentWitness witness)\n',
               NH_NEW_METHOD + '            private EditIntentReadResult ReadOnActiveDocument(SessionId session, string rackId, long? chosenReference, EditIntentWitness witness, string originText)\n', 'NH-3')
    nh = _once(nh, '                        catalog.Catalog);\n                    return read.Succeeded\n',
               '                        catalog.Catalog);\n'
               '                    if (originText != null)\n'
               '                        read = RackPilotIntentAdapter.AtOrigin(read, originText, catalog.Catalog);\n'
               '                    return read.Succeeded\n', 'NH-4')
    nh = _once(nh, '? EditIntentReadResult.FromPilot(read.Pilot)', '? EditIntentReadResult.FromReads(read.Pilot, read.Full, catalog.Catalog)', 'NH-5')

    ria = base[RIA_P]
    ria = _once(ria, 'using RackCad.Application.Systems.PushBack.Pilot;\n',
                'using RackCad.Application.Systems.PushBack.Draft;\nusing RackCad.Application.Systems.PushBack.Pilot;\n', 'RIA-1')
    ria = _once(ria, '            Pilot = pilot;\n            FailureReason = failureReason ?? string.Empty;\n        }\n',
                '            Pilot = pilot;\n            FailureReason = failureReason ?? string.Empty;\n        }\n\n' + RIA_CTOR, 'RIA-2')
    ria = _once(ria, '        internal PushBackPilotIntentResult? Pilot { get; }\n',
                '        internal PushBackPilotIntentResult? Pilot { get; }\n\n' + RIA_PROPS, 'RIA-3')
    ria = _once(ria, '                return new RackPilotIntentReadResult(read, null);\n',
                '                return new RackPilotIntentReadResult(read, PushBackFullDraftReader.Read(input, catalog), input);\n', 'RIA-4')
    anchor = '        /// <summary>\n        /// The light read of what a preview shows'
    ria = _once(ria, anchor, RIA_AT_ORIGIN + anchor, 'RIA-5')

    wei = base[WEI_P]
    wei = _once(wei, 'using RackCad.Application.Systems.PushBack.Pilot;\n',
                'using RackCad.Application.Catalogs;\nusing RackCad.Application.Systems.PushBack.Draft;\nusing RackCad.Application.Systems.PushBack.Pilot;\n', 'WEI-1')
    wei = _once(wei, WEI_CTOR_OLD, WEI_CTOR_NEW, 'WEI-2')
    wei = _once(wei, WEI_PROP_OLD, WEI_PROP_NEW, 'WEI-3')
    wei = _once(wei, WEI_FROMPILOT_OLD, WEI_FROMPILOT_NEW, 'WEI-4')
    for status in ('NoActiveDocument', 'Busy', 'Rejected', 'Failed'):
        wei = _once(wei, 'new(EditIntentReadStatus.%s, reason, null);' % status, 'new(EditIntentReadStatus.%s, reason, null, null, null);' % status, 'WEI-' + status)

    g = {WHC_P: whc, NH_P: nh, RIA_P: ria, WEI_P: wei, IFACE_P: IFACE, DRAFT_P: DRAFT}
    return base, g
