# I-62 — A-8 candidata (borrador): activación por capacidades, primera capacidad R1 «Principal portable»

```text
Estado:     BORRADOR CANDIDATO (decisiones §105, punto 5). No aplicado, no publicado como enmienda y sin revisión formal pedida: ambas cosas
            esperan al primer resultado funcional de R1a y a una disposición nueva del Coordinator.
Alcance:    exactamente los cuatro cambios de propuesta-entrega-1.md §4.2. No rebaja C-24 ni ninguna obligación de FX-02. ADR-0046 sin cambios.
Base:       Freeze V14 (b64a3b64) con A-1..A-7 AGREED.
```

## A8-1. Perfil de aplicabilidad `I62_R1` (Proposal V14 §14.0)

Se añade una tercera aplicabilidad entre DIRECT_ONLY e I62_DELEGATED, con el valor de marcador de G0 `I62-DELEGATED-EXECUTION: I62_R1`:

| Aplicabilidad | Qué exige | Qué permite | Qué impide |
|---|---|---|---|
| **I62_R1** | `state/v2` con el arranque BOOTSTRAP → G0 → QU de §8.6; preflight y binding del Principal; custodia en BOOTSTRAP, QU, QH y QR; autoverificación del Principal al abrir | el trabajo directo de WORKFLOW, LIFECYCLE y AGENTS, y el relevo entre Principales: liberación en QH y reanudación verificada con RESUME_DECISION | contrato de gate `/v2`, delegaciones, bindings de Controller, Worker, Architect o Reviewer, y la orquestación de §20 (P-15) |

Transiciones: I62_R1 → DIRECT_ONLY por T21 (vuelta a `/v1`); I62_R1 → I62_DELEGATED por la adopción T20, solo cuando la capacidad que la
habilita sea efectiva (A8-4). La obligación sobre la sesión principal de §14.0 (último párrafo) aplica también a I62_R1.

## A8-2. Vigencia por lista cerrada (Proposal V14 §15; AUTOMATION_PLAN 16.14)

La activación deja de ser única para todo el plano (b). Cada capacidad tiene su propio commit normativo con el trailer
`Agent-Protocol-Normative: I-62-<capacidad>` y su propio punto efectivo, definido como hoy (`I62_EFFECTIVE_SHA`, con su tag `integration/I-62-<capacidad>`).
Ese punto activa **solo** las superficies de la lista cerrada de la capacidad; el resto del plano (b) sigue inactivo. Lista de R1:

- AUTOMATION_PLAN 16.14 (con esta regla), 16.15, 16.16, la parte del Principal de 16.18 y 16.19, 16.25 (`state/v2`) y 16.28 (G0);
- README de ejecución delegada §12, §13, §14.1-§14.2 solo para el Principal, §17.1, §17.4 y §17.5;
- esquemas `preflight.v1`, `binding.v1` y `automation-state.v2`; descriptor y esquema de hechos de `claude-desktop-session`; `routing.md` §8
  (PRINCIPAL_COORDINATION);
- el validador `StateV2Validator` y sus guardas Core.

Una superficie fuera de la lista citada como autoridad por una unidad I62_R1 es P-15.

## A8-3. Gate F6-R1 y Owner Validation de R1 (Proposal V14 §17 y §18)

- **F6-R1** cierra con FX-01/C-23, FX-04a/C-25a y FX-05/C-27 (PASS acreditados) y con AT-1..AT-6 del piloto R1a. No exige FX-02, FX-03, FX-04b ni FX-06.
- **Owner Validation de R1:** OV-I62-01, OV-I62-02 y OV-I62-05 (a) sobre el Candidato de R1, en ejecución compacta (D.5). Basta para C-21: un cambio
  normativo posterior a MC_I62 que solo afecte a R1 se cubre con esa ejecución compacta sobre el Candidato de R1, sin resembrar escenarios ajenos.
- **OD-1-R1:** el Owner decide un ADR sucesor limitado a R1; ADR-0046 no cambia. OD-1 completa sigue pendiente para I62_DELEGATED.

## A8-4. Incrementos con gate propio

Cada incremento posterior (R2..R7 de `propuesta-entrega-1.md` §7) se activa solo con su lista cerrada (A8-2), su gate, su Owner Validation y sus OD, y
nunca rebaja una obligación del Freeze. En particular, FX-02/C-24, OV-I62-03 y OD-7 siguen siendo obligatorios para la capacidad que habilite el
Controller (R3), y FX-06/C-39 y OV-I62-06 para la de autonomía (R5).
