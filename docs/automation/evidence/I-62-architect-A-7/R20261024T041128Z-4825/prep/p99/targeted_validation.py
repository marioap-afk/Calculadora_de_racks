"""Decisiones §99, punto 3 (validación incremental): pruebas afectadas por la fijación de §99 en kit/owner_gate.py (9d4a8f96 → solo las tres
constantes AUTH_*), sin repetir automáticamente las 321 pruebas ni los 209 mutantes. Derivado del de §97 (ronda 3). Justificación: la fijación cambia tres constantes de un solo
módulo; ninguna regla, ningún otro componente, el cierre ni el prompt. Lo que lee esas constantes:
  * owner_gate.pinned_authorization / authorization_pending (O00c: estructura del módulo del kit, pendiente o fijado entero) y owner_gate.check sobre
    el worktree real (L00, que ahora debe pasar en estado FIJADO con exactamente §99 y ninguna sección posterior);
  * los casos de auditoría que construyen o comparan el registro de la compuerta 11 con las constantes del módulo (00 base acreditada con la
    autorización del KIT, 150-151p y las categorías 152-153);
  * los 19 sitios de regla de owner_gate.py: en la batería completa de la ronda 3 cada uno lo mataron SOLO casos O (O00-O29), que
    llaman a owner_gate.evaluate con argumentos explícitos; por eso la mutación dirigida se juzga sobre owner_cases() (los mismos sitios y el mismo
    mutante del arnés congelado: mutation-post-review.sites y mutant_source), con la línea base fijada en PASS.
Usa los módulos congelados del kit sin modificarlos (selftest-post-review.py vía el arnés de mutación; variant se envuelve en memoria para ejecutar
solo los casos de auditoría elegidos; los demás no se ejecutan ni se cuentan). Temporales bajo <kit>/../.tmp/st (los borra el arnés).
Uso: python -X utf8 -B targeted_validation.py <kit> <salida.json>
"""
import hashlib
import importlib.util
import json
import os
import sys

KIT, OUT = sys.argv[1], os.path.abspath(sys.argv[2])
sys.path.insert(0, KIT)
spec = importlib.util.spec_from_file_location("mutation_post_review", os.path.join(KIT, "mutation-post-review.py"))
MP = importlib.util.module_from_spec(spec)
spec.loader.exec_module(MP)
ST = MP.ST
sha = lambda n: hashlib.sha256(open(os.path.join(KIT, n), "rb").read()).hexdigest()
SELECTED = ("00-", "150", "151", "152", "153")
DUMMY_OUT = {"Fidelity": {"FailedReadsNeverCredited": []}, "Categories": {"Execution": None}, "Accreditation": {"Reasons": []},
             "Result": {"Check": {"SchemaValid": True, "Coherence": []}}}

# ---- A. targeted self-test
pr = ST.load_auditor()
ST.init_globals(pr)
pins = dict(ST.PINS)
o_rows = ST.owner_cases()
l_row = ST.live_owner_gate_case()
orig_variant, orig_fid = ST.variant, ST.fidelity_cases
ran = []


def wrapped(name, *a, **kw):
    if name.startswith(SELECTED):
        rv = orig_variant(name, *a, **kw)
        ran.append(rv[0])
        return rv
    return {"Case": name, "Result": "SKIPPED"}, DUMMY_OUT


ST.variant, ST.fidelity_cases = wrapped, (lambda: [])
try:
    ST.audit_cases()
finally:
    ST.variant, ST.fidelity_cases = orig_variant, orig_fid
selftest_rows = o_rows + [l_row] + ran
part_a = {"OwnerCases": len(o_rows), "OwnerPass": sum(1 for c in o_rows if c["Result"] == "PASS"),
          "L00": {k: l_row.get(k) for k in ("ExpectedState", "Ok", "Problems", "AuthorizationSections", "LaterSections", "ObjectBlobAtHead",
                                            "A6BlobAtHead", "WorktreeHead", "Result")},
          "AuditCases": [{"Case": c["Case"], "Result": c["Result"], "Accreditation": c.get("Accreditation"), "Missing": c.get("Missing"),
                          "Unexpected": c.get("Unexpected")} for c in ran],
          "AuditPass": sum(1 for c in ran if c["Result"] == "PASS"), "PinsForAuditCases": pins,
          "Failing": [c["Case"] for c in selftest_rows if c["Result"] != "PASS"]}
part_a["AllPass"] = not part_a["Failing"] and l_row.get("ExpectedState") == "PINNED" and len(ran) >= 20 and pins.get("Source", "").startswith("KIT")

# ---- B. targeted mutation of owner_gate.py (frozen sites and mutant; judged on owner_cases)
mut_dir = os.path.join(ST.TMPROOT, "mut99")
os.makedirs(mut_dir, exist_ok=True)


def judge_owner(og_path=None):
    p = ST.load_auditor()
    if og_path:
        p.OG = ST.load("owner_gate_mutant", og_path)
    ST.init_globals(p)
    return [c["Case"] for c in ST.owner_cases() if c["Result"] != "PASS"]


base_fail = judge_owner()
src, found = MP.sites("owner_gate.py")
mutants = []
for k, (i, a, b) in enumerate(found):
    path = os.path.join(mut_dir, "owner_gate.m%03d.py" % k)
    open(path, "w", encoding="utf-8", newline="\n").write(MP.mutant_source(src, i, a, b))
    try:
        fails = judge_owner(path)
    except Exception as e:
        fails = ["EXCEPTION %s: %s" % (type(e).__name__, str(e)[:200])]
    mutants.append({"Line": i + 1, "Code": src[i].strip()[:150], "Killed": bool(fails), "KilledBy": fails[:4], "KillingCases": len(fails)})
    os.remove(path)
part_b = {"Module": "owner_gate.py", "Sha256": sha("owner_gate.py"), "Sites": len(found), "BaselineFailing": base_fail,
          "Killed": sum(1 for m in mutants if m["Killed"]), "Mutants": mutants}
part_b["AllKilled"] = not base_fail and bool(found) and all(m["Killed"] for m in mutants)
doc = {"Schema": "a7r4-targeted-validation-s99/1",
       "Why": "decisiones §99, punto 3: la fijación solo cambia AUTH_SECTION, AUTH_HEADING y AUTH_TEXT_SHA de owner_gate.py (prep/p99/owner_gate.pin.diff); "
              "se repiten las pruebas que leen esas constantes y la mutación dirigida del módulo cambiado, juzgada sobre los casos O, los únicos que "
              "mataron sus 19 sitios en la batería completa; la autoprueba de 321 casos y la mutación dirigida del kit (targeted-mutation-result.json) "
              "siguen vigentes para los demás componentes (sin cambio)",
       "Modules": {n: sha(n) for n in ("owner_gate.py", "post-review.py", "selftest-post-review.py", "mutation-post-review.py", "transport_gate.py", "fidelity.py")},
       "SelftestTargeted": part_a, "MutationTargeted": part_b, "AllPass": part_a["AllPass"] and part_b["AllKilled"]}
text = ST.USER_RX.sub("%USERPROFILE%", json.dumps(doc, ensure_ascii=False, indent=1))
with open(OUT, "w", encoding="utf-8", newline="\n") as f:
    f.write(text + "\n")
print(json.dumps({"OwnerCases": "%d/%d" % (part_a["OwnerPass"], part_a["OwnerCases"]), "L00": part_a["L00"]["Result"], "L00State": part_a["L00"]["ExpectedState"],
                  "AuditCases": "%d/%d" % (part_a["AuditPass"], len(ran)), "Failing": part_a["Failing"],
                  "Mutation": "%d/%d" % (part_b["Killed"], part_b["Sites"]), "AllPass": doc["AllPass"]}, ensure_ascii=False))
ST.safe_remove_tmproot()
sys.exit(0 if doc["AllPass"] else 1)
