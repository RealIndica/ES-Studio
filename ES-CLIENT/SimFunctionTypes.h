#ifndef SIMFUNCTIONTYPES_H
#define SIMFUNCTIONTYPES_H

#include "Defs.h"
//TO DO: Update parameter names
typedef __int64(__fastcall* _ignitionModuleUpdate)(__int64 a1, double a2);
typedef void(_fastcall* _simProcess)(__int64 a1, float a2);
typedef double(_fastcall* _sampleTriangle)(__int64 a1, double a2);
typedef __int64(__fastcall* _rTachRender)(QWORD* a1, __int64 a2);
typedef __int64(__fastcall* _changeGear)(__int64 a1, signed int a2);
typedef double(__fastcall* _getManifoldPressure)(QWORD a1);
typedef __int64(__fastcall* _setThrottlePiston)(__int64 a1, double a2);
typedef __int64(__fastcall* _setThrottleRotary)(__int64 a1, double a2);
typedef double(__fastcall* _getCycleAngle)(__int64 a1, double a2);
typedef __int64(__fastcall* _gasSystemReset)(__int64 instance, double P, double T, __int64 mix);
typedef unsigned __int64*(__fastcall* _showMainText)(void* Src, const void* a2, size_t a3);

#endif