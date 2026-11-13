# Revisión directa de la sesión — I-64 F4-T3B1-EDIT-SURFACE, intento 2 (corrección)

- **Rango:** `1c69fbc4..1708e764`, con el RED `40f5727c` y el GREEN `1708e764`; 9 archivos del `AllowedWriteScope`. El GREEN no toca `tests/`.
- **CI del SHA exacto:**
  - RED `43380164657`: UI en rojo (20 de 352 en el filtro); Core en verde (2300/2300), como exige `ExpectRed` false;
  - GREEN `43380342743`: 4/4. Core 2300/2300 en el filtro y 17838/17838 en la suite; UI 352/352 en el filtro y 1989 aprobadas, 0 fallos, en la suite (Debug).
- **Modo:** lectura del diff frente al contrato del intento 2 y al análisis del intento 1.

## Veredicto

**Producto conforme.** El bloqueante queda resuelto y los menores corregidos, sin defectos nuevos.

## Lo comprobado

- **Bloqueante de teclado (INV-F4T3B1-04):**
  - se retira el atributo `KeyDown=` de la caja y el manejador `OnBrowserFieldBoxKeyDown`;
  - con la caja editable, el `PreviewKeyDown` solo marca Enter (con `CommitPending`) y Esc, y toda otra tecla llega al editor sin marcar;
  - con la caja en solo lectura, la tecla del primer gesto se consume (`Handled`) y no se aplica.
  - Las pruebas nuevas lo discriminan:
    - una tecla de carácter queda sin `Handled` ni en el túnel ni en la burbuja;
    - la caja editable responde a cada tecla exactamente como un `TextBox` sin manejadores;
    - la caja no tiene manejador de `KeyDown` de burbuja.

  Junto con el experimento de la sesión (`keyboard-experiment/`), esto prueba que el texto vuelve a llegar a la caja.
- **Menor 4 (INV-F4T3B1-03):**
  - `CanOpenByGesture` exige el botón «Editar», sin «Recargar», sin fallo y sin diagnóstico vigente;
  - el clic y la tecla pasan por `BeginFieldEditFromBox`, y el doble clic por `BeginFieldEditFromTab`, ambos con esa condición;
  - `e.IsRepeat` no lanza nada;
  - con un `NotAdmitted` vigente, solo «Editar» vuelve a leer;
  - con un Error, solo «Recargar»;
  - con la caja editable, el doble clic no hace nada y conserva el aviso.
- **Menor 8 (INV-F4T3B1-06):** el `catch` de `CrossObservedBoundaries` ya no pone en Error el campo de la superficie nueva.
- **Marca conservadora (INV-F4T3B1-07):** `IsProtected` devuelve true si `GetField` lanza.
- **Menor 2 (INV-F4T3B1-11):** `ThisTaskAddsNoFile` nombra como cota inferior los 5 archivos de `Drafts/` y los 9 de `Plugin/Workspace`, sin techo, y lleva su autoprueba.
- **Menores 3, 5 y 7:**
  - las guardas re-acotadas;
  - el doble aplica el `Proceeded`;
  - la prueba T06 recorre caminos reales (`Proceeded` y `Kept`);
  - la reentrada con la misma sesión;
  - T15 sin afirmar la ausencia de `Take*Notices`.
- **El intento 1 conserva lo que hacía bien:** las lecturas solo desde gestos, el binding y el setter, `TabChange`, las fronteras y «Nombre *».

## Observación

El Worker lanzó las suites completas en segundo plano pese al prompt; esperó su resultado antes de entregar. No afecta a la entrega. Es una lección conocida de esta unidad.

## Riesgos para Smoke-2

- las teclas de D-01 en el host, en edición y en solo lectura;
- `LostFocus` frente a un clic en el dibujo;
- el clic de pestaña tras un compromiso;
- la relectura del binding y el cursor tras una restauración.
