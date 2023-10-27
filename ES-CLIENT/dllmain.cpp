#include <cstdio>
#include <iostream>
#include <thread>
#include <algorithm>
#include <iterator>
#include <nlohmann/json.hpp>

#include "comPipe.h"
#include "MinHook.h"
#include "RTTI.h"
#include "SimHooks.h"
#include "Helpers.h"

using json = nlohmann::json;

static EngineEdit* engineEdit;
static EngineUpdate* engineUpdate;
static SimHooks* simHooks;
static SimFunctions* simFunctions;
static Helpers* helpers;
static Globals* _g;

void sendJson() {
    json j;

    j["Status"] = engineUpdate->Status;
    j["Name"] = engineUpdate->Name;
    j["cylinderCount"] = engineUpdate->cylinderCount;
    j["RPM"] = engineUpdate->RPM;
    j["maxRPM"] = engineUpdate->maxRPM;
    j["sparkAdvance"] = engineUpdate->sparkAdvance;
    j["tps"] = engineUpdate->tps;
	j["vehicleSpeed"] = engineUpdate->vehicleSpeed;
    j["sparkTimingList"] = engineUpdate->sparkTimingList;
    j["manifoldPressure"] = engineUpdate->manifoldPressure;
    j["gear"] = engineUpdate->gear;
    j["clutchPosition"] = engineUpdate->clutchPosition;
    j["atLimiter"] = engineUpdate->atLimiter;
    j["twoStepActive"] = engineUpdate->twoStepActive;
    j["engineLoad"] = engineUpdate->engineLoad;

    std::string pre = j.dump();
    pre.erase(std::remove(pre.begin(), pre.end(), '\n'), pre.cend());
    const char* result = pre.c_str();
    pipeSendText(result);
}

void UpdateParams() {
    engineUpdate->Status = "Connected";
}

void HandleUpdate(const char* input) {
    json data = json::parse(input);
    engineEdit->sparkAdvance = data["sparkAdvance"];
    engineEdit->useIgnTable = data["useIgnTable"];
    engineEdit->customSpark = data["customSpark"];
    engineEdit->useRpmTable = data["useRpmTable"];
    engineEdit->customRevLimit = data["customRevLimit"];
    engineEdit->useCylinderTable = data["useCylinderTable"];
    engineEdit->useCylinderTableRandom = data["useCylinderTableRandom"];
    engineEdit->activeCylinderCount = data["activeCylinderCount"];
    engineEdit->activeCylindersRandomUpdateTime = data["activeCylindersRandomUpdateTime"];
    engineEdit->quickShiftEnabled = data["quickShiftEnabled"];
    engineEdit->quickShiftTime = data["quickShiftTime"];
    engineEdit->quickShiftRetardTime = data["quickShiftRetardTime"];
    engineEdit->quickShiftRetardDeg = data["quickShiftRetardDeg"];
    engineEdit->quickShiftMode = data["quickShiftMode"];
    engineEdit->quickShiftAutoClutch = data["quickShiftAutoClutch"];
    engineEdit->quickShiftCutThenShift = data["quickShiftCutThenShift"];
    engineEdit->autoBlipEnabled = data["autoBlipEnabled"];
    engineEdit->autoBlipThrottle = data["autoBlipThrottle"];
    engineEdit->autoBlipTime = data["autoBlipTime"];
    engineEdit->dsgFarts = data["dsgFarts"];
    engineEdit->twoStepEnabled = data["twoStepEnabled"];
    engineEdit->disableRevLimit = data["disableRevLimit"];
    engineEdit->rev1 = data["rev1"];
    engineEdit->rev2 = data["rev2"];
    engineEdit->rev3 = data["rev3"];
    engineEdit->useCustomIgnitionModule = data["useCustomIgnitionModule"];
    engineEdit->twoStepLimiterMode = data["twoStepLimiterMode"];
    engineEdit->twoStepCutTime = data["twoStepCutTime"];
    engineEdit->twoStepRetardDeg = data["twoStepRetardDeg"];
    engineEdit->twoStepSwitchThreshold = data["twoStepSwitchThreshold"];
    engineEdit->allowTwoStepInGear = data["allowTwoStepInGear"];
    engineEdit->idleHelper = data["idleHelper"];
    engineEdit->idleHelperRPM = data["idleHelperRPM"];
    engineEdit->idleHelperMaxTps = data["idleHelperMaxTps"];
    engineEdit->speedLimiter = data["speedLimiter"];
    engineEdit->speedLimiterSpeed = data["speedLimiterSpeed"];
    engineEdit->speedLimiterMode = data["speedLimiterMode"];
    engineEdit->useAfrTable = data["useAfrTable"];
    engineEdit->targetAfr = data["targetAfr"];
    engineEdit->loadCalibrationMode = data["loadCalibrationMode"];
}

void PipeReader() {
    while (true) {
        HANDLE hPipe;
        char buffer[10240];
        DWORD dwRead;

        hPipe = CreateNamedPipe(TEXT("\\\\.\\pipe\\est-output-pipe"),
            PIPE_ACCESS_DUPLEX,
            PIPE_TYPE_BYTE | PIPE_READMODE_BYTE | PIPE_WAIT,
            1,
            10240 * 16,
            10240 * 16,
            NMPWAIT_USE_DEFAULT_WAIT,
            NULL);
        while (hPipe != INVALID_HANDLE_VALUE)
        {
            if (ConnectNamedPipe(hPipe, NULL) != FALSE)
            {
                while (ReadFile(hPipe, buffer, sizeof(buffer) - 1, &dwRead, NULL) != FALSE)
                {
                    buffer[dwRead] = '\0';
                    HandleUpdate(buffer);
                }
            }

            DisconnectNamedPipe(hPipe);
        }
    }
}

void Update() {
    while (true) {
        UpdateParams();
        helpers->buildSparkList();
        helpers->UpdateEngineType();
        sendJson();;
    }
}

void openConsole() {
    AllocConsole();
    FILE* fDummy;
    //freopen_s(&fDummy, "CONIN$", "r", stdin);
    freopen_s(&fDummy, "CONOUT$", "w", stderr);
    freopen_s(&fDummy, "CONOUT$", "w", stdout);
}

void Main() {
    #ifdef _DEBUG
    openConsole();	
    #endif
    printf("ES Client Loaded!\n");

    MH_Initialize();
    _g = new Globals();
    engineEdit = new EngineEdit();
    engineUpdate = new EngineUpdate();
    simFunctions = new SimFunctions();
    simHooks = new SimHooks(engineUpdate, engineEdit, simFunctions, _g);
    helpers = new Helpers(engineUpdate, engineEdit, simFunctions, _g);

    std::thread update(Update);
    std::thread reader(PipeReader);
    update.join();
    reader.join();
}


BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        DisableThreadLibraryCalls(hModule);
        CreateThread(0, 0, (LPTHREAD_START_ROUTINE)Main, hModule, 0, 0);
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

