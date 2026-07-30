# ADR-0028: Cantilever visible — persistencia, registro, vistas, editor y materialización

- **Estado:** propuesto
- **Fecha:** 2026-07-29 (redacción)
- **Decisores:** Mario Pérez, Owner del repositorio (decisiones de producto emitidas al abrir I-37D);
  Claude Opus 5 (redacción)
- **Iniciativa relacionada:** I-37D `feature/cantilever-mvp-final`
- **No reemplaza a ninguna ADR.** Complementa
  [ADR-0027](0027-linea-cantilever-intervalos-y-arriostramiento.md), que decide la geometría de la línea;
  este decide **cómo se guarda, cómo se registra, cómo se ve y cómo se dibuja**. No reabre ADR-0024,
  ADR-0025 ni ADR-0026.

> **Sobre los nombres.** Este ADR decide **reglas**, no una lista de APIs. Los nombres exactos de las
> costuras vigentes —el enum de kind, el registro, los handlers, los stores, el shell, los builders de vista,
> los `[CommandMethod]`— se leen del código en la fase de auditoría de I-37D y se registran en el contrato y
> en el estado de automatización. Escribirlos aquí de memoria sería inventar procedencia, que es exactamente
> lo que ADR-0020 prohíbe para los catálogos y lo que este repositorio ya pagó una vez.

## Contexto

I-37A, I-37B e I-37C son puras y ninguna dibuja. Fue la decisión correcta —cada una entregó contratos
verificables sin arrastrar UI ni AutoCAD— y tiene una consecuencia que ahora hay que pagar de golpe: el
usuario **no ve nada**. Todo lo construido existe únicamente para las pruebas.

Hacerlo visible obliga a cruzar cuatro fronteras que las tres anteriores evitaron, y cada una tiene una forma
cómoda de cruzarla mal.

### 1. Persistencia

El repositorio ya tiene un patrón de documento versionado con campos nullable, fallback legacy y preservación
de campos desconocidos. Un sistema nuevo que invente su propio formato —o que guarde el resultado resuelto en
vez de la intención— rompe la única propiedad que hace útil un archivo: poder abrirlo dentro de un año.

### 2. Registro del sistema

Hay un registro con un handler por kind. La forma cómoda de añadir un sexto sistema es un `switch` nuevo en
cada consumidor, o un string de kind escrito a mano donde haga falta. Las dos convierten «añadir un sistema»
en «tocar N sitios y esperar acordarse de todos».

### 3. Vistas

Las vistas se pueden producir de dos maneras: un plan neutral que un materializador consume, o código que
dibuja mientras calcula. La segunda es más corta y hace imposible probar la vista sin AutoCAD.

### 4. Editor

Existe un shell visual común, adoptado ya por dos ventanas. Escribir una ventana propia sería más rápido y
dejaría a Cantilever fuera del único sitio donde la apariencia y el comportamiento están unificados.

## Decisión

### D1 — Se persiste la INTENCIÓN, versionada, y nunca el resultado

Cantilever se persiste con el patrón de documento versionado **vigente** del repositorio: DTO propio, campos
nullable con fallback legacy declarado, y preservación de los campos desconocidos que la versión actual no
entiende. Lo que se guarda es el **diseño editable** de la línea —topología, niveles, claro, altura,
overrides de celda y arriostramiento— y **nunca** la geometría resuelta: los troqueles, las elevaciones, los
cortes y las envolventes son derivados, y un archivo que los guardara guardaría una respuesta que sus propias
entradas ya determinan.

El round-trip es **obligatorio y determinista**:

```
Design → serialize → deserialize → resolve → la MISMA firma
```

Una versión mayor incompatible **falla cerrado**, con el precedente que I-11 fijó: se aborta con diagnóstico
en vez de abrir a medias.

Los **overrides de celda** viajan en el documento. Una persistencia que los omitiera devolvería una línea
uniforme donde el usuario había puesto brazos distintos, y sin decirlo.

### D2 — Un solo registro, y ningún string de kind fuera de él

Cantilever se añade al **registro de sistemas vigente** como un kind más, con su descriptor y sus handlers de
creación, serialización, construcción, validación y copia profunda. **Ningún string de kind vive fuera del
registro**: el valor se resuelve por el registro o no se resuelve, y un kind sin handler es un **error
visible** —la regla que el fix de kind-handler ya estableció— y no un silencio.

La lista de diseños y la biblioteca lo reconocen por el mismo camino que los cinco sistemas vigentes, sin una
rama especial para él.

### D3 — Las vistas son PLANES puros, y el dibujo vive sólo en el Plugin

Las tres vistas obligatorias —**frontal**, **lateral** y **planta**— se producen como **planes de
representación deterministas** en Application, y un materializador **del Plugin** los consume. Domain y
Application **no dibujan**: es la dirección de dependencias que AGENTS fija, y es también lo que permite
probar una vista sin AutoCAD y comparar dos planes por igualdad.

La **frontal** proyecta la línea completa en X-Z: columnas, placas, separadores, los tensores en X, los
troqueles según la convención vigente, y los brazos por su conexión. La **planta** proyecta en X-Y:
estaciones, bases, brazos, separadores, placas, y la diferencia entre góndola sencilla y doble. La **lateral**
es Y-Z de **una** estación seleccionada, y el editor guarda cuál —con default la primera— porque una línea con
overrides tiene estaciones que hay que poder mirar por separado.

Cada vista se deriva de la estación **resuelta y colocada**, nunca de su diseño editable: dos fuentes para una
misma vista es cómo el dibujo empieza a discrepar del BOM.

La geometría de cada cosa viene de su autoridad: los perfiles de los contornos de I-36, las placas y
cartabones de sus planes, el cold rolled de su eje y su diámetro, los troqueles de su datum y su diámetro. La
**X de los tensores va en el mismo plano**, sin offset visual falso — la misma decisión que ADR-0027 D6 tomó
para la geometría, sostenida en el dibujo.

### D4 — El editor va sobre el shell visual común

Cantilever se edita en el **shell visual común**, con el precedente de las dos ventanas que ya lo adoptaron, y
reutiliza los controles compartidos en vez de duplicarlos.

La matriz es **estación × nivel × lado**, con los alcances **celda**, **nivel** —ese nivel en todas las
estaciones y lados activos—, **estación** —todos los niveles y lados de una columna— y **todo**. En góndola
sencilla el lado inactivo **no existe** como celda, y aplicar un valor igual al default **guarda `null`**:
las dos reglas son de ADR-0026 D3 y aquí sólo se extienden a la tercera dimensión.

**Una operación de matriz produce UNA notificación y UNA regeneración.** No hay regeneración por celda: es el
coste O(N) que I-15 midió y quitó, y volver a introducirlo haría que un «aplicar a todo» sobre cuatro
estaciones y cinco niveles hiciera cuarenta veces el trabajo de una edición que el usuario vive como una.

### D5 — La materialización reutiliza el Drawing vigente, y no copia otro sistema

El comando de Cantilever sigue la convención real del repositorio, y la materialización reutiliza el catálogo
de bloques, la colocación, el dibujo por vista, el regen canónico y la persistencia por Xrecord que ya
existen. Debe soportar **insertar**, **redibujar en sitio**, **editar un diseño existente**, **regenerar
vistas**, **conservar la identidad del documento**, **reconstruir desde la persistencia** y **producir el
BOM** — el mismo contrato de comportamiento que los cinco sistemas vigentes cumplen.

**No se copia código de Selectivo, Dinámico ni Push Back.** Si aparece una responsabilidad realmente
compartida se **extrae**, con caracterización previa; si no, se escribe la de Cantilever. Copiar es cómo dos
sistemas empiezan a divergir en silencio.

### D6 — El gate es el Owner en AutoCAD, y no el CI

I-37D es la primera de la línea que cambia dibujo e interfaz, así que **su gate no se resuelve sobre el
código**. Exige DLL Debug del worktree, bundle, instrucciones exactas de `NETLOAD`, checklist y el **veredicto
manual del Owner en AutoCAD 2025**. CI verde es necesario y **no** suficiente: las pruebas no ven los bloques
DWG reales, que es precisamente lo que AGENTS registra como criterio final.

Sin ese veredicto **no se integra** y **I-37 no se cierra**.

## Alternativas consideradas

**Persistir el resultado resuelto** para abrir más rápido. Descartada: guardaría una respuesta derivada de sus
propias entradas, y quedaría obsoleta en cuanto una regla cambiara.

**Un formato propio para Cantilever.** Descartada: el patrón versionado vigente ya resuelve compatibilidad
legacy y campos desconocidos, y un segundo formato duplica ese trabajo y sus bugs.

**Un `switch` por kind en cada consumidor.** Descartada por el fix de kind-handler: el registro existe para
que añadir un sistema sea un registro y no una búsqueda.

**Dibujar directamente desde Application.** Descartada: rompe la dirección de dependencias de AGENTS y hace
imposible probar una vista sin AutoCAD.

**Una ventana propia fuera del shell.** Descartada: dejaría a Cantilever fuera del único sitio donde la
apariencia y el comportamiento están unificados, justo después de dos iniciativas dedicadas a unificarlos.

**Copiar el Drawing de Push Back y adaptarlo.** Descartada: es la vía rápida a dos sistemas que divergen. Se
extrae lo compartido o se escribe lo propio.

## Consecuencias

**Positivas.** Cantilever pasa a ser un sistema de primera clase: se guarda, se abre, se lista, se edita, se
dibuja en tres vistas y se cotiza, por los mismos caminos que los cinco vigentes. Los planes de vista puros
hacen que el dibujo sea **probable sin AutoCAD**, que es lo que permite tener goldens. Y el editor sobre el
shell significa que la próxima mejora del shell llega a Cantilever sin tocarlo.

**Negativas, y asumidas.** Es la primera subiniciativa de la línea que **no** puede cerrarse sobre el código:
depende de una persona delante de AutoCAD, y eso alarga su ciclo. Toca UI y Plugin, así que el riesgo de
regresión sobre los cinco sistemas vigentes es real y se paga con guardas y con la suite completa. Y el
alcance es grande —línea, arriostramiento, persistencia, registro, tres vistas, editor y materialización—, lo
que lo hace la subiniciativa más difícil de revisar de las cuatro; se acepta porque partirlo más dejaría al
producto invisible durante más tiempo, que es el estado que I-37D existe para terminar.

**Lo que este ADR NO decide.** Cálculo resistente, cargas, capacidad, peso, costo, optimización, soldaduras,
tornillería, anclas, roscas, tolerancias, preparación de extremos, CNC, shop drawings, la interferencia física
en el cruce de tensores, y cualquier catálogo nuevo sin procedencia. Siguen fuera **incluso al cerrar I-37**.

## Referencias

- [ADR-0026](0026-estacion-cantilever-niveles-altura-y-bom.md) — la estación que la línea compone.
- [ADR-0027](0027-linea-cantilever-intervalos-y-arriostramiento.md) — la geometría de la línea.
- [Contrato de I-37D](../initiatives/I-37D-cantilever-mvp-final.md).
- [Decisiones del Owner para I-37](../automation/decisions/I-37.md).
- [Guía de validación manual en AutoCAD](../guias/validacion-manual-autocad.md).
