# I-67 — Ficha de Owner Validation (H58)

Matriz: [Proposal V3 §9](I-67-proposal-v3.md). Procedimiento general: [guía manual](../guias/validacion-manual-autocad.md).

## Identidad (la toma la sesión; el Owner no la mide)

```text
FINAL_CANDIDATE_SHA: b6b988fb815b233d4e621b7f24d2e94b5800ed96
Worktree:            C:\Users\alejandra-mendoza\.codex\worktrees\fix-elevaciones-niveles
DLL Debug:           src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll
ProductVersion:      1.0.0+b6b988fb815b233d4e621b7f24d2e94b5800ed96
SHA-256 del DLL:     2AEE670FE0D896D5756EADF900FC4015A76569D689BF2850CEAF92966B5A4E52
```

La versión de AutoCAD y la biblioteca de bloques (ruta y SHA-256) las registra la sesión al abrir la ronda.

## Preparación

1. Cierra AutoCAD, ábrelo y carga el DLL con `NETLOAD`.
2. Para OV-I67-06A, 06B y 09 hace falta un DWG **creado antes de I-67**, con un rack Dinámico y uno Push Back de requisito impar (por ejemplo, tarima de 33"). Sirve cualquier DWG tuyo; si no tienes, créalo con la versión instalada antes de cargar este DLL.

**Caso base:** tarima 42 × 48 × 33", larguero C6, 4 niveles, primer nivel 6", 4 fondos de 48". El requisito es 45" (33 + 6 + 6) y la retícula es de 2".

## Escenarios

| ID | Qué hacer | Qué debe verse | Resultado |
|---|---|---|---|
| OV-I67-01 | Dinámico nuevo con el caso base | Separaciones 46 / 46 / 46 en lateral y cotas; cabecera 180" | |
| OV-I67-02 | Dinámico con tarimas de 33", 40" y 35" en los niveles 1, 2 y 3 | Separaciones 46 / 52 / 48, sin imponer un valor uniforme | |
| OV-I67-03 | Revisa las vistas frontales (salida y entrada) y la lateral del caso base | Mismas elevaciones en todas; misma subida de cama en todos los niveles, 6" (la del nivel 1) | |
| OV-I67-04 | Push Back nuevo con el caso base | 46 / 46 / 46; cama y largueros frontal y posterior coherentes; pendiente como antes | |
| OV-I67-05 | Push Back compuesto A/B | Ambos lados con la regla; pendiente como antes | |
| OV-I67-06A | Rack **Dinámico** del DWG anterior: abre y guarda; luego `RACKEDITAR` y **Actualizar** | Abrir y guardar no cambian nada. Actualizar lo corrige, todas las vistas coinciden y el nivel 1 (salida y entrada) no se mueve | |
| OV-I67-06B | Lo mismo con un rack **Push Back** | Igual que 06A y pendiente como antes. La cabecera puede cambiar (excepción del Freeze §4); también al **Insertar** una vista desde `RACKEDITAR` | |
| OV-I67-07 | Guarda, cierra y reabre el caso base; luego cambia la tarima a 35" y actualiza | Valores conservados al reabrir; con 35", separaciones 48 / 48 / 48 | |
| OV-I67-08 | BOM del rack actualizado | Altura y postes coherentes con el dibujo | |
| OV-I67-09 | Sobre un rack existente **sin actualizar**: BOM y `RACKPROYECTAR` | BOM y vista nueva usan la regla; las vistas hermanas siguen igual hasta Actualizar. Es lo que decidió el Owner (D-LEG) | |
| OV-I67-10 | **Dinámico** sin el defecto: tarima 60", primer nivel 12", 2 niveles, 4 fondos de 48" | Separación 72"; entrada superior 90.6053"; cabecera 120", igual que antes de I-67 | |

Además, el checklist aplicable de la guía manual §5 (dibujo, actualización en sitio, vistas ligadas, round-trip, BOM y legacy).

## Veredicto

```text
Fallos y severidad: ninguno declarado
Resultado global: aprobado
Confirmación explícita del Owner: «Aprobada» (chat, 2026-11-09), sobre FINAL_CANDIDATE_SHA b6b988fb815b233d4e621b7f24d2e94b5800ed96
Resultado por escenario: el Owner declaró el resultado global; no dictó resultados individuales
```
