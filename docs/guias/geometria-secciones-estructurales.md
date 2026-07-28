# Geometría de secciones estructurales

Esta guía explica cómo las **983** secciones que I-36A dejó en el catálogo se convierten en geometría
dibujable: qué se genera, con qué fidelidad, qué **no** se inventa y cómo llega a AutoCAD.

La decisión que la gobierna es
[ADR-0022](../adr/0022-geometria-parametrica-de-secciones-estructurales.md). El catálogo del que parte
todo está en [secciones-estructurales.md](secciones-estructurales.md), y las unidades siguen siendo las
de [ADR-0005](../adr/0005-estrategia-de-unidades.md): **pulgadas**, sin conversión en ningún punto.

---

## 1. Paramétrica, no dibujada

El contorno de un perfil estándar **se deriva de sus dimensiones**. Por eso no hay —ni habrá— un bloque
por designación: 983 bloques dibujados a mano, atados a un archivo que no se versiona, perderían justo
lo que hace útil un perfil normalizado.

Cada familia tiene un constructor en código que recibe las dimensiones tabuladas y devuelve contornos.
No hay bloques dinámicos, ni parámetros nativos de AutoCAD, ni ninguna otra autoridad geométrica: si
mañana cambia la forma de una W, cambia en un solo lugar.

---

## 2. Ejes y origen

| Concepto | Convención |
|---|---|
| Plano de la sección | XY local |
| Eje longitudinal | Z local |
| Origen transversal | **El centroide tabulado** |
| Contorno exterior | Antihorario |
| Huecos | Horario |

El origen es el centroide porque es lo que hace componibles a dos secciones distintas: apoyar una W
sobre una HSS no debería depender de qué borde eligió tabular la fuente. AISC publica `x` e `y` para
los perfiles asimétricos —canal y ángulo— y esos son los valores que se usan para centrar; no se
recalcula un centroide propio para ese fin.

### Dos centroides, dos nombres

Conviene tenerlo claro porque son cosas distintas y se confunden con facilidad:

| Propiedad | Qué es | Para qué sirve |
|---|---|---|
| `Origin` | `(0,0)`, el origen transversal | Colocar. Es la autoridad |
| `OriginBasis` | `Symmetry` (W, HSS) o `TabulatedCentroid` (C, L) | Saber **cómo** se resolvió |
| `GeometricContourCentroid` | El centroide del contorno **aproximado** | Diagnóstico |
| `GeometricCentroidResidual` | Cuánto se separan | Medir la aproximación |

El contorno que se puede derivar no incluye lo que la fuente no publica, así que su centroide de área
no cae exactamente en el tabulado. **El residuo se informa; no coloca nada.** Mover la geometría hasta
anularlo sustituiría la autoridad de AISC por nuestro propio contorno incompleto, y de paso desplazaría
el perfil de donde la fuente dice que está.

Ningún miembro público se llama simplemente `Centroid`: dos conceptos distintos bajo un mismo nombre es
justamente como se confunden.

El sentido de los contornos no es decorativo: es como se distingue material de vacío sin llevar una
bandera aparte, y se **normaliza al construir**, no se confía en que cada constructor acierte.

---

## 3. Nivel de detalle y fidelidad obtenida

Son dos cosas distintas y conviene no confundirlas:

- **Nivel de detalle** es lo que se PIDE: `Simplified` (esquinas vivas) o `Tabulated` (todo el detalle
  que la fuente permita derivar).
- **Fidelidad** es lo que se OBTUVO, y viaja con el resultado.

| Fidelidad | Significado |
|---|---|
| `Simplified` | Se pidió simplificada y eso se entregó |
| `TabulatedComplete` | Tabulada y **no falta nada** que la fuente publique |
| `TabulatedDerived` | Tabulada, pero la fuente no publica todo el detalle de la forma real |
| `DegradedToSimplified` | Se pidió tabulada y faltó un dato, así que se degradó **diciéndolo** |

Una degradación **nunca es silenciosa**: si el resultado dice que degradó, lleva un diagnóstico que
explica por qué. Hay una prueba que recorre las 983 en los dos niveles y falla si aparece una
degradación sin diagnóstico, un diagnóstico sin código o un código no declarado.

Sobre la v16.0 completa: **289** `TabulatedComplete` (las W), **694** `TabulatedDerived` (HSS, C y L) y
**cero** degradadas.

---

## 4. Qué se deriva y qué no

Esta es la línea que la iniciativa no cruza: **no se inventa un radio que la fuente no publique**.

**Sí se deriva, y está documentado:**

| Radio | Regla | Familias |
|---|---|---|
| Filete de raíz | `r = kdes − tf` | W, C, L |
| Esquina exterior HSS | `r = (Ht − h)/2 ≈ (B − b)/2` | HSS |
| Esquina interior HSS | exterior − pared nominal | HSS |

Las dos derivaciones del radio del HSS —una por cada par de paredes planas— tienen que coincidir dentro
de tolerancia; si no coinciden, se dice.

**No se deriva, y por eso se declara:**

- El **redondeo de la punta del ala** (W, C, L). AISC no lo publica y no hay forma documentada de
  obtenerlo. Las puntas quedan vivas.
- El **alma variable del canal**. El ala real es cónica; el contorno usa el espesor medio tabulado.

Añadir un radio de punta «plausible» habría mejorado el error de área de los canales. No se hizo: sería
exactamente inventar una dimensión.

---

## 5. El área, medida y explicada

El error de área frente al `A` publicado se **mide y se reporta**, no se ajusta:

| Familia | n | Máximo | Media | Sobre 5 % |
|---|---|---|---|---|
| W | 289 | 0.732 % | 0.118 % | 0 |
| HSS rect./cuad. | 525 | 10.927 % | 8.34 % | 525 |
| C | 32 | 5.545 % | 2.812 % | 3 |
| L | 137 | 3.012 % | 0.814 % | 0 |

Las dos filas altas tienen causa acreditada:

**HSS: es una diferencia de definición, no un error del contorno.** AISC calcula `A` con el espesor de
**diseño**; la geometría usa el **nominal**, porque eso es lo que se dibuja y lo que mide alguien con un
calibrador. Una prueba dedicada reconstruye el mismo contorno con `tdes` y el error cae a **1.068 %** de
media y **4.581 %** máximo, con **cero** filas por encima del 5 %: el contorno es correcto; lo que
difiere es qué espesor significa cada número.

**Canal: son 3 filas de 32**, y la causa es el redondeo de punta que la fuente no publica (sección 4).

---

## 6. La longitud no está en la sección

Una sección **no tiene largo**. La longitud vive en una **instancia prismática**, que además aporta
rotación alrededor de Z y espejo. Así el catálogo no crece una fila por medida, y la misma W12X26 sirve
para un tramo de 40" y para uno de 240".

El peso total sale de multiplicar el peso por longitud del catálogo por la longitud de la instancia:
una sola autoridad, la de I-36A.

---

## 7. Vistas

| Vista | Qué muestra |
|---|---|
| `CrossSection` | La sección; **no depende de la longitud** |
| `LongitudinalX` | El prisma mirado a lo largo de un eje transversal |
| `LongitudinalY` | El prisma mirado a lo largo del otro |
| `Isometric` | Isométrica escorzada en los tres ejes |
| Personalizada | Cualquier cámara válida |

No se llaman *frontal*, *lateral* ni *planta* a propósito: esos nombres ya significan otra cosa en los
cuatro sistemas vigentes, y reutilizarlos aquí habría invitado a confundir la vista de una pieza con la
vista de un rack.

Lo que se dibuja es un **wireframe sin eliminación de líneas ocultas**. En una isométrica se ve también
el extremo de atrás. Está dicho en vez de arreglado: un paso de líneas ocultas es caro, frágil, y cuando
falla produce dibujos sutilmente equivocados en lugar de obviamente rotos.

**Un tubo se dibuja como tubo.** Todos los contornos aportan líneas longitudinales, huecos incluidos:
un HSS visto de lado tiene cuatro caras, dos exteriores y dos interiores, separadas exactamente una
pared nominal. Los roles lo distinguen —`Generatrix` / `InteriorGeneratrix` y `EndProfile` /
`EndProfileHole`— para que un consumidor no tenga que deducir de qué contorno salió cada curva.

**Lo que colapsa deja de ser una curva cerrada.** Mirando exactamente a lo largo de X o de Y, la sección
se ve de canto: su contorno pasa de figura a recta. Emitirlo cerrado haría que cada arista se recorriera
de ida y de vuelta —en AutoCAD, una polilínea de área cero imposible de seleccionar—, así que se reduce
a los segmentos abiertos únicos que dibujan lo mismo. Por la misma razón, dos líneas colineales se
funden en una: dibujan la misma tinta.

Vista de canto, la boca del tubo cae **dentro** de la cara de corte, así que no se dibuja aparte. Lo que
delata el hueco ahí son sus generatrices interiores, no su perfil de extremo.

Las curvas llegan **teseladas** con una tolerancia de cuerda declarada. En la vista de sección un arco
es un arco, pero en cualquier vista oblicua la proyección de un círculo es una **elipse**, y emitirla
como arco sería callar un error.

---

## 8. Un solo plan neutral

El preview de la UI y el adaptador de AutoCAD consumen **el mismo objeto**:
`StructuralSectionRepresentationPlan`. Contiene curvas ya proyectadas con un **rol** (contorno, hueco,
perfil de extremo, generatriz, eje, envolvente), los límites, la fidelidad, los diagnósticos y una firma
determinista.

Esto no es un detalle de implementación: **no puede haber dos generadores geométricos**. Ni la UI ni el
plugin calculan una dimensión; reciben puntos y roles. Hay guardas de código que lo comprueban.

La **firma** es una huella del plan —puntos *y* orden de recorrido—, no una clase de equivalencia
geométrica. Sirve para que una prueba diga «no se movió nada» sin listar miles de coordenadas. Un
espejo cambia la firma aunque la figura se vea igual, porque invierte el recorrido; para comparar
figuras hay que ordenar los puntos.

---

## 9. En AutoCAD: `RACKSECCION`

```
RACKSECCION
```

…o, desde la interfaz, **`RACKCAD` → «Generar perfil estructural»**. Son **la misma cosa**: las dos
puertas llaman al mismo caso de uso (`StructuralSectionCommandFlow`), no a dos implementaciones que
coinciden. El botón del menú no es un generador aparte; sólo hace visible el que ya existe.

Abre el inspector, y al aceptar pide un punto e inserta la representación como **bloque interno del
dibujo**.

Lo que conviene saber:

- **Sin biblioteca.** No usa `blocks-library.dwg`, no añade filas a `blocks.csv` y no depende de ningún
  bloque previo. El nombre del bloque es una **salida** legible; nada se resuelve por él.
- **Falla cerrada.** Si el catálogo de secciones no valida, el comando se detiene y lo dice. Es
  deliberadamente distinto del catálogo de producto, que degrada a vacío: dibujar una viga con
  dimensiones que no pasaron la validación es peor que no dibujarla.
- **El punto se pide antes de crear nada**, así que cancelar no deja un bloque fantasma en el dibujo.
- **Definición y referencia se confirman juntas.** Una excepción no deja nada a medias porque nunca
  hubo commit.
- **La pieza va BYBLOCK en la capa 0**, de modo que la referencia insertada manda sobre color, tipo de
  línea y grosor. **Eje y envolvente** van a `RACKCAD_ANOTACIONES`, la misma capa que usa el resto de
  RackCad, para poder congelarlas sin perder la pieza.
- **Es geometría, no un rack**: sin payload, sin GUID y sin round-trip. `RACKEDITAR` no la reconoce, y
  no debería.

El comando **no aparece** en `RACKAYUDA` (ver sección 11).

---

## 10. El inspector

Superficie mínima para **ver** una sección: buscarla por designación —vale tanto la comercial como la
EDI—, filtrarla por familia, darle longitud, elegir vista, detalle, rotación y espejo, y mostrar u
ocultar eje y envolvente. Debajo se leen el peso total y la fidelidad con sus diagnósticos.

Solo ofrece secciones **habilitadas**. Una deshabilitada sigue resolviendo por id —un diseño guardado
debe seguir dibujando— pero no se vuelve a ofrecer.

**No es un configurador de miembro y no debe volverse uno.** No hay rol, material, cargas, niveles,
brazos, conexiones, fabricación, guardado ni edición de lo insertado. Todo eso es I-37. Hay una prueba
de frontera que vigila por reflexión que no aparezcan esos conceptos.

---

## 11. Limitaciones conocidas

- **Wireframe sin líneas ocultas** (sección 7).
- **Puntas vivas** en W, C y L, y **ala del canal sin conicidad** (sección 4).
- **El área del HSS no coincide con la publicada** por diferencia de definición (sección 5).
- **Lo insertado no se puede reeditar**: es geometría plana. Reabrirlo como objeto configurable es
  trabajo de I-37.
- **El error de área del canal está aceptado por decisión del dueño**, no pendiente de arreglo: 5.545 %
  máximo en 3 de 32 filas, por la conicidad y el radio de punta que AISC no publica. Está registrado en
  [`decisions/I-36B.md`](../automation/decisions/I-36B.md), y **no** convierte la fidelidad de C en
  `TabulatedComplete`.
- **Los canales no se ven como los de una librería CAD comercial**, y eso está aceptado: ver §11.b.
- **`RACKSECCION` no está en `RACKAYUDA`.** Añadirlo exigiría tocar `RackCommandReference`, que queda
  fuera del alcance de I-36B; `RACKPUSHBACK` tiene la misma ausencia desde antes. Anotado en
  [ideas-futuras.md](../ideas-futuras.md). **Desde I-36C sí está en el menú principal**, que es por
  donde entra un usuario.

---

### 11.b Los perfiles laminados no tienen aún su apariencia comercial

Al validar en AutoCAD, el dueño comparó los canales de RackCad con los perfiles comerciales de las
librerías CAD. **No reproducen completamente su apariencia**: falta la **conicidad de los patines**, los
**redondeos o chaflanes de punta** y las **transiciones características del laminado**. La comparación
confirma que esa diferencia es la que explica el error de área de la familia.

Está **aceptado y no bloquea**: los canales son `TabulatedDerived`, la geometría es honesta sobre lo que
puede afirmar, y dentro de I-36B **no se inventan** radios, chaflanes ni conicidades. Lo que esta guía
**no** dice, y conviene que nadie lo lea así, es que un canal de RackCad sea geométricamente idéntico a
uno de librería comercial: su fidelidad es **derivada** y la diferencia es conocida y está medida.

**Requisito futuro obligatorio.** Una iniciativa futura y separada deberá incorporar perfiles **IPS**
—verificando antes su correspondencia con la familia AISC `S` o con el catálogo comercial de la
empresa—, importar su fuente, modelar la inclinación de los patines, representar radios y chaflanes
**cuando exista una regla acreditada**, y mejorar visualmente C y los demás laminados. Deberá mantener
**separadas** la geometría tabulada y la visual, **declarar** cuándo una representación visual es
aproximada, y **no sustituir ni alterar** la geometría tabulada que I-36B fija. No se mezcla con
Cantilever salvo que el contrato de aquélla lo exija. Detalle en
[`ideas-futuras.md`](../ideas-futuras.md).

---

## 12. Dónde mirar en el código

| Tema | Ruta |
|---|---|
| Primitivas 2D/3D | `src/RackCad.Application/Geometry/` |
| Tolerancias compartidas | `Geometry/Vector2D.cs` (`GeometryTolerance`) |
| Contorno cerrado, área y centroide exactos | `Geometry/ClosedContour2D.cs` |
| Constructores por familia | `StructuralSections/Geometry/*SectionGeometryBuilder.cs` |
| Filete derivado (una autoridad) | `Geometry/SectionGeometrySupport.cs` |
| Códigos de diagnóstico | `Geometry/SectionGeometryDiagnostics.cs` |
| Factoría y caché | `Geometry/StructuralSectionGeometryFactory.cs` |
| Longitud, rotación y espejo | `Geometry/PrismaticSectionInstance.cs` |
| Vistas y cámaras | `Geometry/SectionViewpoint.cs` |
| Plan neutral, roles y firma | `Geometry/StructuralSectionRepresentationPlan.cs` |
| Canonicalización de proyecciones | `Geometry/SectionProjectionCanonicalizer.cs` |
| Proyección y teselado | `Geometry/StructuralSectionPlanBuilder.cs` |
| Inspector y preview | `src/RackCad.UI/StructuralSections/` |
| Materialización en AutoCAD | `src/RackCad.Plugin/Drawing/StructuralSections/` |
| Comando (punto de entrada) | `src/RackCad.Plugin/RackSeccionCommands.cs` |
| Caso de uso compartido con el menú | `src/RackCad.Plugin/StructuralSectionCommandFlow.cs` |
| Acción tipada del menú | `src/RackCad.UI/MainMenuAction.cs` |

---

## 13. Qué queda para después

**I-37 — Cantilever.** Convertir una sección en un **miembro**: rol, material, troqueles, conectores,
ménsulas, perforaciones, placas terminales, cortes de extremo, reglas de fabricación, persistencia y
BOM. Nada de eso vive aquí, y esta guía no debería crecer hacia allá.

**Perfiles IPS/S y geometría visual mejorada** (§11.b), en su propia iniciativa, aún sin abrir.
