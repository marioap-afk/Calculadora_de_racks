# I-64 — F4-T3B1-EDIT-SURFACE: paquete de cierre

Lo prepara la sesión responsable el 2026-11-13, bajo la orden de autonomía del Owner (`F4-DRAFTS/coordinator/owner-overnight-autonomy-2026-11-13.md`). Las decisiones están en `I-64-pilot/F4-DRAFTS/f4-dispositions.md`, F4-E40 a F4-E43. La decisión de corregir es de la sesión, delegada por esa orden (`delegated-f4t3b1-correction-decision.md`). El cierre lo dispone la orden del Owner y del Coordinator `F4-DRAFTS/coordinator/owner-f4t3b1-close-disposition.md` (F4-E43), que autoriza las propuestas (a), (b) y (c) de la sesión (`delegated-f4t3b1-close-decision.md`).

Este paquete cierra una subtarea, no un gate. No declara GATE PASS.

## 1. Estado resultante

| Hecho | Valor |
|---|---|
| Producto | `1708e76428ba81012596b0c643074449a1db41f3`, CI `43380342743` push 4/4 |
| Verificación | intento 2: `EXECUTION_VERIFIED` / NONE, 14/14 |
| Cadena | ChainBaseSha `e0e0166b`; intento 1: RED `f0637250`, GREEN `5de2fab3`; intento 2: RED `40f5727c`, GREEN `1708e764` |
| Contratos | `gate-contract.json` (AuthorityRevision `495f10e6`) y `gate-contract-attempt2.json` (AuthorityRevision `9a552361`, la custodia del intento 1) |
| Intentos | 2 de 3; el intento 3 no se gasta en los menores (F4-E43) |
| Config | Snapshot G (`7C1516A9…`) en todas las invocaciones |
| Resultado | **CLOSED / PASS WITH LIMITATIONS** (F4-E43; limitaciones en el §5) |
| F4-T3 | T3A y T3B1 cerradas; T3B2 autorizada (1 de 3 al emitirse) y T3C en espera, en ese orden |

## 2. Planificación y corridas

| Corrida | Resultado | Notas |
|---|---|---|
| `54f9` | aceptada (fidelidad PASS a la primera, A1-A8 8/8, Horizonte Medium) | primera delegación; nc4 `fca8` con su oráculo |
| `8515` | entrega del intento 1 | RED `f0637250`, GREEN `5de2fab3` |
| `2130` | `EXECUTION_VERIFIED`, 14/14 | revisión directa con un BLOQUEANTE de teclado; independiente NO ACEPTABLE (1/0/7); el defecto era del contrato; decisión delegada: corregir |
| `4b5e` | aceptada (fidelidad PASS a la primera, A1-A8 8/8, Horizonte Medium) | corrección, intento 2 |
| `a807` | entrega del intento 2 | RED `40f5727c`, GREEN `1708e764` |
| verificación del intento 2 | `EXECUTION_VERIFIED`, 14/14 | revisión directa: producto conforme; re-revisión independiente ACEPTABLE CON MENORES (0/0/3); cierre por la disposición del Owner (F4-E43) |

## 3. Controles negativos (sobre las entradas de la verificación del intento 2, `R20261113T070017Z-c3a6`)

| Control | Corrida ordinaria | Resultado ordinario (se conserva tal cual) | Sustituto | Registro final |
|---|---|---|---|---|
| nc1 (CurrentSha falso) | `R20261113T070324Z-e470` | CONTROL NOT PASSED: Identity en pass ante el SHA falso; la entrega mutada no se verificó (REWORK, Handoff primera en fail) | F4T3B1-NC1-IDENTITY-SIGNAL-01 (`R20261113T111727Z-3c75`), sustituto NUEVO y local: PASS | SATISFIED_UNDER_F4T3B1_IDENTITY_SIGNAL |
| nc2 (alcance estrechado) | `R20261113T070654Z-5b85` | CONTROL NOT PASSED: firma de PD-F4-01 | F4T3B1-NC2-EXACT-WHITELIST-01 (`R20261113T071219Z-8c85`), por la regla de nc2 de F4-E39: PASS | SATISFIED_UNDER_DETERMINISTIC_SCOPE_SUBSTITUTE (su resultado lo registra como SATISFIED_UNDER_F4T3B1_EXACT_WHITELIST_SUBSTITUTE) |
| nc3 (término de gate en `WorkCompleted`) | `R20261113T071151Z-ad7c` | CONTROL NOT PASSED: FreeText en fail y REWORK, pero Ci primera en fail por la variación PD-F4-06 | F4T3B1-NC3-FREETEXT-DETECTOR-01 (`R20261113T111731Z-8a67`): PASS | SATISFIED_UNDER_F4T3B1_FREETEXT_DETECTOR |

- Ninguna corrida ordinaria se reinterpreta como CONTROL PASS.
- **nc1:** el sustituto NO es la firma de PD-F4-04 ni el sustituto de identidad de F4-T2B2B o F4-T2C. Prueba las 12 propiedades mínimas de la disposición, más dos de la sesión. Entre ellas:
  - el SHA falso no es el CurrentSha real, ni HEAD, ni la rama remota, y no existe en la base de objetos;
  - las relaciones BaseSha, RedSha y CurrentSha siguen válidas;
  - el Controller no verificó la entrega mutada, y su evidencia de Handoff nombra el SHA falso y el real;
  - el predicado determinista de identidad da PASS con el SHA real y FAIL con el falso.
  - Autoprueba: 26 casos, deterministas.
- **nc3:** el detector usa exactamente los campos y términos de 16.10 en la AuthorityRevision `9a552361`. Sus 49 pruebas son deterministas. Da la entrega real CLEAN y la mutada VIOLATION (solo «GATE PASS» en `WorkCompleted`).
  - La señal FreeText del Controller también detectó la violación.
  - La variación de Ci y Trailer no la causa la mutación: su evidencia no la nombra, y las mismas fallas aparecen en nc1 y nc2, que no tocan FreeText.
- Los sustitutos están en `F4-T3B1-EDIT-SURFACE-nc1-substitute/2/`, `F4-T3B1-EDIT-SURFACE-nc2-substitute/2/` y `F4-T3B1-EDIT-SURFACE-nc3-substitute/2/`, cada uno con su script y su resultado.

## 4. Producto entregado

**La superficie de edición del campo `AnnotationScale`** del piloto de Push Back en la superficie K = 1 del navegador del panel, sobre el puerto de F4-T3A. Sin escritura authored, sin cambios en el host, el controlador, el puente ni `BrowserSession`, y sin `+=`, `Dispatcher` ni API de foco nuevos.
- **Cableado:** la paleta pasa `_controller.PilotEdit` al VM del panel, que lo pasa al navegador. Sin el puerto, el navegador es el de antes.
- **`WorkspacePilotFieldViewModel`** (archivo nuevo):
  - es editable solo con borrador, `CanEdit`, pestaña fijada y sin un Limpio posiblemente obsoleto;
  - se renderiza desde `GetField` al final de cada `Rebuild`, conservando el aviso;
  - el primer gesto («Editar», el clic, la pulsación o el doble clic en la pestaña activa) fija la preview y pide la intención sin testigo, con la recarga perezosa y la comprobación de reentrada;
  - los gestos dependen del estado;
  - el setter del texto es la única entrada de edición (`SetPendingText`, restauración tras un rechazo, contención propia);
  - `CommitField` con Enter o al salir de la caja, solo con pendiente y caja editable;
  - Error con «Recargar» desde la memoria;
  - «No editable aquí» con la indicación de RACKEDITAR.
- **Navegador:**
  - `TabChange` bloqueante con pendiente;
  - `DocumentChange` y `Dematerialize` solo en la presentación;
  - «Nombre *» y la fijación de una preview protegida, conservadora si el puerto falla.
- **Vista:**
  - la caja con binding bidireccional;
  - sin `KeyDown` de burbuja, con Enter y Esc en el `PreviewKeyDown` de la caja editable;
  - el primer gesto por tecla consumido en la caja en solo lectura, sin repetición.
- **Cambios declarados:** G-01 a G-04 con su texto nuevo, y los arrastres de F4-T3A (menores 1, 2, 3, 5 y 7, y N-5).

## 5. Limitaciones y abierto

**Menores → F4-T3B2** (disposición del Owner, F4-E43; el paquete de F4-T3B2 los vincula de forma explícita):
- N-1: el doble del puerto aplica el pendiente de un borrador no editable en `TabChange`; la mitad `Kept` de la prueba 6 debe recorrer el camino del host;
- N-3: las repeticiones de una tecla mantenida sobre la caja en solo lectura se escriben una vez abierta.

**Limitación declarada:** N-2 (comportamiento de la tecla Space). Space lo marca el propio `TextBox`; la prueba lo compara con un `TextBox` sin manejadores, que es correcto en sustancia.

**Controles:** nc1, nc2 y nc3 se satisfacen con sustitutos deterministas, no con su oráculo ordinario (§3).

**Deudas de protocolo** (F4-E42, F4-E43; el AUTOMATION_PLAN no cambia):
- PD-F4-05: el Controller puede informar Identity en PASS ante un CurrentSha deliberadamente falso mientras rechaza la entrega por otra comprobación;
- PD-F4-06: los controles negativos pueden variar Ci y Trailer, sin relación con la mutación, respecto de la misma verificación real. Es solo deuda de protocolo: no se usa para debilitar Ci ni Trailer en la verificación real del producto.

**Menor 6 de la revisión del intento 1 → F4-T3C:** los motivos del host en inglés y el Error de un borrador sin motivo (limitación del puerto de F4-T3A).

**Arrastres anteriores que siguen vivos:**
- de F4-T3A a F4-T3C, los menores 4 y 6;
- R-T2C-01, DIFERIDO a F6/F7.

**Interinos hasta F4-T3B2:**
- cerrar una pestaña no descarta su borrador;
- una selección cambia la pestaña activa también con pendiente (sin pérdida: el pendiente vive en el borrador).

**Smoke-2:**
- las teclas de D-01 en el host (las que el `TextBox` no consume pueden llegar a AutoCAD);
- `LostFocus` frente a un clic en el dibujo;
- el clic de pestaña tras un compromiso que cambia «*»;
- la relectura del binding y el cursor;
- la reentrada si `ReadIntent` bombea mensajes.

**Lección registrada:** una mecánica de entrada de WPF se prueba con mensajes reales antes de fijarla en un contrato.

**Sin AutoCAD:** pruebas de VM y de vista (STA) con dobles; el Plugin, con guardas de texto y el build Release de la CI.
