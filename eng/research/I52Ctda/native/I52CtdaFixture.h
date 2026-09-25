#pragma once

#include "dbid.h"
#include "acadstrc.h"
#include "gepnt3d.h"

#include <string>

class AcDbDatabase;
class AcTransactionManager;
class AcDbTransactionManager;

// fixture-v35.json: eight persistent identities (F-TRIGGER-XR added by V35). R-SM-LINK, F-TRIGGER-APPEND,
// R-PAYLOAD-ARX, observer registrations and command resources are not identities.
struct I52FixtureIds
{
    AcDbObjectId triggerModify;        // F-TRIGGER-MOD
    AcDbObjectId triggerEraseDatabase; // F-TRIGGER-ERASE-DB
    AcDbObjectId triggerEraseObject;   // F-TRIGGER-ERASE-OBJ
    AcDbObjectId triggerXrecord;       // F-TRIGGER-XR
    AcDbObjectId semanticXrecord;      // F-XR
    AcDbObjectId materialReference;    // F-REF-B
    AcDbObjectId siblingA;             // F-REF-A
    AcDbObjectId siblingC;             // F-REF-C (declared absent; appended by MUT-SM)
    AcDbObjectId layerA;
    AcDbObjectId layerB;
    AcDbObjectId referenceBlock;
    AcDbObjectId modelSpace;
    // SM-LINK carrier (V34 binding, decisions section 164): relation store for the sibling set, not a fixture identity.
    AcDbObjectId linkCarrier;
    AcDbObjectId triggerAppend;        // F-TRIGGER-APPEND (DisposableTriggerResource), set by the append triggers
};

struct I52FixtureResolution
{
    int declared{};
    int resolved{};
    bool bindingResolved{};
    std::string snapshot;
};

// Raw facts of the SM relation; the control plane classifies them. Capturing never opens F-REF-B for write or
// erased, and a post-trigger capture never opens F-REF-B at all.
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

struct I52MaterialFacts
{
    bool live{};
    double x{}, y{}, z{};
    std::string layer;
};

class I52CtdaFixture final
{
public:
    static constexpr int kDeclaredIdentities = 8;
    static const ACHAR* const kCancelStagedBytes;
    static const ACHAR* const kTriggerXrWritten;
    static const AcGePoint3d kTriggerModifiedPosition;
    static const AcGePoint3d kTriggerAppendPosition;

    // BOOT-01: materializes the eight identities and R-SM-LINK inside the caller-owned transaction. Fails with
    // eDuplicateRecordName when any fixture name already exists: a fresh scratch drawing is required.
    static Acad::ErrorStatus materialize(AcDbDatabase* database, AcTransactionManager* manager, I52FixtureIds& ids);

    void bind(const I52FixtureIds& ids) { ids_ = ids; bound_ = true; }
    bool isBound() const { return bound_; }
    const I52FixtureIds& ids() const { return ids_; }
    void setTriggerAppend(AcDbObjectId id) { ids_.triggerAppend = id; }
    I52FixtureResolution resolve() const;

    // Fresh independent reads (own read transaction, aborted after the read).
    I52SmFacts captureSm(bool includeB) const;
    std::string readSemanticFresh() const;
    std::string readTriggerXrFresh() const;
    I52MaterialFacts readMaterialFresh() const;

    // Reads/writes through the caller's active top transaction (no transaction is started here).
    static Acad::ErrorStatus writeXrecordText(AcDbTransactionManager* manager, AcDbObjectId id, const ACHAR* text);
    static Acad::ErrorStatus readXrecordText(AcDbTransactionManager* manager, AcDbObjectId id, std::string& text);

    // Mutation actions through the caller's active top transaction (MUT-S, MUT-M, MUT-SM, MUT-ALL).
    Acad::ErrorStatus mutateSemantic();
    Acad::ErrorStatus mutateMaterial();
    Acad::ErrorStatus mutateMixed();
    Acad::ErrorStatus mutateAll();

    // CLN-BASE inside the caller-owned transaction: erases every live fixture resource (incl. F-TRIGGER-APPEND), never
    // reopening an object that is already erased, removes the NOD entries and R-SM-LINK (LINK-CARRIER-REMOVED).
    Acad::ErrorStatus removeFixtureResources(AcTransactionManager* manager, int& erased, int& skippedErased);
    bool linkCarrierRemoved() const;

private:
    I52SmLinkFacts readLink(AcDbTransactionManager* manager) const;
    I52FixtureIds ids_{};
    bool bound_{};
};
