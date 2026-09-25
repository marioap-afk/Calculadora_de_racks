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
// Normative values come from NPM-V34 §2 (STATE-S-0, STATE-M-0). Names without a V34 value (block, NOD key,
// trigger entities) are materialization bindings only; no probe outcome depends on them.
constexpr const ACHAR* kLayerA = L"RACKCAD_CTDA_V30_A";
constexpr const ACHAR* kLayerB = L"RACKCAD_CTDA_V30_B";
constexpr const ACHAR* kReferenceBlock = L"RACKCAD_CTDA_V34_REF";
constexpr const ACHAR* kSemanticKey = L"RACKCAD_CTDA_V34_F-XR";
constexpr const ACHAR* kStateS0 = L"HFV30:S:0";
const AcGePoint3d kStateM0Displacement(10.0, 20.0, 0.0);
const AcGePoint3d kSiblingAPosition(0.0, 0.0, 0.0);
const AcGePoint3d kSiblingCPosition(20.0, 20.0, 0.0);

Acad::ErrorStatus addLayer(AcDbLayerTable* table, AcTransactionManager* manager, const ACHAR* name, AcDbObjectId& id)
{
    if (table->has(name)) return Acad::eDuplicateRecordName;
    auto* record = new AcDbLayerTableRecord();
    Acad::ErrorStatus status = record->setName(name);
    if (status == Acad::eOk) status = table->add(id, record);
    if (status != Acad::eOk) { delete record; return status; }
    return manager->addNewlyCreatedDBRObject(record);
}

Acad::ErrorStatus appendEntity(AcDbBlockTableRecord* owner, AcTransactionManager* manager, AcDbEntity* entity, AcDbObjectId& id)
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

Acad::ErrorStatus appendReference(AcDbDatabase* database, AcDbBlockTableRecord* modelSpace, AcTransactionManager* manager, const AcGePoint3d& position, AcDbObjectId block, AcDbObjectId layer, AcDbObjectId& id)
{
    auto* reference = new AcDbBlockReference(position, block);
    reference->setDatabaseDefaults(database);
    Acad::ErrorStatus status = reference->setLayer(layer);
    if (status != Acad::eOk) { delete reference; return status; }
    return appendEntity(modelSpace, manager, reference, id);
}

std::string handleText(const AcDbObjectId& id)
{
    ACHAR buffer[32]{};
    if (id.isNull() || !id.handle().getIntoAsciiBuffer(buffer)) return "NULL";
    std::string text;
    for (const ACHAR* c = buffer; *c != L'\0'; ++c) text.push_back(static_cast<char>(*c));
    return text;
}

bool semanticStateIsS0(const AcDbXrecord* record)
{
    resbuf* chain = nullptr;
    if (record->rbChain(&chain) != Acad::eOk || chain == nullptr) return false;
    const bool matches = chain->restype == AcDb::kDxfText && chain->resval.rstring != nullptr
        && std::wcscmp(chain->resval.rstring, kStateS0) == 0 && chain->rbnext == nullptr;
    acutRelRb(chain);
    return matches;
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
    AcDbObjectId definitionId;
    if ((status = definition->setName(kReferenceBlock)) == Acad::eOk) status = blocks->add(definitionId, definition);
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
    if ((status = appendReference(database, modelSpace, manager, kSiblingAPosition, definitionId, created.layerA, created.siblingA)) != Acad::eOk) return status;
    if ((status = appendReference(database, modelSpace, manager, kStateM0Displacement, definitionId, created.layerA, created.materialReference)) != Acad::eOk) return status;
    created.siblingB = created.materialReference;
    if ((status = appendReference(database, modelSpace, manager, kSiblingCPosition, definitionId, created.layerA, created.siblingC)) != Acad::eOk) return status;
    AcDbObject* siblingC = nullptr;
    if ((status = manager->getObject(siblingC, created.siblingC, AcDb::kForWrite)) != Acad::eOk) return status;
    if ((status = siblingC->erase(true)) != Acad::eOk) return status;

    AcDbDictionary* nod = nullptr;
    if ((status = manager->getObject(reinterpret_cast<AcDbObject*&>(nod), database->namedObjectsDictionaryId(), AcDb::kForWrite)) != Acad::eOk) return status;
    if (nod->has(kSemanticKey)) return Acad::eDuplicateRecordName;
    auto* semantic = new AcDbXrecord();
    if ((status = nod->setAt(kSemanticKey, semantic, created.semanticXrecord)) != Acad::eOk) { delete semantic; return status; }
    if ((status = manager->addNewlyCreatedDBRObject(semantic)) != Acad::eOk) return status;
    resbuf value{};
    value.restype = AcDb::kDxfText;
    value.resval.rstring = const_cast<ACHAR*>(kStateS0);
    if ((status = semantic->setFromRbChain(value)) != Acad::eOk) return status;

    ids = created;
    return Acad::eOk;
}

I52FixtureResolution I52CtdaFixture::resolve() const
{
    I52FixtureResolution result{};
    result.declared = kDeclaredIdentities;
    if (!bound_) return result;
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
    {
        AcDbObjectPointer<AcDbBlockReference> reference(ids_.siblingA, AcDb::kForRead);
        const bool ok = reference.openStatus() == Acad::eOk && !reference->isErased() && ids_.siblingB == ids_.materialReference;
        line("F-REF-A", ids_.siblingA, "PRESENT", ok, "STATE-SM-0 siblings {F-REF-A,F-REF-B}");
    }
    {
        AcDbObjectPointer<AcDbBlockReference> reference(ids_.siblingC, AcDb::kForRead, true);
        line("F-REF-C", ids_.siblingC, "ABSENT", reference.openStatus() == Acad::eOk && reference->isErased(), "declared absent (erased)");
    }
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

Acad::ErrorStatus I52CtdaFixture::mutateMixed()
{
    if (!bound_ || ids_.siblingB.isNull() || ids_.siblingC.isNull()) return Acad::eNullObjectId;
    AcDbObjectPointer<AcDbBlockReference> siblingB(ids_.siblingB, AcDb::kForWrite, true);
    if (siblingB.openStatus() != Acad::eOk) return siblingB.openStatus();
    Acad::ErrorStatus status = siblingB->erase(true);
    if (status != Acad::eOk) return status;
    AcDbObjectPointer<AcDbBlockReference> siblingC(ids_.siblingC, AcDb::kForWrite, true);
    if (siblingC.openStatus() != Acad::eOk) return siblingC.openStatus();
    status = siblingC->erase(false);
    if (status != Acad::eOk) return status;
    return mutateSemantic();
}

Acad::ErrorStatus I52CtdaFixture::mutateAll()
{
    Acad::ErrorStatus status = mutateSemantic();
    if (status != Acad::eOk) return status;
    status = mutateMaterial();
    if (status != Acad::eOk) return status;
    return mutateMixed();
}

bool I52CtdaFixture::protectedTargetsAreLive() const
{
    return bound_ && !ids_.semanticXrecord.isNull() && !ids_.materialReference.isNull() && !ids_.siblingA.isNull() && !ids_.siblingB.isNull();
}
