#include "SimFunctions.h"

SimFunctions::SimFunctions() {
	m_ignitionModuleUpdate = 0;
	m_simProcess = 0;
	m_sampleTriangle = 0;
	m_sampleTriangleMod = 0;
	m_rTachRender = 0;
	m_changeGear = 0;
	m_getManifoldPressure = 0;
	m_setThrottlePiston = 0;
	m_setThrottleRotary = 0;
	m_getCycleAngle = 0;
	m_gasSystemReset = 0;
	m_showMainText = 0;
	m_updateHpAndTorque = 0;
	m_afrClusterRender = 0;
	m_getCamAngle = 0;
	m_getCamAngle2 = 0;
}