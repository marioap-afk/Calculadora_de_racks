# Estado actual del MVP

## Objetivo del MVP actual

Validar que un ingeniero pueda configurar una cabecera de rack mas rapido que dibujarla manualmente, antes de invertir en dibujo AutoCAD, bloques, BOM o base de datos.

## Flujo actual

1. AutoCAD ejecuta `RACKCABECERA`.
2. El plugin crea una configuracion estandar temporal.
3. Se abre una ventana WPF modal.
4. El usuario modifica horizontales, paneles, arreglo de celosia, perfiles, caras y puntos.
5. La vista previa WPF se actualiza.
6. El sistema muestra validacion, excepciones y estado.

## Funcionalidades disponibles

- Ventana WPF modal desde AutoCAD.
- Cabecera estandar temporal hardcodeada.
- Tabla de horizontales.
- Tabla de paneles.
- Arbol de modelo.
- Panel de propiedades contextual.
- Vista previa vertical esquematica.
- Seleccion sincronizada entre arbol, tablas y vista previa.
- Edicion de:
  - horizontales;
  - paneles;
  - arreglo de panel;
  - perfil diagonal;
  - cara de montaje;
  - direccion diagonal;
  - puntos inicial/final;
  - postes y placas basicas.
- Operaciones sobre horizontales/paneles:
  - agregar horizontal;
  - eliminar horizontal;
  - duplicar horizontal;
  - dividir panel;
  - combinar paneles.
- Excepciones agrupadas.
- Validacion de altura y consistencia de modelo.
- Layout redimensionable con splitters.
- Persistencia local de layout de usuario.
- Boton `Restaurar layout predeterminado`.
- Boton `Restaurar cabecera estandar`.
- Lista de materiales (BOM) con exportacion a CSV.
- Guardar/abrir proyecto (`.rackcad.json`).
- Dibujo de la cabecera en vista lateral (block-based) en AutoCAD con el boton `Insertar en AutoCAD`
  y el comando `RACKCABECERALATERAL`.

## Estandar temporal actual

- Nombre: `Cabecera estandar temporal`.
- Unidades: `in`.
- Altura objetivo: `132 in`.
- Fondo: `42 in`.
- Poste izquierdo: `POSTE_OMEGA_3_X_3_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA_DE_CINTA_NEGRA_CALIBRE_14`.
- Poste derecho: el mismo poste.
- Placa base: `PLACA_BASE_DE_CABECERA_ATORNILLABLE_DE_PLACA_CALIBRE_3_16_DE_4_X_4_13_16`.
- Horizontales (todas el mismo perfil de celosia `TRAVESAÑO_CINTA_NEGRA_CALIBRE_14_DE_2_X_1_1_8_DE_CINTA_NEGRA_CALIBRE_14`):
  - H1 = 0 in, cantidad 2.
  - H2 = 44 in, cantidad 1.
  - H3 = 88 in, cantidad 1.
  - H4 = 132 in, cantidad 1.
- Paneles derivados:
  - P1 = H1-H2.
  - P2 = H2-H3.
  - P3 = H3-H4.
- Arreglo default: `SingleDiagonal`.
- Cara default: `Front`.
- Direccion default: `AutoAlternating`.

## Restauraciones disponibles

`Restaurar layout predeterminado`:

- Solo restaura tamanos de ventana, paneles y tablas.
- No modifica la configuracion tecnica.

`Restaurar cabecera estandar`:

- Restaura la configuracion tecnica inicial.
- Limpia modificaciones manuales.
- Limpia excepciones.
- Reconstruye horizontales y paneles.
- Refresca tabla, arbol, seleccion y vista previa.

## Fuera de alcance todavia

- Dibujo de las vistas frontal y planta (solo existe la cabecera lateral).
- Definicion automatica de los bloques dinamicos en el DWG (deben existir previamente).
- SQLite.
- Exportacion a Excel (hoy el BOM exporta CSV).
- Guardado de metadatos en DWG.

