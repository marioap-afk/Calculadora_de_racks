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

