# I-52 — ObjectARX 2025 authority V28

> **OA-V28-1 / AUTHORITY DISCOVERY ONLY.** No contiene ejecucion CT-DA, helper compilado ni codigo de producto.

## 1. Resultado de disponibilidad

```text
ObjectARX 2025 SDK installed locally = NO
Exact ObjectARX 2025 headers locally available = NO
Official ObjectARX 2025 SDK download path = AVAILABLE / EULA+CAPTCHA GATED
Official exact-version reference documentation = AVAILABLE
Native definition authority used by V28 = OFFICIAL_EXACT_VERSION_DOC
EXACT_HEADER evidence = NOT AVAILABLE
Header hashes = NOT OBSERVED / NOT APPLICABLE TO THE CLAIMED EVIDENCE CLASS
```

Se buscaron `dbmain.h`, `dbtrans.h`, `aced.h`, `acdocman.h`, `rxdlinkr.h`, `acedads.h` y `rxregsvc.h` en las
instalaciones Autodesk locales, rutas comunes del SDK, registro y repositorio. AutoCAD 2025 esta instalado, pero el SDK
ObjectARX no. El portal APS ofrece ObjectARX 2025 tras aceptar su licencia y resolver CAPTCHA; V28 no acepta contratos
en nombre del usuario ni descarga el paquete.

Esto no obliga a usar evidencia cross-version. El Reference Guide y Developer Guide bajo el namespace oficial
`cloudhelp/2025/ENU` exponen clases, headers, firmas y fases necesarias. V28 usa esa evidencia
`OFFICIAL_EXACT_VERSION_DOC`, una de las dos clases admitidas para cerrar definicion. El helper futuro debera obtener el
SDK 2025 oficial y capturar hashes de sus headers/libs antes de compilar; una divergencia reabre las filas afectadas.

## 2. Identidad observada

| Autoridad | Identidad |
|---|---|
| Producto host instalado | AutoCAD 2025, API managed `25.0.0.0`, file/product `25.0.171.0.0` |
| Portal SDK | APS `ObjectARX for AutoCAD SDK`, seccion ObjectARX 2025 |
| Toolset publicado | ObjectARX 2025: Visual Studio 2022 `17.14.0`, C++, .NET 8.0 |
| Native reference | Autodesk CloudHelp `2025/ENU/OARX-RefGuide` |
| Native developer guide | Autodesk CloudHelp `2025/ENU/OARX-DevGuide` |
| Fecha de observacion | 2026-09-22 |

El portal APS es una pagina viva y no sustituye identidad de un SDK descargado. Su fingerprint observado es
`fc24ba8ab26bd8f166e7d5d699529f8ceeb6539d88655f53e895cb5caefb67cd`; sólo prueba los bytes HTTP observados, no
un package/header hash.

## 3. Headers y autoridades exactas

| Header declarado por Autodesk 2025 | Clases/funciones usadas | Evidencia V28 | Header SHA-256 |
|---|---|---|---|
| `dbmain.h` | `AcDbDatabaseReactor` | OFFICIAL_EXACT_VERSION_DOC | NOT OBSERVED |
| `dbtrans.h` | `AcTransactionReactor` | OFFICIAL_EXACT_VERSION_DOC | NOT OBSERVED |
| `aced.h` | `AcEditorReactor` | OFFICIAL_EXACT_VERSION_DOC | NOT OBSERVED |
| `acdocman.h` | `AcApDocManager` | OFFICIAL_EXACT_VERSION_DOC | NOT OBSERVED |
| `rxdlinkr.h` | `AcRxDLinkerReactor` | OFFICIAL_EXACT_VERSION_DOC | NOT OBSERVED |
| `acedads.h` | `acedArxLoad` | OFFICIAL_EXACT_VERSION_DOC | NOT OBSERVED |
| `rxregsvc.h` | `acrxLoadApp` | OFFICIAL_EXACT_VERSION_DOC | NOT OBSERVED |

No se inventa un hash para bytes ausentes. Antes del helper futuro, el SDK descargado debe coincidir con las firmas
documentadas y quedar fijado por version, path, header/lib hashes, MSVC toolset y plataforma x64.

## 4. Authority snapshot

Los hashes siguientes son fingerprints de las respuestas HTML oficiales observadas, no sustitutos de headers:

| Documento 2025 | SHA-256 observado |
|---|---|
| `AcDbDatabaseReactor` class | `1de7155203ea3bac1f7ac698ac5a1c0241a7540403929e54720d7ea4bd37f4ca` |
| `AcDbDatabaseReactor` methods | `8ff2e5dc285708f1f445e1250ff42c735fe954943683fe6de2d001d5670ed0a0` |
| `AcTransactionReactor` class | `37fb8fe751e81695706b13a85e0a7fa17d94992764d3cab21377710325498a9a` |
| `AcTransactionReactor` methods | `7bc62d6f668160aa341330aef11fe5b93535a3197f86c2a7434145c31df5b0d0` |
| `AcEditorReactor` class | `e47b25fd8acee1b014e464118c4a0bf9770ccc505ba272a2da22ac29f46280bd` |
| `AcEditorReactor` methods | `80ac83e78bd736133a87bdf0c3aec022eec2b265246936f6e75d2615a374a6c8` |
| `AcApDocManager` class | `501ec15b143585f144d17d10fb566a8d79ea456c47ed9c9113e6b88748975e64` |
| `AcApDocManager` methods | `fafaa63927188a8e70bd348bd217c59dda20928da2430c590e0abfea904e753a` |
| Application execution context | `f7fe9ea289ed565c3598d0236be1ae9681ac0696caa575dddb3216fe5d789d10` |
| `AcRxDLinkerReactor` class | `56faf7df851e7f5ab1041f35c4ead715b5ea6ddb953b9300da45b5a220e66371` |
| `AcRxDynamicLinker::loadApp` | `7e90c5ac99d693334fdc83fe2f6f4c83bf01a1ac12e9e15208c6825472ba1972` |
| `AcRxDLinkerReactor::rxAppLoaded` | `3f40e48610fd65e54c1d6f9e7ad643b33726ed7ee6c702616626cebec9133043` |

Normative source URLs:

- [ObjectARX 2025 SDK overview](https://aps.autodesk.com/developer/overview/objectarx-autocad-sdk)
- [AcDbDatabaseReactor class](https://help.autodesk.com/cloudhelp/2025/ENU/OARX-RefGuide/files/OARX-RefGuide-AcDbDatabaseReactor.html)
- [AcDbDatabaseReactor methods](https://help.autodesk.com/cloudhelp/2025/ENU/OARX-RefGuide/files/OARX-RefGuide-__MEMBERTYPE_Methods_AcDbDatabaseReactor.html)
- [AcTransactionReactor class](https://help.autodesk.com/cloudhelp/2025/ENU/OARX-RefGuide/files/OARX-RefGuide-AcTransactionReactor.html)
- [AcTransactionReactor methods](https://help.autodesk.com/cloudhelp/2025/ENU/OARX-RefGuide/files/OARX-RefGuide-__MEMBERTYPE_Methods_AcTransactionReactor.html)
- [AcEditorReactor class](https://help.autodesk.com/cloudhelp/2025/ENU/OARX-RefGuide/files/OARX-RefGuide-AcEditorReactor.html)
- [AcEditorReactor methods](https://help.autodesk.com/cloudhelp/2025/ENU/OARX-RefGuide/files/OARX-RefGuide-__MEMBERTYPE_Methods_AcEditorReactor.html)
- [AcApDocManager class](https://help.autodesk.com/cloudhelp/2025/ENU/OARX-RefGuide/files/OARX-RefGuide-AcApDocManager.html)
- [AcApDocManager methods](https://help.autodesk.com/cloudhelp/2025/ENU/OARX-RefGuide/files/OARX-RefGuide-__MEMBERTYPE_Methods_AcApDocManager.html)
- [Application execution context](https://help.autodesk.com/cloudhelp/2025/ENU/OARX-DevGuide/files/GUID-634ADE0B-35FD-4146-A2D0-3621D2FB5B0C.htm)
- [acedArxLoad](https://help.autodesk.com/cloudhelp/2025/ENU/OARX-RefGuide/files/OARX-RefGuide-acedArxLoad_ACHAR__.html)
- [rxAppLoaded](https://help.autodesk.com/cloudhelp/2025/ENU/OARX-RefGuide/files/OARX-RefGuide-AcRxDLinkerReactor__rxAppLoaded_ACHAR__.html)

## 5. Exact callback inventory

### 5.1 Database reactor

`AcDbDatabaseReactor`, header `dbmain.h`:

```cpp
virtual void objectOpenedForModify(const AcDbDatabase*, const AcDbObject*);
virtual void objectModified(const AcDbDatabase*, const AcDbObject*);
virtual void objectErased(const AcDbDatabase*, const AcDbObject*, bool);
```

Autodesk 2025 define `objectOpenedForModify` antes de la operacion Modify, `objectModified` despues de completarla y
`objectErased` despues de erase/unerase. El notifier es `const`; V28 nunca intenta mutarlo desde ese puntero. Toda
mutacion adversarial abre un target fixture distinto mediante su `ObjectId` y una T observable. Legalidad/resultados
del intento son resultado futuro, no una afirmacion anticipada.

### 5.2 Transaction reactor

`AcTransactionReactor`, header `dbtrans.h`:

```cpp
virtual void transactionAboutToAbort(int&, AcDbTransactionManager*);
virtual void transactionAborted(int&, AcDbTransactionManager*);
virtual void transactionAboutToEnd(int&, AcDbTransactionManager*);
virtual void transactionEnded(int&, AcDbTransactionManager*);
```

El conteo de `transactionAboutToEnd` incluye la T que termina; `numTransactions == 1` identifica el caso outermost
sin inventar `endCalledOnOutermostTransaction`, que no aparece en la autoridad 2025 inspeccionada. `transactionEnded`
excluye la T que esta terminando del conteo. No se llama a ninguno "commit begin".

### 5.3 Editor reactor

`AcEditorReactor`, header `aced.h`:

```cpp
virtual void commandWillStart(const ACHAR*);
virtual void commandEnded(const ACHAR*);
virtual void commandCancelled(const ACHAR*);
virtual void commandFailed(const ACHAR*);
```

`commandEnded` indica que el command se completo, no `C_host`. La autoridad 2025 documenta que el ultimo entity puede
seguir abierto para read y recomienda usar una transaction dentro de `commandEnded`, terminandola o abortandola antes
de retornar. Esa regla da una autoridad exacta para intentar 09N-D y 10N-S/M/SM; no predice que esos probes pasen.

### 5.4 Dynamic linker

`AcRxDLinkerReactor::rxAppLoaded(const ACHAR*)`, header `rxdlinkr.h`, ocurre despues de cargar/inicializar el ARX y
registrar sus clases. `acedArxLoad(const ACHAR*)`, header `acedads.h`, carga un modulo por path. V28 usa
`acedArxLoad` como `NL-ARX-PATH` y `rxAppLoaded` como marker; el load no es por si mismo la mutacion.

## 6. Exact native scheduler authority

### 6.1 NS-SEND

`AcApDocManager`, header `acdocman.h`:

```cpp
virtual Acad::ErrorStatus sendStringToExecute(
    AcApDocument* target,
    const ACHAR* input,
    bool activate = true,
    bool wrapUpInactiveDoc = false,
    bool echo = true) = 0;
```

El Developer Guide 2025 establece que desde document context `activate=false` encola el string, mientras
`activate=true` suspende inmediatamente ese contexto. V28 fija `activate=false`, `wrapUpInactiveDoc=false`,
`echo=false`, target=current document. El string invoca uno de tres commands research-only registrados por el helper.

### 6.2 NS-BEGIN-CMDCTX

```cpp
Acad::ErrorStatus beginExecuteInCommandContext(void (*procAddr)(void*), void* pData);
```

La referencia 2025 dice que el callback se encola, corre cuando el sistema pueda ejecutar un command, requiere que el
caller retorne y sólo acepta application context. Se cataloga para T13/T16 de application context, pero 16N usa
NS-SEND porque su SchedulePoint nace dentro de un reactor en document context.

### 6.3 APIs que no reciben credito scheduler

`executeInApplicationContext` es llamada sincronica a application context y no demuestra deferred work. La pagina de
`beginExecuteInApplicationContext` contiene wording internamente ambiguo y no se usa para closure. Windows messages,
sleep, managed Idle y managed `SendStringToExecute` no acreditan 16N.

## 7. Evidence class rule

```text
EXACT_HEADER                  may close definition authority
OFFICIAL_EXACT_VERSION_DOC    may close definition authority
RUNTIME_PROBE_FUTURE          validates behavior, never definition alone
CROSS_VERSION_DOC             supporting context only
INFERENCE                     no closure credit
```

Todos los EventIds y SchedulerIds normativos V28 tienen `OFFICIAL_EXACT_VERSION_DOC`. Ninguna fila normativa depende
de 2018/2027 docs, managed analogy o inference.

## 8. Future helper boundary

El helper futuro vive fuera de `RackCad.Plugin`, se compila x64 con el SDK oficial 2025 y registra sólo los reactors,
commands y log bridge del catalogo V28. Para T15 consta de un bootstrap ya cargado y un payload ARX separado: el
bootstrap llama `acedArxLoad(payloadPath)` y el payload registra N-DB-MOD durante su inicializacion; esto evita asumir
que un modulo puede cargarse a si mismo. Canal preferido: Xrecord fixture para observations DB y file log append-only
para orden/eventos; el verifier managed relee DB/log sin consultar el writer. Los SHA de ambos modulos,
SDK/header/lib hashes y toolset entran en el exact tuple. Nada de esto se implementa en V28.
