# I-26 — preparación de integración

Fecha: 2026-07-19, America/Mexico_City

Rama: `refactor/test-catalog-ids`

Worktree: `C:\Users\alejandra-mendoza\.codex\worktrees\refactor-test-catalog-ids`

Base usada: `origin/main` en `1e2a8b0cecdc92d8d5a6b96ea61912217ca3ba16`

Claim-Id: `8de3b4b4-8d13-4527-b650-b28820f2902a`

## Resultado del rebase final

`git rev-list --left-right --count origin/main...HEAD` devolvió `0 7`: `origin/main` no avanzó
desde la base de I-26 y la rama contenía sus siete commits propios. No fue necesario rebasear ni
publicar con `--force-with-lease`; la validación y el CI previamente confirmados conservaron vigencia.

## Commits de implementación

1. `9981b9dd786fa1fea4b445b87a6cb892878486a7` — reclamo atómico de I-26.
2. `e5f4039c6b4e582ee163129dc917fdf98e94e757` — contrato detallado.
3. `cdbdc642d923665cf48c6f46340ebac6d4b69ba5` — `TestCatalogIds` canónicos.
4. `f1c16d282a8afb06c8f1b4facc68645316656005` — migración contextual de pruebas.
5. `99d06f314d00a063dc0988c28ac3e54ceed9e1b4` — guardián de catálogos.
6. `c39a5d3093250bf54f4209e7d5499eccebfaa84d` — cobertura Cobertura en CI.
7. `2cf3f12684dbe495403f0a16eeaa882e4873e3c6` — cierre de implementación F1-F6.

## Validación automatizada

- `git diff origin/main --check`: limpio.
- Suite Debug: 636/636 pruebas verdes, sin fallos ni omitidas.
- Guardián dirigido: verde contra los catálogos reales copiados al output.
- Build `RackCad.UI` Debug: 0 errores y 0 advertencias.
- Cobertura con la configuración exacta del workflow: 636/636 verdes y exactamente un
  `coverage.cobertura.xml` normalizado.
- XML Cobertura válido: 2,724,796 bytes, versión 1.9, con `RackCad.Domain` y
  `RackCad.Application`; tasas observadas de 91.77 % de líneas y 75.26 % de ramas, sin umbral.

La prueba negativa de F4 retiró temporalmente el poste canónico de la copia ignorada de
`secciones.csv`. El guardián reportó el ID ausente y tres FKs de plantillas afectadas; el archivo se
restauró byte por byte y el guardián volvió a verde. `assets/catalogs` nunca se modificó.

## Cobertura remota y validación del dueño

El dueño confirmó CI #40 verde sobre `2cf3f12684dbe495403f0a16eeaa882e4873e3c6`, incluidos los jobs
de tests y build UI. También confirmó que el artifact `rackcad-coverage-cobertura` existe, fue
descargado y contiene `coverage.cobertura.xml`.

El commit documental creado después de esta evidencia todavía debe recibir CI verde; esta sesión no
lo declara anticipadamente.

## Archivos y alcance

La implementación creó el contrato, `TestCatalogIds` y el guardián; migró expectativas canónicas en
tests; añadió Coverlet exclusivamente a `RackCad.Tests`; y actualizó `ci.yml` y `.gitignore`. La
preparación de integración modifica únicamente:

- `docs/ROADMAP.md`;
- `docs/HANDOFF.md`;
- `docs/initiatives/I-26-test-catalog-ids.md`;
- esta evidencia.

El diff completo bajo `src/`, `assets/catalogs/` y `deploy/` es vacío. No cambiaron producto,
catálogos distribuidos, Plugin, UI, Domain ni Application.

## Paralelismo revisado

- I-07 (`docs/adr-retroactivos`) no modifica `docs/HANDOFF.md` ni `docs/ROADMAP.md` respecto de
  `origin/main`; no existe otra integración concurrente sobre los archivos calientes.
- I-13 (`experiment/refs-autocad-ci`) no modifica `.github/workflows/ci.yml` respecto de
  `origin/main`; sus commits no fueron incorporados, copiados ni mezclados con I-26.
- Después del merge de I-26, I-13 puede rebasarse sobre `main` antes de continuar trabajo en CI.

## Estado de la operación

I-26 queda completada y preparada para integración manual. No se creó Pull Request, no se usaron
GitHub CLI ni API, no se programaron automatizaciones, no se modificó `main`, no hubo merge y no se
eliminaron ramas ni worktrees.
