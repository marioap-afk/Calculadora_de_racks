# I-65 F0 — Sonda P1 del entorno instalado de RAM Elements (instrumento de medicion, no producto).
#
# Ejecutar SOLO con autorizacion del Owner, dentro de RAM Elements 2026, sobre un MODELO NUEVO Y VACIO:
#   File > New  ->  Python Console  ->  Run File...  (este archivo)
# Antes de ejecutarla, el Coordinator copia este archivo a una carpeta NUEVA fuera del repositorio y fija OUT_DIR abajo a esa carpeta
# (ya creada): la consola podria bloquear os.mkdir/os.makedirs (politica de seguridad observada en el ejecutable), asi que la sonda
# no crea carpetas. Si OUT_DIR no existe o no se puede escribir, el informe completo se imprime en la pestana Output.
# No abre ni modifica archivos existentes del usuario. Al terminar, el modelo debe DESCARTARSE sin guardar (File > New, "No").
# Cada paso captura su excepcion y sigue: el informe registra lo observado, no lo que la documentacion promete.
# Imports deliberadamente minimos (sin traceback, threading, tempfile ni shutil).

import json
import sys

OUT_DIR = r"__OUT_DIR__"
REPORT = {"probe": "I-65-F0-P1", "out_dir": OUT_DIR, "steps": {}}


def step(name):
    def wrap(fn):
        try:
            REPORT["steps"][name] = {"ok": True, "observed": fn()}
        except BaseException as exc:  # the probe records failures instead of stopping
            REPORT["steps"][name] = {"ok": False, "error": type(exc).__name__ + ": " + str(exc)}
        return fn
    return wrap


import ramelements as re
from ramelements._p4d.engine import get_core
from ramelements.enums import LoadDirection
from ramelements.entities.member import EndReleases

m = re.model()


@step("E1_environment")
def _():
    core = get_core()
    raw = object.__getattribute__(core, "_module")
    return {
        "python": sys.version,
        "ramelements_version": getattr(re, "__version__", None),
        "core_functions": sorted(n for n in dir(raw) if not n.startswith("__")),
        "model_title": m.title,
        "initial_units": str(m.units.current),
        "initial_counts": {"nodes": len(m.nodes), "members": len(m.members), "load_cases": len(m.load_condition)},
    }


@step("E2_sandbox")
def _():
    out = {}
    for mod in ("hashlib", "datetime", "math", "os", "threading", "traceback"):
        try:
            __import__(mod)
            out["import_" + mod] = "ok"
        except BaseException as exc:
            out["import_" + mod] = type(exc).__name__ + ": " + str(exc)
    import os
    out["os.makedirs_is_none"] = getattr(os, "makedirs", None) is None
    out["os.replace_is_none"] = getattr(os, "replace", None) is None
    try:
        import hashlib
        out["sha256_abc"] = hashlib.sha256(b"abc").hexdigest()
    except BaseException as exc:
        out["sha256_abc"] = type(exc).__name__ + ": " + str(exc)
    out["out_dir_exists"] = os.path.isdir(OUT_DIR)
    try:
        with open(OUT_DIR + "\\write-test.txt", "w", encoding="utf-8") as fh:
            fh.write("ok")
        out["open_write_in_out_dir"] = "ok"
    except BaseException as exc:
        out["open_write_in_out_dir"] = type(exc).__name__ + ": " + str(exc)
    return out


@step("U1_units_change_after_data")
def _():
    # Is a stored length converted when the active unit pair changes? (Discovery RC-01/RC-02)
    m.units.set(re.Units.N_MM)
    n = m.nodes.add(1000.0, 0.0, 0.0, description="U1")
    before = n.get_coordinates()
    m.units.set(re.Units.KN_M)
    after = n.get_coordinates()
    m.units.set(re.Units.N_MM)
    back = n.get_coordinates()
    return {"set_in_N_MM": before, "read_in_KN_M": after, "read_back_in_N_MM": back}


@step("C5_load_case_category_forms")
def _():
    out = {}
    for case_id, category in (("P1DL", "DL"), ("P1N1", "OTHER"), ("P1N2", "OTHER")):
        try:
            m.load_condition.add_load_case(case_id, "probe " + case_id, category)
            out[case_id] = "ok (plain string)"
        except BaseException as exc:
            out[case_id] = type(exc).__name__ + ": " + str(exc)
    try:
        from ramelements.enums import LoadCategory
        m.load_condition.add_load_case("P1ENUM", "probe enum category", LoadCategory.DEAD)
        out["enum_member"] = {"ok": True, "category_read_back": m.load_condition["P1ENUM"].category}
    except BaseException as exc:
        out["enum_member"] = type(exc).__name__ + ": " + str(exc)
    return out


@step("A1_gravity_and_axes")
def _():
    m.units.set(re.Units.N_MM)
    lc = m.load_condition["P1DL"]
    sw_default = lc.self_weight
    lc.enable_self_weight(True)
    sw_enabled = lc.self_weight
    base = m.nodes.add(0.0, 0.0, 0.0, description="A1-base")
    top_y = m.nodes.add(0.0, 1000.0, 0.0, description="A1-topY")
    top_z = m.nodes.add(0.0, 0.0, 1000.0, description="A1-topZ")
    end_x = m.nodes.add(1000.0, 0.0, 0.0, description="A1-endX")
    my = m.members.add(base.id, top_y.id, section="W 10X49", material="A36", description="A1-vertY")
    mz = m.members.add(base.id, top_z.id, section="W 10X49", material="A36", description="A1-vertZ")
    mx = m.members.add(base.id, end_x.id, section="W 10X49", material="A36", description="A1-horX")
    axes0 = {"Y": my.get_local_axes(), "Z": mz.get_local_axes(), "X": mx.get_local_axes()}
    my.rotation = 90.0
    mx.rotation = 90.0
    axes90 = {"Y": my.get_local_axes(), "X": mx.get_local_axes()}
    lc.enable_self_weight(False)
    return {"self_weight_default": sw_default, "self_weight_enabled": sw_enabled,
            "local_axes_rot0": axes0, "local_axes_rot90": axes90}


@step("S1_section_and_material_names")
def _():
    a = m.nodes.add(5000.0, 0.0, 0.0, description="S1-a")
    b = m.nodes.add(5000.0, 1000.0, 0.0, description="S1-b")
    res = {"members_before": len(m.members)}
    for label, sec, mat in [("known", "W 10X49", "A36"),
                            ("unknown_section", "RACKCAD_PROBE_NO_SECTION", "A36"),
                            ("unknown_material", "W 10X49", "RACKCAD_PROBE_NO_MATERIAL"),
                            ("cold_formed", "aisiCS 4x2x059", "A570 Gr50 cold form"),
                            ("aisc_hss", "HSS_RECT 6X4X1_4", "A500 GrC rectangular")]:
        try:
            mem = m.members.add(a.id, b.id, section=sec, material=mat, description="S1-" + label)
            res[label] = {"requested": [sec, mat], "read_back": [mem.section, mem.material]}
        except BaseException as exc:
            res[label] = {"requested": [sec, mat], "error": type(exc).__name__ + ": " + str(exc)}
        res[label]["members_after"] = len(m.members)
    return res


@step("R1_releases_restraints_offsets")
def _():
    a = m.nodes.add(8000.0, 0.0, 0.0, restraints=re.PINNED, description="R1-a")
    b = m.nodes.add(9000.0, 0.0, 0.0, description="R1-b")
    mem = m.members.add(a.id, b.id, section="W 10X49", material="A36", description="R1")
    mem.end_releases = EndReleases(i_m33=True, i_m22=True, j_m33=True, j_m22=True)
    mem.rigid_end_offset_i = (10.0, 0.0, 0.0)
    return {"restraints_a": repr(m.nodes[a.id].restraints), "releases": repr(mem.end_releases),
            "offset_i": mem.rigid_end_offset_i, "cardinal_point": int(mem.cardinal_point)}


@step("L1_member_point_load_readback")
def _():
    mem = [x for x in m.members if x.description == "R1"][0]
    mem.add_point_load("P1DL", LoadDirection.GLOBAL_Y, -1000.0, distance=50.0, is_percentage=True)
    return {"count": mem.point_load_count("P1DL"), "load": repr(mem.get_point_load("P1DL", 0))}


@step("U2_offset_and_load_groups")
def _():
    # The help puts rigid offsets in the "Section dimensions" unit group and loads in their own groups, while the API documents a
    # single force-length pair. Measure each quantity across a unit switch.
    m.units.set(re.Units.N_MM)
    a = m.nodes.add(30000.0, 0.0, 0.0, description="U2-a")
    b = m.nodes.add(31000.0, 0.0, 0.0, description="U2-b")
    mem = m.members.add(a.id, b.id, section="W 10X49", material="A36", description="U2")
    mem.rigid_end_offset_i = (25.0, 0.0, 0.0)
    mem.add_point_load("P1DL", LoadDirection.GLOBAL_Y, -1000.0, distance=250.0, is_percentage=False)
    mem.add_distributed_load("P1DL", LoadDirection.GLOBAL_Y, -2.0)
    mem.rotational_stiffness_i = 1.0e9
    snap_mm = {"offset_i": mem.rigid_end_offset_i, "point": repr(mem.get_point_load("P1DL", 0)),
               "dist": repr(mem.get_distributed_load("P1DL", 0)), "spring_i": mem.rotational_stiffness_i}
    m.units.set(re.Units.KN_M)
    snap_m = {"offset_i": mem.rigid_end_offset_i, "point": repr(mem.get_point_load("P1DL", 0)),
              "dist": repr(mem.get_distributed_load("P1DL", 0)), "spring_i": mem.rotational_stiffness_i}
    m.units.set(re.Units.N_MM)
    return {"written_in_N_MM": snap_mm, "read_in_KN_M": snap_m}


@step("N1_analysis_equilibrium")
def _():
    # Portal frame with known vertical nodal loads; the sum of reactions must balance the applied load.
    m.units.set(re.Units.N_MM)
    n0 = m.nodes.add(20000.0, 0.0, 0.0, restraints=re.FIXED, description="N1-0")
    n1 = m.nodes.add(26000.0, 0.0, 0.0, restraints=re.FIXED, description="N1-1")
    n2 = m.nodes.add(20000.0, 3000.0, 0.0, description="N1-2")
    n3 = m.nodes.add(26000.0, 3000.0, 0.0, description="N1-3")
    for i, j in [(n0, n2), (n1, n3), (n2, n3)]:
        m.members.add(i.id, j.id, section="W 10X49", material="A36 (weightless)", description="N1")
    m.nodes[n2.id].add_force("P1N1", fy=-10000.0)
    m.nodes[n3.id].add_force("P1N1", fy=-10000.0)
    res = m.analyze()
    reactions = {}
    for node in (n0, n1):
        reactions[node.description] = repr(m.results.node_reaction(node.id, "P1N1"))
    return {"analysis": repr(res), "reactions": reactions}


@step("N2_cantilever_scale")
def _():
    # Unit-scale check independent of equilibrium: tip deflection against P*L^3/(3*E*I), with W10X49 Ix = 272 in^4,
    # Iy = 93.4 in^4 and E = 29000 ksi. Also exposes the end-force sign convention.
    m.units.set(re.Units.N_MM)
    base = m.nodes.add(40000.0, 0.0, 0.0, restraints=re.FIXED, description="N2-base")
    tip = m.nodes.add(42000.0, 0.0, 0.0, description="N2-tip")
    mem = m.members.add(base.id, tip.id, section="W 10X49", material="A36 (weightless)", description="N2")
    m.nodes[tip.id].add_force("P1N2", fy=-1000.0)
    res = m.analyze()
    e_mpa = 29000.0 * 6.894757293168361
    inch4 = 25.4 ** 4
    return {"analysis": repr(res), "tip_displacement": repr(m.results.node_displacement(tip.id, "P1N2")),
            "end_forces": repr(m.results.member_end_forces(mem.id, "P1N2")), "local_axes": mem.get_local_axes(),
            "expected_mm_strong_axis": 1000.0 * 2000.0 ** 3 / (3.0 * e_mpa * 272.0 * inch4),
            "expected_mm_weak_axis": 1000.0 * 2000.0 ** 3 / (3.0 * e_mpa * 93.4 * inch4)}


@step("S9_disposable_save_as")
def _():
    import os
    target = OUT_DIR + "\\probe-model.retx"
    before = set(os.listdir(OUT_DIR))
    m.save_as(target)
    after = set(os.listdir(OUT_DIR))
    return {"requested_path": target, "new_files": sorted(after - before), "title_after": m.title}


text = json.dumps(REPORT, indent=2, default=repr)
try:
    with open(OUT_DIR + "\\p1-report.json", "w", encoding="utf-8") as fh:
        fh.write(text)
    print("I-65 P1 report written to", OUT_DIR + "\\p1-report.json")
except BaseException as exc:
    print("I-65 P1: could not write the report (" + type(exc).__name__ + "); full report follows")
    print(text)
for name, data in REPORT["steps"].items():
    print(name, "OK" if data["ok"] else "FAILED: " + data["error"])
