#ifndef ENGINEUPDATE_H
#define ENGINEUPDATE_H

#include <vector>
#include <map>

class EngineUpdate {
public:
	EngineUpdate();

	const char* Status;
	const char* Name;
	int cylinderCount;
	double RPM;
	double maxRPM;
	double sparkAdvance;
	double tps;
	double vehicleSpeed;
	std::vector<double> sparkTimingList;
	double manifoldPressure;
	int gear;
	double clutchPosition;
	bool atLimiter;
	bool twoStepActive;
	double engineLoad;
	double power;
	double torque;
	double airSCFM;
	double afr;
	double temperature;
};

#endif