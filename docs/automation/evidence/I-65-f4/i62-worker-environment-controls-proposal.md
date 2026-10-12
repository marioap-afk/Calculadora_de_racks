# Propuesta para I-62: controles preventivos del entorno del Worker (invocaciones de intérpretes no autorizadas)

- **Estado:** SUSTITUIDA el 2026-10-12 por el [paquete de escalamiento](i62-escalation-package.md); se conserva como antecedente.
- **Estado:** PROPUESTA. No adoptada. Preparada desde I-65 por orden del Owner («Owner Disposition: F4-T3 C-08 and Codex Budget», punto 5).
- **Destinatario:** I-62 (infraestructura de ejecución delegada). **Este documento no modifica** la infraestructura de I-61/I-62, sus
  esquemas, sus scripts ni sus prompts; solo registra el problema y propone opciones para que I-62 las evalúe con su propio proceso.
- **Fuente de hechos:** `docs/automation/decisions/I-65.md` (CD-I65-F3-12, DEV-I65-F4-01, DEV-I65-F4-05, CD-I65-F4-14, DEV-I65-F4-06) y la
  evidencia custodiada de I-65.

## 1. Problema recurrente

Los Workers delegados (subagente `claude-sonnet-5-5` de la sesión de escritorio) invocan un intérprete Python aunque el prompt lo prohíbe.
En I-65 ocurrió cinco veces:

| Registro | Tarea | Qué se ejecutó | Momento | Efecto observado | Tratamiento |
|---|---|---|---|---|---|
| DEV-I65-F3-02 / CD-I65-F3-12 | F3 | Python para leer `p1-report.json` | durante el trabajo | ninguno en producto, pruebas ni evidencia | desviación no bloqueante (C-08 cubría solo la dependencia) |
| DEV-I65-F4-01 | F4-T1 | dos invocaciones triviales para comprobar si existía | durante el trabajo | ninguno | no bloqueante; prompts endurecidos |
| DEV-I65-F4-05 | F4-T2 (corrección) | `python3 --version` dentro de una orden `grep`, salida descartada | durante el trabajo | ninguno | no bloqueante; C-08 de T3-T5 ampliada a «cualquier motivo» |
| DEV-I65-F4-06 | F4-T3 | `python --version 2>$null \| Out-Null` al final de una comprobación del texto libre | después de los commits RED y GREEN | ninguno (árbol limpio, CI verde) | C-08 activada → STOP del Worker → verificación incoherente → disposición del Owner |
| DEV-I65-F4-14 | F4-T5 | `python3 --version 2>/dev/null` | antes de cualquier commit (con la prohibición expresa y la aclaración del caso de F4-T3 en el prompt) | ninguno | C-08 (junto con C-05) → STOP del Worker → S-11 → Owner |

Patrón: el modelo añade por hábito sondas de disponibilidad de herramientas. La prohibición textual (prompt y condición de parada) redujo
la frecuencia y el alcance, pero no la eliminó.

## 2. Dos clases distintas

**A. Infracción de procedimiento sin efectos observados.** La invocación no ejecutó código del repositorio ni del producto (plantillas,
scripts), no leyó ni escribió archivos que el producto, las pruebas o la evidencia usen, no instaló nada, no usó la red, no cambió el
árbol, los commits ni la CI, y su salida no se usó como evidencia. Se demuestra con la transcripción (orden exacta y momento), el diff,
el árbol limpio y la CI del SHA exacto.

**B. Violación con consecuencias reales.** Cualquiera de: ejecutar una plantilla o un script del producto; generar o modificar con el
intérprete archivos del alcance, pruebas o evidencia; hacer que el producto o las pruebas dependan del intérprete; usar su salida como
evidencia; instalar paquetes o cambiar el entorno; tocar la red; ejecutar el programa de análisis.

Hoy las dos clases caen en la misma condición de parada de contrato (C-08), y el Controller no dispone de una vía coherente con README §8
para verificar una entrega cuya única incidencia es de clase A: el caso de F4-T3 terminó en una salida con las 14 comprobaciones en
`pass` y `EXECUTION_BLOCKED/STOP`, inválida por la regla de coherencia, y necesitó una disposición del Owner.

## 3. Opciones preventivas para que I-62 las evalúe

1. **Bloqueo en el entorno (preferente).** Lanzar el Worker con un entorno en el que el intérprete no sea invocable (por ejemplo, `PATH`
   sin los directorios del intérprete y sin los alias de ejecución de aplicaciones de Windows), o con un gancho previo a cada orden que
   rechace las órdenes que invoquen un intérprete cuando la delegación lo prohíbe. Definir antes cómo se clasifica ese rechazo, para que
   no se confunda con una denegación de permisos (comprobación `Denials`).
2. **Detección mecánica en el relevo.** Añadir al registro del trabajo una señal que enumere las invocaciones de intérpretes de la
   transcripción (orden exacta, momento relativo a los commits y archivos tocados), para que la clase A o B se decida con hechos y no con
   lo que declare el Worker.
3. **Separar las condiciones del contrato.** Mantener como parada solo la clase B y tratar la clase A como desviación de procedimiento con
   disposición predefinida (registro obligatorio, sin STOP), o bien definir en el protocolo cómo una parada resuelta sin cambio del trabajo
   llega a una verificación coherente. README §9 solo cubre «STOP resuelto por el Coordinator con un cambio del trabajo».
4. **Prompt.** Mantener la prohibición explícita y la instrucción de no sondear la disponibilidad de herramientas (salvo `git` y `dotnet`).
   Es una medida complementaria, no suficiente por sí sola.

## 4. Otro problema recurrente del mecanismo (Controller, no entorno del Worker)

En F4-T3 el control negativo nc2 no discriminó una mutación de `Scope` (DEV-I65-F4-08): con una delegación que dejaba fuera un archivo del
diff, el Controller afirmó que un prefijo ya retirado lo cubría y devolvió `EXECUTION_VERIFIED`. Es el mismo modo de fallo que I-63
DEV-G2-01, y ocurrió pese a la instrucción del prompt de comparar con la delegación de entrada. La comprobación mecánica de la sesión
(diff ⊆ `AllowedWriteScope`, intersección vacía con `ForbiddenWriteScope`) sí lo detecta. Opción para I-62: convertir esa comprobación
mecánica en una señal del relevo «sesión → Controller», como las de `Termination` o `Remote`, en lugar de depender de la lectura del
Controller.

## 5. Escrituras fuera del worktree (F4-T4 y F4-T5)

- **F4-T4, intento 2 (violación con consecuencias reales, no declarada):** el Worker reescribió el `RackCad.sln` del checkout principal del
  Owner. En PowerShell hizo `Set-Location` al worktree y después `[IO.File]::ReadAllText/WriteAllText` con una ruta relativa, que .NET resuelve
  contra el directorio del proceso (el checkout principal, que es el directorio por defecto de las herramientas de la sesión). Nadie lo detectó
  durante la cesión ni en la verificación: los hechos del relevo solo miran el worktree. Se descubrió una hora después.
- **F4-T5 (violación transitoria, declarada):** la herramienta de escritura creó `placeholder.txt` en el checkout principal; el Worker lo borró
  en 3 s y paró por C-05.

Opciones para I-62: (a) lanzar al Worker con el directorio de proceso en el worktree o en un directorio vacío que no sea otro checkout;
(b) añadir a los hechos de salida y de entrada del relevo el estado de Git del checkout principal (y de cualquier otro checkout del mismo
repositorio), de modo que un cambio durante la cesión sea una señal mecánica; (c) en el prompt, exigir rutas absolutas en toda API de archivos
(.NET, PowerShell y Bash). La opción (b) habría detectado el caso de F4-T4 en la misma cesión.

## 6. Fuera de alcance

Nada de esto se implementa desde I-65. C-08 sigue vigente y sin debilitar en I-65. La aceptación de DEV-I65-F4-06 por el Owner se limita a
la ejecución histórica de F4-T3.
