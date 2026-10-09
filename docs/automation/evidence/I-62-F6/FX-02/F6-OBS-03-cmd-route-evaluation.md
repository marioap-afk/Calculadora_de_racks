# F6-OBS-03 — ¿Satisface la ruta `cmd.exe` el contrato de ejecución de FX-02? (evaluación con evidencia ya custodiada; decisiones §60, punto 2)

**Clasificación** (decisiones §60): limitación material observada de `codex-cli 0.162.0-alpha.2` en la ejecución de PowerShell. OD-2 está aceptada
(decisiones §59) y no se vuelve a pedir mientras sus invalidadores sigan iguales. Evidencia: bloque A2-P2,
[result.json](../OD-2/R20261009T041427Z-a2p2-block/result.json) y eventos de las dos sondas (evidencia §93).

## 1. Hechos (solo evidencia custodiada)

| Hecho | Fuente |
|---|---|
| La receta 16.4 se aplicó literal: binario por ruta verificada, `-C`, `-s read-only`, `-m` y effort de la celda, `--output-schema`, `-o`, `--json`, sin `--ephemeral`, stdin cerrado y el `pwsh` del runtime primero en el `PATH`. El hecho `RuntimeShellFirstInPath` del descriptor vale YES | `run_block.sh`; descriptor `codex-cli`, operación 3 y tabla de hechos |
| La CLI 0.162.0-alpha.2 no usa ese `pwsh`: lanza el alias `%USERPROFILE%\AppData\Local\Microsoft\WindowsApps\pwsh.exe`, que bajo `read-only` falla con `CreateProcessAsUserW … Acceso denegado (5)` | `probe1-events.jsonl`, `probe2-events.jsonl` |
| **Sonda 1, celda del Controller** (`gpt-6-luna/high`, invocación medida de la celda: decisiones §55, U-02(b)). Hizo 19 comandos: 3 intentos con el alias, todos rechazados, y 16 por `cmd.exe`, que el modelo eligió por su cuenta. Resultado 8/8 correcto: `git rev-parse`, `branch`, `status` (dos formas), `log`, recuento de `git ls-files` (lo obtuvo listando, tras 6 intentos fallidos con `Measure-Object`, `find`, `py`, `wc` y `for /f`), `dotnet --version` y la primera línea de `README.md` (con `more`, tras fallar `Get-Content`). Unos 300 000 tokens de entrada, frente a unos 67 000 de la misma sonda con 0.160.1 | `result.json` (`Probes.1`); OD-2d-PROBE (evidencia §70-§71) |
| **Sonda 2, celda del Architect** (`gpt-6.1-sol/high`). 0/6: los 6 comandos fueron con el alias y todos se rechazaron; el modelo no buscó otra shell. `head_sha` null | `result.json` (`Probes.2`) |
| Huella, estructura, binario y versión de la app no cambiaron; los clones tampoco | `measurements.log` |

## 2. Qué exige el contrato de ejecución de FX-02 al Controller

| Invocación (D.3, topología A) | Operaciones que necesita dentro de la sesión | ¿Cubiertas por la sonda 1? |
|---|---|---|
| PLAN → `rackcad-delegation/v2` | leer el contrato, los insumos del cierre y los archivos del clon; Git de lectura (`HEAD`, rama) | **sí, en parte:** Git de lectura y lectura de archivos, por la ruta improvisada |
| VERIFY → `rackcad-controller-verification/v2` (14 controles, AP 16.9) | `Scope` = pass «solo con la salida reproducible de `git diff --name-only BaseSha..CurrentSha`» (`controller-contracts.md` §3.2); `AuthorityResolution` con `ClauseMapBlob` (resolución de cláusulas por el mapa de 16.13); hashes y blobs de las entregas; lectura de JSON de entregas y hechos remotos | **no demostrado:** la sonda no ejerció `git diff`, `git hash-object` ni el mapa de cláusulas. La sonda 2 sí pedía `git diff --stat` y `git hash-object`, pero la celda del Architect no ejecutó nada |
| 8 controles con invocación (nc1-nc3, N4-N7b) | las mismas de VERIFY con entradas mutadas | **no demostrado** |

## 3. Conclusión

1. **No hay cambio en la receta.** La ruta `cmd.exe` no altera 16.4: sus elementos se aplicaron literalmente. El cambio está en lo que hace la CLI
   con ellos.
2. **La evidencia custodiada no basta para el contrato de ejecución de FX-02.**
   - Muestra que la celda del Controller llegó a ejecutar Git de lectura y lecturas de archivos, pero solo porque el modelo improvisó la ruta, que
     no es un parámetro de la receta. La sonda 2 prueba que otra celda no lo hace.
   - No cubre las operaciones de VERIFY que el contrato nombra (`git diff --name-only` reproducible, hashes, resolución por el mapa de cláusulas).
   - Tratar la ruta como equivalente a la medida con 0.160.1 sería asumir una equivalencia que §60 prohíbe.
3. **Hacerla explícita es material.** Dar por buena la ruta (decir a la celda en qué shell ejecutar y medirla con las operaciones de VERIFY)
   cambia lo que hoy supone la receta materializada: que los comandos corren en el `pwsh` del runtime. Además necesita una invocación medida nueva,
   y el bloque A2-P2 de este par ya consumió sus 2 sondas. Por eso exige una disposición o una enmienda antes de ejecutarla.
4. **Architect de FX-02.**
   - V14 D.3 fija `codex-cli` para el Architect de la topología A («Architect (`codex-cli`, invocación propia) | revisión del contrato»). D.3 solo
     tiene variante de adapter para el Principal B de FX-04a. `claude-cli` en ese papel es, por tanto, un cambio material de D.3.
   - Tampoco lo cubren las autorizaciones vigentes: OD-3 = A alcanza al Reviewer y al Architect de FX-03 y a los Architects de FX-06, y
     CLAUDE-CLI-I62 = A a las revisiones reales de I-62.
   - Capacidad: la celda `claude-cli` 2.1.293 está caracterizada y es elegible para ARCHITECT (§57, punto 3).
   - Independencia frente a AUTHOR (la sesión de supervisión): Actor, Sesión y Contexto distintos. El Proveedor coincide, y eso es PREFERRED, no
     REQUIRED (contrato T1).
   - Consumo: una invocación y `+ pool`, como en D.3.

## 4. Vías (decisión del Coordinator; las que tocan el Freeze van en una A-n)

| Vía | Qué cambia | Autoridad | Coste y riesgo |
|---|---|---|---|
| **V1. Controller `codex-cli` con ruta de shell explícita y medida; Architect de FX-02 por `claude-cli`** | A-n: (a) para un `codex-cli` cuya selección de shell medida rechaza el `pwsh` del runtime, la invocación del rol declara la shell (`cmd.exe`) y la elegibilidad de la celda exige una invocación medida que ejerza las operaciones de VERIFY (`git diff --name-only`, `git hash-object`, lectura de JSON); (b) el Architect de la topología A puede ir por `claude-cli` cuando la celda `codex-cli` del Architect esté medida BELOW_REQUIRED, con los mismos topes | A-n MATERIAL (Architect independiente + Coordinator); Owner: una sonda read-only de `codex-cli` fuera de A2-P2 y la ampliación de `claude-cli` al Architect de FX-02 | **una A-n y una sonda**. Conserva el Controller de otro proveedor (topología A). Riesgo: la ruta explícita puede no bastar para el mapa de cláusulas |
| V2. Controller y Architect de FX-02 por `claude-cli` | A-n: sustitución de adapter de los dos roles de la topología A ante una limitación medida | A-n MATERIAL; Owner: `claude-cli` para EXECUTION_CONTROLLER (hoy excluido: CLAUDE-CLI-I62 solo ARCHITECT/REVIEWER) y para el Architect de FX-02 | **una A-n, sin sondas de Codex.** La topología A pierde el Controller de otro proveedor (Proveedor PREFERRED sin cumplir frente al Worker). Las herramientas de `claude-cli` (Read, Grep y Glob) no ejecutan `git diff`: la salida tendría que entrar como insumo, y eso también es material |
| V3. Esperar una actualización de la CLI que corrija la regresión | nada: A2-P2 permite pedir un bloque por cada actualización observada | disposición + consumo del Owner por bloque (A-2) | **sin coste ahora, plazo desconocido.** Las tres actualizaciones anteriores llegaron en unas 41 h |
| V4. FX-02 UNVERIFIED con causa F6-OBS-03 | nada | Coordinator | F6 no se cierra (D.4: FX-02 PASS es condición) |

**Propuesta de la preparación (etiquetada; no es una decisión):** V1 y, en paralelo, V3, porque V1 se puede preparar sin bloquear la llegada de
una corrección. Si llega una actualización que corrige la regresión antes de que V1 esté acordada, se pide su bloque A2-P2 y V1(a) deja de hacer
falta. V1(b) sirve en cualquier caso, porque la celda del Architect `codex-cli` no ejecuta comandos.

Además, FX-02 no se puede ejecutar solo con esto: siguen pendientes las disposiciones del grupo (b) de la línea FX-02 (solicitud consolidada,
U-06..U-28 y U-68). Se presentan en una sola solicitud con propuestas.
