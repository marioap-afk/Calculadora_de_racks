# I-39D — Checklist de validación manual en AutoCAD 2025 (Owner)

> Estado: **APROBADA**. Registro factual del resultado proporcionado por el dueno; no incluye
> capturas ni detalles no proporcionados. Contrato:
> [`../../initiatives/I-39D-dialogos-y-utilitarias.md`](../../initiatives/I-39D-dialogos-y-utilitarias.md) ·
> Auditoría: [`I-39D-auditoria-dialogos-y-utilitarias.md`](I-39D-auditoria-dialogos-y-utilitarias.md) ·
> Base vs contrato: [`I-39D-caracterizacion-base-vs-contrato.md`](I-39D-caracterizacion-base-vs-contrato.md) ·
> Estado: [`../state/I-39D.yml`](../state/I-39D.yml)

## Resultado — EJECUTADA Y APROBADA (2026-08-07)

**`OWNER_APPROVED_I39D_MANUAL_VALIDATION`.** El Owner valido en **AutoCAD 2025** el checklist completo
de **24 puntos** sobre el DLL Debug del SHA candidato, y **todos se cumplen**. Sin observaciones y sin
rondas rechazadas.

El arbol validado es el que se integra: `origin/main` **no avanzo** desde la base `7eb96cb`, asi que
**no hubo rebase**, y desde el candidato `f513e12` no hay un solo cambio en `src/` ni en `tests/`
—verificado con `git diff`—, de modo que la aprobacion es transferible sin reservas.

## Artefacto validado

| Campo | Valor |
|---|---|
| Iniciativa | I-39D — Diálogos del arquetipo C, utilitarias del D y papel final de `RackDialogWindow` |
| Rama | `architecture/dialogos-y-utilitarias` |
| Claim-Id | `d0adcb2c-336e-4b24-82cc-1999ff80ec30` |
| DLL a cargar | `<worktree>\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll` |
| Worktree | `.claude\worktrees\architecture-dialogos-y-utilitarias` |
| **SHA candidato** | `f513e12751a1d5a03a32bd1d50ae345852ff2298` |
| SHA-256 del DLL | `E93653E12BA28CEEDA5735EEFA22F1335E10CEDDD0EF154D3BB0A4F150D54ED8` |
| `AssemblyInformationalVersion` | `1.0.0+f513e12751a1d5a03a32bd1d50ae345852ff2298` |
| CI del candidato | run [`32154543819`](https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/32154543819), ✅ 4/4 |

**El DLL es el del worktree de la iniciativa, no el del principal.** Cierra AutoCAD antes de recompilar.

## Qué cambió, y qué no

El checklist va **por familias**, no ventana por ventana: once de estas dieciséis comparten exactamente
el mismo contrato y repetir el punto once veces no añade evidencia.

**Cambió, y es lo que hay que validar:**

1. **`SafetyDefensaGridWindow` abre con el fondo compartido.** Era la única de los diez diálogos que no
   aplicaba el chrome: abría en blanco liso. Es su **único** delta.
2. **Las dos ventanas de almacén construyen su barra con la fábrica común.** Mismas etiquetas —«Colocar»
   y «Calcular», no «Aceptar»—, mismo padding, mismo teclado.
3. **El motivo de bloqueo de la barra de selección masiva ya se lee** con el botón apagado.
4. **El diagnóstico obsoleto se limpia** al revalidar, en defensa y desviador.
5. Por dentro: nueve diálogos toman su chrome de una sola fuente y `RackDialogWindow` se retiró. **No
   debería notarse nada**, y ese es justo el punto que hay que comprobar.

**No cambió**: geometría, BOM, persistencia, identidad, catálogos, reglas de producto, el contenido de
ninguna ventana, la ubicación de ninguna ventana, ni la paleta de colores del diagnóstico.

## Checklist

### A. Los seis subdiálogos de seguridad (`RACKSELECTIVO` → Seguridad → cada elemento)

| # | Punto | Resultado |
|---|---|---|
| 1 | Los seis abren centrados sobre su ventana padre, como siempre | APROBADO |
| 2 | Los seis se ven con el mismo fondo y la misma tipografía de siempre | APROBADO |
| 3 | **Defensa** abre ahora con el mismo fondo gris que sus hermanas, y no en blanco | APROBADO |
| 4 | En los cuatro de rejilla, la matriz, Todos/Ninguno y Aceptar/Cancelar siguen donde estaban | APROBADO |
| 5 | `Enter` sigue aceptando y `Escape` sigue cancelando en los seis | APROBADO |
| 6 | Aceptar devuelve la configuración al diálogo de seguridad, como antes | APROBADO |
| 7 | Cancelar la descarta, como antes | APROBADO |

### B. Selección masiva de las rejillas

| # | Punto | Resultado |
|---|---|---|
| 8 | Con un botón de alcance **apagado**, al pasar el ratón por encima aparece el motivo | APROBADO |
| 9 | Con el botón activo, aplicar el alcance sigue funcionando igual | APROBADO |

### C. Diagnóstico que ya no se queda pegado

| # | Punto | Resultado |
|---|---|---|
| 10 | En **defensa**, provoca un aviso (longitud cero), corrígelo y vuelve a aceptar: el aviso **desaparece** | APROBADO |
| 11 | En **desviador**, lo mismo: el aviso desaparece cuando el valor pasa a ser válido | APROBADO |

### D. Almacén (`RACKLAYOUT` y `RACKRELLENAR`)

| # | Punto | Resultado |
|---|---|---|
| 12 | Ambas abren con el mismo aspecto y el mismo tamaño de siempre | APROBADO |
| 13 | El botón primario sigue diciendo **«Colocar»** y **«Calcular»**, no «Aceptar» | APROBADO |
| 14 | `Enter` dispara el primario y `Escape` cancela | APROBADO |
| 15 | Colocar y Calcular siguen produciendo el mismo layout en el dibujo | APROBADO |
| 16 | Con un valor inválido, sigue avisando y **no** cierra | APROBADO |

### E. Utilitarias (`RACKCAD`, `RACKLISTA`, `RACKBOMTOTAL`, `RACKAYUDA`, BOM de un rack)

| # | Punto | Resultado |
|---|---|---|
| 17 | El menú principal abre igual y lanza los editores igual | APROBADO |
| 18 | La biblioteca de diseños, la lista de racks y las dos ventanas de BOM abren con el mismo tamaño de siempre | APROBADO |
| 19 | Exportar a Excel y a CSV sigue funcionando desde las dos de BOM | APROBADO |
| 20 | La ayuda de comandos abre igual | APROBADO |
| 21 | Ninguna de las seis ha ganado botones de Aceptar/Cancelar que antes no tuviera | APROBADO |

### F. Regresión general

| # | Punto | Resultado |
|---|---|---|
| 22 | Crear un rack selectivo completo con seguridad dibuja lo mismo que antes | APROBADO |
| 23 | Editar un rack existente por su GUID sigue funcionando | APROBADO |
| 24 | El BOM sigue dando los mismos totales | APROBADO |

## Veredicto

| Campo | Valor |
|---|---|
| Fecha | 2026-08-07 |
| Validador | dueno del repositorio |
| Resultado global | **APROBADO** — 24 de 24 puntos |
| Observaciones | ninguna |

**Token de aprobacion emitido**: `OWNER_APPROVED_I39D_MANUAL_VALIDATION`.

## Las tres cuestiones que quedan fuera de este cierre

Ninguna de las tres corrige un defecto observado, y por eso no se hicieron: mueven producto validado por
preferencia, no por contrato. **Quedan como hallazgo y deuda futura, no como trabajo pendiente de
I-39D**, y lo unico que el Owner ha decidido sobre ellas es **no incorporarlas a este cierre**; su
destino final sigue abierto.

1. **Ubicación de cinco ventanas.** `RackWarehouseLayoutWindow`, `RackWarehouseFillWindow`,
   `RackListWindow`, `RackConsolidatedBomWindow` y `RackCommandHelpWindow` declaran `CenterOwner` sin que
   pueda existir `Owner` —las abre un comando de AutoCAD—, así que WPF degrada la ubicación en silencio.
   Ponerles `CenterScreen` explícito **cambia dónde aparecen**.
2. **Paleta del diagnóstico.** Siete archivos usan `Firebrick` en vez del `#B00020` compartido.
   Unificarla cambia el color del aviso en siete ventanas.
3. **Foco inicial de las cuatro rejillas.** Hoy es emergente. Hacerlo determinista exige que
   `SelectionMatrix` acepte foco: es un cambio de **control**, no de arquetipo.
