"""Test-only sabotages of pure composition; production authorities are never edited."""
from pathlib import Path
import hashlib
import json
import subprocess
import xml.etree.ElementTree as ET

root = Path(__file__).resolve().parents[4]
source = root / 'tests/RackCad.Tests/I58F3SeamTests.cs'
directory = Path(__file__).parent / 'controls'
directory.mkdir(exist_ok=True)
original = source.read_bytes()
text = original.decode('utf-8')
controls = [
    ('double-authority', [('authorityCalls++; return authority(d);', 'authorityCalls++; authority(d); authorityCalls++; return authority(d);')], 14),
    ('prepare-reresolve', [('builderCalls++;', 'resolveCalls++; var extra = resolve.Resolve(working); Assert.True(extra.IsSuccess, extra.Diagnostic); builderCalls++;')], 14),
    ('double-builder', [('built = builder(r);', 'builder(r); builderCalls++; built = builder(r);')], 14),
    ('without-working-copy', [('working = copy(comparison.Authored);', 'working = comparison.Authored;'),
                              ('separate(comparison.Authored, working);', '// Allow the real mutation to reach the value/isolation oracle.')], 14),
    ('resolve-on-failure', [('Assert.Null(comparison.Authored);', 'resolveCalls++; resolve.Resolve(comparison.Authored); Assert.Null(comparison.Authored);')], 42),
]
results = []
try:
    for name, edits, expected in controls:
        mutant = text
        for before, after in edits:
            assert mutant.count(before) == 1, (name, before)
            mutant = mutant.replace(before, after)
        source.write_bytes(mutant.encode('utf-8'))
        command = ['dotnet', 'test', 'tests/RackCad.Tests/RackCad.Tests.csproj', '--filter',
                   'FullyQualifiedName~I58F3SeamTests', '--logger', f'trx;LogFileName={name}.trx',
                   '--results-directory', str(directory)]
        with (directory / f'{name}.log').open('w', encoding='utf-8') as log:
            completed = subprocess.run(command, cwd=root, stdout=log, stderr=subprocess.STDOUT)
        tree = ET.parse(directory / f'{name}.trx')
        counters = tree.find('.//{*}Counters').attrib
        failures = tree.findall('.//{*}UnitTestResult[@outcome="Failed"]')
        assert completed.returncode != 0 and int(counters['failed']) == expected and int(counters['total']) == 56, counters
        assert all('Assert.' in ''.join(f.itertext()) for f in failures), 'Every RED must reach an assertion'
        results.append(dict(name=name, expected_failed=expected, counters=counters, command=command,
                            edits=edits, mutant_sha256=hashlib.sha256(mutant.encode('utf-8')).hexdigest()))
        print(name, counters['failed'], 'expected assertion failures', flush=True)
finally:
    source.write_bytes(original)
    (directory / 'summary.json').write_text(json.dumps(dict(
        original_sha256=hashlib.sha256(original).hexdigest(),
        restored=source.read_bytes() == original, controls=results), indent=2), encoding='utf-8')
