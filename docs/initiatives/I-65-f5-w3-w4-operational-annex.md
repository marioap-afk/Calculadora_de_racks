# I-65 — F5: anexo operativo de las autorizaciones de W3 (AutoCAD) y W4 (RAM Elements)

Documento del Coordinator (2026-10-19), por la disposición del Owner «F5 Milestone Candidate Declaration»
([texto literal](../automation/decisions/I-65-owner-disposition-f5-milestone-candidate-declaration.original.txt); decisiones
[§89](../automation/decisions/I-65.md), CD-I65-F5-23). Recoge lo que el Owner ordena para A65-F5C-03..06.

> **Alcance.** Es una **autorización operativa** registrada después del Candidato declarado `bd1fcf8a` (permitido solo para el hito F5,
> CD-I65-F5-21). Se suma a los [procedimientos de W3/W4](I-65-f5-w3-w4-procedures.md) del Candidato y **no** cambia su contenido:
> ni condiciones de entrada, oráculos, tolerancias, matriz AC-F5-01..10, cargas, tabla, producto ni Freeze. Ante cualquier
> contradicción manda el procedimiento del Candidato y se detiene la ventana (STOP). **W3 = NO y W4 = NO** hasta la autorización
> específica del Owner, que citará este anexo.

## 1. Identidades exactas de herramientas (A65-F5C-03)

Cada herramienta se mide (SHA-256) inmediatamente antes de usarla. Si un hash no coincide: STOP, sin sustituir la herramienta.

### 1.1 Producto y entradas (Candidato declarado)

| Elemento | SHA-256 / identidad |
|---|---|
| `D:\r65-smoke\bd1fcf8a\bin-Debug-net8.0-windows\RackCad.Plugin.dll` | `81299b87eb14f41b61721e79c266d76a15345cdcaac23b193496d2d57f31fef5`, ProductVersion `1.0.0+bd1fcf8a8dc1339c0a7d4c1d9a379186dbb1b109`; salida de 32 archivos, manifiesto `bd096bec0f2410449a61dae28a3571bb77e55af99e6cc5e86d978605b03a301f` |
| Tabla junto a la DLL | `fc3d91464ac48a15730661efcd50ecb770114559c68bdefae67e0816972db619` (blob `d088e2a9…`) |
| `D:\r65-smoke\bd1fcf8a\inputs\ov-i65-02.loads.json` | `d675d9f6bb537083a63571f08fb9884c1ab085a0eca4ad1555ee98bea4f8ba4c` |
| `D:\r65-smoke\bd1fcf8a\expected-inputs.json` | `c61a6e1255783f3c59f3e236c0057bbd88b3b1eabbbe424ba7dced71f22ddaf8` (bytes en disco, CRLF) |
| Biblioteca de bloques `D:\Base_de_datos_AutoCAD_V.0.dwg` | `b4ca2248db9c3d72487ac8b5b1e5510cdd8aba231ab340541d91bebca2d560e8` |
| Herramienta de F4 `D:\r65-smoke\bd1fcf8a\ramtool\` (comparador y `dump-script`) | `.exe` `21290785e13f2daee51b10b9d0b8911f86e73798e2cf49e668ce791a78e9ce7c`, `.dll` `86298d2cb2321229df08f6cdfd198822f1834d88e869bce7e3c34141787a6c83`; 11 archivos, manifiesto `3e6ec61f389a3f88e231665550d44e6a55509dc08d5d195398eb0f8f58c8780d` |
| Hoja manual de OV-I65-02 | `docs/automation/evidence/I-65-f5-prep/ov-i65-02-manual-entry-sheet.json` en `bd1fcf8a` (generador `f5_manual_entry_sheet.py` `66d099c356f981fd1dc104768733959a5a715be60c1667e5391f84f255e65c56`) |

### 1.2 Herramientas del Coordinator

| Uso | Herramienta | SHA-256 del archivo ejecutado |
|---|---|---|
| W3: BASELINE/POST del host de AutoCAD | `w1_hostsnap.ps1` | `5c33a842fa1a39d4dd9ce9ec1dd4b9143b3271fceeca0d59d570f3b551b35ba4` |
| W3/W4: inactividad y proceso en primer plano | `w1_idle.ps1` | `ac61c01bae9975e335b5632bb0ed6066f341f1005cb44af3fd30c06e690fa760` |
| W3: validador del paquete (16 comprobaciones) | `validate_smoke2_package_v2.py` | `1f56b8cba1a64f908c46eb5691930891c3dc0ef1d9bf8ed9c45f8e5f8bdd297a` |
| W3: contraste de la carga útil con la tabla (6/6) | `f5_dryrun_compare.py` sobre la línea `PAYLOAD` del script de `pkg-OV02` | `39ac443a5a20368afbab217309c9e8afa27af77e7e7dcf97cdfd0aa950f85265` |
| W4: preparación y PRE/POST del host de RAM | `w2_readiness.ps1`, `w2_hostsnap.ps1` | `466f7a933042b34e423eb021cf99dd942c57a45d62391db306935d0297bab827`, `6904a43965de69b33b78c304cd57d1958b31b2018121906cf22d034724ff5485` |
| W4: custodia de sesión (opción 0) | `session_custody.ps1` | `5487544aec2685c8554145b46d68bd4fe8fe9545e0b1628deee84d7fc080491b` |
| W4: contraste lectura de vuelta ↔ volcado | `readback_dump_contrast.py` | `0790930297d221ddefeadaef8e04611858b2e81446717155e754d34155114bb1` |

### 1.3 Identidades de las aplicaciones (se miden en la ventana)

| Aplicación | Referencia | Regla |
|---|---|---|
| AutoCAD 2025 | `acad.exe` y versión medidos en la BASELINE de W3 y registrados en el bloque de la guía §7 | la misma identidad en BASELINE y POST |
| ACL de `AcadLocation` | medida por el **mismo camino y la misma página de códigos** en BASELINE y POST (A65-F4F-08) | una diferencia explicada solo por la página de códigos no es un cambio |
| RAM Elements | `RAMElements.exe` 26.00.01.15 `7dcb48555741fa368f5d5f6a14e760fe0662dd5c0b3cc53f6e88f1c0231e7970` | distinto = STOP |
| API `ramelements` | 0.1.0, árbol de 30 archivos `f71a017f49573030219a4213465c9ec454d3b6282f3f60b648a4300bfeba0a60` | distinto = STOP |
| Bases de RAM | `W.sec` `dffd9e00…b977`, `HSS_SQR.sec` `992ddac9…3df0`, `Steel.mat` `2a8b4239…9ce1`; árboles `Sections\United States` `ccdc2346…dc05` y `Materials\United States` `4cb742a5…a87e` | medidas antes y después de W4; distinto = STOP |

## 2. Signo de las cargas en el modelo manual (A65-F5C-04)

Una **única inversión de signo**, conforme al contrato (D-04, D-10b): la dirección −Z del archivo de cargas pasa a la dirección
`GLOBAL_Y` de RAM con **valor negativo**. En W4-05 cada carga se teclea en dirección `GLOBAL_Y` con el valor negativo de la hoja (por
ejemplo, −0.449617886 kip). **No** se aplica además una segunda inversión de dirección. Si la interfaz de RAM exigiera elegir un sentido
aparte del signo del valor: STOP y consulta, sin improvisar.

## 3. Ventana de W4 (A65-F5C-05)

- Reserva de **hasta 2.5 horas** (solicitud a I-64 con esa duración), contadas desde la apertura de RAM (W4-01) hasta su cierre (W4-07).
- **STOP al límite:** si se alcanzan las 2.5 h sin cerrar RAM, se descarta el modelo sin guardar, se cierra RAM, se registra
  `session-end` y la ventana queda INCOMPLETE; no se prolonga ni se reabre sin una autorización nueva.
- Los pasos fuera de RAM (W4-08..W4-11: comparador, contraste, cadena de custodia, POST) se hacen después del cierre.

## 4. Capturas (A65-F5C-06)

- Toda captura se **redacta antes de cualquier publicación** (commit, push o texto para terceros), manteniendo **legible la información
  técnica**: consola de Python, rejillas, barra de estado, mensajes de AutoCAD y RAM, rutas de `D:\r65-smoke\…` y listas de archivos del
  paquete.
- Se redacta: barras de título con cuenta o nombre de dibujo ajeno a la prueba, paneles de navegación con carpetas personales o
  comerciales, nombres de personas y organizaciones, rutas con el nombre de usuario, barra de tareas y ventanas ajenas a la tarea.
- La redacción es por rectángulos opacos sobre una copia; el original queda solo en local y no se versiona; se custodian los SHA-256 de
  ambos. Antes de versionar, comprobación mecánica de los textos y revisión visual de cada captura redactada.
- Mientras el PRIVACY HOLD siga activo, no se versiona ninguna captura con datos identificables aunque esté redactada parcialmente.

## 5. Lo que no cambia

Condiciones de entrada (W3-PR-01..08, W4-PR-01..06), pasos y oráculos (W3-01..08, W4-01..11), Q2 (§4.3), presupuestos y STOP de los
procedimientos; tolerancias congeladas; matriz AC-F5-01..10 con la lectura de CD-I65-F5-24 para A65-F5C-08.

## 6. Adenda: resolución del PRIVACY HOLD y autorización de W3 (decisiones §90, 2026-10-19)

Por la disposición del Owner «Privacy Hold Resolution / F5 W3 AutoCAD Authorization»
([texto literal](../automation/decisions/I-65-owner-disposition-privacy-resolution-f5-w3-authorization.original.txt)):

- **W3-PR-01 y W4-PR-01 (repositorio privado comprobado por lectura) quedan sustituidas** por la decisión del Owner
  «REPOSITORY VISIBILITY = PUBLIC / OWNER ACCEPTED». Antes de cada ventana se sigue registrando la visibilidad por lectura, como dato y
  no como condición. También queda sustituida la regla del §2 de los procedimientos «No se abre ninguna ventana mientras el
  repositorio sea público», y la última viñeta del §4 de este anexo.
- **Política de evidencias nuevas (sustituye al PRIVACY HOLD como condición bloqueante):** nunca credenciales, tokens, secretos ni
  información confidencial de terceros; se recorta o redacta la información identificable que no sea necesaria para demostrar
  resultados técnicos; se conservan diagnósticos, valores y lo necesario para auditar las pruebas; si no puede prepararse una evidencia
  segura, se custodia en local y se informa la limitación. Las reglas de recorte y redacción del §4 siguen vigentes. AC-F5-07 se
  sigue cumpliendo en su forma literal: la evidencia versionada no lleva datos identificables innecesarios, y los originales con datos
  de ese tipo quedan en local con su SHA-256 custodiado.
- **Medición del host en W3:** la BASELINE y el POST usan `w1_hostsnap.ps1` **sin modificar** (`5c33a842…`, versión custodiada). Sus
  líneas de DLL, manifiesto, tabla y cargas apuntan a la carpeta del Candidato de F4 (`D:\r65-smoke\5a5ca577\`) y son informativas.
  La identidad del Candidato de F5 se mide aparte, con órdenes de solo lectura (SHA-256 y ProductVersion de la DLL, salida, tabla,
  cargas, valores esperados y biblioteca; huella de la ACL de `D:\r65-smoke\bd1fcf8a\bin-Debug-net8.0-windows` por el mismo camino y
  la misma página de códigos; cobertura por `TRUSTEDPATHS`; existencia de `pkg-OV02`), antes de NETLOAD y en el POST.
- W3-PR-07 (comprobación en seco con las DLL del Candidato, 6/6) ya se cumplió (evidencia §92).
- No cambian: oráculos, tolerancias, matriz AC-F5-01..10, presupuestos ni STOP. W4 = NO.
