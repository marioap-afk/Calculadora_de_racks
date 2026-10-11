# I-62 — Solicitud futura de medición del Controller de FX-02 con el instrumento B-min (preparada; decisiones §76, punto 6)

> Supervisión, plano a. **Preparación: no se pide nada y no se invoca nada.** Esta solicitud es **distinta** de la decisión del Owner sobre ADR-0046 #9
> (paquete OWNER-RESERVED aparte): ni la implica ni la sustituye. No reutiliza presupuesto agotado: el bloque A2-P2 de `d13914ce…` está 2/2 (§74.1) y
> la medición de A4-1 está consumida.

## 1. Qué se mediría

**Una** invocación read-only de `codex-cli` que demuestre la ruta del Controller de FX-02 con el instrumento fijado por blob (A-6, punto 7;
patrón de A4-1, reglas 2-4: tope 1, sin reintento, invalidadores declarados). Mediría:

- **operaciones de PLAN y VERIFY con el instrumento.** Cuántas invocaciones hacen falta lo fija A-6: PLAN y VERIFY no se juntan en una
  RoleInvocation (AP L1240), y la forma de la medición la decide la disposición;
- **fidelidad por representación ASCII**;
- **detección de una salida alterada del instrumento**, si A-6 lo prevé dentro o fuera de la ronda (dictamen: dentro sería material).

## 2. Condiciones previas (todas)

1. Decisión del Owner sobre ADR-0046 #9 = A (paquete OWNER-RESERVED).
2. A-6 AGREED (Architect y Coordinator) en el texto que se revise.
3. Disposición del Coordinator que nombre:
   - el modo, la celda (`codex-cli` / modelo / effort) y el `-C`;
   - el par de identidad vigente en ese momento (BinaryHash, AppVersion, huella), con OD-2 al día;
   - los blobs de la herramienta y del aceptante;
   - la declaración del intérprete.
4. **Línea de consumo del Owner** (§3).
5. Preflight: identidad exacta, blobs exactos, intérprete, clon aislado, ninguna sesión activa sobre él y ningún trabajo crítico de I-64 en curso.

## 3. Línea del Owner (texto exacto de A-6 §7 (b); no se pide hasta que el Coordinator la disponga)

```text
A6-CONTROLLER-MEDICION-CONSUMO = A (1 invocación de medición read-only de codex-cli, celda gpt-6-luna/high del EXECUTION_CONTROLLER de FX-02 en F6, con el instrumento de blob 9a829b0eeb1210238052b1feac06cfa8f7964aed y el intérprete que nombre la disposición, para el trío exacto d13914ced6c7af174d8d638db938284231227312104b06a25e9608c632a7daec/26.1007.2314.0/F909F480E535E0D3D64817972361077B13AEFEEDF5EB191B5E6B941E7E1C8113, con huella, binario y versión antes y después; fuera de A2-P2, sin reutilizar su bloque agotado ni la invocación consumida de A4-1; sin reintento; sin workspace-write ni cambios de config.toml, trust_level o sandbox; sin lectura de valores; solo con A-6 AGREED, con A6-ADR0046-9-FX02 = A y con la disposición del Coordinator que la nombre; distinta de A6-ADR0046-9-FX02 y no implícita en ella; no acepta su resultado ni amplía OD-2 ni OD-5)
```

SHA-256 de la línea: `b52a46e34aea2926c53234e9ca4241d7e3f16bfe6655fbf3623c74d99128fa44`.

## 4. Invalidadores (cualquiera anula la medición, sin reintento)

- binario, versión o huella distintos del par nombrado;
- blob de la herramienta o del aceptante distinto del fijado;
- intérprete distinto del declarado o ejecución sin `-I`;
- lectura fuera del cierre;
- cualquier escritura en el clon.

## 5. Qué no hace esta medición

- No da `EXECUTION_VERIFIED` ni C-24 PASS: D.3 cuenta la planificación y la verificación por separado, con el Worker entre las dos.
- No usa el oráculo de C-24 ni se lo entrega al Controller.
- No abre FX-U2 ni crea B1.

## 6. Observación de la preparación

La línea (b) nombra el trío vigente (`d13914ce…` / `26.1007.2314.0` / `F909F480…`) y la celda `gpt-6-luna/high`. Si Codex se actualiza antes de la
medición, la línea queda desfasada: haría falta OD-2 sobre la identidad nueva y una línea de consumo nueva con el trío nuevo (A-6, §7).
