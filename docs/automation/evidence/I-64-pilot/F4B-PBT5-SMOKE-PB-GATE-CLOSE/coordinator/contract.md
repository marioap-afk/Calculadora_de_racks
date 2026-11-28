# Contrato de F4B-PBT5-SMOKE-PB-GATE-CLOSE (PB-T5)

> Es, tal cual, el §6 del paquete de PB-T5 v2.4 (`docs/initiatives/I-64-f4b-pbt5-package.md`), con la redacción común. Forma B, la que eligió el Owner (`F4B-DRAFTS/coordinator/owner-pbt5-decisions.md`, Q-PBT5-8): sin `gate-contract.json`, sin Controller, sin nc1 a nc4 y sin D2, con una revisión independiente de la evidencia antes de la firma. Las abreviaturas ([F], [G], [S2], [BM], [OD5], [CA5] y las demás) son las de ese paquete; [SR] es `F4B-DRAFTS/pbt5/smoke-run-v2.md`.

## 6. El contrato (forma B, la que eligió el Owner)

### 6.1 La forma

- **El Owner eligió «Adaptación con revisión»** (Q-PBT5-8):
  - contrato propio, con invariantes, paradas, evidencia y presupuesto;
  - sin Controller, sin nc1 a nc4 y sin D2;
  - **antes de su firma, un revisor aparte comprueba en solo lectura que el registro, las capturas y los inventarios coinciden.**
- **El contrato es este §6.** El guion de la emisión lo saca tal cual a `F4B-PBT5-SMOKE-PB-GATE-CLOSE/coordinator/contract.md` (§6.7).
- **La v3 del sustituto de nc2** (F4B-E58) se eligió con una opción que decía «la aplico a este nc2 y a PB-T5». Con la forma B no hay nc2 en PB-T5, así que no se usa. La forma B la eligió el Owner después, sabiendo que no tiene nc1 a nc4.
- **El cierre es por la firma** ([F] §9; F4-E81), tras la revisión de la evidencia.

### 6.2 La identidad y el alcance de escritura

- **`TaskId`:** `F4B-PBT5-SMOKE-PB-GATE-CLOSE`. La emisión lo fija.
- **En el repositorio,** solo:
  - `I-64-pilot/F4B-SMOKE-PB/`;
  - `I-64-pilot/F4B-PBT5-SMOKE-PB-GATE-CLOSE/` (el contrato, la revisión de la evidencia y el paquete de cierre);
  - `F4B-DRAFTS/pbt5/`, `F4B-DRAFTS/coordinator/` y `F4B-DRAFTS/gate-close.md`;
  - [DISP], `I-64-evidence.md` e `I-64.yml`;
  - `docs/initiatives/I-64-f4b-pbt5-package.md`, este paquete;
  - `docs/ideas-futuras.md`, solo por orden del Owner.
- **Fuera:** `<raiz-de-trabajo>\` y el `bin/` y el `obj/` del worktree.
- **Prohibido:**
  - `src/`, `tests/`, `assets/` y los archivos de proyecto;
  - [G], HANDOFF, ROADMAP, WORKFLOW y AGENTS;
  - `main` y otras ramas;
  - los originales;
  - la configuración de AutoCAD y el registro;
  - las bibliotecas y `settings.json` del usuario.

### 6.3 Invariantes

| Id | Invariante |
|---|---|
| INV-PBT5-01 | `git diff <Candidato> <punta del cierre de F4B> -- src tests assets` vacío |
| INV-PBT5-02 | El DLL sale del Candidato: su `ProductVersion` lo identifica, y su SHA-256 se registra antes de NETLOAD y se comprueba en el proceso |
| INV-PBT5-03 | Solo copias; los originales, con respaldo y sin abrir |
| INV-PBT5-04 | Ningún cambio de configuración: «Load Once» y ningún «Always …» |
| INV-PBT5-05 | Nada se finge. Ninguna `RACKCAD_DEBUG_*` en el usuario, la máquina ni el shell; solo en el entorno de los AutoCAD de SPB-59 a SPB-61 (Q-PBT5-10) |
| INV-PBT5-06 | Los A/B y los cuatro contextos construidos llevan el rótulo **CONSTRUCTED REPRESENTATIVE TEST CASES**, y cada contexto construido tiene su gemela clásica (Q-PBT5-4) |
| INV-PBT5-07 | Los resultados de la sesión son propuestas. F4B se cierra solo con la firma del Owner, después de la revisión independiente de la evidencia (Q-PBT5-8) |
| INV-PBT5-08 | La evidencia custodiada no lleva rutas locales, nombres de clientes ni datos personales. La lista local de la selección de Smoke-2 no se custodia, ni se copia su contenido. Antes de custodiar, los patrones locales de privacidad pasan por todo, y lo que case va solo por su sha256 |
| INV-PBT5-09 | [G] no cambia |
| INV-PBT5-10 | Sin integración en `main`; HANDOFF y ROADMAP sin cambios |
| INV-PBT5-11 | Una captura antes de cada clic, abierta y revisada; coordenadas y captura de después en cada resultado negativo |
| INV-PBT5-12 | N-I1 |
| INV-PBT5-13 | Las salidas del producto, solo en `<carpeta-del-Smoke-PB>\salidas` |
| INV-PBT5-14 | Ningún FAIL se corrige en PB-T5 |
| INV-PBT5-15 | La UI completa no corre en local: la dispensa de Smoke-2, extendida por el Owner (Q-PBT5-1). La regla de F4-E59 no se levanta |
| INV-PBT5-16 | Cada bloque de PowerShell funciona solo, para si un valor está vacío o una ruta no existe, nunca escribe en la raíz de una unidad y termina con `exit 0` |
| INV-PBT5-17 | El estilo de cotas no se fabrica. Si ningún dibujo real lo guarda, SPB-07 vuelve al Owner (Q-PBT5-2) |

### 6.4 Paradas

Las 17 paradas, enteras. Este §6 es el contrato, y no remite a ninguna versión anterior.

| Id | Cuándo | Qué se hace |
|---|---|---|
| PBT5-C01 | **El entorno.** **Antes de la emisión:** el árbol sucio; HEAD distinto del Candidato; el diff `<producto>..<Candidato> -- src tests assets` no vacío; la CI sin la corrida de push exacta del Candidato; el DLL que no identifica el Candidato; o un SHA-256 del DLL que cambia. **Después de la emisión** (HEAD es su commit documental): el árbol sucio; el Candidato que no es antepasado de HEAD; el diff `<Candidato>..HEAD -- src tests assets` no vacío; o lo mismo del DLL | STOP antes de abrir AutoCAD, o en el momento |
| PBT5-C02 | Una copia distinta de su original, o un original cambiado durante la corrida (D1 o uno de P-09). Que un original de P-09 no coincida con su SHA-256 de Smoke-2 **antes** de copiarlo no es parada: se registra (P-09) | STOP; si cambió un original, aviso inmediato al Owner |
| PBT5-C03 | Haría falta cambiar la configuración (§5.3) | STOP |
| PBT5-C04 | Una `RACKCAD_DEBUG_*` en el usuario, la máquina o el shell; o en el entorno de un AutoCAD que no sea el de su inyección | STOP |
| PBT5-C05 | Un FAIL de producto | Se registra con toda su evidencia, se siguen solo los pasos independientes e inocuos, y vuelve al Owner |
| PBT5-C06 | Una escritura posible fuera de las copias o de `<carpeta-del-Smoke-PB>\salidas` | STOP del paso; LIMITATION |
| PBT5-C07 | La tercera repetición por error del operador, o la duda | Al Owner |
| PBT5-C08 | Un AutoCAD del Owner abierto al compilar, o que haría falta tocar | No se toca; STOP del build. Nunca se usa COM para abrir o cerrar documentos, ni `Stop-Process`. Antes de `CloseMainWindow`, el PID se comprueba por su ruta, su hora de inicio y su título |
| PBT5-C09 | Un Worker u otra sesión en el worktree | STOP |
| PBT5-C10 | Solo observable fingiendo | NO VERIFICABLE FÍSICAMENTE |
| PBT5-C11 | **La frontera horaria** (§5.4) | **Si la emisión y P-01 a P-08 no están hechas antes de las 12:00 UTC,** la sesión no opera AutoCAD sola: prepara lo que no lo necesita y espera al Owner. **A las 13:00 UTC,** se para en el límite de paso y se espera al Owner; después, solo SPB-51 y SPB-52, que leen |
| PBT5-C12 | AutoCAD se cuelga o se cierra | Se registra y se cierra de forma ordinaria. El bloque a medias se repite en una copia nueva, dentro de la misma corrida; el segundo caso, al Owner |
| PBT5-C13 | Un comportamiento no caracterizado | OBSERVADO, con toda la evidencia |
| PBT5-C14 | Haría falta el intento 3 | Al Owner |
| PBT5-C15 | **Una prueba falla en la CI o en local sobre el Candidato** | Es un fallo de producto y un modo nuevo: vuelve al Owner, no es una parada de entorno. **La regla de reejecutar, confirmada por el Owner (Q-PBT5-8):** una CI en rojo solo por el detector de cuelgue (`blame-hang`) en una prueba lenta ya conocida admite una sola reejecución de los trabajos fallidos sobre el mismo SHA; otra causa, o un segundo rojo, vuelve al Owner |
| PBT5-C16 | **La revisión independiente de la evidencia** encuentra que el registro, las capturas o los inventarios no coinciden | **La sesión solo corrige transcripciones:** el registro frente a la captura o al inventario. Después, vuelve a la revisión. **Si la discrepancia cambia un resultado, o exige repetir un paso en AutoCAD, vuelve al Owner,** que puede pedir una adenda. Solo la adenda que pida el Owner no cuenta como intento (§6.6) |
| PBT5-C17 | **Una de las tres condiciones de la emisión condicional** (Q-PBT5-7) no se cumple, **después de resolver los hallazgos y recomprobar:** la recomprobación acotada no da 0 bloqueantes y 0 importantes sin resolver; aparece una decisión material nueva; o la CI exacta del Candidato no está en verde | No se emite, y vuelve al Owner. **Si la CI está en rojo solo por el detector de cuelgue** en una prueba lenta conocida, antes va la reejecución de PBT5-C15; si con ella queda en verde, la condición se cumple |

### 6.5 La evidencia esperada

- `I-64-pilot/F4B-SMOKE-PB/prep/` y `run/`;
- `I-64-pilot/F4B-PBT5-SMOKE-PB-GATE-CLOSE/coordinator/contract.md`, la revisión independiente de la evidencia y su encargo, y el paquete de cierre;
- `F4B-DRAFTS/pbt5/`: las versiones del paquete y de las preguntas, las revisiones, [SR] redactada, las guías de la corrida previa como historia, los traspasos y las herramientas.
  - Las `_tools/*.local.txt` no se custodian.
  - Lo que case un patrón prohibido tras la redacción va solo por su sha256 ([DISP] F4B-E60).
- `F4B-DRAFTS/coordinator/owner-pbt5-decisions.md` y, después, la disposición y la firma del Owner;
- `F4B-DRAFTS/gate-close.md`, con la tabla de los INV;
- F4B-E62 (la emisión), la §292 y las entradas y secciones siguientes; los cambios de `I-64.yml`.

### 6.6 El presupuesto (Q-PBT5-9, «Definición y reglas»)

- **1 de 3** (OB-10.1); el intento 3, siempre al Owner (OB-10.2).
- **Un intento es una corrida del Smoke-PB sobre un Candidato.**
- **No cuentan:**
  - repetir un paso por error del operador (dos veces como máximo);
  - una parada del entorno antes de abrir AutoCAD;
  - una adenda que pida el Owner al revisar.
- **Solo la adenda que pida el Owner no cuenta** ([CA5] I-04). Una discrepancia de la revisión de la evidencia no permite repetir un paso como adenda: la sesión corrige transcripciones, o vuelve al Owner (PBT5-C16). **Un paso repetido por otra causa** no es una adenda: cuenta según la definición de intento.
- **Se adoptan las tres reglas de Smoke-2:**
  - las etiquetas: NOT AVAILABLE FOR PHYSICAL SMOKE; NO VERIFICABLE FÍSICAMENTE / CUBIERTO POR PRUEBA AUTOMATIZADA; OBSERVADO;
  - traer la evidencia antes de decidir un OBSERVADO;
  - una copia de trabajo por escenario.
- **Una tarea de corrección** que nazca de un FAIL tiene su propio contrato y su 1 de 3.

### 6.7 La cadena

1. **Esta v2.4 y su recomprobación acotada,** que tiene que dar 0 bloqueantes y 0 importantes sin resolver. Su resultado va en una sola línea `BOUNDED_RESULT:`, que lee el guion. **La línea** empieza por el token exacto `EMITIBLE;`, lleva `B 0 / I 0` y los prefijos sha256 (8 caracteres) de lo revisado, en la forma `paquete <p>, guia <g>, guion <e>`. El guion los compara con el paquete, la guía y el guion que emite ([CA5] M-02).
2. **El cierre de PB-T4e,** hecho: F4B-E61 y §291, en `ebd304a3`, que es el Candidato. La CI de push del Candidato terminó 4/4 (`45147867500`).
3. **`emit_pbt5.py --dry`,** que corre la sesión con HEAD = Candidato, después del bloque 1 de [SR]. Solo lee. Comprueba:
   - la numeración: existen F4B-E61 y la §291, y no existen F4B-E62 ni la §292;
   - el Candidato, su árbol y la CI exacta en verde;
   - la línea `BOUNDED_RESULT:` y sus sha256;
   - los sha256 de lo que se custodia y la redacción, con la guía y el paquete exigidos como copia;
   - que no existan el contrato ni el archivo del mensaje del commit;
   - y el cambio de `I-64.yml`, calculado sobre `HEAD:` (sus claves, sus marcas y que no diga ya «PB-T5 EMITIDA»).

   Escribe su listado.
4. **La preparación y el build en el Candidato** ([SR] §4.10: B1 a B4 y los bloques 2, 3, 4 y 4b). El build va antes de la emisión.
5. **`emit_pbt5.py --commit`,** que corre la sesión:
   - repite las comprobaciones y compara con el listado del `--dry`;
   - copia con la redacción;
   - saca el contrato (§6) a `contract.md`;
   - añade F4B-E62 y la §292;
   - cambia `I-64.yml`;
   - y hace el commit documental y lo sube, sin forzar.

   **Es la emisión,** por la autorización condicional del Owner (Q-PBT5-7).

   **Si el `--commit` para a mitad** y deja un estado parcial: `git -C <worktree> status`; se restauran [DISP], `I-64-evidence.md` e `I-64.yml` desde HEAD (`git -C <worktree> restore --source=HEAD --staged --worktree -- <esas tres rutas>`), y se retiran solo las rutas nuevas del listado del `--dry` y el contrato, si quedaron. Si el estado no se entiende, o el commit llegó a hacerse sin subirse, al Owner.
6. **El Smoke-PB contratado** ([SR]), con la frontera de §5.4: la sesión lo opera sola solo si la emisión y P-01 a P-08 están hechas antes de las 12:00 UTC.
7. **La custodia** de la corrida, redactada, en un commit documental de la sesión.
8. **La revisión independiente de la evidencia** (Q-PBT5-8), en solo lectura.
9. **La disposición del Owner** (los S+O, los OBSERVADO, SPB-07 si toca) **y su firma.**
10. **El gate-close** (§3.3).

---
