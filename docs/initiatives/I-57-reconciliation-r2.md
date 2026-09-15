# Shared View Foundation — Reconciliacion R2

```text
Version              = R2
Autor                = I-57
Predecesor inmutable = R1 @ aa37264381e7d386339305d11030314ffd21915f
R1 blob              = 2faa5a316680aa92306c7bce75c42cc311f96a26
Foundation/claim     = I-57 / 869cf464129da65323781dbebe2425083f21915f
Estado               = PROPOSED BY I-57 · I-52 NOT REGISTERED · I-55 NOT REGISTERED · NOT EFFECTIVE
Integration SHA      = —
```

R2 no edita R1. Incorpora la respuesta remota de I-52, el claim real y las disposiciones del Discovery. Solo es
efectiva cuando I-52, I-55 e I-57 registran este mismo commit exacto; aun entonces solo es consumible tras integrar.

| X | AUTH/titular | Contrato R2 | I-52 conserva | I-55 conserva |
|---|---|---|---|---|
| X-1 selection | 06-07 / I-57 | snapshot+clasificacion minima; fachada duplication intacta | ST/read-set/identidad | seleccion ID19 |
| X-2 views | 01-04,09-11 / I-57 | reuse enum; address, codec, availability y adapters | exposicion/build mirror | exposicion/materializacion |
| X-3 authored | 13 / I-57 | comparator por kind, Selectivo reutilizado | miembros mirror | hermanas/remedio |
| X-4 blocks | 12 / I-57 | requirement puro + query Plugin; importer intacto | AUTH-15 | policy de fallo |
| X-5 protocol | tres iniciativas | consumo solo con Integration SHA en main | reporte | reporte |
| X-6 baselines | 14 / I-57 | CT compartidas; consumidor mantiene fixtures propios | C-2/CT-06 | G-M9 |
| X-7 placement | 08 / I-57 | Transform2D+facts; tolerancia inyectada | reflexion/policy/valor | rigidez/anclaje/Z |
| X-8 frame | 05 / I-57 | un span y centro exacto | c/delta CT-06 | K_min/K_max por policy |

## CR-SVF-I52-01..06

1. La tolerancia es entrada del consumidor y CT-GEO registra la usada.
2. I-57 es titular neutral de 06/07/08/13; producto queda fuera.
3. Gates consumidores requieren Integration SHA alcanzable desde `origin/main`.
4. CT-04/05/16 son I-57; CT-06 permanece I-52.
5. `c=(K_min+K_max)/2`, sin forma afin ni delta absorbida.
6. Query observa despues de `EnsureForPlan`; consultar no importa e importar no declara cumplimiento.

AUTH-01..14 quedan CLAIMED/DESIGNED, NOT IMPLEMENTED, NOT CONSUMABLE; owner/characterization owner I-57. AUTH-15
queda OUT OF FOUNDATION. R2 es inmutable desde publicacion; un cambio crea R3. Solo un nuevo SHA R dispara
relectura. I-52/I-55 deben registrar el SHA exacto o abrir CR. Hasta entonces F1 NOT OPEN.
