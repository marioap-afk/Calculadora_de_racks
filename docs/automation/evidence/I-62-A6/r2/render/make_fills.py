# Fill texts for the A-6 round-2 render (fills.json). Evidence-dependent fills are read from fills_ev/*.md when present.
import json, os
NL = chr(10)
HERE = os.path.dirname(os.path.abspath(__file__))
R1_SURF = ('`src/`, `tests/`, `eng/`, `tools/`, `deploy/`, `assets/`, `docs/adr/`, `docs/automation/agent-execution/`, `docs/AUTOMATION_PLAN.md`, '
           '`docs/WORKFLOW.md`, `docs/INITIATIVE_LIFECYCLE.md`, `docs/FOUNDATIONS.md`, `AGENTS.md`, `CLAUDE.md`, `Directory.Build.props`, '
           '`Directory.Build.targets`, `RackCad.sln`, `global.json`, `NuGet.Config`, `docs/initiatives/I-62-proposal-v14.md`, '
           '`docs/initiatives/I-62-consensus-freeze.md`, `docs/initiatives/I-62-A-1.md`, `docs/initiatives/I-62-A-2.md`, `docs/initiatives/I-62-A-3.md`, '
           '`docs/initiatives/I-62-A-4.md`, `docs/initiatives/I-62-A-5.md`, `docs/initiatives/PROMPT_TEMPLATES.md`, `docs/automation/evidence/I-62-F4/`, '
           '`docs/automation/evidence/I-62-F6/MAP-INVALID/`, `docs/automation/evidence/I-62-F6/FX-02/bmin/`, `docs/automation/evidence/I-62-F6/kits/FX-02/`, '
           '`docs/automation/evidence/I-62-architect-bmin/`, `docs/automation/evidence/I-62-A5/`')
SURFACES = ('**Superficies prohibidas** (ninguna cambia en el commit que publique la ronda 2; las guardas comparan la lista y sus objetos en la base y en '
            'la cabeza): ' + R1_SURF + ', `docs/automation/evidence/I-62-architect-A-6/` (la revisión de la ronda 1) y todos los archivos de '
            '`docs/automation/evidence/I-62-A6/` fuera de `r2/` (la evidencia de la ronda 1).' + NL + NL +
            '- **Blobs sin cambio:** V14 `34ad80ea…`, Freeze `0f6b8608…`, A-1 `c01899a7…`, A-2 `f1e1d6f0…`, A-3 `ea6721f7…`, A-4 `5e2ba68e…`, A-5 `ad94ceed…`,' + NL +
            '  AUTOMATION_PLAN `f525cb1e…`, WORKFLOW `446c32ba…`, INITIATIVE_LIFECYCLE `f19896a8…`, `PROMPT_TEMPLATES.md` `384d0b0c…`, ADR-0046 `da275e1a…`,' + NL +
            '  ADR-0048 `e1bd8d91…`, RAE `592dcfd4…`, descriptor `codex-cli.md` `155f3469…`, árbol `docs/automation/agent-execution/` `809fd58f…`, kit' + NL +
            '  `controller-contracts.md` `90929ad1…`, árbol del paquete B-min `3caa0dfa…` (con el instrumento `9a829b0e…` y el aceptante `434ef1fc…`), árbol' + NL +
            '  del dictamen `ee74d0f9…`, `output.json` del dictamen `96c3efbc…`, árbol `I-62-A5/` `f03a0048…`, árbol de la revisión de la ronda 1' + NL +
            '  `I-62-architect-A-6/` `520e7a1f…` (`output.json` `e90d685b…`) y árbol `I-62-A6/` de la ronda 1 `7f504a17…` fuera de `r2/`.' + NL +
            '- **Aplicada sobre V14 + A-2 + A-4 + A-5:** seis párrafos al final de D.3 (tras A5-2), dos al final de D.4 y uno al final de D.6; añadidos puros que' + NL +
            '  solo cambian D.3, D.4 y D.6 y sus secciones ancestro. C-20b sigue EQUAL.')
fills = {'SURFACES': SURFACES}
evdir = os.path.join(HERE, 'fills_ev')
for key in ('OWNER_NOTE', 'OWNER_SECTION_NOTE', 'DERIVATION_TABLE', 'INDEPENDENCE_EVIDENCE', 'GUARDS'):
    p = os.path.join(evdir, key + '.md')
    fills[key] = open(p, encoding='utf-8').read().rstrip(NL) if os.path.exists(p) else 'PENDIENTE ' + key
json.dump(fills, open(os.path.join(HERE, 'fills.json'), 'w', encoding='utf-8'), ensure_ascii=False, indent=1)
print('ok', sorted(fills))
