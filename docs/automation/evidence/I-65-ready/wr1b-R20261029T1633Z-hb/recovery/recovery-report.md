# I-65 W-R1b: recuperación controlada del registro (opción A)

- **Candidato:** `4a03b65b`.
- **Corrida:** `<corrida>\hb\recovery\`, 2026-10-30, de 21:28Z a 21:54Z.
- **Disposición:** «W-R1b Registry Recovery / Controlled Cleanup» ([original](../../../../decisions/I-65-owner-disposition-wr1b-registry-recovery.original.txt)).
- No se abrieron AutoCAD ni RAM, no se cambió código y no hubo operaciones de pantalla.

**Resumen.** Borré exactamente las dos claves autorizadas a las 21:44:52Z, después de pasar una puerta de comprobación. Con eso, la clase protegida queda igual que en la BASELINE de HB-01:

- `hkcu-applications` = `13430739…`;
- `hkcu-loaded` = `dd27f202…`;
- 0 referencias a `RackCad.bundle` en las exportaciones B-04.

No toqué `AssemblyMap`. **Recomiendo W-R1b GLOBAL = PASS** (punto 7).

## 1. Respaldo e inventario de las claves

| Clave (HKCU\Software\Autodesk\AutoCAD\R25.0\ACAD-8101:409\…) | Respaldo (`reg.exe export`, local) | SHA-256 | Bytes |
|---|---|---|---|
| `Applications\RackCad` (y su subclave `Groups`) | `recovery\precheck\backup-applications-rackcad.reg` | `627cfbef5f2c0f3ad1c7e5d5d09d6b83f929f605e9c1a7455b379e4f538881ba` | 2314 |
| `Loaded\C:/Users/<usuario>/AppData/Roaming/Autodesk/ApplicationPlugins/RackCad.bundle` (y sus subclaves `#AssemblyMappings` y `./Contents/RackCad.Plugin.dll`) | `recovery\precheck\backup-loaded-rackcad-bundle.reg` | `8b98159f2bb1d880f07707ebefeb6c512090a7a1de2bc73b2a0cbfb83b0ad5ff` | 1564 |

**Inventario exacto** (`inventory.redacted.txt`; el original local tiene SHA-256 `313d3ab0…`).

Todas las claves tienen como última escritura `2026-10-30T01:56:55.9352452Z`.

| Clave | Valor | Tipo | Dato |
|---|---|---|---|
| `Applications\RackCad` | `DESCRIPTION` | REG_SZ | «Comandos: RACKCAD, RACKSELECTIVO, … RACKAYUDA.» |
| | `LOADCTRLS` | REG_DWORD | 2 |
| | `LOADER` | REG_EXPAND_SZ | `%UserProfile%\AppData\Roaming\Autodesk\ApplicationPlugins\RackCad.bundle.\Contents\RackCad.Plugin.dll` |
| | `MANAGED` | REG_DWORD | 1 |
| `Applications\RackCad\Groups` | `@` | REG_SZ | `""` |
| `Loaded\…RackCad.bundle` | `ProductCode` | REG_SZ | `C:/Users/<usuario>/AppData/Roaming/Autodesk/ApplicationPlugins/RackCad.bundle` |
| | `Publisher` | REG_SZ | `RackCad` |
| | `Version` | REG_SZ | `1.0.0` |
| `…RackCad.bundle\#AssemblyMappings` | `RackCad.Plugin` | REG_SZ | `""` |
| `…RackCad.bundle\./Contents/RackCad.Plugin.dll` | `Name` | REG_SZ | `RackCad` |
| | `Type` | REG_DWORD | 1 |

- **Transcripción del respaldo:** `backup-*.redacted.txt`, con la cuenta sustituida por `<usuario>` y `LOADER` decodificado.
- **Copias versionadas:** no se versionan los `.reg`, porque llevan la cuenta de Windows en las rutas. Quedan en local con su SHA-256.

## 2. Evidencia de que se crearon durante HB

1. **No existían antes de HB.** Faltan en la BASELINE de HB-01 (21:09:50Z) y en la recomprobación previa a instalar (01:54:02Z). En ambas, `hkcu-applications` = `13430739…` y `hkcu-loaded` = `dd27f202…`, sin «RackCad» ni referencias a `RackCad.bundle`.
2. **Se escribieron dentro de la ventana de HB.** El bundle estuvo instalado de 01:55:56Z a 02:05:28Z, y AutoCAD (pid 29912) corrió de 01:56:41Z a 02:04:42Z. Las cinco claves tienen la misma última escritura, 01:56:55.935Z, ya dentro de esa ventana. Ninguna se volvió a escribir después.
3. **Pertenecen a la instalación temporal:**
   - `LOADER` y `ProductCode` apuntan al bundle temporal del usuario, que ya no existe.
   - Cada padre tiene una única subclave «RackCad», con el nombre exacto.
   - Ninguna de las cuatro raíces de `ApplicationPlugins` contiene RackCad.
   - `HKLM\SOFTWARE\Autodesk` no tiene referencias.
   - Los otros productos de AutoCAD de HKCU (tres de R24.3) tampoco las tienen.
4. **Coinciden con lo medido.**
   - El estado previo (`recovery\baseline`, 21:34:21Z) es igual byte a byte al POST de HB en las seis exportaciones B-04.
   - Los bloques de las dos claves son idénticos a los del POST de HB (`26102842…` y `b91f054a…`).
   - Justo antes de borrar, la reexportación era idéntica al respaldo.

## 3. Operaciones de eliminación

**Comprobaciones previas** (`rr_precheck.ps1`, solo lectura, 21:34:38Z):

| Punto del Owner | Resultado |
|---|---|
| 1. Respaldo de las dos claves y sus valores | Hecho (tabla del punto 1) |
| 2. Rutas exactas, tipos, datos y hashes | Inventario de 16 líneas; SHA-256 de los respaldos |
| 3. BASELINE y POST contrastados de nuevo | Estado previo = POST de HB (0 diferencias). Frente a HB-01 difieren solo las tres exportaciones ya conocidas (`hkcu-profiles`, `hkcu-applications`, `hkcu-loaded`). |
| 4. Las claves pertenecen a la instalación temporal | Sí (punto 2) |
| 5. Ninguna otra aplicación o proceso usa esas entradas | 0 procesos `acad`, `accoreconsole` o `RAMElements`. Ningún proceso lanzado desde la carpeta de AutoCAD o de RAM. 0 procesos con un módulo RackCad cargado. Las claves no se escribieron después de 01:56:55Z. |
| 6. AutoCAD y RAM cerrados | Sí |

En segundo plano solo había servicios de inicio automático: licencias y acceso de Autodesk, Autodesk CER y Bentley Property Catalog, además de los clientes Bentley CONNECT. No se pueden enumerar los handles de registro abiertos sin herramientas fuera de las autorizadas.

**STOP automático y clasificación.** El script terminó con `RESULTADO=STOP` por una regla mía más estricta que la disposición: cualquier referencia a `RackCad.bundle` fuera de las dos claves y de `AssemblyMap`. La búsqueda en todo `HKCU\Software` encontró siete. Las inventarié solo leyendo (`rr_finding.ps1`, 21:42:06Z):

- **Seis claves en `HKCU\Software\appdatalow\software\Autodesk\AutoCAD\R25.0\CoreUser\`** (`15FCE767F24F74D1` y `BlockPreviewUser`; en cada una, `Applications\RackCad`, `AssemblyMap` y `Loaded\…RackCad.bundle`):
  - Última escritura: 2026-07-23T19:42:04Z y 2026-07-28T22:09:24Z, tres meses antes de HB.
  - Datos: los mismos que el registro de HB, con `LOADCTRLS` = 2, `LOADER` al bundle del usuario y `AssemblyMap` `RackCad.Plugin` a la misma ruta.
  - Interpretación: son restos de una **instalación anterior del bundle en la misma ruta**, ajenos a HB.
- **`HKCU\…\Explorer\TypedPaths`** (historial de la barra de direcciones del Explorador): última escritura 2026-10-14T18:45:42Z. Su valor `url6` nombra la carpeta `publish\RackCad.bundle` de un worktree. Es anterior a HB.

Con eso concluí que **no se cumple ninguna condición de parada del Owner**:
- las claves autorizadas no existían en HB-01 y no pertenecen a otra instalación;
- las siete referencias son anteriores a la BASELINE, así que no son una diferencia nueva;
- el alcance está identificado;
- borrar no exige tocar `AssemblyMap`.

Las siete referencias quedan fuera del alcance y **no se tocaron**. La regla y su resultado quedan custodiados tal cual.

**Puerta inmediata y borrado** (`rr_cleanup.ps1`):

- **Puerta, de 21:44:34Z a 21:44:52Z:**
  - G1: AutoCAD y RAM a 0, y 0 módulos RackCad.
  - G2: la reexportación es igual al respaldo y el nombre es exacto.
  - G3: la última escritura cae dentro de la ventana de HB.
  - G4: las referencias fuera de alcance son anteriores a la BASELINE.
  - G5: estado de `AssemblyMap` (`2267f029…`) y número de subclaves de los padres.
  
  Resultado: **PUERTA = PASS**.
- **Primer borrado:** `reg.exe delete "HKCU\Software\Autodesk\AutoCAD\R25.0\ACAD-8101:409\Applications\RackCad" /f`. Dio `rc=0` y la clave ya no existe. El padre pasa de 8 a 7 subclaves y solo falta esa.
- **Segundo borrado:** `reg.exe delete "HKCU\Software\Autodesk\AutoCAD\R25.0\ACAD-8101:409\Loaded\C:/Users/<usuario>/AppData/Roaming/Autodesk/ApplicationPlugins/RackCad.bundle" /f`. Dio `rc=0` y la clave ya no existe. El padre pasa de 4 a 3 subclaves y solo falta esa.
- **Después:**
  - `AssemblyMap` tiene los mismos valores y la misma hora de última escritura.
  - Las siete referencias fuera de alcance conservan su hora de última escritura.

No se borraron ramas superiores ni otras entradas.

## 4. POST actualizado

Fuentes:
- `wr_hostsnap -Phase post -Run <corrida>\hb\recovery`, a las 21:45:12Z;
- `rr_post.ps1`, a las 21:45:54Z;
- `hb_roots`, a las 21:53:39Z.

| Comprobación | Resultado |
|---|---|
| Las dos claves autorizadas | Ya no existen |
| Referencias a `RackCad.bundle` en B-04 | **0** (en HB-01 también 0) |
| `hkcu-applications`, `hkcu-loaded` | `13430739…` y `dd27f202…`, **iguales a HB-01 byte a byte** |
| `hkcu-fixedprofile`, `hkcu-installedapplications`, `hklm-applications` | Iguales a HB-01 |
| TRUSTEDPATHS / SECURELOAD | 19 entradas `a9ba0fd1…` / 1, iguales a HB-01. No cubren la raíz del usuario ni el bundle. |
| Permisos (B-08, ACL de las seis rutas) | Iguales a HB-01 |
| ApplicationPlugins (cuatro raíces) | Igual a HB-01 (`hb01-roots`, 0 diferencias). Raíz del usuario vacía y ninguna raíz con RackCad. |
| Resto del host (acad.exe, perfil, variables, DLL, manifiesto, tabla, cargas, `%APPDATA%\RackCad`, biblioteca) | Igual a HB-01 |
| Checkouts | K3 = K1 = K2 (digest `d0105e96…`). K2 → K3: OWN (este worktree) y SIGNAL (el worktree de I-62 avanzó, limpio e igual a su remoto). `stop=false`. |

## 5. Estado de AssemblyMap

`HKCU\…\ACAD-8101:409\AssemblyMap` no se tocó.

| Aspecto | Estado |
|---|---|
| Valor `RackCad.Plugin` | Sigue presente: `C:\Users\<usuario>\AppData\Roaming\Autodesk\ApplicationPlugins\RackCad.bundle\Contents\RackCad.Plugin.dll` |
| Destino | **Apunta a la ruta temporal del bundle**, que ya no existe |
| Otros valores | `AppManagerPlugin` y `FeaturedAppsPlugin` (Autodesk), cuyos destinos existen |
| Última escritura de la clave | 01:56:55.935Z, el mismo instante que el registro de HB; el borrado no la cambió |
| Valor anterior | **No se puede establecer de forma fiable.** `AssemblyMap` está fuera de B-04, así que no hay BASELINE. Además, las copias de `CoreUser` de julio tienen el mismo valor `RackCad.Plugin` hacia la misma ruta. Hay, por tanto, un indicio de que pudo existir antes de HB con el mismo dato. |

**Clasificación:** diferencia fuera de B-04, sin BASELINE y separada de la clase protegida. **No se puede demostrar que sea nueva ni exclusiva de HB**, así que no pido su eliminación.

## 6. Diferencias restantes respecto de la BASELINE

| Diferencia | Clase | Estado |
|---|---|---|
| `hkcu-profiles` (`1d7aac3d…` → `30195c03…`): `Select File` `FileNameMRU0..6` y `Variables\VIEWBACKSTATUS` | INFORMAR (historial de diálogos y estado de vista de la sesión de AutoCAD de HB; igual que en HB-05c) | Sin cambios; fuera del alcance autorizado |
| `AssemblyMap\RackCad.Plugin` | Fuera de B-04, sin BASELINE | Permanece (punto 5); no se modifica |
| Hora de última escritura de los padres `Applications` (21:44:52.650Z) y `Loaded` (21:44:52.708Z) | Metadato del borrado | El contenido exportado es igual a HB-01 |
| Copias en `appdatalow\…\CoreUser` y `TypedPaths` | Preexistentes, ajenas a HB | Sin cambios; fuera de I-65 |

**No queda ninguna diferencia protegida sin resolver.** Aun así, el host no es idéntico a la BASELINE en todo; las diferencias son las de esta tabla.

## 7. Recomendación

**W-R1b GLOBAL = PASS.**

- La validación funcional ya fue aceptada (HB-01..04 y HB-05a/b).
- Con la recuperación, la clase protegida vuelve exactamente a HB-01. Eso cubre las referencias, TRUSTEDPATHS, SECURELOAD, los permisos y ApplicationPlugins.
- Lo que queda está fuera de la clase protegida o no viene de HB, y está documentado: `hkcu-profiles` (INFORMAR), `AssemblyMap` (no autorizado) y las copias preexistentes de `CoreUser`.

**Hallazgos para documentación o una iniciativa futura** (sin cambiar el Candidato):

1. **Desinstalar borrando la carpeta no basta.** Al primer arranque, el Autoloader escribe en HKCU `Applications\RackCad`, `Loaded\…RackCad.bundle` y `AssemblyMap`, aunque se pulse «Load Once». Borrar la carpeta no limpia nada de eso.
2. **Hay restos de una instalación anterior.** `HKCU\Software\appdatalow\…\CoreUser\{15FCE767F24F74D1,BlockPreviewUser}`, de julio de 2026, conserva el registro de un bundle anterior en la misma ruta, con `LOADCTRLS` = 2 hacia un DLL inexistente. No los creó I-65. Cualquier acción sobre ellos, o sobre `AssemblyMap`, necesita una decisión aparte del Owner.

**Incidencias:**
- El STOP automático de mi regla de *precheck* (punto 3) se resolvió con un inventario de solo lectura, sin escribir antes en el registro.
- X-03 dio 0.6 s y 0 s: el equipo estaba en uso. Esta fase no tiene operaciones de pantalla.

**Restricciones:**
- W-R2 = NO.
- OWNER VALIDATION FINAL = PENDING.
- INTEGRATION = NO.
- Codex sigue en 55/57 y `attempts` en 7/11, sin consumo.

Me detengo tras este informe.
