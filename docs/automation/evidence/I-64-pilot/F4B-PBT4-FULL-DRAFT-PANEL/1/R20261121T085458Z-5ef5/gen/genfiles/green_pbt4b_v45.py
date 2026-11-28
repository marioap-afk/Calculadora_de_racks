"""PB-T4b (paquete v4.5): el texto literal del GREEN de src/ que prescribe el paquete, construido en memoria.

Es el GREEN de la v4.4 (green_pbt4b.py, sin cambio) más la optimización que pidió el Owner (owner-pbt4a-cost-decision.md, «Cerrar y optimizar
en PB-T4b»; D-S4, D-PBT4-21): una sola materialización del modelo completo por lectura. Solo cambia RIA (src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs):
- `Read` ya no lee el modelo completo del `Primary` en el acto: lo deja diferido sobre el mismo `input`, en un `System.Lazy`, y se lee la primera
  vez que alguien pide `Full` (en NH, en `FromReads`, dentro del bloqueo);
- `AtOrigin`, que no cambia, trae el modelo completo del origen por `ReadAt` y no pide el del `Primary`, así que con el origen encontrado el del
  `Primary` no se materializa nunca;
- NH, `ReadAt`, WHC y los demás archivos no cambian (respecto de la v4.4).
B-08 sigue (EIG:350-364): en todo RIA, un `new PushBackPilotIntentScan(`, un `PushBackPilotIntentReader.Read(` y un `PushBackFullDraftReader.Read(`,
los tres en `Read` y en ese orden. Nada traduce: lo completo sigue saliendo del lector completo sobre el escaneo.

Módulo: green(repo, rev) -> (base, green), skeleton(repo, rev) -> (base, red), EXISTING, NEW. Solo `git show` (objetos git).
"""
import importlib.util
import os
import subprocess

HERE = os.path.dirname(os.path.abspath(__file__))


def _load(name):
    spec = importlib.util.spec_from_file_location(name, os.path.join(HERE, name + '.py'))
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


gb = _load('green_pbt4b')
WHC_P, WEI_P, FVM_P, PV_P, NFV_P = gb.WHC_P, gb.WEI_P, gb.FVM_P, gb.PV_P, gb.NFV_P
RULE_P, SCALE_P, ADAPTER_P, PRES_P, IFACE_P = gb.RULE_P, gb.SCALE_P, gb.ADAPTER_P, gb.PRES_P, gb.IFACE_P
RIA_P = 'src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs'
NH_P = 'src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs'
DRAFT_ORIGIN_P = 'src/RackCad.Application/Systems/PushBack/Draft/PushBackOriginByText.cs'
EXISTING = list(gb.EXISTING) + [RIA_P]
NEW = list(gb.NEW)

RIA_CTOR_OLD = ('        internal RackPilotIntentReadResult(PushBackPilotIntentResult pilot, PushBackFullIntentResult full, PushBackPilotIntentScan input)\n'
                '        {\n'
                '            Pilot = pilot;\n'
                '            Full = full;\n'
                '            Input = input;\n'
                '            FailureReason = string.Empty;\n'
                '        }\n')
RIA_CTOR_NEW = ('        internal RackPilotIntentReadResult(PushBackPilotIntentResult pilot, PushBackFullIntentResult full, PushBackPilotIntentScan input)\n'
                '        {\n'
                '            Pilot = pilot;\n'
                '            _full = new System.Lazy<PushBackFullIntentResult>(full);\n'
                '            Input = input;\n'
                '            FailureReason = string.Empty;\n'
                '        }\n'
                '\n'
                '        /// <summary>\n'
                '        /// I-64 F4B PB-T4b (D-S4, OWNER-02): the read with the full model of its input still to be read. It is read the\n'
                '        /// first time <see cref="Full"/> is asked for, and only then, so that a reread at another origin, which brings the full\n'
                '        /// model of that origin, never materializes the one of the Primary: one materialization of the full model per read.\n'
                '        /// </summary>\n'
                '        internal RackPilotIntentReadResult(PushBackPilotIntentResult pilot, System.Lazy<PushBackFullIntentResult> full, PushBackPilotIntentScan input)\n'
                '        {\n'
                '            Pilot = pilot;\n'
                '            _full = full;\n'
                '            Input = input;\n'
                '            FailureReason = string.Empty;\n'
                '        }\n'
                '\n'
                '        private readonly System.Lazy<PushBackFullIntentResult>? _full;\n')
RIA_FULL_OLD = '        internal PushBackFullIntentResult? Full { get; }\n'
RIA_FULL_NEW = '        internal PushBackFullIntentResult? Full => _full?.Value;\n'
RIA_READ_OLD = '                return new RackPilotIntentReadResult(read, PushBackFullDraftReader.Read(input, catalog), input);\n'
RIA_READ_NEW = '                return new RackPilotIntentReadResult(read, new System.Lazy<PushBackFullIntentResult>(() => PushBackFullDraftReader.Read(input, catalog)), input);\n'
RIA_SUBS = [('el constructor de PB-T4a y el nuevo, diferido', RIA_CTOR_OLD, RIA_CTOR_NEW),
            ('Full, de la lectura diferida', RIA_FULL_OLD, RIA_FULL_NEW),
            ('la línea del éxito de Read', RIA_READ_OLD, RIA_READ_NEW)]


def _show(repo, rev, path):
    p = subprocess.run(['git', '-C', repo, 'show', '%s:%s' % (rev, path)], capture_output=True)
    if p.returncode:
        return None
    return p.stdout.decode('utf-8').replace('\r\n', '\n')


def t4a_plugin(repo, rev):
    """RIA, NH y Draft/PushBackOriginByText.cs con el GREEN de PB-T4a: de la revisión si ya lo trae, o aplicado en memoria (green_pbt4a.py)."""
    if _show(repo, rev, IFACE_P) is None:
        _, g4a = _load('green_pbt4a').green(repo, rev)
        out = {RIA_P: g4a[RIA_P], NH_P: g4a[NH_P], DRAFT_ORIGIN_P: g4a[DRAFT_ORIGIN_P]}
    else:
        out = {p: _show(repo, rev, p) for p in (RIA_P, NH_P, DRAFT_ORIGIN_P)}
    for p, t in out.items():
        assert t is not None and '\r' not in t, p
    return out


def ria_green(ria):
    for what, old, new in RIA_SUBS:
        n = ria.count(old)
        assert n == 1, 'RIA %s: %d coincidencias' % (what, n)
        ria = ria.replace(old, new)
    return ria


def green(repo, rev):
    base, g = gb.green(repo, rev)
    plugin = t4a_plugin(repo, rev)
    base = dict(base)
    g = dict(g)
    base[RIA_P] = plugin[RIA_P]
    g[RIA_P] = ria_green(plugin[RIA_P])
    return base, g


def skeleton(repo, rev):
    """El RED de la v4.5 es el de la v4.4: RIA no cambia en el RED (como WHC), así que CT-PBT4B-21 cae por aserción."""
    return gb.skeleton(repo, rev)
