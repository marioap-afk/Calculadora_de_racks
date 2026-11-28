"""Arma la v4.8 del paquete de PB-T4 desde la v4.7, el fragmento v48/f48-head.md y la revisión pbt4b-equivalents-review-v48.txt (el anexo V48-A, que
sustituye al V47-A: el texto literal de lo escrito a mano que cambia o entra en la v4.8 y de las cuatro `Theory` partidas por filas, con los nombres
de clase de D-S6). Solo lee archivos de hs65/pbt4 y objetos git del cierre de PB-T4a (el `BaseSha` de la guarda nueva), y escribe la v4.8.
Uso: python -I make_v48.py <checkout>"""
import hashlib
import os
import re
import subprocess
import sys

sys.stdout.reconfigure(encoding='utf-8', errors='replace')
HERE = os.path.dirname(os.path.abspath(__file__))
REPO = sys.argv[1]
V47 = os.path.join(HERE, 'I-64-f4b-pbt4-gate-contract-package-v47.md')
OUT = os.path.join(HERE, 'I-64-f4b-pbt4-gate-contract-package-v48.md')
raw = open(V47, 'rb').read()
assert hashlib.sha256(raw).hexdigest() == 'b86ef9cbd29eea5fa1629af37db105dff8ef2feaa3a6999b2504ec49751d40c9'
t = raw.decode('utf-8')


def sha(name):
    data = open(os.path.join(HERE, name), 'rb').read()
    assert b'\r' not in data, name
    return hashlib.sha256(data).hexdigest()


def short(h):
    return '%s…%s' % (h[:8], h[-4:])


LIST478 = sha('pbt4b-substitution-list-478.tsv')
assert LIST478 == '8b0fcd0ca775ce3a5e46ef2059f4b06e7ec7319883daf3cb37b1fe91290e9e8b'
MAP5 = sha('pbt4b-copy-map-v5.tsv')
assert MAP5 == 'bda79bb7fb6e1dfe885cdc5942b2af45dfac8861663ccc99dbb38a61b8844524'
SESSION, OWNER478 = sha('session-pbt4b-design-decisions.md'), sha('owner-pbt4b-list-478-decision.md')
GEN, EDITS, REVIEW = sha('make_copies_pbt4b_v48.py'), sha('pbt4b_hand_edits_v48.py'), sha('pbt4b-equivalents-review-v48.txt')
probe = [l.split()[1] for l in open(os.path.join(HERE, 'v48', 'copies_probe_v48-out.txt'), encoding='utf-8').read().split('\n') if l.startswith('conjunto ')]
assert len(probe) == 1
PROBE = probe[0]
WHHG = 'tests/RackCad.Tests/Workspace/WorkspaceHostHardeningGuardTests.cs'
_g = subprocess.run(['git', '-C', REPO, 'show', '78a7cef3:' + WHHG], capture_output=True)
assert _g.returncode == 0
WHHG_BASE = hashlib.sha256(_g.stdout.decode('utf-8').lstrip('﻿').replace('\r\n', '\n').encode('utf-8')).hexdigest()

# ---------------------------------------------------------------- la revisión: el anexo V48-A
SEP = '=' * 100
entries = {}
parts = open(os.path.join(HERE, 'pbt4b-equivalents-review-v48.txt'), encoding='utf-8').read().split(SEP + '\n')
assert parts[0] == ''
for k in range(1, len(parts), 2):
    path, name = parts[k][:-1].split(' :: ', 1)
    body = parts[k + 1]
    entries[(path, name)] = body[:-1] if body.endswith('\n') else body
assert len(entries) == 114, len(entries)
WPP = 'tests/RackCad.Tests/Workspace/WorkspacePilotPrepareTests.cs'
WLHT = 'tests/RackCad.Tests/Workspace/WorkspaceLifecycleHostTests.cs'
PEG = 'tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs'
INVALID_E = 'InvalidE_AMutableMemberThatIsNotAvailableInTheOutgoingSystemOrWhoseEnvelopeDoesNotDecodeIsInvalidAndNeverErasesAView'
NEVER = 'ThePilotNeverErasesAViewAPhantomThatWasAlreadyInTheDrawingMakesTheUpdateInvalidAndIsNeverInThePlan'
NONE30 = 'NoneOfThe30LiteralsInEnglishStaysInTheHostOrTheSupportAndTheFourIdentifiersAreKept'
annex = ['## Anexo V48-A. El texto literal de lo escrito a mano de la v4.8 y de las cuatro `Theory` partidas por filas', '',
         'Copiado por `make_v48.py` de la revisión del guion, `pbt4b-equivalents-review-v48.txt` (sha256 `%s`, 114 entradas). **Sustituye al anexo '
         'V47-A:** el texto de las mitades de D-S5 y de los dos miembros nuevos no cambia; cambian los nombres de sus clases (D-S6). Cada prueba va en '
         'su clase de la copia, en el lugar de la prueba original y con la indentación de la clase; lo que manda es el `CopySha` de su archivo.' % REVIEW, '']
n_annex = 0


def block(title, key):
    global n_annex
    body = entries[key]
    assert '```' not in body
    annex.extend([title, '', '```csharp', body.strip('\n'), '```', ''])
    n_annex += 1


annex.extend(['### D-S1 en dos copias de WPP (V48-02)', ''])
block('**`FullPanelCopyWorkspacePilotPrepare<Red|Green>Tests.%s`** (la clase, la de la partición):' % INVALID_E, (WPP, INVALID_E))
block('**`FullPanelCopyWorkspacePilotPrepare<Red|Green>Tests.%s`**:' % NEVER, (WPP, NEVER))
annex.extend(['### La guarda de la sexta tanda (V48-03; C-5)', ''])
block('**`FullPanelCopyWorkspaceHostHardeningGuard<Red|Green>Tests.%s`** (sin cambio):' % NONE30, (WHHG, NONE30))
block('**`WorkspaceHostHardeningGuardFull`, el ayudante `Table`** (la tabla, con tres cuentas nuevas):', (WHHG, 'ayudante Table'))
annex.extend(['### Las cuatro `Theory` partidas por filas (D-S5) y los dos miembros nuevos (con los nombres de D-S6)', ''])
for path, name, stem in ((WLHT, 'EachOperationThatCanChangeADraftEndsTheMarkSoTheSessionIsRetiredConservatively', 'WorkspaceLifecycleHost'),
                         (PEG, 'TheHostGuardTripsOnAPublicOperationAnOrderOfBasesOrAnAdapterThatIsAFieldOfInstance', 'WorkspacePilotEditGuard'),
                         (WPP, 'ConfirmAppliesOnlyToCleanDirtyAndConflictAndTheOthersReadNothing', 'WorkspacePilotPrepare'),
                         (WPP, 'KeepAppliesToConflictAndToDirtyMarkedStaleApplyingWaitsAndTheOthersAreNotNeededAllWithoutReading', 'WorkspacePilotPrepare')):
    for half in ('Red', 'Green'):
        block('**`FullPanelCopy%s%sTests.%s`** (la mitad %s):' % (stem, half, name, half), (path, '%s (%s, D-S5)' % (name, half)))
for name in ('StatusNamesRed', 'StatusNamesGreen'):
    block('**`WorkspacePilotPrepareFull`, el miembro nuevo `%s`** (solo en la copia; en la clase de la partición cuyas pruebas lo alcanzan):' % name,
          (WPP, 'ayudante nuevo ' + name))
assert n_annex == 14
ANNEX = '\n'.join(annex)

# ---------------------------------------------------------------- la cabecera
head = open(os.path.join(HERE, 'v48', 'f48-head.md'), encoding='utf-8').read()
for k_, v_ in (('@@OWNER478@@', OWNER478), ('@@SESSION@@', SESSION), ('@@LIST478@@', LIST478), ('@@MAP5@@', MAP5)):
    head = head.replace(k_, v_)
assert '@@' not in head
TITLE = '# I-64 F4B — Paquete del contrato de PB-T4 («Editar todo» en el panel del Workspace), v4.7\n'
assert t.count(TITLE) == 1
t = t.replace(TITLE, TITLE.replace('v4.7', 'v4.8') + '\n' + head + '\n## La cabecera de la v4.7, sin cambio salvo lo que dice el registro v4.7 → v4.8\n')
assert t.count('**Estado:** v4.7. Corrige la parte de') == 1 and t.count('**Lo que sale de la v4.7 (PB-T4b):**') == 1
t = t.replace('**Estado:** v4.7. Corrige la parte de', '**Estado de la v4.7:** Corrige la parte de').replace(
    '**Lo que sale de la v4.7 (PB-T4b):**', '**Lo que salió de la v4.7 (PB-T4b):**')

# ---------------------------------------------------------------- el cuerpo (de §0 al final) y el anexo
cut = t.index('\n## 0. En una página')
a47 = t.index('\n## Anexo V47-A.')
headpart, body, old_annex = t[:cut], t[cut:a47 + 1], t[a47 + 1:]
done = []


def R(old, new, times=1, what=None):
    global body
    n = body.count(old)
    assert n == times, (what or old[:90], n, times)
    body = body.replace(old, new)
    done.append(what or old[:60])


def RX(pattern, repl, times, what):
    global body
    body, n = re.subn(pattern, repl, body)
    assert n == times, (what, n, times)
    done.append(what)


def SEC(head_, until, pairs, what):
    """Reemplazos dentro de una sección (de `head_` a `until`)."""
    global body
    i = body.index('\n' + head_) + 1
    j = body.index('\n' + until, i) + 1
    s = body[i:j]
    for old, new, times in pairs:
        n = s.count(old)
        assert n == times, (what, old[:90], n, times)
        s = s.replace(old, new)
    body = body[:i] + s + body[j:]
    done.append(what)


# ---- §0 y §1
R('| **477** (428 de las 9 clases, «Saltar y duplicar», 358 copias y 70 a mano; 22 de la letra; 27 de la UI)',
  '| **478** (428 de las 9 clases, «Saltar y duplicar», 356 copias y 72 a mano; 23 de la letra; 27 de la UI)')
R('y, en la quinta tanda, «Añadirlas a la lista» (`owner-pbt4b-list-477-decision.md`). La sesión decidió D-S1 a D-S3 y D-S5 (`session-pbt4b-design-decisions.md`)',
  '; en la quinta tanda, «Añadirlas a la lista» (`owner-pbt4b-list-477-decision.md`), y en la sexta, «Añadirla a la lista» (`owner-pbt4b-list-478-decision.md`). '
  'La sesión decidió D-S1 a D-S3, D-S5 y D-S6 (`session-pbt4b-design-decisions.md`)')
R('| **La decisión del Owner sobre las dos de PB-T4a** (`owner-pbt4b-list-477-decision.md`, 2026-11-20) | «Añadirlas a la lista»: 477 en PB-T4b (§8.5) |\n',
  '| **La decisión del Owner sobre las dos de PB-T4a** (`owner-pbt4b-list-477-decision.md`, 2026-11-20) | «Añadirlas a la lista»: 477 en PB-T4b (§8.5) |\n'
  '| **La decisión del Owner sobre la guarda de los textos del anfitrión** (`owner-pbt4b-list-478-decision.md`, 2026-11-20, la sexta tanda) | «Añadirla a la lista»: **478** en PB-T4b (§8.5) |\n')
R('| D-S1 a D-S3 y D-S5, y D-S4 (§8.5, D-PBT4-21);', '| D-S1 a D-S3, D-S5 y D-S6, y D-S4 (§8.5, D-PBT4-21);')

# ---- D-PBT4-11
R('V45-01, V45-05, V45-07, V45-08; V46-01, V46-02; V47-01)', 'V45-01, V45-05, V45-07, V45-08; V46-01, V46-02; V47-01; V48-01 a V48-03)')
R('**La lista de PB-T4b, fijada por sha256** (V45-01): **`pbt4b-substitution-list-477.tsv`**, sha256 **`5975c5b6b9111c3a4048193b5ebf10b619a567996dea8707b88ff6a00105a651`** (105 044 bytes, LF, 477 filas y la cabecera): ',
  '- **«Añadirla a la lista (Recomendado)»** (`owner-pbt4b-list-478-decision.md`, la sexta tanda; V48-03): **la lista pasa de 477 a 478** con '
  '`WorkspaceHostHardeningGuardTests.NoneOfThe30LiteralsInEnglishStaysInTheHostOrTheSupportAndTheFourIdentifiersAreKept` (WHHG:491-499), en C-5. '
  'La guarda cuenta cuántas veces está exactamente cada texto en español en WHC y su soporte, y el GREEN cambia tres cuentas por el diseño ya '
  'decidido (D-PBT4-04 y D-PBT4-08): «La relectura no trajo resultado.» 2 → 0, «El borrador se está aplicando o ya no está registrado.» 5 → 6 y '
  '«El rack no tiene escala de anotación.» 1 → 0. Su equivalente, `WorkspaceHostHardeningGuardFull`, lleva la misma tabla con esas tres cuentas, '
  'cada una con su `// before N` (el anexo V48-A).\n\n'
  '**La lista de PB-T4b, fijada por sha256** (V48-03): **`pbt4b-substitution-list-478.tsv`**, sha256 **`%s`** (105 379 bytes, LF, 478 filas y la '
  'cabecera): la de 477 (`pbt4b-substitution-list-477.tsv`, `5975c5b6…a651`), sin cambio y en su orden, con la fila de WHHG al final; la genera '
  '`make_list_478.py` sobre `78a7cef3` (`v48/make_list_478-out.txt`). **La de 477** (105 044 bytes, que generó `make_list_477.py`, `v45/make_list_477-out.txt`, '
  'con las 475 y las dos de PB-T4a al final) ' % LIST478, 1, 'D-11 lista')
R('**La de 475 y la de 470** (`pbt4b-substitution-list.tsv`, `44393b19…63e4`) quedan como historia',
  '**La de 477, la de 475 y la de 470** (`pbt4b-substitution-list.tsv`, `44393b19…63e4`) quedan como historia')
R('| **C-0**, las 9 clases | 428 | **358 copias mecánicas** y **70 equivalentes escritos a mano** (los 53 de la v4.3, los 7 de D-S1, los 8 de la v4.6 y los 2 de D-S5), por el guion de la copia |',
  '| **C-0**, las 9 clases | 428 | **356 copias mecánicas** y **72 equivalentes escritos a mano** (los 53 de la v4.3, los 7 de D-S1, los 8 de la v4.6, los 2 de D-S5 y los 2 de la v4.8, D-S1), por el guion de la copia |')
R('| **C-5**, la letra de las guardas | **22** (las 15 de la v4.3, las 5 de la tercera tanda y las 2 de PB-T4a) | **CT-PBT4A-06:** CT-PBT4B-13. **Las otras 21:**',
  '| **C-5**, la letra de las guardas | **23** (las 15 de la v4.3, las 5 de la tercera tanda, las 2 de PB-T4a y la de la sexta tanda) | **CT-PBT4A-06:** CT-PBT4B-13. **Las otras 22:**')
R('| **Total** | **477** | **Core 450 y UI 27**, en 20 archivos |', '| **Total** | **478** | **Core 451 y UI 27**, en 21 archivos |')
R('**Las copias y los equivalentes los genera el guion de la copia, `make_copies_pbt4b_v47.py`** (con las piezas de `make_copies_pbt4b.py` de la v4.4, sin tocarlo, y las de `make_copies_pbt4b_v46.py`, sin tocarlo; las ediciones de `pbt4b_hand_edits_v47.py`, las dos clases de PB-T4a, la lista de 477, el mapa v4,',
  '**Las copias y los equivalentes los genera el guion de la copia, `make_copies_pbt4b_v48.py`** (con las piezas de `make_copies_pbt4b.py` de la v4.4, `make_copies_pbt4b_v46.py` y `make_copies_pbt4b_v47.py`, sin tocarlos; las ediciones de `pbt4b_hand_edits_v48.py`, las dos clases de PB-T4a y la guarda de la sexta tanda, la lista de 478, el mapa v5, D-S6 (el espacio raíz y el prefijo `FullPanelCopy`),')
R('en el espacio de nombres `RackCad.Tests.FullPanelCopies` (`RackCad.UI.Tests.FullPanelCopies` en la UI);',
  'en el **espacio de nombres raíz de su ensamblado**, `RackCad.Tests` (`RackCad.UI.Tests` en la UI), como el resto de su proyecto (**D-S6**, V48-01: lo exige '
  '`NamespaceFolderGuardTests.TestProjects_KeepExactlyOneAssemblyRootNamespace`, la excepción de I-23; la v4.7 las ponía en `…FullPanelCopies`);')
R('**la clase de prueba se parte en `<Clase>FullRedTests` y `<Clase>FullGreenTests`** según la partición cerrada (en el modo de medida, una sola, `<Clase>FullProbeTests`;',
  '**la clase de prueba se parte en `FullPanelCopy<Clase>RedTests` y `FullPanelCopy<Clase>GreenTests`** según la partición cerrada (en el modo de medida, una sola, `FullPanelCopy<Clase>ProbeTests`; '
  '**el prefijo `FullPanelCopy`** hace que ningún filtro heredado las seleccione, porque `…Tests.FullPanelCopy…` no contiene `RackCad.Tests.Workspace` ni ningún otro; es el mecanismo de las CT;')
R('la clase de prueba `<Clase>Tests` pasa a `<Clase>FullRedTests` y `<Clase>FullGreenTests` |',
  'la clase de prueba `<Clase>Tests` pasa a `FullPanelCopy<Clase>RedTests` y `FullPanelCopy<Clase>GreenTests` (D-S6) |')
R('**Los 70 a mano** (el mapa de la copia v4, `pbt4b-copy-map-v4.tsv`, sha256 `0c5abc6d00d9dfc9943812aae4c9fd31338c3b64f17cbff632f949378e9d9b1c`, que lleva',
  '**Los 72 a mano** (el mapa de la copia v5, `pbt4b-copy-map-v5.tsv`, sha256 `%s`, que lleva las dos de la v4.8 con `R-D-S1-borra` (V48-02) y' % MAP5)
R('; el v3, `11b26ab1…0743`, el v2, `3ffadf5b…9e01`, y `pbt4-copy-map.tsv`, `1091ae50…`, quedan como historia):',
  '; el v4, `0c5abc6d…9b1c`, el v3, `11b26ab1…0743`, el v2, `3ffadf5b…9e01`, y `pbt4-copy-map.tsv`, `1091ae50…`, quedan como historia):')
R('- **30 con texto propio:** en la v4.6 (V46-02),',
  '- **32 con texto propio:** en la v4.8 (V48-02, D-S1), las dos de WPP que el modelo completo borra, `InvalidE_AMutableMemberThatIsNotAvailableInTheOutgoingSystemOrWhoseEnvelopeDoesNotDecodeIsInvalidAndNeverErasesAView` '
  '(el lateral fantasma sale de los `Invalid` y da `Ready` con borrados [B2]; `side-b`, `Invalid` con `FrontalBOfOneWayPrefix`/`Suffix`; los dos descriptores, como hoy) y '
  '`ThePilotNeverErasesAViewAPhantomThatWasAlreadyInTheDrawingMakesTheUpdateInvalidAndIsNeverInThePlan` (con el fantasma, `Ready`, miembros [A1] y borrados [B2]; sin él, '
  'como hoy y sin borrados), según **D-PB-04** («borrar vistas obsoletas, sí; crear vistas no está autorizado»), **D-PBT3-09 (e)** y **D-PBT4-07**; en la v4.6 (V46-02),')
R('Su texto literal, en el anexo V47-A; la tabla, en la cabecera de la v4.7.', 'Su texto literal, en el anexo V48-A (el mismo de la V47-A, con los nombres de clase de D-S6); la tabla, en la cabecera de la v4.7.')
R('**Los 21 equivalentes de las guardas** (`GUARD_EDITS` y `HELPER_EDITS`):',
  '**Los 22 equivalentes de las guardas** (`GUARD_EDITS` y `HELPER_EDITS`): en la v4.8 (V48-03), **la de los textos del anfitrión**, WHHG `NoneOfThe30…`, con su prueba sin '
  'cambio y su tabla `Table` con tres cuentas nuevas (2 → 0, 5 → 6 y 1 → 0, cada una con su `// before N`; el anexo V48-A); y, de antes,')
R('**Su texto literal:** el de `make_copies_pbt4b_v47.py` con `pbt4b_hand_edits_v47.py`, el kit y la partición. Para revisarlo sin generar nada, `pbt4b-equivalents-review-v47.txt` lleva el texto final de las 102 entradas (70 a mano, 21 de guarda, el ayudante editado, los dos miembros nuevos y las ocho mitades de D-S5), generado en el modo de medida (§6.2.1).',
  '**Su texto literal:** el de `make_copies_pbt4b_v48.py` con `pbt4b_hand_edits_v48.py`, el kit y la partición. Para revisarlo sin generar nada, `pbt4b-equivalents-review-v48.txt` lleva el texto final de las 114 entradas (72 a mano, 22 de guarda, las nueve ediciones de ayudante de las guardas, el ayudante editado de C-0, los dos miembros nuevos y las ocho mitades de D-S5), generado en el modo de medida (§6.2.1).')
R('**C02 no tiene excepción en PB-T4b:** el único cambio en los 20 archivos de origen (los 18 de la v4.4, FRP y FRG)',
  '**C02 no tiene excepción en PB-T4b:** el único cambio en los 21 archivos de origen (los 18 de la v4.4, FRP, FRG y WHHG)')

# ---- §4.2
R('**Las roturas por letra (C-5), método a método: 22.** Las 15 de la v4.3 (`pbt4-substitution-lists.md`, «C-5 y C-6»), las cinco que se midieron sobre el GREEN literal de PB-T4b y que el Owner añadió a la lista (`owner-pbt4b-breaks-decision.md`; la verificación, en la §17 de `independent-review-v1.md`) y las dos de PB-T4a, que se vieron al construir la v4.5 sobre su código real (`owner-pbt4b-list-477-decision.md`):',
  '**Las roturas por letra (C-5), método a método: 23.** Las 15 de la v4.3 (`pbt4-substitution-lists.md`, «C-5 y C-6»), las cinco que se midieron sobre el GREEN literal de PB-T4b y que el Owner añadió a la lista (`owner-pbt4b-breaks-decision.md`; la verificación, en la §17 de `independent-review-v1.md`), las dos de PB-T4a, que se vieron al construir la v4.5 sobre su código real (`owner-pbt4b-list-477-decision.md`), y la de la sexta tanda, que vio la precondición 6 de la v4.7 con la suite completa de Core (`owner-pbt4b-list-478-decision.md`; V48-03):')
R('| Su equivalente en `PushBackFullReadGuardFull`, con `FullEditIntentFakeReader` y `FullReadRig` |\n',
  '| Su equivalente en `PushBackFullReadGuardFull`, con `FullEditIntentFakeReader` y `FullReadRig` |\n'
  '| **WHHG:491-499** (`NoneOfThe30LiteralsInEnglish…`; v4.8) | **1** | **Cuenta exactamente los textos en español del anfitrión**, y el GREEN cambia tres: «La relectura no trajo resultado.» 2 → 0 (WHC:426 y :611 de la base: `CheckNow` y `ConfirmBase` ya no leen el piloto y responden `FullReadMissingReason`, D-PBT4-04), «El borrador se está aplicando o ya no está registrado.» 5 → 6 (la rama nueva de «Conservar» que presenta, D-PBT4-08, paso 2) y «El rack no tiene escala de anotación.» 1 → 0 (WHC:1783: la escala del modelo completo no es anulable) | Su equivalente en `WorkspaceHostHardeningGuardFull`, con la misma tabla y esas tres cuentas |\n')
R('**Ninguna prueba fuera de la lista de 477 cae.**',
  '**El recorrido de la v4.5 no veía los campos de tipo tupla** (su expresión `FIELD`, `walk_guards_pbt4b_v45.py:91`, no admitía espacios en el tipo), y por eso no marcó la '
  'guarda de la sexta tanda, cuya tabla es `(string English, string Spanish, int Times)[] Table` (WHHG:421). La precondición 6 de la v4.7 la vio caer con la suite completa '
  'de Core (`v47/analysis-outside-477.md`, punto 2).\n\n'
  '**El recorrido de la v4.8** (V48-04; `walk_guards_pbt4b_v48.py`, el de la v4.5 con esa corrección y nada más: el tipo admite una tupla y un genérico con espacios; '
  'reconoce campos nuevos en 18 archivos de guardas, `v48/walk-v48-same-inputs-as-v45-out.txt`):\n'
  '- **con las entradas de la v4.5** (la lista de 477, las 20 copias y el kit de la v4.5; `v48/compare_walks_v48-same-inputs-out.txt`): las mismas 1 096 pruebas, **tres '
  'marcadas más fuera de la lista**, y dos de G-9 de IG (ya marcadas) con más diferencias:\n'
  '  - **WHHG `NoneOfThe30LiteralsInEnglish…`**: las tres cuentas de arriba. **Entra en la lista** (la sexta tanda);\n'
  '  - **WHHG `TheGuardOfTheLiteralsTripsOnAnEnglishLiteralAMissingSpanishTextAndALostIdentifier`**: su hermana, la autoprueba de la guarda, que lee la misma tabla. **No '
  'entra en la lista:** es una autoprueba que inyecta textos sobre el anfitrión y **pasa en las dos suites completas del GREEN** (`/d/r4m` y `/d/r4o`);\n'
  '  - **`WorkspaceCommandAdvisorGuardTests.TheCatalogBodyDoesNotNameTheDocumentTypeAndNoSpanishReasonUsesAWordThatTheNeighbouringGuardsForbid`** (WCAG:468-474): el '
  'paseo ve que `"Read"` pasa de 21 a 22 en RIA. **No entra en la lista:** `"Read"` es el valor de la columna `Body` de su tupla `ReasonTable` (WCAG:408 y :447), el '
  'nombre de un cuerpo, no un literal que se cuente en RIA; la prueba solo comprueba que `CatalogBody()` no nombra `Document` y mira la columna `Spanish`. Es un falso '
  'positivo del paseo, y **pasa en las dos suites completas del GREEN** (`/d/r4m` y `/d/r4o`);\n'
  '  - G-9 de IG (`G9NoFileOfThePlugin…` y `G9TripsOnAPilotFile…`), ya marcadas desde la v4.4 y leídas: pasan en el GREEN;\n'
  '- **con las copias de la v4.8 y la lista de 478** (`v48/pbt4b-guard-walk-v48.tsv`, `v48/compare_walks_v48-copies-v48-out.txt` y `v48/walk-v48-flagged-diff.txt`): '
  'las pruebas de la revisión, las mismas; **ninguna cambia de categoría**; fuera de la lista, sin contar las copias ni el kit, **133 marcadas: las 131 de la v4.5 y las '
  'dos de arriba**, declaradas. La de la sexta tanda ya está en la lista.\n\n'
  '**Si la precondición 6 de la v4.8 hace fallar alguna de esas dos, para** y vuelve a la sesión.')
R('**Las 22 las rompe el cambio del anfitrión de PB-T4b, no la retirada** (M-33).', '**Las 23 las rompe el cambio del anfitrión de PB-T4b, no la retirada** (M-33).')

# ---- §5.2
R('### 5.2 PB-T4b: la lista cerrada CT-PBT4B-01 a 21 (entera; V44-07; V45-02, V45-03, V45-11; V47-02, V47-03)',
  '### 5.2 PB-T4b: la lista cerrada CT-PBT4B-01 a 21 (entera; V44-07; V45-02, V45-03, V45-11; V47-02, V47-03; V48-01, V48-05)')
SEC('### 5.2 PB-T4b', '### 5.3 PB-T4c', [
    ('(1) Cada uno de los 20 archivos de origen', '(1) Cada uno de los 21 archivos de origen', 1),
    ('(las cuatro de D-S5, dos: una en `<Clase>FullRedTests` y otra en `<Clase>FullGreenTests`)', '(las cuatro de D-S5, dos: una en `FullPanelCopy<Clase>RedTests` y otra en `FullPanelCopy<Clase>GreenTests`)', 1),
    ('(las 358 del mapa de la copia v4)', '(las 356 del mapa de la copia v5)', 1),
    ('está en `<Clase>FullRedTests` y en `<Clase>FullGreenTests`', 'está en `FullPanelCopy<Clase>RedTests` y en `FullPanelCopy<Clase>GreenTests`', 1),
    ('(20 líneas; los n de §6.2.1)', '(21 líneas; los n de §6.2.1)', 1),
    ('`copia <Clase> pruebas=<n> sha256=True` (11)', '`copia <Clase> pruebas=<n> sha256=True` (12)', 1),
    ('`los 20 nombres de clase de origen`', '`los 21 nombres de clase de origen`', 0),
    ('- **Core:** 450 de la lista (428 de C-0, 15 de C-5 de la v4.3, las 5 de la tercera tanda y las 2 de PB-T4a), 449 con copia o equivalente, y 18 CT.',
     '- **Core:** 451 de la lista (428 de C-0, 15 de C-5 de la v4.3, las 5 de la tercera tanda, las 2 de PB-T4a y la de la sexta tanda), 450 con copia o equivalente, y 18 CT.', 1),
    ('19 604', '19 605', 3),
    ('457 `Skipped`', '458 `Skipped`', 4),
    ('19 586 =', '19 587 =', 3),
    ('- 19 605 = 19 137 + 449 + 18; 19 147 = 19 130 − 1 + 18; 457 = 7 + 450.', '- 19 605 = 19 137 + 450 + 18; 19 147 = 19 130 − 1 + 18; 458 = 7 + 451.', 1),
    ('**Las dos de PB-T4a suben el RED sin las CT de 19 584 (v4.4) a 19 586** (cada una suma un resultado); la UI no cambia.',
     '**La de la sexta tanda sube el RED sin las CT de 19 586 (v4.7) a 19 587**, como las dos de PB-T4a lo subieron de 19 584 (v4.4) a 19 586 (cada una suma un resultado); la UI no cambia.', 1),
    ('de las clases `…FullRedTests` de Core y de UI', 'de las clases `FullPanelCopy…RedTests` de Core y de UI', 1),
    ('(`partition_pbt4b_v47.py red`,', '(`partition_pbt4b_v48.py red`,', 1),
    ('va a `…FullRedTests`; todos `Passed`, a `…FullGreenTests`;', 'va a `FullPanelCopy…RedTests`; todos `Passed`, a `FullPanelCopy…GreenTests`;', 1),
    ('uno en `…FullRedTests` con las filas que fallan y otro en `…FullGreenTests` con las que pasan', 'uno en `FullPanelCopy…RedTests` con las filas que fallan y otro en `FullPanelCopy…GreenTests` con las que pasan', 1),
    ('`SPLIT` y `RED_KEYS` de `pbt4b_hand_edits_v47.py`).', '`SPLIT` y `RED_KEYS` de `pbt4b_hand_edits_v48.py`, los de la v4.7).', 1),
    ('19 586 y 2 478', '19 587 y 2 478', 1),
    ('**una prueba que falle fuera de la lista de 477 vuelve al Owner.**', '**una prueba que falle fuera de la lista de 478 vuelve al Owner.**', 1),
], '§5.2')

# ---- §6.0 (RR)
R('**En PB-T4b,** lo mismo para los 20 archivos con sus `Skip`; y una reparación tampoco toca el kit ni las 20 copias, cuyo texto es el del paquete.',
  '**En PB-T4b,** lo mismo para los 21 archivos con sus `Skip`; y una reparación tampoco toca el kit ni las 21 copias, cuyo texto es el del paquete.')
R('en PB-T4b, ningún archivo de `src/`, ni los 20 archivos de los `Skip`, ni el kit, ni las copias;', 'en PB-T4b, ningún archivo de `src/`, ni los 21 archivos de los `Skip`, ni el kit, ni las copias;')
R('los 20 nombres de clase de origen, las nueve parejas de M-00 a M-08 (con los seis tipos de M-00 de §5.2), los `BaseSha` de los 20 archivos de los `Skip` (§6.2.1) y los `CopySha` de las 20 copias y del kit;',
  'los 21 nombres de clase de origen, las nueve parejas de M-00 a M-08 (con los seis tipos de M-00 de §5.2), los `BaseSha` de los 21 archivos de los `Skip` (§6.2.1) y los `CopySha` de las 21 copias y del kit;')
RR3 = ('| PB-T4b | `WorkspacePilotPrepareFull`, `%s` (a mano, R-D-S1-borra; D-S1, V48-02) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |\n' % INVALID_E
       + '| PB-T4b | `WorkspacePilotPrepareFull`, `%s` (a mano, R-D-S1-borra; D-S1, V48-02) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |\n' % NEVER
       + '| PB-T4b | `WorkspaceHostHardeningGuardFull`, `%s` (equivalente de una guarda, C-5; owner-pbt4b-list-478-decision.md) | **El texto entero de la prueba, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |\n' % NONE30
       + '| PB-T4b | `WorkspaceHostHardeningGuardFull`, el ayudante `Table` (las tres cuentas nuevas, V48-03) | **El texto entero del ayudante, el del guion** (el `CopySha` de su archivo): no admite reparación (RR-2) |\n')
R('| PB-T4b | Las 358 copias mecánicas, las 27 de la interfaz y el kit |', RR3 + '| PB-T4b | Las 356 copias mecánicas, las 27 de la interfaz y el kit |')

# ---- §6.2
SEC('### 6.2 PB-T4b', '### 6.3 PB-T4c', [
    ('(entero; V44-09; V45-01, V45-02, V45-10; V46-01, V46-02; V47-01, V47-02)', '(entero; V44-09; V45-01, V45-02, V45-10; V46-01, V46-02; V47-01, V47-02; V48-01 a V48-05)', 1),
    ('**con la lista exacta de 477** que el Owner tomó como la que vio (las 470 de `owner-pbt4b-retire-list-decisions.md`, pregunta 2, las cinco de `owner-pbt4b-breaks-decision.md` y las dos de `owner-pbt4b-list-477-decision.md`; `pbt4b-substitution-list-477.tsv`, sha256 `5975c5b6…a651`)',
     '**con la lista exacta de 478** que el Owner tomó como la que vio (las 470 de `owner-pbt4b-retire-list-decisions.md`, pregunta 2, las cinco de `owner-pbt4b-breaks-decision.md`, las dos de `owner-pbt4b-list-477-decision.md` y la de `owner-pbt4b-list-478-decision.md`; `pbt4b-substitution-list-478.tsv`, sha256 `%s`)' % short(LIST478), 1),
    ('- **`AllowedWriteScope`** (57), ruta a ruta:', '- **`AllowedWriteScope`** (59), ruta a ruta:', 1),
    ('  - **las copias (20), nuevas, que genera el guion:**', '  - **las copias (21), nuevas, que genera el guion:**', 1),
    ('`WorkspaceAssociativeBookkeepingFull.cs`, `WorkspacePilotUpdateIntegrationGuardFull.cs`,', '`WorkspaceAssociativeBookkeepingFull.cs`, `WorkspaceHostHardeningGuardFull.cs`, `WorkspacePilotUpdateIntegrationGuardFull.cs`,', 1),
    ('  - **solo para los `Skip` (20):** los 20 archivos de origen: las 9 clases de C-0, `PushBackWritePathGuardTests.cs`, IG, EIG, PEG, RG, WLG, WPUG,',
     '  - **solo para los `Skip` (21):** los 21 archivos de origen: las 9 clases de C-0, `PushBackWritePathGuardTests.cs`, IG, EIG, PEG, RG, WLG, WPUG, WHHG (`tests/RackCad.Tests/Workspace/WorkspaceHostHardeningGuardTests.cs`),', 1),
    ('  - **los 477 `Skip`** literales, en los 20 archivos de origen, y nada más en esos archivos;', '  - **los 478 `Skip`** literales, en los 21 archivos de origen, y nada más en esos archivos;', 1),
    ('  - **las 20 copias,** byte a byte las que genera `make_copies_pbt4b_v47.py`', '  - **las 21 copias,** byte a byte las que genera `make_copies_pbt4b_v48.py`', 1),
    ('apply_red_pbt4b_v47.py', 'apply_red_pbt4b_v48.py', 3),
    ('check_skips_pbt4b_v47.py', 'check_skips_pbt4b_v48.py', 3),
    ('check_skips_copy_v47.py', 'check_skips_copy_v48.py', 1),
    ('pbt4b_skips_v47.py', 'pbt4b_skips_v48.py', 2),
    ('partition_pbt4b_v47.py', 'partition_pbt4b_v48.py', 2),
    # el contrato, en ASCII
    ('segun el paquete del contrato de PB-T4 v4.7, las decisiones del Owner sobre PB-T4 (sus cinco tandas,', 'segun el paquete del contrato de PB-T4 v4.8, las decisiones del Owner sobre PB-T4 (sus seis tandas,', 1),
    ('la lista exacta de 477 sustituciones', 'la lista exacta de 478 sustituciones', 1),
    ('INV-PBT4B-01. Alcance: dentro de las 57 rutas.', 'INV-PBT4B-01. Alcance: dentro de las 59 rutas.', 1),
    ('en tests/ solo las seis clases de las CT, el kit, las 20 copias y las lineas de atributo con Skip de los 20 archivos de la lista.',
     'en tests/ solo las seis clases de las CT, el kit, las 21 copias y las lineas de atributo con Skip de los 21 archivos de la lista.', 1),
    ('el kit y las 20 copias byte a byte (su sha256, CopySha), los 477 Skip', 'el kit y las 21 copias byte a byte (su sha256, CopySha), los 478 Skip', 1),
    ('ni el kit, ni las copias, ni los 20 archivos de los Skip; el criterio', 'ni el kit, ni las copias, ni los 21 archivos de los Skip; el criterio', 1),
    ('salvo las 477 de la lista cerrada (pbt4b-substitution-list-477.tsv, sha256 5975c5b6b9111c3a4048193b5ebf10b619a567996dea8707b88ff6a00105a651)',
     'salvo las 478 de la lista cerrada (pbt4b-substitution-list-478.tsv, sha256 %s)' % LIST478, 1),
    ('the Skip of each of the 477 tests of pbt4b-substitution-list-477.tsv, with its literal reason, in its attribute line, and nothing else in those 20 files.',
     'the Skip of each of the 478 tests of pbt4b-substitution-list-478.tsv, with its literal reason, in its attribute line, and nothing else in those 21 files.', 1),
    ('(the 477 of pbt4b-substitution-list-477.tsv)', '(the 478 of pbt4b-substitution-list-478.tsv)', 1),
    ('la de las clases FullRedTests de las copias', 'la de las clases FullPanelCopy<Raiz>RedTests de las copias', 1),
    ('el sha256 del kit y de cada una de las 20 copias (iguales a CopySha)', 'el sha256 del kit y de cada una de las 21 copias (iguales a CopySha)', 1),
    ('ni el kit, ni las 20 copias, ni los 20 archivos de los Skip.', 'ni el kit, ni las 21 copias, ni los 21 archivos de los Skip.', 1),
    ('la tabla por clase y por causa de las 477 (C-0 con sus 9 clases, 358 copias y 70 a mano,', 'la tabla por clase y por causa de las 478 (C-0 con sus 9 clases, 356 copias y 72 a mano,', 1),
    ('C-5 con sus 22, las 2 de PB-T4a entre ellas;', 'C-5 con sus 23, las 2 de PB-T4a y la de la sexta tanda entre ellas;', 1),
    ('PB-T4b pone 477 Skip (450 en Core y 27 en UI)', 'PB-T4b pone 478 Skip (451 en Core y 27 en UI)', 1),
    ('anade 449 copias o equivalentes en Core', 'anade 450 copias o equivalentes en Core', 1),
    ('El GREEN: Core 19 604 resultados, con 19 147 Passed, 0 Failed y 457 Skipped;', 'El GREEN: Core 19 605 resultados, con 19 147 Passed, 0 Failed y 458 Skipped;', 1),
    ('de las clases FullRedTests de la particion de Core y de UI', 'de las clases FullPanelCopy<Raiz>RedTests de la particion de Core y de UI', 1),
    ('frente a BaseSha: los 20 archivos, iguales a su base con su Skip literal y nada mas (477 Skip).', 'frente a BaseSha: los 21 archivos, iguales a su base con su Skip literal y nada mas (478 Skip).', 1),
    # la tabla de los Skip
    ('| C-0, copia mecánica (358) |', '| C-0, copia mecánica (356) |', 1),
    ('| C-0, a mano (70) |', '| C-0, a mano (72) |', 1),
    ('decision del Owner owner-pbt4b-list-477-decision.md).` |\n',
     'decision del Owner owner-pbt4b-list-477-decision.md).` |\n'
     '| C-5, la de la sexta tanda (WHHG) | `I-64 F4B PB-T4b: sustituida por su equivalente en WorkspaceHostHardeningGuardFull (registro declarado superseding-characterization.md; decision del Owner owner-pbt4b-list-478-decision.md).` |\n', 1),
    ('lee los 477 (los 49 de C-5 y de la interfaz, y los 428 de C-0) en los 20 archivos', 'lee los 478 (los 50 de C-5 y de la interfaz, y los 428 de C-0) en los 21 archivos', 1),
    # BaseSha
    ('| `tests/RackCad.Tests/Workspace/WorkspaceEditIntentHostTests.cs` |',
     '| `tests/RackCad.Tests/Workspace/WorkspaceHostHardeningGuardTests.cs` | `%s` | 1 |\n| `tests/RackCad.Tests/Workspace/WorkspaceEditIntentHostTests.cs` |' % WHHG_BASE, 1),
    # los pasos
    ('las 20 copias en modo Probe, una clase `<Clase>FullProbeTests` por archivo;', 'las 21 copias en modo Probe, una clase `FullPanelCopy<Clase>ProbeTests` por archivo;', 1),
    ('las TRX de `FullyQualifiedName~RackCad.Tests.FullPanelCopies` y de `FullyQualifiedName~RackCad.UI.Tests.FullPanelCopies`;',
     'las TRX de `FullyQualifiedName~RackCad.Tests.FullPanelCopy` y de `FullyQualifiedName~RackCad.UI.Tests.FullPanelCopy` (D-S6);', 1),
    ('**Core 19 586 = (19 129 − R_C) + R_C + 457**', '**Core 19 587 = (19 129 − R_C) + R_C + 458**', 1),
    ('**Core 19 586 = 19 129 + 0 + 457**', '**Core 19 587 = 19 129 + 0 + 458**', 1),
    ('**Una prueba que falle fuera de la lista de 477** vuelve al Owner;', '**Una prueba que falle fuera de la lista de 478** vuelve al Owner;', 1),
    ('el contrato lleva por sha256 la partición, las 20 copias generadas con ella', 'el contrato lleva por sha256 la partición, las 21 copias generadas con ella', 1),
    ('**La partición cerrada en `<Clase>FullRedTests` y `<Clase>FullGreenTests`', '**La partición cerrada en `FullPanelCopy<Clase>RedTests` y `FullPanelCopy<Clase>GreenTests`', 1),
    # RequiredTests
    ('| 12 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.Workspace` | **2719** | false |',
     '| 12 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.Workspace` | **2718** | false |', 1),
    ('`FullyQualifiedName~RackCad.Tests.FullPanelCopies.PushBackFullReadGuardFullGreenTests` | `<medido en la precondicion 6>` | false |\n',
     '`FullyQualifiedName~RackCad.Tests.FullPanelCopies.PushBackFullReadGuardFullGreenTests` | `<medido en la precondicion 6>` | false |\n'
     '| 87 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopies.WorkspaceHostHardeningGuardFullRedTests` | `<medido en la precondicion 6>` | true |\n'
     '| 88 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.FullPanelCopies.WorkspaceHostHardeningGuardFullGreenTests` | `<medido en la precondicion 6>` | false |\n', 1),
    ('- **Nota (no va al contrato).** Los filtros heredados no seleccionan las copias: su espacio de nombres es `RackCad.Tests.FullPanelCopies` (o `RackCad.UI.Tests.FullPanelCopies`), y `FullyQualifiedName~` contiene `RackCad.Tests.Workspace` o `RackCad.Tests.PushBackFullRead…` solo en las clases del espacio `RackCad.Tests`.',
     '- **Nota (no va al contrato).** Los filtros heredados no seleccionan las copias (D-S6): sus clases están en el espacio raíz y empiezan por `FullPanelCopy`, y `FullyQualifiedName~RackCad.Tests.FullPanelCopy…` no contiene `RackCad.Tests.Workspace`, `RackCad.Tests.PushBackFullRead…` ni ningún otro filtro de la tabla (medido por texto, `pbt4bcc/compile/out-diag-analysis/filter-overlap.txt`: 0 de 46). La guarda de los textos del anfitrión de la sexta tanda (WHHG) está en `RackCad.Tests` y el filtro n.º 12 la seleccionaba: con su `Skip`, su `MinSelected` baja de 2 719 a **2 718**.', 1),
], '§6.2')
# las 42 filas de las clases de las copias (n.º 47 a 88), con los nombres de D-S6
RX(r'`FullyQualifiedName~(RackCad(?:\.UI)?\.Tests)\.FullPanelCopies\.(\w+?)Full(Red|Green)Tests`', r'`FullyQualifiedName~\1.FullPanelCopy\2\3Tests`', 42, 'filas 47-88')
s62 = body[body.index('\n### 6.2 PB-T4b'):body.index('\n### 6.3 PB-T4c')]
assert 'FullPanelCopies' not in s62.replace('`v47/analysis-outside-477.md`', ''), [l[:120] for l in s62.split('\n') if 'FullPanelCopies' in l]
gen_line = [l for l in s62.split('\n') if l.startswith('**El guion de la copia y los equivalentes, literales:**')]
assert len(gen_line) == 1
R(gen_line[0],
  '**El guion de la copia y los equivalentes, literales:** `make_copies_pbt4b_v48.py` (sha256 `%s`), que usa las piezas de `make_copies_pbt4b_v47.py` (sha256 `%s`), '
  '`make_copies_pbt4b_v46.py` (sha256 `%s`) y `make_copies_pbt4b.py` de la v4.4 (sha256 `%s`) sin tocarlas, con `pbt4b_hand_edits_v48.py` (sha256 `%s`), que carga '
  '`pbt4b_hand_edits_v47.py` (`%s`), `pbt4b_hand_edits_v46.py` (`%s`), `pbt4b_hand_edits_v45.py` (`%s`) y `pbt4b_hand_edits.py` (`%s`), `copy_map_defs.py`, '
  '`lists_core.py`, la lista de 478 y el mapa de la copia v5. En el modo de medida, sobre `78a7cef3`, da 21 archivos (`v48/copies_probe_v48`; el sha256 del conjunto '
  'de las líneas «sha256  ruta», ordenadas, `%s`; `v48/copies_probe_v48-out.txt`), y su revisión, `pbt4b-equivalents-review-v48.txt` (sha256 `%s`), lleva el texto '
  'final de las 114 entradas. Las de la v4.5 a la v4.7 quedan como historia. **La autoprueba** (`selftest_pbt4b_v48.py`, `v48/selftest_pbt4b_v48-out.txt`) comprueba los '
  '478 `Skip` (72 a mano), D-S6 en las 21 copias, el texto de D-S1 en las dos de WPP, las tres cuentas de la guarda nueva y, con una partición sintética, las '
  'clases de las cuatro de D-S5.'
  % (GEN, sha('make_copies_pbt4b_v47.py'), sha('make_copies_pbt4b_v46.py'), sha('make_copies_pbt4b.py'), EDITS, sha('pbt4b_hand_edits_v47.py'),
     sha('pbt4b_hand_edits_v46.py'), sha('pbt4b_hand_edits_v45.py'), sha('pbt4b_hand_edits.py'), PROBE, REVIEW), 1, 'generador')
blk = body[body.index('\n#### 6.2.1 '):body.index('\n#### 6.2.2 ')]
code = blk[blk.index('```text\n') + 8:blk.index('\n```', blk.index('```text\n'))]
assert all(ord(ch) < 0x80 for ch in code)

# ---- §6.2.2
SEC('#### 6.2.2 ', '### 6.3 PB-T4c', [
    ('cita el paquete v4.7, CT-PBT4B-01..21, las **477** sustituciones (antes, 475)', 'cita el paquete v4.8, CT-PBT4B-01..21, las **478** sustituciones (antes, 477)', 1),
    ('| Las 57 rutas, en ese orden;', '| Las 59 rutas, en ese orden;', 1),
    ('(con el de la v4.4, 407) |', '(con el de la v4.4, 407; el de la v4.8 añade dos rutas de `tests/`, la copia y el archivo de la guarda nueva: `<medido por el generador>`) |', 1),
    ('**C02** no tiene excepción fuera de los `Skip` de la lista de 477 |', '**C02** no tiene excepción fuera de los `Skip` de la lista de 478 |', 1),
    ('n.º 12: **2 719**;', 'n.º 12: **2 718**;', 1),
    ('y las 40 filas de las clases de las copias', 'y las 42 filas de las clases de las copias (D-S6: `FullPanelCopy<Clase>RedTests` y `…GreenTests`)', 1),
    ('de `Skip` (**477**)', 'de `Skip` (**478**)', 1),
    ('más las clases `FullRedTests` de la partición', 'más las clases `FullPanelCopy…RedTests` de la partición', 1),
    ('| **La lista** | `pbt4b-substitution-list-477.tsv` | sha256 `5975c5b6…a651`; es la de 475 (`654c3b7b…`) con las dos filas al final; recuentos C-0 428, C-5 **22** y UI 27 |',
     '| **La lista** | `pbt4b-substitution-list-478.tsv` | sha256 `%s`; es la de 477 (`5975c5b6…a651`) con la fila de WHHG al final; recuentos C-0 428, C-5 **23** y UI 27 |' % short(LIST478), 1),
    ('| **El mapa de la copia** | `pbt4b-copy-map-v4.tsv` | sha256 `0c5abc6d…9b1c`; **358** mecánicas y **70** a mano (los motivos de la v4.3, siete `R-D-S1`, ocho `R-D-S1-compila`, dos `R-D-S5` y uno `R-D-S1,R-D-S5`) |',
     '| **El mapa de la copia** | `pbt4b-copy-map-v5.tsv` | sha256 `%s`; **356** mecánicas y **72** a mano (los motivos de la v4.3, siete `R-D-S1`, ocho `R-D-S1-compila`, dos `R-D-S5`, uno `R-D-S1,R-D-S5` y dos `R-D-S1-borra`) |' % short(MAP5), 1),
    ('`make_copies_pbt4b_v47.py`, `make_copies_pbt4b_v46.py` y `make_copies_pbt4b.py`, `pbt4b_hand_edits_v47.py`, `pbt4b_hand_edits_v46.py`, `pbt4b_hand_edits_v45.py` y `pbt4b_hand_edits.py`, la partición y las 20 copias',
     '`make_copies_pbt4b_v48.py`, `make_copies_pbt4b_v47.py`, `make_copies_pbt4b_v46.py` y `make_copies_pbt4b.py`, `pbt4b_hand_edits_v48.py`, `pbt4b_hand_edits_v47.py`, `pbt4b_hand_edits_v46.py`, `pbt4b_hand_edits_v45.py` y `pbt4b_hand_edits.py`, la partición y las 21 copias', 1),
    ('| Los 20, medidos en `78a7cef3` (la tabla de arriba) |', '| Los 21, medidos en `78a7cef3` (la tabla de arriba) |', 1),
    ('CopySha (las 20), R_C, R_UI y `MinSelected` de las 40 clases de las copias (0: la fila sale)', 'CopySha (las 21), R_C, R_UI y `MinSelected` de las 42 clases de las copias, por el nombre de la clase (`FullPanelCopy<Clase>RedTests` o `…GreenTests`; 0: la fila sale)', 1),
    ('`owner-pbt4b-list-477-decision.md` (la quinta tanda, que la emisión de PB-T4b custodiará junto a las otras tandas y a `session-pbt4b-design-decisions.md`), `session-pbt4b-design-decisions.md` (D-S1 a D-S3 y D-S5;',
     '`owner-pbt4b-list-477-decision.md` (la quinta tanda) y `owner-pbt4b-list-478-decision.md` (la sexta), que la emisión de PB-T4b custodiará junto a las otras tandas y a `session-pbt4b-design-decisions.md`, `session-pbt4b-design-decisions.md` (D-S1 a D-S3, D-S5 y D-S6;', 1),
    ('este paquete (v4.7), con su comprobación acotada', 'este paquete (v4.8), con su comprobación acotada', 1),
], '§6.2.2')

# ---- §6.3, §7, §8 y §11
R('segun el paquete del contrato de PB-T4 v4.7 y la lista cerrada CT-PBT4C-01..15', 'segun el paquete del contrato de PB-T4 v4.8 y la lista cerrada CT-PBT4C-01..15')
R('(470; en la tercera tanda, 475; en la quinta, 477)', '(470; en la tercera tanda, 475; en la quinta, 477; en la sexta, 478)')
R('**358 copias mecánicas y 70 equivalentes a mano** (D-S1 y D-S5; la v4.3, 375 y 53; la v4.5, 368 y 60; la v4.6, 360 y 68; `pbt4b-copy-map-v4.tsv`)',
  '**356 copias mecánicas y 72 equivalentes a mano** (D-S1 y D-S5; la v4.3, 375 y 53; la v4.5, 368 y 60; la v4.6, 360 y 68; la v4.7, 358 y 70; `pbt4b-copy-map-v5.tsv`)')
R('| «unas 12»; en la tercera tanda, 20; en la quinta, **22** | **22** (15, las cinco de la tercera tanda y las dos de PB-T4a) |',
  '| «unas 12»; en la tercera tanda, 20; en la quinta, 22; en la sexta, **23** | **23** (15, las cinco de la tercera tanda, las dos de PB-T4a y la de la sexta) |')
R('en la quinta, **477** | **477** | **Aprobada como la que vio** (segunda vuelta, pregunta 2, y tercera y quinta tandas); `pbt4b-substitution-list-477.tsv`, sha256 `5975c5b6b9111c3a4048193b5ebf10b619a567996dea8707b88ff6a00105a651` (las de 475, `654c3b7b…0707c`, y 470, `44393b19…63e4`, quedan como historia',
  'en la quinta, 477; en la sexta, **478** | **478** | **Aprobada como la que vio** (segunda vuelta, pregunta 2, y tercera, quinta y sexta tandas); `pbt4b-substitution-list-478.tsv`, sha256 `%s` (las de 477, `5975c5b6…a651`, 475, `654c3b7b…0707c`, y 470, `44393b19…63e4`, quedan como historia' % LIST478)
R('**Las 477 no dependen de la retirada** (M-33)', '**Las 478 no dependen de la retirada** (M-33)')
R('es la lista que vio el Owner, y si aparece otra prueba que falle fuera de ella, vuelve al Owner |\n',
  'es la lista que vio el Owner, y si aparece otra prueba que falle fuera de ella, vuelve al Owner |\n'
  '| **6.ª** (2026-11-20) | `owner-pbt4b-list-478-decision.md` | «Guarda T4b»: «Añadirla a la lista (Recomendado)». «Mantener las cuentas» y «No emitir PB-T4b» no se hacen | '
  '**La lista de PB-T4b pasa de 477 a 478** (WHHG `NoneOfThe30…`), con su `Skip` y su equivalente `WorkspaceHostHardeningGuardFull`, la misma tabla con las cuentas '
  '0, 6 y 0, cada una con su `// before N`; es la lista que vio el Owner, y si aparece otra prueba que falle fuera de ella, vuelve al Owner |\n')
R('- **D-S1 a D-S3 y D-S5** (`session-pbt4b-design-decisions.md`, 2026-11-20): **D-S1**,', '- **D-S1 a D-S3, D-S5 y D-S6** (`session-pbt4b-design-decisions.md`, 2026-11-20): **D-S1**,')
R('sin volver al Owner, porque la lista, las aserciones, las filas y los totales no cambian (358/70 en C-0);',
  'sin volver al Owner, porque la lista, las aserciones, las filas y los totales no cambian (358/70 en C-0); **D-S6** (tras la v4.7; V48-01), las copias en el '
  'espacio de nombres raíz de su ensamblado, con el prefijo `FullPanelCopy` en sus clases, para que `NamespaceFolderGuardTests` siga en verde sin tocarla ni tocar ningún '
  'filtro heredado (el precedente de D-S2); y **D-S1 en la v4.8** (V48-02), las dos copias de WPP que fijaban que el piloto nunca borra una vista pasan a equivalentes '
  'que afirman lo que ya fijan D-PB-04, D-PBT3-09 (e) y D-PBT4-07 (356/72 en C-0);')
R('que custodia los cinco registros del Owner sobre PB-T4', 'que custodia los seis registros del Owner sobre PB-T4')
R('y D-S1 a D-S5 como las escribió la sesión.', 'y D-S1 a D-S6 como las escribió la sesión; la v4.8 aplica la 6.ª como dice (la guarda de los textos del anfitrión, en la lista con su equivalente).')
R('(470, 475 en la tercera tanda y 477 en la quinta)', '(470, 475 en la tercera tanda, 477 en la quinta y 478 en la sexta)')
R('`owner-pbt4a-cost-decision.md` y `owner-pbt4b-list-477-decision.md`), con su número F4B-E al custodiarlas; y las decisiones de la sesión, `session-pbt4b-design-decisions.md` (D-S1 a D-S3 y D-S5)',
  '`owner-pbt4a-cost-decision.md`, `owner-pbt4b-list-477-decision.md` y `owner-pbt4b-list-478-decision.md`), con su número F4B-E al custodiarlas; y las decisiones de la sesión, `session-pbt4b-design-decisions.md` (D-S1 a D-S3, D-S5 y D-S6)')
p11 = [l for l in body.split('\n') if l.startswith('   - **PB-T4b** (V45-04, V46-07 y V47-05;')]
assert len(p11) == 1
new11 = (p11[0].replace('(V45-04, V46-07 y V47-05; los ocho pasos de §6.2.1, con los guiones de la v4.7;', '(V45-04, V46-07, V47-05 y V48-06; los ocho pasos de §6.2.1, con los guiones de la v4.8;')
         .replace('apply_red_pbt4b_v47.py', 'apply_red_pbt4b_v48.py').replace('partition_pbt4b_v47.py', 'partition_pbt4b_v48.py').replace('check_skips_pbt4b_v47.py', 'check_skips_pbt4b_v48.py')
         .replace('**Core 19 586 = (19 129 − R_C) + R_C + 457**', '**Core 19 587 = (19 129 − R_C) + R_C + 458**').replace('**Core 19 586 = 19 129 + 0 + 457**', '**Core 19 587 = 19 129 + 0 + 458**')
         .replace('19 604 = 19 147 + 0 + 457', '19 605 = 19 147 + 0 + 458').replace('fuera de la lista de 477', 'fuera de la lista de 478'))
assert 'v47' not in new11 and '477' not in new11 and '19 586' not in new11, new11
R(p11[0], new11, 1, '11.6')
R('**PB-T4b, las 477 de `pbt4b-substitution-list-477.tsv`** (sha256 `5975c5b6b9111c3a4048193b5ebf10b619a567996dea8707b88ff6a00105a651`)',
  '**PB-T4b, las 478 de `pbt4b-substitution-list-478.tsv`** (sha256 `%s`)' % LIST478)
p9 = [l for l in body.split('\n') if l.startswith('9. **Para PB-T4b y PB-T4c:**')]
assert len(p9) == 1
new9 = (p9[0].replace('esta versión (v4.7), la v4.6, la v4.5 y la v4.4, sus fragmentos de `v44/`, `v45/`, `v46/` y `v47/`,', 'esta versión (v4.8), la v4.7, la v4.6, la v4.5 y la v4.4, sus fragmentos de `v44/`, `v45/`, `v46/`, `v47/` y `v48/`,')
        .replace('las listas (`pbt4b-substitution-list-477.tsv`,', 'las listas (`pbt4b-substitution-list-478.tsv`, `pbt4b-substitution-list-477.tsv`,')
        .replace('los mapas de la copia v2, v3 y v4,', 'los mapas de la copia v2 a v5,')
        .replace('los guiones de la v4.4 a la v4.7 y sus salidas, la partición y las 20 copias', 'los guiones de la v4.4 a la v4.8 y sus salidas, la partición y las 21 copias')
        .replace('`pbt4b-equivalents-review-v46.txt` y `pbt4b-equivalents-review-v47.txt`,', '`pbt4b-equivalents-review-v46.txt`, `pbt4b-equivalents-review-v47.txt` y `pbt4b-equivalents-review-v48.txt`, el análisis de la v4.7 (`v47/analysis-outside-477.md`),')
        .replace('los registros del Owner de la tercera y la quinta tandas (`owner-pbt4b-breaks-decision.md` y `owner-pbt4b-list-477-decision.md`;', 'los registros del Owner de la tercera, la quinta y la sexta tandas (`owner-pbt4b-breaks-decision.md`, `owner-pbt4b-list-477-decision.md` y `owner-pbt4b-list-478-decision.md`;'))
assert new9.count('v4.8') == 2 and 'list-478' in new9 and 'review-v48' in new9 and '21 copias' in new9 and 'list-478-decision' in new9, new9
R(p9[0], new9, 1, '11.9')
R('Su texto literal, en el anexo V47-A', 'Su texto literal, en el anexo V48-A', 0)
# lo que queda de la v4.7 en el cuerpo y no es historia
R('Los motivos, en ASCII, los fija `pbt4b_skips_v47.py` (con el mapa de la copia v4)', 'Los motivos, en ASCII, los fija `pbt4b_skips_v48.py` (con la lista de 478 y el mapa de la copia v5)')
R('**Con la partición, el guion pone su texto dos veces,** en `<Clase>FullRedTests` con las filas que fallan en el RED y en `<Clase>FullGreenTests`',
  '**Con la partición, el guion pone su texto dos veces,** en `FullPanelCopy<Clase>RedTests` con las filas que fallan en el RED y en `FullPanelCopy<Clase>GreenTests`')
R('(9, con los números del mapa de la copia v4;', '(9, con los números del mapa de la copia v5;')
R('- Sin las CT: 19 587 = 19 137 + 449;', '- Sin las CT: 19 587 = 19 137 + 450;')
R('(`SPLIT` y `RED_KEYS` de `pbt4b_hand_edits_v47.py`): da dos filas de la partición', '(`SPLIT` y `RED_KEYS` de `pbt4b_hand_edits_v48.py`, los de la v4.7): da dos filas de la partición')
R('`PushBackFullReadPathTests` 1 y `PushBackFullReadGuardTests` 2. **Core 450 y UI 27** (medido, `v45/selftest_pbt4b_v45-out.txt`).',
  '`PushBackFullReadPathTests` 1, `PushBackFullReadGuardTests` 2 y `WorkspaceHostHardeningGuardTests` 1. **Core 451 y UI 27** (medido, `v45/selftest_pbt4b_v45-out.txt` y, la de la sexta tanda, `v48/selftest_pbt4b_v48-out.txt`).')
R('el git diff -U0 BaseSha..CurrentSha de los 20 archivos de pruebas existentes', 'el git diff -U0 BaseSha..CurrentSha de los 21 archivos de pruebas existentes')

# ---- comprobaciones y escritura
assert 'el anexo V47-A' not in body and body.count('V47-A') == 1, [l[:100] for l in body.split('\n') if 'V47-A' in l]
KEEP = []
for h_, u_ in (('### 5.1 PB-T4a', '### 5.2 PB-T4b'), ('### 6.1 PB-T4a', '### 6.2 PB-T4b')):
    i_ = t.index('\n' + h_) + 1
    KEEP.append(t[i_:t.index('\n' + u_, i_) + 1])
v = headpart + body + ANNEX + '\n'
for k_ in KEEP:
    assert v.count(k_) == 1, 'PB-T4a cambió'
data = v.encode('utf-8')
assert b'\r' not in data
open(OUT, 'wb').write(data)
print('v4.8', hashlib.sha256(data).hexdigest(), len(data), 'bytes; reemplazos', len(done))
print('lista 478', LIST478, '| mapa v5', MAP5, '| sesion', SESSION, '| owner 478', OWNER478)
print('generador', GEN, '| ediciones', EDITS, '| revision', REVIEW, '| conjunto', PROBE, '| BaseSha WHHG', WHHG_BASE)
print('filas RR-3 de PB-T4b', sum(1 for l in v.split('\n') if l.startswith('| PB-T4b |')), '| anexo', n_annex, 'bloques | D-S6', v.count('D-S6'))
body_req = [l for l in v[v.index('\n#### 6.2.1 '):v.index('\n#### 6.2.2 ')].split('\n') if re.match(r'^\| \d+ \| `tests/', l)]
print('RequiredTests de PB-T4b', len(body_req), '| filas de las copias', sum(1 for l in body_req if 'FullPanelCopy' in l))
