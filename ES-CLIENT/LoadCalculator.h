#pragma once
#include <cmath>
#include <algorithm>
#include <map>
#include <mutex>

class LoadCalculator
{
private:
    double peakAirflowAtWOT_STP(double rpm);

    double previousLoadPct;
    double previousAirflowEMA ;
    double maxAlpha;
    double minAlpha;
    double currentAlpha;
    double maxLoadChangePctPerCycle;
    double previousTPS;

    double scfmSum;
    int scfmSampleCount;

    std::map<double, double> rpmToSCFM;

public:
    LoadCalculator();
    void Calibrate(double rpm, double scfm);
    void ClearCalibration();
    std::map<double, double> getCalibrationTable();
    double SCFMtoMAF(double SCFM, double T_C);
    double calculateLoadPct(double currentAirflow, double BARO, double AAT, double rpm, double TPS);
    double calculateLoadPctForDiesel(double currentFuelFlow, double BARO, double AAT, double rpm, double TPS);
};

