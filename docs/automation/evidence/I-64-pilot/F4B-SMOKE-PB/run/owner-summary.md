# Smoke-PB (PB-T5): resumen para el Owner

**Estado (2026-11-24):**
- Las 18 órdenes de la guía están hechas. La sesión operó sola desde tu «avanza con todo lo que puedas».
- Ningún AutoCAD de la sesión sigue abierto, ninguna copia se guardó y los originales están intactos (SPB-51).
- No queda configuración de RackCad (SPB-52).
- **Una revisión independiente** revisó la evidencia antes de custodiarla: CUSTODIABLE TRAS CORRECCIONES, sin ninguna fuga de privacidad. Sus correcciones ya están hechas, y **cambió ocho propuestas**: SPB-02, 06 (a), 15, 19, 24, 34, 41 y 44.
- El detalle está en `run-log.md`; las propuestas revisadas, en su sección final.

Todo lo de abajo son **propuestas de la sesión**: el juicio y la firma son tuyos.

## Los veredictos propuestos

| Resultado | Pasos |
|---|---|
| **PASS** | SPB-01, 03, 04, 05, 06 (b) y la repetición de 06 (a), 10 a 14, 17, 21, 24, 26, 28 a 32, 35, 37, 38, 40, 42 (a)(b), 44, 46, 49 a 55, 57 a 61, 64 |
| **PASS con límite** | SPB-02 (`RackCad.Domain.dll` no se vio cargado); SPB-34 (PASS en la copia; la gemela, LIMITATION, porque «Editar» no se ofrece en ese estado); SPB-44 (que el origen no cae en el `Primary` es derivado, no observado) |
| **Falta tu juicio O** (la parte S pasa) | SPB-08, 09, 16, 18 (e), 20, 23, 25, 36, 39 |
| **FAIL frente al criterio escrito,** con una pregunta para ti | SPB-06 (a) (el intento 1), SPB-15, SPB-19 (con el orden escrito), SPB-27 (el intento 1), SPB-33 |
| **OBSERVADO** | SPB-41, 62, 63 |
| **LIMITATION** | SPB-45 (la parada de §4.12), SPB-47 |
| **NOT AVAILABLE FOR PHYSICAL SMOKE** | SPB-43, 48, 56 |
| **NO VERIFICABLE FÍSICAMENTE** | SPB-22, 65 |
| **Pendiente de ti** | SPB-07 |

## Lo que solo puedes decidir tú

1. **SPB-33**, dos preguntas. La primera decide también SPB-62.
   - (1) ¿«Borrar todas las vistas» son solo las referencias, o también las definiciones?
   - (2) ¿Vale que un solo Deshacer no lo restaure todo?
2. **SPB-15:** tras «Cerrar» sin cambios, la lista de Deshacer tiene dos entradas nuevas, «Group of commands» y «Rcdump». El criterio pide «sin entrada nueva». El `RCDUMP` que manda la guía cae dentro de la ventana medida. ¿Das esas entradas por del inventario? Si es así, sería PASS.
3. **SPB-06 (a):** con el ejemplo de la guía (48.0004), el valor se guardó redondeado (48). Es por diseño (D-PBTC-01). ¿El ejemplo valía para este criterio? La repetición, con otro valor, pasó.
4. **SPB-19:** con el orden escrito, `REDO` no rehízo; con el orden que decidiste (`U` y `REDO` seguidos), sí. ¿Qué orden cuenta?
5. **SPB-27:** el segundo «Sí» compara por grupos. ¿Es lo esperado?
6. **SPB-45:** ¿«Already in paper space.» cuenta como aviso? Si no, se repite en copias nuevas, con tu orden.
7. **Los OBSERVADO:** SPB-41, 62 y 63, y los informativos de la sección siguiente. Decide si alguno pasa a `ideas-futuras`.
8. **Las desviaciones y las repeticiones** de abajo: si las aceptas.

## Los OBSERVADO informativos

Funcionan, pero hay algo que no es como dice la guía:

- **SPB-15:** el `RCDUMP` de la sesión deja `DBMOD` = 1 y el panel en «posiblemente obsoleto».
- **c05:** al «Cerrar» tras devolver los valores, la ventana preguntó «Hay cambios sin aplicar al borrador que se perderán al cerrar. ¿Deseas continuar?».
- **SPB-19:** tras un `U`, el panel queda «posiblemente obsoleto» hasta «Comprobar ahora».
- **SPB-25:** la frase de la escala dice «El resto del rack se toma del dibujo», pero se conservó el borrador entero.
- **SPB-40 y SPB-53:** los avisos nombran las vistas por su handle, no por su nombre.
- **SPB-41:** el tooltip que cita la guía es el del XAML, no el que se ve.
- **SPB-44:** con el borrador limpio, el panel no ofrece «Editar».
- **Hecho informado de SPB-46:** el panel y el clásico borran la referencia anidada sin avisar y dejan vacío el bloque del usuario.

## Para tu revisión (los 9 S+O)

Las capturas están en `screens/`, recortadas.

| Paso | Qué mira el O | Las capturas | Lo que midió la sesión |
|---|---|---|---|
| SPB-08 | Si el aviso del lado B sin frentes se entiende | `c01-SPB-08-casilla-lado-B.png`, `c01-SPB-08-aviso.png`, `c01-SPB-08-reabrir-casilla.png` | El aviso sale y no bloquea. El texto, igual al del código, se comparó **a la vista** |
| SPB-09 | Si la geometría restaurada se ve correcta | `c03-SPB-09-r2-lateral.png`, `c03-SPB-09-r2-planta.png` (y las de antes y de r1) | r2 vuelve a los valores estándar, con las vistas redibujadas |
| SPB-16 | El juicio de SPB-08, sobre la ventana de «Editar todo» | `c05-SPB-16-aviso-ampliado.png`, `c05-SPB-16-lado-B-marcado.png`, `c05-SPB-16-lado-B-desmarcado.png` | El aviso, en la ventana de «Editar todo» del panel. El texto se comparó **a la vista** |
| SPB-18 (e) | El nombre, los niveles y las cotas tras «- frente» y la escala 2.5 | `c07-SPB-18-e-lateral-post.png`, `c07-SPB-18-e-planta-post.png` | (a) a (d) y (f) pasan; el panel y RACKEDITAR dejan el dibujo idéntico |
| SPB-23 | Si la espera es aceptable | (sin capturas: tiempos) | «Actualizar» del panel en PB E, de 9 a 13 s (c07) y de 15 a 20 s (c09); RACKEDITAR, de 9 a 13 s |
| SPB-20 | El rótulo «PB G» en las dos vistas | `c10-SPB-20-lateral-rotulo.png`, `c10-SPB-20-planta-rotulo.png` | El `Name` del sobre de origen en las dos vistas; los sobres, sin cambios |
| SPB-25 | Si la presentación basta para elegir | `c12-SPB-25-primer-si-grupos.png`, `c12-SPB-25-grupos-y-segundo-si.png`, `c12-SPB-25-conservar-pregunta.png` | Salen las líneas que tocan. OBSERVADO: la frase de la escala |
| SPB-36 | Si la espera es aceptable | `c19-SPB-36-marca-obsoleto.png`, `c19-SPB-36-editar-todo-con-marca.png`, `c19-SPB-36-cerrado-responde.png` | 1,8 s sin la marca y 4,6 s con ella |
| SPB-39 · CONSTRUCTED | Las vistas A/B | `c20-CONSTRUCTED-SPB-39-actualizado.png`, `c21-CONSTRUCTED-SPB-39-gemela-actualizado.png`, `c20-CONSTRUCTED-SPB-39-frente2-activo.png` | Dos frentes de B, las vistas redibujadas y ninguna vista nueva, igual que la gemela |

## Desviaciones del contrato, declaradas

1. **COM para cerrar AutoCAD, dos veces,** cuando pediste cerrarlo. Las copias quedaron intactas.
2. **Una instancia de AutoCAD de más,** abierta por error por la herramienta. No se tocó.
3. **Texto por el portapapeles:** la herramienta escribe así las expresiones con comillas. Siempre fue en la línea de órdenes, sin ningún `PASTECLIP`.
4. **Pulsaciones que pudieron ir a tu AutoCAD** en el incidente de c11.
5. **El STOP de `catalogs`** en P-04, resuelto por la sesión: la única diferencia era `.gitkeep`.
6. **Operación sin ti delante,** que autorizaste. Tras el FAIL de SPB-33 se siguió solo con pasos independientes, cada uno en su copia.
7. **Gestos y lectores añadidos por la sesión.** La lista está en `run-log.md`.

**Repeticiones:** SPB-06 (a), SPB-19 y SPB-27. Las tres son **por otra causa**: el diseño, el orden de la guía o tu decisión. Ninguna fue por error del operador.

## Lo que falta

1. **Tus respuestas y tus juicios O.** Si decides repetir SPB-45, hace falta AutoCAD otra vez.
2. **La custodia** (commit y push). La evidencia ya está preparada y revisada: 293 capturas recortadas con su registro de derivación, el registro redactado, los resúmenes de los inventarios, la integridad y el entorno.
3. **Tu firma, el cierre de PB-T5 y el gate-close de F4B.**
