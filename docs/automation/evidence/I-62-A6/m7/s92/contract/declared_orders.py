# declared_orders.py - the minimal mechanical check proposed for the next preparation of the FX-02 kit (I-62, decisiones §92,
# punto 4): once the Coordinator's disposition names the declared command with a closed set of instrument orders (A6-1, rules 1
# and 8; A6-7, rules 3, 5 and 7), an execution of the instrument whose Argv[0] is outside that set does not have "exactly the
# form of the declared command" (A6-3, rule 6 (a)): the log is not reconstructible, no request is built and the output is not
# accepted (A6-5, rule 4). Pure function over the result of the reconstruction rule; no I/O, no process. Stdlib only.
#
# This file is a specification by example for the kit; it is NOT part of any custodied kit and changes nothing in it.

DECLARED_ORDERS_S92 = ('identity', 'diff', 'scope', 'hashes', 'cleantree', 'read-text', 'read-json', 'trx', 'handoff',
                       'freetext', 'clausemap')        # the 12 orders of the instrument minus `contract`


def check(reconstruction, declared=DECLARED_ORDERS_S92):
    """reconstruction: the object returned by reconstruct_runs_m7.reconstruct (or its CLI JSON). Returns
    {'Ok', 'Outside': [{'Seq', 'ItemId', 'Order'}], 'Reconstructible': the input flag AND Ok}. A run without Argv (a run that the
    rule already rejected) keeps the rule's own verdict."""
    outside = []
    for r in reconstruction.get('Runs') or []:
        argv = r.get('Argv')
        if isinstance(argv, list) and argv and argv[0] not in declared:
            outside.append({'Seq': r.get('Seq'), 'ItemId': r.get('ItemId'), 'Order': argv[0]})
    ok = not outside
    return {'Ok': ok, 'Outside': outside, 'Declared': list(declared),
            'Reconstructible': bool(reconstruction.get('Reconstructible')) and ok}
