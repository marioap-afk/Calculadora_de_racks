> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<checkout-principal>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# Encargo: revisión independiente adversarial del paquete v1 de PB-T4 (I-64 F4B)

Eres revisor independiente. Tu trabajo es **encontrar lo que está mal** en el paquete v1 del contrato de PB-T4, antes de que se pregunte al Owner y se emita. Trabajas **en solo lectura**. No redactas el paquete ni lo corriges: informas.

## Qué revisas

- `<scratchpad>\hs65\pbt4\I-64-f4b-pbt4-gate-contract-package.md` (v1);
- `pbt4-measurements.md`, `measure_pbt4.py` y `measure_pbt4-out.txt`, en la misma carpeta;
- el encargo que recibió el redactor, `drafting-brief.md`, en la misma carpeta.

## El contexto

- **I-64 F4B** lleva «Push Back completo» al Workspace de RackCad (plugin de AutoCAD 2025, .NET 8). Las tareas PB-T0 a PB-T3b están cerradas. PB-T3c (la fachada de borrado de vistas del Plugin) está emitida en `7dfc7c21` y su Worker trabaja ahora en el worktree.
- **PB-T4** es «Editar todo» en el panel del Workspace. Lo que pide el Owner, literal: «ACCEPT. Add "Editar todo" to the Workspace only after PB-T1/T2 establish the draft-based editor seam. Do not make PB-T4 invent state translation logic. It should consume the already proven full Push Back draft/state API.»
- **La fase y sus decisiones:**
  - el paquete de la fase es `docs/initiatives/I-64-f4b-push-back-completo-package.md` v3.1;
  - las decisiones del Owner están en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/`;
  - el registro de disposiciones es `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/f4b-dispositions.md` (F4B-E01 a F4B-E47).
- **La emisión de PB-T4 no está autorizada.** El paquete propone tres preguntas al Owner (su §8).

## Cómo leer (obligatorio)

Hay un Worker trabajando en el worktree de la rama. Está **PROHIBIDO** leer, listar o ejecutar nada en `<worktree-de-I-64>`, y escribir esa ruta en una orden.
- **Lee el repositorio solo por objetos git** desde el checkout principal, que comparte el almacén: `git -C "<checkout-principal>" show 7dfc7c21:<ruta>`. También valen `grep -n <patrón> 7dfc7c21 -- <rutas>`, `ls-tree`, `log` y `diff`. No leas los archivos de ese checkout en disco.
- **No ejecutes nada que compile o escriba:** sin compilar, sin ejecutar pruebas, sin órdenes git que escriban, sin `fetch` y sin AutoCAD.
- **Python solo de lectura,** con `-I`, en guiones escritos con la herramienta Write, nunca con `-c`, `python -` ni heredoc. Los guiones llevan el prefijo `ir4_`.
- **Escribe solo en `hs65\pbt4\`:** tu informe y tus guiones `ir4_*`.

## Qué buscar (sin limitarte a esto)

1. **Fidelidad al Owner.** ¿Algo del diseño es, en el fondo, la «traducción de estado» que prohíbe [OD]? Esto incluye cualquier conversión entre el borrador del piloto y el completo, por cualquier camino. ¿El paquete cumple D-PB-01 (modal, draft-in/draft-out, cero escrituras en el DWG; WORKSPACE nunca llega al camino clásico)? ¿Cumple D-PB-05 con sus pasos y OB-05.4 (elección destructiva informada, sin diff JSON), D2 (las dos referencias compartidas no se mutan), AUTH-13 («Basta así») y la nota del Owner sobre la interfaz (no añadir botones ni campos que no pida el contrato)?
2. **Los hechos medidos.** Comprueba por muestreo, con ruta:línea en `7dfc7c21`, los hechos en los que se apoya el diseño. Mira con especial cuidado:
   - «K = 1 por el modal del panel, que ya existe y nadie usa»;
   - la UI del Workspace no puede abrir diálogos;
   - la cota de 644 pruebas candidatas en 17 clases;
   - las guardas que no pueden seguir en verde (CT-PBT3A-12, CT-PBT3B-13, G-4 y `WorkspaceEditIntentGuardTests:711`);
   - los miembros con cuerpo por defecto en `IWorkspacePilotEditPort` frente a las guardas existentes;
   - OB-03.2 (si PB-T3 ya quitó el rechazo A/B o no).
3. **La división y el orden** de PB-T4a y PB-T4b. ¿Es necesaria? ¿Cabe cada una en un Worker sin latitud? ¿Hay un camino más pequeño que cumpla [OD]?
4. **Las sustituciones con registro.** Hasta 644 métodos es mucho. ¿El criterio para decidir qué se sustituye es cerrado y verificable antes de emitir (precondición 6, PBT4A-C15)? ¿Hay riesgo de debilitar en silencio una guarda (el bloque T1B de [OD]: «Do not silently weaken the old guard»)? ¿Qué pasa con las guardas de D-1 y con E-3, E-5 y E-6, que según el §5.3 del paquete de PB-T3 se sustituyen «al quitar el piloto»?
5. **Las preguntas al Owner (§8).**
   - ¿Son necesarias?
   - ¿Son autosuficientes? El Owner solo ve el diálogo.
   - ¿Son neutrales, o la redacción empuja hacia la recomendada?
   - ¿Las opciones son reales y están completas?
   - ¿Falta alguna decisión material que el paquete toma por su cuenta?
   - ¿Sobra alguna que la sesión pueda decidir?
6. **Los arrastres:** A-T2b-OWNER-01, la validación única (Q-PBT2-3), D-1 y E-3/E-5/E-6, A-2 y L-5, OWNER-01 de Restaurar, PBT3B-IR-M01 y OWNER-2, el layout del configurador y A-T3-9. Para cada uno: ¿está bien clasificado y citado?
7. **El contrato propuesto, por subtarea,** frente a las lecciones de F4B:
   - la lista cerrada de miembros con sus ayudantes;
   - la CI del RED;
   - el esqueleto compilado y la suite completa de Core antes de emitir;
   - el total exacto de cada rama (F4B-E46);
   - el tamaño (F4B-E21);
   - nada en el esqueleto que lean las guardas;
   - UNIT_CHANGE;
   - la regla RR;
   - los sustitutos de nc1 y nc2 con `--task PBT4` (comprueba que sus listas cerradas lo admiten);
   - las pruebas de UI que crean ventanas reales o escriben el perfil.
8. **Las CT:** ¿cada una discrimina? ¿Hay tautologías o lecturas en el momento equivocado? ¿Qué no se prueba sin AutoCAD y queda para el Smoke-PB (D-PB-09)?

## El informe

Escríbelo en `hs65\pbt4\independent-review-v1.md`:
- **secciones numeradas** (§1, §2, …);
- **una tabla de hallazgos:** Id (B-xx, I-xx o M-xx), gravedad, dónde, qué, por qué y qué propones;
- **el veredicto en una línea propia,** con la forma «**Veredicto: …** B/I/M = **n / n / n**», usando uno de estos: NECESITA v2, EMITIBLE TRAS CORRECCIONES o EMITIBLE;
- **una sección propia sobre las preguntas al Owner:** para cada una, si se mantiene, se reformula (con tu texto propuesto) o se quita; y las que falten.

**Responde con:** el veredicto, los hallazgos bloqueantes e importantes (uno por línea), y lo que digas de las preguntas al Owner.
