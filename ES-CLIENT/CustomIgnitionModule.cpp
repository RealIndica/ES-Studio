#include <iostream>
#include <cmath>

#include "CustomIgnitionModule.h"
#include "MinHook.h"
#include "Memory.h"
#include "units.h"

static EngineUpdate* engineUpdate;
static EngineEdit* engineEdit;
static SimFunctions* simFunctions;
static Globals* _g;

double positiveMod(double x, double mod) {
	if (x < 0) {
		x = std::ceil(-x / mod) * mod + x;
	}

	return std::fmod(x, mod);
}

/*
 	const double cycleAngle = simFunctions->m_getCycleAngle(*(__int64*)(instance + 0x60), 0.0);

	bool m_enabled = *(bool*)(instance + 0x50);
	double m_revLimitTimer = *(double*)(instance + 0x90);
	int m_postCount = *(int*)(instance + 0x78);
	double crank_v_theta = *(double*)(*(QWORD*)(instance + 0x60) + 0x30);
	double m_lastCrankshaftAngle = *(double*)(instance + 0x80);
	double m_revLimit = *(double*)(instance + 0x88);
	double m_limiterDuration = *(double*)(instance + 0x98);

	Post* m_posts = *(Post**)(instance + 0x68);
	bool* m_ignitionEvents = *(bool**)(instance + 0x70);
*/

void QuickShiftLogic(double dt) {
	if (_g->quickShift) {
		_g->quickShiftTimer -= dt;
		if (_g->quickShiftTimer <= 0) {
			_g->quickShift = false;
			_g->quickShiftTimer = 0;
			if (_g->transmissionInstance != NULL) {
				if (engineEdit->quickShiftCutThenShift) {
					simFunctions->m_changeGear(_g->transmissionInstance, engineUpdate->gear + 1);
				}
			}
		}
	}
}

void AutoBlipLogic(double dt) {
	if (_g->autoBlip) {
		_g->autoBlipTimer -= dt;
		if (_g->autoBlipTimer <= 0) {
			_g->autoBlip = false;
			_g->autoBlipTimer = 0;
		}
	}
}

__int64 CustomIgnitionModule::ignitionProcess(__int64 instance, double dt) {
	const double cycleAngle = simFunctions->m_getCycleAngle(m_crankShaft, 0.0);
	bool m_enabled = *(bool*)(instance + 0x50);
	double crank_v_theta = *(double*)(m_crankShaft + 0x30);

	QuickShiftLogic(dt);
	AutoBlipLogic(dt);

	if (_g->quickShift && engineEdit->quickShiftMode == 0) {
		return m_crankShaft;
	}

	if (m_enabled && m_revLimitTimer == 0 && m_twoStepRevLimitTimer == 0 && m_speedLimiterTimer == 0) {
		const double cyclePeriod = *(double*)(m_crankShaft + 0xA0);
		const double advance = simFunctions->m_sampleTriangleMod(_g->ignitionFunctionInstance, -crank_v_theta);

		const double r0 = m_lastCrankshaftAngle;
		double r1 = cycleAngle;

		int postCount = m_postCount;

		if (_g->quickShift && engineEdit->quickShiftMode == 1 && engineEdit->dsgFarts) {
			postCount = 1;
		}

		for (int i = 0; i < postCount; ++i) {
			double adjustedAngle = positiveMod(m_posts[i].angle - advance, cyclePeriod);

			if (crank_v_theta < 0) {
				if (r1 < r0) {
					r1 += cyclePeriod;
					adjustedAngle += cyclePeriod;
				}

				if (adjustedAngle >= r0 && adjustedAngle < r1) {
					m_ignitionEvents[m_posts[i].target] |= m_posts[i].enabled;
				}
			}
			else {
				if (r1 > r0) {
					r1 -= cyclePeriod;
					adjustedAngle -= cyclePeriod;
				}

				if (adjustedAngle >= r1 && adjustedAngle < r0) {
					m_ignitionEvents[m_posts[i].target] |= m_posts[i].enabled;
				}
			}
		}
	}

	m_revLimitTimer -= dt;
	m_twoStepRevLimitTimer -= dt;
	if (!engineEdit->disableRevLimit) {
		if (std::fabs(crank_v_theta) > m_revLimit) {
			m_revLimitTimer = m_limiterDuration;
		}
	}

	m_speedLimiterTimer -= dt;
	if (engineEdit->speedLimiter && engineUpdate->gear != -1 && engineUpdate->clutchPosition != 0) {
		double maxSpeed = engineEdit->speedLimiterSpeed;
		if (engineEdit->speedLimiterMode == 0) {
			maxSpeed = units::convert(maxSpeed, units::mile, units::hour);
		}
		else {
			maxSpeed = units::convert(maxSpeed, units::km, units::hour);
		}

		if (round(engineUpdate->vehicleSpeed) >= round(maxSpeed)) {
			m_speedLimiterTimer = m_speedLimiterDuration;
		}
	}

	if (engineEdit->twoStepEnabled) {
		m_twoStepLimiterDuration = engineEdit->twoStepCutTime;
		if (engineEdit->twoStepLimiterMode == 0) {
			if (std::fabs(crank_v_theta) > units::rpm(engineEdit->rev2 - 200)) {
				m_twoStepRevLimitTimer = m_twoStepLimiterDuration;
			}
			else if (std::fabs(crank_v_theta) > units::rpm(engineEdit->rev1 - 200) && engineUpdate->clutchPosition < engineEdit->twoStepSwitchThreshold) {
				if (engineUpdate->gear == -1 || engineUpdate->gear == 0 || engineEdit->allowTwoStepInGear) {
					m_twoStepRevLimitTimer = m_twoStepLimiterDuration;
					engineUpdate->twoStepActive = true;
				}
				else {
					engineUpdate->twoStepActive = false;
				}
			}
			else {
				engineUpdate->twoStepActive = false;
			}
		}
		if (engineEdit->twoStepLimiterMode == 1) {
			if (std::fabs(crank_v_theta) > units::rpm(engineEdit->rev2)) {
				m_twoStepRevLimitTimer = m_twoStepLimiterDuration;
			}
			else if (std::fabs(crank_v_theta) > units::rpm(engineEdit->rev1) && engineUpdate->clutchPosition < engineEdit->twoStepSwitchThreshold) {
				if (engineUpdate->gear == -1 || engineUpdate->gear == 0 || engineEdit->allowTwoStepInGear) {
					m_twoStepRevLimitTimer = m_twoStepLimiterDuration;
					engineUpdate->twoStepActive = true;
				}
				else {
					engineUpdate->twoStepActive = false;
				}
			}
			else {
				engineUpdate->twoStepActive = false;
			}
		}
		if (engineEdit->twoStepLimiterMode == 2) {
			if (std::fabs(crank_v_theta) > units::rpm(engineEdit->rev2)) {
				_g->twoStepActive = true;
			}
			else if (std::fabs(crank_v_theta) > units::rpm(engineEdit->rev1) && engineUpdate->clutchPosition < engineEdit->twoStepSwitchThreshold) {
				if (engineUpdate->gear == -1 || engineUpdate->gear == 0 || engineEdit->allowTwoStepInGear) {
					_g->twoStepActive = true;
					engineUpdate->twoStepActive = true;
				}
				else {
					_g->twoStepActive = false;
					engineUpdate->twoStepActive = false;
				}
			}
			else {
				_g->twoStepActive = false;
				engineUpdate->twoStepActive = false;
			}
		}
	}

	if (m_revLimitTimer < 0) {
		m_revLimitTimer = 0;
	}

	if (m_twoStepRevLimitTimer < 0) {
		m_twoStepRevLimitTimer = 0;
	}

	if (m_speedLimiterTimer < 0) {
		m_speedLimiterTimer = 0;
	}

	if (m_revLimitTimer != 0 || m_twoStepRevLimitTimer != 0 || m_speedLimiterTimer || _g->twoStepActive) {
		engineUpdate->atLimiter = true;
	}
	else {
		engineUpdate->atLimiter = false;
	}

	m_lastCrankshaftAngle = cycleAngle;
	return m_crankShaft;
}

void CustomIgnitionModule::SetParameters() {
	QWORD instance = _g->ignitionInstance;
	m_crankShaft = *(QWORD*)(instance + 0x60);
	m_revLimitTimer = *(double*)(instance + 0x90);
	m_postCount = *(int*)(instance + 0x78);
	m_lastCrankshaftAngle = *(double*)(instance + 0x80);
	m_revLimit = *(double*)(instance + 0x88);
	m_limiterDuration = *(double*)(instance + 0x98);
	m_posts = *(Post**)(instance + 0x68);
	m_ignitionEvents = *(bool**)(instance + 0x70);
}

CustomIgnitionModule::CustomIgnitionModule(EngineUpdate* update, EngineEdit* edit, SimFunctions* functions, Globals* g) {
	m_postCount = 0;
	m_revLimit = 0;
	m_limiterDuration = 0;
	m_twoStepLimiterDuration = 0;
	m_speedLimiterDuration = 0.00001;
	m_posts = nullptr;
	m_ignitionEvents = nullptr;
	m_revLimitTimer = 0;
	m_twoStepRevLimitTimer = 0;
	m_speedLimiterTimer = 0;
	m_lastCrankshaftAngle = 0;
	m_crankShaft = 0;

	engineUpdate = update;
	engineEdit = edit;
	simFunctions = functions;
	_g = g;
}
