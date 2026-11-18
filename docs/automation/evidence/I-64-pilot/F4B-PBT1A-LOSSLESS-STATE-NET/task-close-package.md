# Paquete de cierre — F4B-PBT1A-LOSSLESS-STATE-NET (PB-T1a)

**Resultado:** **CLOSED / PASS WITH LIMITATIONS** sobre `73d30982b0c0ce2b8c9213e626df0ab0d50b0475`, con attempts **1 de 3**. El producto no cambia: `git diff a3b63ccc 73d30982 -- src` está vacío.

**Autoridades:**
- el contrato `F4B-PBT1A-LOSSLESS-STATE-NET/coordinator/gate-contract.json` (AuthorityRevision `95c8126b`; BaseSha = ChainBaseSha `a3b63ccc`);
- el paquete `docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md` v2.4;
- las decisiones del Owner F4B-E09 (`owner-pbt1-pre-decisions.md`), F4B-E11 (`owner-d1-d2-decisions.md`, D2) y F4B-E14 (`owner-pbt1a-decisions.md`).

## 1. La cadena

| Paso | Corrida | Resultado |
|---|---|---|
| Planificación | `R20261118T141628Z-d049` | En el primer lanzamiento: fidelidad PASS en las 8 colecciones y A1-A8 8/8; 10 AcceptanceCriteria del Controller. La replanificación de PBT1A-C08 queda sin usar |
| nc4 | `R20261118T142102Z-7228` | Oráculo cumplido: solo A3 en fail |
| Worker | `R20261118T142230Z-d31d` | IMPLEMENTATION_COMPLETE, sin paradas. Paso 1 con las corridas p1 a p4; base 14/14 en verde (`b1`); los quince mutantes aislados (`m01` a `m14`, con `m07a` y `m07b`), cada uno hace fallar el CT de su fila; RED `15f1f225` (los siete archivos nuevos y los quince mutantes, exactamente los de la tabla); GREEN `73d30982` (`git restore --source=BaseSha` de W y WC) |
| CI del RED | `44333116533` | ui-tests en failure; tests, build-ui y build-plugin en success. `PushBackStateNet` 14/14 en fallo por aserción; 42 pruebas existentes fallan, todas atribuidas a un mutante (T-8); Core 19 070/19 070 |
| CI del GREEN | `44338475965` | 4/4. UI 2 415 Passed y 21 Skipped de 2 436; Core 19 070. Son **exactamente** las cifras del §6.1 |
| Revisión directa | — | Conforme en alcance, mutantes, discriminación, RED, GREEN, CI y contención; D-1 y la lista de INV-PBT1A-13, al Owner |
| Revisión adversarial independiente | — | **REWORK 0/1/9**. I-01 = CT-02 rediseñado para que MU-T1A-02 discrimine. Con la ratificación del Owner (F4B-E14, pregunta 1) pasa a **ACEPTABLE CON MENORES 0/0/9** |
| Vuelta al Owner | `F4B-DRAFTS/pbt1a/owner-return-pbt1a.md` | **F4B-E14:** «Ratificar (Recomendado)» CT-02; c-09, «Como hoy, regla explícita (Recomendado)»; los demás, «Tal cual, anotar mejoras (Recomendado)» |
| Verificación del Controller | `R20261118T163922Z-06b0` | `EXECUTION_VERIFIED` / NONE, 14/14, con la ratificación de CT-02 como disposición de entrada |
| nc1, nc2 y nc3, primera ronda | `R20261118T164355Z-8b4e`, `R20261118T164820Z-996a` y `R20261118T165356Z-bdb7` | Los tres **CONTROL NOT PASSED**. nc1 detecta el SHA falso (Handoff primero e Identity, STOP S-04), pero la evidencia de Identity no lo nombra: no es la firma exacta de PD-F4-04. nc2 **no detecta** el `AllowedWriteScope` estrechado (Scope en pass). nc3 **no detecta** « GATE PASS.» al final de `WorkCompleted`: no ejecutó ninguna búsqueda. Son falsos negativos del verificador con una entrega grande; la verificación real sí buscó por script, y la sesión lo confirmó de forma determinista |
| Vuelta al Owner | `F4B-DRAFTS/pbt1a/owner-return-controls.md` | **F4B-E15: «Reforzar y repetir (Recomendado)»**. El prompt de la verificación gana tres comprobaciones deterministas: FreeText por script sobre los siete campos completos, Scope contado por script contra la delegación, e Identity con los dos SHA. Vale también para PB-T1b y PB-T1c |
| Verificación reforzada | `R20261118T171552Z-b71b` | `EXECUTION_VERIFIED` / NONE, 14/14. Las tres comprobaciones, hechas por script (0 términos en los siete campos; 9 entradas de la delegación cubren el diff; los tres SHA escritos) |
| nc1 | `R20261118T171929Z-3203` | **CONTROL PASS:** Identity primero (nombra `deadbeef…` y el HEAD), y Remote y Ci como consumidores del `CurrentSha` falso; `EXECUTION_BLOCKED` / STOP con S-04; oráculo cumplido, sin sustituto |
| nc2 | `R20261118T172756Z-77ec` | **CONTROL PASS:** Scope en fail por la ruta quitada; `EXECUTION_BLOCKED` / STOP; oráculo cumplido, sin sustituto |
| nc3 | `R20261118T173153Z-7969` | **CONTROL PASS:** FreeText en fail (el término, en el carácter 14 793 de `WorkCompleted`); REWORK; oráculo cumplido |

**Relevos:**
- 0 participantes en toda la cadena (salvo la primera lectura de salida de la planificación, que contó dos procesos bash de la propia sesión y que la relectura dio en 0);
- la Snapshot I y el binario acreditado;
- `main` = `812baf87`, sin tocar.

## 2. Lo que fija PB-T1a

Catorce CT nuevos en `tests/RackCad.UI.Tests/PushBackStateNet*.cs`, sobre la salida persistida de la ventana clásica:
- los huecos de D-5;
- M-03 de PB-TC y de PB-TC2;
- M-04;
- M-07 (INCUMPLIMIENTO CARACTERIZADO);
- la observación de CT-16;
- M-02 de PB-TC;
- D-3;
- las 105 huellas del corpus extendido por los guiones G-1 a G-6, con F-03 y su autocomprobación (`pasadas-restaurar=2` y `frontal=A`).

Es la red con la que se miden PB-T1b y PB-T1c.

## 3. Desviaciones, límites y arrastres

| Id | Qué | Va a |
|---|---|---|
| I-01 / D-1 | CT-02 ganó pasos (`DepthBox=54` y «+ frente») en la medición, para que MU-T1A-02 discrimine; sigue fijando su fila | **Ratificado por el Owner solo para este caso** (F4B-E14). La regla SIN LATITUD y PBT1A-C04 no cambian |
| M-05 | CT-03 pasa a una ventana por caja (la cadena dejaba el diseño inválido); la fila se cumple | Anotado |
| M-06 / D-2 | Las corridas `t01` a `t14` corrieron en local 19 pruebas de UI fuera de los filtros del contrato (INV-PBT1A-10); limpieza del perfil idéntica | Desviación declarada, sin efecto |
| M-01 | G-2 de CT-14 tiene una huella de respaldo que tapa un «Actualizar» inerte, y no fija que la ventana se cierre | Lo cubren otras pruebas; se anota para una reemisión |
| M-02 / OWNER-5 | F-02 da las mismas huellas que `pb-20-ab` (el lado frontal no se guarda) | `FrontalSideBox` queda cubierto por F-03 y por CT-PBT1C-02 |
| M-03, M-04, M-07, M-08 | `BuildX01` no es copia literal (reabre un diseño deserializado); sin el `finally` interno de D-PBT1-12; `Command` no siempre es la orden exacta; la transcripción previa al RED solo se demuestra por `p4.trx` | Higiene de evidencia; anotados |
| F4B-E15 | Los controles de la primera ronda no pasaron por falsos negativos del verificador | Refuerzo determinista del prompt de la verificación, también para PB-T1b y PB-T1c |
| M-09 | La lista de `UnexpectedFindings`: `AbsentSlotsB=[0]` de `pb-01-base` no es un hallazgo; c-09 era más grave de lo declarado | Corregido en la vuelta al Owner |
| OWNER-2 | c-09 al reconstruir (1000 ton → 1000 kg) | **PB-T1b**, como regla explícita con su CT (F4B-E14, pregunta 2) |
| OWNER-3, OWNER-4 | «lb» y `PalletsDeepBox` no llegan solos a la salida; aceptar Seguridad sin cambios añade el portador de botas | Tal cual en PB-T1; anotados en `docs/ideas-futuras.md` (F4B-E14, pregunta 3) |
| A-6 | M-07, la caja de c-02 que se ve vacía y M-01 de PB-T0 | Anotados en `docs/ideas-futuras.md` en este cierre |

**Siguiente:** los anclajes de PB-T1b sobre este cierre y su generador (con la regla de c-09 que decidió el Owner), su comprobación acotada y su emisión. La precondición 5 queda satisfecha por F4B-E14.
