# I-62 — Fixture B1: disposición de construcción y solicitudes del Owner (preparadas; decisiones §73, punto 8)

> Supervisión, plano a. **Preparación: no se pide ni se ejecuta nada todavía.** A-5 está AGREED (decisiones §73; blob `ad94ceed`). Sustituye al
> borrador `B1-fixture-request-draft-2026-10-11.md`, escrito antes del acuerdo. La evidencia histórica del fixture actual y de FX-U1 se conserva.

## 1. Disposición de construcción que tiene que emitir el Coordinator (A5-1, regla 7: «la creación del repositorio exige la autorización del Owner»)

Ítems que la disposición tiene que fijar; entre corchetes, la propuesta de la preparación:

1. **Constructor:** `docs/automation/evidence/I-62-A5/build_candidate_a5.py`, blob `38b228a4e883ac02b3652e4c67fb2b7cd6e2ed12` (el del preflight de A-5), sin cambios.
2. **Fechas fijas** de F_seed, F_norm y F_eff [las del preflight, `--date 2026-10-11T02:00:00+00:00` (02:00:0X por paso), para que los SHA sean reproducibles; con fechas nuevas, los SHA cambian y el preflight
   se repite].
3. **Unidad:** FX-U2, con su Claim-Id [nuevo; uuid4 generado en la disposición] y su rama [`fx/u2`].
4. **Texto del manifiesto** (`FIXTURE-MANIFEST.json`) con la semántica de `COPIED` de A5-1, regla 1 (A62-A5-O5: blob y commit de RackCad de origen por
   entrada).
5. **Comprobaciones antes de publicar** (A5-1, regla 4, y A62-A5-O4):
   - MV-1..MV-7 = MAP_VALID con `mv_check.py`;
   - un único commit con el trailer en todo el repositorio;
   - el árbol de F_eff igual al de F_norm;
   - F4 `clause_map.py` EQUAL.
6. **U-07 y U-08:** si el TRX de la CI y el `.gitignore` van en el seed [propuesta: sí, como archivos FIXTURE_LOCAL; no afectan a MV].
7. **Origen local nuevo** [`D:\r62-fixture\fixture-origin-b1.git`] y **remoto con CI nuevo** [`rackcad-i62-fixture-b1`, público por defecto].
8. **Auditoría pública** de todos los objetos antes del primer push (P-16, sin identidades reales). La primera publicación puede no disparar la CI:
   comprobar la corrida y, si falta, documentarlo.
9. **Sin unidad todavía:** la construcción publica solo el linaje de activación. El arranque de FX-U2 (D.1, pasos 4-6) espera a que el Controller sea
   elegible (§73.8).

## 2. Solicitudes del Owner (texto literal de A-5 §7, blob `ad94ceed`)

**A. Repositorio B1:** se puede pedir con A-5 AGREED y la disposición del §1:

```text
A5-FIXTURE-B1-REPO = A (un repositorio público nuevo de GitHub, rackcad-i62-fixture-b1, en la cuenta del Owner, con GitHub Actions y la CI del fixture de OD-7 (fixture-build y fixture-tests, contents: read, sin secretos ni despliegues), como remoto con CI del fixture B1 de A5-1; solo tras la auditoría pública de todos sus objetos; lo crea el Owner, o la supervisión con gh bajo esta autorización; identidad sintética y sin datos reales; el repositorio del fixture actual y sus refs no se tocan; no autoriza ninguna sesión ni invocación; solo con A-5 AGREED, A5-1 incluida, y la disposición del Coordinator que nombre la construcción)
```

SHA-256 de la línea: `1d860c4d8326f5e719c16558c7534954eb286942b96a49409bdef9927604e0ce`.

**B. Sesión de Principal de FX-U2:** su apertura queda diferida hasta tener un Controller elegible (§73.8). Se pedirá cuando el Coordinator disponga la
apertura:

```text
A5-PRINCIPAL-FXU2-CONSUMO = A (una sesión de Principal, claude-desktop-session, abierta por el Owner para arrancar la unidad FX-U2 del fixture B1 y ejecutar en ella FX-02 en la Topología A, en la fila propia de A5-2 y por encima del límite de la ronda A de D.3 que cubre OD-5; solo con A-5 AGREED, A5-2 incluida, y la disposición del Coordinator que nombre la apertura; sin reapertura ni ninguna otra sesión; no usa el cupo de A4-6; no acepta su resultado ni amplía OD-5)
```

SHA-256 de la línea: `5ebf399eb3727bc19cdd0b24a8a4bd3848e5d3b8050e534a8c4276185a90d111`.

## 3. Secuencia

1. Disposición de construcción del Coordinator (§1).
2. El Owner da la línea A.
3. La supervisión construye en local, comprueba (§1, punto 5), audita, publica en el origen nuevo y en GitHub y registra la CI.
4. Cuando el Controller sea elegible: disposición de apertura, línea B del Owner, P-07 y apertura de FX-U2.

## 4. Estado tras las decisiones §74 (punto 5)

- La solicitud y el procedimiento de activación de §1-§3 se conservan listos, sin pedir.
- **La construcción se puede solicitar por separado** (§74.5) porque cumple las dos condiciones:
  - no consume la sesión de Principal: la construcción publica solo el linaje de activación (§1, punto 9) y la línea B no se usa;
  - conserva los resultados históricos: el fixture actual, FX-U1 y sus refs no se tocan (A5-1, regla 6; línea A: «el repositorio del fixture actual
    y sus refs no se tocan»).
- **FX-U2 no se abre** mientras no haya una ruta elegible para el Controller (§74.5).
- Si el Coordinator dispone la construcción, el siguiente paso es su disposición (§1) y después la línea A del Owner.
