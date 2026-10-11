# I-62 — Fixture B1: solicitud exacta (BORRADOR; decisiones §70, punto 5)

> Supervisión, plano a. **Borrador: no se pide nada todavía y no se ejecuta ninguna acción reservada al Owner.** Antes hacen falta las decisiones del
> Coordinator del §2. La evidencia histórica de los pilotos se conserva: el repositorio `marioap-afk/rackcad-i62-fixture`, su origen local
> `D:\r62-fixture\fixture-origin.git` y todas sus refs no se tocan.

## 1. Estado técnico (listo)

- **Candidato B1:** un repositorio de fixture nuevo, con el seed en la base I-61 `bb0d5522` y las autoridades de MC_I62 `6f0187cb` solo por el segundo
  padre, un trailer único y EFF'.
  - Da **MAP_VALID** (MV-1..MV-7; F4 `clause_map.py` EQUAL; 7 negativos que fallan como deben).
  - Es reproducible (paquete `59b24493…`).
  - Los blobs invalidadores (`routing.md`, catálogo, descriptor de `codex-cli`, mapa y esquema) son idénticos a los actuales.
  - **No invalida ningún resultado acreditado** (`I-62-F6/MAP-INVALID/`).

## 2. Obstáculos normativos (decide el Coordinator; no se crea ninguna A-n sin disposición)

1. **D.1 fija el método que causó el defecto.** V14 D.1, paso 2 (L2386-L2396): «`main` del fixture, commit **F_seed**: copias **byte a byte** de las
   autoridades de RackCad en MC_I62». Ese seed deja EFF sin derivación (MAP_INVALID por AP 16.13). B1 siembra con la base I-61 y trae MC_I62 por F_norm,
   lo que **se aparta de la letra de D.1**. Opciones:
   - (a) una A-n que enmiende D.1, pasos 2-3: seed = autoridades previas a la activación (la base de los `BaseBlob` del mapa) y MC_I62 solo por F_norm.
     Es material: Architect independiente y acuerdo del Coordinator;
   - (b) una lectura del Coordinator según la cual AP 16.13 (normativo) prevalece sobre la letra de D.1 para la validez de la activación de prueba.
2. **Presupuesto de Principal.** La unidad nueva (propuesta FX-U2) necesita una sesión de Principal para su arranque (D.1, pasos 5-6) y para FX-02 en la
   Topología A.
   - En la ronda A están usadas A y R, y el cupo extra de A4-6 vale solo para «el titular A2 de FX-02 designado por T16» de FX-U1.
   - Una sesión para FX-U2 exige una A-n que reasigne ese cupo sin subir el total (o que suba el tope), más la autorización de consumo del Owner.
   - Puede ir en la misma A-n que el obstáculo 1.
3. **Decisiones ordinarias:** la opción B1; los invariantes (`reconstruction-plan.md` §2); el texto del manifiesto; el nombre de la unidad (FX-U2) y su
   Claim-Id; las fechas fijas de los commits; si U-07 (TRX en la CI) y U-08 (`.gitignore`) entran en el seed; la ruta del origen local nuevo (propuesta
   `D:\r62-fixture\fixture-origin-b1.git`); y las disposiciones de C-22 y del oráculo de C-25a.

## 3. Líneas del Owner (borrador; se pedirán solo tras el §2)

```text
FIXTURE-B1-REPO = A (repositorio público nuevo de GitHub marioap-afk/rackcad-i62-fixture-b1, con GitHub Actions, para publicar el linaje de activación reconstruido del fixture de I-62 (candidato B1, paquete 59b24493); lo crea el Owner, o la supervisión con gh bajo esta autorización; sin datos reales; marioap-afk/rackcad-i62-fixture y sus refs no se tocan)
B1-PRINCIPAL-CONSUMO = A (una sesión de Principal, claude-desktop-session, claude-opus-5-5 xhigh, abierta por el Owner para arrancar la unidad FX-U2 del fixture B1 y ejecutar FX-02 en la Topología A; solo con la A-n o la disposición que la autorice y la disposición del Coordinator que nombre la apertura; ninguna otra sesión ni reapertura; no acepta su resultado)
```

## 4. Secuencia propuesta

1. Disposición del Coordinator sobre el §2 (y, si elige 2.1 (a) o 2.2, la A-n con su revisión del Architect por `claude-cli` y su acuerdo).
2. Línea `FIXTURE-B1-REPO` del Owner → la supervisión construye el linaje (`build_candidate.py`), comprueba MV-1..MV-7 sobre él, lo publica en el
   origen nuevo y en GitHub y registra la CI. **Sin unidad.**
3. El arranque de FX-U2 (con `B1-PRINCIPAL-CONSUMO`) espera a que el Controller tenga una vía elegible: B1 resuelve el mapa, no el Controller.
