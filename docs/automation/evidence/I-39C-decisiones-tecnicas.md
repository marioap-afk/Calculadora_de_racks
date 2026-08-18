# I-39C — Decisiones técnicas del arquetipo B

> Evidencia de I-39C ([contrato](../../initiatives/I-39C-adopcion-editores-acotados.md),
> [ADR-0029](../../adr/0029-contrato-funcional-comun-de-ventanas-wpf.md)). **Son conclusiones técnicas
> del agente sobre hechos medidos, no decisiones del Owner**: ajustan *cómo* se implementa el alcance
> autorizado, no *qué* debe entregarse. Las decisiones del Owner viven en
> [`../decisions/I-39.md`](../decisions/I-39.md).

## 1. El contrato de tamaño se aplica con un estilo **hermano**, no con una variante

`BoundedEditorWindowStyle` no deriva de `EditorShellWindowStyle` ni lo sobrescribe: es un estilo
independiente alimentado por los tokens `BoundedEditor*`. ADR-0029 D9 dice que «un arquetipo no hereda
implícitamente restricciones de tamaño de otro», y una variante por herencia habría reproducido
exactamente la herencia implícita que la anomalía de I-39A demostró dañina.

Fondo y tipografía **sí** son los mismos tokens del shell: entre arquetipos cambia el tamaño, no la piel.

**Los cuatro literales se retiran, no se conservan.** Habrían seguido siendo letra muerta —el mínimo del
arquetipo B es menor, así que ahora sí se producirían, pero cada ventana declararía un tamaño distinto
para el mismo arquetipo—. Un arquetipo declara su tamaño **una vez**.

**Qué cambia observablemente**: las cuatro Cantilever pasan de abrir en `1120×700` y `1120×672` —que no
era lo que declaraban— a `1040×680`, con mínimo `820×520` en vez de `1120×672`. El Larguero pasa de
`720×440` con mínimo `640×380` al mismo contrato. El inspector **no** cambia de tamaño: los valores del
contrato salieron de él cuando I-39A lo escribió.

**Que al mínimo no se pierde nada no se afirma, se mide**: una prueba lleva las cuatro ventanas al cliente
más apretado —el mínimo menos una holgura fija de marco, el mismo criterio con que I-30 fijó el mínimo del
arquetipo A— y comprueba que diagnóstico, receta y los tres botones de la barra caben enteros.

## 2. El respaldo de recursos es respaldo, no sombreado

Simetría exacta con lo que I-39B corrigió en el shell rico. I-39A mergeaba el diccionario compartido
**siempre**, en el constructor. Eso pone el diccionario del control por delante del de la ventana también
para el contenido que el editor inyecta en las ranuras, que pasa a resolver **otra instancia** del mismo
estilo: misma apariencia, distinto objeto. En el shell rico eso rompió una prueba de identidad de I-37D.

Aquí todavía no rompía nada —ningún consumidor del arquetipo B declara un solo `x:Key`, medido antes de
tocar—, y **por eso** I-39B lo dejó asignado a I-39C en vez de corregirlo a ciegas. Ahora el respaldo
entra en `OnApplyTemplate` y solo cuando el token no resuelve.

## 3. `EditorAction` evoluciona; los botones de XAML **no** se convierten a código

La evolución era obligatoria y su motivo estaba medido desde el censo de I-39A: la fábrica no sabía
declarar acción por defecto ni de cancelación, así que sustituir un botón escrito a mano por uno suyo
**borraba Enter y Escape en silencio**. Con `IsDefault` e `IsCancel`, el contrato de teclado viaja con la
descripción.

Prominencia visual y rol de teclado quedan **separados**: D7 solo admite Enter en una acción segura y
contextual, así que una acción primaria que dibuja puede tener que no responder a Enter. Y una acción no
puede ser a la vez la de defecto y la de cancelación —Enter y Escape harían lo mismo—, lo que el
constructor rechaza.

**Adopción**: el piloto, que construye sus botones en código y es el consumidor natural. La prueba de que
la evolución es correcta es que las treinta caracterizaciones de I-39A siguen verdes **sin tocarse**,
incluida la que fija que Enter dispara `Insertar` y Escape dispara `Cerrar`.

**Los botones declarados en XAML no se convierten**: llevan `x:Name` del que dependen su propio
code-behind y las suites funcionales, y sustituirlos sería una reescritura sin ganancia para el usuario.
`EditorActionBar` sigue **sin** consumidor en el arquetipo B: su valor —las cuatro categorías neutrales y
el envoltorio que no recorta— lo resuelve aquí el `DockPanel` que las cuatro ventanas ya tienen, y la
prueba de mínimo demuestra que no recorta. Desviación **medida**, no pendiente.

## 4. Cierre y dirty: `NotApplicable` con razón de producto

Ninguna de las seis declara un ámbito transaccional pendiente y ninguna intercepta el cierre. No es una
omisión: es lo que el producto es.

- Las **cuatro Cantilever** editan una **copia** que solo se devuelve al aceptar, y lo dicen con su propia
  acción `Restaurar`, que vuelve a los valores con que se abrió la ventana. Escape descarta una edición
  que el producto declara provisional, no trabajo guardado.
- El **Larguero** no acumula nada perdible: lo que persiste lo persiste su botón de guardar.
- El **inspector** no edita, inspecciona.

Inventarles un dirty global sería exactamente lo que I-39B se negó a hacer en los editores ricos. La
guarda ata las dos cosas en una sola aserción: **quien declare ámbito tiene que interceptar el cierre, y
quien no, no**. Si alguna empezara a acumular trabajo perdible, la prueba lo obligaría.

## 5. Preview: autoridad derivada, frescura siempre actual

Medido en las seis: el preview es siempre **derivado del borrador capturado** y siempre **actual**, porque
cada ventana lo rehace en el mismo paso en que recalcula. Ninguna conserva un último-válido obsoleto.

No se inventa un modelo de frescura que el producto no tiene —D4 dice expresamente que una ventana no está
obligada a implementar estados que hoy no exhibe—. Lo que sí se fija es que no aparezca uno por accidente:
sin plan resuelto no se dibuja **ni una figura**, y lo que se ve es un **mensaje**, no un residuo.

## 6. El foco inicial se declara porque la plantilla no lo garantiza

Sin declararlo, el foco caía donde el árbol **visual** del shell lo pusiera, y la plantilla acopla la barra
de acciones arriba en el `DockPanel`, antes de la zona de parámetros: el primer elemento enfocable podía
ser un botón, y `Restaurar` descarta lo editado. D9 exige foco inicial determinista que **no** recaiga en
una acción destructiva ni bloqueada.

Cada ventana lo declara en su primer control de captura. El **separador** lo declara en su selector de
sección porque es su **único** control editable: su corte no se escribe, se deriva.

## 7. La superficie del preview del Larguero se queda clara

El token natural por simetría habría sido `ShellPreviewBackgroundBrush`, el fondo oscuro del editor rico.
No se usa: el esquema del larguero se dibuja con rótulos grises que sobre fondo oscuro serían ilegibles.
Se usa `ShellSurfaceBrush`, que vale **exactamente** el blanco que la ventana ya pintaba. Coherencia
visual que rompe legibilidad es una regresión de producto disfrazada.
