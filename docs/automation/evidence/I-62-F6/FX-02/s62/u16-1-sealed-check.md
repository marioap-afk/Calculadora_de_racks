# U-16 (1) — Comprobación de las mutaciones exactas selladas (decisiones §62, punto 3)

> Preparación de la supervisión (plano a), una sola pasada, 2026-10-09. Nada se decide aquí. Los archivos sellados son **solo de supervisión**: este
> documento no copia ni resume su contenido. Recoge solo hashes, tamaños, número de encabezados y, para cada id, si la parte A define la mutación
> exacta (sí/no).
>
> Mandato (§62.3): «**U-16 (1):** solo tras comprobar las mutaciones exactas selladas».

## 1. Método

- **Lectura:** solo de `D:/r62-fixture/evidence-out/supervision/fx02/`, sin escribir en `D:\r62-fixture`.
- **Hashes y tamaños:** `sha256sum` y `stat`.
- **Estructura:** `tools/sealed_struct.py` y `tools/sealed_flags.py` (en este directorio). Los dos imprimen solo recuentos, banderas booleanas y
  longitudes de celda, nunca texto del archivo.
- **Registro de comparación:**
  - `docs/automation/evidence/I-62-F6/kits/sealed-supervision-files.json` (worktree, `cca8c60e`);
  - cabecera y §8 del `README.md` del kit de FX-02;
  - evidencia §77 y §81.

## 2. Integridad

| Archivo | SHA-256 calculado | Bytes | `sealed-supervision-files.json` | README del kit, §8 (2026-10-08) | README del kit, cabecera (L7-L8) | Resultado |
|---|---|---|---|---|---|---|
| `negatives.md` | `63e84ba3d7f06371e4c44a7c2cf671757ac34d61caf35bf56fed94524507b0e7` | 12 897 | `63e84ba3…`, 12 897 | `63e84ba3…`, 12 897 | `63e84ba3…`, 12 897 | **intacto** |
| `supervision-checks.md` | `729fc3ef34b0704d36fa0313b980554bbd5253c2cd64c96a95c3c4cd9522a0fd` | 22 954 | `729fc3ef…`, 22 954 | `729fc3ef…`, 22 954 | `44494358…`, 22 058 (desfasada) | **intacto** frente al registro vigente |

Archivos archivados en `history/` (registrados como `Superseded`), también comprobados:

| Archivo | SHA-256 calculado | Bytes | Registro |
|---|---|---|---|
| `negatives.c88f1627.md` | `c88f162788db3bcf72d269e7f3925be132c7b74f527b0d19bbd6bbccf9c4071c` | 11 772 | igual |
| `supervision-checks.02a64b85.md` | `02a64b85b7672158a8774e2f4b9443f8ed56b4626b6e597258c2f3dc17944e18` | 15 641 | igual |
| `supervision-checks.44494358.md` | `444943584d3d792a8d152377fd62ee07aa8a54de110aeb61ea3fbbf933a28512` | 22 058 | igual |

**Discordancia documental, no de integridad.**
- La cabecera del `README.md` del kit de FX-02 (L7-L8) sigue citando como vigente `44494358…` (22 058 bytes).
- El JSON de sellado y la entrada de §8 del 2026-10-08 citan `729fc3ef…` (22 954 bytes), igual que la evidencia §81 («resellado (`729fc3ef…`; la
  versión anterior `44494358…` queda archivada)»).
- El archivo vigente es `729fc3ef…`, y `44494358…` está archivado e intacto.
- Corregir la cabecera es tarea aparte de la supervisión. No se hace aquí: no se escribe en el worktree.

## 3. Estructura (sin contenido)

| Archivo | Encabezados | Por nivel |
|---|---|---|
| `negatives.md` | 3 | 1 de nivel 1, 2 de nivel 2 (parte A y parte B) |
| `supervision-checks.md` | 13 | 2 de nivel 1, 8 de nivel 2, 3 de nivel 3 |

**Parte A de `negatives.md`:**
- tiene una sola tabla con la columna «Mutación exacta», de 5 columnas;
- la tabla tiene 12 filas: nc1, nc2, nc3, nc4, N4, N5, N6, N7a, N7b, N8, N9 y N10;
- ninguna celda de mutación está vacía ni es un marcador.

La parte B tiene una fila para cada uno de esos 12 ids.

| Id | ¿La parte A define la mutación exacta? | Longitud de la celda (caracteres) |
|---|---|---|
| N4 | **sí** | 142 |
| N5 | **sí** | 295 |
| N7a | **sí** | 157 |
| N7b | **sí** | 81 |
| N8 | **sí**, pero con premisa caducada (§4) | 302 |

(N6 también tiene fila. §62 la dispuso NOT_APPLICABLE con causa: U-16 (2).)

## 4. N8: premisa obsoleta

- La etiqueta congelada de V14 D.3 es «N8 credencial → P-10» (negativos de sesión, sin invocación).
- La mutación sellada de la parte A instancia esa etiqueta con una candidata `claude-cli` sin credencial. La comprobación booleana sobre la fila
  da: menciona `claude-cli` = sí; menciona credencial = sí.
- Desde **OD-3 = A** (decisiones §56: «el Owner autenticó claude-cli»; punto 4: «la premisa `claude-cli = NOT_AUTHENTICATED` queda obsoleta»), esa
  premisa ya no se cumple, con A-4 o sin ella (A-4, Q-A4-08; clasificación de FX-02, fila #21).

Consecuencia: N8 no puede transcribirse a `{N_CONTROLS}` tal como está sellado. Hace falta una de estas dos cosas, por disposición del Coordinator:
- **una mutación nueva**, que exige resellar `negatives.md`, registrar el SHA nuevo en `sealed-supervision-files.json` y archivar la versión actual.
  La clasificación (W10) la hace esperar a U-16 y a las comprobaciones de A4-3 y A4-4;
- **o NOT_APPLICABLE con causa**, con los precedentes de N11 (§51, §53: «no se fabrica una entrada») y de N6 (§62).

Aquí no se propone el contenido de ninguna mutación.

## 5. Resultado

- **N4, N5, N7a y N7b:** las mutaciones exactas están selladas en la parte A de `negatives.md` (SHA-256 y tamaño iguales al registro vigente). U-16
  (1) puede proceder para esas cuatro cuando el Coordinator lo registre: solo la columna «Mutación exacta» pasa a la orden O4. No se ha evaluado si
  su texto sigue siendo coherente con A-4 (shell declarada del Controller) ni con los commits de U-07 y U-08: eso exigiría leer el contenido.
- **N8:** sellada, pero no aplicable tal como está. Necesita una mutación nueva o NOT_APPLICABLE con causa (§4).
- **`supervision-checks.md`:** intacto (`729fc3ef…`).
- **Fuera del alcance:** los sellados siguen en el mismo host que A2 (riesgo del README del kit §5; U-26 (d) pendiente).
