#ifndef THROTTLE_CONTROLLER_H
#define THROTTLE_CONTROLLER_H

class ThrottleController {
public:
    double AdjustThrottle(double currentRPM, double inputThrottle, double maxRPM);
    double AdjustIdle(double currentRPM, double inputThrottle, double idleRPM, double maxTps);
};

#endif