Aplica la opción 1 ya autorizada. No hace falta una nueva decisión.
La reverificación debe tratar `7bd5a743` únicamente como descendiente de custodia docs-only del GREEN `4f45f446`, manteniendo:

```
VerifiedSha = 4f45f4464f5d434f7fa478fea1117d82283ee997
HEAD = origin/<branch> = 7bd5a743...
attempts = 2 / 3
```

`Identity` puede pasar solo si se vuelve a comprobar mecánicamente que:

1. `4f45f446` es ancestro de `7bd5a743`.
2. `4f45f446..7bd5a743` modifica exclusivamente documentación/custodia.
3. No cambia `src/`, `tests/`, `.csproj`, CI ni build config.
4. Los bytes funcionales siguen siendo exactamente los del GREEN.
5. La CI funcional usada sigue siendo la de `4f45f446`.

Además, la excepción no debe debilitar `nc1`: un `CurrentSha` inexistente o diferente sigue fallando `Identity`.
Puedes lanzar `R20261003T023614Z-e734` con:

* mi decisión sobre `RepresentativeDefinitionId`;
* la excepción de Identity;
* mismo GREEN;
* sin Worker;
* sin replanificación;
* sin consumir `attempts`.

Si sale `EXECUTION_VERIFIED`, sigue directamente con Scope mecánico → pruebas existentes intactas → nc1–nc3 → custodia → juicio G4.
