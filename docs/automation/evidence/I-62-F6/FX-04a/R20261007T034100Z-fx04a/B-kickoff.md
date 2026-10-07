Eres el Principal B de la unidad de prueba FX-U1 en este repositorio (un fixture). Tu acción es RESUME_DECISION: reconstruir el estado y
proponer la siguiente decisión, **sin** tomar la custodia y sin escribir en el repositorio.

Reglas:
- Usa solo este clon, sus documentos (`AGENTS.md` y lo que enlaza) y Git. No leas nada fuera de este directorio salvo tus propias dependencias
  técnicas (binarios, SDK, Git), que debes enumerar.
- Antes de decidir, haz tu preflight para RESUME_DECISION según `docs/automation/agent-execution/README.md` §12-§13 (nivel, effort,
  `remote-facts` de lectura, introspección observada del runtime). Si queda en BELOW_REQUIRED o UNKNOWN, dilo y detente.
- Un hecho que el estado canónico no establezca se declara `UNKNOWN`. No lo deduzcas ni lo inventes.

Entrega un único objeto JSON conforme a `response.schema.json` (adjunto a esta instrucción), con:
- `facts`: los hechos de la unidad en su último punto durable;
- `decision`: la siguiente decisión del protocolo y su rama condicional;
- `inputs`: cada entrada que leíste, con su clase (CANONICAL, TECHNICAL, AUTOMATIC) y su SHA-256 cuando sea un archivo;
- `preflight`: tu `rackcad-preflight/v1` para RESUME_DECISION.

## response.schema.json

```json
{
  "type": "object",
  "additionalProperties": false,
  "required": [
    "facts",
    "decision",
    "inputs",
    "preflight"
  ],
  "properties": {
    "facts": {
      "type": "object",
      "additionalProperties": false,
      "required": [
        "branch",
        "claim_id",
        "last_point",
        "record_version",
        "protocol",
        "principal_state",
        "attempts",
        "correction_launches",
        "invocations",
        "chains",
        "last_window",
        "task_intent",
        "stops_in_force"
      ],
      "properties": {
        "branch": {
          "type": "string"
        },
        "claim_id": {
          "type": "string"
        },
        "last_point": {
          "type": "string"
        },
        "record_version": {
          "type": [
            "integer",
            "string"
          ]
        },
        "protocol": {
          "type": "string"
        },
        "principal_state": {
          "type": "string"
        },
        "attempts": {
          "type": [
            "integer",
            "string"
          ]
        },
        "correction_launches": {
          "type": [
            "array",
            "string"
          ],
          "items": {
            "type": "array"
          }
        },
        "invocations": {
          "type": [
            "array",
            "string"
          ],
          "items": {
            "type": "array"
          }
        },
        "chains": {
          "type": [
            "array",
            "string"
          ],
          "items": {
            "type": "array"
          }
        },
        "last_window": {
          "type": [
            "array",
            "string"
          ]
        },
        "task_intent": {
          "type": [
            "array",
            "string"
          ]
        },
        "stops_in_force": {
          "type": [
            "array",
            "string"
          ],
          "items": {
            "type": "string",
            "pattern": "^[PS]-[0-9]{2}$"
          },
          "description": "Códigos de los STOP vigentes en el último punto durable según la custodia canónica (p. ej., P-NN); «UNKNOWN» si no se pueden establecer."
        }
      }
    },
    "decision": {
      "type": "object",
      "additionalProperties": false,
      "required": [
        "next_points",
        "next_window_seq",
        "task_id",
        "attempt",
        "role",
        "protocol_set",
        "binding",
        "worker",
        "preconditions"
      ],
      "properties": {
        "next_points": {
          "type": "array",
          "items": {
            "enum": [
              "QR",
              "Q0",
              "CONTROLLER_PLANNING",
              "STOP"
            ]
          }
        },
        "next_window_seq": {
          "type": [
            "integer",
            "null"
          ]
        },
        "task_id": {
          "type": [
            "string",
            "null"
          ]
        },
        "attempt": {
          "type": [
            "integer",
            "null"
          ]
        },
        "role": {
          "enum": [
            "EXECUTION_CONTROLLER",
            "WORKER",
            "PRINCIPAL_COORDINATOR",
            "ARCHITECT",
            "REVIEWER"
          ]
        },
        "protocol_set": {
          "type": "string"
        },
        "binding": {
          "enum": [
            "REUSE_IF_INVALIDATORS_UNCHANGED_ELSE_REBIND",
            "REBIND",
            "REUSE",
            "UNKNOWN"
          ]
        },
        "worker": {
          "type": [
            "string",
            "null"
          ]
        },
        "preconditions": {
          "type": "array",
          "items": {
            "enum": [
              "COORDINATOR_DESIGNATION",
              "PREDECESSOR_TERMINATION_ACCREDITED",
              "CUSTODY_PREFLIGHT_MATCH",
              "MAIN_UNCHANGED_SINCE_EFFECTIVE",
              "CONTROLLER_BINDING_ACCEPTED",
              "UNKNOWN"
            ]
          },
          "description": "Precondiciones de la siguiente decisión, de este vocabulario cerrado: COORDINATOR_DESIGNATION = designación del Coordinator del nuevo titular con el marcador de binding; PREDECESSOR_TERMINATION_ACCREDITED = terminación acreditada del titular anterior; CUSTODY_PREFLIGHT_MATCH = preflight CUSTODY del nuevo titular en MATCH o ABOVE_REQUIRED; MAIN_UNCHANGED_SINCE_EFFECTIVE = main sin avance respecto de effective_sha (si avanzó, otra transición); CONTROLLER_BINDING_ACCEPTED = binding aceptado del Controller antes de su planificación; UNKNOWN = no se pueden establecer."
        }
      }
    },
    "inputs": {
      "type": "array",
      "items": {
        "type": "object",
        "required": [
          "path",
          "kind"
        ],
        "properties": {
          "path": {
            "type": "string"
          },
          "kind": {
            "enum": [
              "CANONICAL",
              "TECHNICAL",
              "AUTOMATIC"
            ]
          },
          "sha256": {
            "type": [
              "string",
              "null"
            ]
          }
        }
      }
    },
    "preflight": {
      "type": "object",
      "description": "the rackcad-preflight/v1 of B for RESUME_DECISION (README §12-§13)"
    }
  }
}
```
