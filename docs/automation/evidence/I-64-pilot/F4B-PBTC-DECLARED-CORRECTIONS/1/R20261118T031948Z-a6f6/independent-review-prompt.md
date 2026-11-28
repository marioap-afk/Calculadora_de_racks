Revisión adversarial INDEPENDIENTE del código de I-64 F4B-PBTC-DECLARED-CORRECTIONS, intento 1 (RackCad, plugin de AutoCAD 2025, C#/.NET 8, WPF). No participaste en ella. Busca defectos reales y no te fíes del resumen de la entrega.

Worktree (solo lectura): `<worktree-de-I-64>`, rama `architecture/workspace-persistente-rackcad`. Usa siempre `git -C <worktree>`, sin checkout.

**El objeto es todo el cambio de PB-TC:** `git diff 48fe199e3a024d98be28dc697c1e9ce0324e95a5 4440f67d4c76fb2ec019a566231353acdcca61f6`.
- BaseSha = ChainBaseSha `48fe199e`: el commit de emisión del contrato.
- RED `f665edac046014533f031adfa797291a3d027df2`: las pruebas nuevas y la línea del atributo de las cuatro pruebas sustituidas SUS-01 a SUS-04, sin cambios en `src/`.
- GREEN `4440f67d4c76fb2ec019a566231353acdcca61f6`: la corrección, solo en la ventana clásica de Push Back (`RackPushBackSystemWindow.xaml.cs`, `.Composite.cs` y `.xaml`).
- PB-TC corrige, como cambios DECLARADOS de RACKEDITAR y por decisión expresa del Owner:
  - D-PB-06 (a): diez campos que hoy se guardan redondeados a tres decimales, con la regla `Keep` de Cantilever;
  - D-PB-06 (b): el estilo de cotas cargado;
  - D-PB-08: un aviso de solo texto en la banda de estado;
  - D-PB-11: «Restaurar estándar» llega al Actualizar.
  - El caso H-31 (`GapBox`, `ModuleLengthBox` y las cuatro cajas avanzadas anulables) NO entra.

**Autoridades** (léelas en el GREEN):
- el contrato `docs/automation/evidence/I-64-pilot/F4B-PBTC-DECLARED-CORRECTIONS/coordinator/gate-contract.json`: Invariants (INV-PBTC-01 a 19), AllowedWriteScope (13 rutas exactas), ForbiddenWriteScope (131), RequiredTests y StopConditions (en especial PBTC-C01, C02, C03, C04, C07, C09, C10 y C14);
- el paquete `docs/initiatives/I-64-f4b-pbtc-gate-contract-package.md` v2.2:
  - §2, los hechos medidos;
  - §3, el diseño POR LÍNEA de D-PBTC-01, 02, 03 y 11;
  - §4, los CT con sus líneas esperadas y el mapa 4.4;
  - §5, el RED y las sustituciones;
  - §6, el contrato;
- y su registro `docs/initiatives/I-64-f4b-pbtc-measurements.md`;
- las decisiones literales del Owner `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-pbtc-decisions.md`;
- el registro declarado `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md`;
- el prompt del Worker `artifacts/orchestration/I-64/F4B-PBTC-DECLARED-CORRECTIONS/1/R20261118T021306Z-069b/prompt.md`;
- la entrega `…/R20261118T021306Z-069b/worker-handoff.json`, con los TRX y los logs bajo `…/R20261118T021306Z-069b/tests/`. **Ojo:** la entrega cita esos artefactos con un prefijo erróneo `System.Xml.XmlElement/tests/…`; están en `…/R20261118T021306Z-069b/tests/…`. Los de la CI están en `ci-core/`, `ci-ui/`, `red-ci-core/` y `red-ci-ui/`.

**QUÉ REVISAR**, con evidencia `ruta:línea` en el GREEN o en el RED:

1. **Alcance.** Compara tú las listas, sin fiarte de ningún resumen:
   - `git diff --name-only` BaseSha..CurrentSha frente al AllowedWriteScope exacto, y ninguna ruta bajo el ForbiddenWriteScope;
   - el RED no toca `src/` y el GREEN no toca `tests/`;
   - en los cuatro archivos de SUS, el diff es exactamente la línea del atributo, con el literal del §5.2;
   - nada del caso H-31 ni de ningún otro miembro que el paquete deje fuera (INV-PBTC-15).
2. **El diseño frente al código del GREEN.** Para D-PBTC-01, 02, 03 y 11: ¿el código hace exactamente lo que el paquete fija por línea, ni más ni menos?
   - ¿La regla `Keep` es correcta en todas las rutas: carga, «Restaurar», selección de celda, «Ambos lados» y `SetMixed`, alcance y datos del frente, biblioteca?
   - ¿Hay algún lector de esas cajas que siga redondeando, o algún comportamiento que cambie sin estar declarado?
   - ¿Los portadores pueden quedar desfasados respecto a la caja, por ejemplo tras una pasada con un campo en error, tras «Restaurar» o tras una carga nueva sobre la misma ventana?
   - ¿`OptionalVal` conserva la semántica de nulo?
   - ¿El aviso se evalúa siempre que cambia la presencia de B, y nunca bloquea nada?
   - ¿D-PB-11 puede reiniciar los avanzados en un caso en que el usuario no pidió «Restaurar estándar» (por ejemplo un commit de módulos sin ese flag, o un restore individual)?
3. **Cada CT** (CT-PBTC-01 a 08, 10 y 14, en `tests/RackCad.UI.Tests/PushBackDeclaredCorrection*.cs`):
   - ¿la clase y el método son los del mapa 4.4?
   - ¿las líneas esperadas son las del §4.2, sin ajustar?
   - ¿fija de verdad lo que promete, o alguna aserción es tautológica o pasaría con cualquier producto?
   - ¿se cumplen las reglas de las ventanas (`Ventana()`, la carga dentro del `try`, `Cerrar` en el `finally`, ninguna ventana mostrada), sin teclado ni foco real, sin `MessageBox`, BOM, configurador, «Guardar en biblioteca» ni «Insertar»?
   - Marca los CT débiles.
4. **El RED** (TRX de la CI en `red-ci-ui/`): ¿fallan los diez métodos nuevos, y por aserción y no por excepción? ¿Ninguna prueba existente falla? ¿Los únicos `Skipped` añadidos son SUS-01 a SUS-04?
5. **La medición de la base de D-PBTC-10 (PBTC-C14).** ¿La corrida temporal `m10` (en `tests/m10/`, con el código temporal en `temp-measure-k.cs.txt`) mide lo que el paquete pide, con la fórmula de CT-PB0-21? ¿Su conclusión («no material», sin líneas bajo `Modules` ni `HeaderLineOverrides` en F1 ni en M-02) se sostiene con el TRX?
6. **Las sustituciones.** ¿La comprobación temporal sobre el GREEN (`tests/s1` a `tests/s4`) demuestra lo que dice el §5.2? ¿Cada prueba nueva cubre todo lo que cubría la vieja?
7. **El GREEN y la estabilidad.** CI 4/4 (TRX en `ci-core/` y `ci-ui/`). ¿Hay dependencias de orden, de reloj, de cultura, de rutas de la máquina, de foco o de ventanas que hagan frágiles las pruebas? ¿Alguna deja viva una ventana, un modal o un dispatcher (PBTC-C09)?
8. **Contención.** Ninguna prueba escribe fuera de una ubicación temporal aislada ni en los datos reales del usuario, ni carga el Plugin ni AutoCAD.
9. **El producto.** ¿Ves algún defecto nuevo que introduzca la corrección en RACKEDITAR, por ejemplo una pérdida de datos, un valor que ya no se puede cambiar, un aviso que no desaparece o un reinicio no pedido, y que el Owner deba conocer? No propongas corregir nada fuera del alcance.

**REGLAS:**
- Solo lectura, con git y lectura de archivos.
- No ejecutes dotnet ni pruebas.
- No escribas en el worktree ni en ningún otro repositorio.
- Lo que no puedas verificar, dilo.

**SALIDA:** el informe en español en `<scratchpad>\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\pbtc\independent-review-code.md`, **sin rutas locales absolutas**, con:
- el veredicto: ACEPTABLE, ACEPTABLE CON MENORES o REWORK;
- el recuento B/I/M (bloqueantes, importantes, menores);
- la tabla de hallazgos, con id, gravedad, contenido, evidencia y corrección;
- la comparación de las listas de alcance;
- la tabla diseño → código por cada D-PBTC (conforme sí/no, con la evidencia);
- la tabla de los CT con su valoración: bien, débil o no fija lo prometido;
- los hallazgos OWNER: una decisión material nueva o un posible defecto de producto que deba conocer;
- lo no verificado.

Tu respuesta final, en dos líneas: el veredicto con B/I/M, y los ids de los bloqueantes, importantes u OWNER.
