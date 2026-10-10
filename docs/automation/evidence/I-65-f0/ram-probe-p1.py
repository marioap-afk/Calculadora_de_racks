# I-65 F0 — Sonda P1 v3 del entorno instalado de RAM Elements (instrumento de medicion, no producto).
#
# Ejecutar SOLO con autorizacion del Owner (Q-O4), dentro de RAM Elements 2026, sobre un MODELO NUEVO Y VACIO:
#   File > New  ->  Python Console  ->  Run File...  (este archivo)
# Antes, el Coordinator copia este archivo a una carpeta NUEVA fuera del repositorio y reemplaza OUT_DIR por la ruta absoluta de esa
# carpeta (ya creada): la consola podria bloquear os.mkdir/os.makedirs, asi que la sonda no crea carpetas.
# PUERTA (A65-V1-02, A65-V2-01d): antes de la PRIMERA escritura la sonda aborta sin tocar nada si el modelo tiene nudos o miembros, si
# OUT_DIR no es absoluta, no existe o sigue siendo el marcador, o si OUT_DIR ya contiene artefactos de una corrida anterior. El numero de
# casos de carga de File > New se REGISTRA (es lo que esta sonda mide).
# Al terminar, el modelo se descarta sin guardar (salvo la copia desechable del paso S9, dentro de OUT_DIR).
# Orden (A65-V1-03a): los analisis N1/N2 corren primero, con apoyos completos y sin nudos coincidentes; cada paso usa su propia region.

import json
import sys

OUT_DIR = r"__OUT_DIR__"
REPORT = {"probe": "I-65-F0-P1-v3", "out_dir": OUT_DIR, "gate": None, "steps": {}}


STEPS = []


def step(name):
    # Registers the step; nothing runs until the gate below has passed.
    def wrap(fn):
        STEPS.append((name, fn))
        return fn
    return wrap


def run_steps():
    for name, fn in STEPS:
        try:
            REPORT["steps"][name] = {"ok": True, "observed": fn()}
        except Exception as exc:  # the probe records failures instead of stopping
            REPORT["steps"][name] = {"ok": False, "error": type(exc).__name__ + ": " + str(exc)}


def emit():
    text = json.dumps(REPORT, indent=2, default=repr)
    try:
        with open(OUT_DIR + "\\p1-report.json", "x", encoding="utf-8") as fh:
            fh.write(text)
        print("I-65 P1 report written to", OUT_DIR + "\\p1-report.json")
    except Exception as exc:
        print("I-65 P1: could not write the report (" + type(exc).__name__ + "); full report follows")
        print(text)
    for name, data in REPORT["steps"].items():
        print(name, "OK" if data["ok"] else "FAILED: " + data["error"])


import ramelements as re
from ramelements._p4d.engine import get_core
from ramelements.enums import LoadDirection

m = re.model()

# ---- gate: nothing is written before this passes -------------------------------------------------------------------------------
import os
_reasons = []
if OUT_DIR.startswith("__") or not os.path.isabs(OUT_DIR) or not os.path.isdir(OUT_DIR):
    _reasons.append("OUT_DIR is the placeholder, not absolute, or does not exist")
else:
    _leftovers = [f for f in ("write-test.txt", "p1-report.json", "probe-model.retx") if os.path.exists(OUT_DIR + "\\" + f)]
    if _leftovers:
        _reasons.append("OUT_DIR holds artifacts of an earlier run: " + ", ".join(_leftovers))
if len(m.nodes) != 0 or len(m.members) != 0:
    _reasons.append("model is not empty: nodes=%d members=%d" % (len(m.nodes), len(m.members)))
REPORT["gate"] = {"passed": not _reasons, "reasons": _reasons,
                  "baseline_load_cases": [lc.id for lc in m.load_condition], "title": m.title,
                  "initial_units": str(m.units.current)}
# No SystemExit here: in an embedded interpreter it could end the host application.


@step("E1_environment")
def _():
    raw = object.__getattribute__(get_core(), "_module")
    out = {"python": sys.version, "ramelements_version": getattr(re, "__version__", None),
           "core_functions": sorted(n for n in dir(raw) if not n.startswith("__"))}
    try:
        out["__file__"] = __file__
    except NameError:
        out["__file__"] = "NameError"
    try:
        out["cwd"] = os.getcwd()
    except Exception as exc:
        out["cwd"] = type(exc).__name__ + ": " + str(exc)
    return out


@step("E2_sandbox")
def _():
    out = {}
    for mod in ("json", "math", "hashlib", "base64", "binascii", "datetime", "threading", "traceback"):
        try:
            __import__(mod)
            out["import_" + mod] = "ok"
        except Exception as exc:
            out["import_" + mod] = type(exc).__name__ + ": " + str(exc)
    out["os.makedirs_is_none"] = getattr(os, "makedirs", None) is None
    out["os.replace_is_none"] = getattr(os, "replace", None) is None
    try:
        import hashlib
        out["sha256_abc"] = hashlib.sha256(b"abc").hexdigest()
    except Exception as exc:
        out["sha256_abc"] = type(exc).__name__ + ": " + str(exc)
    try:
        with open(OUT_DIR + "\\write-test.txt", "x", encoding="utf-8") as fh:
            fh.write("ok")
        out["open_x_in_out_dir"] = "ok"
    except Exception as exc:
        out["open_x_in_out_dir"] = type(exc).__name__ + ": " + str(exc)
    payload = json.loads("{\"a\": [1.5, \"\\u00f1\"]}")
    out["ascii_json_literal_roundtrip"] = payload
    return out


@step("C5_load_case_category_forms")
def _():
    out = {}
    for case_id, category in (("P1N1", "OTHER"), ("P1N2", "OTHER"), ("P1L", "OTHER")):
        try:
            m.load_condition.add_load_case(case_id, "probe " + case_id, category)
            out[case_id] = "ok (plain string)"
        except Exception as exc:
            out[case_id] = type(exc).__name__ + ": " + str(exc)
    try:
        from ramelements.enums import LoadCategory
        m.load_condition.add_load_case("P1ENUM", "probe enum category", LoadCategory.DEAD)
        out["enum_member"] = {"ok": True, "category_read_back": m.load_condition["P1ENUM"].category}
    except Exception as exc:
        out["enum_member"] = type(exc).__name__ + ": " + str(exc)
    out["self_weight_default_P1N1"] = m.load_condition["P1N1"].self_weight
    try:
        m.load_condition.add_load_case("P1SW", "probe self weight", "OTHER")
        sw = m.load_condition["P1SW"]
        sw.enable_self_weight(True)
        out["self_weight_enabled_P1SW"] = sw.self_weight
        sw.enable_self_weight(False)
        out["self_weight_disabled_P1SW"] = sw.self_weight
    except Exception as exc:
        out["self_weight_P1SW"] = type(exc).__name__ + ": " + str(exc)
    return out


@step("N2_cantilever_scale_LB_IN")
def _():
    # Unit scale in the provisional pair LB_IN: tip deflection against P*L^3/(3*E*I) with W10X49 (Ix 272, Iy 93.4 in^4),
    # E = 29e6 psi. A cantilever along RAM +X loaded along -Y. Also exposes the end-force sign convention.
    m.units.set(re.Units.LB_IN)
    base = m.nodes.add(0.0, 0.0, 0.0, restraints=re.FIXED, description="N2-base")
    tip = m.nodes.add(100.0, 0.0, 0.0, description="N2-tip")
    mem = m.members.add(base.id, tip.id, section="W 10X49", material="A36 (weightless)", description="N2")
    mem.add_point_load("P1N2", LoadDirection.GLOBAL_Y, -1000.0, distance=100.0, is_percentage=True)
    res = m.analyze()
    return {"analysis": repr(res), "tip_displacement": repr(m.results.node_displacement(tip.id, "P1N2")),
            "reaction_base": repr(m.results.node_reaction(base.id, "P1N2")),
            "end_forces": repr(m.results.member_end_forces(mem.id, "P1N2")), "local_axes": mem.get_local_axes(),
            "expected_in_strong": 1000.0 * 100.0 ** 3 / (3.0 * 29.0e6 * 272.0),
            "expected_in_weak": 1000.0 * 100.0 ** 3 / (3.0 * 29.0e6 * 93.4)}


@step("N1_equilibrium_vertical_member")
def _():
    # Vertical cantilever (column-like) along RAM +Y, fixed foot, horizontal tip load along +X; reactions must balance.
    m.units.set(re.Units.LB_IN)
    foot = m.nodes.add(1000.0, 0.0, 0.0, restraints=re.FIXED, description="N1-foot")
    top = m.nodes.add(1000.0, 120.0, 0.0, description="N1-top")
    mem = m.members.add(foot.id, top.id, section="W 10X49", material="A36 (weightless)", description="N1")
    mem.add_point_load("P1N1", LoadDirection.GLOBAL_X, 500.0, distance=100.0, is_percentage=True)
    res = m.analyze()
    return {"analysis": repr(res), "reaction_foot": repr(m.results.node_reaction(foot.id, "P1N1")),
            "tip_displacement": repr(m.results.node_displacement(top.id, "P1N1")), "local_axes": mem.get_local_axes(),
            "expected_in_if_load_bends_strong_axis": 500.0 * 120.0 ** 3 / (3.0 * 29.0e6 * 272.0),
            "expected_in_if_load_bends_weak_axis": 500.0 * 120.0 ** 3 / (3.0 * 29.0e6 * 93.4)}


@step("U1_units_change_after_data")
def _():
    m.units.set(re.Units.LB_IN)
    n = m.nodes.add(2000.0, 0.0, 0.0, description="U1")
    before = n.get_coordinates()
    m.units.set(re.Units.N_MM)
    after_mm = n.get_coordinates()
    mem = [x for x in m.members if x.description == "N2"][0]
    load_mm = repr(mem.get_point_load("P1N2", 0))
    m.units.set(re.Units.KIP_FT)
    after_ft = n.get_coordinates()
    load_kip = repr(mem.get_point_load("P1N2", 0))
    m.units.set(re.Units.LB_IN)
    return {"set_in_LB_IN": before, "read_in_N_MM": after_mm, "read_in_KIP_FT": after_ft,
            "N2_load_in_N_MM": load_mm, "N2_load_in_KIP_FT": load_kip, "read_back_in_LB_IN": n.get_coordinates()}


@step("A1_orientation_coverage")
def _():
    # Members the Cantilever slice produces under the hypothesis X_RAM=X, Y_RAM=Z, Z_RAM=-Y: vertical (+Y), horizontal along
    # RAM -Z and +Z, inclined in the RAM Y-Z plane, and reversed; each at rotation 0 and 90.
    m.units.set(re.Units.LB_IN)
    o = (3000.0, 0.0, 0.0)
    ends = {"vert_+Y": (0.0, 100.0, 0.0), "horiz_-Z": (0.0, 0.0, -100.0), "horiz_+Z": (0.0, 0.0, 100.0),
            "incl_YZ": (0.0, 4.375, -100.0), "horiz_+X": (100.0, 0.0, 0.0)}
    origin = m.nodes.add(o[0], o[1], o[2], restraints=re.FIXED, description="A1-o")
    out = {}
    for k, (dx, dy, dz) in ends.items():
        far = m.nodes.add(o[0] + dx, o[1] + dy, o[2] + dz, description="A1-" + k)
        for label, (i, j) in (("fwd", (origin.id, far.id)), ("rev", (far.id, origin.id))):
            mem = m.members.add(i, j, section="W 10X49", material="A36", description="A1-" + k + "-" + label)
            rec = {"rot0": mem.get_local_axes()}
            mem.rotation = 90.0
            rec["rot90"] = mem.get_local_axes()
            mem.rotation = 12.3456
            rec["rot12.3456"] = mem.get_local_axes()
            rec["rotation_read_back"] = mem.rotation
            out[k + "-" + label] = rec
    return out


@step("S1_names_and_orphans")
def _():
    m.units.set(re.Units.LB_IN)
    a = m.nodes.add(4000.0, 0.0, 0.0, restraints=re.FIXED, description="S1-a")
    b = m.nodes.add(4000.0, 100.0, 0.0, restraints=re.FIXED, description="S1-b")
    res = {}
    for label, sec, mat in [("w_known", "W 12X26", "A992 Gr50"), ("hss_known", "HSS_SQR 4X4X1_4", "A500 GrC rectangular"),
                            ("s_known", "S 10X35", "A36"), ("eu_name", "IPE 200", "A36"),
                            ("unknown_section", "RACKCAD_PROBE_NO_SECTION", "A36"),
                            ("unknown_material", "W 12X26", "RACKCAD_PROBE_NO_MATERIAL")]:
        before = len(m.members)
        try:
            mem = m.members.add(a.id, b.id, section=sec, material=mat, description="S1-" + label)
            res[label] = {"requested": [sec, mat], "read_back": [mem.section, mem.material]}
        except Exception as exc:
            res[label] = {"requested": [sec, mat], "error": type(exc).__name__ + ": " + str(exc)}
        res[label]["members_added"] = len(m.members) - before
    return res


@step("R1_member_defaults_and_setters")
def _():
    m.units.set(re.Units.LB_IN)
    a = m.nodes.add(5000.0, 0.0, 0.0, restraints=re.FIXED, description="R1-a")
    b = m.nodes.add(5100.0, 0.0, 0.0, restraints=re.PINNED, description="R1-b")
    mem = m.members.add(a.id, b.id, section="W 12X26", material="A992 Gr50", description="R1")
    defaults = {"cardinal_point": int(mem.cardinal_point), "offset_i": mem.rigid_end_offset_i, "offset_j": mem.rigid_end_offset_j,
                "stiffness_factors": mem.get_stiffness_factors(), "axial_behavior": repr(mem.axial_behavior),
                "spring_i": mem.rotational_stiffness_i, "spring_j": mem.rotational_stiffness_j,
                "releases": repr(mem.end_releases), "restraints_b": repr(m.nodes[b.id].restraints)}
    mem.cardinal_point = re.CardinalPoint.CENTROID
    defaults["cardinal_point_after_set"] = int(mem.cardinal_point)
    setters = {}
    for label, action, reader in (
            ("offset_i", lambda: setattr(mem, "rigid_end_offset_i", (1.25, 0.0, 0.0)), lambda: mem.rigid_end_offset_i),
            ("offset_i_reset", lambda: setattr(mem, "rigid_end_offset_i", (0.0, 0.0, 0.0)), lambda: mem.rigid_end_offset_i),
            ("offset_j", lambda: setattr(mem, "rigid_end_offset_j", (0.0, 0.75, 0.0)), lambda: mem.rigid_end_offset_j),
            ("offset_j_reset", lambda: setattr(mem, "rigid_end_offset_j", (0.0, 0.0, 0.0)), lambda: mem.rigid_end_offset_j),
            ("stiffness_factors", lambda: mem.set_stiffness_factors(0.5, 0.5, 0.5, 0.5), mem.get_stiffness_factors),
            ("stiffness_factors_reset", lambda: mem.set_stiffness_factors(1.0, 1.0, 1.0, 1.0), mem.get_stiffness_factors),
            ("axial_behavior", lambda: setattr(mem, "axial_behavior", mem.axial_behavior), lambda: repr(mem.axial_behavior)),
            ("spring_i", lambda: setattr(mem, "rotational_stiffness_i", 1.0e7), lambda: mem.rotational_stiffness_i),
            ("spring_i_reset", lambda: setattr(mem, "rotational_stiffness_i", 0.0), lambda: mem.rotational_stiffness_i),
            ("spring_j", lambda: setattr(mem, "rotational_stiffness_j", 1.0e7), lambda: mem.rotational_stiffness_j),
            ("spring_j_reset", lambda: setattr(mem, "rotational_stiffness_j", 0.0), lambda: mem.rotational_stiffness_j)):
        try:
            action()
            setters[label] = {"ok": True, "read_back": reader()}
        except Exception as exc:
            setters[label] = {"ok": False, "error": type(exc).__name__ + ": " + str(exc)}
    defaults["setters"] = setters
    return defaults


@step("L1_percent_point_load_readback")
def _():
    mem = [x for x in m.members if x.description == "R1"][0]
    mem.add_point_load("P1L", LoadDirection.GLOBAL_Y, -1234.5, distance=37.5, is_percentage=True)
    return {"count": mem.point_load_count("P1L"), "load": repr(mem.get_point_load("P1L", 0))}


@step("P2_precision_non_round_values")
def _():
    m.units.set(re.Units.LB_IN)
    a = m.nodes.add(6123.456789012, 12.345678901, -7.654321098, restraints=re.FIXED, description="P2-a")
    b = m.nodes.add(6223.456789012, 12.345678901, -7.654321098, restraints=re.FIXED, description="P2-b")
    mem = m.members.add(a.id, b.id, section="W 12X26", material="A992 Gr50", description="P2")
    mem.rotation = 12.3456
    mem.add_point_load("P1L", LoadDirection.GLOBAL_Y, -1234.56789, distance=37.123456, is_percentage=True)
    snap_lb_in = {"node_a": m.nodes[a.id].get_coordinates(), "rotation": mem.rotation,
                  "load": repr(mem.get_point_load("P1L", 0))}
    m.units.set(re.Units.N_MM)
    snap_n_mm = {"node_a": m.nodes[a.id].get_coordinates(), "rotation": mem.rotation,
                 "load": repr(mem.get_point_load("P1L", 0))}
    m.units.set(re.Units.LB_IN)
    snap_back = {"node_a": m.nodes[a.id].get_coordinates(), "load": repr(mem.get_point_load("P1L", 0))}
    return {"written": {"node_a": [6123.456789012, 12.345678901, -7.654321098], "rotation": 12.3456,
                        "load": [-1234.56789, 37.123456]},
            "read_LB_IN": snap_lb_in, "read_N_MM": snap_n_mm, "read_back_LB_IN": snap_back}


@step("S9_disposable_save_as")
def _():
    target = OUT_DIR + "\\probe-model.retx"
    before = set(os.listdir(OUT_DIR))
    m.save_as(target)
    return {"requested_path": target, "new_files": sorted(set(os.listdir(OUT_DIR)) - before), "title_after": m.title}



if _reasons:
    print("I-65 P1 ABORTED before any change:", "; ".join(_reasons))
    print(json.dumps(REPORT, indent=2, default=repr))
else:
    run_steps()
    emit()
