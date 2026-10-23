# patch_tools_r3b.py - second step after patch_tools_r3.py (already applied): the coherence of the rebase record also requires that
# MainAfterSha is a commit after MainBeforeSha from which Target.commit descends (A7-1 round 3, point 2 (c), after the preflight found that a
# substituted record with MainAfterSha = MainBeforeSha passed: no advance of main, and MainSha_eval would be K.MainSha). Applied once on
# 2026-10-23 to the round-3 builder and validator; kept to document the derivation (running it again fails on its anchors).
def patch(p, reps):
    t = open(p, encoding='utf-8', newline='').read()
    for a, b in reps:
        assert t.count(a) == 1, (p, a[:80])
        t = t.replace(a, b)
    open(p, 'w', encoding='utf-8', newline='').write(t)
    print(p, 'ok')


patch('make_eic_a7.py', [("""    mse = m.get('MainAfterSha')
    if not MEV.is_commit(repo, mse):
        return None, 'rule 2 (c): MainAfterSha of the RebaseMap is not a commit of the -C'
    return mse, None""",
"""    mse = m.get('MainAfterSha')
    if not MEV.is_commit(repo, mse):
        return None, 'rule 2 (c): MainAfterSha of the RebaseMap is not a commit of the -C'
    anc = lambda a, b: MEV.git(repo, 'merge-base', '--is-ancestor', a, b)[0] == 0
    if mse == m.get('MainBeforeSha') or not anc(m['MainBeforeSha'], mse) or not anc(mse, T.get('Commit')):
        return None, ('rule 2 (c): MainAfterSha is not a commit after MainBeforeSha from which Target.commit descends (a rebase onto another '
                      'main, or no advance of main)')
    return mse, None"""),
    ("""# OriginalSha is the CurrentSha of the delivery, with ImageSha = Target.commit and PatchIdEqual true. MainSha_eval of the reverification = MainAfterSha.""",
     """# OriginalSha is the CurrentSha of the delivery, with ImageSha = Target.commit and PatchIdEqual true. MainSha_eval of the reverification = MainAfterSha,
# a commit after MainBeforeSha from which Target.commit descends.""")])
patch('check_a7_closure.py', [("""    if not repo.is_commit(mp.get('MainAfterSha')):
        out.append('MainAfterSha is not a commit')
    return out""",
"""    ma = mp.get('MainAfterSha')
    if not repo.is_commit(ma):
        out.append('MainAfterSha is not a commit')
    else:
        anc = lambda a, b: git(repo.path, 'merge-base', '--is-ancestor', a, b)[0] == 0
        if K is None or ma == K.get('MainSha') or not anc(K.get('MainSha'), ma) or not anc(ma, (inv.get('Target') or {}).get('Commit')):
            out.append('MainAfterSha is not after MainBeforeSha or Target.commit does not descend from it')
    return out"""),
    ("""# OriginalSha is the CurrentSha of the delivery, with ImageSha = Target.commit and PatchIdEqual true. MainSha_eval of the reverification = MainAfterSha.""",
     """# OriginalSha is the CurrentSha of the delivery, with ImageSha = Target.commit and PatchIdEqual true. MainSha_eval of the reverification = MainAfterSha,
# a commit after MainBeforeSha from which Target.commit descends.""")])
