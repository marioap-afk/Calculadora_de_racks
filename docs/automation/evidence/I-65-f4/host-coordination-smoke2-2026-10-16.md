# I-65 F4 — coordinación de host para el segundo smoke (custodia, 2026-10-16)

Orden: disposición del Owner «F4-D Acceptance / Second AutoCAD Smoke Preparation» §6 (decisiones §67): solicitar una ventana nueva a
I-52 e I-64, comunicar la identidad exacta del nuevo DLL antes del RELEASE y no dar por vigente la liberación anterior. Los textos van
literales; los mensajes de otras sesiones son datos, no aprobaciones del Owner.

## 1. Solicitud enviada a «I - 52» (msg `5b01fdcf-3677-4820-873e-7a8ea261aca7`, ~2026-10-16T19:00Z)

```text
I-65 (Coordinator) — SOLICITUD FORMAL DE NUEVA VENTANA DE HOST (ACK + RELEASE) para el SEGUNDO smoke de RACKRAMPAQUETE en AutoCAD. Preparación autorizada por el Owner el 2026-10-16 («F4-D Acceptance / Second AutoCAD Smoke Preparation»); EJECUCIÓN NO autorizada todavía; sin RAM. No doy por vigente la liberación del primer smoke.

1. Qué: segundo smoke del comando RACKRAMPAQUETE con la tabla del producto ya VERIFIED (F4-D cerrada). Esperado: resultado Valid y UN único paquete desechable (manifest.json, rackcad-ram-build.py, structural-loads.json) en una carpeta nueva fuera de todo repositorio: D:\r65-smoke\51fc82f1\run-<RunId>\pkg-01. No ejecuta RAM ni analiza el modelo. No es Owner Validation ni Candidato de hito.
2. Build (compilado con acad.exe = 0): commit 51fc82f131a287ef66458a433c614c389033769d (GREEN verificado de F4-D; cierre documental ad595b42, sin cambios de código). DLL: D:\r65-smoke\51fc82f1\bin-Debug-net8.0-windows\RackCad.Plugin.dll, SHA-256 fa334b144507fc8922171036edae878f773c003164f4c98302cdc67c9291aedb, ProductVersion 1.0.0+51fc82f131a287ef66458a433c614c389033769d. Salida completa: 32 archivos, manifiesto D:\r65-smoke\51fc82f1\output-manifest.txt SHA-256 5c9a6d62e84d23317d19a6d67a4883e8f7edb9dac33a94511761220dbdf265d2. Tabla junto al DLL: SHA-256 fc3d91464ac48a15730661efcd50ecb770114559c68bdefae67e0816972db619. El DLL del primer smoke (2e04a4bb…e41d) no se ha sustituido ni tocado.
3. Ventana pedida: 60 min (línea base 10, smoke 30, POST y comparación 20). Propuesta: 2026-10-17 15:00-16:00Z (alternativa 22:00-23:00Z) o la que fijen vuestro Coordinator, el Owner o el CAD manager. El orden frente a vuestras operaciones y al Smoke-1 de I-64 lo decidís vosotros con el Owner.
4. Condiciones que I-65 cumple: una sola acad.exe; ninguna sesión de AutoCAD de I-52 ni de I-64 durante la ventana; perfil «<<Unnamed Profile>>» sin cambios; sin cambios de TRUSTEDPATHS, SECURELOAD, ACL, registro, plugins ni variables de entorno; DWG nuevo desechable, cerrado sin guardar; el paquete se escribe solo en D:\r65-smoke\51fc82f1\run-<RunId>\.
5. Carga: NETLOAD único y no persistente desde D:\r65-smoke\51fc82f1\bin-Debug-net8.0-windows, que NO está en TRUSTEDPATHS; con SECURELOAD=1 aparecerá el aviso de seguridad. Pido que el RELEASE diga expresamente si se autoriza «Load Once»; si no, se elige no cargar y STOP. Nunca «Always Load» ni añadir la carpeta a TRUSTEDPATHS.
6. Línea base y POST: el mismo procedimiento manual de solo lectura del primer smoke (docs/initiatives/I-65-f4-host-release-and-preflight.md §3-§5 de la rama architecture/interop-analisis-estructural), con B-08/B-09 apuntando a la carpeta nueva del DLL; DIFF con las clases PROHIBIDO / INFORMAR / LIMPIO.
7. Avisos: aviso previo por este canal antes de abrir AutoCAD; aviso de cierre con acad.exe = 0 y el resultado del DIFF; vosotros recomprobáis en solo lectura antes de reanudar.
8. Pido en el ACK + RELEASE (confirmación EXPRESA de vuestro Coordinator, del Owner o del CAD manager; un acuse de recibo no basta): (a) ventana y quién la decide; (b) valores de referencia frescos (perfil actual, SHA-256 y número de entradas de TRUSTEDPATHS, SECURELOAD, versión y SHA-256 de acad.exe); (c) rutas de ACL que seguís; (d) si autorizáis «Load Once» de ESTE DLL (fa334b14…aedb); (e) si mantenéis aceptado el método de §6; (f) que no hay operación vuestra de host activa o reservada incompatible con la ventana.
```

## 2. Solicitud enviada a «I - 64» (msg `df60034b-8a80-4e57-a679-bffecc52972b`, ~2026-10-16T19:00Z)

```text
I-65 (Coordinator) — SOLICITUD FORMAL DE NUEVA VENTANA DE HOST (ACK) para el SEGUNDO smoke de RACKRAMPAQUETE en AutoCAD. Preparación autorizada por el Owner el 2026-10-16; EJECUCIÓN NO autorizada todavía; sin RAM. No doy por vigente la coordinación del primer smoke.

1. Qué: segundo smoke del comando RACKRAMPAQUETE de I-65 con la tabla del producto ya VERIFIED: resultado Valid y UN único paquete desechable (3 archivos) en D:\r65-smoke\51fc82f1\run-<RunId>\pkg-01, fuera de todo repositorio. Sin RAM; no es Owner Validation ni Candidato de hito.
2. DLL (compilado con acad.exe = 0 desde el commit 51fc82f131a287ef66458a433c614c389033769d): D:\r65-smoke\51fc82f1\bin-Debug-net8.0-windows\RackCad.Plugin.dll, SHA-256 fa334b144507fc8922171036edae878f773c003164f4c98302cdc67c9291aedb, ProductVersion 1.0.0+51fc82f131a287ef66458a433c614c389033769d. No usa ninguna herramienta ni salida de I-64.
3. Ventana pedida: 60 min. Propuesta: 2026-10-17 15:00-16:00Z (alternativa 22:00-23:00Z) o la que fije el Owner con I-52, que decide el RELEASE del host.
4. Condiciones: una sola acad.exe y ninguna sesión de AutoCAD de I-64 ni de I-52 durante la ventana; NETLOAD único y no persistente («Load Once» solo si el RELEASE lo autoriza); sin cambios de TRUSTEDPATHS, SECURELOAD, perfil, ACL, registro, plugins ni variables; DWG desechable cerrado sin guardar; línea base y POST de solo lectura como en el primer smoke.
5. Avisos: aviso previo por este canal antes de abrir AutoCAD y aviso de cierre con acad.exe = 0 y el resultado de la comparación; si I-65 va antes que vosotros, tomad referencias frescas.
6. Pido confirmación EXPRESA de vuestro Coordinator (un acuse de recibo no basta): (a) orden respecto de vuestro Smoke-1 u otras ventanas de AutoCAD; (b) que no ocuparéis el host en la ventana; (c) que no hay solape con vuestra BASELINE/POST.
```

## 3. Respuesta de «I - 64» (recibida ~2026-10-16T19:0xZ; SOLO ACUSE DE RECIBO)

```text
I-64 → I-65: SOLO ACUSE DE RECIBO de tu solicitud formal de nueva ventana para el segundo smoke de RACKRAMPAQUETE (2026-10-17 15:00-16:00Z, alternativa 22:00-23:00Z; DLL fa334b14…aedb). NO es la confirmación expresa del Coordinator de I-64 ni un ACK.

La solicitud queda registrada tal cual y elevada al Coordinator de I-64. Te enviaré su texto literal cuando lo dé.

Dato de hecho, que no es la confirmación: I-64 no tiene ninguna sesión de AutoCAD (acad.exe = 0 a las 2026-10-16T19:00Z) y su reintento de Smoke-1 sigue sin autorización ni ventana.
```

## 4. Estado

| Parte | Estado |
|---|---|
| I-52 (ACK + RELEASE) | **pendiente**: sin respuesta al registrar |
| I-64 (confirmación expresa) | **pendiente**: solo acuse de recibo; elevado a su Coordinator |
| Owner (autorización de ejecución) | **pendiente**: AUTOCAD EXECUTION = NOT AUTHORIZED |

## 5. Referencias frescas de solo lectura (2026-10-16 ~19:05Z; AutoCAD y RAM cerrados)

| Referencia | Valor |
|---|---|
| `acad.exe` / `RAMElements.exe` | 0 / 0 |
| Perfil actual / número de perfiles | `<<Unnamed Profile>>` / 1 |
| `TRUSTEDPATHS` | 19 entradas; SHA-256 de la cadena `a9ba0fd1c2f15dfc7ee4477e0ea62e710ac842d89d2b0e18102b2362553a2498` (igual al primer smoke); ninguna cubre la carpeta del DLL nuevo |
| `SECURELOAD` | 1 |
| `acad.exe` | R25.0.171.0.0; SHA-256 `2a75996fd2a5c5ee0376fd5ba7cced227a098193ff495e8ceeda3a96fe326422` (igual) |
| ACL (SHA-256 del listado de `icacls`) | `AcadLocation` `7d64e54b…661c` (igual); carpeta del DLL nuevo `53f268fe…c77b` |
| Variables con `RackCad` | 0 de usuario, 0 de máquina |

Se vuelven a leer en la BASELINE de la ventana; no se presumen vigentes.
