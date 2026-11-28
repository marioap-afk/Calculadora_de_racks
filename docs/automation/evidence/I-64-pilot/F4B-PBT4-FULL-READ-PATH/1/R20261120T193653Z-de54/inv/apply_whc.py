"""Applies the inline statement of D-PBT4-03 to the five rereads of WorkspaceHostCore.cs (the first sentence, the one of Begin, stays)."""
import sys

path = sys.argv[1]
with open(path, "rb") as handle:
    raw = handle.read().decode("utf-8")
OLD = "var read = _editIntentReader!.ReadIntent(request.Session, request.RackId, request.ChosenReference, request.Witness);"
indent = " " * 16
NEW = (
    "var read = _editIntentReader is IWorkspaceEditIntentOriginReader originReader\r\n"
    + indent + "    ? originReader.ReadIntentAtOrigin(request.Session, request.RackId, draft.CurrentDraftState.Origin, request.Witness)\r\n"
    + indent + "    : _editIntentReader!.ReadIntent(request.Session, request.RackId, request.ChosenReference, request.Witness);"
)
first = raw.index(OLD)
head, tail = raw[:first + len(OLD)], raw[first + len(OLD):]
count = tail.count(OLD)
assert count == 5, count
with open(path, "wb") as handle:
    handle.write((head + tail.replace(OLD, NEW)).encode("utf-8"))
print("replaced", count)
