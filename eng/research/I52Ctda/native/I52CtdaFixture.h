#pragma once

#include "dbid.h"
#include "acadstrc.h"

struct I52FixtureIds
{
    AcDbObjectId triggerModify;
    AcDbObjectId triggerEraseDatabase;
    AcDbObjectId triggerEraseObject;
    AcDbObjectId semanticXrecord;
    AcDbObjectId materialReference;
    AcDbObjectId siblingA;
    AcDbObjectId siblingB;
    AcDbObjectId siblingC;
    AcDbObjectId layerB;
};

class I52CtdaFixture final
{
public:
    void bind(const I52FixtureIds& ids) { ids_ = ids; bound_ = true; }
    bool isBound() const { return bound_; }
    Acad::ErrorStatus mutateSemantic();
    Acad::ErrorStatus mutateMaterial();
    Acad::ErrorStatus mutateMixed();
    Acad::ErrorStatus mutateAll();
    bool protectedTargetsAreLive() const;

private:
    I52FixtureIds ids_{};
    bool bound_{};
};
