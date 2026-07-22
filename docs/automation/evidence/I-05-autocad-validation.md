# I-05 — Validacion manual en AutoCAD (APROBADA por el dueño)

> Gate **RESUELTO**. El dueño, Mario Perez, cargo el DLL Debug del worktree en AutoCAD 2025 y
> **aprobo sin observaciones** el 2026-07-22. Esta confirmacion resuelve los gates `autocad` y
> `owner-validation` de I-05.

## Identificacion

| Campo | Valor |
|---|---|
| Iniciativa | I-05 — Guardia de unidades |
| Rama | `feature/guardrail-unidades` |
| Implementacion validada | `f78baaf209c118d168c68620e236341996f9d93e` |
| AutoCAD | 2025 |
| Validador | Mario Perez, dueño del repositorio |
| Fecha | 2026-07-22 |
| Resultado global | **APROBADO** |

## DLL validado (NETLOAD), dentro del worktree de la iniciativa

```
D:\Documentos\Codex\Calculadora de racks-I-05-guardrail-unidades\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll
```

- Punta de codigo validada: **`f78baaf209c118d168c68620e236341996f9d93e`** (CI verde, cuatro jobs).
- `origin/main` no avanzo desde la base `9a895e4` al momento de la validacion, por lo que esta se
  conserva sobre el arbol vigente (WORKFLOW §6): sin rebase.

## Criterio general

I-05 añade una guardia de unidades **no bloqueante** en el limite de AutoCAD: al insertar un sistema o
una vista nueva, y en `RACKLAYOUT`/`RACKRELLENAR`, lee `INSUNITS` y avisa una sola vez si el dibujo no
esta en pulgadas. **No** convierte, reescala ni reinterpreta geometria (ADR-0005). El comportamiento
positivo/negativo, la exclusividad del lector de `INSUNITS`, la ausencia de transformacion geometrica y
el cableado de las rutas autorizadas estan cubiertos por `RackCad.Tests` (decision pura +
source-guards); el aviso real en el dibujo lo confirmo el dueño en AutoCAD.

## Hechos confirmados por el dueño (sin observaciones)

- [x] **Pulgadas** (`INSUNITS`=1): la insercion desde el menu `RACKCAD` y desde los comandos directos
      **no** muestra advertencia de unidades.
- [x] **No-pulgadas** (unidad metrica) y **unitless** (`INSUNITS`=0): aparece **una sola** advertencia
      por operacion; la insercion continua. Durante la prueba con un DWG no configurado en pulgadas el
      dueño confirmo que aparecio **correctamente la advertencia completa de RackCad**.
- [x] El aviso **no bloquea** el comando y **no convierte ni reescala** el dibujo (geometria igual que
      antes; solo aparece el mensaje).
- [x] **`RACKEDITAR`** diferencia correctamente **actualizacion en sitio** (no avisa) e **insercion de
      vista nueva** (avisa).
- [x] **`RACKLAYOUT`**, **`RACKRELLENAR`** y los **aliases** representativos se comportan correctamente:
      el aviso aparece antes del primer prompt en layout/relleno y **nunca** se emite dos veces por una
      misma operacion (los aliases no duplican el aviso).
- [x] No cambian dimensiones, posicion, GUID, BOM, capas ni el round-trip; layout y relleno conservan su
      comportamiento anterior.

## Resultado

- Estado: **APROBADO** (resuelve `autocad` y `owner-validation`).
- Fecha: **2026-07-22**.
- Codigo validado: **`f78baaf`**, DLL Debug del worktree I-05 (ruta arriba).
- Confirmacion del dueño: **«Ok, todo funciona»**. Verifico el comportamiento solicitado de
  pulgadas/no-pulgadas, actualizacion frente a insercion, comandos de layout/relleno y aliases, sin
  observaciones.
- Observaciones: ninguna.
