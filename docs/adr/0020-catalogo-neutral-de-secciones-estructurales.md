# ADR-0020: Catálogo neutral de secciones estructurales

- **Estado:** aceptado
- **Fecha:** 2026-07-27 (redacción y aceptación)
- **Decisores:** Mario Pérez, dueño del repositorio (decisiones vinculantes emitidas al abrir I-36A);
  Claude Opus 5 (redacción)
- **Iniciativa relacionada:** I-36A `architecture/catalogo-secciones-estructurales`
- **Reemplaza a:** [ADR-0008](0008-secciones-unificadas-por-rol.md)

## Contexto

RackCad describe hoy todo perfil estructural en `assets/catalogs/secciones.csv`, una hoja unificada en
la que cada fila declara un **`rol`** —`POSTE`, `CELOSIA`/`CELOSÍA`, `LARGUERO` o `SEPARADOR`— y
`JsonRackCatalogProvider` la proyecta en `PostProfiles`, `TrussProfiles`, `BeamProfiles` y
`SpacerProfiles`. Esa decisión está registrada en [ADR-0008](0008-secciones-unificadas-por-rol.md) y
funciona: es la fuente vigente de todos los sistemas actuales.

Su límite es de modelado, no de calidad. Una fila de `secciones.csv` mezcla **tres cosas distintas**:

1. la **sección transversal** (qué forma tiene el material: peralte, ancho, espesor, área, inercias);
2. el **rol de miembro** (para qué se usa esa sección dentro de un rack);
3. la **pieza comercial** (`partNumber`, `manufacturer`, `unitCost`, `currency`, `costUnit`, y la
   `mensula` que un larguero arrastra como FK).

Mientras el catálogo describió perfiles propios de rack —omega troquelado, travesaño de cinta,
larguero escalón— la mezcla no dolía: cada fila era, a la vez, una sección, un rol y un SKU. Los
sistemas que el ROADMAP pone a continuación rompen esa coincidencia. Un Cantilever se arma con perfil
estructural **estándar** (W, HSS, C, L de la AISC Shapes Database): la misma `W12X26` puede ser
columna, brazo o base, y su designación comercial no es un SKU de RackCad sino una designación de
norma. Modelarla como una fila con `rol` obligaría a duplicar la sección una vez por rol, y a que el
rol —dato del miembro— viviera dentro del dato de la sección.

`CatalogEntryBase` refuerza el problema desde el otro lado: su contrato es el de una **pieza
comercial** con bolsa abierta `Properties`, y `CsvCatalogReader` es deliberadamente **tolerante** —una
celda malformada deja el campo en su valor por defecto y sigue—. Esa tolerancia es correcta para un
catálogo que el usuario edita en Excel y es exactamente lo contrario de lo que necesita un cuerpo de
983 secciones normalizadas importadas de una fuente oficial, donde un `0` silencioso por parseo
fallido es un dato falso indistinguible de un dato real.

Esta decisión restringe trabajo futuro en más de una capa (Application, catálogos, sistemas nuevos) y
es cara de revertir (formato en disco y esquema de identidad), así que se registra antes de
implementar, conforme a los criterios 1 y 2 de [`adr/README.md`](README.md).

## Decisión

1. **`StructuralSection` es la autoridad neutral de la sección transversal.** Describe geometría
   transversal, dimensiones, peso por unidad de longitud y propiedades de sección. Es la única fuente
   de esos datos para los consumidores que la adopten.

2. **Una `StructuralSection` NO tiene rol de miembro.** No es un poste, un larguero, una celosía, un
   separador, un brazo ni una columna. `POSTE`, `CELOSIA`, `LARGUERO` y `SEPARADOR` no pertenecen al
   catálogo neutral y no aparecen en su esquema. El rol es un dato del **miembro**, no de la sección.

3. **El catálogo neutral es independiente de `RackCatalog` y de `CatalogEntryBase`.** Vive en su
   propio namespace `RackCad.Application.StructuralSections`, con su propio proveedor, su propio
   lector CSV **estricto** y su propio validador. No se agrega una colección de secciones a
   `RackCatalog`, no se derivan sus tipos de `CatalogEntryBase` y no se reutiliza `CsvCatalogReader`
   como está: su tolerancia histórica se preserva intacta para los catálogos que la necesitan, y el
   catálogo neutral exige lo contrario (cualquier valor inválido es error con archivo, fila, columna
   e identificador).

4. **`secciones.csv` permanece como catálogo legado de los miembros actuales**, sin cambio funcional,
   hasta que migraciones futuras —una por configurador— lo retiren o lo reduzcan. ADR-0008 describe
   correctamente cómo funciona ese catálogo hoy y ese comportamiento sigue vigente mientras exista.

4.b **Los datos importados y el overlay del operador son cosas distintas y se tratan distinto.** Los
   archivos generados por el importador son función del libro y de nada más: el manifiesto declara su
   SHA-256 y una carga validada los verifica. `structural-section-status.csv` es un **overlay local**
   —la decisión de retirar una sección de las selecciones nuevas— y por eso **no participa de ningún
   hash**: si participara, una edición legítima parecería corrupción de los datos AISC. Se valida
   aparte (esquema, duplicados y existencia de cada `sectionId`), el importador nunca lo reescribe, y
   una reimportación cuyo overlay quede huérfano **se detiene con error** en vez de descartar la
   decisión.

4.c **La única puerta pública de carga valida y falla cerrada.** Un consumidor no puede recibir un
   catálogo meramente parseado: `Load()` comprueba invariantes, manifiesto, metadata, conjunto exacto
   de archivos y hashes antes de entregar nada, y no existe una vía pública para saltárselo. La
   publicación de una importación, además, escribe el manifiesto **al final**, de modo que un estado a
   medias nunca se puede cargar como válido.

5. **I-36A no migra ni borra nada.** No toca `secciones.csv`, ni los perfiles, postes, largueros,
   celosías, separadores o sistemas existentes, ni `blocks.csv`, ni `blocks-library.dwg`. El catálogo
   neutral nace **junto** a lo vigente, sin consumidores de producto.

6. **ADR-0008 queda reemplazado por este ADR** en cuanto a **cuál es la autoridad conceptual de una
   sección estructural**. El reemplazo es de autoridad, no de comportamiento: **no cambia todavía el
   funcionamiento operativo de ningún sistema actual**, que sigue leyendo `secciones.csv` exactamente
   como antes.

7. **La adopción es strangler, configurador por configurador.** Lo nuevo se construye al lado de lo
   viejo; cada consumidor migra cuando su propia iniciativa lo declare; lo viejo se retira al final.
   Ninguna migración forma parte de I-36A.

8. **Los configuradores futuros de miembro son los que aportan lo que la sección no tiene**:
   troqueles, conectores, ménsulas, perforaciones, soldaduras, placas terminales y reglas de
   fabricación. Nada de eso entra al catálogo neutral.

## Alternativas consideradas

- **Añadir las secciones AISC como filas de `secciones.csv` con un `rol` nuevo** — es el camino más
  corto y el peor: obliga a inventar un rol para un dato que no tiene rol, mete 983 filas normalizadas
  en el archivo caliente que comparten todos los sistemas y ~23 archivos de tests, y somete datos de
  fuente oficial a un lector tolerante que convierte un error de parseo en un cero silencioso.
  Rechazada.

- **Derivar los tipos nuevos de `CatalogEntryBase` y añadir una colección a `RackCatalog`** —
  reutiliza infraestructura, pero hereda el contrato equivocado (pieza comercial con bolsa abierta),
  arrastra la tolerancia de `CsvCatalogReader` y hace que `CatalogBlockManifest` empiece a esperar un
  bloque por designación cuando I-36A no crea ni un solo bloque. Rechazada.

- **Migrar `secciones.csv` al modelo neutral dentro de I-36A** — es el destino, pero hacerlo aquí
  convierte una iniciativa de fundación en una migración de todos los sistemas, con validación en
  AutoCAD y riesgo de regresión de dibujo y BOM. El principio 3 del ROADMAP (strangler, no
  reescritura) lo prohíbe explícitamente. Diferida.

- **Modelar la sección con un único tipo de decenas de campos opcionales** — evita cuatro tipos de
  dimensiones, pero deja un objeto sin invariantes donde `Ht` de un HSS y `bf` de una W conviven como
  nulos mutuos y nada impide una combinación imposible. Se prefiere la composición por familia:
  filas planas por familia, un tipo de dimensiones por familia, propiedades opcionales explícitas.
  Rechazada.

## Consecuencias

- **Positivas**: la sección deja de heredar el rol del miembro, así que la misma `W12X26` sirve a
  cualquier configurador sin duplicarse; el sistema N+1 (Cantilever) recibe un catálogo estándar y
  completo sin tocar el catálogo de los sistemas vigentes; la fuente oficial se importa con un lector
  estricto que no puede degradar un dato en silencio; `secciones.csv` y todos los sistemas quedan
  literalmente sin cambios, de modo que I-36A no puede romper dibujo ni BOM.

- **Negativas / costos aceptados**: durante el período strangler conviven **dos** catálogos de perfil
  —el legado por rol y el neutral—, y esa duplicidad conceptual dura hasta que las migraciones
  terminen; el catálogo neutral nace **sin consumidores de producto**, así que su valor es diferido y
  su cobertura vive solo en pruebas; el esquema neutral es más estricto y por tanto menos indulgente
  con una edición manual descuidada.

- **A vigilar**: que ningún rol de miembro se cuele al esquema neutral (verificable: la palabra
  `rol`/`POSTE`/`LARGUERO` no aparece en los CSV ni en los tipos del namespace); que nadie agregue
  secciones a `RackCatalog` ni filas a `blocks.csv` por designación; que las migraciones futuras
  ocurran una por iniciativa y no «de paso»; y que la geometría detallada (contornos, radios, filetes,
  centroide como origen, longitudes arbitrarias, proyecciones, materialización AutoCAD) espere a
  I-36B, porque I-36A conserva **datos**, no dibuja.

## Referencias

- Contrato: [`docs/initiatives/I-36A-catalogo-secciones-estructurales.md`](../initiatives/I-36A-catalogo-secciones-estructurales.md)
- Decisión versionada del dueño: [`docs/automation/decisions/I-36A.md`](../automation/decisions/I-36A.md)
- [ADR-0008: Perfiles estructurales unificados en secciones.csv por rol](0008-secciones-unificadas-por-rol.md) (reemplazado por éste)
- [ADR-0007: Catálogos CSV Excel-first sin base de datos](0007-catalogos-csv-excel-first.md) (el medio CSV no cambia)
- [ADR-0021: Identidad, unidades y presentación de secciones estructurales](0021-identidad-unidades-y-presentacion-de-secciones.md)
- [ADR-0012: Código de producto sin dependencias NuGet](0012-producto-sin-dependencias-nuget.md) (el importador es BCL puro)
- `docs/ROADMAP.md` Fase 6: I-36A → I-36B → I-37 → I-38
- Guía: [`docs/guias/secciones-estructurales.md`](../guias/secciones-estructurales.md)
- Fuente de datos: AISC Shapes Database v16.0 (American Institute of Steel Construction)
