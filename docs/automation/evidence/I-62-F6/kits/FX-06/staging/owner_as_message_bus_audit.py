#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""FX-06 / C-32 / C-37 — auditor determinista del transporte (SOLO SUPERVISION; plano a).

Calcula OWNER_AS_MESSAGE_BUS y las entradas AUTONOMY_GAP de una corrida de FX-06 a partir de:
  (1) la custodia del fixture (puntos state/v2 con orchestration.autonomy_gaps[] e intentos, commits del archivo de decisiones del
      fixture, todos los commits de fx/u1 y de main del fixture con la identidad de su autor, el commit del paso 1, registros de
      lanzamiento ligados a cada intento por launch_evidence o runtime_evidence) y
  (2) la lista de eventos humanos de la sesion del Principal (y las aperturas de otras sesiones), que la supervision construye de la
      transcripcion y de los metadatos de la app DESPUES de la corrida (este programa nunca lee transcripciones: solo su archivo de
      entrada).

Reglas (autoridades citadas en message-bus-auditor.md):
  - V14 D.8 / C-32 / C-39: PASS solo con OWNER_AS_MESSAGE_BUS = false EN LOS PASOS 1-7, sin relevo del Owner ni decision intermedia
    del Coordinator; C-37 / V14 §20.9: todo relevo manual es AUTONOMY_GAP y no es PASS.
  - F4 (AutonomyGaps.OwnerAsMessageBus sobre los puntos de una secuencia; F4-OBS-22): verdadero si algun punto de la secuencia custodia
    un AUTONOMY_GAP con RelayedBy OWNER. La secuencia auditada es la ventana 1-7: cuentan los huecos que aparecen por primera vez en un
    punto de la ventana o que nombran una solicitud logica de la ventana; los heredados de antes del arranque se enumeran y no cuentan.
  - Decisiones del Coordinator §49 (fila FX-06) y §54 («Bus de mensajes»): abrir una sesion asignada al Owner no es
    OWNER_AS_MESSAGE_BUS ni AUTONOMY_GAP; el transporte manual de prompts o resultados entre roles si.
  - Fallo cerrado: la ventana se fija por instantes (nunca por Seq); una entrada con orden de Seq distinto del orden temporal, una
    ventana vacia o invertida, un arranque que no precede al push del paso 1 (D.8-1) o un punto del paso 7 que no es la ingestion
    ARCHITECT_SATISFIED de la segunda solicitud se rechaza (codigo 2). Lo que no puede clasificarse con certeza deja
    OWNER_AS_MESSAGE_BUS = UNDETERMINED y la elegibilidad en false hasta una disposicion del Coordinator. El auditor nunca declara PASS:
    solo la elegibilidad por transporte.
  - v3 (revision R2 del kit): todo commit en el archivo de decisiones dentro de 1-7 exige disposicion, y un marcador I62-* o
    FIXTURE-ORDER de un autor que no es el Coordinator levanta una bandera; todo commit de fx/u1 o de main del fixture de un autor
    distinto del Principal dentro de 1-7 levanta una bandera; el arranque debe ser un MESSAGE literal «continua» o la plantilla neutra
    (por su SHA-256); solo las aperturas de la sesion del Principal cuentan para su tope; un AUTONOMY_GAP que cuenta con RelayedBy
    desconocido deja el bus en UNDETERMINED; un «continua» literal de la supervision antes de la ventana exige disposicion sin bandera
    (OQ-20) y SUPERVISION_LABEL_LEAK solo se levanta con la etiqueta observada (ObservedSenderLabel).
  - v3.1 (verificacion de R2): el marcador I62-* o FIXTURE-ORDER de un autor que no es el Coordinator levanta la bandera
    DECISIONS_FILE_MARKER_BY_NON_COORDINATOR (con disposicion) en todo commit del archivo de decisiones desde el commit de la RLA vigente (P5)
    o desde la apertura de la sesion del Principal si es anterior, no solo dentro de 1-7; todo commit de decisiones y todo punto desde ese
    instante deben figurar en BranchCommits (asi el Issuer = AuthorRole cubre tambien la RLA); un arranque MESSAGE o
    ANSWER_TO_PRINCIPAL_QUESTION sin Text se rechaza.

Uso:
  python owner_as_message_bus_audit.py audit --input <entrada.json> --output <resultado.json>
  python owner_as_message_bus_audit.py self-test [--output <selftest-result.json>]

Sin dependencias externas; salida determinista (mismo archivo de entrada -> mismos bytes de salida).
"""
import argparse
import copy
import datetime
import hashlib
import json
import re
import sys
import unicodedata

SCHEMA_IN = "fx06-transport-audit-input/v3"
SCHEMA_OUT = "fx06-transport-audit-result/v3"
AUDITOR_VERSION = "3.1.0"

# Literal minimo de continuacion. Base: practica vigente (las continuaciones a sesiones del fixture las escribe el Owner; decisiones §54
# «Bus de mensajes»; precedente de R, evidencia §71). Decisiones §50 autorizo otro emisor (la supervision) y otro alcance (G0, QU y QH):
# pregunta abierta OQ-20. Se compara tras NFC, recorte, casefold y sin «¡», «.» ni «!» finales; «continua» sin tilde es variante tipografica.
LITERAL_CONTINUE = {"continúa", "continua"}

# Marcadores de contenido del protocolo en un mensaje humano. Su presencia sin coincidencia con un artefacto custodiado deja el
# evento en UNDETERMINED (posible relevo); con coincidencia de lineas de un artefacto de rol, es relevo manual (MANUAL_RELAY).
RELAY_MARKERS = [
    ("LOGICAL_REVIEW_REQUEST_ID", re.compile(r"\bL[0-9]{8}T[0-9]{6}Z-[0-9a-f]{4}\b")),
    ("INVOCATION_ID", re.compile(r"\bI[0-9]{8}T[0-9]{6}Z-[0-9a-f]{4}\b")),
    ("RUN_ID", re.compile(r"\bR[0-9]{8}T[0-9]{6}Z-[0-9a-f]{4}\b")),
    ("BINDING_ID", re.compile(r"\bB[0-9]{8}T[0-9]{6}Z-[0-9a-f]{4}\b")),
    ("PREFLIGHT_ID", re.compile(r"\bP[0-9]{8}T[0-9]{6}Z-[0-9a-f]{4}\b")),
    ("SCHEMA_NAME", re.compile(r"\brackcad-[a-z0-9-]+/v[0-9]+\b")),
    ("VERDICT", re.compile(r"CHANGES REQUIRED|BLOCKED — OWNER DECISION|\bAGREED\b")),
    ("DISPOSITION_STATE", re.compile(r"\b(STILL_OPEN|SUPERSEDED|CLOSED)\b")),
    ("GIT_SHA", re.compile(r"\b[0-9a-f]{40}\b")),
    ("SHA256", re.compile(r"\b[0-9a-f]{64}\b")),
    ("JSON_OBJECT", re.compile(r"\{\s*\"[A-Za-z][A-Za-z0-9_]*\"\s*:")),
]
LRID_RX = re.compile(r"\bL[0-9]{8}T[0-9]{6}Z-[0-9a-f]{4}\b")

MIN_LINE_CHARS = 24  # lineas mas cortas no se usan para la coincidencia con artefactos (demasiado genericas)

HUMAN_KINDS = {
    "SESSION_OPENED", "SESSION_REOPENED", "MESSAGE", "ANSWER_TO_PRINCIPAL_QUESTION", "TOOL_APPROVAL", "EFFORT_CHANGE",
    "SESSION_STOPPED", "OTHER",
}
ACTORS = {"OWNER", "SUPERVISION", "OTHER"}
ISSUERS = {"COORDINATOR", "PRINCIPAL", "OWNER", "OTHER"}
PROOF_SOURCES = {"RELAY_RECORD_V2", "RUNTIME_EVIDENCE"}
LAUNCHED_STATES = {"LAUNCHED", "RESULT_RECEIVED", "RESULT_INGESTED", "LAUNCH_UNCERTAIN"}
STATE_ORDER = {
    "INVOCATION_PLANNED": 0, "BUDGET_RESERVED": 1, "LAUNCHING": 2, "LAUNCHED": 3, "LAUNCH_UNCERTAIN": 4, "RESULT_RECEIVED": 4,
    "RESULT_INGESTED": 5, "CANCELLED_BEFORE_LAUNCH": 5,
}
AUTHORIZATION_MARKER = "I62-REVIEW-LOOP-AUTHORIZATION:"
# Marcadores del protocolo que el lector de F4 (Orchestration.DecisionBlock) acepta sin mirar el autor: solo el Coordinator puede emitirlos.
PROTOCOL_MARKER_PREFIXES = ("I62-", "FIXTURE-ORDER")
HEX64_RX = re.compile(r"^[0-9a-f]{64}$")


class InputError(Exception):
    pass


# ----------------------------------------------------------------------------------------------------------------------------- utilidades

def nfc(text):
    return unicodedata.normalize("NFC", text)


def sha256_text(text):
    return hashlib.sha256(text.encode("utf-8")).hexdigest()


def norm_line(line):
    return nfc(line).strip()


def line_digests(text):
    out = []
    for raw in nfc(text).replace("\r\n", "\n").split("\n"):
        line = norm_line(raw)
        if len(line) >= MIN_LINE_CHARS:
            out.append(sha256_text(line))
    return out


def is_literal_continue(text):
    t = nfc(text).strip().casefold()
    t = t.lstrip("¡").rstrip(".!").strip()
    return t in LITERAL_CONTINUE


def markers_in(text):
    found = []
    for name, rx in RELAY_MARKERS:
        if rx.search(text):
            found.append(name)
    return found


def ts(value):
    """Instante ISO-8601 con Z (con o sin fraccion de segundo) como datetime con zona; nunca comparacion de cadenas."""
    if not isinstance(value, str) or not value.endswith("Z"):
        raise InputError("instante sin Z: " + repr(value))
    try:
        return datetime.datetime.fromisoformat(value[:-1] + "+00:00")
    except ValueError:
        raise InputError("instante no ISO-8601: " + repr(value))


def require(cond, message):
    if not cond:
        raise InputError(message)


def ref_key(ref):
    if not isinstance(ref, dict):
        return None
    return (ref.get("Path"), ref.get("Blob"))


# ----------------------------------------------------------------------------------------------------------------------------- validacion

def validate(inp):
    """Rechaza (fallo cerrado) toda entrada incompleta o incoherente. Devuelve el contexto de la ventana ya comprobado."""
    require(isinstance(inp, dict), "la entrada no es un objeto JSON")
    require(inp.get("Schema") == SCHEMA_IN, "Schema distinto de " + SCHEMA_IN)
    for key in ("Unit", "Window", "Points", "Decisions", "BranchCommits", "RelayRecords", "ArtifactLineDigests", "HumanEvents",
                "SupervisionOnlyTokens", "Caps"):
        require(key in inp, "falta el campo obligatorio " + key)
    w = inp["Window"]
    require(isinstance(w, dict) and isinstance(w.get("KickoffEventSeq"), int), "Window.KickoffEventSeq obligatorio (entero)")
    require("Step7RecordVersion" in w and (w["Step7RecordVersion"] is None or isinstance(w["Step7RecordVersion"], int)),
            "Window.Step7RecordVersion obligatorio (entero, o null si el paso 7 no se alcanzo)")
    require(isinstance(w.get("PrincipalSession"), str) and w["PrincipalSession"], "Window.PrincipalSession obligatorio (cadena no vacia)")
    require(isinstance(w.get("KickoffTemplateSha256"), list) and all(isinstance(h, str) and HEX64_RX.match(h) for h in w["KickoffTemplateSha256"]),
            "Window.KickoffTemplateSha256 obligatorio (lista, posiblemente vacia, de SHA-256 en hex minuscula)")
    require("Step1Commit" in w, "Window.Step1Commit obligatorio ({Commit, PushedUtc}, o null si el paso 1 no se alcanzo)")
    s1 = w["Step1Commit"]
    if s1 is not None:
        require(isinstance(s1, dict) and isinstance(s1.get("Commit"), str) and s1["Commit"], "Window.Step1Commit.Commit obligatorio (cadena)")
        ts(s1.get("PushedUtc"))
    require(w["Step7RecordVersion"] is None or s1 is not None, "Window.Step7RecordVersion sin Window.Step1Commit: el paso 7 exige el paso 1")

    # Eventos humanos: el orden de Seq debe ser el orden temporal (no decreciente).
    events = inp["HumanEvents"]
    seqs = [e.get("Seq") for e in events]
    require(all(isinstance(s, int) for s in seqs), "todo HumanEvent necesita Seq entero")
    require(len(seqs) == len(set(seqs)), "Seq duplicado en HumanEvents")
    require(w["KickoffEventSeq"] in seqs, "KickoffEventSeq no corresponde a ningun HumanEvent")
    for e in events:
        require(e.get("Kind") in HUMAN_KINDS, "Kind desconocido en HumanEvent " + str(e.get("Seq")))
        require(e.get("Actor") in ACTORS, "Actor desconocido en HumanEvent " + str(e.get("Seq")))
        require(isinstance(e.get("Session"), str) and e["Session"], "Session obligatoria (cadena) en HumanEvent " + str(e.get("Seq")))
        require(e.get("ObservedSenderLabel") is None or isinstance(e.get("ObservedSenderLabel"), str),
                "ObservedSenderLabel debe ser null o cadena en HumanEvent " + str(e.get("Seq")))
        ts(e.get("Utc"))
        if e.get("Text") is not None:
            require(e.get("TextSha256") == sha256_text(e["Text"]), "TextSha256 no coincide con Text en HumanEvent " + str(e.get("Seq")))
    by_seq = sorted(events, key=lambda e: e["Seq"])
    for a, b in zip(by_seq, by_seq[1:]):
        require(ts(b["Utc"]) >= ts(a["Utc"]),
                "orden de Seq distinto del orden temporal entre HumanEvent %d y %d" % (a["Seq"], b["Seq"]))

    # Puntos: el orden de RecordVersion debe ser el orden temporal de los push (no decreciente).
    points = inp["Points"]
    rvs = [p.get("RecordVersion") for p in points]
    require(all(isinstance(r, int) for r in rvs), "todo punto necesita RecordVersion entero")
    require(len(rvs) == len(set(rvs)), "RecordVersion duplicado")
    for p in points:
        ts(p.get("PushedUtc"))
        require(isinstance(p.get("LoopPhase"), str) and p["LoopPhase"], "todo punto necesita LoopPhase (cadena)")
    by_rv = sorted(points, key=lambda p: p["RecordVersion"])
    for a, b in zip(by_rv, by_rv[1:]):
        require(ts(b["PushedUtc"]) >= ts(a["PushedUtc"]),
                "orden de RecordVersion distinto del orden temporal entre los puntos %d y %d" % (a["RecordVersion"], b["RecordVersion"]))

    for d in inp["Decisions"]:
        ts(d.get("PushedUtc"))
        require(d.get("Issuer") in ISSUERS, "Decisions[].Issuer obligatorio en " + "/".join(sorted(ISSUERS)) + ": " + repr(d.get("Commit")))
        require(isinstance(d.get("Markers", []), list) and all(isinstance(m, str) for m in d.get("Markers", [])),
                "Decisions[].Markers debe ser una lista de cadenas: " + repr(d.get("Commit")))
    # Commits de fx/u1 y de main del fixture con la identidad de su autor (AuthorRole), al menos los de la ventana.
    branch = inp["BranchCommits"]
    require(isinstance(branch, list), "BranchCommits debe ser una lista")
    seen_bc = set()
    for b in branch:
        require(isinstance(b, dict) and isinstance(b.get("Commit"), str) and b["Commit"], "BranchCommits[].Commit obligatorio (cadena)")
        require(isinstance(b.get("Ref"), str) and b["Ref"], "BranchCommits[].Ref obligatorio (cadena): " + repr(b.get("Commit")))
        require(b.get("AuthorRole") in ISSUERS, "BranchCommits[].AuthorRole obligatorio en " + "/".join(sorted(ISSUERS)) + ": " + repr(b.get("Commit")))
        ts(b.get("PushedUtc"))
        require((b["Commit"], b["Ref"]) not in seen_bc, "BranchCommits duplicado: " + repr((b["Commit"], b["Ref"])))
        seen_bc.add((b["Commit"], b["Ref"]))
    bc_by_commit = {}
    for b in branch:
        bc_by_commit.setdefault(b["Commit"], []).append(b)
    for d in inp["Decisions"]:
        for b in bc_by_commit.get(d.get("Commit"), []):
            require(b["AuthorRole"] == d["Issuer"],
                    "Decisions[].Issuer distinto del AuthorRole del mismo commit en BranchCommits (el Issuer es la identidad del autor del commit, "
                    "no el autor que declara el bloque): " + repr(d.get("Commit")))
    for r in inp["RelayRecords"]:
        require(r.get("Source") in PROOF_SOURCES, "RelayRecords[].Source obligatorio en " + "/".join(sorted(PROOF_SOURCES)) + ": " + repr(r.get("Path")))
    caps = inp["Caps"]
    require(isinstance(caps.get("PrincipalSessions"), int), "Caps.PrincipalSessions obligatorio (entero)")

    # Ventana: se abre en el instante del arranque y se cierra en el push del punto del paso 7.
    kickoff = next(e for e in events if e["Seq"] == w["KickoffEventSeq"])
    require(kickoff["Session"] == w["PrincipalSession"],
            "el arranque no es un evento de la sesion del Principal (Session distinta de Window.PrincipalSession)")
    if kickoff["Kind"] in ("MESSAGE", "ANSWER_TO_PRINCIPAL_QUESTION"):
        # Sin texto no se comprueban el literal, la plantilla neutra (TextSha256 solo se verifica contra Text) ni los tokens: fallo cerrado.
        require(kickoff.get("Text") is not None,
                "el arranque (Kind %s) necesita Text: sin texto no se comprueban el literal, la plantilla neutra ni los tokens" % kickoff["Kind"])
    open_t = ts(kickoff["Utc"])
    step7 = None
    close_t = None
    if w["Step7RecordVersion"] is not None:
        require(w["Step7RecordVersion"] in rvs, "Step7RecordVersion no corresponde a ningun punto")
        step7 = next(p for p in points if p["RecordVersion"] == w["Step7RecordVersion"])
        close_t = ts(step7["PushedUtc"])
        require(close_t > open_t, "ventana vacia o invertida: CloseUtc (push del paso 7) no es posterior a OpenUtc (arranque)")
        require(step7["LoopPhase"] == "ARCHITECT_SATISFIED",
                "el punto del paso 7 debe tener LoopPhase ARCHITECT_SATISFIED (ingestion de C); tiene " + repr(step7["LoopPhase"]))
        earlier = [p["RecordVersion"] for p in by_rv
                   if p["RecordVersion"] < step7["RecordVersion"] and ts(p["PushedUtc"]) > open_t and p["LoopPhase"] == "ARCHITECT_SATISFIED"]
        require(not earlier, "el punto del paso 7 no es el primero con ARCHITECT_SATISFIED en la ventana (antes: %s)" % earlier)
        window_rids = []
        for p in by_rv:
            if open_t < ts(p["PushedUtc"]) <= close_t:
                for a in p.get("Attempts", []):
                    rid = a.get("LogicalReviewRequestId")
                    if rid not in window_rids:
                        window_rids.append(rid)
        require(len(window_rids) >= 2, "la ventana no contiene dos solicitudes logicas (B y C)")
        require(any(a.get("State") == "RESULT_INGESTED" and a.get("LogicalReviewRequestId") != window_rids[0]
                    for a in step7.get("Attempts", [])),
                "el punto del paso 7 no ingiere (RESULT_INGESTED) un intento de la segunda solicitud logica")
    loop_points = [p for p in by_rv if p["LoopPhase"] != "NONE"]
    if loop_points:
        require(open_t < ts(loop_points[0]["PushedUtc"]),
                "el arranque no es anterior al primer punto con bucle activo (RecordVersion %d): precondicion de opcion A o arranque mal fijado"
                % loop_points[0]["RecordVersion"])

    # Paso 1 (D.8-1: commit de X v1 y push): ancla de la ventana. El arranque es el ultimo mensaje humano antes del paso 1.
    step1_t = None
    if s1 is not None:
        step1_t = ts(s1["PushedUtc"])
        require(open_t < step1_t, "el arranque no es anterior al push del paso 1 (Window.Step1Commit): arranque designado despues del paso 1")
        require(any(ts(b["PushedUtc"]) == step1_t for b in bc_by_commit.get(s1["Commit"], [])),
                "Window.Step1Commit no figura en BranchCommits con el mismo PushedUtc")
        if loop_points:
            require(step1_t <= ts(loop_points[0]["PushedUtc"]),
                    "el push del paso 1 (Window.Step1Commit) es posterior al primer punto con bucle activo (RecordVersion %d)"
                    % loop_points[0]["RecordVersion"])

    # Ambito del archivo de decisiones (v3.1): desde el commit de la RLA vigente (el ultimo del Coordinator con el marcador de autorizacion anterior
    # al arranque; P5) o desde el primer evento de la sesion del Principal si es anterior; sin RLA, desde ese primer evento. El Principal ya actua
    # antes del arranque (P8): un bloque I62-* suyo en ese intervalo lo aceptaria el lector de F4 igual que uno de la ventana.
    rla_commits = [d for d in inp["Decisions"]
                   if d["Issuer"] == "COORDINATOR" and any(m.startswith(AUTHORIZATION_MARKER) for m in d.get("Markers", []))
                   and ts(d["PushedUtc"]) < open_t]
    principal_events = sorted((e for e in events if e["Session"] == w["PrincipalSession"]), key=lambda e: (ts(e["Utc"]), e["Seq"]))
    scope_t, scope_utc, scope_basis = ts(principal_events[0]["Utc"]), principal_events[0]["Utc"], "PRINCIPAL_SESSION_FIRST_EVENT"
    if rla_commits:
        rla_last = max(rla_commits, key=lambda d: (ts(d["PushedUtc"]), str(d.get("Commit"))))
        if ts(rla_last["PushedUtc"]) <= scope_t:
            scope_t, scope_utc, scope_basis = ts(rla_last["PushedUtc"]), rla_last["PushedUtc"], "RLA_COMMIT"

    # Cobertura de BranchCommits (entrada incompleta = rechazo): todo commit de decisiones desde el ambito y todo punto desde el ambito hasta el paso 7.
    # Asi el Issuer de la RLA y de todo commit de decisiones del ambito se contrasta con el AuthorRole de su commit.
    for p in by_rv:
        t = ts(p["PushedUtc"])
        if scope_t <= t and (close_t is None or t <= close_t):
            require(p.get("Commit") in bc_by_commit,
                    "el punto %d (desde el commit de la RLA o el primer evento de la sesion del Principal hasta el paso 7) no figura en "
                    "BranchCommits (entrada incompleta)" % p["RecordVersion"])
    for d in inp["Decisions"]:
        if ts(d["PushedUtc"]) >= scope_t:
            require(d.get("Commit") in bc_by_commit,
                    "el commit de decisiones %r (desde el commit de la RLA o el primer evento de la sesion del Principal) no figura en "
                    "BranchCommits (entrada incompleta)" % d.get("Commit"))
    return {"kickoff": kickoff, "open_t": open_t, "step7": step7, "close_t": close_t, "step1_t": step1_t, "rla_commits": rla_commits,
            "scope_t": scope_t, "scope_utc": scope_utc, "scope_basis": scope_basis}


# ----------------------------------------------------------------------------------------------------------------------------- auditoria

def audit(inp):
    ctx = validate(inp)
    kickoff, open_t, step7, close_t = ctx["kickoff"], ctx["open_t"], ctx["step7"], ctx["close_t"]
    events = sorted(inp["HumanEvents"], key=lambda e: e["Seq"])
    points = sorted(inp["Points"], key=lambda p: p["RecordVersion"])

    def phase_of_instant(t):
        if t < open_t:
            return "PRE_WINDOW"
        if close_t is None or t <= close_t:
            return "IN_WINDOW"  # un instante igual al del arranque cuenta dentro (fallo cerrado)
        return "POST_WINDOW"

    point_phase = {p["RecordVersion"]: phase_of_instant(ts(p["PushedUtc"])) for p in points}
    window_rids = set()
    for p in points:
        if point_phase[p["RecordVersion"]] == "IN_WINDOW":
            for a in p.get("Attempts", []):
                window_rids.add(a.get("LogicalReviewRequestId"))

    artifact_index = {}
    for art in inp["ArtifactLineDigests"]:
        for d in art.get("LineSha256", []):
            artifact_index.setdefault(d, []).append({"Path": art.get("Path"), "Blob": art.get("Blob"), "Kind": art.get("Kind")})

    tokens = [t for t in inp["SupervisionOnlyTokens"] if isinstance(t, str) and t]

    gaps = []
    preexisting = []
    post_window_gaps = []
    flags = []
    dispositions = []
    classifications = []
    undetermined_reasons = []
    definite_owner_relay = False

    # ---- (1) custodia: registros AUTONOMY_GAP, deduplicados por (Path, Blob) y acotados a la ventana 1-7
    first_seen = {}
    for p in points:
        rv = p["RecordVersion"]
        ph = point_phase[rv]
        for g in p.get("AutonomyGaps", []):
            key = (g.get("Path"), g.get("Blob"))
            ref = {"Path": g.get("Path"), "Blob": g.get("Blob"), "RecordVersion": rv}
            if not g.get("ResolvesInTree", False):
                if ph == "PRE_WINDOW":
                    preexisting.append({"Ref": ref, "ResolvesInTree": False, "Note": "referencia colgante anterior al arranque"})
                else:
                    flags.append({"Code": "AUTONOMY_GAP_REF_DANGLING", "Ref": ref,
                                  "Rule": "I-S13: un StateRef que no resuelve en el arbol del punto no es custodia y no cuenta (F4 AutonomyGaps.Missing)"})
                continue
            rec = g.get("Record") or {}
            if rec.get("Kind") != "AUTONOMY_GAP":
                if ph != "PRE_WINDOW":
                    flags.append({"Code": "AUTONOMY_GAP_REF_NOT_A_RECORD", "Ref": ref, "Rule": "README §18.7: el registro custodiado debe ser un AUTONOMY_GAP"})
                continue
            if key not in first_seen:
                first_seen[key] = (rv, ph, rec)
    f4_all_supplied = any(v[2].get("RelayedBy") == "OWNER" for v in first_seen.values())
    custody_owner_gaps = []
    counted_any_gap = False
    for key in sorted(first_seen, key=lambda k: (first_seen[k][0], str(k[0]), str(k[1]))):
        rv, ph, rec = first_seen[key]
        lrid = rec.get("LogicalReviewRequestId", "UNKNOWN")
        relayed_by = rec.get("RelayedBy", "UNKNOWN")
        tied = lrid in window_rids
        ref = {"Path": key[0], "Blob": key[1], "FirstRecordVersion": rv, "FirstPointPhase": ph}
        if ph == "PRE_WINDOW" and not tied:
            preexisting.append({"Ref": ref, "ResolvesInTree": True, "RelayedBy": relayed_by, "LogicalReviewRequestId": lrid,
                                "Note": "heredado de antes del arranque; no cuenta en 1-7 (D.8; C-39)"})
            continue
        if ph == "POST_WINDOW" and not tied:
            post_window_gaps.append({"Ref": ref, "RelayedBy": relayed_by, "LogicalReviewRequestId": lrid})
            dispositions.append({"Ref": ref, "Class": "POST_WINDOW_AUTONOMY_GAP"})
            continue
        counted_any_gap = True
        entry = {
            "Source": "CUSTODY",
            "Scope": "WINDOW" if ph == "IN_WINDOW" else "TIED_TO_WINDOW_REQUEST",
            "Ref": ref,
            "Readme18_7": {
                "Kind": "AUTONOMY_GAP",
                "LogicalReviewRequestId": lrid,
                "Round": rec.get("Round", "UNKNOWN"),
                "RelayedBy": relayed_by,
                "Medium": rec.get("Medium", "UNKNOWN"),
                "Artifact": rec.get("Artifact", "UNKNOWN"),
                "Cause": rec.get("Cause", "UNKNOWN"),
            },
            "V14_20_9": {
                "RequiredRole": rec.get("RequiredRole", "UNKNOWN"),
                "RequiredAction": rec.get("RequiredAction", "UNKNOWN"),
                "MissingCapabilityOrAuthority": rec.get("MissingCapabilityOrAuthority", "UNKNOWN"),
                "AttemptedTransport": rec.get("AttemptedTransport", "UNKNOWN"),
                "WhyAutomaticRelayUnavailable": rec.get("WhyAutomaticRelayUnavailable", rec.get("Cause", "UNKNOWN")),
                "ManualFallbackUsed": rec.get("ManualFallbackUsed", rec.get("Medium", "UNKNOWN")),
            },
        }
        gaps.append(entry)
        if relayed_by == "OWNER":
            custody_owner_gaps.append(lrid)
        elif relayed_by != "COORDINATOR":
            # README §18.7: el registro dice quien relevo. Sin ese dato no se sabe si el Owner fue el bus (fallo cerrado; C-39 «causa exacta»).
            undetermined_reasons.append("AUTONOMY_GAP que cuenta en 1-7 con RelayedBy desconocido (%r): %s" % (relayed_by, str(key)))
            flags.append({"Code": "AUTONOMY_GAP_RELAYER_UNKNOWN", "Ref": ref, "RelayedBy": relayed_by,
                          "Rule": "README §18.7 (quien relevo); un hueco sin RelayedBy OWNER o COORDINATOR deja OWNER_AS_MESSAGE_BUS = UNDETERMINED"})
    f4_rule = len(custody_owner_gaps) > 0
    if f4_rule:
        definite_owner_relay = True

    # ---- (2) custodia: transporte de cada intento. La prueba del lanzamiento es un registro external-process de ARCHITECT con el mismo
    # RunId y custodiado por el propio intento (launch_evidence, o runtime_evidence cuando el intento paso de LAUNCHING a RESULT_RECEIVED).
    latest = {}
    refs_by_attempt = {}
    for p in points:
        for a in p.get("Attempts", []):
            key = (a.get("LogicalReviewRequestId"), a.get("AttemptSeq"))
            prev = latest.get(key)
            if prev is None or STATE_ORDER.get(a.get("State"), -1) >= STATE_ORDER.get(prev[1].get("State"), -1):
                latest[key] = (p["RecordVersion"], a)
            for field in ("LaunchEvidence", "RuntimeEvidence"):
                k = ref_key(a.get(field))
                if k is not None:
                    refs_by_attempt.setdefault(key, set()).add(k)
    transport_checks = []
    for key in sorted(latest, key=lambda k: (str(k[0]), k[1] if isinstance(k[1], int) else 0)):
        rv, a = latest[key]
        state = a.get("State")
        check = {"LogicalReviewRequestId": key[0], "AttemptSeq": key[1], "LastState": state, "LastRecordVersion": rv,
                 "RunId": a.get("RunId"), "Launch": "NOT_APPLICABLE", "LaunchProofSources": [], "Result": "NOT_APPLICABLE"}
        if state in LAUNCHED_STATES:
            owned = refs_by_attempt.get(key, set())
            rr = [r for r in inp["RelayRecords"]
                  if r.get("RunId") == a.get("RunId") and (r.get("Path"), r.get("Blob")) in owned
                  and r.get("Transport") == "external-process" and r.get("Role") == "ARCHITECT"]
            check["LaunchProofSources"] = sorted({r["Source"] for r in rr})
            if state == "LAUNCH_UNCERTAIN":
                check["Launch"] = "UNCERTAIN_BY_PROTOCOL"
            elif rr:
                check["Launch"] = "AUTOMATIC"
            else:
                check["Launch"] = "UNPROVEN"
                undetermined_reasons.append("lanzamiento sin registro external-process de ARCHITECT custodiado por el intento (launch_evidence o "
                                            "runtime_evidence) para " + str(key))
            res = a.get("Result")
            if res is not None:
                outs = {r.get("OutputSha256") for r in rr if r.get("OutputSha256")}
                if res.get("Sha256") in outs:
                    check["Result"] = "AUTOMATIC"
                else:
                    check["Result"] = "UNPROVEN"
                    undetermined_reasons.append("resultado custodiado sin igualdad con el OutputSha256 del lanzamiento para " + str(key))
        transport_checks.append(check)

    # ---- (3) commits del archivo de decisiones del fixture. El Issuer es la identidad del autor del commit (BranchCommits[].AuthorRole),
    # nunca el autor que declara el bloque: el lector de F4 acepta un bloque con el marcador sin mirar quien lo escribio.
    rla_before = ctx["rla_commits"]
    if not rla_before:
        flags.append({"Code": "RLA_NOT_CUSTODIED_BEFORE_WINDOW",
                      "Rule": "V14 §20.5 y D.8: la ReviewLoopAuthorization del Coordinator se emite antes; sin ella no hay materializacion autorizada (§20.5.1)"})
    scope_t = ctx["scope_t"]
    intermediate = []
    principal_commits = []
    for d in sorted(inp["Decisions"], key=lambda d: (ts(d["PushedUtc"]), str(d.get("Commit")))):
        t = ts(d["PushedUtc"])
        if t < scope_t:  # historia anterior a la RLA vigente y a la sesion del Principal (p. ej., la de FX-02)
            continue
        ph = phase_of_instant(t)  # un commit en el instante exacto del arranque cuenta dentro
        item = {"Commit": d.get("Commit"), "PushedUtc": d["PushedUtc"], "Issuer": d["Issuer"], "Markers": d.get("Markers", [])}
        protocol_markers = [m for m in d.get("Markers", []) if m.startswith(PROTOCOL_MARKER_PREFIXES)]
        if d["Issuer"] == "COORDINATOR":
            if ph == "IN_WINDOW":
                intermediate.append(item)
            continue
        if protocol_markers:
            # Desde el commit de la RLA (P5), no solo en 1-7: el Principal ya actua antes del arranque (P8).
            flags.append({"Code": "DECISIONS_FILE_MARKER_BY_NON_COORDINATOR", "Commit": d.get("Commit"), "Issuer": d["Issuer"], "Phase": ph,
                          "MarkerCount": len(protocol_markers),
                          "Rule": "V14 §20.2 AUT-I1 («La autonomía cambia el transporte y la orquestación, nunca la autoridad»); §20.5.1 («Nunca "
                                  "simula una decisión individual del Coordinator que no ocurrió»); 16.20 P-20; D.8 FAIL («materializa un binding fuera "
                                  "de los criterios»); ambito: desde el commit de la RLA vigente (P5) o el primer evento de la sesion del Principal"})
        if ph == "IN_WINDOW":
            if d["Issuer"] == "PRINCIPAL":
                principal_commits.append(item)
                dispositions.append({"Commit": d.get("Commit"), "Class": "DECISIONS_FILE_COMMIT_BY_PRINCIPAL"})
            else:
                flags.append({"Code": "DECISIONS_FILE_COMMIT_BY_NON_COORDINATOR_IN_WINDOW", "Commit": d.get("Commit"), "Issuer": d["Issuer"],
                              "Rule": "fallo cerrado: un commit del Owner u otro actor en el archivo de decisiones dentro de 1-7 exige disposicion"})
                dispositions.append({"Commit": d.get("Commit"), "Class": "DECISIONS_FILE_COMMIT_BY_" + d["Issuer"]})
        else:
            # Antes del arranque (desde la RLA) o despues del paso 7: el archivo de decisiones lo escribe el Coordinator del fixture
            # (evidence-schema.md §2); fallo cerrado.
            dispositions.append({"Commit": d.get("Commit"), "Class": "DECISIONS_FILE_COMMIT_BY_%s_%s" % (d["Issuer"], ph)})
    if intermediate:
        flags.append({"Code": "INTERMEDIATE_COORDINATOR_DECISION", "Count": len(intermediate),
                      "Rule": "V14 D.8 (PASS: sin decision intermedia del Coordinator en 1-7; paso 6); recipes.md FX-06"})

    # ---- (3b) todo commit de fx/u1 o de main del fixture dentro de 1-7 cuyo autor no es el Principal
    non_principal_commits = []
    for b in sorted(inp["BranchCommits"], key=lambda b: (ts(b["PushedUtc"]), str(b["Commit"]), str(b["Ref"]))):
        if phase_of_instant(ts(b["PushedUtc"])) != "IN_WINDOW" or b["AuthorRole"] == "PRINCIPAL":
            continue
        non_principal_commits.append({"Commit": b["Commit"], "Ref": b["Ref"], "PushedUtc": b["PushedUtc"], "AuthorRole": b["AuthorRole"]})
        flags.append({"Code": "NON_PRINCIPAL_COMMIT_IN_WINDOW", "Commit": b["Commit"], "Ref": b["Ref"], "AuthorRole": b["AuthorRole"],
                      "Rule": "V14 D.8 (pasos 1-7 todos del Principal; sin decision intermedia del Coordinator): un commit ajeno en la rama de la unidad "
                              "o en main del fixture (orden, cambio que fuerza un rebase) exige disposicion"})
        dispositions.append({"Commit": b["Commit"], "Class": "NON_PRINCIPAL_COMMIT_BY_" + b["AuthorRole"]})

    # ---- (4) eventos humanos (fase por instante; el arranque se identifica por Seq)
    principal_session = inp["Window"]["PrincipalSession"]
    template_hashes = set(inp["Window"]["KickoffTemplateSha256"])
    sessions_opened = 0
    manual_relays = []
    for e in events:
        seq, actor, kind, text = e["Seq"], e["Actor"], e["Kind"], e.get("Text")
        phase = "KICKOFF" if seq == kickoff["Seq"] else phase_of_instant(ts(e["Utc"]))
        c = {"Seq": seq, "Utc": e["Utc"], "Actor": actor, "Kind": kind, "Phase": phase, "TextSha256": e.get("TextSha256"),
             "Class": None, "OwnerAsMessageBusEffect": "NONE", "DispositionRequired": False, "Markers": [], "ArtifactMatches": []}
        if text is not None:
            c["Markers"] = markers_in(text)
            seen = set()
            for d in line_digests(text):
                for hit in artifact_index.get(d, []):
                    k2 = (hit["Path"], hit["Blob"])
                    if k2 not in seen:
                        seen.add(k2)
                        c["ArtifactMatches"].append(hit)
            leaked = [t for t in tokens if t in text]
            if leaked:
                flags.append({"Code": "SUPERVISION_ONLY_CONTENT_IN_HUMAN_EVENT", "Seq": seq, "TokenCount": len(leaked),
                              "Rule": "D.6 y orden: los esperados y el oraculo nunca llegan a una sesion del fixture"})
                c["DispositionRequired"] = True

        literal = kind == "MESSAGE" and text is not None and is_literal_continue(text)
        if kind in ("SESSION_OPENED", "SESSION_REOPENED"):
            if actor == "OWNER":
                if e["Session"] == principal_session:
                    sessions_opened += 1  # solo la sesion del Principal cuenta para su tope (D.8: 1 sesion + 1 reapertura)
                    c["Class"] = "OWNER_OPENED_SESSION"  # §49, §54: no es OWNER_AS_MESSAGE_BUS ni AUTONOMY_GAP
                elif phase == "IN_WINDOW":
                    # architect-invocation-contract.md §2: una sesion nueva abierta por el Owner en 1-7 es un relevo manual candidato
                    c["Class"] = "OWNER_OPENED_OTHER_SESSION_IN_LOOP"
                    c["OwnerAsMessageBusEffect"] = "UNDETERMINED"
                    c["DispositionRequired"] = True
                    undetermined_reasons.append("apertura por el Owner de una sesion distinta de la del Principal en la ventana: Seq " + str(seq))
                else:
                    c["Class"] = "OWNER_OPENED_OTHER_SESSION"  # fuera de 1-7; no cuenta para el tope del Principal
                if phase == "KICKOFF":
                    c["Class"] = "KICKOFF_NONSTANDARD"  # el arranque debe ser un mensaje
                    c["DispositionRequired"] = True
            else:
                c["Class"] = "SESSION_OPENED_BY_NON_OWNER"
                c["DispositionRequired"] = True
        elif actor == "SUPERVISION":
            # §50 autorizo el «continua» literal de la supervision para G0, QU y QH; la practica vigente no envia mensajes de la supervision a
            # sesiones del fixture (P-16 / D.6). Mientras OQ-20 no se decida: disposicion siempre; bandera solo con la etiqueta observada.
            c["DispositionRequired"] = True
            if phase in ("PRE_WINDOW", "KICKOFF"):
                c["Class"] = "FIXTURE_CONTROL_CONTINUE_PRE_WINDOW" if literal else "SUPERVISION_MESSAGE_PRE_WINDOW"
            elif phase == "IN_WINDOW":
                if literal:
                    c["Class"] = "SUPERVISION_CONTINUE_IN_LOOP"  # §50: «no hacen verdadero OWNER_AS_MESSAGE_BUS»; alcance en 1-7: OQ-04, OQ-20
                else:
                    c["Class"] = "SUPERVISION_MESSAGE_IN_LOOP"
                    c["OwnerAsMessageBusEffect"] = "UNDETERMINED"
                    undetermined_reasons.append("mensaje de la supervision distinto del «continúa» literal en la ventana: Seq " + str(seq))
            else:
                c["Class"] = "SUPERVISION_MESSAGE_POST_WINDOW"
            if e.get("ObservedSenderLabel"):
                flags.append({"Code": "SUPERVISION_LABEL_LEAK", "Seq": seq,
                              "Rule": "P-16 / D.6: etiqueta de una sesion del plano real observada en la sesion del fixture (ObservedSenderLabel)"})
        elif actor == "OTHER":
            c["Class"] = "UNATTRIBUTED_HUMAN_EVENT"
            c["DispositionRequired"] = True
            if phase in ("IN_WINDOW", "KICKOFF"):
                c["OwnerAsMessageBusEffect"] = "UNDETERMINED"
                undetermined_reasons.append("evento humano no atribuible en la ventana: Seq " + str(seq))
        elif phase == "KICKOFF":
            c["Class"] = "KICKOFF"
            if c["Markers"] or c["ArtifactMatches"]:
                c["DispositionRequired"] = True
                c["Class"] = "KICKOFF_WITH_PROTOCOL_CONTENT"
            elif not (literal or (kind == "MESSAGE" and e.get("TextSha256") in template_hashes)):
                # README §2.1 y §5: el arranque es el literal «continua» (o la plantilla neutra, por su SHA-256); otro contenido o una respuesta
                # a una pregunta del Principal puede llevar una decision (V14 §20.1) o una instruccion.
                c["DispositionRequired"] = True
                c["Class"] = "KICKOFF_NONSTANDARD"
        elif phase == "PRE_WINDOW":
            if literal:
                c["Class"] = "FIXTURE_CONTROL_CONTINUE_PRE_WINDOW"  # practica vigente (Owner); §54; alcance de §50 en OQ-20
            else:
                c["Class"] = "PRE_WINDOW_HUMAN_EVENT"
        elif phase == "POST_WINDOW":
            c["Class"] = "POST_WINDOW_HUMAN_EVENT"  # paso 8: decidir si procede; fuera de 1-7
        else:  # IN_WINDOW, actor OWNER
            if c["ArtifactMatches"]:
                c["Class"] = "MANUAL_RELAY"
                c["OwnerAsMessageBusEffect"] = "TRUE"
                definite_owner_relay = True
                manual_relays.append(c)
            elif literal:
                c["Class"] = "CONTINUE_STIMULUS_IN_LOOP"
                c["DispositionRequired"] = True  # su alcance en 1-7 no esta fijado (OQ-04)
            elif c["Markers"]:
                c["Class"] = "PROTOCOL_CONTENT_IN_LOOP"
                c["OwnerAsMessageBusEffect"] = "UNDETERMINED"
                c["DispositionRequired"] = True
                undetermined_reasons.append("contenido del protocolo en un mensaje humano de la ventana sin coincidencia con artefactos: Seq " + str(seq))
            elif kind == "ANSWER_TO_PRINCIPAL_QUESTION":
                c["Class"] = "HUMAN_DECISION_IN_LOOP"
                c["OwnerAsMessageBusEffect"] = "UNDETERMINED"
                c["DispositionRequired"] = True
                undetermined_reasons.append("respuesta humana a una pregunta del Principal en la ventana: Seq " + str(seq))
            elif kind == "TOOL_APPROVAL":
                c["Class"] = "OWNER_CLICK_IN_LOOP"
                c["OwnerAsMessageBusEffect"] = "UNDETERMINED"
                c["DispositionRequired"] = True
                undetermined_reasons.append("aprobacion humana de una herramienta en la ventana: Seq " + str(seq))
            elif kind == "EFFORT_CHANGE":
                c["Class"] = "RUNTIME_CONTROL_IN_LOOP"
                c["DispositionRequired"] = True
            elif kind == "SESSION_STOPPED":
                c["Class"] = "SESSION_STOPPED_IN_LOOP"
                c["DispositionRequired"] = True
            else:
                c["Class"] = "OWNER_INTERVENTION_UNCLASSIFIED"
                c["OwnerAsMessageBusEffect"] = "UNDETERMINED"
                c["DispositionRequired"] = True
                undetermined_reasons.append("intervencion humana sin clasificar en la ventana: Seq " + str(seq))
        if c["DispositionRequired"]:
            dispositions.append({"Seq": seq, "Class": c["Class"]})
        classifications.append(c)

    if sessions_opened > inp["Caps"]["PrincipalSessions"]:
        flags.append({"Code": "PRINCIPAL_SESSION_CAP_EXCEEDED", "Opened": sessions_opened, "Cap": inp["Caps"]["PrincipalSessions"],
                      "Session": principal_session, "Rule": "V14 D.8: Principal A 1 sesion + 1 reapertura, tope 2"})

    # ---- (5) AUTONOMY_GAP derivados de los mensajes y P-21
    missing = []
    for c in manual_relays:
        lids = sorted(set(LRID_RX.findall(next(e for e in events if e["Seq"] == c["Seq"]).get("Text") or "")))
        art = c["ArtifactMatches"][0]
        gaps.append({
            "Source": "HUMAN_EVENT",
            "Seq": c["Seq"],
            "Readme18_7": {"Kind": "AUTONOMY_GAP", "LogicalReviewRequestId": lids[0] if lids else "UNKNOWN", "Round": "UNKNOWN",
                           "RelayedBy": "OWNER", "Medium": "mensaje humano a la sesion del Principal", "Artifact": {"Path": art["Path"], "Blob": art["Blob"]},
                           "Cause": "UNKNOWN"},
            "V14_20_9": {"RequiredRole": "UNKNOWN", "RequiredAction": "UNKNOWN", "MissingCapabilityOrAuthority": "UNKNOWN",
                         "AttemptedTransport": "UNKNOWN", "WhyAutomaticRelayUnavailable": "UNKNOWN",
                         "ManualFallbackUsed": "mensaje humano con lineas de " + str(art["Kind"])},
        })
        covered = any(l in custody_owner_gaps for l in lids) if lids else len(custody_owner_gaps) > 0
        if not covered:
            missing.append({"Seq": c["Seq"], "LogicalReviewRequestIds": lids or ["UNKNOWN"],
                            "Rule": "P-21 (16.29): relevo manual sin registro AUTONOMY_GAP custodiado; la evidencia no vale para el criterio 15"})

    # ---- (6) agregados
    if definite_owner_relay:
        bus = "TRUE"
    elif undetermined_reasons:
        bus = "UNDETERMINED"
    else:
        bus = "FALSE"
    blocking_flags = [f["Code"] for f in flags]
    reasons_not_eligible = []
    if step7 is None:
        reasons_not_eligible.append("paso 7 no alcanzado (Window.Step7RecordVersion = null): ventana abierta hasta el ultimo evento")
    if bus != "FALSE":
        reasons_not_eligible.append("OWNER_AS_MESSAGE_BUS = " + bus)
    if counted_any_gap:
        reasons_not_eligible.append("AUTONOMY_GAP custodiado en 1-7 (C-37: el criterio 15 no es PASS)")
    if missing:
        reasons_not_eligible.append("P-21: relevos manuales sin registro")
    for code in blocking_flags:
        reasons_not_eligible.append("bandera " + code)
    if dispositions:
        reasons_not_eligible.append("elementos que exigen disposicion del Coordinator: " + str(len(dispositions)))
    result = {
        "Schema": SCHEMA_OUT,
        "AuditorVersion": AUDITOR_VERSION,
        "Unit": inp["Unit"],
        "Window": {"KickoffEventSeq": kickoff["Seq"], "OpenUtc": kickoff["Utc"],
                   "Step1Commit": inp["Window"]["Step1Commit"], "PrincipalSession": principal_session,
                   "Step7RecordVersion": step7["RecordVersion"] if step7 else None, "CloseUtc": step7["PushedUtc"] if step7 else None,
                   "DecisionsFileScopeUtc": ctx["scope_utc"], "DecisionsFileScopeBasis": ctx["scope_basis"],
                   "PhaseBasis": "instantes (Utc de los eventos, PushedUtc de los puntos y commits); el Seq solo identifica el arranque"},
        "OWNER_AS_MESSAGE_BUS": bus,
        "OwnerAsMessageBusF4Rule": f4_rule,
        "OwnerAsMessageBusF4RuleAllSuppliedPoints": f4_all_supplied,
        "UndeterminedReasons": undetermined_reasons,
        "AutonomyGaps": gaps,
        "PreexistingAutonomyGaps": preexisting,
        "PostWindowAutonomyGaps": post_window_gaps,
        "MissingAutonomyGapRecords": missing,
        "IntermediateCoordinatorDecisions": intermediate,
        "PrincipalCommitsToDecisionsFileInWindow": principal_commits,
        "NonPrincipalCommitsInWindow": non_principal_commits,
        "TransportChecks": transport_checks,
        "HumanEventClassification": classifications,
        "DispositionRequired": dispositions,
        "Flags": flags,
        "PrincipalSessionsOpened": sessions_opened,
        "PassEligibleOnTransport": not reasons_not_eligible,
        "ReasonsNotEligible": reasons_not_eligible,
        "Note": "Elegibilidad por transporte, no un resultado de FX-06: el PASS exige ademas los pasos 1-8 completos (D.8) y lo decide el Coordinator.",
    }
    return result


def dumps(obj):
    return json.dumps(obj, ensure_ascii=False, indent=1) + "\n"


# ----------------------------------------------------------------------------------------------------------------------------- autoprueba
# Solo datos sinteticos: ningun identificador, hash ni texto de una corrida real.

def _ev(seq, utc, kind, text=None, actor="OWNER", session="SYN-PRINCIPAL", label=None):
    e = {"Seq": seq, "Utc": utc, "Session": session, "Actor": actor, "Kind": kind, "Text": text,
         "TextSha256": sha256_text(text) if text is not None else None, "ObservedSenderLabel": label}
    return e


SYN_RESULT_LINES = [
    "  \"Verdict\": \"CHANGES REQUIRED\",",
    "  \"AffectedSection\": \"§2 Contrato del ejemplo sintetico\",",
    "  \"CorrectionRequired\": \"alinear el contrato sintetico con sus pruebas\",",
]
L1 = "L20300101T100000Z-0001"
L2 = "L20300101T110000Z-0002"


def _base():
    result_sha_b = "a" * 64
    result_sha_c = "b" * 64
    return {
        "Schema": SCHEMA_IN,
        "Unit": "SYN-U9",
        "SupervisionOnlyTokens": ["SYNTHETIC-ORACLE-TOKEN-0001"],
        "Window": {"KickoffEventSeq": 3, "Step7RecordVersion": 18, "PrincipalSession": "SYN-PRINCIPAL", "KickoffTemplateSha256": [],
                   "Step1Commit": {"Commit": "9" * 40, "PushedUtc": "2030-01-01T10:00:00Z"}},
        "Points": [
            {"RecordVersion": 9, "Commit": "0" * 40, "Point": "QH", "PushedUtc": "2030-01-01T09:20:00Z", "LoopPhase": "NONE",
             "AutonomyGaps": [], "Attempts": []},
            {"RecordVersion": 10, "Commit": "1" * 40, "Point": "QU", "PushedUtc": "2030-01-01T10:05:00Z", "LoopPhase": "REVIEW_PENDING",
             "AutonomyGaps": [], "Attempts": [{"LogicalReviewRequestId": L1, "AttemptSeq": 1, "State": "BUDGET_RESERVED",
                                                "RunId": None, "LaunchEvidence": None, "RuntimeEvidence": None, "Result": None}]},
            {"RecordVersion": 13, "Commit": "2" * 40, "Point": "QU", "PushedUtc": "2030-01-01T10:30:00Z", "LoopPhase": "CORRECTING",
             "AutonomyGaps": [], "Attempts": [{"LogicalReviewRequestId": L1, "AttemptSeq": 1, "State": "RESULT_INGESTED",
                                                "RunId": "R20300101T100600Z-0001", "LaunchEvidence": {"Path": "syn/l1/1/launch.json", "Blob": "3" * 40},
                                                "RuntimeEvidence": None,
                                                "Result": {"Path": "syn/l1/1/result.json", "Blob": "4" * 40, "Sha256": result_sha_b}}]},
            {"RecordVersion": 18, "Commit": "5" * 40, "Point": "QU", "PushedUtc": "2030-01-01T11:30:00Z", "LoopPhase": "ARCHITECT_SATISFIED",
             "AutonomyGaps": [], "Attempts": [{"LogicalReviewRequestId": L2, "AttemptSeq": 1, "State": "RESULT_INGESTED",
                                                "RunId": "R20300101T110500Z-0002", "LaunchEvidence": {"Path": "syn/l2/1/launch.json", "Blob": "6" * 40},
                                                "RuntimeEvidence": None,
                                                "Result": {"Path": "syn/l2/1/result.json", "Blob": "7" * 40, "Sha256": result_sha_c}}]},
        ],
        "Decisions": [{"Commit": "8" * 40, "PushedUtc": "2030-01-01T09:00:00Z", "Markers": ["I62-REVIEW-LOOP-AUTHORIZATION: SYN-RLA-1"],
                       "Issuer": "COORDINATOR"}],
        "BranchCommits": [
            {"Commit": "8" * 40, "Ref": "refs/heads/fx/u1", "PushedUtc": "2030-01-01T09:00:00Z", "AuthorRole": "COORDINATOR"},
            {"Commit": "0" * 40, "Ref": "refs/heads/fx/u1", "PushedUtc": "2030-01-01T09:20:00Z", "AuthorRole": "PRINCIPAL"},
            {"Commit": "9" * 40, "Ref": "refs/heads/fx/u1", "PushedUtc": "2030-01-01T10:00:00Z", "AuthorRole": "PRINCIPAL"},
            {"Commit": "1" * 40, "Ref": "refs/heads/fx/u1", "PushedUtc": "2030-01-01T10:05:00Z", "AuthorRole": "PRINCIPAL"},
            {"Commit": "2" * 40, "Ref": "refs/heads/fx/u1", "PushedUtc": "2030-01-01T10:30:00Z", "AuthorRole": "PRINCIPAL"},
            {"Commit": "ab" * 20, "Ref": "refs/heads/fx/u1", "PushedUtc": "2030-01-01T10:45:00Z", "AuthorRole": "PRINCIPAL"},
            {"Commit": "5" * 40, "Ref": "refs/heads/fx/u1", "PushedUtc": "2030-01-01T11:30:00Z", "AuthorRole": "PRINCIPAL"},
        ],
        "RelayRecords": [
            {"Path": "syn/l1/1/launch.json", "Blob": "3" * 40, "Source": "RELAY_RECORD_V2", "RunId": "R20300101T100600Z-0001", "Role": "ARCHITECT",
             "AdapterId": "codex-cli", "Transport": "external-process", "OutcomeKind": "COMPLETED", "OutputSha256": result_sha_b},
            {"Path": "syn/l2/1/launch.json", "Blob": "6" * 40, "Source": "RELAY_RECORD_V2", "RunId": "R20300101T110500Z-0002", "Role": "ARCHITECT",
             "AdapterId": "codex-cli", "Transport": "external-process", "OutcomeKind": "COMPLETED", "OutputSha256": result_sha_c},
        ],
        "ArtifactLineDigests": [{"Path": "syn/l1/1/result.json", "Blob": "4" * 40, "Kind": "ROLE_RESULT",
                                 "LineSha256": [sha256_text(norm_line(l)) for l in SYN_RESULT_LINES]}],
        "HumanEvents": [
            _ev(1, "2030-01-01T09:30:00Z", "SESSION_OPENED"),
            _ev(2, "2030-01-01T09:40:00Z", "MESSAGE", "Eres el Principal de la unidad de prueba SYN-U9 de este repositorio."),
            _ev(3, "2030-01-01T09:50:00Z", "MESSAGE", "continúa"),
        ],
        "Caps": {"PrincipalSessions": 2},
    }


def _gap_record(relayed_by, lid):
    return {"Kind": "AUTONOMY_GAP", "LogicalReviewRequestId": lid, "Round": 1, "RelayedBy": relayed_by, "Medium": "sintetico",
            "Artifact": {"Path": "syn/l1/1/result.json", "Blob": "4" * 40}, "Cause": "sintetico"}


def _gap(relayed_by, lid, resolves=True, path="syn/l1/1/autonomy-gap.json", blob="9" * 40):
    return {"Path": path, "Blob": blob, "ResolvesInTree": resolves, "Record": _gap_record(relayed_by, lid)}


def _point(i, rv):
    return next(p for p in i["Points"] if p["RecordVersion"] == rv)


def _decision(i, commit, utc, markers, issuer, branch=True, author=None):
    """Commit del archivo de decisiones; con branch=True figura tambien en BranchCommits (AuthorRole = issuer salvo author)."""
    i["Decisions"].append({"Commit": commit, "PushedUtc": utc, "Markers": markers, "Issuer": issuer})
    if branch:
        i["BranchCommits"].append({"Commit": commit, "Ref": "refs/heads/fx/u1", "PushedUtc": utc, "AuthorRole": author or issuer})


def _cases():
    cases = []
    rejects = []

    def case(name, mutate, expect):
        inp = _base()
        mutate(inp)
        cases.append((name, inp, expect))

    def reject(name, mutate, expect_substring):
        inp = _base()
        mutate(inp)
        rejects.append((name, inp, expect_substring))

    case("T01 corrida limpia", lambda i: None,
         {"OWNER_AS_MESSAGE_BUS": "FALSE", "PassEligibleOnTransport": True, "OwnerAsMessageBusF4Rule": False})

    def t02(i):
        txt = "Esto devolvio el Architect:\n" + "\n".join(SYN_RESULT_LINES)
        i["HumanEvents"].append(_ev(4, "2030-01-01T10:20:00Z", "MESSAGE", txt))
    case("T02 el Owner pega lineas de un resultado en la ventana", t02,
         {"OWNER_AS_MESSAGE_BUS": "TRUE", "PassEligibleOnTransport": False, "MissingCount": 1, "ClassOf4": "MANUAL_RELAY"})

    def t03(i):
        _point(i, 13)["AutonomyGaps"] = [_gap("OWNER", L1)]
    case("T03 custodia con AUTONOMY_GAP RelayedBy OWNER en la ventana (regla de F4)", t03,
         {"OWNER_AS_MESSAGE_BUS": "TRUE", "OwnerAsMessageBusF4Rule": True, "PassEligibleOnTransport": False})

    def t04(i):
        _point(i, 13)["AutonomyGaps"] = [_gap("COORDINATOR", L1)]
    case("T04 AUTONOMY_GAP del Coordinator: no es el Owner como bus, pero no es PASS", t04,
         {"OWNER_AS_MESSAGE_BUS": "FALSE", "OwnerAsMessageBusF4Rule": False, "PassEligibleOnTransport": False})

    def t05(i):
        i["HumanEvents"].insert(0, _ev(0, "2030-01-01T09:10:00Z", "SESSION_OPENED"))
    case("T05 dos sesiones abiertas por el Owner antes de la ventana", t05,
         {"OWNER_AS_MESSAGE_BUS": "FALSE", "PassEligibleOnTransport": True, "Sessions": 2})

    def t06(i):
        i["HumanEvents"].append(_ev(4, "2030-01-01T10:40:00Z", "MESSAGE", "  Continúa.  "))
    case("T06 «continúa» literal dentro de la ventana", t06,
         {"OWNER_AS_MESSAGE_BUS": "FALSE", "PassEligibleOnTransport": False, "ClassOf4": "CONTINUE_STIMULUS_IN_LOOP"})

    def t07(i):
        i["HumanEvents"].append(_ev(4, "2030-01-01T10:40:00Z", "MESSAGE", "sigue con la revision"))
    case("T07 texto libre del Owner en la ventana", t07,
         {"OWNER_AS_MESSAGE_BUS": "UNDETERMINED", "PassEligibleOnTransport": False, "ClassOf4": "OWNER_INTERVENTION_UNCLASSIFIED"})

    def t08(i):
        _decision(i, "a" * 40, "2030-01-01T10:50:00Z", ["FIXTURE-ORDER: SYN-O5"], "COORDINATOR")
    case("T08 decision intermedia del Coordinator en la ventana", t08,
         {"OWNER_AS_MESSAGE_BUS": "FALSE", "PassEligibleOnTransport": False, "Intermediate": 1})

    def t09(i):
        i["RelayRecords"][1]["OutputSha256"] = "c" * 64
    case("T09 resultado custodiado distinto de la salida del lanzamiento", t09,
         {"OWNER_AS_MESSAGE_BUS": "UNDETERMINED", "PassEligibleOnTransport": False})

    def t10(i):
        i["HumanEvents"].append(_ev(4, "2030-01-01T10:40:00Z", "MESSAGE", "continúa", actor="SUPERVISION", label="SYN-REAL-PLANE-TITLE"))
    case("T10 mensaje de la supervision con la etiqueta de una sesion real observada en el fixture", t10,
         {"PassEligibleOnTransport": False, "FlagPresent": "SUPERVISION_LABEL_LEAK"})

    def t11(i):
        i["HumanEvents"][2] = _ev(3, "2030-01-01T09:50:00Z", "MESSAGE", "continúa. Referencia SYNTHETIC-ORACLE-TOKEN-0001")
    case("T11 contenido solo de supervision en el arranque", t11,
         {"PassEligibleOnTransport": False, "FlagPresent": "SUPERVISION_ONLY_CONTENT_IN_HUMAN_EVENT"})

    def t12(i):
        _point(i, 13)["AutonomyGaps"] = [_gap("OWNER", L1)]
        txt = "Para " + L1 + ":\n" + SYN_RESULT_LINES[1]
        i["HumanEvents"].append(_ev(4, "2030-01-01T10:20:00Z", "MESSAGE", txt))
    case("T12 relevo manual registrado (P-21 cubierto)", t12,
         {"OWNER_AS_MESSAGE_BUS": "TRUE", "MissingCount": 0, "PassEligibleOnTransport": False})

    def t13(i):
        _point(i, 13)["AutonomyGaps"] = [_gap("OWNER", L1, resolves=False)]
    case("T13 referencia AUTONOMY_GAP colgante en la ventana: no cuenta, pero se marca", t13,
         {"OwnerAsMessageBusF4Rule": False, "FlagPresent": "AUTONOMY_GAP_REF_DANGLING", "PassEligibleOnTransport": False})

    def t14(i):
        i["HumanEvents"].append(_ev(4, "2030-01-01T10:10:00Z", "TOOL_APPROVAL", None))
    case("T14 aprobacion humana de herramienta en la ventana", t14,
         {"OWNER_AS_MESSAGE_BUS": "UNDETERMINED", "ClassOf4": "OWNER_CLICK_IN_LOOP", "PassEligibleOnTransport": False})

    def t15(i):
        i["HumanEvents"].insert(0, _ev(0, "2030-01-01T09:10:00Z", "SESSION_REOPENED"))
        i["HumanEvents"].insert(0, _ev(-1, "2030-01-01T08:00:00Z", "SESSION_OPENED"))
    case("T15 tope de sesiones de Principal superado", t15,
         {"FlagPresent": "PRINCIPAL_SESSION_CAP_EXCEEDED", "PassEligibleOnTransport": False})

    def t16(i):
        i["Decisions"] = []
    case("T16 sin ReviewLoopAuthorization antes de la ventana", t16,
         {"FlagPresent": "RLA_NOT_CUSTODIED_BEFORE_WINDOW", "PassEligibleOnTransport": False})

    def t17(i):
        i["HumanEvents"].append(_ev(4, "2030-01-01T10:40:00Z", "ANSWER_TO_PRINCIPAL_QUESTION", "Mantener la opcion recomendada"))
    case("T17 respuesta del Owner a una pregunta del Principal en la ventana", t17,
         {"OWNER_AS_MESSAGE_BUS": "UNDETERMINED", "ClassOf4": "HUMAN_DECISION_IN_LOOP", "PassEligibleOnTransport": False})

    def t18(i):
        i["HumanEvents"].append(_ev(4, "2030-01-01T12:00:00Z", "MESSAGE", "decido la opcion A"))
    case("T18 mensaje posterior al paso 7 (paso 8: decision)", t18,
         {"OWNER_AS_MESSAGE_BUS": "FALSE", "PassEligibleOnTransport": True, "ClassOf4": "POST_WINDOW_HUMAN_EVENT"})

    def t19(i):
        i["HumanEvents"].append(_ev(4, "2030-01-01T10:40:00Z", "MESSAGE", "revisa I20300101T110000Z-0002"))
    case("T19 identificador del protocolo sin artefacto en la ventana", t19,
         {"OWNER_AS_MESSAGE_BUS": "UNDETERMINED", "ClassOf4": "PROTOCOL_CONTENT_IN_LOOP", "PassEligibleOnTransport": False})

    def t20(i):
        p18 = _point(i, 18)
        p18["Attempts"][0]["AttemptSeq"] = 2
        p18["Attempts"].insert(0, {"LogicalReviewRequestId": L2, "AttemptSeq": 1, "State": "LAUNCH_UNCERTAIN", "RunId": "UNKNOWN",
                                   "LaunchEvidence": None, "RuntimeEvidence": None, "Result": None})
    case("T20 LAUNCH_UNCERTAIN: incierto por protocolo, no transporte humano", t20,
         {"OWNER_AS_MESSAGE_BUS": "FALSE", "PassEligibleOnTransport": True})

    def t23(i):
        # 250 ms despues del push del punto del paso 7: fuera de la ventana aunque como cadena «...00.250Z» < «...00Z».
        i["HumanEvents"].append(_ev(4, "2030-01-01T11:30:00.250Z", "MESSAGE", "sigue con la revision"))
    case("T23 instantes con fraccion de segundo (comparacion temporal, no de cadenas)", t23,
         {"OWNER_AS_MESSAGE_BUS": "FALSE", "ClassOf4": "POST_WINDOW_HUMAN_EVENT", "PassEligibleOnTransport": True})

    def t24(i):
        _point(i, 9)["AutonomyGaps"] = [_gap("OWNER", "L20291231T100000Z-0009", path="syn/prev/autonomy-gap.json", blob="e" * 40)]
        for rv in (10, 13, 18):
            _point(i, rv)["AutonomyGaps"] = [_gap("OWNER", "L20291231T100000Z-0009", path="syn/prev/autonomy-gap.json", blob="e" * 40)]
    case("T24 AUTONOMY_GAP heredado de antes del arranque y arrastrado: se enumera y no cuenta en 1-7", t24,
         {"OWNER_AS_MESSAGE_BUS": "FALSE", "OwnerAsMessageBusF4Rule": False, "PassEligibleOnTransport": True, "GapCount": 0,
          "PreexistingCount": 1, "F4AllSupplied": True})

    def t25(i):
        p19 = {"RecordVersion": 19, "Commit": "f" * 40, "Point": "QU", "PushedUtc": "2030-01-01T11:40:00Z", "LoopPhase": "NONE",
               "AutonomyGaps": [_gap("COORDINATOR", "UNKNOWN", path="syn/post/autonomy-gap.json", blob="d" * 40)], "Attempts": []}
        i["Points"].append(p19)
    case("T25 AUTONOMY_GAP nuevo despues del paso 7 y ajeno a la ventana: disposicion, no bus", t25,
         {"OWNER_AS_MESSAGE_BUS": "FALSE", "PassEligibleOnTransport": False, "GapCount": 0, "PostWindowCount": 1})

    def t26(i):
        p13 = _point(i, 13)
        p13["Attempts"][0]["LaunchEvidence"] = None
        p13["Attempts"][0]["RuntimeEvidence"] = {"Path": "syn/l1/1/runtime-evidence.json", "Blob": "c" * 40}
        i["RelayRecords"][0].update({"Path": "syn/l1/1/runtime-evidence.json", "Blob": "c" * 40, "Source": "RUNTIME_EVIDENCE"})
    case("T26 LAUNCHING -> RESULT_RECEIVED sin launch_evidence: prueba desde runtime_evidence", t26,
         {"OWNER_AS_MESSAGE_BUS": "FALSE", "PassEligibleOnTransport": True, "LaunchOfL1": "AUTOMATIC"})

    def t27(i):
        i["RelayRecords"][0]["Blob"] = "b" * 40  # mismo RunId, pero no es el registro que custodia el intento
    case("T27 registro de lanzamiento no custodiado por el intento: no prueba el lanzamiento", t27,
         {"OWNER_AS_MESSAGE_BUS": "UNDETERMINED", "PassEligibleOnTransport": False, "LaunchOfL1": "UNPROVEN"})

    def t28(i):
        _decision(i, "c" * 40, "2030-01-01T10:50:00Z", [], "PRINCIPAL")
    case("T28 commit del Principal en el archivo de decisiones en la ventana: no es decision del Coordinator, pero exige disposicion "
         "(sustituye al T28 de v2, que lo daba por elegible)", t28,
         {"OWNER_AS_MESSAGE_BUS": "FALSE", "PassEligibleOnTransport": False, "Intermediate": 0,
          "DispositionClass": "DECISIONS_FILE_COMMIT_BY_PRINCIPAL", "FlagAbsent": "DECISIONS_FILE_MARKER_BY_NON_COORDINATOR"})

    def t29(i):
        _decision(i, "c" * 40, "2030-01-01T10:50:00Z", [], "OWNER")
    case("T29 commit del Owner en el archivo de decisiones en la ventana: disposicion", t29,
         {"PassEligibleOnTransport": False, "Intermediate": 0, "FlagPresent": "DECISIONS_FILE_COMMIT_BY_NON_COORDINATOR_IN_WINDOW"})

    def t30(i):
        i["Decisions"][0]["Issuer"] = "OWNER"
        i["BranchCommits"][0]["AuthorRole"] = "OWNER"
    case("T30 RLA que no emite el Coordinator: no cuenta como RLA", t30,
         {"PassEligibleOnTransport": False, "FlagPresent": "RLA_NOT_CUSTODIED_BEFORE_WINDOW"})

    def t31(i):
        _point(i, 13)["AutonomyGaps"] = [_gap("OWNER", L1)]
        _point(i, 18)["AutonomyGaps"] = [_gap("OWNER", L1)]
    case("T31 el mismo AUTONOMY_GAP en varios puntos cuenta una vez", t31,
         {"OWNER_AS_MESSAGE_BUS": "TRUE", "GapCount": 1, "PassEligibleOnTransport": False})

    def t32(i):
        i["Window"]["Step7RecordVersion"] = None
        i["HumanEvents"].append(_ev(4, "2030-01-01T12:00:00Z", "MESSAGE", "decido la opcion A"))
    case("T32 paso 7 no alcanzado: ventana abierta y no elegible", t32,
         {"OWNER_AS_MESSAGE_BUS": "UNDETERMINED", "PassEligibleOnTransport": False, "ClassOf4": "OWNER_INTERVENTION_UNCLASSIFIED"})

    # v3 (revision R2): archivo de decisiones y commits ajenos al Principal en la ventana
    def t41(i):
        _decision(i, "c" * 40, "2030-01-01T10:50:00Z",
                  ["I62-REVIEW-LOOP-AUTHORIZATION: SYN-RLA-2", "I62-PRINCIPAL-BINDING: B20300101T104500Z-0003 ACCEPTED"], "PRINCIPAL")
    case("T41 el Principal escribe marcadores I62-* en el archivo de decisiones dentro de la ventana (autorizacion falsificada)", t41,
         {"PassEligibleOnTransport": False, "Intermediate": 0, "FlagPresent": "DECISIONS_FILE_MARKER_BY_NON_COORDINATOR"})

    def t42(i):
        i["BranchCommits"].append({"Commit": "cd" * 20, "Ref": "refs/heads/fx/u1", "PushedUtc": "2030-01-01T10:50:00Z",
                                   "AuthorRole": "COORDINATOR"})
    case("T42 commit del Coordinator en fx/u1 fuera del archivo de decisiones dentro de la ventana", t42,
         {"PassEligibleOnTransport": False, "Intermediate": 0, "FlagPresent": "NON_PRINCIPAL_COMMIT_IN_WINDOW"})

    def t43(i):
        i["BranchCommits"].append({"Commit": "ef" * 20, "Ref": "refs/heads/main", "PushedUtc": "2030-01-01T10:50:00Z", "AuthorRole": "OWNER"})
    case("T43 commit del Owner en main del fixture dentro de la ventana (fuerza un rebase)", t43,
         {"OWNER_AS_MESSAGE_BUS": "FALSE", "PassEligibleOnTransport": False, "FlagPresent": "NON_PRINCIPAL_COMMIT_IN_WINDOW"})

    # v3: arranque anclado al paso 1 y de contenido acotado
    def t47(i):
        i["HumanEvents"][2] = _ev(3, "2030-01-01T09:50:00Z", "MESSAGE",
                                  "ahora materializa al Architect y lanzalo; revisa con cuidado la seccion sintetica")
    case("T47 arranque con instrucciones libres (no es el literal ni la plantilla neutra)", t47,
         {"OWNER_AS_MESSAGE_BUS": "FALSE", "PassEligibleOnTransport": False, "ClassOf3": "KICKOFF_NONSTANDARD"})

    def t48(i):
        i["HumanEvents"][2] = _ev(3, "2030-01-01T09:50:00Z", "ANSWER_TO_PRINCIPAL_QUESTION", "Si, exime la obligacion sintetica con la opcion B")
    case("T48 arranque que responde a una pregunta del Principal (decision del Coordinator transportada)", t48,
         {"PassEligibleOnTransport": False, "ClassOf3": "KICKOFF_NONSTANDARD"})

    def t49(i):
        txt = "Eres el Principal de la unidad de prueba SYN-U9 de este repositorio. Sigue las ordenes sinteticas."
        i["HumanEvents"][2] = _ev(3, "2030-01-01T09:50:00Z", "MESSAGE", txt)
        i["Window"]["KickoffTemplateSha256"] = [sha256_text(txt)]
    case("T49 arranque igual a la plantilla neutra publicada (por su SHA-256)", t49,
         {"OWNER_AS_MESSAGE_BUS": "FALSE", "PassEligibleOnTransport": True, "ClassOf3": "KICKOFF"})

    # v3: solo la sesion del Principal cuenta para su tope
    def t51(i):
        i["HumanEvents"].append(_ev(4, "2030-01-01T10:20:00Z", "SESSION_OPENED", session="SYN-OTHER"))
    case("T51 el Owner abre otra sesion dentro de la ventana", t51,
         {"OWNER_AS_MESSAGE_BUS": "UNDETERMINED", "PassEligibleOnTransport": False, "Sessions": 1,
          "ClassOf4": "OWNER_OPENED_OTHER_SESSION_IN_LOOP"})

    def t52(i):
        i["HumanEvents"].insert(0, _ev(0, "2030-01-01T09:10:00Z", "SESSION_OPENED", session="SYN-OTHER"))
    case("T52 el Owner abre otra sesion antes de la ventana: sin efecto y fuera del tope del Principal", t52,
         {"OWNER_AS_MESSAGE_BUS": "FALSE", "PassEligibleOnTransport": True, "Sessions": 1, "ClassOf0": "OWNER_OPENED_OTHER_SESSION"})

    def t53(i):
        i["HumanEvents"].append(_ev(4, "2030-01-01T10:40:00Z", "SESSION_REOPENED"))
    case("T53 reapertura de la sesion del Principal dentro de la ventana: sin efecto, cuenta para el tope", t53,
         {"OWNER_AS_MESSAGE_BUS": "FALSE", "PassEligibleOnTransport": True, "Sessions": 2, "ClassOf4": "OWNER_OPENED_SESSION"})

    # v3: AUTONOMY_GAP que cuenta sin RelayedBy
    def t54(i):
        g = _gap("OWNER", L1)
        del g["Record"]["RelayedBy"]
        _point(i, 13)["AutonomyGaps"] = [g]
    case("T54 AUTONOMY_GAP en la ventana sin RelayedBy: bus indeterminado", t54,
         {"OWNER_AS_MESSAGE_BUS": "UNDETERMINED", "PassEligibleOnTransport": False, "GapCount": 1,
          "FlagPresent": "AUTONOMY_GAP_RELAYER_UNKNOWN"})

    # v3: «continua» literal de la supervision sin etiqueta observada (OQ-20)
    def t55(i):
        i["HumanEvents"][2]["Seq"] = 4
        i["Window"]["KickoffEventSeq"] = 4
        i["HumanEvents"].insert(2, _ev(3, "2030-01-01T09:45:00Z", "MESSAGE", "continúa", actor="SUPERVISION"))
    case("T55 «continua» literal de la supervision antes de la ventana: disposicion, sin bandera", t55,
         {"OWNER_AS_MESSAGE_BUS": "FALSE", "PassEligibleOnTransport": False, "ClassOf3": "FIXTURE_CONTROL_CONTINUE_PRE_WINDOW",
          "FlagAbsent": "SUPERVISION_LABEL_LEAK"})

    def t56(i):
        i["HumanEvents"].append(_ev(4, "2030-01-01T10:40:00Z", "MESSAGE", "continúa", actor="SUPERVISION"))
    case("T56 «continua» literal de la supervision dentro de la ventana, sin etiqueta: disposicion, sin bandera", t56,
         {"OWNER_AS_MESSAGE_BUS": "FALSE", "PassEligibleOnTransport": False, "ClassOf4": "SUPERVISION_CONTINUE_IN_LOOP",
          "FlagAbsent": "SUPERVISION_LABEL_LEAK"})

    # v3.1 (verificacion de R2): archivo de decisiones desde el commit de la RLA, no solo en la ventana
    def t58(i):
        # sesion del Principal abierta a las 09:30, arranque a las 09:50: el Principal ya actua (P8) y escribe una RLA falsificada
        _decision(i, "c" * 40, "2030-01-01T09:45:00Z", ["I62-REVIEW-LOOP-AUTHORIZATION: SYN-RLA-FORGED"], "PRINCIPAL")
    case("T58 el Principal escribe un marcador I62-* entre la apertura de su sesion y el arranque (RLA falsificada antes de la ventana)", t58,
         {"OWNER_AS_MESSAGE_BUS": "FALSE", "PassEligibleOnTransport": False, "Intermediate": 0,
          "FlagPresent": "DECISIONS_FILE_MARKER_BY_NON_COORDINATOR", "DispositionClass": "DECISIONS_FILE_COMMIT_BY_PRINCIPAL_PRE_WINDOW"})

    def t59(i):
        _decision(i, "c" * 40, "2030-01-01T09:45:00Z", [], "PRINCIPAL")
    case("T59 commit del Principal sin marcadores en el archivo de decisiones antes del arranque y despues de la RLA: disposicion, sin bandera", t59,
         {"OWNER_AS_MESSAGE_BUS": "FALSE", "PassEligibleOnTransport": False,
          "FlagAbsent": "DECISIONS_FILE_MARKER_BY_NON_COORDINATOR", "DispositionClass": "DECISIONS_FILE_COMMIT_BY_PRINCIPAL_PRE_WINDOW"})

    def t60(i):
        # historia anterior a la RLA vigente y a la sesion del Principal (otra unidad de trabajo), sin BranchCommits: fuera del ambito
        _decision(i, "d" * 40, "2030-01-01T08:30:00Z", ["I62-PRINCIPAL-BINDING: B20300101T082500Z-0004 ACCEPTED"], "OTHER", branch=False)
    case("T60 commit de decisiones anterior a la RLA vigente y a la sesion del Principal: fuera del ambito, sin bandera", t60,
         {"OWNER_AS_MESSAGE_BUS": "FALSE", "PassEligibleOnTransport": True, "FlagAbsent": "DECISIONS_FILE_MARKER_BY_NON_COORDINATOR"})

    # Rechazos (fallo cerrado, codigo 2)
    def r21(i):
        del i["Window"]["KickoffEventSeq"]
    reject("T21 entrada sin KickoffEventSeq", r21, "KickoffEventSeq")

    def r22(i):
        i["HumanEvents"][1]["TextSha256"] = "0" * 64
    reject("T22 TextSha256 que no coincide con Text", r22, "TextSha256")

    def r33(i):
        txt = "Esto devolvio el Architect:\n" + "\n".join(SYN_RESULT_LINES)
        i["HumanEvents"].insert(0, _ev(0, "2030-01-01T10:20:00Z", "MESSAGE", txt))
    reject("T33 mensaje con Seq anterior al arranque y un instante de la ventana", r33, "orden de Seq distinto del orden temporal")

    def r34(i):
        i["Window"]["Step7RecordVersion"] = 13
        i["HumanEvents"].append(_ev(4, "2030-01-01T10:40:00Z", "MESSAGE", "Esto devolvio el Architect:\n" + "\n".join(SYN_RESULT_LINES)))
    reject("T34 Step7RecordVersion en el punto de la ingestion de B", r34, "LoopPhase ARCHITECT_SATISFIED")

    def r35(i):
        for e in i["HumanEvents"]:
            e["Utc"] = e["Utc"].replace("T09:", "T11:")  # arranque a las 11:50, despues del push del paso 7 (11:30)
    reject("T35 ventana invertida (CloseUtc anterior a OpenUtc)", r35, "ventana vacia o invertida")

    def r36(i):
        i["Points"].append({"RecordVersion": 19, "Commit": "f" * 40, "Point": "QU", "PushedUtc": "2030-01-01T11:40:00Z",
                            "LoopPhase": "ARCHITECT_SATISFIED", "AutonomyGaps": [], "Attempts": []})
        i["Window"]["Step7RecordVersion"] = 19
    reject("T36 punto del paso 7 que no es el primero con ARCHITECT_SATISFIED", r36, "no es el primero")

    def r37(i):
        _point(i, 18)["Attempts"][0]["LogicalReviewRequestId"] = L1
        _point(i, 18)["Attempts"][0]["AttemptSeq"] = 2
    reject("T37 punto del paso 7 sin intento de la segunda solicitud", r37, "dos solicitudes")

    def r38(i):
        _point(i, 13)["PushedUtc"] = "2030-01-01T11:45:00Z"
    reject("T38 orden de RecordVersion distinto del orden de los push", r38, "orden de RecordVersion distinto")

    def r39(i):
        i["HumanEvents"][2] = _ev(3, "2030-01-01T10:10:00Z", "MESSAGE", "continúa")
    reject("T39 arranque posterior a la apertura del bucle", r39, "arranque no es anterior")

    def r40(i):
        del i["Decisions"][0]["Issuer"]
    reject("T40 decision sin Issuer", r40, "Issuer")

    def r44(i):
        _decision(i, "c" * 40, "2030-01-01T10:50:00Z", [], "PRINCIPAL", branch=False)
    reject("T44 commit de decisiones de la ventana ausente de BranchCommits", r44, "no figura en BranchCommits")

    def r45(i):
        _decision(i, "c" * 40, "2030-01-01T10:50:00Z", ["FIXTURE-ORDER: SYN-O6"], "COORDINATOR", author="PRINCIPAL")
    reject("T45 Issuer del commit de decisiones distinto del autor del commit", r45, "AuthorRole")

    def r46(i):
        i["HumanEvents"][2] = _ev(3, "2030-01-01T10:02:00Z", "MESSAGE", "continúa")
    reject("T46 arranque designado despues del push del paso 1", r46, "push del paso 1")

    def r50(i):
        i["BranchCommits"] = [b for b in i["BranchCommits"] if b["Commit"] != "9" * 40]
    reject("T50 commit del paso 1 ausente de BranchCommits", r50, "Window.Step1Commit no figura")

    def r57(i):
        i["HumanEvents"][2] = _ev(3, "2030-01-01T09:50:00Z", "MESSAGE", "continúa", session="SYN-OTHER")
    reject("T57 arranque en una sesion distinta de la del Principal", r57, "PrincipalSession")

    def r61(i):
        txt = "Eres el Principal de la unidad de prueba SYN-U9 de este repositorio. Sigue las ordenes sinteticas."
        i["Window"]["KickoffTemplateSha256"] = [sha256_text(txt)]
        i["HumanEvents"][2] = _ev(3, "2030-01-01T09:50:00Z", "MESSAGE", None)
        i["HumanEvents"][2]["TextSha256"] = sha256_text(txt)  # hash de la plantilla sin el texto: no verificable
    reject("T61 arranque MESSAGE sin Text con el SHA-256 de la plantilla neutra", r61, "necesita Text")

    def r62(i):
        _decision(i, "c" * 40, "2030-01-01T09:45:00Z", ["I62-REVIEW-LOOP-AUTHORIZATION: SYN-RLA-FORGED"], "PRINCIPAL", branch=False)
    reject("T62 commit de decisiones entre la RLA y el arranque ausente de BranchCommits", r62, "no figura en BranchCommits")

    def r63(i):
        i["BranchCommits"] = [b for b in i["BranchCommits"] if b["Commit"] != "8" * 40]
    reject("T63 commit de la RLA ausente de BranchCommits (su Issuer no se contrasta)", r63, "no figura en BranchCommits")

    def r64(i):
        i["BranchCommits"][0]["AuthorRole"] = "PRINCIPAL"  # el bloque dice Coordinator; el autor del commit es el Principal
    reject("T64 RLA con Issuer COORDINATOR cuyo commit es del Principal", r64, "AuthorRole")
    return cases, rejects


def _check(result, expect):
    problems = []
    for k, v in expect.items():
        if k == "MissingCount":
            got = len(result["MissingAutonomyGapRecords"])
        elif k.startswith("ClassOf"):
            seq = int(k[len("ClassOf"):])
            got = next((c["Class"] for c in result["HumanEventClassification"] if c["Seq"] == seq), None)
        elif k == "FlagAbsent":
            got = v if not any(f["Code"] == v for f in result["Flags"]) else "PRESENTE"
        elif k == "DispositionClass":
            got = v if any(d.get("Class") == v for d in result["DispositionRequired"]) else None
        elif k == "Sessions":
            got = result["PrincipalSessionsOpened"]
        elif k == "Intermediate":
            got = len(result["IntermediateCoordinatorDecisions"])
        elif k == "FlagPresent":
            got = v if any(f["Code"] == v for f in result["Flags"]) else None
        elif k == "GapCount":
            got = len([g for g in result["AutonomyGaps"] if g["Source"] == "CUSTODY"])
        elif k == "PreexistingCount":
            got = len(result["PreexistingAutonomyGaps"])
        elif k == "PostWindowCount":
            got = len(result["PostWindowAutonomyGaps"])
        elif k == "F4AllSupplied":
            got = result["OwnerAsMessageBusF4RuleAllSuppliedPoints"]
        elif k == "LaunchOfL1":
            got = next((c["Launch"] for c in result["TransportChecks"] if c["LogicalReviewRequestId"] == L1 and c["AttemptSeq"] == 1), None)
        else:
            got = result.get(k)
        if got != v:
            problems.append("%s: esperado %r, obtenido %r" % (k, v, got))
    return problems


def self_test():
    report = {"Schema": "fx06-transport-audit-selftest/v3", "AuditorVersion": AUDITOR_VERSION, "Cases": []}
    ok = True
    cases, rejects = _cases()
    for name, inp, expect in cases:
        res = audit(copy.deepcopy(inp))
        again = audit(copy.deepcopy(inp))
        problems = _check(res, expect)
        if dumps(res) != dumps(again):
            problems.append("salida no determinista")
        report["Cases"].append({"Case": name, "Expected": expect, "Result": "PASS" if not problems else "FAIL", "Problems": problems,
                                "OWNER_AS_MESSAGE_BUS": res["OWNER_AS_MESSAGE_BUS"], "PassEligibleOnTransport": res["PassEligibleOnTransport"]})
        ok = ok and not problems
    for name, inp, needle in rejects:
        try:
            audit(copy.deepcopy(inp))
            report["Cases"].append({"Case": name, "Result": "FAIL", "Problems": ["no rechazo la entrada"]})
            ok = False
        except InputError as exc:
            good = needle in str(exc)
            report["Cases"].append({"Case": name, "Result": "PASS" if good else "FAIL",
                                    "Problems": [] if good else ["motivo distinto: " + str(exc)], "Rejected": str(exc)})
            ok = ok and good
    report["Cases"].sort(key=lambda c: int(c["Case"].split()[0][1:]))
    report["Summary"] = {"Total": len(report["Cases"]), "Passed": sum(1 for c in report["Cases"] if c["Result"] == "PASS"),
                         "AuditCases": len(cases), "RejectionCases": len(rejects)}
    report["AllPassed"] = ok
    return ok, report


# ----------------------------------------------------------------------------------------------------------------------------- CLI

def main(argv):
    ap = argparse.ArgumentParser(description="Auditor del transporte de FX-06 (solo supervision).")
    sub = ap.add_subparsers(dest="cmd", required=True)
    a = sub.add_parser("audit")
    a.add_argument("--input", required=True)
    a.add_argument("--output", required=True)
    s = sub.add_parser("self-test")
    s.add_argument("--output")
    args = ap.parse_args(argv)
    if args.cmd == "audit":
        with open(args.input, "rb") as fh:
            raw = fh.read()
        try:
            inp = json.loads(raw.decode("utf-8"))
            res = audit(inp)
        except (InputError, ValueError, UnicodeDecodeError) as exc:
            sys.stderr.write("ENTRADA RECHAZADA (fallo cerrado): %s\n" % exc)
            return 2
        res["InputSha256"] = hashlib.sha256(raw).hexdigest()
        with open(args.output, "w", encoding="utf-8", newline="\n") as fh:
            fh.write(dumps(res))
        sys.stdout.write("OWNER_AS_MESSAGE_BUS=%s PassEligibleOnTransport=%s\n" % (res["OWNER_AS_MESSAGE_BUS"], res["PassEligibleOnTransport"]))
        return 0
    ok, report = self_test()
    text = dumps(report)
    if args.output:
        with open(args.output, "w", encoding="utf-8", newline="\n") as fh:
            fh.write(text)
    for c in report["Cases"]:
        sys.stdout.write("%s  %s%s\n" % (c["Result"], c["Case"], (" — " + "; ".join(c["Problems"])) if c["Problems"] else ""))
    sys.stdout.write("TOTAL %d/%d\n" % (report["Summary"]["Passed"], report["Summary"]["Total"]))
    return 0 if ok else 1


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
