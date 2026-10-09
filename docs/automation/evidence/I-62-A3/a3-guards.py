#!/usr/bin/env python3
"""Guardas mecánicas de la enmienda candidata I-62 A-3 (compatibilidad de un runtime sucesor).

Uso:
  python a3-guards.py self-test --out <json> [--a3 <I-62-A-3.md> | --no-text]
      G5: modelo neutral respecto del proveedor de observaciones, invalidadores, sonda de compatibilidad, predicado, medición de V14 y huella
      material (regla 11, sin verificador: la huella se compara solo por su valor exacto; A-3 rechaza toda petición de aceptar huellas por clase
      o de leer valores, y la evidencia de un cambio de huella es solo por nombre; un mecanismo así solo podría introducirlo la A-n de la regla
      11, c); vectores BASE,
      positivos y negativos por regla (motivo de rechazo comprobado), mutantes que deben fallar en dos pasadas (con motivo y ciega al motivo, esta
      por un vector esperado), perfil RED de V14 sin A-3 (RedV14), cobertura de las reglas 1-11 y vínculo con el delta (frases exigidas, patrones
      prohibidos y neutralidad), con el blob del texto leído (A3TextBlob). Sin repositorio. Por defecto toma I-62-A-3.md junto a este archivo; sin
      texto, el resultado es FAIL salvo con --no-text explícito.
  python a3-guards.py preview --repo <worktree> --a3 <I-62-A-3.md> --out <json>
      G1 sobre un borrador sin commit: cada literal de §2 igual, línea a línea, a su fuente fijada por blob (git cat-file, solo lectura); cabecera
      (incluidos el blob de A-2, su acuerdo y el anexo); neutralidad y patrones prohibidos del delta; privacidad de A-3, del anexo y de los demás
      archivos del borrador (paquete, investigación, guardas y resultado: sin rutas de perfil de usuario, también en forma POSIX, ni correos, ni el
      nombre de usuario o el correo del host, leídos en tiempo de ejecución y nunca escritos); aplicación del párrafo sobre V14 tras el ancla: solo
      cambian §6 y el título (función `sections` de clause_map.py, por blob). Además, G3 parcial: blobs congelados en HEAD.
  python a3-guards.py run --repo <ruta> --base <sha> --head <sha> --out <json>
      Marcador para el commit exacto que publique A-3 (A3_BASE se fija entonces; sin fijar, FAIL): G1 (como preview, con A-3 y el anexo leídos de
      head), G2 rutas (base = padre de head; investigación y paquete de OD-2-MAT añadidos en ese mismo commit con los blobs de
      RESEARCH_BLOBS; privacidad de todo archivo añadido o modificado (en los de solo añadido, de lo añadido)), G3 blobs congelados y A-2 sin
      cambio entre base y head e igual a A2_BLOB, G4 C-20b (clause_map.py check), G5 con el texto de head y G5b: a3-selftest.json de head con
      Result PASS, A3TextBlob = blob de A-3 en head, A3Text = ruta de A-3 y G5 igual al recalculado en head.
Sin escrituras salvo --out y un directorio temporal (TMPDIR). Sin red. Nunca ejecuta runtimes de IA ni lee configuración real: la regla 11 se
modela con archivos sintéticos, cuyos valores solo sirven para comprobar que la evidencia nunca los publica ni los compara.
"""
import argparse, copy, datetime, hashlib, hmac, importlib.util, json, os, re, subprocess, sys, tempfile

# ---------------------------------------------------------------- identidades fijadas
A3_BASE = "76b48e785d87af5591f4363246751b7eb1182506"  # padre del commit que publica A-3 (preparada sobre b088421649204742ff682e1eb1ebdafc762823d0)
V14 = "docs/initiatives/I-62-proposal-v14.md"
V14_COMMIT = "4c617e82b32b6c810b68d75fc19472efed22b393"
A3 = "docs/initiatives/I-62-A-3.md"
PKG = "docs/initiatives/I-62-architect-package-A-3.md"
A2_FILES = ("docs/initiatives/I-62-A-2.md", "docs/initiatives/I-62-architect-package-A-2.md")
A2_BLOB = "f1e1d6f08d3cf677500794c7d0019433a4dc7d4e"
A2_PREFIX = "docs/automation/evidence/I-62-A2/"
CLAUSE_MAP = "docs/automation/evidence/I-62-F4/compat/clause_map.py"
CLAUSE_MAP_BLOB = "ffe6ea57257f30fa9eec3e9aaf5bf9982bf59d08"
FROZEN = {
    V14: "34ad80ea1bfff144bfc5169f62920a4c904c1bfa",
    "docs/initiatives/I-62-consensus-freeze.md": "0f6b860837e2478db8525e1fb30a485bc4646816",
    "docs/initiatives/I-62-A-1.md": "c01899a72b940503bb85a0fab42bc085c603fd0f",
    "docs/adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md": "da275e1a141a750fee2a8f501b36aa280d7aa534",
    "docs/adr/0048-ejecucion-delegada-portable-roles-binding-y-autoverificacion.md": "e1bd8d91f10cb4c86eaf7753f9ef2531ae62498e",
    CLAUSE_MAP: CLAUSE_MAP_BLOB,
}
SOURCES = {  # etiqueta de los literales de §2 -> blob fijado (todos leídos en b0884216; V14 igual que en 4c617e82 y A2 que en 3bbaabef)
    "V14": "34ad80ea1bfff144bfc5169f62920a4c904c1bfa",
    "AP": "f525cb1e9d24db3cf5fa5c8e013be7edbdb5e6a5",
    "README": "592dcfd45e8743fd83b3423249fbd3c73e3d3055",
    "ROUTING": "bba08fc4686d7192deeb559752636a467103b4b1",
    "CODEX": "155f3469e345e2397fa26347a9cb52d6fdb91122",
    "CLAUDE": "ae570380509cacabf11f757a27573bf6fe616a36",
    "CATALOG": "166d978d114e635bbef67e295c2ec1da73af6d1f",
    "PREFLIGHT": "6a0540796a473e887677e5043d2105b932b5865f",
    "BINDING": "13501476b775ca18c4c61ba3e1793796b51067d9",
    "FACTS": "3d1b7478b433ebff2f882fb3edae82aff970d151",
    "DEC": "27591f161aabb2de30610b0d17341597fd2a4079",  # creció por añadido desde 015d6fa4 (fd411b13): decisiones §57
    "EV": "6a2b1569caefff15a5b3eeab9529e731d68a03af",   # evidencia hasta §87
    "LC": "f19896a8f1a7c82f74bac7231636f68462a87271",
    "ADR46": "da275e1a141a750fee2a8f501b36aa280d7aa534",
    "A2": A2_BLOB,
}
ANCHOR = "no una foto anterior."
SECTION6 = "## 6. Observación de capacidad y comprobaciones de relevo (D-06)"
GUARDS = "docs/automation/evidence/I-62-A3/a3-guards.py"
SELFTEST = "docs/automation/evidence/I-62-A3/a3-selftest.json"  # a3-guards-result.json va en el commit siguiente, no en el de A-3
ANNEX = "docs/initiatives/I-62-A-3-annex-maf-codex.md"  # anexo no normativo de la regla 11 (nombres de producto, clasificación saneada)
RESEARCH = ("docs/automation/evidence/I-62-A3/maf-codex.md", "docs/automation/evidence/I-62-A3/od2-evolution.md",
            "docs/automation/evidence/I-62-A3/od2-material-baseline-owner-packet.md")  # añadidos (A) en el mismo commit que A-3
RESEARCH_BLOBS = {  # blobs que fija A-3 (cabecera y §10): el paquete que revisa el Architect es el que decide el Owner (M1-08)
    RESEARCH[0]: "a297efb2078931965ea15e14e05162cfb14dcbd8",
    RESEARCH[1]: "3b8dc62039210a48c26674c3c964466192b8b31e",
    RESEARCH[2]: "b2c8ec8a295188d05c0e07d153442f9268aaa20e",
}
ALLOWED_EXACT = {A3, ANNEX, PKG, GUARDS, SELFTEST, "docs/automation/evidence/I-62-evidence.md", "docs/automation/evidence/I-62-F6/README.md",
                 "docs/automation/decisions/I-62.md", "docs/automation/state/I-62.yml"} | set(RESEARCH)
APPEND_ONLY = ("docs/automation/evidence/I-62-evidence.md", "docs/automation/decisions/I-62.md")
REQUIRED_ADDED = (A3, ANNEX, PKG, GUARDS, SELFTEST) + RESEARCH  # la investigación y el paquete van en el mismo commit que A-3 (sin otra vía)
# rutas de perfil con un nombre de usuario (con letra de unidad o en forma POSIX: /c/Users/<nombre>, /mnt/c/Users/<nombre>) o correos (M2-12)
PRIVACY = re.compile(r"(?i)(?:\b[a-z]:|(?<![\w.])/(?:mnt/)?[a-z])[\\/]+users[\\/]+[^\\/\s%<*]|\b[\w.+-]+@[\w-]+\.[a-z]{2,}\b")


def host_identities():  # nombre de usuario y correo del host, leídos en tiempo de ejecución; nunca se escriben ni se imprimen (M2-12)
    ids = [os.environ.get("USERNAME", ""), os.path.basename(os.environ.get("USERPROFILE", "").rstrip("\\/"))]
    try:
        r = subprocess.run(["git", "config", "user.email"], capture_output=True)
        ids.append(r.stdout.decode("utf-8", "replace").strip() if r.returncode == 0 else "")
    except OSError:
        pass
    # `git config user.name` no entra: es el propietario público de los repositorios y figura con derecho en sus nombres (`<propietario>/<repo>`)
    return sorted({i for i in ids if len(i) >= 3}, key=len, reverse=True)


def privacy_findings(name, text, ids=None):  # hallazgos de privacidad de un archivo, sin repetir nunca el nombre ni el correo del host
    f = []
    if PRIVACY.search(text):
        f.append("%s con una ruta de usuario o un correo: %s" % (name, sorted(set(m.group(0) for m in PRIVACY.finditer(text)))[:3]))
    low = text.lower()
    if any(re.search(r"(?<![\w.@-])" + re.escape(i.lower()) + r"(?![\w@-])", low) for i in (host_identities() if ids is None else ids)):
        f.append("%s con el nombre de usuario o el correo del host" % name)
    return f
PRODUCT_TOKENS = re.compile(r"(?i)\b(codex|claude|openai|anthropic|gpt-|opus|sonnet|haiku|chatgpt|gemini|google|copilot|mistral|llama|deepseek|grok|xai"
                            r"|cursor)|\b[a-z0-9]+-cli\b")
# solo el texto entre el sujeto y el verbo puede negar la afirmación: una negación posterior al verbo («… acepta la huella sin otra decisión») no la
# oculta (M1-12)
NOT_NEG = r"(?:(?!\b(?:no|ni|nunca|ning[uú]n|ninguna)\b)[^.;:])"
FORBIDDEN = [  # lecturas de «más nuevo = mejor», aceptación de huellas por clase o subida por la sonda: ninguna puede aparecer en el delta
    r"(?i)salvo que[^.;]{0,60}(versi[oó]n|reciente|nuev)",
    r"(?i)(m[aá]s (reciente|nuevo)|versi[oó]n (mayor|superior|posterior))" + NOT_NEG + r"{0,60}?\b(basta|bastan|acredita|acreditan|compatible|hereda"
    r"|heredan)\b",
    r"(?i)acepta[rn]?\b[^.;]{0,30}\bhuellas? (futuras?|por clase|autom[aá]ticamente|sucesoras?)",
    r"(?i)sin (sonda|medici[oó]n)[^.;]{0,30}\bhereda",
    r"(?i)la sonda (puede )?(sube|aumenta|ampl[ií]a|eleva)",
    # regla 11: un veredicto o una clasificación que acepta, sustituye la sonda o desactiva P-01; un modo MATERIAL vigente por sí mismo
    r"(?i)\b(VERSION_COUPLED|IDENTICAL|veredicto|clasificaci[oó]n)\b" + NOT_NEG + r"{0,40}?\b(acepta|aceptan|sustituye|sustituyen|desactiva|desactivan"
    r"|suspende|suspenden|autoriza|autorizan)\b",
    r"(?i)modo MATERIAL\b" + NOT_NEG + r"{0,40}?\b(rige|vigente|se aplica|est[aá] en vigor)\b",
]
FORBIDDEN_SAMPLES = ["Un sucesor más reciente acredita la compatibilidad", "salvo que la versión sea mayor", "se aceptan huellas futuras",
                     "la versión posterior basta para heredar", "la sonda amplía permisos", "Sin sonda, el sucesor hereda",
                     "el veredicto VERSION_COUPLED acepta la huella", "la clasificación de claves desactiva P-01", "VERSION_COUPLED sustituye la sonda",
                     "el modo MATERIAL rige desde el acuerdo de A-3",
                     # negación posterior al verbo (M1-12): deben detectarse igual
                     "el veredicto VERSION_COUPLED acepta la huella sin otra decisión del Owner", "el modo MATERIAL rige sin decisión del Owner",
                     "VERSION_COUPLED desactiva P-01 solo para la receta", "Un sucesor más reciente acredita la compatibilidad, no la huella"]
TEXT_BINDING = {
    "A3.0": ["Que un runtime sea más reciente no acredita ni eleva nada"],
    "A3.1": ["Los hechos de identidad del runtime son `BinaryHash`",
             "Un hecho de identidad sin fuente declarada en el descriptor es UNKNOWN en las dos observaciones y no cuenta como cambio",
             "uno con fuente declarada que quede sin valor observado (UNKNOWN) en el suelo o en la observación del sucesor",
             "sustituye al medido en la instalación verificada", "nunca es la elección de otro binario presente junto al medido",
             "con `HostInstanceState` OBSERVED", "no cambió nada más que esos hechos: ni la huella ni el estado de autenticación",
             "sin valor observado (UNKNOWN) en alguna de las dos observaciones impide la sucesión", "a medio completar no fija el sucesor",
             "durante una cesión o un intento en curso", "incluida la huella"],
    "A3.2": ["invalida la observación (§6)", "`OBSERVATION_STALE`", "no invalida para siempre la elegibilidad medida", "no hay binding ni trabajo de modelo",
             "no es el `STALE` de la celda"],
    "A3.3": ["Como máximo **una** sonda por sucesor observado y por unidad, rol, acción y celda", "como máximo **una** invocación de modelo de solo lectura",
             "La huella se observa antes y después: un cambio de su valor exacto deja la sonda incompleta",
             "Esta regla no crea autoridad ni presupuesto", "P-07 la comprueba antes",
             "no se hace para un sucesor todavía no observado", "solo bajo una autorización expresa del Owner", "sin aceptar la huella",
             "Si la observación del sucesor ya muestra que no hay sucesor, no se hace la sonda y no se consume",
             "si lo muestra la propia sonda (sus observaciones sin modelo o la huella después), la sonda cuenta como hecha"],
    "A3.4": ["Una **medición completa** es una medición autorizada", "`RUNTIME_COMPATIBILITY_CLASS`", "identidad del adapter, del transporte y del proveedor",
             "o, para el aislamiento del contexto y el modo de consumo, con la que fijan §11.1, §20.3 y `routing.md` §5",
             "distinta de los hechos de identidad del runtime del punto 1", "observabilidad de la terminación; y modo de consumo",
             "solo se comparan por la relación de sucesor del punto 1, nunca por igualdad", "su cambio no basta para invalidar la elegibilidad",
             "por debajo de RUNTIME_OBSERVED es UNKNOWN", "sin valor registrado en el suelo", "con una fila por cada requisito obligatorio"],
    "A3.5": ["`SUCCESSOR_COMPATIBLE` = true solo si", "evaluadas en la fecha de la sonda", "recalculada en esa fecha", "en MATCH o ABOVE_REQUIRED",
             "una fila omitida es UNKNOWN", "solo el nivel de introspección puede ser igual o superior", "no pide ni registra más permisos",
             "no hubo ninguna sonda de compatibilidad fallida", "también en lo que observa la propia sonda",
             "la sonda observa las que su invocación ejerce", "vale por la declaración DISPONIBLE del descriptor con el mismo blob",
             "si lo observado por la sonda no la contradice", "nunca es una autodeclaración de la sesión que sondea"],
    "A3.6": ["heredan la elegibilidad medida del suelo", "sin refrescarse", "`Stale` se recalcula", "la herencia nunca lo devuelve a false",
             "Se registra la identidad nueva del runtime", "pero no se declara", "nunca en `model-catalog.md`"],
    "A3.7": ["UNKNOWN o BELOW_REQUIRED", "no hay herencia", "la observación anterior al cambio sigue invalidada",
             "el preflight de la sonda vale como observación nueva solo por §5 y §6",
             "No hay otra sonda de compatibilidad para el mismo sucesor, la misma unidad, el mismo rol, la misma acción y la misma celda",
             "una sonda fallida no acredita nada", "Con false rigen §5 y §6 como sin esta regla", "cuando cumple por sí misma §5, paso 3"],
    "A3.8": ["Sin aumento silencioso", "nunca una autorización de consumo", "no se hereda con una sonda de solo lectura"],
    "A3.9": ["hasta que el Owner acepte la huella exacta", "`SUCCESSOR_COMPATIBLE` no acepta huellas",
             "la herencia no se usa mientras la huella vigente no esté aceptada", "no se heredan los controles por invocación"],
    "A3.10": ["se compara con el mismo suelo medido"],
    "A3.11": ["Huella material del adapter (`MATERIAL_ADAPTER_FINGERPRINT`): requisitos y posición de esta regla (sin verificador)",
              "la huella del adapter se compara solo por su valor exacto (OD-2)",
              "Todo cambio de la huella deja al sucesor fuera de `SUCCESSOR_COMPATIBLE` (punto 1, c) y conserva P-01 y P-11",
              "Esta regla no acepta ninguna huella, no define ninguna clase de cambios aceptables ni ningún verificador, y no lee valores de "
              "configuración ni autoriza leerlos",
              "Cuando un evento de runtime sucesor (un cambio de los hechos de identidad del punto 1) coincide con un cambio de la huella",
              "el hash exacto de la huella antes y después, con su estabilidad",
              "los hechos de identidad del runtime del punto 1 (identidad y versión del binario) antes y después",
              "los nombres de clave saneados añadidos o eliminados y la estructura, por nombre y sin valores",
              "Ningún otro dato, tampoco digests por clave de valores",
              "Localizar por clave los cambios de valor no forma parte de esta regla",
              "solo podría permitirlo una autorización expresa del Owner que nombre el programa y su blob, las superficies que lee y su caducidad; "
              "esta regla no pide ninguna",
              "solo puede introducirlo la A-n que ordene una decisión expresa del Owner",
              "la clasificación de las claves cambiadas, la comparación con las propiedades materiales ya acreditadas",
              "el fallo cerrado ante cambios desconocidos y la ausencia de ampliaciones implícitas de permisos o de consumo",
              "sin aceptación implícita de versiones nuevas, sin presumir mejor a un sucesor y sin desactivar P-01 en producción bajo la autoridad actual",
              # M5-05: las familias que nunca son acopladas a la versión ni no materiales, por su función (origen S57-03 y M2-07)
              "las claves que fijan el sandbox, la política de aprobación o de permisos, la confianza de los espacios de trabajo, las extensiones "
              "habilitadas o su origen, o el modo de consumo, y las que designan código que el runtime ejecuta o lanza y las fronteras de confianza de "
              "ese código",
              "nunca son acopladas a la versión ni no materiales, por su función y no por la etiqueta de un mapa",
              "y sin función verificada para la versión observada cuentan como tales",
              # M5-06: independencia de los tokens, cambios durante una cesión y autorización de lectura expresa y no sustituible
              "tokens de versión observados con independencia de la propia huella", "nunca leídos de la configuración que explican",
              "se fijan y se cambian solo con la autoridad que esa A-n declare, y se custodian por blob",
              "leer valores, solo con una autorización expresa del Owner, registrada de forma explícita, que nombre el blob del programa ejecutable, "
              "las superficies que lee y su caducidad",
              "ni una disposición del Coordinator, ni una autorización de medición o de consumo, ni la línea de OD-2-MAT la sustituyen",
              "un cambio observado durante una cesión o un intento en curso (P-11 y S-04 sin cambio)",
              "publicación por una lista cerrada", "nombres saneados por una lista de permitidos",
              "una línea base derivada solo de una huella aceptada con OD-2 exacta, y un resultado que el aceptante reproduce",
              "Son requisitos de esa A-n, no una regla operativa de esta enmienda; sin esa A-n rige el punto a)",
              "Nada de esta regla aumenta permisos",
              "Un cambio de la huella es todo cambio de su valor exacto (punto 11, a)",
              "Otro modo de aceptar la huella solo puede fijarlo la A-n del punto 11, c)"],
}


# ---------------------------------------------------------------- G5: modelo
class Reject(Exception):
    pass


LEVEL = ["Eficiente", "Equilibrado", "Frontera"]
EFFORT = ["Routine", "Balanced", "Deep", "Long-horizon", "Maximum"]
EFFORT_VALUES = ["low", "medium", "high", "xhigh", "max"]
ASSURANCE = ["NONE", "REQUESTED", "CONFIGURED", "RUNTIME_OBSERVED", "SERVICE_ATTESTED"]
YN = ["NO", "YES"]
SCALES = {"level": LEVEL, "effort": EFFORT, "introspection": ASSURANCE, "read": YN, "tool-use": YN, "write-commit-push": YN}
WRITE_REQS = {"write-commit-push"}
CLASS = {  # RUNTIME_COMPATIBILITY_CLASS (orden, punto 14; regla 4): propiedad -> comparación con el suelo (solo la introspección admite «superior»)
    "authentication": "EQ", "adapter_transport_provider": "EQ", "model_capability_class": "EQ", "effort_semantics": "EQ",
    "required_permissions": "EQ", "sandbox_behavior": "EQ", "introspection_level": "FLOOR", "context_isolation": "EQ",
    "output_capture": "EQ", "termination_observability": "EQ", "consumption_mode": "EQ",
}
INVARIANTS = ("authentication", "required_permissions", "sandbox_behavior", "context_isolation", "output_capture", "termination_observability")  # (f)
ROLE_OPS = (2, 3, 4, 5, 6, 7, 8, 9)  # operaciones del contrato (§7) que necesita un rol invocado; (h)
UNEXERCISED = (6,)  # (h): la cancelación no la ejerce una invocación de solo lectura terminada antes del tope; vale por la declaración del descriptor
PER_INVOCATION = ("P-24", "closure", "P-23")  # regla 9: controles por invocación (fidelidad de insumos, cierre efectivo, contraste); no se heredan
IDENTITY_FACTS = ("BinaryHash", "AppVersion", "AdapterVersion", "BinaryPathHash")
UNK = "UNKNOWN"
CLAIM_SCALES = {"permissions": ["READ_ONLY", "WORKSPACE_WRITE", "FULL_ACCESS"], "write_scope": ["NONE", "FIXTURE", "REPOSITORY"], "capability_claim": LEVEL}
CLAIM_DIMS = ("permissions", "write_scope", "role_actions", "capability_claim", "consumption_authority")

ID1 = {"BinaryHash": "b1", "AppVersion": "1.0.0", "AdapterVersion": "0.1.0", "BinaryPathHash": "p1"}
ID2 = {"BinaryHash": "b2", "AppVersion": "1.1.0", "AdapterVersion": "0.1.1", "BinaryPathHash": "p2"}
ID3 = {"BinaryHash": "b3", "AppVersion": "1.2.0", "AdapterVersion": "0.1.2", "BinaryPathHash": "p3"}
ID_BIN = dict(ID1, BinaryHash="b9")       # solo cambia el binario (misma versión: actualización real sin cambio de versión)
ID_PATH = dict(ID1, BinaryPathHash="p9")  # solo cambia la ruta del binario
ID_APP = dict(ID1, AppVersion="1.0.1")    # solo cambia la versión de la aplicación anfitriona (B.4 no la tiene)
ID_ADP = dict(ID1, AdapterVersion="0.1.9")  # solo cambia AdapterVersion
ID_NOAPP = dict(ID1, AppVersion=UNK)      # AppVersion sin fuente declarada: UNKNOWN en las dos observaciones
DECLARED_NOAPP = ("BinaryHash", "AdapterVersion", "BinaryPathHash")
ENV1 = {"AdapterId": "adapter-x", "Provider": "provider-p", "Transport": "external-process", "HostInstanceHash": "h1", "HostInstanceState": "OBSERVED",
        "AuthState": "AUTHENTICATED", "DescriptorBlob": "d1", "CatalogEntryBlob": "c1", "RoutingBlob": "r1"}
ENV_Y = dict(ENV1, AdapterId="adapter-y", Provider="provider-r", DescriptorBlob="dy")  # otro adapter, con otra huella declarada
CELL_M = "adapter-x:model-m:Deep"
KEYS = {"A": ("ARCHITECT", "REVIEW", CELL_M), "A2": ("ARCHITECT", "RE_REVIEW", CELL_M), "R": ("REVIEWER", "REVIEW", CELL_M),
        "C": ("CONTROLLER", "PLANNING", "adapter-x:model-k:Balanced"), "W": ("WORKER", "IMPLEMENT", CELL_M)}
TODAY = datetime.date(2026, 10, 8)
VERIFIED = "2026-09-30"

R_INVALID = "observación invalidada"
R_NONSUCC = "no es sucesor"
R_FP = "huella vigente sin aceptar por OD-2"
R_STALE = "Stale (B.5)"

# ---- regla 11 (S-01): MATERIAL_ADAPTER_FINGERPRINT sin verificador. A-3 solo compara la huella por su valor exacto; no acepta huellas por
# clase, no define ningún verificador y no lee valores ni autoriza leerlos. La evidencia de un cambio de huella es solo por nombre (lista cerrada)
EVIDENCE_FIELDS = {"FingerprintBefore", "FingerprintAfter", "FingerprintStable", "IdentityBefore", "IdentityAfter", "KeysAdded", "KeysRemoved",
                   "StructureEqual"}  # regla 11, b): nada más; nunca valores ni digests por clave de valores
# mutantes del verificador de las rondas anteriores, retirados por la decisión S-01: sin objeto, porque A-3 ya no define verificador, mapa, línea
# base material, veredictos, saneado ni registro de autorización (lo que sigue vigente lo prueban los vectores y mutantes de la regla 11 de abajo)
RETIRED_BY_S01 = (
    "M_ExactModeHonorsClassification", "M_UnknownKeyIgnored", "M_StructureIgnored", "M_KeyNameOnlyProjection", "M_AnyMaterialKeyVersionCoupled",
    "M_VersionCoupledAcceptsAndRestores", "M_NoVersionChangeRequired", "M_WideningMapAdmitted", "M_CapabilityKeysIgnored", "M_RulesDigestIgnored",
    "M_VerifierNotPinned", "M_TokenFromConfigAccepted", "M_ResidualIgnored", "M_UnstablePairVersionCoupled", "M_VerdictIgnoresCession",
    "M_BaselineFromUnacceptedFp", "M_RecordPublishesValues", "M_ShortValuesPublished", "M_UnkeyedDigest", "M_RecordNoClassNoDigest",
    "M_NonMaterialReasonUnversioned", "M_NonMaterialAlwaysUnexplained", "M_VerdictNewerOnly", "M_RulesUnpinned_class_map",
    "M_RulesUnpinned_projection", "M_RulesUnpinned_token_kinds", "M_RulesUnpinned_nm_verified", "M_VerifierWithoutOwner", "M_FamilyFromMap",
    "M_UnverifiedFamilyOutside", "M_RawKeyNames", "M_RawCauseNames", "M_RecordNoProvenance", "M_SurfacesIgnored", "M_FamilyVerifiedOnce",
    "M_RulesUnpinned_key_family", "M_LaunchKeyVersionCoupled", "M_AbsentAuthorizationAllowed", "M_AuthorizationForOtherVerifier",
    "M_DoubleQuoteOnlySanitizer", "M_PathCharsOnlySanitizer", "M_RecordExtraField", "M_HostNameInRules")


def syn_cfg(ident, **over):
    """Archivo de configuración sintético de un host, con nombres ya saneados por la huella. La evidencia de la regla 11, b) solo usa su hash y sus
    nombres; los valores existen solo para comprobar que nunca salen ni se comparan por clave."""
    v = {"ui.layout": "compact", "tools.server.command": "%LOCALAPPDATA%/HostApp/app-" + ident["AppVersion"] + "/server.exe",
         "tools.server.env.HOST_APP_VERSION": ident["AppVersion"],
         "notifier": "[%LOCALAPPDATA%/HostApp/bin/" + ident["BinaryHash"] + "/notify.exe]", "sandbox.impl": "unelevated",
         "consumption.tier": "standard"}
    for k, val in over.items():
        if val is None:
            v.pop(k, None)
        else:
            v[k] = val
    return v


def cfg_hash(values):  # la huella exacta de un archivo sintético (SHA-256 del archivo y sus nombres)
    return "CFG-" + hashlib.sha256(json.dumps(sorted(values.items())).encode("utf-8")).hexdigest()[:16]


class CountingFile(dict):  # M6-02: el archivo del host cuenta cada acceso a un valor; registrar la evidencia no debe leer ninguno
    def __init__(self, *a, **kw):
        super().__init__(*a, **kw)
        self.reads = 0

    def __getitem__(self, k):
        self.reads += 1
        return super().__getitem__(k)

    def get(self, k, default=None):
        self.reads += 1
        return super().get(k, default)

    def values(self):
        self.reads += 1
        return super().values()

    def items(self):
        self.reads += 1
        return super().items()


def fp_obs(ident, values, stable=True):  # observación de la huella: hash exacto, nombres saneados, estabilidad, hechos de identidad y, en el host,
    # el archivo (que la huella resume en su hash y sus nombres; la evidencia nunca lo publica, ni lo lee ni lo compara por clave)
    return {"Fingerprint": cfg_hash(values), "Names": sorted(values), "Stable": stable, "Identity": dict(ident), "File": CountingFile(values)}


def vtuple(v):
    return tuple(int(x) for x in v.split("."))


def d(s):
    return datetime.date.fromisoformat(s)


def make_floor(name, identity=ID1, level=None, verified=VERIFIED, env=ENV1):
    role, action, cell = KEYS[name]
    write = name == "W"
    model, lvl, req_lvl = ("model-k", "Eficiente", "Eficiente") if name == "C" else ("model-m", "Equilibrado", "Equilibrado")
    if level:
        lvl = level
    props = {
        "authentication": ("AUTHENTICATED", "subscription"),
        "adapter_transport_provider": (env["AdapterId"], env["Transport"], env["Provider"]),
        "model_capability_class": (model, lvl),
        "effort_semantics": ("Deep", "high", "high") if name != "C" else ("Balanced", "high", "high"),
        "required_permissions": "WORKSPACE_WRITE" if write else "READ_ONLY",
        "sandbox_behavior": "workspace-write" if write else "read-only",
        "introspection_level": "RUNTIME_OBSERVED",
        "context_isolation": ("auto-inputs:none", "closure:enumerated"),
        "output_capture": ("structured", "file", "events"),
        "termination_observability": "process-identity",
        "consumption_mode": ("subscription", "no-paid-key", "no-limit-warning"),
    }
    required = {"level": req_lvl, "effort": "Deep" if name != "C" else "Balanced", "read": "YES", "tool-use": "YES", "introspection": "RUNTIME_OBSERVED"}
    if write:
        required["write-commit-push"] = "YES"
    return {"key": KEYS[name], "identity": dict(identity), "env": dict(env), "props": props, "required": required,
            "descriptor_ops": {n: "DISPONIBLE" for n in ROLE_OPS},  # declaración del descriptor (mismo blob en el suelo y en el sucesor)
            "claims": {"permissions": props["required_permissions"], "write_scope": "FIXTURE" if write else "NONE",
                       "role_actions": frozenset({(role, action)}), "capability_claim": lvl, "consumption_authority": 0},
            "eligibility": {"MeasuredInvocation": "MEASURED", "RunRef": "RUN-" + name, "ConsumptionCovered": "MEASURED", "CatalogVerifiedOn": verified}}


def obs(floor, identity, env=None, props=None, reqs=None, assurance=None, contradiction=(), drop=(), nosource=(), ops=None):
    """Observación de la sonda sobre el sucesor: por defecto, igual al suelo; los cambios se dan explícitos."""
    p = {k: {"value": v, "assurance": "RUNTIME_OBSERVED", "contradiction": False, "source": "declared"} for k, v in floor["props"].items()}
    base = {"level": floor["props"]["model_capability_class"][1], "effort": floor["props"]["effort_semantics"][0], "read": "YES", "tool-use": "YES",
            "introspection": "RUNTIME_OBSERVED", "write-commit-push": "YES"}
    r = {k: {"value": base[k], "assurance": "RUNTIME_OBSERVED", "contradiction": False} for k in floor["required"]}
    for k, v in (props or {}).items():
        p[k]["value"] = v
    for k, v in (reqs or {}).items():
        r[k]["value"] = v
    for k, a in (assurance or {}).items():
        (p if k in p else r)[k]["assurance"] = a
    for k in contradiction:
        (p if k in p else r)[k]["contradiction"] = True
    for k in drop:
        r.pop(k, None)
    for k in nosource:
        p[k]["source"] = None
    # la invocación de solo lectura ejerce y observa sus operaciones; la cancelación (UNEXERCISED) no se ejerce si termina antes del tope
    o = {n: ({"state": None, "observed": False} if n in UNEXERCISED else {"state": "DISPONIBLE", "observed": True}) for n in ROLE_OPS}
    for n, v in (ops or {}).items():
        o[n] = v
    return {"identity": dict(identity), "env": dict(env or floor["env"]), "props": p, "requirements": r, "operations": o}


def claim_gt(name, v, fv):
    if name == "role_actions":
        return not set(v) <= set(fv)
    if name == "consumption_authority":
        return v > fv
    sc = CLAIM_SCALES[name]
    return sc.index(v) > sc.index(fv)


def aggregate(statuses):
    s = list(statuses)
    if "BELOW_REQUIRED" in s:
        return "BELOW_REQUIRED"
    if "UNKNOWN" in s:
        return "UNKNOWN"
    return "ABOVE_REQUIRED" if "ABOVE_REQUIRED" in s else "MATCH"


class RuntimeModel:
    a3 = True

    def __init__(self, identity=ID1, env=ENV1, fingerprint="FP1", accepted=("FP1",), today=TODAY, declared=IDENTITY_FACTS):
        self.identity, self.env, self.stable = dict(identity), dict(env), True
        self.fp, self.accepted = fingerprint, set(accepted)
        self.declared = tuple(declared)  # hechos de identidad con fuente declarada en el descriptor (ID_D); los demás, UNKNOWN en las dos
        self.today, self.retirement = today, None
        self.floors, self.state, self.elig, self.claims, self.contract_claims, self.failed_since = {}, {}, {}, {}, {}, {}
        self.probed, self.records, self.history = set(), [], [dict(identity)]
        self.cession_stop, self.s04 = False, False
        self.invocations = 0
        # regla 11 (S-01): A-3 solo compara la huella por su valor exacto; sin verificador ni lectura de valores; evidencia solo por nombre
        self.od2_mode, self.fp_evidence, self.value_reads, self._obs_stable = "EXACT", [], 0, True

    # ---- frescura (routing.md §5: mín(verificación + 90 días, retiro anunciado)), siempre recalculada en una fecha
    def _stale_on(self, verified, when):
        limit = d(verified) + datetime.timedelta(days=90)
        if self.retirement is not None:
            limit = min(limit, self.retirement)
        return when >= limit

    def advance(self, days):
        self.today = self.today + datetime.timedelta(days=days)

    def _binding_stale(self, key):
        return self._stale_on(self.elig[key]["CatalogVerifiedOn"], self.today)

    def _floor_stale_at_probe(self, f):
        return self._stale_on(f["eligibility"]["CatalogVerifiedOn"], self.today)

    def fp_unaccepted(self):  # P-01, P-11 o la línea base de §18: neutral respecto del adapter; solo la huella exacta que aceptó el Owner
        return self.fp not in self.accepted

    # ---- V14 §5, paso 3 (fuera de A-3): la medición completa fija el suelo
    def full_measurement(self, floor, authority=True, owner=True):
        if not authority:
            raise Reject("medición completa sin autoridad")
        if floor["identity"] != self.identity or floor["env"] != self.env:
            raise Reject("medición de otra identidad")
        if self.fp_unaccepted() and not owner:
            raise Reject("con la huella sin aceptar, la medición exige una autorización del Owner")
        k = floor["key"]
        f = copy.deepcopy(floor)
        f["fingerprint"], f["measured_on"] = self.fp, self.today
        self.floors[k] = f
        self.state[k], self.failed_since[k] = "VALID", False
        self.elig[k], self.claims[k], self.contract_claims[k] = copy.deepcopy(f["eligibility"]), copy.deepcopy(f["claims"]), copy.deepcopy(f["claims"])

    def _env_known(self, env, fp):  # regla 1, c): ningún invalidador fuera de ID sin valor; (b): blobs de descriptor, catálogo y routing conocidos
        return all(env.get(k) not in (None, UNK) for k in ("HostInstanceHash", "AuthState", "CatalogEntryBlob", "RoutingBlob", "DescriptorBlob")) \
            and fp != UNK

    def _id_delta(self, a, b):  # Δ ∩ ID_D: solo hechos con fuente declarada y con valor en las dos observaciones
        return {k for k in self.declared if UNK not in (a.get(k), b.get(k)) and a.get(k) != b.get(k)}

    def _id_known(self, ident):  # un hecho de identidad con fuente declarada sin valor observado impide la sucesión
        return all(ident.get(k) not in (None, UNK) for k in self.declared)

    def _successor_change(self, identity, env, fp, during_cession, in_attempt, replaces):
        id_changed = bool(self._id_delta(self.identity, identity)) and self._id_known(self.identity) and self._id_known(identity)
        return id_changed and env == self.env and self._huella_eq(self.fp, self.identity, fp, identity, self._obs_stable) and not during_cession \
            and not in_attempt and replaces and self._env_known(env, fp)

    def observe(self, identity, env=None, stable=True, during_cession=False, in_attempt=False, fingerprint=None, replaces=True):
        env = dict(self.env if env is None else env)
        fp = self.fp if fingerprint is None else fingerprint
        changed = identity != self.identity or env != self.env or fp != self.fp
        if during_cession and changed:
            self.cession_stop = True
        if changed:
            self._obs_stable = stable
            successor = self._successor_change(identity, env, fp, during_cession, in_attempt, replaces)
            for k in self.floors:
                self.state[k] = "OBSERVATION_STALE" if successor and self.state[k] != "NON_SUCCESSOR" else "NON_SUCCESSOR"
                self.elig[k] = {"MeasuredInvocation": "NOT_MEASURED", "RunRef": None}
            self.history.append(dict(identity))
        self.identity, self.env, self.stable, self.fp = dict(identity), env, stable, fp

    # ---- regla 11 (S-01): solo el valor exacto; sin verificador, sin aceptación por clase y sin lectura de valores
    def request_fp_acceptance_mode(self, request):  # regla 11, a) y c): A-3 no acepta huellas por clase ni define otro modo de OD-2; toda
        # petición se rechaza, también la del Owner con una A-n: solo la A-n que ordene OD-2-MAT podría introducir un mecanismo así
        if not self.a3:
            raise Reject("sin A-3: OD-2 exacta")
        raise Reject("aceptación de huellas por clase u otro modo de OD-2: A-3 no lo define; solo podría introducirlo la A-n que ordene OD-2-MAT "
                     "(regla 11, c)")

    def request_value_reading(self, request):  # regla 11, b): A-3 no lee valores ni autoriza leerlos (verificador, localización por clave o
        # clasificación por valores), aunque la petición venga del Owner y nombre el programa, su blob, las superficies y la caducidad: esa
        # autorización quedaría fuera de A-3, que no pide ninguna
        if not self.a3:
            raise Reject("sin A-3: OD-2 exacta, sin lectura de valores")
        raise Reject("lectura de valores: A-3 no la define ni la pide (regla 11, b)")

    def record_fp_change(self, before, after):  # regla 11, b): evidencia de un cambio de huella con un evento de sucesor, por nombre y sin valores
        if not self.a3:
            raise Reject("sin A-3: sin la evidencia de la regla 11, b)")
        if before["Fingerprint"] == after["Fingerprint"]:
            raise Reject("sin cambio de huella: nada que registrar")
        rec = self._evidence(before, after)
        self.fp_evidence.append(rec)
        return rec

    def _evidence(self, before, after):  # lista cerrada: hash exacto antes y después con su estabilidad, hechos de identidad del runtime antes y
        # después, nombres saneados añadidos o eliminados y estructura; nunca el archivo, sus valores ni digests por clave
        nb, na = set(before["Names"]), set(after["Names"])
        return {"FingerprintBefore": before["Fingerprint"], "FingerprintAfter": after["Fingerprint"], "FingerprintStable": bool(after["Stable"]),
                "IdentityBefore": dict(before["Identity"]), "IdentityAfter": dict(after["Identity"]), "KeysAdded": sorted(na - nb),
                "KeysRemoved": sorted(nb - na), "StructureEqual": nb == na}

    def _huella_eq(self, f_fp, f_ident, s_fp, s_ident, stable=True):  # HuellaEq de §3.4: solo el valor exacto (regla 11, a)
        return s_fp == f_fp

    def accept_od2(self, fp, by):
        if by != "OWNER":
            raise Reject("OD-2: solo el Owner acepta una huella")
        self.accepted.add(fp)

    def _tag(self, key):
        return (tuple(sorted(self.identity.items())), key)

    def probe(self, key, o, authority, invocations=1, read_only=True, requested=None, complete=True, fp_change_during=False):
        if not self.a3:
            raise Reject("sin A-3: medición completa")
        if self.cession_stop:
            raise Reject("cambio durante una cesión: S-04/P-11, fuera de A-3")
        if key not in self.floors:
            raise Reject("sin suelo medido de esa unidad, rol, acción y celda")
        st = self.state[key]
        if st == "NON_SUCCESSOR":
            raise Reject("no es sucesor: medición completa (§6)")
        self._check_host_observed()
        if st != "OBSERVATION_STALE":
            raise Reject("sin cambio de identidad del runtime pendiente")
        self._check_successor_observed(o)
        if self._tag(key) in self.probed:
            raise Reject("como máximo una sonda por sucesor, unidad, rol, acción y celda")
        self._check_failed_since(key)
        if not read_only:
            raise Reject("la sonda es de solo lectura")
        if invocations > 1:
            raise Reject("como máximo una invocación de modelo")
        f = self.floors[key]
        self._check_requested(key, requested)
        self._check_authority(authority, invocations)
        self._check_owner_for_unaccepted_fp(authority)
        authority["remaining"] -= invocations
        self.probed.add(self._tag(key))
        if self._fp_changed_during(fp_change_during):
            ok, causes, st_req = False, ["huella cambiada durante la sonda: sonda incompleta, fuera de A-3 (P-01/P-11)"], {}
            complete = False
        else:
            ok, causes, st_req = self.evaluate(f, o, complete)
        run = "PROBE-%d" % (len(self.records) + 1)
        self.records.append({"Run": run, "Key": key, "Identity": dict(self.identity), "InheritsFrom": f["eligibility"]["RunRef"], "Compatible": ok,
                             "Causes": causes, "Complete": complete, "Requirements": st_req, "Controls": PER_INVOCATION,
                             "Observed": {k: (o["props"][k]["value"] if k in o["props"] and self._observed(o["props"][k]) else None) for k in CLASS},
                             "Operations": {n: ("OBSERVED" if (o["operations"].get(n) or {}).get("observed") else "DESCRIPTOR") for n in ROLE_OPS}})
        if ok:
            self._inherit(key, run, o)
        else:
            self._fail(key)
        # un no-sucesor que detecta la propia sonda (regla 3): la sonda ya cuenta como hecha; el invalidador cambiado rige para todo (§6)
        if fp_change_during and causes and causes[0].startswith("huella cambiada"):
            self.observe(self.identity, fingerprint=self.fp + "*")
        elif o["env"] != self.env:
            self.observe(self.identity, env=o["env"])
        return ok, causes

    def _fp_changed_during(self, flag):
        return flag

    def _check_host_observed(self):
        if self.env.get("HostInstanceState") != "OBSERVED":
            raise Reject("no es sucesor: instancia de host UNOBSERVED")

    def _check_successor_observed(self, o):
        if not self.stable or o["identity"] != self.identity:
            raise Reject("sucesor no observado o no estable")

    def _check_failed_since(self, key):
        if self.failed_since[key]:
            raise Reject("tras una sonda fallida: medición completa")

    def _check_requested(self, key, requested):
        for name, v in (requested or {}).items():
            if claim_gt(name, v, self.contract_claims[key][name]):
                raise Reject("aumento silencioso: " + name)

    def _check_authority(self, authority, invocations):
        if authority is None or authority.get("identity") != self.identity or authority.get("remaining", 0) < invocations:
            raise Reject("sin autoridad o presupuesto vigentes (P-07)")

    def _check_owner_for_unaccepted_fp(self, authority):
        if self.fp_unaccepted() and not authority.get("owner"):
            raise Reject(R_FP + ": la sonda solo con una autorización expresa del Owner")

    def _inherit(self, key, run, o):
        f = self.floors[key]
        self.elig[key] = {"MeasuredInvocation": "MEASURED", "RunRef": run, "InheritsFrom": f["eligibility"]["RunRef"],
                          "ConsumptionCovered": f["eligibility"]["ConsumptionCovered"], "CatalogVerifiedOn": f["eligibility"]["CatalogVerifiedOn"]}
        self.claims[key], self.state[key] = copy.deepcopy(self.contract_claims[key]), "INHERITED"

    def _fail(self, key):
        self.state[key], self.failed_since[key] = "OBSERVATION_STALE", True
        self.elig[key] = {"MeasuredInvocation": "NOT_MEASURED", "RunRef": None}

    def _observed(self, x):
        return x is not None and x["value"] is not None and not x["contradiction"] and ASSURANCE.index(x["assurance"]) >= ASSURANCE.index("RUNTIME_OBSERVED")

    def _req_status(self, rid, x, req):
        if rid in WRITE_REQS or not self._observed(x):  # una sonda de solo lectura no observa la escritura; una fila omitida es UNKNOWN
            return "UNKNOWN"
        i, j = SCALES[rid].index(x["value"]), SCALES[rid].index(req)
        return "BELOW_REQUIRED" if i < j else ("ABOVE_REQUIRED" if i > j else "MATCH")

    def _req_statuses(self, f, o):
        return {rid: self._req_status(rid, o["requirements"].get(rid), req) for rid, req in f["required"].items()}

    def _check_prop(self, k, kind, x, fv, f):
        if fv is None:
            return "UNKNOWN (sin valor en el suelo): " + k
        if x is None:
            return "UNKNOWN (no observada): " + k
        if x.get("source") is None:
            return "UNKNOWN (sin fuente declarada): " + k
        if not self._observed(x):
            return "UNKNOWN: " + k
        v = x["value"]
        if kind == "EQ" and v != fv:
            return ("invariante cambiado: " if k in INVARIANTS else "cambiado: ") + k
        if kind == "FLOOR" and ASSURANCE.index(v) < ASSURANCE.index(fv):
            return "por debajo del suelo: " + k
        return None

    def _check_op(self, n, x, declared):
        if x is not None and x.get("observed"):  # observada por la sonda: manda lo observado, también contra la declaración
            return None if x["state"] == "DISPONIBLE" else "operación no DISPONIBLE: %d" % n
        if n in UNEXERCISED and declared == "DISPONIBLE":  # no ejercida: vale la declaración del descriptor con el mismo blob, no contradicha
            return None
        return "UNKNOWN: operación %d" % n

    def _env_differs(self, f, o):
        return o["env"] != f["env"] or o["env"].get("HostInstanceState") != "OBSERVED"

    def evaluate(self, f, o, complete=True):
        causes = []
        if self._floor_stale_at_probe(f):
            causes.append("suelo no vigente: " + R_STALE + " en la fecha de la sonda")
        if not complete:
            causes.append("sonda incompleta")
        if self._env_differs(f, o) or not self._env_known(o["env"], self.fp):
            causes.append("no es sucesor (entorno)")
        if not self._huella_eq(f["fingerprint"], f["identity"], self.fp, self.identity, self.stable):  # HuellaEq (regla 11)
            causes.append("no es sucesor (huella)")
        if not self._id_delta(f["identity"], o["identity"]):
            causes.append("no es sucesor (identidad sin cambio)")
        if not (self._id_known(f["identity"]) and self._id_known(o["identity"])):
            causes.append("no es sucesor (hecho de identidad declarado UNKNOWN)")
        causes += self._extra_identity_causes(f, o)
        st = self._req_statuses(f, o)
        agg = aggregate(st.values())
        if agg not in ("MATCH", "ABOVE_REQUIRED"):
            causes.append("requisitos obligatorios: %s %s" % (agg, sorted(r for r, s in st.items() if s in ("UNKNOWN", "BELOW_REQUIRED"))))
        for k, kind in CLASS.items():
            c = self._check_prop(k, kind, o["props"].get(k), f["props"][k], f)
            if c:
                causes.append(c)
        for n in ROLE_OPS:
            c = self._check_op(n, o["operations"].get(n), f.get("descriptor_ops", {}).get(n))
            if c:
                causes.append(c)
        # una contradicción (propiedad o fila de requisito) da false y además S-04 (§3.5)
        self.s04 = self.s04 or any(x and x.get("contradiction") for x in list(o["props"].values()) + list(o["requirements"].values()))
        return not causes, causes, st

    def _extra_identity_causes(self, f, o):  # los hechos de identidad no se comparan por igualdad (regla 4)
        return []

    # ---- regla 7, no regresión: la invocación de la sonda puede ser la medición de §5, paso 3, si la cumple por sí misma
    def measure_from_probe(self, key, authority):
        recs = [r for r in self.records if r["Key"] == key and r["Identity"] == self.identity]
        if not recs:
            raise Reject("sin sonda que pueda contar como medición")
        r = recs[-1]
        if authority is None or authority.get("identity") != self.identity:
            raise Reject("sin autoridad de medición (§5, paso 3)")
        if not r["Complete"]:
            raise Reject("sonda incompleta: no es una medición")
        if self.state.get(key) == "NON_SUCCESSOR":
            raise Reject("fuera de A-3: rige §6")
        if aggregate(r["Requirements"].values()) not in ("MATCH", "ABOVE_REQUIRED"):
            raise Reject("requisitos no acreditados (P-10): no hay elegibilidad")
        f = copy.deepcopy(self.floors[key])
        f["identity"], f["env"], f["fingerprint"], f["measured_on"] = dict(self.identity), dict(self.env), self.fp, self.today
        f["props"] = {k: r["Observed"][k] for k in CLASS}  # suelo nuevo: completo solo si la sonda registró toda la clase
        f["eligibility"] = {"MeasuredInvocation": "MEASURED", "RunRef": r["Run"], "ConsumptionCovered": "MEASURED",
                            "CatalogVerifiedOn": self.floors[key]["eligibility"]["CatalogVerifiedOn"]}
        self.floors[key] = f
        self.state[key], self.failed_since[key] = "VALID", False
        self.elig[key] = copy.deepcopy(f["eligibility"])

    def ordinary_invocation(self, key, consumption_authorized=True, requested=None, per_invocation=PER_INVOCATION):
        if self.cession_stop:
            raise Reject("S-04/P-11: cambio durante una cesión")
        if self.state.get(key) not in ("VALID", "INHERITED") or self.elig.get(key, {}).get("MeasuredInvocation") != "MEASURED":  # B5
            raise Reject(R_INVALID + " (OBSERVATION_STALE) o sin elegibilidad: sin trabajo de modelo")
        if self._binding_stale(key):
            raise Reject("celda " + R_STALE + " en la fecha del binding")
        if self.fp_unaccepted():
            raise Reject(R_FP)
        self._check_claims(key, requested)
        self._check_consumption(key, consumption_authorized)
        self._check_per_invocation(key, per_invocation)
        self.invocations += 1

    def _check_per_invocation(self, key, fresh):  # regla 9: P-24, cierre efectivo y P-23 se renuevan en cada lanzamiento, también tras heredar
        missing = [c for c in PER_INVOCATION if c not in (fresh or ())]
        if missing:
            raise Reject("controles por invocación sin renovar: %s" % missing)

    def _check_claims(self, key, requested):
        for dim in CLAIM_DIMS:
            cur, base = self.claims[key][dim], self.contract_claims[key][dim]
            if claim_gt(dim, cur, base) or (requested and dim in requested and claim_gt(dim, requested[dim], cur)):
                raise Reject("aumento silencioso: " + dim)

    def _check_consumption(self, key, consumption_authorized):
        if not consumption_authorized:
            raise Reject("sin autorización de consumo")


class V14Model(RuntimeModel):  # antes de A-3: solo una medición (§5, paso 3) restaura la elegibilidad
    a3 = False


def auth(identity, n=1, owner=True):
    return {"identity": dict(identity), "remaining": n, "owner": owner}


def setup(cls, names=("A",), fp="FP1", accepted=("FP1",), level=None, verified=VERIFIED, env=ENV1, identity=ID1, declared=IDENTITY_FACTS):
    m = cls(identity, env, fp, accepted, declared=declared)
    for n in names:
        m.full_measurement(make_floor(n, identity=identity, level=level, verified=verified, env=env), authority=True, owner=True)
    return m


def vectors(cls=RuntimeModel, out=None, blind=False):
    """blind=True: pasada ciega al motivo (cualquier rechazo vale y no se comprueban causas); solo cuentan los resultados."""
    out = [] if out is None else out
    A, A2, C, W, R = KEYS["A"], KEYS["A2"], KEYS["C"], KEYS["W"], KEYS["R"]
    FA = lambda ident=ID2, **kw: obs(make_floor("A"), ident, **kw)  # observación construida con el oráculo independiente

    def case(cid, rule, desc, ok, detail=""):
        out.append({"Id": cid, "Kind": cid.split("-")[0], "Rule": rule, "Case": desc, "Result": "PASS" if ok else "FAIL", "Detail": detail})

    def rej(fn, reason):
        try:
            fn()
        except Reject as e:
            return (True if blind else reason in str(e)), str(e)
        return False, "no rechazado"

    def has(causes, sub):
        return True if blind else any(sub in c for c in causes)

    def P(m, key, o, a, **kw):  # una sonda que se rechaza inesperadamente cuenta como resultado false, no como excepción
        try:
            return m.probe(key, o, a, **kw)
        except Reject as ex:
            return False, ["rechazada: %s" % ex]

    def INV(m, key, **kw):
        try:
            m.ordinary_invocation(key, **kw)
            return True
        except Reject:
            return False

    def oracle_claims(m, key, name):
        return m.claims.get(key) == make_floor(name)["claims"]

    def incompatible(cid, rule, desc, cause, s04=False, **kw):
        m = setup(cls)
        m.observe(ID2)
        a = auth(ID2)
        ok, causes = P(m, A, FA(**kw), a)
        o2, d2 = rej(lambda: m.ordinary_invocation(A), R_INVALID)
        ran = a["remaining"] == 0 and bool(m.records) and m.records[-1]["Compatible"] is False and m.failed_since.get(A) is True
        good = (not ok) and ran and m.state[A] == "OBSERVATION_STALE" and m.elig[A]["MeasuredInvocation"] == "NOT_MEASURED" and o2 and has(causes, cause)
        case(cid, rule, desc, good and (m.s04 if s04 else True), "; ".join(causes) + " / " + d2)

    def not_successor(cid, rule, desc, setup_kw=None, observe_kw=None, obs_kw=None, probe_auth=None, ident=ID2):
        m = setup(cls, **(setup_kw or {}))
        m.observe(ident, **(observe_kw or {}))
        a = probe_auth or auth(ident)
        ok, dd = rej(lambda: m.probe(A, FA(ident, **(obs_kw or {})), a), R_NONSUCC)
        o2, _ = rej(lambda: m.ordinary_invocation(A), R_INVALID)
        case(cid, rule, desc, ok and a["remaining"] == 1 and m.state[A] in ("NON_SUCCESSOR", "OBSERVATION_STALE") and o2 and not m.records, dd)
        return m

    # ---- BASE: V14 sin A-3
    m = setup(V14Model); m.observe(ID2)
    ok1, d1 = rej(lambda: m.probe(A, FA(), auth(ID2)), "sin A-3")
    ok2, d2 = rej(lambda: m.ordinary_invocation(A), R_INVALID)
    m.full_measurement(make_floor("A", identity=ID2), authority=True)
    ok3 = INV(m, A)
    case("BASE-01", "A3.2", "V14 sin A-3: tras el cambio, ni sonda de compatibilidad ni trabajo; solo una medición nueva (§5, paso 3) restaura la elegibilidad",
         ok1 and ok2 and ok3 and m.state[A] == "VALID" and m.invocations == 1, d1 + " / " + d2)

    # ---- positivos
    m = setup(cls); m.observe(ID2)
    ok, causes = P(m, A, FA(), auth(ID2))
    INV(m, A)
    case("POS-01", "A3.6", "sucesor con propiedades iguales: compatible, hereda (RunRef = sonda, InheritsFrom = medición), declara el suelo y trabaja",
         ok and m.state[A] == "INHERITED" and m.elig[A]["RunRef"] == "PROBE-1" and m.elig[A]["InheritsFrom"] == "RUN-A"
         and m.elig[A]["MeasuredInvocation"] == "MEASURED" and oracle_claims(m, A, "A") and m.invocations == 1, "; ".join(causes))
    m = setup(cls); m.observe(ID2)
    ok, causes = P(m, A, FA(props={"introspection_level": "SERVICE_ATTESTED"}), auth(ID2))
    case("POS-02", "A3.5", "nivel de introspección superior al suelo (única propiedad «igual o superior»): compatible; se registra y se declara el suelo",
         ok and oracle_claims(m, A, "A") and bool(m.records) and m.records[-1]["Observed"]["introspection_level"] == "SERVICE_ATTESTED", "; ".join(causes))
    m = setup(cls, level="Frontera"); m.observe(ID2)
    ok, causes = P(m, A, obs(make_floor("A", level="Frontera"), ID2), auth(ID2))
    case("POS-03", "A3.5", "requisito obligatorio en ABOVE_REQUIRED (suelo por encima del requisito): compatible", ok, "; ".join(causes))
    m = setup(cls, fp="FP2", accepted=("FP1",)); m.observe(ID2)
    ok, _ = P(m, A, FA(), auth(ID2, owner=True))
    blocked, dd = rej(lambda: m.ordinary_invocation(A), R_FP)
    m.accept_od2("FP2", "OWNER"); INV(m, A)
    case("POS-04", "A3.9", "suelo medido con la huella sin aceptar (bajo autorización del Owner): la sonda del sucesor, con autorización expresa del Owner, "
         "es compatible; el trabajo espera a OD-2 sobre la huella exacta y después sigue", ok and blocked and m.invocations == 1, dd)
    m = setup(cls, names=("A", "C", "A2")); m.observe(ID2)
    block = auth(ID2, n=2, owner=True)
    okA, _ = P(m, A, FA(), block)
    okC, _ = P(m, C, obs(make_floor("C"), ID2), block)
    ok3, d3 = rej(lambda: m.probe(A2, obs(make_floor("A2"), ID2), block), "presupuesto")
    case("POS-05", "A3.3", "dentro de un bloque de dos sondas (composición con A2-P2): dos celdas heredan; una tercera sonda se rechaza (P-07)",
         okA and okC and block["remaining"] == 0 and ok3 and oracle_claims(m, A, "A") and oracle_claims(m, C, "C"), d3)
    m = setup(cls); m.observe(ID2)
    ok1, _ = P(m, A, FA(props={"introspection_level": "SERVICE_ATTESTED"}), auth(ID2))
    m.observe(ID3)
    ok2, causes = P(m, A, FA(ID3), auth(ID3))
    case("POS-06", "A3.10", "sucesión encadenada: el tercero se compara con el suelo medido (oráculo independiente), no con el intermedio",
         ok1 and ok2 and m.floors[A]["props"] == make_floor("A")["props"] and oracle_claims(m, A, "A"), "; ".join(causes))
    m = setup(cls); m.observe(ID2); P(m, A, FA(), auth(ID2)); f = make_floor("A")["eligibility"]
    case("POS-07", "A3.6", "la herencia copia ConsumptionCovered y CatalogVerifiedOn del suelo, sin refrescarlos; Stale no se copia, se recalcula",
         m.state[A] == "INHERITED" and all(m.elig[A].get(k) == f[k] for k in ("ConsumptionCovered", "CatalogVerifiedOn")) and "Stale" not in m.elig[A]
         and oracle_claims(m, A, "A"))
    case("POS-08", "A3.4", "BinaryHash y AppVersion se registran como hechos; la identidad anterior queda como historia",
         bool(m.records) and m.records[-1]["Identity"]["BinaryHash"] == "b2" and m.records[-1]["Identity"]["AppVersion"] == "1.1.0" and m.history[:2] == [ID1, ID2])
    m = setup(cls); m.observe(ID2); m.observe(ID3)
    ok, causes = P(m, A, FA(ID3), auth(ID3))
    case("POS-09", "A3.1", "un sucesor intermedio sin sondar no impide la sonda del sucesor vigente", ok, "; ".join(causes))
    m = setup(cls); m.observe(ID_BIN)
    ok, causes = P(m, A, FA(ID_BIN), auth(ID_BIN))
    case("POS-10", "A3.1", "solo cambia el hash del binario (misma versión): sucesor; la sonda compatible hereda", ok and m.state[A] == "INHERITED", "; ".join(causes))
    m = setup(cls, names=("A", "C")); m.observe(ID2)
    block = auth(ID2, n=2, owner=True)
    ok, causes = P(m, A, FA(props={"effort_semantics": ("Deep", "high", "xhigh")}), block)
    try:
        m.measure_from_probe(A, block); meas = True
    except Reject:
        meas = False
    INV(m, A)
    case("POS-11", "A3.7", "A-2 y A-3 acordadas: sonda false por una propiedad que no es requisito (effort aplicado), preflight en MATCH; la misma invocación "
         "cuenta como medición (§5, paso 3) dentro del bloque y fija un suelo nuevo, sin consumir otra sonda",
         (not ok) and meas and m.state[A] == "VALID" and block["remaining"] == 1 and m.floors[A]["props"]["effort_semantics"] == ("Deep", "high", "xhigh")
         and m.invocations == 1, "; ".join(causes))
    m = setup(cls); m.observe(ID2)
    ok, causes = P(m, A, FA(), auth(ID2))
    rec = m.records[-1] if m.records else {"Observed": {}, "Identity": {}}
    case("POS-12", "A3.4", "la identidad del adapter, del transporte y del proveedor sigue igual mientras cambian los cuatro hechos de identidad del runtime",
         ok and rec["Observed"].get("adapter_transport_provider") == make_floor("A")["props"]["adapter_transport_provider"]
         and all(rec["Identity"].get(k) != ID1[k] for k in IDENTITY_FACTS), "; ".join(causes))
    m = setup(cls); m.floors[A]["props"]["context_isolation"] = None; m.observe(ID2)
    a = auth(ID2, n=2)
    ok, causes = P(m, A, FA(), a)
    try:
        m.measure_from_probe(A, a); meas = True
    except Reject:
        meas = False
    complete_floor = m.floors[A]["props"]["context_isolation"] == make_floor("A")["props"]["context_isolation"]
    m.observe(ID3)
    ok3, _ = P(m, A, FA(ID3), auth(ID3))
    case("POS-13", "A3.7", "suelo incompleto y A-2 acordada: A-3 false, pero la sonda que registró toda la clase es medición completa; la celda queda medida "
         "y el sucesor siguiente hereda", (not ok) and has(causes, "sin valor en el suelo") and meas and complete_floor and ok3, "; ".join(causes))
    s14 = dict(ID_NOAPP, BinaryHash="b9")
    m = setup(cls, identity=ID_NOAPP, declared=DECLARED_NOAPP); m.observe(s14)
    ok, causes = P(m, A, obs(make_floor("A", identity=ID_NOAPP), s14), auth(s14))
    case("POS-14", "A3.1", "AppVersion sin fuente declarada (UNKNOWN en las dos observaciones) no cuenta en Δ ni impide la sucesión; cambia BinaryHash: "
         "sucesor, la sonda compatible hereda", ok and m.state[A] == "INHERITED" and oracle_claims(m, A, "A"), "; ".join(causes))
    m = setup(cls); m.observe(ID_APP)
    ok, causes = P(m, A, FA(ID_APP), auth(ID_APP))
    case("POS-15", "A3.1", "solo cambia AppVersion (B.4 no la tiene): sucesor; la sonda compatible hereda", ok and m.state[A] == "INHERITED", "; ".join(causes))
    m = setup(cls); m.observe(ID2)
    o = FA()
    ok, causes = P(m, A, o, auth(ID2))
    rec = m.records[-1] if m.records else {"Operations": {}}
    case("POS-16", "A3.5", "la sonda de solo lectura termina antes del tope y no ejerce la cancelación (operación 6): vale la declaración DISPONIBLE del "
         "descriptor con el mismo blob, no contradicha; hereda", ok and not o["operations"][6]["observed"] and rec["Operations"].get(6) == "DESCRIPTOR"
         and all(rec["Operations"].get(n) == "OBSERVED" for n in ROLE_OPS if n not in UNEXERCISED) and m.state[A] == "INHERITED", "; ".join(causes))

    # ---- negativos: propiedades y requisitos (fallo cerrado)
    incompatible("NEG-01", "A3.4", "requisito obligatorio con fuente por debajo de RUNTIME_OBSERVED: UNKNOWN → false",
                 "requisitos obligatorios: UNKNOWN ['introspection']", assurance={"introspection": "CONFIGURED"})
    incompatible("NEG-02", "A3.5", "requisito obligatorio en BELOW_REQUIRED (effort) → false", "requisitos obligatorios: BELOW_REQUIRED ['effort']",
                 reqs={"effort": "Balanced"})
    incompatible("NEG-60", "A3.5", "método de autenticación cambiado con el mismo AuthState → false", "invariante cambiado: authentication",
                 props={"authentication": ("AUTHENTICATED", "api-key")})
    incompatible("NEG-04", "A3.5", "permisos cambiados (más amplios) → false", "invariante cambiado: required_permissions", props={"required_permissions": "WORKSPACE_WRITE"})
    incompatible("NEG-05", "A3.5", "comportamiento del sandbox cambiado → false", "invariante cambiado: sandbox_behavior", props={"sandbox_behavior": "read-only+network"})
    incompatible("NEG-06", "A3.5", "aislamiento del contexto automático cambiado (entrada automática nueva) → false", "invariante cambiado: context_isolation",
                 props={"context_isolation": ("auto-inputs:global-memory", "closure:enumerated")})
    incompatible("NEG-07", "A3.5", "captura de la salida cambiada → false", "invariante cambiado: output_capture", props={"output_capture": ("text", "file", "events")})
    incompatible("NEG-08", "A3.5", "observabilidad de la terminación perdida → false", "invariante cambiado: termination_observability",
                 props={"termination_observability": "unobservable"})
    incompatible("NEG-09", "A3.5", "modo de consumo cambiado (créditos) → false", "cambiado: consumption_mode",
                 props={"consumption_mode": ("credits", "no-paid-key", "no-limit-warning")})
    incompatible("NEG-10", "A3.5", "clase de capacidad inferior → false", "cambiado: model_capability_class",
                 props={"model_capability_class": ("model-m", "Eficiente")}, reqs={"level": "Eficiente"})
    incompatible("NEG-11", "A3.5", "otro modelo en la misma celda → false", "cambiado: model_capability_class", props={"model_capability_class": ("model-n", "Equilibrado")})
    incompatible("NEG-79", "A3.5", "mismo modelo con una clase declarada superior: la clase es «igual», no «superior» → false", "cambiado: model_capability_class",
                 props={"model_capability_class": ("model-m", "Frontera")})
    incompatible("NEG-12", "A3.5", "semántica del effort cambiada (el valor pedido aplica otro) → false", "cambiado: effort_semantics",
                 props={"effort_semantics": ("Deep", "high", "xhigh")})
    incompatible("NEG-62", "A3.5", "la sonda observa otro transporte con el mismo descriptor (propiedad 2) → false", "cambiado: adapter_transport_provider",
                 props={"adapter_transport_provider": ("adapter-x", "stdio-bridge", "provider-p")})
    incompatible("NEG-35", "A3.4", "propiedad contradictoria → UNKNOWN → false, y además S-04", "UNKNOWN: output_capture", s04=True,
                 contradiction=("output_capture",))
    incompatible("NEG-88", "A3.5", "fila de requisito contradictoria (effort) → UNKNOWN → false, y además S-04", "requisitos obligatorios: UNKNOWN ['effort']",
                 s04=True, contradiction=("effort",))
    incompatible("NEG-46", "A3.5", "nivel de introspección por debajo del suelo → false", "por debajo del suelo: introspection_level",
                 props={"introspection_level": "CONFIGURED"})
    incompatible("NEG-47", "A3.4", "propiedad de la clase observada solo a nivel CONFIGURED: UNKNOWN → false", "UNKNOWN: termination_observability",
                 assurance={"termination_observability": "CONFIGURED"})
    incompatible("NEG-63", "A3.4", "propiedad de la clase solo pedida (REQUESTED): UNKNOWN → false", "UNKNOWN: context_isolation",
                 assurance={"context_isolation": "REQUESTED"})
    incompatible("NEG-64", "A3.4", "propiedad de la clase sin la fuente de la regla 4: UNKNOWN → false", "UNKNOWN (sin fuente declarada): sandbox_behavior",
                 nosource=("sandbox_behavior",))
    incompatible("NEG-48", "A3.5", "preflight del sucesor sin la fila tool-use: UNKNOWN → false", "requisitos obligatorios: UNKNOWN ['tool-use']",
                 drop=("tool-use",))
    incompatible("NEG-72", "A3.5", "la clasificación de procesos (operación 8) deja de estar DISPONIBLE → false", "operación no DISPONIBLE: 8",
                 ops={8: {"state": "UNVERIFIED", "observed": True}})
    incompatible("NEG-73", "A3.5", "una operación que la invocación de la sonda ejerce (7, terminación) queda sin observar: UNKNOWN → false",
                 "UNKNOWN: operación 7", ops={7: None})
    incompatible("NEG-91", "A3.5", "la sonda observa la cancelación (operación 6) y contradice la declaración DISPONIBLE del descriptor → false",
                 "operación no DISPONIBLE: 6", ops={6: {"state": "UNVERIFIED", "observed": True}})
    m = setup(cls); m.floors[A]["descriptor_ops"][6] = "UNVERIFIED"; m.observe(ID2)
    ok, causes = P(m, A, FA(), auth(ID2))
    case("NEG-92", "A3.5", "cancelación no ejercida por la sonda y sin declaración DISPONIBLE del descriptor: UNKNOWN → false",
         (not ok) and has(causes, "UNKNOWN: operación 6") and m.state[A] == "OBSERVATION_STALE", "; ".join(causes))
    m = setup(cls); m.floors[A]["props"]["context_isolation"] = None; m.observe(ID2)
    ok, causes = P(m, A, FA(), auth(ID2))
    case("NEG-44", "A3.4", "propiedad sin valor registrado en el suelo: UNKNOWN → false, aunque el sucesor la observe",
         (not ok) and has(causes, "sin valor en el suelo") and m.state[A] == "OBSERVATION_STALE", "; ".join(causes))
    m = setup(cls, level="Frontera"); m.observe(ID2)
    o = obs(make_floor("A", level="Frontera"), ID2, props={"model_capability_class": ("model-m", "Equilibrado")}, reqs={"level": "Equilibrado"})
    agg = aggregate(RuntimeModel._req_statuses(m, make_floor("A", level="Frontera"), o).values())
    ok, causes = P(m, A, o, auth(ID2))
    case("NEG-51", "A3.5", "suelo por encima del requisito y sucesor que baja al requisito: requisitos en MATCH, pero la clase cambia → false",
         agg == "MATCH" and (not ok) and has(causes, "cambiado: model_capability_class") and m.state[A] == "OBSERVATION_STALE", "; ".join(causes))

    # ---- negativos: sin aumento silencioso
    for cid, name, val in (("NEG-41", "permissions", "WORKSPACE_WRITE"), ("NEG-15", "write_scope", "REPOSITORY"),
                           ("NEG-16", "role_actions", frozenset({KEYS["A"][:2], KEYS["W"][:2]})), ("NEG-17", "capability_claim", "Frontera"),
                           ("NEG-18", "consumption_authority", 5)):
        m = setup(cls); m.observe(ID2); a = auth(ID2)
        ok, dd = rej(lambda: m.probe(A, FA(), a, requested={name: val}), "aumento silencioso: " + name)
        case(cid, "A3.8", "la sonda intenta subir %s: rechazada sin consumir" % name, ok and a["remaining"] == 1 and m.state[A] == "OBSERVATION_STALE", dd)
    for cid, name, val in (("NEG-74", "permissions", "WORKSPACE_WRITE"), ("NEG-75", "write_scope", "REPOSITORY"),
                           ("NEG-76", "role_actions", frozenset({KEYS["A"][:2], KEYS["W"][:2]})), ("NEG-77", "consumption_authority", 5),
                           ("NEG-87", "capability_claim", "Frontera")):
        m = setup(cls); m.observe(ID2); P(m, A, FA(), auth(ID2))
        ok, dd = rej(lambda: m.ordinary_invocation(A, requested={name: val}), "aumento silencioso: " + name)
        case(cid, "A3.8", "tras heredar, un trabajo que pide %s por encima del suelo: rechazado" % name, ok and m.state[A] == "INHERITED" and m.invocations == 0, dd)
    m = setup(cls); m.observe(ID2); okh, _ = P(m, A, FA(), auth(ID2))
    ok, dd = rej(lambda: m.ordinary_invocation(A, per_invocation=("closure", "P-23")), "controles por invocación sin renovar: ['P-24']")
    INV(m, A)
    case("NEG-89", "A3.9", "tras heredar, un trabajo sin el preflight de fidelidad de los insumos (P-24) renovado: rechazado; con los controles renovados, "
         "sigue", okh and ok and m.state[A] == "INHERITED" and m.invocations == 1, dd)
    m = setup(cls); m.observe(ID2)
    ok, dd = rej(lambda: m.probe(A, FA(), auth(ID2), read_only=False), "solo lectura")
    case("NEG-14", "A3.8", "sonda con escritura: rechazada", ok, dd)
    m = setup(cls, names=("W",)); m.observe(ID2)
    ok, causes = P(m, W, obs(make_floor("W"), ID2), auth(ID2))
    case("NEG-32", "A3.8", "acción de escritura con sonda de solo lectura: requisito de escritura UNKNOWN → false",
         (not ok) and has(causes, "write-commit-push") and m.state[W] == "OBSERVATION_STALE", "; ".join(causes))
    m = setup(cls); m.observe(ID2); P(m, A, FA(), auth(ID2))
    ok, dd = rej(lambda: m.ordinary_invocation(A, consumption_authorized=False), "consumo")
    case("NEG-36", "A3.8", "heredar ConsumptionCovered no autoriza consumo", ok and m.invocations == 0, dd)

    # ---- negativos: sin sonda, cota y autoridad
    m = setup(cls); m.observe(ID2)
    ok, dd = rej(lambda: m.ordinary_invocation(A), R_INVALID)
    case("NEG-19", "A3.2", "sucesor más nuevo sin sonda: ningún trabajo (más nuevo no es mejor)", ok, dd)
    m = setup(cls); m.observe(ID2); P(m, A, FA(reqs={"effort": "Balanced"}), auth(ID2))
    ok, dd = rej(lambda: m.probe(A, FA(), auth(ID2)), "como máximo una sonda")
    case("NEG-20", "A3.3", "segunda sonda para el mismo sucesor, rol, acción y celda (tras un fallo): rechazada", ok and m.state[A] == "OBSERVATION_STALE", dd)
    m = setup(cls); m.observe(ID2)
    ok, dd = rej(lambda: m.probe(A, FA(), auth(ID2, n=2), invocations=2), "como máximo una invocación")
    case("NEG-21", "A3.3", "sonda con dos invocaciones de modelo: rechazada", ok, dd)
    m = setup(cls); m.observe(ID2)
    ok1, d1 = rej(lambda: m.probe(A, FA(), None), "sin autoridad")
    ok2, d2 = rej(lambda: m.probe(A, FA(), auth(ID2, n=0)), "presupuesto")
    case("NEG-22", "A3.3", "sonda sin autoridad o con el presupuesto agotado: rechazada (P-07)", ok1 and ok2 and m.state[A] == "OBSERVATION_STALE", d1 + " / " + d2)
    m = setup(cls); m.observe(ID2)
    ok, dd = rej(lambda: m.probe(A, FA(ID3), auth(ID2)), "no observado")
    case("NEG-29", "A3.3", "sonda anticipada para una identidad todavía no observada (con autoridad del sucesor vigente): rechazada",
         ok and m.state[A] == "OBSERVATION_STALE", dd)
    m = setup(cls); m.observe(ID2)
    ok, causes = P(m, A, FA(), auth(ID2), complete=False)
    case("NEG-37", "A3.3", "sonda incompleta o interrumpida → false", (not ok) and m.state[A] == "OBSERVATION_STALE" and has(causes, "sonda incompleta"), "; ".join(causes))
    m = setup(cls); a2 = auth(ID2); m.observe(ID2); m.observe(ID3)
    ok, dd = rej(lambda: m.probe(A, FA(ID3), a2), "sin autoridad")
    case("NEG-39", "A3.3", "otra actualización tras la autorización: la autorización del par anterior no cubre el nuevo", ok, dd)
    m = setup(cls); m.observe(ID2)
    ok, causes = P(m, A, FA(), auth(ID2), fp_change_during=True)
    o2, _ = rej(lambda: m.ordinary_invocation(A), R_INVALID)
    case("NEG-78", "A3.3", "la huella cambia durante la sonda: incompleta y fuera de A-3 (P-01/P-11)",
         (not ok) and has(causes, "huella cambiada durante la sonda") and m.state[A] == "NON_SUCCESSOR" and o2, "; ".join(causes))
    m = setup(cls); m.observe(ID2)
    a = auth(ID2, n=2)
    env_lost = dict(ENV1, AuthState="NOT_AUTHENTICATED")
    ok, causes = P(m, A, FA(env=env_lost, props={"authentication": ("NOT_AUTHENTICATED", "subscription")}), a)
    o2, _ = rej(lambda: m.ordinary_invocation(A), R_INVALID)
    again, dd = rej(lambda: m.probe(A, FA(), a), R_NONSUCC)
    case("NEG-90", "A3.3", "la observación del sucesor coincide con el suelo y el estado de autenticación cambia dentro de la sonda: la sonda cuenta "
         "como hecha (false, sin herencia, bloqueada) y el caso queda fuera de A-3",
         (not ok) and has(causes, "no es sucesor (entorno)") and a["remaining"] == 1 and bool(m.records) and m.records[-1]["Compatible"] is False
         and m.failed_since.get(A) is True and m.state[A] == "NON_SUCCESSOR" and o2 and again, "; ".join(causes) + " / " + dd)

    # ---- negativos: sucesor (fuera de A-3, sin consumir sonda)
    not_successor("NEG-03", "A3.1", "estado de autenticación perdido (invalidador de B.4): no es sucesor; sin sonda",
                  observe_kw={"env": dict(ENV1, AuthState="NOT_AUTHENTICATED")},
                  obs_kw={"env": dict(ENV1, AuthState="NOT_AUTHENTICATED"), "props": {"authentication": ("NOT_AUTHENTICATED", "subscription")}})
    not_successor("NEG-13", "A3.1", "otro proveedor: no es sucesor; sin sonda", observe_kw={"env": dict(ENV1, Provider="provider-q")},
                  obs_kw={"env": dict(ENV1, Provider="provider-q"), "props": {"adapter_transport_provider": ("adapter-x", "external-process", "provider-q")}})
    not_successor("NEG-61", "A3.1", "otro AdapterId con el mismo proveedor: no es sucesor; sin sonda", observe_kw={"env": dict(ENV1, AdapterId="adapter-z")},
                  obs_kw={"env": dict(ENV1, AdapterId="adapter-z"), "props": {"adapter_transport_provider": ("adapter-z", "external-process", "provider-p")}})
    for cid, k, v, desc in (("NEG-25", "HostInstanceHash", "h2", "otra instancia de host"), ("NEG-26", "CatalogEntryBlob", "c2", "otro blob de catálogo"),
                            ("NEG-55", "RoutingBlob", "r2", "otra versión de routing.md"), ("NEG-56", "DescriptorBlob", "d2", "otro blob de descriptor")):
        env = dict(ENV1, **{k: v})
        not_successor(cid, "A3.1", "%s: no es sucesor; sin sonda (medición completa)" % desc, observe_kw={"env": env}, obs_kw={"env": env})
    m = setup(cls); m.observe(ID1, fingerprint="FP2")
    a = auth(ID1, owner=True)
    ok, dd = rej(lambda: m.probe(A, FA(ID1), a), R_NONSUCC)
    o2, _ = rej(lambda: m.ordinary_invocation(A), R_INVALID)
    case("NEG-66", "A3.1", "solo cambia la huella, con la identidad igual: fuera de A-3 (§6, P-01/P-11); observación invalidada; sin sonda",
         ok and a["remaining"] == 1 and m.state[A] == "NON_SUCCESSOR" and o2, dd)
    not_successor("NEG-67", "A3.1", "cambian a la vez la identidad y la huella: fuera de A-3", observe_kw={"fingerprint": "FP2"}, probe_auth=auth(ID2, owner=True))
    not_successor("NEG-68", "A3.1", "cambio observado durante un intento en curso: fuera de A-3", observe_kw={"in_attempt": True})
    not_successor("NEG-70", "A3.1", "el binario medido sigue presente y se elige otro más nuevo junto a él: no es sucesor", observe_kw={"replaces": False})
    not_successor("NEG-71", "A3.1", "invalidador sin valor (huella UNKNOWN, sin declarar) en las dos observaciones: no es sucesor",
                  setup_kw={"fp": "UNKNOWN", "accepted": ()}, probe_auth=auth(ID2, owner=True))
    for cid, k in (("NEG-84", "CatalogEntryBlob"), ("NEG-85", "RoutingBlob"), ("NEG-86", "DescriptorBlob")):
        env = dict(ENV1, **{k: UNK})
        not_successor(cid, "A3.1", "%s sin valor (UNKNOWN) en el suelo y en el sucesor: no es sucesor; sin sonda" % k, setup_kw={"env": env}, obs_kw={"env": env})
    not_successor("NEG-80", "A3.1", "BinaryHash, con fuente declarada, sin valor observado en el sucesor mientras cambia AppVersion: no es sucesor; "
                  "sin sonda ni herencia", ident=dict(ID_APP, BinaryHash=UNK))
    not_successor("NEG-81", "A3.1", "el único cambio es que AppVersion, con fuente declarada, pasa a UNKNOWN: fuera de A-3, observación invalidada; "
                  "sin sonda ni trabajo", ident=dict(ID1, AppVersion=UNK))
    m = setup(cls, env=dict(ENV1, HostInstanceState="UNOBSERVED")); m.observe(ID2)
    a = auth(ID2)
    ok, dd = rej(lambda: m.probe(A, obs(make_floor("A", env=dict(ENV1, HostInstanceState="UNOBSERVED")), ID2), a), "instancia de host UNOBSERVED")
    case("NEG-45", "A3.1", "suelo y sucesor con HostInstanceState UNOBSERVED: no es sucesor; sin consumir la sonda",
         ok and a["remaining"] == 1 and m.state[A] == "OBSERVATION_STALE" and not m.records, dd)
    m = setup(cls); m.observe(ID2, during_cession=True)
    ok1, d1 = rej(lambda: m.probe(A, FA(), auth(ID2)), "cesión")
    ok2, _ = rej(lambda: m.ordinary_invocation(A), "S-04")
    case("NEG-27", "A3.1", "cambio observado durante una cesión: fuera de A-3 (S-04/P-11)", ok1 and ok2, d1)
    m = setup(cls); m.observe(ID2, stable=False)
    ok, dd = rej(lambda: m.probe(A, FA(), auth(ID2)), "no estable")
    case("NEG-28", "A3.1", "observación durante una actualización a medio completar: no fija el sucesor", ok, dd)
    m = setup(cls); m.observe(ID2)
    ok, dd = rej(lambda: m.probe(W, obs(make_floor("W"), ID2), auth(ID2)), "sin suelo")
    case("NEG-30", "A3.5", "sin suelo medido de esa unidad, rol, acción y celda: nada que heredar", ok, dd)
    for cid, ident, what in (("NEG-49", ID_BIN, "el hash del binario"), ("NEG-50", ID_PATH, "la ruta del binario"),
                             ("NEG-82", ID_APP, "AppVersion (que B.4 no nombra)"), ("NEG-83", ID_ADP, "AdapterVersion")):
        m = setup(cls); m.observe(ident)
        ok, dd = rej(lambda: m.ordinary_invocation(A), R_INVALID)
        case(cid, "A3.2", "solo cambia %s: la observación queda invalidada y no hay trabajo" % what, ok and m.state[A] == "OBSERVATION_STALE", dd)

    # ---- negativos: resultado falso, alcance de la sonda fallida y medición
    m = setup(cls); m.observe(ID2)
    ok, causes = P(m, A, FA(reqs={"effort": "Balanced"}), auth(ID2))
    okp, dp = rej(lambda: m.measure_from_probe(A, auth(ID2)), "P-10")
    okn, dn = rej(lambda: m.full_measurement(make_floor("A", identity=ID2), authority=False), "sin autoridad")
    m.full_measurement(make_floor("A", identity=ID2), authority=True); INV(m, A)
    case("NEG-31", "A3.7", "sonda fallida por BELOW_REQUIRED: no cuenta como medición elegible (P-10); solo una medición autorizada restaura la elegibilidad",
         (not ok) and has(causes, "BELOW_REQUIRED") and okp and okn and m.state[A] == "VALID" and m.invocations == 1, dp + " / " + dn)
    m = setup(cls); m.observe(ID2); P(m, A, FA(reqs={"effort": "Balanced"}), auth(ID2)); m.observe(ID3)
    ok, dd = rej(lambda: m.probe(A, FA(ID3), auth(ID3)), "tras una sonda fallida")
    case("NEG-42", "A3.7", "tras una sonda fallida, un sucesor posterior no hereda sin medición completa", ok, dd)
    m = setup(cls); m.observe(ID2); P(m, A, FA(reqs={"effort": "Balanced"}), auth(ID2))
    case("NEG-43", "A3.7", "una sonda fallida no acredita nada: MeasuredInvocation NOT_MEASURED y RunRef nulo",
         m.elig[A] == {"MeasuredInvocation": "NOT_MEASURED", "RunRef": None})
    m = setup(cls, names=("A", "A2")); m.observe(ID2)
    okA, _ = P(m, A, FA(), auth(ID2), complete=False)
    okA2, _ = P(m, A2, obs(make_floor("A2"), ID2), auth(ID2))
    again, dd = rej(lambda: m.probe(A, FA(), auth(ID2)), "como máximo una sonda")
    case("NEG-54", "A3.7", "la sonda fallida de (ARCHITECT, REVIEW, c) no bloquea la de (ARCHITECT, RE_REVIEW, c); repetir la primera sí se rechaza",
         (not okA) and okA2 and m.state[A2] == "INHERITED" and again, dd)
    m = setup(cls, names=("A", "R")); m.observe(ID2)
    okA, _ = P(m, A, FA(), auth(ID2))
    before, dd = rej(lambda: m.ordinary_invocation(R), R_INVALID)
    okR, _ = P(m, R, obs(make_floor("R"), ID2), auth(ID2))
    after = INV(m, R)
    case("NEG-57", "A3.6", "dos roles con la misma acción y celda: la herencia del ARCHITECT no vale para el REVIEWER, que necesita su propia sonda",
         okA and before and okR and after, dd)

    # ---- negativos: huella, OD-2 y frescura
    m = setup(cls, fp="FP2", accepted=("FP1",)); m.observe(ID2)
    ok, _ = P(m, A, FA(), auth(ID2, owner=True))
    okr, dd = rej(lambda: m.ordinary_invocation(A), R_FP)
    case("NEG-23", "A3.9", "compatible con la huella vigente sin aceptar y sin OD-2: sin trabajo", ok and okr and m.fp_unaccepted(), dd)
    m = setup(cls)
    ok, dd = rej(lambda: m.accept_od2("FP2", "COORDINATOR"), "solo el Owner")
    case("NEG-24", "A3.9", "la aceptación de la huella sigue siendo del Owner (OD-2)", ok, dd)
    m = setup(cls, fp="FP2", accepted=("FP1",)); m.observe(ID2)
    ok, dd = rej(lambda: m.probe(A, FA(), auth(ID2, owner=False)), "Owner")
    case("NEG-38", "A3.9", "con la huella vigente sin aceptar, la sonda exige una autorización expresa del Owner", ok and m.state[A] == "OBSERVATION_STALE", dd)
    m = setup(cls, fp="FP2", accepted=("FP1",)); m.observe(ID2); P(m, A, FA(), auth(ID2, owner=True)); m.accept_od2("FP2", "OWNER"); INV(m, A)
    m.observe(ID2, fingerprint="FP3")
    m.full_measurement(make_floor("A", identity=ID2), authority=True, owner=True)
    ok, dd = rej(lambda: m.ordinary_invocation(A), R_FP)
    case("NEG-52", "A3.9", "tras una aceptación OD-2, otra huella (medida bajo autorización del Owner) sigue sin aceptar: sin trabajo",
         ok and m.invocations == 1 and m.state[A] == "VALID", dd)
    m = setup(cls, fp="FP2", accepted=("FP1",)); m.accept_od2("FP9", "OWNER")
    ok, dd = rej(lambda: m.ordinary_invocation(A), R_FP)
    case("NEG-53", "A3.9", "el Owner acepta otra huella (FP9): la vigente (FP2) sigue sin aceptar; sin trabajo", ok and m.fp_unaccepted(), dd)
    m = setup(cls, env=ENV_Y, fp="FY1", accepted=("FY1",)); m.observe(ID1, fingerprint="FY2")
    a = auth(ID1, owner=True)
    okp, _ = rej(lambda: m.probe(A, obs(make_floor("A", env=ENV_Y), ID1), a), R_NONSUCC)
    m.full_measurement(make_floor("A", env=ENV_Y), authority=True, owner=True)
    blocked, dd = rej(lambda: m.ordinary_invocation(A), R_FP)
    m.accept_od2("FY2", "OWNER")
    case("NEG-69", "A3.9", "cambio de huella en otro adapter (huella distinta de la de P-01): fuera de A-3, y sin trabajo hasta OD-2 de la huella exacta",
         okp and a["remaining"] == 1 and blocked and INV(m, A), dd)
    m = setup(cls, verified="2026-06-01"); m.observe(ID2)
    ok, causes = P(m, A, FA(), auth(ID2))
    okr, _ = rej(lambda: m.ordinary_invocation(A), R_INVALID)
    case("NEG-34", "A3.5", "suelo ya Stale (frescura del catálogo) en la fecha de la sonda: no hay herencia ni se refresca", (not ok) and okr and has(causes, R_STALE),
         "; ".join(causes))
    m = setup(cls); m.observe(ID2); m.advance(85)
    ok, causes = P(m, A, FA(), auth(ID2))
    case("NEG-58", "A3.5", "el suelo pasa a Stale entre la medición y la sonda: false en la fecha de la sonda", (not ok) and has(causes, R_STALE), "; ".join(causes))
    m = setup(cls); m.observe(ID2); ok, _ = P(m, A, FA(), auth(ID2)); m.advance(85)
    okr, dd = rej(lambda: m.ordinary_invocation(A), R_STALE)
    case("NEG-59", "A3.6", "la celda pasa a Stale entre la sonda y el binding: la herencia no la mantiene vigente; sin trabajo", ok and okr and m.invocations == 0, dd)
    m = setup(cls); m.observe(ID2); ok, _ = P(m, A, FA(), auth(ID2)); m.retirement = TODAY + datetime.timedelta(days=12); m.advance(15)
    okr, dd = rej(lambda: m.ordinary_invocation(A), R_STALE)
    case("NEG-65", "A3.6", "retiro anunciado tras la herencia: Stale en la fecha del binding; sin trabajo", ok and okr and m.invocations == 0, dd)
    m = setup(cls); m.observe(ID2); P(m, A, FA(), auth(ID2)); m.observe(ID3)
    ok, dd = rej(lambda: m.ordinary_invocation(A), R_INVALID)
    case("NEG-33", "A3.2", "otra actualización tras heredar: la herencia deja de valer para la identidad nueva hasta otra sonda", ok, dd)
    m = setup(cls, names=("A", "A2")); m.observe(ID2); P(m, A, FA(), auth(ID2))
    ok, dd = rej(lambda: m.ordinary_invocation(A2), R_INVALID)
    case("NEG-40", "A3.6", "la herencia vale solo para esa acción: otra acción de la misma celda sigue invalidada", ok, dd)

    # ---- regla 11 (S-01): solo el valor exacto; sin verificador, sin aceptación por clase y sin lectura de valores; evidencia solo por nombre
    # (los identificadores POS-17 a POS-19 y NEG-93 a NEG-117 del verificador retirado no se reutilizan)
    v1, v2 = syn_cfg(ID1), syn_cfg(ID2, **{"ui.theme": "dark"})  # la actualización cambia valores de versión y añade una clave
    b_obs, a_obs = fp_obs(ID1, v1), fp_obs(ID2, v2)
    m = setup(cls, fp=b_obs["Fingerprint"], accepted=(b_obs["Fingerprint"],))
    m.observe(ID2, fingerprint=a_obs["Fingerprint"])
    b_obs["File"].reads = a_obs["File"].reads = 0
    try:
        rec = m.record_fp_change(b_obs, a_obs)
    except Reject as e:
        rec = {"Rechazo": str(e)}
    no_reads = b_obs["File"].reads == 0 and a_obs["File"].reads == 0 and m.value_reads == 0  # M6-02: registrar no lee valores en el host
    want = {"FingerprintBefore": b_obs["Fingerprint"], "FingerprintAfter": a_obs["Fingerprint"], "FingerprintStable": True, "IdentityBefore": ID1,
            "IdentityAfter": ID2, "KeysAdded": ["ui.theme"], "KeysRemoved": [], "StructureEqual": False}
    txt = json.dumps({k: x for k, x in rec.items() if k not in ("IdentityBefore", "IdentityAfter")}, ensure_ascii=False, default=sorted)
    leak = sorted({x for x in list(v1.values()) + list(v2.values()) if x in txt})  # ningún valor fuera de los hechos de identidad
    a = auth(ID2, owner=True)
    okp, dp = rej(lambda: m.probe(A, FA(), a), R_NONSUCC)
    okw, _ = rej(lambda: m.ordinary_invocation(A), R_INVALID)
    unacc = m.fp_unaccepted()
    m.accept_od2(a_obs["Fingerprint"], "OWNER")
    m.full_measurement(make_floor("A", identity=ID2), authority=True)
    case("POS-20", "A3.11", "un evento de sucesor coincide con un cambio de huella: la evidencia registra solo la lista cerrada por nombre (hash exacto "
         "antes y después con su estabilidad, hechos de identidad del runtime antes y después, nombres saneados añadidos o eliminados y estructura), "
         "sin valores, sin digests por clave y sin localizar por clave los cambios de valor; la huella sigue sin aceptar (P-01), sin sonda ni trabajo, "
         "hasta la OD-2 exacta del Owner y una medición nueva",
         rec == want and set(rec) == EVIDENCE_FIELDS and not leak and no_reads and m.fp_evidence == [want] and okp and a["remaining"] == 1 and okw and unacc
         and m.state[A] == "VALID" and INV(m, A), "%d valores en la evidencia; campos %s / %s" % (len(leak), sorted(rec), dp))

    def evidence_case(cid, desc, after_obs, after_values, want_fields):  # M5-02 y M5-03: otras formas del cambio de huella, con la misma lista cerrada
        m = setup(cls, fp=b_obs["Fingerprint"], accepted=(b_obs["Fingerprint"],))
        m.observe(ID2, stable=after_obs["Stable"], fingerprint=after_obs["Fingerprint"])
        b_obs["File"].reads = after_obs["File"].reads = 0
        try:
            r = m.record_fp_change(b_obs, after_obs)
        except Reject as e:
            r = {"Rechazo": str(e)}
        no_reads = b_obs["File"].reads == 0 and after_obs["File"].reads == 0 and m.value_reads == 0  # M6-02: registrar no lee valores en el host
        w = dict({"FingerprintBefore": b_obs["Fingerprint"], "FingerprintAfter": after_obs["Fingerprint"], "IdentityBefore": ID1, "IdentityAfter": ID2},
                 **want_fields)
        t = json.dumps({k: x for k, x in r.items() if k not in ("IdentityBefore", "IdentityAfter")}, ensure_ascii=False, default=sorted)
        lk = sorted({x for x in list(v1.values()) + list(after_values.values()) if x in t})
        unacc_after_record = m.fp_unaccepted()  # registrar la evidencia nunca acepta la huella
        a = auth(ID2, owner=True)
        okp, dp = rej(lambda: m.probe(A, FA(), a), R_NONSUCC)
        okw, _ = rej(lambda: m.ordinary_invocation(A), R_INVALID)
        st = m.state[A]
        # M6-01: la ventana real de F6 (medición autorizada por el Owner y OD-2 después): sin el Owner no hay medición, y con la medición, pero con la
        # huella sin aceptar, sigue sin haber trabajo (P-01); solo la OD-2 exacta del Owner lo devuelve
        okm, dm = rej(lambda: m.full_measurement(make_floor("A", identity=ID2), authority=True, owner=False), "Owner")
        m.full_measurement(make_floor("A", identity=ID2), authority=True, owner=True)
        okfp, dfp = rej(lambda: m.ordinary_invocation(A), R_FP)
        no_work = m.invocations == 0
        m.accept_od2(after_obs["Fingerprint"], "OWNER")
        case(cid, "A3.11", desc, r == w and set(r) == EVIDENCE_FIELDS and not lk and no_reads and m.fp_evidence == [w] and unacc_after_record and okp
             and a["remaining"] == 1 and not m.records and okw and st == "NON_SUCCESSOR" and okm and okfp and no_work and INV(m, A),
             "%d valores en la evidencia; campos %s / %s / %s / %s" % (len(lk), sorted(r), dp, dm, dfp))

    v2s = syn_cfg(ID2)  # solo cambian valores (los de versión y de binario), con los mismos nombres: el caso real de las actualizaciones medidas
    evidence_case("POS-21", "un evento de sucesor coincide con un cambio de huella solo de valores, con la estructura igual (el caso de las "
                  "actualizaciones medidas): la evidencia es la misma lista cerrada por nombre (ningún nombre añadido ni eliminado, estructura igual), "
                  "sin localizar por clave los cambios de valor y sin digests por clave; registrarla no lee valores en el host ni acepta la huella (P-01); no "
                  "hay sonda (no se consume) ni herencia; sin el Owner no hay medición, y con la medición del Owner y la huella sin aceptar sigue sin "
                  "haber trabajo hasta la OD-2 exacta", fp_obs(ID2, v2s), v2s,
                  {"FingerprintStable": True, "KeysAdded": [], "KeysRemoved": [], "StructureEqual": True})
    v3 = syn_cfg(ID2, **{"ui.layout": None, "ui.theme": "dark"})  # un nombre eliminado y otro añadido (mismo número de nombres), a medio completar
    evidence_case("POS-22", "un evento de sucesor coincide con un cambio de huella observado inestable que elimina un nombre y añade otro (mismo "
                  "número de nombres): la evidencia registra la estabilidad observada (false), el nombre añadido, el eliminado y la estructura distinta; "
                  "sin valores ni lectura de valores en el host, sin aceptar la huella y sin sonda; sin trabajo con la medición del Owner hasta la OD-2 "
                  "exacta", fp_obs(ID2, v3, stable=False), v3,
                  {"FingerprintStable": False, "KeysAdded": ["ui.theme"], "KeysRemoved": ["ui.layout"], "StructureEqual": False})
    m = setup(cls)
    m.observe(ID2, fingerprint="FP2")
    a = auth(ID2, owner=True)
    okp, dp = rej(lambda: m.probe(A, FA(), a), R_NONSUCC)
    okw, _ = rej(lambda: m.ordinary_invocation(A), R_INVALID)
    unacc, st = m.fp_unaccepted(), m.state[A]
    m.accept_od2("FP2", "OWNER")
    a2 = auth(ID2, owner=True)  # M5-01: con la huella nueva ya aceptada, el caso sigue fuera de A-3: tampoco hay sonda
    okp2, dp2 = rej(lambda: m.probe(A, FA(), a2), R_NONSUCC)
    after, d1 = rej(lambda: m.ordinary_invocation(A), R_INVALID)
    m.full_measurement(make_floor("A", identity=ID2), authority=True)
    case("NEG-118", "A3.11", "un evento de sucesor coincide con un cambio de huella: no hay herencia ni sonda (no se consume) ni trabajo; P-01 se "
         "conserva (la actualización no deja aceptada la huella nueva), y la OD-2 exacta del Owner sola no devuelve ni la sonda ni el trabajo: el "
         "caso sigue fuera de A-3 (sin sonda ni consumo, también con la huella ya aceptada) y hace falta una medición nueva (§5, paso 3)",
         okp and a["remaining"] == 1 and okp2 and a2["remaining"] == 1 and not m.records and okw and unacc and st == "NON_SUCCESSOR" and after
         and INV(m, A), dp + " / " + dp2 + " / " + d1)
    m = setup(cls)
    reqs = ({"by": "COORDINATOR", "mode": "MATERIAL"}, {"by": "OWNER", "an_agreed": False, "accept": "class"},
            {"by": "OWNER", "an_agreed": True, "accept": "class"})
    res = [rej(lambda x=x: m.request_fp_acceptance_mode(x), "A-3 no lo define") for x in reqs]
    m.observe(ID2, fingerprint="FP2")
    a = auth(ID2, owner=True)
    okp, dp = rej(lambda: m.probe(A, FA(), a), R_NONSUCC)
    case("NEG-119", "A3.11", "petición de aceptar huellas por clase o de otro modo de OD-2 (del Coordinator, del Owner sin A-n o del Owner con una "
         "A-n): A-3 no lo define y la rechaza; la huella se sigue comparando por su valor exacto y el cambio de huella con el de identidad sigue "
         "siendo P-01, sin sonda", all(r[0] for r in res) and m.od2_mode == "EXACT" and okp and a["remaining"] == 1 and m.fp_unaccepted(),
         " / ".join([r[1] for r in res] + [dp]))
    m = setup(cls)
    reqs = ({"by": "OWNER", "kind": "PER_KEY_LOCALIZATION", "program_blob": "b" * 40, "surfaces": ("config",), "expires": "2026-12-31"},
            {"by": "COORDINATOR", "kind": "PER_KEY_COMPARISON", "order": "medición pasiva"},
            {"by": "OWNER", "kind": "VALUE_CLASSIFICATION", "an_agreed": True, "program_blob": "c" * 40, "surfaces": ("config",),
             "expires": "2026-12-31"})
    res = [rej(lambda x=x: m.request_value_reading(x), "A-3 no la define") for x in reqs]
    case("NEG-120", "A3.11", "petición, bajo A-3, de localizar por clave los cambios de valor, de compararlos por clave en una medición pasiva o de "
         "clasificar claves por sus valores (también del Owner, con el programa, su blob, las superficies y la caducidad nombrados): A-3 no define "
         "ninguna y la rechaza; no se lee ningún valor", all(r[0] for r in res) and m.value_reads == 0, " / ".join(r[1] for r in res))
    m = setup(cls, accepted=("FP1", "FP2"))  # M5-01: una huella ya aceptada por OD-2 no hace igual la huella (HuellaEq solo por el valor exacto)
    m.observe(ID2, fingerprint="FP2")
    a = auth(ID2, owner=True)
    okp, dp = rej(lambda: m.probe(A, FA(), a), R_NONSUCC)
    okw, _ = rej(lambda: m.ordinary_invocation(A), R_INVALID)
    case("NEG-121", "A3.11", "cambian a la vez la identidad del runtime y la huella, y la huella nueva ya estaba aceptada por OD-2: la huella se "
         "compara solo por su valor exacto con la del suelo, así que no es sucesor; sin sonda (no se consume), sin herencia y sin trabajo",
         okp and a["remaining"] == 1 and m.state[A] == "NON_SUCCESSOR" and not m.records and okw, dp)
    return out


# ---------------------------------------------------------------- mutantes (cada uno debe hacer fallar un vector esperado, también a ciegas del motivo)
def _skip(*props):
    class M(RuntimeModel):
        def _check_prop(self, k, kind, x, fv, f):
            return None if k in props else super()._check_prop(k, kind, x, fv, f)
    M.__name__ = "M_Ignore_" + "_".join(props)
    return M


def _env_ignored(name, *keys):
    class M(RuntimeModel):
        def observe(self, identity, env=None, stable=True, during_cession=False, in_attempt=False, fingerprint=None, replaces=True):
            if env is not None:
                env = dict(env, **{k: self.env[k] for k in keys})
            super().observe(identity, env, stable, during_cession, in_attempt, fingerprint, replaces)

        def _env_differs(self, f, o):
            return super()._env_differs(f, dict(o, env=dict(o["env"], **{k: f["env"][k] for k in keys})))
    M.__name__ = name
    return M


def _inherit_raises(dim, value):
    class M(RuntimeModel):
        def _inherit(self, key, run, o):
            super()._inherit(key, run, o)
            self.claims[key][dim] = value
    M.__name__ = "M_InheritRaises_" + dim
    return M


class M_NewerIsBetter(RuntimeModel):
    def observe(self, identity, env=None, stable=True, during_cession=False, in_attempt=False, fingerprint=None, replaces=True):
        newer = UNK not in (identity["AppVersion"], self.identity["AppVersion"]) and vtuple(identity["AppVersion"]) > vtuple(self.identity["AppVersion"]) \
            and (env is None or env == self.env)
        before = dict(self.state)
        super().observe(identity, env, stable, during_cession, in_attempt, fingerprint, replaces)
        if newer:
            for k, s in before.items():
                if s in ("VALID", "INHERITED"):
                    self.state[k], self.elig[k] = s, copy.deepcopy(self.floors[k]["eligibility"])


class M_IgnoreUnknown(RuntimeModel):
    def _observed(self, x):
        return x is not None and x["value"] is not None

    def _req_status(self, rid, x, req):
        return "MATCH" if rid in WRITE_REQS else super()._req_status(rid, x, req)


class M_LaxPropAssurance(RuntimeModel):
    def _check_prop(self, k, kind, x, fv, f):
        if x is not None and x.get("source") is not None:
            x = dict(x, assurance="RUNTIME_OBSERVED")
        return super()._check_prop(k, kind, x, fv, f)


class M_NoSourceAccepted(RuntimeModel):
    def _check_prop(self, k, kind, x, fv, f):
        if x is not None and x.get("source") is None:
            x = dict(x, source="declared")
        return super()._check_prop(k, kind, x, fv, f)


class M_MissingRequirementRowIsMatch(RuntimeModel):
    def _req_status(self, rid, x, req):
        return "MATCH" if x is None and rid not in WRITE_REQS else super()._req_status(rid, x, req)


class M_SkipProbe(RuntimeModel):
    def ordinary_invocation(self, key, **kw):
        if self.state.get(key) == "OBSERVATION_STALE":
            self.state[key], self.elig[key] = "INHERITED", copy.deepcopy(self.floors[key]["eligibility"])
        super().ordinary_invocation(key, **kw)


class M_UnboundedRetry(RuntimeModel):  # sin cota ni bloqueo tras el fallo (sustituye a M_UnboundedProbe, equivalente por resultado)
    def probe(self, key, o, authority, **kw):
        self.probed.discard(self._tag(key))
        return super().probe(key, o, authority, **kw)

    def _check_failed_since(self, key):
        return None


class M_RetryAfterFail(RuntimeModel):
    def _check_failed_since(self, key):
        return None


class M_PerCellLockout(RuntimeModel):
    def _check_failed_since(self, key):
        if any(v and k[2] == key[2] for k, v in self.failed_since.items()):
            raise Reject("tras una sonda fallida: medición completa")


class M_MultiInvocation(RuntimeModel):
    def probe(self, key, o, authority, invocations=1, **kw):
        return super().probe(key, o, authority, invocations=1, **kw)


class M_WritableProbe(RuntimeModel):
    def probe(self, key, o, authority, read_only=True, **kw):
        return super().probe(key, o, authority, read_only=True, **kw)


class M_ClaimIncrease(RuntimeModel):
    def _check_requested(self, key, requested):
        return None


class M_WorkIgnoresRequested(RuntimeModel):
    def _check_claims(self, key, requested):
        return super()._check_claims(key, None)


class M_InheritAcrossActions(RuntimeModel):
    def _inherit(self, key, run, o):
        super()._inherit(key, run, o)
        for k in self.floors:
            if k[2] == key[2]:
                self.state[k], self.elig[k] = "INHERITED", copy.deepcopy(self.elig[key])


class M_CompatImpliesOD2(RuntimeModel):
    def _inherit(self, key, run, o):
        super()._inherit(key, run, o)
        self.accepted.add(self.fp)


class M_CoordinatorAcceptsOD2(RuntimeModel):
    def accept_od2(self, fp, by):
        self.accepted.add(fp)


class M_OD2Sticky(RuntimeModel):
    def accept_od2(self, fp, by):
        super().accept_od2(fp, by)
        self._od2_done = True

    def fp_unaccepted(self):
        return False if getattr(self, "_od2_done", False) else super().fp_unaccepted()


class M_OD2NotExact(RuntimeModel):
    def accept_od2(self, fp, by):
        super().accept_od2(fp, by)
        self.accepted.add(self.fp)


class M_FpGateOnlyForOneAdapter(RuntimeModel):  # P-01 leído como propio de un solo adapter
    def fp_unaccepted(self):
        return super().fp_unaccepted() if self.env.get("AdapterId") == "adapter-x" else False


class M_EffortAsFloor(RuntimeModel):
    def _check_prop(self, k, kind, x, fv, f):
        if k == "effort_semantics" and x is not None and self._observed(x) and fv is not None:
            v = x["value"]
            return None if v[:2] == fv[:2] and EFFORT_VALUES.index(v[2]) >= EFFORT_VALUES.index(fv[2]) else "cambiado: " + k
        return super()._check_prop(k, kind, x, fv, f)


class M_CapabilityAsFloor(RuntimeModel):  # «mismo modelo; clase igual o superior» (sustituye a M_InheritObservedCapability, equivalente con la clase igual)
    def _check_prop(self, k, kind, x, fv, f):
        if k == "model_capability_class" and x is not None and self._observed(x) and fv is not None:
            v = x["value"]
            return None if v[0] == fv[0] and LEVEL.index(v[1]) >= LEVEL.index(fv[1]) else "cambiado: " + k
        return super()._check_prop(k, kind, x, fv, f)


class M_ClassVsRequirement(RuntimeModel):
    def _check_prop(self, k, kind, x, fv, f):
        if k == "model_capability_class" and x is not None and self._observed(x) and fv is not None:
            v = x["value"]
            return None if v[0] == fv[0] and LEVEL.index(v[1]) >= LEVEL.index(f["required"]["level"]) else "cambiado: " + k
        return super()._check_prop(k, kind, x, fv, f)


class M_IdentityFactsCompared(RuntimeModel):  # lectura literal del borrador anterior: identidad del runtime en la clase, comparada por igualdad
    def _extra_identity_causes(self, f, o):
        return ["cambiado: identidad del runtime"] if o["identity"] != f["identity"] else []


class M_RefreshFreshness(RuntimeModel):
    def _inherit(self, key, run, o):
        super()._inherit(key, run, o)
        self.elig[key]["CatalogVerifiedOn"] = self.today.isoformat()


class M_StaleCopied(RuntimeModel):  # copia Stale del suelo en la herencia en lugar de recalcularlo
    def _inherit(self, key, run, o):
        super()._inherit(key, run, o)
        self.elig[key]["Stale"] = self._floor_stale_at_probe(self.floors[key])

    def _binding_stale(self, key):
        return self.elig[key]["Stale"] if "Stale" in self.elig[key] else super()._binding_stale(key)


class M_StaleNotAtProbe(RuntimeModel):  # evalúa la frescura del suelo en la fecha de la medición, no en la de la sonda
    def _floor_stale_at_probe(self, f):
        return self._stale_on(f["eligibility"]["CatalogVerifiedOn"], f["measured_on"])


class M_StaleFloorInherits(RuntimeModel):
    def _floor_stale_at_probe(self, f):
        return False


class M_NoAuthorityCheck(RuntimeModel):
    def _check_authority(self, authority, invocations):
        return None

    def probe(self, key, o, authority, **kw):
        return super().probe(key, o, authority if authority is not None else {"identity": self.identity, "remaining": 99, "owner": True}, **kw)


class M_AuthorityIdentityIgnored(RuntimeModel):
    def _check_authority(self, authority, invocations):
        if authority is None or authority.get("remaining", 0) < invocations:
            raise Reject("sin autoridad o presupuesto vigentes (P-07)")


class M_CessionIgnored(RuntimeModel):
    def observe(self, identity, env=None, stable=True, during_cession=False, in_attempt=False, fingerprint=None, replaces=True):
        super().observe(identity, env, stable, False, in_attempt, fingerprint, replaces)


class M_IgnoreAttemptInProgress(RuntimeModel):
    def observe(self, identity, env=None, stable=True, during_cession=False, in_attempt=False, fingerprint=None, replaces=True):
        super().observe(identity, env, stable, during_cession, False, fingerprint, replaces)


class M_NewerSelectionIsSuccessor(RuntimeModel):
    def observe(self, identity, env=None, stable=True, during_cession=False, in_attempt=False, fingerprint=None, replaces=True):
        super().observe(identity, env, stable, during_cession, in_attempt, fingerprint, True)


class M_FingerprintOnlyIsSuccessor(RuntimeModel):
    def _successor_change(self, identity, env, fp, during_cession, in_attempt, replaces):
        return env == self.env and not during_cession and not in_attempt and replaces and self._env_known(env, fp)

    def evaluate(self, f, o, complete=True):
        ok, causes, st = super().evaluate(f, o, complete)
        causes = [c for c in causes if c not in ("no es sucesor (huella)", "no es sucesor (identidad sin cambio)")]
        return not causes, causes, st


class M_FingerprintCoChangeIsSuccessor(RuntimeModel):
    def _successor_change(self, identity, env, fp, during_cession, in_attempt, replaces):
        return identity != self.identity and env == self.env and not during_cession and not in_attempt and replaces and self._env_known(env, fp)

    def evaluate(self, f, o, complete=True):
        ok, causes, st = super().evaluate(f, o, complete)
        causes = [c for c in causes if c != "no es sucesor (huella)"]
        return not causes, causes, st


class M_UnknownInvalidatorIsEqual(RuntimeModel):
    def _env_known(self, env, fp):
        return True


class M_UnobservedHostIsSuccessor(RuntimeModel):
    def _check_host_observed(self):
        return None

    def _env_differs(self, f, o):
        return o["env"] != f["env"]


class M_IdentityIsAppVersionOnly(RuntimeModel):
    def observe(self, identity, env=None, stable=True, during_cession=False, in_attempt=False, fingerprint=None, replaces=True):
        if identity.get("AppVersion") == self.identity.get("AppVersion"):
            identity = dict(self.identity)
        super().observe(identity, env, stable, during_cession, in_attempt, fingerprint, replaces)


class M_FpChangeDuringProbeIgnored(RuntimeModel):
    def _fp_changed_during(self, flag):
        return False


class M_UnstableAccepted(RuntimeModel):
    def _check_successor_observed(self, o):
        if o["identity"] != self.identity:
            raise Reject("sucesor no observado o no estable")


class M_AnticipatedProbe(RuntimeModel):
    def _check_successor_observed(self, o):
        if not self.stable:
            raise Reject("sucesor no observado o no estable")


class M_NoFloorInherits(RuntimeModel):
    def probe(self, key, o, authority, **kw):
        if key not in self.floors:
            name = [n for n, k in KEYS.items() if k == key][0]
            ident = dict(self.identity)
            self.identity = dict(self.history[0])
            self.full_measurement(make_floor(name, identity=self.identity, env=self.env), authority=True)
            self.identity = ident
            self.state[key] = "OBSERVATION_STALE"
        return super().probe(key, o, authority, **kw)


class M_ChainRaisesFloor(RuntimeModel):
    def _inherit(self, key, run, o):
        super()._inherit(key, run, o)
        for k in CLASS:
            self.floors[key]["props"][k] = o["props"][k]["value"]


class M_ConsumptionCoveredIsAuthorization(RuntimeModel):
    def _check_consumption(self, key, consumption_authorized):
        if self.state.get(key) != "INHERITED":
            super()._check_consumption(key, consumption_authorized)


class M_WriteInheritsFromReadProbe(RuntimeModel):
    def _req_status(self, rid, x, req):
        return "MATCH" if rid in WRITE_REQS else super()._req_status(rid, x, req)


class M_IncompleteAccepted(RuntimeModel):
    def evaluate(self, f, o, complete=True):
        return super().evaluate(f, o, True)


class M_ProbeWithoutOwnerOnUnacceptedFp(RuntimeModel):
    def _check_owner_for_unaccepted_fp(self, authority):
        return None


class M_MissingFloorAccepted(RuntimeModel):
    def _check_prop(self, k, kind, x, fv, f):
        return None if fv is None and x is not None and self._observed(x) else super()._check_prop(k, kind, x, fv, f)


class M_FailedProbeAccredits(RuntimeModel):
    def _fail(self, key):
        super()._fail(key)
        self.elig[key] = copy.deepcopy(self.floors[key]["eligibility"])


class M_ProbeNeverMeasures(RuntimeModel):  # opción (ii), no elegida: A-3 estrecharía la medición de §5, paso 3 (regresión frente a A2-P2)
    def measure_from_probe(self, key, authority):
        raise Reject("una sonda de compatibilidad nunca es una medición")


class M_IgnoreOperations(RuntimeModel):
    def _check_op(self, n, x, declared):
        return None


class M_UnexercisedOpUnknown(RuntimeModel):  # (h) estricta del borrador anterior: la cancelación no ejercida queda UNKNOWN
    def _check_op(self, n, x, declared):
        return super()._check_op(n, x, None)


class M_UnexercisedOpAlwaysValid(RuntimeModel):  # la operación no ejercida vale aunque el descriptor no la declare DISPONIBLE
    def _check_op(self, n, x, declared):
        return super()._check_op(n, x, "DISPONIBLE" if n in UNEXERCISED else declared)


class M_DeclarationOverridesObservation(RuntimeModel):  # la declaración del descriptor vence a lo que la sonda observa
    def _check_op(self, n, x, declared):
        return None if declared == "DISPONIBLE" else super()._check_op(n, x, declared)


class M_UnknownIdentityIsChange(RuntimeModel):  # lectura del borrador anterior: identidades comparadas como diccionarios (UNKNOWN cuenta como cambio)
    def _id_delta(self, a, b):
        return {k for k in IDENTITY_FACTS if a.get(k) != b.get(k)}

    def _id_known(self, ident):
        return True


class M_UnknownDeclaredIdentityIgnored(RuntimeModel):  # un hecho declarado sin valor no impide la sucesión si cambia otro
    def _id_known(self, ident):
        return True


class M_UndeclaredIdentityCounts(RuntimeModel):  # un hecho sin fuente declarada se exige como si la tuviera
    def _id_delta(self, a, b):
        return {k for k in IDENTITY_FACTS if UNK not in (a.get(k), b.get(k)) and a.get(k) != b.get(k)}

    def _id_known(self, ident):
        return all(ident.get(k) not in (None, UNK) for k in IDENTITY_FACTS)


def _identity_key_ignored(key):  # un cambio solo de ese hecho no se trata como cambio: la observación anterior sigue VALID
    class M(RuntimeModel):
        def observe(self, identity, env=None, stable=True, during_cession=False, in_attempt=False, fingerprint=None, replaces=True):
            if {k for k in IDENTITY_FACTS if identity.get(k) != self.identity.get(k)} <= {key}:
                identity = dict(self.identity)
            super().observe(identity, env, stable, during_cession, in_attempt, fingerprint, replaces)
    M.__name__ = "M_%sChangeIgnored" % key
    return M


class M_UnknownBlobIsEqual(RuntimeModel):  # el modelo anterior: solo host, autenticación y huella cuentan como invalidadores sin valor
    def _env_known(self, env, fp):
        return env.get("HostInstanceHash") != UNK and env.get("AuthState") != UNK and fp != UNK


class M_WorkIgnoresCapability(RuntimeModel):
    def _check_claims(self, key, requested):
        return super()._check_claims(key, {k: v for k, v in (requested or {}).items() if k != "capability_claim"})


class M_ContradictionWithoutS04(RuntimeModel):
    def evaluate(self, f, o, complete=True):
        r = super().evaluate(f, o, complete)
        self.s04 = False
        return r


class M_PerInvocationInherited(RuntimeModel):  # toma los controles por invocación del registro de la sonda heredada
    def _check_per_invocation(self, key, fresh):
        if self.state.get(key) == "INHERITED":
            run = self.elig[key]["RunRef"]
            fresh = tuple(set(fresh or ()) | set(next(r["Controls"] for r in self.records if r["Run"] == run)))
        return super()._check_per_invocation(key, fresh)


class M_InProbeEnvChangeRefunded(RuntimeModel):  # un no-sucesor que detecta la propia sonda se trata como si se hubiera detectado antes
    def probe(self, key, o, authority, **kw):
        if key in self.floors and self.state.get(key) == "OBSERVATION_STALE" and self._env_differs(self.floors[key], o):
            raise Reject(R_NONSUCC + " (entorno): sin consumir sonda")
        return super().probe(key, o, authority, **kw)


# ---- mutantes de la regla 11 (S-01: solo el valor exacto; sin verificador, sin aceptación por clase y sin lectura de valores)
class M_FpChangeAutoAccepted(RuntimeModel):  # la actualización deja aceptada la huella nueva (aceptación implícita de una versión nueva)
    def observe(self, identity, env=None, stable=True, during_cession=False, in_attempt=False, fingerprint=None, replaces=True):
        id_changed = bool(self._id_delta(self.identity, identity))
        super().observe(identity, env, stable, during_cession, in_attempt, fingerprint, replaces)
        if id_changed and fingerprint is not None:
            self.accepted.add(fingerprint)


class M_OD2RestoresAfterFpChange(RuntimeModel):  # la OD-2 exacta de la huella nueva devuelve la elegibilidad sin una medición nueva
    def accept_od2(self, fp, by):
        super().accept_od2(fp, by)
        if fp == self.fp:
            for k in self.floors:
                if self.state[k] == "NON_SUCCESSOR":
                    self.state[k], self.elig[k] = "VALID", copy.deepcopy(self.floors[k]["eligibility"])


class M_MaterialAdoptedByA3(RuntimeModel):  # A-3 adopta la aceptación por clase con una decisión del Owner y una A-n, y la honra como sucesión
    def request_fp_acceptance_mode(self, request):
        if request.get("by") != "OWNER" or not request.get("an_agreed"):
            return super().request_fp_acceptance_mode(request)
        self.od2_mode = "MATERIAL"

    def _huella_eq(self, f_fp, f_ident, s_fp, s_ident, stable=True):
        return super()._huella_eq(f_fp, f_ident, s_fp, s_ident, stable) or self.od2_mode == "MATERIAL"


class M_MaterialWithoutOwner(RuntimeModel):  # cualquier petición de aceptación por clase se acepta, sin decisión del Owner ni A-n
    def request_fp_acceptance_mode(self, request):
        self.od2_mode = "MATERIAL"


class M_OwnerPinnedValueReading(RuntimeModel):  # A-3 lee valores si el Owner nombra el programa, su blob, las superficies y la caducidad
    def request_value_reading(self, request):
        if request.get("by") == "OWNER" and all(request.get(x) for x in ("program_blob", "surfaces", "expires")):
            self.value_reads += 1
            return None
        return super().request_value_reading(request)


class M_PassiveKeyComparison(RuntimeModel):  # una comparación por clave (p. ej., de una medición pasiva que ordena el Coordinator) lee valores
    def request_value_reading(self, request):
        if str(request.get("kind", "")).startswith("PER_KEY"):
            self.value_reads += 1
            return None
        return super().request_value_reading(request)


class M_EvidenceWithValues(RuntimeModel):  # la evidencia publica el contenido del archivo
    def _evidence(self, before, after):
        r = super()._evidence(before, after)
        r["Values"] = dict(after["File"])
        return r


class M_EvidenceWithValueDigests(RuntimeModel):  # la evidencia publica un digest con clave de cada valor, por clave (lee valores)
    def _evidence(self, before, after):
        r = super()._evidence(before, after)
        r["KeyDigests"] = {k: hmac.new(b"clave-solo-local", (k + "\0" + v).encode("utf-8"), hashlib.sha256).hexdigest()[:16]
                           for k, v in after["File"].items()}
        return r


class M_EvidenceLocalizesValueChanges(RuntimeModel):  # la evidencia localiza por clave los cambios de valor (lee y compara valores)
    def _evidence(self, before, after):
        r = super()._evidence(before, after)
        r["ChangedKeys"] = sorted(k for k, v in after["File"].items() if k in before["File"] and before["File"][k] != v)
        return r


class M_EvidenceWithoutStability(RuntimeModel):  # la evidencia omite la estabilidad de la huella y los hechos de identidad del runtime
    def _evidence(self, before, after):
        r = super()._evidence(before, after)
        for x in ("FingerprintStable", "IdentityBefore", "IdentityAfter"):
            r.pop(x, None)
        return r


# ---- séptima ronda (M5-01..M5-03): huella aceptada tratada como igual, OD-2 que devuelve el caso a la sonda y evidencia condicionada a la estructura
class M_AcceptedFpIsHuellaEq(RuntimeModel):  # HuellaEq cuenta como igual toda huella ya aceptada por OD-2, no solo el valor exacto del suelo
    def _huella_eq(self, f_fp, f_ident, s_fp, s_ident, stable=True):
        return s_fp == f_fp or s_fp in self.accepted


class M_OD2ReentersA3(RuntimeModel):  # la OD-2 exacta de la huella nueva devuelve el caso a la sonda de A-3 (herencia tras un cambio de huella)
    def accept_od2(self, fp, by):
        super().accept_od2(fp, by)
        if fp == self.fp:
            for k in self.floors:
                if self.state[k] == "NON_SUCCESSOR" and self._id_delta(self.floors[k]["identity"], self.identity):
                    self.state[k] = "OBSERVATION_STALE"

    def _huella_eq(self, f_fp, f_ident, s_fp, s_ident, stable=True):
        return s_fp == f_fp or (s_fp in self.accepted and bool(self._id_delta(f_ident, s_ident)))


class M_EvidenceLocalizesWhenStructureEqual(RuntimeModel):  # con la estructura igual (el caso real), localiza por clave los cambios de valor
    def _evidence(self, before, after):
        r = super()._evidence(before, after)
        if r["StructureEqual"]:
            r["ChangedKeys"] = sorted(k for k, v in after["File"].items() if before["File"].get(k) != v)
        return r


class M_EvidenceDigestsWhenStructureEqual(RuntimeModel):  # con la estructura igual, publica un digest con clave de cada valor, por clave
    def _evidence(self, before, after):
        r = super()._evidence(before, after)
        if r["StructureEqual"]:
            r["KeyDigests"] = {k: hmac.new(b"clave-solo-local", (k + "\0" + v).encode("utf-8"), hashlib.sha256).hexdigest()[:16]
                               for k, v in after["File"].items()}
        return r


class M_FpAutoAcceptedWhenStructureEqual(RuntimeModel):  # con la estructura igual, registrar la evidencia deja aceptada la huella nueva
    def record_fp_change(self, before, after):
        rec = super().record_fp_change(before, after)
        if rec["StructureEqual"]:
            self.accepted.add(after["Fingerprint"])
        return rec


class M_EvidenceStabilityHardcoded(RuntimeModel):  # la evidencia da la huella por estable, sea cual sea la observación
    def _evidence(self, before, after):
        r = super()._evidence(before, after)
        r["FingerprintStable"] = True
        return r


class M_EvidenceRemovedKeysDropped(RuntimeModel):  # omite los nombres eliminados y compara la estructura por el número de nombres
    def _evidence(self, before, after):
        r = super()._evidence(before, after)
        r["KeysRemoved"], r["StructureEqual"] = [], len(before["Names"]) == len(after["Names"])
        return r


class M_P01WaivedWhenStructureEqual(RuntimeModel):  # M6-01: el trabajo trata como aceptada la huella vigente si hay evidencia con la estructura
    # igual para ella (fp_unaccepted sigue siendo true): aceptación implícita de una reescritura solo de valores, puesta en la puerta del trabajo
    def ordinary_invocation(self, key, **kw):
        if any(r.get("StructureEqual") and r.get("FingerprintAfter") == self.fp for r in self.fp_evidence):
            saved = set(self.accepted)
            self.accepted.add(self.fp)
            try:
                return super().ordinary_invocation(key, **kw)
            finally:
                self.accepted = saved
        return super().ordinary_invocation(key, **kw)


class M_OwnerlessMeasurementAfterEvidence(RuntimeModel):  # M6-01: con evidencia registrada para la huella vigente, la medición ya no exige al Owner
    def full_measurement(self, floor, authority=True, owner=True):
        if any(r.get("FingerprintAfter") == self.fp for r in self.fp_evidence):
            owner = True
        return super().full_measurement(floor, authority, owner)


class M_EvidenceComparesValuesInHost(RuntimeModel):  # M6-02: compara por clave en el host los valores del archivo antes y después al registrar
    # la evidencia y guarda la localización solo en memoria (no publica nada): lee valores, que es lo que suspende el §4 de la solicitud de recuperación
    def _evidence(self, before, after):
        bf, af = before.get("File") or {}, after.get("File") or {}
        self._local_changed = [k for k in bf if k in af and bf[k] != af[k]]
        return super()._evidence(before, after)


MUTANTS = [M_NewerIsBetter, M_IgnoreUnknown, M_LaxPropAssurance, M_NoSourceAccepted, M_MissingRequirementRowIsMatch,
           _skip("required_permissions"), _skip("sandbox_behavior"), _skip("context_isolation"), _skip("output_capture"), _skip("termination_observability"),
           _skip("consumption_mode"), _skip("authentication"), _skip("model_capability_class"), _skip("adapter_transport_provider"),
           _skip("introspection_level"), _skip("effort_semantics"), M_SkipProbe, M_UnboundedRetry, M_RetryAfterFail, M_PerCellLockout, M_MultiInvocation,
           M_WritableProbe, M_ClaimIncrease, M_WorkIgnoresRequested, _inherit_raises("permissions", "FULL_ACCESS"),
           _inherit_raises("write_scope", "REPOSITORY"), _inherit_raises("role_actions", frozenset({("ARCHITECT", "REVIEW"), ("WORKER", "IMPLEMENT")})),
           _inherit_raises("capability_claim", "Frontera"),
           _inherit_raises("consumption_authority", 99), M_InheritAcrossActions, M_CompatImpliesOD2, M_CoordinatorAcceptsOD2, M_OD2Sticky, M_OD2NotExact,
           M_FpGateOnlyForOneAdapter, M_EffortAsFloor, M_CapabilityAsFloor, M_ClassVsRequirement, M_IdentityFactsCompared, M_RefreshFreshness,
           M_StaleCopied, M_StaleNotAtProbe, M_StaleFloorInherits,
           _env_ignored("M_HostChangeIsSuccessor", "HostInstanceHash"), _env_ignored("M_CatalogChangeIsSuccessor", "CatalogEntryBlob"),
           _env_ignored("M_RoutingChangeIsSuccessor", "RoutingBlob"), _env_ignored("M_DescriptorChangeIsSuccessor", "DescriptorBlob"),
           _env_ignored("M_ProviderChangeIsSuccessor", "AdapterId", "Provider"), _env_ignored("M_AuthStateChangeIsSuccessor", "AuthState"),
           M_FingerprintOnlyIsSuccessor, M_FingerprintCoChangeIsSuccessor, M_UnknownInvalidatorIsEqual, M_UnobservedHostIsSuccessor,
           M_IgnoreAttemptInProgress, M_NewerSelectionIsSuccessor, M_IdentityIsAppVersionOnly, M_FpChangeDuringProbeIgnored, M_NoAuthorityCheck,
           M_AuthorityIdentityIgnored, M_CessionIgnored, M_UnstableAccepted, M_AnticipatedProbe, M_NoFloorInherits, M_ChainRaisesFloor,
           M_ConsumptionCoveredIsAuthorization, M_WriteInheritsFromReadProbe, M_IncompleteAccepted, M_ProbeWithoutOwnerOnUnacceptedFp,
           M_MissingFloorAccepted, M_FailedProbeAccredits, M_ProbeNeverMeasures, M_IgnoreOperations, M_UnexercisedOpUnknown, M_UnexercisedOpAlwaysValid,
           M_DeclarationOverridesObservation, M_UnknownIdentityIsChange, M_UnknownDeclaredIdentityIgnored, M_UndeclaredIdentityCounts,
           _identity_key_ignored("AppVersion"), _identity_key_ignored("AdapterVersion"), M_UnknownBlobIsEqual, M_WorkIgnoresCapability,
           M_ContradictionWithoutS04, M_PerInvocationInherited, M_InProbeEnvChangeRefunded,
           M_FpChangeAutoAccepted, M_OD2RestoresAfterFpChange, M_MaterialAdoptedByA3, M_MaterialWithoutOwner,
           M_OwnerPinnedValueReading, M_PassiveKeyComparison, M_EvidenceWithValues, M_EvidenceWithValueDigests,
           M_EvidenceLocalizesValueChanges, M_EvidenceWithoutStability,
           M_AcceptedFpIsHuellaEq, M_OD2ReentersA3, M_EvidenceLocalizesWhenStructureEqual, M_EvidenceDigestsWhenStructureEqual,
           M_FpAutoAcceptedWhenStructureEqual, M_EvidenceStabilityHardcoded, M_EvidenceRemovedKeysDropped,
           M_P01WaivedWhenStructureEqual, M_OwnerlessMeasurementAfterEvidence, M_EvidenceComparesValuesInHost]
EXPECTED_KILLERS = {  # mutante -> vectores que deben eliminarlo por el resultado (pasada ciega al motivo)
    "M_NewerIsBetter": {"NEG-19", "NEG-33"}, "M_IgnoreUnknown": {"NEG-01", "NEG-35", "NEG-47", "NEG-32"}, "M_LaxPropAssurance": {"NEG-47", "NEG-63"},
    "M_NoSourceAccepted": {"NEG-64"}, "M_MissingRequirementRowIsMatch": {"NEG-48"},
    "M_Ignore_required_permissions": {"NEG-04"}, "M_Ignore_sandbox_behavior": {"NEG-05"}, "M_Ignore_context_isolation": {"NEG-06"},
    "M_Ignore_output_capture": {"NEG-07"}, "M_Ignore_termination_observability": {"NEG-08"}, "M_Ignore_consumption_mode": {"NEG-09"},
    "M_Ignore_authentication": {"NEG-60"}, "M_Ignore_model_capability_class": {"NEG-11", "NEG-51", "NEG-79"},
    "M_Ignore_adapter_transport_provider": {"NEG-62"}, "M_Ignore_introspection_level": {"NEG-46"}, "M_Ignore_effort_semantics": {"NEG-12"},
    "M_SkipProbe": {"NEG-19"}, "M_UnboundedRetry": {"NEG-20"}, "M_RetryAfterFail": {"NEG-42"}, "M_PerCellLockout": {"NEG-54"},
    "M_MultiInvocation": {"NEG-21"}, "M_WritableProbe": {"NEG-14"}, "M_ClaimIncrease": {"NEG-41", "NEG-15", "NEG-16", "NEG-17", "NEG-18"},
    "M_WorkIgnoresRequested": {"NEG-74", "NEG-75", "NEG-76", "NEG-77", "NEG-87"},
    "M_InheritRaises_permissions": {"POS-01", "POS-07"}, "M_InheritRaises_write_scope": {"POS-01", "POS-07"},
    "M_InheritRaises_role_actions": {"POS-01", "POS-07"}, "M_InheritRaises_capability_claim": {"POS-01", "POS-07"},
    "M_InheritRaises_consumption_authority": {"POS-01", "POS-07"},
    "M_InheritAcrossActions": {"NEG-40", "NEG-57"}, "M_CompatImpliesOD2": {"NEG-23", "POS-04"}, "M_CoordinatorAcceptsOD2": {"NEG-24"},
    "M_OD2Sticky": {"NEG-52"}, "M_OD2NotExact": {"NEG-53"}, "M_FpGateOnlyForOneAdapter": {"NEG-69"}, "M_EffortAsFloor": {"NEG-12"},
    "M_CapabilityAsFloor": {"NEG-79"}, "M_ClassVsRequirement": {"NEG-51"}, "M_IdentityFactsCompared": {"POS-01", "POS-12"},
    "M_RefreshFreshness": {"POS-07", "NEG-59"}, "M_StaleCopied": {"NEG-59", "NEG-65"}, "M_StaleNotAtProbe": {"NEG-58"}, "M_StaleFloorInherits": {"NEG-34", "NEG-58"},
    "M_HostChangeIsSuccessor": {"NEG-25"}, "M_CatalogChangeIsSuccessor": {"NEG-26"}, "M_RoutingChangeIsSuccessor": {"NEG-55"},
    "M_DescriptorChangeIsSuccessor": {"NEG-56"}, "M_ProviderChangeIsSuccessor": {"NEG-13", "NEG-61"}, "M_AuthStateChangeIsSuccessor": {"NEG-03"},
    "M_FingerprintOnlyIsSuccessor": {"NEG-66"}, "M_FingerprintCoChangeIsSuccessor": {"NEG-67"},
    "M_UnknownInvalidatorIsEqual": {"NEG-71", "NEG-84", "NEG-85", "NEG-86"},
    "M_UnobservedHostIsSuccessor": {"NEG-45"}, "M_IgnoreAttemptInProgress": {"NEG-68"}, "M_NewerSelectionIsSuccessor": {"NEG-70"},
    "M_IdentityIsAppVersionOnly": {"NEG-49", "NEG-50"}, "M_FpChangeDuringProbeIgnored": {"NEG-78"}, "M_NoAuthorityCheck": {"NEG-22"},
    "M_AuthorityIdentityIgnored": {"NEG-39"}, "M_CessionIgnored": {"NEG-27"}, "M_UnstableAccepted": {"NEG-28"}, "M_AnticipatedProbe": {"NEG-29"},
    "M_NoFloorInherits": {"NEG-30"}, "M_ChainRaisesFloor": {"POS-06"}, "M_ConsumptionCoveredIsAuthorization": {"NEG-36"},
    "M_WriteInheritsFromReadProbe": {"NEG-32"}, "M_IncompleteAccepted": {"NEG-37"}, "M_ProbeWithoutOwnerOnUnacceptedFp": {"NEG-38"},
    "M_MissingFloorAccepted": {"NEG-44"}, "M_FailedProbeAccredits": {"NEG-43"}, "M_ProbeNeverMeasures": {"POS-11", "POS-13"},
    "M_IgnoreOperations": {"NEG-72", "NEG-73"}, "M_UnexercisedOpUnknown": {"POS-01", "POS-16"}, "M_UnexercisedOpAlwaysValid": {"NEG-92"},
    "M_DeclarationOverridesObservation": {"NEG-72", "NEG-91"}, "M_UnknownIdentityIsChange": {"NEG-80", "NEG-81"},
    "M_UnknownDeclaredIdentityIgnored": {"NEG-80"}, "M_UndeclaredIdentityCounts": {"POS-14"}, "M_AppVersionChangeIgnored": {"NEG-82"},
    "M_AdapterVersionChangeIgnored": {"NEG-83"}, "M_UnknownBlobIsEqual": {"NEG-84", "NEG-85", "NEG-86"}, "M_WorkIgnoresCapability": {"NEG-87"},
    "M_ContradictionWithoutS04": {"NEG-35", "NEG-88"}, "M_PerInvocationInherited": {"NEG-89"}, "M_InProbeEnvChangeRefunded": {"NEG-90"},
    "M_FpChangeAutoAccepted": {"NEG-118"}, "M_OD2RestoresAfterFpChange": {"NEG-118"}, "M_MaterialAdoptedByA3": {"NEG-119"},
    "M_MaterialWithoutOwner": {"NEG-119"}, "M_OwnerPinnedValueReading": {"NEG-120"}, "M_PassiveKeyComparison": {"NEG-120"},
    "M_EvidenceWithValues": {"POS-20"}, "M_EvidenceWithValueDigests": {"POS-20"}, "M_EvidenceLocalizesValueChanges": {"POS-20"},
    "M_EvidenceWithoutStability": {"POS-20"},
    "M_AcceptedFpIsHuellaEq": {"NEG-121"}, "M_OD2ReentersA3": {"NEG-118"}, "M_EvidenceLocalizesWhenStructureEqual": {"POS-21"},
    "M_EvidenceDigestsWhenStructureEqual": {"POS-21"}, "M_FpAutoAcceptedWhenStructureEqual": {"POS-21"},
    "M_EvidenceStabilityHardcoded": {"POS-22"}, "M_EvidenceRemovedKeysDropped": {"POS-22"},
    "M_P01WaivedWhenStructureEqual": {"POS-21"}, "M_OwnerlessMeasurementAfterEvidence": {"POS-21", "POS-22"},
    "M_EvidenceComparesValuesInHost": {"POS-20", "POS-21", "POS-22"},
}
# RED de C-43: con V14 sin A-3 (V14Model) y el motivo comprobado, solo pasan BASE-01 y los negativos que expresan reglas ya vigentes en V14;
# a ciegas del motivo fallan los positivos y los negativos que exigen la sonda de A-3 (RedV14.FailingReasonBlind). El gate lo compara mecánicamente.
RED_V14_PASSING_WITH_REASON = {"BASE-01", "NEG-19", "NEG-24", "NEG-33", "NEG-40", "NEG-43", "NEG-49", "NEG-50", "NEG-53", "NEG-82", "NEG-83"}
EQUIVALENT = {  # mutantes retirados por equivalentes en resultado, con su sustituto
    "M_UnboundedProbe": "equivalente: tras un fallo rechaza la regla 7 y tras un éxito el estado INHERITED; sustituido por M_UnboundedRetry",
    "M_InheritObservedCapability": "equivalente con la clase de capacidad «igual» (regla 4); sustituido por M_CapabilityAsFloor",
    "M_MaterialModeInert": "equivalente tras M1-01: A-3 no define ningún modo MATERIAL, así que dejarlo inerte es el propio modelo; sustituido por "
                           "M_MaterialAdoptedByA3",
}
RULES = ["A3.%d" % i for i in range(1, 12)]


def delta_text(a3_md):
    m = re.search(r"^### 3\.2 Delta exacto\n(.*?)(?=^### |^## )", a3_md, re.S | re.M)
    block = m.group(1) if m else ""
    lines = [l[2:] if l.startswith("> ") else ("" if l == ">" else None) for l in block.split("\n")]
    q = "\n".join(l for l in lines if l is not None).strip()
    return q[1:-1] if q.startswith("«") and q.endswith("»") else q


def forbidden_hits(text):
    return sorted({m.group(0) for p in FORBIDDEN for m in re.finditer(p, text)})


def g5_model(a3_md=None):
    v, vb = vectors(), vectors(blind=True)
    mutants = []
    for mcls in MUTANTS:
        res = {}
        for blind in (False, True):
            mv = []
            try:
                vectors(mcls, mv, blind)
                extra = []
            except Exception as e:  # una excepción inesperada también elimina al mutante
                extra = ["excepción tras %s: %s: %s" % (mv[-1]["Id"] if mv else "inicio", type(e).__name__, e)]
            res[blind] = [x["Id"] for x in mv if x["Result"] == "FAIL"] + extra
        exp = EXPECTED_KILLERS.get(mcls.__name__, set())
        by_expected = sorted(set(res[True]) & exp)
        mutants.append({"Mutant": mcls.__name__, "KilledBy": res[False], "KilledByOutcome": res[True], "ExpectedKillers": sorted(exp),
                        "KilledByExpectedOutcome": by_expected, "Result": "PASS" if res[False] and by_expected else "FAIL"})
    covered = sorted({x["Rule"] for x in v})
    missing = [r for r in RULES if r not in covered]
    ids = [x["Id"] for x in v]
    dup = sorted({i for i in ids if ids.count(i) > 1})
    names = [m.__name__ for m in MUTANTS]
    unmapped = sorted(n for n in names if n not in EXPECTED_KILLERS)
    unknown_ids = sorted({i for s in EXPECTED_KILLERS.values() for i in s} - set(ids))
    samples_missed = [s for s in FORBIDDEN_SAMPLES if not forbidden_hits(s)]
    binding = None
    if a3_md is not None:
        delta = " ".join(delta_text(a3_md).split())
        miss = {k: [p for p in ph if " ".join(p.split()) not in delta] for k, ph in TEXT_BINDING.items()}
        prod = sorted(set(m.group(0) for m in PRODUCT_TOKENS.finditer(delta)))
        forb = forbidden_hits(delta)
        binding = {"DeltaChars": len(delta), "Missing": {k: x for k, x in miss.items() if x}, "ProductNamesInDelta": prod, "ForbiddenInDelta": forb,
                   "Result": "PASS" if delta and not any(miss.values()) and not prod and not forb else "FAIL"}
    kinds = {k: sum(1 for x in v if x["Kind"] == k) for k in ("BASE", "POS", "NEG")}
    red, redb = vectors(V14Model), vectors(V14Model, blind=True)
    red_pass = sorted(x["Id"] for x in red if x["Result"] == "PASS")
    red_fail_blind = sorted(x["Id"] for x in redb if x["Result"] == "FAIL")
    red_ok = all(x["Result"] == "FAIL" for x in redb if x["Kind"] == "POS") and all(x["Result"] == "PASS" for x in red + redb if x["Kind"] == "BASE") \
        and set(red_pass) == RED_V14_PASSING_WITH_REASON
    red_v14 = {"PassingWithReason": red_pass, "ExpectedPassingWithReason": sorted(RED_V14_PASSING_WITH_REASON), "FailingReasonBlind": red_fail_blind,
               "FailingReasonBlindPositive": sum(1 for i in red_fail_blind if i.startswith("POS-")),
               "FailingReasonBlindNegative": sum(1 for i in red_fail_blind if i.startswith("NEG-")), "Result": "PASS" if red_ok else "FAIL"}
    ok = all(x["Result"] == "PASS" for x in v) and all(x["Result"] == "PASS" for x in vb) and all(m["Result"] == "PASS" for m in mutants) \
        and not missing and not dup and not unmapped and not unknown_ids and not samples_missed and (binding is None or binding["Result"] == "PASS") \
        and red_ok
    return {"Check": "G5 modelo de compatibilidad del sucesor", "Total": len(v), "Base": kinds["BASE"], "Positive": kinds["POS"], "Negative": kinds["NEG"],
            "Passed": sum(1 for x in v if x["Result"] == "PASS"), "PassedReasonBlind": sum(1 for x in vb if x["Result"] == "PASS"), "Vectors": v,
            "RedV14": red_v14,
            "MutantsTotal": len(mutants), "MutantsKilled": sum(1 for m in mutants if m["Result"] == "PASS"), "Mutants": mutants,
            "EquivalentMutantsRetired": EQUIVALENT, "MutantsRetiredByS01": list(RETIRED_BY_S01), "MutantsWithoutExpectedKillers": unmapped, "ExpectedKillerIdsUnknown": unknown_ids,
            "RulesCovered": covered, "RulesMissing": missing, "DuplicateIds": dup, "ForbiddenSamplesMissed": samples_missed, "TextBinding": binding,
            "Result": "PASS" if ok else "FAIL"}


# ---------------------------------------------------------------- G1..G4 (repositorio, solo lectura)
def git(repo, *a):
    return subprocess.run(["git", "-C", repo] + list(a), capture_output=True, check=True).stdout.decode("utf-8")


def cat_blob(repo, blob):
    return git(repo, "cat-file", "-p", blob).replace("\r\n", "\n")


def load_clause_map(repo, td):
    path = os.path.join(td, "clause_map.py")
    open(path, "w", encoding="utf-8", newline="\n").write(cat_blob(repo, CLAUSE_MAP_BLOB))
    spec = importlib.util.spec_from_file_location("clause_map_pinned", path)
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod, path


MARK = re.compile(r"^\*\*(" + "|".join(SOURCES) + r") L(\d+)(?:-(\d+))?\*\* — ")


def literals(repo, a3_md):
    src = {t: cat_blob(repo, b).split("\n") for t, b in SOURCES.items()}
    sec2 = re.search(r"^## 2\. .*?(?=^## 3\. )", a3_md, re.S | re.M)
    lines = (sec2.group(0) if sec2 else "").split("\n")
    f, n = [], 0
    for i, l in enumerate(lines):
        m = MARK.match(l)
        if not m:
            continue
        tag, a = m.group(1), int(m.group(2))
        b = int(m.group(3) or a)
        j = i + 1
        while j < len(lines) and lines[j] == "":
            j += 1
        quote = []
        while j < len(lines) and lines[j].startswith(">"):
            quote.append(lines[j][2:] if lines[j].startswith("> ") else "")
            j += 1
        want = src[tag][a - 1:b]
        n += 1
        if quote != want:
            f.append("%s L%d-%d: literal distinto de la fuente" % (tag, a, b))
    if n == 0:
        f.append("§2 sin literales marcados")
    return n, f


def g1_delta_scope(repo, a3_md, cm, annex_md, extra=None):
    f = []
    n, lf = literals(repo, a3_md)
    f += lf
    v14 = cat_blob(repo, FROZEN[V14])
    para = delta_text(a3_md)
    if not para:
        f.append("delta sin párrafo citado")
    if "Ubicación: al final de V14 §6, inmediatamente después de «no una foto anterior.»" not in a3_md:
        f.append("delta sin ubicación exacta")
    prod = sorted(set(m.group(0) for m in PRODUCT_TOKENS.finditer(para)))
    if prod:
        f.append("nombres de producto en el delta: %s" % prod)
    forb = forbidden_hits(" ".join(para.split()))
    if forb:
        f.append("patrones prohibidos en el delta: %s" % forb)
    s6, s7 = v14.find(SECTION6 + "\n"), v14.find("\n## 7. ")
    pos = v14.find(ANCHOR)
    if v14.count(ANCHOR) != 1 or not (s6 < pos < s7):
        f.append("ancla ausente, repetida o fuera de §6")
    applied = v14.replace(ANCHOR, ANCHOR + "\n\n" + para, 1)
    before = {(lv, h): t for lv, h, t in cm.sections(v14)}
    after = {(lv, h): t for lv, h, t in cm.sections(applied)}
    added, removed = sorted(set(after) - set(before)), sorted(set(before) - set(after))
    changed = sorted(k for k in before if k in after and before[k] != after[k])
    unexpected = [k for k in changed if k[0] != 1 and k[1] != cm.norm(SECTION6)]
    if added or removed:
        f.append("al aplicar aparecen o desaparecen secciones: %s %s" % (added, removed))
    if unexpected:
        f.append("al aplicar cambian secciones fuera de §6: %s" % unexpected)
    if not any(k[1] == cm.norm(SECTION6) for k in changed):
        f.append("al aplicar no cambia §6")
    for k, val in (("Applies-to", "Applies-to: I-62"), ("FREEZE_SHA", "FREEZE_SHA     = b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43"),
                   ("blob V14", "blob         = 34ad80ea1bfff144bfc5169f62920a4c904c1bfa"), ("A-1", "blob c01899a72b940503bb85a0fab42bc085c603fd0f"),
                   ("A-2", "blob " + A2_BLOB), ("acuerdo de A-2", "A-2 AGREED (decisiones §57"), ("anexo", ANNEX)):
        if val not in a3_md:
            f.append("cabecera sin %s" % k)
    priv = [("A-3", a3_md), ("anexo", annex_md)] + sorted((extra or {}).items())  # en preview, también los demás archivos del borrador (M1-18)
    ids = host_identities()
    for name, text in priv:
        if text is None:
            f.append("%s ausente" % name)
        else:
            f += privacy_findings(name, text, ids)
    return {"Check": "G1 literales, neutralidad, privacidad y aplicación sobre V14", "Literals": n, "ChangedSections": [k[1][:80] for k in changed],
            "PrivacyChecked": [name for name, _ in priv], "Findings": f, "Result": "PASS" if not f else "FAIL"}


def g2_paths(repo, base, head):
    f = []
    if A3_BASE is None:
        f.append("A3_BASE sin fijar")
    elif base != A3_BASE:
        f.append("base distinta de A3_BASE")
    if base != git(repo, "rev-parse", head + "^1").strip():
        f.append("base distinta del padre de head")
    rows = [l.split("\t") for l in git(repo, "diff", "--name-status", "--no-renames", base, head).splitlines() if l]
    status = {r[1]: r[0] for r in rows}
    for p in REQUIRED_ADDED:
        if status.get(p) != "A":
            f.append("%s no aparece como añadido (A)" % p)
    bad = [p for p in status if p not in ALLOWED_EXACT]
    if bad:
        f.append("rutas no permitidas: %s" % bad)
    touched_a2 = [p for p in status if p in A2_FILES or p.startswith(A2_PREFIX)]
    if touched_a2:
        f.append("A-3 modifica A-2: %s" % touched_a2)
    research = {}
    for p in RESEARCH:  # investigación y paquete de OD-2-MAT: añadidos en el commit de A-3 (REQUIRED_ADDED), con el blob que fija A-3
        r = subprocess.run(["git", "-C", repo, "rev-parse", "%s:%s" % (head, p)], capture_output=True)
        research[p] = r.stdout.decode("utf-8").strip() if r.returncode == 0 else None
        if research[p] is None:
            f.append("%s ausente en head" % p)
        elif research[p] != RESEARCH_BLOBS[p]:
            f.append("%s con un blob distinto del fijado por A-3 (%s)" % (p, research[p]))
    for p in APPEND_ONLY:
        if p in status and (status[p] != "M" or not git(repo, "show", "%s:%s" % (head, p)).startswith(git(repo, "show", "%s:%s" % (base, p)))):
            f.append("%s no cambia solo por añadido" % p)
    priv = []  # privacidad de todo archivo añadido o modificado; en los de solo añadido, de lo añadido (M1-18, M2-03)
    ids = host_identities()
    for p, st in sorted(status.items()):
        if st == "A":
            text = git(repo, "show", "%s:%s" % (head, p))
        elif st == "M" and p in APPEND_ONLY:
            new, old = git(repo, "show", "%s:%s" % (head, p)), git(repo, "show", "%s:%s" % (base, p))
            text = new[len(old):] if new.startswith(old) else new
        elif st == "M":
            text = git(repo, "show", "%s:%s" % (head, p))
        else:
            continue
        priv.append(p)
        f += privacy_findings(p, text, ids)
    return {"Check": "G2 rutas", "Base": base, "Head": head, "Changed": status, "ResearchBlobs": research, "PrivacyChecked": priv, "Findings": f,
            "Result": "PASS" if not f else "FAIL"}


def g3_frozen(repo, head, base=None):
    got = {p: git(repo, "rev-parse", "%s:%s" % (head, p)).strip() for p in FROZEN}
    diff = {p: {"Expected": FROZEN[p], "Got": got[p]} for p in FROZEN if got[p] != FROZEN[p]}
    a2 = {A2_FILES[0]: {"Head": git(repo, "rev-parse", "%s:%s" % (head, A2_FILES[0])).strip()}}
    if base is not None:
        for p in A2_FILES:
            b0, b1 = git(repo, "rev-parse", "%s:%s" % (base, p)).strip(), git(repo, "rev-parse", "%s:%s" % (head, p)).strip()
            a2[p] = {"Base": b0, "Head": b1}
            if b0 != b1:
                diff[p] = {"Expected": b0, "Got": b1}
    if a2[A2_FILES[0]]["Head"] != A2_BLOB:  # A-3 compone con esta A-2 exacta (cabecera, literales A2 y §3.7): otra A-2 exige preparar A-3 de nuevo
        diff["A-2 fijada por A-3"] = {"Expected": A2_BLOB, "Got": a2[A2_FILES[0]]["Head"]}
    return {"Check": "G3 blobs congelados y A-2 = A2_BLOB" + (" sin cambio entre base y head" if base else ""), "Blobs": got, "A2": a2, "Mismatches": diff,
            "Result": "PASS" if not diff else "FAIL"}


def blob_id(raw):
    return hashlib.sha1(b"blob %d\0" % len(raw) + raw).hexdigest()


def g5b_selftest(repo, head, g5):  # el resultado custodiado de self-test corresponde a la A-3 exacta de head y a estas guardas (M1-17)
    f = []
    try:
        st = json.loads(git(repo, "show", "%s:%s" % (head, SELFTEST)))
    except (subprocess.CalledProcessError, ValueError) as e:
        st, f = {}, ["a3-selftest.json ilegible en head: %s" % type(e).__name__]
    want = git(repo, "rev-parse", "%s:%s" % (head, A3)).strip()
    if st.get("A3TextBlob") != want:
        f.append("A3TextBlob distinto del blob de A-3 en head")
    if st.get("A3Text") != A3:
        f.append("A3Text distinto de la ruta de A-3")
    if st.get("Result") != "PASS":
        f.append("a3-selftest.json sin Result PASS")
    if st.get("G5") != json.loads(json.dumps(g5, ensure_ascii=False, default=sorted)):  # G5 es determinista: mismo texto, mismas guardas
        f.append("G5 custodiado distinto del recalculado en head")
    return {"Check": "G5b self-test custodiado", "A3Blob": want, "A3TextBlob": st.get("A3TextBlob"), "A3Text": st.get("A3Text"), "Findings": f,
            "Result": "PASS" if not f else "FAIL"}


def g4_clause_map(repo, head, changed, cm, tool):
    base = git(repo, "merge-base", head, "origin/main").strip()
    in_surf = [p for p in changed if cm.in_surfaces(p)]
    with tempfile.TemporaryDirectory() as td:
        r = subprocess.run([sys.executable, tool, "check", base, head, os.path.join(td, "c20b.json")], cwd=repo, capture_output=True)
        text = (r.stdout + r.stderr).decode("utf-8", "replace")
    equal = r.returncode == 0 and "EQUAL" in text
    return {"Check": "G4 C-20b", "Base": base, "SurfacesTouched": in_surf, "ExitCode": r.returncode, "Output": text.strip()[-300:],
            "Result": "PASS" if equal and not in_surf else "FAIL"}


def main():
    ap = argparse.ArgumentParser()
    sub = ap.add_subparsers(dest="cmd", required=True)
    s = sub.add_parser("self-test"); s.add_argument("--out", required=True); s.add_argument("--a3"); s.add_argument("--no-text", action="store_true")
    p = sub.add_parser("preview"); p.add_argument("--repo", required=True); p.add_argument("--a3", required=True); p.add_argument("--out", required=True)
    r = sub.add_parser("run"); r.add_argument("--repo", required=True); r.add_argument("--base", required=True); r.add_argument("--head", required=True)
    r.add_argument("--out", required=True)
    a = ap.parse_args()
    if a.cmd == "self-test":
        path = None if a.no_text else (a.a3 or os.path.join(os.path.dirname(os.path.abspath(__file__)), "I-62-A-3.md"))
        raw = open(path, "rb").read() if path and os.path.exists(path) else None
        text = raw.decode("utf-8").replace("\r\n", "\n") if raw is not None else None
        g5 = g5_model(text)
        findings = [] if text is not None or a.no_text else ["sin texto de A-3 (%s): el vínculo con el delta no se comprueba; --no-text para omitirlo" % path]
        res = {"Tool": "a3-guards.py self-test", "A3Text": a.a3 if (a.a3 and raw is not None) else (os.path.basename(path) if raw is not None else None),
               # blob de Git del texto leído, con finales de línea LF (los del blob aunque la copia de trabajo use CRLF)
               "A3TextBlob": blob_id(text.encode("utf-8")) if text is not None else None, "NoText": bool(a.no_text), "Findings": findings, "G5": g5,
               "Result": "PASS" if g5["Result"] == "PASS" and not findings else "FAIL"}
    elif a.cmd == "preview":
        text = open(a.a3, encoding="utf-8").read()
        annex_path = os.path.join(os.path.dirname(os.path.abspath(a.a3)), os.path.basename(ANNEX))
        annex = open(annex_path, encoding="utf-8").read() if os.path.exists(annex_path) else None
        draft = os.path.dirname(os.path.abspath(a.a3))  # demás archivos del borrador junto a A-3 (paquete, guardas, resultado, investigación)
        extra = {}
        for rel in [os.path.basename(PKG), os.path.basename(GUARDS), os.path.basename(SELFTEST)] + \
                ["research/" + os.path.basename(p) for p in RESEARCH]:
            fp_ = os.path.join(draft, rel)
            if os.path.exists(fp_):
                extra[rel] = open(fp_, encoding="utf-8").read()
        head = git(a.repo, "rev-parse", "HEAD").strip()
        with tempfile.TemporaryDirectory() as td:
            cm, _ = load_clause_map(a.repo, td)
            checks = [g1_delta_scope(a.repo, text, cm, annex, extra), g3_frozen(a.repo, head), g5_model(text)]
        res = {"Tool": "a3-guards.py preview", "RepoHead": head, "Checks": checks,
               "Result": "PASS" if all(c["Result"] == "PASS" for c in checks) else "FAIL"}
    else:
        head = git(a.repo, "rev-parse", a.head).strip()
        base = git(a.repo, "rev-parse", a.base).strip()
        text = git(a.repo, "show", "%s:%s" % (head, A3))
        try:
            annex = git(a.repo, "show", "%s:%s" % (head, ANNEX))
        except subprocess.CalledProcessError:
            annex = None
        with tempfile.TemporaryDirectory() as td:
            cm, tool = load_clause_map(a.repo, td)
            g2 = g2_paths(a.repo, base, head)
            g5 = g5_model(text)
            checks = [g1_delta_scope(a.repo, text, cm, annex), g2, g3_frozen(a.repo, head, base), g4_clause_map(a.repo, head, list(g2["Changed"]), cm, tool),
                      g5, g5b_selftest(a.repo, head, g5)]
        res = {"Tool": "a3-guards.py run", "A3Base": A3_BASE, "Base": base, "Head": head, "Checks": checks,
               "Result": "PASS" if all(c["Result"] == "PASS" for c in checks) else "FAIL"}
    open(a.out, "w", encoding="utf-8", newline="\n").write(json.dumps(res, ensure_ascii=False, indent=1, default=sorted) + "\n")
    print(res["Result"])
    sys.exit(0 if res["Result"] == "PASS" else 1)


if __name__ == "__main__":
    main()
