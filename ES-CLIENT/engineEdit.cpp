#include "engineEdit.h"

EngineEdit::EngineEdit() {
	sparkAdvance = 0;
	customSpark = 0;
	useIgnTable = false;
	quickShiftEnabled = false;
	quickShiftTime = 0;
	quickShiftRetardTime = 0;
	quickShiftRetardDeg = 0;
	quickShiftMode = 0;
	quickShiftAutoClutch = false;
	quickShiftCutThenShift= false;
	autoBlipEnabled = false;
	autoBlipThrottle = 0;
	autoBlipTime = 0;
	dsgFarts = false;
	twoStepEnabled = false;
	disableRevLimit = false;
	rev1 = 0.0;
	rev2 = 0.0;
	useCustomIgnitionModule = false;
	twoStepLimiterMode = 0;
	twoStepCutTime = 0.0;
	twoStepRetardDeg = 0.0;
	twoStepSwitchThreshold = 1.0;
	allowTwoStepInGear = false;
	idleHelper = false;
	idleHelperRPM = 0.0;
	speedLimiter = false;
	speedLimiterSpeed = 186;
	speedLimiterMode = 0;
}