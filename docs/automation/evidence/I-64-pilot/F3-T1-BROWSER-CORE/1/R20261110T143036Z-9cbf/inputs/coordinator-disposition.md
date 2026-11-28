I-64 F3-T1-BROWSER-CORE - DISPOSICION DEL EMISOR DEL CONTRATO SOBRE LA PREGUNTA ABIERTA DE LA ENTREGA

Autoridad: la sesion que emitio el contrato de F3-T1 por orden del Owner I-64 F2 CLOSE RATIFICATION / F3 AUTHORIZATION.

Pregunta abierta de la entrega (OpenQuestions y KnownLimitations): si Destroy de una SessionId para la que nunca se creo una sesion de navegador debe bloquear un GetOrCreate posterior.

Disposicion: INV-F3T1-13 dice que una SessionId destruida no se resucita y que GetOrCreate devuelve null para ella, sin distinguir si esa SessionId llego a tener sesion de navegador. Destroy deja destruida cualquier SessionId que reciba. La pregunta queda cerrada en ese sentido; el contrato no deja ambiguedad material en este punto.

Esta disposicion no cambia el contrato, la delegacion ni la entrega.
