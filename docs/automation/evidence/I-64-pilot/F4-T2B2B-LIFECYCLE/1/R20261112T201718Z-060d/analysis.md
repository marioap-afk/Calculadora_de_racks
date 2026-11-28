# F4-T2B2B-LIFECYCLE, intento 1: análisis de las revisiones y disposición

**Entrega:**
- Worker `R20261112T183825Z-36c7`: RED `a2e0ec13`, GREEN `8f249e3a`.
- CI `43294223849` 4/4.
- RED `43287541049`: Core en rojo con 128 fallos, todos dentro de `RackCad.Tests.Workspace`.

**Verificación del Controller** `R20261112T201718Z-060d`: `EXECUTION_VERIFIED`, 14/14, `VerifiedSha` `8f249e3a`. Sin términos de 16.10.

## Revisión directa de la sesión

**Producto conforme.** Sin defectos de corrección frente a INV-F4T2B2B-01..09 ni frente a la tabla de veto del Owner (F4-E29). El informe completo está en `direct-review.md`, junto a este archivo.

Dos menores en las guardas nuevas:
- D-1: conteo exacto de los archivos de `Drafts/`.
- D-2: conteo exacto de `_knownClean.Remove` en todo el host; coincide con M-2.1.

## Revisión adversarial independiente

**Veredicto: ACEPTABLE CON MENORES** (0 bloqueantes, 0 importantes, 5 menores). El producto no tiene defectos de corrección y la tabla de veto se cumple en sus cinco filas. El informe completo está en `independent-review.md`.

| Id | Severidad | Hallazgo | Tipo |
|---|---|---|---|
| M-1 | MENOR (regla I-1) | La guarda del controlador exige el texto `_bridge.SetLifecycleAdvisor(_host);`, y la del puente el texto exacto del reenvío. Impiden que F4-T3 envuelva el asesor para vaciar el texto vivo antes del cierre, algo que INV-F4T2B2B-11 y D-T2B2-10 permiten expresamente | Pruebas |
| M-2 | MENOR (regla I-1) | Conteos y conjuntos exactos de archivo entero donde bastaba una cota o el cuerpo propio: `_knownClean.Remove` = 9 y `.DiscardAndReload(` = 1 en todo el host; la interfaz del asesor con exactamente dos métodos; la cola de avisos fijada como `internal` | Pruebas |
| M-3 | MENOR | `AnInPlaceEditAfterTheAllowThatIsNotNotedIsTheExactResidualOfTheDesign…` afirma `"silent"` para el residual declarado: lo convierte en contrato | Pruebas |
| M-4 | MENOR | La documentación compartida de `EditIntentStatus.Waiting` («nothing was read») y `NotAdmitted` («no draft exists») es falsa para la recarga | Documentación |
| M-5 | MENOR | El NotAdmitted de la recarga no afirma `Detail` ni `Witness`. La prueba de la excepción del descarte no comprueba la no degradación que dice su nombre | Pruebas |

**Riesgos para Smoke-2** (no son defectos):
- el orden real entre cierre y destrucción en CLOSE, CLOSEALL, QUIT, la pestaña y la API;
- el cierre cancelado;
- un `e.Veto()` que lanza o se ignora;
- el documento vetado no activo;
- `Stop()`;
- el fallo al enganchar `BeginDocumentClose`;
- la reentrada durante la lectura de la recarga;
- el coste;
- la vía de retirada sin rastro.

## Disposición

1. **El modo de fallo se repite.** La regla I-1 ya produjo el IMPORTANTE de F4-T2B2A (intento 1) y el N-1 de su re-revisión, y reaparece con severidad menor. La sesión lo devolvió al Owner antes de clasificarlo por analogía (`owner-f4t2b2b-close-decision.md`, junto a este archivo).
2. **Decisión del Owner:** «Cerrar y arrastrar a T3 (Recomendado)». Implica:
   - completar nc1..nc3;
   - cerrar F4-T2B2B sobre `8f249e3a` como PASS WITH LIMITATIONS, sin gastar el intento 2;
   - registrar como desviación la contradicción de M-1 con INV-F4T2B2B-11.
3. **Arrastres a F4-T3**, como cambios declarados de su contrato:
   - aflojar las guardas de M-1 y M-2 (1 a 4) y D-1 a orden, cota o cuerpo propio;
   - en M-3, afirmar solo la vía tomada;
   - documentar `Waiting` y `NotAdmitted` para la recarga (M-4);
   - cubrir `Detail` y `Witness` del NotAdmitted de la recarga y la no degradación del descarte (M-5).
4. **Arrastres a Smoke-2:** los riesgos de arriba, más la regla de devolución al Owner de D-T2B2-10 si la secuencia nunca queda intacta o hay falsos inciertos.
