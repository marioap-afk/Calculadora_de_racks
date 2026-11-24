# SPB-52: el entorno al final (bloques 2, 3, 4 y 15 de `smoke-run-v2.md`)

- **Hora:** 2026-11-24 05:44 UTC, con ningún AutoCAD vivo.
- **Cómo se corrieron:** cada bloque en su orden; solo leen.
- **Valores locales:**
  - `<carpeta-de-registros-de-RackCad>` = `<datos-de-aplicacion-del-usuario>\RackCad\logs`, la de `RackLog.cs:65-69` del Candidato.
  - `<settings-de-P-05>` = el SHA-256 que dio el bloque 4 en P-05.

## Bloque 2

```text
OK: sin RACKCAD_DEBUG_* y sin bundles
```

No hay ningún `acad` ni `accoreconsole`.

## Bloque 3

```text
Entradas de RackCad en Applications, Loaded o AssemblyMap: 0
Dialogs\RackCad.* (solo se cuentan): 20
```

Es lo mismo que en P-05: 0 entradas y 20 `Dialogs`.

## Bloque 4

```text
Carpeta de disenos existe: True
Disposicion 7AAB8FF7A3E52CE52399DE3B2C8A8AFEEC0C01720D90DAD0CA49D108BFF82621 2026-11-23T23:47:10.8214882Z
settings.json A7667DBDCEDA5A5175C844ED396BA1F0B17207295F40E9986C29D826B48B2873
```

Los tres últimos `rackcad-*.log` de la carpeta son de 2026-11-03, 2026-11-10 y 2026-11-15. Los tres son anteriores a la corrida.

## Bloque 15

```text
Desde (UTC): 2026-11-23T13:16:23.3747514Z
Registros de RackCad desde el inicio: 0
Eventos Application Error, .NET Runtime o Application Hang desde el inicio: 0; de acad: 0
settings.json antes A7667DBDCEDA5A5175C844ED396BA1F0B17207295F40E9986C29D826B48B2873 despues A7667DBDCEDA5A5175C844ED396BA1F0B17207295F40E9986C29D826B48B2873 igual=True
```

## Dos cosas, explicadas

1. **La disposición `RackFrameConfigurator.layout`:**
   - Su contenido es el mismo que en P-05 (`7AAB8FF7…`).
   - Se escribió de nuevo a las **2026-11-23T23:47:10Z**. En P-05 su última escritura era de 2026-11-19T21:41:29Z.
   - A esa hora **no había ninguna instancia de la sesión viva**: la 2 se cerró a las 20:42 y la 3 arrancó a las 23:50:34, según el `StartTicksUtc` de su `acad-session-3.json`.
   - Fue durante la pausa en la que el Owner usaba el equipo. No es una escritura de la sesión.
2. **La carpeta de registros:**
   - En P-05 se anotó que la carpeta de registros «no lista ningún `rackcad-*.log`», pero el valor que se usó entonces no quedó en el registro.
   - Ahora se usó la carpeta del código y se miró también `<datos-locales-del-usuario>\RackCad\logs`, que solo tiene un `rackcad-20260620.log`.
   - **En ninguna de las dos hay un archivo escrito desde el inicio de la corrida**, así que la conclusión no cambia.

## Resultado propuesto

**PASS**:
- no queda configuración de RackCad;
- no hay errores de RackCad ni de acad;
- `settings.json` no cambió;
- las 20 `Dialogs` son las de P-05 (la escritura esperada del host).
