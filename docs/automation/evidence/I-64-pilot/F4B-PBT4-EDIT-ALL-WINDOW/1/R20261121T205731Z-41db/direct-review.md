# PB-T4c: revisión directa de la sesión (intento 1, Worker `R20261121T205731Z-41db`)

**Qué se revisa:** el RED `280c79beac61d31b03f86488c03686eef4f694bc` y el GREEN `7cc1076eae8159294a1498ab556851977223b64c`, sobre la BASE `d5026014` (F4B-E54), sin reparación del RED. La lectura va por objetos git y, ya terminado el Worker, por la copia exportada de la precondición 6.

**Resultado: conforme, con una desviación que el Owner ratificó.** La revisión independiente (ACEPTABLE CON MENORES 0/1/7, más OWNER-01 a OWNER-05) cubre el detalle, y la sesión no añade nada que la contradiga.

| Qué | Resultado |
|---|---|
| **El RED** (`d5026014..280c79be`) | 21 rutas, +2 934/−4: el esqueleto, que escribe el generador custodiado `green_pbt4c_v51.py`, igual al registro de la precondición 6; los 4 `Skip`; los 4 equivalentes; las cinco clases de las CT |
| **El GREEN** (`280c79be..7cc1076e`) | 10 archivos de `src/`, +250/−13. **Los 10 son iguales, byte a byte (con los saltos de línea a `\n`), al GREEN literal** que el generador aplicó en la copia `r5j` de la precondición 6 de la v5.1. Ningún archivo de `tests/` ni de `Pilot/` |
| **CI del RED** (`44956801926`) | tests y ui-tests en failure; build-ui en success; build-plugin omitido. Core 19 619 = 19 149 + 9 + 461; UI 2 486 = 2 432 + 5 + 49. Es la rama prevista (F4B-E46) |
| **CI del GREEN** (`44959653142`) | 4/4. Core 19 619 = 19 158 + 0 + 461; UI 2 486 = 2 437 + 0 + 49. Exacto |
| **El relevo del trabajo** | Válido: la forma del RED, el esqueleto, los `Skip`, los `EquivalentSha` y cero `warning CS` |
| **Las desviaciones de procedimiento declaradas** | La suite local de Core en tres particiones disjuntas, con suma exacta, y más tiempo para una clase lenta tras dos cortes del detector de cuelgue local. La revisión independiente las da por procedimiento local, que el contrato no regula. La prueba ya era lenta en PB-T4b: 276 s en la CI de PB-T4b; 321 s y 188 s en las de PB-T4c |
| **La desviación no declarada** | CT-PBT4C-11, segunda receta: sustituye el informe de módulos y no la línea base (I-01 de la revisión independiente). **Owner: «Ratificar como errata (Recomendado)»** (`owner-pbt4c-code-decisions.md`, pregunta 1) |
| **Trailer** | `Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>` en los dos commits |

**Lo que la sesión no puede ver sin AutoCAD,** y queda para el Smoke-PB:
- la ventana de Push Back abierta en WORKSPACE desde «Editar todo», modal, con la paleta deshabilitada;
- el foco y el teclado;
- «Actualizar» tras editar;
- «Editar todo» sobre un borrador Clean posiblemente obsoleto (OWNER-02);
- el BOM y «Guardar en biblioteca» desde esa ventana (OWNER-03);
- «Cerrar» con la pregunta combinada y el aviso tras cerrar (OWNER-04).
