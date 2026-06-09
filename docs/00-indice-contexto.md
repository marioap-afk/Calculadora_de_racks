# Indice de contexto RackCad

Este indice resume como entender rapidamente el proyecto antes de continuar el desarrollo.

## Lectura recomendada

1. `README.md`
   - Vista rapida del proyecto, build y prueba con `NETLOAD`.

2. `docs/01-estado-actual-mvp.md`
   - Que funciona hoy, que no funciona todavia y como se usa el configurador.

3. `docs/02-modelo-tecnico-vigente.md`
   - Modelo actual: horizontales como fuente de verdad, paneles derivados, miembros fisicos y excepciones.

4. `docs/03-guia-desarrollo-y-validacion.md`
   - Entorno, build, AutoCAD 2025, warnings conocidos y checklist de validacion.

5. `docs/04-roadmap-operativo.md`
   - Siguientes pasos recomendados sin mezclar dibujo, BOM, catalogos y persistencia antes de tiempo.

## Documentos historicos existentes

Estos documentos son utiles para decisiones de arquitectura, pero son mas extensos:

- `arquitectura-autocad-racks.md`
- `mvp-configurador-cabeceras.md`
- `modelo-datos-cabecera-rack-selectivo.md`
- `plan-implementacion-mvp-csharp-autocad.md`
- `analisis-macro-vba-cabeceras.md`

## Decisiones clave ya tomadas

- AutoCAD completo, no AutoCAD LT.
- AutoCAD .NET API.
- C# y Visual Studio.
- `net8.0-windows` para UI/plugin.
- WPF para el configurador modal.
- El MVP primero valida configuracion y vista previa, no dibujo CAD.
- El usuario parte de una cabecera estandar y modifica excepciones.
- Las horizontales son entidades fisicas y fuente de verdad.
- Los paneles son espacios derivados entre horizontales consecutivas.
- Los offsets y puntos de conexion deben terminar en catalogos/configuracion, no hardcodeados en dibujo.

## Estado del repositorio

El codigo actual es un prototipo funcional de configurador. Todavia no debe tratarse como motor CAD final.

La carpeta de salida `bin/`, `obj/`, caches locales `.dotnet_home`, `.nuget_packages`, `.appdata` y `.localappdata` no son parte logica del codigo fuente y estan ignoradas por `.gitignore`.

