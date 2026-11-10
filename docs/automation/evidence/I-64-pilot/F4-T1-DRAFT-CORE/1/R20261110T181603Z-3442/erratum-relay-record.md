# Fe de erratas del registro de relevo del trabajo de F4-T1, intento 1

- **Artefacto original:** `relay-record.json` de esta carpeta (`F4-T1-DRAFT-CORE/1/R20261110T181603Z-3442`). Su SHA-256 consta en el `custody-manifest.json` de esta carpeta. No se reescribe.
- **Error:** la nota de procesos de `Notes` dice que el primer recuento de salida dio 2 participantes y que se repitió con una orden sin la ruta del worktree.
- **Hecho:** en esta cesión el primer recuento de salida dio 0 participantes y no se repitió, como acredita el `processes-exit.json` custodiado en esta carpeta.
- **Causa:** el script que generó el registro se derivó por plantilla del de F3-T1, donde esa frase sí era cierta, y la heredó sin cambios.
- **Efecto:** ninguno sobre el producto ni sobre la verificación `R20261110T184730Z-e455`, que evaluó los procesos desde sus propias entradas.
- **Autoridad:** disposición del Owner `F4-DRAFTS/coordinator/owner-f4-disposition-2.md` (F4-E09).
