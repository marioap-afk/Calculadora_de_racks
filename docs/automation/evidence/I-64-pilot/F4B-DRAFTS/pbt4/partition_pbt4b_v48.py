"""PB-T4b (paquete v4.8): la partición cerrada de las copias y la comprobación de que pasan en el GREEN, desde las TRX de la precondición 6. Es
partition_pbt4b_v47.py (que no se toca: la regla de la aserción de INV-PBT4B-02 y el reparto por filas de D-S5) con dos cambios:
- **D-S6:** las clases de las copias están en el espacio de nombres raíz, con el prefijo `FullPanelCopy`:
  `RackCad[.UI].Tests.FullPanelCopy<Raíz>{Probe,Red,Green}Tests` (la v4.7 las buscaba en `…FullPanelCopies.<Raíz>Full…Tests`);
- **la lista de 478:** las raíces de los archivos de origen salen de pbt4b-substitution-list-478.tsv (con la guarda de los textos del anfitrión).
Las `Theory` declaradas para partirse por filas son las de pbt4b_hand_edits_v48.py (las cuatro de D-S5, sin cambio).

Uso:
    python -I partition_pbt4b_v48.py red <core.trx> <ui.trx> <salida.tsv>
    python -I partition_pbt4b_v48.py green <core.trx> <ui.trx>
    python -I partition_pbt4b_v48.py selftest
Solo lee las TRX."""
import importlib.util
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))


def _load(name, alias):
    spec = importlib.util.spec_from_file_location(alias, os.path.join(HERE, name + '.py'))
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


v47 = _load('partition_pbt4b_v47', 'partition_pbt4b_v47_base48')
p45 = v47.p45
v47.he = _load('pbt4b_hand_edits_v48', 'pbt4b_hand_edits_v48_p48')
CLASS = re.compile(r'^RackCad(?:\.UI)?\.Tests\.FullPanelCopy(\w+)(Probe|Red|Green)Tests$')
LIST = 'pbt4b-substitution-list-478.tsv'
_SOURCES = {}


def sources():
    if _SOURCES:
        return _SOURCES
    rows = [l.split('\t') for l in open(os.path.join(HERE, LIST), encoding='utf-8').read().split('\n')[1:] if l]
    assert len(rows) == 478, len(rows)
    for r in rows:
        path = r[1]
        if path.startswith('(PB-T4a) '):
            continue
        name = os.path.basename(path)[:-3]
        _SOURCES[name[:-len('Tests')] if name.endswith('Tests') else name] = path
    assert len(_SOURCES) == 21, len(_SOURCES)
    return _SOURCES


p45.CLASS, p45.sources = CLASS, sources


def selftest():
    cases = [('RackCad.Tests.FullPanelCopyWorkspacePilotPrepareRedTests', ('WorkspacePilotPrepare', 'Red')),
             ('RackCad.UI.Tests.FullPanelCopyWorkspacePilotReconciliationViewGreenTests', ('WorkspacePilotReconciliationView', 'Green')),
             ('RackCad.Tests.FullPanelCopyWorkspaceHostHardeningGuardProbeTests', ('WorkspaceHostHardeningGuard', 'Probe')),
             ('RackCad.Tests.FullPanelCopies.WorkspacePilotPrepareFullRedTests', None),
             ('RackCad.Tests.WorkspaceFullCopyGuardTests', None)]
    got = []
    for cls, want in cases:
        m = CLASS.match(cls)
        got.append((m.groups() if m else None) == want and (want is None or want[0] in sources()))
    print('selftest de las clases de D-S6: %d de %d' % (sum(got), len(got)))
    if not all(got):
        raise SystemExit(1)
    v47.selftest()


def main():
    if sys.argv[1:2] == ['selftest']:
        selftest()
        return
    v47.main()


if __name__ == '__main__':
    main()
