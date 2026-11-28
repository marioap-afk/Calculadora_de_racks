"""I-64 Smoke-1: builds the fixture drawing D (DXF) with the PR-12 DIAGNOSTICO and PR-13 SIN IDENTIDAD blocks.

Each block definition (BLOCK_RECORD) carries, exactly like RackBlockData.Write does in the product, an extension
dictionary entry RACKCAD_SELECTIVE -> XRECORD whose data is the envelope JSON chunked into <=255-char group-1 strings.
The envelopes are the files produced by the Candidate's own RackEmbedStore.Serialize (FixtureHarness make).
No AutoCAD is used. Usage: make_smoke1_fixtures.py <envelopeDir> <outDxf>
"""
import os
import sys

import ezdxf

ENV_DIR, OUT = sys.argv[1], sys.argv[2]
assert not os.path.exists(OUT), 'output exists'
DICT_KEY = 'RACKCAD_SELECTIVE'
CHUNK = 255
FIXTURES = [
    ('I64_FX_PR13_SIN_IDENTIDAD', 'envelope-pr13-sin-identidad.json', 'PR-13 SIN IDENTIDAD', (0.0, 0.0)),
    ('I64_FX_PR12_DIAGNOSTICO', 'envelope-pr12-diagnostico.json', 'PR-12 DIAGNOSTICO', (150.0, 0.0)),
]

doc = ezdxf.new('R2013', setup=False, units=1)  # INSUNITS = 1 (inches): RackCad's unit (ADR-0005)
doc.header['$MEASUREMENT'] = 0
msp = doc.modelspace()

for name, env_file, label, insert in FIXTURES:
    with open(os.path.join(ENV_DIR, env_file), 'rb') as fh:
        raw = fh.read()
    envelope = raw.decode('utf-8')
    assert envelope.isascii() and '\n' not in envelope and '\r' not in envelope
    block = doc.blocks.new(name=name)
    block.add_lwpolyline([(0, 0), (120, 0), (120, 40), (0, 40)], close=True)
    block.add_text(label, height=6.0, dxfattribs={'insert': (6.0, 22.0)})
    block.add_text('FIXTURE I-64 SMOKE-1 - NO GUARDAR', height=3.0, dxfattribs={'insert': (6.0, 8.0)})
    xdict = block.block_record.new_extension_dict()
    xrecord = xdict.add_xrecord(DICT_KEY)
    xrecord.reset([(1, envelope[i:i + CHUNK]) for i in range(0, len(envelope), CHUNK)])
    msp.add_blockref(name, insert)

msp.add_text('I-64 SMOKE-1 - DIBUJO D DE PRUEBA - CERRAR SIN GUARDAR', height=5.0, dxfattribs={'insert': (0.0, 70.0)})

auditor = doc.audit()
print('audit errors', len(auditor.errors), 'fixes', len(auditor.fixes))
assert not auditor.has_errors
doc.saveas(OUT)
print('saved')
