#include "ThrottleController.h"
#include <algorithm>

double ThrottleController::AdjustThrottle(double currentRPM, double inputThrottle, double maxRPM) {
    double adjustmentRange = maxRPM - (maxRPM * 0.9);
    double over90Percent = currentRPM - (maxRPM * 0.9);
    double throttleReductionFactor = 1 - (over90Percent / adjustmentRange);

    double adjustedThrottle = inputThrottle;

    if (currentRPM >= maxRPM * 0.9) {
        adjustedThrottle = inputThrottle * throttleReductionFactor;
    }

    return std::clamp(adjustedThrottle, 0.0, 1.0);
}

double ThrottleController::AdjustIdle(double currentRPM, double inputThrottle, double idleRPM, double maxTps) {
    if (currentRPM > idleRPM) {
        return inputThrottle;
    }

    double scale = 0.0008;
    double dampingFactor = 0.05;

    double rpmDifference = idleRPM - currentRPM;
    static double previousRPM = currentRPM;
    double rateOfChange = currentRPM - previousRPM;  
    double damping = rateOfChange * dampingFactor;

    double adjustmentFactor = (rpmDifference * scale) - damping;
    double adjustedThrottle = inputThrottle + adjustmentFactor;

    previousRPM = currentRPM;
    return std::clamp(adjustedThrottle, 0.0, maxTps);
}