# I-64 F4 — Smoke-2: preparación de los dibujos (versión pública)

La hizo la sesión el 2026-11-15, por instrucción del Owner (`F4-DRAFTS/coordinator/owner-f4-smoke2-operator-instruction.md`): elegir y preparar los dibujos de las pruebas. Smoke-2 lo opera el Owner. **No se ejecutó ningún paso de Smoke-2.**

**Privacidad (repo público):** los dibujos son proyectos reales de clientes del Owner. Aquí no van sus nombres, sus rutas ni sus datos. El detalle, con nombres y SHA-256 de cada original, se queda en la máquina del Owner, en la carpeta de las copias (`copias.json`) y en su subcarpeta de candidatos.

## Cómo se eligieron

- **Fuentes:** solo dibujos locales (OneDrive sincronizado y la unidad de datos). La unidad de red de la oficina no estaba accesible.
- **Lectura:** solo de copias, con `accoreconsole.exe` (sin interfaz) y un AutoLISP que lista cada definición de bloque con el sobre `RACKCAD_SELECTIVE` en su diccionario de extensión.
- **Volcado:** de los dibujos con racks Push Back se volcó el JSON completo, para analizarlo fuera de AutoCAD. Ninguna copia se guardó. De cada original se midió el SHA-256 antes y después, y ninguno cambió.
- **Control positivo:** el dibujo sintético de H0 dio sus 2 vistas con `"Kind":"pushback"`.
- **Cobertura:**
  - 7 dibujos con «Push Back» en el nombre: 0 sobres de RackCad (dibujados sin el plugin);
  - 160 dibujos modificados desde 2026-06: 31 con sobres de RackCad y 13 con racks Push Back.
- **Criterios estáticos del código** (la admisión real la ve el Owner en Smoke-2):
  - Kind `pushback`;
  - un solo lado: sin módulos `B:` ni `GAP` (`PushBackCompositeStructure.cs:115-118`, `PushBackPilotMaterializer.cs:147-148`);
  - un solo diseño entre las vistas del rack;
  - Id presente;
  - `AnnotationScale` = 1 y `DrawRackName` = true.
- **Dibujo antiguo:** faltan `FirstLevelDatum`, los campos de I-41 o `Composite`, como en los fixtures `pb-10`, `pb-11` y `pb-15` de `LegacyDeltas` (`WorkspaceDraftPilotMaterializationTests.cs:706-731`).

## Lo elegido

Todos son dibujos reales del Owner; ninguno lo generó la sesión.

| Papel | Qué trae |
|---|---|
| **D1** | Proyecto real con 8 racks Push Back de un solo lado (SoloA), con nombre, `AnnotationScale` 1 y nombre dibujado. A = un rack con planta y lateral de nombres canónicos; B y C, también planta y lateral. Otro rack tiene una planta y dos laterales enlazadas (una con sufijo `_1`), para «Copia k de M» |
| **D2** | Otro proyecto real, con un rack piloto de planta y lateral y GUID distintos de D1 |
| **D3a** | Un rack Push Back **divergente**: dos vistas con diseños distintos |
| **D3b** | Un rack Push Back **A/B** (topología «Encontradas», con módulos de lado B) |
| **D4** | Un rack Push Back **antiguo** (sin `FirstLevelDatum`, sin I-41 y sin `Composite`) |
| **S2-15** | Cinco dibujos reales: admitidos, otros sistemas, una topología «Encontradas» sin lado B, el divergente, el A/B y el antiguo |

**D3 por la prioridad del Owner:**
1. sobre sin Id: **NO DISPONIBLE**;
2. divergente: **D3a**;
3. Cama de dos miembros: **NO DISPONIBLE** (ningún sobre `cama`);
4. A/B: **D3b**, y además la conversión segura del paso 64;
5. referencia externa: **NO DISPONIBLE**.

No se fabricó ninguno.

**Clases de S2-15 NO DISPONIBLE:** sin Id, ilegible o de versión futura (incluida la Cama), referencia externa, miembros de sistemas distintos, origen de otro rack y no se pudo preparar.

## Las copias

36 copias de trabajo en la carpeta de Smoke-2, según el plan de la guía v3 (una por escenario, decisión D2 del Owner):
- 24 de D1;
- 4 de D2;
- D3a y D3b;
- D4;
- 5 de S2-15.

Cada copia sale de su original, nunca de otra copia, y es idéntica byte a byte a él. Los 7 originales siguen intactos.

## El resto de 0.3

- No había `acad.exe` ni `accoreconsole.exe` abiertos al terminar.
- No hay variables `RACKCAD_DEBUG_*` en el usuario, la máquina ni el proceso.
- No hay bundles de RackCad ni entradas de carga automática en `Applications`, `Loaded` ni `AssemblyMap`.
- Se registró el SHA-256 de la biblioteca de bloques configurada.
- El punto de partida del registro de RackCad del día es su tamaño. Solo trae trazas WARN del arnés de H0.
- I-52 recibió el aviso de inicio, su corrección y el aviso de cierre (`i52-ack-smoke2-prep.md`).
