# Disposicion del Owner sobre la desviacion de CT-PB0-17 (F4B-PBT0-CHARACTERIZATION, intento 1)

Fecha: 2026-11-17. Decision del Owner en el chat de la sesion (pregunta con dos opciones). Respuesta literal: "Ratificar el cambio (Recomendado)". El texto literal completo, con tildes, se custodia con el cierre de la tarea.

Hechos sobre los que decide (medidos por la sesion y por la revision independiente en la BaseSha f2eedd9a):
- Push Back deserializa con DynamicRackSystemDocument.ToDesign (PushBackDesignDocument.cs:161). Alli Dimensions = ValidDimensions(Dimensions) (DynamicRackSystemDocument.cs:227), y ValidDimensions (:429-432) convierte todo valor fuera de [None, Detailed] en None. Por eso 7 y -1 (c-10 y c-11) nunca llegan al recorte de W:407, y MU-PB0-16 no podia discriminar CT-PB0-17 con c-01..c-15. La alternativa W:387 tampoco discrimina: ToDesign ya sustituye la unidad en blanco por "kg" (:205).
- El Worker anadio el caso c-16 (Dimensions = 3, el limite valido; DimensionDetail.Detailed = 3) y, con el, MU-PB0-16 hace fallar CT-PB0-17 por asercion. Lo declaro en Deviations[0]. La cita ":322" de la entrega es la de ToDomain; la correcta es :227 y :429-432.
- c-12 (PalletsDeep = 1) lo rechaza RackProjectStore.ValidateProject (RackProjectStore.cs:343-353) y c-13 (PalletTolerance = 0) hace lanzar DynamicRackSystemResolver.Validate (:473-474) dentro de LoadExisting; CT-PB0-17 los fija como "RECHAZADO <tipo>: <mensaje>" (Deviations[1]).

Lo que ratifica el Owner:
1. La enmienda de CT-PB0-17: el caso c-16 se acepta como caso de la lista de CT-PB0-17, y la fila MU-PB0-16 de INV-PBT0-14 se apoya en el.
2. La forma "RECHAZADO <tipo>: <mensaje>" de c-12 y c-13 en CT-PB0-17.
3. PBT0-C10 no se aplica a este intento por la falta de discriminacion de MU-PB0-16 con los casos originales. No gasta intento: attempts sigue en 1 de 3.

Lo que NO ratifica ni cambia: el resto del contrato; los demas mutantes, CT, invariantes y paradas siguen vigentes y se verifican como estan. Los menores de las revisiones (M-01 a M-06 de la independiente y D-2 a D-5 de la directa) se registran para tareas siguientes, sin cambiar las pruebas despues del RED.
