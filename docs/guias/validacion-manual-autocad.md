# Validación manual de RackCad en AutoCAD

Esta guía describe cómo comprobar un build de RackCad con los bloques DWG reales. Los comandos
canónicos y la definición de terminado viven en [AGENTS.md](../../AGENTS.md); los gates de
integración, en [WORKFLOW.md](../WORKFLOW.md). Un build verde demuestra que el código compila y que
las pruebas automatizadas pasan; **no demuestra que AutoCAD dibuje, edite o persista correctamente**.

## 1. Preparar el entorno

Requisitos:

- Windows y AutoCAD 2025 completo, no LT;
- .NET SDK compatible con `net8.0-windows`;
- `blocks-library.dwg` real del dueño configurado desde `RACKCAD`;
- worktree y commit exactos que se pretenden validar.

Antes de compilar:

1. Cierra AutoCAD por completo y confirma que no exista un proceso `acad`.
2. Verifica rama, commit y árbol limpio con `git status`, `git branch --show-current` y
   `git rev-parse HEAD`.
3. Usa el worktree de la iniciativa. No cargues un DLL del worktree principal para validar otra
   rama.

AutoCAD bloquea `RackCad.Plugin.dll` mientras está cargado. Si el build falla con `MSB3021` o
`MSB3027`, cierra AutoCAD y vuelve a compilar; ese fallo de copia no debe ocultarse como error de
código. Los avisos `MSB3277` conocidos provienen de las referencias de AutoCAD, pero no justifican
errores o advertencias propias nuevas.

## 2. Compilar y seleccionar el DLL correcto

Ejecuta desde la raíz del worktree:

```powershell
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug
dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug
dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug
```

Si AutoCAD está instalado en otra ruta:

```powershell
dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug `
  -p:AutoCADInstallDir="C:\Program Files\Autodesk\AutoCAD 2025"
```

El DLL que se carga es siempre el Debug producido dentro del worktree validado:

```text
<worktree>\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll
```

Registra el SHA de Git, la ruta absoluta del DLL, su fecha y, cuando la validación sea gate de
integración, su SHA-256. Un DLL sin trazabilidad no valida la rama.

## 3. Cargar con NETLOAD

1. Abre AutoCAD 2025 con un dibujo de prueba recuperable.
2. Ejecuta `NETLOAD`.
3. Selecciona el DLL exacto del worktree.
4. Ejecuta `RACKCAD` y confirma que el menú abre sin excepciones.
5. Verifica en el menú la ruta de `blocks-library.dwg`; no inventes bloques faltantes ni sustituyas
   nombres catalogados.

Si una pieza falta, registra el `pieceId`, la vista y el `blockName` esperado. Un stretch que
funciona manualmente pero no mediante API puede indicar dirección incorrecta del grip en el bloque,
no una regla geométrica defectuosa.

## 4. Comandos que deben considerarse

| Comando | Comprobación principal |
|---|---|
| `RACKCAD` | Menú principal, bibliotecas y acceso a los editores. |
| `RACKCABECERA` / `QUICKCABECERA` | Cabecera estándar, edición y colocación. |
| `RACKSISTEMADINAMICO` | Sistema dinámico y sus vistas ligadas. |
| `QUICKCAMA` | Cama de rodamiento dinámica o pushback. |
| `RACKSELECTIVO` | Selectivo, matriz, fondos, vistas y seguridad. |
| `RACKEDITAR` | Recuperación del diseño y redibujo en sitio. |
| `RACKDUPLICAR` | Identidad independiente frente a COPY de AutoCAD. |
| `RACKLISTA` | Inventario de racks, vistas y copias. |
| `RACKBOMTOTAL` | BOM consolidado sin duplicar vistas. |
| `RACKLAYOUT` / `RACKRELLENAR` | Colocación y relleno determinista. |
| `RACKAYUDA` | Referencia vigente de comandos y alias. |

El contrato de la iniciativa determina qué subconjunto debe recorrerse. No se declara validación
total si solo se ejecutó un smoke test.

## 5. Checklist funcional

### 5.1 Dibujo y redibujo

- Inserta la vista principal con el mouse y confirma origen, orientación, escala y capas.
- Comprueba bloques y parámetros dinámicos con los nombres catalogados.
- Modifica una dimensión que produzca un cambio visible y usa `Actualizar`.
- Confirma que el bloque se redefine en sitio y que ninguna copia se desplaza.
- Comprueba que un cambio multi-vista termina coherente en todas las vistas ligadas.

### 5.2 Edición y round-trip

- Guarda el DWG, ciérralo, vuelve a abrirlo y ejecuta `RACKEDITAR`.
- Confirma `Kind`, nombre, GUID, vista/sección y valores editables.
- Edita desde cada clase de vista aplicable: frontal, lateral o planta.
- Confirma que `Actualizar` conserva identidad y posición.
- Confirma que `Insertar {vista}` crea una vista ligada al mismo GUID, no un rack huérfano.
- Usa `RACKDUPLICAR` y verifica que el duplicado obtenga GUID nuevo; contrástalo con COPY, que
  comparte definición e identidad.

### 5.3 Vistas ligadas y multivista

- Selectivo: frontal por fondo, cortes laterales por poste y planta.
- Dinámico: laterales por poste, frontal de salida, frontal de entrada y planta.
- Cabecera: lateral y planta.
- Confirma que la `Section` seleccionada reaparece correctamente y que todos los bloques con el
  mismo GUID se redibujan una sola vez, sin vistas huérfanas.

### 5.4 BOM

- Abre el BOM desde cada editor incluido en el alcance.
- Compara físicamente piezas, componentes, cantidades, longitudes y peraltes con el dibujo.
- Ejecuta `RACKBOMTOTAL` sobre un dibujo con varias vistas y copias.
- Confirma que una pieza física no se duplique por aparecer proyectada en otra vista.
- Cuando aplique, exporta CSV/XLSX y revisa encabezados, totales y caracteres.

### 5.5 Persistencia y escenarios legacy

- Repite guardar/cerrar/abrir y `RACKEDITAR` después de modificar campos nuevos.
- Comprueba que un documento legacy representativo abre con los fallbacks documentados.
- Guarda el legacy actualizado y vuelve a abrirlo para comprobar el segundo round-trip.
- Rechaza versiones futuras incompatibles de manera visible; no aceptes pérdida silenciosa.
- En seguridad selectiva, comprueba selección, `DeepCopy`, redibujo y persistencia de cada campo
  afectado.

### 5.6 Escenarios por sistema

Para cabecera, comprueba horizontales ordenadas, paneles consecutivos, arreglos, placa base y
restauración estándar. Para selectivo, cubre al menos troquel, piso, fondos, medio frente, seguridad,
cotas y tarimas. Para dinámico, cubre fondos/niveles variables, IN/OUT, intermedios, camas, seguridad
y sus cuatro clases de vista. Para cama standalone, comprueba riel, rodillos, frenos, tope y el tipo
seleccionado.

Para **Push Back**, además de lo del dinámico, cubre la configuración por celda (I-41):

- un **fondo distinto por celda** dentro de un mismo frente: cada nivel debe terminar en su propio
  larguero posterior, con su propia cama y su propia pendiente, y la estructura del frente (cabeceras,
  separadores y postes derivados) debe dimensionarse por el nivel MÁS PROFUNDO;
- **restaurar el fondo** de una celda: vuelve a heredar el «Fondos frente» y la envolvente baja con él;
- los cinco alcances (`Celda / Selección / Nivel / Frente / Todo`) de «Aplicar fondo» y «Aplicar
  tarima»: deben cambiar SOLO esa propiedad, sin tocar claro, largo ni largueros de las demás celdas;
- **tarimas por celda**: aparecen en los cortes laterales y en los dos cortes frontales, siguen la
  altura de su celda, y **no aparecen en el BOM** (compruébalo con `RACKBOMTOTAL`). El lateral NO
  seccionado no las dibuja: es una envolvente, no una celda;
- **regresión de I-40**: con un rack que tenga cabeceras personalizadas por línea, cambiar un fondo
  INTERNO (sin mover el más profundo) no debe perder ninguna de esas cabeceras;
- **legacy**: un rack Push Back dibujado antes de I-41 debe reabrirse y redibujarse EXACTAMENTE igual —
  todos sus niveles al mismo fondo y sin ninguna tarima.

Y, desde I-42, el **Push Back compuesto (lado A / lado B)**:

- **declarar el lado B**: con «Rack de dos sentidos» apagado el rack es el de siempre y toda la sección
  compuesta está deshabilitada; al encenderlo aparece la segunda mitad sobre la MISMA estructura, sin que
  el lado A cambie de sitio ni de altura;
- **selector de lado**: la matriz Frente x Nivel, la celda seleccionada y los cinco alcances pasan a
  trabajar sobre el lado elegido; al volver al otro, su selección y su configuración siguen intactas;
- **el número de frentes es del RACK**: subirlo crece los DOS lados a la vez. Con 4 frentes y 3 niveles
  aplicados a todo, los CUATRO frentes deben traer sus camas en los tres niveles y en los dos lados —
  ninguno puede quedarse sólo con cabeceras y postes;
- **A = 3 y B = 4**: se declara con «La ranura existe en este lado» en el frente que sobra. La cuarta
  ranura existe solo en B, su estructura vive solo en la mitad de B y la retícula transversal (líneas de
  postes y BFR) es una sola para los dos lados. Quitar una ranura que quedaría sin ningún lado, o la
  última de un lado, debe rehusarse y explicarse;
- **topes por lado**: «Tope lado A» y «Tope lado B» deciden las cuatro combinaciones (ninguno / A / B /
  ambos) con los mismos cinco alcances. Con una sola cama, la casilla del lado que no tiene extremo alto
  queda deshabilitada con su motivo y conserva lo elegido — al volver a encontradas reaparece;
- **niveles y elevaciones independientes**: con 2 niveles en A y 5 en B, los postes se dimensionan por la
  mayor demanda y cada lado dibuja SUS elevaciones;
- **topologías distintas entre niveles del mismo frente**: nivel 1 corrida, nivel 2 encontradas, nivel 3
  solo A y nivel 4 solo B deben coexistir en el mismo frente;
- **encontradas**: DOS camas físicas con pendientes opuestas y sus extremos altos enfrentados en el
  centro, cada una con su propio tope (ninguno / A / B / ambos desde Seguridad);
- **corrida**: UNA sola cama que atraviesa A + hueco + B, con una sola pendiente continua y como mucho UN
  tope, en su extremo alto. Cámbiale el sentido (A→B y B→A) y comprueba que el extremo ALTO se mueve
  físicamente al otro lado y que el tope lo sigue;
- **fondo de la cama corrida**: con la celda en corrida, el campo de fondo pasa a llamarse «Fondo de cama
  corrida» y edita la profundidad TOTAL de esa cama. Sobre una estructura 5 + 8, escribir **10** debe dar
  una cama de 10 fondos —ni 13, ni 5, ni 8— sin mover ninguna de las dos estructuras. Devuelve la celda a
  encontradas: las dos camas por lado deben reaparecer intactas, y al volver a corrida el 10 sigue escrito;
- **dónde APOYA la cama**: su extremo bajo debe caer siempre sobre una línea de módulo. Una corrida que
  atraviesa el rack arranca en el PRIMER fondo (con hueco 0 y con hueco positivo); una corrida corta
  arranca en el apoyo que le corresponde, nunca a media posición;
- **largueros intermedios en PLANTA**: deben existir en las cuatro topologías **y en TODOS los frentes**,
  no sólo en el primero. Con una corrida corta, deben cubrir todo su recorrido —incluida la parte que pisa
  el otro lado— y ninguno por delante de su extremo bajo;
- **cada corte lateral**: recórrelos todos. Cada uno trae los niveles y los largueros de los frentes que
  tiene al lado; ninguno puede quedarse sólo con la estructura;
- **una estructura más larga que la cama**: sube la estructura del lado a 8 fondos y deja una celda en 4.
  Esa cama ocupa 4 y en el tramo sobrante NO hay riel, rodillo, intermedio ni tarima; otro nivel del mismo
  rack puede usar los 8 a la vez. La estructura es capacidad, no longitud obligatoria;
- **ninguna tarima dentro del hueco**: una corrida que lo cruza lo atraviesa, pero no almacena en él;
- **gap**: llévalo de 0 a un valor positivo y comprueba que el rack se ALARGA esa misma medida y que las
  dos líneas de postes de la interfaz siguen existiendo también con 0;
- **separador central**: con hueco positivo aparece UNA sola pieza —la misma que usa el rack— y se cuenta
  una vez en `RACKBOMTOTAL`; con hueco 0 se avisa y no se coloca;
- **estructura efectiva por lado**: sube la estructura del lado activo por encima de la propuesta y
  comprueba que el rack crece; bájala por debajo y comprueba que NO se corrige sola, que se avisa y que
  las celdas que no caben quedan bloqueadas con su motivo; «Restaurar estructura» vuelve a la propuesta;
- **fondos y tarimas por celda en los DOS lados**: son independientes; las tarimas siguen la pendiente de
  SU cama y siguen fuera del BOM;
- **cortes frontales**: los cuatro (entrada/salida y posterior de cada lado) se insertan y se actualizan;
  una celda corrida NO debe mostrar larguero posterior en la línea interior de su lado BAJO;
- **planta y laterales**: llevan las etiquetas **A** y **B** y muestran un larguero de entrada/salida en
  los DOS pasillos;
- **BOM**: la estructura NO se duplica por tener dos lados; una corrida cuenta UNA cama, a la longitud
  FÍSICA que se dibuja —la de su apoyo, que con fondo propio es menor que el rack—, y dos encontradas
  cuentan DOS;
- **round trip**: guarda, cierra, reabre con `RACKEDITAR` y comprueba que topología, sentido, hueco,
  separador, estructura manual y las dos configuraciones vuelven idénticas; `RACKDUPLICAR` produce una
  copia independiente;
- **legacy**: un rack Push Back dibujado antes de I-42 debe reabrirse como de un solo sentido, sin pedir
  ninguna reconfiguración, y redibujarse EXACTAMENTE igual.

## 6. Criterios de aprobación

Una validación manual se aprueba solo si:

- identifica commit y DLL exactos;
- no hay pérdida de datos ni error silencioso;
- dibujo, redibujo, BOM y round-trip concuerdan en el alcance declarado;
- las vistas ligadas conservan GUID, sección y posición;
- los bloques reales y sus parámetros se comportan correctamente;
- todo fallo se clasifica con pasos reproducibles y severidad;
- la persona que validó declara explícitamente el resultado.

La suite y los builds pueden estar verdes mientras un bloque DWG, un jig, una transacción o un
round-trip falla dentro de AutoCAD. Por eso ambos tipos de evidencia se registran por separado.

## 7. Formato de evidencia

```text
Fecha y zona:
Validador:
Iniciativa / rama:
Commit:
Worktree:
Ruta del DLL Debug:
SHA-256 del DLL:
Versión de AutoCAD:
DWG / escenario:
Bloques reales disponibles:

Pruebas automatizadas:
Build UI:
Build Plugin:

Checklist ejecutado:
- [ ] NETLOAD y menú
- [ ] dibujo / colocación
- [ ] actualización en sitio
- [ ] round-trip tras reabrir DWG
- [ ] vistas ligadas / multivista
- [ ] BOM y consolidado
- [ ] persistencia / legacy
- [ ] escenarios específicos de la iniciativa

Resultado por punto:
Fallos y severidad:
Evidencia adjunta:
Resultado global: aprobado | rechazado | parcial
Confirmación explícita del validador:
```

Una validación parcial no desbloquea un gate que exige el checklist completo. Después de un rebase
final, la evidencia anterior solo sigue siendo válida si `main` no avanzó desde el árbol validado.
