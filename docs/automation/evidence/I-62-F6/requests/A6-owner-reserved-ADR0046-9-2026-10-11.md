# I-62 — Paquete OWNER-RESERVED: ADR-0046 #9 frente al instrumento B-min en FX-02/F6 (preparado; decisiones §76, punto 4)

> Supervisión, plano a. **Preparación: no se pide nada todavía y no hay ninguna excepción vigente.** El Coordinator no sustituye esta decisión
> (§76.4). ADR-0046 no se modifica. La decisión es del Owner, con las autoridades normativas que correspondan. Se presenta junto con A-6 (blob
> `eac473c0d2e58cf1c4594f48a8816b3833af6509`, PROPUESTA), que no va a acuerdo mientras esta consecuencia esté sin disponer (§76.5).

## 1. Qué se decide

Si **ADR-0046 #9** («**Nivel A:** documentación, esquemas y piloto real. Sin plataforma ni scripts operativos hasta que la evidencia lo justifique»;
ADR-0046 L69, aceptado por el Owner, L3; conservado por ADR-0048, L40) admite una **excepción limitada**, sobre la base de la evidencia de F6, para
este uso:

- **solo** el EXECUTION_CONTROLLER de FX-02, en F6, Topología A;
- **solo** el instrumento B-min fijado por blob: herramienta de hechos `bmin_facts.py` `9a829b0eeb1210238052b1feac06cfa8f7964aed` y aceptante independiente `bmin_accept.py`
  `434ef1fccb0d15762b30244b4a60928120698411`, ejecutados desde el objeto Git con `python -I`, sin escrituras ni red;
- **nunca** en producción, en otras unidades, en otros roles ni en D.5 (Owner Validation), y sin cambiar la receta 16.4, el adapter `codex-cli`, los
  esquemas, los presupuestos ni la autoridad del Controller (§76.2).

**No decide** (son decisiones aparte):
- el consumo de una medición nueva de `codex-cli` (línea propia, §3; §76.6);
- el acuerdo de A-6, que es del Architect y del Coordinator después de esta decisión;
- la creación del repositorio B1 y la sesión de FX-U2 (líneas de A-5 §7);
- el resultado de FX-02 o C-24.

## 2. Por qué es OWNER-RESERVED

- ADR-0046 es una decisión aceptada por el Owner (L3); ADR-0048 solo la acepta el Owner y conserva #9 (L40).
- LIFECYCLE §3 reserva al Owner el cambio de una decisión suya; AUT-I1 (V14 L1039-L1047) prohíbe que el Principal la elija.
- #9 no atribuye a nadie la determinación de que «la evidencia lo justifica». Ante la duda, rige el arquetipo superior.
- Así lo concluyó el dictamen independiente acreditado: R20261011T162328Z-5c5e, `OwnerReserved[0]`; decisiones §76.1.

## 3. Evidencia disponible (hechos; ninguna conclusión de elegibilidad)

| Hecho | Fuente |
|---|---|
| El Controller `codex-cli` sin instrumento no demostró PLAN ni VERIFY en tres mediciones: A4-1 (`cmd.exe`) y las dos sondas A2-P2 (`pwsh` nativo). En la última hizo todos los pasos, pero falló en copias, formatos y vínculos | evidencia §120, §128, §140; decisiones §66, §70, §74.1 |
| B-min: herramienta 66/66 pruebas y 27/27 mutantes; aceptante independiente 52/52 | evidencia §146 |
| Extremo a extremo en un clon del fixture: ACCEPT_FACTS; cero escrituras; salidas alteradas rechazadas; los 13 mutantes que cambian una salida, rechazados | evidencia §146 |
| La divergencia MV-4/MV-7 queda explícita en hechos con su ancla, sin tocar las sondas | evidencia §143, §146 |
| Dictamen: MATERIAL (M-02, M-03, M-04, M-05, M-08); A-6 necesaria; dos decisiones OWNER-RESERVED | evidencia §147; decisiones §76.1 |
| No hay evidencia todavía de un Controller **con** el instrumento: ninguna sonda lo midió | evidencia §147 (observaciones) |

## 4. Opciones

| Opción | Efecto | Consecuencia para C-24 |
|---|---|---|
| **A** — excepción limitada, en el alcance del §1 | autoriza solo la base de ADR-0046 #9 para que A-6 pueda ir a revisión formal y a acuerdo; no autoriza la medición ni la ejecución | permite intentar la ruta B-min: A-6 acordada, luego la medición (línea aparte) y la ronda de FX-02 en B1/FX-U2 |
| **B** — no admitir la excepción | el Nivel A sigue sin excepción; A-6 no puede acordarse con el punto 8 | C-24 solo por la ruta A (Controller actual, tras una actualización observada de Codex y un bloque nuevo) o C (otro modelo o transporte, con su A-n); F6 GATE PASS sigue imposible mientras C-24 no sea PASS |
| **C** — aplazar | ningún cambio | como B mientras dure el aplazamiento |

## 5. Línea del Owner (texto exacto de A-6 §7 (a); no se pide hasta que el Coordinator lo disponga)

```text
A6-ADR0046-9-FX02 = A (determinación OWNER-RESERVED, previa al acuerdo de A-6, de que la evidencia de F6 (paquete B-min del commit 9356edf141c795ea718a360e266334ded10d6a76 y dictamen R20261011T162328Z-5c5e, ACCREDITED) justifica frente a ADR-0046 #9 (Nivel A), conservado por ADR-0048, una excepción limitada: el EXECUTION_CONTROLLER de FX-02 en F6, Topología A, puede usar en su verificación solo el instrumento de hechos de blob 9a829b0eeb1210238052b1feac06cfa8f7964aed, y la supervisión, del lado del Coordinator, solo el aceptante de blob 434ef1fccb0d15762b30244b4a60928120698411; excluye la producción, cualquier otra unidad, escenario, rol o topología, D.5 y la Owner Validation; no modifica ADR-0046 ni ADR-0048 ni crea precedente; no autoriza ninguna invocación ni consumo ni acepta ningún resultado; solo tiene efecto con A-6 AGREED y la disposición del Coordinator que la cite)
```

SHA-256 de la línea: `83152eae4f451961b9dc43920aa60c6f0910d4d023e9e93d8f5d6160e7ac8b14`.

## 6. Secuencia, si la opción es A

1. Línea del Owner (§5).
2. Revisión formal de A-6 por un Architect independiente; acuerdo del Coordinator.
3. Disposición de medición del Coordinator y línea de consumo del Owner (solicitud aparte).
4. Una medición.
5. Binding del Controller, si es elegible.
6. B1 (línea A5-FIXTURE-B1-REPO) y FX-U2 (línea A5-PRINCIPAL-FXU2-CONSUMO).
7. FX-02, con el oráculo de C-24 sellado y sin entregarlo al Controller.
