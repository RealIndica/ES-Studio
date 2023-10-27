#include "LoadCalculator.h"
#include "units.h"

LoadCalculator::LoadCalculator() {
    previousLoadPct = 0.0;
    previousAirflowEMA = 0.0;
    maxAlpha = 0.05;
    minAlpha = 0.01;
    currentAlpha = maxAlpha;
    maxLoadChangePctPerCycle = 0.01;
    previousTPS = 0.0;

    scfmSum = 0.0;
    scfmSampleCount = 0;

    rpmToSCFM = { {1000, 100} };
}

void LoadCalculator::Calibrate(double rpm, double scfm) {
    if (rpm > 500 && scfm > 1) {
        auto lastElem = rpmToSCFM.rbegin();

        if (lastElem == rpmToSCFM.rend()) {
            printf("Calibrate (Start): Adding rpm: %f, scfm: %f\n", rpm, scfm);
            rpmToSCFM[rpm] = scfm;
            scfmSum = 0.0;
            scfmSampleCount = 0;
            return;
        }

        if (rpm - lastElem->first < 500) {
            scfmSum += scfm;
            scfmSampleCount++;
        }
        else if (rpm - lastElem->first >= 500) {
            double averageScfm = scfmSum / scfmSampleCount;

            printf("Calibrate: Adding rpm: %f, average scfm: %f\n", rpm, averageScfm);
            rpmToSCFM[rpm] = averageScfm;

            scfmSum = 0.0;
            scfmSampleCount = 0;
        }
    }
}

void LoadCalculator::ClearCalibration() {
    rpmToSCFM.clear();
}

std::map<double, double> LoadCalculator::getCalibrationTable() {
    return rpmToSCFM;
}

double LoadCalculator::peakAirflowAtWOT_STP(double rpm) {
    if (rpmToSCFM.empty()) {
        return SCFMtoMAF(100.0, 25.0);
    }

    auto it = rpmToSCFM.lower_bound(rpm);
    if (it != rpmToSCFM.begin() && (it == rpmToSCFM.end() || std::fabs(rpm - it->first) > std::fabs(rpm - (std::prev(it)->first)))) {
        --it;
    }

    double peakAirFlowSCFM = it->second;

    return SCFMtoMAF(peakAirFlowSCFM, 25.0);
}

double LoadCalculator::SCFMtoMAF(double SCFM, double T_C) {
    double T_K = units::kelvin(units::celcius(T_C));

    double AirDensity = (units::AirMolecularMass * units::pressure(1.0, units::atm)) /
        (constants::R * T_K);

    AirDensity = units::convert(AirDensity, units::kg / units::m3, units::g / units::cubic_feet);

    return SCFM * AirDensity / 60.0;
}

double LoadCalculator::calculateLoadPct(double currentAirflow, double BARO, double AAT, double rpm, double TPS) {
    const double STP_BARO = 29.92;
    const double STP_TEMP_C = 25.0;

    currentAirflow = currentAlpha * currentAirflow + (1 - currentAlpha) * previousAirflowEMA;
    previousAirflowEMA = currentAirflow;

    double load_pct = currentAirflow /
        (peakAirflowAtWOT_STP(rpm) *
            (BARO / STP_BARO) *
            std::sqrt(298.0 / (AAT + 273.0)));

    double change = load_pct - previousLoadPct;
    if (std::abs(change) > maxLoadChangePctPerCycle) {
        load_pct = previousLoadPct + std::copysign(maxLoadChangePctPerCycle, change);
    }

    if (TPS >= 0.995) {
        load_pct = 1.0;
    }

    if (TPS == 0) {
        load_pct = 0.0;
    }

    previousLoadPct = load_pct;

    return std::clamp(load_pct * 100, 0.0, 100.0);
}


double LoadCalculator::calculateLoadPctForDiesel(double currentFuelFlow, double BARO, double AAT, double rpm, double TPS) { // Maybe engine sim will add diesel one day
    return calculateLoadPct(currentFuelFlow, BARO, AAT, rpm, TPS);
}