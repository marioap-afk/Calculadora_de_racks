> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<checkout-principal>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# Encargo: comprobación acotada de la v2.8 de PB-T1 (las correcciones que deja PB-T1b para PB-T1c)

Eres un revisor INDEPENDIENTE: no redactaste la v2.8. Busca defectos reales y no te fíes del delta.

**Reglas:**
- Solo lectura. Sin compilar ni ejecutar pruebas.
- Python solo de lectura, escrito con la herramienta de archivos y nunca con `-c` ni con un heredoc.
- **Prohibido** leer, listar o ejecutar nada en `<worktree-de-I-64>`, y escribir esa ruta en una línea de órdenes: otro proceso trabaja allí.
- El repositorio se lee SOLO por objetos git desde el checkout principal: `git -C "<checkout-principal>" show 2f5d3dec:<ruta>`, y también `grep <rev>`, `ls-tree`, `log` y `diff`. Nunca sus archivos en disco ni órdenes git que escriban.

**Contexto:**
- PB-T1 tiene tres subtareas. PB-T1a está CERRADA. PB-T1b (el tipo `PushBackEditableState` en Application) está en su intento 2, una corrección de pruebas; su `src/` final es el de `2f5d3dec`.
- PB-T1c (la ventana guarda su estado en ese tipo, D-PBT1-13) se emitirá con la v2.8.
- Las decisiones del Owner que mueven la v2.8 están en `…\scratchpad\hs65\pbt1b\owner-pbt1b-rework-decisions.md` (F4B-E22): ratificar M10 y M14 solo para PB-T1b, y «el paquete de PB-T1c corrige el texto». También mueven la v2.8 F4B-E19 (Snapshot J) y F4B-E21 (colecciones opacas por la sesión).
- PD-V28-01 la decidió la sesión: la opción (A), parada ante una declaración nueva fuera de la lista. Comprueba que estrecha la regla y no la relaja.

## Entradas, en `…\scratchpad\hs65\`

- **El paquete:** `pbt1\I-64-f4b-pbt1-gate-contract-package-v28-draft.md`, con `pbt1\v28-delta.md` (V28-01 a V28-13) y `pbt1\v28.diff`. La base es la v2.7 custodiada: `git show 2f5d3dec:docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md`.
- **Las mediciones de anclas de PB-T1c:** `pbt1c\anchors-report.md`, la simulación `pbt1c\simulate-out.txt` y `pbt1c\sim\`.
- **La comprobación de rangos:** `pbt1\check_inv_pbt1c_03_ranges.py`.
- **La revisión y el análisis del intento 1 de PB-T1b:** `pbt1b\independent-review-code.md` y `pbt1b\analysis.md` (I01: la regla 1b de G-L1 sobre los alias; M09, M11, M12 y M15 como arrastres).
- **El generador de PB-T1c**, en borrador: `pbt1c\make_pbt1c_contract.py` y `pbt1c\generator-notes.md`. Puedes ejecutarlo solo en seco: `--dry --objects-rev 2f5d3dec`, más `--package-file <v28>` si ya lo admite. Su contrato en seco: `pbt1c\contract-dry-2f5d3dec*.json`.

## Qué haces

1. **Cada cambio V28-01 a V28-13:** ¿es correcto contra el código de `2f5d3dec` (léelo tú), contra el paquete y contra las decisiones del Owner? ¿Va más allá de lo decidido?
2. **INV-PBT1C-03 con rangos (V28-08):**
   - ¿los rangos son exactos en `2f5d3dec`, documentación incluida?
   - ¿Dejan sitio a un cambio no previsto, o impiden algo que D-PBT1-13 exige?
   - ¿Encajan con V28-07, la parada C13 ante declaraciones nuevas?
3. **CT-PBT1C-04 (V28-09):**
   - ¿las quince autopruebas son exactamente las de D-PBT1-15 más la (k)?
   - ¿La (k) discrimina la regla 1b sobre los alias?
   - ¿La G-L1 sobre W y WC queda igual de estricta que la corregida de PB-T1b?
4. **La evidencia del RED de PB-T1c (V28-01 y V28-12):** ¿es cumplible con `.github/workflows/ci.yml`? ¿Declara todo lo que fallará en el RED, incluido el filtro UI `PushBack`?
5. **Lo que falte:** ¿algún hallazgo del informe de anclas, de la revisión del intento 1 de PB-T1b o del generador (sus contradicciones C-1 a C-8) que la v2.8 no recoja?
6. **El generador frente a la v2.8:** ¿el contrato en seco coincide con el §6.3 de la v2.8?
7. **Decisiones materiales nuevas para el Owner.** Sé estricto: una decisión que el Owner ya tomó no es nueva, y una regla que solo se estrecha no es material.

## El informe

En español, en `…\scratchpad\hs65\pbt1\independent-review-pbt1c-v28-check.md`, sin rutas absolutas locales. Con:
- el veredicto arriba;
- §1, método y límites;
- §2, los cambios uno a uno;
- §3, INV-PBT1C-03;
- §4, CT-PBT1C-04;
- §5, la evidencia del RED;
- §6, lo que falta;
- §7, el generador;
- §8, los hallazgos (Id, gravedad B/I/M, contenido, evidencia y corrección);
- §9, el veredicto: EMITIBLE, EMITIBLE TRAS CORRECCIONES o NO EMITIBLE, con B/I/M y las decisiones para el Owner.

**Tu respuesta final** es una sola línea: el veredicto, el recuento, las decisiones nuevas y la ruta.
