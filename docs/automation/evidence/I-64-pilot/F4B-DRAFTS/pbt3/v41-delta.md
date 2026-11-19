> **Redacción para el repo público:** las rutas locales se sustituyen por marcadores (`<carpeta-del-paquete>`). El resto no cambia.

# I-64 F4B PB-T3 — v41-delta: el menor de la recomprobación de la v4

**Qué es:** el cambio de la v4.1 para el único hallazgo de la recomprobación de la v4 (`independent-review-v1.md`, §12: LISTO PARA EMITIR TRAS REMEDICIÓN, B/I/M 0/0/1), con sus ids V41-xx. Todo se leyó por objetos git desde el checkout principal, en `d8ccbcb8`; el worktree no se tocó.

**Resultado:**
- **P-01 se aplica entero** (V41-01 a V41-04), con una precisión: la línea nueva de W mueve **tres** anclas del paquete, no dos;
- **un hecho nuevo, sin efecto en PB-T3:** F4B-E30 (V41-05);
- **una incidencia de procedimiento repetida,** que se devuelve al Coordinator (V41-06);
- **el diseño no cambia,** ni ningún CT, ni la letra de los contratos de la variante (A). `pbt3-measurements.md` no cambia: la remedición sobre el cierre de PB-T1 lo pondrá al día.

| Parte de P-01 | Cambio | Id | Dónde |
|---|---|---|---|
| La precondición 4, pendiente | **Cumplida:** F4B-E29, «Un solo código (Recomendado)», en `f4b-dispositions.md:800` y `owner-pbt3-d1-decision.md`, custodiados en `d8ccbcb8`. Falta que F4B-E29 entre en la `AuthorityRevision` de PB-T3b. F4B-E29 entra en §1 y en §8.1, y la cabecera ya no anuncia decisiones pendientes | V41-01 | Precondición 4; cabecera; §1; §8.1 |
| El motivo del `Skip` con `F4B-E<n>` | El motivo literal cita **`F4B-E29`**, con `<E-n>` = el equivalente de cada método (E-1 para S-1, …, E-6 para S-6) | V41-02 | §5.3 |
| Las dos variantes, todavía ofrecidas | **(A), la elegida.** §8.2 trae la respuesta literal del Owner y lo que implica, según F4B-E29. D-PBT3-09b queda como registro, y no va al contrato. En §6.2 y §6.0, lo normativo es solo de (A): los 11 del `AllowedWriteScope`, los 76 archivos del `ForbiddenWriteScope`, el `MinSelected` 6 de CT-14, los `Skip` de S-1 a S-6, PBT3B-C13 de (A) y el GREEN previsto Core + 21. Lo de (A′) pasa a una **Nota (no va al contrato)**. En §5.3, un aviso al principio marca lo que dice «con (A′)» como registro, y el régimen, las clases y CT-14 quedan solo con (A). R-T3-20 deja de aplicar; R-T3-06 y A-T3-1, con (A) | V41-03 | Cabecera; D-PBT3-01; D-PBT3-09b; D-PBT3-10; §4; §5.3; §6.0; §6.2; §7; §8.2; §10 |
| Las anclas que se movieron | En `d8ccbcb8`, el RED de PB-T1c (`e5a21989`) añadió una línea a W (`:168`, la costura inerte):<br>• W:659-704 pasa a **W:660-705**;<br>• **precisión:** W:662-670, el `catch` de esa misma adopción (D-PBT3-03), pasa a **W:663-671**;<br>• W:145-146 queda antes de la línea nueva y no cambia.<br><br>El `BodyOf` de S-5 está en **IntegrationGuard:882**, no en `:881` (ya era así en `833ea6bf`). Las demás anclas, en la remedición (precondición 2) | V41-04 | D-PBT3-03; §8.2; §9; §10 (A-T3-9) |

## Lo demás de la v4.1

- **V41-05, F4B-E30:** tras ciertos gestos de la parte compuesta de la ventana, `LastComputation.Design` va una pasada por detrás. CT-PBT3A-01 no hace gestos: compara tras `LoadExisting`, que termina con una pasada (W:440 en `d8ccbcb8`), así que no le afecta. Queda en A-T3-4, para PB-T4.
- **V41-06, una incidencia de procedimiento repetida:** una orden de la v4.1 encadenó `python -I -c "pass"` delante de una lectura con `sed`. La regla del encargo prohíbe Python con `-c`. No leyó ni escribió nada. Es el mismo modo de fallo que el heredoc de la v1 y que H-12 de PB-T1 (Python en línea, no un script escrito con la herramienta de archivos), así que **se devuelve al Coordinator como modo repetido**, sin clasificarlo por analogía.

## Lo que no cambia

- La cuenta de D-1 con (A): seis ahora y tres en PB-T4, nueve por esta decisión, con la letra estricta de E-1 a E-6.
- La pregunta y sus opciones quedan en §8.2 tal como se hicieron, como registro.
- **Siguiente paso, según la recomprobación:** la remedición sobre el cierre de PB-T1. Con ella, PB-T3a se puede emitir; PB-T3b, tras cerrar PB-T3a, con (A); y PB-T3c, tras cerrar PB-T3b.
