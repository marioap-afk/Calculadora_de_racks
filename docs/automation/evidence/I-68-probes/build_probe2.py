"""Sonda desechable 2 (ID72): campos pedidos por el Owner.
Filtros Rack y Nivel (sin filtro fijo), filas Descripcion -> Longitud, valores SUM(Cantidad total),
columna calculada en la Table, celdas precalculadas para Vista protegida. No forma parte del repo.
Uso: build_probe2.py <salida.xlsx> <salida_stale.xlsx> <expected.json>"""
import json
import sys
import zipfile
from collections import defaultdict
from xml.sax.saxutils import escape, quoteattr

NS = "http://schemas.openxmlformats.org/spreadsheetml/2006/main"
REL = "http://schemas.openxmlformats.org/officeDocument/2006/relationships"
PKG = "http://schemas.openxmlformats.org/package/2006/relationships"
HDR = '<?xml version="1.0" encoding="UTF-8" standalone="yes"?>'

headers = ["Rack", "Tipo", "Copias", "Nivel", "Categoria", "Perfil", "Descripcion", "Longitud (in)", "Cantidad", "Cantidad total"]
base = [
    ["R-A", "Selectivo", 3, "Componente", "Cabecera", "", "Cabecera 42x144", 144, 2],
    ["R-A", "Selectivo", 3, "Pieza", "Poste", "C3", "Poste C3", 144, 4],
    ["R-A", "Selectivo", 3, "Pieza", "Travesano", "T1", "Travesaño & diagonal", 40.5, 20],
    ["R-B", "Selectivo", 2, "Componente", "Larguero", "L1", "Larguero 96", 96, 8],
    ["R-B", "Selectivo", 2, "Pieza", "Viga", "V1", "Poste C3", 96, 8],
    ["R-B", "Selectivo", 2, "Pieza", "Viga", "V1", "Poste C3", 144, 1],
    ["R-C", "Cama", 1, "Pieza", "Rodillo", "RD", "Rodillo", 40.5, 30],
]
rows = [r + [r[2] * r[8]] for r in base]
FORMULA = "BomPorRack[[#This Row],[Cantidad]]*BomPorRack[[#This Row],[Copias]]"
CAP_ROW = "Descripcion / Longitud"
CAP_DATA = "Suma de Cantidad total"
CAP_GRAND = "Total general"
CAP_ALL = "(Todas)"


def col(i):
    return chr(ord("A") + i)


def num(v):
    return repr(v) if isinstance(v, float) else str(v)


def cell(ref, v, bold=False):
    s = ' s="1"' if bold else ""
    if isinstance(v, (int, float)):
        return f'<c r="{ref}"{s}><v>{num(v)}</v></c>'
    return f'<c r="{ref}"{s} t="inlineStr"><is><t xml:space="preserve">{escape(v)}</t></is></c>'


def data_sheet():
    xml = '<row r="1">' + "".join(cell(f"{col(i)}1", h, True) for i, h in enumerate(headers)) + "</row>"
    for r, row in enumerate(rows, start=2):
        cells = "".join(cell(f"{col(i)}{r}", v) for i, v in enumerate(row[:-1]))
        cells += f'<c r="J{r}"><f>{escape(FORMULA)}</f><v>{num(row[-1])}</v></c>'
        xml += f'<row r="{r}">{cells}</row>'
    return (f'{HDR}<worksheet xmlns="{NS}" xmlns:r="{REL}"><sheetData>{xml}</sheetData>'
            '<tableParts count="1"><tablePart r:id="rId1"/></tableParts></worksheet>')


LAST = f"J{len(rows) + 1}"


def table():
    columns = ""
    for i, h in enumerate(headers):
        if h == "Cantidad total":
            columns += f'<tableColumn id="{i + 1}" name={quoteattr(h)}><calculatedColumnFormula>{escape(FORMULA)}</calculatedColumnFormula></tableColumn>'
        else:
            columns += f'<tableColumn id="{i + 1}" name={quoteattr(h)}/>'
    return (f'{HDR}<table xmlns="{NS}" id="1" name="BomPorRack" displayName="BomPorRack" ref="A1:{LAST}" totalsRowShown="0">'
            f'<autoFilter ref="A1:{LAST}"/><tableColumns count="{len(headers)}">{columns}</tableColumns>'
            '<tableStyleInfo name="TableStyleMedium2" showFirstColumn="0" showLastColumn="0" showRowStripes="1" showColumnStripes="0"/></table>')


IDX = {h: i for i, h in enumerate(headers)}
racks = sorted({r[IDX["Rack"]] for r in rows})
niveles = sorted({r[IDX["Nivel"]] for r in rows})
descs = sorted({r[IDX["Descripcion"]] for r in rows})
lens = sorted({r[IDX["Longitud (in)"]] for r in rows})

tree = defaultdict(lambda: defaultdict(int))
for r in rows:
    tree[r[IDX["Descripcion"]]][r[IDX["Longitud (in)"]]] += r[IDX["Cantidad total"]]
grand = sum(sum(v.values()) for v in tree.values())

layout = []
for d in descs:
    layout.append((d, sum(tree[d].values()), True))
    for ln in sorted(tree[d]):
        layout.append((ln, tree[d][ln], False))
TOP = 4
LAST_ROW = TOP + len(layout) + 1


def resumen_sheet(stale):
    k = 1000 if stale else 1
    xml = '<row r="1">' + cell("A1", "Rack") + cell("B1", CAP_ALL) + "</row>"
    xml += '<row r="2">' + cell("A2", "Nivel") + cell("B2", CAP_ALL) + "</row>"
    xml += f'<row r="{TOP}">' + cell(f"A{TOP}", CAP_ROW, True) + cell(f"B{TOP}", CAP_DATA, True) + "</row>"
    r = TOP
    for label, value, bold in layout:
        r += 1
        xml += f'<row r="{r}">' + cell(f"A{r}", label, bold) + cell(f"B{r}", value * k, bold) + "</row>"
    r += 1
    xml += f'<row r="{r}">' + cell(f"A{r}", CAP_GRAND, True) + cell(f"B{r}", grand * k, True) + "</row>"
    return (f'{HDR}<worksheet xmlns="{NS}" xmlns:r="{REL}"><sheetData>{xml}</sheetData></worksheet>')


def cache_field(name):
    items = {"Rack": racks, "Nivel": niveles, "Descripcion": descs}.get(name)
    if items is not None:
        body = "".join(f"<s v={quoteattr(x)}/>" for x in items)
        return f'<cacheField name={quoteattr(name)} numFmtId="0"><sharedItems count="{len(items)}">{body}</sharedItems></cacheField>'
    if name == "Longitud (in)":
        body = "".join(f'<n v="{num(x)}"/>' for x in lens)
        return (f'<cacheField name={quoteattr(name)} numFmtId="0"><sharedItems containsSemiMixedTypes="0" containsString="0" containsNumber="1" '
                f'minValue="{num(min(lens))}" maxValue="{num(max(lens))}" count="{len(lens)}">{body}</sharedItems></cacheField>')
    return f'<cacheField name={quoteattr(name)} numFmtId="0"><sharedItems/></cacheField>'


cache = (f'{HDR}<pivotCacheDefinition xmlns="{NS}" xmlns:r="{REL}" saveData="0" refreshOnLoad="1" refreshedBy="RackCad" '
         'refreshedDate="45000" createdVersion="6" refreshedVersion="6" minRefreshableVersion="3" recordCount="0">'
         '<cacheSource type="worksheet"><worksheetSource name="BomPorRack"/></cacheSource>'
         f'<cacheFields count="{len(headers)}">' + "".join(cache_field(h) for h in headers) + "</cacheFields></pivotCacheDefinition>")


def field_items(n):
    return "".join(f'<item x="{i}"/>' for i in range(n)) + '<item t="default"/>'


def pivot_field(name):
    if name == "Rack":
        return f'<pivotField axis="axisPage" showAll="0"><items count="{len(racks) + 1}">{field_items(len(racks))}</items></pivotField>'
    if name == "Nivel":
        return f'<pivotField axis="axisPage" showAll="0"><items count="{len(niveles) + 1}">{field_items(len(niveles))}</items></pivotField>'
    if name == "Descripcion":
        return f'<pivotField axis="axisRow" showAll="0" sortType="ascending"><items count="{len(descs) + 1}">{field_items(len(descs))}</items></pivotField>'
    if name == "Longitud (in)":
        return f'<pivotField axis="axisRow" showAll="0" sortType="ascending"><items count="{len(lens) + 1}">{field_items(len(lens))}</items></pivotField>'
    if name == "Cantidad total":
        return '<pivotField dataField="1" showAll="0"/>'
    return '<pivotField showAll="0"/>'


def row_items():
    out = ""
    for di, d in enumerate(descs):
        out += "<i><x/></i>" if di == 0 else f'<i><x v="{di}"/></i>'
        for ln in sorted(tree[d]):
            li = lens.index(ln)
            out += '<i r="1"><x/></i>' if li == 0 else f'<i r="1"><x v="{li}"/></i>'
    out += '<i t="grand"><x/></i>'
    return out


n_row_items = len(layout) + 1
pivot = (f'{HDR}<pivotTableDefinition xmlns="{NS}" name="ResumenBom" cacheId="1" applyNumberFormats="0" applyBorderFormats="0" '
         'applyFontFormats="0" applyPatternFormats="0" applyAlignmentFormats="0" applyWidthHeightFormats="1" dataCaption="Valores" '
         f'updatedVersion="6" minRefreshableVersion="3" useAutoFormatting="1" itemPrintTitles="1" createdVersion="6" indent="0" '
         f'outline="1" outlineData="1" multipleFieldFilters="0" rowHeaderCaption={quoteattr(CAP_ROW)} grandTotalCaption={quoteattr(CAP_GRAND)}>'
         f'<location ref="A{TOP}:B{LAST_ROW}" firstHeaderRow="1" firstDataRow="1" firstDataCol="1" rowPageCount="2" colPageCount="1"/>'
         f'<pivotFields count="{len(headers)}">' + "".join(pivot_field(h) for h in headers) + "</pivotFields>"
         f'<rowFields count="2"><field x="{IDX["Descripcion"]}"/><field x="{IDX["Longitud (in)"]}"/></rowFields>'
         f'<rowItems count="{n_row_items}">{row_items()}</rowItems>'
         '<colItems count="1"><i/></colItems>'
         f'<pageFields count="2"><pageField fld="{IDX["Rack"]}" hier="-1"/><pageField fld="{IDX["Nivel"]}" hier="-1"/></pageFields>'
         f'<dataFields count="1"><dataField name={quoteattr(CAP_DATA)} fld="{IDX["Cantidad total"]}" baseField="0" baseItem="0"/></dataFields>'
         '<pivotTableStyleInfo name="PivotStyleLight16" showRowHeaders="1" showColHeaders="1" showRowStripes="0" '
         'showColStripes="0" showLastColumn="1"/></pivotTableDefinition>')

styles = (f'{HDR}<styleSheet xmlns="{NS}"><fonts count="2"><font><sz val="11"/><name val="Calibri"/></font>'
          '<font><b/><sz val="11"/><name val="Calibri"/></font></fonts><fills count="2"><fill><patternFill patternType="none"/></fill>'
          '<fill><patternFill patternType="gray125"/></fill></fills><borders count="1"><border/></borders>'
          '<cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs>'
          '<cellXfs count="2"><xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/>'
          '<xf numFmtId="0" fontId="1" fillId="0" borderId="0" xfId="0" applyFont="1"/></cellXfs></styleSheet>')
workbook = (f'{HDR}<workbook xmlns="{NS}" xmlns:r="{REL}"><sheets><sheet name="Por rack" sheetId="1" r:id="rId1"/>'
            '<sheet name="Resumen" sheetId="2" r:id="rId2"/></sheets><pivotCaches><pivotCache cacheId="1" r:id="rId4"/></pivotCaches></workbook>')
ct_main = "application/vnd.openxmlformats-officedocument.spreadsheetml"
content_types = (f'{HDR}<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">'
                 '<Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>'
                 '<Default Extension="xml" ContentType="application/xml"/>'
                 f'<Override PartName="/xl/workbook.xml" ContentType="{ct_main}.sheet.main+xml"/>'
                 f'<Override PartName="/xl/styles.xml" ContentType="{ct_main}.styles+xml"/>'
                 f'<Override PartName="/xl/worksheets/sheet1.xml" ContentType="{ct_main}.worksheet+xml"/>'
                 f'<Override PartName="/xl/worksheets/sheet2.xml" ContentType="{ct_main}.worksheet+xml"/>'
                 f'<Override PartName="/xl/tables/table1.xml" ContentType="{ct_main}.table+xml"/>'
                 f'<Override PartName="/xl/pivotCache/pivotCacheDefinition1.xml" ContentType="{ct_main}.pivotCacheDefinition+xml"/>'
                 f'<Override PartName="/xl/pivotTables/pivotTable1.xml" ContentType="{ct_main}.pivotTable+xml"/></Types>')


def rels(*items):
    return f'{HDR}<Relationships xmlns="{PKG}">' + "".join(
        f'<Relationship Id="{i}" Type="{REL}/{t}" Target="{g}"/>' for i, t, g in items) + "</Relationships>"


def write(path, stale):
    parts = {
        "[Content_Types].xml": content_types,
        "_rels/.rels": rels(("rId1", "officeDocument", "xl/workbook.xml")),
        "xl/workbook.xml": workbook,
        "xl/_rels/workbook.xml.rels": rels(("rId1", "worksheet", "worksheets/sheet1.xml"), ("rId2", "worksheet", "worksheets/sheet2.xml"),
                                           ("rId3", "styles", "styles.xml"), ("rId4", "pivotCacheDefinition", "pivotCache/pivotCacheDefinition1.xml")),
        "xl/styles.xml": styles,
        "xl/worksheets/sheet1.xml": data_sheet(),
        "xl/worksheets/sheet2.xml": resumen_sheet(stale),
        "xl/worksheets/_rels/sheet1.xml.rels": rels(("rId1", "table", "../tables/table1.xml")),
        "xl/worksheets/_rels/sheet2.xml.rels": rels(("rId1", "pivotTable", "../pivotTables/pivotTable1.xml")),
        "xl/tables/table1.xml": table(),
        "xl/pivotCache/pivotCacheDefinition1.xml": cache,
        "xl/pivotTables/pivotTable1.xml": pivot,
        "xl/pivotTables/_rels/pivotTable1.xml.rels": rels(("rId1", "pivotCacheDefinition", "../pivotCache/pivotCacheDefinition1.xml")),
    }
    with zipfile.ZipFile(path, "w", zipfile.ZIP_DEFLATED) as z:
        for name, content in parts.items():
            z.writestr(name, content.encode("utf-8"))


write(sys.argv[1], False)
write(sys.argv[2], True)
expected = [[("" if not isinstance(lb, (int, float)) and lb == "" else lb), v] for lb, v, _ in layout] + [[CAP_GRAND, grand]]
with open(sys.argv[3], "w", encoding="utf-8") as f:
    json.dump({"top": TOP, "rows": expected, "grand": grand}, f, ensure_ascii=False)
print("OK filas_tabla=", len(rows), "filas_resumen=", len(layout) + 1, "grand=", grand)
