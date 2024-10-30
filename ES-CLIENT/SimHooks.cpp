#include <iostream>
#include <string>
#include <cctype>

#include "SimHooks.h"
#include "Memory.h"
#include "MinHook.h"
#include "SimFunctionTypes.h"
#include "CustomIgnitionModule.h"
#include "LoadCalculator.h";
#include "units.h"

static EngineUpdate* engineUpdate;
static EngineEdit* engineEdit;
static SimFunctions* simFunctions;
static Globals* _g;

static CustomIgnitionModule* customIgnition;
static LoadCalculator* loadCalculator;

void* ignitionModulePtr;
void* simProcessPtr;
void* sampleTrianglePtr;
void* rTachRenderPtr;
void* changeGearPtr;
void* setThrottleRotaryPtr;
void* setThrottlePistonPtr;
void* gasSystemResetPtr;
void* showMainTextPtr;
void* updateHpAndTorquePtr;
void* afrClusterPtr;
void* camAnglePtr;
void* camAngle2Ptr;

double camSpeed = 1.0; //default is 0.5 for half crank speed

struct Mix {
    double p_fuel = 0.0;
    double p_inert = 1.0;
    double p_o2 = 0.0;
};

struct Intake { // Size 0x1A0
    char pad0[0xC8];
    double m_throttle; //0xC8
    double m_flow; //0xD0
    double m_flowRate; //0xD8
    double m_totalFuelInjected; //0xE0
    double m_crossSectionArea; //0xE8
    double m_inputFlowK; //0xF0
    double m_idleFlowK; //0xF8
    double m_runnerFlowRate; //0x100
    double m_molecularAfr; //0x108
    double m_idleThrottlePlatePosition; //0x110
    double m_runnerLength; //0x118
    double m_velocityDecay; //0x120
    char pad1[0x1A0 - 0x120 - sizeof(double)];
};

struct CombustionChamber { //Size 0x2A8 (TODO: Disect Combustion Chamber Struct)
    char pad0[0x18];
    double v; //0x18
    double t; //0x20
    char pad1[0x30];
    int c; //0x58
    char pad2[0x2A8 - 0x58 - sizeof(int)];
};

#pragma region Functions

double getIntakeFlowRate() {
    double airIntake = 0;
    int m_intakeCount = *(int*)(_g->engineInstance + 0x180);
    Intake* intakes = *(Intake**)(_g->engineInstance + 0x178);
    for (int i = 0; i < m_intakeCount; ++i) {
        airIntake += intakes[i].m_flowRate;
    }
    return airIntake;
}

void setThrottle(double throttle) {
    if (_g->isRotary) {
        simFunctions->m_setThrottleRotary(_g->engineInstance, throttle);
    }
    else {
        simFunctions->m_setThrottlePiston(_g->engineInstance, throttle);
    }
}

void setDyno(bool enabled) {
    *(bool*)(_g->simulatorInstance + 0xE1) = enabled;
}

void setIngnition(bool enabled) {
    *(bool*)(_g->ignitionInstance + 0x50) = enabled;
}

void setStarter(bool enabled) {
    *(bool*)(_g->simulatorInstance + 0x1C0) = enabled;
}

double getStarterRPM() {
    return units::toRpm(std::fabs(*(double*)(_g->simulatorInstance + 0x1B8)));
}

double getTemperaturePiston() {
    if (_g->engineInstance) {
        double temperature = 0;
        int m_chamberCount = engineUpdate->cylinderCount;
        if (m_chamberCount == 0) return 0;

        CombustionChamber* chambers = *(CombustionChamber**)(_g->engineInstance + 0x1C0);
        if (chambers) {
            for (int i = 0; i < m_chamberCount; ++i) {
                double v = chambers[i].v;
                double c = (double)chambers[i].c * 0.5 * v * 8.31446261815324;
                if (c == 0) continue;

                double t = chambers[i].t / c;
                temperature += t;
            }
            return m_chamberCount > 0 ? temperature / m_chamberCount : 0;
        }
    }
    return 0;
}

double getTemperatureRotary() {
    return 0;
}

#pragma endregion

#pragma region Hook Functions

__int64 __fastcall ignitionModuleHk(__int64 a1, double a2) {
    if (_g->fullAttached) {
        _g->ignitionInstance = a1;
        _g->ignitionFunctionInstance = *(QWORD*)(a1 + 0x58);

        engineUpdate->maxRPM = units::toRpm(*(double*)(_g->ignitionInstance + 0x88));
        engineUpdate->cylinderCount = *(int*)(_g->ignitionInstance + 0x78);
        engineUpdate->RPM = units::toRpm(std::fabs(*(double*)(*(QWORD*)(_g->ignitionInstance + 0x60) + 0x30)));

        if (_g->speedInstance != NULL) {
            unsigned __int64 v1 = *(unsigned __int64*)(_g->speedInstance + 0x298);
            engineUpdate->vehicleSpeed = std::abs(*(double*)(_g->speedInstance + 0x298) * *(double*)(_g->speedInstance + 0x30));
        }

        if (engineEdit->useCustomIgnitionModule && _g->ignitionInstance != NULL) {
            if (_g->customIgnitionNeedsUpdate) {
                customIgnition->SetParameters();
                _g->customIgnitionNeedsUpdate = false;
            }
            return customIgnition->ignitionProcess(a1, a2);
        }

        _g->customIgnitionNeedsUpdate = true;
    }
    return simFunctions->m_ignitionModuleUpdate(a1, a2);
}

void __fastcall simProcessHk(__int64 a1, float a2) {
    if (_g->fullAttached) {
        _g->appInstance = a1;
        _g->simulatorInstance = *(QWORD*)(_g->appInstance + 0x1618);
        _g->engineInstance = *(QWORD*)(_g->appInstance + 0x1600);

        _g->transmissionInstance = *(QWORD*)(_g->simulatorInstance + 0x520);
        engineUpdate->gear = *(int*)(_g->transmissionInstance + 0x348);
        engineUpdate->clutchPosition = *(double*)(_g->appInstance + 0x28);
        _g->cleanTps = *(double*)(a1 + 0x18);

        double intakeFlow = units::convert(getIntakeFlowRate(), units::scfm);

        engineUpdate->engineLoad = loadCalculator->calculateLoadPct(loadCalculator->SCFMtoMAF(intakeFlow, 25.0), units::convert(units::pressure(1.0, units::atm), units::inHg), 25.0, engineUpdate->RPM, engineUpdate->tps);
        engineUpdate->manifoldPressure = simFunctions->m_getManifoldPressure(_g->engineInstance);
        engineUpdate->airSCFM = intakeFlow;

        if (_g->isRotary) {
            engineUpdate->temperature = getTemperatureRotary();
        }
        else {
            engineUpdate->temperature = getTemperaturePiston();
        }

        if (engineEdit->loadCalibrationMode) {
            if (!_g->calibrationCleared) {
                loadCalculator->ClearCalibration();
                _g->calibrationCleared = true;
            }
            _g->calibrationFinished = true;
            _g->overrideThrottle = true;
            *(int*)(_g->transmissionInstance + 0x348) = -1;
            setThrottle(1.0);
            setDyno(true);
            setIngnition(true);

            if (engineUpdate->RPM > getStarterRPM() + 500) {
                setStarter(false);
            }
            else {
                setStarter(true);
            }

            loadCalculator->Calibrate(engineUpdate->RPM, intakeFlow);
        }
        else {
            if (_g->calibrationFinished) {
                _g->calibrationCleared = false;
                _g->overrideThrottle = false;
                _g->calibrationFinished = false;
                setDyno(false);
                setStarter(false);
            }
        }

        if (!_g->debugShow) {
            printf("\r\nInstance Addresses\r\n------------------\r\n");
            Memory::WriteLogAddress("App", _g->appInstance, false);
            Memory::WriteLogAddress("Simulator", _g->simulatorInstance, false);
            Memory::WriteLogAddress("Engine", _g->engineInstance, false);
            Memory::WriteLogAddress("Transmission", _g->transmissionInstance, false);
            Memory::WriteLogAddress("Ignition", _g->ignitionInstance, false);
            Memory::WriteLogAddress("Ignition Function", _g->ignitionFunctionInstance, false);
            Memory::WriteLogAddress("Speed", _g->speedInstance, false);
            Memory::WriteLogAddress("Dyno", _g->dynoInstance, false);
            Memory::WriteLogAddress("AFR", _g->afrInstance, false);
            _g->debugShow = true;
        }

        //for some reason, the name address changes into a pointer to the name if the length is 16 or above
        //simple hacky workaround that works 99% of the time
        uintptr_t nameAddress = _g->engineInstance + 0x50;
        if (std::isalnum(*reinterpret_cast<char*>(nameAddress))) {
            engineUpdate->Name = reinterpret_cast<char*>(nameAddress);
        }
        else {
            engineUpdate->Name = *reinterpret_cast<char**>(nameAddress);
        }

        if (_g->quickShift && engineEdit->quickShiftAutoClutch) {
            *(double*)(_g->transmissionInstance + 0x368) = 0.0;
        }

        if (!engineEdit->loadCalibrationMode) {
            if (_g->autoBlip) {
                _g->overrideThrottle = true;
                setThrottle(engineEdit->autoBlipThrottle);
            }
            else {
                _g->overrideThrottle = false;
            }

            if ((engineEdit->twoStepLimiterMode == 3 && engineEdit->twoStepEnabled) || engineEdit->idleHelper) {
                _g->overrideThrottle = true;
            }
        }

        engineUpdate->tps = 1.00 - *(double*)(_g->engineInstance + 0x188);
    }

    if (!_g->quickShift && !engineEdit->loadCalibrationMode) {
        simFunctions->m_simProcess(a1, a2);
    }
}

double __fastcall sampleTriangleHk(__int64 a1, double a2) {
    if (_g->fullAttached) {
        if (a1 == _g->ignitionFunctionInstance) {
            double sample = simFunctions->m_sampleTriangle(a1, a2);
            double Offset = 0;

            if (_g->dfco) {
                engineUpdate->sparkAdvance = engineEdit->dfcoSpark;
                return engineEdit->dfcoSpark * units::deg;
            }

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
    }
    return simFunctions->m_sampleTriangle(a1, a2);
}

__int64 __fastcall rTachRenderHk(QWORD* a1, __int64 a2) {
    if (_g->fullAttached) {
        _g->speedInstance = *(QWORD*)(a1[14] + 0x528);
    }
    return simFunctions->m_rTachRender(a1, a2);
}

__int64 __fastcall changeGearHk(__int64 a1, signed int a2) {
    if (_g->fullAttached) {
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

__int64 __fastcall gasSystemResetHk(__int64 instance, double P, double T, __int64 mix) {
    if (_g->fullAttached) {
        if (_g->cutFuel) {
            Mix* mixPtr = reinterpret_cast<Mix*>(mix);
            mixPtr->p_fuel = 0;
            return simFunctions->m_gasSystemReset(instance, P, T, reinterpret_cast<__int64>(mixPtr));
        }

        if (engineEdit->useAfrTable) {
            Mix* afMix = reinterpret_cast<Mix*>(mix);
            double target_afr = 0.8 * (13.8 * (engineEdit->targetAfr / 14.7)) * 4;
            double p_air = target_afr / (1 + target_afr);
            afMix->p_fuel = 1 - p_air;
            afMix->p_inert = p_air * 0.75;
            afMix->p_o2 = p_air * 0.25;
            return simFunctions->m_gasSystemReset(instance, P, T, reinterpret_cast<__int64>(afMix));
        }
    }
    return simFunctions->m_gasSystemReset(instance, P, T, mix);
}

unsigned __int64* __fastcall showMainTextHk(void* Src, const void* a2, size_t a3) {
    if (_g->fullAttached) {
        const char* text = static_cast<const char*>(a2);
        const char* substring = " // ";
        char buffer[1024];

        if (strstr(text, substring)) {
            const char* newText = " // ES-Studio Connected";

            strncpy_s(buffer, sizeof(buffer), text, _TRUNCATE);
            strncat_s(buffer, sizeof(buffer), newText, _TRUNCATE);

            text = buffer;
            a3 = strlen(text);
        }
        return simFunctions->m_showMainText(Src, text, a3);
    }
    return simFunctions->m_showMainText(Src, a2, a3);
}

unsigned __int64* __fastcall updateHpAndTorqueHk(__int64 instance, float dt) {
    if (_g->fullAttached) {
        _g->dynoInstance = instance;
        engineUpdate->torque = *(double*)(instance + 0xA0);
        engineUpdate->power = *(double*)(instance + 0xA8);
    }
    return simFunctions->m_updateHpAndTorque(instance, dt);
}

__int64 __fastcall afrClusterRenderHk(__int64 a1) {
    if (_g->fullAttached) {
        _g->afrInstance = a1;
        QWORD LabeledGauge = *(QWORD*)(a1 + 0x78);
        QWORD Gauge = *(QWORD*)(LabeledGauge + 0x70);
        engineUpdate->afr = *(float*)(Gauge + 0x70);
    }
    return simFunctions->m_afrClusterRender(a1);
}

double __fastcall getCamAngleHk(void* instance, int index, __int64 a2, double theta) {
    if (!engineEdit->doubleCamSpeed || !_g->fullAttached) {
        return simFunctions->m_getCamAngle(instance, index, a2, theta);
    }

    __int64 baseAddress = *(QWORD*)((char*)instance + 0x50);
    double camAngleDelta = *(double*)(baseAddress + 0x28) - *(double*)(baseAddress + 0x60);

    double v4 = *(double*)(*(QWORD*)((char*)instance + 0x60) + 8 * index);
    double adjustedCamAngle = *(double*)((char*)instance + 0x68);

    double angle = fmod((camAngleDelta - adjustedCamAngle) * camSpeed, 2 * constants::pi);
    if (angle < 0.0) {
        angle += 2 * constants::pi;
    }

    double finalAngle = fmod(0.0 - (angle + v4), 2 * constants::pi);
    if (finalAngle < 0.0) {
        finalAngle += 2 * constants::pi;
    }
    if (finalAngle >= constants::pi) {
        finalAngle -= 2 * constants::pi;
    }

    __int64 sampleBaseAddress = *(QWORD*)((char*)instance + 0x58);
    return simFunctions->m_sampleTriangle(sampleBaseAddress, finalAngle);
}


double __fastcall getCamAngle2Hk(__int64 a1) {
    if (engineEdit->doubleCamSpeed && _g->fullAttached) {
        double camAngle = *(double*)(*(QWORD*)(a1 + 0x50) + 0x28) - *(double*)(*(QWORD*)(a1 + 0x50) + 0x60);

        double angle = fmod((camAngle - *(double*)(a1 + 0x68)) * camSpeed, 2 * constants::pi);
        return (angle < 0)
            ? angle + 2 * constants::pi
            : angle;
    }
    return simFunctions->m_getCamAngle2(a1);
}


#pragma endregion

void SetupHooks() {
    printf("------------------\r\nFunction Addresses\r\n------------------\r\n");

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

    uintptr_t gasSystemResetFunc = Memory::FindPatternIDA("F2 0F 59 49 ? 0F 28 C2 33 C0 F2 0F 59 05 ? ? ? ? F2 0F 5E C8 66 0F 6E 41 ? F3 0F E6 C0 F2 0F 11 09 F2 0F 59 05 ? ? ? ? F2");
    gasSystemResetPtr = (void*)gasSystemResetFunc;

    uintptr_t showMainTextFunc = Memory::FindPatternIDA("48 89 74 24 ? 57 48 83 EC 30 48 8B F9 49 8B F0 48 8B 49 10 4C 8B 47 18 49 8B C0 48 2B C1 48 3B F0 77 3F");
    showMainTextPtr = (void*)showMainTextFunc;

    uintptr_t updateHpAndTorqueFunc = Memory::FindPatternIDA("40 53 48 83 EC 40 48 8B D9 48 8B 89 ? ? ? ? 48 85 C9 0F 84 ? ? ? ? 48 8B 01 0F 29 74 24 ? 0F 29 7C 24 ? 0F 57 FF F3 0F 5A F9 0F 28 C7 F2 0F 58 05 ? ? ? ? F2");
    updateHpAndTorquePtr = (void*)updateHpAndTorqueFunc;

    uintptr_t afrClusterFunc = Memory::FindPatternIDA("48 8B C4 53 48 81 EC ? ? ? ? 0F 29 70 E8 48 8B D9 F3 0F 10 35 ? ? ? ? 0F 29 78 D8 0F 28 CE 44 0F 29 40 ? F3 44 0F 10 05 ? ? ? ? 44 0F 29 48 ? 41 0F 28 C0 44 0F 29 50");
    afrClusterPtr = (void*)afrClusterFunc;

    uintptr_t getCamAngleFunc = Memory::FindPatternIDA("40 53 48 83 EC 50 48 8B 41 60 48 8B D9 48 8B 49 50 48 63 D2 0F 29 74 24 ? 0F 29 7C 24 ? 44 0F 29 44 24 ? F2 44 0F 10 04 D0 E8 ? ? ? ? F2 0F 5C 43 ? F2 0F 10 35 ? ? ? ? 0F 28 CE");
    camAnglePtr = (void*)getCamAngleFunc;

    uintptr_t getCamAngle2Func = Memory::FindPatternIDA("40 53 48 83 EC 20 48 8B D9 48 8B 49 50 E8 ? ? ? ? F2 0F 5C 43 ? F2 0F 10 0D ? ? ? ? F2 0F 59 05 ? ? ? ? E8 ? ? ? ? 0F 57 C9 66 0F 2F C8 76 08 F2 0F 58 05 ? ? ? ? 48 83 C4 20 5B C3");
    camAngle2Ptr = (void*)getCamAngle2Func;

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
    Memory::WriteLogAddress("Gas System Reset", gasSystemResetFunc);
    Memory::WriteLogAddress("Show Main Text", showMainTextFunc);
    Memory::WriteLogAddress("Dyno Update", updateHpAndTorqueFunc);
    Memory::WriteLogAddress("AFR Render", afrClusterFunc);
    Memory::WriteLogAddress("Cam Angle", getCamAngleFunc);
    Memory::WriteLogAddress("Cam Angle 2", getCamAngle2Func);
    Memory::WriteLogAddress("Get Manifold Pressure", (uintptr_t)simFunctions->m_getManifoldPressure);
    Memory::WriteLogAddress("Get Cycle Angle", (uintptr_t)simFunctions->m_getCycleAngle);


    if (MH_CreateHook(ignitionModulePtr, &ignitionModuleHk, reinterpret_cast<LPVOID*>(&simFunctions->m_ignitionModuleUpdate)) != MH_OK) {
        printf("Unable to hook ignition module\n");
        return;
    }
    else {
        MH_EnableHook(ignitionModulePtr);
    }
    
    if (MH_CreateHook(sampleTrianglePtr, &sampleTriangleHk, reinterpret_cast<LPVOID*>(&simFunctions->m_sampleTriangle)) != MH_OK) {
        printf("Unable to hook sample triangle\n");
        return;
    }
    else {
        MH_EnableHook(sampleTrianglePtr);
    }

    if (MH_CreateHook(simProcessPtr, &simProcessHk, reinterpret_cast<LPVOID*>(&simFunctions->m_simProcess)) != MH_OK) {
        printf("Unable to hook main process\n");
        return;
    }
    else {
        MH_EnableHook(simProcessPtr);
    }

    if (MH_CreateHook(rTachRenderPtr, &rTachRenderHk, reinterpret_cast<LPVOID*>(&simFunctions->m_rTachRender)) != MH_OK) {
        printf("Unable to hook right tach render\n");
        return;
    }
    else {
        MH_EnableHook(rTachRenderPtr);
    }

    if (MH_CreateHook(changeGearPtr, &changeGearHk, reinterpret_cast<LPVOID*>(&simFunctions->m_changeGear)) != MH_OK) {
        printf("Unable to hook change gear\n");
        return;
    }
    else {
        MH_EnableHook(changeGearPtr);
    }

    if (MH_CreateHook(setThrottleRotaryPtr, &setThrottleRotaryHk, reinterpret_cast<LPVOID*>(&simFunctions->m_setThrottleRotary)) != MH_OK) {
        printf("Unable to hook throttle rotary\n");
        return;
    }
    else {
        MH_EnableHook(setThrottleRotaryPtr);
    }

    if (MH_CreateHook(setThrottlePistonPtr, &setThrottlePistonHk, reinterpret_cast<LPVOID*>(&simFunctions->m_setThrottlePiston)) != MH_OK) {
        printf("Unable to hook throttle piston\n");
        return;
    }
    else {
        MH_EnableHook(setThrottlePistonPtr);
    }

    if (MH_CreateHook(gasSystemResetPtr, &gasSystemResetHk, reinterpret_cast<LPVOID*>(&simFunctions->m_gasSystemReset)) != MH_OK) {
        printf("Unable to hook gas system reset\n");
        return;
    }
    else {
        MH_EnableHook(gasSystemResetPtr);
    }

    if (MH_CreateHook(showMainTextPtr, &showMainTextHk, reinterpret_cast<LPVOID*>(&simFunctions->m_showMainText)) != MH_OK) {
        printf("Unable to hook show main text\n");
        return;
    }
    else {
        MH_EnableHook(showMainTextPtr);
    }

    if (MH_CreateHook(updateHpAndTorquePtr, &updateHpAndTorqueHk, reinterpret_cast<LPVOID*>(&simFunctions->m_updateHpAndTorque)) != MH_OK) {
        printf("Unable to hook dyno update\n");
        return;
    }
    else {
        MH_EnableHook(updateHpAndTorquePtr);
    }

    if (MH_CreateHook(afrClusterPtr, &afrClusterRenderHk, reinterpret_cast<LPVOID*>(&simFunctions->m_afrClusterRender)) != MH_OK) {
        printf("Unable to hook afr render\n");
        return;
    }
    else {
        MH_EnableHook(afrClusterPtr);
    }

    if (MH_CreateHook(camAnglePtr, &getCamAngleHk, reinterpret_cast<LPVOID*>(&simFunctions->m_getCamAngle)) != MH_OK) {
        printf("Unable to hook get cam angle\n");
        return;
    }
    else {
        MH_EnableHook(camAnglePtr);
    }

    if (MH_CreateHook(camAngle2Ptr, &getCamAngle2Hk, reinterpret_cast<LPVOID*>(&simFunctions->m_getCamAngle2)) != MH_OK) {
        printf("Unable to hook get cam angle 2\n");
        return;
    }
    else {
        MH_EnableHook(camAngle2Ptr);
    }

    _g->fullAttached = true;
}

SimHooks::SimHooks(EngineUpdate* update, EngineEdit* edit, SimFunctions* functions, Globals* g) {
    engineUpdate = update;
    engineEdit = edit;
    simFunctions = functions;
    _g = g;
    customIgnition = new CustomIgnitionModule(update, edit, functions, g);
    loadCalculator = new LoadCalculator();
    SetupHooks();
}