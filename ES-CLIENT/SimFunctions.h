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
	_gasSystemReset m_gasSystemReset;
	_showMainText m_showMainText;
	_updateHpAndTorque m_updateHpAndTorque;
	_afrClusterRender m_afrClusterRender;
	__getCamAngle m_getCamAngle;
	__getCamAngle2 m_getCamAngle2;
};

#endif