# I-55 — Paquete de revision del Coordinador (Proposal V4)

```text
PROPOSAL V4 — NOT CONSENSUS
Coordinator      = REVIEW REQUIRED
Architect formal = PENDING (paquete independiente: I-55-architect-review-package-v4.md)
Owner            = PENDING
Consensus        = NOT REACHED
Implementation   = BLOCKED
Open Material    = M-01 (Owner) · adopcion de X-1..X-8 por I-52

Objeto de la revision  = docs/initiatives/I-55-proposal-v4.md @ fe70d7a76c77f0c0eec01f242924a7a50a2adc4c (§9)
Misma version del plan = docs/initiatives/I-55-implementation-map-v4.md · docs/adr/0042-preparacion-de-vistas-antes-de-materializar.md
Sustituye a            = docs/initiatives/I-55-coordinator-review-package-v3.md (historico)
```

> **Que es este paquete.** Una guia para que el Coordinador verifique que V4 incorpora **solo** su decision CQ-01 y lo que deriva de
> ella, y emita **su** veredicto. El ejecutor **no** emite ni simula veredictos, **no** registra decisiones del Owner y **no** declara
> consenso. Por CR-02, `Coordinator = AGREED` sigue sin ser posible mientras M-01 este abierta.

## 1. Veredicto que se solicita

```text
Coordinator: AGREED | AGREED WITH CHANGES | CHANGES REQUIRED
Hallazgos:   BLOCKER | HIGH | MEDIUM | LOW, con seccion y cambio exigido
Version:     solo para el SHA exacto revisado
M-01:        decision del Owner a transmitir (§5)
```

## 2. Trazabilidad

| Pieza | SHA |
|---|---|
| CURRENT_MAIN (preflight y re-fetch de G2E) | `dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093` |
| Proposal V3 (historico; decision CQ-01 del Coordinador) | `8e35a51058033c2876c1935afd3f3a94931748ae` |
| Paquetes V3 (historico) | `110cd39aded983df6c409bbd32d5bca05ceed0ed` |
| **Proposal V4** + mapa V4 + ADR-0042 revisado + indice de ADR + registro G2E | **`fe70d7a76c77f0c0eec01f242924a7a50a2adc4c`** (§9) |
| Paralelas | I-49 `c8cfee2` (Amendment A3-R1); I-52 `e59bc89` (Proposal V14: ubicaciones de G4..G7 provisionales, [V14-D08]; V3 registrada sin adoptar, [V14-D14]); I-56 `18401da` (G2, Proposal V1; no se aplica a I-55) |

## 3. Verificacion de CQ-01

| # | Lo exigido en G2E | Donde verificarlo | Que comprobar |
|---|---|---|---|
| CQ1-A | V4 no es un rediseno | V4 §0.3, §0.4 | Solo cambian D-15, §11, §13-§16, §17.2 (X-4, X-5, X-6), §18-§22, mapa G9..G12 y ADR-0042 decision 6; el resto es V3 |
| CQ1-B | Dos operaciones separadas | V4 §11 (introduccion), §11.1, §11.3; INV-BATCH-1 | Redibujo de hermanas: una MUTATE con rollback total. Colocacion: una transaccion por jig; Esc o Enter conservan el redibujo y lo ya colocado |
| CQ1-C | Fases | V4 §11.1 pasos 4-6; INV-RED-3; §15 | PREPARE sin escrituras del rack (solo importacion de biblioteca, como el precedente); MUTATE sin importar, purgar, regenerar, renombrar ni transacciones anidadas; POST tras el commit y sin rollback |
| CQ1-D | Envoltorios minimos por kind | V4 §11.2 (tabla); mapa G9a | Existen en Selectivo y lateral de cabecera; faltan pares de dos metodos en Dinamico (3), Push Back (3) y planta de cabecera, con la lambda de plan de su `RedrawInPlace`; unidad propia para Cantilever; escritor de borrado de huerfanas; sin tocar `SystemBlockWriter.cs`, `LateralHeaderDrawer.cs` ni `CantileverViewMaterializer.cs`; `ProjectVariableMutationExecutor` intacto (OM-26) |
| CQ1-E | D-15 e invariantes | V4 §11.1, §13, §22.1 | D-15 «redibujo atomico de hermanas antes de Insertar»; INV-AUTH-1, INV-RED-1, INV-RED-2, INV-RED-3, INV-BATCH-1 |
| CQ1-F | Huerfanas | V4 §11.1 paso 3 y justificaciones; §11.3; §14 | Con alguna hermana que redibujar, borrado dentro de la MUTATE; sin ninguna (aunque queden dependientes de xref), en la transaccion del primer jig con el jig en OK, porque borrarlas antes destruiria la unica identidad persistida si el jig se cancela; residual R-25 |
| CQ1-G | Xref y conjuntos | V4 §11.1 pasos 0 y 3; §9.2 | READ-ONLY = dependientes de xref, fuera de gates, PREPARE y MUTATE; MUTABLE = propias del dibujo (REDRAW y ERASE); gate de propiedades sobre las propias del dibujo |
| CQ1-H | Resultados | V4 §11.4; §14 | `PREPARE_FAILED`, `REDRAW_ROLLED_BACK` (con el texto exigido), `REDRAW_APPLIED`, `PLACEMENT_CANCELLED_PARTIAL_BATCH`, `PLACEMENT_FAILED_PARTIAL_BATCH`, `COMPLETED`; se conservan `VARIANT_CANCELLED` y `SIBLING_GATE_FAILED` de V3 y se anade `REDRAW_NOT_REQUIRED` cuando no hay nada que redibujar |
| CQ1-I | Ubicacion en el mapa | Mapa V4 §1, G9a, G9b | G9a = seam atomico sin cablear (RED/GREEN puro con puerto falso + guarda de fuentes); G9b = cableado en Insertar; G10..G12 dependen de G9b; sin renumerar el resto |
| CQ1-J | Pruebas y OV | Mapa V4 G9a (1-6, 9-15), G11 (7, 8), OV-RED-01..05 | Cada caso de la orden con su clase; rollback cubierto por pruebas si no puede provocarse a mano |
| CQ1-K | ADR-0042 | ADR-0042 decision 6, alternativas, consecuencias | Sigue propuesto y complementario; ADR-0010 sin modificar |
| CQ1-L | I-52 | V4 §17.2 | V14 releida; X-4 separa crear (I-52) de redefinir (existente); X-5 y X-6 con los disparadores; sin MATERIAL CONFLICT |
| CQ1-M | Revision adversarial | V4 §23 | Ataques de la orden; 3 HIGH corregidos; pase de verificacion; sin BLOCKER ni HIGH abiertos |

## 4. Precisiones derivadas que V4 fija (no son decisiones nuevas de producto)

| Precision | Por que deriva de CQ-01 o del codigo |
|---|---|
| La importacion de definiciones de biblioteca ocurre en PREPARE y no se revierte | El importador abre su propia transaccion y no puede ir dentro de MUTATE (`P/Systems/Shared/SystemBlockWriter.cs:96-101`); el precedente que nombra la orden hace lo mismo (`P/Systems/Shared/ViewBlockDraw.cs:86`) |
| Los renombres de definicion van en POST | El contrato vigente declara el nombre cosmetico (`P/Drawing/RackBlockRenamer.cs:55`); sin seam en transaccion; si hay rollback, POST no corre |
| Las dependientes de xref dejan de redibujarse en Insertar | Consecuencia directa de «Mantenerlas fuera de MUTATE» y «No intentar modificar xref» (orden G2E) |
| Actualizar conserva su redibujo vista por vista | La orden limita la atomicidad a Insertar |

## 5. M-01 — decision que el Coordinador debe llevar al Owner

Sin cambios respecto de V3 (Proposal V4 §12.7). **Recomendacion del Coordinador: todas las A. Owner = PENDING.** Consecuencia a ponderar
dentro de M-01: OM-24 (sentido del eje comun por mayoria). ADR-0042: aceptarlo solo tras consenso, M-01, OD-1..OD-8 y la reconciliacion
con I-52.

## 6. Cambios de comportamiento nuevos en V4

| Cambio | Donde |
|---|---|
| En Insertar sobre un rack existente, las hermanas cambian todas o ninguna antes de colocar ninguna vista nueva | §11.1 |
| Un fallo del redibujo ya no deja el rack redibujado en parte: no aplica ninguna actualizacion | §11.4 |
| Las hermanas dependientes de xref no se redibujan en Insertar | §11.1; OM-25 |

El resto de cambios de comportamiento es el de V3 (paquete V3 §6).

## 7. Cumplimiento de la orden G2E

| Regla | Estado |
|---|---|
| Preflight y re-fetch antes de cada commit | `main` `dad4e77`; I-52 avanzo a `e59bc89` (V14), releida sin MATERIAL CONFLICT; antes del commit, I-49 a `c8cfee2` e I-56 a `18401da`, sin cruce |
| Solo documentacion | Si |
| Proposals V1, V2 y V3 intactas | Si |
| Produccion, pruebas, HANDOFF, ROADMAP | Sin tocar |
| ADR-0042 | Solo `propuesto`; ADR-0010 sin modificar |
| Decisiones del Owner o consenso declarados | No |

## 8. Lo que queda para `Coordinator = AGREED`

Solo razones externas: M-01 (Owner), la adopcion de X-1..X-8 por I-52 (necesaria para el freeze) y el veredicto del Arquitecto
independiente (necesario para el consenso).

## 9. SHA revisado

```text
PROPOSAL_V4_SHA = fe70d7a76c77f0c0eec01f242924a7a50a2adc4c   (CI push 34889818061: success 4/4)
```
