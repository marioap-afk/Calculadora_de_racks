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
// Normative values: NPM-V34 section 2 (STATE-S-0/1, STATE-M-0/1, STATE-SM-0/1, retained by V35) and fixture-v35.json.
// The SM-LINK carrier and the F-REF-C lifecycle follow the V34 binding clarifications of decisions section 164.
constexpr const ACHAR* kLayerA = L"RACKCAD_CTDA_V30_A";
constexpr const ACHAR* kLayerB = L"RACKCAD_CTDA_V30_B";
constexpr const ACHAR* kReferenceBlock = L"RACKCAD_CTDA_V34_REF";
constexpr const ACHAR* kSemanticKey = L"RACKCAD_CTDA_V34_F-XR";
constexpr const ACHAR* kTriggerXrKey = L"RACKCAD_CTDA_V35_F-TRIGGER-XR";
constexpr const ACHAR* kTriggerXr0 = L"HFV35:TRIGGER-XR:0";
constexpr const ACHAR* kStateS0 = L"HFV30:S:0";
constexpr const ACHAR* kStateS1 = L"HFV30:S:1";
constexpr const ACHAR* kLinkKey = L"RACKCAD_CTDA_V34_SM-LINK";
constexpr const ACHAR* kLinkPrefix = L"HFV30:LINK:";
constexpr const ACHAR* kLinkSm0 = L"HFV30:LINK:A,B";
constexpr const ACHAR* kLinkSm1 = L"HFV30:LINK:A,C";
const AcGePoint3d kStateM0Displacement(10.0, 20.0, 0.0);
const AcGeVector3d kStateM1Displacement(110.0, 220.0, 0.0);
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

Acad::ErrorStatus addNodXrecord(AcDbDictionary* nod, AcTransactionManager* manager, const ACHAR* key, const ACHAR* text, AcDbObjectId& id)
{
    auto* record = new AcDbXrecord();
    Acad::ErrorStatus status = nod->setAt(key, record, id);
    if (status != Acad::eOk) { delete record; return status; }
    if ((status = manager->addNewlyCreatedDBRObject(record)) != Acad::eOk) return status;
    return writeText(record, text);
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

// Model Space inserts of the reference block other than A and B, identified without opening A or B.
int countForeignInserts(AcDbTransactionManager* manager, AcDbDatabase* database, const I52FixtureIds& ids, I52SmFacts* facts)
{
    int count = 0;
    AcDbObject* object = nullptr;
    AcDbBlockTableRecord* modelSpace = nullptr;
    AcDbBlockTableRecordIterator* entities = nullptr;
    if (manager->getObject(reinterpret_cast<AcDbObject*&>(modelSpace), ids.modelSpace, AcDb::kForRead) != Acad::eOk) return -1;
    if (database == nullptr || modelSpace->newIterator(entities, true, true) != Acad::eOk) return -1;
    for (; !entities->done(); entities->step())
    {
        AcDbObjectId id;
        if (entities->getEntityId(id) != Acad::eOk) continue;
        if (id == ids.siblingA || id == ids.materialReference) continue;
        if (manager->getObject(object, id, AcDb::kForRead) != Acad::eOk) continue;
        const AcDbBlockReference* reference = AcDbBlockReference::cast(object);
        if (reference == nullptr || reference->blockTableRecord() != ids.referenceBlock) continue;
        ++count;
        if (facts != nullptr && id == ids.siblingC)
        {
            facts->cFound = true;
            facts->cLive = true;
            facts->cAtCreationPosition = reference->position() == kSiblingCCreationPosition;
            facts->cEvidence = describe(reference);
        }
    }
    delete entities;
    return count;
}

AcDbTransactionManager* databaseManager(const I52FixtureIds& ids)
{
    AcDbDatabase* database = ids.siblingA.database();
    return database == nullptr ? nullptr : database->transactionManager();
}
}

const ACHAR* const I52CtdaFixture::kCancelStagedBytes = L"HFV35:XR:CANCEL-STAGED";
const ACHAR* const I52CtdaFixture::kTriggerXrWritten = L"HFV35:TRIGGER-XR:1";
const AcGePoint3d I52CtdaFixture::kTriggerModifiedPosition(1000.0, 1.0, 0.0);
const AcGePoint3d I52CtdaFixture::kTriggerAppendPosition(1030.0, 0.0, 0.0);

// Reads the SM-LINK relation through the NOD key in the caller's active transaction and cross-checks the bound
// ObjectId. Never opens erased objects: an erased carrier is detected by eWasErased.
I52SmLinkFacts I52CtdaFixture::readLink(AcDbTransactionManager* manager) const
{
    I52SmLinkFacts facts{};
    AcDbDatabase* database = ids_.linkCarrier.database();
    AcDbObject* object = nullptr;
    if (database == nullptr || manager->getObject(object, database->namedObjectsDictionaryId(), AcDb::kForRead) != Acad::eOk) return facts;
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
    facts.identityMatches = !ids_.linkCarrier.isNull() && id == ids_.linkCarrier;
    AcDbObject* carrier = nullptr;
    const Acad::ErrorStatus status = manager->getObject(carrier, id, AcDb::kForRead);
    facts.erased = status == Acad::eWasErased;
    if (status != Acad::eOk) return facts;
    const AcDbXrecord* record = AcDbXrecord::cast(carrier);
    facts.isXrecord = record != nullptr;
    if (record != nullptr) readPayload(record, facts.resbufCount, facts.isText, facts.value);
    return facts;
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

    if ((status = blocks->getAt(ACDB_MODEL_SPACE, created.modelSpace)) != Acad::eOk) return status;
    AcDbBlockTableRecord* modelSpace = nullptr;
    if ((status = manager->getObject(reinterpret_cast<AcDbObject*&>(modelSpace), created.modelSpace, AcDb::kForWrite)) != Acad::eOk) return status;

    if ((status = appendTrigger(database, modelSpace, manager, 1000.0, created.triggerModify)) != Acad::eOk) return status;
    if ((status = appendTrigger(database, modelSpace, manager, 1010.0, created.triggerEraseDatabase)) != Acad::eOk) return status;
    if ((status = appendTrigger(database, modelSpace, manager, 1020.0, created.triggerEraseObject)) != Acad::eOk) return status;
    if ((status = appendReference(database, modelSpace, manager, kSiblingAPosition, created.referenceBlock, created.layerA, created.siblingA)) != Acad::eOk) return status;
    if ((status = appendReference(database, modelSpace, manager, kStateM0Displacement, created.referenceBlock, created.layerA, created.materialReference)) != Acad::eOk) return status;
    // F-REF-C is declared absent: it is not created here; MUT-SM appends it.

    AcDbDictionary* nod = nullptr;
    if ((status = manager->getObject(reinterpret_cast<AcDbObject*&>(nod), database->namedObjectsDictionaryId(), AcDb::kForWrite)) != Acad::eOk) return status;
    if (nod->has(kSemanticKey) || nod->has(kTriggerXrKey) || nod->has(kLinkKey)) return Acad::eDuplicateRecordName;
    if ((status = addNodXrecord(nod, manager, kSemanticKey, kStateS0, created.semanticXrecord)) != Acad::eOk) return status;
    if ((status = addNodXrecord(nod, manager, kTriggerXrKey, kTriggerXr0, created.triggerXrecord)) != Acad::eOk) return status;
    if ((status = addNodXrecord(nod, manager, kLinkKey, kLinkSm0, created.linkCarrier)) != Acad::eOk) return status;

    ids = created;
    return Acad::eOk;
}

I52SmFacts I52CtdaFixture::captureSm(bool includeB) const
{
    I52SmFacts facts{};
    if (!bound_) return facts;
    AcDbDatabase* database = ids_.siblingA.database();
    AcDbTransactionManager* manager = databaseManager(ids_);
    if (manager == nullptr || manager->startTransaction() == nullptr) return facts;

    facts.link = readLink(manager);
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
    facts.foreignReferenceInserts = countForeignInserts(manager, database, ids_, &facts);
    manager->abortTransaction();
    return facts;
}

std::string I52CtdaFixture::readSemanticFresh() const
{
    AcDbTransactionManager* manager = databaseManager(ids_);
    if (!bound_ || manager == nullptr || manager->startTransaction() == nullptr) return "<UNAVAILABLE>";
    std::string text;
    if (readXrecordText(manager, ids_.semanticXrecord, text) != Acad::eOk) text = "<UNAVAILABLE>";
    manager->abortTransaction();
    return text;
}

std::string I52CtdaFixture::readTriggerXrFresh() const
{
    AcDbTransactionManager* manager = databaseManager(ids_);
    if (!bound_ || manager == nullptr || manager->startTransaction() == nullptr) return "<UNAVAILABLE>";
    std::string text;
    if (readXrecordText(manager, ids_.triggerXrecord, text) != Acad::eOk) text = "<UNAVAILABLE>";
    manager->abortTransaction();
    return text;
}

// VER-M: the only reader of F-REF-B (VER-SM never opens it).
I52MaterialFacts I52CtdaFixture::readMaterialFresh() const
{
    I52MaterialFacts facts{};
    AcDbTransactionManager* manager = databaseManager(ids_);
    if (!bound_ || manager == nullptr || manager->startTransaction() == nullptr) return facts;
    AcDbObject* object = nullptr;
    if (manager->getObject(object, ids_.materialReference, AcDb::kForRead) == Acad::eOk)
    {
        if (const AcDbBlockReference* reference = AcDbBlockReference::cast(object))
        {
            facts.live = true;
            const AcGePoint3d p = reference->position();
            facts.x = p.x; facts.y = p.y; facts.z = p.z;
            facts.layer = reference->layerId() == ids_.layerA ? "RACKCAD_CTDA_V30_A" : reference->layerId() == ids_.layerB ? "RACKCAD_CTDA_V30_B" : "OTHER";
        }
    }
    manager->abortTransaction();
    return facts;
}

Acad::ErrorStatus I52CtdaFixture::writeXrecordText(AcDbTransactionManager* manager, AcDbObjectId id, const ACHAR* text)
{
    if (manager == nullptr || manager->numActiveTransactions() == 0) return Acad::eNoActiveTransactions;
    AcDbObject* object = nullptr;
    Acad::ErrorStatus status = manager->getObject(object, id, AcDb::kForWrite);
    if (status != Acad::eOk) return status;
    AcDbXrecord* record = AcDbXrecord::cast(object);
    return record == nullptr ? Acad::eWrongObjectType : writeText(record, text);
}

Acad::ErrorStatus I52CtdaFixture::readXrecordText(AcDbTransactionManager* manager, AcDbObjectId id, std::string& text)
{
    if (manager == nullptr || manager->numActiveTransactions() == 0) return Acad::eNoActiveTransactions;
    AcDbObject* object = nullptr;
    Acad::ErrorStatus status = manager->getObject(object, id, AcDb::kForRead);
    if (status != Acad::eOk) return status;
    const AcDbXrecord* record = AcDbXrecord::cast(object);
    if (record == nullptr) return Acad::eWrongObjectType;
    int count = 0; bool single = false;
    readPayload(record, count, single, text);
    return single ? Acad::eOk : Acad::eInvalidInput;
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
    line("F-TRIGGER-XR", ids_.triggerXrecord, "PRESENT", readTriggerXrFresh() == ascii(kTriggerXr0), "HFV35:TRIGGER-XR:0");
    line("F-XR", ids_.semanticXrecord, "PRESENT", readSemanticFresh() == ascii(kStateS0), "STATE-S-0");
    const I52MaterialFacts m = readMaterialFresh();
    line("F-REF-B", ids_.materialReference, "PRESENT", m.live && m.x == kStateM0Displacement.x && m.y == kStateM0Displacement.y && m.z == kStateM0Displacement.z && m.layer == "RACKCAD_CTDA_V30_A", "STATE-M-0");
    line("F-REF-A", ids_.siblingA, "PRESENT", sm.anchorALive, "STATE-SM-0 sibling A");
    line("F-REF-C", ids_.siblingC, "ABSENT", !sm.cBound && sm.foreignReferenceInserts == 0, "declared absent (not created)");
    result.bindingResolved = linkHolds(sm.link, kLinkSm0) && sm.anchorALive && sm.bLive && !sm.cBound && sm.foreignReferenceInserts == 0;
    snapshot << "BINDING:SM-LINK|" << handleText(ids_.linkCarrier) << '|' << (sm.link.keyPresent ? "PRESENT" : "ABSENT")
             << "|STATE-SM-0|" << (result.bindingResolved ? "RESOLVED" : "MISMATCH") << '\n';
    result.snapshot = snapshot.str();
    return result;
}

// MUT-S: exactly STATE-S-1 on F-XR through the caller's active top transaction.
Acad::ErrorStatus I52CtdaFixture::mutateSemantic()
{
    if (!bound_ || ids_.semanticXrecord.isNull()) return Acad::eNullObjectId;
    return writeXrecordText(databaseManager(ids_), ids_.semanticXrecord, kStateS1);
}

// MUT-M: exactly STATE-M-1 on F-REF-B (displacement (110,220,0), layer RACKCAD_CTDA_V30_B) through the top transaction.
Acad::ErrorStatus I52CtdaFixture::mutateMaterial()
{
    if (!bound_ || ids_.materialReference.isNull() || ids_.layerB.isNull()) return Acad::eNullObjectId;
    AcDbTransactionManager* manager = databaseManager(ids_);
    if (manager == nullptr || manager->numActiveTransactions() == 0) return Acad::eNoActiveTransactions;
    AcDbObject* object = nullptr;
    Acad::ErrorStatus status = manager->getObject(object, ids_.materialReference, AcDb::kForWrite);
    if (status != Acad::eOk) return status;
    AcDbBlockReference* reference = AcDbBlockReference::cast(object);
    if (reference == nullptr) return Acad::eWrongObjectType;
    if ((status = reference->setBlockTransform(AcGeMatrix3d::translation(kStateM1Displacement))) != Acad::eOk) return status;
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
    AcDbTransactionManager* manager = databaseManager(ids_);
    if (manager == nullptr || manager->numActiveTransactions() == 0) return Acad::eNoActiveTransactions;
    const I52SmLinkFacts before = readLink(manager);
    AcDbObject* anchor = nullptr;
    const bool anchorLive = manager->getObject(anchor, ids_.siblingA, AcDb::kForRead) == Acad::eOk;
    if (!linkHolds(before, kLinkSm0) || !anchorLive || countForeignInserts(manager, database, ids_, nullptr) != 0) return Acad::eInvalidInput;

    AcDbBlockTableRecord* modelSpace = nullptr;
    Acad::ErrorStatus status = manager->getObject(reinterpret_cast<AcDbObject*&>(modelSpace), ids_.modelSpace, AcDb::kForWrite);
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

Acad::ErrorStatus I52CtdaFixture::removeFixtureResources(AcTransactionManager* manager, int& erased, int& skippedErased)
{
    erased = 0; skippedErased = 0;
    if (!bound_ || manager == nullptr) return Acad::eNullObjectId;
    AcDbDatabase* database = ids_.linkCarrier.database();
    if (database == nullptr) return Acad::eNullPtr;
    Acad::ErrorStatus status = Acad::eOk;
    // Entities: an id reported erased (the trigger's own erase) is never reopened.
    const AcDbObjectId entities[] = { ids_.triggerModify, ids_.triggerEraseDatabase, ids_.triggerEraseObject, ids_.triggerAppend, ids_.materialReference, ids_.siblingA, ids_.siblingC };
    for (const AcDbObjectId& id : entities)
    {
        if (id.isNull()) continue;
        if (id.isErased()) { ++skippedErased; continue; }
        AcDbObject* object = nullptr;
        if ((status = manager->getObject(object, id, AcDb::kForWrite)) != Acad::eOk) return status;
        if ((status = object->erase()) != Acad::eOk) return status;
        ++erased;
    }
    AcDbDictionary* nod = nullptr;
    if ((status = manager->getObject(reinterpret_cast<AcDbObject*&>(nod), database->namedObjectsDictionaryId(), AcDb::kForWrite)) != Acad::eOk) return status;
    const struct { const ACHAR* key; AcDbObjectId id; } records[] = { { kSemanticKey, ids_.semanticXrecord }, { kTriggerXrKey, ids_.triggerXrecord }, { kLinkKey, ids_.linkCarrier } };
    for (const auto& record : records)
    {
        AcDbObjectId removed;
        if ((status = nod->remove(record.key, removed)) != Acad::eOk) return status;
        if (removed != record.id) return Acad::eInvalidInput;
        if (record.id.isErased()) { ++skippedErased; continue; }
        AcDbObject* object = nullptr;
        if ((status = manager->getObject(object, record.id, AcDb::kForWrite)) != Acad::eOk) return status;
        if ((status = object->erase()) != Acad::eOk) return status;
        ++erased;
    }
    return Acad::eOk;
}

bool I52CtdaFixture::linkCarrierRemoved() const
{
    if (!bound_ || ids_.linkCarrier.isNull()) return false;
    AcDbDatabase* database = ids_.linkCarrier.database();
    if (database == nullptr) return false;
    AcDbObjectPointer<AcDbDictionary> nod(database->namedObjectsDictionaryId(), AcDb::kForRead);
    if (nod.openStatus() != Acad::eOk || nod->has(kLinkKey)) return false;
    return ids_.linkCarrier.isErased();
}
