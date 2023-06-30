#ifndef ENGINEEDIT_H
#define ENGINEEDIT_H

class EngineEdit {
public:
	EngineEdit();

	int sparkAdvance;
	double customSpark;
	bool useIgnTable;
	bool quickShiftEnabled;
	double quickShiftTime;
	double quickShiftRetardTime;
	double quickShiftRetardDeg;
	int quickShiftMode;
	bool quickShiftAutoClutch;
	bool quickShiftCutThenShift;
	bool autoBlipEnabled;
	double autoBlipThrottle;
	double autoBlipTime;
	bool dsgFarts;
	bool twoStepEnabled;
	bool disableRevLimit;
	double rev1;
	double rev2;
	bool useCustomIgnitionModule;
	int twoStepLimiterMode;
	double twoStepCutTime;
	double twoStepRetardDeg;
	double twoStepSwitchThreshold;
	bool allowTwoStepInGear;
	bool idleHelper;
	double idleHelperRPM;
	bool speedLimiter;
	double speedLimiterSpeed;
	int speedLimiterMode;
};

#endif