# R20261105T050250Z-785a — Petición de decisión antes de seguir: SM-07 (2026-11-05 ~17:37Z)

AutoCAD operated by implementation session under Coordinator OPERATOR_EXCEPTION for this occurrence. La ocurrencia está en PAUSA en
SM-07:
- AutoCAD sigue abierto, solo con AUTHORIZED_AUTOCAD_PID 78512 (`Check PAUSA-SM-07` PASS, 17:37:26Z), y la vigilancia continúa;
- la sesión no toca AutoCAD hasta la decisión;
- ningún dibujo se guardó.

## Hechos
- SM-00a..SM-05 PASS. SM-03, crítica de C1: PASS, paleta flotante dentro de la pantalla.
- **SM-06 PASS**:
  - clic en un rack con identidad de A, el de abajo, el único que estaba en la vista;
  - el panel mostró «Un rack» y «Rack seleccionado: 54ef4061-d1f3-4191-9400-70fe08bafc0c», con las filas 1 `selection.read` y
    2 `drain`.
- **SM-07** pide «Añadir con clic la copia del mismo rack (mismo RackId)».
  - Tras alejar la vista con la rueda, solo vista, A muestra 3 racks: el seleccionado abajo y dos arriba.
  - Según el registro de a578, la pareja con el mismo RackId (`56ec2c3a-…`) son los dos de arriba, y el de abajo (`54ef4061`) no
    tiene copia. En a578 el SM-06 en orden fue sobre `56ec2c3a`, y `54ef4061` fue un clic de exploración anterior.
- Con la selección actual, SM-07 no se puede ejecutar tal como está escrito. Añadir un rack de arriba daría «2 racks», que es la
  expectativa de SM-08, no la de SM-07.

## Propuesta de la sesión (decide el Coordinator)
- **A (recomendada).**
  1. Repetir SM-06 sobre el rack de arriba a la izquierda: `Esc` y clic.
  2. SM-07 con el de arriba a la derecha (la copia).
  3. SM-08 con el de abajo.
  - La primera ejecución de SM-06 (`54ef4061`, PASS) se conserva tal como se registró, y la repetición se anota como incidente del
    operador. Es lo que se hizo en a578.
- **B.** STOP de la ocurrencia: cierre sin guardar y POST.
