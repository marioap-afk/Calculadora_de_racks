# I-62 — F4 experimental: secuencias combinadas (EXPERIMENTAL — NOT AUTHORIZED FOR PRODUCTION)

```text
Autoridad:  orden nocturna §3.D y §9 (decisiones §41). No es F4: no se publica en las superficies de producción ni se atribuye PASS de C-15..C-42
Variantes:  (2) Freeze + A-1 propuesta = oráculo afirmado; (1) Freeze literal (validador V14) = solo informativo; (3) P4/P8 en sus dossiers
Motor:      el de docs/automation/evidence/I-62-A1/a1-counterexamples.py, importado en solo lectura; las decisiones de prueba (A2low, A2eq) viven en memoria
```

## Reproducción

```text
python combo_sequences.py <a1-counterexamples.py> combo-result.json
```

- **Arnés del objeto revisado** (`D:\r62-arch-a1r3`, A-1 blob `39c2f831`): `combo-result-before.json`. Ahí aparece el defecto A62-A1A-01:
  `c2n-satisfecho-con-el-BLOCKING-heredado-abierto` da VALID cuando se esperaba INVALID.
- **Arnés corregido en el mismo commit que A-1** (blob `cdcd98d2`): `combo-result.json`. Las 16 secuencias pasan.

## Secuencias

| Combinación de la orden §9 | Positiva (VALID) | Negativas (conjunto exacto) |
|---|---|---|
| expiración + rebase | `c1-expiracion-y-rebase`: EXPIRED, rebase con `loop.object` a la imagen, LOOP_CLOSED por la decisión y bucle nuevo que hereda LIN-1 | sin decisión → A1-P07; `loop.object` sin reconciliar → I-H02 |
| REVIEWER con BLOCKING heredado + cierre administrativo + nueva autorización | `c2-…`: EXHAUSTED, cierre (E), bucle nuevo con GC-2, LIN-R1 cerrado por su emisor, REVIEWER_SATISFIED y cierre (S) | reapertura con GC-1 agotada → A1-R05; **satisfecho con LIN-R1 abierto → A1-R02** (era VALID antes de A62-A1A-01) |
| dos rebases + caída en LAUNCHING | `c3-dos-rebases-con-la-caida-resuelta-entre-ambos` | segundo rebase con LAUNCHING pendiente → A1-P08 (observación, pregunta 17 del paquete); LAUNCHING reescrito → A1-P08, A1-P09; no lanzado sin replanificar → I-H02, V14-S18-target |
| autorización sustituta con límites menores a lo consumido | `c4-sustituta-con-topes-iguales-a-lo-consumido` | topes < consumo → A1-F01 (fail-closed: el estado no se puede registrar); reserva sobre un tope estrechado → A1-F01, A1-P01, A1-P02 |
| respuesta tardía + intento cancelado | `c5-…`: LAUNCH_UNCERTAIN, reejecución reservada, cancelación por EXPIRED, resultado tardío registrado sin ingerirse (V14 §20.6 caso B.5) | resultado tardío ingerido → A1-P09; cancelado relanzado → A1-P09, A1-P13 |

**Corrección de expectativa declarada.** Tras la primera corrida, `c3n-segundo-rebase-sin-replanificar-el-no-lanzado` pasó de {A1-P15, I-H02,
V14-S18-target} a {I-H02, V14-S18-target}. A1-P15 solo se aplica a invocaciones replanificadas, y el `Target` obsoleto lo detectan I-H02 y V14-S18-target. Las
otras correcciones posteriores a la primera corrida son errores de construcción de las trazas, no expectativas:
- faltaba el paso LAUNCHING;
- faltaban `OpenFindings` y `BudgetSnapshot` en la reserva.

## Hallazgos

| Id | Tipo | Qué | Dónde queda |
|---|---|---|---|
| **A62-A1A-01** | defecto de A-1 (seguridad) | D1-17 (3) y D1-18 (S) contaban solo los BLOCKING «abiertos en una solicitud del bucle»: un bucle REVIEWER nuevo podía satisfacerse con un BLOCKING heredado abierto | corregido en A-1 (blob `cdcd98d2`), con dos trazas nuevas en el arnés; pregunta 16 del paquete |
| F4X-OBS-01 | observación | un segundo rebase con un intento en LAUNCHING sin resolver para sin publicar (D2-2 exige el `Target` en el mapa nuevo). Es conservador, pero puede bloquear la apertura de sesión hasta resolver el intento | pregunta 17 del paquete; sin cambio en A-1 |
| F4X-OBS-02 | observación | una autorización sustituta con topes menores que lo consumido no se puede registrar (I-S18 «ningún contador sobre su tope»; es el mismo comportamiento que V14). Para cortar el bucle se usa la revocación o los topes iguales al consumo | sin cambio; se documenta aquí |

## Versión C# experimental (clon aislado)

```text
Clon:        D:/r62-f4-exp (git clone --no-local; rama exp-f4 en d97ce3d0; sin remoto); nada se publica en la rama de I-62
Archivo:     tests/RackCad.Tests/Experimental/I62/I62OrchestrationExperimentalTests.cs (diff completo: csharp-experimental.patch)
Herramientas: SDK .NET 8.0.423 del usuario; xunit 2.9.2 y Microsoft.NET.Test.Sdk 17.11.1, los del proyecto (ningún paquete nuevo)
Comando:     dotnet test tests/RackCad.Tests/RackCad.Tests.csproj --filter "FullyQualifiedName~RackCad.Tests.Experimental.I62"
Resultado:   9/9 superadas, 0 omitidas (TRX fuera del repo: D:/r62-exp-night/csharp/i62-exp.trx, SHA-256 c07b6f52…); sin avisos propios
```

Modelo mínimo:
- presupuesto por bucle con topes mínimos, sin reinicio y con EXPIRED inmutable;
- bucles REVIEWER con cierre (S)/(E) y sin resurrección;
- aristas de intento de §20.6, con el resultado tardío tras LAUNCH_UNCERTAIN registrado como evidencia;
- la regla de D2-2 para un intento en LAUNCHING.

Las variantes van separadas como parámetros:
- `ReviewerBlockingScope.LoopOnly`: A-1 `39c2f831`; reproduce el defecto A62-A1A-01.
- `ReviewerBlockingScope.Unit`: el texto corregido.
- `LaunchingTargetRule.CurrentMapOnly`: D2-2 tal como está escrito.
- `LaunchingTargetRule.Chain`: la alternativa de la pregunta 17.

No modela la pertenencia por apertura de A62-A1S-01, que llegó después; esa la cubre el arnés simbólico.

## Límites

Esto no es la máquina de estados de F4. Los veredictos son del modelo simbólico del arnés, que es evidencia de apoyo y no autoridad, y no hay PASS de
C-15..C-42.
