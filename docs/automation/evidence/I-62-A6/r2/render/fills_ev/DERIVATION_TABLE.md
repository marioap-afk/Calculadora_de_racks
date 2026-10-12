```text
DERIVACIÓN  rackcad-input-closure/v1 (E) -> rackcad-bmin-closure/0-experimental
Entradas: E; la delegación D, la entrega H y el contrato K cuyos blobs fija E; la invocación del rol I (UnitId, TaskId, Attempt, RunId,
Target); el repositorio del -C (solo primitivas de grafo).

C1 Commits  = {E.AuthorityRevision} U {D.BaseSha, D.MainSha, D.ChainBaseSha, D.ChainRedSha si no es null}
              U {H.BaseSha, H.CurrentSha, H.RedSha si no es null} U {I.Target.Commit} U {K.AuthorityRevision, K.MainSha}
              U RevList(D.BaseSha..H.CurrentSha)
              U FirstParentChain(D.MainSha) U Parents(m) para cada m de FirstParentChain(D.MainSha) con >= 2 padres
              U {MergeBase(D.MainSha, K.AuthorityRevision)}
C2 Refs     = {HEAD, refs/remotes/origin/<D.ExpectedBranch>, refs/remotes/origin/main}
C3 RevPaths = {(c, p) : c en Commits, (p, b) en E.CanonicalInputs U E.AllowedTransitiveInputs U E.AutomaticInstructions,
              Blob(c:p) = b}                                   (identidad por blob)
C4 Paths    = {p de cada entrada de E} U {o.Target : o en E.Obligations, o.Class = READ, o.Target no null}
              U Surfaces(M) U {f.Path : f en Files(M)} U {a.Path : a en K.Authorities} U {IgnorePath(I)}
              M = el mapa de cláusulas cuyo blob fija E en su ruta fija;
              IgnorePath(I) = artifacts/orchestration/<I.UnitId>/<I.TaskId>/<I.Attempt>/<I.RunId>/output.json
C5 Objects  = {}                                               (árboles de Commits y blobs de RevPaths, implícitos)
C6 Files    = {(Name(p), SHA256(bytes(p, b))) : (p, b) en E.CanonicalInputs, Name(p) definido}
              Name: ruta del contrato -> CONTRACT; artifacts/orchestration/<u>/<t>/<a>/<D.RunId>/output.json -> DELEG;
              .../<H.RunId>/worker-handoff.json -> H; *.trx -> TRX
              bytes(p, b) = los bytes custodiados cuyo blob Git es b (verificados)
```

La tabla no es mecánica desde `input-closure/v1` solo: C1 necesita los bytes de D, H, K e I, que E fija por blob, y tres primitivas de grafo de
Git; C2 y los nombres de C6 son reglas fijas de la tabla, porque el esquema no los representa; `IgnorePath` sale de I. Con esas entradas, la
derivación no tiene ningún paso de juicio. Sobre los datos reales, el cierre derivado deja dentro las 11 ejecuciones VERIFY de la prueba (R9
`MATCH`) y fuera las 4 que leen fuera (R9 `MISMATCH`). Si el cierre efectivo aplica la regla I6 de forma estricta, lecturas legítimas sobre la
entrega en `CurrentSha` quedan fuera: es la pregunta Q-A6R2-06 (§8).
