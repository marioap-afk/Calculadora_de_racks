# Encargo: la v2.1 del paquete del contrato de PB-TC (la que se emite)

Lo da la sesión principal al redactor de la v1 y la v2.

**Entradas, en `pbtc/`:**
- la comprobación acotada de la v2, `independent-review-v2-check.md`, con el veredicto EMITIBLE TRAS CORRECCIONES 0/1/9;
- las decisiones literales del Owner, en `owner-pbtc-decisions.md`.

**Las reglas son las de siempre:**
- solo lectura en el worktree (HEAD `f9f2afbb`);
- sin compilar ni ejecutar;
- Python solo de lectura, escrito con la herramienta de archivos;
- sin rutas locales ni nombres de usuario o de máquina.

**Salidas:**
- `pbtc/I-64-f4b-pbtc-gate-contract-package-v21.md`. No sobrescribas ni la v1 ni la v2.
- Si mides algo nuevo, añade un apartado L a `pbtc-measurements.md`.

La v2.1 empieza, tras la cabecera, con un **registro de cambios v2 → v2.1**: una fila por cada N-xx y por cada decisión del Owner incorporada. Conserva después el registro v1 → v2.

## Lo que decidió el Owner (transcrito en `owner-pbtc-decisions.md`)

1. **OD-PBTC-1 = (i), con R-03 (i), la regla `Keep` de Cantilever, y la extensión declarada.** El §8 pasa a «decidido», cita el archivo de decisiones y conserva PBTC-C14.
2. **OD-PBTC-2 = (i).**
3. **OD-PBTC-3 = el texto corto, en L2.**
   - El literal queda fijado byte a byte, tal como está en el archivo de decisiones.
   - L1 se retira del diseño: queda solo como alternativa descartada, con una línea.
   - El texto largo se retira.
4. **OD-PBTC-4 = (A): el anexo B se incorpora** como corrección declarada nueva **D-PB-11** (H-PB-28).
   - Va completo al cuerpo del paquete: diseño, CT-PBTC-14, la sustitución S-04 de CT-PB0-05 con su literal de `Skip` y sus aserciones vieja y nueva, las rutas en `AllowedWriteScope` y fuera de `ForbiddenWriteScope`, `RequiredTests`, los invariantes, `ExpectedEvidence`, las paradas, el registro del §5.4 y las cifras del §6.4 y del §6.9.
   - Aplica antes N-06.
   - El `Objective` del §6.1 nombra D-PB-11.
   - Busca cualquier sitio de la v2 que diga «H-PB-28 queda fuera» (D-PBTC-07, INV-PBTC-15, PBTC-C10 y R-xx) y corrígelo.
5. **OD-PBTC-5 = (B): el anexo A NO entra.**
   - Se retira del paquete. Deja una sección corta que diga que el caso de H-31 queda registrado y vuelve al Owner antes del contrato de PB-T1 (OB-06.5), con el archivo de decisiones como autoridad.
   - Las cajas de H-31 siguen fuera del alcance: añádelas de forma explícita a INV-PBTC-15 y a PBTC-C10, para que el Worker no las toque.
   - El registro del §5.4 las lista como «pendiente: decisión del Owner antes de PB-T1».
6. **Las precondiciones de emisión:**
   - las decisiones quedan cumplidas por `F4B-DRAFTS/coordinator/owner-pbtc-decisions.md` (F4B-E06);
   - la precondición 2 la cumple la comprobación acotada de la v2 **más** una comprobación acotada de la v2.1, porque el anexo B entra en el cuerpo.

## Las correcciones de la comprobación acotada

- **N-01: la opción (a).** Las llamadas `SetNumber` de W:386-406 no cambian. Cada portador se asigna en una línea propia con la misma expresión; para el fondo basta `generalPallet.Depth`. INV-PBTC-03 (1) queda tal cual.
- **N-02:** `CATALOGO` solo para la fila F4 (`Structure.BeamDepth`). Las demás clasifican `REDONDEA` o `CONSERVA` como en CT-PB0-19. Corrige el §5.1 y renombra el método de CT-PBTC-01 para que no diga «Eleven»; por ejemplo, `CT_PBTC_01_TheLoadedFieldsKeepTheirExactValueAfterLoading`. Revisa el mapa CT → método.
- **N-03:** INV-PBTC-03 (2) dice «diez llamadas `Keep(` sobre esas diez cajas, y el cambio de `OptionalVal` (W:3625-3626)».
- **N-04:**
  - añade K4 (coordenadas de más de 0,001) a PBTC-C14;
  - en D-PBTC-10, «los cambios temporales» en lugar de «único cambio»;
  - usa `Assert.Fail(texto)`.
- **N-05:** «al menos una línea».
- **N-06:** las anclas del anexo B, corregidas antes de incorporarlo.
- **N-07:** se completa K-6 solo si sigue siendo pertinente sin el anexo A. Si no, anota en L que la medición de las cajas de H-31 queda para la vuelta al Owner antes de PB-T1, y conserva lo ya medido.
- **N-08:** decae con el anexo A fuera. Anótalo.
- **N-09:** el ancla de D-PBTC-09 pasa a `EditorWindowTestSupport.cs:89-95`.
- **N-10:** el §8.0 pasa a ser un resumen de lo **decidido**, en lenguaje llano y sin identificadores internos. Escribe «la escala de textos y cotas».

**Tu respuesta final** es corta: la ruta, las cifras finales (`AllowedWriteScope`, `ForbiddenWriteScope`, el filtro con `ExpectRed`, y el §6.9 para el RED y el GREEN), lo que no pudiste medir y cualquier contradicción con este encargo.
