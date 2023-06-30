#ifndef SIMFUNCTIONS_H
#define SIMFUNCTIONS_H
#include "SimFunctionTypes.h"

class SimFunctions {
public:
	SimFunctions();

	_ignitionModuleUpdate m_ignitionModuleUpdate;
	_simProcess m_simProcess;
	_sampleTriangle m_sampleTriangle;
	_sampleTriangle m_sampleTriangleMod;
	_rTachRender m_rTachRender;
	_changeGear m_changeGear;
	_getManifoldPressure m_getManifoldPressure;
	_setThrottlePiston m_setThrottlePiston;
	_setThrottleRotary m_setThrottleRotary;
	_getCycleAngle m_getCycleAngle;
};

#endif