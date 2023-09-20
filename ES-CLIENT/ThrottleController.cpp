#include "ThrottleController.h"

double ThrottleController::AdjustThrottle(double currentRPM, double inputThrottle, double maxRPM) {
    double adjustmentRange = maxRPM - maxRPM * 0.9;
    double over90Percent = currentRPM - maxRPM * 0.9;
    double throttleReductionFactor = 1 - (over90Percent / adjustmentRange);

    double adjustedThrottle = inputThrottle;

    if (currentRPM >= maxRPM * 0.9) {
        adjustedThrottle = inputThrottle * throttleReductionFactor;
    }

    if (adjustedThrottle < 0) {
        return 0;
    }
    else if (adjustedThrottle > 1) {
        return 1;
    }
    else {
        return adjustedThrottle;
    }
}