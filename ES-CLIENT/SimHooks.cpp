#include <iostream>
#include <string>
#include <cctype>

#include "SimHooks.h"
#include "Memory.h"
#include "MinHook.h"
#include "SimFunctionTypes.h"
#include "CustomIgnitionModule.h"
#include "units.h"

static EngineUpdate* engineUpdate;
static EngineEdit* engineEdit;
static SimFunctions* simFunctions;
static Globals* _g;

static CustomIgnitionModule* customIgnition;

void* ignitionModulePtr;
void* simProcessPtr;
void* sampleTrianglePtr;
void* rTachRenderPtr;
void* changeGearPtr;
void* setThrottleRotaryPtr;
void* setThrottlePistonPtr;

__int64 __fastcall ignitionModuleHk(__int64 a1, double a2) {
    engineUpdate->maxRPM = units::toRpm(*(double*)(a1 + 0x88));
    engineUpdate->cylinderCount = *(int*)(a1 + 0x78);
    engineUpdate->RPM = units::toRpm(std::fabs(*(double*)(*(QWORD*)(a1 + 0x60) + 0x30)));
    _g->ignitionInstance = a1;
    _g->ignitionFunctionInstance = *(QWORD*)(a1 + 0x58);

    if (_g->speedInstance != NULL) {
        unsigned __int64 v1 = *(unsigned __int64*)(_g->speedInstance + 0x298);
        engineUpdate->vehicleSpeed = std::abs(*(double*)(_g->speedInstance + 0x298) * *(double*)(_g->speedInstance + 0x30));
    }

    if (_g->engineInstance != NULL) {
        engineUpdate->manifoldPressure = simFunctions->m_getManifoldPressure(_g->engineInstance);

        //for some reason, the name address changes into a pointer to the name if the length is 16 or above
        //simple hacky workaround that works 99% of the time
        uintptr_t nameAddress = _g->engineInstance + 0x50;
        if (std::isalnum(*(char*)nameAddress)) {
            engineUpdate->Name = (char*)(nameAddress);
        }
        else {
            engineUpdate->Name = *(char**)(nameAddress);
        }
    }

    if (engineEdit->useCustomIgnitionModule && _g->ignitionInstance != NULL) {
        if (_g->customIgnitionNeedsUpdate) {
            customIgnition->SetParameters();
            _g->customIgnitionNeedsUpdate = false;
        }
        return customIgnition->ignitionProcess(a1, a2);
    }
    
    _g->customIgnitionNeedsUpdate = true;
    return simFunctions->m_ignitionModuleUpdate(a1, a2);
}

void __fastcall simProcessHk(__int64 a1, float a2) {
    _g->engineInstance = *(QWORD*)(a1 + 0x1600);
    
    _g->transmissionInstance = *(QWORD*)(*(QWORD*)(a1 + 0x1618) + 0x520);
    engineUpdate->gear = *(int*)(_g->transmissionInstance + 0x348);
    engineUpdate->clutchPosition = *(double*)(a1 + 0x28);
    _g->cleanTps = *(double*)(a1 + 0x18);

    if (_g->quickShift && engineEdit->quickShiftAutoClutch) {
        *(double*)(_g->transmissionInstance + 0x368) = 0.0;
    }

    if (_g->autoBlip) {
        _g->overrideThrottle = true;
        if (_g->isRotary) {
            simFunctions->m_setThrottleRotary(_g->engineInstance, engineEdit->autoBlipThrottle);
        }
        else {
            simFunctions->m_setThrottlePiston(_g->engineInstance, engineEdit->autoBlipThrottle);
        }
    }
    else {
        _g->overrideThrottle = false;
    }

    if (engineEdit->twoStepLimiterMode == 4 && engineEdit->twoStepEnabled) {
        _g->overrideThrottle = true;
    }

    engineUpdate->tps = 1.00 - *(double*)(_g->engineInstance + 0x188);

    if (!_g->quickShift) {
        simFunctions->m_simProcess(a1, a2);
    }
}

double __fastcall sampleTriangleHk(__int64 a1, double a2) {
    if (a1 == _g->ignitionFunctionInstance) {
        double sample = simFunctions->m_sampleTriangle(a1, a2);
        double Offset = 0;

        if (_g->quickShift && engineEdit->quickShiftMode == 1) {
            Offset = engineEdit->quickShiftRetardDeg;
        }

        if (_g->twoStepActive) {
            Offset += engineEdit->twoStepRetardDeg;
        }

        if (!engineEdit->useIgnTable) {
            engineUpdate->sparkAdvance = sample / units::deg;
            double deg = sample / units::deg;
            double offset = deg + engineEdit->sparkAdvance;
            engineUpdate->sparkAdvance += engineEdit->sparkAdvance;
            return (offset + Offset) * units::deg;
        }
        engineUpdate->sparkAdvance = engineEdit->customSpark;
        engineUpdate->sparkAdvance += engineEdit->sparkAdvance;

        return (engineEdit->customSpark + engineEdit->sparkAdvance + Offset) * units::deg;
    }
    return simFunctions->m_sampleTriangle(a1, a2);
}

__int64 __fastcall rTachRenderHk(QWORD* a1, __int64 a2) {
    _g->speedInstance = *(QWORD*)(a1[14] + 0x528);
    return simFunctions->m_rTachRender(a1, a2);
}

__int64 __fastcall changeGearHk(__int64 a1, signed int a2) {
    if (engineEdit->quickShiftEnabled && a2 > engineUpdate->gear && engineUpdate->gear != -1) {
        if (engineEdit->quickShiftMode == 0) {
            _g->quickShiftTimer = engineEdit->quickShiftTime;
        }
        else {
            _g->quickShiftTimer = engineEdit->quickShiftRetardTime;
        }
        _g->quickShift = true;

        if (!engineEdit->quickShiftCutThenShift) {
            return simFunctions->m_changeGear(a1, a2);
        }
        else {
            return 0;
        }
    }

    if (engineEdit->autoBlipEnabled && a2 < engineUpdate->gear && engineUpdate->gear != -1 && a2 != -1) {
        _g->autoBlipTimer = engineEdit->autoBlipTime;
        _g->autoBlip = true;
    }

    return simFunctions->m_changeGear(a1, a2);
}

__int64 __fastcall setThrottleRotaryHk(__int64 a1, double a2) {
    if (!_g->overrideThrottle) {
        return simFunctions->m_setThrottleRotary(a1, a2);
    }
}

__int64 __fastcall setThrottlePistonHk(__int64 a1, double a2) {
    if (!_g->overrideThrottle) {
        return simFunctions->m_setThrottlePiston(a1, a2);
    }
}

void SetupHooks() {
    uintptr_t ignitionModFunc = Memory::FindPatternIDA("40 53 48 81 EC ? ? ? ? 44 0F 29 54 24 ? 48 8B D9 48 8B 49 60 44 0F 29 4C 24 ? 45 0F 57 C9 44 0F 29 6C 24 ? 44 0F 28 E9 0F 57 C9 E8 ? ? ? ?");
    ignitionModulePtr = (void*)ignitionModFunc;

    uintptr_t sampleTriangleFunc = Memory::FindPatternIDA("40 53 48 83 EC 50 0F 29 74 24 ? 48 8B D9 0F 28 F1 F2 0F 59 71 ? 0F 28 CE E8 ? ? ? ? 4C 63 53 44 48 63 D0 45 85 D2 75 0E 0F 57 C0 0F 28");
    sampleTrianglePtr = (void*)sampleTriangleFunc; 

    uintptr_t processFunc = Memory::FindPatternIDA("48 8B C4 48 89 58 10 48 89 70 18 48 89 78 20 55 41 54 41 55 41 56 41 57 48 8D 68 A1 48 81 EC ? ? ? ? 0F 29 70 C8 0F 29 78 B8 44 0F 29 40 ? 44");
    simProcessPtr = (void*)processFunc;

    uintptr_t rTachRenderFunc = Memory::FindPatternIDA("48 8B C4 55 53 57 48 8D 68 A1 48 81 EC ? ? ? ? 0F 29 70 D8 0F 57 C0 0F 29 78 C8 48 8B DA 44 0F 29 40 ? 48 8B F9 44 0F 29 48 ? 45 0F 57 C9");
    rTachRenderPtr = (void*)rTachRenderFunc; 

    uintptr_t changeGearFunc = Memory::FindPatternIDA("83 FA FF 7C 0E 3B 91 ? ? ? ? 7D 06 89 91 ? ? ? ? C3");
    changeGearPtr = (void*)changeGearFunc; 

    uintptr_t setThrottleRotaryFunc = Memory::FindPatternIDA("48 8B 89 ? ? ? ? 48 8B 01 48 FF 60 08 CC CC 48 8B 81 ? ? ? ? 4C 8B C1 48 8B C8 48 8B 10 48 FF 62 10 CC CC CC CC CC CC CC CC CC CC CC CC 40 53 48 83 EC 20 48 8B D9 E8 ? ? ? ?");
    setThrottleRotaryPtr = (void*)setThrottleRotaryFunc;

    uintptr_t setThrottlePistonFunc = Memory::FindPatternIDA("48 8B 89 ? ? ? ? 48 8B 01 48 FF 60 08");
    setThrottlePistonPtr = (void*)setThrottlePistonFunc;

    simFunctions->m_sampleTriangleMod = (_sampleTriangle)(sampleTriangleFunc); //So we can call the hooked function instead of the original easily

    simFunctions->m_getManifoldPressure = (_getManifoldPressure)(Memory::FindPatternIDA("4C 63 91 ? ? ? ? 45 33 C9 F2 0F 10 2D ? ? ? ? 48 8B D1 0F 57 D2 0F 57 DB 4D 8B C2 49 83 FA 04 0F 8C ? ? ? ? 48 8B 81 ? ?"));
    simFunctions->m_getCycleAngle = (_getCycleAngle)(Memory::FindPatternIDA("48 83 EC 38 F2 0F 10 41 ? 0F 28 D1 F2 0F 5C 41 ? 0F 29 74 24 ? F2 0F 10 B1 ? ? ? ? 0F 28 CE F2 0F 5C D0 0F 28 C2 E8 ? ? ? ? 0F 57 C9 66"));

    Memory::WriteLogAddress("Base", Memory::getBase(), false);
    Memory::WriteLogAddress("Ignition Module", ignitionModFunc);
    Memory::WriteLogAddress("Sample Triangle", sampleTriangleFunc);
    Memory::WriteLogAddress("SimProcess", processFunc);
    Memory::WriteLogAddress("Right Tach Render", rTachRenderFunc);
    Memory::WriteLogAddress("Change Gear", changeGearFunc);
    Memory::WriteLogAddress("Set Throttle Rotary", setThrottleRotaryFunc);
    Memory::WriteLogAddress("Set Throttle Piston", setThrottlePistonFunc);
    Memory::WriteLogAddress("Get Manifold Pressure", (uintptr_t)simFunctions->m_getManifoldPressure);
    Memory::WriteLogAddress("Get Cycle Angle", (uintptr_t)simFunctions->m_getCycleAngle);


    if (MH_CreateHook(ignitionModulePtr, &ignitionModuleHk, reinterpret_cast<LPVOID*>(&simFunctions->m_ignitionModuleUpdate)) != MH_OK) {
        printf("Unable to hook ignition module\n");
    }
    else {
        MH_EnableHook(ignitionModulePtr);
    }
    
    if (MH_CreateHook(sampleTrianglePtr, &sampleTriangleHk, reinterpret_cast<LPVOID*>(&simFunctions->m_sampleTriangle)) != MH_OK) {
        printf("Unable to hook sample triangle\n");
    }
    else {
        MH_EnableHook(sampleTrianglePtr);
    }

    if (MH_CreateHook(simProcessPtr, &simProcessHk, reinterpret_cast<LPVOID*>(&simFunctions->m_simProcess)) != MH_OK) {
        printf("Unable to hook main process\n");
    }
    else {
        MH_EnableHook(simProcessPtr);
    }

    if (MH_CreateHook(rTachRenderPtr, &rTachRenderHk, reinterpret_cast<LPVOID*>(&simFunctions->m_rTachRender)) != MH_OK) {
        printf("Unable to hook right tach render\n");
    }
    else {
        MH_EnableHook(rTachRenderPtr);
    }

    if (MH_CreateHook(changeGearPtr, &changeGearHk, reinterpret_cast<LPVOID*>(&simFunctions->m_changeGear)) != MH_OK) {
        printf("Unable to hook change gear\n");
    }
    else {
        MH_EnableHook(changeGearPtr);
    }

    if (MH_CreateHook(setThrottleRotaryPtr, &setThrottleRotaryHk, reinterpret_cast<LPVOID*>(&simFunctions->m_setThrottleRotary)) != MH_OK) {
        printf("Unable to hook throttle rotary\n");
    }
    else {
        MH_EnableHook(setThrottleRotaryPtr);
    }

    if (MH_CreateHook(setThrottlePistonPtr, &setThrottlePistonHk, reinterpret_cast<LPVOID*>(&simFunctions->m_setThrottlePiston)) != MH_OK) {
        printf("Unable to hook throttle piston\n");
    }
    else {
        MH_EnableHook(setThrottlePistonPtr);
    }
}

SimHooks::SimHooks(EngineUpdate* update, EngineEdit* edit, SimFunctions* functions, Globals* g) {
    engineUpdate = update;
    engineEdit = edit;
    simFunctions = functions;
    _g = g;
    customIgnition = new CustomIgnitionModule(update, edit, functions, g);
    SetupHooks();
}