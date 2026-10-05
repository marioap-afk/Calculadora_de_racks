### 16.3 Lectura de autoridades

`AuthorityRevision` es el SHA del commit cuyo árbol contiene los documentos y los cambios normativos de la propia unidad que obligan a la delegación. Para I-61 es el SHA de cierre de su
gate G2, revisado por el Coordinator contra el Freeze; o un commit posterior X de la rama, elegido por el Coordinator, tal que `git diff --name-only <B> X` no contiene rutas `EXTERNAL`
ni rutas con secciones `UNIT_CHANGE`, donde `B` es el cierre de G2 o la `AuthorityRevision` vigente o, tras un rebase, su imagen en el último `RebaseMap`; o la imagen de cualquiera de
ellos registrada en un `RebaseMap`. Es ancestro de `BaseSha`. Al emitir o reemitir un contrato tras una A-n o decisión de la unidad, el Coordinator elige un X que la contenga.

Clases, declaradas en el contrato por ruta y sección:

- `UNIT_DOC`: documentos de la propia unidad (contrato `docs/initiatives/<I>-*.md`, Freeze y A-n, `decisions/<I>.md`, evidencia y mandato registrado). Se leen en `AuthorityRevision`.
- `UNIT_CHANGE`: cambios normativos de la unidad bajo su Freeze, declarados por sección (en I-61, los que enumera su Freeze §2.1). Se leen en `AuthorityRevision`.
- `EXTERNAL`: toda otra autoridad. Se lee en `MainSha`. Con `MB` = `git merge-base MainSha AuthorityRevision`: si su archivo no contiene secciones `UNIT_CHANGE`,
  `git diff --name-only MB AuthorityRevision -- <ruta>` es vacío; si las contiene, el texto de cada sección `EXTERNAL` es idéntico en `MB` y en `AuthorityRevision` (con espacios
  normalizados) y toda diferencia del archivo cae dentro de secciones `UNIT_CHANGE`. Además, `git diff --name-only AuthorityRevision BaseSha` no contiene rutas `EXTERNAL` ni rutas con
  secciones `UNIT_CHANGE`.

Una **sección** va desde su encabezado hasta el siguiente encabezado con el mismo número de `#` o menos (incluye sus subsecciones); el **preámbulo** es el texto anterior al primer
encabezado `##`. Si algo de esto no es verificable → STOP (S-12).
