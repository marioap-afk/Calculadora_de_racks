# I-64 → I-52: petición de recheck de host de solo lectura tras 785a (enviado)

Enviado el 2026-11-07 (~17:09Z) a la sesión «I - 52», que estaba inactiva (id de mensaje 245731d3-3083-4fc4-a90d-2012d58dcc75).

- Contexto dado:
  - 785a = NOT PASS / CLOSED, custodia `1917bc19` aceptada (CI 42324525500 push 4/4), sin corrección de producto;
  - la orden del Coordinator: FULL NEW SMOKE, «a fresh host recheck after closure»;
  - la actividad de host del Owner desde el recheck de I-52 de 2026-11-05T19:51:33Z: PID 16088 y una segunda apertura breve;
  - sin RunId, BASELINE ni NETLOAD de I-64;
  - Candidato propuesto para reutilizar: `cc7503e3`, DLL `3C261B8F…B6A0`, sin recompilar.
- Incluye la lectura propia de la sesión de las 17:06:34Z y el HOST-EXTRAS frente al AFTER de 785a.
- Pide los mismos 9 puntos de los rechecks anteriores, en solo lectura:
  1. procesos;
  2. perfil;
  3. acad.exe;
  4. TRUSTEDPATHS;
  5. SECURELOAD como referencia;
  6. registro sin RackCad y AssemblyMap;
  7. ApplicationPlugins, y ACL y bytes de P2;
  8. CoreUser frente a su lectura del 2026-11-05;
  9. operaciones de host y compromiso de no tocar el host desde la BASELINE nueva hasta su POST y su recomprobación.
- No pide escribir, borrar, reparar ni recrear nada.
  - No pide ACK/RELEASE.
  - No autoriza P2 H10.
