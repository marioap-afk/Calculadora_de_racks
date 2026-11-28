# I-64 F4 — Hallazgo: el «Actualizar» del piloto redibuja sin el nombre del rack (2026-11-15)

**Origen:** el diagnóstico de solo lectura de RACKPROYECTAR (`rackproyectar-diagnostico.md`), pedido a raíz del reporte del Owner: «no se coloca el nombre de rack, niveles, cotas etc. … hasta que se da rackeditar y se actualiza ese rack». La sesión lo verificó en el código. Todavía **no se ha provocado en AutoCAD**.

## El mecanismo

| Paso | Evidencia (producto `b88dabf2`) |
|---|---|
| El diseño Push Back no guarda el nombre: las claves de `Design.PushBack.Structure` de un dibujo real no traen `Name`. El nombre solo está en el sobre (`Name`) | medido en D1 |
| `PushBackSystem.Name` es `Structure.Name` | `src/RackCad.Domain/Systems/PushBack/PushBackSystem.cs:140-143` |
| `PushBackResolver.Resolve(design)` no asigna el nombre | `src/RackCad.Application/Systems/PushBack/PushBackResolver.cs` (ninguna asignación de `Name`) |
| RACKEDITAR lo asigna a mano antes de redibujar: `system.Name = name` (y `= rackName` al insertar) | `src/RackCad.Plugin/RackPushBackCommands.cs:190`, `:98` |
| **El escritor del piloto** reconstruye con `PushBackPilotSystemRebuild.Build(member.DesignText, _catalog)` y **no asigna el nombre** | `src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs:118`; `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotSystemRebuild.cs` |
| La etiqueta del nombre solo se dibuja con `DrawRackName` y un nombre no vacío | `src/RackCad.Application/Systems/Dynamic/DynamicViewDecorations.cs:91` (y `:156`, `:256`); Push Back la usa en `PushBackSystemLateralBuilder.cs:213` |

**Consecuencia esperada:** en un rack con `DrawRackName = true`, como «PB G» de D1, un «Actualizar» del panel redibuja sus vistas en sitio sin la etiqueta del nombre. El resultado authored deja de ser equivalente al de RACKEDITAR.

**Por qué no lo vio nadie:**
- POST compara las claves, el texto del sobre y la escala, no la geometría (`PushBackPilotUpdateRun.cs:163-183`, `:478-487`);
- el escritor está en el Plugin, que ninguna suite puede cargar (ADR-0003), así que solo lo cubren guardas de texto;
- H0 usó un dibujo sintético.

**Confirmación física indirecta:** RACKPROYECTAR tiene el mismo patrón (reconstruye sin el nombre), y el Owner ve en AutoCAD que la vista proyectada sale sin nombre hasta que RACKEDITAR la actualiza.

**Alcance:**
- es un defecto del producto de I-64 (T4B, integrado por T4C) que **solo existe en la rama** `architecture/workspace-persistente-rackcad`. main (`812baf87`) no tiene el piloto;
- el defecto equivalente de RACKPROYECTAR sí está en main (I-55).

**Niveles y cotas:** el diagnóstico no encontró en el código por qué faltan en RACKPROYECTAR. En el D1 real, `Dimensions` = 0 (sin cotas). Smoke-2 lo observa en el paso 14.

**Tratamiento propuesto:** decisión del Owner. Es un modo de fallo nuevo (regla D2).
- **(a) [recomendada]:** una tarea de corrección pequeña antes de Smoke-2. El escritor asignaría el nombre del sobre al sistema antes de cada `PrepareRedraw`, como hace RACKEDITAR. Lleva su contrato, su RED con una prueba nueva en Application (por ejemplo, que la reconstrucción del plan lleve el nombre del sobre) y su cadena. Toca `RackPilotUpdateWriter.cs`, que tiene guardas de cuenta (G-5/G-9, D-T4C-14), o `PushBackPilotSystemRebuild`.
- **(b):** ejecutar Smoke-2 tal cual. El paso 14 daría FAIL del resultado authored, y la corrección vendría después.
