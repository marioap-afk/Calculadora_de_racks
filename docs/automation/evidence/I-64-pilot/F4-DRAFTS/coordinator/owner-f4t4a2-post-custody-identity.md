I-64 — F4-T4A2-PREPARE-UI — POST-CUSTODY IDENTITY (pregunta de la sesión y respuesta del Owner, literales)

Pregunta de la sesión (2026-11-14):
"La reverificación única de F4-T4A2 fallaría en Identity por la custodia, no por el producto. Identity exige HEAD = remoto = CurrentSha. Tras custodiar el intento 2, HEAD = remoto = c3ae1c97, y el producto es CurrentSha = ae0a7b40. Medido: ae0a7b40 es ancestro de HEAD; las 46 rutas entre ambos son de docs/, con 0 en src/ y tests/. En F4-T1 autorizaste una sustitución acotada de Identity (condiciones A–I, owner-f4t1-post-custody-identity.md), pero solo para F4-T1. ¿Autorizas la misma para esta reverificación de T4A2 y sus nc1–nc3? CurrentSha seguiría siendo ae0a7b40, nc1 seguiría fallando en Identity y no cambia ningún otro hecho."

Opción elegida por el Owner:
"Sí, mismas A–I (Recommended)"

Descripción de la opción, tal como se presentó:
"Misma sustitución que en F4-T1: HEAD = remoto; CurrentSha (ae0a7b40) ancestro de HEAD; solo rutas docs/ entre ambos, con la lista exacta; 0 en src/ y tests/; BaseSha, RedSha, rama y worktree válidos. Vale para esta reverificación única y nc1–nc3. Va como nota acotada en la entrada del Controller."
