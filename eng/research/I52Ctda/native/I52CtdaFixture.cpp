#include "I52CtdaFixture.h"

#include "dbents.h"
#include "dbobjptr.h"
#include "dbxrecrd.h"
#include "geassign.h"

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
