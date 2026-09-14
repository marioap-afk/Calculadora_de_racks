# I-49 — Proposal V6 · Amendment A3: fallos con varias causas y conjunto de causas raíz

Amendment documental de la [Proposal V6](I-49-proposal-v6.md), leída con el
[Amendment A1](I-49-proposal-v6-amendment-a1-text-guard.md) y el
[Amendment A2](I-49-proposal-v6-amendment-a2-exact-key-qualifier.md), para **un solo** hueco contractual: qué
diagnósticos recibe un símbolo cuando falla por **más de una causa a la vez**, qué es su conjunto de causas raíz y qué
datos estables consumen de él el `PlanReadSet`, la razón `Upstream` y el mensaje de recuperación de §7.2. Lo encontró el
STOP de G7 antes de RED y lo confirmó la revisión del Arquitecto (`MULTI_CAUSE_AMBIGUITY = CONFIRMED`). Esta es la
revisión **A3-R1**, que resuelve los hallazgos MAT-1 a MAT-5 de la revisión exacta del Arquitecto sobre el texto
anterior.

```text
Amendment          = A3
Revisión           = A3-R1, tras la revisión exacta del Arquitecto (MAT-1 a MAT-5)
Texto anterior     = blob 97996b7a6117e036f1e3defcb70db0e1fd7b035b (f6f0991); CHANGES REQUIRED, no es autoridad
Finding            = MULTI-CAUSE DEPENDENCYFAILED / ROOTCAUSES CONTRACT GAP

Enmienda a         = Proposal V6, blob ef4db3aa400483ff25a8f39b2beb93708fa43d1a (no se edita)
Junto a            = Amendment A1, blob d62019088b9e7a140d5066799afe6ace6db303ba (no se edita ni se enmienda)
                     Amendment A2, blob 49a925336dd3775929a35b0f40b73cb7f8c487f7 (no se edita ni se enmienda)
Cambian            = ADR-0041 D20 (dato estable de Upstream), D18 puntos 1-4 (sugerencia y bloqueo de
                     recuperación) y D14 decisión 7 (ruptura de un ciclo)
Se leen            = D4, D8, D11, D14 decisiones 1 y 6, D15, D18 (precedencia, tabla y puntos 5-7), D19,
                     D20 (pruebas), D22, D23, D24 y Consecuencias
                     ADR aceptado e inmutable, blob c6a3d2ba0ab9df93e51efcddf04ed9255b1a2231
Freeze afectado    = docs/initiatives/I-49-consensus-freeze-v6-a1-a2.md
                     blob 57736f725663aab79a24b29685ed34e8c3e9ebad (no se reescribe)
G6 final           = 75f18623e7ced836de7eb2e36d6db8e311efb76d
                     CI 34862975299 success 4/4; G6 CLOSED
G7                 = BLOCKED BEFORE RED, sin código ni pruebas
Alcance            = SOLO los diagnósticos de un símbolo con más de una causa de fallo, las comprobaciones estáticas
                     que impiden evaluarlo, su orden, sus causas raíz y los datos estables que de ellas consumen
                     PlanReadSet, Upstream y §7.2
Sin cambio         = identidad y cualificador (A2), guarda del parser (A1), extracción de dependencias,
                     dirección del grafo, detección de SCC y catálogo de códigos
```

---

## 1. Qué es y qué no es

- **Es** un amendment del Coordinador que recoge el modelo M1–M10 de la revisión del Arquitecto de G7, corregido con
  los hallazgos de su revisión exacta, y queda sometido a una nueva revisión exacta. Llena **un** hueco y nada más.
- **A3-R1 edita este mismo documento.** El texto anterior (blob `97996b7`) recibió `CHANGES REQUIRED` y **no** es
  autoridad acordada; no hay un A4.
- **No** edita V6, A1, A2, ADR-0041, ninguno de los tres Consensus Freeze ni el Discovery: todos siguen en sus blobs
  exactos.
- **No** autoriza implementación: G7 sigue BLOCKED antes de RED y G8 sigue BLOCKED (A3 §13).
- **No** cambia la identidad ni el cualificador (A2), la guarda del parser (A1), la extracción de dependencias (P11), la
  dirección del grafo (P12.2), las aristas rotas (P12.4), la detección de ciclos por SCC (P13.1) ni el catálogo cerrado de
  códigos: no hay códigos nuevos y ninguno cambia de número o de clase.
- **Activa** la regla de invalidación del freeze vigente (§6 de ese registro): cambia la semántica de Expression, el
  `PlanReadSet` y la `RepairDecisionObservation`, y cambia decisiones de ADR-0041 (A3 §12).
- Como en V6, A1 y A2, los nombres de tipos, miembros y operaciones (`RootCauses`, `CycleRoot`, `SourceRoots`) son
  **ilustrativos**: el contrato es el comportamiento.

---

## 2. Nomenclatura

- **Referencias.** `§n` sin prefijo, `Pn.m`, `T-V…` y `ALT-n` remiten a V6; `Dn`, a ADR-0041; `A1 §n` y `A2 §n`, a esos
  amendments; y **`A3 §n`** a este documento.
- **R1, R2 y R3.** En A3 designan **solo** las reglas de intents correctivos de P21.3 (OPEN B). Los riesgos R1–R16 de
  §11.2 son otra serie, y los contraejemplos R1 y R2 de A2 (A2 §2.3 y A2 §2.4) no tienen relación con este documento.
  A3 no define nada con esos nombres.
- **U1–U9**: los casos normativos (A3 §4). U1–U4 vienen del hallazgo; U5–U9, de la revisión exacta.
- **M1–M10**: las reglas del modelo decidido (A3 §5).
- **MAT-1 a MAT-5** y **N-3 a N-5**: hallazgos materiales y notas de la revisión exacta del Arquitecto sobre el blob
  `97996b7` (A3 §3.5).
- **MC-A, MC-B y MC-C**: los modelos candidatos que la revisión del Arquitecto llamó R-A, R-B y R-C (A3 §9). Se
  renombran para que no se confundan con R1, R2 y R3.
- **CX-1, CX-2 y CX-3**: contraejemplos y carreras de A3 (A3 §3.4, A3 §5.10).
- **S1**: la decisión sobre el orden de los `BrokenReference` frente al evaluador de G6 (A3 §7.1).
- **DI-1 a DI-4**: las lecturas confirmadas del orden topológico, del orden de la lista de ciclos, de los conjuntos
  transitivos y del alcance de los conjuntos derivados de G7 (A3 §6.3–§6.6).
- **T-A3-01…**: pruebas futuras obligatorias (A3 §11).
- **Falla**: da `Failed` en la misma `RegistryEvaluation`.
- **SCC(s)**: la componente fuertemente conexa de s (P13.1). Es **cíclica** si tiene más de un miembro o una arista a
  sí misma. Una SCC acíclica es un solo símbolo.
- **Arista interna** de una SCC C: una arista dueña → dependencia con los dos extremos en C, autorreferencias incluidas.
  Cada dependencia cuenta una vez, como en la extracción sin repetición (P11.1).
- **Ciclo simple**: una SCC cíclica en la que cada miembro tiene exactamente una arista interna saliente y exactamente
  una entrante (A3 §6.2).
- **Notación.** `DF{causa; cadena; raíz}` es un `DependencyFailed` con su causa directa, los ids de su cadena y la firma
  de su raíz. Las firmas de raíz son `(dueño, código, datos)` y `CycleRoot[miembros]` (A3 §5.5). En las tablas de
  pruebas, `{A1, A2}` abrevia el conjunto de las firmas de raíz de A1 y A2 cuando no hay ambigüedad.
- **Supuesto de los ejemplos.** Los ids de los símbolos siguen en P11.1 el orden alfabético de sus nombres
  (A < A1 < A2 < B < C < D < E < F < G < H < S < W < X < Y < Z, y `Holgura` después de `H`). `a`, `a1`, `a2`, `b`, `gone`,
  `h`, `m`, `z` e `id-roto` son ids ausentes, y `K` es un rack. Los tokens de función son las mayúsculas canónicas de
  P10.5 (`ABS`, `MAX`, `MIN`).

---

## 3. Hallazgo

### 3.1 Lo que V6 fija: el caso lineal

Las líneas son las de los blobs exactos de V6 (`ef4db3a`) y de ADR-0041 (`c6a3d2b`).

| Regla | Texto |
|---|---|
| P14.3 (l. 1204-1205; D15, l. 976-977) | «si una dependencia falla, sus dependientes reciben `DependencyFailed` con el id de la causa, sin evaluarse» |
| P15.1 (l. 1233-1234) | «Un fallo por causa superior lleva además la cadena hasta la causa raíz» |
| P21.6, fila `DependencyFailed` (l. 1676; D19, l. 1202) | «La cadena de causas hasta la raíz (P15.1): el id de cada eslabón, y el código y los datos de la causa raíz» |
| P21.6, fila `Upstream` (l. 1694; D20, l. 1289) | «Por cada variable leída que falla, en el orden de P11.1: su `SymbolId` y su causa raíz, es decir, el símbolo donde empieza el fallo, su código y sus datos estables» |
| P21.6, coincidencia de `Upstream` (l. 1702-1703; D20, l. 1298-1299) | «la misma variable sigue fallando por la misma causa raíz → coincide» |
| §0.3, razón `Upstream` (l. 123-124) | «si la variable leída sigue fallando por la misma causa raíz estructurada, coincide» |
| P13.2 y P13.5 (l. 1178-1179, 1186-1187) | Cada miembro recibe `Cycle` con la lista completa; sus dependientes dan `DependencyFailed` |
| P15.7 (l. 1250) | «Orden determinista: posición, código y dueño» |

Todos los ejemplos de V6 con una cadena de fallos son lineales, por ejemplo: P8.10 caso C (l. 987-995), T-V4-01
(l. 2355), T-V5-01 y T-V5-02 (l. 2372-2373), T-V6-01 a T-V6-03 (l. 2388-2390) y el escenario de `RepairBrokenRack` de
P21.6 (l. 1844-1853).

### 3.2 Lo que V6 no fija

1. **Varias dependencias directas que fallan a la vez**: uno o varios `DependencyFailed`, y, si uno, con qué causa.
2. **Varias raíces detrás de una misma dependencia**: qué cadena es «la cadena» y cuál «la causa raíz».
3. **Una `BrokenReference` propia junto a una dependencia que falla**: si coexisten o cuál gana. La única precedencia de
   V6 (P24.6, l. 2168-2169; D18, l. 1033-1034) es la de `InspectBinding` sobre una **fuente de rack**, no la de un
   símbolo.
4. **Un ciclo junto a otras causas**: si un miembro recibe además `DependencyFailed` por dependencias fuera del ciclo, y
   cuántos diagnósticos recibe un dependiente que lee a dos miembros del mismo ciclo.
5. **El desempate entre diagnósticos del mismo código** de un mismo dueño sin posición: P15.7 no lo resuelve.
6. **Una variable con varias raíces en las firmas por variable**: `Upstream` define **una** causa raíz por variable
   leída que falla, mientras §7.2 clasifica cada fuente por su **conjunto** de causas raíz (l. 2759-2766; D18,
   l. 1059-1066).

**Confirmación independiente.** Un barrido de solo lectura, sin conocer el STOP, sobre V6, A1, A2, ADR-0041 y el freeze
vigente dio `NOT DEFINED BY BINDING AUTHORITY` para los puntos 1-6. En la historia no vinculante —V1–V5, el Discovery,
`decisions/I-49.md`, los dos freezes anteriores, ADR-0038 y ADR-0040— ninguna versión tuvo una regla con varias causas:
V1 ya traía el P14.3 singular, V3 añadió la frase de la cadena de P15.1, V4 el conjunto de causas raíz de §7.2 y V5 la
tabla de datos estables. V6 no dejó caer nada.

### 3.3 Por qué es material

- **G7.** Los diagnósticos de `RegistryEvaluation` son observables: cuántos, qué códigos, qué ids y qué cadena.
- **G9, `SymbolResultObservation`.** `Failed` compara cada diagnóstico y sus datos estables (P21.6), y
  `Failed(causa A) → Failed(causa B)` aborta (l. 1796; T-V5-02).
- **G9, `RepairDecisionObservation`.** `Upstream` protege la razón de la fuente que se retira (regla de derivación de
  D19, l. 1158-1163).
- **G9 y G10, §7.2.** La sugerencia de recuperación depende de que la causa raíz sea única (l. 2762-2766) y no puede
  afirmar una recuperación que no se pueda demostrar (§7.2.6, l. 2792-2797).
- **Coste.** Enumerar cada camino de un nodo hasta sus raíces crece como 2^k con k diamantes apilados.

### 3.4 Contraejemplos

**CX-1 — causa oculta en una observación de símbolo.**

```text
A = #<a> + 1     → Failed
B = 1 / 0        → Failed
D = A + B        → Failed
```

Con un solo `DependencyFailed` por símbolo, el de la primera dependencia (A), B se recupera entre preflight y commit, D
sigue mostrando el mismo fallo y el commit escribe, aunque el fallo de D ya no tiene las mismas causas.

**CX-2 — premisa de la fuente retirada con una sola raíz representativa.**

```text
A1 = #<a1> + 1   → Failed
A2 = #<a2> + 1   → Failed
H  = A1 + A2     → Failed
K: palletTolerance = H + 2   → RepairableSemanticFailure(Upstream, H), única fuente fallida del rack K
```

El preflight ve las raíces {A1, A2}: no hay una causa raíz única, así que no muestra la sugerencia de recuperación, y el
usuario confirma la reparación. Antes del commit reaparece `a2`. Si `Upstream(H)` guardara una sola raíz representativa
(A1), la decisión coincidiría y el commit borraría la fórmula, aunque **por qué falla H**, la razón de esa misma fuente
retirada, ya cambió: sus causas raíz pasan de {A1, A2} a {A1}. Lo prohíbe la regla de derivación (D19, l. 1158-1163):
la razón de la fuente retirada incluye por qué falla la variable que lee. CX-2 se apoya **solo** en ese cambio de
`RootCauses`; no afirma que `Upstream` proteja el estado de otras fuentes ni la elegibilidad de la sugerencia de §7.2
(A3 §5.10(b), N-3).

### 3.5 Hallazgos de la revisión exacta (A3-R1)

La revisión exacta del Arquitecto sobre el blob `97996b7` dio `CHANGES REQUIRED` con cinco hallazgos materiales, cada
uno con un contraejemplo que A3-R1 convierte en caso normativo:

| Hallazgo | Defecto del texto anterior | Caso | Corrección |
|---|---|---|---|
| MAT-1 | `RootCauses` de un miembro de ciclo solo seguía sus propias aristas: una raíz externa que entraba por **otro** miembro quedaba fuera, y §7.2 podía prometer una recuperación falsa | U5, U4 | `RootCauses` por SCC (M6) |
| MAT-2 | La condición de §7.2 se reescribía sin exigir que cada fuente fuera `Upstream` | U8 | Clasificación `Upstream` obligatoria (M10(c)) |
| MAT-3 | La aridad errónea solo se veía evaluando, pero V6 la comprueba sobre el árbol en cada snapshot (P2.2, P7.5) | U6 | `InvalidArguments` estático antes de evaluar (M1, M2) |
| MAT-4 | Un `CycleRoot` como única raíz permitía prometer la recuperación en cualquier SCC; con varios ciclos internos, corregir un miembro no basta (defecto heredado de §7.2.3) | U7 | Solo en un ciclo simple (A3 §6.2, M10(c)) |
| MAT-5 | Se afirmaba igualdad con V6 para toda falla lineal, también con raíz de ciclo, donde V6 no definía el símbolo raíz | U9 | Alcance reducido a los escenarios lineales sin ciclo (A3 §8) |

Notas incorporadas: **N-3** (`Upstream` no observa las fuentes de otros racks, A3 §5.10(b)), **N-4** (coste de
`RootCauses` y memoización por snapshot, A3 §5.9) y **N-5** (fallos numéricos latentes, A3 §5.1).

---

## 4. Casos normativos U1–U9: decisión A3

### 4.1 U1 — dos raíces independientes

```text
A = #<a> + 1     → [BrokenReference(a)]
B = 1 / 0        → [DivisionByZero]
D = A + B        → no se evalúa
                   [DF{A; [A]; (A, BrokenReference, [a])},
                    DF{B; [B]; (B, DivisionByZero)}]

RootCauses(D) = { (A, BrokenReference, [a]), (B, DivisionByZero) }
```

D recibe un `DependencyFailed` por cada dependencia directa que falla, en el orden de M3. Ninguna causa queda oculta.

### 4.2 U2 — dos raíces detrás de una sola dependencia

```text
A1 = #<a1> + 1   → [BrokenReference(a1)]
A2 = #<a2> + 1   → [BrokenReference(a2)]
B  = A1 + A2     → no se evalúa
                   [DF{A1; [A1]; (A1, BrokenReference, [a1])},
                    DF{A2; [A2]; (A2, BrokenReference, [a2])}]
D  = B * 2       → no se evalúa
                   [DF{B; [B, A1]; (A1, BrokenReference, [a1])}]

RootCauses(D) = { (A1, BrokenReference, [a1]), (A2, BrokenReference, [a2]) }
```

- D tiene **un** `DependencyFailed`, porque tiene una sola dependencia directa que falla.
- Su cadena representativa sigue el **primer** diagnóstico de B en el orden de M3: `[B, A1]`.
- La segunda raíz **no** se descarta porque D tenga una sola dependencia fallida: está en `RootCauses(D)`.
- Los dos caminos **no** se enumeran como diagnósticos distintos de D.

### 4.3 U3 — `BrokenReference` propia y dependencia que falla

```text
A = 1 / 0        → [DivisionByZero]
D = A + #<m>     → no se evalúa
                   [BrokenReference(m),
                    DF{A; [A]; (A, DivisionByZero)}]

RootCauses(D) = { (D, BrokenReference, [m]), (A, DivisionByZero) }
```

- Los dos diagnósticos coexisten; ninguna precedencia retira uno.
- `BrokenReference(m)` es **propio** de la definición de D: D es raíz de su propio fallo.
- `DF{A}` es **superior**: viene de una dependencia.
- No se intenta ninguna evaluación numérica de D una vez conocidas esas condiciones: un fallo numérico propio de D no se
  busca ni se informa (A3 §5.1, N-5).

### 4.4 U4 — ciclo y fallo externo

```text
A = B + E        → miembro de la SCC {A, B}
B = A            → miembro de la SCC {A, B}
E = 1 / 0        → [DivisionByZero]
F = A + B        → no miembro

A = [Cycle[A, B], DF{E; [E]; (E, DivisionByZero)}]
B = [Cycle[A, B]]
F = [DF{A; [A]; CycleRoot[A, B]}, DF{B; [B]; CycleRoot[A, B]}]

RootCauses(A) = RootCauses(B) = RootCauses(F) = { CycleRoot[A, B], (E, DivisionByZero) }
```

- La SCC cíclica es **una** unidad de causa raíz.
- Los diagnósticos son **locales**: A lleva `DF{E}` porque su definición lee E; B no, porque no lee E.
- Un miembro recibe `DependencyFailed` por cada dependencia directa que falla **fuera** de su SCC, y **ninguno** por las
  aristas dentro de su SCC.
- F recibe un `DependencyFailed` por cada dependencia directa que falla, A y B.
- `RootCauses` se calcula **por SCC** (M6): B contiene la raíz de E aunque E entre en la SCC por A, y F la cuenta una
  sola vez junto con el `CycleRoot`.

### 4.5 U5 — varias raíces externas que entran por miembros distintos (MAT-1)

```text
A = B + E ; B = C ; C = D + F ; D = A ; E = 1 / 0 ; F = 1 / 0
SCC = {A, B, C, D}; aristas internas A→B, B→C, C→D, D→A: ciclo simple

E = [DivisionByZero]
F = [DivisionByZero]
A = [Cycle[A, B, C, D], DF{E; [E]; (E, DivisionByZero)}]
B = [Cycle[A, B, C, D]]
C = [Cycle[A, B, C, D], DF{F; [F]; (F, DivisionByZero)}]
D = [Cycle[A, B, C, D]]

RootCauses(A) = RootCauses(B) = RootCauses(C) = RootCauses(D)
              = { CycleRoot[A, B, C, D], (E, DivisionByZero), (F, DivisionByZero) }

K: palletTolerance = B + D   → Upstream; SourceRoots de tres elementos: sin sugerencia; razón SeveralRoots
```

- Un miembro **no** necesita una arista propia hacia E o F para que esas raíces estén en su `RootCauses`.
- Con el texto anterior, `SourceRoots` era `{CycleRoot}` y se prometía la recuperación, pero ninguna corrección válida
  de un solo miembro la consigue: corregir A deja C y B fallando por F; corregir B deja A y D fallando por E.
- Si E se recupera entre preflight y commit, `RootCauses(B)` pasa a `{CycleRoot[A, B, C, D], (F, DivisionByZero)}`: la
  observación `Upstream` no coincide y el commit aborta antes de escribir.

### 4.6 U6 — aridad errónea junto a otros fallos (MAT-3)

```text
(a) H = #<h> + 1     → [BrokenReference(h)]
    S = ABS(H, 2)    → no se evalúa
                       [DF{H; [H]; (H, BrokenReference, [h])},
                        InvalidArguments(ABS, 2)]
    RootCauses(S) = { (H, BrokenReference, [h]), (S, InvalidArguments, [(ABS, 2)]) }
    K: palletTolerance = S + 1   → Upstream; SourceRoots de dos elementos: sin sugerencia; razón SeveralRoots

(b) D = ABS(#<m>, 2) → no se evalúa
                       [BrokenReference(m), InvalidArguments(ABS, 2)]
    RootCauses(D) = { (D, BrokenReference, [m]), (D, InvalidArguments, [(ABS, 2)]) }

(c) D = MIN(1) + ABS(1, 2) + MAX(3) + ABS(4, 5) + ABS()
                     → no se evalúa
                       [InvalidArguments(ABS, 0), InvalidArguments(ABS, 2),
                        InvalidArguments(MAX, 1), InvalidArguments(MIN, 1)]
    RootCauses(D) = { (D, InvalidArguments, [(ABS, 0), (ABS, 2), (MAX, 1), (MIN, 1)]) }

(d) D = 1 / 0 + ABS(1, 2)
                     → no se evalúa: [InvalidArguments(ABS, 2)], sin DivisionByZero
```

- La aridad se comprueba sobre el árbol enlazado **antes** de la evaluación numérica (P2.2, P7.5, P10.4), con la
  autoridad de aridad de `FunctionRegistry` (P10.2).
- En (a), con el texto anterior, S solo tenía `DF{H}` y §7.2 prometía «Corregir H permitiría recuperar»; al corregir H,
  S pasa a `InvalidArguments` y R1 bloquea.
- En (c), las dos llamadas `ABS(…, 2)` dan un solo diagnóstico: uno por par distinto (token, número de argumentos).

### 4.7 U7 — SCC no simple (MAT-4)

```text
B = C ; C = B + A ; A = D ; D = A + B
SCC = {A, B, C, D}; aristas internas B→C, C→B, C→A, A→D, D→A, D→B: 6 aristas, 4 miembros: no simple

A = B = C = D = [Cycle[A, B, C, D]]
RootCauses(cada miembro) = { CycleRoot[A, B, C, D] }

K: palletTolerance = B + D   → Upstream; SourceRoots = {CycleRoot[A, B, C, D]}: sin sugerencia de recuperación
```

- Corregir B deja el ciclo A↔D; corregir A deja el ciclo B↔C: ningún `ChangeDefinition` de un solo miembro recupera la
  fuente, y R1 bloquea cada uno.
- Aunque `SourceRoots` tenga un solo elemento, un `CycleRoot` de una SCC no simple **nunca** permite la sugerencia. El
  bloqueo se informa con la razón `NonSimpleCycle` y los miembros del ciclo, sin afirmar que haya otras fuentes
  inválidas (M10(d)).

### 4.8 U8 — fuente hermana `MissingTarget` (MAT-2)

```text
Holgura = #<id-roto> + 1                     → [BrokenReference(id-roto)]
K: verticalClearance = Holgura + 2           → RepairableSemanticFailure(Upstream, Holgura)
K: palletTolerance  = Holgura + #<gone>      → RepairableMissingTarget({gone})   (precedencia de P24.6)
```

- `SourceRoots(verticalClearance) = {(Holgura, BrokenReference, [id-roto])}`.
- `palletTolerance` no es `Upstream`: su `RootCauses` no la convierte en una fuente con causa superior.
- **Sin sugerencia de recuperación** para ninguna de las dos: tras corregir Holgura, `gone` sigue ausente y R1 bloquea.
  `verticalClearance` lleva la razón `OtherInvalidSources` con `palletTolerance` (M10(d)).

### 4.9 U9 — cambio del miembro de entrada en la misma SCC (MAT-5)

```text
A = B ; B = A                                 → SCC cíclica {A, B}, ciclo simple
antes:   H = A + 1   → [DF{A; [A]; CycleRoot[A, B]}]    RootCauses(H) = {CycleRoot[A, B]}
después: H = B + 1   → [DF{B; [B]; CycleRoot[A, B]}]    RootCauses(H) = {CycleRoot[A, B]}
```

- `RootCauses(H)` no cambia: las dos raíces son la unidad `CycleRoot[A, B]`, sin dueño.
- Una `RepairDecisionObservation` `Upstream` de una fuente retirada que lee H **coincide**.
- Una `SymbolResultObservation` de H **no** coincide: cambian la causa y la cadena.
- Es semántica **nueva** de A3: V6 no definía el símbolo raíz de un fallo que empieza en una SCC (A3 §8).

---

## 5. Decisión A3: modelo M1–M10

### 5.1 M1 — Elegibilidad de evaluación

Antes de evaluar, `RegistryEvaluation` aplica a cada definición la **comprobación semántica estática** de V6 (P2.2,
l. 714-716; P7.5, l. 914-917; D4, l. 319-320): sobre el `BoundExpression`, contra el contexto del snapshot y **sin
evaluación numérica**. `ExpressionEvaluator` evalúa la definición de un símbolo **solo** si no tiene **ninguno** de estos
bloqueos:

1. es miembro de una SCC cíclica (P13.1);
2. su propia definición referencia algún id ausente (P12.4);
3. su propia definición tiene alguna llamada con aridad errónea (`InvalidArguments` estático; P7.5, P10.4);
4. su definición es `NonCanonicalForm` (P2.8, l. 750-754; D11, l. 825-828: «no se evalúa»);
5. alguna de sus dependencias directas falla.

- Con cualquier bloqueo, `RegistryEvaluation` lo marca `Failed` **sin evaluación numérica** (P14.3, P13.4).
- El evaluador de G6 nunca recibe un símbolo presente sin valor: su contrato no cambia. Un literal sigue evaluándose a
  su propio valor (P14.6).
- `NonCanonicalForm` de una definición es una `Expression` que solo es un número sin unidad (`Number` o
  `Negate(Number)`). Ningún escritor la produce (P2.8), así que en G7 solo aparece con una tabla sintética; G8 la prueba
  por su camino real de persistencia. Al no tener referencias, nunca coexiste con los bloqueos 1, 2, 3 o 5.
- **Fallos numéricos latentes (N-5).** `DivisionByZero` y `NonFiniteResult` solo existen si el evaluador corre. Si un
  símbolo tiene cualquier bloqueo, los fallos numéricos que aparecerían al quitarlo **no** se descubren en ese snapshot:

  ```text
  A = #<a> + 1 ; D = A + 1 / 0   → D = [DF{A; [A]; (A, BrokenReference, [a])}]; el DivisionByZero de D es latente
  ```

  `RegistryEvaluation` no evalúa especulativamente alrededor de valores que faltan. Una sugerencia de recuperación solo
  se apoya en los diagnósticos conocidos del snapshot, conforme a «ninguna otra fuente inválida conocida» de §7.2.6
  (l. 2792-2797).

### 5.2 M2 — Diagnósticos de un símbolo no evaluado

Recibe **todos** los aplicables:

- **A.** un `BrokenReference` por cada id ausente distinto de su propia expresión, con ese id en `RelatedSymbols`;
- **B.** un `Cycle` si pertenece a una SCC cíclica, con la lista completa de miembros en el orden de P11.1 (P13.2);
- **C.** un `DependencyFailed` por cada dependencia directa que falla y está **fuera** de su propia SCC (M4);
- **D.** un `InvalidArguments` estático por cada par distinto (token de la función, número de argumentos) de las llamadas
  de su propia definición con aridad errónea, con esos dos datos estables;
- **E.** `NonCanonicalForm`, cuando aplica.

Ninguno suprime a otro. Los diagnósticos son **locales**: un símbolo solo recibe los que salen de su propia definición y
de sus propias aristas; `RootCauses` (M6) no añade diagnósticos.

Un símbolo evaluado:

- da `Success`; o
- da `Failed` con **un** diagnóstico numérico del evaluador de G6 (`DivisionByZero` o `NonFiniteResult`): el primer fallo
  termina la evaluación. Por M1, el evaluador nunca llega a un `InvalidArguments` en `RegistryEvaluation`.

La precedencia de `InspectBinding` sobre una fuente de rack (P24.6; D18) **no cambia**: M2 trata de símbolos.

### 5.3 M3 — Orden de los diagnósticos

1. **Orden primario: P15.7**, posición, código y dueño. Los diagnósticos de evaluación no tienen posición (P15.1) y los
   de un símbolo comparten dueño, así que decide el código en el orden del catálogo cerrado (P15.3), el que ya fijaron G5
   y G6:

   ```text
   BrokenReference 22 < Cycle 23 < DependencyFailed 24 < InvalidArguments 25
                      < DivisionByZero 26 < NonFiniteResult 27 < NonCanonicalForm 28
   ```

2. **Desempate nuevo**, con el mismo dueño y el mismo código:
   - `BrokenReference`: el id ausente, en el orden de P11.1;
   - `DependencyFailed`: el id de la causa directa, en el orden de P11.1;
   - `InvalidArguments`: el token de la función en orden `Ordinal` y, después, el número de argumentos;
   - `Cycle`: un símbolo tiene a lo sumo uno, y su lista de miembros ya va en el orden de P11.1;
   - `DivisionByZero`, `NonFiniteResult` y `NonCanonicalForm`: a lo sumo uno por símbolo.
3. **Nunca** por texto, mensaje, cultura, hash ni orden de las entradas.

Para un símbolo no evaluado, el orden es `BrokenReference` (ids ascendentes) → `Cycle` → `DependencyFailed` (causas
ascendentes) → `InvalidArguments` (token y número ascendentes). Ejemplo:

```text
A = ABS(B, #<m>) + E ; B = A ; E = 1 / 0
A = [BrokenReference(m), Cycle[A, B], DF{E; [E]; (E, DivisionByZero)}, InvalidArguments(ABS, 2)]
```

El «primer diagnóstico» de un símbolo, en M4, es el primero en este orden.

### 5.4 M4 — Estructura de `DependencyFailed`

Cada `DependencyFailed` pertenece a **una** dependencia directa `d` que falla, fuera de la SCC de su dueño, y tiene
esta estructura estable:

```text
Causa  = d                                   // el «id de la causa» de P14.3; también en RelatedSymbols
Cadena = [d] + cola
         cola = Cadena del primer diagnóstico de d, si ese diagnóstico es DependencyFailed
         cola = []                           en otro caso
Raíz   = firma (M5) del primer diagnóstico que no es DependencyFailed al que se llega,
         es decir, del primer diagnóstico del último eslabón de la cadena
```

- La cadena es **una** cadena representativa, basada en nodos y aristas: **no** es por SCC. Cumple P15.1 y el dato
  «el id de cada eslabón» de P21.6. **No** es el conjunto de causas raíz (M6).
- Es finita y sin repeticiones: cada causa pertenece a una SCC anterior en el orden de la condensación.
- El primer diagnóstico de un miembro de ciclo nunca es `DependencyFailed`: `Cycle` (23), y en su caso
  `BrokenReference` (22), van antes. Si la cadena llega a un miembro, la raíz es su `CycleRoot` cuando su primer
  diagnóstico es `Cycle`, o la firma agregada de su `BrokenReference` cuando tiene una referencia rota propia.
- En un símbolo no miembro con `DependencyFailed` e `InvalidArguments` propio, el primer diagnóstico es
  `DependencyFailed` (24 < 25): la cadena sigue por él, y la raíz propia de `InvalidArguments` está en `RootCauses`
  (U6(a)).
- La cola se **comparte** con la cadena del diagnóstico de `d`: construir un `DependencyFailed` cuesta O(1), y ninguna
  cadena se copia ni se enumera por caminos.

Ejemplo, con `A = B + 1`, `B = C + 1` y `C = 1 / 0`:

```text
C = [DivisionByZero]
B = [DF{C; [C]; (C, DivisionByZero)}]
A = [DF{B; [B, C]; (C, DivisionByZero)}]
```

### 5.5 M5 — Firma de causa raíz

| Raíz | Firma | Notas |
|---|---|---|
| Numérica | `(dueño, DivisionByZero)` o `(dueño, NonFiniteResult)` | Solo el código (P21.6) |
| Aridad | `(dueño, InvalidArguments, [pares (token, número de argumentos)])` | **Agregada por dueño**: la lista ordenada de sus pares distintos (M3); una sola raíz por dueño |
| Forma no canónica | `(dueño, NonCanonicalForm)` | Solo el código (P21.6) |
| Referencia rota | `(dueño, BrokenReference, [ids ausentes en el orden de P11.1])` | **Agregada por dueño**: nunca una raíz distinta por cada id ausente |
| Ciclo | `CycleRoot[miembros de la SCC en el orden de P11.1]` | **Una** unidad: ningún miembro es un dueño privilegiado, y entrar por A o por B da la misma raíz |

- Un símbolo cuyo fallo es solo `DependencyFailed` no es raíz.
- Una firma nunca contiene texto localizado, salida del formatter, posiciones ni la traza opt-in.

### 5.6 M6 — `RootCauses`, por SCC

`RootCauses` es una operación **pura y derivada** del núcleo de grafo y evaluación de G7, sobre una
`RegistryEvaluation`. **La unidad de propagación es la SCC.** Definición autoritativa:

```text
RootCauses(s)        = RootCausesScc(SCC(s))                       si s da Failed
RootCauses(s)        = { }                                         si s da Success

RootCausesScc(C)     = RaícesPropias(m)             para cada miembro m de C
                     ∪ { CycleRoot(C) }             si C es cíclica
                     ∪ RootCauses(d)                para cada arista m → d con m ∈ C, d ∉ C y d que falla
```

`RaícesPropias(m)` puede contener:

- la firma agregada de `BrokenReference`, si m tiene alguno;
- la firma agregada de `InvalidArguments`, si m tiene alguno;
- la firma de `NonCanonicalForm`, si aplica;
- la firma numérica, si m se evaluó y falló.

Propiedades:

- en una SCC acíclica (un solo símbolo) se reduce a la regla por símbolo: sus raíces propias y las de sus dependencias
  que fallan;
- en una SCC cíclica, **todos** los miembros tienen el **mismo** `RootCauses`: el `CycleRoot`, las raíces propias de
  cada miembro y las raíces de cada dependencia externa que falla de cualquier miembro;
- vacío si s da `Success`, y no vacío si s da `Failed`;
- **deduplicado** por firma;
- **determinista** y ordenado estructuralmente (A3 §5.7);
- sin enumeración de caminos;
- nunca texto localizado, salida del formatter, posiciones ni la traza opt-in;
- **no se persiste** (P27.4).

`RootCauses` y los diagnósticos son distintos: un miembro sin arista hacia E no recibe `DF{E}`, pero su `RootCauses`
contiene la raíz de E si E entra en su SCC por otro miembro (U4, U5). Es la **única** autoridad del conjunto de causas
raíz. La consumen el contrato futuro de `SymbolResultObservation`, de `Upstream` y de §7.2 (M10), y el panel de
diagnóstico (P22.9). Nadie reimplementa el recorrido.

### 5.7 Orden de `RootCauses`

1. El código de la raíz, en el orden del catálogo: `BrokenReference` 22, `Cycle` 23, `InvalidArguments` 25,
   `DivisionByZero` 26, `NonFiniteResult` 27 y `NonCanonicalForm` 28.
2. El id relevante, en el orden de P11.1: el dueño, o el **miembro mínimo** para un `CycleRoot`.
3. Los datos estables: los ids ausentes, la lista completa de miembros, o la lista de pares (token, número de
   argumentos).

Dentro de una `RegistryEvaluation`, los criterios 1 y 2 ya identifican cada raíz: un dueño tiene a lo sumo una raíz de
cada código y las SCC son disjuntas. El criterio 3 completa el orden total. Nunca se ordena por hash. Ejemplo:
`A = 1 / 0 ; B = #<b> + 1 ; D = A + B` da `RootCauses(D) = [(B, BrokenReference, [b]), (A, DivisionByZero)]`, aunque
A < B.

### 5.8 M7 — Varias causas raíz

| Situación | Diagnósticos | `RootCauses` |
|---|---|---|
| Raíces detrás de dependencias directas distintas (U1) | Un `DependencyFailed` por dependencia directa que falla | Todas |
| Varias raíces detrás de una sola dependencia (U2) | Un `DependencyFailed`, con su cadena representativa | Todas: ninguna se descarta |
| Una raíz alcanzada por varios caminos (diamante) | Un `DependencyFailed` por dependencia directa que falla | La raíz, una vez |
| Un ciclo leído por varios miembros (U4) | Un `DependencyFailed` por miembro leído | El `CycleRoot` una sola vez, junto con las demás raíces de la SCC |
| Raíces externas que entran en una SCC por miembros distintos (U5) | Solo en el miembro que lee cada una | Todas, en todos los miembros y en sus dependientes |
| Fallo propio y fallo superior en el mismo símbolo (U3, U6) | Todos los aplicables (M2) | Las raíces propias y las superiores |

Nunca hay un diagnóstico por camino.

### 5.9 M8 — Coste · M9 — Determinismo

**M8.**

- Construcción del grafo, SCC iterativa, comprobación estática y diagnósticos de todos los símbolos: `O(V + E)` más el
  tamaño de las definiciones (acotado por P1.9) y el coste determinista de ordenar en P11.1.
- Orden topológico con el desempate de P11.1: `O(V log V + E)`.
- A lo sumo un `DependencyFailed` por arista del grafo y un `Cycle` por nodo. Los `BrokenReference` y los
  `InvalidArguments` están acotados por el tamaño de cada definición (P1.9).
- **Listas de miembros compartidas.** Todos los `Cycle` de una SCC y su `CycleRoot` referencian **una** lista inmutable
  de miembros por SCC; copiarla por miembro costaría `O(Σ|C|²)`.
- Cadenas con colas compartidas: `O(1)` por `DependencyFailed`.
- **Coste de `RootCauses` (N-4).** Una derivación sin memoria de `RootCauses(s)` es un recorrido **iterativo** de la
  condensación con visitados, sin recursión en profundidad, que visita cada SCC a lo sumo una vez por llamada:
  `O(V + E)`, más ordenar su resultado. Leída como recursión literal y sin visitados, la definición de M6 sería
  exponencial en diamantes apilados; esa lectura queda excluida. Pedirla sin memoria para muchos o todos los símbolos
  que fallan —el panel de P22.9 o la condición de §7.2— puede costar `O(V · (V + E))`.
- **Memoización por snapshot.** `RegistryEvaluation` **puede** guardar los `RootCauses` ya derivados, por SCC. Esa
  memoria pertenece a la `RegistryEvaluation` inmutable de un snapshot; no cambia ningún resultado, no es global, no se
  persiste y es determinista. Cada `RegistryEvaluation` nueva empieza sin memoria. No es observable semánticamente.
- Ningún camino raíz-nodo se materializa y ningún conjunto de raíces se persiste.

**M9.** Sin paralelismo, cultura ni reloj (P14.5). Cada elección sale de P11.1, de P15.7, del orden `Ordinal` de los
tokens y del orden del catálogo. El resultado no depende del orden de las entradas del registro, del hash, de la
cultura, del hilo ni de la memoización.

### 5.10 M10 — Contrato futuro de `PlanReadSet`, `Upstream` y §7.2

A3 **define** este contrato ahora; **G9 lo implementa** (y G10, la capa de texto de §7.2).

**(a) `SymbolResultObservation` con `Failed`.** Compara:

- los diagnósticos en el orden de M3;
- los datos estables de cada diagnóstico según P21.6: el id ausente de cada `BrokenReference`, cuya secuencia es la de
  los ids ausentes en el orden de P11.1; los miembros de `Cycle`; el token y el número de argumentos de cada
  `InvalidArguments`; y solo el código de los demás;
- para cada `DependencyFailed`: `Causa`, los ids de la `Cadena` representativa y la firma de la `Raíz`;
- `RootCauses(símbolo)` como dato estable **adicional**, con la regla de P21.6 y de D19: «Si la implementación necesita
  más datos para distinguir dos causas, los añade como datos estables» (l. 1682-1683; D19, l. 1206-1207).

Por tanto, un fallo con raíces {A, B} y otro con raíces {A} **no** son observaciones iguales, aunque los diagnósticos
coincidan (U2 con `a2` recuperado): `ABORT BEFORE WRITE`. `Success` sigue comparando el valor exacto.

**(b) `RepairDecisionObservation` con `Upstream`.** Este es el cambio material de D20 que exige el ADR de reemplazo
(A3 §12).

| | Contrato vigente (ADR-0041 D20) | Contrato de A3 |
|---|---|---|
| Datos estables | Por cada variable leída que falla: `SymbolId` + «su causa raíz» | Por cada variable leída que falla, en el orden de P11.1: `SymbolId` + `RootCauses(variable)` **completo**, por SCC (M6) |
| Coincidencia | «la misma variable sigue fallando por la misma causa raíz» | Las mismas variables fallan y el `RootCauses` de cada una es igual, firma a firma y en el mismo orden |

- Los eslabones intermedios de la cadena **no** entran: `Upstream` compara **por qué** falla la variable, no el camino
  representativo elegido.
- Si cualquier miembro del conjunto desaparece, aparece o cambia de firma, la observación **no** coincide:
  `ABORT BEFORE WRITE`, y no se borra ninguna fórmula (U5).
- `MissingTarget`, `Intrinsic` y `Domain` no cambian, ni la precedencia de D18.
- **Alcance de la protección (N-3).** `Upstream` protege la razón de **la fuente concreta que se retira**: por qué
  falla cada variable que lee. **No** observa por sí sola:
  - las demás fuentes, de este rack o de otros, que participaron en la clasificación global de §7.2 durante el
    preflight;
  - si el ciclo de un `CycleRoot` es simple o no: con los mismos miembros, `RootCauses` no cambia aunque cambien las
    aristas internas (`A = B ; B = A + B` pasa a `B = A`), y la observación coincide.

  Toda obligación de revalidar ese estado tiene que salir de las observaciones exactas que lleve el plan de G9, nunca de
  una implicación; ni CX-2 ni ninguna otra observación `Upstream` la dan por cubierta.

**(c) §7.2, sugerencia de recuperación.** Para una fuente clasificada por `InspectBinding` como
`RepairableSemanticFailure(Upstream)`:

```text
SourceRoots(fuente) = ∪ RootCauses(v)   para cada variable v que la fuente lee y que falla
```

`SourceRoots` solo se define para fuentes `Upstream`. La sugerencia «Corregir <X> permitiría recuperar…» para esa fuente
existe **solo** si se cumplen todas estas condiciones:

1. `SourceRoots(fuente) = {X}`;
2. si X es un `CycleRoot`, su SCC es un **ciclo simple** (A3 §6.2);
3. **todas** las fuentes fallidas de todos los racks que consumen el cierre de X —la propia fuente incluida— están
   clasificadas como `RepairableSemanticFailure(Upstream)` **y** tienen `SourceRoots = {X}`, con la misma X. El cierre de
   X es el de su dueño o, para un `CycleRoot`, el de sus miembros;
4. siguen vigentes las demás reglas de §7.2, entre ellas §7.2.6 y R1 sin relajar.

- Cualquier fuente fallida del conjunto clasificada como `RepairableMissingTarget`, `Intrinsic`, `Domain`, estructural
  (`FatalMalformedReference`, `FatalUnknownProperty`) o `FatalIncompatibleTarget` hace que la condición **no** se cumpla,
  aunque las variables que lee tengan raíces que coinciden con X (U8). Su `RootCauses` no la convierte en una fuente
  `Upstream`.
- La condición 3 exige **igualdad**: una fuente `Upstream` con `SourceRoots = {X, Y}` también la incumple, aunque
  contenga X.
- **Fallo propio estático de una fuente.** Una fuente cuya propia expresión tiene aridad errónea o forma no canónica es
  `Intrinsic` aunque además lea una variable que falla: el fallo propio precede al superior, leído del orden «(propio,
  superior y dominio)» de la precedencia de P24.6 (l. 2168-2169), y hace a la fuente «no corregible desde arriba»
  (§7.2.1, l. 2759-2761). La precedencia anterior de P24.6 se mantiene: por
  ejemplo, `ABS(#<gone>, 2)` es `RepairableMissingTarget`.
- Si X es un `CycleRoot` de un ciclo simple, `<X>` nombra a sus miembros y «corregir cualquiera de ellos rompe el ciclo»
  es cierto. En una SCC no simple **nunca** hay sugerencia (U7).
- **A3 cuenta firmas, no variables.** Con más de una firma nunca hay sugerencia, también si un mismo dueño aporta dos
  (U6(b)). §7.2.1 cuenta «variables cuya propia definición falla», con la que `H = ABS(#<m>, 2)` tendría una sola causa
  raíz y la sugerencia «Corregir H». A3 la oculta: es un cambio **directo y conservador** de D18 puntos 1-2 (A3 §10.3).
- **Sin sugerencias falsas respecto de lo conocido.** Todo se decide con los diagnósticos **conocidos** del snapshot
  (A3 §5.1, N-5). La regla puede ocultar una sugerencia válida y nunca muestra una que desmientan las fuentes inválidas
  conocidas de este rack o de los racks que consumen el cierre de X, que es el alcance de §7.2.6. Un fallo numérico
  latente puede aparecer después de la corrección (T-A3-39).
- **Límite heredado de V6.** Una fuente estructuralmente mal formada en un rack cuyo consumo del cierre no se puede
  determinar hace que las mutaciones que necesitan descubrimiento aborten (§5.1, l. 2601-2603; P21.4), y la condición no
  la mira. A3 no cambia esa regla y la registra para el ADR de reemplazo.

**(d) Bloqueo de la recuperación: razones.** Cuando la condición no se cumple para una fuente `Upstream`, nunca hay
sugerencia y el diagnóstico lleva, como datos estructurados y no como texto, **todas** las razones que aplican, en este
orden:

| Razón | Cuándo | Datos |
|---|---|---|
| `OtherInvalidSources` | Hay al menos una **fuente bloqueante** (abajo) | Las fuentes bloqueantes, con rack, propiedad y causa, como en §7.2.4, y si cada una está en el rack de la fuente o en otro |
| `SeveralRoots` | `SourceRoots` tiene más de una firma | `SourceRoots`, en el orden de A3 §5.7 |
| `NonSimpleCycle` | `SourceRoots = {CycleRoot(C)}` y C no es un ciclo simple | Los miembros de C |

**Fuente bloqueante.** Para una fuente `Upstream` f, otra fuente fallida g es bloqueante si está en un rack que consume
el cierre de alguna raíz de `SourceRoots(f)` y además:

- g no está clasificada como `RepairableSemanticFailure(Upstream)`; o
- g es `Upstream` y `SourceRoots(g)` contiene alguna raíz que **no** está en `SourceRoots(f)`.

Con una sola raíz X, «g no es bloqueante» equivale exactamente a la condición 3 de M10(c): corregir X recupera toda g
cuyas raíces son {X}. Con varias raíces, la lista es **informativa**: excluye las fuentes cuyas raíces están todas en
`SourceRoots(f)`, que no afirman nada que `SeveralRoots` no diga ya, aunque alguna pueda bloquear la corrección de una
raíz concreta. Por ejemplo, con `f = A1 + A2` en el rack K y `s3 = A1 + 1` en el rack K3, corregir A1 recupera s3 y la
corrección queda bloqueada solo por f; en cambio, con `A2 = #<a2> + A1` y `g = A1 + 1` en el mismo rack que
`f = A2 + 1`, corregir A2 recupera f y R1 bloquea por g.

- **Completitud.** Las tres razones corresponden a las condiciones 1, 2 y 3 de M10(c): cada condición que falla da su
  razón, y la condición 4 no añade ninguna condición de presentación. `SeveralRoots` y `NonSimpleCycle` se excluyen, y
  sus listas nunca están vacías: una SCC no simple tiene al menos dos miembros.
- **Texto literal de §7.2.4.** «La corrección de <X> no puede aplicarse mientras este rack mantenga otras fuentes
  inválidas» (l. 2778-2779) se usa **solo** si se cumplen las tres cosas: hay `OtherInvalidSources` con al menos una
  fuente bloqueante en el rack de f; no aplica `SeveralRoots`; y no aplica `NonSimpleCycle`. Entonces `<X>` es la raíz
  única, y la lista incluye también las fuentes bloqueantes de otros racks, como en V6.
- **En los demás casos**, la capa de texto nombra con verdad cada razón que aplica y lista las fuentes bloqueantes con su
  rack, las raíces o los miembros. **No** dice «este rack» si ninguna fuente bloqueante está en él, y **no** afirma que
  haya otras fuentes inválidas si no hay `OtherInvalidSources`.
- **Veracidad de `SeveralRoots` y `NonSimpleCycle`.** Su redacción solo puede afirmar que la recuperación corrigiendo una
  sola causa **no está garantizada**, nunca que sea imposible con una corrección. Corregir un intermedio común puede
  recuperarla, como B en U2 o H en CX-2; y en una SCC no simple, corregir un miembro que esté en todos sus ciclos
  también.
- **Aviso de reparación.** El aviso de §7.2.4 con la fórmula canónica (l. 2781-2784) se conserva **solo** si el rack es
  reparable (`RackRepairability = Repairable`, P24.6). Si el rack está `Blocked` —por ejemplo, con una fuente
  estructural hermana—, el texto dice que el rack está bloqueado sin reparación (§5.1, l. 2601-2603; §7.2, l. 2731-2733)
  y **no** anuncia que la reparación eliminará la fórmula.
- La redacción en español de los casos nuevos es de la capa de texto (P15.4, G10). Como amplía el punto 4 de D18, que
  fija textos literales, se fija en el ADR de reemplazo con estas restricciones (A3 §12).
- **Precisión sobre la indicación del Coordinador.** La orden de A3-R1 pedía que, sin sugerencia, siguiera «el
  diagnóstico de bloqueo genérico». El bloqueo se mantiene, sin sugerencia y con el aviso de reparación cuando el rack
  es reparable; A3-R1 solo precisa qué razón informa, porque el texto literal de §7.2.4 sería falso en U7, con varias
  raíces sin fuentes bloqueantes, con bloqueos solo en otros racks y, en su aviso de reparación, con un rack `Blocked`.
- **Rechazo de R1.** Usa la misma clasificación y las mismas razones (§7.2.7, l. 2798-2799; D18 punto 7), las del
  **estado diagnosticado antes de la corrección**, como el mensaje de P21.2 paso 8 (l. 1538-1541): para cada fuente
  fallida de los racks que bloquean, sus razones del panel. No describen el estado resultante del plan rechazado. El
  fallo tipado del estado resultante —el rack que no resuelve, con su `OutOfRange` o su clasificación— se sigue
  informando como hoy (P22.8, P24.5); las razones del panel se añaden solo para las fuentes que ya fallaban.

**(e) Cadena representativa frente a causas raíz (CX-3).**

```text
Z = #<z> + 1 ; X = Z + 1 ; Y = Z + 2 ; B = X + Y ; D = B * 2 + W
preflight: D = [DF{B; [B, X, Z]; (Z, BrokenReference, [z])}]    RootCauses(D) = {(Z, BrokenReference, [z])}
commit:    X pasa a 5 por una edición ajena; B = [DF{Y; [Y, Z]; …}]
           D = [DF{B; [B, Y, Z]; (Z, BrokenReference, [z])}]    RootCauses(D) = {(Z, BrokenReference, [z])}
```

- Una `SymbolResultObservation` de D (por ejemplo, `Before(D)` y `After(D)` de un `ChangeDefinition(W, …)` que R2
  admite) **no** coincide, porque P21.6 compara los ids de la cadena: `ABORT BEFORE WRITE`.
- Una `RepairDecisionObservation` `Upstream` de una fuente retirada `palletTolerance = D + 1` **coincide**, porque solo
  compara `RootCauses`: si nada más difiere, el commit sigue.

La distinción es deliberada: la observación de símbolo conserva la comparación conservadora de P21.6, y la decisión de
reparación compara la razón, no el camino (D20, l. 1289). Lo mismo ocurre en U9, donde además cambia la causa directa.

---

## 6. Ciclos, orden y conjuntos

### 6.1 Ciclos

- La detección sigue basada en SCC (P13.1); un nodo con arista a sí mismo es una SCC cíclica de un miembro.
- Firma de raíz: `CycleRoot[miembros en el orden de P11.1]`.
- Todo miembro recibe `Cycle` con la misma lista ordenada.
- Un miembro puede tener además `BrokenReference` e `InvalidArguments` propios de su definición, y `DependencyFailed`
  por dependencias que fallan fuera de su SCC. **Nunca** tiene `DependencyFailed` por una arista dentro de su SCC,
  tampoco por una autorreferencia.
- `RootCauses` es el mismo para todos los miembros y reúne las raíces de toda la SCC (M6).
- P13.4 («no son evaluables (`Cycle` o `DependencyFailed`)», l. 1182-1183) se lee así: un miembro tiene siempre `Cycle`;
  un dependiente transitivo que no es miembro tiene al menos un `DependencyFailed`; ninguno se evalúa y ninguno tiene
  fallback.
- P13.5 (l. 1186-1187) se cumple sin cambio: todos los miembros dan `Failed` con `Cycle` y sus dependientes dan
  `DependencyFailed`.

### 6.2 Ciclo simple y ruptura de un ciclo

Una SCC cíclica C es un **ciclo simple** si, considerando solo sus aristas internas, cada miembro tiene exactamente una
arista interna saliente y exactamente una entrante. Para una SCC fuertemente conexa equivale a:

```text
número de aristas internas de C == número de miembros de C
```

- Una autorreferencia (un miembro, una arista a sí mismo) es un ciclo simple.
- Se decide sobre las aristas del grafo, nunca sobre la forma textual de las definiciones.
- En un ciclo simple sin otras raíces, cualquier `ChangeDefinition` válido de un miembro, que por R2 tiene que evaluar
  bien, rompe el único ciclo y deja evaluables a todos los miembros.
- En una SCC **no simple**, un `ChangeDefinition` de un miembro puede no romper todos los ciclos internos (U7).
- Las autorreferencias cuentan como aristas internas: en `A = A + B ; B = A` hay tres aristas internas (A→A, A→B,
  B→A) y dos miembros, así que la SCC no es simple; corregir B deja la autorreferencia de A.

**Refinamiento de P13.5 y de §5.2, decisión 7.** La lectura amplia de que un ciclo «se rompe con `ChangeDefinition` de
**un** miembro» (P13.5, l. 1190; «se rompen con `ChangeDefinition` de un miembro», §5.2 decisión 7, l. 2624-2625, y D14,
l. 942-943) se precisa: la **recuperación garantizada con un solo miembro** vale solo para un ciclo simple. En una SCC
no simple pueden hacer falta varias correcciones, cada una gobernada por R1–R3 y P13.3, y no se promete ninguna. Este
refinamiento afecta a la semántica de la sugerencia de recuperación y tiene que entrar en el ADR de reemplazo (A3 §12).

### 6.3 Orden topológico (DI-1)

Conceptualmente: se elige repetidamente, entre los nodos que no son miembros de un ciclo y cuyas dependencias que tampoco
lo son ya se emitieron, el menor `SymbolId` en el orden de P11.1.

- Se admite cualquier algoritmo equivalente.
- Las aristas hacia miembros de un ciclo y hacia ids ausentes no bloquean.
- Los miembros de un ciclo quedan **fuera** del orden de evaluación de P16.2.
- Los dependientes de un ciclo **aparecen** en ese orden, porque la regla conceptual los emite, y fallan sin evaluación
  numérica (M1).
- El orden no depende de la enumeración de las entradas.

### 6.4 Orden de la lista de ciclos (DI-2)

Ascendente por el miembro mínimo de cada ciclo en el orden de P11.1. Las SCC son disjuntas, así que el orden es total
(P12.5).

### 6.5 Conjuntos transitivos (DI-3)

- Transitivo = alcanzable por un camino de longitud ≥ 1.
- Un miembro de ciclo puede pertenecer a sus propios conjuntos transitivos de dependencias y dependientes a través de un
  camino no vacío del ciclo.
- Los ids ausentes no son nodos del grafo.
- Coincide con P21.2 (`I = {X} ∪ dependientes transitivos de X`, l. 1530) y con P20.7 (un miembro de ciclo tiene
  dependientes).

### 6.6 Conjuntos derivados de G7 (DI-4)

G7 expone:

- `Dependencies(s)` directas;
- `Dependents(s)` directos;
- `Dependencies(s)` transitivas;
- `Dependents(s)` transitivos;
- `RootCauses(s)` (M6);
- para cada ciclo de la lista, si es un ciclo simple (A3 §6.2).

`RootCauses` y el predicado de ciclo simple son las únicas adiciones de A3. **No** entran en G7: el cierre afectado de
racks, el descubrimiento de consumidores, la implementación del `PlanReadSet`, la ejecución de la reparación, la
sugerencia de recuperación ni el `MutationPlan`. El conjunto afectado y el conjunto leído de P12.7 siguen en P21.2 y
P21.6, en G9.

---

## 7. S1, `InvalidArguments` y traza

### 7.1 S1 = A

- G7 crea los `BrokenReference` de un símbolo desde los datos autoritativos de dependencias del grafo (P11.1, P12.4), en
  el orden de P11.1 (M3).
- Por M1, `RegistryEvaluation` nunca llama al evaluador para un símbolo con ids ausentes ni con aridad errónea.
- `ExpressionEvaluator` de G6 **no** se modifica. Llamado directamente, sigue informando los ids ausentes en orden de
  primera aparición (el bucle de `src/RackCad.Application/Expressions/ExpressionEvaluator.cs:148-176` sobre
  `ReferencesOf`, `:181-221`), como fija
  `ExpressionEvaluatorTests.UNA_REFERENCIA_A_UN_ID_AUSENTE_DA_BROKENREFERENCE_SIN_EVALUAR_NADA`, y sigue comprobando la
  aridad al llegar a cada llamada (`:280-283`). Para un mismo árbol, la llamada directa puede dar otro resultado que
  `RegistryEvaluation` (U6(d)); para los símbolos del registro manda `RegistryEvaluation`.
- El evaluador no se convierte en una segunda autoridad del grafo: P11.4 enumera los consumidores de la extracción de
  dependencias y el recorrido interno del evaluador no es uno de ellos, así que P11.5 se cumple.

### 7.2 `InvalidArguments`: forma estable en G7

- El texto anterior difería a G8 los datos estables de `InvalidArguments`. A3-R1 **cierra ese hueco** para la semántica
  del grafo: el `InvalidArguments` estático de G7 (M1, M2.D) lleva ya el **token de la función** y el **número de
  argumentos** que exige P21.6 (l. 1677; D19, l. 1203).
- Uno por par distinto (token, número de argumentos) de la misma definición; los duplicados exactos se deduplican.
  Orden: código, token `Ordinal`, número de argumentos (M3). Raíz agregada por dueño (M5).
- La comprobación usa la autoridad de aridad de `FunctionRegistry` (P10.2) sobre el contexto del snapshot, la misma que
  usan el binder y el evaluador.
- G8 sigue obligado a probar los árboles persistidos con aridad errónea por su camino real de persistencia, y
  `InspectBinding` clasifica una fuente con aridad errónea como `Intrinsic`, con la misma comprobación estática y los
  mismos datos, salvo que la precedencia de P24.6 la clasifique antes (estructural, id ausente o tipo incompatible;
  A3 §5.10(c)). El diagnóstico sin datos del evaluador de G6 (`ExpressionEvaluator.cs:329-330`) no llega a
  `RegistryEvaluation`.

### 7.3 Traza opt-in y Explain

- La «cadena de causas de un fallo» de la traza (P16.3, l. 1277-1278) son los `DependencyFailed` del símbolo con sus
  cadenas representativas, referenciadas y no copiadas.
- `RootCauses` no forma parte de la traza: se deriva de la `RegistryEvaluation`.
- La traza sigue siendo opt-in, acotada, desactivable sin cambiar ningún resultado, y nunca se compara (P21.6).
- Para ID28 (P27.1, l. 2255; D24, l. 1456-1461), los diagnósticos llevan su cadena representativa y `RootCauses` se
  deriva sin re-evaluar. Nada se persiste.

---

## 8. Escenarios lineales de V6: alcance de la compatibilidad

**Solo** los escenarios **lineales sin ciclo** de V6 que enumera esta tabla conservan su resultado. A3 no afirma nada
más: no declara igualdad con V6 para ningún otro caso, tampoco para un fallo con una sola raíz (un diamante no estaba
definido en V6, A3 §3.2) ni para un fallo con raíz de ciclo.

| Escenario | Con A3 | Resultado |
|---|---|---|
| P8.10 caso C (l. 987-995) y T-V4-01 (l. 2355) | `A = [BrokenReference(id-roto)]`; `B = [DF{A; [A]; (A, BrokenReference, [id-roto])}]`; `AlturaFinal = [DF{B; [B, A]; (A, BrokenReference, [id-roto])}]` | Igual: `OutOfRange(B)` y plan vacío |
| T-V5-01 (l. 2372) | `D = X + #<id-roto>` con X sana: `D = [BrokenReference(id-roto)]`, `RootCauses(D) = {(D, BrokenReference, [id-roto])}` | Igual: **COMMIT** |
| T-V5-02 (l. 2373) | Otra causa: cambian los diagnósticos y `RootCauses` | Igual: `ABORT BEFORE WRITE` |
| T-V6-01 (l. 2388) | `Upstream(Holgura: {(Holgura, BrokenReference, [id-roto])})` | Igual: **COMMIT** |
| T-V6-02 (l. 2389) | Holgura se recupera: `Healthy` | Igual: `ABORT BEFORE WRITE` |
| T-V6-03 (l. 2390) | Otra causa raíz: cambia `RootCauses(Holgura)` | Igual: `ABORT BEFORE WRITE` |
| P21.6, escenario de `RepairBrokenRack` (l. 1844-1853) | `RepairDecision(R, palletTolerance, Upstream(Holgura: {(Holgura, BrokenReference, [id-roto])}))` y `After(Y, Success(40))` | Igual: commit o `ABORT BEFORE WRITE` en cada rama del escenario |
| T-V4-05, T-V4-06 y T-V4-11 (l. 2359-2360, 2365) | Cada fuente `Upstream` tiene `SourceRoots` de un elemento; la fuente de T-V4-06 (`ABS(A, B)`) es `Intrinsic` y no tiene `SourceRoots`; los bloqueos de T-V4-06 y T-V4-11 son `OtherInvalidSources` | Igual: sugerencia o bloqueo según §7.2, con el texto de §7.2.4 |

En esos escenarios, la única diferencia es el dato estable adicional `RootCauses`, presente de forma mecánica con un solo
elemento.

**Raíces de ciclo: semántica nueva.** V6 no definió un único «símbolo donde empieza el fallo» (l. 1694; D20, l. 1289)
cuando el fallo empieza en una SCC. A3 fija la raíz como la unidad **sin dueño** `CycleRoot` (M5). Por tanto, cambiar el
miembro por el que se entra en la misma SCC no cambia `RootCauses` (U9). Eso es semántica fijada por A3, no «igual que
V6»: con la lectura literal de D20, la raíz sería un símbolo y el cambio de miembro de entrada podría no coincidir.

---

## 9. Modelos candidatos

| Modelo | Qué haría | Decisión | Por qué |
|---|---|---|---|
| **MC-A** (R-A) — un `DependencyFailed` por dependencia directa que falla, con cadena representativa | Cardinalidad por dependencia y cadenas lineales con colas compartidas | **Aceptado con modificación** | Encaja con P14.3, P15.1 y P21.6 y es `O(V + E)`. Modificaciones: (1) miembros de ciclo con `DependencyFailed` solo fuera de su SCC; (2) coexistencia con los fallos propios estáticos, sin evaluación; (3) orden por código (P15.7) antes que por causa, con desempates deterministas; (4) «primer diagnóstico» según M3, raíces agregadas por dueño y raíz de ciclo como unidad; (5) `RootCauses`, porque «varias raíces = varios `DependencyFailed`» es falso con varias raíces detrás de una sola dependencia (U2) o dentro de una SCC (U4, U5); (6) `RootCauses` por SCC (MAT-1); (7) aridad y forma canónica como bloqueos estáticos (MAT-3); (8) datos del `PlanReadSet` y condición de §7.2 según M10, con clasificación `Upstream` y ciclo simple (MAT-2, MAT-4) |
| **MC-B** (R-B) — un `DependencyFailed` por símbolo, el de la primera dependencia que falla en P11.1 | Un solo diagnóstico superior por símbolo | **Rechazado** | Incumple P14.3 para las dependencias no elegidas; oculta causas posteriores (CX-1); con una sola raíz representativa, §7.2 mostraría una raíz única falsa y la reparación podría borrar una fórmula con la razón de su fuente cambiada (CX-2) |
| **MC-C** (R-C) — un `DependencyFailed` por causa raíz distinta | Diagnósticos por raíz | **Rechazado** como modelo de diagnósticos | Materializa las raíces en los diagnósticos de todos los símbolos, `O(E·R)` en el peor caso, las pida alguien o no; exige un segundo desempate de camino por cada par símbolo-raíz; pierde el id de la causa directa de P14.3. El conjunto de raíces se conserva en `RootCauses`, bajo demanda y con memoización por snapshot (M6, M8) |
| Un `DependencyFailed` por cada camino hasta una raíz | Enumeración completa | **Rechazado** | Crece como 2^k con k diamantes apilados |
| Una sola causa raíz representativa en `Upstream` (lectura literal del singular de D20) | Mantener una raíz por variable | **Rechazado** | CX-2: incumple la regla de derivación de D19 |
| `RootCauses` por aristas propias del símbolo (texto anterior, blob `97996b7`) | Seguir solo las aristas del símbolo, también dentro de una SCC | **Rechazado** (MAT-1) | Pierde las raíces que entran en la SCC por otro miembro: promesa falsa en U5 y `Upstream` que no detecta la recuperación de E |
| Aridad solo al evaluar (texto anterior) | Ocultar `InvalidArguments` tras un bloqueo | **Rechazado** (MAT-3) | V6 comprueba la aridad sobre el árbol en cada snapshot (P2.2, P7.5); oculta una invalidez conocida (U6(a)) |
| `CycleRoot` como raíz única en cualquier SCC (texto anterior, heredado de §7.2.3) | Prometer la recuperación con un solo miembro | **Rechazado** (MAT-4) | Falso en una SCC no simple (U7) |
| Texto de bloqueo de §7.2.4 para todo fallo de la condición | Decir siempre «mientras este rack mantenga otras fuentes inválidas» | **Rechazado** | Afirma algo falso cuando no hay otras fuentes inválidas (U7, o una fuente única con varias raíces) y deja `<X>` sin referente con varias raíces; A3 usa razones estructuradas (M10(d)) |

---

## 10. Cláusulas afectadas y lectura conjunta

Cuando exista el nuevo freeze (A3 §13), estos textos se leen con A3. Las líneas son las de los blobs exactos de V6
(`ef4db3a`) y de ADR-0041 (`c6a3d2b`). En la columna «Lugar», `§n` es de V6; en la columna «Lectura con A3», las
referencias a este documento llevan el prefijo `A3`.

**Criterio de clasificación.** En V6, una cláusula está **enmendada directamente** cuando A3 precisa o sustituye su
regla: cardinalidad, orden, datos, condición o alcance. Está **leída con A3** cuando su texto sigue siendo cierto y A3
solo la aplica. En ADR-0041, una decisión **cambia directamente** solo si su texto aceptado deja de ser cierto con A3; si
sigue siendo cierto aunque A3 lo complete, **se lee con A3**. Por eso P14.3, P13.4 y la fila `DependencyFailed` de P21.6
están enmendadas en V6, mientras que sus gemelas D15 y D19 se leen: sus textos aceptados siguen siendo ciertos con M2 y
M4.

**Regla general de lectura.** Toda otra mención en V6 o en ADR-0041 de «la causa raíz» de una variable, de «la cadena
hasta la causa raíz» o de «la causa» de un `DependencyFailed` se lee: cadena representativa y firma de raíz para cada
`DependencyFailed` (M4, M5), y `RootCauses` por SCC para el conjunto de causas raíz de un símbolo (M6).

### 10.1 V6, enmendadas directamente

| Lugar | Texto vigente | Lectura con A3 |
|---|---|---|
| P13.4 (l. 1182-1183) | «no son evaluables (`Cycle` o `DependencyFailed`)» | A3 §6.1: un miembro tiene siempre `Cycle`, y puede tener fallos propios estáticos y `DependencyFailed` por dependencias fuera de su SCC; un dependiente no miembro tiene al menos un `DependencyFailed` |
| P13.5, ruptura (l. 1190) | «se rompe con `ChangeDefinition` de **un** miembro» | A3 §6.2: la recuperación garantizada con un solo miembro vale solo para un ciclo simple |
| P14.3 (l. 1204-1205) | «si una dependencia falla, sus dependientes reciben `DependencyFailed` con el id de la causa, sin evaluarse» | M1 y M2: un `DependencyFailed` por cada dependencia directa que falla fuera de la SCC del dueño, con `Causa` = ese id; coexiste con los fallos propios estáticos y con `Cycle` |
| P15.1 (l. 1233-1234) | «Un fallo por causa superior lleva además la cadena hasta la causa raíz» | M4: cada `DependencyFailed` lleva una cadena representativa y la firma de su raíz; el conjunto completo es `RootCauses` (M6) |
| P15.7 (l. 1250) | «Orden determinista: posición, código y dueño» | M3: con los desempates por id sujeto en el orden de P11.1 y, para `InvalidArguments`, por token y número de argumentos |
| P16.3 (l. 1277-1278) | Traza con la «cadena de causas de un fallo» | A3 §7.3 |
| P21.6, fila `DependencyFailed` (l. 1676) | «La cadena de causas hasta la raíz (P15.1): el id de cada eslabón, y el código y los datos de la causa raíz» | M4 y M10(a): `Causa`, ids de la `Cadena` y firma de la `Raíz`, más `RootCauses(símbolo)` como dato estable adicional |
| P21.6, fila `Upstream` (l. 1694) y coincidencia (l. 1702-1703) | «su `SymbolId` y su causa raíz»; «la misma causa raíz» | M10(b): `SymbolId` + `RootCauses` completo por SCC; coincide solo con el mismo conjunto |
| §0.3, `ExpectedRepairReason` y razón `Upstream` (l. 105, 123-124) | `Upstream(rootCauseSignature)`; «la misma causa raíz estructurada» | M10(b) |
| §0.1, fila RR-1 (l. 65) | `Upstream(firma de causa raíz)` | M10(b) |
| P24.6, tabla, fila `Upstream` (l. 2162) | «Una variable leída falla: `Cycle`, `DependencyFailed`, `BrokenReference` o un error propio de esa variable» | La clasificación no cambia; sus datos estables son los de M10(b) |
| §5.2, decisión 7 (l. 2624-2625) | «se rompen con `ChangeDefinition` de un miembro» | A3 §6.2 |
| §7.2.1 (l. 2759-2761) | «conjunto de causas raíz (variables cuya propia definición falla, o un ciclo tomado como una unidad)» | `SourceRoots` sobre `RootCauses` por SCC, contado por **firmas** y no por variables (M6, M10(c)) |
| §7.2.2 (l. 2762-2766) | «Para una fuente con causa superior y causa raíz única X … todas las fuentes fallidas … tienen causa superior con causa raíz exactamente X» | M10(c): clasificación `Upstream` y `SourceRoots = {X}` para todas; ciclo simple si X es un `CycleRoot` |
| §7.2.3 (l. 2767-2773) | «Si X es un ciclo, `<X>` nombra a sus miembros: corregir cualquiera de ellos rompe el ciclo» | Solo si X es un ciclo simple (A3 §6.2, M10(c)) |
| §7.2.4 (l. 2774-2788) | Si no se cumple la condición, mensaje de bloqueo con «otras fuentes inválidas» | M10(d): razones `OtherInvalidSources`, `SeveralRoots` y `NonSimpleCycle`; el texto literal de §7.2.4 solo en el caso que fija M10(d) (fuente bloqueante en el mismo rack, una sola raíz y sin `NonSimpleCycle`); el aviso de reparación solo si el rack es `Repairable` |
| P28.3, fila G7 (l. 2290) | «… cortocircuito `DependencyFailed`; conjuntos de P12.7» | Además, las pruebas de G7 de A3 §11.1 |
| P28.3, fila G8 (l. 2291) | Resultados de `InspectBinding` para los dos kinds, entre otras | Además, T-A3-46 |
| P28.3, fila G9 (l. 2292) | Pruebas de G9 | Además, las pruebas de A3 §11.2 sin marca o marcadas G9 |
| P28.3, fila G10 (l. 2293) | Pruebas de G10 | Además, las pruebas de A3 §11.2 marcadas G10 |

### 10.2 V6, leídas con A3

| Lugar | Texto vigente | Lectura con A3 |
|---|---|---|
| §0.6 (l. 185-189) | `Failed` compara «el código y sus datos estructurados estables, como los ids y la causa»; `Failed(causa A) → Failed(causa B)` aborta | M10(a) |
| P2.2 (l. 714-716) | «Existencia de referencias, aridad, unidades, ámbito y forma canónica se comprueban sobre un `BoundExpression` contra un contexto … se re-comprueba en cada snapshot» | M1: comprobación estática antes de evaluar; `BrokenReference`, `InvalidArguments` y `NonCanonicalForm` propios |
| P2.8, forma no canónica (l. 750-754) | «Una forma no canónica leída de persistencia es error SEMÁNTICO `NonCanonicalForm` … No se evalúa» | M1 bloqueo 4, M2.E, M5 |
| P7.5 (l. 914-917) | Comprobación semántica: existencia, aridad (`InvalidArguments`), unidad, ámbito, autorreferencia, forma canónica y límites; sobre un árbol persistido corre sin resolver nombres | M1 y M2.D |
| P8.10, escenario del caso C (l. 987-995) | `A` rota, `B` y `AlturaFinal` en `DependencyFailed` | Sin cambio (A3 §8) |
| P10.3 y P10.4 (l. 1096-1106) | Aridades de `MIN`, `MAX` y `ABS`; «función conocida con aridad errónea → error semántico `InvalidArguments`» | M2.D y A3 §7.2 |
| P10.5 (l. 1107) | Mayúsculas canónicas en el árbol | Tokens del dato estable de `InvalidArguments` |
| P12.7 (l. 1163-1169) | `Dependencies(s)` y `Dependents(s)`, directos y transitivos; conjuntos afectado y leído | A3 §6.5 y A3 §6.6: G7 añade `RootCauses` y el predicado de ciclo simple; los conjuntos afectado y leído siguen en G9 |
| P13.2 (l. 1178-1179) | Cada miembro recibe `Cycle` con la lista completa | Sin cambio (A3 §6.1) |
| P13.5 (l. 1184-1191) | Miembros `Failed(Cycle)` con la ruta completa; dependientes en `DependencyFailed` | Se cumple sin cambio (A3 §6.1); la ruptura (l. 1190), en A3 §10.1 |
| P14.1 (l. 1200-1201) | Orden topológico con el desempate de P11.1 | A3 §6.3 (DI-1) |
| P14.4 (l. 1206-1208) | Semántica numérica | Sin cambio: `DivisionByZero` y `NonFiniteResult` solo si el evaluador corre (M1, N-5) |
| P15.3 (l. 1242) | `Cycle` y `DependencyFailed` entre los códigos semánticos | Sin cambio: ningún código nuevo |
| P17.7 (l. 1356-1359) | Los errores semánticos dan un resultado por símbolo o por fuente | Sin cambio |
| P21.2, paso 4 (l. 1530) | `I = {X} ∪ dependientes transitivos de X` | Sin cambio (A3 §6.5) |
| P21.3 (l. 1556) | «también con dos errores dentro de un ciclo» | Si los dos errores recaen en un mismo miembro, coexisten como diagnósticos (M2); la salida por R1, R2 y la reparación no cambia |
| P21.6, sin dependencias transitivas por separado (l. 1641-1645) | «El resultado de un símbolo observado ya incorpora el de sus dependencias» | Con M10(a), ese resultado incorpora también `RootCauses` |
| P21.6, resultado comparable (l. 1667-1670) | «`Failed` compara, en el orden determinista de P15.7, el código de cada diagnóstico del símbolo y sus datos estructurados estables» | M3 y M10(a) |
| P21.6, filas `BrokenReference`, `Cycle` e `InvalidArguments` (l. 1674-1677) | Ids ausentes y miembros en el orden de P11.1; token y número de argumentos | M3 ordena los diagnósticos de un símbolo; los datos de `InvalidArguments` existen desde G7 (A3 §7.2) |
| P21.6, ampliación de datos estables (l. 1682-1683) | «Si la implementación necesita más datos para distinguir dos causas, los añade como datos estables» | Base de `RootCauses` como dato adicional en M10(a) |
| P21.6, tabla «Un fallo no aborta por sí mismo» y su regla (l. 1796, 1799-1801) | `Failed(causa A)` → `Failed(causa B)` → `ABORT BEFORE WRITE` | M10(a): raíces {A, B} frente a {A} es un cambio de causa |
| P21.6, qué no cubre (l. 1807-1810) | El authored de los racks no entra en el `PlanReadSet`; la `RepairDecisionObservation` re-inspecciona la entrada que transporta el plan | M10(b), alcance de la protección (N-3) |
| P21.6, escenario de `RepairBrokenRack` (l. 1844-1853) | `Upstream(Holgura: BrokenReference #<id-roto>)`; «Holgura falla por otra causa raíz → ABORT» | `Upstream(Holgura: {(Holgura, BrokenReference, [id-roto])})`; mismo resultado (A3 §8) |
| P21.6, casos fijados por prueba (l. 1872, 1875-1877) | «fallo con otra causa»; «fallo superior idéntico, recuperado o con otra causa» | M10(a) y M10(b) |
| P22.9 (l. 1951) | «las variables en error, con su causa raíz, los miembros y la ruta de cada ciclo y sus dependientes» | Con su `RootCauses` |
| P22.9 (l. 1954) | Mensaje de recuperación condicional para cada fuente con causa superior | M10(c) |
| P24.6, precedencia (l. 2168-2169) | Estructural, id ausente, tipo incompatible y fallo semántico, para una fuente de rack | Sin cambio: M2 trata de símbolos. Una fuente con aridad errónea o forma no canónica propia es `Intrinsic` aunque lea una variable que falla, salvo que esta precedencia la clasifique antes (A3 §5.10(c), A3 §7.2) |
| P27.1 (l. 2255) | «diagnósticos (P15), con la cadena hasta la causa raíz» | A3 §7.3 |
| §5.1, filas `Cycle`, `InvalidArguments`, `DependencyFailed` y `NonCanonicalForm` (l. 2592-2596) | Clasificación semántica | Sin cambio |
| §5.2, decisión 5 (l. 2619-2621) | El panel muestra la causa raíz | Con `RootCauses` |
| §7.2, commit de la reparación (l. 2734-2738) | «ya no es reparable por la misma razón —se recuperó, cambió su causa…—» | M10(b): «cambió su causa» incluye cualquier cambio de `RootCauses` |
| §7.2, tabla de confirmación (l. 2749, 2751) | `FailureCause`; `UpstreamCause`: «la variable superior y su código» | Cada variable superior que falla, en el orden de P11.1, con su `RootCauses`, en la capa de texto (G10) |
| §7.2.5 (l. 2789-2791) | Consecuencia de dos causas raíz independientes | Sin cambio |
| §7.2.6 (l. 2792-2797) | «ninguna otra fuente inválida conocida» | Diagnósticos conocidos del snapshot; fallos numéricos latentes (A3 §5.1, N-5) |
| §7.2.7 (l. 2798-2799) | «Mismo texto en el rechazo … usa la misma clasificación: lista las fuentes que bloquean» | La misma clasificación incluye las razones de M10(d), del estado diagnosticado antes de la corrección |
| P28.5 a P28.8: T-V3-09 (l. 2333), T-V3-13 (l. 2337), T-V4-01 (l. 2355), T-V4-05 y T-V4-06 (l. 2359-2360), T-V4-11 (l. 2365), T-V5-01 y T-V5-02 (l. 2372-2373), T-V5-09 (l. 2380), T-V6-01 a T-V6-03 (l. 2388-2390) | Escenarios vigentes | A3 solo afirma igualdad con V6 en los escenarios enumerados en A3 §8. T-V5-09 no lleva observaciones y A3 no lo toca. En T-V3-09, si los dos errores recaen en un mismo miembro, coexisten como diagnósticos (M2), y A3 no cambia las reglas R1–R3 que dan la salida. T-V3-13 se lee con M10(c) y M10(d) |
| §9, lo que el ADR tiene que fijar de V5 y V6 (l. 2916-2924) | Comparación por resultado y por razón estructurada | M10 |
| §10, costes (l. 3013), y §11.2, R11 (l. 3069) | Dos causas raíz independientes; «diagnósticos con causa raíz» | Sin cambio; con `RootCauses` |
| §14, estado (l. 3233) | «`Intrinsic`, `Upstream` y `Domain` comparadas sin texto» | `Upstream` con M10(b) |

### 10.3 ADR-0041, en el ADR de reemplazo

**Cambian directamente:**

| Lugar | Texto vigente | Lectura con A3 |
|---|---|---|
| D20, fila `Upstream` (l. 1289) | «su `SymbolId` y su causa raíz —el símbolo donde empieza el fallo, su código y sus datos estables—» | `SymbolId` + `RootCauses` completo por SCC (M10(b)) |
| D20, coincidencia (l. 1298-1299) | «la misma variable sigue fallando por la misma causa raíz → coincide; se recupera o cambia la causa raíz → no coincide» | El mismo `RootCauses` completo; si cualquier miembro aparece, desaparece o cambia de firma → no coincide |
| D18, puntos 1-2 (l. 1059-1066) | «conjunto de causas raíz (variables cuya propia definición falla, o un ciclo tomado como una unidad)»; «causa raíz única X»; «causa raíz exactamente X» | `SourceRoots` por SCC contado por **firmas**, no por variables; clasificación `Upstream` e igualdad `SourceRoots = {X}` para todas las fuentes (M10(c)) |
| D18, punto 3 (l. 1067-1073) | «Si X es un ciclo, `<X>` nombra a sus miembros: corregir cualquiera de ellos rompe el ciclo» | Solo si X es un ciclo simple; en una SCC no simple nunca hay sugerencia (A3 §6.2, M10(c)) |
| D18, punto 4 (l. 1074-1088) | «Si no se cumple, porque hay cualquier fuente con fallo propio, dominio inválido, id ausente u otra causa superior independiente …, el mensaje es: …» | Razones estructuradas de M10(d), con la definición de fuente bloqueante; el texto literal solo en el caso que fija M10(d); el aviso de reparación solo con el rack `Repairable`; la redacción nueva —`SeveralRoots`, `NonSimpleCycle`, bloqueos solo en otros racks y rack `Blocked`— la fija el ADR de reemplazo con las restricciones de veracidad de M10(d) |
| D14, decisión 7 (l. 942-943) | «se rompen con `ChangeDefinition` de un miembro» | La recuperación garantizada con un solo miembro vale solo para un ciclo simple (A3 §6.2) |

**Se leen con A3:**

| Lugar | Texto vigente | Lectura con A3 |
|---|---|---|
| D4, comprobación semántica (l. 319-320) | «existencia, aridad, unidades, ámbito, forma canónica y límites … un árbol leído de persistencia se re-comprueba en cada snapshot» | M1 y M2: bloqueos estáticos antes de evaluar |
| D8, diagnósticos del parser (l. 718-719) | «El orden determinista de P15.7 no cambia» | Sin cambio para los diagnósticos con posición; M3 solo desempata diagnósticos de evaluación del mismo dueño y código |
| D11, forma no canónica (l. 825-828) | `NonCanonicalForm`: «no se evalúa» | M1 bloqueo 4, M2.E |
| D14, semánticos y decisiones 1 y 6 (l. 917-923, 940-941) | Lista de fallos semánticos; alcance del bloqueo; «también con dos errores dentro de un ciclo» | Sin cambio; los dos errores coexisten como diagnósticos del miembro (M2) |
| D15, aristas rotas y conjuntos (l. 966-968) | «expone dependencias y dependientes directos y transitivos, el conjunto afectado y el conjunto leído» | A3 §6.5 y A3 §6.6 |
| D15, ciclos (l. 969-973) | «cada ciclo informado con sus miembros en orden»; miembros y dependientes transitivos no evaluables | A3 §6.1, A3 §6.2 y A3 §6.4 |
| D15, orden y cortocircuito (l. 974-977) | «topológico sobre la parte acíclica, con el desempate de P11.1»; «reciben `DependencyFailed` con el id de la causa, sin evaluarse» | A3 §6.3; M1 y M2 |
| D18, precedencia de `InspectBinding` (l. 1031-1036) | Estructural, id ausente, tipo incompatible y fallo semántico | Sin cambio |
| D18, tabla de confirmación (l. 1051, 1053) | `FailureCause`; `UpstreamCause` | Cada variable superior que falla, con su `RootCauses`; M10(c) y M10(d) |
| D18, puntos 5 y 6 (l. 1089-1097) | Consecuencia de dos causas raíz independientes; «ninguna otra fuente inválida conocida» | Sin cambio; diagnósticos conocidos del snapshot (A3 §5.1, N-5) |
| D18, punto 7 (l. 1098-1099) | «Mismo texto en el rechazo … usa la misma clasificación» | La misma clasificación incluye las razones de M10(d), del estado diagnosticado antes de la corrección |
| D19, regla de derivación (l. 1158-1163) | «Todo dato o clasificación semántica que justifica una decisión destructiva queda representado por una observación comparable hasta el commit» | Sin cambio; A3 la aplica a la razón de la fuente retirada (CX-2), sin extenderla a otras fuentes (N-3) |
| D19, sin dependencias transitivas por separado (l. 1176-1180) | «El resultado de un símbolo observado ya incorpora el de sus dependencias» | Con M10(a) |
| D19, resultado comparable (l. 1194-1207) | Orden de P15.7; filas `DependencyFailed` (l. 1202) e `InvalidArguments` (l. 1203); ampliación de datos (l. 1206-1207) | M3, M4 y M10(a); los datos de `InvalidArguments` existen desde G7 |
| D19, «Un fallo no aborta por sí mismo» (l. 1248-1259) | `Failed(causa A)` → `Failed(causa B)` → `ABORT BEFORE WRITE` (l. 1256) | M10(a) |
| D19, escenarios normativos (l. 1270-1273) | «recuperado o con otra causa → abort» | M10(a) y M10(b) |
| D20, pruebas (l. 1316-1318) | T-V6-01 a T-V6-09 | T-V6-01 a T-V6-03 y T-V6-09, igual que en A3 §8 (T-V6-09 es el escenario de `RepairBrokenRack` de P21.6); T-V6-04 a T-V6-08 prueban `MissingTarget`, `Domain` e `Intrinsic`, que A3 no cambia; más las pruebas de G9 de A3 §11.2 |
| D22, panel (l. 1400-1402) | «las variables en error con su causa raíz, los miembros y la ruta de cada ciclo» | Con `RootCauses` |
| D23, cierre (l. 1424) | `I = {X} ∪ dependientes transitivos de X` | Sin cambio (A3 §6.5) |
| D24, ID28 e ID29 (l. 1456-1461) | «los diagnósticos con su cadena hasta la causa raíz» | A3 §7.3 |
| Consecuencias (l. 1610) | «un rack con dos causas raíz independientes solo sale por la reparación» | Sin cambio |

### 10.4 No cambian

- La identidad de `projectVariable`, el cualificador `Q(clave)` y su gramática (A2).
- La guarda de recurso del parser y el orden de P15.7 para los diagnósticos con posición (A1, l. 301-302; D8,
  l. 718-719).
- La extracción de dependencias y su función única (P11), la dirección del grafo (P12.2), las aristas rotas sin nodo
  (P12.4), la detección de SCC (P13.1) y el `Cycle` del preflight con plan vacío (P13.3).
- El catálogo cerrado de códigos 1–29: sin códigos nuevos, sin renumerar y sin cambiar de clase.
- El contrato y la semántica numérica del evaluador de G6 (P14.4), las funciones y sus aridades (P10), y las unidades
  (P9).
- La comparación de `Success`, las razones `MissingTarget`, `Intrinsic` y `Domain`, y la precedencia de D18.
- Las reglas R1, R2 y R3 de P21.3 y el contrato raíz de P8.10.
- La estructura del `PlanReadSet`: tipos de observación, fases, invariantes, orden de las observaciones y secuencia de
  commit; la revisión acotada de V8-R05.
- Schema V-0 y la persistencia: A3 no persiste nada nuevo.
- La UI y el BOM: A3 solo fija qué datos consume la capa de texto de §7.2.
- ADR-0041 D1–D3, D5–D7, D9, D10, D12, D13, D16, D17, D21 y D25; D8 y D23 figuran en A3 §10.3 solo como lecturas
  sin cambio.

---

## 11. Contrato de pruebas futuro

### 11.1 G7 (obligatorias)

| # | Prueba | Qué fija |
|---|---|---|
| T-A3-01 | U1, raíces independientes | Los diagnósticos exactos de A3 §4.1 y `RootCauses(D)` |
| T-A3-02 | U2, dos raíces detrás de una dependencia | Un `DF{B; [B, A1]; …}` y `RootCauses(D) = {A1, A2}` |
| T-A3-03 | U3, `BrokenReference` propia y fallo superior | Los dos diagnósticos y el orden de `RootCauses(D)`; D no se evalúa: `D = (A + #<m>) + 1 / 0` no da `DivisionByZero` |
| T-A3-04 | U4, ciclo y fallo externo | Los diagnósticos locales de A, B y F, y `RootCauses(A) = RootCauses(B) = RootCauses(F) = {CycleRoot[A, B], (E, DivisionByZero)}` |
| T-A3-05 | Desempate de `DependencyFailed` del mismo código | Dependencias escritas en orden inverso: los `DependencyFailed` salen en el orden de P11.1 de sus causas |
| T-A3-06 | Desempate de `BrokenReference` del mismo código | Ids ausentes escritos en orden inverso: los `BrokenReference` de `RegistryEvaluation` salen en el orden de P11.1 (S1 = A) |
| T-A3-07 | Orden completo de un símbolo no evaluado | `BrokenReference` (22) antes que `Cycle` (23) antes que `DependencyFailed` (24) antes que `InvalidArguments` (25), con el ejemplo de M3 |
| T-A3-08 | `DependencyFailed` de un miembro solo fuera de su SCC | Ciclo de dos, ciclo largo y autorreferencia: ningún `DependencyFailed` por aristas internas |
| T-A3-09 | Deduplicación de raíces en un diamante | Una raíz alcanzada por dos caminos aparece una vez en `RootCauses` |
| T-A3-10 | Deduplicación de raíces al entrar dos veces en el mismo ciclo | `RootCauses(F)` con un solo `CycleRoot` |
| T-A3-11 | Preservación del caso lineal sin ciclo | La cadena de P8.10 caso C y la forma de T-V5-01: diagnósticos con los datos de V6 y `RootCauses` de un elemento |
| T-A3-12 | Determinismo por permutación | Toda permutación de las entradas da diagnósticos, cadenas, `RootCauses`, ciclos, predicado de ciclo simple y orden idénticos |
| T-A3-13 | Determinismo por cultura | Resultados idénticos bajo culturas distintas |
| T-A3-14 | Sin enumeración exponencial de caminos ni recursión profunda | k diamantes apilados (2^k caminos): el número de `DependencyFailed` es el de aristas hacia dependencias que fallan, `RootCauses` tiene un elemento y la derivación visita cada SCC a lo sumo una vez (conteo de visitas, no tiempos); una cadena lineal de miles de símbolos no desborda la pila |
| T-A3-15 | Lista de ciclos y conjuntos transitivos | Ciclos ascendentes por su miembro mínimo (DI-2); un miembro está en sus propios conjuntos transitivos y un id ausente no es nodo (DI-3) |
| T-A3-16 | Orden topológico | Menor `SymbolId` listo en P11.1, miembros fuera del orden y dependientes de un ciclo presentes y fallidos (DI-1) |
| T-A3-22 | MAT1-A: U5 | Cada miembro de la SCC {A, B, C, D} tiene `RootCauses = {CycleRoot[A, B, C, D], (E, DivisionByZero), (F, DivisionByZero)}` |
| T-A3-23 | MAT1-B: diagnósticos locales | En U5, B y D tienen solo `Cycle`, sin `DF{E}` ni `DF{F}`, aunque su `RootCauses` sea por SCC |
| T-A3-24 | MAT3-A: aridad errónea y dependencia que falla | U6(a): los dos diagnósticos coexisten, S no se evalúa y `RootCauses(S)` tiene las dos raíces |
| T-A3-25 | MAT3-B: aridad errónea y `BrokenReference` propia | U6(b): los dos diagnósticos coexisten y hay dos raíces del mismo dueño |
| T-A3-26 | MAT3-C: varias llamadas con aridad errónea | U6(c): un diagnóstico por par distinto, en el orden de token `Ordinal` y número, y una sola raíz agregada |
| T-A3-27 | Aridad estática antes de la evaluación numérica | U6(d): `InvalidArguments`, sin `DivisionByZero` |
| T-A3-28 | BR-AGG: raíz de referencias rotas agregada | `H = #<a> + #<b>`: dos `BrokenReference` y una sola raíz `(H, BrokenReference, [a, b])` |
| T-A3-29 | ROOT-ORDER: orden de `RootCauses` por código | `A = 1 / 0 ; B = #<b> + 1 ; D = A + B`: `(B, BrokenReference, [b])` antes que `(A, DivisionByZero)` aunque A < B |
| T-A3-30 | CHAIN-CYCLE-BR: cadena que entra en un miembro con `BrokenReference` propia | `A = B + #<m> ; B = A + E ; E = 1 / 0 ; G = A + 1`: `G = [DF{A; [A]; (A, BrokenReference, [m])}]` y `RootCauses(G) = {(A, BrokenReference, [m]), CycleRoot[A, B], (E, DivisionByZero)}`, con la raíz de E entrando por B |
| T-A3-31 | SIMPLE-CYCLE | Autorreferencia, ciclo de dos y el ciclo de U5: ciclo simple |
| T-A3-32 | NON-SIMPLE-CYCLE | U7 (seis aristas, cuatro miembros), un ciclo con una cuerda y `A = A + B ; B = A` (autorreferencia dentro de una SCC de dos miembros: tres aristas): no simple |
| T-A3-33 | U9, parte de G7: cambio del miembro de entrada | `RootCauses(H)` igual antes y después; diagnósticos distintos |
| T-A3-34 | `NonCanonicalForm` con una tabla sintética | Una definición `Expression` que solo es un número sin unidad: `[NonCanonicalForm]`, no se evalúa, raíz `(dueño, NonCanonicalForm)`, y sus dependientes dan `DependencyFailed` |
| T-A3-41 | Invariante de la raíz representativa | En todos los casos U1–U9, la `Raíz` de cada `DependencyFailed` pertenece a `RootCauses` de su dueño |
| T-A3-42 | Cadena a través de un eslabón con `DependencyFailed` e `InvalidArguments` | U6(a) con `T = S + 1`: `T = [DF{S; [S, H]; (H, BrokenReference, [h])}]` y `RootCauses(T) = RootCauses(S)` |
| T-A3-43 | Dos `CycleRoot` en el mismo conjunto | `A = B ; B = A ; C = D + A ; D = C`: `RootCauses(C) = {CycleRoot[A, B], CycleRoot[C, D]}`, ordenados por su miembro mínimo |

### 11.2 G9, G10 y G8 (obligatorias)

Las filas sin marca son de G9; las marcadas indican sus gates.

| # | Carrera o escenario | Resultado |
|---|---|---|
| T-A3-17 | Forma exacta de U2: antes, raíces {A1, A2}; después, `a2` reaparece y quedan {A1}, con los diagnósticos de D idénticos | `SymbolResultObservation`: no coincide por `RootCauses`, `ABORT BEFORE WRITE`. `Upstream`: no coincide, `ABORT BEFORE WRITE` |
| T-A3-18 | Antes, raíces {A}; después, raíces {A, B} | `ABORT BEFORE WRITE`, en `SymbolResultObservation` y en `Upstream` |
| T-A3-19 | Mismas raíces; solo cambia la cadena representativa porque cambia un camino del grafo (CX-3) | `SymbolResultObservation`: `ABORT BEFORE WRITE`, porque P21.6 compara los ids de la cadena. `Upstream`: coincide, porque solo compara `RootCauses`, y el commit sigue si nada más difiere |
| T-A3-20 | CX-2: `H = A1 + A2` con las dos rotas, fuente retirada `palletTolerance = H + 2`; reaparece `a2` antes del commit | `ABORT BEFORE WRITE`; la fórmula no se borra |
| T-A3-21 | §7.2 con raíces detrás de una sola variable (G9 y G10) | Fuente `Upstream` con `SourceRoots` de un elemento y el resto de condiciones cumplidas → sugerencia; `SourceRoots = {A1, A2}` a través de una sola variable, sin otras fuentes fallidas → nunca sugerencia, razón `SeveralRoots` con las dos raíces y sin razón `OtherInvalidSources` |
| T-A3-35 | MAT-1: U5, E se recupera entre preflight y commit | `RootCauses(B)` cambia; `Upstream` no coincide; `ABORT BEFORE WRITE` |
| T-A3-36 | MAT-2: U8, fuente hermana `MissingTarget` (G9 y G10) | Sin sugerencia de recuperación; razón `OtherInvalidSources` con `palletTolerance` |
| T-A3-37 | MAT-4: U7, SCC no simple con solo `CycleRoot` (G9 y G10) | Sin sugerencia de recuperación con un solo cambio; razón `NonSimpleCycle` con los miembros, sin razón `OtherInvalidSources` |
| T-A3-38 | MAT-5: U9, cambio del miembro de entrada | `Upstream`: coincide. `SymbolResultObservation` de H: no coincide |
| T-A3-39 | Fallo numérico latente (G9 y G10): `A = #<a> + 1 ; D = A + 1 / 0`, fuente `palletTolerance = D + 2`, sin otras fuentes fallidas | `RegistryEvaluation` no informa ningún `DivisionByZero` de D; con los diagnósticos conocidos se cumple la condición y **se muestra** la sugerencia «Corregir A…» (§7.2.6); tras corregir A, el nuevo snapshot muestra el `DivisionByZero` y R1 bloquea la corrección |
| T-A3-40 | Ciclo simple con sugerencia válida (G9 y G10): `A = B ; B = A`, fuente `palletTolerance = A + 1`, sin otras fuentes fallidas | Sugerencia de recuperación; corregir A o B deja la fuente recuperada |
| T-A3-44 | Condición 3 exige igualdad (G9 y G10): `H = #<h> + 1 ; G = 1 / 0`; rack K con `f1 = H + 1` y `f2 = H + G`, las dos `Upstream` | f1 no recibe sugerencia aunque `SourceRoots(f2)` contenga la raíz de H; razón `OtherInvalidSources` con f2, que tiene la raíz de G fuera de `SourceRoots(f1)`. f2 no recibe sugerencia; solo la razón `SeveralRoots`: f1 no es bloqueante, porque sus raíces están en `SourceRoots(f2)` |
| T-A3-45 | Fuentes hermanas `Domain`, estructural y de otro rack (G9 y G10) | Con `f1 = H + 1` `Upstream`: una hermana `Domain` en K, una fuente estructural en K, o `g = H + #<gone>` (`RepairableMissingTarget`) en un rack K2 que consume el cierre de H → en cada caso, sin sugerencia para f1 y razón `OtherInvalidSources`. Con la hermana en K se usa la primera frase de §7.2.4; con g solo en K2, el texto no dice «este rack». Con la hermana `Domain` o con g, el rack K es reparable y se muestra el aviso de reparación; con la fuente estructural, K está `Blocked`: el texto lo dice y no hay aviso de que la reparación eliminará la fórmula |
| T-A3-46 | Fallo propio estático de una fuente que lee una variable que falla (G8 y G9) | Con H rota: `palletTolerance = ABS(H, 2)` es `Intrinsic(InvalidArguments)`; una fuente `expression` que solo es `Reference(H)` es `Intrinsic(NonCanonicalForm)`; `ABS(#<gone>, 2)` es `RepairableMissingTarget`; ninguna es `Upstream` ni recibe «Corregir H» |
| T-A3-47 | Autorreferencia dentro de una SCC de dos miembros (G9 y G10): `A = A + B ; B = A`, fuente `palletTolerance = A + 1` | Sin sugerencia; razón `NonSimpleCycle` |
| T-A3-48 | Razones de bloqueo y texto (G10) | El texto de §7.2.4 solo con una fuente bloqueante en el rack de la fuente, una sola raíz y sin `NonSimpleCycle`; en los demás casos, el texto no afirma otras fuentes inválidas inexistentes, no dice «este rack» sin bloqueantes en él, y para `SeveralRoots` y `NonSimpleCycle` solo dice que la recuperación con una corrección no está garantizada; el aviso de reparación con la fórmula canónica solo con el rack `Repairable`, y con el rack `Blocked`, el bloqueo sin reparación |
| T-A3-49 | Fuente de otro rack con raíces contenidas (G9 y G10): `A1 = #<a1> + 1 ; A2 = #<a2> + 1`; rack K con `f = A1 + A2`; rack K3 con `s3 = A1 + 1`, sin más fuentes fallidas | f: sin sugerencia, solo la razón `SeveralRoots`; s3 no es bloqueante de f y no se usa el texto de §7.2.4. s3: sin sugerencia, razón `OtherInvalidSources` con f, en otro rack, sin decir «este rack» |
| T-A3-50 | Rechazo de R1 con la misma clasificación (G9 y G10) | Un `ChangeDefinition` que R1 bloquea en U7, en T-A3-44 y en T-A3-49 informa, para cada fuente fallida de los racks que bloquean, las razones del estado diagnosticado **antes** de la corrección, las mismas del panel (§7.2.7; P21.2 paso 8), y no las del estado resultante |

---

## 12. ADR

- ADR-0041 está **aceptado** y es inmutable.
- A3 cambia **materialmente** decisiones aceptadas:

  ```text
  D20:           de una causa raíz por cada variable leída que falla
                 al conjunto completo RootCauses, por SCC, por cada variable leída que falla
  D18 puntos 1-2: de causas raíz contadas por variables
                 a SourceRoots por SCC contado por firmas
  D18 punto 3:   de «corregir cualquiera de ellos rompe el ciclo»
                 a la recuperación garantizada con un solo miembro solo en un ciclo simple
  D14 decisión 7: de «se rompen con ChangeDefinition de un miembro»
                 a la misma restricción al ciclo simple
  D18 punto 4:   de un único mensaje de bloqueo
                 a razones estructuradas OtherInvalidSources, SeveralRoots y NonSimpleCycle
  ```

- Cambiar la decisión de un ADR aceptado exige un ADR nuevo que la reemplace (README de `docs/adr`; preámbulo de
  ADR-0041, l. 43-45). Por tanto: **REPLACEMENT ADR REQUIRED**.
- **Corrección de gobierno.** La primera revisión del Arquitecto concluyó `CONTRACT_ACTION = AMENDMENT REQUIRED` y
  `Architect = AMENDMENT REQUIRED`, sin ADR de reemplazo, con una condición explícita: si el Coordinador leía el singular
  de D20 («su causa raíz —el símbolo donde empieza el fallo—») como una decisión de usar exactamente una raíz, la parte
  de `Upstream` cambiaba D20 y la acción pasaba a `REPLACEMENT ADR REQUIRED`. El Coordinador aplicó esa lectura
  (`GOVERNANCE CORRECTION: REPLACEMENT ADR REQUIRED`), y la revisión exacta del Arquitecto la confirmó: D20 no admite
  datos adicionales, porque la cláusula de ampliación está en D19.
- **No se asigna número ahora.** Antes de publicarlo se censan de nuevo los números de `main` y de **todas** las ramas
  activas.
- ADR-0041 **no se edita**. Cuando el Owner acepte el ADR de reemplazo, ADR-0041 pasa a `reemplazado por ADR-NNNN`, con
  el mismo patrón que ADR-0038 → ADR-0040 → ADR-0041.
- **Alcance exigido al ADR de reemplazo.**
  - Es un sucesor **completo**, D1–D25.
  - D20 recoge M10(b), con `RootCauses` por SCC.
  - D18 punto 3 y D14 decisión 7 recogen la regla del ciclo simple (A3 §6.2).
  - D18 puntos 1-2 recogen la condición de recuperación de M10(c): `SourceRoots` por SCC contado por firmas, y, ya
    explícitos, la clasificación `Upstream` y la igualdad para todas las fuentes, el fallo propio estático de una fuente
    y los diagnósticos conocidos.
  - D18 punto 4 recoge las razones de bloqueo de M10(d), con la definición de fuente bloqueante, y fija la redacción en
    español de los casos nuevos con las restricciones de veracidad de M10(d).
  - D4, D8, D11, D14 decisiones 1 y 6, D15, D18 (precedencia, tabla y puntos 5-7), D19, D20 (pruebas), D22, D23, D24 y
    Consecuencias recogen las lecturas de A3 §10.3, incluidos los bloqueos estáticos de M1 y los datos de
    `InvalidArguments`.
  - Queda registrado para el ADR de reemplazo el límite heredado de V6 sobre fuentes estructurales fuera del cierre
    (A3 §5.10(c)).
  - Las demás decisiones conservan su semántica.
  - **Precedencia.** Remite a V6 leída con A1, A2 y A3 en sus blobs exactos.
- **Quién decide.** Solo el Owner acepta o rechaza el ADR de reemplazo; un agente puede redactarlo en estado
  `propuesto`. Hoy **no existe** ninguna decisión del Owner sobre A3 ni sobre ese ADR.

Autoridad futura, tras la revisión exacta de A3, la aceptación del Owner y el nuevo freeze:

```text
V6 + A1 + A2 + A3 + ADR de reemplazo + un nuevo Consensus Freeze
```

---

## 13. Freeze

- `docs/initiatives/I-49-consensus-freeze-v6-a1-a2.md` sigue **byte a byte igual**, como evidencia histórica, igual que
  `docs/initiatives/I-49-consensus-freeze-v6-a1.md` y `docs/initiatives/I-49-consensus-freeze.md`.
- A3 activa su regla de invalidación (§6 de ese registro) **para seguir implementando G7**, porque cambia la semántica de
  Expression, el `PlanReadSet` y la `RepairDecisionObservation`, y cambia decisiones de ADR-0041.

  ```text
  STOP IMPLEMENTATION     → G7 BLOCKED BEFORE RED, sin código ni pruebas
  → document finding      → STOP de G7 y este amendment
  → Coordinator review    → AGREED WITH REVISED TECHNICAL MODEL; PENDING ARCHITECT EXACT RE-REVIEW
  → Architect review      → CHANGES REQUIRED sobre el blob 97996b7; PENDING EXACT REVIEW del blob revisado
  → Owner decision        → PENDING (el ADR cambia)
  ```

- Un nuevo freeze declarará como autoridad conjunta V6, A1, A2 y A3 en sus blobs exactos y el ADR de reemplazo
  aceptado. Solo entonces G7 se reanuda desde RED, con A3 §11.1.
- G6 sigue CLOSED. G7 sigue BLOCKED. G8 sigue BLOCKED.

---

## 14. Evidencia

| Punto | Resultado |
|---|---|
| Línea base de G6 en `75f1862` | G6-B 298/298; A2 focal 294/294; G6-A 38/38; evaluador, binder, formatter y P2.5 146/146; build sin errores |
| Barrido independiente | Solo lectura y sin conocer el STOP: V6, A1, A2, ADR-0041 y el freeze vigente no definen los puntos 1-6 de A3 §3.2. ADR-0041 contiene la ampliación de datos estables (l. 1206-1207), la regla de derivación (l. 1158-1163) y la condición de causa raíz única (l. 1062-1066). La historia V1–V5, el Discovery, `decisions/I-49.md`, los freezes anteriores, ADR-0038 y ADR-0040 no tienen ninguna regla con varias causas |
| Evaluador de G6 | Un `BrokenReference` por id ausente en orden de primera aparición (bucle en `ExpressionEvaluator.cs:148-176` sobre `ReferencesOf`, `:181-221`), fijado por `ExpressionEvaluatorTests.UNA_REFERENCIA_A_UN_ID_AUSENTE_DA_BROKENREFERENCE_SIN_EVALUAR_NADA` (`[Id(99), Id(98)]`); exige un valor para cada símbolo presente (`:156-164`); comprueba la aridad al llegar a cada llamada (`:280-283`); el primer fallo termina la evaluación; `InvalidArguments`, `DivisionByZero` y `NonFiniteResult` sin datos (`:329-330`) |
| Catálogo | `ExpressionDiagnosticCode.cs:93-111`: `BrokenReference` 22 … `NonCanonicalForm` 28; `Cycle` y `DependencyFailed` declarados y aún no producidos |
| Guarda G1 | `DependencyGraph` sigue prohibido en Application (`tests/RackCad.Tests/ProjectVariablesConformanceTests.cs:184-203`); evoluciona en G7 según P28.4, sin cambio por A3 |
| Texto anterior (blob `97996b7`) | Dos pasadas de auditoría independiente de conformidad antes de su commit; CI 34875190797 success 4/4 sobre `f6f0991` |
| Revisión exacta del Arquitecto del texto anterior | `CHANGES REQUIRED`: MAT-1 a MAT-5 (A3 §3.5) y notas N-3 a N-5, con contraejemplos verificados contra V6; confirmados U1–U3, M4, M5, M7, M9, la forma de M10, el orden de raíces, CX-3, DI-1 a DI-4, S1 y el ADR de reemplazo |
| Auditorías independientes de A3-R1 | Solo lectura, antes del commit. Conformidad con la orden A3-R1: sin hallazgos materiales y trece menores de citas, censo y redacción. Revisión adversarial de solidez: un hallazgo material —el texto de bloqueo de §7.2.4 afirmaba «otras fuentes inválidas» cuando no las hay (U7) y dejaba `<X>` sin referente con varias raíces—, resuelto con las razones de M10(d), y ocho notas: autorreferencias dentro de una SCC, igualdad en la condición 3, fallo propio estático de una fuente, «nunca falsa» respecto de lo conocido, el predicado de ciclo simple fuera de `Upstream`, derivación iterativa y listas compartidas, conteo por firmas como cambio de D18 y pruebas de invariantes. En la tercera pasada, la conformidad no halló materiales y la revisión adversarial halló uno: el aviso de reparación exigido «en todos los casos» era falso con un rack `Blocked`, resuelto limitándolo a racks reparables, junto con la precisión de que la lista de bloqueantes solo es exacta con una sola raíz, el estado que describe un rechazo de R1 y cinco menores. En la cuarta pasada, la revisión adversarial confirmó esas correcciones sin hallazgos materiales y con una nota —el fallo tipado del estado resultante se sigue informando—, incorporada. En la segunda pasada, la conformidad no halló materiales y la revisión adversarial halló dos: `OtherInvalidSources` listaba como bloqueantes fuentes cuyas raíces ya estaban en las de la fuente, y el texto de §7.2.4 decía «este rack» con bloqueos solo en otros racks. Se resolvieron con la definición de fuente bloqueante y las reglas de texto de M10(d), junto con §7.2.7, el límite heredado de fuentes estructurales y siete menores. Todo se incorporó antes del commit; el resultado de la última re-auditoría consta en el mensaje del commit |
| Archivos | A3-R1 cambia solo este documento; ninguna medición dejó cambios en el árbol |
| Autoridades | V6 `ef4db3aa400483ff25a8f39b2beb93708fa43d1a`, A1 `d62019088b9e7a140d5066799afe6ace6db303ba`, A2 `49a925336dd3775929a35b0f40b73cb7f8c487f7`, ADR-0041 `c6a3d2ba0ab9df93e51efcddf04ed9255b1a2231` y freeze vigente `57736f725663aab79a24b29685ed34e8c3e9ebad`, sin cambio |
| `main` | `dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093`, sin cambios en el núcleo de expresiones. La rama no se rebasa por un amendment documental |
| Ramas paralelas | Medidas antes del commit: I-52 `e59bc89335c776a24cd48b30f54547696f820dfa` (ADR-0036 en su rama); I-55 `110cd39aded983df6c409bbd32d5bca05ceed0ed` (ADR-0042 en su rama); I-56 `0d66df26882cb5cdd2efc21155c121f58d2af669`. Todas solo documentación; ninguna toca `src/RackCad.Application/Expressions`, las pruebas de expresiones, la guarda G1 ni los documentos de I-49 |

---

## 15. Estado

```text
Amendment = A3

Revision = A3-R1 after Architect exact-review findings MAT-1..MAT-5

Finding = MULTI-CAUSE DEPENDENCYFAILED / ROOTCAUSES CONTRACT GAP

Coordinator = AGREED WITH REVISED TECHNICAL MODEL
              PENDING ARCHITECT EXACT RE-REVIEW

Architect = CHANGES REQUIRED on prior blob
            PENDING EXACT REVIEW of revised blob

Replacement ADR = REQUIRED

Owner = PENDING FUTURE REPLACEMENT ADR

G6 = CLOSED

G7 = BLOCKED

G8 = BLOCKED
```
