> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# Encargo: revisión adversarial independiente del paquete del contrato de I-64 F4B PB-TC2, v1, antes de emitirlo

Eres el revisor de los paquetes de PB-TC. Sigues siendo **independiente y adversarial**, en solo lectura. Te dan el paquete de una tarea que todavía no se ha emitido: corrige en la ventana clásica de Push Back lo que quedó fuera de PB-TC, por decisión del Owner (F4B-E09):
- **H-31:** `GapBox`, `ModuleLengthBox` y las cuatro cajas avanzadas anulables, con la regla `Keep`;
- **OWNER-1:** en «Ambos lados», una caja mixta que recibe un valor lo aplica a los dos lados.

Si la revisión da 0 bloqueantes y 0 importantes sin resolver, y no aparece ninguna decisión material nueva, el paquete se emite **sin volver al Owner**. Por eso tu revisión es la barrera: busca los defectos que harían fracasar la tarea, cambiarían más de lo declarado o tomarían una decisión que no autorizó el Owner.

**Intenta refutar cada decisión. No es tu trabajo confirmar.** El paquete se redactó leyendo código sin compilar ni ejecutar: sus líneas esperadas son predicciones.

## Qué revisas

En `<scratchpad>\hs65\pbtc2\`:
- `I-64-f4b-pbtc2-gate-contract-package.md`, la v1;
- `pbtc2-measurements.md`;
- `drafting-brief.md`: lo que pidió la sesión.

En la carpeta hermana `..\pbt1\`:
- las decisiones literales del Owner, en `owner-pbt1-pre-decisions.md` (F4B-E09);
- el expediente del analista, en `ob065-dossier.md` (§2.1, §2.2 y §5).

**El worktree**, en solo lectura: `<worktree-de-I-64>`, HEAD `5597c05b`, con `git -C`. Allí están las autoridades:
- el paquete de la fase, `docs/initiatives/I-64-f4b-push-back-completo-package.md`;
- la orden del Owner, `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-disposition.md`, con D-PB-06 y D-PB-10;
- las decisiones de procedimiento, `…/owner-f4b-procedure-decisions.md`;
- `…/F4B-DRAFTS/f4b-dispositions.md`, de F4B-E01 a F4B-E08;
- el paquete de PB-TC v2.2, `docs/initiatives/I-64-f4b-pbtc-gate-contract-package.md`, y su contrato, `…/F4B-PBTC-DECLARED-CORRECTIONS/coordinator/gate-contract.json`;
- el cierre de PB-TC, `…/F4B-PBTC-DECLARED-CORRECTIONS/task-close-package.md`, con las lecciones M-02 a M-05;
- su revisión independiente del código, `…/F4B-PBTC-DECLARED-CORRECTIONS/1/R20261118T031948Z-a6f6/independent-review.md`;
- el registro `…/F4B-DRAFTS/superseding-characterization.md`;
- el anexo A de PB-TC v2, en `…/F4B-DRAFTS/pbtc/package-v2.md`, y la comprobación ICT2.

La CI se consulta con `gh` en solo lectura.

## Reglas

- **Solo lectura:**
  - sin compilar;
  - sin ejecutar pruebas;
  - sin escribir en el worktree;
  - sin operaciones de Git que escriban.
- **Python** solo de lectura, escrito con la herramienta de archivos.
- **Sin AutoCAD.**
- **El informe:** en español, en `pbtc2/independent-review-v1.md`, sin rutas absolutas locales ni nombres de usuario o de máquina. Cita el paquete como `TC2:n`.

## Qué tienes que atacar como mínimo

1. **Fidelidad a F4B-E09 y a [OD].** ¿El alcance es exactamente H-31 y OWNER-1? ¿Se cuela algo de c-02, de `pb-21-cola` o de PB-T1? ¿Las extensiones que el paquete declara («Ambos lados» al cambiar de celda, el alcance, los datos del frente; la longitud que se ve igual deja de quedar pendiente; la caja avanzada vacía) caben en lo decidido o son decisiones materiales nuevas?
2. **El diseño por línea, frente al código de HEAD:**
   - `Keep` en `GapBox`;
   - `Keep` en `ModuleLengthBox`, con la tolerancia de 0,0001 y el paso a manual de `SetLength`;
   - `KeepOptional` en las cuatro avanzadas;
   - la regla mixta en `ReadCellValues` y en `OptionalVal`;
   - la retirada de `Val`.

   ¿Corrige todo lo que dice y nada más? ¿El indicador mixto sigue de verdad en pie cuando se lee la celda tras escribir? ¿Puede la regla mixta escribir a los dos lados un valor que el usuario no escribió, por ejemplo con la caja mixta vacía o con un valor sembrado? ¿Hay otros lectores o escritores de esas cajas?
3. **Cada CT.**
   - ¿Falla en la base **por aserción**, con la primera diferencia que dice el paquete?
   - ¿Sus líneas esperadas se cumplirían tras la corrección? Rehazlas por el código, en especial el módulo que no debe volverse manual y la caja mixta.
   - ¿Alguna aserción es tautológica (M-02)?
   - ¿Hay casos negativos donde hacen falta (M-03)?
   - ¿Se respeta M-05: ninguna ventana se cierra con cambios de módulos pendientes ni con un aviso de descarte real? Comprueba la costura `EditorDiscardPrompt.Substitute`.
   - ¿Son deterministas? ¿Los valores de prueba son válidos para el producto: 100,00044 y 120 en `pb-20-ab`, 24,00044 en `pb-07`, 48 → 56 en M2 de `pb-31`?
4. **Las sustituciones (0 según el paquete).** Busca con `git grep` toda prueba que fije el comportamiento actual de esas cajas o de «Ambos lados» con caja mixta. ¿Alguna se rompería con el GREEN?
5. **El contrato.**
   - El `AllowedWriteScope` (4 rutas) y el `ForbiddenWriteScope` (129).
   - Los `RequiredTests` (18 filtros), comprobando las colisiones de prefijos con las clases de PB-TC.
   - Los invariantes, las `StopConditions` (con las transcripciones de [DP] literales) y PBTC2-C14.
   - Las `EligibleCells` y su vigencia.
   - La línea base de la CI y los conteos previstos.
6. **Guardas existentes.** ¿Alguna salta con el RED o con el GREEN? Por ejemplo:
   - T1B (T-SG, T-KB y las demás);
   - la guarda de módulos (Fact3), que lee W en crudo;
   - I-55, el desviador, G-10 y el corpus de WLC.
7. **El efecto declarado (OB-06.4) y la medición previa en la cadena.** ¿Es suficiente? ¿Hay un efecto material posible que no se mida?
8. **Las anclas.** Comprueba una muestra amplia.
9. **Decisiones materiales nuevas.** ¿Hay alguna que deba ir al Owner antes de emitir? Sé estricto: con una sola, la emisión se para.

## Formato del informe

1. Objeto, método y límites.
2. Un apartado por punto, del 1 al 9, con su evidencia.
3. La tabla de hallazgos: Id, gravedad (BLOQUEANTE, IMPORTANTE o MENOR), contenido, evidencia y corrección.
4. El veredicto, con el recuento B/I/M y las decisiones nuevas para el Owner:
   - **EMITIBLE**;
   - **EMITIBLE TRAS CORRECCIONES**;
   - **NO EMITIBLE**.

**Tu respuesta final** es una sola línea: el veredicto, el recuento, si hay alguna decisión nueva para el Owner y la ruta del informe.
