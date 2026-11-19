> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<checkout-principal>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# Encargo: revisión adversarial INDEPENDIENTE del paquete v1 del contrato de PB-T2 (I-64 F4B: aplicar, cerrar y escribir separados; los dos modos)

Eres un revisor independiente: no redactaste el paquete. Busca defectos reales: diseño que no cumple al Owner, hechos mal medidos, pruebas que no discriminan, guardas que se romperían sin declararlo, contratos imposibles de cumplir y decisiones del Owner escondidas, inventadas o mal planteadas. No te fíes del resumen del redactor.

**Reglas:**
- Solo lectura. Sin compilar ni ejecutar pruebas.
- Python solo de lectura, escrito con la herramienta de archivos y nunca con `-c` ni con un heredoc.
- **Prohibido** leer, listar o ejecutar nada en `<worktree-de-I-64>`, y escribir esa ruta en una línea de órdenes: otro proceso trabaja allí.
- El repositorio se lee SOLO por objetos git desde el checkout principal: `git -C "<checkout-principal>" show 2f5d3dec:<ruta>`, y también `grep <rev>`, `ls-tree`, `log` y `diff`. Nunca sus archivos en disco ni órdenes git que escriban.

## Entradas (base `<scratchpad>\hs65\`)

- **El paquete:** `pbt2\I-64-f4b-pbt2-gate-contract-package-v1.md`, con su registro `pbt2\pbt2-measurements.md` y los scripts de `pbt2\`.
- **El encargo que recibió el redactor:** `pbt2\drafting-brief.md`, con las fuentes y lo que pide el Owner.
- **Las decisiones del Owner:**
  - [OD], fila de PB-T2: `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-disposition.md`, en el repo;
  - D-PB-07, «BOM y biblioteca»: `pbfull\owner-f4b-dpb07-extensiondata-decisions.md`, y el expediente `pbfull\owner-return-dpb07.md`;
  - D2: `owner-d1-d2-decisions.md`, en el repo;
  - las demás, en `F4B-DRAFTS/f4b-dispositions.md` del repo.
- **El paquete de la fase:** `docs/initiatives/I-64-f4b-push-back-completo-package.md` v3.1, en el repo.
- **El paquete de PB-T1:** la v2.8 en borrador, `pbt1\I-64-f4b-pbt1-gate-contract-package-v28-draft.md`, sobre todo el diseño de PB-T1c, D-PBT1-13, sobre el que se apoya PB-T2. Las anclas de PB-T1c están en `pbt1c\anchors-report.md` y su simulación, en `pbt1c\sim\`.

## Qué revisas

1. **El Owner.** ¿El diseño cumple la fila de PB-T2 del [OD]?
   - Separar aplicar, cerrar y escribir.
   - WORKSPACE devuelve el estado a DraftSession.
   - El clásico sigue con su flujo compatible.
   - Una sola validación de campos.

   ¿Cumple el modo explícito y la guarda «sin camino clásico» del §4 de la fase? ¿Cumple OB-01.x, OB-06.3 (el paso 4 de D-PB-06) y D-PB-07, con sus obligaciones (dos costuras y la prueba UI-STA con GUID e InsertRequested)? ¿Respeta D2?
2. **Los hechos.** Comprueba tú mismo en `2f5d3dec` una muestra amplia de anclas: todas las que sostienen el diseño (la ventana, la sesión, el piloto Workspace, DraftSession y el panel) y las de las guardas de T1B y de texto (S-7, S-9, CT-22, CT-23, G-10, I-55). Para cada DIFF, da la cita y lo que hay. ¿Es cierta la afirmación de que todas las guardas de T1B siguen en verde sin sustituir ninguna?
3. **La división en PB-T2a y PB-T2b.** ¿Cada una se puede entregar? ¿Las dependencias con PB-T1c son correctas? ¿El plan B (`CopyInto` en PB-T2a, si PB-T1c no permite reasignar `editable`) es viable sin tocar lo que PB-T1c fija?
4. **Las pruebas** (los 17 CT).
   - ¿Discriminan o son tautológicas?
   - ¿El RED de cada subtarea se puede cumplir con `.github/workflows/ci.yml`? `build-plugin` necesita `tests` y `build-ui` y queda omitido si Core falla (lección M14 de PB-T1b).
   - ¿La UI-STA pulsa de verdad todos los gestos visibles?
   - ¿Hay ventanas o modales vivos, o dependencias de AutoCAD?
5. **Las preguntas al Owner, Q-PBT2-1 a 4.**
   - ¿Son decisiones materiales de verdad?
   - ¿Están bien planteadas, con hechos y opciones reales?
   - ¿Alguna ya la decidió el Owner, en el [OD] o en D-PB-07?
   - ¿Falta alguna? Por ejemplo: los 11 literales nuevos visibles para el usuario; la normalización N-PBT2-01; que el dueño de los subdiálogos sea la ventana en los dos modos.
6. **Los contratos propuestos.**
   - ¿El AllowedWriteScope, el ForbiddenWriteScope, los RequiredTests, los invariantes y las paradas son mecánicos y se pueden cumplir?
   - ¿Las listas cerradas de miembros admiten los ayudantes privados necesarios o declaran la parada (lección M10 de PB-T1b)?
   - ¿El tamaño cabe con F4B-E21?
   - ¿Se ajustan a AUTOMATION_PLAN §16?

## El informe

En español, en `…\scratchpad\hs65\pbt2\independent-review-v1.md`, sin rutas locales absolutas. Lleva:
- el veredicto arriba;
- §1, método y límites;
- §2, el Owner;
- §3, los hechos (tabla de anclas comprobadas);
- §4, la división;
- §5, las pruebas;
- §6, las preguntas al Owner;
- §7, los contratos;
- §8, los hallazgos (Id, gravedad BLOQUEANTE, IMPORTANTE o MENOR, contenido, evidencia y corrección);
- §9, el veredicto: LISTO PARA EMITIR TRAS REMEDICIÓN, NECESITA v2 o NO VIABLE, con B/I/M y las decisiones para el Owner.

**Tu respuesta final** es una línea: el veredicto, el recuento B/I/M, las decisiones para el Owner y la ruta.
