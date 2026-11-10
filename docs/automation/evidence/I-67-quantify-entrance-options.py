"""Cuantifica las dos opciones de entrada para I-67 con la base real del catalogo (solo aritmetica)."""
import math

B = 0.6053
P = 2.0
CLEAR = 6.0
BEAM = 6.0
FOOT = 12.0
ALLOW = 6.0  # HeaderEndAllowance se imprime aparte y se comprueba
PALLET_DEPTH = 48.0


def away(x):
    return math.copysign(math.floor(abs(x) + 0.5), x)


def near(v):
    return B + away((v - B) / P) * P


def up(v):
    return B + math.ceil((v - B) / P - 1e-9) * P


def slope(deep):
    d = deep * PALLET_DEPTH + 2 * ALLOW
    return (d / 12.0) * (7.0 / 16.0)


def header(top_ent, pallet):
    v = top_ent + BEAM + (pallet + CLEAR) / 3.0
    return math.ceil(v / FOOT) * FOOT


def old(first, req, n, s):
    ex, en = [], []
    raw = first
    for _ in range(n):
        ex.append(near(raw))
        en.append(near(raw + s))
        raw += req
    return ex, en


def new_exits(first, req, n):
    ex = [near(first)]
    for _ in range(n - 1):
        ex.append(up(ex[-1] + max(P, req)))
    return ex


def e1(ex, s):
    return [near(x + s) for x in ex]


def e3(ex, first, s):
    rise = near(first + s) - ex[0]
    return [x + rise for x in ex]


rows = {"even": [0, 0, 0, 0, 0], "odd": [0, 0, 0, 0, 0]}
rise_stats = {"E1": [], "E3": [], "old": []}
for first in [6.0, 4.0, 8.0, 12.0, 7.5]:
    for pallet in [x * 1.0 for x in range(30, 73)]:
        for n in range(2, 7):
            for deep in range(2, 9):
                s = slope(deep)
                req = BEAM + pallet + CLEAR
                kind = "even" if abs(req % 2.0) < 1e-9 else "odd"
                ox, oe = old(first, req, n, s)
                nx = new_exits(first, req, n)
                a = e1(nx, s)
                c = e3(nx, first, s)
                oh = header(oe[-1], pallet)
                rows[kind][0] += 1
                rows[kind][1] += 1 if abs(header(a[-1], pallet) - oh) > 1e-6 else 0
                rows[kind][2] += 1 if abs(header(c[-1], pallet) - oh) > 1e-6 else 0
                rows[kind][3] += 1 if abs(a[0] - oe[0]) > 1e-6 else 0
                rows[kind][4] += 1 if abs(c[0] - oe[0]) > 1e-6 else 0
                if first == 6.0:
                    rise_stats["E1"].append(abs((a[0] - nx[0]) - s))
                    rise_stats["E3"].append(abs((c[0] - nx[0]) - s))
                    rise_stats["old"].append(sum(abs((oe[i] - ox[i]) - s) for i in range(n)) / n)

print("clase  casos  cabecera_cambia(B-6/E1)  cabecera_cambia(E3)  entrada_N1_cambia(E1)  entrada_N1_cambia(E3)")
for k, v in rows.items():
    print(k, v[0], "%d (%.1f%%)" % (v[1], 100 * v[1] / v[0]), "%d (%.1f%%)" % (v[2], 100 * v[2] / v[0]),
          "%d (%.1f%%)" % (v[3], 100 * v[3] / v[0]), "%d (%.1f%%)" % (v[4], 100 * v[4] / v[0]))

for k, v in rise_stats.items():
    print("error medio de subida frente a la nominal", k, "%.3f pulg, max %.3f" % (sum(v) / len(v), max(v)))

print("\nCaso H58 (6 pulg, tarima 33, 4 niveles, 4 fondos):")
s = slope(4)
ox, oe = old(6.0, 45.0, 4, s)
nx = new_exits(6.0, 45.0, 4)
print("S nominal", s)
print("salidas antes", [round(x, 4) for x in ox], "despues", [round(x, 4) for x in nx])
print("entradas antes", [round(x, 4) for x in oe], "subidas", [round(oe[i] - ox[i], 3) for i in range(4)])
a = e1(nx, s)
c = e3(nx, 6.0, s)
print("E1 entradas", [round(x, 4) for x in a], "subidas", [round(a[i] - nx[i], 3) for i in range(4)], "cabecera", header(a[-1], 33.0))
print("E3 entradas", [round(x, 4) for x in c], "subidas", [round(c[i] - nx[i], 3) for i in range(4)], "cabecera", header(c[-1], 33.0))
print("cabecera antes", header(oe[-1], 33.0))

print("\nCaso sin defecto (12 pulg, tarima 60, 2 niveles, 4 fondos, req 72):")
ox, oe = old(12.0, 72.0, 2, s)
nx = new_exits(12.0, 72.0, 2)
a = e1(nx, s)
c = e3(nx, 12.0, s)
print("salidas", [round(x, 4) for x in ox], [round(x, 4) for x in nx])
print("entradas antes", [round(x, 4) for x in oe], "E1", [round(x, 4) for x in a], "E3", [round(x, 4) for x in c])
print("cabecera antes", header(oe[-1], 60.0), "E1", header(a[-1], 60.0), "E3", header(c[-1], 60.0))
