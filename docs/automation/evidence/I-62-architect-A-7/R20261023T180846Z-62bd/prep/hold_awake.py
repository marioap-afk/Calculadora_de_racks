# Holds a per-process power request (SetThreadExecutionState ES_CONTINUOUS | ES_SYSTEM_REQUIRED: idle sleep only, no settings changed) while the
# kit chain runs; releases it when chain.log says DONE or STOP, or after the hour limit. Usage: python -I -B hold_awake.py <chain.log> <hours>
import ctypes
import sys
import time

LOG, HOURS = sys.argv[1], float(sys.argv[2])
ES_CONTINUOUS, ES_SYSTEM_REQUIRED, ES_DISPLAY_REQUIRED = 0x80000000, 0x00000001, 0x00000002
FLAGS = ES_CONTINUOUS | ES_SYSTEM_REQUIRED | (ES_DISPLAY_REQUIRED if 'display' in sys.argv[3:] else 0)  # modern standby: display-off = standby
k = ctypes.windll.kernel32
assert k.SetThreadExecutionState(FLAGS) != 0
t0 = time.time()
reason = "limit"
try:
    while time.time() - t0 < HOURS * 3600:
        try:
            t = open(LOG, encoding="utf-8").read()
        except OSError:
            t = ""
        if "DONE" in t or "STOP" in t:
            reason = "chain ended"
            break
        time.sleep(30)
finally:
    k.SetThreadExecutionState(ES_CONTINUOUS)
print("released:", reason, round((time.time() - t0) / 60), "min")
