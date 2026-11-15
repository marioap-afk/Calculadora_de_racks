import json, re

ART = 'artifacts/orchestration/I-64/F4-T4C2-PANEL/1/R20261115T190629Z-9b55'
patch = open(ART + '/wip-green-impl.patch', encoding='utf-8').read()

cases = {
    'ApplyingText': 'indicador de la actualizacion en vuelo (InFlight) o del estado Applying',
    'UpdateAppliedText': 'desenlace Applied',
    'UpdateAppliedWithWarningsPrefix': 'desenlace AppliedWithWarnings, seguido de los avisos separados por «; »',
    'UpdateConflictPrefix': 'desenlace NotAppliedConflict, seguido de las diferencias',
    'UpdateNotAppliedPrefix': 'desenlaces NotAppliedAdmission, NotAppliedPreflight y NotAppliedFailure, seguido de su causa',
    'UpdateClosedInErrorPrefix': 'desenlace ClosedInError (M-03): clasificacion + motivo + causa + diferencias + avisos; nunca un exito',
    'UpdateClosedDetachedText': 'desenlace ClosedDetached',
    'UpdateAnotherInProgressText': 'desenlace AnotherUpdateInProgress',
    'UpdateUnrecognizedText': 'estado de desenlace que ningun enumerador nombra (neutro: ni fallo ni exito)',
    'UpdateNotStartedSuffix': 'sufijo de todo valor de inicio que no es Accepted y del Busy de la frontera',
    'UpdateNotStartedPrefix': 'valor de inicio con un estado que ningun enumerador nombra',
    'UpdateAbortedPrefix': 'frontera Update con decision Aborted (mensaje del campo o motivo)',
    'UpdateKeptText': 'frontera Update con decision Kept o una decision desconocida (no se pide)',
    'UpdateRefusedPrefix': 'desenlace Refused',
    'UpdateNothingToApplyPrefix': 'desenlace NothingToApply',
    'UpdatePendingTextPrefix': 'desenlace PendingText',
    'UpdateRejectedPrefix': 'desenlace Rejected y valor de inicio Rejected',
    'UpdateNoDraftPrefix': 'desenlace NoDraft',
    'UpdateNotApplicablePrefix': 'desenlace NotApplicable',
    'UpdateFailedPrefix': 'desenlace Failed y valor de inicio Failed (incluye Failed(HostNotRunReason))',
    'UpdateUnavailablePrefix': 'valor de inicio Unavailable',
    'UpdateWaitingPrefix': 'valor de inicio Waiting',
    'UpdateBusyPrefix': 'valor de inicio Busy y decision Busy de la frontera',
    'DifferencesPrefix': 'diferencias de un ClosedInError',
    'WarningsPrefix': 'avisos de un ClosedInError',
    'ListSeparator': 'separador de listas de avisos y diferencias',
    'ClassifiedAppliedName': 'clasificacion Applied de un ClosedInError',
    'ClassifiedAppliedWithWarningsName': 'clasificacion AppliedWithWarnings de un ClosedInError',
    'ClassifiedConflictName': 'clasificacion NotAppliedConflict de un ClosedInError',
    'ClassifiedAdmissionName': 'clasificacion NotAppliedAdmission de un ClosedInError',
    'ClassifiedPreflightName': 'clasificacion NotAppliedPreflight de un ClosedInError',
    'ClassifiedFailureName': 'clasificacion NotAppliedFailure de un ClosedInError',
    'ClassifiedCommitThrewName': 'clasificacion CommitThrew de un ClosedInError',
    'ClassifiedPostRereadName': 'clasificacion AppliedPostRereadFailed de un ClosedInError',
    'ClassifiedNoneName': 'clasificacion sin valor de un ClosedInError',
}

consts = []
for line in patch.splitlines():
    m = re.match(r'^\+\s+private const string (\w+) = "(.*)";', line)
    if m and m.group(1) in cases:
        consts.append('Constante nueva del VM del campo: ' + m.group(1) + ' = «' + m.group(2) + '». Caso: ' + cases[m.group(1)] + '.')
assert len(consts) == len(cases), (len(consts), len(cases))

evidence = [
    'ESTADO: BLOQUEADO por tres defectos de las pruebas del propio RED (ver UnexpectedFindings). El producto de la parte [2] esta implementado y verificado en local, pero no esta confirmado en Git: vive como parche en ' + ART + '/wip-green-impl.patch (3 archivos de produccion; el arbol queda limpio en el commit RED). No hay commit GREEN.',
    'RED: commit ' + 'a6511c0493e93edd2bb4c0503b2e74fce900a1d0' + ' (arbol a1c734e98c838651d508f2ab3e0052586df667a0), 13 de los 14 archivos del alcance (WorkspaceHostCore.cs solo cambia en el GREEN). Esqueleto inerte: ShowUpdateButton falso, Update() vacio, boton del XAML con su manejador contenido, y AdmissionDiagnostic/ClassicEditorHint de EditCheckOutcome con fabricas inertes (los argumentos se ignoran).',
    'RED en Core (filtro Workspace, ExpectRed true): 3437 seleccionadas (3421 de la base + 16 nuevas), 3427 pasan, 10 fallan por asercion; ninguna falla por una excepcion del esqueleto. RED en UI (filtro Workspace, ExpectRed true): 694 seleccionadas (601 de la base + 93 nuevas), 619 pasan, 75 fallan por asercion (todos los mensajes son Assert.*). Las 23 clases de regresion de Core y PushBackClassicCharacterization (55) pasan en el RED con sus conteos exactos (ExpectRed false).',
    'Pruebas nuevas o cambiadas que ya pasan en el RED (lista completa en ' + ART + '/tests/r1/red-pass.txt): Core 9 metodos (autopruebas de guardas sobre texto sintetico, el XAML con el boton del esqueleto, y las del diagnostico cuyo resultado ya es el correcto: lectura admitida sin diagnostico, un fallo del diagnostico que no cambia la clasificacion, la forma del valor y la lectura unica); UI 9 metodos (U01, U03, U05, U20, U25, U26, U32 y las dos de la vista y de la reconciliacion que no dependen del gesto).',
    'GREEN local (arbol de trabajo con el parche, sin confirmar): Core filtro Workspace 3437/3437 en verde; UI filtro Workspace 692 de 694 (fallan solo las dos pruebas de la vista descritas abajo). Con el parche aplicado, las guardas de las filas 12, 18 y 21 y la regla H-19 pasan, y las guardas G-1 a G-10, las de los VM sin mecanismos y las de contenido de texto siguen en verde.',
    'Mapa evidencia por invariante: INV-F4T4C-01 y 24: el gesto solo llama RequestUpdate/TakeUpdateProgress/CrossBoundary del puerto y presenta el EditUpdateOutcome por su clase (U06, U17, U18, guardas G-10 intactas). INV-F4T4C-02: orden y una sola peticion (U06, U07, U10, U11, U14, U15; guardas RequestViolations con orden boton, pendiente, frontera, peticion, recogida). INV-F4T4C-03: sin Task/async/Dispatcher/Timer/event/+= nuevos (guardas del VM sin mecanismos, que pasan). INV-F4T4C-11: boton (U01 a U05, vista), frontera H-27 (U11 a U14), Aplicando solo con InFlight o Applying (U06, U23, U24), una vez y sin reclasificar (U17 a U20), IsMine y consumo por superficie (U21, U25 a U28), valor de inicio en el gesto (U15, U16), K=1 (U08, U34). INV-F4T4C-12: diagnostico antes de CheckNow, en su try, null si falla, una lectura (pruebas del nucleo real en WorkspacePilotReconciliationHostTests; PresentCheck en WorkspacePilotReconciliationTests). INV-F4T4C-13: ClosedInError con clasificacion y nunca exito (U18, U19). INV-F4T4C-14/15/16/19: sin segunda via de escritura, sin API de Undo ni comando, sin tocar T1B ni fachadas, solo IWorkspacePilotEditPort (git diff --name-only BaseSha..RedSha lista solo archivos del alcance). INV-F4T4C-17: ver los cambios de guarda. INV-F4T4C-18: ver contencion. INV-F4T4C-20: RED con cuerpos inertes. INV-F4T4C-23: en el parche WorkspaceHostCore.cs solo cambia el cuerpo de TryCheckPilotEdit y WorkspaceEditIntent.cs solo EditCheckOutcome. INV-F4T4C-25: el doble registra RequestUpdate, TakeUpdateProgress y CrossBoundary en UpdateLog sin cambiar Calls, Operations() ni Reads() (U33).',
    'Cambios de guarda aplicados frente a D-T4C-20 (todos en el RED, cada uno con su autoprueba en negativo; ninguna fila [1] ni otra guarda cambia): FILA 12 y tabla H-19 (WorkspaceEditIntentGuardTests): CheckBody() sale del bucle de cero y pasa a exactamente un AdmissionDiagnosticPattern y dos pilot.Read, ambos dentro de esa expresion; IntentBody, KeepBody y DiscardAndReloadBody siguen en cero; la expresion pasa de una a dos apariciones en el nucleo (una en la confirmacion y una en la comprobacion), con negativas: una tercera en el nucleo, una segunda en CheckBody y una en KeepBody. FILA 18 (WorkspacePilotSurfaceGuardTests): la presentacion del campo (miembros Present, Refresh, Show, Clear y demas) puede llamar a TakeUpdateProgress y a ninguna de las seis lecturas ni a RequestUpdate; TakeUpdateProgress solo en el VM del campo y exactamente tres veces (una en Show y dos en Update), con negativas: una lectura en la presentacion, una segunda recogida, una recogida fuera del campo. FILA 21 (WorkspacePilotReconciliationGuardTests y IntentionReads de WorkspacePilotCloseSyncGuardTests): el gesto Update() declarado una vez en el VM del campo y llamado solo por su manejador OnBrowserFieldUpdateClick dentro de Contained; RequestUpdate solo en el VM del campo, una vez, como _port.RequestUpdate(request) dentro de Update y en el orden boton, pendiente, frontera, peticion, recogida; el boton BrowserFieldUpdateButton en la tabla de botones (contenido, nombre de automatizacion, manejador y visibilidad); IntentionReads incluye RequestUpdate para el VM del navegador, con negativas de un segundo llamador, del navegador, del manejador sin Contained, del grupo de metodos y del orden roto. Regla de UI de D-T4A2-06: PresentCheck guarda el diagnostico como persistente y solo retira si la respuesta no trae diagnostico (WorkspacePilotReconciliationTests).',
    'Los textos de CommitThrew y AppliedPostRereadFailed son los existentes A2-11 (UnknownCheckText, «Estado desconocido; comprueba.») y A2-12 (UnknownAppliedText, «Aplicado; recarga para seguir editando.»); no se gasta una correccion por su redaccion.',
]
evidence.extend(consts)
evidence.extend([
    'Como las trampas medidas respetan el parche: WorkspaceLifecycleGuardTests (dos _knownClean y la forma literal de los case Recovered y Conflict: se conservan, con Recovered(admission, hint) y Conflict(DescribeDifferences(...), admission, hint)); WorkspaceEditIntentGuardTests:1003-1016 (palabras prohibidas tambien en comentarios de Core y de WorkspaceEditIntent.cs: ninguna nueva); nombres de tipo de H-20 (ningun tipo nuevo); WorkspaceBrowserHostGuardTests (Content unico en la superficie y ningun Width ni Height: «Actualizar» es unico y el boton no fija tamanos); textos de WorkspacePilotReconciliationGuardTests:414-477 (no se repiten); G-10 de Task y async (0 en el VM del campo); y los VM sin mecanismos (no hay +=, -=, event, delegate, Dispatcher, Timer, Task ni async: la lista del mensaje se compone con text = text + ...).',
    'Contencion INV-F4T4C-18 (F4-E59): en local solo se corrieron los dos filtros de UI de RequiredTests (Workspace y PushBackClassicCharacterization), nunca la suite completa de UI; en Core, los filtros de Workspace y de las 23 clases en Debug. Ninguna prueba nueva escribe fuera de memoria ni usa la configuracion real, ni deja modal, dispatcher o estado persistente; no se uso ninguna sonda con entradas validas en caminos con dialogo o escritura. No se corrio la suite completa de Core en Debug ni los filtros en Release: el bloqueo se detecto antes.',
    'Builds: linea base Release de la solucion (RunRef b0-build-release-base): 0 errores, 82 avisos (MSB3277 del Plugin y xUnit de pruebas existentes). Release con el parche aplicado (g2-build-release-wip): 0 errores, 83 avisos; el unico aviso nuevo es xUnit2013 en WorkspacePilotReconciliationHostTests.cs (defecto 3 de UnexpectedFindings). No se compilo el Plugin a su carpeta bin por defecto; no se toco AutoCAD.',
])

findings = [
    'DEFECTO DE PRUEBA 1 (RED, no del producto): tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs, TheUpdateButtonIsVisibleOnlyForADirtyDraftOnAPinnedSurfaceAndNeverInTheOtherStatesNorOnAPreview, linea 757. Para el caso de la vista previa usa un borrador Dirty con isProtected true, y el navegador (WorkspaceBrowserViewModel.cs, RebuildTabs, lineas 1295-1300) fija toda pestana con trabajo protegido, por lo que la pestana nunca es una vista previa y la asercion Assert.True(IsPreview) falla antes de comprobar el boton. Correccion propuesta: en ese caso usar un Dirty con isProtected false (no protegido), que se queda como vista previa. No se puede arreglar con producto: el navegador esta fuera del alcance.',
    'DEFECTO DE PRUEBA 2 (RED, no del producto): mismo archivo, ARealClickOnTheUpdateButtonRunsTheGestureOnceInTheOrderOfThePackageAndTheButtonDisappearsWhileTheUpdateIsInFlight, linea 778. Espera exactamente cuatro entradas en el registro del doble (recogida, frontera, peticion, recogida). En la vista real el gesto termina con el aviso de cambio (_changed), el navegador reconstruye y llama a Show, que por diseno (F-1, D-T4C-03) recoge el progreso de la superficie viva: queda una quinta entrada TakeUpdateProgress. El orden del gesto y la unica peticion si son correctos. Correccion propuesta: esperar las cinco entradas (la quinta es el repintado) o comprobar una frontera y una peticion y el orden relativo de las cuatro primeras.',
    'DEFECTO DE PRUEBA 3 (RED, no del producto): tests/RackCad.Tests/Workspace/WorkspacePilotReconciliationHostTests.cs, linea 726, Assert.Equal(0, rig.Host.Reader.Reads.Count) introduce un aviso nuevo xUnit2013 (el criterio pide ningun aviso nuevo). Correccion propuesta: Assert.Empty(rig.Host.Reader.Reads).',
    'Por la regla de la delegacion (16.8: desde el commit RED no se modifica, anade ni borra nada bajo tests/, y si una prueba necesita cambio, parada y bloqueo estructurado) no se corrigieron las pruebas ni se confirmo el GREEN. Ninguna condicion C01 a C16 se activo: el id S-04 (evidencia contradictoria) es el mas cercano de la lista del contrato, porque las pruebas del RED contradicen un comportamiento medido del navegador y del repintado; no es una contradiccion del paquete.',
    'La unica pareja de archivos del alcance que sigue sin cambios en el RED es WorkspaceHostCore.cs, cuyo cambio (solo el cuerpo de TryCheckPilotEdit) esta en el parche junto con EditCheckOutcome y el VM del campo.',
]

limitations = [
    'El producto implementado en el parche no esta confirmado en Git ni lo ha corrido la CI; la CI del SHA RED debe dar Core y UI en failure (ExpectRed true), y no existe SHA GREEN.',
    'Segun el diseno, los avisos de un AppliedWithWarnings viven como aviso del gesto (NoticeText) y se pierden si la superficie cambia despues de presentarlos (D-T4C-11, O-14); PICKFIRST se vacia con EXECUTEFUNCTION (aceptado, O-15). El diagnostico persistente de T4A2 puede quedar mientras Actualizar admite (R-M3-16).',
    'Si la superficie viva es una vista previa o el campo esta en Error, el resultado no se recoge (decision conservadora): queda en el almacen hasta que la superficie fijada de su rack vuelva a pintarse.',
]

deviations = [
    'El GREEN no se confirmo ni se envio (bloqueo por defectos de las pruebas del RED, regla 16.8). Todo lo demas del RED se hizo segun el contrato.',
    'La presentacion de un resultado en la vista previa o con el campo en Error se omite sin consumirlo (decision conservadora dentro del comportamiento fijado; sin cambio de semantica).',
]

next_action = (
    'Emitir una delegacion de correccion nueva (intento 2) que parta del RED a6511c0493e93edd2bb4c0503b2e74fce900a1d0 y que permita corregir exactamente las tres pruebas del RED descritas en UnexpectedFindings (dos en WorkspacePilotFieldViewTests.cs y una en WorkspacePilotReconciliationHostTests.cs, ambos en el alcance), y despues aplicar el parche ' + ART + '/wip-green-impl.patch como GREEN (con git apply) y repetir las corridas: filtros de Workspace de Core (3437 o mas) y de UI (694 o mas), las 23 clases, PushBackClassicCharacterization, la suite completa de Core en Debug, el build Release y la CI exacta. No hay que cambiar el producto del parche para esas tres pruebas.'
)

json.dump({
    'WorkCompleted': 'RED completo y enviado (a6511c0493e93edd2bb4c0503b2e74fce900a1d0): esqueleto inerte, pruebas nuevas y cambiadas, doble de UI con registro propio y los cambios de guarda de las filas 12, 18 y 21. Producto de la parte [2] implementado en el arbol de trabajo (VM del campo: boton, gesto Update() con sus cinco pasos, presentacion de cada desenlace sin reclasificarlo, M-03, Aplicando, recogida por repintado y por superficie; PresentCheck con diagnostico persistente; EditCheckOutcome con AdmissionDiagnostic y ClassicEditorHint; TryCheckPilotEdit con el diagnostico antes de CheckNow en su propio try) y verificado en local, pero detenido antes del commit GREEN porque tres pruebas del RED son defectuosas y la regla 16.8 impide corregirlas. El parche queda en ' + ART + '/wip-green-impl.patch y el arbol de trabajo esta limpio en el commit RED.',
    'Evidence': evidence,
    'UnexpectedFindings': findings,
    'KnownLimitations': limitations,
    'Deviations': deviations,
    'RecommendedNextAction': next_action,
}, open(ART + '/evidence.json', 'w', encoding='utf-8', newline='\n'), ensure_ascii=False, indent=1)
print('ok', len(evidence))
