"""I-62, decisiones §87 (recuperación del transporte; opción A aprobada por el Coordinator): deriva los archivos del kit de la revisión formal acotada de
A-6 ronda 4 (R20261014T180556Z-5e02) que cambian por el binario nuevo de claude-cli (2.1.299) y por la autorización §87, por sustituciones EXACTAS
(prep/subst.py) desde el kit fijado a §86 y congelado (manifiesto cf71191e, copia verificada en prep/s87/pinned-kit/).

Cambia SOLO:
  * launch.py: las constantes del binario (BIN_DISPLAY, BIN, BIN_SHA, VERSION) y las dos menciones de la docstring (SHA-256 y versión);
  * make_closure.py: el bloque de transporte (BINARY; AddDirStatus, ExpectedInit.claude_code_version y Characterization, que nombran el registro de
    caracterización nuevo) y la versión de claude-cli de DeclaredRuntimeContext (referencia de transporte fuera de la clave Transport);
  * verify_clone.py: la comprobación del registro de caracterización: copia en kit/ (fuera del clon, que sigue en 088b3227) con su SHA-256 fijado y
    su blob igual al del registro en el commit 1c960c0a de la rama (leído del worktree, solo lectura), en lugar de exigirlo dentro del clon;
  * post-review.py: la versión de la docstring (2.1.293 → 2.1.299);
  * selftest-post-review.py: las versiones 2.1.293 de los fixtures (transcripción, adjunto, init y los casos de decodificación);
  * owner_gate.py: las tres constantes AUTH_SECTION, AUTH_HEADING y AUTH_TEXT_SHA, de §86 a §87 (pin_authorization.values sobre el texto de
    decisiones de la HEAD 1c960c0a; Ready exigido).
Escribe kit/<archivo>, los diffs en prep/diffs/s87/ y prep/s87/pin87.json (antes/después de la fijación).
Uso: python -I -B prep/s87/adapt_transport_s87.py   (desde la carpeta que contiene kit/)
"""
import hashlib
import importlib.util
import json
import os
import subprocess
import sys

sys.stdout.reconfigure(encoding="utf-8")
ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
PREP = os.path.join(ROOT, "prep")
KIT = os.path.join(ROOT, "kit")
SRC = os.path.join(PREP, "s87", "pinned-kit")
sys.path.insert(0, PREP)
from subst import derive  # noqa: E402

WORKTREE = os.path.join(os.environ["USERPROFILE"], ".codex", "worktrees", "architecture-portabilidad-coordinador-principal")
HEAD_EXPECTED = "1c960c0a2bcf07b4a074db78179489ae4a9e4822"
DECISIONS_FILE = os.path.join(PREP, "s87", "decisiones-1c960c0a.txt")
DECISIONS_BLOB = "ed6550c96cd77e1f57dbc967a4d2205efe2afb95"
AUTH_N = 87
BS = chr(92)

PINNED = {   # SHA-256 de las fuentes en el kit fijado a §86 (kit-manifest.json cf71191e)
    "launch.py": "2802716c22483aebfb7c666b65d0ecd66c50e77de82117278a17b1e5605d00a2",
    "make_closure.py": "74c88d269de6fb0d1a8785c4eb9e71f5d16a7472b86a2db60ee3080f973e5ced",
    "verify_clone.py": "bc028e3fd13a555087cf8fbf5fec1b43ecff8573ef49a99f75058b1fd2f655d4",
    "post-review.py": "4f46caa66c5fcd299000e2d2f0c42ebf3a13c4c9fd027f0a57b314c2a8fc485f",
    "selftest-post-review.py": "a86eeac7fb857ec8066563e6284a8c2c572450cf8b1923b41c6e25944aab9529",
    "owner_gate.py": "b8efee09df1f700fc72e274edd005340322addd58967cc34534c75d2b2231ac0",
    "pin_authorization.py": "a8e4070751ae604010149c8678aa9320766c533230bf92de3aa2e88ea6e571cc",
    "closure.json": "8c70a195fd0b1b7e34f3b9095b3669df34b84a173c3e117fedccd117acdefc42",
}
OLD_DIR = "%APPDATA%" + BS + "Claude" + BS + "claude-code" + BS + "2.1.293" + BS + "83cb0bd7fed4" + BS + "claude.exe"
NEW_DIR = "%APPDATA%" + BS + "Claude" + BS + "claude-code" + BS + "2.1.299" + BS + "f0584be0fbaf" + BS + "claude.exe"
OLD_SHA = "8693c4a02dde7441d0066ede68af8ddfc408bb982d77e12b506286268224e6fa"
NEW_SHA = "295b49ec092f31a7de9c6ff286d9d8f120a4f029b9ad9a5ac0b12c35cbf4df2b"
ELL = chr(0x2026)


def sha_file(p):
    return hashlib.sha256(open(p, "rb").read()).hexdigest()


def src(n):
    return os.path.join(SRC, n)


def go(name, subs, diff):
    r = derive(src(name), PINNED[name], subs, os.path.join(KIT, name), "s87/" + diff, "kit fijado a §86 (cf71191e)/" + name,
               "kit/" + name + " (decisiones §87)")
    r["OldSha256"] = PINNED[name]
    return r


out = {}
for n, s in PINNED.items():
    assert sha_file(src(n)) == s, "fuente %s distinta de la fijada" % n

# ---- launch.py
out["launch.py"] = go("launch.py", [
    ("L1-docstring-sha", "SHA-256 = 8693c4a0" + ELL + ",", "SHA-256 = 295b49ec" + ELL + ","),
    ("L2-docstring-version", "= «2.1.293 (Claude Code)»", "= «2.1.299 (Claude Code)»"),
    ("L3-BIN_DISPLAY", 'BIN_DISPLAY = r"' + OLD_DIR + '"', 'BIN_DISPLAY = r"' + NEW_DIR + '"'),
    ("L4-BIN", '"claude-code", "2.1.293", "83cb0bd7fed4", "claude.exe")', '"claude-code", "2.1.299", "f0584be0fbaf", "claude.exe")'),
    ("L5-BIN_SHA", 'BIN_SHA = "%s"' % OLD_SHA, 'BIN_SHA = "%s"' % NEW_SHA),
    ("L6-VERSION", 'VERSION = "2.1.293 (Claude Code)"', 'VERSION = "2.1.299 (Claude Code)"'),
], "launch.diff")

# ---- make_closure.py (bloque de transporte)
I38, I42 = " " * 38, " " * 42
out["make_closure.py"] = go("make_closure.py", [
    ("C1-BINARY", 'BINARY = {"Path": r"' + OLD_DIR + '",\n          "Sha256": "' + OLD_SHA + '",\n          "Version": "2.1.293 (Claude Code)"',
     'BINARY = {"Path": r"' + NEW_DIR + '",\n          "Sha256": "' + NEW_SHA + '",\n          "Version": "2.1.299 (Claude Code)"'),
    ("C2-AddDirStatus",
     '"AddDirStatus": "medido en C3 de la caracterización R20261008T183600Z-char sobre la plantilla exacta con marcadores (decisiones §56, "\n'
     + I38 + '"punto 7; §57, punto 3): kit/transport-gate.json en MEASURED con el SHA-256 del registro fijado",',
     '"AddDirStatus": "medido en C3 de la caracterización R20261016T185242Z-char de claude-cli 2.1.299 sobre la plantilla exacta "\n'
     + I38 + '"con marcadores (decisiones §87, puntos 2-4; procedimiento de §56, punto 7): kit/transport-gate.json en MEASURED "\n'
     + I38 + '"con el SHA-256 del registro fijado (kit/claude-cli-characterization.json, copia del registro del commit 1c960c0a)",'),
    ("C3-ExpectedInit", '"claude_code_version": "2.1.293"}', '"claude_code_version": "2.1.299"}'),
    ("C4-Characterization",
     '"Characterization": "claude-cli-characterization.json de R20261008T183600Z-char (decisiones §56, punto 7; §57, punto 3): C1 "\n'
     + I42 + '"completada, C3 PASS sobre la plantilla con --add-dir"},',
     '"Characterization": "claude-cli-characterization.json de R20261016T185242Z-char (decisiones §87, puntos 2-3; evidencia §174), "\n'
     + I42 + '"copia en kit/ del registro del commit 1c960c0a (blob 19b8a27e): C1 completada, C3 PASS sobre la plantilla "\n'
     + I42 + '"con --add-dir"},'),
    ("C5-DeclaredRuntimeContext", '"prompt de sistema propio de claude-cli 2.1.293 en modo -p', '"prompt de sistema propio de claude-cli 2.1.299 en modo -p'),
], "make_closure.diff")

# ---- verify_clone.py (registro de caracterización fuera del clon)
out["verify_clone.py"] = go("verify_clone.py", [
    ("V1-docstring-blobs", "el del registro de caracterización que fija la compuerta)",
     "el del registro de caracterización de 2.1.293 que conserva el clon)"),
    ("V2-docstring-registro", "de caracterización (su SHA-256 en el clon = el de transport-gate.json)",
     "de caracterización que fija la compuerta (decisiones §87: copia en kit/, fuera del clon; su SHA-256 = el fijado = el de\n"
     "transport-gate.json, y su blob = el del registro en el commit 1c960c0a de la rama, leído del worktree con git de solo lectura)"),
    ("V3-comprobacion",
     'char_path = gate["Characterization"]["Path"]\n'
     'char_in_clone = os.path.normcase(os.path.abspath(char_path)).startswith(os.path.normcase(CLONE) + os.sep)\n'
     'char_sha = sha(open(char_path, "rb").read())\n',
     'char_path = gate["Characterization"]["Path"]\n'
     '# decisiones §87 (opción A del Coordinator): el registro de caracterización de 2.1.299 no está en el clon (088b3227); la compuerta fija una copia en\n'
     '# kit/, extraída con `git cat-file blob` del commit de la caracterización: su SHA-256 = el fijado y su blob = el del registro en ese commit.\n'
     'CHAR_COMMIT = "1c960c0a2bcf07b4a074db78179489ae4a9e4822"\n'
     'CHAR_REPO_PATH = "docs/automation/evidence/I-62-claude-cli/R20261016T185242Z-char/claude-cli-characterization.json"\n'
     'CHAR_SHA, CHAR_BLOB = "4ba6ba586440ce5c9374c383a9a1b787cf42a17d8451c5d27e762c555cfc2a8c", "19b8a27e0de4c92749147a15c7cea5012289040b"\n'
     'char_bytes = open(char_path, "rb").read()\n'
     'char_sha = sha(char_bytes)\n'
     'char_blob_at_commit = subprocess.run(["git", "-C", WORKTREE, "rev-parse", CHAR_COMMIT + ":" + CHAR_REPO_PATH], capture_output=True, text=True,\n'
     '                                     env=dict(os.environ, GIT_OPTIONAL_LOCKS="0")).stdout.strip()\n'
     'char_in_kit = os.path.normcase(os.path.abspath(char_path)) == os.path.normcase(os.path.join(KIT, "claude-cli-characterization.json"))\n'
     'char_external_ok = char_in_kit and char_sha == CHAR_SHA and git_blob(char_bytes) == CHAR_BLOB and char_blob_at_commit == CHAR_BLOB\n'),
    ("V4-registro",
     '    "TransportCharacterization": {"Path": char_path, "InClone": char_in_clone, "Sha256": char_sha,\n',
     '    "TransportCharacterization": {"Path": char_path, "InKit": char_in_kit, "Sha256": char_sha, "PinnedSha256": CHAR_SHA,\n'
     '                                  "Blob": git_blob(char_bytes), "PinnedBlob": CHAR_BLOB, "Commit": CHAR_COMMIT, "RepoPath": CHAR_REPO_PATH,\n'
     '                                  "BlobAtCommit": char_blob_at_commit, "ExternalCopyOk": char_external_ok,\n'),
    ("V5-AllChecks", 'char_in_clone and doc["TransportCharacterization"]["MatchesGate"]',
     'char_external_ok and doc["TransportCharacterization"]["MatchesGate"]'),
], "verify_clone.diff")

# ---- post-review.py (docstring)
out["post-review.py"] = go("post-review.py", [
    ("P1-docstring-version", "    = 2.1.293 (Claude Code), loggedIn = true", "    = 2.1.299 (Claude Code), loggedIn = true"),
], "post-review.diff")

# ---- selftest-post-review.py (fixtures)
NL, CRNL = BS + "n", BS + "r" + BS + "n"
out["selftest-post-review.py"] = go("selftest-post-review.py", [
    ("S1-transcripcion", '"cwd": CLONE, "version": "2.1.293", "entrypoint": "sdk-cli"}', '"cwd": CLONE, "version": "2.1.299", "entrypoint": "sdk-cli"}'),
    ("S2-adjunto", '"cwd": CLONE, "version": "2.1.293",\n', '"cwd": CLONE, "version": "2.1.299",\n'),
    ("S3-init", '"claude_code_version": "2.1.293", "apiKeySource": "none"}', '"claude_code_version": "2.1.299", "apiKeySource": "none"}'),
    ("S4-G41-G42", 'v = b"2.1.293 (Claude Code)' + NL + '"', 'v = b"2.1.299 (Claude Code)' + NL + '"'),
    ("S5-G46", 'decode_case("G46-decodificacion-utf16-con-bom", "2.1.293 (Claude Code)"', 'decode_case("G46-decodificacion-utf16-con-bom", "2.1.299 (Claude Code)"'),
    ("S6-G48b", 'decode_case("G48b-decodificacion-texto-ya-decodificado", "2.1.293"', 'decode_case("G48b-decodificacion-texto-ya-decodificado", "2.1.299"'),
    ("S7-G50", 'b"2.1.293 (Claude Code)' + CRNL + '", TG.cli_version, version_check("2.1.293 (Claude Code)"))',
     'b"2.1.299 (Claude Code)' + CRNL + '", TG.cli_version, version_check("2.1.299 (Claude Code)"))'),
    ("S8-G51", '"INFORMACIÓN: 2.1.293".encode("cp850")', '"INFORMACIÓN: 2.1.299".encode("cp850")'),
    ("S9-G52", 'standin_case("G52-sustituto-version-utf16", "2.1.293 (Claude Code)"', 'standin_case("G52-sustituto-version-utf16", "2.1.299 (Claude Code)"'),
    ("S10-G54", 'TG.UTF8_BOM + b"2.1.293 (Claude Code)' + NL + '", TG.cli_version, version_check("2.1.293 (Claude Code)", bom=True))',
     'TG.UTF8_BOM + b"2.1.299 (Claude Code)' + NL + '", TG.cli_version, version_check("2.1.299 (Claude Code)", bom=True))'),
], "selftest-post-review.diff")

# ---- owner_gate.py: fijación de §87 (pin_authorization.values sobre el texto de decisiones de la HEAD)
head = subprocess.run(["git", "-C", WORKTREE, "rev-parse", "HEAD"], capture_output=True, text=True, env=dict(os.environ, GIT_OPTIONAL_LOCKS="0")).stdout.strip()
assert head == HEAD_EXPECTED, "HEAD del worktree %s distinta de %s" % (head, HEAD_EXPECTED)
head_blob = subprocess.run(["git", "-C", WORKTREE, "rev-parse", "HEAD:docs/automation/decisions/I-62.md"], capture_output=True, text=True,
                           env=dict(os.environ, GIT_OPTIONAL_LOCKS="0")).stdout.strip()
db = open(DECISIONS_FILE, "rb").read()
assert b"\r" not in db, "el texto de decisiones tiene CR"
file_blob = hashlib.sha1(b"blob %d\x00" % len(db) + db).hexdigest()
assert file_blob == DECISIONS_BLOB == head_blob, "decisiones: blob del archivo %s, fijado %s, HEAD %s" % (file_blob, DECISIONS_BLOB, head_blob)
spec = importlib.util.spec_from_file_location("pin_authorization", src("pin_authorization.py"))
PA = importlib.util.module_from_spec(spec)
spec.loader.exec_module(PA)
obj_blob = json.load(open(src("closure.json"), encoding="utf-8"))["ObjectBlob"]
v = PA.values(db.decode("utf-8"), AUTH_N, obj_blob)
assert v["Ready"] is True, v
new_lines = PA.python_lines(v["Values"])
og_lines = open(src("owner_gate.py"), encoding="utf-8").read().split("\n")
i = next(k for k, l in enumerate(og_lines) if l.startswith("AUTH_SECTION = "))
old_lines = og_lines[i:i + 3]
assert old_lines[0].startswith("AUTH_SECTION = 86 ") and old_lines[1].startswith('AUTH_HEADING = "## 86. ') and \
    old_lines[2].startswith('AUTH_TEXT_SHA = "f85aa811f237b084e222650d2e7e8e88d665d30e5d5931cab57dcd3107a56b76"'), old_lines
out["owner_gate.py"] = go("owner_gate.py", [("O1-autorizacion-87", "\n".join(old_lines), "\n".join(new_lines))], "owner_gate.diff")
pin_record = {"WorktreeHead": head, "DecisionsFile": "prep/s87/decisiones-1c960c0a.txt (git cat-file blob en Git Bash)", "DecisionsBlob": file_blob,
              "DecisionsSha256": hashlib.sha256(db).hexdigest(), "ObjectBlob": obj_blob, "Number": AUTH_N, "Values": v["Values"], "Checks": v["Checks"],
              "Ready": v["Ready"], "Before": old_lines, "After": new_lines,
              "OwnerGateSha256": {"Before": PINNED["owner_gate.py"], "After": sha_file(os.path.join(KIT, "owner_gate.py"))}}
with open(os.path.join(PREP, "s87", "pin87.json"), "w", encoding="utf-8", newline="\n") as f:
    f.write(json.dumps(pin_record, ensure_ascii=False, indent=1) + "\n")
print(json.dumps(out, ensure_ascii=False, indent=1))
