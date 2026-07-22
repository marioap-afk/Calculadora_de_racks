# I-22 — Validacion manual en AutoCAD (APROBADA por el dueño)

> Gate **RESUELTO**. El dueño probo la colocacion de seguridad del selectivo en AutoCAD 2025 y
> **aprobo sin observaciones** el 2026-07-22: «Listo, probé todo, parece estar correcto». Esta
> confirmacion resuelve los gates `autocad` y `owner-validation`.

## DLL validado (NETLOAD), dentro del worktree de la iniciativa

```
D:\Documentos\Codex\Calculadora de racks-I-22-safety-placement\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll
```

- Punta de codigo validada: **`3ce71394f8858cf600b1e28d042ecebc5ba6a7c2`** (los commits posteriores de la
  rama son solo documentales —registro de esta aprobacion + cierre de integracion— y **no cambian
  codigo**). El DLL Debug se compilo en la sesion de implementacion desde ese arbol (punta publicada
  `1e78b2c`, cuyo unico contenido sobre `3ce7139` es documentacion/estado).
- **SHA-256 del DLL de referencia**: `969580AE67EAC69C8018304F3A9DD963C7DDD77307D5A26E913C32CC1A31038C`.
- AutoCAD: **2025**.
- `origin/main` esta en **`27ffdf3`** (Merge I-24) y **no avanzo** desde que la rama quedo rebasada sobre esa
  punta; la validacion se conserva sobre el arbol vigente (WORKFLOW seccion 6): sin rebase adicional. I-05 e
  I-24 (integradas antes) tocan codigo **disjunto** de I-22.

## Criterio general

I-22 es un refactor que **preserva comportamiento**: extrae servicios de colocacion por familia parametrizados
por vista (tope/parrilla/tarima/separador), descompone `SelectiveSafetySelection` en configuraciones por subtipo
con DTO por familia (formato de alambre plano byte-identico), enruta el paso de troquel a una sola constante y
adopta `SelectionMatrix` en las rejillas tope/desviador/guia. La geometria resuelta, los planes y el BOM salen
**identicos** a lo vigente. La equivalencia automatizada esta cubierta por la caracterizacion **golden** de
`RackCad.Tests` (7 baselines: multiset de instancias frontal/lateral/planta + BOM, incl. medio frente y cuadruple
profundidad) y por la adopcion STA de `RackCad.UI.Tests`; el dibujo real de los bloques DWG lo confirmo el dueño
en AutoCAD.

## Checklist (aprobado por el dueño, sin observaciones)

- [x] **Topes (larguero tope):** rejilla nivel×frente (adopta `SelectionMatrix`), `TopeShared` vs per-fondo,
      `TopeFondo`, `SAQUE`, toggle frontal; identicos en frontal/lateral/planta; `TROQUEL_TOPE`; BOM por tramo.
- [x] **Parrilla (deck):** toggles frontal/lateral, `FRENTE`/`FONDO`, frente/cantidad manual, off-cells, contador
      por celda; frontal/lateral y BOM sin diferencias (incl. clamp "no cabe").
- [x] **Tarima (referencia visual):** "Mostrar tarimas" frontal + lateral; sin BOM; `TARIMA_GENERICA`.
- [x] **Separadores:** doble/triple/cuadruple profundidad; separadores en lateral y planta; BOM.
- [x] **Desviador y guia-entrada:** rejillas que adoptan `SelectionMatrix`; longitud/altura, nota de holgura viva,
      IN/OUT, off-cells preservados; frontal/lateral/planta y BOM sin diferencias.
- [x] **Bota / protector lateral / defensa:** sin cambios (no refactorizados aqui) — verificacion de no-regresion.
- [x] **Vistas frontal, lateral y planta**, **medio frente** y **multiples fondos**: geometria y colocacion sin
      diferencias.
- [x] **BOM** del sistema (ver y exportar): sin diferencias.
- [x] **Round-trip / persistencia / biblioteca:** reabrir con `RACKEDITAR` (mismo GUID); reabrir desde biblioteca;
      guardar; documento legacy sin campos de seguridad; sin perdida.
- [x] **Multivista:** actualizacion en sitio e insercion de vistas ligadas (lateral/planta) con el **mismo GUID**.
- [x] **Apariencia e interaccion de las rejillas `SelectionMatrix`** (tope/desviador/guia): identicas — contenido,
      cabeceras, orden, off-cells y controles auxiliares conservados.

## Resultado

- Estado: **APROBADO**.
- Fecha: **2026-07-22**.
- Validador: **dueño del proyecto**.
- Iniciativa / rama: **I-22** / `refactor/safety-placement`.
- Commit/DLL validado: codigo **`3ce7139`**, DLL Debug del worktree I-22 (ruta y SHA-256 arriba).
- Confirmacion del dueño: «Listo, probé todo, parece estar correcto» — geometria y colocacion de topes,
  parrillas, tarimas, separadores y elementos relacionados; BOM; vistas frontal/lateral/planta; medio frente y
  multiples fondos; actualizacion y vistas ligadas con conservacion del GUID; persistencia, biblioteca y
  round-trip; apariencia e interaccion de las rejillas `SelectionMatrix`. Resuelve `autocad` y `owner-validation`.
- Observaciones: **ninguna**. Sin pendientes funcionales.
