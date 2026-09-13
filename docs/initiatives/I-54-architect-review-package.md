# I-54 — Paquete de revision exact-SHA: Proposal V4 (G2H, ultra-limitada)

> Paquete **autonomo** para revisar [I-54-proposal-v4.md](I-54-proposal-v4.md) sin la conversacion que la
> produjo. No autoriza implementar ni sustituye a la Proposal: la resume y dice como atacarla. Las versiones
> anteriores de este paquete viven, tras el rebase de G2G, en `ff98b9e` (V3; pre-rebase `5d25da8`), `2d0d666` (V2;
> pre-rebase `36c337b`) y `728d11f` (V1; pre-rebase `7c197af`).

## 0. Que se pide y que no

**Se pide** una revision **exact-SHA** de la Proposal V4, por el Coordinador y por el Arquitecto, cada uno con su
veredicto, sobre el **mismo** SHA (§1). La revision esta limitada **exclusivamente** a:

1. **C-F1** (AR-54-V3-01): no-caracteres en `Name`; ninguna excepcion de NFC; `Value` sin cambios.
2. **C-F2** (AR-54-V3-02): residual de reescritura con surrogate escapado (F-14b); garantias acotadas; fallo sin
   commit; diseño de la caracterizacion.
3. **Integridad del rebase**: el V3 rebasado es byte a byte el revisado antes de aplicar el delta de V4, y lo que
   trae `main` no altera ninguna semantica.

«CLOSED IN V4» es la disposicion del ejecutor; los cierra esta revision.

**No se pide**:
- C-1..C-8, que G2F ya verifico, ni ninguna otra RD que no sea RD-04 o RD-07;
- el orden de autoridad, la arquitectura de `JsonElement?`, la profundidad, el `Kind`, los nombres repetidos, la
  unificacion, la biblioteca ni la UI general;
- el alcance candidato del ADR: `ADR SCOPE = AGREED` en G2F y §15 es identica a V3;
- implementar, estimar, redactar el ADR o editar la Proposal;
- tocar `docs/HANDOFF.md` o `docs/ROADMAP.md`;
- revisar el alcance de I-49, I-52 o I-53.

Una seccion de V4 que ni C-F1 ni C-F2 tocan es identica a V3; el diff de §1 lo demuestra.

## 1. Identificacion exacta

```text
Repositorio                = marioap-afk/Calculadora_de_racks
Rama                       = architecture/propiedades-personalizadas
BASE_SHA                   = 46fcac2b071929d2bd5b07aa28373941417f74a8   (base del Discovery; la evidencia de codigo se cita aqui)
POST_REBASE_BASE_SHA       = f8deb675c6d1ef0e64693b157d69c4cc170d7b24   (origin/main usado en el rebase de G2G)
PRE_REBASE_V3_SHA          = 5d25da89972df2468f1d03243301761f5463e6eb   (SHA que reviso G2F)
REBASED_V3_EQUIVALENT_SHA  = ff98b9ee95ea7c4f8da0b90fe949b8ebb42157ed   (equivalente; no revisado)
REVIEW_V3                  = Architect Review — I-54 Proposal V3 @ 5d25da8 = AGREED WITH CHANGES
                             G2F CLOSED · AR-54-V3-01 y AR-54-V3-02 (MINOR) · el Coordinador acepta la lista cerrada
PROPOSAL_V4                = el commit que introduce docs/initiatives/I-54-proposal-v4.md   ← SHA REVISADO
```

Un documento no puede contener el SHA del commit que lo crea, asi que el SHA revisado se fija **antes** de leer:

```bash
git fetch origin
git log -1 --format=%H origin/architecture/propiedades-personalizadas -- docs/initiatives/I-54-proposal-v4.md
```

Delta exacto de V4 sobre el V3 rebasado, con `<V4_SHA>` = el SHA anterior:

```bash
git diff ff98b9ee95ea7c4f8da0b90fe949b8ebb42157ed:docs/initiatives/I-54-proposal-v3.md <V4_SHA>:docs/initiatives/I-54-proposal-v4.md
```

El veredicto vale **solo** para ese SHA. El de G2F pertenece a `5d25da8` y **no** se transfiere a V4 ni al V3
rebasado.

## 2. Contexto minimo

- **RackCad**: plugin de AutoCAD 2025 (.NET 8), con capas `Domain ← Application ← UI ← Plugin`. **Ninguna suite de
  pruebas carga el Plugin** (ADR-0003).
- **Un rack** = una o varias definiciones de bloque que comparten un `RackId`, cada una con un **sobre** JSON
  (`RackEmbedDocument`) en un Xrecord de su diccionario de extension.
- **I-54** guarda propiedades `{ Id, Name, Value }` en un Xrecord del NOD (Proyecto) y en el miembro
  `CustomProperties: JsonElement?` del sobre (Rack). `Name` se normaliza a NFC; `Value` no.

## 3. Que cambio desde la revision de V3

### 3.1 Las dos correcciones

| Cambio | Hallazgo | V3 | V4 | Donde, sobre todo |
|---|---|---|---|---|
| **C-F1** | AR-54-V3-01; AR-54-V2-03 (3B) | Validar UTF-16 bien formado antes de NFC se daba por suficiente | Un `Name` con un **no-caracter** (U+FDD0–U+FDEF y todo U+nFFFE/U+nFFFF) es intent invalido al escribir y `PresentButUnreadable` al leer, siempre antes de NFC; defensa acotada de `ArgumentException` documentada; `Value` sin cambios | §1, P-35, D-05.1, D-05.2, D-05.4, D-07.4, INV-23, T-STO-08/16/18/20 |
| **C-F2** | AR-54-V3-02; AR-54-V2-03 (3C) | Solo se declaraba el residual del UTF-16 crudo; INV-07 y D-13 prometian «ningun efecto» | Residual preexistente **F-14b** junto a **F-14a**; INV-07, D-13 y RP-11 acotados a sobres que BASE puede leer y reserializar; sin commit si una serializacion lanza (D-22.10); T-CHR-04 y T-ENV-13 | P-36, D-08.4, D-13, D-22.10, INV-07, RP-11, T-CHR-04, T-ENV-13, T-MUT-08 |

### 3.2 Rebase de G2G

`git rebase origin/main`: 6/6 commits, sin conflictos.

| Artefacto | Pre-rebase reviewed/published SHA | Post-rebase equivalent SHA | `git range-diff` |
|---|---|---|---|
| CLAIM | `143490d8ecfb1c5f3011cf752c5cfcd11af13784` | `c60c17fa46ef1eb2e9ab696432721d7649fe7c1d` | `=` |
| BOOTSTRAP | `f908b2f4ba508bf365e55adbda78cd4eac505295` | `1c2d17b3de233738d646d82811ddecd8ed673611` | `!` solo por contexto de `ROADMAP.md` |
| G1 | `195964b00006d2be74eefb8b6ae4f5f46de1cfc9` | `97e27ea7c50289460a2c69be0e70339c3aa674d0` | `=` |
| V1 | `7c197af81b91df88366c873eddf9e11ddc5e87bb` | `728d11f09e727adda3cf993fd39601fc18614ba6` | `=` |
| V2 | `36c337b84c47f9ac7c97d860fce42d3a5f4370e8` | `2d0d6665ff286a97f25024e5fa800532cf96f624` | `=` |
| V3 | `5d25da89972df2468f1d03243301761f5463e6eb` | `ff98b9ee95ea7c4f8da0b90fe949b8ebb42157ed` | `=` |

## 4. Lectura obligatoria, en este orden

1. **Proposal V4**:
   - §0 y §22: que cambio y el estado;
   - C-F1: §1 (filas «UTF-16 bien formado», «No-caracter Unicode» y «Name acreditado»), P-32, P-35, D-05.1,
     D-05.2 con su «Frontera de excepciones de NFC», D-05.4, D-07.4 (paso 3, tabla de excepciones y parrafo
     siguiente), D-22.5, INV-23, T-STO-08, T-STO-16, T-STO-18 y T-STO-20;
   - C-F2: P-31, P-36, D-08.4 (restriccion A, residuales F-14a y F-14b y «Alcance de la afirmacion»), D-13,
     D-22.10, INV-07, RP-11, T-CHR-04, T-ENV-13, T-MUT-08 y la fila G4 de §14;
   - rebase: §2 y §13;
   - trazas: §12.11, §14, §17.1, §18.1, §18.2 (AR-54-V2-03), §19 (RD-04 y RD-07), §20 y §21.
2. **Codigo** en `BASE_SHA` (`git show 46fcac2:<ruta>`), identico en `POST_REBASE_BASE_SHA`:

   | Archivo | Lineas | Para |
   |---|---|---|
   | `src/RackCad.Application/Persistence/RackEmbedDocument.cs` | 76-84 | `Serialize` sin captura (F-14b) |
   | `src/RackCad.Application/Persistence/RackEmbedDocument.cs` | 86-101 | `Deserialize` solo captura `JsonException` (F-14a) |
   | `src/RackCad.Application/Persistence/RackEmbedComposer.cs` | 21-33 | `Compose` hereda `ExtensionData` del origen |
   | `src/RackCad.Plugin/RackEnvelopeRestamp.cs` | 52-88 | el restamp deserializa (`:60`) y serializa el mismo objeto (`:87`) |

3. **Contrato**: `docs/initiatives/I-54-propiedades-personalizadas.md` §4 y §7, para el momento de registrar F-14.

## 5. Hechos ejecutados [X] y como reproducirlos

Sondas de .NET 8.0.29 **fuera del repositorio**, las mismas de G2F. Para evitar escapes accidentales en el fuente,
los caracteres especiales se construyen con su codigo (`(char)0xFFFE`, un surrogate suelto `(char)0xD800`) y el
escape JSON se arma concatenando la barra invertida, obtenida como `(char)0x5C`, con `u` y los cuatro digitos.

| # | Caso | Resultado observado | Sostiene |
|---|---|---|---|
| Z-1 | `("A" + c).Normalize(NormalizationForm.FormC)` e `IsNormalized(FormC)` con `c` = U+FFFE, U+FFFF, U+FDD0, U+0378, U+E000, U+1FFFE (par), U+10FFFF (par), U+0000 y «Área», en modo ICU (por defecto) | Solo U+FFFE lanza `ArgumentException` (y el surrogate suelto de control); los demas no | P-35, C-F1 |
| Z-2 | Los mismos casos con `DOTNET_SYSTEM_GLOBALIZATION_USENLS=1` | Lanzan U+FFFE, U+FFFF, U+FDD0, U+1FFFE y U+10FFFF; no lanzan U+0378, U+E000, U+0000 ni «Área» | P-35, C-F1 |
| Z-3 | `JsonDocument.Parse` de un objeto cuyo `Name` es el escape JSON de U+FFFE; `GetString()`; serializar y releer un `Value` con U+FFFE | `GetString` no lanza y devuelve texto bien formado; `Normalize` sobre ese texto lanza; el `Value` vuelve exacto | P-35, C-F1 |
| Z-4 | Sobre con el escape JSON de un surrogate suelto (a) en un `Value` dentro de `CustomProperties`, (b) en un nombre de propiedad dentro de `CustomProperties`, (c) en un campo desconocido del sobre; con copias de BASE y de la variante: `Deserialize`; `Serialize` del mismo objeto; `Compose(source)` + `Serialize`; mutar `Id` y `Name` del mismo objeto + `Serialize` | En los tres casos y en los dos builds: `Deserialize` legible; las tres reescrituras lanzan `JsonException` («The object or value could not be serialized», ruta `$.CustomProperties` o `$.ExtensionData`) | P-36, C-F2 |
| Z-5 | Control de Z-4 con un par de surrogates valido escapado | Las tres reescrituras funcionan | P-36, C-F2 |
| Z-6 | `JsonElement.WriteTo(Utf8JsonWriter)` sobre un valor con el escape de un surrogate suelto; `GetRawText()` | `WriteTo` lanza `InvalidOperationException`; `GetRawText` no lanza | P-36, C-F2 |

## 6. Hallazgos a re-verificar

| Hallazgo | Sev. | Donde lo cierra V4 | Que re-verificar |
|---|---|---|---|
| AR-54-V3-01 | MINOR | §1, P-35, D-05.1, D-05.2, D-05.4, D-07.4, D-22.5, INV-23, T-STO-08/16/18/20 | Que ningun `Name` no acreditado llegue a `Normalize` o `IsNormalized` en el store, el preflight, el workspace ni la revalidacion; que la regla sea determinista y previa a NFC; que la defensa acotada no oculte errores; que `Value` no cambie |
| AR-54-V3-02 | MINOR | P-36, D-08.4, D-13, D-22.10, INV-07, RP-11, T-CHR-04, T-ENV-13, T-MUT-08 | Que F-14b este descrito con exactitud y fuera de I-54; que ninguna garantia prometa «ningun efecto» para F-14a o F-14b; que un fallo de serializacion no deje escrituras; que la caracterizacion no finja una prueba del restamp |
| AR-54-V2-03 | MINOR | Por AR-54-V3-01 (3B) y AR-54-V3-02 (3C) | Que las dos causas de su reapertura en G2F queden cerradas |

## 7. Integridad del rebase

| # | Comprobacion | Como verificarla | Resultado del ejecutor |
|---|---|---|---|
| R-1 | El V3 rebasado es byte a byte el revisado | `git rev-parse 5d25da8:docs/initiatives/I-54-proposal-v3.md` frente a `git rev-parse ff98b9e:docs/initiatives/I-54-proposal-v3.md` | mismo blob `eb0de53…` |
| R-2 | V4 nace de ese V3 | el diff de §1 solo contiene C-F1, C-F2 y trazas | secciones fuera de alcance identicas, §15 incluida |
| R-3 | Los seis commits de I-54 son equivalentes | `git range-diff 46fcac2..5d25da8 f8deb67..ff98b9e` | cinco `=`; BOOTSTRAP `!` solo por contexto de `ROADMAP.md` |
| R-4 | `main` no altera semantica de I-54 | `git diff --name-only f8deb67 ff98b9e`; `git diff --stat 46fcac2 f8deb67 -- <archivos del mapa de §11>` | solo documentos de I-54 y su fila de ROADMAP; ningun archivo de codigo del mapa cambia |
| R-5 | Nada productivo ni HANDOFF | `git diff --name-only f8deb67 <V4_SHA> -- src tests assets eng deploy .github docs/HANDOFF.md` | vacio |

Los SHAs pre-rebase siguen en el almacen local de objetos (reflog de la rama). Tras publicar con
`--force-with-lease`, pueden dejar de ser alcanzables desde `origin`.

## 8. Preguntas de ataque para V4

Sugerencias, no conclusiones.

| # | Cambio | Pregunta | Pista |
|---|---|---|---|
| A5-01 | C-F1 | ¿La definicion de no-caracter es exacta (66 code points) y se comprueba sobre valores escalares, no sobre `char`? | §1; D-05.1 pasos 1 y 2 |
| A5-02 | C-F1 | ¿Queda algun camino en que `Normalize` o `IsNormalized` reciba un `Name` no acreditado: store, unicidad del preflight, `NombreRepetido` o revalidacion? | D-05.2; D-07.4 paso 3; D-22.5 |
| A5-03 | C-F1 | ¿La defensa acotada de `ArgumentException` es coherente entre D-05.2 y D-07.4 y no oculta un error de programacion? | tabla de excepciones de D-07.4 |
| A5-04 | C-F1 | ¿`Value` queda como estaba: valido con no-caracteres, sin normalizar y con ida y vuelta exacta? | D-05.4; T-STO-18; Z-3 |
| A5-05 | C-F1 | ¿La regla en el paso 3 conserva `IncompatibleMajor` para un major 2 y no altera la deteccion de duplicados? | D-07.4; §17.1 |
| A5-06 | C-F2 | ¿F-14b esta descrito con exactitud (lectura legible, reescritura que lanza en BASE y variante) y separado de I-54? | D-08.4; P-36; Z-4..Z-6 |
| A5-07 | C-F2 | ¿INV-07, D-13 y RP-11 dejan de prometer «ningun efecto» para F-14a y F-14b, sin debilitar la garantia en sobres reserializables? | INV-07; D-13 |
| A5-08 | C-F2 | ¿D-22.10 garantiza que no hay escrituras parciales: plan completo antes de la primera escritura y transaccion sin commit? | D-22.3; D-22.10; T-MUT-08; T-GRD-04 |
| A5-09 | C-F2 | ¿T-CHR-04 y T-ENV-13 caracterizan el residual sin fingir una prueba directa del restamp? | T-CPY-01; T-GRD-03 |
| A5-10 | Rebase | ¿El mapa de SHAs y la equivalencia byte a byte son correctos? | §7 R-1..R-3 |
| A5-11 | Rebase | ¿Lo que trae `main` cambia alguna evidencia citada por C-F1 o C-F2? | §7 R-4 |

## 9. Ramas paralelas (medidas al publicar V4)

| Rama | SHA | Impacto en C-F1 y C-F2 |
|---|---|---|
| `main` | `f8deb675c6d1ef0e64693b157d69c4cc170d7b24` | Ninguno: base del rebase; `RackEmbedDocument.cs`, `RackEmbedComposer.cs` y `RackEnvelopeRestamp.cs` identicos a `BASE_SHA` |
| I-49 `architecture/motor-expresiones-parametricas` | `1ed93a0525ec11c5092df89c55cbf498c99f8e2a` | Ninguno: rebasada sobre `main` con su Proposal V6 identica; ninguna regla usada por I-54 cambia |
| I-52 `feature/rackmirror-espejo-semantico` | `545c2229de8d7850e03981d85aced0ac63c24040` | Ninguno en su diseño: Proposal V3, solo docs. Su espejo reserializa el sobre, asi que F-14a y F-14b le afectan como a los demas flujos que reescriben el sobre |
| I-53 `feature/cabeceras-configurables-multidestino` | `4e00a273e22634cb3abbc1b3fb3778edf49dbc7e` | Ninguno: G3 añade tipos nuevos en `Application/Systems/Shared` y pruebas; no toca sobre, `Compose`, Custom Properties, NFC ni UI |

Si al revisar alguna rama se movio, citar su SHA nuevo y medir solo si invalida C-F1 o C-F2.

## 10. Formato del veredicto

```text
Architect Review — I-54 Proposal V4
Reviewed SHA = <SHA de 40 hex>
GLOBAL VERDICT = AGREED | AGREED WITH CHANGES | NOT AGREED

C-F1             = VERIFIED | DEFECT
C-F2             = VERIFIED | DEFECT
REBASE INTEGRITY = VERIFIED | DEFECT

AR-54-V3-01 = CLOSED | REOPENED
AR-54-V3-02 = CLOSED | REOPENED
AR-54-V2-03 = CLOSED | REOPENED

New findings (solo dentro de §0) = NONE | [BLOCKER | MATERIAL | MINOR] AR-54-V4-XX — <cambio> — <problema> — <evidencia> — <cambio requerido>

RD-04 = AGREE | AGREE WITH CHANGE | DISAGREE
RD-07 = AGREE | AGREE WITH CHANGE | DISAGREE

Required Proposal changes = NONE | <lista cerrada y vinculante>

G2H Architect Review = CLOSED | BLOCKED
Architect = AGREED | AGREED WITH CHANGES | NOT AGREED
Consensus = NOT REACHED
Implementation = BLOCKED
```

El Coordinador emite su veredicto con el mismo encabezado («Coordinator Review — I-54 Proposal V4»).

| Severidad | Significado |
|---|---|
| **BLOCKER** | Inimplementable tal como esta escrito, o contradice un ADR aceptado o una doctrina vigente |
| **MATERIAL** | Cambia una decision, un invariante o una superficie |
| **MINOR** | Precision de redaccion o de evidencia que no cambia decisiones |

## 11. Reglas de la revision

- La evidencia de codigo se cita sobre `BASE_SHA`; esos archivos son identicos en `POST_REBASE_BASE_SHA`. Si
  `origin/main` vuelve a avanzar al revisar, se citan ambos SHAs.
- Fuera del alcance de §0 solo se levanta un BLOCKER; cualquier otra observacion se anota aparte y no es
  vinculante para G2H.
- Una guarda de texto no es criterio de aceptacion: el criterio es el comportamiento (leccion de I-48).
- **La revision no desbloquea la implementacion.** Si converge, se evalua G2-FREEZE. El consenso exige
  `Coordinator = AGREED` y `Architect = AGREED` sobre el mismo SHA, el ADR en estado `propuesto` y la aprobacion
  del Owner.
