import sys
for p in sys.argv[1:]:
    x = open(p, "rb").read()
    i = x.find("es una barra invertida".encode())
    print(p, x[i - 12:i + 2], "CR:", x.count(b"\r"), "double-backslash:", x.count(bytes([92, 92])), "single:", x.count(bytes([92])),
          "final LF:", x.endswith(b"\n"), "nonascii-escape u:", x.count(bytes([92]) + b"u"))
