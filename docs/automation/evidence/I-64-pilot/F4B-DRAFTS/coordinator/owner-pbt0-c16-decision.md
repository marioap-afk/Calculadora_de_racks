# Decisión del Owner sobre la desviación c-16 de CT-PB0-17 (F4B-PBT0-CHARACTERIZATION, intento 1)

Fecha: 2026-11-17 (chat, AskUserQuestion). Texto literal de la pregunta y de la respuesta.

**Pregunta:**

> PB-T0 (las pruebas que fijan cómo funciona hoy el editor Push Back): el Worker añadió por su cuenta un caso a una de las 21 pruebas (tipo de cotas = 3) porque los casos del plan nunca podían detectar uno de los 20 errores de control. El contrato decía que en ese caso debía PARAR. El código es correcto y las dos revisiones confirman que el caso añadido es bueno; la revisión independiente lo marca como importante solo porque esa decisión no le tocaba al Worker. ¿Qué hago?

**Opciones ofrecidas:**
- «Ratificar el cambio (Recomendado)»: Se acepta el caso añadido como enmienda de la prueba y se sigue: verificación del Controller, controles y cierre de PB-T0. Los 6 menores se registran para tareas siguientes. Sin gastar intento.
- «Tratarlo como parada»: Se aplica la parada del contrato: se corrige el contrato con ese caso y se repite PB-T0 como intento 2 de 3. El resultado esperado son las mismas pruebas; cuesta un intento y unas horas.

**Respuesta del Owner:** «Ratificar el cambio (Recomendado)».

## Alcance de la ratificación (lectura de la sesión, sin ampliar)

- Se ratifica como enmienda de CT-PB0-17 y de la fila MU-PB0-16:
  - el caso c-16 (`Dimensions = 3`, el límite válido), en el que se apoya MU-PB0-16;
  - la forma `RECHAZADO <tipo>: <mensaje>` de c-12 y c-13. Es la misma desviación de CT-17 que la opción describe como «el caso añadido» y su consecuencia; las dos revisiones la tratan dentro de OWNER-1.
- PBT0-C10 no se aplica a este intento. No gasta intento: attempts sigue en 1 de 3.
- Se registran para tareas siguientes, sin tocar las pruebas tras el RED:
  - los menores M-01 a M-06 de la revisión independiente;
  - los menores D-2 a D-5 de la directa.
- La cita `DynamicRackSystemDocument.cs:322` de la entrega se corrige en el registro: es `:227` y `:429-432` (M-02).
- Defecto de la sesión, registrado: la cláusula «corrige la prueba si no caracteriza bien» del paso 3 del prompt del Worker daba una latitud que el contrato no tenía.
- No decide nada sobre OWNER-2, OWNER-3 ni OWNER-4. Vuelven al Owner en sus puntos previstos (antes de PB-TC y antes del contrato de PB-T1, OB-06.4 y OB-06.5).
