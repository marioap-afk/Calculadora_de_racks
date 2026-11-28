Revision adversarial independiente (I-64, F4-T1-DRAFT-CORE). Solo lectura: no edites, no hagas commit, no ejecutes AutoCAD. Puedes ejecutar git y dotnet test en el worktree indicado.

Worktree: <worktree-de-I-64>
Rango a revisar: git diff 13775d8fb89f3ac983fbeeb6bdcf749e7fd746fc..aefc34a53ff5c00ff1e3663044df067c41ee53cb (rama architecture/workspace-persistente-rackcad). RED = a362744ceaf4c6a86315cd1fcac688f63cd1de93.
dotnet: usa <perfil-de-usuario>\AppData\Local\Microsoft\dotnet\dotnet.exe (el del PATH no sirve). Si ejecutas pruebas, pon resultados solo bajo el directorio temporal del sistema, nunca dentro del worktree.

Fuentes que mandan (leelas en el worktree):
- Contrato: docs/automation/evidence/I-64-pilot/F4-T1-DRAFT-CORE/coordinator/gate-contract.json (Invariants INV-F4T1-01..18, AllowedWriteScope, StopConditions).
- Diseno congelado: docs/initiatives/I-64-proposal-v5.md, decisiones D-10, D-12, D-13 y D-17.
- Paquete del contrato: docs/initiatives/I-64-f4-gate-contract-package.md section 6.
- ADR-0032 D6 (texto pendiente en dos fases) en docs/adr/.

Tu trabajo es intentar REFUTAR que este diff cumple el contrato. Busca, con evidencia de codigo concreta (archivo:linea) y, cuando puedas, una prueba o un escenario reproducible:
1. Incumplimientos de cualquier invariante INV-F4T1-01..18 o de la semantica congelada de D-10, D-12, D-13 y D-17 (estados, transiciones, quien puede salir de Conflicto, Desconocido con sus dos origenes, CheckNow, una sola operacion Aplicando, recargas que sustituyen base e inicial a la vez, fronteras bloqueables y no bloqueables del texto pendiente, atribucion de 4 valores de la StaleBase, union de claves, comparacion ordinal sin normalizacion, veto de cierre desde memoria).
2. Pruebas que no prueban lo que dicen: aserciones vacias o tautologicas, pruebas que pasarian con el esqueleto del RED, escenarios minimos de INV-F4T1-18 que faltan, guarda nueva que no detectaria una violacion real de cada familia prohibida.
3. Estados alcanzables invalidos: transiciones permitidas desde estados que el contrato no admite, perdida de texto pendiente o del borrador, excepciones en flujo normal, estado mutable compartido, dependencias de orden de iteracion.
4. Alcance: archivos fuera de las diez rutas, cambios en archivos existentes, conceptos de host, UI, persistencia o mutacion en el nucleo.

No cuentes preferencias cosmeticas como defectos. Clasifica cada hallazgo como BLOQUEANTE (viola contrato, Freeze, seguridad o comportamiento observable del producto), IMPORTANTE (riesgo real o cobertura que falta para un invariante) o MENOR. Para cada uno: id, clase, invariante o decision afectada, archivo:linea, descripcion, escenario concreto que falla y correccion minima sugerida. Si no encuentras nada en un apartado, dilo explicitamente.

Responde en espanol, en texto plano, terminando con un recuento por clase.
