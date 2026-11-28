# Encargo: revisión independiente y adversarial de la enmienda I-64 A-14 (borrador v1)

## Rutas

- W (worktree de I-64): `<worktree-de-I-64>`
- S: `<carpeta-de-trabajo-de-A-14>`

## Reglas

- W es SOLO LECTURA: ninguna escritura ni orden git que cambie estado. Lectura con Read/Grep/Glob o `git -C <W> show/log/cat-file`.
  Nunca toques `main` ni `<checkout-principal>`.
- Escribe SOLO `S/independent-review-a14-v1.md`. No edites el borrador.
- Sin AutoCAD, sin compilar, sin pruebas. Python solo con Write + `python -I`.
- No leas `S/a14-drafting-notes.md` hasta haber terminado tu propia lectura del borrador contra las fuentes: queremos tu juicio, no
  el del redactor. Después puedes usarlo para comprobar si se le escapó algo o si afirma algo falso.

## Objeto

`S/I-64-A-14-draft-v1.md`: enmienda al plan congelado de I-64 (Proposal V5, `W/docs/initiatives/I-64-proposal-v5.md`, blob
`dc1924ff5a73a4e529fbc5f995621d29351a091b`) para reorientar el resto de la iniciativa hacia una ventana RackCad que no bloquea AutoCAD.

## Fuentes de verdad

- V5 (el Freeze) y sus reglas de cambio; `W/docs/WORKFLOW.md`; enmiendas `W/docs/initiatives/I-64-A-1.md` … `I-64-A-13.md`.
- Estado de F4B: `W/docs/automation/evidence/I-64-pilot/F4B-DRAFTS/f4b-dispositions.md`, `W/docs/initiatives/I-64-f4b-push-back-completo-package.md`,
  el archivo de estado de I-64 en W.
- Lo que aprobó el Owner: `S/owner-a14-decisions.md` (literal) y la tabla §3 y las fases §4 de `S/replaneacion-i64.html`.

## Qué buscar (adversarial: intenta romperla)

1. **Fidelidad:** cada cita o paráfrasis de V5, de F4B y de las decisiones del Owner, ¿es exacta? ¿Alguna afirmación sin fuente,
   algún campo de cabecera inventado por analogía, alguna decisión atribuida al Owner que el Owner no tomó?
2. **Alcance:** ¿A-14 cambia algo que el Owner NO aprobó (más allá de §3/§4 y de `owner-a14-decisions.md`)? ¿Deja fuera algo que sí
   aprobó?
3. **Autoridad:** para cada cambio, ¿la autoridad que declara es la que exigen V5/WORKFLOW? ¿Algo OWNER-RESERVED tratado como de
   Architect/Coordinator, o al revés?
4. **Coherencia interna y con V5:** decisiones de V5 que A-14 no toca pero que quedan contradichas por los cambios (busca en TODAS
   las D-nn, INV-nn, OV-nn y no-objetivos de V5, no solo las citadas). Dependencias entre fases rotas o circulares.
5. **Riesgos técnicos que V5 quería evitar** (en especial D-09/D-20 sobre el lote, D-01 foco/PaletteSet, privacidad del índice,
   identidad de racks): ¿A-14 los trata de verdad o solo los nombra?
6. **F4B:** ¿A-14 deja intactos el contrato de PB-T5 y el Smoke-PB en curso? ¿Reabre algo cerrado?
7. **Publicabilidad:** el repo es PÚBLICO: rutas locales, nombres de personas, correos o datos de clientes en el texto.
8. **Preguntas al Owner:** ¿las que lista son necesarias y suficientes? ¿Alguna se puede resolver con las fuentes sin preguntar?
   ¿Falta alguna?

## Entrega

`S/independent-review-a14-v1.md` con: veredicto (APTO / APTO TRAS CORRECCIONES / NO APTO), recuento bloqueantes/importantes/menores,
y cada hallazgo con id, severidad, cita exacta del borrador (línea), la fuente que lo contradice (archivo y línea) y la corrección
propuesta. Al final, la lista de preguntas que de verdad deben ir al Owner, cada una con sus opciones y la que recomiendas.

Tu respuesta final: veredicto, recuento y las 5 cosas más importantes. Breve.
