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

## Catalogos, datos y generacion de cabecera (estado actual)

Estos son los documentos vigentes del modelo de datos y de la nueva logica de cabecera:

6. `docs/catalogos-y-plantillas.md`
   - Como editar los catalogos (CSV/Excel) y las plantillas (JSON). Refleja la unificacion:
     **horizontales y diagonales = una sola lista `truss-profiles.csv` (celosia/truss)** y
     **los refuerzos son postes** (`post-profiles.csv`); ya no existen `diagonal-profiles.csv` ni
     `reinforcement-profiles.csv`.

7. `docs/modelo-de-datos.md`
   - Como se conectan las tablas (FK) y como se cargan (`RackCatalog`), con diagrama ASCII + Mermaid.

8. `docs/generacion-cabecera-lateral.md`
   - **Logica nueva block-based** de cabecera lateral anclada a puntos de conexion (poste como base).
     Incluye el **handoff del "paso 2"**: lo que falta cablear en AutoCAD (comando del Plugin + drawer),
     a realizar por Claude local en Windows (el Plugin no compila en Linux).

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

