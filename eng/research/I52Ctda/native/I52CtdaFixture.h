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
    AcDbObjectId siblingC;
    AcDbObjectId layerA;
    AcDbObjectId layerB;
    AcDbObjectId referenceBlock;
    // SM-LINK carrier (V34 binding clarification, §164): relation store for the sibling set, not a fixture identity.
    AcDbObjectId linkCarrier;
};

// Resolution of the seven FEC-V34 identities. F-REF-C is declared absent and does not exist until MUT-SM creates it.
// The SM-LINK binding line is reported separately and is never counted as an identity.
struct I52FixtureResolution
{
    int declared{};
    int resolved{};
    bool bindingResolved{};
    std::string snapshot;
};

// Raw facts of the SM relation; the managed SmRules classify them (UNKNOWN / FAIL-SM / OK). Capturing never opens
// F-REF-B for write or erased, and a post-trigger capture never opens F-REF-B at all.
struct I52SmLinkFacts
{
    bool keyPresent{};
    bool identityMatches{};
    bool erased{};
    bool isXrecord{};
    int resbufCount{};
    bool isText{};
    int carriersFound{};
    std::string value;
};

struct I52SmFacts
{
    I52SmLinkFacts link;
    bool anchorALive{};
    bool bRead{};
    bool bLive{};
    int foreignReferenceInserts{};
    bool cBound{};
    bool cFound{};
    bool cLive{};
    bool cAtCreationPosition{};
    std::string aEvidence;
    std::string cEvidence;
};

class I52CtdaFixture final
{
public:
    static constexpr int kDeclaredIdentities = 7;

    // Materializes the fixture inside the caller-owned transaction. Fails with eDuplicateRecordName when any
    // fixture name or the SM-LINK key already exists, because the fixture requires a fresh scratch drawing.
    static Acad::ErrorStatus materialize(AcDbDatabase* database, AcTransactionManager* manager, I52FixtureIds& ids);

    void bind(const I52FixtureIds& ids) { ids_ = ids; bound_ = true; }
    bool isBound() const { return bound_; }
    const I52FixtureIds& ids() const { return ids_; }
    I52FixtureResolution resolve() const;
    // Fresh read transaction. includeB is true only before a trigger; after a trigger F-REF-B is never opened.
    I52SmFacts captureSm(bool includeB) const;
    Acad::ErrorStatus mutateSemantic();
    Acad::ErrorStatus mutateMaterial();
    Acad::ErrorStatus mutateMixed();
    Acad::ErrorStatus mutateAll();
    // Cleanup inside the caller-owned transaction: erases C if created, removes the SM-LINK NOD entry and erases it.
    Acad::ErrorStatus removeSmResources(AcTransactionManager* manager);
    bool linkCarrierRemoved() const;
    bool protectedTargetsAreLive() const;

private:
    I52FixtureIds ids_{};
    bool bound_{};
};
