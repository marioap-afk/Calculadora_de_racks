# I-62 — Fixture B1: solicitud de creación del repositorio, como acción separada (preparada; decisiones §90, punto 5)

> Supervisión, plano a. **Preparación: no se pide ni se ejecuta nada.** Sustituye, solo para la creación del repositorio, a la sección 2.A y a la
> secuencia de [`B1-construction-request-2026-10-11.md`](B1-construction-request-2026-10-11.md), que se conserva sin cambios. La sesión de Principal de
> FX-U2 queda fuera de esta solicitud (decisiones §90, punto 5): ni se pide su línea ni se abre FX-U2 mientras no exista una ruta elegible del
> Controller.

## 1. Hechos comprobados el 2026-10-17 sobre `400a72d8`

| Hecho | Valor | Consecuencia |
|---|---|---|
| A-5 | AGREED (decisiones §73, punto 1), `docs/initiatives/I-62-A-5.md`, blob `ad94ceedacbf3dbf70f0217a583978d0e32592fc` | sin cambio desde la preparación del 2026-10-11 |
| Constructor | `docs/automation/evidence/I-62-A5/build_candidate_a5.py`, blob `38b228a4e883ac02b3652e4c67fb2b7cd6e2ed12` | idéntico al del preflight de A-5: su resultado (MV-1..MV-7 7/7, F4 `clause_map.py` EQUAL, 11 negativos; evidencia §131, [`preflight.md`](../../I-62-A5/preflight.md)) se reutiliza por identidad hasta la construcción real, que repite las comprobaciones antes de publicar |
| Comprobador MV | `docs/automation/evidence/I-62-F6/MAP-INVALID/mv_check.py`, blob `9536d051372cb03643ac1392d85b3e8a563d35bc` | el de la evidencia de B1 |
| Nombre del repositorio | `rackcad-i62-fixture-b1` no existe en la cuenta del Owner (consulta de solo lectura con `gh repo view`, 2026-10-17) | el nombre propuesto está libre |
| Visibilidad | público por defecto (OD-7 = A público; regla del Owner para los repositorios de CI temporales o de fixture) | la línea A ya lo dice |
| Línea A del Owner | literal de A-5 §7; SHA-256 de la línea (sin fin de línea) `1d860c4d8326f5e719c16558c7534954eb286942b96a49409bdef9927604e0ce`, igual al de la preparación anterior | sin cambio |

## 2. Disposición de construcción que tiene que emitir el Coordinator (A5-1, regla 7)

Los ítems de la preparación del 2026-10-11, §1, sin cambio; entre corchetes, la propuesta:

1. Constructor `build_candidate_a5.py`, blob `38b228a4…`, sin cambios.
2. Fechas fijas de F_seed, F_norm y F_eff [`--date 2026-10-11T02:00:00+00:00`, las del preflight, para que los SHA sean reproducibles].
3. Unidad FX-U2, con Claim-Id [uuid4 nuevo, generado en la disposición] y rama [`fx/u2`]; la construcción **no** arranca la unidad.
4. `FIXTURE-MANIFEST.json` con la semántica de `COPIED` de A5-1, regla 1 (blob y commit de RackCad de origen por entrada; A62-A5-O5).
5. Comprobaciones antes de publicar (A5-1, regla 4; A62-A5-O4): MV-1..MV-7 = MAP_VALID con `mv_check.py`; un único commit con el trailer en todo
   el repositorio; árbol de F_eff igual al de F_norm; F4 `clause_map.py` EQUAL.
6. U-07 y U-08 [el TRX de la CI y el `.gitignore` en el seed, como archivos FIXTURE_LOCAL; no afectan a MV].
7. Origen local nuevo [`D:\r62-fixture\fixture-origin-b1.git`] y remoto con CI nuevo [`rackcad-i62-fixture-b1`, público].
8. Auditoría pública de todos los objetos antes del primer push (P-16, sin identidades reales); si la primera publicación no dispara la CI, se
   comprueba y se documenta.
9. Sin unidad: solo el linaje de activación. FX-U2 (D.1, pasos 4-6) espera a una ruta elegible del Controller (decisiones §73, punto 8; §90, punto 5).

## 3. Línea del Owner que se pediría (texto literal de A-5 §7, blob `ad94ceed`)

```text
A5-FIXTURE-B1-REPO = A (un repositorio público nuevo de GitHub, rackcad-i62-fixture-b1, en la cuenta del Owner, con GitHub Actions y la CI del fixture de OD-7 (fixture-build y fixture-tests, contents: read, sin secretos ni despliegues), como remoto con CI del fixture B1 de A5-1; solo tras la auditoría pública de todos sus objetos; lo crea el Owner, o la supervisión con gh bajo esta autorización; identidad sintética y sin datos reales; el repositorio del fixture actual y sus refs no se tocan; no autoriza ninguna sesión ni invocación; solo con A-5 AGREED, A5-1 incluida, y la disposición del Coordinator que nombre la construcción)
```

SHA-256 de la línea: `1d860c4d8326f5e719c16558c7534954eb286942b96a49409bdef9927604e0ce`.

**No se pide** `A5-PRINCIPAL-FXU2-CONSUMO` (`5ebf399e…`): la sesión de Principal de FX-U2 no se consume con esta acción.

## 4. Secuencia de la acción separada

1. Disposición de construcción del Coordinator (§2).
2. El Owner registra la línea A (§3).
3. La supervisión construye en local con las fechas fijas, repite las comprobaciones de §2, punto 5, audita todos los objetos, crea el origen local y
   el repositorio público con `gh` bajo la línea A, publica el linaje de activación y registra la CI.
4. Nada más: ni Claim de FX-U2, ni sesión de Principal, ni invocación. La construcción no depende de la medición del Controller ni compite con I-64
   (disco local y GitHub; ningún proceso de AutoCAD ni de Codex).
