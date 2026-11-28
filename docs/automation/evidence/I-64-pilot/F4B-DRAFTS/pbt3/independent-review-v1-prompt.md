> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<checkout-principal>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# Encargo: revisión adversarial INDEPENDIENTE del paquete v1 del contrato de PB-T3 (I-64 F4B, la escritura completa del Workspace)

Eres un revisor independiente: no redactaste el paquete. Busca defectos reales: diseño que no cumple al Owner, hechos mal medidos, pruebas que no discriminan, guardas que se romperían sin declararlo, contratos imposibles de cumplir y decisiones del Owner escondidas o inventadas. No te fíes del resumen del redactor.

**Reglas:**
- Solo lectura. Sin compilar ni ejecutar pruebas.
- Python solo de lectura, escrito con la herramienta de archivos y nunca con `-c` ni con un heredoc.
- **Prohibido** leer, listar o ejecutar nada en `<worktree-de-I-64>`, y escribir esa ruta en una línea de órdenes: otro proceso trabaja allí.
- El repositorio se lee SOLO por objetos git desde el checkout principal: `git -C "<checkout-principal>" show 2f5d3dec:<ruta>`, y también `grep <rev>`, `ls-tree`, `log` y `diff`. Nunca sus archivos en disco ni órdenes git que escriban.

## Entradas (base `<scratchpad>\hs65\`)

- **El paquete:** `pbt3\I-64-f4b-pbt3-gate-contract-package-v1.md`, con su registro `pbt3\pbt3-measurements.md` y los scripts de `pbt3\`.
- **El encargo que recibió el redactor:** `pbt3\drafting-brief.md`, con las fuentes y lo que pide el Owner.
- **Las decisiones del Owner:**
  - [OD], la fila de PB-T3, en el repo: `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-disposition.md`;
  - la ExtensionData anidada, «Basta así»: `pbfull\owner-f4b-dpb07-extensiondata-decisions.md`;
  - D2: `owner-d1-d2-decisions.md`, en el repo;
  - las demás, en `F4B-DRAFTS/f4b-dispositions.md` del repo.
- **El paquete de la fase:** `docs/initiatives/I-64-f4b-push-back-completo-package.md` v3.1, en el repo.
- **El paquete de PB-T1:** la v2.8 en borrador, `pbt1\I-64-f4b-pbt1-gate-contract-package-v28-draft.md`. El código de PB-T1b está en `2f5d3dec`.

## Qué revisas

1. **El Owner:** ¿el diseño cumple cada punto de la fila de PB-T3 del [OD]?
   - campos; módulos y frentes; lado B; renombrar;
   - borrar vistas obsoletas;
   - bloquear la escritura por diagnósticos;
   - no crear vistas (bloque M);
   - PREPARE → MUTATE → POST y un solo «Actualizar» lógico.

   ¿Respeta D-PB-05 (la composición sin conectar, OB-05.2) y la decisión de la ExtensionData (AUTH-13 fijado por prueba)? ¿Respeta D2 y su límite?
2. **Los hechos:** comprueba en `2f5d3dec`, tú mismo, una muestra amplia de anclas. Como mínimo, todas las del §1 que sostienen el diseño y todas las de las guardas de F4 de D-1. Para cada DIFF, da la cita y lo que hay.
3. **La división en PB-T3a, b y c:**
   - ¿cada una es entregable por un Worker, con su RED, su GREEN y su alcance?
   - ¿Las dependencias son correctas?
   - ¿PB-T3a de verdad no rompe ninguna guarda?
   - ¿La fusión de b y c que menciona el redactor es segura?
4. **Las pruebas** (los 33 CT):
   - ¿discriminan, o son tautológicas?
   - ¿El RED de cada subtarea es cumplible con `.github/workflows/ci.yml`? Recuerda que `build-plugin` necesita `tests` y `build-ui`, y queda omitido si Core falla. Es la lección M14 de PB-T1b.
   - Los `Skip` de PB-T3b: ¿son admisibles?
   - ¿Qué cubre de verdad lo que no se puede probar sin AutoCAD?
5. **D-1:** ¿la lista de seis métodos es exacta y completa? Busca tú las guardas de F4 que fijan por texto el escritor o el piloto, con las clases y los métodos. ¿Falta alguna? ¿Sobra alguna? ¿Los equivalentes conservan la seguridad que protegían? ¿D-2 es de verdad solo informativa?
6. **Los contratos propuestos:**
   - ¿los AllowedWriteScope, ForbiddenWriteScope, RequiredTests, invariantes y paradas son mecánicos y cumplibles?
   - ¿Las listas cerradas de miembros admiten los ayudantes privados que hacen falta, o declaran la parada? Es la lección M10 de PB-T1b.
   - ¿El tamaño cabe con F4B-E21?
   - ¿Cumple AUTOMATION_PLAN §16?
7. **Decisiones materiales para el Owner:** ¿hay alguna que el paquete no declara? ¿Alguna de las declaradas no es material?

## El informe

En español, en `…\scratchpad\hs65\pbt3\independent-review-v1.md`, sin rutas locales absolutas. Con:
- el veredicto arriba;
- §1, método y límites;
- §2, el Owner;
- §3, los hechos (tabla de anclas comprobadas);
- §4, la división;
- §5, las pruebas;
- §6, D-1 y D-2;
- §7, los contratos;
- §8, los hallazgos (Id, gravedad BLOQUEANTE, IMPORTANTE o MENOR, contenido, evidencia y corrección);
- §9, el veredicto: LISTO PARA EMITIR TRAS REMEDICIÓN, NECESITA v2 o NO VIABLE, con B/I/M y las decisiones para el Owner.

**Tu respuesta final** es una línea: el veredicto, el recuento B/I/M, las decisiones para el Owner y la ruta.
