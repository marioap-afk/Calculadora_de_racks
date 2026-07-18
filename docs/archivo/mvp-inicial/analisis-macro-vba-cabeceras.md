# Analisis tecnico de macro VBA: generacion de cabecera de rack

> **Archivo histórico; no es una fuente vigente.** Consulta [ARCHITECTURE](../../ARCHITECTURE.md)
> y [HANDOFF](../../HANDOFF.md).

Archivo analizado: `C:\Users\alejandra-mendoza\Desktop\PruebaCabeceras.dvb`

Fecha del analisis: 2026-06-03

## 1. Alcance

Este documento analiza una macro VBA funcional de AutoCAD que genera una cabecera de rack.

No se realiza conversion a C#.
No se propone una traduccion linea por linea.
No se toma la macro como arquitectura objetivo.

El objetivo es identificar:

- Que hace actualmente.
- Que datos utiliza.
- Que reglas de negocio contiene.
- Que partes conviene migrar.
- Que partes conviene descartar o redisenar.

## 2. Estructura encontrada en el archivo DVB

El archivo contiene un proyecto VBA llamado `ACADProject`.

Modulos encontrados:

| Modulo | Tipo | Responsabilidad observada |
|---|---|---|
| ThisDrawing | Documento AutoCAD | No contiene logica propia relevante. |
| CabecerasForm | UserForm | Captura datos del usuario y lanza la generacion. |
| Funciones | Modulo procedural | Contiene la logica principal de calculo y dibujo. |
| Subs | Modulo procedural | Muestra el formulario. |

Dependencias externas detectadas:

- AutoCAD VBA / ActiveX.
- Microsoft Forms.
- Bloques dinamicos existentes en el dibujo o en la biblioteca cargada.
- Propiedades dinamicas de bloques con nombres especificos.

## 3. Que hace la macro

La macro permite al usuario abrir un formulario, capturar datos basicos de una cabecera y generar un bloque de AutoCAD que representa esa cabecera.

Flujo funcional:

1. El usuario ejecuta `MostrarCabecerasForm`.
2. Se abre `CabecerasForm`.
3. El formulario carga tipos de cabecera.
4. Al seleccionar un tipo de cabecera, se cargan tipos de poste.
5. El usuario captura:
   - Tipo de poste.
   - Altura.
   - Fondo.
   - Claro de celosia.
   - Inicio de celosia.
6. Al presionar Generar:
   - Se construye un nombre de bloque para la cabecera.
   - Se llama a `CrearCabeceraFunc`.
   - Se crea una definicion de bloque de cabecera.
   - Se insertan postes, travesanos y diagonales dentro de esa definicion.
   - Se solicita un punto de insercion.
   - Se inserta el bloque resultante en ModelSpace.

Resultado en AutoCAD:

- Una definicion de bloque llamada con una convencion descriptiva.
- Una referencia insertada en ModelSpace.
- Geometria generada a partir de bloques dinamicos existentes.

La macro no genera, al menos en esta version:

- Capas especificas.
- Atributos.
- Metadatos.
- Lista de materiales.
- Cotas.
- Etiquetas.
- Placas base.
- Tornilleria.
- Validaciones robustas.
- Integracion con Excel.

## 4. Datos que utiliza

### 4.1 Datos capturados desde el usuario

| Dato | Control observado | Uso |
|---|---|---|
| Tipo de cabecera | `TipoDeCabeceraComboBox` | Solo filtra o carga opciones de poste; no modifica el algoritmo de dibujo. |
| Tipo de poste | `TipoDePosteComboBox` | Nombre del bloque de poste que se inserta. |
| Altura | `AlturaTextBox` | Altura del poste y limite vertical de la cabecera. |
| Fondo | `FondoTextBox` | Separacion entre postes y longitud base de travesanos. |
| Claro de celosia | `ClaroCelosiaTextBox` | Separacion vertical entre travesanos y patron de diagonales. |
| Inicio de celosia | `InicioCelosiaTextBox` | Altura inicial del primer travesano. |

### 4.2 Catalogos codificados en el formulario

Tipos de cabecera disponibles:

- Cabecera formada.
- Cabecera formada reforzada.
- Cabecera estructural.
- Cabecera estructural reforzada.

Tipos de poste disponibles:

- Poste omega 3x3.
- Poste omega 3.5x3.
- Poste omega 4x3.
- Poste omega 4.5x3.
- Poste omega 5x3.

Observacion importante:

Todos los tipos de cabecera cargan exactamente los mismos postes. En la practica, el tipo de cabecera no cambia el comportamiento del generador.

### 4.3 Bloques dinamicos requeridos

La macro depende de bloques con nombres exactos:

| Bloque | Uso |
|---|---|
| Nombre seleccionado en `TipoDePosteComboBox` | Poste principal de la cabecera. |
| `Travesaño dinamico omega 3x3` | Travesanos horizontales y diagonales. |

Propiedades dinamicas requeridas:

| Propiedad | Bloque | Uso |
|---|---|---|
| `AlturaPoste` | Poste | Ajusta la altura del poste. |
| `Distancia1` | Travesano dinamico | Ajusta longitud de travesanos y diagonales. |

### 4.4 Constantes numericas embebidas

La macro contiene constantes que hoy viven dentro del codigo:

| Constante | Uso observado | Interpretacion probable |
|---|---|---|
| `4.6875` | `Fondo - 4.6875` | Descuento horizontal para longitud del travesano. |
| `2.34` | Coordenada X inicial de travesanos y diagonales. | Offset desde el borde/poste. Aproxima la mitad de `4.6875`. |
| `4` | Inicio de diagonal y descuento vertical inferior/superior. | Offset de conexion de diagonal. |
| `44` | Calculo de cantidad de travesanos. | Separacion maxima o criterio historico de celosia. |
| `26` | Umbral para dos travesanos de cierre. | Regla de cierre superior. |
| `14` | Umbral para un travesano de cierre. | Regla de cierre superior. |
| `2` | Descuento en claro de cierre. | Holgura o separacion de remate. |
| `4` | Ajuste alterno de cierre. | Alineacion a patron divisible entre 4. |

Estas constantes son reglas de ingenieria o de geometria de producto. No deberian quedar embebidas en C# sin nombre, unidad, descripcion y propietario tecnico.

## 5. Reglas de negocio contenidas

### 5.1 Nombre de cabecera

La macro genera el nombre del bloque con el patron:

`Cabecera de poste [TipoDePoste] de [Fondo] de fondo y [Altura] de altura`

Esta es una regla de nomenclatura, no de calculo. Conviene conservarla solo como referencia inicial y reemplazarla por una convencion formal.

### 5.2 Longitud del travesano

Regla:

`LongitudTravesaño = Fondo - 4.6875`

Interpretacion:

El travesano no mide el fondo completo. Se descuenta una distancia fija relacionada con el ancho/offset de conexion contra postes.

Debe migrarse, pero como parametro de catalogo o regla configurable, no como constante anonima.

### 5.3 Inicio de travesanos

Regla:

`InicioTravesaño = InicioCelosia`

El primer travesano inicia exactamente en la altura definida por el usuario.

Debe migrarse como parte del layout de cabecera.

### 5.4 Inicio de diagonal

Regla:

`InicioDiagonal = InicioCelosia + 4`

Interpretacion:

La diagonal inicia 4 unidades arriba del inicio de la celosia. Probablemente representa un offset de barreno/conexion.

Debe migrarse como regla parametrizada por tipo de poste, patron de perforacion o tipo de arriostre.

### 5.5 Angulo y longitud de diagonal

Reglas:

`AnguloDiagonal = Atn((Claro - 8) / LongitudTravesaño)`

`LongitudDiagonal = (Claro - 8) / Sin(AnguloDiagonal)`

Interpretacion:

La diagonal se calcula como la hipotenusa de un claro vertical efectivo y una longitud horizontal efectiva. El claro vertical efectivo es `Claro - 8`.

Debe migrarse al motor de geometria/calculo, pero con nombres claros:

- claro vertical nominal.
- offsets de conexion.
- claro vertical efectivo.
- distancia horizontal efectiva.
- angulo de arriostre.
- longitud de arriostre.

### 5.6 Cantidad de travesanos

Regla:

`NTravesaños = Int((Altura - InicioCelosia) / 44) + 1`

Observacion critica:

La cantidad se calcula con `44`, pero el arreglo de travesanos se coloca usando `Claro`. Esto implica que `44` funciona como criterio fijo para determinar cantidad, mientras `Claro` controla la separacion real dibujada.

Esto puede ser intencional si `44` es un claro estandar, pero tambien puede ser una inconsistencia si el usuario puede capturar otro claro.

Debe revisarse con ingenieria antes de migrar.

### 5.7 Ajuste de fin de celosia

Regla:

`FinCelosia = Ceil(InicioCelosia) + Ceil(InicioCelosia) Mod 2 - InicioCelosia`

Interpretacion probable:

Calcula un ajuste desde el inicio de celosia hacia el siguiente valor entero par. Esto parece estar relacionado con alineacion a patron de perforaciones o alturas permitidas.

Debe migrarse, pero no como formula opaca. Debe expresarse como regla de alineacion:

- redondear a siguiente perforacion valida.
- usar paso de perforacion.
- respetar offset de primer barreno.
- cerrar contra posicion superior valida.

### 5.8 Travesanos de cierre

La macro calcula el espacio restante arriba del patron principal:

`Restante = Altura - (Claro * (NTravesaños - 1) + InicioCelosia) - FinCelosia`

Luego aplica dos umbrales:

| Condicion | Resultado |
|---|---|
| `Restante >= 26` | Se habilita condicion de dos travesanos de cierre. |
| `Restante >= 14` | Se habilita condicion de un travesano de cierre. |

Si ambas condiciones son verdaderas, se usa el claro calculado para dos cierres:

- Si `(Restante - 2) Mod 4 = 0`, entonces `Claro1Cierre = (Restante - 2) / 2`.
- Si no, `Claro1Cierre = (Restante - 4) / 2`.

Si solo aplica un cierre:

- `Claro2Cierre = Restante - 2`.

Interpretacion:

Existe una regla de remate superior de la celosia para no dejar espacios grandes sin travesano. Los cierres se ajustan con holguras de 2 o 4 unidades para caer en una modulacion aceptable.

Esta es una regla de negocio importante. Debe migrarse, pero con nombres tecnicos y pruebas unitarias.

### 5.9 Simetria de postes

La macro inserta un poste en el origen y genera el segundo con mirror sobre una linea vertical ubicada en `Fondo / 2`.

Interpretacion:

La cabecera tiene dos postes simetricos separados por el fondo nominal.

Debe migrarse como regla geometrica del layout.

### 5.10 Patron alternado de diagonales

La macro inserta una primera diagonal, luego:

- Si hay mas de 2 travesanos, crea una diagonal espejeada.
- Si hay mas de 3 travesanos, genera arreglos rectangulares de diagonales alternadas cada `Claro * 2`.

Interpretacion:

La celosia usa un patron alternado en zig-zag entre travesanos.

Debe migrarse como estrategia de arriostramiento, no como llamadas directas a `Mirror` y `ArrayRectangular`.

## 6. Partes que deberian migrarse

### 6.1 Reglas de geometria de cabecera

Migrar:

- Calculo de longitud de travesanos.
- Calculo de angulo y longitud de diagonales.
- Calculo de cantidad de travesanos.
- Calculo de espacio restante.
- Reglas de travesanos de cierre.
- Patron alternado de diagonales.
- Simetria de postes.
- Posiciones de origen de postes, travesanos y diagonales.

Destino recomendado:

- `RackCad.Calculation` para calculos dimensionales.
- `RackCad.Domain` para representar cabecera, postes, arriostres y cierres.
- `RackCad.Drawing` para generar un plan de dibujo abstracto.

### 6.2 Conocimiento de bloques dinamicos

Migrar:

- Uso de bloque dinamico de poste.
- Uso de bloque dinamico de travesano/diagonal.
- Propiedades dinamicas `AlturaPoste` y `Distancia1`.

Pero deben migrarse a un manifiesto de bloques, no quedar escritos directamente en servicios C#.

Destino recomendado:

- Tabla `BlockDefinitions`.
- Tabla o archivo de mapeo `DynamicBlockProperties`.
- Adaptador `RackCad.AutoCad`.

### 6.3 Catalogo inicial

Migrar:

- Tipos de poste actualmente codificados.
- Tipos de cabecera actualmente codificados.
- Relacion entre tipo de cabecera y postes compatibles.

Destino recomendado:

- Catalogo externo SQLite/JSON inicial.
- Tablas de compatibilidad.

### 6.4 Flujo de usuario

Migrar conceptualmente:

- Abrir formulario.
- Capturar parametros.
- Validar.
- Solicitar punto de insercion.
- Insertar cabecera.

Pero debe redisenarse para que el formulario no haga calculos ni cree entidades directamente.

Destino recomendado:

- UI WPF o formulario .NET.
- Application Service `CreateRackFrame`.
- AutoCAD command `RACKCABECERA`.

### 6.5 Funcion de busqueda de propiedad dinamica

La funcion `IndicePropiedadBloque` busca una propiedad dinamica por nombre.

Debe migrarse como utilidad del adaptador AutoCAD, con mejoras:

- Error claro si la propiedad no existe.
- Lista de propiedades disponibles en el mensaje tecnico.
- Validacion de tipo de dato.
- Registro en log.

## 7. Partes que deberian descartarse o redisenarse

### 7.1 Mezcla de UI, calculo y dibujo

Actualmente el formulario:

- Lee controles.
- Construye nombres.
- Llama generacion.
- Pide punto de insercion.
- Inserta el bloque final.

Y `CrearCabeceraFunc`:

- Calcula reglas.
- Crea bloques.
- Inserta referencias.
- Modifica propiedades dinamicas.
- Hace mirrors.
- Hace arrays.

Esto debe descartarse como estructura. En C# debe separarse por responsabilidades.

### 7.2 Catalogos hardcodeados

Los tipos de cabecera y postes no deben vivir dentro de eventos de formulario.

Descartar:

- Listas fijas en `TipoDeCabeceraComboBox_Change`.
- Nombres de bloques usados como codigos de negocio.

Reemplazar por:

- Catalogos externos.
- Codigos estables.
- Descripciones visibles.
- Compatibilidades.

### 7.3 Casos duplicados

El `Select Case` tiene `Case "Cabecera formada"` duplicado.

Debe eliminarse. Tambien debe revisarse por que todos los tipos cargan los mismos postes.

### 7.4 Controles y eventos sin uso

Se detectan controles o eventos que no aportan al flujo actual:

- `Label1_Click` vacio.
- `NombreBloqueTextBox`, detectado en el formulario pero no usado por la logica.
- `InicioTroquelCheckBox`, detectado pero no usado.
- `ClaroCelosiaCheckBox`, detectado pero no usado.
- `CLimpiarButton`, detectado pero no implementado en el codigo extraido.

Deben descartarse del MVP o implementarse con una razon funcional clara.

### 7.5 Variables implicitas

La macro no usa `Option Explicit` y contiene variables sin declaracion formal, por ejemplo:

- `InicioTravesaño`.
- `LongitudTravesañoValue`.
- `InicioDiagonal`.
- `AnguloDiagonalValue`.
- `LongitudDiagonalValue`.
- `NTravesaños`.
- `FinCelosia`.
- `RestanteTravesaños`.
- Variables leidas desde el formulario.

Esto no debe migrarse. En C# todo debe ser tipado, explicito y validado.

### 7.6 Falta de validaciones

No se observan validaciones para:

- Campos vacios.
- Numeros invalidos.
- Altura menor al inicio de celosia.
- Fondo insuficiente para restar `4.6875`.
- Claro menor o igual a 8, que puede romper la diagonal.
- Bloques inexistentes.
- Propiedades dinamicas inexistentes.
- Nombre de bloque duplicado.
- Cancelacion del punto de insercion.

La falta de validaciones debe corregirse en el MVP nuevo.

### 7.7 Uso directo de nombres visibles como llaves

La macro usa nombres visibles como:

- `Poste omega 3x3`.
- `Travesaño dinamico omega 3x3`.
- `AlturaPoste`.
- `Distancia1`.

En el sistema nuevo deben existir identificadores internos y mapeos. Los nombres visibles pueden cambiar sin romper el generador.

### 7.8 Falta de capas, atributos y metadatos

La macro genera geometria, pero no deja trazabilidad de ingenieria.

Debe redisenarse para agregar:

- Capas por componente.
- Atributos de bloque.
- Identificador de instancia.
- Version de catalogo.
- Parametros de generacion.
- Relacion con lista de materiales.

## 8. Clasificacion por elemento

| Elemento actual | Accion recomendada | Motivo |
|---|---|---|
| `MostrarCabecerasForm` | Migrar conceptualmente | Equivale al comando inicial del MVP. |
| `CabecerasForm` | Redisenar | La pantalla es util como referencia, pero mezcla responsabilidades y tiene controles no usados. |
| Lista de tipos de cabecera | Migrar a catalogo | Debe vivir fuera del codigo. |
| Lista de postes | Migrar a catalogo | Debe tener codigos, dimensiones, bloques y compatibilidades. |
| `CrearCabeceraFunc` | Migrar por partes | Contiene reglas valiosas, pero debe separarse en calculo, dominio y dibujo. |
| `IndicePropiedadBloque` | Migrar mejorada | Es una utilidad necesaria para bloques dinamicos. |
| `Ceil` | Descartar como funcion propia | En C# existe `Math.Ceiling`; la regla especial de alineacion debe nombrarse aparte. |
| Constantes numericas | Parametrizar | Son reglas de producto o geometria, no literales de codigo. |
| `Mirror` para postes | Migrar como regla geometrica | Representa simetria de cabecera. |
| `ArrayRectangular` para travesanos | Migrar como generacion de instancias | El dominio debe saber cuantas piezas existen; AutoCAD solo debe dibujarlas. |
| `ArrayRectangular` para diagonales | Migrar como patron de arriostramiento | Debe expresarse como zig-zag o estrategia de celosia. |
| Nombres de bloque generados | Redisenar | Pueden causar conflictos y no son identificadores robustos. |

## 9. Reglas que requieren confirmacion de ingenieria

Antes de migrar la logica, conviene confirmar:

1. Unidad base de la macro. Los valores parecen estar en pulgadas.
2. Significado exacto de `4.6875`.
3. Significado exacto de `2.34`.
4. Por que la diagonal usa `Claro - 8`.
5. Si `44` es separacion maxima, claro estandar o residuo historico.
6. Por que los cierres usan umbrales `26` y `14`.
7. Por que el cierre ajusta con `2` o `4`.
8. Si el inicio de celosia debe alinearse al siguiente numero par.
9. Si todos los tipos de cabecera realmente comparten los mismos postes.
10. Si el tipo de cabecera debe cambiar reglas de dibujo, refuerzo o componentes.
11. Si el bloque `Travesaño dinamico omega 3x3` representa tanto horizontal como diagonal o solo se reutilizo por conveniencia.
12. Que pasa cuando ya existe una definicion de bloque con el mismo nombre.

## 10. Recomendacion para el MVP en C#

El MVP nuevo puede usar esta macro como referencia funcional, pero no como plantilla de arquitectura.

Se recomienda modelar la generacion en cuatro pasos:

1. `RackFrameInput`
   - Tipo de cabecera.
   - Tipo de poste.
   - Altura.
   - Fondo.
   - Claro de celosia.
   - Inicio de celosia.

2. `RackFrameCalculator`
   - Calcula travesanos principales.
   - Calcula diagonales.
   - Calcula cierres.
   - Devuelve advertencias o errores.

3. `RackFrameDrawingPlan`
   - Lista de componentes a dibujar.
   - Posiciones.
   - Rotaciones.
   - Longitudes dinamicas.
   - Capas.
   - Atributos.

4. `AutoCadRackFrameDrawer`
   - Crea o inserta bloques.
   - Aplica propiedades dinamicas.
   - Escribe metadatos.
   - Inserta la cabecera en el punto elegido.

## 11. Conclusiones

La macro actual es valiosa porque ya contiene una logica funcional de generacion de cabecera, especialmente:

- Patron de postes simetricos.
- Longitud efectiva de travesanos.
- Calculo de diagonales.
- Cantidad de travesanos.
- Reglas de cierre superior.
- Patron alternado de diagonales.

Pero no debe convertirse directamente. La macro es una prueba funcional y una fuente de reglas de negocio, no una arquitectura objetivo.

Para el nuevo sistema, lo mas importante es extraer las reglas, nombrarlas, validarlas con ingenieria y moverlas a un motor independiente de AutoCAD. AutoCAD debe quedar como adaptador de dibujo, no como el lugar donde se decide la configuracion de la cabecera.

