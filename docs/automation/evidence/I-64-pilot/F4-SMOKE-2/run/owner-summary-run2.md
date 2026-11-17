# Smoke-2 (corrida 2, DLL de C2): resumen para el Owner

Terminado el 2026-11-17 a las 05:20Z. Desde el paso 30 operó la sesión, con control de pantalla y por tu orden. Falta **tu firma** y tres decisiones (apartado 2). El registro completo, paso a paso, está en `run-log-2.md`.

## 1. Resultado por bloques

| Bloque / paso | Resultado |
|---|---|
| 1 a 4 (S2-03 con el DLL de C2: U/REDO refrescan solos) | PASS. El 3 y el 11 son OBSERVADO solo por el DBMOD de vista |
| 5 a 11 (pasos 30 a 64) | PASS, salvo lo de abajo |
| 42, 47 | OBSERVADO: no provocado |
| **50 (SN-10)** | **FAIL**: con una selección activa en el dibujo, «Ver» no hace nada |
| 52, 53 a 56 | OBSERVADO: el 52 no tiene oráculo; la X de la paleta (56) no respondió a la herramienta |
| 64 (S2-R-06) | OBSERVADO: el editor clásico no guardó la casilla A/B |
| 65 | PASS |
| 66 (S2-12) | **NO DISPONIBLE** en los cinco casos (ver 2.c) |
| 67 (`LegacyDeltas`) | **NO DISPONIBLE** (ver 2.c) |
| 68 (S2-15) | PASS en lo observable: 19 Push Back admitidos; «fuera del piloto (F5)» en cabecera, selectivos y dinámico; las clases especiales NO DISPONIBLE |
| **69 a 74 (S2-06)** | **FAIL por la letra de la guía**, con el veto funcionando (ver 2.a) |
| 75, 76 (en vuelo) | PASS, rama (a) |
| 77 (QUIT en vuelo) | OBSERVADO: no provocado (la X no actuó durante «Aplicando») |
| 79 (S2-09, UNIT=2) | PASS: «No se aplicó…unit 2», ninguna vista cambió, Undo con «Executefunction» |
| 82 (SEAL=before) | PASS: Desconocido y A2-11 ×2, A2-07. Nota: la marca «(posiblemente obsoleto)» |
| 84 (SEAL=after) | PASS: Desconocido y A2-11 ×2, vistas escritas, A2-09 con SEAL_FAILED, A2-20 y A2-30. El redibujo no se pudo juzgar a ojo |
| 87 (S2-11) | PASS: «Actualizado con avisos: El nombre del bloque de la vista 2F0D63 no es «PB G - planta».», Limpio |
| 88 (cierre) | Originales 7/7 y copias 37/37 intactos; ni acad ni variables `RACKCAD_DEBUG_*`; ni ERROR ni eventos de error; I-52 avisado |

## 2. Lo que necesita tu decisión

**a. S2-06 (pasos 70 a 72): la pregunta de guardar sale antes del veto.**
- En la copia de D1, AutoCAD preguntó «Save changes?» antes de que actuara el veto, y por la guía eso es FAIL.
- Respondiendo «No», el veto de RackCad actuó siempre: CLOSE (×2), la X de la pestaña, CLOSEALL y QUIT, con su mensaje y el aviso del panel.
- **Causa:** D1 queda con DBMOD = 1. Lo medí: 0 tras abrir, y 1 tras un ZOOM que regenera, sin el panel. D1 trae objetos proxy (CAPsCAPDesignerDBX), una causa posible.
- **La pregunta es si lo cuentas como fallo del producto o como condición del ensayo.** Si alguien responde «Sí», AutoCAD guardaría antes del veto.

**b. Paso 50:** «Ver» se ignora cuando hay una selección en el dibujo.

**c. Inventario de los dibujos de C-F4-06:**
- El divergente `0bb9386e`, el A/B `f6fa1734` y el Push Back antiguo `d4244f83` existen **solo como definiciones de bloque sin insertar**, en D3a, D3b/D2, D4 y D5-03 a D5-05.
- **D2 no tiene ningún rack insertado**; por eso el paso 69 no tuvo borrador en D2.
- Esos casos quedaron NO DISPONIBLE sin fabricar nada. Si quieres cubrirlos, hacen falta dibujos que los tengan insertados.

## 3. Otras observaciones
- AutoCAD se cerró solo al cerrar su último documento después del QUIT vetado (paso 75). Fue una salida limpia, sin error.
- Cada lanzamiento de AutoCAD pidió «Load Once» al cargar el DLL; lo elegí yo, nunca «Always Load».

## 4. Pendiente
- Tu firma: Smoke-2 PASS / FAIL / OBSERVADO, con la lista de arriba.
- La custodia redactada de la corrida, después de tu firma: hay nombres de cliente en los títulos de los dibujos.
- La limpieza de las carpetas de diagnóstico, solo después de avisarte.
