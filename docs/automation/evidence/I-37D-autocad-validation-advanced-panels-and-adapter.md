# I-37D — Validación manual · **RONDA 4: adaptador físico y editor avanzado de paneles**

Estado: **PENDIENTE DE EJECUTAR**. Los paquetes anteriores se conservan sin reescribir:
[ronda 1](I-37D-autocad-validation.md) · [ronda 2](I-37D-autocad-validation-round-2.md) ·
[columna y base](I-37D-autocad-validation-round-2-column-base-fix.md) ·
[ronda 3, puntos 1–6](I-37D-autocad-validation-round-3.md) ·
[ronda 3, punto 7](I-37D-autocad-validation-round-3-braces.md) ·
[ronda de corrección](I-37D-autocad-validation-round-3-correction.md).

Esta ronda trae **los paneles**, que las dos anteriores dejaron fuera por decisión del dueño, y la revocación
de la aproximación con la que se situaba el agujero de varilla del adaptador.

---

## 1. Identificación

| Campo | Valor |
|---|---|
| Rama | `feature/cantilever-mvp-final` |
| **CODE_SHA** | `dd9e4a5` |
| **VALIDATED_BUILD_SHA** | `a594eb5d5e2cd703cf9063b36147cddc0042302b` |
| **DLL SHA-256** | `F237CC7951A398751C369FB64A0A6FF541F80E37E39C4375905D2AE98985B6E1` |
| Tamaño / fecha | 135 680 bytes · `2026-07-31 18:01:32` |
| Inventario del bundle | [`I-37D-round-4-bundle-inventory.txt`](I-37D-round-4-bundle-inventory.txt) — **nuevo**, 24 archivos |
| Suites | `RackCad.Tests` **2978/2978** · `RackCad.UI.Tests` **621/621** |
| Regresiones | **14/14 en rojo** — [evidencia](I-37D-round-4-regressions.md) |
| CI | [`30672070159`](https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/30672070159) ✅ sobre `3e9859a` |
| Bundle | ✅ Release, AutoCAD cerrado, fail-closed, **153 comprobaciones**, cero DLL de Autodesk |

## 2. Trazabilidad — **léelo antes de cargar nada**

Los tres identificadores están **separados a propósito**, porque no son el mismo:

| | Valor | Qué es |
|---|---|---|
| `CODE_SHA` | `dd9e4a5` | La última punta que tocó `src` o `tests`. **El árbol funcional validado es éste.** |
| `VALIDATED_BUILD_SHA` | `a594eb5` | Ése más **dos commits exclusivamente documentales**. El DLL se compiló de aquí. |
| HEAD documental | posterior | El commit que registra estas huellas, necesariamente después del build. |

El DLL sale de `a594eb5` y no de `dd9e4a5` porque la `AssemblyInformationalVersion` **incrusta la punta de
git**: recompilar tras un commit documental cambia el binario aunque el código sea byte a byte el mismo. Por
eso el inventario del bundle se generó **de cero** y no se reutilizó el de la ronda anterior — los tres DLL
de RackCad cambiaron de huella por esa razón y por ninguna otra.

**Antes de validar:**

1. Cierra AutoCAD. El bundle **aborta** si está abierto, y es a propósito.
2. Carga el DLL Debug del worktree, que ahora vive en
   `D:/Documentos/Codex/worktrees/feature-cantilever-mvp-final`.
3. Comprueba que su SHA-256 es el de la tabla de §1. **Si no coincide, alguien recompiló después**: vuelve a
   anotar el que cargues antes de seguir.

---

## 3. Lo que esta ronda cambió, en una línea cada cosa

- El **adaptador** dejó de ser un contorno dibujado a mano y es un **prisma real** de `AISC-L-L2X2X3_16`.
- Sus **dos agujeros** están centrados **cada uno en su propia ala**, en el plano medio real de esa ala.
- El **agujero de varilla es el datum físico** del extremo del tensor, que por eso **mide 0.1875 in más**.
- El **BOM cambia** con él. Las cantidades no.
- La **secuencia vertical de paneles** admite modo **avanzado**, declarado tramo a tramo.

### Lo que hay que mirar con más cuidado

> **El adaptador NO se lee como una L en ninguna de las tres vistas, y es correcto.** Su eje de corte corre
> perpendicular a la diagonal **dentro del plano del panel**, así que la frontal ve el ala apoyada de frente
> y la del tensor de canto. Ninguna cámara mira por el eje de corte.
>
> Ninguna vista se deformó para disimularlo (decisión 14.6). Para verlo como L está la vista
> **«Sección del adaptador»** del configurador de tensor (decisión 14.7), que **ya está implementada** y
> tiene su propio bloque de comprobaciones abajo.

---

## Bloque A — El adaptador

| # | Comprobación | ✅/❌ |
|---|---|---|
| A1 | La **frontal** dibuja cada extremo de tensor como una figura **física**, sin rotación artificial ni forma forzada | ☐ |
| A2 | Los **cuatro extremos** de las dos diagonales de un panel salen **distintos** entre sí: ninguno parece copiado ni girado al azar | ☐ |
| A3 | Los dos adaptadores de **una misma** diagonal se leen como **espejos**, no como la misma pieza repetida | ☐ |
| A4 | Se ven **dos agujeros por adaptador** y están en **alas distintas**: no comparten ala ni caen uno sobre otro | ☐ |
| A5 | El agujero que bolta al **separador** coincide con el troquel de tensor del separador. No hay dos círculos desalineados | ☐ |
| A6 | El **tensor arranca y termina en el centro del agujero de varilla**, no en el perno del separador | ☐ |
| A7 | El tensor **no atraviesa** el cuerpo del adaptador: entra por su agujero y sale | ☐ |
| A8 | Los **dos cartabones** de cada adaptador se ven **separados** en la frontal, uno a cada extremo del corte | ☐ |
| A9 | La **previa de la ventana** y lo que AutoCAD dibuja son **la misma figura** | ☐ |

> **A8 cambió respecto de la ronda 3.** Antes los dos cartabones caían uno sobre otro en la frontal, porque
> el corte del adaptador corría a lo largo de Y. Ahora corre dentro del plano del panel, así que **se ven
> aparte**. Si los ves superpuestos, el eje de corte volvió a su orientación vieja.

### A10 — La longitud del tensor y el BOM

Sobre el caso normativo de cuatro estaciones con altura manual 264:

| Magnitud | Antes | Ahora |
|---|---|---|
| perno a perno | 94.131526 | 94.131526 |
| **longitud del tensor** | 92.131526 | **92.319026** |
| diferencia | — | **+0.187500 = 3/16 in, el espesor del ángulo** |

| # | Comprobación | ✅/❌ |
|---|---|---|
| A10 | La línea de BOM de la varilla trae la **longitud nueva** | ☐ |
| A11 | Las **cantidades no cambiaron**: 4 varillas, 8 adaptadores, 16 cartabones por intervalo del caso normativo | ☐ |

---

## Bloque B — Paneles automáticos: que **nada** haya cambiado

El modo automático es el de por omisión y **no debe haberse movido un milímetro**. Este bloque es una
regresión, no una novedad.

| # | Comprobación | ✅/❌ |
|---|---|---|
| B1 | Con el caso normativo (4 estaciones, altura manual 264) la distribución vertical se ve **igual que en la ronda anterior**: 32 · 40 · 40 · 40 vacío · 40 · 40 · 32 | ☐ |
| B2 | **6 separadores por intervalo**, 18 en la línea | ☐ |
| B3 | **8 tensores por intervalo**, 24 en la línea | ☐ |
| B4 | El **espacio central vacío** sigue donde estaba, y sigue **sin** tensores | ☐ |
| B5 | Los espacios **externos** de arriba y abajo miden **lo mismo** entre sí | ☐ |
| B6 | El **BOM total** de la línea es el mismo que antes **salvo** la longitud de la varilla | ☐ |

---

## Bloque C — El cambio de modo

| # | Comprobación | ✅/❌ |
|---|---|---|
| C1 | En modo **Automática** la tabla de tramos **no aparece** | ☐ |
| C2 | Al cambiar a **Avanzada** la tabla aparece **ya materializada**: trae los tramos que estabas viendo, no una lista en blanco | ☐ |
| C3 | Materializar **no cambia el dibujo**: mismos separadores, mismos tensores, mismo BOM | ☐ |
| C4 | Al volver a **Automática** sale un **aviso** que dice que la lista manual deja de mandar | ☐ |
| C5 | Si respondes **que no**, la ventana **se queda en avanzada** y la tabla sigue ahí | ☐ |
| C6 | Si respondes **que sí**, el dibujo vuelve a la regla estándar | ☐ |
| C7 | Tras volver a automática, si cambias otra vez a avanzada **la lista sigue ahí**: no se perdió el trabajo | ☐ |

---

## Bloque D — El editor avanzado

Trabaja sobre un intervalo con la tabla en modo **Avanzada**.

### D.1 — Editar y ver

| # | Comprobación | ✅/❌ |
|---|---|---|
| D1 | La tabla muestra **índice, Y1, Y2, alto y tensores** | ☐ |
| D2 | El índice empieza en **1** | ☐ |
| D3 | El **alto no se puede escribir**: es derivado de las dos cotas | ☐ |
| D4 | Editar **Y1** o **Y2** de un tramo y confirmar **redibuja** la línea | ☐ |
| D5 | El alto de esa fila **se recalcula solo** al cambiar una cota | ☐ |
| D6 | Se acepta tanto `12.5` como `12,5` | ☐ |

### D.2 — Tramos con y sin tensores

| # | Comprobación | ✅/❌ |
|---|---|---|
| D7 | Un tramo con la casilla **encendida** dibuja **dos** tensores en X entre sus dos separadores | ☐ |
| D8 | Un tramo con la casilla **apagada** dibuja **cero** tensores, y **sí** conserva sus separadores | ☐ |
| D9 | **Alternar tensores** cambia el dibujo en el acto | ☐ |

### D.3 — Lo que debe RECHAZARSE, con el motivo **en la misma ventana**

| # | Comprobación | ✅/❌ |
|---|---|---|
| D10 | Dejar un **hueco** entre dos tramos (p. ej. 20–60 y 80–120) se **rechaza** y el mensaje dice que un vacío es un tramo con los tensores apagados | ☐ |
| D11 | **Solapar** dos tramos (20–60 y 50–100) se **rechaza** y el mensaje dice cuánto se pisan | ☐ |
| D12 | Un tramo de **altura cero** (60–60) se **rechaza** | ☐ |
| D13 | Un tramo **al revés** (60–20) se **rechaza** | ☐ |
| D14 | Un tramo que **se pasa de la punta** de la columna se **rechaza** y el mensaje dice que los tramos no se comprimen | ☐ |
| D15 | Un tramo **por debajo del piso** se **rechaza** | ☐ |
| D16 | En **todos** los casos anteriores el motivo se lee **en la ventana**, bajo la tabla. Ninguno manda a buscarlo a otro sitio | ☐ |

### D.4 — Las acciones

| # | Comprobación | ✅/❌ |
|---|---|---|
| D17 | **Agregar** pone un tramo **encima** del último y **no** deja hueco | ☐ |
| D18 | **Eliminar** quita el tramo y **cierra el vacío**: lo de arriba baja, la secuencia sigue contigua | ☐ |
| D19 | Eliminar el **último** tramo que queda se **rechaza** con su motivo | ☐ |
| D20 | **Subir** y **bajar** intercambian el tramo con su vecino **sin romper la continuidad** | ☐ |
| D21 | Subir el de **más arriba**, o bajar el de **más abajo**, se **rechaza** con su motivo | ☐ |
| D22 | **Dividir** parte el tramo en dos por su mitad y los dos heredan sus tensores | ☐ |
| D23 | **Unir** dos tramos contiguos **con lo mismo dentro** los junta en uno | ☐ |
| D24 | **Unir** un tramo arriostrado con uno vacío se **rechaza**: la decisión de cuál gana es tuya | ☐ |
| D25 | **Materializar automático** vuelve a traer la secuencia de la regla | ☐ |

### D.5 — Separadores y BOM derivados

| # | Comprobación | ✅/❌ |
|---|---|---|
| D26 | Tres tramos que se tocan producen **cuatro** separadores, no seis: la frontera de en medio de cada par lleva **uno** | ☐ |
| D27 | **No hay dos separadores superpuestos** en ninguna frontera compartida | ☐ |
| D28 | Apagar los tensores de un tramo quita **exactamente dos** tensores del BOM por intervalo, y **ninguno** más | ☐ |
| D29 | Apagar los tensores de un tramo **no cambia** la cuenta de separadores del BOM | ☐ |
| D30 | El BOM refleja la secuencia **que se ve en la tabla**, no la de la regla | ☐ |

### D.6 — Persistencia

| # | Comprobación | ✅/❌ |
|---|---|---|
| D31 | **Guardar, cerrar y reabrir**: la ventana vuelve en modo **Avanzada** | ☐ |
| D32 | Los tramos vuelven **con sus cotas, su orden y sus tensores** | ☐ |
| D33 | El dibujo tras reabrir es **el mismo** que antes de guardar | ☐ |
| D34 | Un proyecto **antiguo** (sin estos campos) abre en modo **Automática** y dibuja **igual que siempre** | ☐ |

---

## Bloque E — La vista «Sección del adaptador»

Ábrela en el **configurador de tensor**: es el tercer botón de la previa, junto a Frontal y Planta.

| # | Comprobación | ✅/❌ |
|---|---|---|
| E1 | El botón **«Sección del adaptador»** existe en la previa del configurador de tensor | ☐ |
| E2 | Al elegirlo se ve **una L**, con sus **dos alas** completas | ☐ |
| E3 | El **ala apoyada** corre hacia la **derecha** de la imagen y la **del tensor hacia arriba** | ☐ |
| E4 | Se aprecia el **espesor** real: la L no es una línea, tiene canto | ☐ |
| E5 | El **filete de raíz** —la curva cóncava donde se juntan las dos alas— está ahí | ☐ |
| E6 | Las **dos puntas libres** están redondeadas, con radios **pequeños**, del orden del espesor | ☐ |
| E7 | Ninguna punta termina en **nariz semicircular**: entre las dos curvas de cada punta queda canto recto | ☐ |
| E8 | El **talón exterior** —la esquina de fuera donde se cruzan las caras externas— está **vivo**, sin redondear | ☐ |
| E9 | Se ven **dos marcas de agujero**, una en cada ala | ☐ |
| E10 | Cada marca está **centrada a lo ancho de su ala** | ☐ |
| E11 | Las marcas se ven como **trazas** (segmentos), no como círculos. Es correcto: en esta proyección los dos agujeros están de canto | ☐ |
| E12 | Cambiar a **Frontal** y volver a **Sección** devuelve la misma figura: la vista no depende de por dónde se pasó | ☐ |
| E13 | Con el tensor puesto en **perfil estructural**, la sección **no dibuja nada** y dice que ese tensor se atornilla directo y no lleva adaptador | ☐ |

> **E7 es el que hay que mirar con más calma.** En una ronda anterior el radio de punta se topaba en medio
> espesor y ahí las dos esquinas de cada punta se tocaban: el ala terminaba en semicírculo y lo rechazaste al
> verlo. El tope actual deja un canto recto pequeño pero real. Si vuelves a ver una nariz, el tope se movió.

> **E11 no es un defecto.** Los dos ejes de agujero son perpendiculares a la dirección de vista de esta
> cámara, así que ninguno se ve como círculo. Dibujarlos redondos pondría en el papel una boca que desde aquí
> no existe.

### E.14 — Que la sección y la línea sean la MISMA pieza

| # | Comprobación | ✅/❌ |
|---|---|---|
| E14 | La L de la sección tiene el **mismo espesor** que el canto que se ve en la frontal | ☐ |
| E15 | El **brazo** de la L mide 2 in, igual que el ancho del adaptador en la frontal | ☐ |
| E16 | Los **dos adaptadores** de un mismo tensor se ven **distintos** entre sí en esta vista: son espejos | ☐ |

---

## 4. Veredicto

| Campo | Valor |
|---|---|
| Fecha | ☐ |
| `VALIDATED_BUILD_SHA` usado | ☐ |
| SHA-256 del DLL cargado | ☐ |
| Veredicto | ☐ ACEPTADA ☐ RECHAZADA |
| Motivos, si se rechaza | ☐ |

**El veredicto de este paquete no acepta ADR-0027 ni ADR-0028**, ni cierra I-37D ni I-37.
