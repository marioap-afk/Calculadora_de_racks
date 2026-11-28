"""Contrato de gate de F4B-PBTC2-DECLARED-CORRECTIONS (ASCII), desde el paquete del contrato de PB-TC2 v2
(docs/initiatives/I-64-f4b-pbtc2-gate-contract-package.md, section 6) y el patron de make_pbtc_contract.py.
El ForbiddenWriteScope de src/ sale del algoritmo de [PKC-C] (section 6.3 del paquete): por cada entrada de src/ en HEAD se emite
<dir>/ solo si ninguna ruta permitida queda debajo; si queda alguna, se desciende; los archivos se emiten uno a uno.
Uso: python make_pbtc2_contract.py <IssuedUtc> <AuthorityRevision> <MainSha> [--dry] [--print]
Con --dry no escribe nada: comprueba e imprime los recuentos y los bytes (con --print, ademas, el ForbiddenWriteScope numerado).
Al custodiarlo, la ruta local del worktree se redacta como en PB-T0 ('<worktree-de-I-64>')."""
import json, os, re, sys, subprocess, hashlib
WT = '<worktree-de-I-64>'  # Redaccion para el repo publico: la ruta local del worktree
os.chdir(WT)
ISSUED, AR, MAIN = sys.argv[1], sys.argv[2], sys.argv[3]
TASK = 'F4B-PBTC2-DECLARED-CORRECTIONS'
OUT = 'artifacts/orchestration/I-64/%s/coordinator/gate-contract.json' % TASK
PILOT = 'docs/automation/evidence/I-64-pilot'


def in_head(path):
    return subprocess.run(['git', 'cat-file', '-e', 'HEAD:' + path.rstrip('/')], capture_output=True).returncode == 0


def show(path):
    return subprocess.run(['git', 'show', 'HEAD:' + path], capture_output=True).stdout.decode('ascii')


T1BC = PILOT + '/F4-T1B-PILOT-CHARACTERIZATION/coordinator/gate-contract.json'
PBTCC = PILOT + '/F4B-PBTC-DECLARED-CORRECTIONS/coordinator/gate-contract.json'
t1b = json.loads(show(T1BC))
pbtc = json.loads(show(PBTCC))
P_STOPS = [dict(s) for s in t1b['StopConditions'] if re.fullmatch(r'[SP]-\d\d', s['Id'])]
assert [s['Id'] for s in P_STOPS] == ['S-02', 'S-03', 'S-04', 'S-05', 'S-07', 'S-08', 'S-12', 'S-13', 'S-14', 'P-01', 'P-02', 'P-03', 'P-04', 'P-05', 'P-06', 'P-07', 'P-08']
assert P_STOPS[9]['Text'] == 'config.toml no longer matches accepted Snapshot A, B, C or D'
P_STOPS[9]['Text'] = ('config.toml no longer matches the accepted Snapshot in force (today Snapshot I, '
                      'docs/automation/evidence/I-64-pilot/F4-ENVIRONMENT/snapshot-I/snapshot-i.json)')
assert P_STOPS == [s for s in pbtc['StopConditions'] if re.fullmatch(r'[SP]-\d\d', s['Id'])], 'las paradas estandar difieren de las de PB-TC'

UT, TR = 'tests/RackCad.UI.Tests/', 'tests/RackCad.Tests/'
W = 'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs'
WC = 'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs'
WX = 'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml'
PRODUCT = [W, WC]
NEW = [UT + 'PushBackDeclaredCorrection2RoundingTests.cs', UT + 'PushBackDeclaredCorrection2MixedBoxTests.cs']
ALLOWED = PRODUCT + NEW
assert len(ALLOWED) == 4 == len(set(ALLOWED))
assert all(os.path.isfile(p) and in_head(p) for p in PRODUCT), [p for p in PRODUCT if not (os.path.isfile(p) and in_head(p))]
assert all(not os.path.exists(p) and not in_head(p) for p in NEW), [p for p in NEW if os.path.exists(p) or in_head(p)]


def tracked(prefix):
    out = subprocess.run(['git', 'ls-tree', '-r', '--name-only', 'HEAD', '--', prefix], capture_output=True, text=True).stdout.split('\n')
    return [p for p in out if p and p.startswith(prefix)]


def cover(prefix):
    """Prefijos y archivos minimos que cubren todo prefix salvo las rutas permitidas (algoritmo de [PKC-C])."""
    inside = [a for a in ALLOWED if a.startswith(prefix)]
    if not inside:
        return [prefix]
    out = []
    names = sorted({p[len(prefix):].split('/')[0] + ('/' if '/' in p[len(prefix):] else '') for p in tracked(prefix)})
    for n in names:
        if n.endswith('/'):
            out += cover(prefix + n)
        elif prefix + n not in ALLOWED:
            out.append(prefix + n)
    return out


SRCF = cover('src/')
for a in ALLOWED:
    assert not any(a == f or (f.endswith('/') and a.startswith(f)) for f in SRCF), a
for p in tracked('src/'):
    assert p in ALLOWED or any(p == f or (f.endswith('/') and p.startswith(f)) for f in SRCF), ('sin cubrir', p)
# forma esperada del paquete (section 6.3): 3 proyectos, 9 carpetas y 34 archivos de la raiz de src/RackCad.UI, 5 sistemas y 8 hermanos (con WX)
UIR = 'src/RackCad.UI/'
assert SRCF[:3] == ['src/RackCad.Application/', 'src/RackCad.Domain/', 'src/RackCad.Plugin/'], SRCF[:3]
ui_dirs = sorted(f[len(UIR):] for f in SRCF if f.startswith(UIR) and f.endswith('/') and f[len(UIR):].count('/') == 1)
ui_files = [f for f in SRCF if f.startswith(UIR) and '/' not in f[len(UIR):]]
assert ui_dirs == ['Controls/', 'Editor/', 'Preview/', 'RackFrames/', 'Shell/', 'StructuralSections/', 'Themes/', 'Views/', 'Workspace/'], ui_dirs
assert len(ui_files) == 34, len(ui_files)
assert sorted(f for f in SRCF if f.startswith(UIR + 'Systems/') and f.endswith('/')) == sorted(
    UIR + 'Systems/' + d for d in ('Cantilever/', 'Dynamic/', 'FlowBed/', 'Larguero/', 'Selective/'))
assert sorted(f for f in SRCF if f.startswith(UIR + 'Systems/PushBack/')) == sorted(
    [UIR + 'Systems/PushBack/' + n for n in ('PushBackBootSection.cs', 'PushBackDefenseSection.cs', 'PushBackMatrixCardModel.cs', 'PushBackPreviewModel.cs',
                                             'PushBackPreviewRenderer.cs', 'PushBackRearTopeDialogAdapter.cs', 'PushBackRearTopeSection.cs')] + [WX])
assert len(SRCF) == 59, len(SRCF)

UI_FILES = (['RackCad.UI.Tests.csproj', 'StaTestRunner.cs', 'EditorWindowTestSupport.cs', 'EditorDiscardPromptCollection.cs',
             'PushBackFullEditorCharacterizationSupport.cs', 'PushBackDeclaredCorrectionSupport.cs']
            + ['PushBackDeclaredCorrection%sTests.cs' % n for n in ('Rounding', 'DimensionStyle', 'PilotParity', 'SideBNotice', 'StandardRestore')]
            + ['PushBackFullEditorCharacterization%sTests.cs' % n for n in ('State', 'Module', 'Composite', 'Subdialog', 'Catalog', 'RoundingLoss')]
            + ['PushBackClassicCharacterization%sTests.cs' % n for n in ('Keyboard', 'Close', 'Intent', 'Owner', 'Resource', 'SourceGuard')]
            + ['BlankFrontDesviadorHandoffTests.cs', 'EditorClosePolicyTests.cs', 'RichEditorCharacterizationTests.cs', 'RichEditorCloseContractTests.cs',
               'RichEditorContractTests.cs', 'PushBackShellAdoptionTests.cs', 'PushBackModuleEditorWindowTests.cs', 'PushBackSafetyPerPostFlowTests.cs',
               'PushBackSidebarCompositionTests.cs', 'UiSystemBoundaryGuardTests.cs', 'WindowCensusGuardTests.cs']
            + ['PushBackAdvancedRackParametersWindowTests.cs', 'PushBackCompositeWindowTests.cs', 'PushBackCompositeLifecycleWiringTests.cs',
               'PushBackCompositeLoadExistingModulesTests.cs', 'PushBackBothModeCellBeamOverrideTests.cs', 'PushBackA1EditorTests.cs',
               'PushBackDormantRestoreTests.cs', 'PushBackDormantTailTransactionTests.cs', 'PushBackEditorSideOwnershipTests.cs',
               'PushBackLibrarySaveGateTests.cs', 'PushBackOutputGateAfterRecomputeTests.cs', 'PushBackHeaderLineWindowTests.cs']
            + ['CantileverAdvancedPanelWindowTests.cs', 'CantileverComponentEditorTests.cs', 'CantileverEditorWindowTests.cs', 'NumericFieldTests.cs',
               'NumericFieldValidationTests.cs'])
assert len(UI_FILES) == 51 == len(set(UI_FILES)), len(UI_FILES)
assert len(tracked(UT + 'Workspace/')) == 24 and not any(a.startswith(UT + 'Workspace/') for a in ALLOWED)
TESTF = [TR] + [UT + f for f in UI_FILES] + [UT + 'Workspace/']
assert len(TESTF) == 53, len(TESTF)
ROOT = ['docs/', '.github/', 'eng/', 'tools/', 'assets/', 'deploy/', 'RackCad.sln', 'Directory.Build.props', 'Directory.Build.targets', 'global.json', 'NuGet.Config',
        'README.md', 'AGENTS.md', 'CLAUDE.md', '.editorconfig', '.gitattributes', '.gitignore']
FORBIDDEN = SRCF + TESTF + ROOT
assert len(FORBIDDEN) == 129 == len(set(FORBIDDEN)), len(FORBIDDEN)
for f in FORBIDDEN:
    assert os.path.exists(f.rstrip('/')) and in_head(f), ('no existe', f)

PKG = 'docs/initiatives/I-64-f4b-pbtc2-gate-contract-package.md'
MEAS = 'docs/initiatives/I-64-f4b-pbtc2-measurements.md'
PHASE = 'docs/initiatives/I-64-f4b-push-back-completo-package.md'
CO = PILOT + '/F4B-DRAFTS/coordinator/'
SC = PILOT + '/F4B-DRAFTS/superseding-characterization.md'
CONTRACT_REL = PILOT + '/%s/coordinator/gate-contract.json' % TASK
INV = [
    'INV-PBTC2-01 (alcance): git diff --name-only BaseSha..CurrentSha dentro de las 4 rutas de AllowedWriteScope; en src/, solo ' + W + ' (W) y ' + WC
    + ' (WC); git diff BaseSha..CurrentSha -- src/RackCad.UI/Controls src/RackCad.UI/Systems/Cantilever src/RackCad.Application src/RackCad.Domain '
      'src/RackCad.Plugin ' + WX + ' vacio.',
    'INV-PBTC2-02 (RED y GREEN): el RED es solo de pruebas (los dos archivos nuevos, sin ningun archivo de src/) y se publica antes del GREEN; el GREEN es '
    'solo W y WC, sin ningun archivo de tests/. Criterio mecanico del RED, SOLO sobre el filtro con ExpectRed y con los resultados de la CI del RED: todos '
    'los resultados con outcome Failed; un Message sin el prefijo de tipo ^(?:[A-Za-z_][\\w`]*[.+])+[A-Za-z_][\\w`]* : ; la primera linea de StackTrace '
    'en un archivo bajo tests/; ninguno NotExecuted ni Skipped; y Failed == Selected >= 4. En el RED, los filtros sin ExpectRed pasan, con los mismos '
    'Skipped que en la base. Ninguna linea anadida ni quitada de git diff -U0 BaseSha..CurrentSha -- W WC contiene SetNumber(.',
    'INV-PBTC2-03 (donde cambia, sobre el diff): cada hunk de git diff -U0 BaseSha..CurrentSha -- src cae en uno de estos sitios, por su rango de lineas '
    'en BaseSha: WC:198 (Gap_Changed, WC:184-200); W:1905 (ModuleLength_Changed, W:1892-1913); W:1639, :1641, :1642 y :1646 (ReadAdvancedRackParameters, '
    'W:1632-1647); W:818-822 (ReadCellValues, W:802-831); W:3667 (OptionalVal, W:3665-3668) y W:3641 en su documentacion; W:3683 en la documentacion '
    'de Keep; la retirada de Val (W:3634-3635); y la insercion de KeepOptional, MixedBoth y KeepCell entre Keep (W:3686) e IntVal (W:3688). Un hunk TOCA '
    'un miembro de rango [s, e] si cambia o quita una linea de ese rango, o si inserta dentro de el (en un hunk @@ -a,0 ..., cuando s <= a < e); una '
    'insercion ENTRE dos miembros es una declaracion nueva y solo se admite si esta en esta lista. Las lineas W:3665, W:3666 y W:3668 de OptionalVal '
    'quedan literalmente como en BaseSha.',
    'INV-PBTC2-04 (las cuentas, sobre CurrentSha, con <W> = ' + W + ' y <WC> = ' + WC + '): git grep -nP \'\\bVal\\(\' -- <W> vacio; '
    'git grep -nF \'cref="Val"\' -- <W> vacio; git grep -cF \'Keep(GapBox, composite.Gap)\' -- <WC> = 1; '
    'git grep -cF \'Keep(ModuleLengthBox, module.Length)\' -- <W> = 1; git grep -cP \'\\bKeepOptional\\(\' -- <W> = 6 (la declaracion, las cuatro de '
    'ReadAdvancedRackParameters y la de OptionalVal); git grep -cP \'\\bKeepCell\\(\' -- <W> = 6 (la declaracion y las cinco de ReadCellValues); '
    'git grep -cP \'\\bMixedBoth\\(\' -- <W> = 3 (la declaracion, KeepCell y OptionalVal); git grep -cP \'\\bIsMixed\\(\' -- <W> = 2 (MixedBoth y la '
    'rama vacia de OptionalVal). El comportamiento lo fijan CT-PBTC2-01 a 04 en verde.',
    'INV-PBTC2-05 (lo que sigue en verde): PB-TC (sus 10 metodos), PB-T0 (16 en UI y 3 en Core), T1B (55 y 10) y Cantilever (67); Fact1 y Fact3, G-10, '
    'I-55, el desviador, T-SG, T-KB, el censo de ventanas y el corpus de WLC; las clases de RequiredTests. W sigue sin forceRebuild: ni forceRebuild) y '
    'con una sola .RequestUpdate(.',
    'INV-PBTC2-06 (sin sustituciones): ninguna prueba existente se edita, se salta, se reactiva ni se borra.',
    'INV-PBTC2-07 (las pruebas de UI): las dos clases nuevas en EditorDiscardPromptCollection; cada ventana con Ventana() y su carga dentro del try, '
    'cerrada en finally; ninguna ventana mostrada; cada metodo corre dentro de EditorDiscardPrompt.Substitute con un contador por metodo; la ultima '
    'linea de cada ventana termina en " descartes=<n>", leido DESPUES de cerrar la ventana: tras Click(w, "UpdateButton"), que la cierra, o tras un '
    'Cerrar(w) explicito, con este orden como ultimas sentencias del try: el texto de la linea sin el sufijo, Cerrar(w); y log.Add(<texto> + " descartes=" '
    '+ descartes); en todas, n = 0; sin teclado ni foco real; prohibidos un MessageBox real, el BOM, Guardar en biblioteca, el configurador real e '
    'Insertar; ninguna asercion dentro de una costura; ningun fixture nuevo en Fixtures/I64; los unicos ayudantes nuevos son NumR, BoolR y StrR, '
    'privados de cada clase, con la navegacion de PushBackDeclaredCorrectionSupport.',
    'INV-PBTC2-08 (determinismo): el GUID fijo de Ventana(); la cultura invariante; los numeros con "R"; el orden ordinal; ni hora ni aleatoriedad.',
    'INV-PBTC2-09 (las corridas locales, F4-E59): nunca la suite completa de UI en local, solo los filtros de RequiredTests; la de Core, si; MAX_PATH: sin '
    'copias de arboles; las corridas temporales (D-PBTC2-07 del paquete) no entran en ningun commit, terminan en Assert.Fail(string.Join("\\n", lineas)) '
    'y guardan su TRX y su log como artefactos citados en la entrega; el perfil del usuario sin cambios, con la prueba de la limpieza de INV-PBT0-06.',
    'INV-PBTC2-10 (CI y compilacion): CI del GREEN 4/4 con las suites completas; CI del RED con ui-tests en failure y tests, build-ui y build-plugin en '
    'success, sin blame-hang ni timeout; los conteos de avisos por codigo, iguales a la linea base.',
    'INV-PBTC2-11 (la evidencia): toda ruta de worker-handoff.json es relativa y se comprueba que existe; TreeSha es el arbol real (git write-tree) o null.',
    'INV-PBTC2-12 (el registro): el Worker no escribe en docs/; la seccion de PB-TC2 de ' + SC + ', sus notas de cierre y su paso de DECLARADA a '
    'EJECUTADA son de la sesion.',
    'INV-PBTC2-13 (reglas de F4B, [DP] y D-PB-10): la planificacion segun PBTC2-C08; los controles segun PBTC2-C11, con las firmas literales de la '
    'section 6.7 del paquete; antes de un intento 3, siempre el Owner (PBTC2-C12).',
    'INV-PBTC2-14 (sin AutoCAD): la comprobacion fisica es de Smoke-PB.',
]
assert len(INV) == 14 and [i.split(' ')[0] for i in INV] == ['INV-PBTC2-%02d' % n for n in range(1, 15)]
EVID = [
    'Antes del Worker: gate-contract valido; delegacion del Controller valida; comparacion byte a byte, aceptacion A1-A8 y nc4 (primera delegacion de la '
    'cadena F4B-PBTC2); AllowedWriteScope de la delegacion = exactamente el del contrato; si hubo una replanificacion por PBTC2-C08, su registro.',
    'Paso 1, sobre BaseSha y antes del RED (D-PBTC2-07 del paquete): la corrida temporal con Assert.Fail(string.Join("\\n", lineas)); por fila de T2-1 a '
    'T2-6, la salida real de la base tras el gesto (LastComputation.Design) comparada con la fila a mano, y entre esa salida y la del GREEN (la carga sin '
    'gesto), resueltas con BuildFrom y w.Session.Catalog: K1, K1o, K2, K3, K4, K4\' y MaxDelta, con la formula de CT-PB0-21 y el detalle del BOM linea '
    'a linea; el TRX y el log, como artefactos. Si PBTC2-C14 se activa, STOP antes del RED; si MaxDelta >= 0.0005 en alguna fila, se declara.',
    'Commit RED y push propios antes del GREEN; CI del RED de INV-PBTC2-10; los TRX del filtro con ExpectRed cumplen el criterio mecanico de INV-PBTC2-02.',
    'Commit GREEN y push: git diff --name-only RedSha..CurrentSha = W y WC; la salida de las ordenes de INV-PBTC2-02 a 04; CI del GREEN 4/4, con los '
    'conteos de la section 6.9 del paquete (UI 2422 resultados: 0 Failed, 21 Skipped y 2401 Passed; Core 19070 Passed) o la diferencia explicada por el diff.',
    'En local: build Release de la solucion, Plugin incluido y sin avisos nuevos, sobre el RED y sobre el GREEN; los filtros de RequiredTests sobre el RED y '
    'sobre el GREEN; tres corridas seguidas en verde del filtro con ExpectRed sobre el GREEN; la prueba de la limpieza del perfil.',
    'worker-handoff.json conforme a su esquema con RED, GREEN, alcance real, trailer y conteos por filtro (Passed, Failed y Skipped); el mapa CT a metodo '
    '(section 4.1 del paquete); por cada CT, la primera diferencia en la base con el mensaje del TRX; la transcripcion del paso 1; rutas relativas '
    'comprobadas; TreeSha real o null; KnownLimitations, con la regla de Ambos lados en el alcance y en los datos del frente sin CT (section 10 del paquete).',
    'Despues: Controller verification valida; nc1, nc2 y nc3 segun README section 10, conservando cada salida ordinaria; un control que no da CONTROL PASS se '
    'resuelve SOLO como dice PBTC2-C11 (el sustituto unicamente con la firma literal exacta; si no, el Owner); cada uso se registra en '
    'F4B-DRAFTS/f4b-dispositions.md; la verificacion 14/14 sola no basta para cerrar.',
    'Sin AutoCAD: la ventana se prueba en UI.Tests con el producto real; la comprobacion fisica es de Smoke-PB.',
]
C11 = ('a negative control (nc1, nc2 or nc3) does not give CONTROL PASS: its ordinary output is kept as is and there are only two exits without returning to the Owner '
       '([DP] question 1, "Extender firmas exactas"): (1) nc2 with the EXACT signature of PD-F4-01 (source f4-dispositions.md:1103-1106: Scope in fail on the removed path; '
       'Contract in fail only by the exactness clause; EXECUTION_BLOCKED/STOP; applied literally at :1540: no check other than the real verification differs; the exactness '
       'clause includes Contract evidence citing the contrast of the narrowed AllowedWriteScope with the contract or with byte-fidelity-check.json, f4-dispositions.md:520, :562, '
       'and nothing else) -> substitute PBTC2-NC2-EXACT-WHITELIST-01, a copy of F4T1B-NC2-EXACT-WHITELIST-01 with ONLY the id and the scope changed, recorded as '
       'nc2 = SATISFIED_UNDER_PBTC2_EXACT_WHITELIST_SUBSTITUTE; (2) nc1 with the EXACT signature of PD-F4-04 (source owner-f4t3-authorization.md:255-265: false CurrentSha '
       'correctly detected; Identity = fail; Handoff also fails first only because the handoff itself contains the false SHA; outcome BLOCKED / STOP; all other intended checks '
       'match; as applied in F4, f4-dispositions.md:1657-1660, Ci in fail only as a consumer of the false CurrentSha is part of the signature, with Remote in pass) -> substitute '
       'PBTC2-NC1-IDENTITY-SIGNAL-01, a copy of F4T3C2-NC1-IDENTITY-SIGNAL-01 with ONLY the id and the scope changed, recorded as '
       'nc1 = SATISFIED_UNDER_PBTC2_NC1_IDENTITY_SIGNAL_SUBSTITUTE. Matching is decided never by check name: the control output is compared with the real verification check by '
       'check (the set of failing checks, the first failing check, Outcome and StopClass, and the evidence text); the only admitted differences are those listed for each signature '
       'in section 6.7 of the package; the session records the comparison table in f4b-dispositions.md. Any other signature or variation (PD-F4-05, PD-F4-06, nc2 with Ci or '
       'Trailer also in fail, any nc1 that does not reproduce the literal signature - including the PB-TC variation with Remote, Scope, Tests and Trailer also in fail, '
       'which the Owner accepted only once (F4B-E08) -, any failure of nc3, or an evidence text that cannot be assigned to an admitted difference) returns to the Owner '
       'before any substitute')
STOPS = P_STOPS + [
    {'Id': 'PBTC2-C01', 'Text': 'the correction would need to change a file outside W and WC (for example NumericField, the window XAML, another window, Application or the Plugin); it is an Owner decision (OB-06.6)'},
    {'Id': 'PBTC2-C02', 'Text': 'an existing test or helper would have to be edited, skipped, re-enabled or deleted, or a .csproj, InternalsVisibleTo, the packages or tests/RackCad.Tests/Fixtures/I64 would have to change; a T1B guard, the Fact3 guard or a PB-TC test that does not stay green returns to the Owner (OB-T1B.3); a needed substitution is a package defect and returns to the session with a proposed SUS2-0n'},
    {'Id': 'PBTC2-C03', 'Text': 'an unmeasured material effect appears (OB-06.4): a difference in the design, the pieces, the levels or positions, or the BOM that does not come from the exact values of the six boxes or from the both-sides equalization; or a RequiredTests test fails on the GREEN; STOP and back to the Owner'},
    {'Id': 'PBTC2-C04', 'Text': 'a method of the ExpectRed filter passes on the base, or fails by exception and not by assertion; it is a contract defect and the Worker does not change the test to make it fail'},
    {'Id': 'PBTC2-C05', 'Text': 'a CT would need AutoCAD, a real MessageBox, the real configurator, the BOM, Guardar en biblioteca, Insertar, the real keyboard or focus, or a write to the user profile or to disk'},
    {'Id': 'PBTC2-C06', 'Text': 'the full CI of the GREEN fails outside the new tests, reproduces on a second run and is not attributable to the new tests (attributable is REWORK; not reproduced is a BLOCKED rerun within the 16.8 limits)'},
    {'Id': 'PBTC2-C07', 'Text': 'the facts of section 2 of the package do not hold on the base in a way that changes a CT or the design; an expected line of a CT is not met on the GREEN; or, in step 1, the real base output after the gesture differs from the hand-built row; the Worker does not adjust the expectation'},
    {'Id': 'PBTC2-C08', 'Text': 'a planning output rejected ONLY by the literal byte-fidelity comparison of the collections before the Worker allows ONE replanning with a reinforced prompt, without consuming an attempt ([DP] question 2); a second planning fidelity rejection is STOP and returns to the Owner; any other planning rejection (A1-A8 or another cause) is STOP and returns to the Owner'},
    {'Id': 'PBTC2-C09', 'Text': 'a test would hang, leave a window, a modal or a dispatcher alive, write to real user profile data or depend on machine-specific paths'},
    {'Id': 'PBTC2-C10', 'Text': 'doing it right would require touching something outside the contracted scope: c-02 (the post outside the catalog), pb-21-cola, the other T-2 normalizations, D-PB-11, the earlier defect M-07 (an empty Largo manual with a stale mixed flag and no side B), the PB-TC boxes outside both-sides editing, or any member not listed in INV-PBTC2-03'},
    {'Id': 'PBTC2-C11', 'Text': C11},
    {'Id': 'PBTC2-C12', 'Text': 'a correction would be attempt 3 of 3: it returns to the Owner before it is issued (D-PB-10)'},
    {'Id': 'PBTC2-C13', 'Text': 'the design would need to change the tooltip of the mixed box (WC:583-584) or the presentation of a box; it is an Owner presentation decision'},
    {'Id': 'PBTC2-C14', 'Text': 'in step 1 (D-PBTC2-07 of the package) a row from T2-1 to T2-6 has K1 or K1o not empty, K2 true, K4 not empty (a coordinate displaced by more than 0.001, the OB-06.4 definition of material), a different number of BOM lines, or a BOM line that differs in anything other than a number with |delta| < 0.001: the Worker stops BEFORE the RED, delivers the transcription, and the session takes it to the Owner (OB-06.4; F4B-E09, an unmeasured material effect); a MaxDelta >= 0.0005 below that threshold is not a stop: it is declared and the session informs the Owner, because the 0.0005 given to the Owner was an estimate, not a threshold'},
]
assert [s['Id'] for s in STOPS[17:]] == ['PBTC2-C%02d' % n for n in range(1, 15)] and len(STOPS) == 31
UIP, CP = UT + 'RackCad.UI.Tests.csproj', TR + 'RackCad.Tests.csproj'
REQ = [
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrection2', 4, True),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrectionRoundingTests', 3, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrectionDimensionStyleTests', 2, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrectionPilotParityTests', 2, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrectionSideBNoticeTests', 2, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrectionStandardRestoreTests', 1, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackFullEditorCharacterization', 16, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackClassicCharacterization', 55, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.Cantilever', 67, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackModuleEditorWindowTests', 13, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackAdvancedRackParametersWindowTests', 9, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackCompositeWindowTests', 52, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackCompositeLifecycleWiringTests', 7, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackCompositeLoadExistingModulesTests', 3, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackBothModeCellBeamOverrideTests', 10, False),
    (CP, 'FullyQualifiedName~RackCad.Tests.PushBackClassicCharacterization', 10, False),
    (CP, 'FullyQualifiedName~RackCad.Tests.PushBackFullEditorCharacterization', 3, False),
    (CP, 'FullyQualifiedName~RackCad.Tests.PushBackModuleEditorCharacterizationTests', 15, False),
]
assert len(REQ) == 18 and [r for r in REQ if r[3]] == [REQ[0]]
for proj, _, _, _ in REQ:
    assert in_head(proj), proj
# las clases de PB-TC se filtran por clase: ninguna es subcadena de una clase nueva de PB-TC2
for _, f, _, e in REQ[1:6]:
    assert not any(f.split('~')[1].split('.')[-1] in n for n in ('PushBackDeclaredCorrection2RoundingTests', 'PushBackDeclaredCorrection2MixedBoxTests')), f
cells = json.loads(json.dumps(t1b['EligibleCells']))
assert [c['Cell'] for c in cells] == ['anthropic/subagent/claude-sonnet-5-5', 'openai/process/gpt-6-luna']
assert cells == pbtc['EligibleCells'], 'las celdas difieren de las de PB-TC'
UNIT_CHANGE = [
    (W, 'la ventana clasica de Push Back: Keep en la longitud del modulo, KeepOptional en las cuatro avanzadas anulables, MixedBoth y KeepCell en la celda, OptionalVal y la retirada de Val'),
    (WC, 'la parte compuesta de la ventana: Keep en el hueco (Gap_Changed)'),
]
EXISTING = [
    (PHASE, 'paquete de la fase F4B v3.1: OB-06.2 a OB-06.6 (con la definicion de material de OB-06.4, :389), OB-T1B.1 a OB-T1B.3, la fila de PB-T2 y section 9'),
    (CO + 'owner-f4b-disposition.md', 'archivo completo ([OD]): D-PB-06 (rounding behavior/state, sus seis pasos y Do not hide these corrections inside PB-T1 state extraction) y D-PB-10'),
    (CO + 'owner-f4b-procedure-decisions.md', 'archivo completo ([DP], F4B-E03): preguntas 1 y 2 (firmas exactas de los sustitutos y la replanificacion unica)'),
    (PILOT + '/F4B-DRAFTS/f4b-dispositions.md', 'archivo completo: F4B-E01 a F4B-E09'),
    (SC, 'el registro declarado ([SC]): EJECUTADA para PB-TC; la seccion de PB-TC2 en estado DECLARADA, y el cierre del Pendiente de H-31 y de la limitacion OWNER-1'),
    ('docs/initiatives/I-64-f4b-pbtc-gate-contract-package.md', 'el paquete de PB-TC v2.2 ([PKC]): el patron, la regla Keep (D-PBTC-01), D-PBTC-10 y PBTC-C14, y las firmas de PBTC-C11'),
    (PBTCC, 'el contrato de PB-TC ([PKC-C]): el metodo del ForbiddenWriteScope, las paradas estandar con P-01 y las celdas'),
    (PILOT + '/F4B-PBTC-DECLARED-CORRECTIONS/task-close-package.md', 'el cierre de PB-TC: section 3 (OWNER-1 y nc1) y section 4 (las lecciones M-02 a M-05)'),
    (PILOT + '/F4B-DRAFTS/pbtc/package-v2.md', 'el anexo A de la v2 de PB-TC (:1217-1302): el diseno de H-31 ya revisado'),
    (PILOT + '/F4B-DRAFTS/pbtc/independent-review-v2-check.md', 'ICT2: I-03, N-07 y N-08'),
    (CO + 'owner-pbtc-mixed-decision.md', 'OWNER-1 registrado y arreglado despues (F4B-E08); lo resuelve F4B-E09'),
    (CO + 'owner-pbtc-nc1-decision.md', 'la disposicion unica de nc1 en PB-TC (F4B-E08), que no se extiende a PB-TC2'),
    ('docs/initiatives/I-64-f4-t1b-gate-contract-package.md', 'el patron de T1B ([T1B]): la caracterizacion vigente de la ventana clasica'),
    (T1BC, 'el contrato de T1B ([T1B-C]): las paradas estandar y las celdas'),
    (PILOT + '/F4-DRAFTS/f4-dispositions.md', 'las fuentes de PD-F4-01 (:1103-1106, :1540, :520, :562) y PD-F4-04 (:1657-1660), y F4-E59'),
    (PILOT + '/F4-DRAFTS/coordinator/owner-f4t3-authorization.md', 'la firma literal de PD-F4-04 (:255-265)'),
    ('docs/automation/state/I-64.yml', 'estado vigente completo'),
    ('docs/automation/evidence/I-64-evidence.md', 'las secciones de F4B (231 y siguientes)'),
]
EXTERNAL = [
    ('docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md', 'D13 (la caracterizacion no se edita para que pase; un cambio de comportamiento exige decision del Owner)'),
    ('docs/AUTOMATION_PLAN.md', 'section 16.5, 16.8, 16.9 y 16.11'),
    ('docs/automation/agent-execution/README.md', 'section 4 (A1-A8) y section 10 (nc1..nc4)'),
    ('docs/automation/agent-execution/routing.md', 'archivo completo'),
    ('docs/automation/agent-execution/model-catalog.md', 'celdas medidas y vigentes para Controller y Worker'),
]
for p, _ in EXISTING + EXTERNAL + UNIT_CHANGE:
    assert in_head(p), ('autoridad inexistente en HEAD', p)
c = {
    'Schema': 'rackcad-gate-contract/v1', 'Unit': 'I-64', 'Gate': 'F4B', 'TaskId': TASK,
    'Objective': ('Corregir en la ventana clasica de Push Back, como cambios declarados de RACKEDITAR por D-PB-06 y la decision del Owner F4B-E09, las '
                  'cajas que aun redondean (H-31: GapBox, ModuleLengthBox y las cuatro cajas avanzadas anulables, con la regla Keep de PB-TC y '
                  'KeepOptional) y la caja mixta de \'Ambos lados\' con el lado B presente (OWNER-1: una caja mixta que recibe un valor lo aplica a los '
                  'dos lados), segun el paquete del contrato de PB-TC2 v2 y la lista cerrada CT-PBTC2-01..04. El RED son pruebas nuevas que fallan por '
                  'asercion en la base, sin sustituciones; el GREEN corrige solo W y WC. NumericField, Cantilever, Application, el Plugin y el XAML no '
                  'cambian; c-02 y pb-21-cola quedan para PB-T1; el modo Workspace queda para PB-T2; no se ejecuta AutoCAD.'),
    'AuthorityRevision': AR, 'MainSha': MAIN,
    'Authorities': [
        {'Path': PKG, 'Section': 'archivo completo (v2): section 2 (hechos medidos), section 3 (D-PBTC2-01 a D-PBTC2-08), section 4 (CT-PBTC2-01 a 04 y el mapa CT a metodo), section 5 (RED, sin sustituciones, y el registro), section 6 (el contrato), section 8 (decisiones del Owner) y section 10 (arrastres)', 'Class': 'UNIT_DOC'},
        {'Path': MEAS, 'Section': 'el registro de mediciones del paquete, apartados A a I', 'Class': 'UNIT_DOC'},
        {'Path': CO + 'owner-pbt1-pre-decisions.md', 'Section': 'archivo completo ([OPT1], F4B-E09): pregunta 1 (H-31 y OWNER-1 en PB-TC2), preguntas 2 y 3 (c-02 y pb-21-cola a PB-T1) y pregunta 4 (la autorizacion condicional de emision)', 'Class': 'UNIT_DOC'},
        {'Path': PILOT + '/F4B-DRAFTS/pbt1/ob065-dossier.md', 'Section': 'el expediente del analista: section 2.1 (H-31), section 2.2 (OWNER-1) y section 5 (la PB-TC2 propuesta)', 'Class': 'UNIT_DOC'},
    ] + [{'Path': p, 'Section': s, 'Class': 'UNIT_DOC'} for p, s in EXISTING] + [
        {'Path': CONTRACT_REL, 'Section': 'este contrato', 'Class': 'UNIT_DOC'},
    ] + [{'Path': p, 'Section': s, 'Class': 'UNIT_CHANGE'} for p, s in UNIT_CHANGE]
      + [{'Path': p, 'Section': s, 'Class': 'EXTERNAL'} for p, s in EXTERNAL],
    'AllowedWriteScope': ALLOWED,
    'ForbiddenWriteScope': FORBIDDEN,
    'Invariants': INV,
    'RequiredTests': [{'Project': p, 'Filter': f, 'MinSelected': m, 'ExpectRed': e} for p, f, m, e in REQ],
    'ExpectedEvidence': EVID, 'StopConditions': STOPS, 'EligibleCells': cells, 'RoutingEnforcement': 'required', 'CorrectionsAuthorized': True,
    'IssuedBy': ('Sesion principal de I-64 (Claude) por las decisiones del Owner F4B-E09 con su autorizacion condicional de emision (pregunta 4) '
                 '(docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-pbt1-pre-decisions.md), las decisiones de procedimiento '
                 'owner-f4b-procedure-decisions.md (F4B-E03) y el paquete del contrato v2 con su revision independiente IRT2-1 y la comprobacion '
                 'acotada de la v2'),
    'IssuedUtc': ISSUED,
}
assert len(c['Authorities']) == 30, len(c['Authorities'])
b = (json.dumps(c, indent=2, ensure_ascii=True) + '\n').encode('ascii')
assert all(x < 0x80 for x in b) and b'\\u' not in b, 'no ASCII'
assert not set(c['AllowedWriteScope']) & set(c['ForbiddenWriteScope'])
for a in c['AllowedWriteScope']:
    assert not any(a.startswith(f) for f in c['ForbiddenWriteScope'] if f.endswith('/')), a
TERMS = ['GATE PASS', 'GATE_PASS', 'Candidato', 'Candidate', 'FINAL_CANDIDATE_SHA', 'gate cerrado', 'gate closed', 'cierre del gate', 'rama integrada', 'integrada en main',
         'integrated into main', 'merged into main', 'lista para integrar', 'ready to merge', 'Owner Validation APROBADA', 'Owner Validation APPROVED']
for t in TERMS:
    assert not re.search(r'(?<![A-Za-z0-9_])' + re.escape(t) + r'(?![A-Za-z0-9_])', b.decode('ascii'), re.I), t
for bad in ('C:' + chr(92), 'C:' + '/', 'D:' + '/', 'Users', 'alej' + 'andra', 'App' + 'Data', 'scratchpad', '.co' + 'dex', 'hs' + '65'):
    assert bad not in b.decode('ascii'), bad
if '--dry' in sys.argv:
    print('DRY allowed', len(ALLOWED), 'forbidden', len(FORBIDDEN), '(src', len(SRCF), 'tests', len(TESTF), 'root', len(ROOT), ') inv', len(INV), 'stops', len(STOPS),
          'required', len(REQ), 'evidence', len(EVID), 'authorities', len(c['Authorities']), 'bytes', len(b))
    if '--print' in sys.argv:
        for i, f in enumerate(FORBIDDEN, 1):
            print(i, f)
    sys.exit(0)
os.makedirs(os.path.dirname(OUT), exist_ok=True)
open(OUT, 'wb').write(b)
o = subprocess.run(['pwsh', '-NoProfile', '-Command', f"Get-Content -Raw '{OUT}' | Test-Json -SchemaFile 'docs/automation/agent-execution/schemas/gate-contract.schema.json'"], capture_output=True, text=True)
assert o.stdout.strip().endswith('True'), o.stderr[-300:]
print('schema True allowed', len(ALLOWED), 'forbidden', len(FORBIDDEN), 'inv', len(INV), 'stops', len(STOPS), 'authorities', len(c['Authorities']), 'sha256', hashlib.sha256(b).hexdigest().upper())
