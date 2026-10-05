# I-62 — Portabilidad: reconstrucción en un clon limpio (EXPERIMENTAL — NOT AUTHORIZED FOR PRODUCTION)

```text
Autoridad:  orden nocturna §10 (decisiones §41). No es un piloto FX: no se abrió ninguna sesión de rol
Clon:       git clone --no-local -c core.autocrlf=false, sin remoto, HEAD aeda77c4; proceso distinto del primero, sin su memoria ni su narrativa
Comando:    python docs/automation/evidence/I-62-prep/night-2026-10-05/portability/reconstruct.py <clon> <dir de trabajo> portability-result.json
Resultado:  7/7 pasos reconstruidos (AllReconstructed = true); Python 3.13.14, git 2.54.0
```

Desde A62-A1T-01 (decisiones §42), el paso 2 compara con `f4-exp/combo-result-a1t01.json` y hay un paso 2b: T8, dos rebases reales y un sucesor
en un clon limpio (`I-62-A1/a1t01/t8-clean-clone.py`), comparado por relaciones porque los SHAs cambian en cada corrida. La corrida de `aeda77c4`
comparó con `combo-result.json`, y sigue siendo reproducible con el script de ese commit. La nueva corrida queda en su propia evidencia.

| Paso | Resultado |
|---|---|
| arnés de A-1 (100 trazas) | IDÉNTICO byte a byte al resultado custodiado |
| secuencias combinadas de F4 experimental | IDÉNTICO |
| arnés de P4 (sintéticos y objetos reales de I-63, alcanzables por `main`) | IDÉNTICO |
| prototipo de P8 | IDÉNTICO |
| helpers: lecturas P1/P2 de G1-G4 desde los artefactos de la rama | IDÉNTICAS |
| helpers: unittest sin TRX (P6 omitido por la propia prueba) | PASS |
| B.11: `gen-manifest.py` + `b11_precision.py` | IDÉNTICO tras el mapeo declarado (abajo) |

## Hallazgos de portabilidad

- **PORT-01 — referencia histórica fijada en un artefacto custodiado.**
  - `gen-manifest.py` (F3) fija el commit de V14 anterior al rebase, `4c617e82`, que no existe en un clon limpio. Es el mismo caso que FC-02 y D2-10.
  - La reconstrucción lo resuelve solo con el `rebase-map.json` custodiado: imagen `1d5cdbec`, con el mismo blob `34ad80ea`. Después devuelve las
    referencias a su SHA original para comparar.
  - Para F4: todo script custodiado que fije un commit de rama debe resolverlo por la cadena de mapas, o fijar el blob y no el commit.
  - El generador no se cambia: es de F3.
- **PORT-02 — dependencia oculta del entorno.**
  - Las pruebas de los helpers dependían del directorio de trabajo, o de la variable `I62_REPO` que pasó la primera ejecución: `git ls-tree` con una
    ruta es relativo al cwd.
  - Corregido con `--full-tree` y `git rev-parse --show-toplevel` (commit `aeda77c4`).
- **PORT-03 — defecto del propio script de reconstrucción en su primera corrida:** abría el manifiesto para escritura antes de leerlo. Corregido y
  declarado (commit `01ade261`).
- **No reconstruible por política:** las lecturas P6 de los TRX de I-63. Los TRX no se versionan (16.12); quedan sus SHA-256 en
  `helpers/real-readings.json`.
