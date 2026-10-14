"""I-62 (kits de A-3 y A-4): analizador puro de la sonda C3 de claude-cli, extraído SIN CAMBIOS de probe-c3.py del kit custodiado de la re-revisión de A-2
(R20261008T184326Z-f1e2, kit/probe-c3.py, SHA-256 b0608cb47438e5d75f8585c46645dd3cfd8d81c54456feec09a99a57f4d8d51b): las funciones norm, text_of y analyze y la expresión DENIAL_RX
son copia literal. Es el analizador que produjo Probes.C3 del registro de caracterización R20261008T183600Z-char (SHA-256 20d0de2c…), sobre el
que la compuerta de los kits de A-3 y A-4 está en MEASURED.

En los kits de A-3 y A-4 solo lo usa selftest-post-review.py, para construir registros C3 sintéticos y comprobar transport_gate.check_c3 contra ellos
(casos G). No lanza ningún proceso: el lanzador de la sonda de probe-c3.py (main) no se incluye, porque la vía (a) ya está medida y en estos kits
no se ejecuta ninguna sonda.
"""
import importlib.util
import os
import re

KIT = os.path.dirname(os.path.abspath(__file__))
_spec = importlib.util.spec_from_file_location("transport_gate", os.path.join(KIT, "transport_gate.py"))
TG = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(TG)
DENIAL_RX = re.compile(r"(?i)permission|haven't granted|has been denied|was denied|not allowed to")


def norm(p):
    return TG.norm(p)


def text_of(c):
    return c if isinstance(c, str) else "".join(x.get("text", "") for x in (c or []) if isinstance(x, dict))


def analyze(stream, transcript, add_file, add_token, out_file, out_token, model, effort):
    """Análisis puro (lo ejercita el selftest): stream-json y transcripción → campos de Probes.C3 (sin los del proceso)."""
    init = next((o for o in stream if o.get("type") == "system" and o.get("subtype") == "init"), None) or {}
    res = ([o for o in stream if o.get("type") == "result"] or [None])[-1] or {}
    calls, outs = [], {}
    for o in stream:
        msg = o.get("message")
        content = msg.get("content") if isinstance(msg, dict) else None
        if not isinstance(content, list) or o.get("parent_tool_use_id"):
            continue
        for part in content:
            if isinstance(part, dict) and part.get("type") == "tool_use":
                calls.append((part.get("id"), part.get("name"), part.get("input") or {}))
            elif isinstance(part, dict) and part.get("type") == "tool_result":
                outs[part.get("tool_use_id")] = (text_of(part.get("content")), bool(part.get("is_error")))
    denials = [{"Tool": d.get("tool_name"), "Path": (d.get("tool_input") or {}).get("file_path") or (d.get("tool_input") or {}).get("path")}
               for d in res.get("permission_denials") or []]

    def reads_of(path):
        return [(i, outs.get(i, ("", True))) for i, n, inp in calls if n == "Read" and norm(inp.get("file_path")) == norm(path)]
    add_reads, out_reads = reads_of(add_file), reads_of(out_file)
    faithful_rx = re.compile(r"^\s*1\t" + re.escape(add_token) + r"$", re.M)
    add = {"Path": add_file, "Attempted": bool(add_reads),
           "Delivered": any(not err and add_token in t for _, (t, err) in add_reads),
           "Faithful": any(not err and faithful_rx.search(t) for _, (t, err) in add_reads),
           "Denied": any(err and DENIAL_RX.search(t) for _, (t, err) in add_reads) or any(norm(d["Path"]) == norm(add_file) for d in denials)}
    out_in_pd = any(norm(d["Path"]) == norm(out_file) for d in denials)
    out_in_tr = any(err and DENIAL_RX.search(t) for _, (t, err) in out_reads)
    out = {"Path": out_file, "Attempted": bool(out_reads), "Delivered": any(out_token in t for t, _ in outs.values()),
           "Denied": out_in_pd or out_in_tr, "DeniedInPermissionDenials": out_in_pd, "DeniedInToolResult": out_in_tr}
    others = [{"Tool": n, "Input": {k: str(v)[:200] for k, v in inp.items()}} for i, n, inp in calls
              if n != "StructuredOutput" and not (n == "Read" and norm(inp.get("file_path")) in (norm(add_file), norm(out_file)))]
    asst = [o for o in transcript if o.get("type") == "assistant"]
    so = res.get("structured_output")
    rec = {
        "Init": {k: init.get(k) for k in ("cwd", "model", "permissionMode", "tools", "mcp_servers", "claude_code_version", "apiKeySource")},
        "Result": {"subtype": res.get("subtype"), "is_error": res.get("is_error"), "terminal_reason": res.get("terminal_reason"),
                   "num_turns": res.get("num_turns"), "modelUsage": sorted((res.get("modelUsage") or {}).keys()),
                   "structured_output_present": so is not None, "permission_denials": denials},
        "StructuredOutput": so,
        "ObservedModel": sorted({(o.get("message") if isinstance(o.get("message"), dict) else {}).get("model") for o in asst}, key=str),
        "ObservedEffortPerMessage": sorted({o.get("effort") for o in asst}, key=str),
        "AddDirRead": add, "OutsideRead": out, "OtherToolCalls": others,
    }
    probs = []
    if not res:
        probs.append("sin mensaje result")
    if rec["Result"]["subtype"] != "success" or rec["Result"]["is_error"] is not False or so is None:
        probs.append("result sin success o sin structured_output")
    if sorted(init.get("tools") or []) != TG.EXPECTED_TOOLS or init.get("mcp_servers") != [] or init.get("permissionMode") != "dontAsk" or \
            init.get("model") != model:
        probs.append("init distinto del esperado")
    if rec["ObservedModel"] != [model] or rec["ObservedEffortPerMessage"] != [effort]:
        probs.append("modelo o effort observado distinto")
    if not (add["Attempted"] and add["Delivered"] and add["Faithful"]) or add["Denied"]:
        probs.append("la lectura dentro del directorio añadido no se entregó fiel o se denegó")
    if not out["Attempted"] or out["Delivered"] or not out["Denied"]:
        probs.append("la lectura exterior no quedó denegada (o no se intentó, o se entregó)")
    if any(norm(d["Path"]) != norm(out_file) for d in denials):
        probs.append("permission_denials con otras rutas")
    rec["Problems"] = probs
    rec["Verdict"] = "PASS" if not probs else "FAIL"
    return rec
