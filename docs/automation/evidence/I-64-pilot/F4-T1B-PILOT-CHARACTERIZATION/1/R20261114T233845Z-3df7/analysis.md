# I-64 F4-T1B-PILOT-CHARACTERIZATION, intento 1: análisis para la disposición del Owner antes del cierre

## Estado de la cadena

| Paso | Resultado |
|---|---|
| Planificación `R20261114T214147Z-851e` | Comparación byte a byte PASS en las 8 colecciones a la primera, A1-A8 8/8, nc4 `R20261114T214529Z-15d5` con su oráculo cumplido |
| Worker `R20261114T214644Z-c077` | IMPLEMENTATION_COMPLETE. RED `92f5f5db` y GREEN `1d36d006`, cada uno con su push |
| CI exacta | RED `43649940464`: tests y ui-tests en failure, build-ui en success y build-plugin omitido, sin cuelgue. GREEN `43650040527`: 4/4 en success |
| RED conjunto | UI 55/55 y Core 10/10 fallan por aserción, con el criterio mecánico del TRX cumplido. La unión de 3 pruebas existentes está atribuida (MU-25, MU-01 y MU-17) |
| GREEN | `git diff ChainBaseSha CurrentSha -- src` vacío. El blob de la base es igual al del GREEN en los 14 archivos de MutantScope |
| Revisión directa (`direct-review.md`) | Conforme. 4 menores (D-1..D-4) y el hallazgo previo H-1 |
| Revisión independiente (`independent-review-code.md`) | ACEPTABLE CON MENORES, 0/3/10. Pide resolver antes del cierre las tres importantes I-01, I-02 e I-03 |
| Verificación del Controller `R20261114T233845Z-3df7` | EXECUTION_VERIFIED, 14/14 en pass, sin hallazgos |
| nc1, nc2 y nc3 | nc1 `R20261114T234208Z-6d40`: CONTROL PASS (Identity). nc2 `R20261114T234618Z-842a`: CONTROL NOT PASSED con la firma exacta de PD-F4-01 (Contract y Scope en fail, EXECUTION_BLOCKED/STOP); su sustituto `F4T1B-NC2-EXACT-WHITELIST-01` da PASS (autoprueba determinista y 10/10 propiedades). nc3 `R20261114T235440Z-c45c`: CONTROL PASS (FreeText, REWORK). Los cuatro relevos son válidos, con Snapshot I, 0 procesos y el árbol limpio |

## Las tres importantes (ninguna toca el código ni consume un intento)

### I-01. La declaración de anclas de la entrega no es exhaustiva

- **Qué pasa.** Evidence[2] declara las aserciones sin mutante propio, pero no todas. La revisión enumera unas 11 omitidas: el contrato inicial de CT-05, `InsertRequested`/`UpdateOnly`/GUID de CT-07, el estado final de CT-04 y otras. Además hay dos declaraciones inexactas: CT-28 cita un ancla que no existe, y CT-23 dice «cinco vistas» y fija tres.
- **Lo que no pasa.** Todas esas aserciones van en métodos que un mutante de la tabla detecta, así que ningún método queda sin discriminar.
- **La letra.** El paquete dice que «la entrega declara todas las aserciones sin mutante propio». F4T1B-C04, en su tercer caso, es una parada: «an assertion without its own mutant that is not declared as an anchor inside a detected method». Leída al pie de la letra, la entrega la activa.
- **El Controller** no la activó (14/14).
- **La sesión no puede dispensarla:** F4-E28.

### I-02. Las suites completas en local reescribieron la fecha del archivo de disposición REAL del perfil

- **Qué pasa.** La suite de UI existente instancia el configurador de cabecera real y, al cerrarlo, escribe `RackFrameConfigurator.layout` en `LocalApplicationData`, una ruta `static readonly` de `RackFrameConfiguratorLayoutStore.cs:26-29`. Lo hacen al menos diez clases existentes. El contenido quedó idéntico (mismo SHA-256) y la fecha cambió dos veces: en la suite completa del RED y en la del GREEN.
- **La causa es de la sesión.** El prompt del Worker exigía las suites completas en local y decía «si la limpieza cambia algo: parada F4T1B-C09 con los dos valores». El Worker no se paró: volvió a acotar la prueba a sus corridas filtradas, que es coherente con el texto de INV-F4T1B-06, y lo registró como limitación.
- **El contrato.** Las pruebas de T1B no escriben, así que el contrato se cumple. Pero durante la delegación hubo una escritura en la configuración real del usuario, y la orden del Owner es «never use a user's real preference/config file… Do not accept writes into the real application/user configuration».
- **Es la misma ruta** que la contención del Owner prohibió instanciar en T1B (H-1 de la revisión directa: la deuda previa de la suite existente).

### I-03. Una sonda temporal de medición disparó `InsertBatch_Click` con entradas válidas

- **Qué pasa.** Fue sobre una ventana sin mostrar: lanzó en `Owner = owner` antes de `ShowDialog`, no escribió nada y no llegó a ningún commit.
- **La letra.** Contradice INV-F4T1B-06, «ninguna prueba… dispara InsertBatch_Click con entradas validas», aunque esa cláusula se refiere a las pruebas.
- **No hubo efectos.** No provocó ninguno de los que el Owner castiga con STOP: ni cuelgue, ni nada vivo, ni escritura, ni AutoCAD, ni rutas de máquina.
- **Era innecesaria.**

## Opciones y recomendación

| Punto | Recomendación de la sesión | Alternativas |
|---|---|---|
| I-01 | **Cerrar con una adenda de la sesión** en la evidencia de custodia: la lista completa de las anclas omitidas y las dos correcciones, medidas por la revisión independiente. El Owner declara que C04 (3) no se activa porque toda aserción va en un método detectado, que es el fin de la regla, y que la declaración incompleta es un defecto documental de la entrega | (b) Corrección como intento 2/3, solo de la entrega (sin `src/` ni `tests/`): consume un intento para un cambio documental. (c) Aplicar C04 al pie de la letra: STOP |
| I-02 | **Ratificar** que T1B no escribe, registrar la escritura como desviación de la delegación (causa: el prompt de la sesión) y fijar la regla para T4B y para cualquier corrección: **nunca suites completas de UI en local con contención de perfil; las suites completas solo en la CI**. H-1 queda como deuda propia, que el Owner decide, por ejemplo con una costura del almacén de disposición en una tarea aparte | Exigir antes del cierre una corrección o una tarea que aísle el almacén (cambio de producto, fuera de T1B) |
| I-03 | **Ratificar** la desviación contenida y prohibir en los prompts futuros las sondas con entradas válidas en los caminos con escritura o diálogo | Tratarla como incumplimiento de INV-06 y corregir: no hay nada que corregir en el código |
| Menores | Llevar M-01..M-10 y D-1..D-4 a la reconciliación previa a T4B (§8 del paquete) y a F5. Ninguno toca el cierre | — |
