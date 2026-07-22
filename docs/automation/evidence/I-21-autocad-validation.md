# I-21 — Validacion manual en AutoCAD (APROBADA por el dueño)

> Gate **RESUELTO**. El dueño probo a profundidad el modulo dinamico en AutoCAD 2025 y **aprobo sin
> observaciones** el 2026-07-21. Esta confirmacion resuelve los gates `autocad` y `owner-validation`.

## DLL validado (NETLOAD), dentro del worktree de la iniciativa

```
D:\Documentos\Codex\Calculadora de racks-I-21-dynamic-editor-state\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll
```

- Punta de codigo validada: **`779ee0c4ea06f2a84bc2c5738979449ed25c269f`** (los commits posteriores de la
  rama son solo documentales y no cambian codigo). SHA-256 del DLL de referencia construido en la sesion
  de implementacion: `BEB69F420D744470B9C6280857CE42C4DA94E739C569AC5E637C8947D6F71BF4`.
- `origin/main` no avanzo desde `bfda406` (Merge I-15) al momento de la validacion e integracion, por lo
  que la validacion se conserva sobre el arbol vigente (WORKFLOW seccion 6): sin rebase.

## Criterio general

I-21 es un refactor que **preserva comportamiento**: extrae a `RackCad.Application` el estado del editor
dinamico (matriz/celdas, seleccion, recomputacion y construccion del diseno). Geometria, planes y BOM
salen **identicos** a lo vigente. La equivalencia automatizada esta cubierta por las pruebas nuevas de
`RackCad.Tests` y por la adopcion STA de `RackCad.UI.Tests`; el dibujo real de los bloques DWG lo
confirmo el dueño en AutoCAD.

## Checklist (aprobado por el dueño, sin observaciones)

- [x] `RACKDINAMICO`: la ventana abre igual (comportamiento y apariencia).
- [x] Matriz de frentes: crecer/decrecer frentes y niveles; seleccion simple y multiple (Ctrl);
      aplicar por celda, seleccion, nivel, frente y todo; aplicar datos estructurales a frentes.
- [x] Cabeceras por modulo: configuracion calculada vs personalizada; preservacion de fondos
      personalizados al cambiar tarimas/pallet; actualizacion de altura en sitio conserva los modulos.
- [x] Seguridad, largueros IN/OUT e intermedios, fondos y niveles: seleccion y proyeccion sin cambios.
- [x] Previews (lateral por poste, frontal de salida, frontal de entrada) y vistas vinculadas: identicos.
- [x] BOM del sistema (ver y exportar): sin diferencias.
- [x] Biblioteca, persistencia legacy y round-trip: guardar/abrir y restaurar diseno legacy sin perdida.
- [x] Insertar la lateral; `RACKEDITAR`: actualizacion en sitio e insercion de vistas enlazadas
      conservando el **mismo GUID**.
- [x] Geometria y BOM **sin diferencias** frente a la version previa a I-21.

## Resultado

- Estado: **APROBADO**.
- Fecha: **2026-07-21**.
- Commit/DLL validado: codigo **`779ee0c`**, DLL Debug del worktree I-21 (ruta y SHA-256 arriba).
- Confirmacion del dueño: probo a profundidad el modulo dinamico en AutoCAD 2025 y aprobo sin
  observaciones —comportamiento y apariencia de la ventana; matriz, selecciones y aplicacion por alcance;
  cabeceras calculadas y personalizadas; seguridad, IN/OUT e intermedios; previews y vistas vinculadas;
  geometria y BOM; biblioteca, persistencia legacy y round-trip; actualizacion en sitio y conservacion
  del GUID. Resuelve `autocad` y `owner-validation`.
- Observaciones: ninguna.
