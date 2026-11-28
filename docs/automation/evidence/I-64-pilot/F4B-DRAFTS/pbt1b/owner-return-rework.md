# I-64 F4B PB-T1b: vuelta al Owner tras la revisión independiente (REWORK 0/2/15)

**Fecha:** 2026-11-18. La prepara la sesión principal.

## Los hechos

**El Worker** `R20261118T204715Z-a4e2` (contrato de la v2.7, intento 1) terminó en IMPLEMENTATION_COMPLETE:
- RED `2d9febc6`, con solo el esqueleto y las seis clases de prueba;
- GREEN `a0a5f699`, solo en `src/` de Application;
- la CI del GREEN da 4/4, con exactamente las cifras previstas (Core 19 086 y UI 2 437);
- en pb-15 aparece exactamente la normalización conocida.

**La revisión directa:** conforme.

**La revisión independiente:** REWORK, 0 bloqueantes, 2 importantes y 15 menores.
- **El producto en `src/` está bien:** la API es exacta, no hay ningún campo nuevo y D2 se respeta (solo se comparten las dos referencias, y nada muta un rack ni un informe).
- **Los dos importantes son defectos de pruebas que venían del borrador heredado de la invocación anterior:**
  - **I01.** La guarda G-L1 (CT-14) no aplica a los alias la regla 1b de D-PBT1-15, como dice el paquete. Una mutación futura del tipo «alias + molde» pasaría sin verse. Hoy no esconde nada: hay 0 coincidencias.
  - **I02.** Un caso de CT-10 no puede fallar, porque los dos estados que compara tienen nombres distintos.
- **Los menores:**
  - siete son de pruebas que se pueden reforzar (M01, M02, M04, M06, M07, M08 y M13);
  - cuatro son de producto, para declararlos o pasarlos a PB-T2 y PB-T4 (M09, M11, M12 y M15);
  - dos van contra la letra del contrato (M10 y M14).

**Las dos discrepancias con la letra del contrato:**
- **M10, INV-PBT1B-03.** Cierra la lista de los ayudantes privados («solo LoadComposite, CopyLiteral, CopyModule y los privados del comparador que copian literal los del piloto»). El comparador y el mapa correctos necesitan otros ayudantes privados (`TailsEqual`, `CarriersEqual`, `BuildTable`, …). No añaden ningún miembro público ni ningún campo a los tipos del censo.
- **M14, ExpectedEvidence[2].** Pide «build-plugin en success» en la CI del RED. Eso es imposible en PB-T1b: en la CI, `build-plugin` depende de `tests`, y en este RED Core falla a propósito, así que `build-plugin` queda omitido. Es un texto que el paquete arrastra desde PB-T1a, donde Core no fallaba.

## Las decisiones

**1. El camino.**
- **(a) Corregir, en el intento 2 de 3.** Una delegación de corrección que:
  - arregla I01 y I02 y los siete menores de pruebas;
  - vuelve a poner el esqueleto en el RED, como manda el §6.0 para una corrección que toca pruebas;
  - restaura en el GREEN la misma implementación de producto.

  Los menores de producto se declaran o pasan a PB-T2 y PB-T4 como arrastres. El intento 3, si llegara, vuelve a ti.
- **(b) Aceptar el intento 1 con I01 e I02 como limitaciones conocidas.** No recomendado: I01 debilita la condición 10 de D2.
- **(c) Parar.**

**2. La letra del contrato (M10 y M14).**
- **(a) Ratificar las dos, solo para PB-T1b.** Se pasan como disposición a la verificación (como CT-02 en PB-T1a), y el paquete de PB-T1c corrige el texto.
- **(b) Exigir la letra.** M10 obligaría a meter esos ayudantes dentro de los métodos permitidos, una refactorización sin valor. M14 no tiene arreglo sin reemitir el contrato.
