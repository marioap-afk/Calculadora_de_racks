# I-62 — Matriz de riesgos: medición directa de A6-7 (A) frente a una sonda previa de caracterización del runtime actual (B) (decisiones §91, punto 6)

> Análisis estático. Ni A ni B se autorizan aquí; ningún bloque A2-P2 se activa. La medición de A6-7 es única, sin reintento (A6-7, regla 4): un fallo
> por el entorno la consume igual que un fallo del Controller, y solo una actualización observada del runtime u otra A-n abren otra.

## 1. Incertidumbres que pueden hacer fallar la medición sin decir nada de la capacidad del Controller

| Id | Incertidumbre (versión 26.1013, sin medir) | Efecto en A | Evidencia previa |
|---|---|---|---|
| U1 | Representación de la orden en el registro: forma exterior, `-NoProfile`, prefijos añadidos (en 0.162 la línea real llevaba `try { [Console]::OutputEncoding=… } catch {}`) | si no es una de las tres admitidas por v2, ninguna ejecución se reconstruye: NOT_DEMONSTRATED | r3/r2static: tres formas en nueve registros de tres versiones |
| U2 | Qué shell usa (pwsh del runtime, `cmd.exe`, alias de WindowsApps) | el alias de pwsh falló con «Acceso denegado» bajo `read-only` en 0.162; con `cmd.exe`, la comprobación (iii) previa no fue con el shell de la invocación | r2static; kit, hallazgo 9 |
| U3 | Modo de lenguaje (ConstrainedLanguage) y ruido de `profile.ps1`; tubería nativa `git … \| python …` con `&` | bytes alterados o sobre no separable: NOT_DEMONSTRATED | ningún registro real contiene una tubería entre dos programas nativos |
| U4 | Si el alias de la Microsoft Store de Python arranca dentro del sandbox | ninguna ejecución del instrumento | v5 (26.1007) lanzó `python` 24 veces; el alias de pwsh falló en 0.162 |
| U5 | `PATH` que Codex compone para el proceso | `git` por nombre distinto del declarado: NOT_DEMONSTRATED | kit: el `PATH` del hijo se arma desde el registro; lo que añade Codex, sin medir |
| U6 | Truncado de salidas grandes | sobre de `clausemap` (≈ 44 KB) truncado: no reconstruible | sin truncado visible hasta 59 805 bytes; por encima, sin datos |
| U7 | Que el modelo copie literalmente las órdenes, complete el esquema y termine en 600 s | NOT_DEMONSTRATED | v5: errores puntuales de copia y formato; 42 órdenes en ≤ 415 s |
| U8 | Celda `gpt-6-luna`/`high` disponible, modelo y effort efectivos, autenticación, avisos de uso (P-06) | invocación inválida o celda distinta | no observable sin invocar |
| U9 | Cierre de VERIFY admitido por las autoridades (I6) | sin un cierre válido no hay medición válida | determinación de §91, punto 2: exige enmienda |

## 2. Comparación

| | A. Medición directa | B. Sonda previa y después la medición |
|---|---|---|
| Qué elimina | nada antes de consumir | U1, U2, U3, U4, U5, U6 (con una salida de prueba ≥ 60 KB) y U8: forma registrada de una tubería `"<git>" … cat-file blob <objeto inocuo> \| "<python>" -I -`, shell y modo de lenguaje, arranque del alias, `where.exe git` y `git --exec-path`, truncado, modelo y effort efectivos |
| Qué no elimina | — | U7 (es lo que mide A6-7) ni U9 (normativa) |
| Autorización | disposición de medición; OD-2 y línea (b) del Owner | además: disposición del Coordinator que nombre el bloque A2-P2 del par nuevo (A2-P2, regla 3; par ya observado con huella estable) y la línea `BLOQUE-CODEX-CONSUMO` del Owner; OD-2 es común a las dos |
| Preparación | kit congelado | un kit mínimo de caracterización (no existe), sin el instrumento ni su blob, que reutiliza puertas, identidad y lanzamiento del kit de A6-7: estimación 1-2 h más autoprueba |
| Ejecución | ≈ 10-15 min más el análisis | sonda ≈ 10 min y análisis ≈ 30 min; después, si hace falta, ajustar y volver a congelar la regla de reconstrucción o el texto antes de nombrarlos en la disposición de medición |
| Consumo | la medición única | una sonda de un bloque nuevo de dos (la otra caduca con la siguiente actualización) más la medición; no repone ni reutiliza nada agotado |
| Riesgo de perder la medición por el entorno | alto: depende de U1-U6 y U8, todos sin medir en 26.1013 | bajo: solo queda lo que la sonda no reproduzca exactamente (p. ej., una orden registrada de forma distinta por su contenido) |
| Riesgo de invalidación | otra actualización de la app invalida la observación y la medición | igual, y además invalida el bloque y su sonda (A2-P2, regla 6) |

## 3. Conclusión

B elimina las incertidumbres de entorno que harían fallar la medición única sin informar sobre el Controller, que son las más probables con una versión
sin medir (las dos actualizaciones anteriores cambiaron el shell o la ruta: alias denegado en 0.162, `.cache` en el runtime). Reduce lo suficiente el
riesgo de perder la medición por el entorno, a cambio de una autorización y una sonda más y de unas horas. No reduce U7, que es justamente lo que A6-7
debe medir, ni U9: ni A ni B tienen sentido antes de resolver el cierre de VERIFY (determinación de §91, punto 2). B no depende de esa resolución (no
usa el instrumento ni el cierre de A-6) y podría hacerse en paralelo, si el Coordinator lo dispone.
