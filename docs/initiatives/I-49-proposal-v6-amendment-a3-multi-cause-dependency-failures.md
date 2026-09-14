# I-49 — Proposal V6 · Amendment A3: fallos con varias causas y conjunto de causas raíz

Amendment documental de la [Proposal V6](I-49-proposal-v6.md), leída con el
[Amendment A1](I-49-proposal-v6-amendment-a1-text-guard.md) y el
[Amendment A2](I-49-proposal-v6-amendment-a2-exact-key-qualifier.md), para **un solo** hueco contractual: qué
diagnósticos recibe un símbolo cuando falla por **más de una causa a la vez**, qué es su conjunto de causas raíz y qué
datos estables consumen de él el `PlanReadSet`, la razón `Upstream` y el mensaje de recuperación de §7.2. Lo encontró el
STOP de G7 antes de RED y lo confirmó la revisión del Arquitecto (`MULTI_CAUSE_AMBIGUITY = CONFIRMED`).

```text
Amendment          = A3
Finding            = MULTI-CAUSE DEPENDENCYFAILED / ROOTCAUSES CONTRACT GAP

Enmienda a         = Proposal V6, blob ef4db3aa400483ff25a8f39b2beb93708fa43d1a (no se edita)
Junto a            = Amendment A1, blob d62019088b9e7a140d5066799afe6ace6db303ba (no se edita ni se enmienda)
                     Amendment A2, blob 49a925336dd3775929a35b0f40b73cb7f8c487f7 (no se edita ni se enmienda)
Decisión afectada  = ADR-0041 D20, dato estable de Upstream; lecturas de D14, D15, D18, D19, D22 y D24
                     ADR aceptado e inmutable, blob c6a3d2ba0ab9df93e51efcddf04ed9255b1a2231
Freeze afectado    = docs/initiatives/I-49-consensus-freeze-v6-a1-a2.md
                     blob 57736f725663aab79a24b29685ed34e8c3e9ebad (no se reescribe)
G6 final           = 75f18623e7ced836de7eb2e36d6db8e311efb76d
                     CI 34862975299 success 4/4; G6 CLOSED
G7                 = BLOCKED BEFORE RED, sin código ni pruebas
Alcance            = SOLO los diagnósticos de un símbolo con más de una causa de fallo, su orden, sus causas
                     raíz y los datos estables que de ellas consumen PlanReadSet, Upstream y §7.2
Sin cambio         = identidad y cualificador (A2), guarda del parser (A1), extracción de dependencias,
                     dirección del grafo, detección de SCC y catálogo de códigos
```

---

## 1. Qué es y qué no es

- **Es** un amendment del Coordinador (`Coordinator = AGREED WITH ARCHITECT M1–M10`) que recoge el modelo M1–M10 de la
  revisión del Arquitecto de G7 y queda sometido a su revisión exacta. Llena **un** hueco y nada más.
- **No** edita V6, A1, A2, ADR-0041, ninguno de los tres Consensus Freeze ni el Discovery: todos siguen en sus blobs
  exactos.
- **No** autoriza implementación: G7 sigue BLOCKED antes de RED y G8 sigue BLOCKED (A3 §13).
- **No** cambia la identidad ni el cualificador (A2), la guarda del parser (A1), la extracción de dependencias (P11), la
  dirección del grafo (P12.2), las aristas rotas (P12.4), la detección de ciclos por SCC (P13.1) ni el catálogo cerrado de
  códigos: no hay códigos nuevos y ninguno cambia de número o de clase.
- **Activa** la regla de invalidación del freeze vigente (§6 de ese registro): cambia la semántica de Expression, el
  `PlanReadSet` y la `RepairDecisionObservation`, y cambia el dato estable de `Upstream` de ADR-0041 D20 (A3 §12).
- Como en V6, A1 y A2, los nombres de tipos, miembros y operaciones (`RootCauses`, `CycleRoot`) son **ilustrativos**:
  el contrato es el comportamiento.

---

## 2. Nomenclatura

- **Referencias.** `§n` sin prefijo, `Pn.m`, `T-V…` y `ALT-n` remiten a V6; `Dn`, a ADR-0041; `A1 §n` y `A2 §n`, a esos
  amendments; y **`A3 §n`** a este documento.
- **R1, R2 y R3.** En A3 designan **solo** las reglas de intents correctivos de P21.3 (OPEN B). Los riesgos R1–R16 de
  §11.2 son otra serie, y los contraejemplos R1 y R2 de A2 (A2 §2.3 y A2 §2.4) no tienen relación con este documento. A3 no
  define nada con esos nombres.
- **U1–U4**: los cuatro casos del hallazgo (A3 §4).
- **M1–M10**: las reglas del modelo decidido (A3 §5).
- **MC-A, MC-B y MC-C**: los modelos candidatos que la revisión del Arquitecto llamó R-A, R-B y R-C (A3 §9). Se
  renombran para que no se confundan con R1, R2 y R3.
- **CX-1, CX-2 y CX-3**: contraejemplos y carreras de A3 (A3 §3.4, A3 §5.10).
- **S1**: la decisión sobre el orden de los `BrokenReference` frente al evaluador de G6 (A3 §7.1).
- **DI-1 a DI-4**: las lecturas confirmadas del orden topológico, del orden de la lista de ciclos, de los conjuntos
  transitivos y del alcance de los conjuntos derivados de G7 (A3 §6.2–§6.5).
- **T-A3-01…**: pruebas futuras obligatorias (A3 §11).
- **Falla**: da `Failed` en la misma `RegistryEvaluation`.
- **Notación.** `DF{causa; cadena; raíz}` es un `DependencyFailed` con su causa directa, los ids de su cadena y la firma
  de su raíz. Las firmas de raíz son `(dueño, código, datos)` y `CycleRoot[miembros]` (A3 §5.5).
- **Supuesto de los ejemplos.** Los ids de los símbolos siguen en P11.1 el orden alfabético de sus nombres
  (A < A1 < A2 < B < C < D < E < F < H < W < X < Y < Z). `a`, `a1`, `a2`, `m`, `z` e `id-roto` son ids ausentes, y `K` es
  un rack.

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

Todos los ejemplos de V6 con una cadena de fallos son lineales, por ejemplo: P8.10 caso C (l. 987-995), T-V4-01 (l. 2355), T-V5-01 y T-V5-02
(l. 2372-2373), T-V6-01 a T-V6-03 (l. 2388-2390) y el escenario de `RepairBrokenRack` de P21.6 (l. 1844-1853).

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
- **G9, `RepairDecisionObservation`.** `Upstream` protege la premisa de una retirada destructiva (regla de derivación
  de D19, l. 1158-1163).
- **G9 y G10, §7.2.** La sugerencia de recuperación depende de que la causa raíz sea única (l. 2762-2766).
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

**CX-2 — premisa destructiva con una sola raíz representativa.**

```text
A1 = #<a1> + 1   → Failed
A2 = #<a2> + 1   → Failed
H  = A1 + A2     → Failed
K: palletTolerance = H + 2   → RepairableSemanticFailure(Upstream, H), única fuente fallida del rack K
```

El preflight ve las raíces {A1, A2}: no hay una causa raíz única, así que no muestra la sugerencia de recuperación, y el
usuario confirma la reparación. Antes del commit reaparece `a2`. Si `Upstream(H)` guardara una sola raíz representativa
(A1), la decisión coincidiría y el commit borraría la fórmula. Pero en el estado nuevo la raíz es única, y §7.2 habría
mostrado «Corregir A1 permitiría recuperar esta fuente sin eliminarla». El commit borraría una fórmula con una premisa
que cambió: lo prohíben la regla de derivación (D19, l. 1158-1163) y §7.2.6 (l. 2792-2797).

---

## 4. Casos U1–U4: decisión A3

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
  busca ni se informa.

### 4.4 U4 — ciclo y fallo externo

```text
A = B + E        → miembro de la SCC {A, B}
B = A            → miembro de la SCC {A, B}
E = 1 / 0        → [DivisionByZero]
F = A + B        → no miembro

A = [Cycle[A, B], DF{E; [E]; (E, DivisionByZero)}]
B = [Cycle[A, B]]
F = [DF{A; [A]; CycleRoot[A, B]}, DF{B; [B]; CycleRoot[A, B]}]

RootCauses(A) = { CycleRoot[A, B], (E, DivisionByZero) }
RootCauses(B) = { CycleRoot[A, B] }
RootCauses(F) = { CycleRoot[A, B], (E, DivisionByZero) }
```

- La SCC cíclica es **una** unidad de causa raíz.
- Un miembro recibe `DependencyFailed` por cada dependencia directa que falla **fuera** de su SCC, y **ninguno** por las
  aristas dentro de su SCC.
- F recibe un `DependencyFailed` por cada dependencia directa que falla, A y B.
- `RootCauses(F)` cuenta una sola vez la raíz del ciclo y contiene además E, porque A depende de E: romper solo el ciclo
  no recupera a F.

---

## 5. Decisión A3: modelo M1–M10

### 5.1 M1 — Elegibilidad de evaluación

`ExpressionEvaluator` evalúa la definición de un símbolo de la `RegistryEvaluation` **solo** si se cumplen las tres
condiciones:

1. el símbolo no es miembro de una SCC cíclica (P13.1);
2. su propia definición no referencia ningún id ausente (P12.4);
3. ninguna de sus dependencias directas falla.

Si falta cualquiera, `RegistryEvaluation` lo marca `Failed` **sin evaluación numérica** (P14.3, P13.4). Nunca se evalúa
para buscar fallos numéricos adicionales una vez establecido un fallo estructural del grafo o superior, y el evaluador de
G6 nunca recibe un símbolo presente sin valor: su contrato no cambia. Un literal sigue evaluándose a su propio valor
(P14.6).

`NonCanonicalForm` (A-07) lo produce el adaptador desde G8 sobre la forma persistida. A3 no cambia cuándo se produce;
solo fija su lugar en el orden (M3) y su papel de causa raíz propia (M5, M6).

### 5.2 M2 — Diagnósticos de un símbolo no evaluado

Recibe **todos** los aplicables:

- **A.** un `BrokenReference` por cada id ausente distinto de su propia expresión, con ese id en `RelatedSymbols`;
- **B.** un `Cycle` si pertenece a una SCC cíclica, con la lista completa de miembros en el orden de P11.1 (P13.2);
- **C.** un `DependencyFailed` por cada dependencia directa que falla y está **fuera** de su propia SCC (M4).

Ninguna precedencia retira otro diagnóstico aplicable.

Un símbolo evaluado:

- da `Success`; o
- da `Failed` con **un** diagnóstico intrínseco del contrato vigente del evaluador de G6 (`InvalidArguments`,
  `DivisionByZero` o `NonFiniteResult`): el primer fallo termina la evaluación.

Por M1, un símbolo nunca mezcla un diagnóstico intrínseco de evaluación con los de A, B o C. La precedencia de
`InspectBinding` sobre una fuente de rack (P24.6; D18) **no cambia**: M2 trata de símbolos.

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
   - `Cycle`: un símbolo tiene a lo sumo uno, y su lista de miembros ya va en el orden de P11.1;
   - `InvalidArguments`, `DivisionByZero`, `NonFiniteResult` y `NonCanonicalForm`: a lo sumo uno por símbolo.
3. **Nunca** por texto, mensaje, cultura, hash ni orden de las entradas.

Para un símbolo no evaluado con todo aplicable, el orden es `BrokenReference` (ids ascendentes) → `Cycle` →
`DependencyFailed` (causas ascendentes). Ejemplo:

```text
A = B + #<m> + E ; B = A ; E = 1 / 0
A = [BrokenReference(m), Cycle[A, B], DF{E; [E]; (E, DivisionByZero)}]
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

- La cadena es **una** cadena representativa. Cumple P15.1 y el dato «el id de cada eslabón» de P21.6. **No** es el
  conjunto de causas raíz (M6).
- Es finita y sin repeticiones. Cada causa pertenece a una SCC anterior en el orden de la condensación, y el primer
  diagnóstico de un miembro de ciclo nunca es `DependencyFailed`: `Cycle` (23), y en su caso `BrokenReference` (22), van
  antes.
- Si la cadena llega a un miembro de ciclo, la raíz es su `CycleRoot` cuando su primer diagnóstico es `Cycle`, o la firma
  agregada de su `BrokenReference` cuando tiene una referencia rota propia. Las dos raíces entran en `RootCauses` (M6).
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
| Intrínseca | `(dueño, código, datos estables de P21.6)` | `InvalidArguments` (token de la función y número de argumentos, A3 §7.2), `DivisionByZero` y `NonFiniteResult` (solo el código); `NonCanonicalForm` desde G8 (solo el código) |
| Referencia rota | `(dueño, BrokenReference, todos los ids ausentes del dueño en el orden de P11.1)` | **Agregada por dueño**: nunca una raíz distinta por cada id ausente |
| Ciclo | `CycleRoot[miembros de la SCC en el orden de P11.1]` | **Una** unidad: ningún miembro es un dueño privilegiado, y entrar por A o por B da la misma raíz |

- Un símbolo cuyo fallo es solo `DependencyFailed` no es raíz.
- Una firma nunca contiene texto localizado, salida del formatter, posiciones ni la traza opt-in.

### 5.6 M6 — `RootCauses`

Una operación **pura y derivada** del núcleo de grafo y evaluación de G7, sobre una `RegistryEvaluation`:

```text
RootCauses(s) = RaícesPropias(s)
                ∪ RootCauses(d)   para cada DependencyFailed(causa = d) de s
```

`RaícesPropias(s)` puede contener:

- la firma agregada de `BrokenReference`, si s tiene alguno;
- el `CycleRoot` de su SCC, si s es miembro de un ciclo;
- la firma intrínseca, si s se evaluó y falló;
- `NonCanonicalForm`, cuando exista en los escenarios persistidos de G8.

Propiedades:

- vacío si s da `Success`, y no vacío si s da `Failed`;
- **deduplicado** por firma;
- **determinista** y ordenado estructuralmente (A3 §5.7);
- nunca texto localizado, salida del formatter, posiciones ni la traza opt-in;
- **no se persiste** (P27.4), y `RegistryEvaluation` no lo precalcula para todos los símbolos: se deriva **bajo
  demanda**.

Es la **única** autoridad del conjunto de causas raíz. La consumen el contrato futuro de `SymbolResultObservation`, de
`Upstream` y de §7.2 (M10), y el panel de diagnóstico (P22.9). Nadie reimplementa el recorrido.

### 5.7 Orden de `RootCauses`

1. El código de la raíz, en el orden del catálogo: `BrokenReference` 22, `Cycle` 23, `InvalidArguments` 25,
   `DivisionByZero` 26, `NonFiniteResult` 27 y `NonCanonicalForm` 28.
2. El id relevante, en el orden de P11.1: el dueño, o el **miembro mínimo** para un `CycleRoot`.
3. Los datos estables: los ids ausentes, la lista completa de miembros, o el token y el número de argumentos.

Dentro de una `RegistryEvaluation`, los criterios 1 y 2 ya identifican cada raíz: un dueño tiene a lo sumo una raíz de
cada código y las SCC son disjuntas. El criterio 3 completa el orden total. Nunca se ordena por hash.

### 5.8 M7 — Varias causas raíz

| Situación | Diagnósticos del símbolo | `RootCauses` |
|---|---|---|
| Raíces detrás de dependencias directas distintas (U1) | Un `DependencyFailed` por dependencia directa que falla | Todas |
| Varias raíces detrás de una sola dependencia (U2) | Un `DependencyFailed`, con su cadena representativa | Todas: ninguna se descarta |
| Una raíz alcanzada por varios caminos (diamante) | Un `DependencyFailed` por dependencia directa que falla | La raíz, una vez |
| Un ciclo leído por varios miembros (U4) | Un `DependencyFailed` por miembro leído | El `CycleRoot`, una vez |

Nunca hay un diagnóstico por camino.

### 5.9 M8 — Coste · M9 — Determinismo

**M8.**

- Construcción del grafo, SCC iterativa y diagnósticos de todos los símbolos: `O(V + E)`, más el coste determinista de
  ordenar en P11.1.
- Orden topológico con el desempate de P11.1: `O(V log V + E)`.
- A lo sumo un `DependencyFailed` por arista del grafo y un `Cycle` por nodo. Los `BrokenReference` están acotados por
  los ids distintos de cada definición, limitados por P1.9.
- Cadenas con colas compartidas: `O(1)` por `DependencyFailed`.
- `RootCauses(s)`: bajo demanda, un recorrido con visitados sobre las causas `DependencyFailed`, `O(V + E)` por
  llamada, más ordenar su resultado.
- Ningún camino raíz-nodo se materializa y ningún conjunto de raíces se persiste.

**M9.** Sin paralelismo, cultura ni reloj (P14.5). Cada elección sale de P11.1, de P15.7 y del orden del catálogo. El
resultado no depende del orden de las entradas del registro, del hash, de la cultura ni del hilo.

### 5.10 M10 — Contrato futuro de `PlanReadSet`, `Upstream` y §7.2

A3 **define** este contrato ahora; **G9 lo implementa**.

**(a) `SymbolResultObservation` con `Failed`.** Compara:

- los diagnósticos en el orden de M3;
- los datos estables de cada diagnóstico según P21.6: el id ausente de cada `BrokenReference`, cuya secuencia es la de
  los ids ausentes en el orden de P11.1; los miembros de `Cycle`; y los datos intrínsecos;
- para cada `DependencyFailed`: `Causa`, los ids de la `Cadena` representativa y la firma de la `Raíz`;
- `RootCauses(símbolo)` como dato estable **adicional**, con la regla de P21.6 y de D19: «Si la implementación necesita
  más datos para distinguir dos causas, los añade como datos estables» (l. 1682-1683; D19, l. 1206-1207).

Por tanto, un fallo con raíces {A, B} y otro con raíces {A} **no** son observaciones iguales: `ABORT BEFORE WRITE`.
`Success` sigue comparando el valor exacto.

**(b) `RepairDecisionObservation` con `Upstream`.** Este es el cambio material que exige el ADR de reemplazo (A3 §12).

| | Contrato vigente (ADR-0041 D20) | Contrato de A3 |
|---|---|---|
| Datos estables | Por cada variable leída que falla: `SymbolId` + «su causa raíz» | Por cada variable leída que falla, en el orden de P11.1: `SymbolId` + `RootCauses(variable)` **completo** |
| Coincidencia | «la misma variable sigue fallando por la misma causa raíz» | Las mismas variables fallan y el `RootCauses` de cada una es igual, firma a firma y en el mismo orden |

- Los eslabones intermedios de la cadena **no** entran: `Upstream` compara **por qué** falla la variable, no el camino
  representativo elegido.
- Si cualquier miembro del conjunto desaparece, aparece o cambia de firma, la observación **no** coincide:
  `ABORT BEFORE WRITE`, y no se borra ninguna fórmula.
- `MissingTarget`, `Intrinsic` y `Domain` no cambian, ni la precedencia de D18.

**(c) §7.2, mensaje de recuperación.** Para una fuente:

```text
SourceRoots(fuente) = ∪ RootCauses(v)   para cada variable v que la fuente lee y que falla
```

- La causa raíz única existe **si y solo si** `|SourceRoots| = 1`; X es su único elemento.
- La condición del punto 2 («todas las fuentes fallidas de todos los racks que consumen el cierre de X tienen causa
  superior con causa raíz exactamente X») se lee: para cada una de esas fuentes, `SourceRoots = {X}`.
- Solo entonces el diagnóstico puede decir «Corregir <X> permitiría recuperar…», con las reglas vigentes de §7.2. Si X es
  un `CycleRoot`, `<X>` nombra a sus miembros, como hoy.
- Con más de una raíz, **nunca** hay una promesa de recuperación por raíz única.

**(d) Cadena representativa frente a causas raíz (CX-3).**

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
reparación compara la razón, no el camino (D20, l. 1289).

---

## 6. Ciclos, orden y conjuntos

### 6.1 Ciclos

- La detección sigue basada en SCC (P13.1); un nodo con arista a sí mismo es una SCC cíclica de un miembro.
- Firma de raíz: `CycleRoot[miembros en el orden de P11.1]`.
- Todo miembro recibe `Cycle` con la misma lista ordenada.
- Un miembro puede tener además `BrokenReference` propios de su definición y `DependencyFailed` por dependencias que
  fallan fuera de su SCC. **Nunca** tiene `DependencyFailed` por una arista dentro de su SCC, tampoco por una
  autorreferencia.
- P13.4 («no son evaluables (`Cycle` o `DependencyFailed`)», l. 1182-1183) se lee así: un miembro tiene siempre `Cycle`;
  un dependiente transitivo que no es miembro tiene al menos un `DependencyFailed`; ninguno se evalúa y ninguno tiene
  fallback.
- P13.5 (l. 1186-1187) se cumple sin cambio: todos los miembros dan `Failed` con `Cycle` y sus dependientes dan
  `DependencyFailed`.

### 6.2 Orden topológico (DI-1)

Conceptualmente: se elige repetidamente, entre los nodos que no son miembros de un ciclo y cuyas dependencias que tampoco
lo son ya se emitieron, el menor `SymbolId` en el orden de P11.1.

- Se admite cualquier algoritmo equivalente.
- Las aristas hacia miembros de un ciclo y hacia ids ausentes no bloquean.
- Los miembros de un ciclo quedan **fuera** del orden de evaluación de P16.2.
- Los dependientes de un ciclo **aparecen** en ese orden, porque la regla conceptual los emite, y fallan sin evaluación
  numérica (M1).
- El orden no depende de la enumeración de las entradas.

### 6.3 Orden de la lista de ciclos (DI-2)

Ascendente por el miembro mínimo de cada ciclo en el orden de P11.1. Las SCC son disjuntas, así que el orden es total
(P12.5).

### 6.4 Conjuntos transitivos (DI-3)

- Transitivo = alcanzable por un camino de longitud ≥ 1.
- Un miembro de ciclo puede pertenecer a sus propios conjuntos transitivos de dependencias y dependientes a través de un
  camino no vacío del ciclo.
- Los ids ausentes no son nodos del grafo.
- Coincide con P21.2 (`I = {X} ∪ dependientes transitivos de X`, l. 1530) y con P20.7 (un miembro de ciclo tiene
  dependientes).

### 6.5 Conjuntos derivados de G7 (DI-4)

G7 expone:

- `Dependencies(s)` directas;
- `Dependents(s)` directos;
- `Dependencies(s)` transitivas;
- `Dependents(s)` transitivos;
- `RootCauses(s)` (M6), la única adición de A3.

**No** entran en G7: el cierre afectado de racks, el descubrimiento de consumidores, la implementación del
`PlanReadSet`, la ejecución de la reparación ni el `MutationPlan`. El conjunto afectado y el conjunto leído de P12.7
siguen en P21.2 y P21.6, en G9.

---

## 7. S1, `InvalidArguments` y traza

### 7.1 S1 = A

- G7 crea los `BrokenReference` de un símbolo desde los datos autoritativos de dependencias del grafo (P11.1, P12.4), en
  el orden de P11.1 (M3).
- Por M1, `RegistryEvaluation` nunca llama al evaluador para un símbolo con ids ausentes.
- `ExpressionEvaluator` de G6 **no** se modifica para alinear su recorrido privado de ids ausentes. Llamado directamente,
  sigue informando en orden de primera aparición (el bucle de
  `src/RackCad.Application/Expressions/ExpressionEvaluator.cs:148-176` sobre `ReferencesOf`, `:181-221`), como fija
  `ExpressionEvaluatorTests.UNA_REFERENCIA_A_UN_ID_AUSENTE_DA_BROKENREFERENCE_SIN_EVALUAR_NADA`.
- El evaluador no se convierte en una segunda autoridad del grafo: P11.4 enumera los consumidores de la extracción de
  dependencias y el recorrido interno del evaluador no es uno de ellos, así que P11.5 se cumple.

### 7.2 Datos estables de `InvalidArguments`: G8

- Los datos estables de `InvalidArguments` están **incompletos** para la comparación estructural de P21.6 y D19
  (l. 1677; D19, l. 1203): el diagnóstico de G6 no lleva el token de la función ni el número de argumentos
  (`ExpressionEvaluator.cs:329-330`).
- Hacen falta los dos **antes de G9**.
- Se programan en **G8**, antes de que `InspectBinding` y la firma `Intrinsic` pasen a ser autoritativas. Es allí donde un
  árbol con aridad errónea aparece por primera vez desde la persistencia: al escribir, el binder ya lo rechaza.
- **No** bloquea G7. G7 transporta la raíz como `(dueño, código, datos estables)` y no fija que `InvalidArguments` tenga
  solo el código.
- A3 no lo implementa.

### 7.3 Traza opt-in y Explain

- La «cadena de causas de un fallo» de la traza (P16.3, l. 1277-1278) son los `DependencyFailed` del símbolo con sus
  cadenas representativas, referenciadas y no copiadas.
- `RootCauses` no forma parte de la traza: se deriva de la `RegistryEvaluation`.
- La traza sigue siendo opt-in, acotada, desactivable sin cambiar ningún resultado, y nunca se compara (P21.6).
- Para ID28 (P27.1, l. 2255; D24, l. 1456-1461), los diagnósticos llevan su cadena representativa y `RootCauses` se
  deriva sin re-evaluar. Nada se persiste.

---

## 8. Casos lineales: sin cambio

Un fallo es **lineal** cuando cada símbolo que falla en el cierre de dependencias del símbolo observado, o de la variable
leída por una razón `Upstream`, tiene diagnósticos de un solo código y, si ese código es `DependencyFailed`,
exactamente uno. Si, antes y después, esos fallos son lineales, toda comparación da el **mismo** resultado que con V6:

- cada `DependencyFailed` lleva la única cadena y la única raíz, las de V6;
- `RootCauses` es un conjunto de un elemento, igual si y solo si la raíz es igual;
- `Upstream` con `{raíz}` coincide exactamente cuando coincidía la raíz de V6.

La única diferencia es que el dato estable adicional `RootCauses` está presente de forma mecánica.

Un fallo con **una sola** raíz pero **no lineal** —un diamante, o un símbolo con diagnósticos de varios códigos— no
estaba definido en V6 (A3 §3.2). Lo define A3 (M2, M4) y su comparación la fija M10, incluida la carrera CX-3
(T-A3-19).

| Escenario | Con A3 | Resultado |
|---|---|---|
| P8.10 caso C (l. 987-995) y T-V4-01 (l. 2355) | `A = [BrokenReference(id-roto)]`; `B = [DF{A; [A]; (A, BrokenReference, [id-roto])}]`; `AlturaFinal = [DF{B; [B, A]; (A, BrokenReference, [id-roto])}]` | Igual: `OutOfRange(B)` y plan vacío |
| T-V5-01 (l. 2372) | `D = X + #<id-roto>` con X sana: `D = [BrokenReference(id-roto)]`, `RootCauses(D) = {(D, BrokenReference, [id-roto])}` | Igual: **COMMIT** |
| T-V5-02 (l. 2373) | Otra causa: cambian los diagnósticos y `RootCauses` | Igual: `ABORT BEFORE WRITE` |
| T-V6-01 (l. 2388) | `Upstream(Holgura: {(Holgura, BrokenReference, [id-roto])})` | Igual: **COMMIT** |
| T-V6-02 (l. 2389) | Holgura se recupera: `Healthy` | Igual: `ABORT BEFORE WRITE` |
| T-V6-03 (l. 2390) | Otra causa raíz: cambia `RootCauses(Holgura)` | Igual: `ABORT BEFORE WRITE` |
| T-V4-05, T-V4-06 y T-V4-11 (l. 2359-2360, 2365) | Cada fuente con causa superior tiene `SourceRoots` de un elemento; la fuente intrínseca de T-V4-06 (`ABS(A, B)`) no tiene `SourceRoots` | Igual: mensaje o bloqueo según §7.2 |

---

## 9. Modelos candidatos

| Modelo | Qué haría | Decisión | Por qué |
|---|---|---|---|
| **MC-A** (R-A) — un `DependencyFailed` por dependencia directa que falla, con cadena representativa | Cardinalidad por dependencia y cadenas lineales con colas compartidas | **Aceptado con modificación** | Encaja con P14.3, P15.1 y P21.6 y es `O(V + E)`. Modificaciones: (1) miembros de ciclo con `DependencyFailed` solo fuera de su SCC; (2) coexistencia con `BrokenReference` propios, sin evaluación; (3) orden por código (P15.7) antes que por causa, con el desempate de P11.1; (4) «primer diagnóstico» según M3, raíz de referencia rota agregada por dueño y raíz de ciclo como unidad; (5) `RootCauses`, porque «varias raíces = varios `DependencyFailed`» es falso con varias raíces detrás de una sola dependencia (U2) o detrás del fallo externo de un miembro (U4); (6) datos del `PlanReadSet` según M10 |
| **MC-B** (R-B) — un `DependencyFailed` por símbolo, el de la primera dependencia que falla en P11.1 | Un solo diagnóstico superior por símbolo | **Rechazado** | Incumple P14.3 para las dependencias no elegidas; oculta causas posteriores (CX-1); con una sola raíz representativa, §7.2 mostraría una raíz única falsa y la reparación podría borrar una fórmula con una premisa cambiada (CX-2) |
| **MC-C** (R-C) — un `DependencyFailed` por causa raíz distinta | Diagnósticos por raíz | **Rechazado** como modelo de diagnósticos | Exige un segundo desempate de camino por cada par símbolo-raíz; calcular las raíces de todos los símbolos cuesta `O(E·R)` en el peor caso, no `O(V + E)`; pierde el id de la causa directa de P14.3. El concepto de conjunto de raíces se conserva, bajo demanda, en `RootCauses` (M6) |
| Un `DependencyFailed` por cada camino hasta una raíz | Enumeración completa | **Rechazado** | Crece como 2^k con k diamantes apilados |
| Una sola causa raíz representativa en `Upstream` (lectura literal del singular de D20) | Mantener una raíz por variable | **Rechazado** | CX-2: incumple la regla de derivación de D19 y §7.2.6 |

---

## 10. Cláusulas afectadas y lectura conjunta

Cuando exista el nuevo freeze (A3 §13), estos textos se leen con A3. Las líneas son las de los blobs exactos de V6
(`ef4db3a`) y de ADR-0041 (`c6a3d2b`). En la columna «Lugar», `§n` es de V6; en la columna «Lectura con A3», las
referencias a este documento llevan el prefijo `A3`.

**Regla general de lectura.** Toda otra mención en V6 o en ADR-0041 de «la causa raíz» de una variable, de «la cadena
hasta la causa raíz» o de «la causa» de un `DependencyFailed` se lee: cadena representativa y firma de raíz para cada
`DependencyFailed` (M4, M5), y `RootCauses` para el conjunto de causas raíz de un símbolo (M6).

### 10.1 V6, enmendadas directamente

| Lugar | Texto vigente | Lectura con A3 |
|---|---|---|
| P13.4 (l. 1182-1183) | «no son evaluables (`Cycle` o `DependencyFailed`)» | A3 §6.1: un miembro tiene siempre `Cycle`, y puede tener `BrokenReference` propios y `DependencyFailed` por dependencias fuera de su SCC; un dependiente no miembro tiene al menos un `DependencyFailed` |
| P14.3 (l. 1204-1205) | «si una dependencia falla, sus dependientes reciben `DependencyFailed` con el id de la causa, sin evaluarse» | M1 y M2: un `DependencyFailed` por cada dependencia directa que falla fuera de la SCC del dueño, con `Causa` = ese id; coexiste con los `BrokenReference` propios y con `Cycle` |
| P15.1 (l. 1233-1234) | «Un fallo por causa superior lleva además la cadena hasta la causa raíz» | M4: cada `DependencyFailed` lleva una cadena representativa y la firma de su raíz; el conjunto completo es `RootCauses` (M6) |
| P15.7 (l. 1250) | «Orden determinista: posición, código y dueño» | M3: con el desempate por el id sujeto en el orden de P11.1 |
| P16.3 (l. 1277-1278) | Traza con la «cadena de causas de un fallo» | A3 §7.3 |
| P21.6, fila `DependencyFailed` (l. 1676) | «La cadena de causas hasta la raíz (P15.1): el id de cada eslabón, y el código y los datos de la causa raíz» | M4 y M10(a): `Causa`, ids de la `Cadena` y firma de la `Raíz`, más `RootCauses(símbolo)` como dato estable adicional |
| P21.6, fila `Upstream` (l. 1694) y coincidencia (l. 1702-1703) | «su `SymbolId` y su causa raíz»; «la misma causa raíz» | M10(b): `SymbolId` + `RootCauses` completo; coincide solo con el mismo conjunto |
| §0.3, `ExpectedRepairReason` y razón `Upstream` (l. 105, 123-124) | `Upstream(rootCauseSignature)`; «la misma causa raíz estructurada» | M10(b) |
| §0.1, fila RR-1 (l. 65) | `Upstream(firma de causa raíz)` | M10(b) |
| P24.6, tabla, fila `Upstream` (l. 2162) | «Una variable leída falla: `Cycle`, `DependencyFailed`, `BrokenReference` o un error propio de esa variable» | La clasificación no cambia; sus datos estables son los de M10(b) |
| §7.2, puntos 1 y 2 (l. 2759-2766) | «conjunto de causas raíz»; «causa raíz única X»; «causa raíz exactamente X» | M10(c): `SourceRoots` |
| P28.3, fila G7 (l. 2290) | «… cortocircuito `DependencyFailed`; conjuntos de P12.7» | Además, T-A3-01 a T-A3-16 (A3 §11.1) |
| P28.3, fila G9 (l. 2292) | Pruebas de G9 | Además, T-A3-17 a T-A3-21 (A3 §11.2); T-A3-21 también en G10 |

### 10.2 V6, leídas con A3

| Lugar | Texto vigente | Lectura con A3 |
|---|---|---|
| §0.6 (l. 185-189) | `Failed` compara «el código y sus datos estructurados estables, como los ids y la causa»; `Failed(causa A) → Failed(causa B)` aborta | M10(a) |
| P8.10, escenario del caso C (l. 987-995) | `A` rota, `B` y `AlturaFinal` en `DependencyFailed` | Sin cambio (A3 §8) |
| P12.7 (l. 1163-1169) | `Dependencies(s)` y `Dependents(s)`, directos y transitivos; conjuntos afectado y leído | A3 §6.4 y A3 §6.5: G7 añade `RootCauses`; los conjuntos afectado y leído siguen en G9 |
| P13.2 (l. 1178-1179) | Cada miembro recibe `Cycle` con la lista completa | Sin cambio (A3 §6.1) |
| P13.5 (l. 1184-1191) | Miembros `Failed(Cycle)` con la ruta completa; dependientes en `DependencyFailed` | Se cumple sin cambio (A3 §6.1) |
| P14.1 (l. 1200-1201) | Orden topológico con el desempate de P11.1 | A3 §6.2 (DI-1) |
| P15.3 (l. 1242) | `Cycle` y `DependencyFailed` entre los códigos semánticos | Sin cambio: ningún código nuevo |
| P17.7 (l. 1356-1359) | Los errores semánticos dan un resultado por símbolo o por fuente | Sin cambio |
| P21.2, paso 4 (l. 1530) | `I = {X} ∪ dependientes transitivos de X` | Sin cambio (A3 §6.4) |
| P21.3 (l. 1556) | «también con dos errores dentro de un ciclo» | Si los dos errores recaen en un mismo miembro, coexisten como diagnósticos (M2); la salida por R1, R2 y la reparación no cambia |
| P21.6, sin dependencias transitivas por separado (l. 1641-1645) | «El resultado de un símbolo observado ya incorpora el de sus dependencias» | Con M10(a), ese resultado incorpora también `RootCauses` |
| P21.6, resultado comparable (l. 1667-1670) | «`Failed` compara, en el orden determinista de P15.7, el código de cada diagnóstico del símbolo y sus datos estructurados estables» | M3 y M10(a) |
| P21.6, filas `BrokenReference` y `Cycle` (l. 1674-1675) | Ids ausentes y miembros en el orden de P11.1 | M3 ordena los `BrokenReference` de un símbolo; `Cycle` sin cambio |
| P21.6, ampliación de datos estables (l. 1682-1683) | «Si la implementación necesita más datos para distinguir dos causas, los añade como datos estables» | Base de `RootCauses` como dato adicional en M10(a) |
| P21.6, tabla «Un fallo no aborta por sí mismo» y su regla (l. 1796, 1799-1801) | `Failed(causa A)` → `Failed(causa B)` → `ABORT BEFORE WRITE` | M10(a): raíces {A, B} frente a {A} es un cambio de causa |
| P21.6, escenario de `RepairBrokenRack` (l. 1844-1853) | `Upstream(Holgura: BrokenReference #<id-roto>)`; «Holgura falla por otra causa raíz → ABORT» | `Upstream(Holgura: {(Holgura, BrokenReference, [id-roto])})`; mismo resultado (A3 §8) |
| P21.6, casos fijados por prueba (l. 1872, 1875-1877) | «fallo con otra causa»; «fallo superior idéntico, recuperado o con otra causa» | M10(a) y M10(b) |
| P22.9 (l. 1951) | «las variables en error, con su causa raíz, los miembros y la ruta de cada ciclo y sus dependientes» | Con su `RootCauses` |
| P22.9 (l. 1954) | Mensaje de recuperación condicional para cada fuente con causa superior | M10(c) |
| P24.6, precedencia (l. 2168-2169) | Estructural, id ausente, tipo incompatible y fallo semántico, para una fuente de rack | Sin cambio: M2 trata de símbolos |
| P27.1 (l. 2255) | «diagnósticos (P15), con la cadena hasta la causa raíz» | A3 §7.3 |
| §5.1, filas `Cycle` y `DependencyFailed` (l. 2592, 2595) | Clasificación semántica | Sin cambio |
| §5.2, decisiones 5 y 7 (l. 2619-2625) | El panel muestra la causa raíz; ciclos persistidos con la ruta | Con `RootCauses`; A3 §6.1 |
| §7.2, commit de la reparación (l. 2734-2738) | «ya no es reparable por la misma razón —se recuperó, cambió su causa…—» | M10(b): «cambió su causa» incluye cualquier cambio de `RootCauses` |
| §7.2, tabla de confirmación (l. 2749, 2751) | `FailureCause`; `UpstreamCause`: «la variable superior y su código» | Cada variable superior que falla, en el orden de P11.1, con su `RootCauses`, en la capa de texto (G10) |
| §7.2, puntos 3 y 5 (l. 2773, 2789-2791) | «Si X es un ciclo, `<X>` nombra a sus miembros»; consecuencia de dos causas raíz independientes | X puede ser un `CycleRoot`; sin cambio |
| P28.5 a P28.8: T-V3-09 (l. 2333), T-V3-13 (l. 2337), T-V4-01 (l. 2355), T-V4-05 y T-V4-06 (l. 2359-2360), T-V4-11 (l. 2365), T-V5-01 y T-V5-02 (l. 2372-2373), T-V5-09 (l. 2380), T-V6-01 a T-V6-03 (l. 2388-2390) | Escenarios vigentes | Sin cambio de expectativa: los escenarios con una cadena de fallos son lineales (A3 §8). En T-V3-09, si los dos errores recaen en un mismo miembro, coexisten como diagnósticos (M2), y la salida por R1, R2 y la reparación no cambia |
| §9, lo que el ADR tiene que fijar de V5 y V6 (l. 2916-2924) | Comparación por resultado y por razón estructurada | M10 |
| §10, costes (l. 3013), y §11.2, R11 (l. 3069) | Dos causas raíz independientes; «diagnósticos con causa raíz» | Sin cambio; con `RootCauses` |
| §14, estado (l. 3233) | «`Intrinsic`, `Upstream` y `Domain` comparadas sin texto» | `Upstream` con M10(b) |

### 10.3 ADR-0041, en el ADR de reemplazo

| Lugar | Texto vigente | Lectura con A3 |
|---|---|---|
| **D20, fila `Upstream` (l. 1289)** | «su `SymbolId` y su causa raíz —el símbolo donde empieza el fallo, su código y sus datos estables—» | **Cambia**: `SymbolId` + `RootCauses` completo (M10(b)) |
| **D20, coincidencia (l. 1298-1299)** | «la misma variable sigue fallando por la misma causa raíz → coincide; se recupera o cambia la causa raíz → no coincide» | **Cambia**: el mismo `RootCauses` completo; si cualquier miembro aparece, desaparece o cambia de firma → no coincide |
| D20, pruebas (l. 1316-1318) | T-V6-01 a T-V6-09 | Sin cambio de expectativa (A3 §8); más T-A3-17 a T-A3-21 |
| D8, diagnósticos del parser (l. 718-719) | «El orden determinista de P15.7 no cambia» | Sin cambio para los diagnósticos con posición; M3 solo desempata diagnósticos de evaluación del mismo dueño y código |
| D14, semánticos y decisiones 1 y 7 (l. 917-923, 942-943) | Lista de fallos semánticos; alcance del bloqueo; ciclos persistidos «con la ruta» | Sin cambio; A3 §6.1 |
| D14, decisión 6 (l. 940-941) | «también con dos errores dentro de un ciclo» | Si los dos errores recaen en un mismo miembro, coexisten como diagnósticos (M2); la salida por R1, R2 y la reparación no cambia |
| D15, aristas rotas y conjuntos (l. 966-968) | «expone dependencias y dependientes directos y transitivos, el conjunto afectado y el conjunto leído» | A3 §6.4 y A3 §6.5 |
| D15, ciclos (l. 969-973) | «cada ciclo informado con sus miembros en orden»; miembros y dependientes transitivos no evaluables | A3 §6.1 y A3 §6.3 |
| D15, orden y cortocircuito (l. 974-977) | «topológico sobre la parte acíclica, con el desempate de P11.1»; «reciben `DependencyFailed` con el id de la causa, sin evaluarse» | A3 §6.2; M1 y M2 |
| D18, precedencia de `InspectBinding` (l. 1031-1036) | Estructural, id ausente, tipo incompatible y fallo semántico | Sin cambio |
| D18, tabla de confirmación (l. 1051, 1053) | `FailureCause`; `UpstreamCause` | M10(c) |
| D18, mensaje de recuperación, puntos 1-3 y 5 (l. 1059-1066, 1073, 1089-1091) | «conjunto de causas raíz»; «causa raíz única X»; «causa raíz exactamente X» | M10(c): `SourceRoots` |
| D19, regla de derivación (l. 1158-1163) | «Todo dato o clasificación semántica que justifica una decisión destructiva queda representado por una observación comparable hasta el commit» | Sin cambio; A3 la aplica (CX-2) |
| D19, sin dependencias transitivas por separado (l. 1176-1180) | «El resultado de un símbolo observado ya incorpora el de sus dependencias» | Con M10(a) |
| D19, resultado comparable (l. 1194-1207) | Orden de P15.7; fila `DependencyFailed` (l. 1202); ampliación de datos (l. 1206-1207) | M3, M4 y M10(a) |
| D19, «Un fallo no aborta por sí mismo» (l. 1248-1259) | `Failed(causa A)` → `Failed(causa B)` → `ABORT BEFORE WRITE` (l. 1256) | M10(a) |
| D19, escenarios normativos (l. 1270-1273) | «recuperado o con otra causa → abort» | M10(a) y M10(b) |
| D22, panel (l. 1400-1402) | «las variables en error con su causa raíz, los miembros y la ruta de cada ciclo» | Con `RootCauses` |
| D23, cierre (l. 1424) | `I = {X} ∪ dependientes transitivos de X` | Sin cambio (A3 §6.4) |
| D24, ID28 e ID29 (l. 1456-1461) | «los diagnósticos con su cadena hasta la causa raíz» | A3 §7.3 |
| Consecuencias (l. 1610) | «un rack con dos causas raíz independientes solo sale por la reparación» | Sin cambio |

### 10.4 No cambian

- La identidad de `projectVariable`, el cualificador `Q(clave)` y su gramática (A2).
- La guarda de recurso del parser y el orden de P15.7 para los diagnósticos con posición (A1, l. 301-302; D8,
  l. 718-719).
- La extracción de dependencias y su función única (P11), la dirección del grafo (P12.2), las aristas rotas sin nodo
  (P12.4), la detección de SCC (P13.1) y el `Cycle` del preflight con plan vacío (P13.3).
- El catálogo cerrado de códigos 1–29: sin códigos nuevos, sin renumerar y sin cambiar de clase.
- El contrato y la semántica numérica del evaluador de G6 (P14.4).
- La comparación de `Success`, las razones `MissingTarget`, `Intrinsic` y `Domain`, y la precedencia de D18.
- Las reglas R1, R2 y R3 de P21.3 y el contrato raíz de P8.10.
- La estructura del `PlanReadSet`: tipos de observación, fases, invariantes, orden de las observaciones y secuencia de
  commit; la revisión acotada de V8-R05.
- Schema V-0 y la persistencia: A3 no persiste nada nuevo.
- ADR-0041 D1–D13, D16, D17, D21, D23 y D25.

---

## 11. Contrato de pruebas futuro

### 11.1 G7 (obligatorias)

| # | Prueba | Qué fija |
|---|---|---|
| T-A3-01 | U1, raíces independientes | Los diagnósticos exactos de A3 §4.1 y `RootCauses(D)` |
| T-A3-02 | U2, dos raíces detrás de una dependencia | Un `DF{B; [B, A1]; …}` y `RootCauses(D) = {A1, A2}` |
| T-A3-03 | U3, `BrokenReference` propia y fallo superior | Los dos diagnósticos; D no se evalúa: `D = (A + #<m>) + 1 / 0` no da `DivisionByZero` |
| T-A3-04 | U4, ciclo y fallo externo | Los diagnósticos exactos de A, B y F y sus `RootCauses` (A3 §4.4) |
| T-A3-05 | Desempate de `DependencyFailed` del mismo código | Dependencias escritas en orden inverso: los `DependencyFailed` salen en el orden de P11.1 de sus causas |
| T-A3-06 | Desempate de `BrokenReference` del mismo código | Ids ausentes escritos en orden inverso: los `BrokenReference` de `RegistryEvaluation` salen en el orden de P11.1 (S1 = A) |
| T-A3-07 | Orden completo de un símbolo no evaluado | `BrokenReference` (22) antes que `Cycle` (23) antes que `DependencyFailed` (24), con el ejemplo de M3 |
| T-A3-08 | `DependencyFailed` de un miembro solo fuera de su SCC | Ciclo de dos, ciclo largo y autorreferencia: ningún `DependencyFailed` por aristas internas |
| T-A3-09 | Deduplicación de raíces en un diamante | Una raíz alcanzada por dos caminos aparece una vez en `RootCauses` |
| T-A3-10 | Deduplicación de raíces al entrar dos veces en el mismo ciclo | `RootCauses(F)` con un solo `CycleRoot` |
| T-A3-11 | Preservación del caso lineal | La cadena de P8.10 caso C: diagnósticos con los datos de V6 y `RootCauses` de un elemento |
| T-A3-12 | Determinismo por permutación | Toda permutación de las entradas da diagnósticos, cadenas, `RootCauses`, ciclos y orden idénticos |
| T-A3-13 | Determinismo por cultura | Resultados idénticos bajo culturas distintas |
| T-A3-14 | Sin enumeración exponencial de caminos | k diamantes apilados (2^k caminos): el número de `DependencyFailed` es el de aristas hacia dependencias que fallan, `RootCauses` tiene un elemento y la prueba mide conteos, no tiempos |
| T-A3-15 | Lista de ciclos y conjuntos transitivos | Ciclos ascendentes por su miembro mínimo (DI-2); un miembro está en sus propios conjuntos transitivos y un id ausente no es nodo (DI-3) |
| T-A3-16 | Orden topológico | Menor `SymbolId` listo en P11.1, miembros fuera del orden y dependientes de un ciclo presentes y fallidos (DI-1) |

### 11.2 G9 (obligatorias)

| # | Carrera | Resultado |
|---|---|---|
| T-A3-17 | Antes, raíces {A, B}; después, raíces {A} | `ABORT BEFORE WRITE`, en `SymbolResultObservation` y en `Upstream` |
| T-A3-18 | Antes, raíces {A}; después, raíces {A, B} | `ABORT BEFORE WRITE`, en `SymbolResultObservation` y en `Upstream` |
| T-A3-19 | Mismas raíces; solo cambia la cadena representativa porque cambia un camino del grafo (CX-3) | `SymbolResultObservation`: `ABORT BEFORE WRITE`, porque P21.6 compara los ids de la cadena. `Upstream`: coincide, porque solo compara `RootCauses`, y el commit sigue si nada más difiere |
| T-A3-20 | CX-2: `H = A1 + A2` con las dos rotas, fuente retirada `palletTolerance = H + 2`; reaparece `a2` antes del commit | `ABORT BEFORE WRITE`; la fórmula no se borra |
| T-A3-21 | §7.2 con raíces detrás de una sola variable (G9 y G10) | `SourceRoots` de un elemento → sugerencia según §7.2; `SourceRoots = {A1, A2}` a través de una sola variable → nunca sugerencia de raíz única |

---

## 12. ADR

- ADR-0041 está **aceptado** y es inmutable.
- A3 cambia **materialmente** el dato estable de `Upstream` de D20 (l. 1289, 1298-1299):

  ```text
  de:  una causa raíz por cada variable leída que falla
  a:   el conjunto completo RootCauses por cada variable leída que falla
  ```

- Cambiar la decisión de un ADR aceptado exige un ADR nuevo que la reemplace (README de `docs/adr`; preámbulo de
  ADR-0041, l. 43-45). Por tanto: **REPLACEMENT ADR REQUIRED**.
- **Corrección de gobierno.** La revisión del Arquitecto concluyó `CONTRACT_ACTION = AMENDMENT REQUIRED` y
  `Architect = AMENDMENT REQUIRED`, sin ADR de reemplazo, con una condición explícita: si el Coordinador leía el singular
  de D20 («su causa raíz —el símbolo donde empieza el fallo—») como una decisión de usar exactamente una raíz, la parte
  de `Upstream` cambiaba D20 y la acción pasaba a `REPLACEMENT ADR REQUIRED`. El Coordinador aplicó esa lectura
  (`GOVERNANCE CORRECTION: REPLACEMENT ADR REQUIRED`), y A3 la registra como vigente.
- **No se asigna número ahora.** Antes de publicarlo se censan de nuevo los números de `main` y de **todas** las ramas
  activas.
- ADR-0041 **no se edita**. Cuando el Owner acepte el ADR de reemplazo, ADR-0041 pasa a `reemplazado por ADR-NNNN`, con
  el mismo patrón que ADR-0038 → ADR-0040 → ADR-0041.
- **Alcance exigido al ADR de reemplazo.**
  - Es un sucesor **completo**, D1–D25.
  - D20 recoge M10(b).
  - D14, D15, D18, D19, D22 y D24 recogen las lecturas de A3 §10.3.
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
  Expression, el `PlanReadSet` y la `RepairDecisionObservation`, y cambia ADR-0041 D20.

  ```text
  STOP IMPLEMENTATION     → G7 BLOCKED BEFORE RED, sin código ni pruebas
  → document finding      → STOP de G7 y este amendment
  → Coordinator review    → AGREED WITH ARCHITECT M1–M10
                            GOVERNANCE CORRECTION: REPLACEMENT ADR REQUIRED
  → Architect review      → MULTI_CAUSE_AMBIGUITY = CONFIRMED; PENDING EXACT A3 REVIEW
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
| Evaluador de G6 | Un `BrokenReference` por id ausente en orden de primera aparición (bucle en `ExpressionEvaluator.cs:148-176` sobre `ReferencesOf`, `:181-221`), fijado por `ExpressionEvaluatorTests.UNA_REFERENCIA_A_UN_ID_AUSENTE_DA_BROKENREFERENCE_SIN_EVALUAR_NADA` (`[Id(99), Id(98)]`); exige un valor para cada símbolo presente (`:156-164`); el primer fallo termina la evaluación; `InvalidArguments`, `DivisionByZero` y `NonFiniteResult` sin datos (`:329-330`) |
| Catálogo | `ExpressionDiagnosticCode.cs:93-111`: `BrokenReference` 22 … `NonCanonicalForm` 28; `Cycle` y `DependencyFailed` declarados y aún no producidos |
| Guarda G1 | `DependencyGraph` sigue prohibido en Application (`tests/RackCad.Tests/ProjectVariablesConformanceTests.cs:184-203`); evoluciona en G7 según P28.4, sin cambio por A3 |
| Auditoría independiente del borrador | Solo lectura, dos pasadas. La primera halló un hallazgo material —la condición «a lo sumo una causa raíz» de A3 §8 era demasiado amplia; ahora exige un fallo lineal— y catorce menores de citas, censo y nomenclatura. La segunda confirmó las correcciones (`NO MATERIAL FINDINGS`) y cuatro menores más. Todos se corrigieron antes del commit |
| Archivos | A3 añade solo este documento; ninguna medición dejó cambios en el árbol |
| Autoridades | V6 `ef4db3aa400483ff25a8f39b2beb93708fa43d1a`, A1 `d62019088b9e7a140d5066799afe6ace6db303ba`, A2 `49a925336dd3775929a35b0f40b73cb7f8c487f7`, ADR-0041 `c6a3d2ba0ab9df93e51efcddf04ed9255b1a2231` y freeze vigente `57736f725663aab79a24b29685ed34e8c3e9ebad`, sin cambio |
| `main` | `dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093`, sin cambios en el núcleo de expresiones. La rama no se rebasa por un amendment documental |
| Ramas paralelas | I-52 `915a52056423291b5fcaff9955ed07a984c094ba` (Proposal V12, solo documentación); I-55 `d091eeb1e96d3cb7cd8989151b6cfdfc6ac0a7fb` (G2C, solo documentación); I-56 `12fb5095100696bcb6d2fd7cef69e110fbcd8dd9` (G0.1, solo documentación). Ninguna toca `src/RackCad.Application/Expressions`, las pruebas de expresiones ni la guarda G1 |

---

## 15. Estado

```text
Amendment = A3

Finding = MULTI-CAUSE DEPENDENCYFAILED / ROOTCAUSES CONTRACT GAP

Coordinator = AGREED WITH ARCHITECT M1–M10
              GOVERNANCE CORRECTION:
              REPLACEMENT ADR REQUIRED

Architect = PENDING EXACT A3 REVIEW

Owner = PENDING FUTURE REPLACEMENT ADR

G6 = CLOSED

G7 = BLOCKED

G8 = BLOCKED

CURRENT FREEZE = HISTORICAL / INVALIDATED FOR FURTHER G7 IMPLEMENTATION
```
