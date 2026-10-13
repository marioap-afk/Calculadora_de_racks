# I-62 — Nota: requisito mínimo de aislamiento preventivo para I-65

```text
Estado:     NOTA BREVE (decisiones de I-62 §82, punto 6). No es una enmienda, no implementa nada y no modifica I-65 ni la infraestructura
            compartida. Es información para el Coordinator y el Owner de I-65, que deciden con su propia autoridad.
Base:       diseño C1-C3 (c1c3-design.md, aceptado como MATERIAL sin implementación en §82, punto 6) y sus experimentos (experiments/).
Fuera:      A-6, FX-02 y la revisión de A-6. C1-C3 NO quedan implementados. I-65 NO queda desbloqueada.
```

## 1. Lo que §82 deja registrado y lo que implica para I-65

| §82.6 | Consecuencia para el aislamiento de I-65 |
|---|---|
| (a) C1 es principalmente detectivo y no garantiza descubrir escrituras transitorias | Una fotografía de checkouts antes y después de cada cesión **no** es aislamiento preventivo. Sirve para detectar y parar, no para impedir. Una escritura que se crea y se borra (como `placeholder.txt` en F4-T5) solo deja rastro en la fecha del directorio |
| (b) C2 debe garantizar el directorio efectivo desde un mecanismo de ejecución confiable | El directorio del proceso del Worker lo fija quien lo **lanza** (el transporte), y se comprueba antes de ceder. No vale un `cd`/`Set-Location` del Worker ni una instrucción del prompt: en `pwsh`, `Set-Location` no cambia el directorio del proceso, y una API .NET con ruta relativa escribe en el checkout de arranque (F4-T4, reproducido en exp-a) |
| (c) un C3 basado solo en el `PATH` es insuficiente frente a rutas absolutas | Quitar el intérprete del `PATH` del hijo y poner shims delante bloqueó 15 de 15 sondas por nombre, pero un intérprete invocado por ruta absoluta sigue ejecutándose (medido con un intérprete falso). Hace falta, además, un control que vea cada orden y la ejecución real |
| (d) los subagentes sin aislamiento propio por invocación no quedan habilitados | El Worker de I-65 en F4 fue un subagente (`claude-subagent`). Ese transporte hereda el directorio y el entorno de la sesión padre y no los fija por llamada, así que **no** cumple el requisito mínimo con el diseño actual |

## 2. Requisito mínimo propuesto (las cinco condiciones a la vez, antes de reanudar cualquier cesión a un Worker)

1. **Transporte con aislamiento por invocación.** El Worker se lanza como proceso hijo. El lanzador fija su `WorkingDirectory` (el worktree de
   la unidad o un directorio trampa vacío) y su entorno (`PATH` filtrado). Un Worker subagente que hereda la sesión padre no es elegible. Abrir la
   sesión padre ya en el worktree y con el entorno restringido podría volverlo elegible, pero es una acción reservada al Owner, y queda sin medir la
   tensión con P-08 (§5.2 del diseño).
2. **Comprobación antes de ceder, con fallo cerrado.** Inmediatamente antes de cada invocación, un hijo lanzado exactamente igual que el Worker
   informa su directorio de proceso (K-C2) y su resolución del intérprete (K-C3). Si algo no coincide, no se invoca.
3. **Barrera contra intérpretes que cubra las rutas absolutas.** El filtro de `PATH` y los shims (C3-P1 + C3-P2) son el mínimo, pero no bastan.
   Hacen falta también:
   - un control de cada orden que vea el ejecutable por nombre y por ruta, o un mecanismo de ejecución que deniegue por ruta;
   - el barrido de la transcripción tras cada Worker (C3-D3): cualquier ejecución de la familia prohibida sin bloqueo emparejado es STOP.

   El gancho previo es complemento y no barrera, porque el clasificador de texto falló 2 de 20 casos. Un mecanismo del sistema que deniegue por
   ruta es un ajuste del host, fuera de la autoridad de I-62. Si I-65 no dispone de ninguno de los dos, la invocación de un intérprete por ruta
   absoluta queda como **residual no prevenido**, que solo el barrido detecta, y I-65 debe decidir con su Owner si lo acepta.
4. **Escrituras fuera del worktree.** La prevención es la condición 1 (el origen medido fue una ruta relativa resuelta en otro checkout). Lo único
   preventivo frente a rutas absolutas sería un guardián de rutas para las herramientas de archivo (C2-P4, opcional y sin medir). La detección es C1
   en cada cesión, con barrido de directorios, `--no-optional-locks` y STOP ante cualquier cambio en otro checkout. Al ser detectiva, C1 no
   sustituye a la condición 1.
5. **Medir antes de confiar.** Lo que el diseño marca INFERENCE no se usa como control hasta medirlo: el `cwd` por entrada de la transcripción,
   la semántica del gancho previo y el alcance de los ajustes locales. Todo control sin medir cuenta como `UNVERIFIED` y no acredita aislamiento.

## 3. Lo que I-65 puede hacer sin I-62 y lo que no

- **Con la autoridad de su Coordinator** (16.5 admite condiciones STOP adicionales, que solo endurecen):
  - una condición STOP local ante cualquier cambio en otro checkout durante la cesión;
  - la fotografía C1 y el barrido de transcripciones registrados en su evidencia;
  - restringir su Worker a un transporte de proceso externo con las condiciones 1 y 2.
- **Reservado al Owner:**
  - abrir la sesión en el worktree o con un entorno restringido;
  - instalar ganchos o shims en ajustes persistentes;
  - cambiar alias, `PATH` del sistema, ACL o permisos.
- **Ni I-62 ni esta nota pueden:**
  - modificar I-65, I-61, los lanzadores compartidos ni los permisos del sistema;
  - declarar C1-C3 implementados;
  - desbloquear I-65.
