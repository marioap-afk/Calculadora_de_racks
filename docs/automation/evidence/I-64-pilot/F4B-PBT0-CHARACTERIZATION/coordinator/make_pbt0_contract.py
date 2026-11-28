"""Contrato de gate de F4B-PBT0-CHARACTERIZATION (ASCII), desde el paquete del contrato de PB-T0 v2.1
(docs/initiatives/I-64-f4b-pbt0-gate-contract-package.md, section 6) y el patron de make_t1b_contract.py.
El ForbiddenWriteScope de src/ sale del algoritmo de [T1B-C] (M-06): por cada entrada de src/ en HEAD se emite <dir>/ solo si
ninguna ruta permitida queda debajo; si queda alguna, se desciende; los archivos se emiten uno a uno.
Uso: python make_pbt0_contract.py <IssuedUtc> <AuthorityRevision> <MainSha> [--dry]"""
import json, os, re, sys, subprocess, hashlib
WT = '<worktree-de-I-64>'  # Redaccion para el repo publico: la ruta local del worktree
os.chdir(WT)
ISSUED, AR, MAIN = sys.argv[1], sys.argv[2], sys.argv[3]
TASK = 'F4B-PBT0-CHARACTERIZATION'
OUT = 'artifacts/orchestration/I-64/%s/coordinator/gate-contract.json' % TASK
PILOT = 'docs/automation/evidence/I-64-pilot'
T1BC = PILOT + '/F4-T1B-PILOT-CHARACTERIZATION/coordinator/gate-contract.json'
t1b = json.loads(subprocess.run(['git', 'show', 'HEAD:' + T1BC], capture_output=True).stdout.decode('ascii'))
P_STOPS = [dict(s) for s in t1b['StopConditions'] if s['Id'][0] in 'SP']
assert [s['Id'] for s in P_STOPS] == ['S-02', 'S-03', 'S-04', 'S-05', 'S-07', 'S-08', 'S-12', 'S-13', 'S-14', 'P-01', 'P-02', 'P-03', 'P-04', 'P-05', 'P-06', 'P-07', 'P-08']
assert P_STOPS[9]['Text'] == 'config.toml no longer matches accepted Snapshot A, B, C or D'
P_STOPS[9]['Text'] = ('config.toml no longer matches the accepted Snapshot in force (today Snapshot I, '
                      'docs/automation/evidence/I-64-pilot/F4-ENVIRONMENT/snapshot-I/snapshot-i.json)')
UT, TR = 'tests/RackCad.UI.Tests/', 'tests/RackCad.Tests/'
NEW = ([UT + 'PushBackFullEditorCharacterization%sTests.cs' % n for n in ('State', 'Module', 'Composite', 'Subdialog', 'Catalog', 'RoundingLoss')]
       + [UT + 'PushBackFullEditorCharacterizationSupport.cs']
       + [TR + 'PushBackFullEditorCharacterization%sTests.cs' % n for n in ('LateralCut', 'DimensionStyleLoss', 'RoundingEffect')])
W = 'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs'
WC = 'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs'
CS = 'src/RackCad.Application/Systems/PushBack/PushBackCompositeEditorState.cs'
DA = 'src/RackCad.Application/Systems/PushBack/PushBackEditorDesignAssembler.cs'
RVA = 'src/RackCad.Application/Systems/Shared/RackViewAvailability.cs'
DVD = 'src/RackCad.Application/Systems/Dynamic/DynamicViewDecorations.cs'
MUTSCOPE = [W, WC, CS, DA, RVA, DVD]
assert len(NEW) == 10 and len(MUTSCOPE) == 6
assert all(os.path.isfile(p) for p in MUTSCOPE), [p for p in MUTSCOPE if not os.path.isfile(p)]
assert all(not os.path.exists(p) for p in NEW), [p for p in NEW if os.path.exists(p)]
ALLOWED = NEW + MUTSCOPE


def tracked(prefix):
    out = subprocess.run(['git', 'ls-tree', '-r', '--name-only', 'HEAD', '--', prefix], capture_output=True, text=True).stdout.split('\n')
    return [p for p in out if p and p.startswith(prefix)]


def cover(prefix):
    """Prefijos y archivos minimos que cubren todo prefix salvo los archivos de MUTSCOPE (algoritmo de [T1B-C], M-06)."""
    inside = [m for m in MUTSCOPE if m.startswith(prefix)]
    if not inside:
        return [prefix]
    out = []
    names = sorted({p[len(prefix):].split('/')[0] + ('/' if '/' in p[len(prefix):] else '') for p in tracked(prefix)})
    for n in names:
        if n.endswith('/'):
            out += cover(prefix + n)
        elif prefix + n not in MUTSCOPE:
            out.append(prefix + n)
    return out


SRCF = cover('src/')
for m in MUTSCOPE:
    assert not any(m == f or (f.endswith('/') and m.startswith(f)) for f in SRCF), m
for p in tracked('src/'):
    assert p in MUTSCOPE or any(p == f or (f.endswith('/') and p.startswith(f)) for f in SRCF), ('sin cubrir', p)
T1B_CLASSES = [UT + 'PushBackClassicCharacterization%sTests.cs' % n for n in ('Keyboard', 'Close', 'Intent', 'Owner', 'Resource', 'SourceGuard')] + [TR + 'PushBackClassicCharacterizationPluginGuardTests.cs']
T1B_HELPERS = [h for h in t1b['ForbiddenWriteScope'] if h.startswith('tests/')]
assert T1B_HELPERS == [UT + f for f in ('StaTestRunner.cs', 'EditorWindowTestSupport.cs', 'EditorDiscardPromptCollection.cs', 'EditorClosePolicyTests.cs',
                                        'RichEditorCharacterizationTests.cs', 'RichEditorCloseContractTests.cs', 'RichEditorContractTests.cs', 'PushBackShellAdoptionTests.cs',
                                        'RackCad.UI.Tests.csproj')] + [TR + f for f in ('PushBackPluginSourceGuardTests.cs', 'PushBackRoundTripSourceGuardTests.cs',
                                                                                        'CallerOwnedFacadeGuardTests.cs', 'RackCad.Tests.csproj', 'Fixtures/')], T1B_HELPERS
TESTF = []
for t in ([TR + 'Fixtures/', TR + 'Workspace/', TR + 'StructuralAnalysis/', UT + 'Workspace/', TR + 'RackCad.Tests.csproj', UT + 'RackCad.UI.Tests.csproj',
           UT + 'StaTestRunner.cs', UT + 'EditorWindowTestSupport.cs', UT + 'EditorDiscardPromptCollection.cs'] + T1B_CLASSES
          + [TR + f for f in ('NamespaceFolderGuardTests.cs', 'PushBackDimensionLegacyCharacterizationTests.cs', 'BlankFrontBoundaryTests.cs', 'RackModuleEditSessionTests.cs',
                              'I55DeferredProductFixCharacterizationTests.cs', 'PushBackModuleEditorCharacterizationTests.cs', 'ProductPolicyIndependenceGuardTests.cs')]
          + [UT + f for f in ('PushBackModuleEditorWindowTests.cs', 'PushBackDimensionViewsWindowTests.cs', 'BlankFrontDesviadorHandoffTests.cs', 'PushBackSafetyPerPostFlowTests.cs')]
          + T1B_HELPERS):
    if t not in TESTF:
        TESTF.append(t)
ROOT = ['docs/', '.github/', 'eng/', 'tools/', 'assets/', 'deploy/', 'RackCad.sln', 'Directory.Build.props', 'Directory.Build.targets', 'global.json', 'NuGet.Config',
        'README.md', 'AGENTS.md', 'CLAUDE.md', '.editorconfig', '.gitattributes', '.gitignore']
FORBIDDEN = SRCF + TESTF + ROOT
assert len(FORBIDDEN) == len(set(FORBIDDEN))
for f in FORBIDDEN:
    assert os.path.exists(f.rstrip('/')), f
# forma esperada del paquete (section 6.3)
assert 'src/RackCad.Domain/' in SRCF and 'src/RackCad.Plugin/' in SRCF
assert sorted(f for f in SRCF if f.startswith('src/RackCad.UI/Systems/PushBack/')) == sorted(
    'src/RackCad.UI/Systems/PushBack/' + n for n in ('PushBackBootSection.cs', 'PushBackDefenseSection.cs', 'PushBackMatrixCardModel.cs', 'PushBackPreviewModel.cs',
                                                    'PushBackPreviewRenderer.cs', 'PushBackRearTopeDialogAdapter.cs', 'PushBackRearTopeSection.cs', 'RackPushBackSystemWindow.xaml'))
for d in ('Cantilever/', 'FlowBed/', 'Larguero/', 'Selective/'):
    assert 'src/RackCad.Application/Systems/' + d in SRCF, d
assert 'src/RackCad.Application/Systems/PushBack/Pilot/' in SRCF

PKG = 'docs/initiatives/I-64-f4b-pbt0-gate-contract-package.md'
PHASE = 'docs/initiatives/I-64-f4b-push-back-completo-package.md'
MU = [
    ('MU-PB0-01', 'W:315', 'antes de LoadFromModel(inputs, rackName); de LoadExisting: if (design.IsComposite) inputs.PalletTolerance += 1.0;', 'CT-01'),
    ('MU-PB0-02', 'W:3365', 'antes de session.Identity.SetName(NameBox.Text?.Trim()); de RequestDraw: lastComputation.Design.Structure.PalletTolerance += 1.0;', 'CT-02'),
    ('MU-PB0-03', 'W:2463', 'se quita RequestRecompute(); de ConfirmModule_Click', 'CT-03'),
    ('MU-PB0-04', 'DA:211-213', 'mustRebuild sin el termino standardRestore ||', 'CT-04'),
    ('MU-PB0-05', 'DA:300', 'if (standardRestore) -> if (standardRestore && editorInputs == null); alternativa: W:2499, antes de RequestStandardRestore();, advanced.DerivedPostHeight = null;', 'CT-05'),
    ('MU-PB0-06', 'WC:839-840', 'el primer argumento de ParkDormantTail -> structure.Modules.Take(0)', 'CT-06'),
    ('MU-PB0-07', 'CS:320-323', 'se quita la rama else if (!present && DefaultTopology == PushBackCellTopology.Encontradas) { ... }; alternativa: WC:119, se quita RequestRecompute(); de SideBPresent_Changed (si se usa, R-02 se vuelve a estimar y se registra, N-08)', 'CT-07'),
    ('MU-PB0-08', 'W:1138', 'composite.SetSlotCount(state.Structure.Count + 1) -> composite.SetSlotCount(state.Structure.Count + 2)', 'CT-08 y CT-09'),
    ('MU-PB0-09', 'RackViewAvailability.cs:111', 'return posts.Contains(address.Variant.Index); -> return posts.Contains(address.Variant.Index) || address.Variant.Index >= 0;', 'CT-10'),
    ('MU-PB0-10', 'W:3365', 'SetName(NameBox.Text?.Trim()) -> SetName(NameBox.Text)', 'CT-11'),
    ('MU-PB0-11', 'W:1946', 'StageHeaderConfiguration(module, edited); -> StageHeaderConfiguration(module, copy);', 'CT-12'),
    ('MU-PB0-12', 'W:2626', 'Authorize(chosen) -> Authorize(safetySelections)', 'CT-13'),
    ('MU-PB0-13', 'W:2642', 'se quita ApplyBootSections(bootSectionA, bootSectionB);', 'CT-14'),
    ('MU-PB0-14', 'W:2638', 'se quita ApplyDefenseSections(defenseSectionA, defenseSectionB, defenseCarrierId);', 'CT-15'),
    ('MU-PB0-15', 'W:2653', 'se quita state.LoadRearTopeConfig(topeSection.Config); y queda else { }', 'CT-16'),
    ('MU-PB0-16', 'W:407', 'Math.Min((int)DimensionDetail.Detailed, ...) -> Math.Min((int)DimensionDetail.Standard, ...); alternativa: W:387, ? "kg" : -> ? "lb" :', 'CT-17'),
    ('MU-PB0-17', 'DynamicViewDecorations.cs:402', 'DimensionStyleName = style -> DimensionStyleName = style?.ToLowerInvariant()', 'CT-18'),
    ('MU-PB0-18', 'W:406', 'AnnotationScaleBox.SetNumber(...) con un segundo argumento "0.######"', 'CT-19'),
    ('MU-PB0-19', 'W:819', 'generalPallet.Height (el tercer argumento) -> Math.Round(generalPallet.Height, 3)', 'CT-20'),
    ('MU-PB0-20', 'DynamicViewDecorations.cs:329', '? system.AnnotationScale : -> ? Math.Round(system.AnnotationScale, 3) :; sin alternativa (M-02)', 'CT-21'),
]
assert len(MU) == 20
MUTEXT = ('INV-PBT0-14 (tabla CERRADA de mutantes, section 5 del paquete; W = ' + W + ', WC = ' + WC + ', CS = ' + CS + ', DA = ' + DA + '; '
          'MutantScope = esos cuatro mas ' + RVA + ' y ' + DVD + ', todos con mutantes de comportamiento): cada mutante con su archivo:linea medido en la base, '
          'su cambio exacto y el metodo del CT que tiene que fallar CON ESE MUTANTE SOLO aplicado sobre BaseSha; "alternativa" es la unica admitida y solo si la '
          'principal no discrimina o hace fallar un metodo nuevo por excepcion: '
          + '; '.join('%s [%s] %s -> falla el metodo de %s' % m for m in MU)
          + '. Un mutante fuera de esta tabla o de MutantScope es STOP (PBT0-C04); un mutante que no discrimina su CT en aislado, o que hace fallar un metodo nuevo '
          'por excepcion sin alternativa valida, es defecto del contrato y STOP (PBT0-C10); el Worker no inventa mutantes. Reglas: ningun mutante toca una asignacion '
          'de Owner, un ShowDialog, un MessageBox ni EditorDiscardPrompt; ni la comprobacion de nulo de una costura (W:1938, :2615, :2722, :2849, :2981, :3018); ni '
          'las guardas de RequestDraw (W:3325-3363) ni el cuerpo del constructor; no anade .RequestUpdate( ni literales de T-SG; no provoca una escritura; no deja '
          'un modal abierto. La conjuncion: con todos aplicados, todos los metodos nuevos fallan por asercion y ningun mutante anula a otro.')
INV = [
    'INV-PBT0-01 (sin cambio de producto, anclado a ChainBaseSha): git diff ChainBaseSha..CurrentSha -- src vacio; git diff --name-only ChainBaseSha..CurrentSha = exactamente los diez archivos nuevos de prueba; git diff --name-only BaseSha..CurrentSha dentro de las 16 rutas de AllowedWriteScope; para cada archivo de MutantScope que el RED toco, los bytes en CurrentSha son identicos a los de ChainBaseSha (cualquier residuo es FAIL).',
    'INV-PBT0-02 (RED por mutantes revertidos): el commit RED lleva el mensaje "RED con mutantes deliberados, revertidos en el GREEN" y ese SHA no sirve para ningun smoke ni para el bloque M. Criterio mecanico, SOLO sobre los resultados de los dos filtros nuevos (I-01) y con todos los mutantes a la vez: todos los resultados con outcome Failed; un Message sin el prefijo de tipo ^(?:[A-Za-z_][\\w`]*[.+])+[A-Za-z_][\\w`]* : ; la primera linea de StackTrace en un archivo bajo tests/; ninguno NotExecuted ni Skipped; y Failed == Selected >= MinSelected contando los resultados cuyo FullyQualifiedName coincide con el filtro. Las pruebas existentes rotas se admiten solo por los mutantes, se miden como union en la CI del RED y se atribuyen por analisis declarado (T-8); para ellas solo cuentan el cuelgue y el timeout. El GREEN es el inverso exacto de los mutantes y no toca tests/. Para el Controller: las escrituras del RED en MutantScope revertidas por el GREEN no son escritura fuera de alcance, y las filas de la tabla son conformes por construccion.',
    'INV-PBT0-03 (se fija lo que hay): ninguna prueba existente se edita, se reactiva ni se borra; sin Skip; lo contrario a una regla se comenta "INCUMPLIMIENTO CARACTERIZADO" con la regla citada; "No product cleanup in PB-T0".',
    'INV-PBT0-04 (medido, no inferido, M-21): todo hecho "lectura" de MA o MB se mide antes de fijarlo; las clases distintas de (a) y (b) llevan su archivo:linea medido; una clase nueva se fija tal cual y se enumera en T-2, sin STOP (OB-06.5); el codigo de medicion es temporal y no entra en ningun commit; no se caracteriza ningun gesto de teclado ni el foco real (section 4.1 del paquete).',
    'INV-PBT0-05 (el aviso): todas las clases de UI van en EditorDiscardPromptCollection; nunca un MessageBox real.',
    'INV-PBT0-06 (ninguna escritura nueva, M-12): texto de [P]:598: "ninguna prueba ejecuta Guardar en biblioteca, el BOM, el configurador real ni un MessageBox real"; lectura declarada (D-PBT0-06): "el BOM" es el gesto, la ventana y la exportacion, y leer PushBackEditorComputation.Bom no es ejecutarlo. Por tanto ninguna prueba ejecuta SaveLibrary_Click, PromptSaveToLibrary, RackProjectStore.Save, Bom_Click, RackBomWindow, InsertBatch_Click, InsertViewAt ni "Insertar"; ninguna instancia RackFrameConfiguratorWindow ni asigna HeaderConfiguratorPresenter ni SafetyWindowDialog; ConfigureModuleHeaderButton solo se pulsa con HeaderConfiguratorDialog asignado. Prueba de la limpieza: el SHA-256 y la fecha del archivo de disposicion del configurador (en los datos locales del usuario) y de la carpeta de biblioteca (en los datos de aplicacion del usuario), antes y despues de las corridas locales, no cambian.',
    'INV-PBT0-07 (las ventanas): sin mostrar, salvo CT-14; sin esperas fijas; todas se cierran en finally; nada queda vivo; ninguna asercion dentro de una costura.',
    'INV-PBT0-08 (solo ejecucion): sin guardas de texto.',
    'INV-PBT0-09 (textos exactos): copiados del producto byte a byte.',
    'INV-PBT0-10 (sin infraestructura nueva): sin costuras nuevas, sin cambios en InternalsVisibleTo, sin paquetes, sin P/Invoke y sin rutas propias de una maquina; solo las costuras existentes de H-03 del paquete (incluidas las de las secciones y las de SafetyPerPostWindow, I-04) y los ayudantes de H-18; T-7 da cada una con su uso.',
    'INV-PBT0-11 (determinismo, M-11): toda ventana se crea con Ventana() del paquete (section 4.1), con el GUID fijo de newIdFactory; cultura invariante fijada y restaurada; orden ordinal; sin hora ni aleatoriedad; en Core, sin coma flotante fijada, con los umbrales de magnitud declarados (0.001 y 1e-9).',
    'INV-PBT0-12 (CI y compilacion): CI del GREEN 4/4 en success con las suites completas; CI del RED con tests y ui-tests en failure, build-ui en success y build-plugin omitido por su needs, sin blame-hang ni timeout; sin avisos nuevos de las clases CS, MSB, NU y NETSDK, tampoco con los mutantes (sin CS0162, CS0168, CS0169, CS0219, CS0414 ni CS8321).',
    'INV-PBT0-13 (correcciones, section 5 del paquete): toda correccion vuelve a aplicar todos los mutantes de los filtros con ExpectRed; Core pasa a ExpectRed false solo si no se vuelve a aplicar ningun mutante de Application, y se declara; las comprobaciones de producto se anclan a ChainBaseSha.',
    MUTEXT,
    'INV-PBT0-15 (prueba por mutante): el Worker aplica cada mutante de la tabla SOLO sobre BaseSha y corre los dos filtros, siempre, y ademas la suite completa de Core si el mutante es de Application (MU-PB0-04, 05, 07, 09, 17 y 20); registra por mutante el archivo:linea medido en la base y el parche custodiado, las pruebas que fallan y si las nuevas fallan por asercion, y la fila que instancia.',
    'INV-PBT0-16 (las clases de escritura): PB-T0 solo ejerce la clase (B), la intencion que la ventana expone y otro consume despues; las clases (A) y (C) las sigue fijando F4-T1B.',
    'INV-PBT0-17 (el corpus): ningun archivo nuevo de fixtures; tests/RackCad.Tests/Fixtures/I64 en solo lectura; toda construccion en codigo va declarada.',
    'INV-PBT0-18 (las perdidas, en sus clases, D-PBT0-03): solo CT-18 a CT-20 aseveran (a) o (b); CT-05 fija solo los seis avanzados (M-07).',
    'INV-PBT0-19 (no duplicacion): section 4.2 del paquete y la tabla T-5.',
    'INV-PBT0-20 (contencion de las corridas locales, F4-E59 y [T1B-X] section 6): nunca la suite completa de UI en local, solo los filtros; la de Core, si; ninguna sonda temporal con entradas validas en un camino con dialogo o escritura ("Actualizar" no es uno de ellos, H-08); MAX_PATH: sin copias de arboles, cada mutante con git apply y git restore en el mismo worktree; las corridas que muestran ventanas, sin uso interactivo del equipo; OD-6: el bloque M no se opera mientras el Worker corre en local los filtros de UI, y el Worker no los corre mientras haya una medicion del bloque M en curso.',
    'INV-PBT0-21 (guardas, H-19): un solo namespace igual a la raiz; INV-14 y los literales partidos en Core; el corpus de WLC.',
    'INV-PBT0-22 (las tablas del informe): T-1 a T-8 van en la entrega; "material" es la formula mecanica de CT-PB0-21; el Worker no juzga la aceptabilidad.',
    'INV-PBT0-23 (reglas de F4B, [P] section 7.2 con [DP] y D-PB-10): la planificacion segun PBT0-C08; los controles segun PBT0-C11, con las firmas literales de la section 6.7 del paquete; antes de un intento 3, siempre al Owner (PBT0-C12).',
    'INV-PBT0-24 (sin AutoCAD): el bloque M es aparte (el anexo pbt0-block-m-procedure.md), no consume intentos de este bloque y respeta OD-6.',
]
assert len(INV) == 24 and [i.split(' ')[0] for i in INV] == ['INV-PBT0-%02d' % n for n in range(1, 25)]
EVID = [
    'Antes del Worker: gate-contract valido; delegacion del Controller valida; comparacion byte a byte, aceptacion A1-A8 y nc4 (primera delegacion de la cadena F4B-PBT0); AllowedWriteScope de la delegacion = exactamente el del contrato; si hubo una replanificacion por PBT0-C08, su registro.',
    'Commit RED y push propios antes del GREEN, con el mensaje de INV-PBT0-02; CI del RED de INV-PBT0-12; los TRX de los dos filtros cumplen el criterio mecanico de INV-PBT0-02; la union de existentes rotas con su atribucion en T-8.',
    'La prueba por mutante de INV-PBT0-15: una corrida local aislada por mutante de la tabla cerrada, con las pruebas que fallan, si las nuevas fallan por asercion y la fila que instancia; la suite completa de Core por cada mutante de Application.',
    'Commit GREEN, inverso exacto de los mutantes, y push: git diff ChainBaseSha..CurrentSha -- src vacio y --name-only igual a los diez archivos; la tabla de SHA-256 por archivo de MutantScope tocado (ChainBaseSha, RedSha distinto, CurrentSha igual al de ChainBaseSha); CI del GREEN 4/4 en success.',
    'En local: build Release de la solucion, Plugin incluido y sin avisos nuevos, sobre el RED (con los mutantes) y sobre el GREEN; los dos filtros sobre el RED y el GREEN; tres corridas seguidas en verde del filtro de UI, sin uso interactivo del equipo; la prueba de la limpieza de INV-PBT0-06.',
    'worker-handoff.json conforme a su esquema con RED, GREEN, alcance real, trailer, conteos, la tabla de mutantes, el mapa CT a metodo, las tablas T-1 a T-8 y KnownLimitations.',
    'Despues: Controller verification valida; nc1, nc2 y nc3 segun README section 10, conservando cada salida ordinaria; un control que no da CONTROL PASS se resuelve SOLO como dice PBT0-C11 (el sustituto unicamente con la firma literal exacta; si no, el Owner); cada uso se registra en F4B-DRAFTS/f4b-dispositions.md; la verificacion 14/14 sola no basta para cerrar.',
    'Sin AutoCAD: la ventana y Application se caracterizan en UI.Tests y RackCad.Tests con el producto real; el bloque M (medicion fisica) va aparte, segun el anexo.',
]
C11 = ('a negative control (nc1, nc2 or nc3) does not give CONTROL PASS: its ordinary output is kept as is and there are only two exits without returning to the Owner '
       '([DP] question 1, "Extender firmas exactas"): (1) nc2 with the EXACT signature of PD-F4-01 (source f4-dispositions.md:1103-1106: Scope in fail on the removed path; '
       'Contract in fail only by the exactness clause; EXECUTION_BLOCKED/STOP; applied literally at :1540: no check other than the real verification differs; the exactness '
       'clause includes Contract evidence citing the contrast of the narrowed AllowedWriteScope with the contract or with byte-fidelity-check.json, f4-dispositions.md:520, :562, '
       'and nothing else) -> substitute PBT0-NC2-EXACT-WHITELIST-01, a copy of F4T1B-NC2-EXACT-WHITELIST-01 with ONLY the id and the scope changed, recorded as '
       'nc2 = SATISFIED_UNDER_PBT0_EXACT_WHITELIST_SUBSTITUTE; (2) nc1 with the EXACT signature of PD-F4-04 (source owner-f4t3-authorization.md:255-265: false CurrentSha '
       'correctly detected; Identity = fail; Handoff also fails first only because the handoff itself contains the false SHA; outcome BLOCKED / STOP; all other intended checks '
       'match; as applied in F4, f4-dispositions.md:1657-1660, Ci in fail only as a consumer of the false CurrentSha is part of the signature, with Remote in pass) -> substitute '
       'PBT0-NC1-IDENTITY-SIGNAL-01, a copy of F4T3C2-NC1-IDENTITY-SIGNAL-01 with ONLY the id and the scope changed, recorded as '
       'nc1 = SATISFIED_UNDER_PBT0_NC1_IDENTITY_SIGNAL_SUBSTITUTE. Matching is decided never by check name: the control output is compared with the real verification check by '
       'check (the set of failing checks, the first failing check, Outcome and StopClass, and the evidence text); the only admitted differences are those listed for each signature '
       'in section 6.7 of the package; the session records the comparison table in f4b-dispositions.md. Any other signature or variation (PD-F4-05, PD-F4-06, nc2 with Ci or '
       'Trailer also in fail, any nc1 that does not reproduce the literal signature, any failure of nc3, or an evidence text that cannot be assigned to an admitted difference) '
       'returns to the Owner before any substitute')
STOPS = P_STOPS + [
    {'Id': 'PBT0-C01', 'Text': 'a characterization point (CT) would need a product change or a new seam beyond the reverted RED mutants of the closed table'},
    {'Id': 'PBT0-C02', 'Text': 'an existing test or helper would have to be edited, re-enabled or deleted; a .csproj, InternalsVisibleTo or the packages would have to change; or something would have to be added under tests/RackCad.Tests/Fixtures/'},
    {'Id': 'PBT0-C03', 'Text': 'a CT would need AutoCAD, a real MessageBox, the real configurator, the BOM gesture, window or export, Guardar en biblioteca, or a write to the user profile or to disk'},
    {'Id': 'PBT0-C04', 'Text': 'a mutant outside the closed table (INV-PBT0-14) or outside MutantScope, or one that violates the rules of section 5 of the package; a new test method that no table mutant detects in the conjunction; or an assertion without its own mutant that is not declared as an anchor'},
    {'Id': 'PBT0-C05', 'Text': 'the full CI of the GREEN fails outside the new tests, reproduces on a second run and is not attributable to the new tests (attributable is REWORK; not reproduced is a BLOCKED rerun within the 16.8 limits)'},
    {'Id': 'PBT0-C06', 'Text': 'a path or a target text of the closed mutant table does not match the base'},
    {'Id': 'PBT0-C07', 'Text': 'the facts of section 2 of the package do not hold on the base in a way that changes a CT'},
    {'Id': 'PBT0-C08', 'Text': 'a planning output rejected ONLY by the literal byte-fidelity comparison of the collections before the Worker allows ONE replanning with a reinforced prompt, without consuming an attempt ([DP] question 2); a second planning fidelity rejection is STOP and returns to the Owner; any other planning rejection (A1-A8 or another cause) is STOP and returns to the Owner'},
    {'Id': 'PBT0-C09', 'Text': 'a characterization test would hang, leave a modal, a dispatcher or a window alive, write to real user profile data or depend on machine-specific paths'},
    {'Id': 'PBT0-C10', 'Text': 'a mutant of the closed table does not discriminate its CT in its isolated run, or makes a NEW test method fail by exception instead of assertion, and no admitted alternative works (contract defect); the Worker does not invent a new mutant'},
    {'Id': 'PBT0-C11', 'Text': C11},
    {'Id': 'PBT0-C12', 'Text': 'a correction would be attempt 3 of 3: it returns to the Owner before it is issued (D-PB-10)'},
]
CONTRACT_REL = PILOT + '/%s/coordinator/gate-contract.json' % TASK
CO = PILOT + '/F4B-DRAFTS/coordinator/'
READ_ONLY = [
    (W, 'la ventana clasica de Push Back; en neto no cambia (el RED la muta solo segun la tabla cerrada)'),
    (WC, 'la parte compuesta de la ventana; en neto no cambia (el RED la muta solo segun la tabla cerrada)'),
    (CS, 'el estado del editor compuesto; en neto no cambia (el RED lo muta solo segun la tabla cerrada)'),
    (DA, 'el ensamblador del diseno; en neto no cambia (el RED lo muta solo segun la tabla cerrada)'),
    (RVA, 'la disponibilidad de vistas; en neto no cambia (el RED la muta solo segun la tabla cerrada)'),
    (DVD, 'las decoraciones de vista (estilo de cotas y escala); en neto no cambia (el RED las muta solo segun la tabla cerrada)'),
    ('src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml', 'el XAML de la ventana; no cambia'),
    (UT + 'StaTestRunner.cs', 'el hilo STA y el suelo del aviso; no cambia'),
    (UT + 'EditorWindowTestSupport.cs', 'los ayudantes de prueba; no cambian'),
    (UT + 'EditorDiscardPromptCollection.cs', 'la coleccion sin paralelismo; no cambia'),
]
for p, _ in READ_ONLY:
    assert os.path.isfile(p), p
cells = json.loads(json.dumps(t1b['EligibleCells']))
assert [c['Cell'] for c in cells] == ['anthropic/subagent/claude-sonnet-5-5', 'openai/process/gpt-6-luna']
c = {
    'Schema': 'rackcad-gate-contract/v1', 'Unit': 'I-64', 'Gate': 'F4B', 'TaskId': TASK,
    'Objective': ('Caracterizar con pruebas nuevas el comportamiento observable ACTUAL del estado editable de la ventana clasica de Push Back y de las funciones '
                  'Application de las que dependen PB-T1, PB-T2 y PB-T3, segun la section 6.2 del paquete de F4B v3.1 y la lista cerrada CT-PB0-01..21. El RED lleva '
                  'solo los mutantes de la tabla cerrada MU-PB0-01..20, y el GREEN los revierte con los bytes del producto identicos a la base. No se corrige nada ni '
                  'se ejecuta AutoCAD.'),
    'AuthorityRevision': AR, 'MainSha': MAIN,
    'Authorities': [
        {'Path': PKG, 'Section': 'archivo completo (v2.1): section 2 (hechos medidos), section 3 (decisiones), section 4 (CT-PB0-01 a CT-PB0-21 y las tablas T-1 a T-8), section 5 (la tabla CERRADA de mutantes), section 6 (el contrato) y section 7', 'Class': 'UNIT_DOC'},
        {'Path': PHASE, 'Section': 'paquete de la fase F4B v3.1: section 6 (PB-T0: 6.1 a 6.5), section 7 (reglas de proceso, T1B y RACKEDITAR) y OB-06', 'Class': 'UNIT_DOC'},
        {'Path': CO + 'owner-f4b-disposition.md', 'Section': 'archivo completo: disposicion del Owner y del Coordinator de F4B ([OD]); PB-T0 characterization / measurement only y la emision condicional con attempts 1/3', 'Class': 'UNIT_DOC'},
        {'Path': CO + 'owner-f4b-procedure-decisions.md', 'Section': 'archivo completo: decisiones de procedimiento del Owner ([DP], F4B-E03): preguntas 1 a 3', 'Class': 'UNIT_DOC'},
        {'Path': PILOT + '/' + TASK + '/coordinator/pbt0-block-m-procedure.md', 'Section': 'el anexo del bloque M (v2, con N-I1); aparte del bloque C', 'Class': 'UNIT_DOC'},
        {'Path': PILOT + '/F4B-DRAFTS/f4b-dispositions.md', 'Section': 'archivo completo: F4B-E01 a F4B-E04', 'Class': 'UNIT_DOC'},
        {'Path': PILOT + '/F4B-DRAFTS/package/map-a-classic-write-path.md', 'Section': 'MA: el camino de escritura clasico (no fiarse sin medir, INV-PBT0-04)', 'Class': 'UNIT_DOC'},
        {'Path': PILOT + '/F4B-DRAFTS/package/map-b-editor-state-testability.md', 'Section': 'MB: el estado del editor y su testabilidad (no fiarse sin medir, INV-PBT0-04)', 'Class': 'UNIT_DOC'},
        {'Path': 'docs/initiatives/I-64-f4-t1b-gate-contract-package.md', 'Section': 'el patron de T1B ([T1B]): D-T1B-03 y la atribucion de las existentes rotas', 'Class': 'UNIT_DOC'},
        {'Path': T1BC, 'Section': 'el contrato de T1B ([T1B-C]): las paradas estandar, los ayudantes y las celdas', 'Class': 'UNIT_DOC'},
        {'Path': PILOT + '/F4-T1B-PILOT-CHARACTERIZATION/task-close-package.md', 'Section': 'las lecciones de T1B ([T1B-X]), en especial section 6', 'Class': 'UNIT_DOC'},
        {'Path': PILOT + '/F4-DRAFTS/f4-dispositions.md', 'Section': 'las fuentes de PD-F4-01 (:1103-1106, :1540, :520, :562) y PD-F4-04 (:1657-1660), y F4-E59', 'Class': 'UNIT_DOC'},
        {'Path': PILOT + '/F4-DRAFTS/coordinator/owner-f4t3-authorization.md', 'Section': 'la firma literal de PD-F4-04 (:255-265)', 'Class': 'UNIT_DOC'},
        {'Path': 'docs/automation/state/I-64.yml', 'Section': 'estado vigente completo', 'Class': 'UNIT_DOC'},
        {'Path': 'docs/automation/evidence/I-64-evidence.md', 'Section': 'section 231 y siguientes', 'Class': 'UNIT_DOC'},
        {'Path': CONTRACT_REL, 'Section': 'este contrato', 'Class': 'UNIT_DOC'},
    ] + [{'Path': p, 'Section': s, 'Class': 'UNIT_CHANGE'} for p, s in READ_ONLY] + [
        {'Path': 'docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md', 'Section': 'D7, D8, D9 y D13', 'Class': 'EXTERNAL'},
        {'Path': 'docs/AUTOMATION_PLAN.md', 'Section': 'section 16.1 a section 16.10', 'Class': 'EXTERNAL'},
        {'Path': 'docs/automation/agent-execution/README.md', 'Section': 'section 4 (A1-A8) y section 10 (nc1..nc4)', 'Class': 'EXTERNAL'},
        {'Path': 'docs/automation/agent-execution/routing.md', 'Section': 'archivo completo', 'Class': 'EXTERNAL'},
        {'Path': 'docs/automation/agent-execution/model-catalog.md', 'Section': 'celdas medidas y vigentes para Controller y Worker', 'Class': 'EXTERNAL'},
        {'Path': 'docs/WORKFLOW.md', 'Section': 'relevo, Git e integracion', 'Class': 'EXTERNAL'},
    ],
    'AllowedWriteScope': ALLOWED,
    'ForbiddenWriteScope': FORBIDDEN,
    'Invariants': INV,
    'RequiredTests': [
        {'Project': 'tests/RackCad.Tests/RackCad.Tests.csproj', 'Filter': 'FullyQualifiedName~RackCad.Tests.PushBackFullEditorCharacterization', 'MinSelected': 3, 'ExpectRed': True},
        {'Project': 'tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj', 'Filter': 'FullyQualifiedName~RackCad.UI.Tests.PushBackFullEditorCharacterization', 'MinSelected': 18, 'ExpectRed': True},
    ],
    'ExpectedEvidence': EVID, 'StopConditions': STOPS, 'EligibleCells': cells, 'RoutingEnforcement': 'required', 'CorrectionsAuthorized': True,
    'IssuedBy': ('Sesion principal de I-64 (Claude) por la autorizacion condicional del Owner y del Coordinator de F4B '
                 '(docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-disposition.md: PB-T0 se emite con attempts 1/3 si la revision de la v2 '
                 'queda sin bloqueantes ni importantes sin resolver y sin decision material nueva), las decisiones de procedimiento owner-f4b-procedure-decisions.md '
                 '(F4B-E03) y la orden del Owner en el chat de arrancar la custodia y la emision de PB-T0 (F4B-E04)'),
    'IssuedUtc': ISSUED,
}
b = (json.dumps(c, indent=2, ensure_ascii=True) + '\n').encode('ascii')
assert all(x < 0x80 for x in b) and b'\\u' not in b, 'no ASCII'
assert not set(c['AllowedWriteScope']) & set(c['ForbiddenWriteScope'])
for a in c['AllowedWriteScope']:
    assert not any(a.startswith(f) for f in c['ForbiddenWriteScope'] if f.endswith('/')), a
TERMS = ['GATE PASS', 'GATE_PASS', 'Candidato', 'Candidate', 'FINAL_CANDIDATE_SHA', 'gate cerrado', 'gate closed', 'cierre del gate', 'rama integrada', 'integrada en main',
         'integrated into main', 'merged into main', 'lista para integrar', 'ready to merge', 'Owner Validation APROBADA', 'Owner Validation APPROVED']
for t in TERMS:
    assert not re.search(r'(?<![A-Za-z0-9_])' + re.escape(t) + r'(?![A-Za-z0-9_])', b.decode('ascii'), re.I), t
for bad in ('C:' + chr(92), 'C:' + '/', 'D:' + '/', 'Users', 'alej' + 'andra', 'App' + 'Data', 'scratchpad'):
    assert bad not in b.decode('ascii'), bad
if '--dry' in sys.argv:
    print('DRY allowed', len(ALLOWED), 'forbidden', len(FORBIDDEN), '(src', len(SRCF), 'tests', len(TESTF), 'root', len(ROOT), ') inv', len(INV), 'stops', len(STOPS),
          'authorities', len(c['Authorities']), 'bytes', len(b))
    if '--print' in sys.argv:
        for i, f in enumerate(FORBIDDEN):
            print(i, f)
    sys.exit(0)
os.makedirs(os.path.dirname(OUT), exist_ok=True)
open(OUT, 'wb').write(b)
o = subprocess.run(['pwsh', '-NoProfile', '-Command', f"Get-Content -Raw '{OUT}' | Test-Json -SchemaFile 'docs/automation/agent-execution/schemas/gate-contract.schema.json'"], capture_output=True, text=True)
assert o.stdout.strip().endswith('True'), o.stderr[-300:]
print('schema True allowed', len(ALLOWED), 'forbidden', len(FORBIDDEN), 'inv', len(INV), 'stops', len(STOPS), 'authorities', len(c['Authorities']), 'sha256', hashlib.sha256(b).hexdigest().upper())
