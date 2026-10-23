# Decisiones §95, punto 6: el auditor congelado v6.0-a7r2 sobre la corrida completa, con la orden de «Lanzamiento» del README del kit (desde la
# carpeta que contiene kit/); salida en audit.json y output.json de esa carpeta y registro en prep/p6/post-review.log.
K=/d/r62-a7/kit-review2
cd $K && python -B kit/post-review.py "$USERPROFILE\\.claude\\projects\\D--r62-arch-a7r2\\3dd5ac31-ddf5-4e4a-b732-b913933a9ae3.jsonl" 'D:\r62-arch-a7r2-run\launch\stdout.jsonl' 'D:\r62-arch-a7r2' 'D:\r62-arch-a7r2-run' kit kit/result.schema.json audit.json output.json > prep/p6/post-review.log 2>&1
echo "post-review rc=$?" >> prep/p6/post-review.log
