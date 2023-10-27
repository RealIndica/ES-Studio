#include "Globals.h"
Globals::Globals() {
	appInstance = 0;
	simulatorInstance = 0;
	ignitionInstance = 0;
	ignitionFunctionInstance = 0;
	speedInstance = 0;
	engineInstance = 0;
	transmissionInstance = 0;
	engineOld = 0;
	quickShift = false;
	quickShiftTimer = 0;
	autoBlip = false;
	autoBlipTimer = 0;
	isRotary = false;
	calibrationCleared = false;
	calibrationFinished = false;
	customIgnitionNeedsUpdate = false;
	twoStepActive = false;
	cleanTps = 0;
	fuelCutTps = 0;
	overrideThrottle = false;
	cutFuel = false;
	fullAttached = false;
	debugShow = false;
}