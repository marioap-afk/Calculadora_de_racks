Decisión del Owner sobre config.toml tras el reinicio del equipo (2026-11-13, durante la verificación de F4-T3B2), por la herramienta de preguntas de la sesión. Pregunta y respuesta literales:

Pregunta: «Tras el reinicio, la verificación del Controller de F4-T3B2 murió sin salida y hay que repetirla. Además, la app de escritorio de Codex (antes cerrada) se abrió sola al arrancar y reescribió config.toml a las 08:44: mismas 104 claves en el mismo orden, pero algún valor distinto, así que ya no es el Snapshot G y por tu regla es STOP. ¿Cómo sigo?»

Opciones ofrecidas:
- «Acepto como Snapshot H (Recommended)»: Registro el config actual como Snapshot H (mismas claves y orden que G), cierro la app de Codex durante la cadena si sigue abierta, y repito la verificación desde cero. La corrida interrumpida se custodia como interrumpida, sin salida.
- «Yo restauro G»: Tú devuelves config.toml al contenido de G (por ejemplo, revirtiendo el ajuste que cambió la app). Yo espero, compruebo que el hash vuelve a ser G y repito la verificación.
- «Detener aquí»: Dejo F4-T3B2 detenida con el Worker entregado, CI verde y las dos revisiones hechas, hasta que revises la configuración.

Respuesta: «Acepto como Snapshot H (Recommended)».

Snapshot H: SHA-256 A859B66CC9A4631FEC01AA2E798AAC4F7E07F911352735A6A35D8169D9639864; 104 nombres; OrderedNamesSha256 EB8775C5F9B3CC5C15031ED064391FFE000AB4A7220360C04F2F89D83D49F820 (igual que G).
