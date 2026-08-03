---
schema: rackcad-initiative/v1
id: I-37D
title: Cantilever MVP final - linea, arriostramiento, vistas y editor
type: feature
status: implementing
branch: feature/cantilever-mvp-final
base_branch: main
priority:
size: XL
depends_on: [I-36A, I-36B, I-36C, I-36D, I-37A, I-37B, I-37C, I-14, I-15, I-30, I-31]
conflicts_with: []
context_packs: [architecture-kernel, catalogs-data, ui-editors, persistence, autocad-plugin, documentation-governance, delivery-validation]
automation_state_path: docs/automation/state/I-37D.yml
decision_paths: [docs/automation/decisions/I-37.md]
requires_ci: true
requires_plugin_build: true
requires_autocad: true
requires_owner_decision: false
requires_owner_validation: true
automation:
  enabled: true
  auto_merge: false
  max_attempts: 3
---

# Cantilever MVP final - linea, arriostramiento, vistas y editor

> **Cuarta y ULTIMA subiniciativa de I-37.** I-37A fundo columna y base, I-37B el brazo, I-37C la estacion
> con su BOM. Las tres son puras y **ninguna dibuja**: el usuario todavia no ve nada. I-37D entrega
> exactamente lo que falta para que el producto sea **visible y utilizable**.
>
> **Es la PRIMERA de la linea que cambia UI y AutoCAD.** Su gate NO se resuelve sobre el codigo: exige DLL,
> bundle, `NETLOAD` y el **veredicto manual del Owner en AutoCAD 2025**. Sin el no se integra, y I-37 no se
> cierra.
>
> **No recalcula ninguna geometria de I-37A, I-37B ni I-37C.** Los compone.

## 1. Objetivo

Que sea posible, de punta a punta:

1. disenar una **linea** de dos o mas estaciones con separacion entre centros de columna;
2. compartir topologia, niveles, claro y **altura comun** entre todas;
3. resolver los **intervalos** entre estaciones adyacentes;
4. distribuir los **paneles arriostrados** con la regla de producto aprobada;
5. colocar las **placas de columna** de separador en la cara correcta;
6. resolver los **separadores** con su corte derivado de los agujeros de esas placas;
7. resolver los **tensores** en X, estructurales o **cold rolled** con adaptadores y cartabones;
8. producir el **BOM completo** por componentes: columna-base, brazo, separador y tensor;
9. **persistir** el diseno con round-trip determinista;
10. **registrar** el sistema en la arquitectura vigente;
11. producir planes puros de vista **frontal, lateral y planta**;
12. editar la linea en el **shell visual comun**, con matriz **estacion x nivel x lado**;
13. **materializar** en AutoCAD con su comando y su flujo completo;
14. hacerlo sin calculo resistente, capacidad, peso, costo ni fabricacion detallada.

## 2. Problema

El producto sabe resolver piezas y una estacion, y no sabe formar un rack ni mostrarlo. Al componer la linea
aparecen cinco preguntas que la estacion no tenia -- de quien son los separadores, cuantos paneles lleva una
columna, donde caen, que mide el corte de un separador y que es un tensor -- y las cinco se responden en
[ADR-0027](../adr/0027-linea-cantilever-intervalos-y-arriostramiento.md).

Y aparecen cuatro mas que ninguna subiniciativa anterior toco, porque las tres eran puras: como se persiste,
como se registra, como se dibuja y como se edita. Esas se responden en
[ADR-0028](../adr/0028-cantilever-persistencia-vistas-editor-y-dibujo.md).

## 3. Alcance

- **Domain**: diseno de linea, topologia de estacion, arriostramiento, separador, tensor y adaptador; el
  documento versionado del sistema.
- **Application**: autoridad de distribucion vertical de paneles; resolver de linea con su altura comun
  verificada; resolver de intervalo; placas de columna de separador; separador; panel arriostrado; tensores y
  adaptadores; matriz de linea; BOM de linea; planes de representacion frontal, lateral y planta;
  persistencia y registro del sistema.
- **UI**: ventana de edicion sobre el **shell visual comun**, con matriz estacion x nivel x lado, reutilizando
  los controles de I-14.
- **Plugin**: comando del sistema, materializacion de las tres vistas, insercion, redibujo en sitio, edicion
  de un diseno existente y round-trip por Xrecord.
- **Extension aditiva** de los enums, tokens y registros vigentes. Ningun valor existente cambia de nombre ni
  de numero.
- **Pruebas** en `tests/RackCad.Tests` y `tests/RackCad.UI.Tests`, con guardas de fuente y regresiones
  verificadas en rojo.
- **Documentacion**: este contrato, ADR-0027, ADR-0028, el estado de automatizacion, la fila del ROADMAP, la
  decision versionada de I-37, el glosario, y el **paquete de validacion manual** con instrucciones exactas
  de `NETLOAD` y checklist.

## 4. Fuera de alcance

Cada uno es **condicion de detencion**, y siguen fuera **incluso al cerrar I-37**:

calculo resistente; cargas; capacidad; I-38; peso; costo; optimizacion; soldaduras; tornillos y tuercas;
anclas; roscas; tolerancias; preparacion de extremos; CNC; shop drawings; **la interferencia fisica en el
cruce de tensores**; y cualquier catalogo nuevo sin procedencia.

**No se corrigen** los hallazgos adyacentes de `docs/ideas-futuras.md`.

## 5. Contexto requerido

`AGENTS.md`; `docs/WORKFLOW.md`; `docs/ARCHITECTURE.md`; `docs/guias/agregar-un-sistema.md`;
`docs/guias/glosario.md`; `docs/guias/validacion-manual-autocad.md`;
`docs/guias/catalogos-y-plantillas.md`; ADR-0020 a **ADR-0028**;
`docs/automation/decisions/I-37.md`; y los contratos, estados y cierres de I-37A, I-37B e I-37C.

Context Packs: `architecture-kernel`, `catalogs-data`, `ui-editors`, `persistence`, `autocad-plugin`,
`documentation-governance`, `delivery-validation`.

**Auditoria obligatoria de costuras, con nombres EXACTOS leidos del codigo y nunca inferidos**, antes de
escribir una linea en cada area: el registro de sistemas y sus handlers por kind; el patron de documento
versionado y sus stores; el shell de editor y los controles de I-14; los builders de vista y la frontera
plan -> materializador; los `[CommandMethod]` y el flujo menu -> modal -> accion tipada -> comando; el
catalogo para el canal de 4 in ligero y para una tabla de calibres si existe; y las guardas de fuente
vigentes sobre dibujo, AutoCAD, kind y regen.

## 6. Dependencias

I-36A a I-36D, **I-37A**, **I-37B** e **I-37C** integradas en `origin/main` (merge `250d469`), con
**ADR-0024, ADR-0025 y ADR-0026 aceptados**. Ademas I-14 (controles), I-15 (shell de editor) y I-30/I-31
(precedentes de adopcion del shell). No estorba con ninguna iniciativa abierta.

## 7. Contratos congelados

**No se reabren** ADR-0024, ADR-0025 ni ADR-0026. I-37D **compone**:
`CantileverColumnBaseAssembly`, `CantileverArmAssembly`, `CantileverStationAssembly`,
`CantileverColumnRegularPunchGrid`, `CantileverArmConnectionMetricsResolver`,
`CantileverStationBomBuilder`, la geometria neutral de I-36, el shell visual comun, y los registros,
persistencia y Drawing existentes.

**No recalcula** la geometria de columna, base, brazo ni estacion.

## 8. Fases

1. Reclamo.
2. Contrato, ADR-0027, ADR-0028, ROADMAP, decision versionada y estado.
3. Auditoria de costuras con nombres exactos.
4. Contratos de Domain: linea, arriostramiento, separador, tensor, adaptador.
5. Autoridad de distribucion vertical de paneles.
6. Resolver de linea con altura comun verificada, e intervalos.
7. Placas de columna de separador y separadores.
8. Paneles arriostrados, tensores y adaptadores.
9. Matriz de linea y BOM completo.
10. Persistencia y registro del sistema.
11. Planes de representacion frontal, lateral y planta.
12. Editor sobre el shell visual comun.
13. Materializacion en AutoCAD, comando y flujo.
14. Pruebas, regresiones en rojo y guardas.
15. Paquete de validacion manual y evidencia final.

## 9. Pruebas y builds

Builds Debug de Domain, Application, UI y Plugin con cero errores propios; pruebas focalizadas de I-36,
I-37A, I-37B, I-37C **e** I-37D; guardas de fuente; BOM; persistencia; registro; UI; Drawing; suites
completas de `RackCad.Tests` y `RackCad.UI.Tests`; **bundle**; CI verde en la rama.

Regresion **verificada fallando** para, al menos: duplicar el separador de una frontera **horizontal**
compartida entre dos segmentos verticales consecutivos del **mismo** intervalo -- dos intervalos adyacentes
tienen separadores fisicamente distintos aunque coincidan en elevacion, y esos **no** se fusionan--; contar
placas como componentes; usar centro a centro como corte directo; omitir el 1.25; invertir el espejo derecho;
usar otro diametro; contar un panel vacio como arriostrado; repartir el espacio central en los externos;
comprimir paneles; dibujar los tensores en planos distintos; anadir union central; omitir adaptadores; omitir
cartabones; duplicar la columna al construir la linea; resolver alturas distintas por estacion; calcular el
BOM desde el diseno; una firma que omite el arriostramiento; persistencia que omite overrides; y una vista
que recalcula geometria.

## 9b. Ronda 2 de correccion visual y funcional

La validacion manual de la **ronda 1** quedo **RECHAZADA**
(`OWNER_REJECTED_I_37D_MANUAL_VALIDATION_ROUND_1`) por seis motivos, y los seis son de **arquitectura
visual y de flujo**, no de geometria resuelta: ventana saturada, propiedades de linea mezcladas con
internas de componentes, perfiles dificiles de seleccionar, la base que no seguia a la columna, troqueles y
placas que no se dibujaban, y una arquitectura que no reflejaba el flujo real de configuracion.

La ronda 2 **reestructura el producto** antes de revisar en detalle cada componente:

- la ventana principal edita **solo** el agregado `CantileverLineDesign`;
- cada componente se edita en su **propia subventana**, sobre un shell comun, con sus parametros, su
  preview, sus diagnosticos, su receta y su insercion independiente;
- un **selector estructural buscable** sustituye al ComboBox en los cuatro configuradores;
- `BaseFollowsColumn` es **intencion persistida** y nunca una comparacion de ids;
- la representacion dibuja **todos** los agujeros resueltos.

La **insercion independiente** sigue el precedente de `RACKSECCION`: sin `RackSystemKind` nuevo, sin
handler de edicion, sin payload y con identidad propia. El bloque queda identificado y **no editable**, y
eso se declara en el paquete manual en vez de prometerse.

## 9c. Correccion de columna y base

La validacion manual de la **ronda 2** quedo **RECHAZADA en columna y base**
(`OWNER_REJECTED_I_37D_MANUAL_VALIDATION_ROUND_2_COLUMN_BASE`); el resto de la ronda 2 no fue objetado.
Cinco motivos, y **tres de ellos geometricos y medibles**, cosa que la ronda 1 no tuvo:

1. **Dos margenes de troquel sin utilidad de producto.** Retirados de la autoridad y de la interfaz. El
   troquel se acota por su **radio**: un agujero entero cabe o no cabe. Las dos propiedades del DTO se
   conservan marcadas LEGACY porque la API de I-37A esta integrada en `main`, y el JSON deja de escribirlas.
2. **Geometria de la planta incorrecta.** La base salia como **una linea de 6.49 in con longitud CERO**. La
   causa estaba una capa mas abajo: se le preguntaba a la **camara** si miraba a lo largo del eje Z *del
   mundo*, y lo que decide si una seccion conserva su forma es el eje del **miembro**. Una camara cenital
   conserva la forma de una columna de pie y no la de una base tumbada. Para una vista expresada en ejes de
   seccion el eje del miembro **es** Z, asi que `RACKSECCION` no cambia en nada.
3. **La lateral omite placas y cartabon.** No las omitia: las dibujaba con **espesor cero**, por proyectar
   el contorno de una sola cara. Ahora se dibuja la silueta del **solido**. Vista de frente la cara lejana
   cae sobre la cercana y la silueta **es** el contorno, asi que nada cambia donde ya estaba bien.
4. **La columna arranca en el piso.** Decision normativa del Owner: **la base se queda en el piso** y solo
   sube la columna, que arranca en la cara superior de su placa inferior. Base y columna comparten el datum
   **logico** de conexion, no el mismo origen fisico en Z — precisado en la nota **N1** de ADR-0024. La
   longitud **nominal** de corte y el BOM comercial no cambian: la placa levanta la columna, no la alarga.
5. **Naturalezas fisicas indistinguibles.** Todo salia BYBLOCK en la capa 0. Application gana el vocabulario
   de **roles visuales** —con `Annotation` declarado antes de que nada lo emita— y dos adaptadores lo
   consumen: color en la previa, capa BYLAYER en el dibujo.

La **correccion arquitectonica** que el Owner exigio esta aplicada: una sola variable ya no significa piso,
inicio de base e inicio de columna a la vez. El ejercicio de las quince regresiones destapo ademas **dos
defectos vivos** —el arranque de la columna se calculaba en dos sitios, y `ColumnTopZ` se habia quedado sin
consumidor—, los dos corregidos.

**Brazo, separador y tensor no se modificaron.**

## 9d. Ronda 4 — el adaptador fisico y el editor avanzado de paneles

El Owner **revoco** la aproximacion con la que se situaba el agujero de varilla del adaptador y **autorizo**
el editor avanzado de paneles, que la ronda 3 habia dejado fuera de alcance.

**El adaptador.** Queda revocada `RodHoleAxialOffset = CutLength / 2`. Su justificacion escrita —«medio
corte, porque las dos caras son perpendiculares»— es justamente la que no se sostiene: si son
perpendiculares, la separacion entre dos agujeros centrados **cada uno en su ala** tiene componente en los
dos ejes, no en uno. El adaptador pasa a ser un **prisma real** de `AISC-L-L2X2X3_16` cortado a 2 in,
proyectado por la **tuberia estructural comun**, y sus dos agujeros estan en el **plano medio real** de su
ala: separacion `(0.820358, -0.906250, 0.385099)`, modulo `1.281631 in`, **DeltaY distinto de cero**.

Como el centro del agujero de varilla es el **datum fisico** del extremo del tensor, su longitud nominal
pasa de `92.131526` a `92.319026 in` — **crece 0.1875 in, el espesor del angulo**, porque la aproximacion
media hasta la **cara** del ala y la fisica mide hasta su **plano medio**. **El BOM cambia con ella**, y es
legitimo: las cantidades no se mueven, solo la longitud que se ordena.

**La consecuencia visual, declarada y no disimulada.** El eje de corte del adaptador queda **perpendicular a
la diagonal y dentro del plano del panel** — la unica orientacion en la que el agujero del ala del tensor
tiene por eje la propia varilla, que es como se sujeta una varilla roscada. Ninguna de las tres camaras de la
linea mira por ese eje, asi que **el adaptador no se lee como una L en ninguna vista de linea**. El Owner
decidio que **ninguna vista de la linea se deforma** para disimularlo y que el configurador de tensor gana
una vista propia, **«Seccion del adaptador»**, que mira por ese eje y consume la **misma**
`StructuralSectionGeometry` que el prisma.

**Los paneles.** La secuencia vertical admite dos modos, `Automatic` y `Advanced`, y **los dos producen la
misma lista efectiva**, que es la **unica entrada** del resolver posterior. Un vacio es un **tramo
explicito** con los tensores apagados; los separadores salen de `Distinct` sobre las fronteras y los tensores
de los tramos `CrossBraced`. La lista cubre el **nucleo** y no la columna entera: si los espacios externos
fueran tramos, se pondria un separador en el piso y otro en la punta, y eso no es el producto.

**El modo automatico no cambio el producto**: linea, BOM y las seis vistas siguen en su pin.

## 9d. Ronda 4 — el adaptador fisico y el editor avanzado de paneles

El Owner **revoco** la aproximacion con la que se situaba el agujero de varilla del adaptador y **autorizo**
el editor avanzado de paneles, que la ronda 3 habia dejado fuera de alcance.

**El adaptador.** Queda revocada `RodHoleAxialOffset = CutLength / 2`. Su justificacion escrita —«medio
corte, porque las dos caras son perpendiculares»— es justamente la que no se sostiene: si son
perpendiculares, la separacion entre dos agujeros centrados **cada uno en su ala** tiene componente en los
dos ejes, no en uno. El adaptador pasa a ser un **prisma real** de `AISC-L-L2X2X3_16` cortado a 2 in,
proyectado por la **tuberia estructural comun**, y sus dos agujeros estan en el **plano medio real** de su
ala: separacion `(0.820358, -0.906250, 0.385099)`, modulo `1.281631 in`, **DeltaY distinto de cero**.

Como el centro del agujero de varilla es el **datum fisico** del extremo del tensor, su longitud nominal
pasa de `92.131526` a `92.319026 in` — **crece 0.1875 in, el espesor del angulo**, porque la aproximacion
media hasta la **cara** del ala y la fisica mide hasta su **plano medio**. **El BOM cambia con ella**, y es
legitimo: las cantidades no se mueven, solo la longitud que se ordena.

**La consecuencia visual, declarada y no disimulada.** El eje de corte del adaptador queda **perpendicular a
la diagonal y dentro del plano del panel** — la unica orientacion en la que el agujero del ala del tensor
tiene por eje la propia varilla, que es como se sujeta una varilla roscada. Ninguna de las tres camaras de la
linea mira por ese eje, asi que **el adaptador no se lee como una L en ninguna vista de linea**. El Owner
decidio que **ninguna vista de la linea se deforma** para disimularlo y que el configurador de tensor gana
una vista propia, **«Seccion del adaptador»**, que mira por ese eje y consume la **misma**
`StructuralSectionGeometry` que el prisma.

**Los paneles.** La secuencia vertical admite dos modos, `Automatic` y `Advanced`, y **los dos producen la
misma lista efectiva**, que es la **unica entrada** del resolver posterior. Un vacio es un **tramo
explicito** con los tensores apagados; los separadores salen de `Distinct` sobre las fronteras y los tensores
de los tramos `CrossBraced`. La lista cubre el **nucleo** y no la columna entera: si los espacios externos
fueran tramos, se pondria un separador en el piso y otro en la punta, y eso no es el producto.

**El modo automatico no cambio el producto**: linea, BOM y las seis vistas siguen en su pin.

## 10. Validacion manual

**OBLIGATORIA.** `requires_autocad: true`, `requires_owner_validation: true`. I-37D cambia dibujo e interfaz,
asi que el gate es el **veredicto del Owner en AutoCAD 2025** sobre el DLL Debug del worktree, cargado por
`NETLOAD`, con su bundle y su checklist. Sin ese veredicto **no se integra** y **I-37 no se cierra**.

El checklist debe cubrir: gondola sencilla y doble; 2 y 4 estaciones; las tres vistas; matriz y alcances;
paneles estandar; el caso normativo de 264 in; tensor cold rolled; tensor estructural; BOM; guardar, cerrar y
reabrir; y editar y redibujar.

### EJECUTADA Y APROBADA — 2026-08-03

**`OWNER_APPROVED_I37D_MANUAL_VALIDATION`.** Todo funciona correctamente, **sin defectos bloqueantes
observados**.

| Campo | Valor |
|---|---|
| `CODE_SHA` | `dd9e4a5` |
| `VALIDATED_BUILD_SHA` | `a594eb5` |
| DLL Debug SHA-256 | `F237CC7951A398751C369FB64A0A6FF541F80E37E39C4375905D2AE98985B6E1` |
| Suites | 2978 core · 621 UI |
| Bundle | Release verificado, 153 comprobaciones, inventario de 24 archivos |
| CI | `30674507385` sobre `4e4e6d9` |
| Paquete | [validacion de paneles avanzados y adaptador](../automation/evidence/I-37D-autocad-validation-advanced-panels-and-adapter.md) |

Con este veredicto **ADR-0027 y ADR-0028 quedan aceptados**. El gate de I-37D **nunca se resolvio sobre el
codigo**: por eso cuatro rondas de CI verde no bastaron.

**INTEGRADA en `main` el 2026-08-03** con merge `--no-ff` **`fa7f8c5`** (padres `250d469` y `a973c7b`),
suites postmerge 2978 + 621 y **CI `30830468566` verde en sus cuatro jobs**. Con ella **se cierra I-37**:
sus cuatro subiniciativas estan integradas.

## 11. Criterios de aceptacion

Los catorce puntos del objetivo, cubiertos por prueba; las doce filas de la tabla de paneles reproducidas por
la regla; el caso de 264 in exacto; los conteos de 4 estaciones (3 intervalos, 18 separadores, 24 tensores);
round-trip determinista; las tres vistas con planes deterministas; el editor sobre el shell sin regeneracion
por celda; el comando y el flujo completos en AutoCAD; guardas activas; y **cero** cambios de comportamiento
en I-36, I-37A, I-37B, I-37C y los cinco sistemas vigentes.

## 12. Condiciones para detenerse

Necesitar un valor sin default aprobado; necesitar un id de catalogo que no existe **de forma inequivoca**
-- una tabla de calibres, por ejemplo --; necesitar reabrir ADR-0024, ADR-0025 o ADR-0026; que la altura
comun mueva un indice de nivel; o que el pase final de una estacion difiera de su layout.

El **canal de 4 in ligero ya no dispara esta condicion**: `DefaultSeparatorSectionId = AISC-C-C4X4_5` es
una decision **CERRADA y vinculante** (12.51). El usuario puede elegir otra seccion explicitamente, pero el
default no esta pendiente.

## 13. Estado versionado y entrega del Pull Request

`docs/automation/state/I-37D.yml`. Sin Pull Request; el merge automatico esta prohibido.

## 14. Evidencia final

Commits de la rama, archivos, pruebas, builds, bundle, CI, el paquete de validacion manual con el SHA del DLL,
y confirmacion de que `main` no fue modificada.
