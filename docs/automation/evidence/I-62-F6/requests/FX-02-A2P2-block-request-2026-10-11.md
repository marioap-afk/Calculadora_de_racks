# I-62 — FX-02: bloque A2-P2 para el Codex actualizado (decisiones §68, punto 3.A y 3.C)

> Supervisión, plano a. Una sola solicitud: §2 al Owner y §3 al Coordinator. Nada se ejecuta sin respuesta expresa; el silencio no es aprobación.
> Una actualización real del runtime no autoriza consumo por sí misma (decisiones §67, punto 3).

## 1. Hecho

Codex se actualizó solo ([result.json](../OD-2/R20261011T034757Z-s68-passive/result.json)):
- binario `d13914ce…` (el único `codex.exe`);
- app `26.1007.2314.0`;
- huella de `config.toml` `F909F480…`, con los mismos 107 nombres saneados;
- **par estable** en dos observaciones pasivas (03:47:57Z y 04:46:50Z). El archivo se reescribió a las 04:30:15Z con contenido idéntico.

Por A-2 (A2-P2) cabe **un** bloque de ≤ 2 sondas de solo lectura para el par exacto, con OD-2 sobre la huella resultante. Por A4-1, reglas 3 y 4, se
abre además otra medición de A4-1 si la ruta `cmd.exe` sigue haciendo falta. Kit preparado y **no ejecutado**:
[a2p2-v4](../kits/FX-02/a2p2-v4/README.md) (autoprueba 22/22, 27/27, 19/19 y puertas 34/34).

## 2. Owner (texto a pegar; antes de cualquier sonda)

```text
OD-2 = A (línea base F909F480E535E0D3D64817972361077B13AEFEEDF5EB191B5E6B941E7E1C8113; binario d13914ced6c7af174d8d638db938284231227312104b06a25e9608c632a7daec)
BLOQUE-CODEX-CONSUMO = A (≤ 2 invocaciones read-only de codex-cli para el par exacto d13914ced6c7af174d8d638db938284231227312104b06a25e9608c632a7daec/26.1007.2314.0, con huella, binario y versión antes y después de cada una; sin workspace-write ni cambios de config.toml, trust_level o sandbox; sin lectura de valores)
```

| Línea | Bytes | SHA-256 |
|---|---|---|
| `OD-2` | 161 | `b76470163f86592dfda474d44c5e2051d1e05ead667362fb8428c32ef7d4ac96` |
| `BLOQUE-CODEX-CONSUMO` | 322 | `953088c953075af184a017fcf48a98c5540ee0cc4e5123edf719fc969f57bf5b` |

Bloque (con LF final): SHA-256 `06ea65fa7a1ea80086b5396f0b53a55c2b125f9bb981fc34409d25db8f797360`.

**Condicional:** solo si la sonda 1 muestra que la limitación de la shell sigue (firma de F6-OBS-03). En ese caso se pedirá aparte la línea
`A4-SONDA-CONSUMO` del trío nuevo, cuyo texto está en `a41-consumo-line.txt` del kit (SHA-256 `4b081e4e37a54fefca931b1ad3a5a621c64bc9c298e9b82333710c8ee9381746`). No se pide ahora.

## 3. Coordinator (disposición del bloque)

1. **Autorizar el bloque A2-P2** nombrando el par `d13914ce…/26.1007.2314.0` y la huella estable `F909F480…`.
2. **Sonda 1, NATIVE_CONTROLLER:** celda `gpt-6-luna/high`, receta 16.4 literal (sin shell declarada), `-C D:\r62-fixture\A2`.
   - Cubre las 32 operaciones de PLAN y VERIFY de la sonda v3.
   - ALL_OPERATIONS_DEMONSTRATED sin la firma de F6-OBS-03 haría elegible al Controller por la vía normal, sin A4-1.
3. **Sonda 2, NATIVE_ARCHITECT:** celda `gpt-6.1-sol/high`, `-C` = un clon de solo lectura nuevo (`arch-a2p2v4`).
   - Clasifica la celda del Architect de Codex con la identidad vigente (MATCH o BELOW_REQUIRED).
   - Si es MATCH, el Architect puede quedarse en Codex sin `claude-cli`.
4. Aprobar la política de comandos de los modos nativos:
   - prohibidas las escrituras, `fetch`/`pull`/`push`, la red y las lecturas fuera del clon;
   - permitidos `pwsh`, `cmd`, intérpretes y herramientas de Unix, como en la receta real.
5. Aprobar las reglas de veredicto, el esquema VERBATIM (canónico sin `$schema`, como en v3) y la secuencia: OD-2 antes de las sondas.
6. Condicional: la disposición de aplicación de A4-1 para el trío nuevo, si la sonda 1 la abre.

## 4. Riesgos

- El primer arranque del binario nuevo puede ejecutar la preparación del sandbox de Windows; si eleva o cambia ACL o cuentas, STOP (la línea excluye
  cambios del sandbox).
- `ls-remote` bajo el sandbox puede volver a fallar.
- La app reescribe `config.toml` por su cuenta: un cambio de contenido antes o durante el bloque es P-01.
- El bloque no resuelve el mapa del fixture (MAP_INVALID): las sondas miden operaciones. VERIFIED exige además B1.
