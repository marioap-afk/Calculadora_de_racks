# I-21 — Validacion manual en AutoCAD (PENDIENTE del dueño)

> Este gate esta **ABIERTO**. Esta sesion NO declara AutoCAD aprobado (AUTOMATION_PLAN seccion 10 y
> Context Pack `delivery-validation`). El dueño ejecuta el checklist sobre el DLL Debug del worktree y
> registra el resultado aqui antes de integrar.

## DLL a cargar (NETLOAD), dentro del worktree de la iniciativa

```
D:\Documentos\Codex\Calculadora de racks-I-21-dynamic-editor-state\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll
```

- Cerrar AutoCAD antes de cada rebuild del worktree (el DLL cargado queda bloqueado — trampa conocida
  de AGENTS.md). Reconstruir con `dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug`.
- SHA-256 del DLL construido en esta sesion (referencia; el dueño recompila desde la punta de codigo y
  compara): `BEB69F420D744470B9C6280857CE42C4DA94E739C569AC5E637C8947D6F71BF4`.
- La validacion que cuenta para integrar es la que se hace **sobre el arbol ya rebasado sobre `main`**
  (WORKFLOW seccion 6). Al reclamar, `origin/main` = `bfda406` (Merge I-15) y la rama parte de esa punta
  exacta, sin trunk por delante.

## Criterio general

I-21 es un refactor que **preserva comportamiento**: extrae a `RackCad.Application` el estado del
editor dinamico (matriz/celdas, seleccion, recomputacion y construccion del diseno). Geometria, planes
y BOM deben salir **identicos** a lo vigente. La equivalencia automatizada (matriz, ensamblador,
seguridad, celdas) esta cubierta por las pruebas nuevas de `RackCad.Tests` y por la adopcion STA de
`RackCad.UI.Tests`; el dibujo real de los bloques DWG solo lo confirma AutoCAD.

## Checklist (marcar por el dueño)

- [ ] `RACKDINAMICO`: la ventana abre igual (controles, textos, orden, apariencia).
- [ ] Matriz de frentes: crecer/decrecer frentes y niveles; seleccion simple y multiple (Ctrl);
      aplicar por celda, seleccion, nivel, frente y todo; aplicar datos estructurales a un frente, a
      los seleccionados y a todos.
- [ ] Cabeceras por modulo: configuracion calculada vs personalizada; preservacion de fondos
      personalizados al cambiar tarimas/pallet; actualizacion de altura en sitio conserva los modulos.
- [ ] Seguridad, IN/OUT, largueros intermedios, fondos y niveles: seleccion y proyeccion sin cambios.
- [ ] Preview lateral por poste, frontal de salida y frontal de entrada: identicos.
- [ ] BOM del sistema (ver y exportar): sin diferencias.
- [ ] Guardar en biblioteca y abrir desde biblioteca (inserta como nuevo, GUID nuevo): OK.
- [ ] Insertar la vista lateral; luego `RACKEDITAR` sobre el rack dibujado: actualizar en sitio e
      insertar vistas enlazadas (frontal salida/entrada, planta) conservan el **mismo GUID** y ligan.
- [ ] Escenario legacy: abrir un diseno dinamico guardado antes (sin campos nuevos) restaura la matriz
      y resuelve sin perdida (fallback de cabeceras legacy preservado).
- [ ] Geometria, planes y BOM **sin diferencias** frente a la version previa a I-21.

## Resultado

- Estado: **pendiente**.
- Fecha:
- Commit/DLL validado:
- Observaciones:
