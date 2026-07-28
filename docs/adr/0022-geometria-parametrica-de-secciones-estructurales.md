# ADR-0022: Geometría paramétrica y representación prismática de secciones estructurales

- **Estado:** **propuesto**
- **Fecha:** 2026-07-28 (redacción); pendiente de validación expresa del dueño
- **Decisores:** Mario Pérez, dueño del repositorio (**pendiente**); Claude Opus 5 (redacción)
- **Iniciativa relacionada:** I-36B `architecture/geometria-secciones-estructurales`

> **Por qué nace `propuesto`.** I-36B declara `requires_owner_validation: true` y su gate incluye ver
> la geometría dibujada en AutoCAD. Las decisiones de abajo son verificables sobre el dibujo real —el
> lado de apertura de un canal, el ala larga de un ángulo desigual, el espesor de un HSS—, así que
> aceptarlas antes de que el dueño las mire diría lo contrario de lo que ocurre. ADR-0020 y ADR-0021
> permanecen `aceptado`; éste no los reabre.

## Contexto

[ADR-0020](0020-catalogo-neutral-de-secciones-estructurales.md) creó el catálogo neutral y
[ADR-0021](0021-identidad-unidades-y-presentacion-de-secciones.md) fijó su identidad y sus unidades.
Lo que I-36A entregó son **datos**: 983 secciones con sus dimensiones y propiedades tabuladas. Lo que
no entregó, deliberadamente, es cualquier forma de **verlas**.

Convertir una sección en algo dibujable plantea preguntas que ninguna decisión previa responde, y que
son caras de revertir porque condicionan todo lo que Cantilever (I-37) construya encima:

1. **Dónde está el origen y cómo se orientan los ejes.** Un perfil no trae origen: AISC publica
   distancias «desde el borde designado», no coordenadas. Elegir mal el origen obliga a corregirlo en
   cada consumidor.
2. **Dónde vive la longitud.** Una sección transversal no tiene longitud; un miembro sí. Si la longitud
   entra en `StructuralSectionDefinition`, el catálogo deja de ser un catálogo.
3. **Cuánto detalle dibujar y qué hacer cuando falta un dato.** AISC publica `kdes` pero no publica el
   radio del filete; publica el «flat» de un HSS pero no su radio de esquina. Algunos radios se
   **derivan** de forma documentada y otros no existen. Inventarlos produciría un dibujo que parece
   exacto y no lo es.
4. **Quién genera la geometría.** Si la UI dibuja de una manera y el Plugin de otra, el preview miente.
   El repositorio ya vivió esa divergencia con las proyecciones de preview, que I-30 tuvo que unificar.
5. **Cómo se materializa en AutoCAD.** El catálogo vigente resuelve cada pieza a un bloque de
   `blocks-library.dwg` por `blocks.csv`. Aplicar eso aquí exigiría 983 bloques dibujados a mano, uno
   por designación, y volvería a atar la geometría a un archivo que no se versiona.

Esta decisión restringe trabajo futuro en cuatro capas y es cara de revertir, así que se registra antes
de implementar, conforme a los criterios 1 y 2 de [`adr/README.md`](README.md).

## Decisión

### Ejes y origen

1. **El eje longitudinal local es Z; la sección transversal vive en el plano XY.** Es la convención de
   toda la literatura de perfiles y la única que permite hablar de una sección sin saber cómo se
   colocará después.

2. **El origen transversal resuelto es el CENTROIDE de la sección.** Los contornos se construyen en el
   sistema que resulte natural a cada familia —el talón exterior de un ángulo, la espalda del alma de
   un canal— y se **trasladan** al final usando el `x`/`y` que la fuente tabula. Después de esa
   traslación, el centroide está en `(0,0)` por construcción, y una prueba lo comprueba sobre las 983.

3. **Los nombres «frontal», «lateral» y «planta» NO se usan en el núcleo.** Dependen de cómo un
   sistema coloque el miembro, y la sección no sabe nada de eso. El núcleo habla de **sección**,
   **longitudinal X**, **longitudinal Y**, **isométrica** y **personalizada**.

### Sección frente a instancia

4. **La sección no contiene longitud.** La longitud, la orientación, la rotación alrededor de Z y el
   espejo transversal pertenecen a una **instancia prismática** separada. Una instancia referencia una
   sección por `SectionId`; nunca la copia ni la modifica. Rotar o espejear **no** cambia el área ni el
   peso, y el peso total reutiliza la autoridad de I-36A: `WeightPerLength × Length`.

5. **No nace `StructuralMember` ni ninguno de sus parientes.** Una instancia prismática es geometría
   con longitud, no un poste, un brazo ni un larguero. El rol de miembro sigue perteneciendo a los
   configuradores futuros (ADR-0020 §2 y §8).

### Dónde se genera

6. **Toda la geometría paramétrica y toda la proyección son PURAS y viven en
   `RackCad.Application`.** Sin WPF, sin AutoCAD, sin estado global. Las primitivas genuinamente
   neutrales se añaden de forma **aditiva** a `RackCad.Application.Geometry`; lo específico de secciones
   vive en `RackCad.Application.StructuralSections.Geometry`.

7. **Existe UN plan neutral y es la autoridad.** La UI de preview y el adaptador de AutoCAD consumen
   **el mismo** plan, con las mismas curvas y los mismos roles semánticos. Ninguno de los dos recalcula
   una dimensión de la sección. Dos generadores geométricos son, por construcción, dos verdades.

### Detalle y fidelidad

8. **El detalle SOLICITADO y la fidelidad OBTENIDA son dos cosas distintas.** Se pide `Simplified` o
   `Tabulated`; se obtiene una fidelidad que el resultado **declara**: completa según los datos
   tabulados, derivada parcialmente, o simplificada por dato no disponible.

9. **Un radio se dibuja solo si la fuente lo publica o permite derivarlo de forma documentada.**
   Las dos derivaciones admitidas, ambas acreditadas contra el Readme de AISC y medidas sobre las 983
   filas:
   - **filete de raíz** de W, C y L: `r = kdes − tf` (o `kdes − t` en el ángulo), porque `kdes` va de
     la cara **exterior** del patín al pie del filete;
   - **esquina exterior del HSS**: `r = (Ht − h)/2 ≈ (B − b)/2`, porque `h` y `b` son las paredes
     **planas**; el radio interior se deriva como `r − tnom`.

10. **Cuando un dato falta, es incoherente o no es físico, la representación DEGRADA de forma explícita
    y lo declara en un diagnóstico.** Nunca inventa y nunca falla en silencio. Sobre AISC v16.0 no hay
    ninguna degradación por falta de radio, pero el camino existe, está probado y es el que sostiene
    cualquier revisión futura o fuente nueva.

11. **Ningún radio de PUNTA se inventa.** AISC redondea la punta del patín de C y L y no publica ese
    radio. Por eso el contorno tabulado de esas dos familias se declara **derivado parcialmente**, no
    completo, y su área geométrica queda por encima de la publicada. Añadir un radio plausible bajaría
    el error, y sería exactamente la clase de mentira cómoda que esta decisión prohíbe.

12. **Para HSS el contorno usa `tnom`, nunca `tdes`** (decisión 11 del dueño en I-36A). En
    consecuencia, el área geométrica **no puede** coincidir con `Properties.Area`, que AISC calcula con
    el espesor de diseño: la diferencia es de **definición**, no un defecto, y la comprobación de área
    de esa familia se hace contra `tdes` dejando constancia de ambas cifras.

### Proyección y representación

13. **La proyección es ortográfica.** Se implementan sección (mirando Z), longitudinal X, longitudinal
    Y, isométrica estándar y una vista personalizada por marco de cámara ortonormal.

14. **El resultado es WIREFRAME, sin eliminación de líneas ocultas.** Se dibujan los contornos de ambos
    extremos, las generatrices necesarias y los contornos interiores. No se ocultan aristas traseras y
    la documentación lo dice: prometer un dibujo «limpio» y entregar wireframe sería engañoso.

15. **Un arco proyectado oblicuamente NO se finge arco circular.** En vistas isométricas o arbitrarias
    la proyección de un círculo es una elipse. En vez de introducir geometría incorrecta, los arcos se
    **teselan** de forma determinista con una tolerancia de cuerda positiva y configurable, que
    conserva los extremos y refina al reducirla.

### Materialización en AutoCAD

16. **La materialización es un ADAPTADOR** en `RackCad.Plugin`, tan delgado como sea posible: recibe el
    plan neutral y crea entidades. No decide geometría.

17. **Se prohíbe un bloque de biblioteca por designación.** La representación se dibuja en un bloque
    **interno del dibujo**, creado en la inserción. No se toca `blocks-library.dwg`, no se añaden filas
    a `blocks.csv` y no se usa el nombre comercial como autoridad. Tampoco se crea una biblioteca de
    definiciones indexada por combinación sección × longitud: una longitud arbitraria no puede hacer
    crecer el dibujo sin límite.

18. **Se prohíbe un catálogo paralelo de geometría.** La geometría se **genera** desde el catálogo
    neutral de I-36A, no se tabula aparte. Un segundo catálogo se desincronizaría del primero.

19. **No hay sólidos 3D.** Ni `Region`, ni `Solid3d`, ni extrusión, ni sweep, ni modelo BIM. Se
    permiten representaciones 2D ortográficas e isométricas de tipo wireframe, y nada más.

## Alternativas consideradas

- **Origen en una esquina o en el borde designado, como publica AISC** — evita una traslación, y
  traslada el problema a cada consumidor: rotar alrededor del eje del miembro dejaría de ser rotar
  alrededor de su centro, y toda composición tendría que corregir el desplazamiento. Rechazada.

- **Longitud dentro de `StructuralSectionDefinition`** — haría trivial el primer dibujo y convertiría
  el catálogo en algo que ya no describe secciones: `W12X26` dejaría de ser una fila para pasar a ser
  una por longitud. Contradice ADR-0020. Rechazada.

- **Un bloque de AutoCAD por designación en `blocks-library.dwg`** — es el patrón que usan los cuatro
  sistemas vigentes y aquí no encaja: exigiría 983 bloques dibujados a mano, ataría la geometría a un
  archivo que no se versiona, y perdería la parametricidad que hace posible una longitud arbitraria.
  Rechazada, y prohibida explícitamente por el dueño (decisión 4).

- **Bloques dinámicos con parámetros nativos** — AutoCAD sabe estirar; el problema es que el parámetro
  pasaría a ser la autoridad de una dimensión que ya vive en el catálogo, y el repositorio ya conoce el
  coste de esa dualidad ([ADR-0011](0011-parametros-dinamicos-con-patron-array.md) existe por eso).
  Rechazada por el dueño (decisión 4).

- **Tabular la geometría en un CSV nuevo** — sería más rápido de leer y crearía un segundo catálogo que
  hay que mantener sincronizado con el primero. Rechazada.

- **Sólidos 3D y extrusión** — darían una representación superior y exigen un motor de modelado, se
  salen del alcance y contradicen la decisión 19 del dueño. Rechazada.

- **Eliminación de líneas ocultas** — mejoraría la lectura de la isométrica a cambio de un algoritmo
  caro y frágil, cuyo fallo produce dibujos sutilmente incorrectos. Se difiere; hoy se documenta que es
  wireframe.

- **Representar el arco proyectado como un arco circular** — es lo que muchas herramientas hacen y es
  falso fuera de la vista de sección. Rechazada en favor de la teselación.

## Consecuencias

- **Positivas**: la misma sección se dibuja igual en el preview y en AutoCAD porque hay un solo
  generador; una longitud arbitraria no cuesta un bloque nuevo; la fidelidad es una propiedad
  **declarada** y comprobable, no una promesa; I-37 recibe una infraestructura que consume sin duplicar
  geometría; y ni el catálogo AISC ni los sistemas vigentes se tocan.

- **Negativas / costos aceptados**: el wireframe sin líneas ocultas es menos legible que un dibujo
  resuelto; la teselación introduce un parámetro de tolerancia que hay que elegir y documentar; el área
  geométrica de C y L queda por encima de la publicada porque no se inventa el radio de punta, y la de
  HSS difiere por definición al usar `tnom`; y las primitivas nuevas de `Application.Geometry` amplían
  una carpeta que hasta ahora era mínima.

- **A vigilar**: que nadie añada un `blocks.csv` por designación «para que se vea mejor»; que la UI o el
  Plugin no empiecen a recalcular dimensiones; que la caché no se convierta en una biblioteca de
  bloques por longitud; que ninguna degradación se vuelva silenciosa; y que la instancia prismática no
  derive en un `StructuralMember` por acumulación de campos.

## Referencias

- Contrato: [`docs/initiatives/I-36B-geometria-secciones-estructurales.md`](../initiatives/I-36B-geometria-secciones-estructurales.md)
- Decisión versionada del dueño: [`docs/automation/decisions/I-36B.md`](../automation/decisions/I-36B.md)
- Evidencia: [`docs/automation/evidence/I-36B-geometria-secciones-estructurales.md`](../automation/evidence/I-36B-geometria-secciones-estructurales.md)
- [ADR-0020: Catálogo neutral de secciones estructurales](0020-catalogo-neutral-de-secciones-estructurales.md)
- [ADR-0021: Identidad, unidades y presentación](0021-identidad-unidades-y-presentacion-de-secciones.md)
- [ADR-0005: Estrategia de unidades](0005-estrategia-de-unidades.md) (la pulgada sigue siendo la unidad interna)
- [ADR-0011: Parámetros dinámicos con patrón ARRAY](0011-parametros-dinamicos-con-patron-array.md) (por qué un parámetro nativo no es autoridad)
- [ADR-0017: Validación de cargas diferida](0017-validacion-cargas-diferida-ram-elements.md) (I-36B no calcula nada)
- Guía: [`docs/guias/geometria-secciones-estructurales.md`](../guias/geometria-secciones-estructurales.md)
- Fuente: AISC Shapes Database v16.0, hoja `Readme` (definiciones de `kdes`, `h`, `b`, `x`, `y`)
