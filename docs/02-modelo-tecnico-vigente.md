# Modelo tecnico vigente

## Principio principal

Las horizontales son la fuente de verdad.

Los paneles no son entidades libres. Un panel es el espacio entre dos horizontales consecutivas.

Por lo tanto:

```text
Horizontales ordenadas por elevacion
        ->
Paneles consecutivos derivados
        ->
Miembros fisicos para vista previa y futuro dibujo/BOM
```

## Horizontales

Clase principal: `FrameHorizontal`.

Campos relevantes:

- `Id`: identificador secuencial por elevacion (`H1`, `H2`, `H3`...).
- `Number`: numero visual.
- `Elevation`: elevacion fisica.
- `ProfileId`: perfil/catalogo.
- `Quantity`: cantidad fisica en esa elevacion.
- `MountingFace`: `Front`, `Back`, `Both`.
- `State`: `Standard`, `Manual`, `Rule`, `Exception`.
- `Notes`.
- `IsStandard`.

Regla actual:

1. Ordenar horizontales por `Elevation`.
2. Renombrar secuencialmente:
   - menor elevacion = `H1`;
   - siguiente = `H2`;
   - siguiente = `H3`.
3. Reconstruir paneles desde esas horizontales ya renombradas.

## Paneles

Clase principal: `BracingPanel`.

Campos relevantes:

- `PanelId`: `P1`, `P2`, `P3`.
- `Number`.
- `LowerHorizontalId`.
- `UpperHorizontalId`.
- `StartElevation`.
- `EndElevation`.
- `ClearHeight`: derivado de `EndElevation - StartElevation`.
- `Arrangement`: arreglo de panel.
- `MountingFace`: cara de montaje.
- `DiagonalProfileId`.
- `DiagonalDirection`.
- `StartConnectionPointId`.
- `EndConnectionPointId`.
- `IsStandard`.
- `IsException`.
- `Members`: diagonales fisicas derivadas del panel.

Los paneles se regeneran completamente despues de operaciones que cambian horizontales:

- agregar;
- eliminar;
- duplicar;
- mover;
- dividir panel;
- combinar paneles.

## Arreglos de panel

Enum actual: `BracingPattern`.

Valores:

- `NoBracing`
- `SingleDiagonal`
- `DoubleDiagonal`
- `XBracing`
- `KBracing`
- `Custom`

Notas:

- `DoubleDiagonal` no es X.
- `XBracing` es cruce.
- `NoBracing` conserva horizontales pero no genera diagonales.

## Direccion diagonal

Enum actual: `DiagonalDirection`.

Valores:

- `AutoAlternating`
- `UpRight`
- `UpLeft`

`AutoAlternating` alterna por numero de panel:

```text
P1 = /
P2 = \
P3 = /
P4 = \
```

## Miembros fisicos

Clase principal: `FrameMember`.

Los genera `BracingPanelMemberBuilder`.

Actualmente se generan para:

- horizontales;
- diagonales de panel.

Estos miembros alimentan la vista previa WPF y deben ser la base futura para:

- dibujo AutoCAD;
- BOM;
- validaciones fisicas;
- metadatos.

## Invariantes del modelo

Siempre debe cumplirse:

```text
Paneles = funcion(Horizontales)
```

Si las horizontales son:

```text
H1 = 0
H2 = 44
H3 = 88
H4 = 132
```

Entonces los paneles validos son:

```text
P1 = H1-H2
P2 = H2-H3
P3 = H3-H4
```

No debe existir:

```text
P2 = H2-H4
```

si `H3` existe.

## Excepciones

Las excepciones se recalculan desde filas editoras:

- `HorizontalEditorRow`.
- `BracingSegmentEditorRow`.
- propiedades de postes y placas.

La restauracion de cabecera estandar limpia todas las excepciones porque reconstruye el modelo desde un snapshot inicial limpio.

