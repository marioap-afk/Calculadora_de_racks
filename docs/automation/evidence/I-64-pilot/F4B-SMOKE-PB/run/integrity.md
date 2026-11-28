# SPB-51: la integridad (bloque 10 de `smoke-run-v2.md`, §4.9)

- **Hora:** 2026-11-24 05:43 UTC, al final de la corrida (orden 18), con ningún AutoCAD vivo.
- **Cómo se corrió:** el bloque 10, tal cual, con tres valores locales:
  - `<worktree>`: el worktree de I-64.
  - `<lista-local-de-la-seleccion>`: su SHA-256 es `DD3613CB94CD4E19712D0FE2687CBDBDED2500C2AA8C4AD450D8556A26D3F2C1`, y se comprobó antes.
  - `<original-D1>`: tomado de la lista local por su campo `D1.Source`, no tecleado. Es lo mismo que en P-06.
- **Ninguna ruta de original se imprimió.** El Candidato es `ebd304a3773495fa9071de6611936c4c01a7b06c`.

## La salida (literal)

```text
D1 original igual: True
D1 respaldo igual: True
c01.dwg igual=True dwl=False
c02.dwg igual=True dwl=False
c03.dwg igual=True dwl=False
c04.dwg igual=True dwl=False
c05.dwg igual=True dwl=False
c06.dwg igual=True dwl=False
c07.dwg igual=True dwl=False
c08.dwg igual=True dwl=False
c09.dwg igual=True dwl=False
c10.dwg igual=True dwl=False
c11.dwg igual=True dwl=False
c12.dwg igual=True dwl=False
c13.dwg igual=True dwl=False
c14.dwg igual=True dwl=False
c15.dwg igual=True dwl=False
c16.dwg igual=True dwl=False
c17.dwg igual=True dwl=False
c18.dwg igual=True dwl=False
c19.dwg igual=True dwl=False
c20.dwg igual=True dwl=False
c21.dwg igual=True dwl=False
c22.dwg igual=True dwl=False
c23.dwg igual=True dwl=False
c24.dwg igual=True dwl=False
c25.dwg igual=True dwl=False
c26.dwg igual=True dwl=False
c27.dwg igual=True dwl=False
c28.dwg igual=True dwl=False
c29.dwg igual=True dwl=False
c30.dwg igual=True dwl=False
c31.dwg igual=True dwl=False
c32.dwg igual=True dwl=False
c33.dwg igual=True dwl=False
c34.dwg igual=True dwl=False
c35.dwg igual=True dwl=False
c36.dwg igual=True dwl=False
c37.dwg igual=True dwl=False
c38.dwg igual=True dwl=False
c39.dwg igual=True dwl=False
c40.dwg igual=True dwl=False
c41.dwg igual=True dwl=False
c42.dwg igual=True dwl=False
c43.dwg igual=True dwl=False
c44.dwg igual=True dwl=False
e01 (D5-02) original igual=True respaldo igual=True copia igual=True dwl=False
e02 (D5-01) original igual=True respaldo igual=True copia igual=True dwl=False
e03 (D4) original igual=True respaldo igual=True copia igual=True dwl=False
e04 (D5-05) original igual=True respaldo igual=True copia igual=True dwl=False
e05 (D3a D5-03) original igual=True respaldo igual=True copia igual=True dwl=False
s-copias.json: no existe (ningun dibujo con estilo, o no se llego)
Worktree limpio: True
El Candidato es antepasado de HEAD, sin cambios en src, tests ni assets: True
```

- No salió ningún archivo ajeno en `r1`, `estilo` ni `spb07`.
- No salió ningún autoguardado `*.sv$` de las copias.
- `exit 0`, sin STOP.
- HEAD es `fa59bd82`, el commit documental de la emisión (F4B-E62), encima del Candidato `ebd304a3` (PBT5-C01).

**Resultado propuesto:** PASS.
