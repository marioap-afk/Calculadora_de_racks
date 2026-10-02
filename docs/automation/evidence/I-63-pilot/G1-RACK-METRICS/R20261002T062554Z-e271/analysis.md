I-63 / G1-RACK-METRICS — STOP S-12 analysis

Source verification:
R20261002T062554Z-e271

FailureClass:
Authority

Root cause 1 — Authority:
The correction delegation added the previous analysis.md as a UNIT_DOC
authority. That file did not exist at AuthorityRevision
658b35ad498520ba3c832a96eea4389df16dfa60.

I-61 requires unit authorities to resolve at AuthorityRevision.
Correction analyses are carried by CorrectionOf and are not authorities.

Decision:
Keep AuthorityRevision unchanged.
Copy Authorities[] byte-for-byte from the issued gate contract.
Do not include any correction analysis.md in Authorities[].

Root cause 2 — Tests:
The implementation correctly declares the six frozen MetricId pairs, but
there is no explicit bidirectional test for the six (scope, token) pairs
required by D-02.

Decision:
Add a new test file within the existing allowed test scope.
Use namespace RackCad.Tests and a ComputedParameters* class name.
Assert all six frozen MetricId pairs bidirectionally and token validity.

RED disposition:
ChainRedSha fc30dc6c is already accredited.
The new test file is not in ChainRedFiles and no protected RED file is
modified, so this correction does not require a new RED.

No Freeze, A-1, contract, scope, AuthorityRevision, materiality or Owner
decision changes.

attempts increments from 1 to 2 before correction delegation.
