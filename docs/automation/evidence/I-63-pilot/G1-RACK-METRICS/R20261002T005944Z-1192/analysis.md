I-63 / G1-RACK-METRICS — STOP S-04 analysis

Source verification:
R20261002T005944Z-1192

Classification:
STOP / S-04 / contradictory evidence

Root cause:
The planning prompt introduced a requirement that G1 test files use
namespace RackCad.Tests.ComputedParameters. That requirement is absent
from the Freeze, A-1 and gate contract and conflicts with the integrated
I-23 NamespaceFolderGuardTests authority.

The gate contract only requires:
FullyQualifiedName~RackCad.Tests.ComputedParameters

That filter is satisfied by tests in namespace RackCad.Tests whose class
names begin with ComputedParameters.

Decision:
Correction authorized under CorrectionsAuthorized=true.

Required correction:
- keep namespace RackCad.Tests;
- use ComputedParameters* class names;
- do not modify the I-23 namespace guard;
- do not modify the gate contract/filter;
- remove forbidden gate terminology from Worker free-text fields.

Because protected RED test files change, a new test-only RED is required
before GREEN.

attempts increments from 0 to 1 before correction delegation.

Chain authority remains:
Freeze f61d0aca859a11b15cbe1797069a83cba873ba95
A-1 658b35ad498520ba3c832a96eea4389df16dfa60

No scope, materiality, Freeze, A-1 or Owner decision change.

RackMetricRequest clarification:
If no sibling has an identifiable RackId, the request violates D-17's
input precondition ("siblings of ONE RackId") and must reject the request
with ArgumentException rather than fabricate
Unavailable(EnvelopeUnreadable).
