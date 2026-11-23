# Disposición de la revisión independiente del v1 de I-64 A-14

**Revisión:** `independent-review-a14-v1.md` (SHA-256 `d3a13ef00d676067be9d5e98a173ea3c585c477341efacdf16a8afae5a3822b4`), sobre el v1
(SHA-256 `7571e16509b6be965efa9d6f6346f8c7701eb9ab104c9d81595ba39fb3342dfb`). Veredicto: APTO TRAS CORRECCIONES.

**Objeto corregido:** `I-64-A-14-draft-v2.md`. Las ubicaciones citan sus secciones (C-nn, §n).

**Recuento:** 29 hallazgos (2 bloqueantes y 2 menores por las rondas nuevas del Owner; 11 importantes y 14 menores del v1).
**Aplicados: 29.** **Rechazados: 0.** Tres se aplican con un ajuste, que se explica en su fila (I-09, M-11 y M-14).

| Id | Severidad | Disposición | Dónde (v2) | Motivo |
|---|---|---|---|---|
| R2-01 | Bloqueante (v2) | Aplicado | C-09 (nuevo); §4 (D-15, D-16, §14, anexo A dec. 10); §5 (INV-19, INV-A14-17, OV-17); C-01 (brief `:393-394`); C-06 y R-07 | Decisión Q-03 del Owner. Se enmiendan de forma explícita: D-16 `:586-587`; el STOP `:593`, que sigue para todo cambio no declarado; D-15 `:570-572`; INV-19 `:757`; M-03 `:996`; OV-17 `:949`, que pasa a la lista del Owner; y el brief `:393-394`. Los comandos de creación se incluyen porque la ventana es la misma (PKG `:171`). Sustitución con registro declarado: OD `:237-243` y `superseding-characterization.md`. Se quitan R-14 y Q-03, y «los flujos clásicos quedan como respaldo» queda acotado a «hasta la fase de cada sistema» |
| R2-02 | Bloqueante (v2) | Aplicado | C-12 (nuevo); §4 (§12.6, D-20, D-21); §5 (asignación por tramo); §6; §8 | Decisión Q-09. Un tramo es una unidad de entrega (LIFECYCLE §2 `:54-64`), con READY-01..09, `FINAL_CANDIDATE_SHA`, conformidad, OV y la integración de WORKFLOW §4.5, limpieza tras §4.5.6 y §4.5.7. Resuelto con las fuentes: ID3 se clasifica en el último tramo (D-20 necesita saber qué sistemas quedaron); la limpieza retira la rama del tramo y el tramo siguiente es una unidad nueva (WORKFLOW §2, §3, §4.1; LIFECYCLE §2 `:63-64`; precedentes I-53S, I-53D e I-52-AUTH15); F4B sin cambio. Para el Coordinator: Smoke-PERF por tramo (Q-C3) y ADR-0047 (Q-C4), con propuesta y opciones. Hitos: PROPUESTA de cinco tramos, para que el Owner la confirme al firmar |
| R2-03 | Menor | Aplicado | C-07; INV-A14-01 y 02; R-13; fase 9 | Decisión Q-07: RK abre la ventana (es el mismo método, `RackMenuCommands.cs:21`); la entrada de respaldo hasta la fase 9, en INV-A14-02 y en la lista de retiro; abrir el menú desde la ventana entra en R-13 |
| R2-04 | Menor | Aplicado | §2 (dec. 9); C-10; C-11; §8 | Decisión Q-08: fuera de A-14; se decide con el diseño de Configuración. Q-08 deja de ser pregunta |
| I-01 | Importante | Aplicado | Cabecera (Amends / Sin cambio); §4 (tabla completa); §8 | Pasan a «Amends», con su delta: D-04, D-05, D-10, D-14, D-18, D-21, §0, §2, §14 y los anexos A (decisiones 1, 6, 8, 10, 11 y consecuencias) y B (las dos notas). Se añade D-07, que la revisión no nombraba y que también habla de pestañas (`:349`, `:352`, `:365`, `:367`, `:371`). «Sin cambio» queda en D-01 (host, contingencia, foco y perfil), D-11, D-12, D-19 y §12.1. ADR-0047 recibe los textos nuevos del anexo A |
| I-02 | Importante | Aplicado | §5 (tabla de invariantes y de OV); C-03; §4 (§12.5) | Se añaden INV-02 (RACKCAD y RK repetidos), INV-05, INV-07, INV-08, INV-15, INV-18, INV-20, INV-25, INV-37, INV-38 e INV-39; OV-11 y OV-20 pasan a la lista del Owner; SP-02 se enmienda; las obligaciones de IC-02 e IC-05 que nombran pestañas se sustituyen (C-03); los resultados de F3 que retira la fase 2 se sustituyen con registro declarado, sin reabrir F3 (C-03). Además, C-03 añade una regla de lectura con la lista cerrada de líneas que nombran pestañas, para no dejar ninguna enmienda invisible (LIFECYCLE §6 `:203`) |
| I-03 | Importante | Aplicado | C-05 (composición, condiciones 1 y 2, autoridad); INV-39, INV-A14-11 y 12; R-14 | Verificado en el código: `RackSiblingRedrawRun.Execute` recibe la pertenencia de un rack y un puerto por sistema (`:30-32`). Se declara como composición nueva sobre la costura de un rack, sin cambiarla. Las condiciones que el paquete de la fase 4 debe demostrar son una transacción y un Deshacer (con INV-39 extendido) y el coste de N barridos (arnés con N ≥ 70; la captura compartida es mecánica posible, no congelada). La autoridad: Architect + Coordinator en el paquete de la fase 4; si exige cambiar la costura, M-08, STOP y el Freeze de I-55 (V5 `:32-33`, `:449-450`). Las fases 6, 7 y 3b extienden las pruebas a sus sistemas |
| I-04 | Importante | Aplicado | C-05 (condiciones 3 y 4); INV-A14-15 | La regla sale de V5: el texto conservado no se compromete fuera de una frontera bloqueable (D-10 `:436-437`, INV-18, la nota a ADR-0032); Conflicto pide reconciliación (`:487-493`), Desconocido pide «Comprobar ahora» (`:531-532`) y Error ofrece Recargar o Descartar (`:396`). «Posiblemente obsoleto» entra, porque la comprobación previa y la relectura deciden |
| I-05 | Importante | Aplicado | C-02 (preview de navegación y vida del borrador); §4 (D-10); INV-A14-07 y 08 | Se separan la pestaña preview de D-08 (desaparece) y la preview de navegación de D-10 (se conserva, con INV-27). La vida del borrador traslada «cerrar una pestaña limpia» a «dejar de tener abierto un rack Limpio»; un borrador dirty sobrevive |
| I-06 | Importante | Aplicado | C-02; INV-A14-06; anexo A dec. 6 | El valor inicial queda congelado en «encendido», que es lo que V5 congela (D-06 `:335`, criterio 3, OV-04) y lo que trae el prototipo. «Apagado» es elección del usuario. No hace falta preguntar al Owner |
| I-07 | Importante | Aplicado | C-11 (nuevo); C-10 (excepciones 2 a 4); C-01 (brief `:444-457`); §6 (fase 5, dependencias); R-08 | La fase 5 entrega dentro de la ventana solo lo que no escribe el DWG. Variables, con su borrador, «Actualizar todo» (Q-02) y su propia A-n (D-10 `:410`, D-12 `:475-477`, D-11 `:451`, D-17, ADR-0034), y depende de la 4. Biblioteca lanza el flujo clásico para insertar (`RackMainMenuWindow.xaml.cs`: `OpenFromLibrary` produce un `InsertionRequest`). D-22 recibe excepciones nombradas. Se declara cambiado el brief `:444-457` y se corrige R-08 |
| I-08 | Importante | Aplicado | C-11 (privacidad); INV-A14-10; §4 (D-05); INV-36 | Invariante nuevo con su RED, porque la guarda de INV-36 es por carpeta (`:774`). Se citan D-05 `:312-313` y MASTER-I63-I64-02 §3 (V5 `:260`) |
| I-09 | Importante | Aplicado, con ajuste | C-10; INV-A14-03; C-01 (brief `:474`) | S1-11 se repite en el smoke de la fase 1, con F1-T3C y el acuse de I-52; se cita el brief `:474`; INV-A14-03 añade «no altera los demás ajustes» (`UserSettings.cs:13-27`). **Ajuste:** la revisión pedía aclarar si el estado acoplado o flotante entra en «la posición». La v2 lo incluye como lectura de la decisión del Owner («la ventana recuerda»), y qué valores se guardan queda como mecánica no congelada |
| I-10 | Importante | Aplicado | C-01 (alcance verificable) | Censo de `CommandMethod` comprobado en `src/RackCad.Plugin`. El objetivo verificable es el menú de RACKCAD, las cuatro secciones, «Nuevo» y la lista; los demás comandos siguen como comandos hasta la lista de la fase 9 |
| I-11 | Importante | Aplicado | §10 (orden nuevo); pie | Orden: revisión independiente y v2; versionar la v2 PROPOSED junto con el registro del Owner y la replaneación (Q-C1 antes); revisión del Architect de ese commit; si pide cambios, A-15; Coordinator AGREED; firma; activación. Precedentes: A-12 → A-13 y A-7 «sin editar» (`I-64.yml`, `f1_a7_status`) |
| M-01 | Menor | Aplicado | §6 (correspondencia) | Se citan OD `:269` («F5-A remains authorized but WAITING») y `:293` («AUTHORIZED / WAITING FOR F4B») |
| M-02 | Menor | Aplicado | C-05 | «Descartar y recargar» (V5 `:488`), con «Volver a leer» como rótulo de la ventana |
| M-03 | Menor | Aplicado | §1 | La v2 dice que el registro anota diferencias con el Freeze y solo prevalece en el mecanismo (ICL `:3`); un cambio de resultado no se hace por IC |
| M-04 | Menor | Aplicado | Cabecera (Relation to F4B); C-08 | «D-PB-01 rige F4B sin cambio; A-14 la sustituye desde el cierre de la fase 3» |
| M-05 | Menor | Aplicado | §11 (nuevo); cabecera (Owner) | Texto exacto de la firma, con la lista OWNER-RESERVED, incluidos los OV-08, 09, 11, 14, 17 y 20 y la autorización de F5-A. Q-05 y Q-06 dejan de ser preguntas aparte, como propone la revisión |
| M-06 | Menor | Aplicado | C-08 | «La ventana deja de medir 320 px: sigue siendo el mismo `PaletteSet`» |
| M-07 | Menor | Aplicado | C-02 | Se quita «si la frontera lo permite». Sin texto pendiente, abre; con texto pendiente, marca |
| M-08 | Menor | Aplicado | C-05; INV-A14-11 | Causa «No aplicado: falló otro rack del conjunto» |
| M-09 | Menor | Aplicado | C-05 | «Ningún otro Actualizar mientras dura» (`:545`), sin limitarlo a la sesión |
| M-10 | Menor | Aplicado | §5 (OV-A14) | Autoridad: la de A-14 (LIFECYCLE §6 `:204-205`: el Coordinator solo añade OV de lo **ya** congelado) |
| M-11 | Menor | Aplicado, con ajuste | §4 (§12.5); §5 (OV-A14-03); R-03 | «Sobre los dibujos más grandes del Owner, al menos uno de unos 70 racks». **Ajuste:** se aplica también a OV-A14-03 y a R-03, que decían lo mismo |
| M-12 | Menor | Aplicado | §2 (decisión 1) | La aprobación de las fases del §4 de la replaneación se marca como lectura de la sesión |
| M-13 | Menor | Aplicado | C-11; §4 (D-23); §9 (M-06) | Las secciones son nuevas; las regiones de inspector de D-23 (`:708-709`) son solo su antecedente. M-06 se justifica ahora por el borrador por lotes, no por la región de variables |
| M-14 | Menor | Aplicado, con ajuste | R-17 (nuevo); §10 (paso 3) | Se redactan también los nombres de racks, de proyectos y de clientes, y lo que no se pueda redactar se custodia solo por SHA-256. **Ajuste de la cita:** el precedente está en PB-T5 v2.4 `:17`, no en `:14` |

## Preguntas de la revisión (§6)

| Pregunta | Destino en la v2 |
|---|---|
| P-1 (Q-02) | Contestada por el Owner en la tercera ronda («Primero racks, luego variables»), que es la opción a) recomendada: C-05 y C-11 |
| P-2 (Q-10) | Contestada por el Owner («Sí, en la fase 2»): C-02 y la fase 2, con la limitación declarada |
| Q-04 | Con su propia A-n antes de la fase 8 (C-06); sale de las preguntas |
| Q-05 y Q-06 | En la firma (§11) |
| Q-11 y Q-12 | Del Coordinator: Q-C1 (antes de la revisión del Architect) y Q-C2 |
| La frontera de cada tramo | Se confirma al abrir cada tramo (C-12) |
