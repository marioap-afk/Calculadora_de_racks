# I-39D — Checklist de validación manual en AutoCAD 2025 (Owner)

> Estado: **PENDIENTE**. Contrato:
> [`../../initiatives/I-39D-dialogos-y-utilitarias.md`](../../initiatives/I-39D-dialogos-y-utilitarias.md) ·
> Auditoría: [`I-39D-auditoria-dialogos-y-utilitarias.md`](I-39D-auditoria-dialogos-y-utilitarias.md) ·
> Base vs contrato: [`I-39D-caracterizacion-base-vs-contrato.md`](I-39D-caracterizacion-base-vs-contrato.md) ·
> Estado: [`../state/I-39D.yml`](../state/I-39D.yml)

## Artefacto a validar

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
| 1 | Los seis abren centrados sobre su ventana padre, como siempre | |
| 2 | Los seis se ven con el mismo fondo y la misma tipografía de siempre | |
| 3 | **Defensa** abre ahora con el mismo fondo gris que sus hermanas, y no en blanco | |
| 4 | En los cuatro de rejilla, la matriz, Todos/Ninguno y Aceptar/Cancelar siguen donde estaban | |
| 5 | `Enter` sigue aceptando y `Escape` sigue cancelando en los seis | |
| 6 | Aceptar devuelve la configuración al diálogo de seguridad, como antes | |
| 7 | Cancelar la descarta, como antes | |

### B. Selección masiva de las rejillas

| # | Punto | Resultado |
|---|---|---|
| 8 | Con un botón de alcance **apagado**, al pasar el ratón por encima aparece el motivo | |
| 9 | Con el botón activo, aplicar el alcance sigue funcionando igual | |

### C. Diagnóstico que ya no se queda pegado

| # | Punto | Resultado |
|---|---|---|
| 10 | En **defensa**, provoca un aviso (longitud cero), corrígelo y vuelve a aceptar: el aviso **desaparece** | |
| 11 | En **desviador**, lo mismo: el aviso desaparece cuando el valor pasa a ser válido | |

### D. Almacén (`RACKLAYOUT` y `RACKRELLENAR`)

| # | Punto | Resultado |
|---|---|---|
| 12 | Ambas abren con el mismo aspecto y el mismo tamaño de siempre | |
| 13 | El botón primario sigue diciendo **«Colocar»** y **«Calcular»**, no «Aceptar» | |
| 14 | `Enter` dispara el primario y `Escape` cancela | |
| 15 | Colocar y Calcular siguen produciendo el mismo layout en el dibujo | |
| 16 | Con un valor inválido, sigue avisando y **no** cierra | |

### E. Utilitarias (`RACKCAD`, `RACKLISTA`, `RACKBOMTOTAL`, `RACKAYUDA`, BOM de un rack)

| # | Punto | Resultado |
|---|---|---|
| 17 | El menú principal abre igual y lanza los editores igual | |
| 18 | La biblioteca de diseños, la lista de racks y las dos ventanas de BOM abren con el mismo tamaño de siempre | |
| 19 | Exportar a Excel y a CSV sigue funcionando desde las dos de BOM | |
| 20 | La ayuda de comandos abre igual | |
| 21 | Ninguna de las seis ha ganado botones de Aceptar/Cancelar que antes no tuviera | |

### F. Regresión general

| # | Punto | Resultado |
|---|---|---|
| 22 | Crear un rack selectivo completo con seguridad dibuja lo mismo que antes | |
| 23 | Editar un rack existente por su GUID sigue funcionando | |
| 24 | El BOM sigue dando los mismos totales | |

## Veredicto

| Campo | Valor |
|---|---|
| Fecha | |
| Validador | |
| Resultado global | |
| Observaciones | |

**Token de aprobación**: `OWNER_APPROVED_I39D_MANUAL_VALIDATION` (solo si **todos** los puntos se cumplen).

## Decisiones que I-39D deja al Owner, y que NO están en este checklist

Ninguna de las tres corrige un defecto observado, y por eso no se hicieron: mueven producto validado por
preferencia, no por contrato. Van aquí para que se decidan, no para que se validen.

1. **Ubicación de cinco ventanas.** `RackWarehouseLayoutWindow`, `RackWarehouseFillWindow`,
   `RackListWindow`, `RackConsolidatedBomWindow` y `RackCommandHelpWindow` declaran `CenterOwner` sin que
   pueda existir `Owner` —las abre un comando de AutoCAD—, así que WPF degrada la ubicación en silencio.
   Ponerles `CenterScreen` explícito **cambia dónde aparecen**.
2. **Paleta del diagnóstico.** Siete archivos usan `Firebrick` en vez del `#B00020` compartido.
   Unificarla cambia el color del aviso en siete ventanas.
3. **Foco inicial de las cuatro rejillas.** Hoy es emergente. Hacerlo determinista exige que
   `SelectionMatrix` acepte foco: es un cambio de **control**, no de arquetipo.
