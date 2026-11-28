# ADR-0052: El editor clásico queda como legacy; la ventana RackCad es el editor soportado

- **Estado:** aceptado
- **Fecha:** 2026-11-27
- **Decisores:** Owner del proyecto (**decide**, 2026-11-27, en el chat y en el cuadro de preguntas). Redactado por la sesión de I-64 (Claude).
- **Iniciativa relacionada:** I-64 — `architecture/workspace-persistente-rackcad`, desde la rama lateral
  `architecture/workspace-persistente-rackcad-selectivo` ([contrato](../initiatives/I-64-workspace-persistente-rackcad.md),
  [A-14](../initiatives/I-64-A-14.md) §14, [lo construido](../automation/evidence/I-64-pilot/VENTANA-RACKCAD/construido.md)).

## Contexto

I-64 llevó la edición de RackCad a la ventana RackCad: una paleta que no bloquea AutoCAD, con la lista de los racks del dibujo, un editor
por sistema en el centro (Selectivo, Dinámico, Push Back, Cantilever, camas y cabeceras), «Actualizar todo» y las secciones (Variables, BOM
y lista de racks, Almacén, Biblioteca de diseños, Configuración y Perfil estructural). Los editores clásicos, modales, siguen en el código:
RACKEDITAR y RACKMENU los abren.

El 2026-11-26 el Owner decidió que el editor clásico se eliminaría al final de I-64 (SD-02), y por eso sus capacidades se fueron llevando a
la ventana. El 2026-11-27 lo revirtió en dos pasos:

- Primero: «aún no hay que borrar el editor clásico, probablemente mejor lo dejaremos por ahora a menos que elimine dificultades de
  escalabilidad en un futuro». La sexta tanda corrigió todavía defectos del propio clásico (B-1, PB-S17, la seguridad y el BOM de Selectivo).
- Después preguntó si las pruebas durarían más con el clásico («¿Las pruebas van a durar más si no se quita el RackCad viejo?») y propuso
  «dejarlo como legacy sin seguir dando soporte y eliminar las pruebas por lo mismo». Eligió separar las pruebas, no borrarlas, en otra
  iniciativa posterior.

## Decisión

1. **La ventana RackCad es el único editor soportado.** Las funciones nuevas y los arreglos se hacen en la ventana.
2. **El editor clásico se queda como legacy:** sigue en el producto y se abre como hoy, sin funciones nuevas ni arreglos.
3. **Sus pruebas se separan, no se borran.** Las que solo prueban su pantalla pasan a una categoría «Legacy», que no corre en el día a día,
   solo en el CI. Las que protegen código compartido se quedan. Al usar una orden clásica, un aviso dice que es legacy. Las pruebas y el
   aviso van en una iniciativa corta posterior, no en I-64.
4. **Retirar el clásico exige antes que la ventana cubra todo lo que hoy solo hace él:** entre otras cosas, Largueros, los diseños de
   cabecera de la biblioteca como borrador y «Guardar como plantilla…» de la cabecera. Lo retira un ADR nuevo que reemplace a este, con la
   lista de órdenes que se retiran.

## Alternativas consideradas

- **Eliminar el clásico al final de I-64 (SD-02).** El Owner la revirtió el 2026-11-27: lo deja «por ahora». La ventana tampoco cubre
  todavía todo lo del clásico: Largueros sigue WIP.
- **Dejarlo legacy y borrar sus pruebas,** la propuesta del Owner: «¿Podríamos simplemente dejarlo como legacy sin seguir dando soporte y
  eliminar las pruebas por lo mismo?». El Owner eligió «Separarlas, no borrarlas»: las que protegen código compartido se quedan.
- **Seguir corrigiendo el clásico,** como en la sexta tanda. Esta decisión lo sustituye: los arreglos van solo a la ventana.
- **Separar las pruebas dentro de I-64.** El Owner lo dejó para después: «Después, en otra iniciativa». No retrasa el merge de I-64 a `main`.

## Consecuencias

- Positivas:
  - un solo editor que evoluciona: lo nuevo y los arreglos se hacen una vez, en la ventana;
  - el clásico sigue disponible para lo que la ventana todavía no hace;
  - con la iniciativa corta, las pruebas que solo miran la pantalla del clásico dejan de correr en el día a día, sin salir del CI (lo
    medido está en [ideas-futuras](../ideas-futuras.md)).
- Negativas / costos aceptados:
  - los defectos del clásico se quedan como están, aunque se siga abriendo;
  - conviven dos editores por sistema, y RACKEDITAR sigue abriendo el clásico (A-14, §14.3);
  - hasta que llegue la iniciativa corta, las pruebas del clásico siguen corriendo como hoy y no hay aviso legacy.
- No reemplaza ni cambia ningún otro ADR. [ADR-0047](0047-workspace-persistente-modeless-rackcad.md) sigue `propuesto`.

## Referencias

- Las decisiones del Owner: [construido.md](../automation/evidence/I-64-pilot/VENTANA-RACKCAD/construido.md), «Cuarta tanda, ut»
  (SD-02), «Sexta tanda: decisiones del Owner» y «Cierre de I-64 (2026-11-27)».
- [A-14](../initiatives/I-64-A-14.md): C-07, C-09, la fase 9 de §6 y §14.
- [ideas-futuras](../ideas-futuras.md), «I-64 — después del cierre (2026-11-27)»: la iniciativa corta, lo medido y las condiciones para
  retirar el clásico.
