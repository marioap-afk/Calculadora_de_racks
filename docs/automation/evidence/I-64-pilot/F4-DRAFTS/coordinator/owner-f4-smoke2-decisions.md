# I-64 F4 — Decisiones del Owner para Smoke-2 (texto literal)

> **Redacción para el repo público:** las rutas locales se sustituyen por marcadores (`<carpeta-de-copias-de-Smoke-2>`, `<carpeta-del-DLL-de-Smoke-2>`, `<directorio-de-H0>` y `<unidad-de-datos>`). El resto del texto no cambia.

Respuesta del Owner, en el chat de la sesión principal, al bloque «SMOKE-2 — DECISIONES DEL OWNER» que la sesión le presentó tras el cierre de F4-T4C2-PANEL (F4-E70) y la entrega de la guía v2. Se transcribe sin cambios.

---

SMOKE-2 — DECISIONES DEL OWNER

1. Relectura después de escribir (S2-10) y rama «Aplicado; recarga para seguir editando.»

Respuesta: A

Dejarlos fuera de Smoke-2 como limitación declarada y cubiertos por pruebas automatizadas.

No autorizo añadir una inyección Debug al escritor solo para provocar el fallo.

Motivo:

* añadiría código de producto exclusivamente para testing;
* requeriría otra cadena contractual completa;
* no forma parte de los escenarios físicos obligatorios ya definidos para Smoke-2.

Registrar explícitamente que esta rama no queda físicamente provocada en AutoCAD.

⸻

2. Paso opcional 87 — «Aplicado con avisos»

Respuesta: SÍ

Incluirlo.

Debe ejecutarse únicamente sobre una copia desechable.

Si el comportamiento real no coincide exactamente con la expectativa:

registrar OBSERVADO.

No reinterpretar automáticamente como FAIL si revela una diferencia no caracterizada previamente; traer la evidencia antes de decidir.

⸻

3. Dibujos

D1:
OBLIGATORIO.

Usaremos un Push Back real con:

* al menos 3 racks de un solo lado;
* admitidos por el piloto;
* con nombre;
* con anotaciones;
* rack A con al menos 2 vistas enlazadas.

D2:
Sí.

Puede ser una segunda copia independiente de D1.

No reutilizar el mismo archivo de trabajo para escenarios cuya historia de Undo/Redo, edición o conflictos pueda contaminar otro escenario.

D3:
Usaremos todos los casos no editables que tenga disponibles.

Prioridad:

1. sobre sin Id;
2. sobre divergente;
3. Cama de dos miembros;
4. A/B;
5. referencia externa.

Si alguno no existe en los dibujos disponibles:

registrarlo como NO DISPONIBLE y no fabricar artificialmente un caso salvo que la guía ya contemple una preparación segura.

D4:
Sí.

Usar un dibujo antiguo real con Push Back si hay uno disponible.

Debe trabajarse únicamente sobre una copia.

Lista S2-15:
usar dibujos/casos reales suficientes para cubrir la matriz de admisión que exige la guía.

No reducir S2-15 a un único dibujo si eso deja clases de admisión sin representar.

⸻

4. Cuándo lo hacemos

Smoke-2 se ejecutará cuando:

1. T4C2 esté formalmente cerrado;
2. la rama y custodia correspondientes estén publicadas;
3. el DLL exacto candidato esté identificado;
4. AutoCAD esté completamente cerrado antes de compilar/cargar ese DLL;
5. estén preparadas las copias de trabajo bajo:

<carpeta-de-copias-de-Smoke-2>\

6. se haya comprobado y registrado el hash/estado de los originales antes de empezar;
7. no haya otra sesión o iniciativa modificando el mismo worktree/producto.

Cuando se cumplan esas condiciones:

PREPARAR SMOKE-2.

No empezar automáticamente una sesión física de AutoCAD sin que el Owner esté presente para operar y responder PASS / FAIL / OBSERVADO.

⸻

DISPOSICIONES ADICIONALES

A. La guía v2 queda aceptada como base operativa de Smoke-2.

B. El comportamiento:

«No se aplicó: Ningún sobre editable de la relectura coincide con el origen del borrador.»

después de perder el origen válido queda aceptado como diseño ya declarado de T4B, no como hueco nuevo.

C. El conflicto mediante RACKPROYECTAR del paso 34 debe ejecutarse.

Como aún no fue validado físicamente:

si no reproduce exactamente la expectativa, registrar OBSERVADO y conservar:

* dibujo;
* comandos ejecutados;
* mensajes;
* estado previo/posterior;
* resultado de Undo si aplica.

No improvisar una corrección durante Smoke-2.

D. No fingir evidencia de condiciones internas que AutoCAD no expone.

Cuando algo no pueda observarse físicamente sin instrumentación:

registrar:

NO VERIFICABLE FÍSICAMENTE / CUBIERTO POR PRUEBA AUTOMATIZADA

según corresponda.

E. Los originales nunca se abren.

Solo copias de trabajo.

F. Antes de cada paso destructivo mantener el aviso explícito de la guía.

G. Los directorios:

<directorio-de-H0>
<directorio-de-H0>-out
<directorio-de-H0>-dwg

siguen como limpieza manual pendiente del Owner.

No son requisito funcional para ejecutar Smoke-2 salvo que alguna comprobación de aislamiento indique lo contrario.
