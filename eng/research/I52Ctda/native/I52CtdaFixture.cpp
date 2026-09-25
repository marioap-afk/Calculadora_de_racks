#include "I52CtdaFixture.h"

#include "dbents.h"
#include "dbobjptr.h"
#include "dbxrecrd.h"
#include "geassign.h"
#include "actrans.h"
#include "acutads.h"
#include "dbapserv.h"
#include "dbdict.h"
#include "dbsymtb.h"
#include "dbtrans.h"

#include <cwchar>
#include <sstream>

namespace
{
// Normative values come from NPM-V34 section 2 (STATE-S-0, STATE-M-0, STATE-SM-0/1). The SM-LINK carrier and
// the F-REF-C lifecycle follow the V34 binding clarifications of decisions section 164. Names without a V34 value
// (block, NOD keys, trigger entities, positions) are materialization bindings only.
constexpr const ACHAR* kLayerA = L"RACKCAD_CTDA_V30_A";
constexpr const ACHAR* kLayerB = L"RACKCAD_CTDA_V30_B";
constexpr const ACHAR* kReferenceBlock = L"RACKCAD_CTDA_V34_REF";
constexpr const ACHAR* kSemanticKey = L"RACKCAD_CTDA_V34_F-XR";
constexpr const ACHAR* kStateS0 = L"HFV30:S:0";
constexpr const ACHAR* kLinkKey = L"RACKCAD_CTDA_V34_SM-LINK";
constexpr const ACHAR* kLinkPrefix = L"HFV30:LINK:";
constexpr const ACHAR* kLinkSm0 = L"HFV30:LINK:A,B";
constexpr const ACHAR* kLinkSm1 = L"HFV30:LINK:A,C";
const AcGePoint3d kStateM0Displacement(10.0, 20.0, 0.0);
const AcGePoint3d kSiblingAPosition(0.0, 0.0, 0.0);
const AcGePoint3d kSiblingCCreationPosition(20.0, 20.0, 0.0);

Acad::ErrorStatus addLayer(AcDbLayerTable* table, AcTransactionManager* manager, const ACHAR* name, AcDbObjectId& id)
{
    if (table->has(name)) return Acad::eDuplicateRecordName;
    auto* record = new AcDbLayerTableRecord();
    Acad::ErrorStatus status = record->setName(name);
    if (status == Acad::eOk) status = table->add(id, record);
    if (status != Acad::eOk) { delete record; return status; }
    return manager->addNewlyCreatedDBRObject(record);
}

Acad::ErrorStatus appendEntity(AcDbBlockTableRecord* owner, AcDbTransactionManager* manager, AcDbEntity* entity, AcDbObjectId& id)
{
    Acad::ErrorStatus status = owner->appendAcDbEntity(id, entity);
    if (status != Acad::eOk) { delete entity; return status; }
    return manager->addNewlyCreatedDBRObject(entity);
}

Acad::ErrorStatus appendTrigger(AcDbDatabase* database, AcDbBlockTableRecord* modelSpace, AcTransactionManager* manager, double x, AcDbObjectId& id)
{
    auto* point = new AcDbPoint(AcGePoint3d(x, 0.0, 0.0));
    point->setDatabaseDefaults(database);
    Acad::ErrorStatus status = point->setLayer(L"0");
    if (status != Acad::eOk) { delete point; return status; }
    return appendEntity(modelSpace, manager, point, id);
}

Acad::ErrorStatus appendReference(AcDbDatabase* database, AcDbBlockTableRecord* modelSpace, AcDbTransactionManager* manager, const AcGePoint3d& position, AcDbObjectId block, AcDbObjectId layer, AcDbObjectId& id)
{
    auto* reference = new AcDbBlockReference(position, block);
    reference->setDatabaseDefaults(database);
    Acad::ErrorStatus status = reference->setLayer(layer);
    if (status != Acad::eOk) { delete reference; return status; }
    return appendEntity(modelSpace, manager, reference, id);
}

Acad::ErrorStatus writeText(AcDbXrecord* record, const ACHAR* text)
{
    resbuf value{};
    value.restype = AcDb::kDxfText;
    value.resval.rstring = const_cast<ACHAR*>(text);
    return record->setFromRbChain(value);
}

std::string ascii(const ACHAR* text)
{
    std::string result;
    for (const ACHAR* c = text; c != nullptr && *c != L'\0'; ++c) result.push_back(*c < 0x80 ? static_cast<char>(*c) : '?');
    return result;
}

std::string handleText(const AcDbObjectId& id)
{
    ACHAR buffer[32]{};
    if (id.isNull() || !id.handle().getIntoAsciiBuffer(buffer)) return "NULL";
    return ascii(buffer);
}

// Counts the resbufs of an Xrecord and reports whether it is exactly one text resbuf and its value.
void readPayload(const AcDbXrecord* record, int& count, bool& singleText, std::string& value)
{
    count = 0; singleText = false; value.clear();
    resbuf* chain = nullptr;
    if (record->rbChain(&chain) != Acad::eOk) return;
    for (const resbuf* item = chain; item != nullptr; item = item->rbnext) ++count;
    if (count == 1 && chain->restype == AcDb::kDxfText && chain->resval.rstring != nullptr)
    {
        singleText = true;
        value = ascii(chain->resval.rstring);
    }
    if (chain != nullptr) acutRelRb(chain);
}

bool semanticStateIsS0(const AcDbXrecord* record)
{
    int count = 0; bool text = false; std::string value;
    readPayload(record, count, text, value);
    return text && value == ascii(kStateS0);
}

bool linkHolds(const I52SmLinkFacts& facts, const ACHAR* expected)
{
    return facts.keyPresent && facts.identityMatches && !facts.erased && facts.isXrecord && facts.resbufCount == 1
        && facts.isText && facts.carriersFound == 1 && facts.value == ascii(expected);
}

std::string describe(const AcDbBlockReference* reference)
{
    std::ostringstream text;
    const AcGeMatrix3d matrix = reference->blockTransform();
    text << "matrix=";
    for (int row = 0; row < 4; ++row)
        for (int column = 0; column < 4; ++column) text << (row + column == 0 ? "" : ",") << matrix(row, column);
    text << ";layer=" << handleText(reference->layerId());
    return text.str();
}

// Reads the SM-LINK relation through the NOD key and cross-checks the bound ObjectId. Never opens erased objects:
// an erased carrier is detected by eWasErased.
I52SmLinkFacts readLinkFacts(AcDbTransactionManager* manager, AcDbDatabase* database, AcDbObjectId bound)
{
    I52SmLinkFacts facts{};
    AcDbObject* object = nullptr;
    if (manager->getObject(object, database->namedObjectsDictionaryId(), AcDb::kForRead) != Acad::eOk) return facts;
    AcDbDictionary* nod = AcDbDictionary::cast(object);
    if (nod == nullptr) return facts;
    AcDbDictionaryIterator* entries = nod->newIterator();
    for (; entries != nullptr && !entries->done(); entries->next())
    {
        AcDbObject* entry = nullptr;
        if (manager->getObject(entry, entries->objectId(), AcDb::kForRead) != Acad::eOk) continue;
        const AcDbXrecord* record = AcDbXrecord::cast(entry);
        if (record == nullptr) continue;
        int count = 0; bool text = false; std::string value;
        readPayload(record, count, text, value);
        if (text && value.rfind(ascii(kLinkPrefix), 0) == 0) ++facts.carriersFound;
    }
    delete entries;
    AcDbObjectId id;
    facts.keyPresent = nod->getAt(kLinkKey, id) == Acad::eOk;
    if (!facts.keyPresent) return facts;
    facts.identityMatches = !bound.isNull() && id == bound;
    AcDbObject* carrier = nullptr;
    const Acad::ErrorStatus status = manager->getObject(carrier, id, AcDb::kForRead);
    facts.erased = status == Acad::eWasErased;
    if (status != Acad::eOk) return facts;
    const AcDbXrecord* record = AcDbXrecord::cast(carrier);
    facts.isXrecord = record != nullptr;
    if (record != nullptr) readPayload(record, facts.resbufCount, facts.isText, facts.value);
    return facts;
}
}

Acad::ErrorStatus I52CtdaFixture::materialize(AcDbDatabase* database, AcTransactionManager* manager, I52FixtureIds& ids)
{
    if (database == nullptr || manager == nullptr) return Acad::eNullPtr;
    I52FixtureIds created{};

    AcDbLayerTable* layers = nullptr;
    Acad::ErrorStatus status = manager->getObject(reinterpret_cast<AcDbObject*&>(layers), database->layerTableId(), AcDb::kForWrite);
    if (status != Acad::eOk) return status;
    if ((status = addLayer(layers, manager, kLayerA, created.layerA)) != Acad::eOk) return status;
    if ((status = addLayer(layers, manager, kLayerB, created.layerB)) != Acad::eOk) return status;

    AcDbBlockTable* blocks = nullptr;
    if ((status = manager->getObject(reinterpret_cast<AcDbObject*&>(blocks), database->blockTableId(), AcDb::kForWrite)) != Acad::eOk) return status;
    if (blocks->has(kReferenceBlock)) return Acad::eDuplicateRecordName;
    auto* definition = new AcDbBlockTableRecord();
    if ((status = definition->setName(kReferenceBlock)) == Acad::eOk) status = blocks->add(created.referenceBlock, definition);
    if (status != Acad::eOk) { delete definition; return status; }
    if ((status = manager->addNewlyCreatedDBRObject(definition)) != Acad::eOk) return status;
    AcDbObjectId markerId;
    if ((status = appendEntity(definition, manager, new AcDbPoint(AcGePoint3d::kOrigin), markerId)) != Acad::eOk) return status;

    AcDbObjectId modelSpaceId;
    if ((status = blocks->getAt(ACDB_MODEL_SPACE, modelSpaceId)) != Acad::eOk) return status;
    AcDbBlockTableRecord* modelSpace = nullptr;
    if ((status = manager->getObject(reinterpret_cast<AcDbObject*&>(modelSpace), modelSpaceId, AcDb::kForWrite)) != Acad::eOk) return status;

    if ((status = appendTrigger(database, modelSpace, manager, 1000.0, created.triggerModify)) != Acad::eOk) return status;
    if ((status = appendTrigger(database, modelSpace, manager, 1010.0, created.triggerEraseDatabase)) != Acad::eOk) return status;
    if ((status = appendTrigger(database, modelSpace, manager, 1020.0, created.triggerEraseObject)) != Acad::eOk) return status;
    if ((status = appendReference(database, modelSpace, manager, kSiblingAPosition, created.referenceBlock, created.layerA, created.siblingA)) != Acad::eOk) return status;
    if ((status = appendReference(database, modelSpace, manager, kStateM0Displacement, created.referenceBlock, created.layerA, created.materialReference)) != Acad::eOk) return status;
    // F-REF-C is declared absent: it is not created here; MUT-SM appends it.

    AcDbDictionary* nod = nullptr;
    if ((status = manager->getObject(reinterpret_cast<AcDbObject*&>(nod), database->namedObjectsDictionaryId(), AcDb::kForWrite)) != Acad::eOk) return status;
    if (nod->has(kSemanticKey) || nod->has(kLinkKey)) return Acad::eDuplicateRecordName;
    auto* semantic = new AcDbXrecord();
    if ((status = nod->setAt(kSemanticKey, semantic, created.semanticXrecord)) != Acad::eOk) { delete semantic; return status; }
    if ((status = manager->addNewlyCreatedDBRObject(semantic)) != Acad::eOk) return status;
    if ((status = writeText(semantic, kStateS0)) != Acad::eOk) return status;
    auto* link = new AcDbXrecord();
    if ((status = nod->setAt(kLinkKey, link, created.linkCarrier)) != Acad::eOk) { delete link; return status; }
    if ((status = manager->addNewlyCreatedDBRObject(link)) != Acad::eOk) return status;
    if ((status = writeText(link, kLinkSm0)) != Acad::eOk) return status;

    ids = created;
    return Acad::eOk;
}

I52SmFacts I52CtdaFixture::captureSm(bool includeB) const
{
    I52SmFacts facts{};
    if (!bound_) return facts;
    AcDbDatabase* database = ids_.siblingA.database();
    AcDbTransactionManager* manager = database == nullptr ? nullptr : database->transactionManager();
    if (manager == nullptr || manager->startTransaction() == nullptr) return facts;

    facts.link = readLinkFacts(manager, database, ids_.linkCarrier);
    AcDbObject* object = nullptr;
    if (manager->getObject(object, ids_.siblingA, AcDb::kForRead) == Acad::eOk)
    {
        const AcDbBlockReference* anchor = AcDbBlockReference::cast(object);
        facts.anchorALive = anchor != nullptr;
        if (anchor != nullptr) facts.aEvidence = describe(anchor);
    }
    if (includeB)
    {
        facts.bRead = true;
        facts.bLive = manager->getObject(object, ids_.materialReference, AcDb::kForRead) == Acad::eOk;
    }
    facts.cBound = !ids_.siblingC.isNull();

    AcDbBlockTable* blocks = nullptr;
    AcDbObjectId modelSpaceId;
    AcDbBlockTableRecord* modelSpace = nullptr;
    AcDbBlockTableRecordIterator* entities = nullptr;
    if (manager->getObject(reinterpret_cast<AcDbObject*&>(blocks), database->blockTableId(), AcDb::kForRead) == Acad::eOk
        && blocks->getAt(ACDB_MODEL_SPACE, modelSpaceId) == Acad::eOk
        && manager->getObject(reinterpret_cast<AcDbObject*&>(modelSpace), modelSpaceId, AcDb::kForRead) == Acad::eOk
        && modelSpace->newIterator(entities, true, true) == Acad::eOk)
    {
        for (; !entities->done(); entities->step())
        {
            AcDbObjectId id;
            if (entities->getEntityId(id) != Acad::eOk) continue;
            // A and B are identified without being opened here, so a post-trigger capture never touches F-REF-B.
            if (id == ids_.siblingA || id == ids_.materialReference) continue;
            if (manager->getObject(object, id, AcDb::kForRead) != Acad::eOk) continue;
            const AcDbBlockReference* reference = AcDbBlockReference::cast(object);
            if (reference == nullptr || reference->blockTableRecord() != ids_.referenceBlock) continue;
            ++facts.foreignReferenceInserts;
            if (id == ids_.siblingC)
            {
                facts.cFound = true;
                facts.cLive = true;
                facts.cAtCreationPosition = reference->position() == kSiblingCCreationPosition;
                facts.cEvidence = describe(reference);
            }
        }
        delete entities;
    }
    manager->abortTransaction();
    return facts;
}

I52FixtureResolution I52CtdaFixture::resolve() const
{
    I52FixtureResolution result{};
    result.declared = kDeclaredIdentities;
    if (!bound_) return result;
    const I52SmFacts sm = captureSm(true);
    std::ostringstream snapshot;
    auto line = [&](const char* id, const AcDbObjectId& objectId, const char* presence, bool ok, const char* detail)
    {
        snapshot << id << '|' << handleText(objectId) << '|' << presence << '|' << detail << '|' << (ok ? "RESOLVED" : "MISMATCH") << '\n';
        if (ok) ++result.resolved;
    };
    auto trigger = [&](const char* id, const AcDbObjectId& objectId)
    {
        AcDbObjectPointer<AcDbPoint> point(objectId, AcDb::kForRead);
        line(id, objectId, "PRESENT", point.openStatus() == Acad::eOk && !point->isErased(), "AcDbPoint");
    };
    trigger("F-TRIGGER-MOD", ids_.triggerModify);
    trigger("F-TRIGGER-ERASE-DB", ids_.triggerEraseDatabase);
    trigger("F-TRIGGER-ERASE-OBJ", ids_.triggerEraseObject);
    {
        AcDbObjectPointer<AcDbXrecord> record(ids_.semanticXrecord, AcDb::kForRead);
        line("F-XR", ids_.semanticXrecord, "PRESENT", record.openStatus() == Acad::eOk && !record->isErased() && semanticStateIsS0(record.object()), "STATE-S-0");
    }
    {
        AcDbObjectPointer<AcDbBlockReference> reference(ids_.materialReference, AcDb::kForRead);
        const bool ok = reference.openStatus() == Acad::eOk && !reference->isErased()
            && reference->position() == kStateM0Displacement && reference->layerId() == ids_.layerA;
        line("F-REF-B", ids_.materialReference, "PRESENT", ok, "STATE-M-0");
    }
    line("F-REF-A", ids_.siblingA, "PRESENT", sm.anchorALive, "STATE-SM-0 sibling A");
    line("F-REF-C", ids_.siblingC, "ABSENT", !sm.cBound && sm.foreignReferenceInserts == 0, "declared absent (not created)");
    result.bindingResolved = linkHolds(sm.link, kLinkSm0) && sm.anchorALive && sm.bLive && !sm.cBound && sm.foreignReferenceInserts == 0;
    snapshot << "BINDING:SM-LINK|" << handleText(ids_.linkCarrier) << '|' << (sm.link.keyPresent ? "PRESENT" : "ABSENT")
             << "|STATE-SM-0|" << (result.bindingResolved ? "RESOLVED" : "MISMATCH") << '\n';
    result.snapshot = snapshot.str();
    return result;
}

Acad::ErrorStatus I52CtdaFixture::mutateSemantic()
{
    if (!bound_ || ids_.semanticXrecord.isNull()) return Acad::eNullObjectId;
    AcDbObjectPointer<AcDbXrecord> record(ids_.semanticXrecord, AcDb::kForWrite);
    if (record.openStatus() != Acad::eOk) return record.openStatus();
    resbuf value{};
    value.restype = AcDb::kDxfText;
    value.resval.rstring = const_cast<ACHAR*>(L"HFV30:S:1");
    return record->setFromRbChain(value);
}

Acad::ErrorStatus I52CtdaFixture::mutateMaterial()
{
    if (!bound_ || ids_.materialReference.isNull() || ids_.layerB.isNull()) return Acad::eNullObjectId;
    AcDbObjectPointer<AcDbBlockReference> reference(ids_.materialReference, AcDb::kForWrite);
    if (reference.openStatus() != Acad::eOk) return reference.openStatus();
    Acad::ErrorStatus status = reference->setBlockTransform(AcGeMatrix3d::translation(AcGeVector3d(110.0, 220.0, 0.0)));
    if (status != Acad::eOk) return status;
    return reference->setLayer(ids_.layerB);
}

// MUT-SM (V34 binding, decisions section 164): the write set is exactly {SM-LINK carrier, one new F-REF-C}.
// F-REF-A is a read-only anchor, F-REF-B is never opened and F-XR is untouched. Both writes happen in the caller's
// active transaction, so they commit or abort together.
Acad::ErrorStatus I52CtdaFixture::mutateMixed()
{
    if (!bound_ || ids_.linkCarrier.isNull() || ids_.referenceBlock.isNull() || ids_.layerA.isNull()) return Acad::eNullObjectId;
    if (!ids_.siblingC.isNull()) return Acad::eInvalidInput;
    AcDbDatabase* database = ids_.siblingA.database();
    AcDbTransactionManager* manager = database == nullptr ? nullptr : database->transactionManager();
    if (manager == nullptr || manager->numActiveTransactions() == 0) return Acad::eNoActiveTransactions;
    const I52SmFacts before = captureSm(false);
    if (!linkHolds(before.link, kLinkSm0) || !before.anchorALive || before.foreignReferenceInserts != 0) return Acad::eInvalidInput;

    AcDbBlockTable* blocks = nullptr;
    Acad::ErrorStatus status = manager->getObject(reinterpret_cast<AcDbObject*&>(blocks), database->blockTableId(), AcDb::kForRead);
    AcDbObjectId modelSpaceId;
    if (status == Acad::eOk) status = blocks->getAt(ACDB_MODEL_SPACE, modelSpaceId);
    AcDbBlockTableRecord* modelSpace = nullptr;
    if (status == Acad::eOk) status = manager->getObject(reinterpret_cast<AcDbObject*&>(modelSpace), modelSpaceId, AcDb::kForWrite);
    if (status != Acad::eOk) return status;
    AcDbObjectId created;
    if ((status = appendReference(database, modelSpace, manager, kSiblingCCreationPosition, ids_.referenceBlock, ids_.layerA, created)) != Acad::eOk) return status;

    AcDbObject* object = nullptr;
    if ((status = manager->getObject(object, ids_.linkCarrier, AcDb::kForWrite)) != Acad::eOk) return status;
    AcDbXrecord* carrier = AcDbXrecord::cast(object);
    if (carrier == nullptr) return Acad::eWrongObjectType;
    if ((status = writeText(carrier, kLinkSm1)) != Acad::eOk) return status;
    ids_.siblingC = created;
    return Acad::eOk;
}

// MUT-ALL: MUT-S {F-XR}, MUT-M {F-REF-B} and MUT-SM {SM-LINK carrier, new F-REF-C}, each exactly once, disjoint.
Acad::ErrorStatus I52CtdaFixture::mutateAll()
{
    Acad::ErrorStatus status = mutateSemantic();
    if (status != Acad::eOk) return status;
    status = mutateMaterial();
    if (status != Acad::eOk) return status;
    return mutateMixed();
}

Acad::ErrorStatus I52CtdaFixture::removeSmResources(AcTransactionManager* manager)
{
    if (!bound_ || manager == nullptr || ids_.linkCarrier.isNull()) return Acad::eNullObjectId;
    AcDbDatabase* database = ids_.linkCarrier.database();
    if (database == nullptr) return Acad::eNullPtr;
    AcDbObject* object = nullptr;
    Acad::ErrorStatus status = Acad::eOk;
    if (!ids_.siblingC.isNull())
    {
        if ((status = manager->getObject(object, ids_.siblingC, AcDb::kForWrite)) != Acad::eOk) return status;
        if ((status = object->erase()) != Acad::eOk) return status;
    }
    AcDbDictionary* nod = nullptr;
    if ((status = manager->getObject(reinterpret_cast<AcDbObject*&>(nod), database->namedObjectsDictionaryId(), AcDb::kForWrite)) != Acad::eOk) return status;
    AcDbObjectId removed;
    if ((status = nod->remove(kLinkKey, removed)) != Acad::eOk) return status;
    if (removed != ids_.linkCarrier) return Acad::eInvalidInput;
    if ((status = manager->getObject(object, ids_.linkCarrier, AcDb::kForWrite)) != Acad::eOk) return status;
    return object->erase();
}

bool I52CtdaFixture::linkCarrierRemoved() const
{
    if (!bound_ || ids_.linkCarrier.isNull()) return false;
    AcDbDatabase* database = ids_.linkCarrier.database();
    if (database == nullptr) return false;
    AcDbObjectPointer<AcDbDictionary> nod(database->namedObjectsDictionaryId(), AcDb::kForRead);
    if (nod.openStatus() != Acad::eOk || nod->has(kLinkKey)) return false;
    AcDbObjectPointer<AcDbObject> carrier(ids_.linkCarrier, AcDb::kForRead);
    return carrier.openStatus() == Acad::eWasErased;
}

bool I52CtdaFixture::protectedTargetsAreLive() const
{
    return bound_ && !ids_.semanticXrecord.isNull() && !ids_.materialReference.isNull() && !ids_.siblingA.isNull() && !ids_.linkCarrier.isNull();
}
