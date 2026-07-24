# I-31 — Validación manual en AutoCAD 2025 (Owner)

Estado: **APROBADA sin observaciones**. El dueño del repositorio validó en **AutoCAD 2025** el editor
**Selectivo migrado al shell visual común** (`RackEditorVisualShell`, I-30) y aprobó **los 12 puntos**
del checklist.

## Identificación

| Campo | Valor |
|---|---|
| Iniciativa | I-31 — Migración del editor Selectivo al shell visual común |
| Rama | `refactor/selective-visual-shell` |
| Claim-Id | `ab7853c9-0150-4db9-a331-92571fa6b6ab` |
| SHA validado | `b638653b10bdba5cd5c1d9f814f196c177f18c3e` |
| `AssemblyInformationalVersion` | `1.0.0+b638653b10bdba5cd5c1d9f814f196c177f18c3e` |
| DLL Debug | `<worktree>/src/RackCad.Plugin/bin/Debug/net8.0-windows/RackCad.Plugin.dll` |
| CI del candidato | run [`30108459424`](https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/30108459424) — **success** sobre `b638653` |
| Base | `origin/main = 40a2c8e` (no avanzó; **sin rebase final** — la aprobación de `b638653` es vigente) |
| Fecha | 2026-07-24 |
| Requisitos | `requires_autocad: true`, `requires_owner_validation: true` |

## Checklist aprobado (12 puntos, sin observaciones)

1. **Apariencia** del editor Selectivo compuesto sobre el shell, **alineada con el Dinámico** (panel lateral con scroll, matriz, preview, banda de estado y action bar por categorías).
2. **Matriz frente × nivel**: clic en celda; ± niveles por frente; altura por frente; casilla **'Piso'** (larguero a piso); medio frente.
3. **Selección de una sola celda** + **'Aplicar a:'** Celda / Nivel / Frente / Todas (comportamiento vigente; sin multiselección nueva).
4. **'Editando fondo'**: doble / triple / cuádruple profundidad con separadores, mediante el **selector de fondo**.
5. **Cabecera por poste** (Personalizar) y **peralte por poste**.
6. **Elementos de seguridad** (entran al BOM).
7. **Preview frontal**.
8. **Preview lateral**.
9. **Insertar frontal** en un rack nuevo (GUID nuevo).
10. **RACKEDITAR**: **Actualizar** en sitio (mismo GUID) + **Insertar lateral** + **Insertar planta** como vistas enlazadas (mismo GUID).
11. **Geometría y BOM sin diferencias**; metadatos y **persistencia I-11** preservados; biblioteca / legacy / **round-trip** íntegros (incluida la reapertura desde biblioteca).
12. **Estados habilitado/deshabilitado** con su **motivo por tooltip** y `ToolTipService.ShowOnDisabled` (Actualizar / Insertar lateral / Insertar planta gateados por origen AutoCAD y edición de existente).

## Resultado

El dueño **aprobó los 12 puntos sin observaciones**. Con ello los gates **`autocad`** y
**`owner-validation`** quedan **resueltos** (y `plugin_build` verde). Como `origin/main` no avanzó desde
`40a2c8e`, **no hubo rebase final** y el árbol validado coincide con el que se integra; el commit
documental de registro de la aprobación **no cambia `src/**`, `tests/**` ni el binario**. La iniciativa
pasa a `integration-ready` y se integra en `main` por `git merge --no-ff` en esta sesión.
