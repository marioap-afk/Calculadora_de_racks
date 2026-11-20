# PBT3x-NC1-CASCADE-SIGNAL-01 (firma PD-F4B-01): notas (v3, tras la recomprobacion 0/2/6)

Script: `nc1_cascade_signal_substitute.py` (SHA-256 `41440EDC4A963BEF890E4336DCCB885837424008C027CDB172D2B926E0F49B40`), con `nc1_cascade_golden.json`
(SHA-256 `8EB3A991371777AB218E434F4ECF43DC1FD42403C64DBF5C9A949084EFA88C24`, fijado en el script como GOLDEN_SHA256; lo construye `build_golden.py`, que ahora incluye
el `relay-record.json` real de cada caso).
Uso: `python -I nc1_cascade_signal_substitute.py --task PBT3A|PBT3B|PBT3C|PBT4|PBT5|VALIDATION <real> <nc1> <salida.json> <repo>`; `--selftest` solo la autoprueba.
El texto revisado no se edita por uso: `--task` deriva el Id y el registro (`SATISFIED_UNDER_PBT3A_NC1_CASCADE_SUBSTITUTE`, mayusculas).

## Cambios de v3

- **I-7 `--task` ligado a los datos** (propiedad g): el TaskId de la real empieza por `F4B-<TAREA>-`; el de nc1 es igual al de la real; `SourceInputs` de `mutation.json` lo contiene.
  `VALIDATION` acepta cualquier tarea `F4B-`, sin registro, y solo con los datos de la autoprueba y los dorados (pares de RunId de `nc1_cascade_golden.json`; otro par da FAIL con motivo).
- **I-8 el repositorio es el bueno**: `git cat-file -e` debe encontrar el `CurrentSha` y el `BaseSha` reales de la entrega real (atomos `a1.RepoHasRealCurrentSha/BaseSha`) y no encontrar el SHA falso.
  El repositorio vacio del perfil del usuario se rechaza (comprobado). Los dorados no ejecutan git y dan por buena la presencia (declarado en LIMITS).
- **M-11** `a4.RealPromptShaInRelayRecord`: el SHA-256 del `prompt.md` real debe aparecer en su `relay-record.json`. Lectura robusta (`prompt_shas`): valor de cualquier clave que contenga
  `prompt` y `sha`, o texto libre `prompt SHA-256 <64 hex>` (sin distinguir mayusculas ni profundidad). Si no hay ningun SHA de prompt: FAIL con el motivo; si hay pero no coincide: atomo en fail.
- **M-12** claves JSON duplicadas rechazadas (`object_pairs_hook`) en output.json, mutation.json, relay-record.json, las entregas y el registro de trabajo; Ids repetidos en `Checks` en forma de lista tambien.
- **M-13** `g.Nc1TaskIdEqualsReal` y `g.Nc1AttemptEqualsReal` (con tipo: `true` no es 1). No se compararon `DelegationRunId` ni `WorkRunId` (no pedido).
- **M-14** a2 sustituye el campo `"CurrentSha": "<Old>"` (como el generador, una vez), no la primera aparicion del SHA (casos A8 y A10).
- **M-15** casos propios: los 6 patrones ALIEN, `DerivedRecompute` sin `Field` y sin `Old`, a4 sin la ruta real (`a4-NoSeg`), `FailureClass=Identity`, parada `S-07` sin S-04.
- **M-16** la carpeta de salida se crea si no existe y el resultado se escribe siempre (si ni asi se puede, mensaje en stderr y exit 2); `Checks` en lista funciona igual que en diccionario (comprobado por una prueba unitaria y por X10).

## Estructura: atomos (51)

Cada propiedad es la conjuncion de condiciones elementales; la autoprueba exige el conjunto EXACTO de atomos que falla en cada caso y que cada atomo falle en algun caso.
`work/mutate_atoms.py` fuerza cada atomo a True y prueba 5 relajaciones (prefijos, `count=1`, patron del campo): 56 mutantes, 0 sobreviven (`work/run-mutants.txt`).

| Prop | Atomos |
|---|---|
| a1 | Control, File, Field, OldIsRealCurrentSha, NewIsMutatedCurrentSha, NewIsHex40, NewDiffersFromOld, NewExistsFlagFalse, GitConfirmsNotExists, **RepoHasRealCurrentSha, RepoHasRealBaseSha**, SourceVerificationIsRealRun, RunIdIsNc1Run |
| a2 | BytesReplaceOldNewOnceEqualsCopy (campo `CurrentSha`), TypedLeafDiffIsOnlyCurrentSha |
| a3 | InputFileSetEqual, ChangedExactlyHandoffAndRelayRecord, ChangedVsVerifiedInputsMatches, RelayRecordDiffOnlyOutputSha, RealOutputShaIsHandoffHash, CopyOutputShaIsHandoffHash, DerivedRecomputeMatches, InputShaKeysMatchFiles, InputShaValuesMatchFiles |
| a4 | PromptShaMatchesMutationJson, PromptIsRealWithOnlyRunIdAndPathChanged, PromptDiffTextDeclared, **RealPromptShaInRelayRecord** |
| g | **RealTaskIdMatchesTask, Nc1TaskIdEqualsReal, Nc1AttemptEqualsReal, SourceInputsContainsTaskId** |
| b | ClassificationBlocked, DispositionStop, StopConditionS04 |
| c | FirstFailIsHandoff, FailureClassIsHandoffOrStopCondition |
| d1/d2/d3 | FailSetWithinAllowed; HandoffFails, IdentityFails; OnlyTheFourteenKnownChecks, RestPassInNc1, RealIsVerified, RealAllFourteenPass |
| e1/e2/e3 | HandoffEvidenceLiteralFake, IdentityEvidenceLiteralFake; OtherFailEvidenceCitesFake; NoAlienInHandoffIdentity, NoAlienInOthers |
| f | IdentityEvidenceHasRealHead, HeadEqualsRealVerifiedSha |

## Autoprueba

97 casos sinteticos (10 PASS, 84 FAIL con su conjunto exacto de atomos y 3 de entrada malformada que deben dar error), 7 pruebas unitarias de las funciones auxiliares y 3 casos dorados con las salidas
reales (PB-T2a PASS, PB-T2b FAIL en c, d1, d2, e2, e3, PB-T2c PASS) empaquetados con su SHA-256. Determinista (se ejecuta dos veces). Salida en `selftest.txt`.

## Casos reales (`runs/`, `--task VALIDATION`, repositorio del checkout principal en solo lectura)

PB-T2a `0302` PASS; PB-T2b `bfda` FAIL (c, d1, d2, e2, e3); PB-T2c `ea01` PASS.

## Sondas del revisor (`work/run-probes-v2.txt`, copia de `review/v2/probes_v2.py` redirigida a `work/probes2`)

- Dan lo esperado: P06-P15b, F01, F06 (M-14), N01 (I-7), N02 y X08 (I-8), N03 (M-11), N04 (M-12), N05 (M-13), X01-X07.
- X09 y X10 ya no son FAIL: ahora X09 escribe el resultado creando la carpeta y X10 (Checks en lista) funciona; ambos dan PASS porque los datos no se tocaron (el comportamiento pedido en M-16).
- Siguen igual por decision: P01-P05 (limite declarado I-6, falso PASS); F02-F05 (M-3, M-4; falso FAIL conservador).

## Mutantes del revisor (`review/v2/mutate_v2.py`; `work/run-mutants-v2.txt`)

33 detectados, 1 sobrevive, 1 no aplicable. M29 (quitar la comprobacion del SHA del dorado) sobrevive POR CONSTRUCCION: la autoprueba no puede ejercitar la manipulacion de su propio dorado;
lo cubre la custodia de `ScriptSha256`/`GoldenSha256` (van en el resultado). M33 ya no aplica (a2 sustituye el campo; su equivalente `count=1` esta en `work/mutate_atoms.py` y se detecta).
**Correccion a la afirmacion anterior:** «0 sobreviven» valia para forzar atomos y las relajaciones propias; contra los 19 mutantes internos del revisor sobrevivian 11. Ahora, con los casos de M-15, solo M29.

## Interpretaciones y limites (tambien en SCOPE y en el resultado, campo `Limits`)

1. **M-3** null, ausente, ausencia, no consta y no presente dan falsos FAIL cuando el sujeto es el SHA falso; conservador.
2. **M-4** El prefijo de 8 o mas hex como lectura de «cita» en Remote, Scope, Ci y Tests esta forzado por `ea01` (Tests dice `CurrentSha deadbeef`); con 7 da FAIL. Handoff e Identity exigen los 40.
3. **I-6** e3 no detecta otras atribuciones (fechas, rutas, conteos, trailers, «falta», «vacio», «missing»).
4. `FailureClass` Handoff o StopCondition; d3 exige la real VERIFIED 14/14; Identity sola en fail no cumple PD-F4B-01.
5. El HEAD real es el `CurrentSha` real, cruzado con el `VerifiedSha` de la real y (en la ejecucion real) con git.
6. a4 exige el texto exacto de `PromptDiffVsVerified` del generador; otra redaccion daria FAIL (conservador).
7. Una entrada malformada escribe un resultado FAIL con `Reason` (texto de la excepcion), sin traceback.
8. Observacion del revisor: `make_pbt3a_control.py` define `recompute_reissue()` para un `reissue-record.json` que nc1 no recalcula; si la verificacion de PB-T3a trae ese archivo, a3 (exactamente dos archivos cambiados) da FAIL
   (conservador). A confirmar antes de PB-T3a.
9. Las ejecuciones sobre PB-T2 son validacion: no son el registro de ningun control.
