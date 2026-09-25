#include "aced.h"
#include "acdocman.h"
#include "rxregsvc.h"
#include "I52CtdaExecutor.h"

// R-NATIVE-ARX (research only; never deployed with RackCad). Registers the CommandResource commands of
// EXEC-CATALOG-V35-1 and the zero-ProbeId R3 smoke. Nothing is logged until I52CTDA_PROBE (or the smoke) starts.
namespace
{
constexpr const wchar_t* kCommandGroup = L"I52_CTDA_RESEARCH";

void bootCommand() { I52Executor::instance().boot(); }
void probeCommand() { I52Executor::instance().probe(); }
void fixtureCommand() { I52Executor::instance().fixtureCommand(); }
void cancelCommand() { I52Executor::instance().cancelCommand(); }
void queuedCommand() { I52Executor::instance().queuedCommand(); }
void finishCommand() { I52Executor::instance().finish(); }
void smokeCommand() { I52Executor::instance().smoke(); }
}

extern "C" AcRx::AppRetCode acrxEntryPoint(AcRx::AppMsgCode message, void* packet)
{
    switch (message)
    {
    case AcRx::kInitAppMsg:
        acrxDynamicLinker->unlockApplication(packet);
        acrxRegisterAppMDIAware(packet);
        acedRegCmds->addCommand(kCommandGroup, L"I52CTDA_BOOT", L"I52CTDA_BOOT", ACRX_CMD_MODAL, bootCommand);
        acedRegCmds->addCommand(kCommandGroup, L"I52CTDA_PROBE", L"I52CTDA_PROBE", ACRX_CMD_MODAL, probeCommand);
        acedRegCmds->addCommand(kCommandGroup, L"I52CTDA_FIXTURE", L"I52CTDA_FIXTURE", ACRX_CMD_MODAL, fixtureCommand);
        acedRegCmds->addCommand(kCommandGroup, L"HFV34_CANCEL", L"HFV34_CANCEL", ACRX_CMD_MODAL, cancelCommand);
        acedRegCmds->addCommand(kCommandGroup, L"I52CTDA_QUEUED", L"I52CTDA_QUEUED", ACRX_CMD_MODAL, queuedCommand);
        acedRegCmds->addCommand(kCommandGroup, L"I52CTDA_FINISH", L"I52CTDA_FINISH", ACRX_CMD_MODAL, finishCommand);
        acedRegCmds->addCommand(kCommandGroup, L"I52CTDA_SMOKE", L"I52CTDA_SMOKE", ACRX_CMD_MODAL, smokeCommand);
        I52Executor::instance().load();
        break;
    case AcRx::kUnloadAppMsg:
        // R-NATIVE-ARX is never unloaded while a row runs; residency ends at process exit.
        I52Executor::instance().unload();
        acedRegCmds->removeGroup(kCommandGroup);
        break;
    default:
        break;
    }
    return AcRx::kRetOK;
}
