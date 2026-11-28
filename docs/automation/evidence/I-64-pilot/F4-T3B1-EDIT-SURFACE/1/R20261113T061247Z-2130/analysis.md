# F4-T3B1-EDIT-SURFACE, intento 1: análisis de las revisiones y disposición

**Planificación:** `R20261113T045322Z-54f9`, fidelidad PASS a la primera, A1-A8 8/8, Horizonte Medium; nc4 `R20261113T045629Z-fca8`, solo A3 en fail.

**Entrega:**
- Worker `R20261113T045722Z-8515`: RED `f0637250` y GREEN `5de2fab3`.
- CI del GREEN `43373240897`: 4/4.
- CI del RED `43371973228`: UI en rojo, con 67 de 327 en el filtro; Core en verde (2298/2298), como exige `ExpectRed` false.

**Verificación del Controller:** `R20261113T061247Z-2130` (ver su `controller-verification`).

## Revisión directa de la sesión

**Producto NO conforme por un BLOQUEANTE de teclado:** la caja editable no admite texto escrito con el teclado. El resto es conforme. El informe está en `direct-review.md`, con el experimento de WPF en `keyboard-experiment/`.

## Revisión adversarial independiente

**Veredicto:** NO ACEPTABLE (1 bloqueante, 0 importantes, 7 menores). El informe completo está en `independent-review.md`.

| Id | Severidad | Hallazgo | Disposición |
|---|---|---|---|
| 1 | BLOQUEANTE | El `KeyDown` de burbuja de la caja editable anula el `TextInput` de toda tecla de texto. Lo exige la letra de INV-F4T3B1-04 | Corrección: sin `KeyDown` de burbuja; Enter y Esc en el `PreviewKeyDown`; prueba que discrimina; guarda |
| 2 | MENOR | El menor 2 de T3A quedó como un techo numérico (`<= 9`, `<= 5`) | Corrección: cota inferior que nombra los 5 archivos de `Drafts/` y los 9 de `Plugin/Workspace`, sin techo |
| 3 | MENOR | Guardas nuevas contra I-1: `TryCheckNow` prohibido en toda la UI, un solo `<TextBox` en todo el XAML y una lista blanca cerrada de miembros del controlador en la paleta | Corrección: limitadas a los cuerpos de T3B1 y en negativo |
| 4 | MENOR | Gestos que no dependen del estado: lecturas repetidas (repetición automática, flechas, Error por fallo) y el doble clic sobre una caja editable que borra el aviso | Corrección: los gestos de clic y tecla solo actúan cuando el campo ofrece «Editar»; sin repetición; con un `NotAdmitted` vigente de la misma superficie, solo «Editar» relee; el doble clic no hace nada con la caja editable |
| 5 | MENOR | Huecos de pruebas: la prueba 9 sin el clic ni la tecla; el doble clic abortado vacuo; el doble no aplica el `Proceeded`; `Dematerialize` con un borrador limpio; la reentrada no aísla el rack; pruebas que pasan en el RED presentadas como RED→GREEN; la prueba 16 sin edición | Corrección: todas |
| 6 | MENOR | Motivos del host en inglés y Error sin motivo | **A F4-T3C** (los mensajes del host); limitación del puerto de T3A |
| 7 | MENOR | Dos pruebas fijan que la presentación no consume los avisos, y R-B2-3 lo permite en T3B2 | Corrección: las pruebas fijan solo lo de T3B1 (ninguna lectura ni mutación), no la ausencia de `Take*Notices` |
| 8 | MENOR | Un fallo al cruzar la frontera de la superficie anterior se muestra en el campo de la nueva | Corrección: se contiene sin afectar al campo nuevo |

**Observación de la revisión directa:** `IsProtected` del navegador devuelve false si `GetField` lanza. Se corrige: true, que es lo conservador.

## Disposición

Decisión de la sesión, delegada por la orden de autonomía del Owner (`delegated-f4t3b1-correction-decision.md`): **corrección, intento 2 de 3**, con el contrato `gate-contract-attempt2.json`. Mantiene el mismo alcance de 19 rutas; los archivos creados por el intento 1 no son guardas ni pruebas existentes (F4T3B1-C02).
