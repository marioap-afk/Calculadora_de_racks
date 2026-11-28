### corrida 1

TRX base sha256 38607095137AF62B808EEF339592FA8D87FD843021D4756A0C7B4E711CB6633B; TRX GREEN sha256 2365CE87AAC7470D35D2E09FEC4ADAF03874D84C50CA8C10168E61D931D7EAC1

- copia=5aaacce3 (BASE)
- entorno framework=.NET 8.0.29 config=Release cpus=8 servergc=False hires=True
- copia=faef7020 (GREEN)
- entorno framework=.NET 8.0.29 config=Release cpus=8 servergc=False hires=True

| Escenario | Caso | Base 5aaacce3, mediana / p95 (ms) | GREEN faef7020, mediana / p95 (ms) | Delta mediana (ms) | Factor (otra copia) | Base emulada en la copia GREEN (ms) | Factor (mismo proceso) |
|---|---|---|---|---|---|---|---|
| pb-01-base x2 vistas | Begin | 3.27 / 5.48 | 38.52 / 51.14 | +35.25 | 11.77x | 3.50 | 11.00x |
| pb-01-base x2 vistas | relectura, origen encontrado | 3.27 / 5.48 | 97.80 / 155.31 | +94.53 | 29.87x | 3.50 | 27.93x |
| pb-01-base x2 vistas | relectura, cae al Primary | 3.27 / 5.48 | 52.18 / 73.42 | +48.91 | 15.94x | 3.50 | 14.90x |
| pb-01-base x2 vistas | relectura, el origen es el Primary | 3.27 / 5.48 | 51.88 / 75.60 | +48.61 | 15.85x | 3.50 | 14.82x |
| pb-07-combinado x2 vistas | Begin | 4.46 / 6.70 | 60.04 / 93.69 | +55.58 | 13.46x | 4.98 | 12.05x |
| pb-07-combinado x2 vistas | relectura, origen encontrado | 4.46 / 6.70 | 109.39 / 158.08 | +104.93 | 24.53x | 4.98 | 21.96x |
| pb-07-combinado x2 vistas | relectura, cae al Primary | 4.46 / 6.70 | 59.83 / 89.83 | +55.37 | 13.41x | 4.98 | 12.01x |
| pb-07-combinado x2 vistas | relectura, el origen es el Primary | 4.46 / 6.70 | 53.87 / 67.29 | +49.41 | 12.08x | 4.98 | 10.82x |
| pb-21-cola x2 vistas | Begin | 7.17 / 9.69 | 9.88 / 15.27 | +2.71 | 1.38x | 5.33 | 1.85x |
| pb-21-cola x2 vistas | relectura, origen encontrado | 7.17 / 9.69 | 19.96 / 33.31 | +12.79 | 2.78x | 5.33 | 3.75x |
| pb-21-cola x2 vistas | relectura, cae al Primary | 7.17 / 9.69 | 10.95 / 20.06 | +3.78 | 1.53x | 5.33 | 2.06x |
| pb-21-cola x2 vistas | relectura, el origen es el Primary | 7.17 / 9.69 | 14.06 / 20.96 | +6.90 | 1.96x | 5.33 | 2.64x |
| pb-20-ab x2 vistas | Begin | 5.53 / 8.56 | 69.40 / 127.30 | +63.87 | 12.55x | 5.97 | 11.62x |
| pb-20-ab x2 vistas | relectura, origen encontrado | 5.53 / 8.56 | 157.13 / 217.42 | +151.60 | 28.40x | 5.97 | 26.31x |
| pb-20-ab x2 vistas | relectura, cae al Primary | 5.53 / 8.56 | 70.50 / 96.94 | +64.96 | 12.74x | 5.97 | 11.80x |
| pb-20-ab x2 vistas | relectura, el origen es el Primary | 5.53 / 8.56 | 60.22 / 95.31 | +54.68 | 10.88x | 5.97 | 10.08x |
| pb-07-combinado x4 vistas +200 racks ajenos | Begin | 6.35 / 9.91 | 71.66 / 103.34 | +65.31 | 11.28x | 6.36 | 11.27x |
| pb-07-combinado x4 vistas +200 racks ajenos | relectura, origen encontrado | 6.35 / 9.91 | 132.79 / 187.25 | +126.43 | 20.90x | 6.36 | 20.88x |
| pb-07-combinado x4 vistas +200 racks ajenos | relectura, cae al Primary | 6.35 / 9.91 | 64.84 / 84.72 | +58.49 | 10.20x | 6.36 | 10.20x |
| pb-07-combinado x4 vistas +200 racks ajenos | relectura, el origen es el Primary | 6.35 / 9.91 | 66.37 / 100.99 | +60.02 | 10.45x | 6.36 | 10.44x |
| pb-20-ab x4 vistas +200 racks ajenos | Begin | 8.59 / 13.48 | 63.78 / 92.33 | +55.20 | 7.43x | 10.84 | 5.88x |
| pb-20-ab x4 vistas +200 racks ajenos | relectura, origen encontrado | 8.59 / 13.48 | 138.48 / 187.73 | +129.90 | 16.13x | 10.84 | 12.78x |
| pb-20-ab x4 vistas +200 racks ajenos | relectura, cae al Primary | 8.59 / 13.48 | 68.00 / 93.44 | +59.41 | 7.92x | 10.84 | 6.27x |
| pb-20-ab x4 vistas +200 racks ajenos | relectura, el origen es el Primary | 8.59 / 13.48 | 83.36 / 122.17 | +74.77 | 9.71x | 10.84 | 7.69x |

| Escenario | Parte | Base (ms, mediana) | GREEN (ms, mediana) |
|---|---|---|---|
| pb-01-base x2 vistas | solo el escaneo (ctor) | 0.000 | 0.000 |
| pb-01-base x2 vistas | solo BuildStaleBase | 0.380 | 0.426 |
| pb-01-base x2 vistas | solo el piloto | 4.417 | 3.825 |
| pb-01-base x2 vistas | solo el completo | - | 47.996 |
| pb-07-combinado x2 vistas | solo el escaneo (ctor) | 0.001 | 0.000 |
| pb-07-combinado x2 vistas | solo BuildStaleBase | 0.539 | 0.485 |
| pb-07-combinado x2 vistas | solo el piloto | 4.686 | 4.015 |
| pb-07-combinado x2 vistas | solo el completo | - | 48.366 |
| pb-21-cola x2 vistas | solo el escaneo (ctor) | 0.001 | 0.001 |
| pb-21-cola x2 vistas | solo BuildStaleBase | 0.717 | 0.935 |
| pb-21-cola x2 vistas | solo el piloto | 5.237 | 5.190 |
| pb-21-cola x2 vistas | solo el completo | - | 5.004 |
| pb-20-ab x2 vistas | solo el escaneo (ctor) | 0.000 | 0.001 |
| pb-20-ab x2 vistas | solo BuildStaleBase | 0.753 | 0.794 |
| pb-20-ab x2 vistas | solo el piloto | 4.883 | 4.572 |
| pb-20-ab x2 vistas | solo el completo | - | 62.367 |
| pb-07-combinado x4 vistas +200 racks ajenos | solo el escaneo (ctor) | 0.032 | 0.019 |
| pb-07-combinado x4 vistas +200 racks ajenos | solo BuildStaleBase | 1.067 | 1.115 |
| pb-07-combinado x4 vistas +200 racks ajenos | solo el piloto | 7.490 | 9.123 |
| pb-07-combinado x4 vistas +200 racks ajenos | solo el completo | - | 48.568 |
| pb-20-ab x4 vistas +200 racks ajenos | solo el escaneo (ctor) | 0.017 | 0.016 |
| pb-20-ab x4 vistas +200 racks ajenos | solo BuildStaleBase | 1.448 | 1.414 |
| pb-20-ab x4 vistas +200 racks ajenos | solo el piloto | 11.260 | 9.136 |
| pb-20-ab x4 vistas +200 racks ajenos | solo el completo | - | 55.976 |

### corrida 2

TRX base sha256 744E4E560EBF627281633D8A8A37F6CE5272E3451DA2EDB70507A2598EDD0CC1; TRX GREEN sha256 5D5453D35C89749631FA6DE529F93FE394715D3141861D25C4A46ED9CC087B92

- copia=5aaacce3 (BASE)
- entorno framework=.NET 8.0.29 config=Release cpus=8 servergc=False hires=True
- copia=faef7020 (GREEN)
- entorno framework=.NET 8.0.29 config=Release cpus=8 servergc=False hires=True

| Escenario | Caso | Base 5aaacce3, mediana / p95 (ms) | GREEN faef7020, mediana / p95 (ms) | Delta mediana (ms) | Factor (otra copia) | Base emulada en la copia GREEN (ms) | Factor (mismo proceso) |
|---|---|---|---|---|---|---|---|
| pb-01-base x2 vistas | Begin | 3.24 / 5.51 | 34.74 / 55.88 | +31.50 | 10.73x | 2.45 | 14.17x |
| pb-01-base x2 vistas | relectura, origen encontrado | 3.24 / 5.51 | 60.46 / 78.59 | +57.22 | 18.67x | 2.45 | 24.67x |
| pb-01-base x2 vistas | relectura, cae al Primary | 3.24 / 5.51 | 31.47 / 41.87 | +28.23 | 9.72x | 2.45 | 12.84x |
| pb-01-base x2 vistas | relectura, el origen es el Primary | 3.24 / 5.51 | 30.86 / 50.15 | +27.63 | 9.53x | 2.45 | 12.59x |
| pb-07-combinado x2 vistas | Begin | 2.71 / 3.92 | 43.17 / 57.15 | +40.46 | 15.94x | 2.67 | 16.19x |
| pb-07-combinado x2 vistas | relectura, origen encontrado | 2.71 / 3.92 | 82.91 / 110.97 | +80.20 | 30.60x | 2.67 | 31.09x |
| pb-07-combinado x2 vistas | relectura, cae al Primary | 2.71 / 3.92 | 40.60 / 51.81 | +37.89 | 14.99x | 2.67 | 15.22x |
| pb-07-combinado x2 vistas | relectura, el origen es el Primary | 2.71 / 3.92 | 38.07 / 44.83 | +35.36 | 14.05x | 2.67 | 14.27x |
| pb-21-cola x2 vistas | Begin | 3.41 / 5.25 | 13.28 / 23.36 | +9.86 | 3.89x | 4.70 | 2.83x |
| pb-21-cola x2 vistas | relectura, origen encontrado | 3.41 / 5.25 | 17.87 / 44.29 | +14.45 | 5.24x | 4.70 | 3.81x |
| pb-21-cola x2 vistas | relectura, cae al Primary | 3.41 / 5.25 | 8.74 / 15.28 | +5.33 | 2.56x | 4.70 | 1.86x |
| pb-21-cola x2 vistas | relectura, el origen es el Primary | 3.41 / 5.25 | 11.59 / 19.52 | +8.18 | 3.40x | 4.70 | 2.47x |
| pb-20-ab x2 vistas | Begin | 4.12 / 7.28 | 51.48 / 68.53 | +47.35 | 12.48x | 4.71 | 10.93x |
| pb-20-ab x2 vistas | relectura, origen encontrado | 4.12 / 7.28 | 108.03 / 155.16 | +103.90 | 26.19x | 4.71 | 22.94x |
| pb-20-ab x2 vistas | relectura, cae al Primary | 4.12 / 7.28 | 53.32 / 73.62 | +49.20 | 12.93x | 4.71 | 11.32x |
| pb-20-ab x2 vistas | relectura, el origen es el Primary | 4.12 / 7.28 | 55.84 / 74.03 | +51.72 | 13.54x | 4.71 | 11.86x |
| pb-07-combinado x4 vistas +200 racks ajenos | Begin | 5.45 / 8.54 | 49.01 / 67.33 | +43.56 | 8.99x | 5.65 | 8.67x |
| pb-07-combinado x4 vistas +200 racks ajenos | relectura, origen encontrado | 5.45 / 8.54 | 123.92 / 160.99 | +118.47 | 22.73x | 5.65 | 21.93x |
| pb-07-combinado x4 vistas +200 racks ajenos | relectura, cae al Primary | 5.45 / 8.54 | 70.89 / 94.55 | +65.44 | 13.00x | 5.65 | 12.54x |
| pb-07-combinado x4 vistas +200 racks ajenos | relectura, el origen es el Primary | 5.45 / 8.54 | 70.71 / 105.61 | +65.26 | 12.97x | 5.65 | 12.51x |
| pb-20-ab x4 vistas +200 racks ajenos | Begin | 7.23 / 10.42 | 87.39 / 114.47 | +80.15 | 12.08x | 10.92 | 8.00x |
| pb-20-ab x4 vistas +200 racks ajenos | relectura, origen encontrado | 7.23 / 10.42 | 181.98 / 257.23 | +174.75 | 25.16x | 10.92 | 16.67x |
| pb-20-ab x4 vistas +200 racks ajenos | relectura, cae al Primary | 7.23 / 10.42 | 88.36 / 118.08 | +81.13 | 12.22x | 10.92 | 8.09x |
| pb-20-ab x4 vistas +200 racks ajenos | relectura, el origen es el Primary | 7.23 / 10.42 | 93.45 / 142.37 | +86.21 | 12.92x | 10.92 | 8.56x |

| Escenario | Parte | Base (ms, mediana) | GREEN (ms, mediana) |
|---|---|---|---|
| pb-01-base x2 vistas | solo el escaneo (ctor) | 0.000 | 0.000 |
| pb-01-base x2 vistas | solo BuildStaleBase | 0.338 | 0.334 |
| pb-01-base x2 vistas | solo el piloto | 2.144 | 1.985 |
| pb-01-base x2 vistas | solo el completo | - | 25.408 |
| pb-07-combinado x2 vistas | solo el escaneo (ctor) | 0.000 | 0.000 |
| pb-07-combinado x2 vistas | solo BuildStaleBase | 0.384 | 0.387 |
| pb-07-combinado x2 vistas | solo el piloto | 2.864 | 2.689 |
| pb-07-combinado x2 vistas | solo el completo | - | 34.313 |
| pb-21-cola x2 vistas | solo el escaneo (ctor) | 0.000 | 0.000 |
| pb-21-cola x2 vistas | solo BuildStaleBase | 0.480 | 0.649 |
| pb-21-cola x2 vistas | solo el piloto | 3.830 | 5.824 |
| pb-21-cola x2 vistas | solo el completo | - | 5.415 |
| pb-20-ab x2 vistas | solo el escaneo (ctor) | 0.000 | 0.000 |
| pb-20-ab x2 vistas | solo BuildStaleBase | 0.571 | 0.692 |
| pb-20-ab x2 vistas | solo el piloto | 3.479 | 4.462 |
| pb-20-ab x2 vistas | solo el completo | - | 47.490 |
| pb-07-combinado x4 vistas +200 racks ajenos | solo el escaneo (ctor) | 0.014 | 0.021 |
| pb-07-combinado x4 vistas +200 racks ajenos | solo BuildStaleBase | 0.798 | 1.276 |
| pb-07-combinado x4 vistas +200 racks ajenos | solo el piloto | 6.520 | 8.175 |
| pb-07-combinado x4 vistas +200 racks ajenos | solo el completo | - | 58.407 |
| pb-20-ab x4 vistas +200 racks ajenos | solo el escaneo (ctor) | 0.016 | 0.020 |
| pb-20-ab x4 vistas +200 racks ajenos | solo BuildStaleBase | 1.363 | 1.757 |
| pb-20-ab x4 vistas +200 racks ajenos | solo el piloto | 9.556 | 10.583 |
| pb-20-ab x4 vistas +200 racks ajenos | solo el completo | - | 78.178 |

