# Traspaso del autor de la v2.3 de la corrida de esta noche

**Encargo:** `pre-run-tonight-v2.3.md`, tras la «Recomprobación 3 (v2.2, la corrida)» de `review-v1/independent-review-pbt5-v1.md` (sha256 del archivo entero al leerlo: `3d437ab897fa013d1c2db50db9b27dc1a619b22d43f4e15fa0cd2c0d7bc02a9d`).
- **El resultado:** EJECUTABLE TRAS CORRECCIONES (I-09); M-31 a M-33, recomendables.
- **Las reglas, las de siempre.** Ningún archivo anterior se sobrescribe.
- **El paquete y las preguntas no cambian:** siguen en la v2.2. El paquete cita `pre-run-tonight-v2.2.md`; para esta noche manda la v2.3.

## Cómo se hizo
- **`_tools/make_v23.py`** genera la v2.3 desde la v2.2. Hace reemplazos exactos y sustituye secciones entre marcas, cada una presente una sola vez, y no sobrescribe. Toca:
  - el título y los cambios;
  - el bloque 1;
  - §4.2, que pasa a los bloques B1, B2 y B3;
  - el código de §4.3;
  - §4.5, con los bloques 4b, 5, 6 y 7;
  - el bloque 8 de §4.7;
  - el bloque 9 de §4.8;
  - el bloque 10 de §4.9;
  - la fila P-03.
- **`_tools/add_exit0_v23.py`** añade `exit 0` al final de 15 bloques (el de §4.3 ya lo llevaba) y la nota correspondiente. El motivo está abajo, en el punto 1.
- **`_tools/ps_parse_v23.ps1`** analiza **sin ejecutar** los 16 bloques de PowerShell con el analizador de PowerShell. Da 0 errores de sintaxis; todos llevan `Set-StrictMode` y `Stop`; y ninguno usa una variable que no asigne él mismo.
- **La prueba del bloque 4b,** que solo lee el proceso de la propia sesión y no escribe nada, corrió una vez en la herramienta (PowerShell 7.7.0). El tipo leído del JSON es `System.Int64`, y los `Ticks` vuelven iguales.

## Lo que se aplicó de otra forma, o se añadió
1. **Nuevo, medido: `exit 0` al final de todos los bloques.**
   - Al correr la prueba del bloque 4b con `Set-StrictMode -Version Latest`, la herramienta dio «Exit code 1» después de escribir «OK». La causa: su envoltura (`Invoke-Expression`) lee `$LASTEXITCODE` al terminar, y si el bloque no ejecutó ninguna orden externa, esa variable no existe, y StrictMode lo convierte en error.
   - Con `exit 0` después del «OK», la orden sale limpia; eso también está medido.
   - Afecta a los bloques 2, 3, 4, 4b, 5, 6, 7 y 9, que no llaman a ninguna orden externa, o no al final.
   - Es la misma solución que el encargo pedía para §4.3 (M-33), extendida a todos. Un `throw` anterior sigue saliendo con un código distinto de 0.
2. **M-31.** B1 (Core) y B2 (los 19 filtros) van en segundo plano, con su salida en el archivo de la tarea y no en `<raiz-de-trabajo>`. B3 (los builds) va en primer plano, y en segundo plano si se acerca a los 10 minutos.
   - **El archivo de disposición** se mide antes y después dentro de B2, que es el único que corre pruebas de UI.
3. **M-32.** Para que el registro se pueda escribir antes de `WaitForInputIdle`, la ruta registrada es la de `$psi.FileName` (`[IO.Path]::GetFullPath(<acad.exe>)`). No es `Get-Process … .Path`, que puede no estar aún disponible al lanzar.
   - Los bloques 6 a 9 la comparan con `$q.Path`. En PowerShell, `-ne` entre textos no distingue mayúsculas.
   - Si `<acad.exe>` se diera con un nombre corto 8.3, no casaría, y el bloque 6 pararía, que es seguro. **Hay que dar la ruta larga.**
4. **La lectura de las copias abiertas.** Atiende lo no verificado de la recomprobación. En el bloque 10, cada `Get-FileHash` de una copia va en un `try`: si AutoCAD la tiene abierta y no se puede leer, se registra «no legible (abierta)» y se sigue con las demás.
5. **M-33.** El bloque 1 exige 40 caracteres hexadecimales para el Candidato y para el producto. B3 y el bloque 10 lo exigen para el Candidato. El bloque 10 exige además 64 para el SHA-256 del manifiesto.

## Lo que sigue sin verificar
- **Ningún bloque, salvo la prueba del 4b, se ha ejecutado,** y nada en AutoCAD.
- **`WaitForInputIdle` con AutoCAD.** Ahora va en un `try`, y el registro ya está escrito antes.
- **Que `$p.StartTime` esté disponible justo después de `Start`.** Si no lo estuviera, el bloque 5 pararía antes de escribir el registro, con AutoCAD vivo y sin registrar, que es el caso que M-32 quería evitar. Es el único punto en que eso puede pasar.
- **Que la herramienta deje correr en segundo plano órdenes de más de 10 minutos** sin cortarlas.
