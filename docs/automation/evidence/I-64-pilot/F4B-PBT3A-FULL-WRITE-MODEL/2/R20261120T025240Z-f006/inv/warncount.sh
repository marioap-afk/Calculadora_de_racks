#!/bin/bash
# Uso: warncount.sh <log>... : imprime por codigo el numero de avisos unicos (linea completa sin duplicados)
for f in "$@"; do
  echo "== $f"
  grep -E ': warning [A-Za-z]+[0-9]+' "$f" | sort -u | sed -E 's/.*: warning ([A-Za-z]+[0-9]+).*/\1/' | sort | uniq -c
  echo "total-unicos: $(grep -E ': warning [A-Za-z]+[0-9]+' "$f" | sort -u | wc -l)"
done
