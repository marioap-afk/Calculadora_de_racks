# Builds claude-cli-characterization.json (claude-cli 2.1.299; decisiones §87) from identity.json and the C1/C2 analyses, in the schema of the
# R20261008T183600Z-char record. `pre` writes it without C3 (char2_c3.py adds Probes.C3); `final` adds the eligibility verdict after C3.
import json, os, sys
D = os.path.dirname(os.path.abspath(__file__))
J = lambda n: json.load(open(os.path.join(D, n), encoding='utf-8'))
REC = os.path.join(D, 'claude-cli-characterization.json')
TEMPLATE_C1 = ["-p", "--model", "claude-opus-5-5", "--effort", "xhigh", "--output-format", "stream-json", "--verbose", "--safe-mode",
               "--strict-mcp-config", "--no-chrome", "--tools", "Read,Grep,Glob", "--permission-mode", "dontAsk", "--permission-prompts", "none"]


def probe(name, a, r):
    p = {"SessionId": r["SessionId"], "Pid": r["Pid"], "Start": r["Start"], "End": r["End"], "ExitCode": r["ExitCode"], "Killed": r["Killed"],
         "ProcessGoneAfter": r["ProcessGoneAfter"], "Args": r["Args"], "PromptSha256": r["PromptSha256"],
         "Init": a["Init"], "InitSessionIdMatches": a["InitSessionIdMatches"], "Result": a["Result"], "ResultMessage": a["ResultMessage"],
         "ObservedModel": a["ObservedModel"], "ObservedEffortPerMessage": a["ObservedEffortPerMessage"],
         "PromptFidelity": "el primer mensaje de usuario de la transcripción es idéntico al prompt" if a["PromptFidelity"] else "DISTINTO",
         "ToolCalls": a["ToolCalls"], "Transcript": a["Transcript"], "AttachmentTypes": a["AttachmentTypes"], "CloneUnchanged": a["CloneUnchanged"]}
    if name == "C1":
        rf = a["ReadFidelity"]
        p["ReadFidelity"] = "%d/%d líneas de docs/initiatives/I-62-A-1.md entregadas por Read idénticas al archivo del clon; solo ese archivo leído: %s" % (
            rf["LinesFaithful"], rf["Of"], rf["OnlyTheRequestedFile"])
        p["StructuredOutputCheck"] = a["StructuredOutputCheck"]
        p["AutomaticContext"] = a["AutomaticContext"]
        if "TerminateAt" in r:
            p["TerminateAt"] = r["TerminateAt"]
    else:
        p["TerminateAt"] = r.get("TerminateAt")
        p["ToolCallsBeforeTermination"] = a["ToolCallsBeforeTermination"]
    return p


def pre():
    ident = J('identity.json')
    a1, a2 = J('analysis-C1.json'), J('analysis-C2.json')
    r1, r2 = J('C1/run.json'), J('C2/run.json')
    f1, f2 = J('C1.run1-oauth/run.json'), J('C2.run1/run.json')
    s1 = J('C1.run1-oauth/analysis.json')
    c1_stream = [json.loads(l) for l in open(os.path.join(D, 'C1', 'stdout.jsonl'), encoding='utf-8', errors='replace') if l.strip().startswith('{')]
    res = ([o for o in c1_stream if o.get('type') == 'result'] or [{}])[-1]
    rec = {
        "Schema": "rackcad-claude-cli-characterization (registro de evidencia; decisiones §56, punto 7; §87, punto 2)",
        "Authority": "decisiones §87, puntos 2-3 (caracterización acotada de solo lectura del binario nuevo); procedimiento de decisiones §56, punto 7; "
                     "Owner CLAUDE-CLI-I62 = A sin cambio ni ampliación (medición y uso read-only para ARCHITECT y REVIEWER de I-62)",
        "Bounds": "procesos claude-cli en un clon limpio de solo lectura (D:/r62-cli-char2 en e8a3de05, sin remoto) y un directorio propio de la sonda C3 "
                  "(D:/r62-c3-probe2); sin escrituras de producto, sin tocar el fixture, sin escrituras Git; sin cambios de configuración, permisos, sandbox "
                  "ni credenciales; credenciales nunca leídas; ninguna revisión formal",
        "Executables": [
            {"Path": ident["Path"].replace("\\", "/"), "Version": ident["Version"]["Text"], "Sha256": ident["Sha256"], "Size": ident["Size"],
             "Modified": ident["Modified"], "Authenticode": "%s; %s" % (ident["Authenticode"]["Status"], 'CN="Anthropic, PBC"'),
             "AuthenticodeThumbprint": ident["Authenticode"]["Thumbprint"], "Usable": True,
             "Note": "binario de Claude Code que gestiona la app de escritorio, instalado por su actualización del 2026-10-16 (sin descarga de la "
                     "sesión); la app borró 2.1.293 (8693c4a0…), el binario de R20261008T183600Z-char. Cada cambio de binario o versión deja STALE esta "
                     "observación"},
            {"Path": "%APPDATA%/Claude/claude-code/2.1.295/d2e29494ec83/claude.exe", "Version": "2.1.295 (Claude Code)",
             "Sha256": "a30012741c3d0b55e2a6c252dab3781134b75be95896ae89d40a93e10f83bcd6", "Usable": False,
             "Reason": "presente pero no caracterizado aquí (no es el binario que la app usa ahora)"},
            {"Path": "%APPDATA%/Claude/claude-code/2.1.293/83cb0bd7fed4/claude.exe", "Version": "2.1.293 (Claude Code)",
             "Sha256": "8693c4a02dde7441d0066ede68af8ddfc408bb982d77e12b506286268224e6fa", "Usable": False,
             "Reason": "AUSENTE desde el 2026-10-16 (borrado por la actualización de la app); no se restaura ni se descarga (decisiones §87, punto 1)"}],
        "Authentication": {"Command": "claude auth status --json (no lee credenciales)", "loggedIn": ident["AuthStatus"]["loggedIn"],
                           "authMethod": ident["AuthStatus"]["authMethod"], "apiProvider": ident["AuthStatus"]["apiProvider"],
                           "subscriptionType": ident["AuthStatus"]["subscriptionType"], "Fields omitted": "correo, organización y sus identificadores"},
        "ConsumptionMode": {"Subscription": "%s (claude.ai)" % ident["AuthStatus"]["subscriptionType"], "apiKeySource": a1["Init"].get("apiKeySource"),
                            "CostReported": res.get("total_cost_usd"), "CostBasis": "list (nocional con suscripción)",
                            "Coverage": "cubierto por CLAUDE-CLI-I62 = A (medición y uso read-only); cada invocación con su autoridad"},
        "Attempts": {
            "C1.run1": {"SessionId": f1["SessionId"], "Start": f1["Start"], "ExitCode": f1["ExitCode"], "ObservedModel": s1["ObservedModel"],
                        "Result": {"is_error": (s1["Result"] or {}).get("is_error"), "terminal_reason": (s1["Result"] or {}).get("terminal_reason")},
                        "Cause": "«Failed to refresh OAuth token: another Claude Code process is refreshing it or exited mid-refresh. This is usually "
                                 "transient; retry in a minute»: la sonda se lanzó segundos después de `auth status` (identidad), sin la espera de 120 s "
                                 "que aplica el lanzador del kit; ninguna llamada al modelo (modelo <synthetic>), ninguna lectura; se conserva y C1 se "
                                 "repitió una vez pasados más de 3 minutos"},
            "C2.run1": {"SessionId": f2["SessionId"], "Start": f2["Start"], "ExitCode": f2["ExitCode"], "Killed": f2["Killed"],
                        "Cause": "terminada a los 8 s sin ninguna orden de herramienta (lanzada justo tras C1.run1, en la misma ventana de renovación del "
                                 "token); se conserva y C2 se repitió una vez con C1"}},
        "Probes": {"C1": probe("C1", a1, r1), "C2": probe("C2", a2, r2)},
        "Capabilities": {
            "ExecutablePath": "medido", "CliVersion": ident["Version"]["Text"].split()[0], "RuntimeBinaryIdentity": "SHA-256 + Authenticode",
            "Authentication": "loggedIn (auth status)", "RequestedModel": "claude-opus-5-5",
            "ObservedModel": "%s (modelUsage y mensajes; RUNTIME_OBSERVED)" % ",".join(a1["ObservedModel"]),
            "RequestedEffort": "xhigh", "ObservedEffort": "%s por mensaje (campo effort de la transcripción; RUNTIME_OBSERVED)" % ",".join(a1["ObservedEffortPerMessage"]),
            "ProcessIdentity": "PID de Windows del proceso lanzado",
            "SessionIdentity": "--session-id fijado por el invocador, igual en init, result y transcripción",
            "WorkingDirectory": "cwd del init = el clon",
            "PermissionSandbox": "--permission-mode dontAsk, --permission-prompts none, --tools Read,Grep,Glob: solo lectura; lo demás se deniega sin "
                                 "preguntar (sin sandbox del SO)",
            "StructuredOutput": "--json-schema → structured_output en result y adjunto en la transcripción",
            "ExitStatus": "0 completada; 1 error de API o terminación",
            "Cancellation": "terminación del proceso por el invocador (C2): proceso terminado, sin mensaje result, transcripción conservada",
            "TerminationObservability": "fin del proceso + presencia o ausencia del mensaje result (terminal_reason) en stream-json",
            "Timeout": "sin tope propio en la CLI: lo aplica el invocador con el mismo camino que la cancelación",
            "AuthSettle": "auth status puede renovar el token OAuth; un proceso lanzado en esa ventana falla con error de API transitorio (C1.run1): "
                          "esperar 120 s, como el lanzador del kit"},
        "Eligibility": {"Requirement": "ARCHITECTURE_REVIEW (routing): nivel Frontera, effort Deep (high), capacidad read; introspección RUNTIME_OBSERVED",
                        "STALE": None, "ARCHITECT": "PENDIENTE de C3", "REVIEWER": "PENDIENTE de C3"},
    }
    assert rec["Probes"]["C1"]["Args"][:len(TEMPLATE_C1)] == TEMPLATE_C1
    open(REC, 'w', encoding='utf-8', newline='\n').write(json.dumps(rec, ensure_ascii=False, indent=1) + '\n')
    print('pre ok')


def final():
    rec = J('claude-cli-characterization.json')
    c1, c2, c3 = rec["Probes"]["C1"], rec["Probes"]["C2"], rec["Probes"].get("C3") or {}
    ex = rec["Executables"][0]
    checks = {
        "installed": ex["Usable"] is True,
        "authenticated": rec["Authentication"]["loggedIn"] is True,
        "binary_identity": ex["Authenticode"].startswith("Valid") and len(ex["Sha256"]) == 64,
        "c1_exit0": c1["ExitCode"] == 0 and not c1["Killed"],
        "model": c1["ObservedModel"] == ["claude-opus-5-5"] and c1["Init"]["model"] == "claude-opus-5-5",
        "effort": c1["ObservedEffortPerMessage"] == ["xhigh"],
        "prompt_fidelity": c1["PromptFidelity"].startswith("el primer"),
        "read_fidelity": c1["ReadFidelity"].startswith("12/12") and c1["ReadFidelity"].endswith("True"),
        "structured_output": (c1["Result"] or {}).get("structured_output_present") is True and c1["StructuredOutputCheck"]["first_line_matches"],
        "tools_permissions": c1["Init"]["tools"] == ["Glob", "Grep", "Read", "StructuredOutput"] and c1["Init"]["mcp_servers"] == []
                             and c1["Init"]["permissionMode"] == "dontAsk",
        "context_isolation": c1["AutomaticContext"]["TranscriptMentions"] == {"CLAUDE.md": 0, "MEMORY.md": 0, "AGENTS.md": 0}
                             and c1["AutomaticContext"]["ProjectMemoryDir"] is False,
        "clone_integrity": c1["CloneUnchanged"] and c2["CloneUnchanged"],
        "cancellation": c2["Killed"] is True and c2["ExitCode"] != 0 and c2["ResultMessage"] is False and c2["ProcessGoneAfter"] is True,
        "c3_add_dir": c3.get("Verdict") == "PASS",
    }
    ok = all(checks.values())
    el = rec["Eligibility"]
    el.update({"Checks": checks, "installed": checks["installed"], "authenticated": checks["authenticated"], "measured_invocation": checks["c1_exit0"],
               "level": "Frontera (claude-opus-5-5, model-catalog) = MATCH" if checks["model"] else "NOT_DEMONSTRATED",
               "effort": "xhigh observado ≥ Deep (high) = ABOVE_REQUIRED" if checks["effort"] else "NOT_DEMONSTRATED",
               "read": "MATCH" if checks["read_fidelity"] else "NOT_DEMONSTRATED",
               "introspection": "RUNTIME_OBSERVED = MATCH" if checks["model"] and checks["effort"] else "NOT_DEMONSTRATED",
               "consumption_covered": "cubierto (CLAUDE-CLI-I62 = A; suscripción max)",
               "STALE": False if ok else True,
               "Actor": "satisfecho: proceso nuevo, distinto de la sesión autora (mismo modelo y proveedor; Provider PREFERRED no se cumple, se declara)",
               "Session": "satisfecho: sesión nueva con id fijado, sin reanudación",
               "Context": "satisfecho: modo seguro, sin CLAUDE.md, memoria ni transcripciones; cierre de insumos del kit",
               "ARCHITECT": ("ELIGIBLE (binario 2.1.299 de %APPDATA%, sha 295b49ec…, en esta observación; decisiones §87, punto 3)" if ok else "NOT_DEMONSTRATED"),
               "REVIEWER": ("ELIGIBLE (mismas condiciones; read-only)" if ok else "NOT_DEMONSTRATED"),
               "Equivalence": "mismas sondas, flags, prompts y comprobaciones que R20261008T183600Z-char; resultados equivalentes en todas" if ok else
                              "no equivalente: ver Checks",
               "Invalidators": "BinaryHash, versión, autenticación, modelo, effort, blobs de catálogo y routing, instancia del host, flags del adapter; "
                               "cualquier cambio → STALE"})
    open(REC, 'w', encoding='utf-8', newline='\n').write(json.dumps(rec, ensure_ascii=False, indent=1) + '\n')
    print('final', 'ELIGIBLE' if ok else 'NOT_DEMONSTRATED', json.dumps(checks))


if __name__ == '__main__':
    {'pre': pre, 'final': final}[sys.argv[1]]()
