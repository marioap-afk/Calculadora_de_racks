# Roadmap operativo recomendado

Este roadmap evita saltar directo a bloques, BOM o SQLite antes de que el configurador sea estable.

## Fase 1 - Consolidar configurador WPF

Objetivo: que configurar una cabecera compleja sea claramente mas rapido que dibujarla manualmente.

Pendientes sugeridos:

- Probar casos con muchas horizontales.
- Mejorar edicion masiva de horizontales.
- Validar seleccion multiple de paneles.
- Mejorar advertencias de duplicados de elevacion.
- Confirmar que restaurar estandar cubre todos los campos tecnicos.
- Revisar si se necesita confirmacion antes de restaurar cabecera estandar.

## Fase 2 - Catalogos externos ligeros

Objetivo: sacar valores hardcodeados sin introducir SQLite todavia.

Formato recomendado inicial:

- JSON o CSV versionado en carpeta `catalogs/`.

Catalogos minimos:

- perfiles de poste;
- perfiles de horizontal;
- perfiles de diagonal;
- placas base;
- puntos de conexion;
- plantillas de cabecera.

## Fase 3 - Vista previa mas tecnica

Objetivo: que la vista previa represente mejor lo que luego se dibujara.

Pendientes:

- Mostrar diferencias entre cara Front/Back/Both con mas claridad.
- Dibujar puntos de conexion seleccionables.
- Mostrar horizontales dobles de forma mas controlada.
- Separar visualmente panel abierto, panel sin diagonales y panel con diagonales.

## Fase 4 - Dibujo AutoCAD simple

Objetivo: generar geometria basica, no bloques.

Primer entregable CAD:

- Seleccionar punto de insercion.
- Dibujar postes como lineas/rectangulos simples.
- Dibujar horizontales.
- Dibujar diagonales.
- Usar capas basicas.
- No generar BOM.
- No guardar metadatos avanzados.

## Fase 5 - Metadatos y regeneracion

Objetivo: poder editar una cabecera ya insertada.

Pendientes:

- Definir formato de metadatos.
- Guardar configuracion serializada en XData o extension dictionary.
- Detectar cabecera existente.
- Abrir configurador con datos existentes.
- Regenerar geometria simple.

## Fase 6 - Bloques y componentes reales

Objetivo: pasar de geometria simple a componentes reutilizables.

Pendientes:

- Libreria de bloques.
- Reglas de insercion.
- Atributos de bloque.
- Puntos de conexion reales.
- Versionado de bloques/catalogos.

## Fase 7 - BOM y cotizacion

Objetivo: usar el mismo modelo de miembros fisicos para listas de materiales.

Pendientes:

- Agrupar miembros por perfil, longitud, cantidad y origen.
- Exportar BOM preliminar.
- Integrar con archivo cotizador existente.
- Agregar validaciones de ingenieria gradualmente.

