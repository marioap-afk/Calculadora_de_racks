#pragma once

#include "dbid.h"
#include "acadstrc.h"

#include <string>

class AcDbDatabase;
class AcTransactionManager;

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
    AcDbObjectId layerA;
    AcDbObjectId layerB;
};

// Resolution of the seven FEC-V34 identities. Present identities are live; F-REF-C is declared absent and is
// materialized as an erased reference so its identity stays resolvable for MUT-SM.
struct I52FixtureResolution
{
    int declared{};
    int resolved{};
    std::string snapshot;
};

class I52CtdaFixture final
{
public:
    static constexpr int kDeclaredIdentities = 7;

    // Materializes the fixture inside the caller-owned transaction. Fails with eDuplicateRecordName when any
    // fixture name already exists, because the fixture requires a fresh scratch drawing.
    static Acad::ErrorStatus materialize(AcDbDatabase* database, AcTransactionManager* manager, I52FixtureIds& ids);

    void bind(const I52FixtureIds& ids) { ids_ = ids; bound_ = true; }
    bool isBound() const { return bound_; }
    const I52FixtureIds& ids() const { return ids_; }
    I52FixtureResolution resolve() const;
    Acad::ErrorStatus mutateSemantic();
    Acad::ErrorStatus mutateMaterial();
    Acad::ErrorStatus mutateMixed();
    Acad::ErrorStatus mutateAll();
    bool protectedTargetsAreLive() const;

private:
    I52FixtureIds ids_{};
    bool bound_{};
};
