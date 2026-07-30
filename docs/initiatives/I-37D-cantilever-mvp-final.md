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

## 10. Validacion manual

**OBLIGATORIA.** `requires_autocad: true`, `requires_owner_validation: true`. I-37D cambia dibujo e interfaz,
asi que el gate es el **veredicto del Owner en AutoCAD 2025** sobre el DLL Debug del worktree, cargado por
`NETLOAD`, con su bundle y su checklist. Sin ese veredicto **no se integra** y **I-37 no se cierra**.

El checklist debe cubrir: gondola sencilla y doble; 2 y 4 estaciones; las tres vistas; matriz y alcances;
paneles estandar; el caso normativo de 264 in; tensor cold rolled; tensor estructural; BOM; guardar, cerrar y
reabrir; y editar y redibujar.

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
