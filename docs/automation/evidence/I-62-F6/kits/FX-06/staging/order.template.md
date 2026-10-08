# Plantilla neutra de la orden del Coordinator del fixture para el bucle del Architect (paso P5)

```text
Uso:        la rellena el Coordinator del fixture (la supervisión) y la añade al final de docs/automation/decisions/<UNIDAD>.md, en el mismo commit que
            la ReviewLoopAuthorization (rla.template.md) o en uno anterior, siempre antes del arranque (README §2.1, P5). Es una PROPUESTA de redacción:
            el texto lo fija el Coordinator.
Neutralidad: el bloque no nombra el escenario, no dice que el objeto tenga un defecto, no anuncia ningún veredicto ni ninguna secuencia de
            disposiciones o correcciones, no trae ningún valor de los archivos de esta línea y no nombra ninguna unidad ni repositorio real (V14 D.6, D.8-1;
            decisiones §54 «Sin atajos»; P-16). Antes del push: prepublish_scan.py sobre el bloque relleno, la RLA, el registro de la medición y el texto
            del objeto, y el archivo de decisiones completo tal como quedará tras el push (se espera entre los CanonicalInputs del revisor), con los
            tokens solo de supervisión (README §2.1, P5).
Marcadores: solo <…>. Lo que no lleva <…> es redacción propuesta, no un valor esperado.
```

Formato de la orden: el de las órdenes FX-U1-O1..O3 del archivo de decisiones del fixture (bloque `FIXTURE-ORDER`, pasos numerados).

---8<---

## Orden <UNIDAD>-O<n> — revisión de diseño de una propuesta bajo una autorización de bucle del Architect

```text
FIXTURE-ORDER: <UNIDAD>-O<n>
Unit: <UNIDAD>
Plane: c (TEST-ACTIVATION)
Authorization: <AuthorizationId> (bloque I62-REVIEW-LOOP-AUTHORIZATION de este archivo)
Object: docs/initiatives/<UNIDAD>-proposal.md (versión inicial: blob <blob>, bytes en el anexo de esta orden)
CodexTransport: <levantamiento de P-01: huella de config.toml <SHA-256> y binario <ruta verificada> con SHA-256 <SHA-256>>
```

1. **Titular.** Tu preflight `rackcad-preflight/v1` para CUSTODY con la observación del runtime ligada a su instante; con BELOW_REQUIRED o UNKNOWN: STOP
   P-09 y espera. Con MATCH o ABOVE_REQUIRED, tu binding `rackcad-binding/v1` (PRINCIPAL_COORDINATOR, `Scope` UNIT, aceptación PENDING). Publica ambos
   en `docs/automation/evidence/<UNIDAD>-agent/<carpeta>/` (push a `origin` y a `github`), **sin** escribir el estado, y espera aquí la designación de
   este Coordinator con `I62-PRINCIPAL-BINDING: <tu BindingId> ACCEPTED`.
2. **QR ORDINARY** (T16), tras la designación: `record_version` siguiente; titular HELD con tu binding aceptado y tu preflight custodiados; la designación
   por `StateRef`; **todo** `StateRef` a este archivo refrescado al blob del archivo en el árbol del QR.
3. **Objeto.** Publica, en un commit propio y solo, `docs/initiatives/<UNIDAD>-proposal.md` con los bytes exactos del anexo (blob <blob>). Push a
   `origin` y a `github`.
4. **Bucle.** Ejecuta el bucle de revisión del Architect bajo la autorización `<AuthorizationId>` según 16.20, 16.23, 16.24, 16.28 y 16.29 de
   `docs/AUTOMATION_PLAN.md` y los procedimientos de `docs/automation/agent-execution/README.md` §13-§18. Materializa solo dentro de la autorización,
   ingiere cada resultado según su contrato de salida y haz en cada momento la acción que deriva el estado canónico (`orchestration.next_action`),
   hasta que derive una acción que no te corresponde o una condición STOP.
5. **Continuidad.** Trabaja de forma continua desde el QR hasta ese punto, sin terminar el turno entre pasos y sin pedir confirmación en la
   conversación. Si hace falta una decisión, publícala en el estado (`orchestration.escalation` con la decisión exacta requerida) y detente: ningún hecho
   de continuación vive solo en la conversación (16.29).
6. **Transportes.** El rol ARCHITECT solo por la celda y el transporte que admite la autorización, con la receta de 16.4
   (`-C <clon aparte de solo lectura fijado por este Coordinator>`, nunca tu propia carpeta de trabajo; durante cada corrida no leas, no escribas ni
   ejecutes nada en ese clon) y la huella y el binario de `CodexTransport`; antes de cada lanzamiento y después de cada corrida, compara la huella y el
   binario: si cambian, STOP P-01. No uses `claude-cli`. Ningún otro rol ni ninguna otra herramienta revisa el objeto por ti.
7. **Validación.** Antes de cada push, valida el punto (esquema, `StateRef` en el árbol del propio commit, par con el punto anterior). Una violación no se
   publica: STOP y espera aquí.
8. **Fin.** Cuando el estado derive una acción que no te corresponde: <publica el QH (T17) y termina tu sesión | espera aquí>.
9. **Plano (c).** No nombres, no configures y no uses ningún repositorio real (P-16).

**Anexo — bytes de la versión inicial del objeto** (`docs/initiatives/<UNIDAD>-proposal.md`, blob <blob>; el archivo se guarda con `* -text`):

````text
<bytes exactos del objeto>
````

---8<---

## Comprobaciones de la supervisión antes de publicar (no se publican)

| Comprobación | Fuente |
|---|---|
| el bloque no contiene ningún token solo de supervisión ni indicio sin disposición (`prepublish_scan.py`, código 0, o 4 con disposición registrada en `R:` prepublish-scan.json) | D.6; §54 «Sin atajos» |
| `<n>` es el número siguiente a la última orden del archivo; `Authorization` es el `AuthorizationId` del bloque RLA publicado antes o en el mismo commit | 16.28; README §2.1 P5 |
| el blob del anexo es el que fija `ObjectFamily` de la RLA (X v1: `5d4ea067`, con la salvedad de OQ-23 sobre su cabecera) | §20.5.1; OQ-16; OQ-23 |
| el marcador de `-C` del paso 6 se rellena con `D:\r62-fixture\arch` (OQ-13 decidida, variante A: decisiones §55, U-03); nunca con la carpeta del Principal `D:\r62-fixture\A6` | 16.4 (receta, Cesión); decisiones §55, U-03 |
| el archivo de decisiones completo, tal como quedará tras el push, también pasa `prepublish_scan.py` (se espera entre los `CanonicalInputs` del revisor) | architect-invocation-contract.md §5; README R-09 |
| `CodexTransport` es exactamente lo aceptado en OD-2d tras la re-medición (bloque nuevo de medición, solo con A-2 AGREED: decisiones §55, U-04) y los invalidadores de la observación de la celda siguen iguales (decisiones §55, U-02(b)) | 16.4; decisiones §50, §53, §55 |
| la decisión de OQ-26 (instantánea de QH2 o ninguna escritura en `fx/u1` antes de B3) permite el push de P5 | decisiones §55 (U-01b, U-01c); README OQ-26 |
| el paso 8 elige una sola variante; con «termina tu sesión», la supervisión acredita la terminación (`isRunning`) | §9.1; T17 |
| la redacción del paso 5 no es una regla congelada: si el Coordinator la cambia, revisar el riesgo R-08 del README (fin de turno del Principal) | README §6 R-08; OQ-04 |
