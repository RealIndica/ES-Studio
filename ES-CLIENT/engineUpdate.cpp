#include "engineUpdate.h"

EngineUpdate::EngineUpdate() {
	Status = "Connected";
	Name = "";
	cylinderCount = 0;
	RPM = 0;
	maxRPM = 0;
	sparkAdvance = 0;
	tps = 0;
	vehicleSpeed = 0;
	sparkTimingList = {};
	manifoldPressure = 0;
	gear = -1;
	clutchPosition = 0;
	atLimiter = false;
	twoStepActive = false;
	engineLoad = 0;
	power = 0;
	torque = 0;
	airSCFM = 0;
	afr = 0;
	temperature = 0;
}