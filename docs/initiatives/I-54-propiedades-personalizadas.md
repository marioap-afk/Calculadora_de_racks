---
schema: rackcad-initiative/v1
id: I-54
title: "ID24 — Custom Properties Foundation"
type: architecture
status: integrated
branch: architecture/propiedades-personalizadas
base_branch: main
priority:
size:
depends_on: []
conflicts_with: []
context_packs: [persistence, autocad-plugin, architecture-kernel, ui-editors]
automation_state_path:
decision_paths: [docs/automation/decisions/I-54.md]
requires_ci: true
requires_plugin_build: true
requires_autocad: true
requires_owner_decision: true
requires_owner_validation: true
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# I-54 — ID24 — Custom Properties Foundation

> **Fase actual: G10 — cierre documental e integracion (2026-09-13).** Candidato G8
> `02987bd18ef0904a0332556928503a9afaeddcd1`, validado por el Owner en G9 (OV-01..OV-14 **PASS**, «Apruebo
> validación»). Este commit documental es el ultimo de la rama antes del merge `--no-ff` en `main`, y marca I-54 como
> **integrada** antes de que exista el CI del merge: la integracion solo queda **verificada** cuando pasen el CI del
> `MERGE_SHA` con su cobertura y la cobertura diferida del Candidato ([WORKFLOW](../WORKFLOW.md) §4.5 pasos 6 y 7).
>
> - Discovery: [I-54-discovery.md](I-54-discovery.md) (G1 `195964b`, equivalente post-rebase `97e27ea`).
> - Proposal consensuada, inmutable: [I-54-proposal-v5.md](I-54-proposal-v5.md) @
>   `26ca923492576185b753d6dbf2a852969df2accf`. La [V1](I-54-proposal-v1.md), la [V2](I-54-proposal-v2.md), la
>   [V3](I-54-proposal-v3.md) y la [V4](I-54-proposal-v4.md) quedan como historia; el mapa de SHAs pre y post rebase
>   esta en la V5 §2.2, y el historial pre-rebase revisado queda preservado en el tag
>   `archive/i-54-custom-properties-pre-rebase-5d25da8`.
> - ADR: [ADR-0039](../adr/0039-custom-properties-persistencia-autoridad.md), `aceptado` (nacio `propuesto` en
>   `d84f480`). Registro de decisiones: [decisions/I-54.md](../automation/decisions/I-54.md), §11 para la aceptacion,
>   §12 para la traza del freeze, §13 para el cierre de G3, §14 para el cierre de G4 y el rebase, §15 para el cierre
>   de G5, §16 para el cierre de G6, §17 para la decision del Owner sobre el nombre del comando, §18 para el cierre
>   de G7, con la desviacion de proceso de G7A y el rebase sobre `104ef3a`, y §19 para el Candidato, la validacion del
>   Owner y la integracion.
> - Paquete de la ultima revision exact-SHA (G2J), limitado a AR-54-V4-01 / C-F3:
>   [I-54-architect-review-package.md](I-54-architect-review-package.md).
> - G2B cerrado: `Architect Review V1 = AGREED WITH CHANGES` (cambios V2-1..V2-22), incorporados en V2.
> - G2D cerrado: `Architect Review V2 = AGREED WITH CHANGES` (cambios C-1..C-8), incorporados en V3.
> - G2F cerrado: `Architect Review V3 = AGREED WITH CHANGES` sobre `5d25da8` (AR-54-V3-01 y AR-54-V3-02, MINOR),
>   incorporados en V4 como C-F1 y C-F2.
> - G2H cerrado: `Architect Review V4 = AGREED WITH CHANGES` sobre `8bc991c` (solo AR-54-V4-01, MINOR de
>   redaccion), con `Coordinator = AGREED`; el Coordinador acepta el cambio, incorporado en V5 como C-F3.
> - G2J cerrado: revision final exact-SHA de V5, limitada a AR-54-V4-01 / C-F3, con `Coordinator = AGREED` y
>   `Architect = AGREED` sobre `26ca923`, sin hallazgos nuevos ni cambios requeridos.
> - G2-FREEZE PREP hecho: `d84f480`, con ADR-0039 `propuesto`, F-01..F-14b en `ideas-futuras.md` y el tag de archivo;
>   CI 34748982744 en verde.
> - G2-FREEZE COMPLETION hecho: el Owner acepta ADR-0039 de forma explicita, con la redaccion literal “Acepto”;
>   `3866252`, CI 34750208424 en verde.
> - G3 hecho, por orden del Coordinador tras el freeze completo: nucleo puro de Custom Properties en Application, con
>   sus pruebas Core; `57f2b4e`, CI 34759032979 en verde. El Coordinador lo acepta.
> - G4 hecho, por orden del Coordinador: caracterizacion del sobre de BASE (G4A, `dcc4048`, CI 34771155216) y miembro
>   `CustomProperties` en el sobre (G4B, `78e6696`, CI 34772215008), ambos en verde. El Coordinador lo acepta.
> - G4-REBASE-CLOSE: historia original preservada en el tag `archive/i-54-g4-pre-rebase-78e6696`; rama rebasada sobre
>   `main` @ `1b091be`; tip de producto post-rebase `7b0f6a9`, CI 34773339064 en verde. Cierre documental `477c149`,
>   CI 34773953746 en verde.
> - G5 hecho, por orden del Coordinador: autoridad por `RackId`, workspace, preflight y commit puros en Application, con
>   sus pruebas Core; `3cf0d35`, CI 34778231177 en verde. El Coordinador lo acepta. Cierre documental `6b438be`, CI
>   34779470825 en verde.
> - G6 hecho, por orden del Coordinador: borde fisico en el Plugin —NOD de Proyecto y ejecutores de Proyecto y de Rack—,
>   sin comando ni UI, con sus pruebas Core y guardas; `ca71962`, CI 34787646732 en verde. El Coordinador lo acepta.
>   Cierre documental `b4e99e4`, CI 34788418383 en verde.
> - Decision del Owner sobre el nombre del comando, transmitida por el Coordinador: «Acepto RACKPROPIEDADES con alias
>   RPR.» (2026-09-13). El Coordinador autoriza G7; G7A la registra antes de cualquier codigo de producto.
> - G7 hecho, por orden del Coordinador, que lo acepta (G7 COMPLETE):
>   - **G7A**, solo documentacion. Se commiteo primero como `e25d159`, sobre la base anterior `1b091be`, aunque `main`
>     ya habia avanzado a `104ef3a` despues del preflight: **desviacion de proceso**, corregida antes de cualquier
>     codigo de producto. La historia previa queda en `archive/i-54-g7a-pre-rebase-e25d159`; la rama entera se rebaso
>     sobre `104ef3a`, y el equivalente de G7A es `5c16ae8`, CI 34789745328 en verde.
>   - **G7B**, producto: el comando `RACKPROPIEDADES` con alias `RPR`, `RackCustomPropertiesWindow` (arquetipo C), la
>     ayuda, los censos, U-01..U-10 y T-GRD-08; `18cf2dc`, sobre esa misma historia rebasada, CI 34792787919 en verde.
>   - **G7-CLOSE**: sincronizacion documental, `02987bd`, CI 34794374450 en verde.
> - G8 hecho, por orden del Coordinador: el Candidato es `02987bd` —la punta de G7-CLOSE, sin commit nuevo—, con Core
>   Full, UI Full y los builds Debug de UI y de Plugin en local sobre ese SHA exacto y su CI de `push` 34794374450 4/4.
> - G9 hecho: validacion del Owner en AutoCAD sobre el DLL de ese Candidato, OV-01..OV-14 **PASS**, con aprobacion
>   explicita «Apruebo validación».
> - G10, por orden del Coordinador: `main` no avanzo desde `104ef3a`, asi que no hay rebase final y lo validado es
>   exactamente lo que se integra. Este cierre documental precede al merge `--no-ff`.
>
> ```text
> G2J                   = CLOSED
> Proposal V5           = 26ca923492576185b753d6dbf2a852969df2accf
> G2-FREEZE PREP        = d84f480f032f6a7e5e481359aeab9fd18f691fbf   (CI 34748982744 GREEN)
> Coordinator           = AGREED
> Architect             = AGREED
> Technical Consensus   = REACHED
> Owner ADR Acceptance  = ACCEPTED   (literal: “Acepto”; 2026-09-13)
> ADR-0039              = ACCEPTED
> Consensus Freeze      = COMPLETE
> FREEZE_COMPLETION_SHA = 386625259922fc90ec735f739d4d5ed8e9510873
> FREEZE_COMPLETION_CI  = 34750208424 success
> G3 authorization      = Coordinator authorized after freeze completion
> G3_SHA                = 57f2b4e062be6b8f83b27e0cdb1b2a42eb2a4799
> G3_CI                 = 34759032979 success
> G4A_SHA               = dcc404853cad549708738488966920331e884fee   (CI 34771155216 success)
> G4_SHA                = 78e6696e64db50ffcfbcd7e87d7dfe0a9ca78417   (CI 34772215008 success)
> G4 pre-rebase archive = archive/i-54-g4-pre-rebase-78e6696 -> 78e6696e64db50ffcfbcd7e87d7dfe0a9ca78417
> G4 rebase base        = 1b091bedafceb67ca57054a9eb3bf5259efff774   (main, merge de I-53 E1; base hasta el rebase de G7A)
> REBASED_G4A_SHA       = b568bef50fc2e9db0ddfc65d882669db6dc036ef
> REBASED_G4_SHA        = 7b0f6a9ca849044acd5326f4bbcc68b6aa8fa5f0   (CI 34773339064 success)
> G4_CLOSE_SHA          = 477c149fb0d57bbd263d38c87b3d29467ac27816   (CI 34773953746 success)
> G5 authorization      = Coordinator
> G5_SHA                = 3cf0d35099a6b91ced32b818fee969147f11fc77   (CI 34778231177 success)
> G5_CLOSE_SHA          = 6b438be79ccc469bf7a40ddac19311a3b238781d   (CI 34779470825 success)
> G6 authorization      = Coordinator
> G6_SHA                = ca7196268ceb73d8c5223b508bbe9570698b38d7   (CI 34787646732 success)
> G6_CLOSE_SHA          = b4e99e4cbd11d3ed50a6f6b4551f592b9dc723f0   (CI 34788418383 success)
> Owner Command Naming Decision = ACCEPTED
> Primary Command       = RACKPROPIEDADES
> Alias                 = RPR
> Owner wording         = "Acepto RACKPROPIEDADES con alias RPR."   (2026-09-13)
> G7 authorization      = Coordinator
> G7A original          = e25d159ef03993c1e886f1a322afda07e7f4bb82   (base anterior 1b091be; CI 34789277774 success)
> G7A archive           = archive/i-54-g7a-pre-rebase-e25d159 -> e25d159ef03993c1e886f1a322afda07e7f4bb82
> Current base          = 104ef3a1b1df249d6e0a56dc4ad3846912b24f12   (main, merge de I-53S E2)
> G7A post-rebase       = 5c16ae8a8134e3b956459bd91ba15d097a24fc34   (CI 34789745328 success)
> G7_SHA                = 18cf2dcc55c1eb0f0c826e1716a43a2ae0a7cf71   (CI 34792787919 success, 4/4)
> G7A process deviation = CORRECTED / NON-MATERIAL / CLOSED
> G7_CLOSE_SHA          = 02987bd18ef0904a0332556928503a9afaeddcd1   (CI 34794374450 success, 4/4)
> Coordinator Review    = G3 ACCEPTED; G4 ACCEPTED / COMPLETE; G5 ACCEPTED / COMPLETE; G6 ACCEPTED / COMPLETE;
>                         G7 ACCEPTED / COMPLETE
> Candidate SHA         = 02987bd18ef0904a0332556928503a9afaeddcd1   (G8; = G7_CLOSE_SHA)
> Candidate evidence    = Core Full local PASS; UI Full local PASS; Debug UI build PASS; Debug Plugin build PASS;
>                         CI 34794374450 success 4/4
> Candidate DLL         = RackCad.Plugin.dll 1.0.0+02987bd18ef0904a0332556928503a9afaeddcd1
>                         SHA-256 5D289A811F4FDF699E7AF5F4931B6CF540671CFDB1684290957264FE85AA6441
> Owner Validation      = PASS   (OV-01..OV-14 PASS; literal: «Apruebo validación»)
> Final rebase          = NONE   (origin/main sigue en 104ef3a1b1df249d6e0a56dc4ad3846912b24f12)
> CLOSURE_SHA           = este commit (docs-only; NO reemplaza al Candidato)
> MERGE_SHA             = PENDING hasta el merge
> G3                    = COMPLETE
> G4                    = COMPLETE
> G5                    = COMPLETE
> G6                    = COMPLETE
> G7                    = COMPLETE
> G8                    = COMPLETE / CANDIDATE
> G9                    = COMPLETE
> G10                   = INTEGRATION
> ```
>
> **G3, G4, G5, G6 y G7 son los gates de implementacion ejecutados.** G3 entrego el nucleo puro de Custom Properties en
> Application:
>
> - **Modelo y documento puro**: `CustomPropertiesDocument` `{SchemaVersion, Entries[{Id, Name, Value}]}` y
>   `CustomPropertyEntryDocument`, con `ExtensionData` en raiz y en entrada. La identidad estable es
>   `CustomPropertyId`: GUID en forma D exacta, igual por valor, escrito en minusculas y con `Guid.Empty` invalido.
> - **Store**: `CustomPropertiesStore` lee el payload tri-estado del NOD (`CustomPropertiesPayload`) y el miembro
>   `JsonElement` del rack, clasifica en el orden de D-07.4 y serializa sin degradar la version.
> - **Resultados tipados**: `CustomPropertiesReadResult` (Absent, Readable, PresentButUnreadable, IncompatibleMajor,
>   AmbiguousIdentity y DepthLimitExceeded) y `CustomPropertiesMutationResult`, con rechazos
>   `CustomPropertiesRejection`.
> - **Validaciones**: UTF-16 bien formado; en `Name`, no-caracteres antes de NFC, NFC, limites y controles; en
>   `Value`, limites y controles, sin normalizar nunca; profundidad <= 16. `CustomPropertiesWriteGuard` solo deja
>   escribir sobre Absent y Readable.
> - **Mutaciones puras**: `CustomPropertiesMutations` crea, renombra, cambia el valor y elimina sobre una copia, sin
>   tocar lo leido. Eliminar la ultima entrada deja `Entries` vacio y conserva el contenedor.
> - **Intent por id**: `CustomPropertiesIntent`. Toda operacion sobre una entrada existente va por id; el nombre nunca
>   localiza.
> - **Pruebas Core**: T-STO-01..20, T-MUT-01..06 y T-MUT-09, en verde.
>
> G4 entrego la preservacion del miembro en el sobre, en dos fases:
>
> - **G4A — caracterizacion de BASE**, sin tocar produccion: T-CHR-01..04 congelan los bytes de sobres sin el miembro,
>   el `Compose` de BASE, la ida y vuelta del store y el residual F-14b.
> - **G4B — el miembro en el sobre** (solo `RackEmbedDocument.cs` y `RackEmbedComposer.cs`):
>   - `RackEmbedDocument.CustomProperties`, de tipo `JsonElement?` y omitido cuando es nulo, sin cambiar
>     `CurrentSchemaVersion` (`"1.0"`);
>   - `Compose` hereda el miembro del origen, con la misma firma y los mismos llamadores;
>   - `WithCustomProperties(source, JsonElement?)` devuelve un sobre nuevo que solo cambia el miembro;
>   - los dos normalizan `Undefined` y `Null` a ausente y rechazan un origen cuyo `ExtensionData` repita un miembro
>     declarado;
>   - pruebas T-ENV-01..13, T-CPY-01 y guardas T-GRD-01..03, en verde.
>
> G5 entrego la autoridad de alcance Rack y el commit puro, en seis archivos nuevos de
> `src/RackCad.Application/CustomProperties/`:
>
> - **Proyeccion plana pura** (D-22.2): `RackCustomPropertiesDefinition` lleva handle, nombre de bloque, si esta
>   colocada, si depende de una xref y el sobre deserializado, o `null` si el payload no se interpreta.
>   `RackCustomPropertiesSelection` lleva el handle elegido y si viene de una referencia externa. Sin `ObjectId`,
>   `Database` ni `Transaction`.
> - **Pertenencia** (D-09.2): es miembro la definicion con sobre legible, `Id` no vacio e igual con `OrdinalIgnoreCase`,
>   y no dependiente de xref. Los miembros van en orden de handle, y el orden del barrido no cambia el resultado.
> - **`IndeterminateMembership`** (D-09.3): basta una definicion no dependiente, colocada o no, con payload no
>   interpretable. Se bloquean las escrituras de Rack del dibujo, con diagnostico de bloque, handle y colocacion. Un
>   sobre legible con `Id` en blanco no bloquea; si es el elegido, da `NoIdentity`.
> - **xref** (D-09.4 y D-22.6): elegir una referencia externa o una definicion dependiente da `XrefRejected`. Las
>   dependientes no son miembros, no bloquean y nunca entran en un plan.
> - **`MixedKind`** (D-09.5): algun `Kind` en blanco, con diagnostico de esa definicion, o mas de un kind distinto,
>   conocidos o no.
> - **`UnknownKind` con predicado inyectado** (C-1): `isKnownKind` llega desde el borde, y un kind comun que el build no
>   conoce queda en solo lectura. Application no depende de `KindHandlerRegistry`.
> - **Clasificacion por coleccion** (D-09.6): cada miembro lee su propia coleccion con el store de G3. Si alguna no es
>   escribible, el rack es `CustomPropertiesReadOnly`; ese miembro nunca se omite para seguir con los demas.
> - **Igualdad canonica** (D-09.7): `CustomPropertiesCanonicalForm` es un texto canonico que ignora el minor:
>   - `ExtensionData` en profundidad, con miembros ordenados, arrays en orden, strings por valor y numeros por texto
>     crudo;
>   - `Id` por valor de GUID;
>   - `Absent` equivale al vacio con cualquier minor.
> - **Orden unico de ocho resultados** (D-09.8): `XrefRejected`, `NoIdentity`, `IndeterminateMembership`, `MixedKind`,
>   `UnknownKind`, `CustomPropertiesReadOnly`, `Divergent` y `Single`. Solo `Single` expone la coleccion del rack, con
>   version de escritura = el minor mayor, nunca menor que la actual.
> - **Unificacion segura** (D-09.10 y C-5):
>   - solo desde `Divergent`, con origen explicito sin valor por defecto (`RackCustomPropertiesUnifyIntent`) y
>     confirmacion;
>   - la bloquea un destino con `ExtensionData` de raiz o de entrada, o con un minor mayor que el del origen;
>   - un origen `Absent` es el vacio canonico (C-5A);
>   - solo se escriben los miembros distintos del origen (PR-07).
> - **Workspace puro** (D-18.3): `CustomPropertiesWorkspace.ForProject` y `ForRack`, sin tipos de UI.
>   - Estados: `Editable`, `ReadOnly` con motivo y `Divergent`.
>   - Filas `{Id, Name, Value, NombreRepetido}` solo si es editable; ademas, resumenes por vista y definiciones no
>     interpretables.
> - **Preflight**: `CustomPropertiesPreflight` da un aviso temprano sobre la instantanea y nunca es autoritativo.
> - **Commit fresco** (D-22.5): `CustomPropertiesCommit.ForRack` y `ForRackUnify` reevaluan desde cero la proyeccion
>   fresca y devuelven un plan o una negativa tipada (`CustomPropertiesCommitRefusal`). Las negativas cubren:
>   - seleccion ausente o autoridad no escribible;
>   - rack cambiado, miembros cambiados o, al unificar, alguna forma mostrada cambiada;
>   - rack que ya no diverge, unificacion no disponible o sin confirmar;
>   - mutacion rechazada.
> - **Plan serializado completo** (D-22.10): cada payload parte del sobre fresco de su miembro con
>   `WithCustomProperties`, y todos se serializan antes de devolver el plan. Si uno lanza, por ejemplo por F-14b en otro
>   campo, la excepcion se propaga y no hay plan parcial. F-14b no se corrige.
> - **Pruebas Core**: T-AUT-01..16, T-MUT-07, T-MUT-08 y T-MUT-10, en verde.
>
> G6 entrego el borde fisico en el Plugin, en dos archivos nuevos de `src/RackCad.Plugin/`: `CustomPropertiesData.cs` y
> `CustomPropertiesExecutor.cs`. Lee el dibujo hacia las entradas puras de G3..G5 y escribe exactamente lo que
> Application decide, sin decisiones propias.
>
> - **Proyecto** (D-07):
>   - **Clave** `RACKCAD_CUSTOM_PROPERTIES` del NOD, declarada una sola vez, en `CustomPropertiesData.DictKey`.
>   - **Xrecord directo**: ni subdiccionario, ni hijo de otra entrada, ni parte de otro documento.
>   - **Lectura fisica tri-estado**, en la transaccion del llamador:
>     - sin entrada, `Absent`;
>     - entrada que no es Xrecord, Xrecord sin datos o sin texto, o fallo del acceso de AutoCAD, `PresentButUnreadable`;
>     - trozos de texto concatenados en orden, `Present`.
>
>     No lanza por el contenido del dibujo: sin casts que lancen y capturando solo la familia de excepciones de AutoCAD.
>     El JSON solo lo interpreta `CustomPropertiesStore`.
>   - **Escritura** del texto ya serializado y acreditado, en trozos `DxfCode.Text` de 255 caracteres como maximo. Una
>     entrada que no sea Xrecord nunca se sustituye.
>   - **Ejecutor de Proyecto con lectura fresca** (D-07.5), en una transaccion y con un solo `Commit`: relee, acredita
>     en el store, exige `Absent` o `Readable`, aplica el intent por id, consulta la guarda, serializa y escribe.
>     Cualquier negativa sale sin confirmar. Sin regeneracion.
>   - **Lectura del workspace**: NOD → `CustomPropertiesStore` → `CustomPropertiesWorkspace.ForProject`.
> - **Rack** (D-09.1, D-22):
>   - **Barrido** `ScanEnvelopes` con `includeReferenceCount: true`, una vez por operacion y nunca `FindRackBlocks`.
>   - **Proyeccion plana**: handle, nombre de bloque, colocada por el recuento de referencias directas, `IsDependent`
>     del registro de bloque y el sobre, o `null` si el payload no se interpreta. Incluye los ilegibles y no filtra por
>     rack, colocacion ni kind.
>   - **Mapa handle → `ObjectId`**, solo en el Plugin: ningun `ObjectId` llega a Application.
>   - **Proyeccion de la seleccion**: el handle de la definicion elegida y si viene de una referencia externa.
>   - **`isKnownKind`**, construido en el borde desde `KindHandlerRegistry.Default.TryGetIgnoreCase`; el borde no mira
>     ningun kind por su cuenta.
>   - **Ejecutor CRUD con lectura fresca** (`CustomPropertiesCommit.ForRack`) y **ejecutor de unificacion con lectura
>     fresca** (`CustomPropertiesCommit.ForRackUnify`), sin repetir ninguna regla de Application.
>   - **Plan fisico**: con una negativa no se escribe nada; con un plan, todos los handles se resuelven antes de la
>     primera escritura y cada payload se escribe con `RackBlockData.Write`.
>   - **Una transaccion y un `Commit`** por ejecutor. Una excepcion al preparar el plan (el residual F-14b, por ejemplo)
>     se propaga antes de cualquier escritura, y F-14b no se corrige.
>   - Sin `Regen`, `RedefineSystemBlock`, `EnsureForPlan`, `PurgeUnreferenced` ni `PurgeAfterCommit`.
> - **Pruebas y guardas**: T-PRJ-01..04 y T-GRD-04..07, en verde. El Plugin no se carga en ninguna suite (ADR-0003):
>   el borde se fija por su forma, con guardas estructurales demostradas en rojo, y por el contrato puro que consume.
>
> G7 entrego la superficie de usuario —comando, ventana, ayuda y censos—, sin cambiar la semantica de G3..G6:
>
> - **Comando** (`src/RackCad.Plugin/RackPropiedadesCommands.cs`): `[CommandMethod("RACKPROPIEDADES")]` y
>   `[CommandMethod("RPR")]`, con el mismo comportamiento (el alias llama al mismo metodo), y la interaccion «Selecciona
>   un rack o [Proyecto]».
>   - **Proyecto**: `ReadProject` → ventana → intent → `ExecuteProject` → relectura. No existe API de preflight temprano
>     de Proyecto; el ejecutor relee en su transaccion fisica y sigue siendo la autoridad.
>   - **Rack**: eleccion → resolucion de la definicion → `ReadRack` → instantanea de lo mostrado → ventana → preflight
>     (`ForRack` o `ForRackUnify`) → `ExecuteRack` o `ExecuteRackUnify` → relectura.
>   - La lectura y la escritura de las dos ramas pasan por `CustomPropertiesExecutor`; el comando no llama a la
>     autoridad ni al commit.
> - **Ventana** `RackCustomPropertiesWindow`, arquetipo C, la misma para Rack y Proyecto:
>   - editable: crear, renombrar, cambiar valor y eliminar por id estable;
>   - solo lectura desde la apertura, con su motivo, en todos los estados de D-09 y D-13;
>   - `Divergent`: solo resumenes por vista, sin filas del rack;
>   - unificar: solo con las opciones que da Application, sin origen por defecto, una vista `Absent` como «vacío / sin
>     propiedades» y con confirmacion;
>   - sin descarte, sin `MessageBox` y sin integracion en los editores de sistema.
> - **Ayuda**: el par `RACKPROPIEDADES`/`RPR` en `RackCommandReference`.
> - **Censos**: comandos 33 → 35, ventanas 29 → 30 (arquetipo C 11 → 12) y pares de ayuda 14 → 15.
> - **Pruebas y guardas**: U-01..U-10 y T-GRD-08, en verde. La interaccion fisica del comando es de G9.
>
> **G3 NO toco** el sobre exterior (`RackEmbedDocument`), `Compose`, la autoridad por `RackId`, Plugin, el NOD fisico,
> UI ni AutoCAD; tampoco Domain ni las pruebas existentes. **G4 NO toco** Plugin (ni `RackEnvelopeRestamp`, ni
> `RackCloner`, ni los llamadores de `Compose`), `RackEmbedStore`, `SchemaVersionPolicy`, la autoridad por `RackId`, el
> NOD, UI, Domain ni AutoCAD.
>
> **G5 NO toco** Plugin, AutoCAD, el NOD fisico, comandos, UI, el ejecutor de Proyecto ni el ejecutor fisico de Rack.
> Tampoco Domain, el sobre (`RackEmbedDocument`, `RackEmbedComposer`, `RackEmbedStore`), el codigo de G3 y G4, el
> restamp, `ScanEnvelopes`, el registro de handlers, Project Variables, la biblioteca ni las pruebas existentes.
>
> **G6 NO añadio** ningun `[CommandMethod]`, nombre de comando, alias, WPF, XAML ni ayuda, y no hizo Owner Validation:
> el censo de comandos quedo en 33 → 33. Tampoco toco Application, Domain, UI, `ProjectVariables*`, `RackBlockFinder`,
> `RackBlockData`, `RackEnvelopeRestamp`, `RackCloner`, el codigo de G3..G5 ni las pruebas existentes.
>
> **G7 NO toco** Domain, Application, el codigo de G3..G6 (incluidos `CustomPropertiesData` y
> `CustomPropertiesExecutor`), el sobre, el compositor, Project Variables, `RackSelectiveWindow` ni ningun otro editor
> de sistema, ni la biblioteca, y no hizo Owner Validation ni trabajo de Candidato. G8..G10 no se inician sin orden
> propia del Coordinador.
>
> La rama esta rebasada sobre `origin/main` @ `104ef3a` (merge de I-53S E2) desde la correccion de G7A; antes lo estuvo
> sobre `1b091be` (merge de I-53 E1), desde G4-REBASE-CLOSE, y sobre `f8deb67`, desde G2G. `main` avanzo a mitad de la
> sesion de G4, despues de su preflight, y el rebase se hizo en la sesion siguiente con la historia previa preservada en
> `archive/i-54-g4-pre-rebase-78e6696`. G5, G5-CLOSE, G6 y G6-CLOSE no rebasaron. En G7, `main` avanzo a `104ef3a`
> despues del preflight de G7A, y G7A se commiteo igualmente sobre la base anterior (`e25d159`): fue una **desviacion
> de proceso**, corregida antes de cualquier codigo de G7B con el tag `archive/i-54-g7a-pre-rebase-e25d159`, el rebase
> de toda la rama sobre `104ef3a` (G7A = `5c16ae8`) y CI en verde. G7B (`18cf2dc`) y G7-CLOSE van sobre esa misma
> historia rebasada. Los SHAs citados en este contrato son los **originales** de cada gate: siguen siendo la autoridad
> de su evidencia y no se sustituyen. Sus equivalentes estan en el registro de decisiones: §14 para el rebase de G4 y
> §18 para el de G7. Los veredictos pertenecen a sus SHAs y no se transfieren.

> **Apertura por autorizacion explicita del Owner sin fila previa**, transmitida por el Coordinador de
> I-54 — caso (d) de [WORKFLOW](../WORKFLOW.md) seccion 2. Esa autorizacion sustituye **unicamente** la
> preexistencia de la fila en ROADMAP: la fila durable y este contrato nacen en el bootstrap
> inmediatamente posterior al reclamo atomico. Hasta G2I **no existio** `docs/automation/decisions/I-54.md` —la
> norma **no lo exige** para el caso (d)— y `decision_paths` quedo vacio en vez de apuntar a un archivo inventado.
> G2-FREEZE lo crea para registrar el consenso tecnico y ADR-0039, y desde entonces `decision_paths` apunta a el.

```text
Initiative = I-54
Owner ID   = ID24 — Custom Properties Foundation
Branch     = architecture/propiedades-personalizadas
Worktree   = ~/.codex/worktrees/architecture-propiedades-personalizadas
BASE_SHA   = 46fcac2b071929d2bd5b07aa28373941417f74a8   (origin/main, merge de I-51)
CLAIM_SHA  = 143490d8ecfb1c5f3011cf752c5cfcd11af13784   (Claim-Id d4b871e9-8a5d-4e67-bc11-a8f3c023788e)
             equivalente tras el rebase de G2G: c60c17fa46ef1eb2e9ab696432721d7649fe7c1d (mismo commit vacio y Claim-Id)
```

## 1. Objetivo

Fundar en RackCad **propiedades personalizadas**: metadatos con nombre y valor definidos por el usuario,
en dos alcances —**Proyecto** (el dibujo) y **Rack**—, persistidos en el DWG con identidad estable. La
fundacion **no** convierte esas propiedades en variables de proyecto ni en expresiones, y deja
**declarados, no implementados**, los puntos de extension hacia el dibujo, las expresiones y las
plantillas.

G0, G1 y G2 **no** entregan la fundacion. Entregan la **evidencia** (G1), la **primera propuesta** (Proposal V1),
su revision de Arquitecto (G2B), la **reconciliacion** (Proposal V2, G2C), la revision exact-SHA de V2 (G2D), la
**segunda reconciliacion** (Proposal V3, G2E), la revision exact-SHA de V3 (G2F), la reconciliacion tras el rebase
(Proposal V4, G2G), la revision exact-SHA de V4 (G2H) y la **reconciliacion final** (Proposal V5, G2I). Coordinador
y Arquitecto convergieron sobre V5 en G2J. G2-FREEZE versiono ese consenso con ADR-0039 `propuesto`, el Owner lo
acepto (“Acepto”) y G2-FREEZE COMPLETION completo el Consensus Freeze, con CI verde. Por orden del Coordinador, G3
entrego la primera pieza de la fundacion: el nucleo puro de Application (documento, store, resultados tipados,
validaciones y mutaciones por id) con sus pruebas Core. G4 entrego la segunda: el miembro `CustomProperties` viaja en
el sobre de cada vista sin que el sobre lo interprete. G5 entrego la tercera, tambien en Application pura: la autoridad
por `RackId` sobre la proyeccion plana, el workspace, el preflight y el commit fresco con su plan completo. G6 entrego
la cuarta, en el Plugin: el borde fisico (la entrada del NOD de Proyecto y los ejecutores de Proyecto y de Rack), sin
comando ni UI. G7 entrego la quinta: el comando `RACKPROPIEDADES` (alias `RPR`, nombres decididos por el Owner), la
ventana `RackCustomPropertiesWindow` para Rack y Proyecto, la ayuda y los censos. G8 declaro Candidato `02987bd`, sin
commit nuevo; en G9 el Owner lo valido en AutoCAD (OV-01..OV-14 PASS, «Apruebo validación»), y G10 lo integra en
`main` sin rebase final.

## 2. Problema

Hoy RackCad persiste en el DWG dos clases de dato del usuario: el **sobre por rack**
(`RackEmbedDocument`, sobre la definicion de bloque; [ADR-0009](../adr/0009-identidad-guid-embebida-en-dwg.md))
y el **registro de variables de proyecto** (I-47,
[ADR-0034](../adr/0034-project-variables-autoridad-drawing-level.md)). Que metadatos de proyecto o de rack
existen ya —nombre, cliente, area, ubicacion, revision, notas, descripcion, codigo o equivalentes—, donde
viven y con que semantica **no se afirma aqui**: es el primer entregable de G1.

El Coordinador entrego hallazgos **preliminares** que G1 debe **confirmar o corregir** contra el codigo, sin
darlos por ciertos:

| # | Hallazgo preliminar del Coordinador |
|---|---|
| H-1 | `origin/main` observado antes de la ejecucion: `46fcac2b071929d2bd5b07aa28373941417f74a8` (no asumido: verificado en el preflight del reclamo) |
| H-2 | `RACKCAD_PROJECT` parece ser hoy un **Xrecord directo** del NOD, no un sub-`DBDictionary` |
| H-3 | `RackEmbedDocument` parece la **costura comun natural** para metadatos de nivel rack |
| H-4 | I-51 parece **conservar los campos adicionales del sobre exterior** al re-estampar; debe probarse contractualmente |
| H-5 | La exportacion a biblioteca del Selectivo parece derivarse del **diseno interior**, y por tanto no exportaria metadatos situados solo en el sobre |
| H-6 | I-49 **no** debe convertirse en dependencia |
| H-7 | I-50 puede tocar DTOs del Selectivo; hay que **medir** el cruce actual |

## 3. Alcance

1. **G0 — reclamo y bootstrap**: reclamo atomico publicado, este contrato y la fila en ROADMAP.
2. **G1 — Discovery, solo documentacion.** Auditar por archivo y simbolo:
   - metadatos existentes: `ProjectName`, `Client`/`Cliente`, `Area`, `Location`/`Ubicacion`, `Revision`,
     `Notes`, `Description`, `Code` y equivalentes;
   - `RACKCAD_PROJECT`, `ProjectVariablesData`/`Registry`/`Document`/`Store` y la semantica
     ABSENT / PRESENT-BUT-UNREADABLE;
   - `RackEmbedDocument`, `RackEmbedStore`, `RackBlockData`, `RackId` y definicion frente a referencia;
   - autoridad entre vistas hermanas;
   - `RACKEDITAR` y los handlers por kind: como redibuja y como crea vistas cada sistema;
   - I-51: `RackDuplicationPlan`, `RackEnvelopeRestamp`, `RackDuplicarCommands`, `RackCloner`;
   - biblioteca, `.rackcad.json`, exportacion/importacion y el cruce real entre dibujos;
   - politicas de schema y version, y `JsonExtensionData`;
   - patrones reutilizables de UI y CRUD, en especial `RACKVARIABLES`;
   - serializadores y ayudantes reutilizables;
   - confirmar o corregir H-1..H-7.
3. **G2 — Proposal.** En esta sesion, solo la **V1**. Compara explicitamente:
   **A** `Dictionary<string,string>`, **B** registros con id estable y **C** documentos de propiedad tipados;
   y decide o propone: identidad; alcances Proyecto/Rack; solo texto frente a Texto/Numero/Booleano;
   persistencia de proyecto; persistencia de rack; autoridad entre hermanas; renombrar y borrar;
   semantica de duplicacion e independencia; biblioteca/exportacion; schema malformado o futuro; punto
   de extension futuro hacia el dibujo; punto de extension futuro hacia expresiones **sin implementar
   expresiones**; punto de extension futuro hacia plantillas; y la UI minima reutilizable.

   El Coordinador pidio **evaluar especialmente** —y **no aceptar por instruccion**— esta opcion:
   `CustomPropertyId + Name + string Value`; Proyecto = documento versionado **independiente** en el NOD,
   **sin** migrar `RACKCAD_PROJECT` salvo evidencia fuerte; Rack = campo **aditivo comun** en
   `RackEmbedDocument`, no en DTOs de producto; los mismos metadatos en **todas** las vistas hermanas del
   `RackId`; renombrar conserva el id; duplicar copia los valores en **colecciones independientes**; ni
   ProjectVariables, ni parser, ni formulas.
4. **Publicacion** en la rama: informe Discovery; Proposal V1; mapa de archivos y simbolos con los cruces
   con las ramas activas; decisiones materiales; riesgos; recomendacion de ADR si/no; y un **paquete
   autonomo** para la revision de Arquitecto.
5. **Implementacion, Candidato, validacion e integracion**: sus gates son los de la Proposal V5 §14 (G3..G10),
   congelados por el consenso de G2J. Con el freeze completo y su CI en verde, el Coordinador autorizo cada gate con su
   propia orden: G3, G4, G5, G6 y G7 estan **COMPLETOS** (§8 y §14), y G7 se ejecuto con la decision del Owner sobre
   el nombre del comando (D-18.1, OQ-03: `RACKPROPIEDADES` con alias `RPR`). G8 declaro el Candidato, G9 cerro la
   validacion del Owner con PASS y G10 integra.

## 4. Fuera de alcance

- **Cualquier cambio de produccion en G0, G1 y G2**: nada en `src/`, `tests/`, `assets/`, `eng/`,
  `deploy/` ni `.github/`. Cada gate de implementacion, por orden propia, se limito a lo suyo, con pruebas nuevas:
  - G3, G4 y G5 solo tocaron Application: el nucleo puro; en G4, `RackEmbedDocument` y `RackEmbedComposer`; en G5,
    archivos nuevos de autoridad, workspace y commit.
  - G6 solo añadio dos archivos nuevos del Plugin: el borde fisico, sin comando ni UI.
  - G7 solo añadio el comando, la ventana independiente, la ayuda, los censos y sus pruebas de UI y Core, sin controles
    de propiedades en ningun editor de sistema.
  - G8 y G9 no cambiaron ningun archivo. G10 solo toca documentacion y hace el merge.
- `docs/HANDOFF.md`: solo en la sesion de integracion (G10), en el ultimo commit de la rama ([WORKFLOW](../WORKFLOW.md)
  §4.5.4); antes, prohibido por las ordenes y por la seccion 2 de WORKFLOW.
- **La semantica de Project Variables** (I-47, ADR-0034) y de la **edicion vinculable** (I-48): una
  propiedad personalizada no es una variable de proyecto, y esta iniciativa no reabre esa semantica.
- **Parser, formulas y expresiones** (ID22B, territorio de I-49). **I-49 no es dependencia** de I-54.
- **Cotas por vista** (I-50) y **RACKMIRROR** (I-52).
- **Migrar `RACKCAD_PROJECT`** sin evidencia fuerte; y aun con ella, eso seria decision, no implementacion.
- **Dibujar** propiedades en el DWG (atributos, textos, cuadros de rotulacion) y **plantillas**: solo se
  declaran sus puntos de extension.
- Los hallazgos laterales se registran —en el Discovery y, al fijar el contrato, en
  [ideas-futuras.md](../ideas-futuras.md)—; **no se arreglan aqui**.

## 5. Contexto requerido

- [ARCHITECTURE.md](../ARCHITECTURE.md): identidad del rack y persistencia embebida.
- [ADR-0009](../adr/0009-identidad-guid-embebida-en-dwg.md), [ADR-0010](../adr/0010-actualizar-redibuja-insertar-liga-vistas.md)
  y [ADR-0034](../adr/0034-project-variables-autoridad-drawing-level.md).
- Contrato de [I-11](I-11-persistencia-uniforme.md) (campos desconocidos y version no degradada).
- Decisiones de [I-47](../automation/decisions/I-47.md), Proposal V8 de [I-48](I-48-proposal-v8.md) y
  contrato y decisiones de [I-51](I-51-rackduplicar-multiples-origenes.md).
- Contratos de I-49, I-50 e I-52 **en sus ramas remotas**, solo lectura y en el SHA exacto que se cite.
- [AGENTS.md](../../AGENTS.md): convenciones de persistencia versionada, pruebas y evidencia.
- Context Packs declarados en el frontmatter.

## 6. Dependencias

- **Sin dependencias pendientes**: I-47, I-48 e I-51 estan integradas.
- **I-49 no es dependencia**, por instruccion expresa del Coordinador.
- **Iniciativas paralelas a vigilar.** Al reclamar, `origin` tenia `main`, **I-49**
  (`architecture/motor-expresiones-parametricas`, solo documentacion), **I-50**
  (`feature/cotas-independientes-por-vista`, con produccion en DTOs y Domain de Selectivo y Dinamico) e
  **I-52** (`feature/rackmirror-espejo-semantico`, reclamada durante el preflight). `conflicts_with` queda
  **vacio** hasta que G1 mida el cruce real de archivos: no se declara un estorbo sin evidencia, ni se
  omite uno que la tenga.
- **Entrada del Owner**: la Proposal V1 listo nueve preguntas de producto (OQ-01..OQ-09), y con esa evidencia
  `requires_owner_decision` paso a `true`. La V2 las **reclasifica** (§16 de la V2), la V3 mantiene esa
  clasificacion con una aclaracion en OQ-02 (la cota de profundidad no es tuning) y **ninguna bloquea el
  consenso**:
  - OQ-08 queda resuelta;
  - OQ-05 pasa a riesgo residual documentado;
  - OQ-02 se divide: la existencia de limites es arquitectura y los numeros son tuning;
  - las demas se difieren, cada una con su gate limite (el nombre del comando, antes de G7).

  La aceptacion explicita de ADR-0039 ya consta (G2-FREEZE COMPLETION), y la del nombre y el alias del comando tambien
  (G7A, OQ-03: `RACKPROPIEDADES` con alias `RPR`; el boton de menu sigue fuera de V1 por D-18.8). Sigue haciendo falta
  el ajuste del Owner de los limites de OQ-02, a mas tardar en G9, asi que `requires_owner_decision` sigue en `true`.
- **Paralelas re-medidas en G2C** ([Discovery](I-54-discovery.md) §2.6; Proposal V2 §13):
  - I-49 @ `ccf21c6` e I-53 @ `f38362d`: solo documentacion;
  - I-50 @ `6cd2970`: G3 en las ventanas del Selectivo, Dinamico y Push Back, fuera del mapa de I-54;
  - I-52 @ `0fc7032`: Proposal V1 y ADR-0036 `propuesto`, solo documentacion. Su espejo compone con el sobre
    fuente y re-estampa, asi que cumple el invariante de preservacion (D-21 de la V2). Conflictos textuales:
    censo de comandos y censo de llamadas a `Compose`.

  Ningun supuesto de I-54 queda invalidado, y `conflicts_with` sigue vacio.
- **Paralelas re-medidas en G2E**, en el preflight y antes de publicar ([Discovery](I-54-discovery.md) §2.7;
  Proposal V3 §2 y §13). Estado al publicar:
  - `main` @ `f8deb67`: **I-50 integrada** durante la sesion (su rama remota se retiro). No cambia ningun archivo
    de codigo del mapa de I-54; `src/` mantiene los censos que vigila I-54, y `git merge-tree` no da conflictos.
    ADR-0035 queda `aceptado` en `main`;
  - I-49 @ `048a508`: Proposal V6, solo documentacion; sigue reservando `Rack`/`Project` para ID20;
  - I-52 @ `0445718`: Proposal V2 y ADR-0036 corregido, solo documentacion; sigue cumpliendo D-21;
  - I-53 @ `d7f17ad`: Proposal V2 y ADR-0037 `propuesto`, solo documentacion.

  Ningun avance invalida C-1..C-8 ni una costura de I-54, y `conflicts_with` sigue vacio. **Sin rebase en G2E**:
  al abrir la sesion `origin/main` seguia en `46fcac2`. WORKFLOW §4.2 exige rebasar al abrir la proxima sesion
  que escriba en la rama; como reescribe los SHAs revisados, su momento exacto (G2-FREEZE o una V4, y siempre
  antes de G3) lo fija la orden del Coordinador.
- **Rebase y paralelas en G2G** (Proposal V4 §2 y §13). Por orden del Coordinador, la sesion G2G rebaso la rama
  sobre `origin/main` @ `f8deb67` antes de escribir (WORKFLOW §4.2): 6/6 commits, sin conflictos, mapa de SHAs en
  la V4 §2.2 y V3 byte a byte identica a la revisada. Estado al publicar V4, medido solo sobre C-F1 y C-F2:
  - I-49 @ `1ed93a0`: rebasada sobre `main` con la misma Proposal V6, solo documentacion;
  - I-52 @ `545c222`: Proposal V3 y ADR-0036 actualizado, solo documentacion; su espejo reserializa el sobre, asi
    que F-14a y F-14b le afectan como a los demas flujos que lo reescriben;
  - I-53 @ `4e00a27`: G3 con tipos nuevos en `Application/Systems/Shared` y pruebas, sin tocar sobre, `Compose`,
    Custom Properties, NFC ni UI.

  Ningun avance invalida C-F1 ni C-F2, y `conflicts_with` sigue vacio.
- **Paralelas en G2I** (Proposal V5 §2.4 y §13), medidas solo sobre C-F3 / F-14b. `main` sigue en `f8deb67`, asi
  que no hubo rebase.
  - I-49 @ `364d6c0`: solo documentacion (ADR-0038 aceptado y consensus freeze de su V6); no toca el sobre.
  - I-52 @ `545c222`: sin cambios; su espejo reserializa el sobre y puede compartir F-14b, sin cambiar C-F3.
  - I-53 @ `4e00a27`: sin cambios.

  `conflicts_with` sigue vacio.
- **Paralelas en G2-FREEZE** ([decisions/I-54.md](../automation/decisions/I-54.md) §9). `main` sigue en `f8deb67`,
  asi que no hubo rebase y el SHA acordado es el de la rama.
  - I-49 @ `364d6c0`: sin cambios.
  - I-52 @ `8e2ae4f`: un commit nuevo, solo documentacion (Proposal V4 y ADR-0036 actualizado). ADR-0036 sigue
    `propuesto`, mantiene las propiedades de rack de I-54 como portador y su espejo sigue cumpliendo D-21.
  - I-53 @ `7de424e`: un commit nuevo, su G4 productivo, en `src/RackCad.Application/Systems/Selective/` y pruebas,
    fuera del mapa de I-54; no toca el sobre ni `docs/adr/`.
  - Censo de ADR: `main` llega a 0035; 0036 (I-52), 0037 (I-53) y 0038 (I-49) estan en sus ramas; 0039 estaba libre
    en todos los refs y se asigna a I-54.

  `conflicts_with` sigue vacio.
- **Paralelas en G2-FREEZE COMPLETION** ([decisions/I-54.md](../automation/decisions/I-54.md) §11). `main` sigue en
  `f8deb67`, sin rebase.
  - I-49 @ `82aa61b`: un commit nuevo, solo documentacion (su contrato e `ideas-futuras.md`); no toca `docs/adr/`. Es
    el cruce documental ya previsto al final de `ideas-futuras.md`.
  - I-52 @ `8e2ae4f` e I-53 @ `7de424e`: sin cambios.
  - 0039 sigue siendo exclusivo de I-54 en todos los refs.

  `conflicts_with` sigue vacio.
- **Paralelas en G3 y G3-CLOSE** ([decisions/I-54.md](../automation/decisions/I-54.md) §13). `main` sigue en
  `f8deb67`, sin rebase.
  - I-49: en G3 seguia en `82aa61b`; ahora esta en `c4880af`, su G5 productivo, publicado despues de G3. Es el nucleo
    sintactico en `src/RackCad.Application/Expressions/`, con sus guardas y pruebas. Esas guardas barren Domain y
    `Application/Expressions`, y la lista de tokens prohibidos en Application no nombra nada de G3. No toca Custom
    Properties, el sobre ni `docs/adr/0039*`.
  - I-52 @ `e998a2b`: un commit nuevo desde `8e2ae4f`, medido en G3. Solo documentacion: Proposal V5, ADR-0036 y su
    registro.
  - I-53 @ `f271fd5`: un commit nuevo desde `7de424e`, medido en G3. Es su G6 productivo, el consumidor Dinamico del
    nucleo de cabeceras, en `Systems/Dynamic` de Application y Domain y en pruebas; queda fuera del mapa de I-54.
  - 0039 sigue siendo exclusivo de I-54 en todos los refs.

  `conflicts_with` sigue vacio.
- **Paralelas en G4 y G4-REBASE-CLOSE** ([decisions/I-54.md](../automation/decisions/I-54.md) §14).
  - `main`: al abrir G4 seguia en `f8deb67` y a mitad de la sesion avanzo a `1b091be`, el merge de I-53 E1, que queda
    integrada. No toca `RackEmbedDocument`, `RackEmbedComposer`, `RackEmbedStore`, `SchemaVersionPolicy`,
    `RackEnvelopeRestamp` ni `RackCloner`, no añade ni quita llamadas a `Compose` (el censo sigue en 7) y no nombra Custom
    Properties. G4-REBASE-CLOSE rebaso sobre el, con conflictos solo documentales.
  - I-49 @ `71268eb`: tras `c4880af`, un commit documental (amendment A1 de su V6), sin impacto en el sobre, el censo
    ni Custom Properties.
  - I-52 @ `e998a2b`: sin cambios.
  - I-53S @ `6ac42ca` (`feature/cabeceras-multidestino-selectivo`, nueva, sobre `1b091be`): reclamo y bootstrap, solo
    documentacion, sin impacto.
  - 0039 sigue siendo exclusivo de I-54 en todos los refs.

  `conflicts_with` sigue vacio.
- **Paralelas en G5 y G5-CLOSE** ([decisions/I-54.md](../automation/decisions/I-54.md) §15). `main` sigue en
  `1b091be`, sin rebase.
  - I-49: en G5 seguia en `71268eb`. Durante G5-CLOSE se publico rebasada sobre `1b091be`, en `cae7a9f`.
    - `git range-diff` da 12 commits `=` y 4 `!`, que solo cambian por contexto documental (ROADMAP, indice de ADR e
      `ideas-futuras.md`).
    - El delta de `src/` y `tests/` es identico.
    - Sin impacto en el sobre, el censo ni Custom Properties.
  - I-52 @ `91bdd38`: observado al abrir G5, un commit documental desde `e998a2b` (Proposal V6, ADR-0036 `propuesto` y
    su registro). No toca archivos de I-54 y registra el G4 de I-54 como no material para ella. El cruce ya previsto
    sigue igual: su Proposal V6 preve que su espejo lleve el censo de T-GRD-02 de 7 a 8 (D-21).
  - I-53S @ `6ac42ca`: sin cambios.
  - Censo de ADR: `main` tiene 0035 y 0037; 0036 sigue solo en I-52 y 0038 solo en I-49; 0039 sigue siendo exclusivo
    de I-54.

  G5 no añade llamadas a `Compose` (el censo sigue en 7) y ninguna paralela toca archivos de Custom Properties, del
  sobre ni de la autoridad de G5. `conflicts_with` sigue vacio.
- **Paralelas en G6 y G6-CLOSE** ([decisions/I-54.md](../automation/decisions/I-54.md) §16). `main` sigue en
  `1b091be`, sin rebase.
  - I-49: al abrir G6 estaba en `cae7a9f`; antes del commit de G6, en `c5b9ede`. Son dos commits solo de documentacion:
    ADR-0040 `propuesto` como sucesor de ADR-0038, su fila del indice y su registro.
  - I-52: al abrir G6 estaba en `91bdd38`; antes del commit de G6, en `b70b5bf`. Es un commit solo de documentacion:
    Proposal V7, ADR-0036 actualizado y su registro, que anota el G5 de I-54 como no material.
  - I-53S @ `e528ef2`: observado al abrir G6, un commit desde `6ac42ca`. Es su G5: la UI del Selectivo
    (`RackSelectiveWindow`) y sus pruebas de UI, sin Plugin, Application ni Custom Properties.
  - Censo de ADR: `main` tiene 0035 y 0037; 0036 sigue solo en I-52; 0038 y 0040 estan solo en I-49; 0039 sigue siendo
    exclusivo de I-54.

  Ninguna paralela toca `RackBlockFinder`, `RackBlockData`, `KindHandlerRegistry`, la infraestructura de transacciones
  del Plugin, `ProjectVariablesData`, `ProjectVariablesRegistry`, su ejecutor ni Custom Properties. G6 no añade comandos
  (el censo sigue en 33) ni llamadas a `Compose` (siguen 7). `conflicts_with` sigue vacio.
- **Paralelas al abrir G7** ([decisions/I-54.md](../automation/decisions/I-54.md) §17 y §18). En el preflight de G7A,
  `main` estaba en `1b091be`. **Correccion (G7-CLOSE):** despues de ese preflight `main` avanzo a `104ef3a` (merge de
  I-53S E2), y G7A se commiteo igualmente sobre la base anterior; la rama se rebaso sobre `104ef3a` antes de G7B (§18).
  - I-49 @ `2eeeab1`: un commit solo de documentacion desde `c5b9ede`, en el que el Owner acepta ADR-0040, que
    reemplaza a ADR-0038.
  - I-52 @ `b70b5bf`: sin cambios.
  - I-53S @ `d043afb`: un commit solo de documentacion desde `e528ef2`, que cierra su documentacion para integrarse
    (HANDOFF, ROADMAP, su registro, `ideas-futuras.md` y su contrato). Es dueña de la UI del Selectivo: G7 no toca
    `RackSelectiveWindow`.
  - Ninguna toca `RackCommandReference`, `DialogWindowChrome`, `EditorActions`, los censos de comandos o de ventanas ni
    Custom Properties. Las tres mantienen el censo de comandos en 33. `conflicts_with` sigue vacio.
- **Paralelas en G7B y G7-CLOSE** ([decisions/I-54.md](../automation/decisions/I-54.md) §18). `main` sigue en `104ef3a`
  desde el rebase de G7A: sin rebase nuevo.
  - I-53S: integrada en `main` (`104ef3a`); su rama remota se retiro.
  - I-49 @ `8ed15f7`: publicada rebasada sobre `104ef3a` (su historia previa, en su propio tag
    `archive/i-49-a1r3-pre-rebase-2eeeab1`), con un commit nuevo solo de documentacion: nuevo Consensus Freeze sobre su
    Proposal V6, el Amendment A1 y ADR-0040.
  - I-52 @ `e0a779f`: un commit solo de documentacion desde `b70b5bf` (Proposal V8, ADR-0036 actualizado y su registro).
  - I-53D @ `aa0f817` (`feature/cabeceras-multidestino-dinamico`, nueva, sobre `104ef3a`): reclamo y bootstrap, solo
    documentacion. Sera dueña de la UI del Dinamico, que G7 no toca.
  - Ninguna toca `RackCommandReference`, `DialogWindowChrome`, `EditorActions`, los censos de comandos, ventanas o ayuda
    ni Custom Properties, y `main` mantiene 33 `[CommandMethod]`. El cruce textual ya previsto con I-52 (censo de
    comandos y ayuda) queda ahora fijado por nombre: quien integre despues clasifica sus registros en T-GRD-08.
    `conflicts_with` sigue vacio.
- **Paralelas en G8, G9 y G10** ([decisions/I-54.md](../automation/decisions/I-54.md) §19). `main` sigue en `104ef3a`:
  sin rebase final.
  - I-49 @ `5f969cc`: su G6 productivo (nucleo semantico del motor de expresiones, unidades de longitud y pruebas), sin
    ningun archivo en comun con I-54.
  - I-52 @ `deb08cd`: un commit solo de documentacion desde `e0a779f` (Proposal V9, ADR-0036 actualizado y su registro).
  - I-53D @ `d280197`: su G7, la UI del Dinamico (`RackDynamicSystemWindow`, su ensamblador de Application y pruebas),
    sin ningun archivo en comun con I-54, sin comandos nuevos y sin tipos de ventana nuevos.
  - Tras este merge, quien integre despues re-mide los censos de comandos, ventanas y ayuda que fija T-GRD-08.
    `conflicts_with` sigue vacio.
- **Cruce medido en G1** ([Discovery](I-54-discovery.md) §2 y §13): cruce **productivo** actual con I-49,
  I-50, I-52 e I-53 = **cero archivos** mientras I-54 no toque DTO ni Domain de sistema, editores de sistema,
  `RackEnvelopeRestamp` ni `RackCloner`. Cruce **documental** previsto con las cuatro (fila de ROADMAP tras I-51,
  final de `ideas-futuras.md`, numeracion de ADR) y **textual** con I-52 si ambas añaden comandos (censo de
  `[CommandMethod]` y ayuda). Cruce **semantico** con I-49: `Rack`/`Project` reservados para ID20. Por eso
  `conflicts_with` sigue vacio.

## 7. Archivos esperados

**Este contrato no fija la lista de archivos de produccion, y no debe fingir que si.** El mapa por archivo
y simbolo **es el entregable de G1**, y los archivos de produccion los fija el consenso de G2.

Lo que si se declara para G0..G2: solo `docs/initiatives/I-54-*.md`, y la fila de I-54 en `docs/ROADMAP.md` solo
en el bootstrap. G2C, G2E, G2G y G2I tocan **unicamente** `docs/initiatives/I-54-*.md`; el rebase de G2G solo
reaplica los commits existentes de I-54. G2-FREEZE toca ademas, conforme a la Proposal V5 §14 y a la orden del
Coordinador, `docs/adr/0039-custom-properties-persistencia-autoridad.md` (nuevo), `docs/adr/README.md` (fila del
indice), `docs/automation/decisions/I-54.md` (nuevo) y `docs/ideas-futuras.md` (F-01..F-13, F-14a, F-14b y la mejora
de D-11.6); no toca `docs/ROADMAP.md` ni `docs/HANDOFF.md`. G2-FREEZE COMPLETION toca solo ADR-0039 (encabezado y
bloque «Aceptación del Owner»), `docs/adr/README.md`, `docs/automation/decisions/I-54.md` y este contrato. G3, por
orden del Coordinador, crea nueve archivos de produccion en `src/RackCad.Application/CustomProperties/` y
`src/RackCad.Application/Persistence/CustomProperties*.cs`, crea tres archivos de pruebas Core en
`tests/RackCad.Tests/CustomProperties*.cs` y anota la traza en `docs/automation/decisions/I-54.md`. G3-CLOSE toca solo
este contrato y ese registro. G4A crea dos archivos de pruebas Core (`CustomPropertiesEnvelopeTestKit.cs` y
`CustomPropertiesEnvelopeCharacterizationTests.cs`). G4B modifica `src/RackCad.Application/Persistence/RackEmbedDocument.cs`
y `RackEmbedComposer.cs` y crea `CustomPropertiesEnvelopeTests.cs` y `CustomPropertiesEnvelopeGuardTests.cs`. El rebase
de G4-REBASE-CLOSE solo reaplica los commits de I-54; sus conflictos, todos documentales (`docs/ROADMAP.md`,
`docs/adr/README.md` e `ideas-futuras.md`), se resuelven conservando los dos lados. G4-CLOSE toca solo este contrato y
el registro de decisiones. G5 crea seis archivos de produccion en `src/RackCad.Application/CustomProperties/`
(`RackCustomPropertiesDefinition.cs`, `CustomPropertiesCanonicalForm.cs`, `RackCustomPropertiesAuthority.cs`,
`RackCustomPropertiesDisplayedState.cs`, `CustomPropertiesWorkspace.cs` y `CustomPropertiesCommit.cs`) y cuatro de
pruebas Core (`CustomPropertiesAuthorityTestKit.cs`, `CustomPropertiesAuthorityTests.cs`,
`CustomPropertiesWorkspaceTests.cs` y `CustomPropertiesCommitTests.cs`), sin modificar ningun archivo existente.
G5-CLOSE toca solo este contrato, el registro de decisiones y, por orden del Coordinador, `docs/adr/README.md`, para
corregir un hecho temporal del parrafo de I-54. G6 crea dos archivos de produccion en `src/RackCad.Plugin/`
(`CustomPropertiesData.cs` y `CustomPropertiesExecutor.cs`) y dos de pruebas Core (`CustomPropertiesProjectTests.cs` y
`CustomPropertiesEdgeGuardTests.cs`), sin modificar ningun archivo existente. G6-CLOSE toca solo este contrato y el
registro de decisiones, igual que G7A. El rebase de G7 solo reaplica los 20 commits de I-54 sobre `104ef3a`; sus dos
conflictos, documentales (`docs/ROADMAP.md` en el bootstrap e `ideas-futuras.md` en el freeze), se resuelven
conservando los dos lados. G7B toca exactamente diez archivos:
- nuevos: `src/RackCad.Plugin/RackPropiedadesCommands.cs`, `src/RackCad.UI/RackCustomPropertiesWindow.xaml` y su
  `.xaml.cs`, `tests/RackCad.Tests/CustomPropertiesCommandGuardTests.cs` y, en `tests/RackCad.UI.Tests/`,
  `CustomPropertiesWindowTests.cs`, `CustomPropertiesWindowTestKit.cs` y `CustomPropertiesHelpCensusTests.cs`;
- modificados: `src/RackCad.UI/RackCommandReference.cs` (la entrada de ayuda) y las guardas de censo
  `tests/RackCad.Tests/SelectiveEditorOpenTests.cs` y `tests/RackCad.UI.Tests/WindowCensusGuardTests.cs`.

G7B no toca Domain, Application, la semantica de G3..G6, el sobre ni el compositor, Project Variables, los editores de
sistema ni la biblioteca. G7-CLOSE toca solo este contrato y el registro de decisiones. G8 y G9 no tocan ningun
archivo. El cierre de G10 es solo documentacion: este contrato, el registro de decisiones, `docs/HANDOFF.md` y
`docs/ROADMAP.md` ([WORKFLOW](../WORKFLOW.md) §4.5.4), mas `README.md` —su tabla de comandos, porque I-54 añade un
comando de AutoCAD (WORKFLOW §8)— e `ideas-futuras.md` —el hallazgo F-15, fuera de alcance (WORKFLOW §5 y §8)—.
**Una desviacion material frente a esto obliga a detenerse.**

## 8. Fases

| # | Fase | Entregable | Estado |
|---|---|---|---|
| G0 | Reclamo + bootstrap | Reclamo atomico publicado, contrato y fila en ROADMAP | **HECHA** — bootstrap `f908b2f` |
| G1 | Discovery | [I-54-discovery.md](I-54-discovery.md): informe por archivo/simbolo, H-1..H-8, mapa de cruces, riesgos y hallazgos fuera de alcance | **HECHA** — `195964b` |
| G2 | Proposal y consenso | **G2A**: [Proposal V1](I-54-proposal-v1.md) y paquete de Arquitecto. **G2B**: Architect Review de V1. **G2C**: [Proposal V2](I-54-proposal-v2.md) y paquete reescrito. **G2D**: revision exact-SHA de V2. **G2E**: [Proposal V3](I-54-proposal-v3.md) y paquete reescrito. **G2F**: revision exact-SHA de V3, limitada a C-1..C-8. **G2G**: rebase obligatorio sobre `origin/main` y [Proposal V4](I-54-proposal-v4.md) con paquete reescrito para la revision exact-SHA (su SHA es el del commit que introduce la V4). **G2H**: revision exact-SHA post-rebase de V4, limitada a C-F1, C-F2 y la integridad del rebase. **G2I**: [Proposal V5](I-54-proposal-v5.md) (C-F3) y paquete reescrito para la revision final exact-SHA (su SHA es el del commit que introduce la V5). **G2J**: revision final exact-SHA de V5 por Coordinador y Arquitecto, limitada a AR-54-V4-01 / C-F3. **G2-FREEZE**: consenso, ADR `propuesto` y aprobacion del Owner. Cada una con orden propia. SHAs pre-rebase y sus equivalentes en la V5 §2.2 | **G2A HECHA** (`7c197af`); **G2B CERRADA** (AGREED WITH CHANGES sobre `7c197af`); **G2C HECHA** (`36c337b`); **G2D CERRADA** (AGREED WITH CHANGES sobre `36c337b`); **G2E HECHA** (`5d25da8`); **G2F CERRADA** (AGREED WITH CHANGES sobre `5d25da8`); **G2G HECHA** (`8bc991c`); **G2H CERRADA** (AGREED WITH CHANGES sobre `8bc991c`); **G2I HECHA** (`26ca923`); **G2J CERRADA** (Coordinator AGREED y Architect AGREED sobre `26ca923`); **G2-FREEZE PREP HECHA** (`d84f480`, CI 34748982744 verde); **G2-FREEZE COMPLETION HECHA** (`3866252`, CI 34750208424 verde): ADR-0039 aceptado por el Owner (“Acepto”), Consensus Freeze COMPLETE |
| G3+ | Implementacion, Candidato, validacion del Owner, integracion | Gates G3..G10 de la [Proposal V5](I-54-proposal-v5.md) §14, congelados por el consenso de G2J | **iniciada**: **G3 HECHA** (`57f2b4e`, CI 34759032979 verde; aceptada por el Coordinador), G3 COMPLETE; **G3-CLOSE**: sincronizacion documental; **G4 HECHA** (G4A `dcc4048`, CI 34771155216; G4B `78e6696`, CI 34772215008; aceptada por el Coordinador), G4 COMPLETE; **G4-REBASE-CLOSE**: tag `archive/i-54-g4-pre-rebase-78e6696`, rebase sobre `1b091be`, equivalentes `b568bef` y `7b0f6a9` (CI 34773339064 verde), y cierre documental (`477c149`, CI 34773953746 verde); **G5 HECHA** (`3cf0d35`, CI 34778231177 verde; aceptada por el Coordinador), G5 COMPLETE; **G5-CLOSE**: sincronizacion documental y correccion factual del indice de ADR (`6b438be`, CI 34779470825 verde); **G6 HECHA** (`ca71962`, CI 34787646732 verde; aceptada por el Coordinador), G6 COMPLETE; **G6-CLOSE**: sincronizacion documental (`b4e99e4`, CI 34788418383 verde); **G7 HECHA** (aceptada por el Coordinador), G7 COMPLETE: **G7A** registra la decision del Owner (`RACKPROPIEDADES`, alias `RPR`), commiteada primero como `e25d159` sobre la base anterior `1b091be` (CI 34789277774), desviacion de proceso corregida con el tag `archive/i-54-g7a-pre-rebase-e25d159` y el rebase de la rama sobre `104ef3a` (equivalente `5c16ae8`, CI 34789745328 verde); **G7B**: comando, ventana, ayuda, censos, U-01..U-10 y T-GRD-08 (`18cf2dc`, CI 34792787919 verde); **G7-CLOSE**: sincronizacion documental (`02987bd`, CI 34794374450 verde); **G8 HECHA**, G8 COMPLETE: Candidato `02987bd` (= G7-CLOSE, sin commit nuevo), con Core Full, UI Full y los builds Debug de UI y de Plugin en local sobre ese SHA y CI 34794374450 4/4; **G9 HECHA**, G9 COMPLETE: Owner Validation **PASS**, OV-01..OV-14 PASS, «Apruebo validación»; **G10**: cierre documental e integracion por merge `--no-ff`, sin rebase final; el CI del merge con su cobertura y la cobertura diferida del Candidato siguen pendientes al escribir este cierre |

Ninguna fase posterior arranca sin que la anterior tenga evidencia revisable.

## 9. Pruebas y builds

Las fija la Proposal V5 §12, congelada por el consenso de G2J, y se ejecutan a partir de G3. Lo que ya es exigible
por norma y no depende de G2: las **dos suites** en local sobre el Candidato, **CI verde sobre el SHA exacto**, y
builds Debug de UI y de Plugin (AGENTS.md, «Pruebas — definicion de terminado»). G0, G1 y G2 no producen codigo: su
evidencia es documental. G3, G4, G5 y G6 si producen codigo, y su evidencia es la suite Core en local mas la CI verde
sobre su SHA exacto (§14); G6, que toca el Plugin, añade el build Debug del Plugin en local. Tras el rebase de
G4-REBASE-CLOSE, la evidencia de G3 y G4 se volvio a medir sobre `7b0f6a9`: los SHAs anteriores no la transfieren. La de
G5 se midio sobre `3cf0d35` y la de G6 sobre `ca71962`. G7 toca UI y Plugin: su evidencia, medida sobre `18cf2dc`,
despues del rebase de G7A, es la suite Core completa, la de UI completa segun LC-UI, las pruebas de UI afectadas, los
builds Debug de UI y de Plugin en local y la CI 4/4 sobre ese SHA exacto (§14).

Ninguna suite carga el Plugin (ADR-0003): el borde de G6 y el comando de G7 se prueban por su forma, con guardas
estructurales, y por el contrato puro que consumen, sin ejecutar AutoCAD. Ninguno de estos gates es Candidato. De G3 a
G6, por LC-UI, la evidencia de UI fue la de la CI, sin UI Full ni build Debug de UI en local; G7 si los ejecuto en
local, porque toca UI. Aun asi, las dos suites y los builds Debug de UI y de Plugin se volvieron a exigir sobre el SHA
del Candidato: G8 los ejecuto sobre `02987bd` con el arbol limpio, y su CI de `push` es la corrida 34794374450 (§14).
El cierre de G10 es un SHA nuevo y solo documental: su evidencia propia es la prueba mecanica de que no toca producto
y su CI exact-SHA ([WORKFLOW](../WORKFLOW.md) §4.5.4).

## 10. Validacion manual

**Requerida** en la implementacion: persistir datos del usuario en el DWG y hacerlos sobrevivir a
`RACKEDITAR`, a la duplicacion y a guardar/reabrir cambia el comportamiento del dibujo (AGENTS.md,
punto 5). El **checklist concreto** es OV-01..OV-14 de la Proposal V5 §12.8, en G9. G0, G1 y G2 **no** la
requieren: no tocan producto. G3, G4 y G5 tampoco: son nucleo puro, sobre y autoridad de Application, sin AutoCAD. G6
tampoco: era un borde fisico que ningun comando alcanzaba. G7 conecta ese borde a un comando y a una ventana, pero no
ejecuto AutoCAD: la interaccion fisica del comando quedo para G9.

**G9 — validacion del Owner: PASS.** El Owner valido en AutoCAD el Candidato `02987bd`, sobre el DLL Debug de G8
(`RackCad.Plugin.dll` `1.0.0+02987bd18ef0904a0332556928503a9afaeddcd1`, SHA-256
`5D289A811F4FDF699E7AF5F4931B6CF540671CFDB1684290957264FE85AA6441`): **OV-01..OV-14 PASS**, resultado global PASS, con
aprobacion explicita «Apruebo validación». La ruta y el SHA-256 de la biblioteca de bloques, la version o build exacta
de AutoCAD y la granularidad observada de `UNDO` (OV-08) **no constan registrados** en la transcripcion del
Coordinador, y no se afirman. **Owner Validation = PASS.**

## 11. Criterios de aceptacion

Redaccion preliminar, anterior a G2. El consenso de G2J los concreta en la Proposal V5 (objetivos §4, invariantes
§7, y pruebas y validacion §12) y en ADR-0039, aceptado. Solo recogen lo que ya es doctrina del repositorio o
limite expreso de la orden:

1. Las propiedades de ambos alcances sobreviven a guardar, cerrar y reabrir el mismo DWG.
2. Un contenedor presente pero ilegible, o de un schema futuro, **no** se trata como vacio
   (`UNKNOWN/UNREADABLE != ABSENT`, ADR-0034).
3. Ninguna propiedad personalizada se lee, escribe ni resuelve como variable de proyecto, expresion o
   formula.
4. El formato persistido existente sigue abriendose, y un binario anterior no destruye en silencio lo que
   no entiende, en la medida que fije la Proposal.

## 12. Condiciones para detenerse

- **COMPUERTA VIGENTE — G10, integracion** ([WORKFLOW](../WORKFLOW.md) §4.5).
  - Este cierre documental es el ultimo commit de la rama; su CI exact-SHA tiene que estar en verde antes del merge.
  - Antes del merge, `origin/main` tiene que seguir en `104ef3a`. Si avanzara, **no se mergea**: un rebase crearia SHAs
    nuevos y la evidencia del Candidato y la validacion del Owner no se transferirian.
  - Merge `--no-ff` en `main`, sin squash, cherry-pick ni rebase-merge; ningun cambio de producto en esta fase.
  - Despues del merge: CI sobre el `MERGE_SHA` 4/4 con el artifact `rackcad-coverage-cobertura`, y cobertura diferida
    del Candidato por `workflow_dispatch` con `candidate_sha = 02987bd18ef0904a0332556928503a9afaeddcd1`. Solo con
    las dos en verde se retiran la rama y el worktree; los tags `archive/*` se conservan.

  (El freeze quedo en `d84f480` y `3866252`; G3 en `57f2b4e`; G4 en `dcc4048` y `78e6696`, preservados en
  `archive/i-54-g4-pre-rebase-78e6696`; G5 en `3cf0d35`; G6 en `ca71962`; G7A en `e25d159`, preservado en
  `archive/i-54-g7a-pre-rebase-e25d159`, con equivalente rebasado `5c16ae8`; G7B en `18cf2dc`; G7-CLOSE y el Candidato
  en `02987bd`.)
- **Condiciones para iniciar la implementacion**, todas **cumplidas**:
  - `Coordinator = AGREED` y `Architect = AGREED` sobre la **misma** Proposal: **cumplida** en G2J, sobre `26ca923`;
  - aceptacion **explicita** de ADR-0039 por el Owner: **cumplida** (“Acepto”, 2026-09-13);
  - CI verde del commit de G2-FREEZE COMPLETION: **cumplida** (`3866252`, CI 34750208424 `success`);
  - la orden de G3 del Coordinador: **cumplida**, limitada a G3;
  - la orden de G4 del Coordinador: **cumplida**, limitada a G4;
  - la orden de G5 del Coordinador: **cumplida**, limitada a G5;
  - la orden de G6 del Coordinador: **cumplida**, limitada a G6;
  - la orden de G7 del Coordinador: **cumplida**, limitada a G7;
  - la orden de G8 del Coordinador: **cumplida**, limitada a G8;
  - G9, validacion del Owner: **cumplida**, PASS;
  - la orden de G10 del Coordinador: en ejecucion en esta sesion.
- **Cada gate, de G8 a G10, exigio su propia orden del Coordinador.**
- **El nombre del comando esta decidido** (D-18.1, OQ-03). G6-CLOSE lo dejo `PENDING`, con `RACKPROPIEDADES` y `RPR`
  como nombres de trabajo. El Owner lo decidio antes de G7, de forma explicita y definitiva para la V1 de I-54, y el
  Coordinador lo transmitio con la orden de G7:

  ```text
  Owner Command Naming Decision = ACCEPTED
  Primary Command               = RACKPROPIEDADES
  Alias                         = RPR
  Owner wording                 = "Acepto RACKPROPIEDADES con alias RPR."
  Date                          = 2026-09-13
  G7                            = AUTHORIZED BY COORDINATOR
  ```

  No hay otros alias, ni comandos separados por alcance: un solo comando atiende Rack y Proyecto. El boton de menu sigue
  fuera de V1 (D-18.8). G7B los registro exactamente asi en `18cf2dc`, y T-GRD-08 los fija por nombre.
- Si G1 encuentra **archivos productivos compartidos materiales con I-49, I-50 o I-52**: reportarlo antes
  de continuar.
- Si la Proposal exige cambiar el **formato persistido existente** (major de `RackEmbedDocument`, forma de
  `RACKCAD_PROJECT`) o la semantica de identidad del rack: **ADR y decision del Owner antes de implementar**.
- Si el alcance deriva hacia Project Variables, expresiones, dibujo de propiedades o plantillas:
  **detenerse** (seccion 4).
- `docs/HANDOFF.md` solo se toca en el cierre de G10 (§4).

## 13. Estado versionado y entrega del Pull Request

Sin `docs/automation/state/I-54.yml`: la automatizacion esta **desactivada** (`automation.enabled:
false`) y el caso (d) no lo exige. [WORKFLOW](../WORKFLOW.md) seccion 8 pide para el bootstrap del caso (d)
exactamente **fila en ROADMAP mas contrato**. El estado vivo se deriva, como manda la seccion 2, de la
existencia de `origin/architecture/propiedades-personalizadas`.

No hay Pull Request. El merge automatico esta prohibido; la integracion es manual (WORKFLOW 4.5): en G10, merge
`--no-ff` de esta rama en `main`, por orden del Coordinador.

## 14. Evidencia final

**Reclamo atomico (G0).**

```text
Iniciativa = I-54 - ID24 - Custom Properties Foundation
Rama       = architecture/propiedades-personalizadas
Worktree   = ~/.codex/worktrees/architecture-propiedades-personalizadas
BASE_SHA   = 46fcac2b071929d2bd5b07aa28373941417f74a8  (origin/main)
CLAIM_SHA  = 143490d8ecfb1c5f3011cf752c5cfcd11af13784  (commit vacio)
Claim-Id   = d4b871e9-8a5d-4e67-bc11-a8f3c023788e
```

Primer `git push -u origin architecture/propiedades-personalizadas` **aceptado sin force**
(`* [new branch]`), con `origin` sin ninguna referencia a I-54 ni a ID24 en el preflight. `main` **no fue
modificada**.

**G1 — Discovery.** Solo documentacion, sobre el codigo de `BASE_SHA` (la rama no difiere en `src/`,
`tests/` ni `assets/`). Preflight de G1: `origin/main` sin mover; I-49 y I-52/I-53 solo documentacion; I-50
con produccion que **no** toca sobre, compositor, restamp, cloner ni autoridad. Sin compilacion ni pruebas:
G1 no produce codigo. Los hallazgos fuera de alcance (F-01..F-12 en G1; F-13 añadido en G2A) quedan en el Discovery y pasan a
`ideas-futuras.md` cuando se fije el contrato.

**G2A — Proposal V1.** Solo documentacion. Compara A/B/C, evalua componente a componente la opcion del
Coordinador sin aceptarla por instruccion, propone D-01..D-20, INV-01..INV-16, gates G2B..G10 no autorizados,
pruebas T/U, validacion OV-01..OV-12 y preguntas OQ-01..OQ-09, y recomienda ADR antes de implementar. El
paquete autonomo de Arquitecto lista RD-01..RD-20 con evidencia verificable. Addendum al Discovery (§2.5:
re-medicion de paralelas antes de publicar —I-50 @ `8ceb3a7`, I-52 @ `339b3ab`, I-53 @ `c8476cc`, sin cambio de
cruce—; §11.5: lista verificada de comandos; §15: F-13, troceado de Xrecord duplicado). Sin compilacion ni pruebas.

**G2B — Architect Review de V1** (sobre `7c197af81b91df88366c873eddf9e11ddc5e87bb`): `GLOBAL VERDICT = AGREED WITH
CHANGES`, `G2B = CLOSED`, implementacion bloqueada. Resultado: 0 BLOCKER, 5 MATERIAL (AR-54-01..05), 13 MINOR
(AR-54-06..18) y 22 cambios vinculantes (V2-1..V2-22). RD-13 salio DISAGREE y las demas AGREE WITH CHANGE. La
revision no modifico el repositorio.

**G2C — Proposal V2.** Solo documentacion.

- **Nuevo**: [I-54-proposal-v2.md](I-54-proposal-v2.md), que incorpora V2-1..V2-22 con matriz de reconciliacion
  (§17) y traza AR-54-01..18 (§18, los cinco MATERIAL en `CLOSED` a la espera de re-verificacion). Tambien:
  - reescribe RD-01..RD-20 (§19: las veinte `ACCEPTED WITH RECONCILIATION`; RD-13 reformulada);
  - reclasifica OQ-01..OQ-09 (§16);
  - define el alcance candidato del ADR sin redactarlo (§15);
  - reconcilia las pruebas T, U y OV (§12) y los gates (§14);
  - declara trece precisiones del ejecutor (PR-01..PR-13, §21), con `OPEN DISAGREEMENT = NINGUNO`.
- **Reescrito**: el paquete de revision, que ahora apunta solo a V2.
- **Addendum**: Discovery §2.6 (re-medicion de paralelas).
- **Contrato**: este archivo, solo en lo que refleja G2C.

Nueva evidencia ejecutada [X] en una sonda de `System.Text.Json` fuera del repositorio. Sin compilacion ni pruebas
del repositorio: G2C no produce codigo.

**G2D — Architect Review exact-SHA de V2** (sobre `36c337b84c47f9ac7c97d860fce42d3a5f4370e8`): `GLOBAL VERDICT =
AGREED WITH CHANGES`, `G2D Architect Review = CLOSED`, implementacion bloqueada.

- AR-54-01, -02, -03 y -05 `CLOSED`; AR-54-04 `REOPENED` por la restriccion B (cota de profundidad).
- Hallazgos nuevos: 2 MATERIAL (AR-54-V2-01 kind desconocido escribible; AR-54-V2-02 cota fuera del ADR y
  contradicciones de edicion) y 4 MINOR (AR-54-V2-03..06: UTF-16 y excepciones, nombres repetidos anidados,
  huecos de la unificacion, `Id` textual).
- Ocho cambios vinculantes, C-1..C-8, que el Coordinador acepta.
- PR-03, -06, -07 y -13 con cambio; PR-12 en desacuerdo; las demas de acuerdo.
- RD-01, -03, -04, -07, -10, -13, -19 y -20 AGREE WITH CHANGE; las demas AGREE.
- `Kind` en blanco = opcion A; ADR requerido con cambios de alcance; ningun bloqueo del Owner.

La revision no modifico el repositorio.

**G2E — Proposal V3.** Solo documentacion.

- **Nuevo**: [I-54-proposal-v3.md](I-54-proposal-v3.md), que es V2 con exactamente C-1..C-8:
  - `UnknownKind` de solo lectura con `isKnownKind` inyectado (PR-12 rechazada);
  - cota 16 como constante del formato 1.x con `DepthLimitExceeded`;
  - clases de excepcion explicitas y UTF-16 validado antes de NFC y de serializar, con el residual preexistente
    del sobre declarado (F-14 para el freeze);
  - nombres repetidos a cualquier profundidad;
  - origen `Absent` canonico, revalidacion de todos los miembros y atomicidad logica al unificar;
  - `Id` igual por valor;
  - alcance del ADR en 16 puntos;
  - orden unico de ocho resultados de autoridad.

  Trazas en §17..§19 y §21: C-1..C-8 `INCORPORATED`, AR-54-V2-01..06 y AR-54-04 `CLOSED IN V3` pendientes de
  re-verificacion, `OPEN DISAGREEMENT = NINGUNO`.
- **Reescrito**: el paquete de revision, limitado a C-1..C-8.
- **Addendum**: Discovery §2.7 (re-medicion de paralelas).
- **Contrato**: este archivo, solo en lo que refleja G2D y G2E.

Las sondas de `System.Text.Json` que sostienen C-2..C-4 se re-ejecutaron fuera del repositorio sobre .NET 8.0.29.
Durante la sesion `origin/main` avanzo a `f8deb67` (integracion de I-50). V3 se publica sin rebase y sin force,
con la medicion de §6. Sin compilacion ni pruebas del repositorio: G2E no produce codigo.

**G2F — Architect Review exact-SHA de V3** (sobre `5d25da89972df2468f1d03243301761f5463e6eb`, CI 34737707481
verde): `Coordinator = AGREED`; `GLOBAL VERDICT = AGREED WITH CHANGES`, `G2F Architect Review = CLOSED`,
implementacion bloqueada.

- C-1, C-2 y C-4..C-8 `VERIFIED`; C-3 `DEFECT`.
- Dos hallazgos MINOR con evidencia ejecutada:
  - **AR-54-V3-01**: `Normalize(FormC)` lanza con UTF-16 bien formado que contiene no-caracteres;
  - **AR-54-V3-02**: un sobre con un surrogate escapado se lee, pero toda reescritura lanza.
- AR-54-04, AR-54-V2-01, -02 y -04..-06 `CLOSED`; AR-54-V2-03 `REOPENED` solo por esos dos puntos.
- PR-03, -06, -07 y -13 `AGREED`; PR-12 `RECONCILED`.
- RD-04 y RD-07 AGREE WITH CHANGE; RD-01, -03, -10, -13, -19 y -20 AGREE.
- `AUTHORITY ORDER = AGREED`; `ADR REQUIRED = YES`; `ADR SCOPE = AGREED`.
- `main` @ `f8deb67` sin impacto sobre C-1..C-8.

El Coordinador acepta la lista cerrada de cambios. La revision no modifico el repositorio.

**G2G — Rebase y Proposal V4.** Solo documentacion.

- **Rebase** sobre `origin/main` @ `f8deb675c6d1ef0e64693b157d69c4cc170d7b24` (WORKFLOW §4.2): 6/6 commits, sin
  conflictos. `git range-diff` da cinco `=` y el bootstrap `!` solo por contexto de `ROADMAP.md`. El V3 rebasado
  (`ff98b9ee95ea7c4f8da0b90fe949b8ebb42157ed`) es byte a byte el revisado en G2F. Mapa de SHAs en la V4 §2.2;
  los historicos no se sustituyen.
- **Nuevo**: [I-54-proposal-v4.md](I-54-proposal-v4.md), que es la V3 rebasada con exactamente:
  - **C-F1**: un `Name` con no-caracteres es invalido al escribir y `PresentButUnreadable` al leer, antes de NFC;
    `Value` sin cambios;
  - **C-F2**: residual F-14b declarado junto a F-14a; INV-07, D-13 y RP-11 acotados a sobres que BASE puede leer y
    reserializar; sin commit si una serializacion lanza; caracterizacion en T-CHR-04 y T-ENV-13.

  AR-54-V3-01, AR-54-V3-02 y AR-54-V2-03 quedan `CLOSED IN V4`, pendientes de re-verificacion; RD-04 y RD-07
  `ACCEPTED WITH FINAL RECONCILIATION`; §15 identica a V3.
- **Reescrito**: el paquete de revision, limitado a C-F1, C-F2 y la integridad del rebase.
- **Contrato**: este archivo, solo en lo que refleja G2F y G2G. El Discovery no cambia.

F-14a y F-14b se registran en `ideas-futuras.md` en G2-FREEZE, como reserva §4. La historia rebasada se publica
con `git push --force-with-lease` (WORKFLOW §4.3). Sin compilacion ni pruebas del repositorio: G2G no produce
codigo.

**G2H — Architect Review exact-SHA de V4** (sobre `8bc991c0e1854bc9eb4013421e01f0515c2e77ca`, CI 34742026072
verde): `Coordinator = AGREED`; `GLOBAL VERDICT = AGREED WITH CHANGES`, `G2H Architect Review = CLOSED`,
implementacion bloqueada.

- `REBASE INTEGRITY = VERIFIED`: V3 byte a byte identica, mapa de SHAs correcto y nada productivo.
- `C-F1 = VERIFIED` (AR-54-V3-01 y AR-54-V2-03 3B `CLOSED`; captura defensiva de `ArgumentException` `ACCEPTED`).
- `C-F2 = DEFECT`, solo por redaccion: AR-54-V3-02 y AR-54-V2-03 3C `CLOSED`;
  `ATOMIC SERIALIZATION FAILURE = VERIFIED`.
- `TRACEABILITY = VERIFIED`; RD-04 AGREE; RD-07 AGREE WITH CHANGE.
- Hallazgo nuevo **AR-54-V4-01** (MINOR): D-08.4, D-13 y RP-11 decian que los consumidores existentes «fallan sin
  escribir» ante F-14b. En realidad `RACKEDITAR` confirma una transaccion por vista y puede quedar parcialmente
  actualizado.
- Paralelas sin impacto material.

El Coordinador acepta el cambio. La revision no modifico el repositorio.

**G2I — Proposal V5.** Solo documentacion, sin rebase (`main` no avanzo).

- **Nuevo**: [I-54-proposal-v5.md](I-54-proposal-v5.md), que es V4 con exactamente **C-F3** (AR-54-V4-01):
  - F-14a y F-14b quedan separados;
  - F-14b describe que falla la reserializacion del sobre afectado, que el punto de fallo depende de la granularidad
    de cada consumidor existente y que `RACKEDITAR` puede quedar parcialmente actualizado (P-37, evidencia de codigo);
  - la atomicidad de D-22.10 queda como exclusiva del ejecutor de I-54;
  - D-13 separa la garantia de I-54 (A) de los residuales (B), y RP-11 se alinea.

  D-22.10, INV-07, las pruebas y §15 no cambian. AR-54-V4-01 queda `CLOSED IN V5`, pendiente de G2J; RD-04 `AGREED`;
  RD-07 `ACCEPTED WITH FINAL RECONCILIATION`, pendiente solo de confirmar la redaccion.
- **Reescrito**: el paquete de revision, limitado a AR-54-V4-01 / C-F3.
- **Contrato**: este archivo, solo en lo que refleja G2H y G2I. El Discovery y `ideas-futuras.md` no cambian.

Sin compilacion ni pruebas del repositorio: G2I no produce codigo.

**G2J — Revision final exact-SHA de V5** (sobre `26ca923492576185b753d6dbf2a852969df2accf`, CI 34747220760 verde):
`Coordinator = AGREED`; `GLOBAL VERDICT = AGREED`, `G2J Architect Review = CLOSED`.

- Q-1..Q-6 `VERIFIED`: descripcion de F-14b; ninguna afirmacion global de atomicidad para los consumidores
  existentes; garantia de D-22.10 intacta y exclusiva del ejecutor de I-54; D-13 y RP-11 alineados; redaccion futura
  de F-14 lista; integridad del alcance.
- AR-54-V4-01, AR-54-V3-01, AR-54-V3-02, AR-54-V2-03 y AR-54-04 `CLOSED`; RD-04 y RD-07 AGREE; `ADR REQUIRED = YES`;
  `ADR SCOPE = AGREED`; `Authority order = AGREED`.
- `New findings = NONE`; `Required Proposal changes = NONE`; paralelas sin impacto material sobre F-14b.
- `Consensus = READY FOR FREEZE`; implementacion bloqueada.

La revision no modifico el repositorio.

**G2-FREEZE — preparacion documental del freeze.** Solo documentacion, sin rebase: `main` no avanzo.

- **Nuevo**: [ADR-0039](../adr/0039-custom-properties-persistencia-autoridad.md) `propuesto`, que congela los
  dieciseis puntos de la V5 §15 sin reabrir decisiones y deja la aceptacion del Owner en `PENDING`.
- **Nuevo**: [decisions/I-54.md](../automation/decisions/I-54.md), con el consenso, el estado del freeze y la
  informacion para el Owner sobre R-12, F-14a y F-14b.
- **Indice de ADR**: fila 0039 `propuesto`; 0036..0038 siguen en sus ramas.
- **`ideas-futuras.md`**: F-01..F-13, con F-01 y F-06 enlazados a registros existentes; F-14a; F-14b con la
  redaccion de C-F3; y la mejora de D-11.6.
- **Tag** `archive/i-54-custom-properties-pre-rebase-5d25da8` → `5d25da8` (Proposal V3 pre-rebase, revisada en G2F),
  anotado y publicado en `origin`.
- **Contrato**: este archivo, con `decision_paths` y el estado del freeze.

La Proposal V5, V1..V4, el Discovery, el paquete, `ROADMAP.md` y `HANDOFF.md` no cambian. Sin compilacion ni pruebas
del repositorio: G2-FREEZE no produce codigo. El SHA de este commit y su CI se reportan al Coordinador.

**G2-FREEZE COMPLETION — aceptacion del Owner y Consensus Freeze completo.** Solo documentacion, sin rebase: `main` no
avanzo.

- **Preparacion verificada**: `d84f480f032f6a7e5e481359aeab9fd18f691fbf`, CI 34748982744 `success` con los cuatro
  jobs en verde.
- **Owner**: acepta ADR-0039 de forma explicita. Redaccion literal “Acepto” (2026-09-13), transmitida por el
  Coordinador con un contexto inequivoco: ADR-0039, I-54 y Proposal V5 @ `26ca923492576185b753d6dbf2a852969df2accf`.
- **ADR-0039** pasa a `aceptado`. Solo cambian su encabezado y su bloque «Aceptación del Owner»; desde «Contexto» hasta
  el final es byte a byte identico al de `d84f480`.
- **Indice de ADR**: fila 0039 `aceptado`.
- **[decisions/I-54.md](../automation/decisions/I-54.md) §11**: el acto, la secuencia, la matriz final de G2-FREEZE y
  las verificaciones. Deja `FREEZE_COMPLETION_SHA` y su CI pendientes, porque un commit no contiene su propio SHA.
- **Contrato**: este archivo.

Verificado sin cambios:
- el tag `archive/i-54-custom-properties-pre-rebase-5d25da8` apunta a `5d25da8` en local y en `origin`, y no se crea
  otro;
- F-14a y F-14b siguen en `ideas-futuras.md`, una sola vez cada uno y con la redaccion acordada;
- la Proposal V5, el Discovery, el paquete, `ideas-futuras.md`, `ROADMAP.md` y `HANDOFF.md` no cambian.

```text
Technical Consensus = REACHED
ADR-0039            = ACCEPTED
Consensus Freeze    = COMPLETE
Implementation      = BLOCKED hasta la CI verde de este commit; despues, NOT STARTED
G3                  = NOT AUTHORIZED hasta esa CI verde; despues, READY FOR COORDINATOR AUTHORIZATION
```

Sin compilacion ni pruebas del repositorio: G2-FREEZE COMPLETION no produce codigo. El SHA de este commit y su CI se
reportan al Coordinador.

**G3 — store puro, resultados tipados y mutaciones puras.** Es el primer gate de implementacion, por orden del
Coordinador (`IMPLEMENTATION AUTHORIZED FOR G3 ONLY`). Sin rebase: `main` no avanzo.

- **Freeze verificado**: `386625259922fc90ec735f739d4d5ed8e9510873`, CI 34750208424 `success` con los cuatro jobs en
  verde; se anota en el registro de decisiones, §11 y §12.
- **Produccion**, solo Application pura:
  - `CustomProperties/`: `CustomPropertyId`, `CustomPropertyText`, `CustomPropertiesIntent` y
    `CustomPropertiesMutations`;
  - `Persistence/`: `CustomPropertiesDocument`, `CustomPropertiesPayload`, `CustomPropertiesReadResult`,
    `CustomPropertiesWriteGuard` y `CustomPropertiesStore`.
- **Pruebas Core nuevas**: `CustomPropertiesStoreTests`, `CustomPropertiesMutationTests` y `CustomPropertiesTestKit`,
  con T-STO-01..20, T-MUT-01..06 y T-MUT-09.
  - **RED** antes de producir: la compilacion falla solo por los tipos de G3 ausentes; sobre un esqueleto temporal, que
    nunca se versiono, fallan 244 de 245 casos.
  - **GREEN**: focal 245/245, impacto 1032/1032 y Core completa 6533/6533 con 0 omitidas, antes del commit y otra vez
    sobre `57f2b4e` con el arbol limpio.
- **Commit** `57f2b4e062be6b8f83b27e0cdb1b2a42eb2a4799`. CI 34759032979 (push) `success` sobre ese head_sha exacto, con
  los cuatro jobs en verde: Tests (Domain + Application), Build UI, UI Tests y Build Plugin without AutoCAD.
- **No toco**: el sobre exterior, `Compose`, la autoridad por `RackId`, Plugin, el NOD fisico, UI, AutoCAD, Domain ni
  las pruebas existentes. Tampoco la Proposal V5, ADR-0039, el Discovery, `ideas-futuras.md`, `ROADMAP.md` ni
  `HANDOFF.md`.
- **Revision del Coordinador**: G3 = ACCEPTED / COMPLETE. Las precisiones de profundidad y de unificacion quedan en el
  registro de decisiones, §13.

**G3-CLOSE — sincronizacion documental tras G3.** Solo documentacion, sin rebase: `main` no avanzo.

- **G3 verificado**: la rama esta en `57f2b4e062be6b8f83b27e0cdb1b2a42eb2a4799`, igual a su upstream, y la CI
  34759032979 dio `success` sobre ese SHA exacto.
- **Contrato**: este archivo deja de describir el estado anterior a G3 y registra G3 COMPLETE, Implementation STARTED,
  G4 NOT AUTHORIZED y Owner Validation NOT STARTED. Las entradas anteriores de esta seccion se conservan como historia.
  El campo `status` sigue en `claimed`, como en las demas iniciativas en implementacion: Git y la evidencia prevalecen
  sobre el ([README](README.md)).
- **[decisions/I-54.md](../automation/decisions/I-54.md) §13**: traza de G3 COMPLETION, sin reescribir §1..§12.
- No cambian la Proposal V5, ADR-0039, el Discovery, `ideas-futuras.md`, `ROADMAP.md`, `HANDOFF.md`, `src/` ni
  `tests/`.

```text
Consensus Freeze = COMPLETE
ADR-0039         = ACCEPTED
G3               = COMPLETE
Implementation   = STARTED
G4               = NOT AUTHORIZED
Owner Validation = NOT STARTED
```

Sin compilacion ni pruebas del repositorio: G3-CLOSE no produce codigo. El SHA de este commit y su CI se reportan al
Coordinador.

**G4A — caracterizacion del sobre de BASE.** Primera fase de G4, por orden del Coordinador, sin cambios de produccion.

- **Pruebas Core nuevas**: `CustomPropertiesEnvelopeTestKit` y `CustomPropertiesEnvelopeCharacterizationTests`, con
  T-CHR-01..04 (103 casos). Corren sobre un codigo del sobre identico al de `BASE_SHA` (`46fcac2`) y congelan:
  - los bytes de sobres sin el miembro, de cada kind, con y sin `ExtensionData`;
  - el `Compose` de BASE y la ida y vuelta del store;
  - los residuales F-14a y F-14b.

  Un DTO local con la forma de BASE, atado al de produccion por una prueba de paridad, representa a los builds
  I-11..I-53.
- **Evidencia local**: focal 103/103 y Core 6636/6636, 0 omitidas.
- **Commit** `dcc404853cad549708738488966920331e884fee`; CI 34771155216 (push) `success`, 4/4.

**G4B — `CustomProperties` en el sobre.**

- **Produccion** (solo `RackEmbedDocument.cs` y `RackEmbedComposer.cs`):
  - miembro `CustomProperties` (`JsonElement?`, omitido si es nulo) sin cambiar `CurrentSchemaVersion`;
  - `Compose` hereda el miembro y normaliza `Undefined` y `Null`;
  - `WithCustomProperties(source, JsonElement?)`;
  - rechazo, antes de producir nada, de un origen cuyo `ExtensionData` repita un miembro declarado.
- **Pruebas Core nuevas**: `CustomPropertiesEnvelopeTests` (T-ENV-01..13 y T-CPY-01, 150 casos) y
  `CustomPropertiesEnvelopeGuardTests` (T-GRD-01..03, 22 casos).
- **RED**: sin produccion no compila (51 errores, todos por los simbolos de G4). Con un esqueleto temporal nunca
  versionado fallan 82 casos: 81 esperados y 1 defecto de la propia prueba, corregido. Cada guarda se vio en rojo con
  una violacion temporal.
- **GREEN local**: focal 275/275, G3+G4 520/520, impacto de persistencia y sobre 1509/1509 y Core 6808/6808, 0
  omitidas. Censo de `Compose`: las 7 llamadas de D-21.
- **Commit** `78e6696e64db50ffcfbcd7e87d7dfe0a9ca78417`; CI 34772215008 (push) `success`, 4/4.
- **No toco**: Plugin (restamp, cloner ni llamadores de `Compose`), `RackEmbedStore`, `SchemaVersionPolicy`, UI,
  Domain, Project Variables, biblioteca ni pruebas existentes. Nada de G5+.
- **Revision del Coordinador**: G4 = ACCEPTED / COMPLETE.

**G4-REBASE-CLOSE — archivo, rebase, revalidacion y cierre documental.**

- **Archivo**: el tag anotado `archive/i-54-g4-pre-rebase-78e6696` apunta a `78e6696` y esta publicado. Preserva por
  ancestralidad la Proposal V5 (`26ca923`), el freeze (`d84f480` y `3866252`), G3, G3-CLOSE, G4A y G4.
- **Rebase** de `f8deb67` a `main` @ `1b091be`, 14 de 14 commits.
  - Conflictos, todos documentales: `docs/ROADMAP.md` en el bootstrap, `docs/adr/README.md` en el freeze y en su
    completion, e `ideas-futuras.md` en el freeze.
  - Se resolvieron conservando los dos lados: lo que añade I-54 es byte a byte lo de los commits originales.
  - `git range-diff`: 11 commits `=` y 3 `!`, que solo cambian por contexto.
  - Publicado con `git push --force-with-lease` sobre `78e6696`.
- **Mapa de SHAs** (completo en el registro de decisiones, §14). Los originales siguen siendo la autoridad de su
  evidencia:

  | Hito | Original | Post-rebase |
  |---|---|---|
  | Proposal V5 | `26ca923492576185b753d6dbf2a852969df2accf` | `64efe8d067c820274f76c4588efe58ccc924b000` |
  | Freeze completion | `386625259922fc90ec735f739d4d5ed8e9510873` | `238cbf705c219d29f42eb6faa5e8d49056f87c7f` |
  | G3 | `57f2b4e062be6b8f83b27e0cdb1b2a42eb2a4799` | `92a61f14e2aac4c1fcfee14c97cbea2c4a3d73f9` |
  | G3-CLOSE | `17b08f8eafbdc0b9dc99334fc79363873b9aca7d` | `4e75bc339f5fe0bba5a49a0a087d61213ba4590e` |
  | G4A | `dcc404853cad549708738488966920331e884fee` | `b568bef50fc2e9db0ddfc65d882669db6dc036ef` |
  | G4 | `78e6696e64db50ffcfbcd7e87d7dfe0a9ca78417` | `7b0f6a9ca849044acd5326f4bbcc68b6aa8fa5f0` |

- **Equivalencia**: sin cambios materiales, asi que no hace falta una nueva Architect Review.
  - La Proposal V5 (blob `a75444702ad354b35b67bfbbf5bb955913a02c81`) y ADR-0039 (blob
    `539065e645aa73105c6a0b6d71d4431f9eef19e2`) son identicos.
  - Los deltas de G3, G3-CLOSE, G4A y G4 son identicos, y tambien el delta de `src/` y `tests/` de I-54 respecto de su
    base (18 archivos, 5689 lineas).
  - `RackEmbedDocument`, `RackEmbedComposer`, `SchemaVersionPolicy`, `RackEnvelopeRestamp` y `RackCloner` no cambian
    entre el tip original y el rebasado.
- **CI post-rebase**: 34773339064 (push), `head_sha` `7b0f6a9ca849044acd5326f4bbcc68b6aa8fa5f0`, `success`, 4/4.
- **Revalidacion local sobre `7b0f6a9`**:
  - G3 245/245, G4 275/275 y G3+G4 520/520;
  - impacto de persistencia y sobre 1517/1517: los 1509 de G4 mas 8 pruebas de persistencia que trajo I-53;
  - Core 6971/6971, 0 omitidas;
  - T-GRD-01, T-GRD-02 y T-GRD-03 en verde; el censo real de `Compose` sigue en 7.
- **G4-CLOSE**: este contrato y el registro de decisiones (§14). La Proposal V5, ADR-0039, el Discovery,
  `ideas-futuras.md`, `ROADMAP.md`, `HANDOFF.md`, `src/` y `tests/` no cambian.

```text
Consensus Freeze = COMPLETE
ADR-0039         = ACCEPTED
G3               = COMPLETE
G4               = COMPLETE
Implementation   = STARTED
G5               = NOT AUTHORIZED
Owner Validation = NOT STARTED
```

Sin compilacion ni pruebas nuevas en G4-CLOSE: es un commit documental. Su SHA y su CI se reportan al Coordinador.

**G5 — autoridad, pertenencia, preflight, commit y workspace.** Por orden del Coordinador (`G5 IS AUTHORIZED`; G6..G10
NOT AUTHORIZED). Sin rebase: `origin/main` seguia en `1b091be`.

- **G4-CLOSE verificado**: `477c149fb0d57bbd263d38c87b3d29467ac27816`, CI 34773953746 `success`.
- **Produccion**, solo Application pura: seis archivos nuevos en `CustomProperties/`.
  - `RackCustomPropertiesDefinition`, con `RackCustomPropertiesSelection`.
  - `CustomPropertiesCanonicalForm`.
  - `RackCustomPropertiesAuthority`.
  - `RackCustomPropertiesDisplayedState`, con `RackCustomPropertiesUnifyIntent`.
  - `CustomPropertiesWorkspace`.
  - `CustomPropertiesCommit`, con `CustomPropertiesPreflight`.
- **Pruebas Core nuevas**: `CustomPropertiesAuthorityTests`, `CustomPropertiesWorkspaceTests`,
  `CustomPropertiesCommitTests` y `CustomPropertiesAuthorityTestKit`, con 143 casos:
  - T-AUT-01..16, T-MUT-07, T-MUT-08 y T-MUT-10;
  - el preflight y la igualdad canonica.
- **RED**:
  - sin produccion no compila: 36 errores unicos, todos CS0246 por los tipos de G5;
  - con las entradas neutralizadas temporalmente (API presente y cuerpo que lanza, nunca versionado), fallan 141 de 143;
  - las 2 que pasan son estructurales: el orden del enum y los tipos sin UI.
- **GREEN local** (SDK 8.0.423):
  - G5 focal 143/143;
  - CustomProperties combinado (G3, G4 y G5) 663/663;
  - impacto de persistencia, sobre y guardas (el filtro de G4) 1660/1660;
  - Core 7114/7114, 0 omitidas.
- **Commit** `3cf0d35099a6b91ced32b818fee969147f11fc77`; CI 34778231177 (push) `success` sobre ese head_sha exacto, con
  los cuatro jobs en verde.
- **No toco**:
  - Plugin, AutoCAD, el NOD fisico, comandos ni UI;
  - el ejecutor de Proyecto ni el ejecutor fisico de Rack;
  - Domain, `RackEmbedDocument`, `RackEmbedComposer`, `RackEmbedStore` ni el codigo de G3 y G4;
  - el restamp, `ScanEnvelopes`, el registro de handlers, Project Variables ni la biblioteca;
  - las pruebas existentes.

  El censo de `Compose` sigue en 7. Nada de G6+.
- **Revision del Coordinador**: G5 = ACCEPTED / COMPLETE. Las tres precisiones (seleccion ausente, lectura
  indeterminada e instantanea del CRUD normal) quedan en el registro de decisiones, §15.

**G5-CLOSE — sincronizacion documental tras G5 y correccion del indice de ADR.** Solo documentacion, sin rebase: `main`
no avanzo.

- **G5 verificado**: la rama esta en `3cf0d35099a6b91ced32b818fee969147f11fc77`, igual a su upstream, y la CI
  34778231177 dio `success` sobre ese SHA exacto.
- **Contrato**: este archivo registra G5 COMPLETE, Implementation STARTED, G6 NOT AUTHORIZED y Owner Validation NOT
  STARTED, con la sintesis factual de G5. Las entradas anteriores de esta seccion se conservan como historia.
- **[decisions/I-54.md](../automation/decisions/I-54.md) §15**: traza de G5 COMPLETION con las tres precisiones del
  Coordinador, sin reescribir §1..§14.
- **Indice de ADR** (`docs/adr/README.md`): el parrafo de I-54 decia que 0036, 0037 y 0038 «aún no figuran en este
  índice», pero 0037 figura desde la integracion de I-53 E1 en `main` (`1b091be`). Solo se corrige ese hecho temporal:
  0036 y 0038 siguen solo en las ramas de I-52 e I-49. El estado, el contenido y la numeracion de ADR-0039 no cambian.
- No cambian la Proposal V5, ADR-0039, el Discovery, `ideas-futuras.md`, `ROADMAP.md`, `HANDOFF.md`, `src/` ni
  `tests/`.

```text
Consensus Freeze = COMPLETE
ADR-0039         = ACCEPTED
G3               = COMPLETE
G4               = COMPLETE
G5               = COMPLETE
Implementation   = STARTED
G6               = NOT AUTHORIZED
Owner Validation = NOT STARTED
```

Sin compilacion ni pruebas nuevas en G5-CLOSE: es un commit documental. Su SHA y su CI se reportan al Coordinador.

**G6 — persistencia fisica y ejecutores del Plugin.** Por orden del Coordinador (`G6 IS AUTHORIZED`; G7..G10 NOT
AUTHORIZED; sin comando y sin WPF). Sin rebase: `origin/main` seguia en `1b091be`.

- **G5-CLOSE verificado**: `6b438be79ccc469bf7a40ddac19311a3b238781d`, CI 34779470825 `success`.
- **Produccion**, solo dos archivos nuevos del Plugin:
  - `CustomPropertiesData`: la entrada `RACKCAD_CUSTOM_PROPERTIES` del NOD como Xrecord directo, con lectura fisica
    tri-estado y escritura en trozos `DxfCode.Text` de 255 caracteres como maximo;
  - `CustomPropertiesExecutor`: la lectura de Proyecto hacia su workspace; el ejecutor de Proyecto; el barrido y su
    proyeccion plana; la seleccion; la lectura de Rack; los ejecutores de CRUD y de unificacion; y el predicado
    `isKnownKind`.
- **Pruebas Core nuevas**: `CustomPropertiesProjectTests` y `CustomPropertiesEdgeGuardTests`, con 84 casos:

  | ID | Casos | | ID | Casos |
  |---|---|---|---|---|
  | T-PRJ-01 | 16 | | T-GRD-04 | 25 |
  | T-PRJ-02 | 2 | | T-GRD-05 | 8 |
  | T-PRJ-03 | 2 | | T-GRD-06 | 9 |
  | T-PRJ-04 | 12 | | T-GRD-07 | 10 |

- **RED**:
  - sin produccion, 84 ejecutadas y 49 fallidas, todas por los archivos del Plugin ausentes o la clave del NOD sin
    declarar; las 35 que pasan son detecciones sobre archivos inventados y el contrato puro ya existente;
  - violaciones temporales en archivos reales o temporales, nunca versionadas: 9 de 9 en rojo, restauradas con hash
    verificado.
- **GREEN local** (SDK 8.0.423):
  - G6 focal 84/84;
  - CustomProperties combinado (G3, G4, G5 y G6) 747/747;
  - impacto con el filtro de G5 1744/1744, y ampliado con las guardas de fuente del Plugin 2007/2007;
  - Core 7198/7198, 0 omitidas, antes del commit y otra vez sobre `ca71962` con el arbol limpio.
- **Build Debug del Plugin**: 0 errores y 2 avisos MSB3277 de las referencias de AutoCAD, preexistentes (los mismos en
  `6b438be`); ningun aviso atribuible a G6.
- **Commit** `ca7196268ceb73d8c5223b508bbe9570698b38d7`; CI 34787646732 (push) `success` sobre ese head_sha exacto, con
  los cuatro jobs en verde.
- **No añadio** ningun `[CommandMethod]`, nombre de comando, alias, WPF, XAML ni ayuda, ni Owner Validation. El censo
  de comandos sigue en 33 → 33.
- **No toco** Application, Domain, UI, `ProjectVariables*`, `RackBlockFinder`, `RackBlockData`, `RackEnvelopeRestamp`,
  `RackCloner`, el codigo de G3..G5 ni las pruebas existentes.
- **Revision del Coordinador**: G6 = ACCEPTED / COMPLETE. Sus precisiones —lectura de un rack sin payload RackCad,
  frontera del ejecutor para G7, evidencia de T-PRJ-03 y validacion fisica diferida a G9— quedan en el registro de
  decisiones, §16.

**G6-CLOSE — sincronizacion documental tras G6.** Solo documentacion, sin rebase: `main` no avanzo.

- **G6 verificado**: la rama esta en `ca7196268ceb73d8c5223b508bbe9570698b38d7`, igual a su upstream, y la CI
  34787646732 dio `success` sobre ese SHA exacto.
- **Contrato**: este archivo registra G6 COMPLETE, Implementation STARTED, G7 NOT AUTHORIZED, Owner Command Naming
  Decision PENDING y Owner Validation NOT STARTED, con la sintesis factual de G6. Las entradas anteriores de esta
  seccion se conservan como historia.
- **[decisions/I-54.md](../automation/decisions/I-54.md) §16**: traza de G6 COMPLETION con las precisiones del
  Coordinador y la decision pendiente del Owner sobre el nombre del comando, sin reescribir §1..§15.
- No cambian la Proposal V5, ADR-0039, el Discovery, `ideas-futuras.md`, `docs/adr/README.md`, `ROADMAP.md`,
  `HANDOFF.md`, `src/` ni `tests/`.

```text
Consensus Freeze              = COMPLETE
ADR-0039                      = ACCEPTED
G3                            = COMPLETE
G4                            = COMPLETE
G5                            = COMPLETE
G6                            = COMPLETE
Implementation                = STARTED
G7                            = NOT AUTHORIZED
Owner Command Naming Decision = PENDING
Owner Validation              = NOT STARTED
```

Sin compilacion ni pruebas nuevas en G6-CLOSE: es un commit documental. Su SHA y su CI se reportan al Coordinador.

**G7A — decision del Owner sobre el nombre del comando.** Primera fase de G7, por orden del Coordinador (`G7 IS
AUTHORIZED`; G8, G9 y G10 NOT AUTHORIZED). Solo documentacion y antes de cualquier codigo de producto. En su preflight,
`origin/main` estaba en `1b091be`. **Correccion (G7-CLOSE):** `main` avanzo a `104ef3a` despues de ese preflight, y
G7A se commiteo igualmente sobre la base anterior como `e25d159`, incumpliendo la disciplina de rebasar antes de
escribir. La correccion esta mas abajo, en «G7A — correccion de la base», y en el registro de decisiones, §18.

- **G6-CLOSE verificado**: `b4e99e4cbd11d3ed50a6f6b4551f592b9dc723f0`, igual a su upstream; CI 34788418383 (push)
  `success` sobre ese SHA exacto, con los cuatro jobs en verde.
- **Decision del Owner**, definitiva para la V1 de I-54 y transmitida por el Coordinador: «Acepto RACKPROPIEDADES con
  alias RPR.» (2026-09-13). Queda `Owner Command Naming Decision = ACCEPTED`, con `Primary Command = RACKPROPIEDADES` y
  `Alias = RPR`.
- **[decisions/I-54.md](../automation/decisions/I-54.md) §17**: la traza de la decision y de la autorizacion de G7, sin
  reescribir §1..§16.
- **Censo de apertura de G7**, medido antes de producir: 33 `[CommandMethod]`, sin `RACKPROPIEDADES` ni `RPR`; 29
  ventanas de producto, 11 de ellas del arquetipo C.
- No cambian la Proposal V5, ADR-0039, el Discovery, `ideas-futuras.md`, `ROADMAP.md`, `HANDOFF.md`, `src/` ni `tests/`.

```text
Consensus Freeze              = COMPLETE
ADR-0039                      = ACCEPTED
Owner Command Naming Decision = ACCEPTED
Primary Command               = RACKPROPIEDADES
Alias                         = RPR
G3                            = COMPLETE
G4                            = COMPLETE
G5                            = COMPLETE
G6                            = COMPLETE
Implementation                = STARTED
G7                            = AUTHORIZED BY COORDINATOR
G8+                           = NOT AUTHORIZED
Owner Validation              = NOT STARTED
```

Sin compilacion ni pruebas nuevas en G7A: es un commit documental. Su SHA y su CI se reportan al Coordinador, y G7B no
empieza hasta que esa CI este en verde.

**G7A — correccion de la base (desviacion de proceso).** Hecha antes de cualquier codigo de producto de G7B:

- **Hechos**:
  1. G7 abrio con `origin/main` = `1b091bedafceb67ca57054a9eb3bf5259efff774`.
  2. Despues del preflight de apertura, `main` avanzo a `104ef3a1b1df249d6e0a56dc4ad3846912b24f12` (merge de I-53S E2).
  3. G7A se commiteo igualmente sobre la base anterior, como `e25d159ef03993c1e886f1a322afda07e7f4bb82` (CI 34789277774
     `success`).
  4. Eso incumplio la disciplina obligatoria de rebasar antes de escribir ([WORKFLOW](../WORKFLOW.md) §4.2 y la orden
     de G7). El G7A original no fue conforme.
- **Correccion**, antes de G7B:
  - tag `archive/i-54-g7a-pre-rebase-e25d159`, que preserva el commit original sin reescribirlo;
  - rebase de toda la rama de I-54 (20 commits) sobre `104ef3a`;
  - comprobacion de las costuras de la lista de STOP de la orden de G7, ninguna afectada;
  - equivalencia verificada;
  - publicacion con `--force-with-lease`;
  - G7A post-rebase = `5c16ae8a8134e3b956459bd91ba15d097a24fc34`, CI 34789745328 `success` 4/4.

  Detalle, mapa de SHAs y equivalencia en el registro de decisiones, §18.
- **Clasificacion del Coordinador**: `PROCESS DEVIATION = CORRECTED / NON-MATERIAL / CLOSED`.

**G7B — comando `RACKPROPIEDADES` y editor de propiedades.** Segunda fase de G7, por orden del Coordinador (`G7 IS
AUTHORIZED`; G8, G9 y G10 NOT AUTHORIZED), sobre G7A `5c16ae8` y `main` @ `104ef3a`, sin rebase nuevo.

- **Producto** (sintesis en la cabecera de este contrato):
  - **Comando**: `RACKPROPIEDADES` y `RPR`, el mismo comportamiento y el prompt de Rack o `[Proyecto]`.
  - **Proyecto**: `ReadProject` → ventana → intent → `ExecuteProject` → relectura, sin preflight temprano de Proyecto
    (no existe esa API); el ejecutor fresco sigue siendo la autoridad.
  - **Rack**: eleccion → resolucion de la definicion → `ReadRack` → instantanea de lo mostrado → ventana → preflight →
    `ExecuteRack` o `ExecuteRackUnify` → relectura.
  - **Ventana** `RackCustomPropertiesWindow`, arquetipo C: CRUD por id estable si es editable; solo lectura en todos los
    estados de D-09 y D-13; `Divergent` con resumenes por vista; unificacion con las opciones de Application, sin
    origen por defecto, `Absent` como «vacío / sin propiedades» y confirmacion; sin descarte, sin `MessageBox` y sin
    integracion en los editores de sistema.
- **Censos**, medidos al abrir (post-rebase, `5c16ae8`) y al cerrar:

  | Censo | Apertura | Final | Delta |
  |---|---|---|---|
  | `[CommandMethod]` | 33 | 35 | +2: `RACKPROPIEDADES` y `RPR` |
  | Ventanas de producto | 29 (A 6, B 6, C 11, D 6) | 30 (A 6, B 6, C 12, D 6) | +1: `RackCustomPropertiesWindow`, +1 del arquetipo C |
  | Pares comando/alias de la ayuda | 14 | 15 | +1: `RACKPROPIEDADES`/`RPR` |

- **Pruebas por ID**, todas en PASS:

  | ID | Casos | | ID | Casos |
  |---|---|---|---|---|
  | U-01 | 4 | | U-06 | 22 |
  | U-02 | 14 | | U-07 | 2 |
  | U-03 | 19 | | U-08 | 2 |
  | U-04 | 7 | | U-09 | 23 |
  | U-05 | 5 | | U-10 | 21 |
  | T-GRD-08 (Core) | 42 | | T-GRD-08 (UI: ayuda 10, ventanas 1) | 11 |

- **RED**:
  - con las pruebas y sin producto: Core, 33 de las 43 pruebas de G7 en rojo (nombres ausentes en el censo, archivo del
    comando ausente y censo 33 frente a 35); UI sin compilar, porque la ventana no existia;
  - con una ventana esqueleto sin comportamiento, nunca versionada: Core 33 de 43 y UI 120 de 137 en rojo;
  - violaciones temporales en archivos reales, nunca versionadas: 10 de 10 en rojo, restauradas con hash verificado.
- **GREEN local** (SDK 8.0.423):
  - UI focal 137/137;
  - guardas Core de G7 y G6 95/95;
  - CustomProperties Core 789/789;
  - Core completa 7240/7240, 0 omitidas, antes del commit y otra vez sobre `18cf2dc` con el arbol limpio;
  - impacto de UI: 297 PASS, 9 omitidas y 0 fallidas;
  - UI completa: 1524 PASS, 17 omitidas y 0 fallidas, las mismas 17 omitidas de la base, antes del commit y otra vez
    sobre `18cf2dc`.
- **Builds Debug**: UI con 0 avisos y 0 errores; Plugin con 0 errores y los 2 avisos MSB3277 de la base.
- **Commit** `18cf2dcc55c1eb0f0c826e1716a43a2ae0a7cf71`; CI 34792787919 (push) `success` sobre ese head_sha exacto, con
  los cuatro jobs en verde.
- **No toco** Domain, Application, el codigo de G3..G6, el sobre, el compositor, Project Variables, los editores de
  sistema, la biblioteca ni la documentacion. Sin Candidato, sin AutoCAD y sin integracion.
- **Revision del Coordinador**: G7 = ACCEPTED / COMPLETE, con cuatro disposiciones sobre el diseño, en el registro de
  decisiones, §18:
  - resolucion de la eleccion en una transaccion de solo lectura: aceptada;
  - Proyecto sin preflight temprano: aceptado;
  - censo de la ayuda: aceptado;
  - `docs/guias/despliegue.md`: diferido y no bloqueante.

**G7-CLOSE — sincronizacion documental tras G7.** Solo documentacion, sin rebase: `main` sigue en `104ef3a`.

- **G7 verificado**: la rama esta en `18cf2dcc55c1eb0f0c826e1716a43a2ae0a7cf71`, igual a su upstream, y la CI
  34792787919 dio `success` sobre ese SHA exacto, con los cuatro jobs en verde. La CI 34789745328 de G7A post-rebase
  tambien es `success`.
- **Contrato**: este archivo registra G7 COMPLETE, Implementation STARTED, G8 NOT AUTHORIZED y Owner Validation NOT
  STARTED, con la sintesis factual de G7. Corrige el texto de estado que G7A dejo afirmando que `main` seguia en
  `1b091be`, que no hubo rebase y que la rama seguia sobre la base anterior. Las entradas historicas se conservan.
- **[decisions/I-54.md](../automation/decisions/I-54.md) §18**: traza de G7 COMPLETION —desviacion de proceso y su
  correccion, SHAs de G7A antes y despues del rebase, SHA y CI exactos de G7, evidencia, censos y las cuatro
  disposiciones del Coordinador—. De §17 solo se corrige su verificacion de `main`.
- No cambian `src/`, `tests/`, `docs/adr/`, la Proposal V5, ADR-0039, el Discovery, `ideas-futuras.md`,
  `docs/guias/despliegue.md`, `ROADMAP.md` ni `HANDOFF.md`.

```text
Consensus Freeze              = COMPLETE
ADR-0039                      = ACCEPTED
Owner Command Naming Decision = ACCEPTED
Primary Command               = RACKPROPIEDADES
Alias                         = RPR
G3                            = COMPLETE
G4                            = COMPLETE
G5                            = COMPLETE
G6                            = COMPLETE
G7                            = COMPLETE
Implementation                = STARTED
G8                            = NOT AUTHORIZED
Owner Validation              = NOT STARTED
```

Sin compilacion ni pruebas nuevas en G7-CLOSE: es un commit documental. Su SHA y su CI se reportan al Coordinador.

**G7-CLOSE — SHA y CI.** `02987bd18ef0904a0332556928503a9afaeddcd1`, CI 34794374450 (push) `success` 4/4 sobre ese SHA
exacto.

**G8 — Candidato.** Por orden del Coordinador (`G8 IS AUTHORIZED`), sin commits ni cambios: gate de evidencia.
`origin/main` seguia en `104ef3a`, asi que no hubo rebase y el Candidato es la punta de G7-CLOSE.

- **Arbol limpio y `HEAD` = Candidato**, comprobados antes y despues de cada suite y de cada build.
- **SDK resuelto**: 8.0.423, el host de usuario `%LOCALAPPDATA%\Microsoft\dotnet\dotnet.exe`. El `dotnet` del `PATH`
  (`C:\Program Files\dotnet`, solo SDK 10.0.401) no resuelve el `global.json`.
- **Core Full local**: `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj`, PASS, 7240/7240, 0 omitidas.
- **UI Full local**: `dotnet test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj`, PASS, 1524 superadas, 17 omitidas
  y 0 fallidas de 1541; las 17 omitidas son las mismas de la base y ninguna es de I-54.
- **Debug UI build**: `dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug --no-incremental`, 0 avisos y 0 errores.
- **Debug Plugin build**: `dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug --no-incremental`, 0 errores;
  los 2 avisos `MSB3277` de las referencias de AutoCAD, identicos a la base.
- **DLL**: `src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll`,
  `1.0.0+02987bd18ef0904a0332556928503a9afaeddcd1`, SHA-256
  `5D289A811F4FDF699E7AF5F4931B6CF540671CFDB1684290957264FE85AA6441`.
- **CI exact-SHA**: corrida 34794374450, `push`, `head_sha` = el Candidato, `success`, con Tests (Domain + Application),
  Build UI, UI Tests y Build Plugin without AutoCAD en verde.

```text
Candidate SHA: 02987bd18ef0904a0332556928503a9afaeddcd1
Arbol limpio: SI
SDK resuelto: 8.0.423
Core Full local: PASS
UI Full local: PASS
Debug UI build: PASS
Debug Plugin build: PASS
CI exact SHA: GREEN (run 34794374450, UI Tests success)
Owner validation: pending
```

**G9 — validacion del Owner: PASS.** OV-01..OV-14 de la Proposal V5 §12.8 en AutoCAD, sobre el DLL de ese Candidato:
**PASS**, resultado global PASS y aprobacion explicita del Owner, «Apruebo validación». No constan registrados en la
transcripcion del Coordinador —y no se afirman— la ruta ni el SHA-256 de la biblioteca de bloques, la version o build
exacta de AutoCAD ni la granularidad observada de `UNDO` (OV-08).

**G10 — cierre documental e integracion.** Por orden del Coordinador (`G10 IS AUTHORIZED`), sin cambios de producto.

- **Preflight**: `origin/main` = `104ef3a1b1df249d6e0a56dc4ad3846912b24f12`, sin avance desde G7A; **sin rebase final**,
  asi que la evidencia de G8 y la validacion de G9 recaen exactamente sobre el contenido que se integra. Rama igual a
  su upstream en el Candidato, arbol limpio, sin stash ni operaciones a medias; la Proposal V5 y ADR-0039 no cambian.
- **Este commit** (`CLOSURE_SHA`) es el ultimo de la rama y **no reemplaza** al Candidato: solo documentacion —este
  contrato, el registro de decisiones (§19), `docs/HANDOFF.md`, `docs/ROADMAP.md`, `README.md` e `ideas-futuras.md`—,
  comprobado con `git diff --name-only 02987bd..HEAD`. Exige su propia CI exact-SHA y no una nueva validacion del Owner.
- **Pendiente al escribirlo**: el merge `--no-ff`, el CI del `MERGE_SHA` con `rackcad-coverage-cobertura`, la
  cobertura diferida del Candidato y la limpieza de rama y worktree ([WORKFLOW](../WORKFLOW.md) §4.5 pasos 5 a 7 y
  §4.6).

```text
Consensus Freeze              = COMPLETE
ADR-0039                      = ACCEPTED
Owner Command Naming Decision = ACCEPTED
Primary Command               = RACKPROPIEDADES
Alias                         = RPR
G3                            = COMPLETE
G4                            = COMPLETE
G5                            = COMPLETE
G6                            = COMPLETE
G7                            = COMPLETE
G8                            = COMPLETE / CANDIDATE
Candidate SHA                 = 02987bd18ef0904a0332556928503a9afaeddcd1
G9                            = COMPLETE
Owner Validation              = PASS   (OV-01..OV-14 PASS; «Apruebo validación»)
G10                           = INTEGRATION
```

El resto de la evidencia —`CLOSURE_SHA`, `MERGE_SHA`, sus CI y la cobertura— se reporta al Coordinador en G10.
