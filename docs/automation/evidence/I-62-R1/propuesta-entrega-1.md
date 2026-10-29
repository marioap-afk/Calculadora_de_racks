# I-62 — Propuesta ejecutiva: primera entrega funcional e incrementos (decisiones §104)

```text
Tipo:       propuesta de diseño y planificación (§104, punto 7: DESIGN AND PLANNING AUTHORIZATION = YES). No implementa, no integra,
            no enmienda el Freeze y no activa ninguna superficie. Decide el Coordinator; las líneas del Owner se piden aparte.
Base:       Freeze V14 (b64a3b64) y A-1..A-7 AGREED; F0-F4 GATE PASS; F6 suspendida con FX-01/C-23, FX-04a/C-25a y FX-05/C-27 PASS,
            FX-02/C-24 y FX-03/C-26 UNVERIFIED, FX-04b/C-25b UNSUPPORTED, FX-06/C-39 sin ejecutar; I-61 integrada y vigente.
```

## 1. Versión mínima funcional recomendada: R1 «Principal portable»

Una unidad real de RackCad puede cambiar de Principal (otra sesión u otro proveedor) a mitad de trabajo, **sin memoria privada**: la sesión nueva
reconstruye los hechos solo desde el repositorio, lo comprueba de forma mecánica y continúa. Es el criterio 9 del mandato y la única capacidad de
I-62 con operación demostrada de punta a punta (FX-04a: 23/23 contra el oráculo).

| Incluido en R1 | Excluido de R1 (sigue con I-61 o queda para un incremento) |
|---|---|
| Autoverificación del Principal al abrir: perfil, modelo y effort; `CONFIGURATION_STATUS`; UNKNOWN nunca es MATCH; STOP solo con BELOW_REQUIRED (FX-01) | Toda ejecución delegada Controller/Worker: sigue el protocolo de I-61 (ADR-0046) sin cambios |
| Custodia canónica en `state/v2`, solo en los puntos BOOTSTRAP, G0, QU, QH y Q7 de la unidad piloto, validada por `StateV2Validator` en cada punto | Bindings por capacidad, preflight de relevo I62, verificación del Controller (FX-02) |
| Liberación en QH y reanudación por otro Principal con `RESUME_DECISION` publicada antes del oráculo y comparación mecánica (FX-04a) | Orquestación autónoma de roles (§20) y FX-06; topología B (FX-03); continuación delegada (FX-04b) |
| Separación de planos: nada de R1 actúa fuera de la unidad piloto (FX-05) | Rebase dentro de ventana, REBASE_TAKEOVER, presupuestos del bucle, diario de delegación |

## 2. Componentes reutilizables (sin reconstruir nada)

- **Contratos y textos F2-F4** (GATE PASS): `state/v2` (B.8) y su validador semántico (`tests/RackCad.Tests/I62/StateV2Validator*.cs`), perfil
  y autoverificación del Principal (F1), `preflight/v1` del host, resolver de §16.13, mapa de cláusulas y guardas C-20a/C-20b/C-20c y C-28 (opt-in).
- **Guardas Core existentes** de esas superficies (`I62F4*Tests`, `PrincipalPortabilityProtocolTests`), con sus oráculos intactos.
- **Herramientas de F6:** el runner del validador (`evidence/I-62-F6/FX-U1-chain/run_validator.ps1`), los preflights de FX-01, el comparador
  mecánico y el oráculo de FX-04a, y el control de FX-05.
- **Controles mecánicos acreditados** para incrementos posteriores: instrumento y aceptante B-min (A-6), validador del cierre A7-1 (A-7).

## 3. Bloqueos normativos concretos

| # | Fuente | Bloqueo | Qué lo resuelve |
|---|---|---|---|
| B1 | Freeze §17 (F6) | F6 GATE PASS exige todos los escenarios FX; sin él no hay READY, FINAL_CANDIDATE_SHA ni integración | A-8: gate F6-R1 propio |
| B2 | Freeze §18 | OV-I62-03 (C-24) no tiene vía de limitación del Owner, a diferencia de OV-04, OV-05 (b) y OV-06; OD-7 es precondición del cierre de F6 por FX-02 | A-8: OV de R1 = OV-I62-01, -02 y -05 (a) |
| B3 | Freeze §15 | Una sola vigencia, `I62_EFFECTIVE_SHA`, para todo el plano (b): no hay activación parcial | A-8: vigencia por lista cerrada de superficies R1; el resto sigue inactivo |
| B4 | Freeze §14.0 | `state/v2` y la autoverificación solo existen dentro de I62_DELEGATED, que exige además bindings, preflight y §20 | A-8: perfil de unidad `I62_R1`, sin contratos Controller/Worker (P-15) |
| B5 | Freeze §18 (OD-1) | El ADR sucesor abarca todo el alcance, incluida la sucesión parcial de ADR-0046, antes de READY-03 | OD-1 restringida a R1; ADR-0046 sin cambios |
| B6 | Freeze §15, C-21 | Un cambio normativo posterior a MC_I62 lo invalida: nuevo cierre, fixture resembrado y pilotos afectados repetidos | A-8 fija que la ejecución compacta de OV (D.5) sobre el Candidato de R1 basta |
| B7 | Plano (a), Freeze §15 | Una unidad real no puede usar reglas I62 sin integrar como autoridad | Piloto en sombra (R1a): I-61 y WORKFLOW siguen siendo la única autoridad |

I-61 no bloquea R1 mientras R1 excluya la ejecución delegada. El proceso de integración (LIFECYCLE §8 y WORKFLOW §11.4-11.6) se aplica igual,
sobre el Candidato de R1.

## 4. Cambio mínimo de autoridad

1. **R1a, piloto en sombra, sin enmienda:** disposición del Coordinator y conformidad del Owner para la unidad piloto. Las comprobaciones de R1 solo
   añaden STOP y nunca autorizan nada que I-61 o WORKFLOW no autoricen ya.
2. **R1b, activación:** una A-8 corta (cuatro reglas: perfil `I62_R1` y exclusiones; vigencia por lista cerrada; gate F6-R1 y OV de R1; cada
   incremento con su gate y su OV, sin rebajar FX-02 ni C-24), revisada una vez por el Architect, más **OD-1-R1** del Owner.

## 5. Iniciativa piloto

Una unidad de producto nueva y pequeña, con aceptación por pruebas Core y sin validación en AutoCAD. **Candidata:** `docs/ideas-futuras.md`,
I-50 H4: `HeaderRunPlan.PlacedClone` no copia `Text`, `TextHeight`, `DimensionOffset` ni `DimensionStyleName`. Es un defecto latente, de una clase
y con prueba RED→GREEN directa. La elige el Coordinator; I-64 e I-65 quedan fuera del piloto.

## 6. Pruebas mínimas de aceptación de R1

| Id | Prueba | PASS |
|---|---|---|
| AT-1 | Autoverificación al abrir de los dos Principales, con un caso negativo de effort inferior | MATCH registrado; BELOW_REQUIRED da STOP |
| AT-2 | `state/v2` en BOOTSTRAP, G0, QU, QH y Q7 de la unidad piloto | `StateV2Validator` sin violaciones en cada punto; CI verde |
| AT-3 | A libera en QH; B, otra sesión y preferiblemente otro proveedor, reconstruye solo desde el repositorio | `RESUME_DECISION` publicada antes del oráculo e igual a él por comparación mecánica |
| AT-4 | B continúa el trabajo directo hasta el gate siguiente de la unidad | GATE PASS del Coordinator; CI verde; la unidad cierra con WORKFLOW normal |
| AT-5 | Negativos de fallo cerrado: contrato Controller/Worker bajo R1; estado corrupto o desfasado; acción fuera de la unidad | STOP P-15; STOP del validador; ningún efecto fuera de la unidad |
| AT-6 | Reversión | Con DIRECT_ONLY (vuelta a `/v1`, T21) la unidad sigue operable solo con I-61 |

**Límites de autonomía:** sin delegación, sin auto-merge, sin consumo sin línea del Owner, una reanudación por piloto; la Owner Validation de
WORKFLOW no cambia. **Reversión:** R1 es opt-in por unidad (G0); en R1b, un commit revierte la vigencia y el Coordinator puede suspender R1, con
I-61 siempre vigente. **Fallo cerrado:** UNKNOWN, violación del validador, comparación distinta del oráculo o identidad BELOW_REQUIRED: STOP y
vuelta al Coordinator, sin actuar. **Autoridad para integrar:** A-8 AGREED (Architect y Coordinator), OD-1-R1, GATE PASS de F6-R1,
OV-I62-01/-02/-05 (a) sobre el Candidato de R1 e integración según WORKFLOW §11.

## 7. Incrementos posteriores (cada uno con su gate y su OV)

| Orden | Incremento | Capacidad demostrable | Aceptación |
|---|---|---|---|
| R2 | Adopción de R1 por I-64 e I-65 | Relevo verificado entre sesiones en iniciativas activas | AT-1..AT-3 en cada una, con conformidad de su Coordinator; I-64 conserva la prioridad |
| R3 | Verificación del Controller | Topología A I62 (FX-02) | C-24 PASS y OV-I62-03; reanuda A6-7 con un runtime de Codex estable |
| R4 | Controles de seguridad de Workers | FC-01..FC-24, cierre A7-1 y B-min como fallo cerrado en cada VERIFY | Negativos mecánicos y ronda de B1 |
| R5 | Ejecución delegada más autónoma | Bucle de revisión sin el Owner como bus (§20; FX-06) | C-39 PASS y OV-I62-06 |
| R6 | Topologías | Topología B (FX-03) y continuación (FX-04b) | C-26 y C-25b, o limitación decidida por el Owner |
| R7 | Automatización adicional | Rebase y toma de custodia, presupuestos del bucle | Sus obligaciones del Anexo C |

## 8. Primera acción de implementación ejecutable

Tras aprobarse este alcance: la **disposición del Coordinator de R1a** que nombre la unidad piloto. Su primer paso ejecutable es la apertura del
piloto, con la autoverificación del Principal (AT-1) y un BOOTSTRAP `state/v2` en sombra validado con el runner existente (AT-2), en la rama de la
unidad y con CI. En paralelo, y sin kit de revisión hasta que se ordene, el borrador de A-8.
