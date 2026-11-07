# I-64 → I-52: petición de recheck de host de solo lectura tras el STOP de a578 (enviado)

Enviado el 2026-11-05 (~04:58Z) a la sesión «I - 52» (estaba inactiva; id de mensaje c051542a-b9e2-4875-aa56-231f2a316361).

- Contexto dado:
  - STOP CONFIRMED de R20261104T194513Z-a578 (HOST_ISOLATION_VIOLATION / MULTIPLE_AUTOCAD_INSTANCES);
  - la orden del Coordinator: «another read-only host recheck after the current AutoCAD activity is fully closed»;
  - acad.exe = 0 desde 2026-11-04T22:36:54Z; la sesión mide acad = 0 y RAM = 0 a las 04:56Z;
  - I-64 no abrió AutoCAD ni tocó el host después del POST (00:38:54Z);
  - sin RunId nuevo ni BASELINE;
  - Candidato propuesto para reutilizar: `cc7503e3` (producto `0e0d536b`), DLL `3C261B8F…B6A0`, sin recompilar.
- Pide los mismos 9 puntos de los rechecks anteriores, en solo lectura:
  1. procesos;
  2. perfil;
  3. acad.exe;
  4. TRUSTEDPATHS;
  5. SECURELOAD como referencia;
  6. registro sin RackCad y AssemblyMap;
  7. ApplicationPlugins, y ACL y bytes de P2;
  8. CoreUser frente a su recheck de las 00:43:29Z;
  9. operaciones de host y compromiso de no tocar el host desde la BASELINE hasta la POST nuevas y su recomprobación.
- No pide escribir, borrar, reparar ni recrear nada.
  - Las dos carpetas ausentes siguen siendo UNKNOWN_EXTERNAL_HOST_CHANGE, NON-BLOCKING, asunto aparte.
  - No pide ACK/RELEASE.
  - No autoriza P2 H10.
- Incluye la lectura propia de la sesión de las 04:56Z (`session-recheck.txt`) y el HOST-EXTRAS de recomprobación frente al AFTER de a578: CLEAN.
