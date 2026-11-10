"""Sonda desechable 3 (I-68): BOM vacio, filas sin Nivel, y Cantidad total como VALOR (preferencia de la especificacion).
Uso: build_probe3.py <directorio_salida>"""
import os
import sys
import zipfile
from collections import defaultdict
from xml.sax.saxutils import escape, quoteattr

NS = "http://schemas.openxmlformats.org/spreadsheetml/2006/main"
REL = "http://schemas.openxmlformats.org/officeDocument/2006/relationships"
PKG = "http://schemas.openxmlformats.org/package/2006/relationships"
HDR = '<?xml version="1.0" encoding="UTF-8" standalone="yes"?>'
CT = "application/vnd.openxmlformats-officedocument.spreadsheetml"
H = ["Rack", "Tipo", "Copias", "Nivel", "Categoria", "Perfil", "Descripcion", "Longitud (in)", "Cantidad", "Cantidad total"]
IDX = {h: i for i, h in enumerate(H)}
TABLE = "RackCadBOM"
CAP_ROW = "Descripcion / Longitud"
CAP_DATA = "Suma de Cantidad total"
CAP_GRAND = "Total general"


def col(i):
    return chr(ord("A") + i)


def num(v):
    return repr(v) if isinstance(v, float) else str(v)


def cell(ref, v, bold=False):
    s = ' s="1"' if bold else ""
    if isinstance(v, (int, float)):
        return f'<c r="{ref}"{s}><v>{num(v)}</v></c>'
    return f'<c r="{ref}"{s} t="inlineStr"><is><t xml:space="preserve">{escape(v)}</t></is></c>'


def rels(*items):
    return f'{HDR}<Relationships xmlns="{PKG}">' + "".join(f'<Relationship Id="{i}" Type="{REL}/{t}" Target="{g}"/>' for i, t, g in items) + "</Relationships>"


STYLES = (f'{HDR}<styleSheet xmlns="{NS}"><fonts count="2"><font><sz val="11"/><name val="Calibri"/></font><font><b/><sz val="11"/><name val="Calibri"/></font></fonts>'
          '<fills count="2"><fill><patternFill patternType="none"/></fill><fill><patternFill patternType="gray125"/></fill></fills><borders count="1"><border/></borders>'
          '<cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs><cellXfs count="2"><xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/>'
          '<xf numFmtId="0" fontId="1" fillId="0" borderId="0" xfId="0" applyFont="1"/></cellXfs></styleSheet>')


def build(path, rows, pivot=True, data_rows=None, message=None):
    """rows: filas con Cantidad total ya calculada. data_rows: filas reservadas en el rango de la tabla (para el caso vacio)."""
    n = len(rows) if data_rows is None else data_rows
    last = f"J{n + 1}"
    xml = '<row r="1">' + "".join(cell(f"{col(i)}1", h, True) for i, h in enumerate(H)) + "</row>"
    for r, row in enumerate(rows, start=2):
        xml += f'<row r="{r}">' + "".join(cell(f"{col(i)}{r}", v) for i, v in enumerate(row)) + "</row>"
    sheet1 = (f'{HDR}<worksheet xmlns="{NS}" xmlns:r="{REL}"><sheetData>{xml}</sheetData><tableParts count="1"><tablePart r:id="rId1"/></tableParts></worksheet>')
    columns = "".join(f'<tableColumn id="{i + 1}" name={quoteattr(h)}/>' for i, h in enumerate(H))
    table = (f'{HDR}<table xmlns="{NS}" id="1" name="{TABLE}" displayName="{TABLE}" ref="A1:{last}" totalsRowShown="0"><autoFilter ref="A1:{last}"/>'
             f'<tableColumns count="{len(H)}">{columns}</tableColumns><tableStyleInfo name="TableStyleMedium2" showFirstColumn="0" showLastColumn="0" showRowStripes="1" showColumnStripes="0"/></table>')

    def uniq(name):
        return sorted({r[IDX[name]] for r in rows}, key=lambda v: (str(v).lower()) if isinstance(v, str) else v)

    racks, niveles, descs, lens = uniq("Rack"), uniq("Nivel"), uniq("Descripcion"), uniq("Longitud (in)")
    tree = defaultdict(lambda: defaultdict(int))
    for r in rows:
        tree[r[IDX["Descripcion"]]][r[IDX["Longitud (in)"]]] += r[IDX["Cantidad total"]]
    grand = sum(sum(v.values()) for v in tree.values())
    layout = []
    for d in descs:
        layout.append((d, sum(tree[d].values()), True))
        for ln in sorted(tree[d]):
            layout.append((ln, tree[d][ln], False))
    top = 4
    lastrow = top + len(layout) + 1

    res = ""
    if pivot:
        res += '<row r="1">' + cell("A1", "Rack") + cell("B1", "(Todas)") + "</row>"
        res += '<row r="2">' + cell("A2", "Nivel") + cell("B2", "(Todas)") + "</row>"
        res += f'<row r="{top}">' + cell(f"A{top}", CAP_ROW, True) + cell(f"B{top}", CAP_DATA, True) + "</row>"
        rr = top
        for label, value, bold in layout:
            rr += 1
            res += f'<row r="{rr}">' + cell(f"A{rr}", label, bold) + cell(f"B{rr}", value, bold) + "</row>"
        rr += 1
        res += f'<row r="{rr}">' + cell(f"A{rr}", CAP_GRAND, True) + cell(f"B{rr}", grand, True) + "</row>"
    elif message:
        res += '<row r="1">' + cell("A1", message) + "</row>"
    sheet2 = f'{HDR}<worksheet xmlns="{NS}" xmlns:r="{REL}"><sheetData>{res}</sheetData></worksheet>'

    def cache_field(name):
        items = {"Rack": racks, "Nivel": niveles, "Descripcion": descs}.get(name)
        if items is not None:
            body = "".join(f"<s v={quoteattr(x)}/>" for x in items)
            return f'<cacheField name={quoteattr(name)} numFmtId="0"><sharedItems count="{len(items)}">{body}</sharedItems></cacheField>'
        if name == "Longitud (in)" and lens:
            body = "".join(f'<n v="{num(x)}"/>' for x in lens)
            return (f'<cacheField name={quoteattr(name)} numFmtId="0"><sharedItems containsSemiMixedTypes="0" containsString="0" containsNumber="1" '
                    f'minValue="{num(min(lens))}" maxValue="{num(max(lens))}" count="{len(lens)}">{body}</sharedItems></cacheField>')
        return f'<cacheField name={quoteattr(name)} numFmtId="0"><sharedItems/></cacheField>'

    cache = (f'{HDR}<pivotCacheDefinition xmlns="{NS}" xmlns:r="{REL}" saveData="0" refreshOnLoad="1" refreshedBy="RackCad" refreshedDate="45000" createdVersion="6" '
             'refreshedVersion="6" minRefreshableVersion="3" recordCount="0">'
             f'<cacheSource type="worksheet"><worksheetSource name="{TABLE}"/></cacheSource><cacheFields count="{len(H)}">' + "".join(cache_field(h) for h in H) + "</cacheFields></pivotCacheDefinition>")

    def items(k):
        return "".join(f'<item x="{i}"/>' for i in range(k)) + '<item t="default"/>'

    def pivot_field(name):
        if name in ("Rack", "Nivel"):
            k = len(racks) if name == "Rack" else len(niveles)
            return f'<pivotField axis="axisPage" showAll="0"><items count="{k + 1}">{items(k)}</items></pivotField>'
        if name in ("Descripcion", "Longitud (in)"):
            k = len(descs) if name == "Descripcion" else len(lens)
            return f'<pivotField axis="axisRow" showAll="0" sortType="ascending"><items count="{k + 1}">{items(k)}</items></pivotField>'
        if name == "Cantidad total":
            return '<pivotField dataField="1" showAll="0"/>'
        return '<pivotField showAll="0"/>'

    ri = ""
    for di, d in enumerate(descs):
        ri += "<i><x/></i>" if di == 0 else f'<i><x v="{di}"/></i>'
        for ln in sorted(tree[d]):
            li = lens.index(ln)
            ri += '<i r="1"><x/></i>' if li == 0 else f'<i r="1"><x v="{li}"/></i>'
    ri += '<i t="grand"><x/></i>'
    pv = (f'{HDR}<pivotTableDefinition xmlns="{NS}" name="ResumenBOM" cacheId="1" applyNumberFormats="0" applyBorderFormats="0" applyFontFormats="0" applyPatternFormats="0" '
          'applyAlignmentFormats="0" applyWidthHeightFormats="1" dataCaption="Valores" updatedVersion="6" minRefreshableVersion="3" useAutoFormatting="1" itemPrintTitles="1" '
          f'createdVersion="6" indent="0" outline="1" outlineData="1" multipleFieldFilters="0" rowHeaderCaption={quoteattr(CAP_ROW)} grandTotalCaption={quoteattr(CAP_GRAND)}>'
          f'<location ref="A{top}:B{lastrow}" firstHeaderRow="1" firstDataRow="1" firstDataCol="1" rowPageCount="2" colPageCount="1"/>'
          f'<pivotFields count="{len(H)}">' + "".join(pivot_field(h) for h in H) + "</pivotFields>"
          f'<rowFields count="2"><field x="{IDX["Descripcion"]}"/><field x="{IDX["Longitud (in)"]}"/></rowFields><rowItems count="{len(layout) + 1}">{ri}</rowItems>'
          f'<colItems count="1"><i/></colItems><pageFields count="2"><pageField fld="{IDX["Rack"]}" hier="-1"/><pageField fld="{IDX["Nivel"]}" hier="-1"/></pageFields>'
          f'<dataFields count="1"><dataField name={quoteattr(CAP_DATA)} fld="{IDX["Cantidad total"]}" baseField="0" baseItem="0"/></dataFields>'
          '<pivotTableStyleInfo name="PivotStyleLight16" showRowHeaders="1" showColHeaders="1" showRowStripes="0" showColStripes="0" showLastColumn="1"/></pivotTableDefinition>')

    wb_cache = '<pivotCaches><pivotCache cacheId="1" r:id="rId4"/></pivotCaches>' if pivot else ""
    workbook = (f'{HDR}<workbook xmlns="{NS}" xmlns:r="{REL}"><sheets><sheet name="Por rack" sheetId="1" r:id="rId1"/><sheet name="Resumen" sheetId="2" r:id="rId2"/></sheets>{wb_cache}</workbook>')
    ct = (f'{HDR}<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types"><Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>'
          f'<Default Extension="xml" ContentType="application/xml"/><Override PartName="/xl/workbook.xml" ContentType="{CT}.sheet.main+xml"/>'
          f'<Override PartName="/xl/styles.xml" ContentType="{CT}.styles+xml"/><Override PartName="/xl/worksheets/sheet1.xml" ContentType="{CT}.worksheet+xml"/>'
          f'<Override PartName="/xl/worksheets/sheet2.xml" ContentType="{CT}.worksheet+xml"/><Override PartName="/xl/tables/table1.xml" ContentType="{CT}.table+xml"/>')
    if pivot:
        ct += (f'<Override PartName="/xl/pivotCache/pivotCacheDefinition1.xml" ContentType="{CT}.pivotCacheDefinition+xml"/>'
               f'<Override PartName="/xl/pivotTables/pivotTable1.xml" ContentType="{CT}.pivotTable+xml"/>')
    ct += "</Types>"
    wb_rels = [("rId1", "worksheet", "worksheets/sheet1.xml"), ("rId2", "worksheet", "worksheets/sheet2.xml"), ("rId3", "styles", "styles.xml")]
    if pivot:
        wb_rels.append(("rId4", "pivotCacheDefinition", "pivotCache/pivotCacheDefinition1.xml"))
    parts = {"[Content_Types].xml": ct, "_rels/.rels": rels(("rId1", "officeDocument", "xl/workbook.xml")), "xl/workbook.xml": workbook,
             "xl/_rels/workbook.xml.rels": rels(*wb_rels), "xl/styles.xml": STYLES, "xl/worksheets/sheet1.xml": sheet1, "xl/worksheets/sheet2.xml": sheet2,
             "xl/worksheets/_rels/sheet1.xml.rels": rels(("rId1", "table", "../tables/table1.xml")), "xl/tables/table1.xml": table}
    if pivot:
        parts.update({"xl/worksheets/_rels/sheet2.xml.rels": rels(("rId1", "pivotTable", "../pivotTables/pivotTable1.xml")),
                      "xl/pivotCache/pivotCacheDefinition1.xml": cache, "xl/pivotTables/pivotTable1.xml": pv,
                      "xl/pivotTables/_rels/pivotTable1.xml.rels": rels(("rId1", "pivotCacheDefinition", "../pivotCache/pivotCacheDefinition1.xml"))})
    with zipfile.ZipFile(path, "w", zipfile.ZIP_DEFLATED) as z:
        for name, content in parts.items():
            z.writestr(name, content.encode("utf-8"))


out = sys.argv[1]
base = [
    ["R-A", "Selectivo", 3, "Componente", "Cabecera", "", "Cabecera 42x144", 144, 2, 6],
    ["R-A", "Selectivo", 3, "Pieza", "Poste", "C3", "Poste C3", 144, 4, 12],
    ["R-B", "Selectivo", 2, "Pieza", "Viga", "V1", "Poste C3", 96, 8, 16],
    ["R-C", "Cama", 1, "Pieza", "Rodillo", "RD", "Rodillo", 40.5, 30, 30],
]
build(os.path.join(out, "e0_header_only.xlsx"), [], pivot=False, data_rows=0, message="Sin partidas")
build(os.path.join(out, "e1_blank_row_pivot.xlsx"), [], pivot=True, data_rows=1)
build(os.path.join(out, "e2_blank_row_nopivot.xlsx"), [], pivot=False, data_rows=1, message="Sin partidas")
build(os.path.join(out, "b1_sin_nivel.xlsx"), base + [["R-D", "Cama", 2, "", "Rodillo", "RX", "", 12, 5, 10]], pivot=True)
build(os.path.join(out, "v1_valor_estatico.xlsx"), base, pivot=True)
print("OK")
