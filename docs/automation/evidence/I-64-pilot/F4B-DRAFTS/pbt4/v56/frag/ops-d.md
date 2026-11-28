%%%% OP after s7
%%%% ANCHOR
y de cada filtro en su proceso, como los corre el contrato |
%%%% TEXT

| **R-T4-39, lo deducido de la base no es lo medido** (V55-10) | La propuesta de lista deducía que, hoy, editar con «Editar todo» sobre una base vieja acaba en el conflicto de «Actualizar» y en «Conservar mi borrador»; la receta de punta a punta de CT-PBT4E-04, con los dobles de CT-PBT4C-10 y sin la recarga, da otra cosa ({{SONDA_RED_UPDATE}}). El motivo dice que la relectura no encuentra el origen del borrador por su texto (A-T3-8; deducido del motivo, no leído por dentro): el cambio de fuera de la receta reescribe también el sobre de origen. Con un cambio de fuera que no toque el sobre de origen, el camino puede ser el del conflicto: no se midió. No cambia PB-T4e (con la recarga, la misma receta se aplica), pero sí lo que la propuesta describía como lo de hoy. Se mide con una sonda antes de describir lo de hoy, y el Smoke-PB lo mira en AutoCAD (§5.5, paso 6) |
| **R-T4-40, una prueba de texto que busca la primera aparición** (D-PBT4-22) | La autoprueba de `WorkspacePilotEditGuardTests.cs:574` muta la primera aparición de `return TryReloadPilotEdit(request);`: el texto nuevo de `EditAllOfPilot` no lleva ese literal (`var reload = TryReloadPilotEdit(request);`), y la suite completa lo confirma |
| **R-T4-41, una condición que llama al comparador sin protegerlo** (CA-T4e, M-01; D-S19) | `NeedsLazyReload` llama a `Status`, y `Status` al comparador sin protegerlo (DS:298-306); con un borrador «Sin decidir» la excepción salía de una condición evaluada antes de la vista (que sí lo protege, WHC:1860-1867). **La lección:** una rama nueva que mira el estado del borrador va detrás de las comprobaciones de la vista, y cada estado que la vista ya distingue lleva su línea en una CT (`marcado-sin-decidir`), medida contra el GREEN anterior |
%%%% END

%%%% OP after s83
%%%% ANCHOR
`d5467dc162e2cda8c539a829cc8f3a7c46260a119f70e840b34e6f78dde0387c`, quedan como historia) |
%%%% TEXT

| PB-T4e | Ninguna: ninguna prueba existente se edita, se salta ni se sustituye | 0 (la propuesta de lista: «La lista de sustituciones sale vacía (K = 0)», a confirmar al ejecutar) | **0**, medido con la suite completa de Core y los 19 filtros de UI (§4.5) | **La elegida** (`owner-pbt4e-list-decisions.md`, pregunta 4: «la lista es la elegida (aquí, ninguna prueba existente sustituida)»); sin archivo de lista |
%%%% END

%%%% OP after s85-tanda
%%%% ANCHOR
salgan más de 10 nuevas sobre las 29; lo que cubra la UI se mide con sus filtros |
%%%% TEXT

| **15.ª** (2026-11-22) | `owner-pbt4e-list-decisions.md` | Pregunta 1, «Alcance»: «Solo la recarga (Recomendado)». Pregunta 2, «Si falla»: «No abrir y avisar (Recomendado)». Pregunta 3, «Orden»: «Antes de PB-T5 (Recomendado)». Pregunta 4, «Emisión»: «Sí, con condiciones (Recomendado)». «La recarga con un aviso», «Recarga y comprobar cambios», «Abrir sobre la base de antes», «Fundirla con PB-T5», «Después de F4B» y «Vuelve antes de emitir» no se hacen | **PB-T4e** (D-PBT4-22): antes de abrir la ventana de «Editar todo», un borrador Clean marcado como posiblemente obsoleto se recarga desde el dibujo, sin aviso, igual que con «Editar»; Dirty o sin la marca, como hoy; si la recarga falla, no abre y el campo muestra lo mismo que «Editar»; una tarea propia, con su cadena y su intento 1 de 3, cerrada antes de PB-T5; la emisión condicional, con la lista vacía (lo que fija para PB-T4e) |
%%%% END

%%%% OP after s85-sesion
%%%% ANCHOR
**con su nota tras la recomprobación 1 de CA-T4d** (M-11): cada entrada nombra la corrida, no la prueba, y ESTABLE solo por la UI exige también un filtro en su proceso.
%%%% TEXT

- **D-S17, D-S18 y D-S19** (para PB-T4e; `session-pbt4e-design-decisions.md`, 2026-11-22, sha256 `b21f2af4bab5321620bf8de3aceeb95585a0e2fe9e4cc2ae96f18e37c3e41e6d`; el Owner dejó a la sesión el sitio de la recarga y el valor de `EditAllStatus` en `owner-pbt4e-list-decisions.md`, «Lo que queda decidido»): **D-S17**, la recarga va en el anfitrión (la opción A de la propuesta de lista), con `TryReloadPilotEdit`; **D-S18**, se reutiliza `EditAllStatus.NotBegun`, sin valor nuevo, y el campo distingue la recarga por `view.Exists`; las dos, propuestas en la v5.5 y confirmadas tal cual; y **D-S19** (tras CA-T4e, M-01), la recarga va detrás de las tres comprobaciones de la vista, solo sobre un borrador que el gesto no acaba de empezar, para que un borrador «Sin decidir» o con texto pendiente responda como hoy sin evaluar la marca (la v5.6). Ninguna cambia la lista ni el comportamiento visible decidido por el Owner. El registro corrige también la etiqueta de la evaluación de R-T4-39 y nombra sus otras dos premisas (CA-T4e, M-03).
%%%% END

%%%% OP after s9
%%%% ANCHOR
no ve:** la cobertura de datos, el Plugin y la UI fuera de los filtros (§4.4).
%%%% TEXT

- **Medidos en copias exportadas, para PB-T4e** (la v5.6, V56-04; los de la v5.5, `/d/r71` a `/d/r75`, `/d/r77`, el primer ensayo de la v5.6, con un `warning CS8604`, y `/d/r78`, la primera principal de la v5.6, sin los 19 filtros de UI, historia): la línea nueva de D-S19 contra el GREEN de la v5.5 (`/d/r76`), el ensayo y la sonda de las CT (`/d/r82`), la precondición 6 (`/d/r83`, la copia principal, sola, y `/d/r79`, la base con cobertura) y el control de las CT (`/d/r80` y `/d/r81`). **Lo que no se ve sin AutoCAD:** el modal real, la paleta deshabilitada, el foco y el coste percibido de la lectura más antes de abrir (§5.5); y lo deducido de D-PBT4-22 (el caso del borde) no se ejecutó.
%%%% END

%%%% OP replace s10
%%%% ANCHOR
- El Smoke-PB, con la lista de §5.5.
%%%% TEXT
- El Smoke-PB, con la lista de §5.5 (desde la v5.5, con la recarga de «Editar todo» de PB-T4e, que se cierra antes: `owner-pbt4e-list-decisions.md`, pregunta 3).
%%%% END

%%%% OP after s11-2
%%%% ANCHOR
y `session-pbt4d-design-decisions.md` (D-S11 a D-S15), con su número F4B-E al custodiarlas.
%%%% TEXT
 **Para PB-T4e:** `owner-pbt4e-list-decisions.md` (la 15.ª tanda) y las decisiones de la sesión D-S17, D-S18 y D-S19 (`session-pbt4e-design-decisions.md`, sha256 `b21f2af4bab5321620bf8de3aceeb95585a0e2fe9e4cc2ae96f18e37c3e41e6d`), con su número F4B-E al custodiarlas.
%%%% END

%%%% OP after s11-6
%%%% ANCHOR
`52e03e061cd62c79597f51b8eaf144a27d68b3bf5db876406a804be2fa978989`). **Una prueba que falle fuera de la lista** vuelve al Owner.
%%%% TEXT

   - **PB-T4e** (los nueve pasos de §6.5.1; `pbt4ecc/compile/compile-check.md`), sobre copias exportadas del cierre de PB-T4d (`160e019d`): el RED literal y el GREEN literal, sin ningún `warning CS`; **Core:** en el RED, {{CORE_RED}}, y solo caen las tres CT de Core de PB-T4e, por aserción; en el GREEN, {{CORE_GREEN}}; los 19 filtros de UI del contrato, cada uno en su proceso: en el RED solo cae CT-PBT4E-03 y en el GREEN, 0 `Failed`; la copia principal, sola; **la cobertura de todo `src/` que se queda, {{COV_RESULT}}**; la sonda y el control de las CT. Medido en la v5.6 (`pbt4e-precondition6.json`, `{{P6_JSON_SHA}}`). **Una prueba existente que falle** vuelve al Owner (la lista está vacía).
%%%% END

%%%% OP after s11-7
%%%% ANCHOR
que el generador comprueba; se emite por su autorización condicional (pregunta 4).
%%%% TEXT
 **PB-T4e, la lista vacía:** ninguna prueba existente sustituida (`owner-pbt4e-list-decisions.md`, pregunta 4), que el generador comprueba (ningún `Skip`, ningún equivalente y ningún archivo existente de `tests/` en el alcance); se emite por su autorización condicional.
%%%% END

%%%% OP before s11-10
%%%% ANCHOR
## Anexo V49-A. El texto literal de lo escrito a mano
%%%% TEXT
10. **Para PB-T4e** (desde la v5.5): **PB-T4d CERRADA** (F4B-E59, `160e019d`) y su remedición, que aquí es la lectura por objetos git de D-PBT4-22 y la precondición 6 sobre `160e019d`. **En el commit de emisión de PB-T4e,** además: esta versión (v5.6) y la v5.5, las piezas de las dos y sus herramientas y sus generadores (`v55/` y `v56/`), la precondición 6 (`pbt4ecc/compile/`: `compile-check.md`, `pbt4e-precondition6.json`, sus guiones, sus registros y el sha256 de sus TRX), el registro del Owner de la 15.ª tanda (`owner-pbt4e-list-decisions.md`), la propuesta de lista (`pbt4e/list-proposal.md`), el registro de la sesión con D-S17 a D-S19 (`session-pbt4e-design-decisions.md`), la comprobación acotada CA-T4e (`independent-review-pbt4e-check.md`) y su recomprobación sobre el diff de la v5.6, el resumen y el registro de la precondición 6 de la v5.5 (`pbt4e-precondition6-v55.json` y `compile-check-v55.md`, historia), y la entrada «## PB-T4e» de `superseding-characterization.md`, con la lista vacía.


%%%% END
