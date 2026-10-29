# Disposiciones del Coordinator sobre la entrega R20261029T005052Z-2be3 (RD11-AUTOLOADER-TABLE, intento 7)

Hechos comprobados por la sesion despues de la entrada del relevo (2026-10-29), antes de la verificacion.

## D1. Lectura del checkout principal no declarada por el Worker (desviacion de procedimiento, sin efectos)

- La transcripcion del Worker (agent jsonl, SHA-256 4DC39016C16037E512E16FE938ED725334B91AFF0C2B5FEE12D300DD3F6F29AA; 57 llamadas de
  herramienta) tiene una sola mencion del checkout principal fuera del informe del directorio efectivo: la ultima llamada (n.o 57,
  2026-10-29T01:01:52Z, PowerShell), escrita para validar la entrega ya escrita, termina con
  `git -C "D:/Documentos/Codex/Calculadora de racks" status --short | Select-Object -First 5`.
- Es una lectura de estado del checkout principal, contraria a la instruccion del prompt («nunca escribas ni leas alli»), y la frase
  de la entrega «No se leyo ni se escribio nada en el checkout principal» (Evidence 1), escrita antes de esa llamada, no la recoge.
- Efectos: ninguno. K1/K2 sin cambios en el checkout principal (2151 archivos rastreados, mismo resumen d0105e960dc69c52) ni en los
  otros cinco worktrees; el indice del checkout principal (.git/index) conserva su fecha de escritura 2026-10-12T09:29:36Z (git status
  no lo reescribio); `git status --porcelain` del checkout principal sigue vacio. No es escritura, no hay E/S relativa resuelta fuera
  del worktree y el directorio efectivo se informo: C-12 no se materializa.
- Disposicion del Coordinator: desviacion de procedimiento no declarada y sin efectos (se registrara como DEV-I65-RG-02). No afecta a
  los archivos entregados, a sus pruebas ni a su alcance; no es motivo de correccion.

## D2. CI de la corrida push del RED

- Corrida 40797120699 (push, refs/heads/architecture/interop-analisis-estructural, head 42b9dcdf): «Tests (Domain + Application)» en
  failure; «Build UI» y «UI Tests» en success; «Build Plugin without AutoCAD» en skipped.
- El job del Plugin se omite porque .github/workflows/ci.yml lo declara con `needs: [tests, build-ui]`: con el job Core en failure no
  se ejecuta, y el harness deploy/test-verify-bundle.ps1 no corre en la CI del RED. El contrato solo exige el job Core en failure en el
  RED (ExpectedEvidence 5). El RED del harness consta en la entrega (corrida local red-harness: codigo 1, caso «valido»).

## D3. TreeSha null en las corridas locales del Worker

- Las corridas locales se hicieron sobre el arbol de trabajo antes de cada commit (la entrega lo declara); el esquema admite TreeSha
  null en ese caso. Los resultados del SHA exacto son los de las corridas push de la CI (RED 40797120699 y CurrentSha, en RemoteFacts).

## D4. Advertencias de la compilacion Release

- La entrega declara 0 errores y advertencias MSB3277 (referencias de AutoCAD del Plugin) y de analizadores xUnit en archivos de prueba
  ajenos al diff, ninguna CS, NU ni NETSDK en archivos del diff. En la corrida push del CurrentSha (40797712877), el job «Build Plugin
  without AutoCAD» registra «Builds: Plugin Release publish and solution Release build succeeded without warnings.» (referencias de
  compilacion de NuGet de la CI, Assert-DotNetSuccess -NoWarnings).

## D6. Fin de linea de la tabla en el bundle de la CI (hecho conocido, sin cambio de alcance)

- En el job del Plugin de 40797712877, el inventario del bundle lista la tabla con SHA-256 b22f7d9dc45c6f59a37d29477e5d9a21fa0b61062d780b57805d93366dbb2e7d,
  que es exactamente el contenido del blob aprobado d088e2a9 con LF convertido a CRLF (el checkout de Windows de la CI convierte; la
  tabla no tiene atributo -text). El verificador compara con la fuente en disco del mismo checkout, como hace con los catalogos, y la
  acepta. Es el hecho RD11-F-08 del contrato del gate (A65-F4D-01, OPTIONAL desde F4-D): .gitattributes esta fuera del alcance. En el
  worktree de la sesion el archivo de trabajo es igual al blob (LF, FC3D9146...B619). No afecta a las huellas: MappingRowsSha256 se
  calcula sobre filas canonicas.

## D5. Procesos del relevo

- Entrada: dos git.exe ilegibles (19260 y su hijo 3372), hijos de claude.exe (PID 25508, la app de escritorio, creada el 2026-10-27),
  creados a las 01:02:08Z, 3 s despues del fin de la cesion (01:02:05Z), ya terminados al releerlos a las 01:02:18Z: falso P-02 conocido
  de las apps del host. Salida: dos git.exe ilegibles releidos a los 2 s, terminados. Ningun descendiente nuevo de la sesion.
