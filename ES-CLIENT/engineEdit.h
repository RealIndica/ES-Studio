#ifndef ENGINEEDIT_H
#define ENGINEEDIT_H

class EngineEdit {
public:
	EngineEdit();

	int sparkAdvance;
	bool useIgnTable;
	double customSpark;
	bool useRpmTable;
	double customRevLimit;
	bool useCylinderTable;
	bool useCylinderTableRandom;
	int activeCylinderCount;
	int activeCylindersRandomUpdateTime;
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
	double rev3;
	bool useCustomIgnitionModule;
	int twoStepLimiterMode;
	double twoStepCutTime;
	double twoStepRetardDeg;
	double twoStepSwitchThreshold;
	bool allowTwoStepInGear;
	bool idleHelper;
	double idleHelperRPM;
	double idleHelperMaxTps;
	bool speedLimiter;
	double speedLimiterSpeed;
	int speedLimiterMode;
	bool useAfrTable;
	double targetAfr;
	bool loadCalibrationMode;
	bool doubleCamSpeed;
	bool dfcoEnabled;
	double dfcoExitRPM;
	double dfcoSpark;
	double dfcoEnterDelay;
};

#endif