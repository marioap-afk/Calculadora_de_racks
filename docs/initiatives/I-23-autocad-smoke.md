# I-23 — Smoke mínimo en AutoCAD (aprobado)

> Registro del smoke mínimo de I-23 ([contrato](I-23-namespaces-sistemas.md),
> [inventario](I-23-inventario-namespaces.md)). **APROBADO por el Owner**: registro factual del
> resultado que proporcionó; no incluye capturas ni detalles no proporcionados.
>
> **No** es la matriz completa de validación de una feature: I-23 es un refactor mecánico bajo
> congelación funcional que **no cambia dibujo, BOM, catálogos, comandos ni alias**. Lo que el smoke
> tiene que descartar es exactamente lo que un movimiento de namespace puede romper y ninguna prueba
> automatizada ve: que AutoCAD **cargue** el ensamblado, **descubra** los comandos y que las ventanas
> WPF **resuelvan sus recursos dentro del proceso de AutoCAD**, que no es el host STA de las pruebas.

## Artefacto validado

| Campo | Valor |
|---|---|
| AutoCAD | 2025 |
| Rama | `refactor/namespaces-sistemas` |
| Commit (SHA exacto) | `5d49a6cc990c5fc72e321aea37dd5bc2d3d4a128` |
| Base | `b43b5d15a242287ffe7514bc41e1086dd25e9387` |
| Divergencia del **candidato** | **11 ahead / 0 behind** sobre la base |
| Divergencia de la **punta documental** posterior | **12 ahead / 0 behind**; el commit extra es solo documentación (0 archivos de `src`, `tests`, `assets`, `deploy` o `.github`), así que el DLL validado sigue siendo el del árbol que se integra |
| Rebase | **no necesario**: `origin/main` no avanzó desde la base |
| Worktree | `~/.codex/worktrees/refactor-namespaces-sistemas` |
| DLL para `NETLOAD` | `src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll` |
| `InformationalVersion` | `1.0.0+5d49a6cc990c5fc72e321aea37dd5bc2d3d4a128` (los cuatro ensamblados) |
| SHA-256 `RackCad.Plugin.dll` | `D2944E25C20098CD57AA15DA143EB2C7412710ED61A78BE548B8CD87146D43EE` |
| SHA-256 `RackCad.UI.dll` | `457BD30540447E5725FD880F73B49DB0F8C13E5E40920623AD99EED80ED7EC0A` |
| SHA-256 `RackCad.Application.dll` | `3BA432196AA7FB8C574D7EEEF6F8BDFA2985C7CC1E37A2571E36633696FF1085` |
| SHA-256 `RackCad.Domain.dll` | `70A52FC474FBC165DC61F904B84ED0110E68229B092F06F1EC45CF10D31FCEBE` |
| CI de rama | run 30304742946, **verde 4/4** sobre este SHA |
| Builds locales | UI Debug 0/0; Plugin Debug 0 errores (solo las 2 `MSB3277` conocidas) |

Ruta absoluta del DLL en esta estación:

```text
C:\Users\alejandra-mendoza\.codex\worktrees\refactor-namespaces-sistemas\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll
```

> Cerrar AutoCAD antes de cualquier recompilación: con el plugin cargado el DLL queda bloqueado
> (trampa conocida de AGENTS.md). El DLL de arriba ya está compilado y sellado con el SHA exacto.

## Guion ejecutado y resultado

| # | Punto | Qué descarta | Resultado |
|---|---|---|---|
| 1 | `NETLOAD` del DLL de la tabla | Que el reparto de namespaces impida cargar el ensamblado o resolver sus dependencias | **Aprobado** — sin errores de carga |
| 2 | `RACKCAD` abre el menú principal | Descubrimiento de comandos y que el menú (que quedó en la raíz de `RackCad.UI`) siga resolviendo el registro de módulos | **Aprobado** — sin errores de comandos |
| 3 | Abrir **un** editor de sistema — `RACKPUSHBACK` (o `RACKSELECTIVO`) — y cerrarlo | Que una ventana movida a `RackCad.UI.Systems.<Sistema>` resuelva su XAML y `AppStyles` **dentro del proceso de AutoCAD** | **Aprobado** — sin errores de XAML ni de recursos |
| 4 | Abrir el **configurador de cabecera** (`RACKCABECERA`) y cerrarlo | Que `RackCad.UI.RackFrames` cargue: es la ventana con el `xmlns:frames` nuevo y la única que referencia dos namespaces de UI a la vez | **Aprobado** — sin errores de XAML ni de recursos |

No hace falta dibujar, insertar ni generar BOM: si algo del refactor estuviera mal, falla al **cargar
o al abrir**, no al calcular. La geometría y el BOM ya están fijados por los 7 goldens byte-idénticos
y por la superficie de API idéntica a la base.

## Por qué estos cuatro y no más

Lo que un refactor de namespaces puede romper y las pruebas **no** ven:

- **Carga del ensamblado** — punto 1.
- **Descubrimiento de comandos** — punto 2. Riesgo bajo y acotado: `[CommandMethod]` se descubre por
  atributo, no por namespace, y el inventario de los **28** comandos y alias es **byte-idéntico** al de
  la base (mismo comando, misma clase, mismo namespace `RackCad.Plugin`; ninguna clase de comando se
  movió). El punto 2 lo confirma en vivo.
- **Recursos WPF en el host real** — puntos 3 y 4. Las 494 pruebas de UI construyen las seis ventanas
  migradas, pero sobre el `StaTestRunner` propio del repo, no dentro de AutoCAD. Las URI son absolutas
  (`/RackCad.UI;component/...`), que es precisamente lo que las hace independientes de la carpeta, y hay
  una guarda que lo fija; aun así, el host real es el único sitio donde eso se demuestra.

## Observaciones

Ninguna. El Owner no reportó errores de carga, de comandos, de XAML ni de recursos.

## Resultado global

**Aprobado.** Con esto queda cerrado el único gate pendiente de I-23 y la iniciativa pasa a
`integration-ready`.
