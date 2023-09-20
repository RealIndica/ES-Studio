#ifndef THROTTLE_CONTROLLER_H
#define THROTTLE_CONTROLLER_H

class ThrottleController {
public:
    double AdjustThrottle(double currentRPM, double inputThrottle, double maxRPM);
};

#endif