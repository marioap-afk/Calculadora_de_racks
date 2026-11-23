# I-64 F4B PB-T5 — Guía operativa del Smoke-PB contratado (smoke-run v1)

> **Redacción para el repo público:** las rutas locales van como marcadores, salvo la carpeta de trabajo, `<raiz-de-trabajo>\`, que fijó la sesión principal. Al custodiar, `<carpeta-del-Smoke-PB>` pasa a `<carpeta-del-Smoke-PB>` y `<carpeta-de-respaldo>` a `<carpeta-de-respaldo>`, como las copias exportadas `r71…` pasaron a `<copia>` (F4B-E60). Ningún nombre personal ni de cliente. La hora del Owner, en UTC.

**Qué es:** la guía con las órdenes exactas del Smoke-PB **contratado** de PB-T5, el intento 1 de 3, después de la emisión (F4B-E62).
- Los pasos, sus orígenes y sus criterios están en `I-64-f4b-pbt5-package-v2.3.md` ([P]).
- Las decisiones del Owner, en `owner-pbt5-decisions.md` ([OD5]).

**De dónde sale:**
- de la guía de la corrida previa, `pre-run-tonight-v2.4.md`, con todas sus correcciones: I-07 a I-09 y M-18 a M-35 de [IR1] y sus recomprobaciones;
- más lo que añaden las decisiones del Owner:
  - las inyecciones (§4.11);
  - los contextos construidos (§4.12);
  - el recuento del estilo de cotas (§4.13);
  - la dispensa de la UI completa (P-02);
  - la frontera de esta noche (§2);
  - lo que se deja al Owner (§6).
- La corrida previa no llegó a hacerse.

**Los valores:**

| Marcador | Valor |
|---|---|
| `<carpeta-del-Smoke-PB>` | `<carpeta-del-Smoke-PB>` (en LISP, `<carpeta-del-Smoke-PB-con-/>`) |
| `<carpeta-de-respaldo>` | `<carpeta-de-respaldo>` |
| `<worktree>`, `<checkout-principal>`, `<dotnet-del-usuario>`, `<python-del-usuario>`, `<acad.exe>` (la ruta larga y completa), `<dir-del-acceso-directo>`, `<original-D1>`, `<carpeta-de-registros-de-RackCad>`, `<lista-local-de-la-seleccion>` (un archivo local, nunca custodiado, con una ruta de original por línea) | Los pone la sesión en cada bloque. Cada bloque para si alguno se queda sin rellenar |
| `<Candidato>`, `<producto>`, `<run>` y los 19 filtros de UI | **[T4e]**: el cierre de PB-T4e y su CI |

**La regla de los bloques de PowerShell** (I-08, M-33 y la medida de la v2.3):
- cada bloque es una orden entera, que funciona sola en un proceso nuevo;
- empieza con `Set-StrictMode -Version Latest` y `$ErrorActionPreference = 'Stop'`;
- para si un valor está vacío o sin rellenar, o si una ruta no existe;
- comprueba `$LASTEXITCODE` tras cada orden externa;
- nunca escribe en la raíz de una unidad;
- y termina con `exit 0` después de su última línea.
- Los bloques largos (B1 y B2) corren en segundo plano.

---

## 1. Qué es, y qué no es

- **Es el Smoke-PB de PB-T5,** emitido por la autorización condicional del Owner (Q-PBT5-7), con el contrato de la forma B (Q-PBT5-8).
  - Un intento es esta corrida sobre este Candidato (Q-PBT5-9).
  - No cuentan las repeticiones por error del operador (dos como máximo), una parada del entorno antes de abrir AutoCAD ni una adenda que pida el Owner.
- **Los resultados que registra la sesión son propuestas.**
  - Los juicios S+O, los OBSERVADO, SPB-07 si ningún dibujo guarda un estilo, la disposición y la firma son del Owner (§6).
  - Antes de su firma, una revisión independiente de solo lectura comprueba que el registro, las capturas y los inventarios coinciden (Q-PBT5-8).
- **Las tres reglas de Smoke-2,** adoptadas (Q-PBT5-9):
  - las etiquetas: NOT AVAILABLE FOR PHYSICAL SMOKE; NO VERIFICABLE FÍSICAMENTE / CUBIERTO POR PRUEBA AUTOMATIZADA; OBSERVADO;
  - traer la evidencia antes de decidir un OBSERVADO;
  - una copia de trabajo por escenario.

## 2. La frontera

**El mensaje del Owner de esta noche**, literal, con la zona horaria redactada:

> «En caso de que se complete antes de las 6 am y quieras ejecutar el smoke, ejecutalo tú ya que estaré dormido, a menos que sean después de las 7 am de `<zona-horaria-del-Owner>`»

**La lectura de la sesión principal** (Q-PBT5-5 no se le preguntó):
- **La sesión opera solo si puede empezar antes de las 13:00 UTC del 2026-11-23,** es decir, si a esa hora tiene cumplidas P-01 a P-08 y hecha la emisión.
- **A las 13:00 UTC,** se para en el siguiente límite de paso y se espera al Owner:
  - no se empieza otro paso con AutoCAD;
  - AutoCAD y la copia abierta se quedan como estén, sin guardar;
  - se registra el punto exacto;
  - y solo se hacen SPB-51 y SPB-52 (§4.9), que leen, con la nota «con AutoCAD abierto».
- **Después, con el Owner:** él opera los gestos que quiera, y la sesión mide lo de máquina, avisa antes de teclear y devuelve el control.
- **Antes de cada paso,** la hora UTC va al registro.

