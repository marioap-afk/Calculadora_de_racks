# I-64 → I-52: petición de recheck de host nuevo para el segundo reintento de Smoke-1 (enviado)

Enviado el 2026-11-04 (~19:35Z) a la sesión «I - 52» (estaba inactiva; id de mensaje d9364c92-b616-44ae-a895-e3a077e57b3d).

- Contexto dado: C2 PASS; Candidato `cc7503e3e4e56bff3a2fc9de7ca30a5322b57ea6` (producto `0e0d536b`); SHA-256 del DLL en cálculo; I-64 sin
  AutoCAD desde el cierre de `0a77`; RunId sin asignar; hay 1 acad.exe abierto que I-64 no lanzó (del Owner), y antes de la BASELINE se
  exigirá acad = 0.
- Pide, solo lectura, los mismos puntos del recheck del 2026-11-03:
  1. acad.exe / RAMElements.exe;
  2. perfil activo;
  3. acad.exe, versión y SHA-256;
  4. TRUSTEDPATHS, número y SHA-256;
  5. SECURELOAD como referencia;
  6. registro sin RackCad, y AssemblyMap con su última escritura;
  7. ApplicationPlugins, ACL y bytes de P2;
  8. AppDataLow/CoreUser frente a su recomprobación posterior a `0a77`;
  9. operaciones de host activas o previstas, y retención de P2 H10 y de su ventana desde la BASELINE hasta la POST y su recomprobación.
- No pide escribir, borrar ni reparar nada. Si el ACK/RELEASE no corresponde a I-52, que lo diga y la sesión lo eleva al Owner.
