# I-62 — Paquete de revisión del Architect (enmienda A-3: compatibilidad de un runtime sucesor y huella material)

```text
A-3            = PROPUESTA (candidata) — preparada, con revisión adversarial previa aplicada (A-3 §12) e integración de decisiones §57, punto 4
                 (A-3 §12, S57-01..S57-08) y revisión adversarial del texto integrado aplicada en seis rondas (A-3 §12, M1-01..M1-29,
                 M2-01..M2-12, M3-01..M3-16, M4-01..M4-15, con la decisión de simplificación S-01: la regla 11, sin verificador, y
                 M5-01..M5-10 y M6-01..M6-03), sin revisión formal y no presentada. Los ocho MAJOR de decisiones §57, punto 4, son K-01,
                 K-02, K-03 y X-01..X-05 de la segunda ronda, todos aplicados
Architect      = REVIEW REQUIRED: una revisión formal acreditada de la A-3 exacta (cambio MATERIAL: LIFECYCLE §6; orden, punto 17)
Coordinator    = veredicto PENDING
Owner          = sin decisión para el acuerdo (A-3 §8; Q-A3-18 y Q-A3-20); si aparece una consecuencia OWNER-RESERVED, la enmienda se detiene.
                 A-3 no define ningún verificador ni ninguna clase de cambios aceptables: la evolución de OD-2 es OWNER-RESERVED y queda entera
                 para la A-n que ordene la decisión del Owner OD-2-MAT (regla 11, c; paquete aparte, PENDIENTE), que A-3 referencia y no resuelve
Presentación   = no antes de repetir la revisión adversarial sobre el texto integrado (decisiones §57, punto 4: «repetir guardas y revisión
                 adversarial antes de publicar la candidata»); la condición anterior (re-revisión de la A-2 corregida lanzada) ya se cumplió:
                 A-2 AGREED (decisiones §57, punto 1)
Invocación     = UNA. Transporte preferido: una celda `claude-cli` medida y elegible (decisiones §56, puntos 6-10; CLAUDE-CLI-I62 = A; decisiones
                 §57, punto 3); si no es elegible en ese momento, HUMAN_LAUNCH_REQUIRED con una sesión nueva. Sin reintento automático
Aplicación     = ninguna antes de AGREED; tampoco la materialización en superficies normativas (A-3 §7; Q-A3-02)

Objeto de la revisión (identidad por contenido; el commit lo da el recibo de publicación):
  docs/initiatives/I-62-A-3.md                                          blob <A3_BLOB: se fija al publicar>
  docs/initiatives/I-62-A-3-annex-maf-codex.md (anexo no normativo)     blob <ANNEX_BLOB: se fija al publicar>
Freeze que enmienda:
  FREEZE_SHA b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43
  docs/initiatives/I-62-proposal-v14.md   commit 4c617e82b32b6c810b68d75fc19472efed22b393   blob 34ad80ea1bfff144bfc5169f62920a4c904c1bfa
Enmiendas anteriores (sin cambio por A-3):
  docs/initiatives/I-62-A-1.md blob c01899a72b940503bb85a0fab42bc085c603fd0f (AGREED, decisiones §43)
  docs/initiatives/I-62-A-2.md blob f1e1d6f08d3cf677500794c7d0019433a4dc7d4e (AGREED, decisiones §57, punto 1; acuerdo append-only en 322cf8a8);
  el blob de A-2 no se edita después del acuerdo; si cambiara, A-3 se vuelve a preparar (G3 lo detecta)
Base de main: bb0d5522e8411f66a51fdfb3f1f0d0514b737453
Preparada sobre: b088421649204742ff682e1eb1ebdafc762823d0
Orden: decisiones §56, fila 11-17 (DEC L1100), y decisiones §57, punto 4 (DEC L1115). «Orden, punto n» = numeración del texto pegado del
Coordinator de §56 (SHA-256 e8b00328…); el de §57 tiene SHA-256 00a13ae5…
```

> **Identidad exacta.** El revisor comprueba que `git rev-parse <commit>:docs/initiatives/I-62-A-3.md` = `<A3_BLOB>` y
> `git rev-parse <commit>:docs/initiatives/I-62-A-3-annex-maf-codex.md` = `<ANNEX_BLOB>` en el commit del recibo de publicación, y que el blob de
> A-2 en ese commit es el mismo que en su padre (A-3 no modifica A-2). Si no coincide, revisa la versión designada o rechaza la discordancia. Este
> paquete no lleva su propio blob.

## 1. Veredicto que se solicita (LIFECYCLE §5 y §6)

```text
Resultado: AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION
Hallazgos: REQUIRED | OPTIONAL; ID, sección exacta, premisa canónica completa (PremiseRefs con líneas), autoridad o contraejemplo, por qué importa, corrección.
Además:    disposición de las preguntas Q-A3-01..Q-A3-22 (A-3 §9) como REQUIRED, OPTIONAL o sin hallazgo; necesidad o no de una decisión del Owner;
           confirmación de que A-3 no cambia el texto de P-01, P-11 ni OD-2 (ni, con la regla 11, su lectura), los topes de
           D.3, el texto ni la forma de los contratos B.1-B.11, state/v2, el texto de §20, A-1, A-2 ni F4, de que los cambios de lectura de B.2,
           B.4, B.5 y §20.5.1 son exactamente los declarados (A-3 §1 y M-02), de que A-3 no define ningún verificador ni ninguna clase de
           cambios aceptables y no lee valores (regla 11, a-c), y de que su materialización queda fuera de este registro.
Modo:      declarado (SAME-SESSION ROLE | SEPARATE SESSION | EXTERNAL HUMAN) y si revisor y autor son la misma persona; contexto inyectado declarado.
Identidad: commit, rutas y blobs revisados.
```

AGREED sobre la A-3 exacta, junto con el veredicto del Coordinator, la convierte en enmienda acordada del Freeze. A partir de ahí, y una vez en vigor
según disponga el Coordinator (A-3 §2.2 y §7; Q-A3-02), una celda con elegibilidad medida puede heredarla por un runtime sucesor con una sonda de
compatibilidad satisfactoria, siempre con la autoridad y el presupuesto de medición que ya rijan y con la huella vigente aceptada por el Owner (OD-2).
La regla 11 no cambia quién acepta la huella: la compara solo por su valor exacto y no define verificador. Una sonda que no es compatible no reduce lo
que la medición de V14 ya permitía. Un REQUIRED abierto solo lo cierra o lo rebaja quien lo emitió o quien tenga esa autoridad. La sesión no declara
ningún veredicto.

## 2. Lectura (insumos canónicos)

| Insumo | Blob | Para qué |
|---|---|---|
| `docs/initiatives/I-62-A-3.md` | `<A3_BLOB>` | el objeto completo: alcance y términos de la orden, literales con líneas (§2), clase, delta (reglas 1-11), cota de la sonda, predicado, resultados, OD-2, composición con A-2, guardas, huella material (§3.9), antes y después, obligaciones y pruebas, materialidad, no impacto, consecuencias del Owner, preguntas, veredictos y registro de la revisión previa y de la integración de §57 |
| `docs/initiatives/I-62-A-3-annex-maf-codex.md` | `<ANNEX_BLOB>` | anexo no normativo de la regla 11 para `codex-cli`: hechos de las tres actualizaciones, qué no neutraliza la receta 16.4, clasificación saneada de los 107 nombres, las 9 claves (8 de lanzamiento o de confianza), las familias de lanzamiento y de confianza, las superficies fuera de la huella, una propuesta no normativa de verificador y EXP-MAF-01 para la A-n de OD-2-MAT y la correspondencia con OD-2-MAT |
| `docs/automation/evidence/I-62-A3/od2-material-baseline-owner-packet.md` | `b2c8ec8a295188d05c0e07d153442f9268aaa20e` (fijado en A-3 §10; añadido en el mismo commit; G2 de `run` lo comprueba en head) | paquete de la decisión del Owner OD-2-MAT, PENDIENTE: comprobar que A-3 lo referencia sin resolverlo, que la regla 11 no define ni presume ningún mecanismo ni ninguna de sus opciones, y que ninguna opción ni la sección «Lectura de valores (no se pide ahora)» autoriza leer valores |
| `docs/automation/evidence/I-62-A3/od2-evolution.md` y `maf-codex.md` | `3b8dc62039210a48c26674c3c964466192b8b31e` y `a297efb2078931965ea15e14e05162cfb14dcbd8` (ídem) | investigación de la evolución de OD-2 (autoridad del Owner: SÍ; vehículo recomendado, una A-4 aparte) y de la huella material (hechos del proveedor con su estado y sus URLs; el diseño del verificador, propuesta no normativa); insumo de §3.9, del anexo y de Q-A3-19..Q-A3-22 |
| `docs/initiatives/I-62-proposal-v14.md` en `4c617e82` | `34ad80ea…` | cláusulas cuya lectura rige A-3: §4.2 (E7, recuperación), §5, §6 (ancla y fila de la observación), §7, §10, §13 (P-11, P-22..P-24), §18 (OD-2 y L977), §20.3, §20.5.1, B.2, B.3, B.4, B.5, Anexo C (C-06..C-11, C-21, C-40, C-41), D.3 y D.4; §1 (no-objetivos) |
| `docs/initiatives/I-62-A-1.md` | `c01899a7…` | formato de una A-n acordada (incluida su sección «Efecto en obligaciones y pruebas»); A-3 no la toca |
| `docs/initiatives/I-62-A-2.md` | `f1e1d6f0…` | A2-P2 (acordada) y la lectura de decisiones §55, punto 9: comprobar que A-3 no la modifica, que la composición de A-3 §3.7 es correcta y no reduce lo que A2-P2 mide, y que la regla 11 respeta A2-P2, reglas 3 y 4 (A2 L222-227) |
| `docs/initiatives/I-62-consensus-freeze.md` | `0f6b8608…` | identidad del Freeze |
| `docs/INITIATIVE_LIFECYCLE.md` §3, §5 y §6 | `f19896a8…` | formato de A-n, REQUIRED, M-01..M-08 (incluida la regla de duda de §3), OWNER-RESERVED (L94) y autoridades de una A-n (L221) |
| `docs/AUTOMATION_PLAN.md` 16.4, 16.9, 16.11, 16.14-16.16, 16.18-16.20 y 16.22 | `f525cb1e…` | textos inactivos que A-3 gobierna (A-3 §2.2), P-01 activo de I-61, que no gobierna, y la receta de Codex (L541-543) y la entrada (L522-523) que A-3 conserva |
| `docs/automation/agent-execution/README.md` §12-§14 | `592dcfd4…` | agregado, producción, invalidación y contraste, reglas B4 y B5, campos con autoridad, `CELL` y A7' |
| `docs/automation/agent-execution/routing.md` §5, §8 y §9 | `bba08fc4…` | elegibilidad de una celda y sondas (I-61, sin cambio), requisitos por acción y paso 5 del binding I62 |
| `docs/automation/agent-execution/adapters/codex-cli.md` y `claude-cli.md` | `155f3469…` y `ae570380…` | fuentes de las propiedades de la clase y de `BinaryHash` en los descriptores; huella declarada (CODEX L30); límite «nunca por el más reciente» |
| `docs/automation/agent-execution/schemas/preflight.v1.schema.json`, `binding.v1.schema.json` y `adapters/codex-cli.facts.v1.schema.json` | `6a054079…`, `13501476…` y `3d1b7478…` | invalidadores estrictos sin `BinaryHash` ni `AppVersion` (consecuencia M-02), `CatalogEntryBlob` del catálogo entero, forma de `Eligibility` y hechos declarados |
| `docs/automation/agent-execution/model-catalog.md` | `166d978d…` | estado local de las celdas y frescura; la herencia no se anota en él (A-3 regla 6) |
| `docs/adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md` | `da275e1a…` | #4 y «Vigilar» (L95), para la materialidad M-08 |
| `docs/automation/decisions/I-62.md` §46-§57 | `<DEC_BLOB>` (al preparar, `27591f16…`) | OD-2, OD-2b, OD-2b-PROBE, OD-2c, OD-2d, OD-2d-PROBE, §54 (L1043), §55 (puntos 9 y 11), §56 (puntos 1-10 y 18, y la fila 11-17, que es la orden de A-3) y §57 (puntos 1-4; el 4 amplía A-3) |
| `docs/automation/evidence/I-62-evidence.md` §64, §67-§68, §70-§71, §75 y §82-§87 | `<EV_BLOB>` (al preparar, `6a2b1569…`) | actualizaciones automáticas, P-01, cadena de OD-2, A-2 corregida y acordada, decisiones OD-3 y CLAUDE-CLI-I62, caracterización de `claude-cli` (§84), re-revisión de A-2 (§85), acuerdo (§86) y tercera actualización de Codex con el par vigente (§87) |
| `docs/automation/evidence/I-62-prep/owner-decision-packets.md` | `cdb86114…` | paquetes OD-2c y OD-2d (L182, L199 y L200, cuya frase sobre la receta corrige el anexo, §2) |
| `docs/automation/evidence/I-62-A3/a3-guards.py` | `<GUARDS_BLOB>` | modelo G5 (con el perfil RED de V14 y la regla 11), `preview` y `run` (G1-G5) |
| `docs/automation/evidence/I-62-A3/a3-selftest.json` | `<SELFTEST_BLOB>` | resultado de `self-test --a3 docs/initiatives/I-62-A-3.md`; su `A3TextBlob` = `<A3_BLOB>`, su `A3Text` es esa ruta y su `G5` es el que recalcula `run` en head (G5b lo comprueba) |
| `docs/automation/evidence/I-62-A3/a3-guards-result.json` | (en el commit siguiente al de publicación) | resultado de `a3-guards.py run --base <padre> --head <commit de A-3>`; su campo `Head` nombra ese commit |

Para las cláusulas de V14 basta leer su sección; los literales de A-3 §2 llevan la línea exacta. No hace falta expandir los documentos que cita su prosa.

## 3. Resumen del delta

- **Ubicación:** un párrafo al final de V14 §6, tras «no una foto anterior.». Ninguna otra cláusula cambia de texto.
- **Sucesor:** el runtime que sustituye al medido en la instalación verificada, nunca otro binario elegido junto a él; mismo adapter, proveedor y
  transporte, misma instancia de host con `HostInstanceState` OBSERVED, misma celda, mismos blobs de descriptor, catálogo y `routing.md`. Cambia al
  menos uno de `BinaryHash`, `AppVersion`, `AdapterVersion` o `BinaryPathHash` (definidos por los `Facts` del descriptor), y no cambia ningún otro
  invalidador: tampoco la huella (su valor exacto) ni el estado de autenticación. Un hecho sin fuente declarada es UNKNOWN en las
  dos observaciones y no cuenta como cambio; uno con fuente declarada sin valor observado, o un invalidador UNKNOWN, impide la sucesión. Los hechos
  se observan estables, y nunca durante una cesión o un intento en curso. Si la observación del sucesor ya muestra que no hay sucesor, no hay sonda;
  si lo muestra la propia sonda, cuenta como hecha y false.
- **Efecto:** el cambio sigue invalidando la observación (requisitos UNKNOWN, `OBSERVATION_STALE`, sin binding ni trabajo de modelo), pero ya no
  invalida para siempre la elegibilidad medida. `OBSERVATION_STALE` no es el `STALE` de la celda ni el `Stale` de B.5.
- **Sonda acotada:** una por sucesor y por unidad, rol, acción y celda, sin reintento; como máximo una invocación de modelo de solo lectura; sin
  escrituras; con la huella observada igual antes y después por su valor exacto. Solo se hace con la autoridad y el presupuesto vigentes (P-07).
  Con la huella vigente sin aceptar por OD-2, además, una autorización expresa del Owner, que para esa sonda hace las veces de “con OD-2” de D.3 sin
  aceptar la huella. A-3 no crea autoridad ni presupuesto.
- **Re-observación y suelo:** las 11 propiedades de `RUNTIME_COMPATIBILITY_CLASS` (el mínimo de la orden, punto 14), con la propiedad 2 como
  identidad del adapter, del transporte y del proveedor y las propiedades 8 y 11 con la fuente de §11.1, §20.3 y `routing.md` §5; todos los
  requisitos obligatorios de la acción; y las operaciones del contrato que el rol necesita, al nivel RUNTIME_OBSERVED (la cancelación, que una
  sonda de solo lectura no ejerce, por la declaración DISPONIBLE del descriptor si la sonda no la contradice). El suelo es la última **medición
  completa**: una medición autorizada (§5, paso 3) que registró toda la clase.
- **Predicado:** suelo vigente (frescura recalculada en la fecha de la sonda; sin sonda fallida desde la última elegibilidad), sucesor, sonda en cota
  y completa, requisitos con fila y en MATCH o ABOVE_REQUIRED, propiedades iguales salvo la introspección (igual o superior), invariantes de
  seguridad, contexto y salida sin cambio, nada pedido o registrado por encima del suelo, operaciones necesarias DISPONIBLES. Lo evalúa
  mecánicamente el registro custodiado y lo reproduce el aceptante; nunca es una autodeclaración.
- **Resultados:** true → herencia de la elegibilidad del suelo (`RunRef` = la sonda; `ConsumptionCovered` y `CatalogVerifiedOn` sin refrescar;
  `Stale` recalculado en cada binding), solo para esa unidad, rol, acción y celda, con la identidad nueva registrada, nunca en `model-catalog.md`.
  False → sin herencia; la observación anterior al cambio sigue `OBSERVATION_STALE` (el preflight de la sonda vale como observación nueva solo
  por §5 y §6); NOT_MEASURED hasta una medición de §5, paso 3, y sin otra sonda. La invocación de la sonda puede ser esa medición si la cumple por
  sí misma (no regresión frente a §55, punto 9 y A2-P2).
- **Sin aumento silencioso ni «más nuevo = mejor».** La huella y OD-2 no cambian: todo cambio de huella queda fuera de A-3, y la
  herencia no se usa sin la huella vigente aceptada por el Owner. Los controles por invocación (P-23, P-24, cierre de insumos) no se heredan.
- **Sucesiones encadenadas:** siempre contra el mismo suelo medido.
- **Huella material (regla 11; decisiones §57, punto 4), sin verificador (decisión S-01, A-3 §12):** a) la huella se compara solo por su valor
  exacto: todo cambio deja al sucesor fuera de `SUCCESSOR_COMPATIBLE` y conserva P-01 y P-11; A-3 no acepta ninguna huella, no define ninguna
  clase de cambios aceptables ni ningún verificador, y no lee valores ni autoriza leerlos; b) cuando un evento de sucesor coincide con un cambio
  de huella, la evidencia es solo por nombre (hash exacto antes y después con su estabilidad, hechos de identidad del runtime antes y después,
  nombres saneados añadidos o eliminados y estructura), sin valores ni digests por clave; localizar por clave los cambios de valor lee valores y
  solo podría permitirlo una autorización expresa del Owner que nombre el programa y su blob, las superficies y la caducidad, que A-3 no pide;
  c) requisitos para la A-n que ordene la decisión del Owner OD-2-MAT, la única que podría introducir un mecanismo de huella material: los seis
  elementos y las tres prohibiciones de decisiones §57, punto 4; las claves que fijan el sandbox, la política de aprobación o de permisos, la
  confianza de los espacios de trabajo, las extensiones habilitadas o su origen, o el modo de consumo, y las que designan código que el runtime
  ejecuta o lanza y las fronteras de confianza de ese código, nunca acopladas a la versión ni no materiales, por su función y no por la etiqueta
  de un mapa (sin función verificada para la versión observada, cuentan como tales); tokens de versión observados con independencia de la propia
  huella, nunca leídos de la configuración que explican; reglas custodiadas por blob y cambiadas solo con la autoridad que declare; lectura de
  valores solo con una autorización expresa del Owner, registrada de forma explícita, que nombre el blob del programa ejecutable, las superficies
  y la caducidad, que ni una disposición del Coordinator, ni una autorización de medición o de consumo, ni la línea de OD-2-MAT sustituyen;
  publicación cerrada; nombres saneados por una lista de permitidos; y un cambio sin cambio de identidad, una observación inestable, una clave
  desconocida, añadida o eliminada, un cambio durante una cesión o un intento en curso (P-11 y S-04 sin cambio) o una versión no observada nunca
  cuentan como explicados; d) sin ampliación. Del elemento (2), la clasificación de claves cambiadas, A-3 solo conserva una parte (A-3 §3.9):
  solo por nombre (nombres añadidos o eliminados y estructura, regla 11, b), que en un cambio solo de valores, como las tres actualizaciones
  medidas de `codex-cli`, no identifica ninguna clave cambiada; la clasificación de las 9 claves está solo en el anexo (no normativo, §5) y
  procede de localizaciones por clave anteriores que leyeron valores (evidencia §71 y §87); localizar los cambios de valor queda diferido a una
  autorización expresa del Owner (regla 11, b), que A-3 no pide. La investigación de `codex-cli` (anexo, no normativo) concluye que 8 de las 9
  claves que cambiaron son de lanzamiento o de confianza y que con los hechos de hoy no se demuestra ninguna clase de cambio irrelevante.
- **Obligación de prueba:** C-43 nueva (ii) MC, con las guardas como oráculo; efectos en C-07, C-08, C-09, C-11 y C-40 (A-3 §5).

## 4. Preguntas para la revisión

1. ¿Implementa el delta exactamente las órdenes del Coordinator (fila 11-17 de decisiones §56 y decisiones §57, punto 4), sin añadir semántica que
   no pidan? ¿Es necesaria y mínima cada precisión añadida? En concreto: sustitución del binario medido; descriptor y blobs en la definición de
   sucesor; huella y autenticación fuera de la relación; invalidador UNKNOWN y hecho de identidad declarado sin valor que impiden la sucesión; suelo
   sin `Stale` recalculado; sin sonda fallida desde la última elegibilidad; propiedad sin valor en el suelo = UNKNOWN; igualdad estricta de la
   clase de capacidad, la semántica del effort y el modo de consumo (A-3 §3.1); operaciones necesarias (h), con la cancelación no ejercida por la
   declaración del descriptor; no-sucesor detectado por la propia sonda que la deja contada; acciones de escritura excluidas; ámbito por unidad,
   rol, acción y celda; sonda con la huella sin aceptar solo con autorización expresa del Owner; y, en la regla 11, la comparación solo por el valor
   exacto, la evidencia por nombre y los requisitos para la A-n de OD-2-MAT.
2. ¿Es el fallo cerrado completo? En particular: ¿algún camino deja heredar con un requisito UNKNOWN, BELOW_REQUIRED u omitido, con un invariante
   cambiado, con la frescura caducada, sin sonda, con una sonda fuera de cota o tras una sonda fallida? ¿Algún camino de la regla 11 deja aceptar
   una huella por clase, leer valores (también por la localización por clave de los cambios de valor) o publicar en la evidencia algo más que la
   lista cerrada por nombre de la regla 11, b)? ¿Son necesarios y suficientes los requisitos de la regla 11, c)?
3. ¿Es correcta la no regresión de la regla 7 (opción (i): la invocación de la sonda puede ser la medición de V14) y está bien acotada para que no
   se lea como herencia?
4. ¿Impide el texto todo aumento silencioso de permisos, alcance de escritura, autoridad de rol, capacidad declarada o autoridad de consumo, y toda
   lectura de «más nuevo = mejor», también por la regla 11?
5. ¿Queda intacta la frontera de OD-2 (huella exacta, aceptación del Owner, P-01, P-11 y línea base de §18) y la de las autorizaciones de consumo?
   ¿Es neutral respecto del adapter la puerta de la huella sin aceptar? ¿Conserva la regla 11, a) P-01 en producción bajo la autoridad actual, y
   queda fuera de A-3 todo mecanismo que acepte huellas por clase o lea valores (regla 11, c; Q-A3-20 y Q-A3-21)?
6. ¿Es correcta la composición con A2-P2, acordada (A-3 §3.7), sin modificar A-2, con A-3 acordada o no, y sin interpretar el orden entre las sondas
   del bloque y la OD-2 sobre la huella resultante (A2-P2, regla 4), que fijan A-2 y la disposición del Coordinator que nombre el bloque?
7. ¿Es correcta la materialidad (M-02 a M-06 sí; M-07 sí por duda; M-01 y M-08 no, con la regla 11 sin verificador ni entradas propias, y sin
   punto de extensión del descriptor) y suficiente el efecto en obligaciones y pruebas (A-3 §5)?
8. ¿Modelan bien las guardas cada regla (vectores BASE, positivos y negativos; mutantes eliminados por un vector esperado también a ciegas del
   motivo; cobertura; patrones prohibidos y vínculo con el texto; privacidad), y prueban `preview` y `run` que A-3 no cambia producción, F4, A-2
   ni las superficies normativas?
9. Q-A3-01..Q-A3-22 de A-3 §9.

## 5. Condiciones de la invocación

- **Momento:** tras repetir la revisión adversarial sobre el texto integrado (decisiones §57, punto 4). La re-revisión de la A-2 corregida ya se
  lanzó y A-2 está acordada (decisiones §57, punto 1).
- **Transporte preferido:** una invocación `claude-cli` lanzada por la sesión principal, solo si la celda es elegible en ese momento según decisiones
  §56, puntos 7 y 8: caracterización acotada de solo lectura hecha (aceptada como evidencia de elegibilidad vigente, decisiones §57, punto 3), y
  celda instalada, autenticada, con invocación medida, capacidades requeridas en MATCH o ABOVE_REQUIRED, consumo cubierto distinto de UNKNOWN,
  `STALE` = false e independencia de Actor, Session y Context satisfecha. La autorización CLAUDE-CLI-I62 = A cubre la medición y el uso de solo
  lectura para ARCHITECT y REVIEWER de I-62. Cada uso lleva su autoridad (decisiones §56, punto 10) y la autorización explícita de consumo del
  Owner (punto 6). Si no consta para esta invocación, se pide antes del lanzamiento, en la solicitud única al Owner (decisiones §57, punto 6).
- **A-3 no se aplica a su propia revisión:** mientras no esté acordada, rige V14. Si el runtime de `claude-cli` cambia de identidad entre su medición
  y la invocación, la observación queda invalidada y hace falta una medición nueva con la autoridad vigente; si no es posible, HUMAN_LAUNCH_REQUIRED.
- **Si la celda no es elegible:** HUMAN_LAUNCH_REQUIRED con una sesión nueva, distinta de las revisiones anteriores. Mientras la vía `claude-cli` siga
  siendo viable, no se pide al Owner otra sesión de escritorio (decisiones §56, punto 18; §57, puntos 3 y 6).
- **Independencia (OD-6, alternativa 1):** Actor, Session y Context REQUIRED; Provider PREFERRED. El revisor es un proceso nuevo, sin el contexto de la
  sesión autora. Es del mismo proveedor que el autor, y eso se declara.
- **Kit:** clon limpio del commit de publicación, sin remoto y sin memoria de proyecto; cierre de insumos cerrado (los de §2); acciones de solo lectura;
  auditor con su prueba previa; el texto fijo de las órdenes (§56 y §57) y el cierre custodiados **antes** del lanzamiento. Un script en línea no lee
  archivos del cierre: las lecturas del cierre se hacen con las acciones de lectura permitidas (lección de la corrida R20261008T014941Z-68fe,
  decisiones §56, punto 1). Ni el cierre ni las acciones se amplían después de ejecutar.
- **Insumos:** los de §2, en el commit del recibo de publicación, sin transcripciones de la sesión autora ni del Coordinator. El registro de la
  revisión previa (A-3 §12) forma parte del objeto y no sustituye esta revisión. Ningún insumo contiene valores de configuración del host.

## 6. Lo que este paquete no hace

- no aplica A-3 ni la materializa: ningún texto congelado, materializado ni de producción cambia;
- no autoriza ninguna sonda, ningún consumo ni ninguna aceptación de huella;
- no define ni adopta ningún verificador, modo de OD-2 ni clase de cambios aceptables, no autoriza leer valores y no decide OD-2-MAT;
- no declara veredictos;
- no modifica A-1, A-2, el Freeze, ADR-0046 ni ADR-0048.
