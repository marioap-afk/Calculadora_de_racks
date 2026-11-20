"""Arma la v4.3 del paquete de PB-T4 desde la v4.2: la cabecera y el registro V43 de v43/f-head.md, y correcciones literales.

Uso: python -I make_v43.py <salida-v43.md>
Lee, en esta carpeta: I-64-f4b-pbt4-gate-contract-package-v42.md, v43/f-head.md y measure_pbt4_v43-out.txt. Escribe solo <salida-v43.md>.
"""
import re
import sys

out_path = sys.argv[1]
here = __file__.replace('make_v43.py', '')


def read(name):
    with open(here + name, encoding='utf-8') as f:
        return f.read()


def once(text, old, new, what):
    n = text.count(old)
    assert n == 1, '%s: %d coincidencias' % (what, n)
    return text.replace(old, new)


v42 = read('I-64-f4b-pbt4-gate-contract-package-v42.md')
head_end = v42.index('## Registro de cambios v4.1 → v4.2')
# La cabecera nueva, y la de la v4.2 se queda como registro (sin su título ni su estado), antes de su tabla de cambios.
abbr = v42[v42.index('**Abreviaturas:**'):v42.index('---\n\n## Registro de cambios v4.1 → v4.2')]
head = read('v43/f-head.md')
head = once(head, '---\n\n## Registro de cambios v4.2 → v4.3', abbr + '---\n\n## Registro de cambios v4.2 → v4.3', 'abreviaturas')
out = head.rstrip('\n') + '\n\n' + v42[head_end:]

g = read('measure_pbt4_v43-out.txt').splitlines()
start = next(i for i, l in enumerate(g) if l.startswith('| Guarda'))
guards43 = '\n'.join(l for l in g[start:] if l.startswith('|')) + '\n\n' + [l for l in g if l.startswith('rojas=')][0].replace(
    'rojas=', '**En rojo:** ').replace('sobre el GREEN; contrafactuales en rojo=', 'sobre el GREEN; contrafactuales en rojo, como se espera: ') + '.'

FIXES = [
    # ---------------------------------------------------------------- I-01, el código literal
    ('        public static string? Choose(DraftStaleBase staleBase, string originText)\n',
     '        public static string Choose(DraftStaleBase staleBase, string originText)\n', 'I-01 Choose'),
    ('                scan.RawEnvelopes.ToDictionary(pair => pair.Key, pair => (string?)pair.Value, StringComparer.Ordinal),\n',
     '                scan.RawEnvelopes.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal),\n', 'I-01 molde'),
    ('`AtKey` copia los sobres con el mismo molde que UR (`ToDictionary(… (string?)pair.Value …)`), porque el constructor del escaneo los pide como `IReadOnlyDictionary<string, string?>` y pasar los de la propiedad daría un aviso de nulabilidad nuevo en el Plugin.',
     '`AtKey` copia los sobres en un diccionario nuevo con el comparador ordinal, como UR (`ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal)`). **Sin ningún `?` de tipo de referencia** (CA-T4a I-01; la regla de §5.0): los proyectos tienen `Nullable=disable` (`Directory.Build.props:28`) y la pureza de `Draft/` prohíbe `#nullable`, así que el archivo es «oblivious». Ahí, pasar un `Dictionary<string, string>` al `IReadOnlyDictionary<string, string?>` del constructor no da ningún aviso, y un `string?` daría CS8632, que rompe `build-plugin`.',
     'I-01 justificación'),
    ('con `Choose` que devuelve `null` (`=> null;`), `AtKey` que devuelve `scan` (`=> scan;`) y `ReadAt` que devuelve `null` (`=> null;`), con sus firmas literales y su `using RackCad.Application.Catalogs;` (B-08);',
     'con `Choose` que devuelve `null` (`=> null;`), `AtKey` que devuelve `scan` (`=> scan;`) y `ReadAt` que devuelve `null` (`=> null;`), con sus firmas literales y su `using RackCad.Application.Catalogs;` (B-08). **La firma es `public static string Choose(DraftStaleBase staleBase, string originText)`, sin `?`** (CA-T4a I-01): con `string?`, el esqueleto ya daba CS8632;',
     'I-01 esqueleto'),
    # ---------------------------------------------------------------- I-01, la regla del §5.0
    ('**Un solo «Actualizar» lógico:** las recetas de PB-T4b no llaman a «Actualizar» desde la ventana; solo desde el panel.',
     '**Ningún `?` de tipo de referencia anulable sin `#nullable enable`** (CA-T4a I-01; V43-01). `Directory.Build.props:28` fija `<Nullable>disable</Nullable>`, y ningún `.csproj` lo cambia (medido en `7f0d9aa3`): ni `src/RackCad.Application`, ni `src/RackCad.Domain`, ni `src/RackCad.Plugin`, ni `src/RackCad.UI`, ni `tests/RackCad.Tests`, ni `tests/RackCad.UI.Tests`, ni las dos herramientas de `tools/`. Por eso:\n'
     '- un archivo nuevo o una línea nueva, de `src/` o de `tests/`, no escribe `string?`, `object?` ni ningún `T?` de un tipo de referencia, salvo que el archivo empiece por `#nullable enable` (como WEI, WHC, RIA o la interfaz nueva);\n'
     '- **en `Draft/`, nunca:** su pureza prohíbe la directiva (CT-PBT3A-14 y CT-PBT4A-08);\n'
     '- los `?` de tipos de valor (`long?`, `double?`, una tupla `(…)?`) y el operador `!` no cuentan;\n'
     '- el motivo: cada uno da **CS8632**, y `build-plugin` compila la solución en Release y falla con cualquier `warning CS` (`eng/ci/verify-autocad-references.ps1`, `Assert-DotNetSuccess -NoWarnings`).\n\n'
     '**Revisado frente a la regla** (`measure_pbt4_v43-out.txt`): el código literal de PB-T4a (WEI, WHC, RIA y la interfaz llevan `#nullable enable`; NH no lo lleva y su texto nuevo no tiene ningún `?` de referencia; `Draft/PushBackOriginByText.cs`, ya corregido) y el de PB-T4b y PB-T4c (`NumericTextRule.Check`, con `double?`; `PushBackAnnotationScaleRule.Validate`; `PushBackKeepMinePresentation.Of` y `Read`; `IWorkspaceFullEditorHost.ShowModal<TLaunch>`; `FullEditorModal.ShowModal`, con `Action<object>`; `RackWorkspaceEditorWindow.Show(object launch)`). **Solo cambia `PushBackOriginByText.cs`.** Lo que el paquete no da literal (el `NumericTextCheck` de PB-T4b, o `_fullEditorHost` en WHC) sigue la regla al escribirse: en un archivo sin `#nullable enable`, sin `?`.\n\n'
     '**Un solo «Actualizar» lógico:** las recetas de PB-T4b no llaman a «Actualizar» desde la ventana; solo desde el panel.',
     '§5.0 regla'),
    # ---------------------------------------------------------------- M-01
    ('**Las guardas del Plugin, medidas sobre el texto literal del GREEN** (§4.1: el recorrido entero de las clases que leen los archivos tocados, `walk_guards_pbt4a.py`, y las aserciones traducidas de `measure_pbt4_v42-out.txt`, 35 de 35 en verde):',
     '**Las guardas del Plugin, medidas sobre el texto literal del GREEN** (§4.1: el recorrido por marcas de las clases que leen los archivos tocados, `walk_guards_pbt4a.py`, con la ampliación de CA-T4a; y las aserciones traducidas de `measure_pbt4_v43-out.txt`, 36 de 36 en verde):',
     'M-01 D-02'),
    ('**El recorrido entero (I-25), por diferencia**', '**El recorrido por marcas (I-25), por diferencia**', 'M-01 título'),
    ('- **Qué recorre:** las **46 clases** de prueba que nombran por texto los archivos que toca PB-T4a',
     '- **Qué recorre, y qué no** (CA-T4a M-01): selecciona por las marcas `MARKERS` de `walk_guards_pbt4a.py`, que son literales de ruta y de ayudante; **no ven** una ruta compuesta con `Path.Combine(…, "src", "RackCad.Application", "Workspace")`, ni los barridos de todo `src/`, de todo Application o de todo el Plugin (las marcas no incluyen `src`, `src/RackCad.Application` ni `src/RackCad.Plugin`). Con ese criterio recorre las **46 clases** de prueba que nombran por texto los archivos que toca PB-T4a',
     'M-01 qué recorre'),
    ('- **La reflexión:** las guardas que la usan sobre WHC (EIH:993-1104, WLG:357-358, PEG:383-385, WPUH:503 e IG:427-428) siguen, porque WHC no declara nada nuevo; las que enumeran tipos del ensamblado (`NoProductPlannerGuardTests`, `RackModuleEditSessionTests:497`, `CustomPropertiesProjectTests:329` y `RackViewCountInvariantTests:39`) miran otros espacios de nombres o nombres que los tipos nuevos no usan.',
     '- **La ampliación de CA-T4a, sobre lo que las marcas no ven** (`cat4a_walk_gap.py` y `cat4a_added_lines.py`, con sus salidas `cat4a_walk_gap-out.txt` y `cat4a_added_lines-all-out.txt`, a custodiar):\n'
     '  - **146** archivos de prueba fuera del recorrido nombran `src/RackCad.(Application|Plugin)`, `"src"` o un ayudante de barrido; **97** nombran como literal una carpeta antecesora de un archivo tocado o nuevo;\n'
     '  - cada literal de esos 97 (de 4 letras o más, o con metacaracteres) se probó contra las **134** líneas que el GREEN añade o cambia en WHC, WEI, NH y RIA, y contra las de los dos archivos nuevos: solo casan fichas genéricas (`Systems`, `RackCad.Application`, `PushBack`, `name`), en barridos acotados a otros archivos, y algún falso positivo de la herramienta (un literal que, como expresión regular, es una clase de caracteres);\n'
     '  - **los que sí leen los archivos tocados se leyeron:** `PushBackSharedReferenceGuardTests` (CT-PBT1B-14, cuyas reglas de G-L1 el GREEN no toca, y CT-PBT1B-15, sin ninguna asignación `.Name =` nueva), `WorkspaceModelBoundaryTests`, `CustomPropertiesEnvelopeGuardTests` y `CustomPropertiesEdgeGuardTests` (eligen sus archivos por nombre), `RackDefinitionCreatorGuardTests`, `NamespaceFolderGuardTests` (los dos archivos nuevos tienen el espacio de nombres de su carpeta, en bloque), `StructuralSectionPluginSourceGuardTests` y `SelectiveDuplicationFailClosedTests` (archivos fijos). **Ninguno cae.**\n'
     '- **La reflexión:** las guardas que la usan sobre WHC (EIH:993-1104, WLG:357-358, PEG:383-385, WPUH:503 e IG:427-428) siguen, porque WHC no declara nada nuevo; **`WorkspaceModelBoundaryTests`** recorre por reflexión los tipos y miembros de `RackCad.Application.Workspace`, y ninguno nuevo (`IWorkspaceEditIntentOriginReader`, `ReadIntentAtOrigin`, `FullRead`, `Catalog` y `FromReads`) casa con `Subscription|Editor|Draft`, `Tab` ni `ProjectSummary|Metric|Provider|…`; las que enumeran tipos del ensamblado (`NoProductPlannerGuardTests`, `RackModuleEditSessionTests:497`, `CustomPropertiesProjectTests:329` y `RackViewCountInvariantTests:39`) miran otros espacios de nombres o nombres que los tipos nuevos no usan.',
     'M-01 ampliación y reflexión'),
    ('**Lo que el recorrido no puede ver** (D = 0 en el GREEN, §5.1): una prueba que lea el texto por un camino que no deja ninguna ruta, carpeta ni ayudante en su código, o que dependa de la reflexión sobre un tipo que no se miró. Es un riesgo declarado, con su parada: **PBT4A-C16**. CA-T4a vuelve a leer las 100 señaladas sobre el texto del paquete custodiado.',
     '**Lo que queda** (D = 0 en el GREEN, §5.1): **una lectura que solo se ve al ejecutar** (una regla que no se escribe como literal, o la reflexión sobre un tipo que no se miró). La cierra la medida de la precondición 6 con `apply_green_pbt4a.py` (la suite completa de Core sobre el `src/` del GREEN literal); si una prueba de fuera de la lista cae en el GREEN, es **PBT4A-C16** y vuelve al Owner. CA-T4a leyó las 100 señaladas y las de la ampliación sobre el texto de la v4.2, igual en esto.',
     'M-01 lo que queda'),
    ('- **Las aserciones traducidas** (`measure_pbt4_v42.py`; sobre el mismo GREEN, con la de B-08 y las dos contrafactuales):',
     '- **Las aserciones traducidas** (`measure_pbt4_v43.py`; sobre el mismo GREEN, con la de B-08, la regla de CA-T4a I-01 y las tres contrafactuales):',
     'M-01 aserciones'),
    ('aplicado en memoria, y **las recorre enteras**, prueba a prueba, en las clases que leen los archivos tocados (para PB-T4a, §4.1: `walk_guards_pbt4a.py` y `measure_pbt4_v42-out.txt`).',
     'aplicado en memoria, y **las recorre por marcas**, prueba a prueba, en las clases que nombran los archivos tocados o sus carpetas, con lo que las marcas no ven medido aparte (para PB-T4a, §4.1: `walk_guards_pbt4a.py`, la ampliación de CA-T4a y `measure_pbt4_v43-out.txt`); y, sobre la copia, **compila el `src/` del GREEN literal** con `apply_green_pbt4a.py` y corre la suite de Core sobre él.',
     'M-01 §5.0'),
    ('**En el GREEN, la precondición 6 no lo confirma,** porque no hay GREEN que compilar: lo sostiene el recorrido entero de §4.1 sobre el texto literal del GREEN, prueba a prueba, de las clases que leen los archivos tocados. Lo que ese recorrido no puede ver (una prueba que lea el texto por un camino que no deja ninguna ruta, carpeta ni ayudante en su código, o la reflexión sobre tipos fuera de los que se miraron) es un riesgo declarado: si una prueba fuera de la lista falla en el GREEN, es PBT4A-C16 y vuelve al Owner.',
     '**En el GREEN,** lo sostienen el recorrido por marcas de §4.1 y la ampliación de CA-T4a sobre los 97 archivos de fuera, por texto; y **lo mide la precondición 6** con `apply_green_pbt4a.py`: el `src/` del GREEN literal sobre la copia y la suite completa de Core, que debe dar **19 129 = 19 122 + 0 + 7** con el `Skip` de CT-PBT3A-12 puesto (19 129 = 19 122 + 1 + 6 sin él, con CT-PBT3A-12 como única caída) y ningún `warning CS`. Lo que queda es una lectura que solo se ve al ejecutar las clases nuevas del Worker: si una prueba fuera de la lista cae en el GREEN, es PBT4A-C16 y vuelve al Owner.',
     'M-01 §5.1'),
    ('- **Las guardas de PB-T4a se recorrieron enteras por diferencia** sobre el texto literal del GREEN que prescribe este paquete (§4.1), y las 100 señaladas se leyeron una a una. El recorrido no ve una prueba que lea el texto sin dejar en su código ninguna ruta, carpeta ni ayudante, ni la reflexión sobre tipos que no se miraron: es el riesgo declarado de PBT4A-C16. CA-T4a vuelve a leer las señaladas, y la precondición 6 compila el esqueleto y corre la suite.',
     '- **Las guardas de PB-T4a se recorrieron por marcas, por diferencia,** sobre el texto literal del GREEN que prescribe este paquete (§4.1), y las 100 señaladas se leyeron una a una. **No es un recorrido entero** (CA-T4a M-01): las marcas no ven las rutas compuestas con `Path.Combine` ni los barridos de todo `src/`, de todo Application o de todo el Plugin; CA-T4a midió aparte esos 97 archivos, y ninguno cae. Lo que queda es una lectura que solo se ve al ejecutar: la mide la precondición 6 con `apply_green_pbt4a.py`, y si cae una prueba fuera de la lista es PBT4A-C16.',
     'M-01 §9'),
    ('PBT4A-C16: an existing test outside the closed list of substitutions of PB-T4a (CT-PBT3A-12) fails. It returns to the Owner (owner-pbt4-decisions.md, question 4).',
     'PBT4A-C16: an existing test outside the closed list of substitutions of PB-T4a (CT-PBT3A-12) fails, including one that reads the text in a way only seen when it runs. It returns to the Owner (owner-pbt4-decisions.md, question 4).',
     'M-01 C16'),
    ('las roturas medidas con el recorrido entero de sus guardas (§4.1)',
     'las roturas medidas con el recorrido por marcas de sus guardas y la ampliación de CA-T4a (§4.1)', 'M-01 §0'),
    ('(contrafactual medido en `measure_pbt4_v41-out.txt` y en `measure_pbt4_v42-out.txt`)',
     '(contrafactual medido en `measure_pbt4_v41-out.txt`, `measure_pbt4_v42-out.txt` y `measure_pbt4_v43-out.txt`)', 'contrafactual'),
    # ---------------------------------------------------------------- la precondición 6 (V43-03, V43-04)
    ('   - el recorrido de las guardas sobre el texto literal del GREEN (PB-T4a: §4.1, 1 026 pruebas, una que cae, la de la lista) y las aserciones traducidas (`measure_pbt4_v42-out.txt`, 35 de 35);',
     '   - el recorrido de las guardas por marcas sobre el texto literal del GREEN (PB-T4a: §4.1, 1 026 pruebas, una que cae, la de la lista), la ampliación de CA-T4a (97 archivos, ninguno cae) y las aserciones traducidas (`measure_pbt4_v43-out.txt`, 36 de 36);\n'
     '   - **PB-T4a, el GREEN literal compilado** (V43-03): `python -I apply_green_pbt4a.py <dir-copia> --repo <checkout-principal> --rev 7f0d9aa3` sobre una copia exportada de `7f0d9aa3` (sin `.git`); con el `Skip` de CT-PBT3A-12 puesto, la suite completa de Core da **19 129 = 19 122 + 0 + 7**, y ningún `warning CS` en los registros del Plugin, de Core y de UI (los `MSB3277` son de las referencias locales de AutoCAD);\n'
     '   - **cero `warning CS`** también sobre el esqueleto (CA-T4a I-01);\n'
     '   - **los 12 filtros de UI** de los `RequiredTests` de PB-T4a (n.º 8, 15, 20, 21, 23, 25 a 27 y 35 a 38) sobre la copia, o la declaración, en `compile-check.md` y en `COMPILE_RESULT`, de que no se corrieron; en ese caso los cubre el trabajo `ui-tests` de la CI del RED (CA-T4a M-03);',
     'precondición 6'),
    # ---------------------------------------------------------------- la versión (V43-05)
    ('- este paquete (v4.2), con su comprobación acotada de PB-T4a.', '- este paquete (v4.3), con su comprobación acotada de PB-T4a (CA-T4a).', 'autoridades'),
    ('- el `Objective` cita «el paquete del contrato de PB-T4 v4.2», en lugar de v4.1; lo demás del `Objective`, igual;',
     '- el `Objective` cita «el paquete del contrato de PB-T4 v4.3» (en la v4.2, v4.2; en la v4.1, v4.1); lo demás del `Objective`, igual;\n'
     '- **en la v4.3** (CA-T4a I-01): la firma literal de `Choose` en el esqueleto y en el GREEN, `public static string Choose(…)`, y el molde de `AtKey`, `pair => pair.Value`; el sha256 del parche del esqueleto cambia con ellos;',
     'generador'),
]
for old, new, what in FIXES:
    out = once(out, old, new, what)
n = out.count('paquete del contrato de PB-T4 v4.2')
assert n == 3, n   # los tres Objective
out = out.replace('segun el paquete del contrato de PB-T4 v4.2', 'segun el paquete del contrato de PB-T4 v4.3')
# la tabla de las aserciones traducidas de §4.1, ahora la de la v4.3
a = out.index('- **Las aserciones traducidas** (`measure_pbt4_v43.py`')
b = out.index('**Lo que queda** (D = 0 en el GREEN, §5.1)')
block = out[a:b]
first_table = block.index('| Guarda')
out = out[:a] + block[:first_table] + guards43 + '\n\n' + out[b:]
code_start = out.index('```csharp\nusing System;\nusing System.Linq;\nusing RackCad.Application.Catalogs;')
code = out[code_start:out.index('\n```\n', code_start + 10)]
assert 'namespace RackCad.Application.Systems.PushBack.Draft' in code and not re.search(r'(?<![\w.])(?:string|object|[A-Z]\w*)\?(?=\s+[A-Za-z_]|\s*[,)>\]])', code), \
    'queda un ? de tipo de referencia en el bloque de PushBackOriginByText'
with open(out_path, 'w', encoding='utf-8', newline='\n') as f:
    f.write(out)
left = [w for w in ['recorrido entero', 'recorrieron enteras', 'las recorre enteras', 'measure_pbt4_v42-out', 'string? Choose', '(string?)pair'] if w in out]
print('bytes', len(out.encode('utf-8')))
print('restos (los del registro de la v4.2 y anteriores son historia):', left)
