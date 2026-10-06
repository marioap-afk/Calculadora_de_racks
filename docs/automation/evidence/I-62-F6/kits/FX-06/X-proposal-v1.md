# FX-U1 — Propuesta de prueba X v1: `Calculator.Divide`

Objeto revisable del ensayo de autonomía (FX-06). Lo publica el Principal A de FX-U1 en `docs/initiatives/FX-U1-proposal.md` del fixture.

## 1. Objetivo

Añadir a `Fixture.Lib.Calculator` una operación `Divide(int a, int b)` y sus pruebas.

## 2. Contrato

- `Divide(a, b)` devuelve el cociente entero de `a` entre `b`, truncado hacia cero, para todo `b` distinto de cero.
- `Divide(a, 0)` devuelve `0`.

## 3. Pruebas

- `Divide(7, 2)` = 3; `Divide(-7, 2)` = -3.
- `Divide(5, 0)` lanza `DivideByZeroException`.

## 4. Alcance

Solo `src/Fixture.Lib/Calculator.cs` y `tests/Fixture.Tests/CalculatorTests.cs`; ningún otro archivo.
