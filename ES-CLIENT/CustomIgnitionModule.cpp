#include <iostream>
#include <cmath>
#include <algorithm>
#include <random>
#include <numeric>
#include <set>

#include "CustomIgnitionModule.h"
#include "MinHook.h"
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

	if (m_enabled && m_revLimitTimer == 0 && m_twoStepRevLimitTimer == 0 && m_speedLimiterTimer == 0 && m_hiLoNextRPM == 0) {
		const double cyclePeriod = *(double*)(m_crankShaft + 0xA0);
		const double advance = simFunctions->m_sampleTriangleMod(_g->ignitionFunctionInstance, -crank_v_theta);

		const double r0 = m_lastCrankshaftAngle;
		double r1 = cycleAngle;

		int postCount = m_postCount;

		if (engineEdit->useCylinderTable) {
			postCount = std::clamp(engineEdit->activeCylinderCount, 0, engineUpdate->cylinderCount);
		}
		else if (engineEdit->useCylinderTableRandom) {
			double cycleAngleR = std::round(cycleAngle * 1.0) / 1.0;
			double cyclePeriodR = std::round(cyclePeriod * 1.0) / 1.0;

			m_cycleTop = std::clamp(engineEdit->activeCylindersRandomUpdateTime, 1, INT32_MAX);

			if (cycleAngleR == cyclePeriodR && m_cycleCounter % m_cycleTop == 0) {
				int cutCount = m_postCount - engineEdit->activeCylinderCount;
				m_cutPosts.clear();
				std::set<int> uniqueCuts;
				std::random_device rd;
				std::default_random_engine generator(rd());
				std::uniform_int_distribution<int> distribution(0, m_postCount - 1);
				while (uniqueCuts.size() < cutCount) {
					uniqueCuts.insert(distribution(generator));
				}
				m_cutPosts.assign(uniqueCuts.begin(), uniqueCuts.end());
			}

			if (cycleAngleR == cyclePeriodR) {
				m_cycleCounter++;
				if (m_cycleCounter >= m_cycleTop) {
					m_cycleCounter = 0;
				}
			}
		}

		if (_g->quickShift && engineEdit->quickShiftMode == 1 && engineEdit->dsgFarts) {
			postCount = 1;
		}

		for (int i = 0; i < postCount; ++i) {
			if (std::find(m_cutPosts.begin(), m_cutPosts.end(), i) != m_cutPosts.end()) {
				continue;
			};

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
		if (engineEdit->useRpmTable) {
			if (std::fabs(crank_v_theta) > units::rpm(engineEdit->customRevLimit)) {
				m_revLimitTimer = m_limiterDuration;
			}
		}
		else if (std::fabs(crank_v_theta) > m_revLimit) {
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

		double absCrank = std::fabs(crank_v_theta);
		bool clutchThresholdMet = engineUpdate->clutchPosition < engineEdit->twoStepSwitchThreshold;
		bool isNeutralOrAllowedGear = engineUpdate->gear == -1 || engineUpdate->gear == 0 || engineEdit->allowTwoStepInGear;

		switch (engineEdit->twoStepLimiterMode) {
		case 0: // Soft Cut
			if ((engineEdit->useRpmTable && absCrank > units::rpm(engineEdit->customRevLimit - 200)) ||
				(!engineEdit->useRpmTable && absCrank > units::rpm(engineEdit->rev2 - 200))) {
				m_twoStepRevLimitTimer = m_twoStepLimiterDuration;
			}
			if (absCrank > units::rpm(engineEdit->rev1 - 200) && clutchThresholdMet && isNeutralOrAllowedGear) {
				m_twoStepRevLimitTimer = m_twoStepLimiterDuration;
				engineUpdate->twoStepActive = true;
			}
			else {
				engineUpdate->twoStepActive = false;
			}
			break;

		case 1: // Hard Cut
			if ((engineEdit->useRpmTable && absCrank > units::rpm(engineEdit->customRevLimit)) ||
				(!engineEdit->useRpmTable && absCrank > units::rpm(engineEdit->rev2))) {
				m_twoStepRevLimitTimer = m_twoStepLimiterDuration;
			}
			if (absCrank > units::rpm(engineEdit->rev1) && clutchThresholdMet && isNeutralOrAllowedGear) {
				m_twoStepRevLimitTimer = m_twoStepLimiterDuration;
				engineUpdate->twoStepActive = true;
			}
			else {
				engineUpdate->twoStepActive = false;
			}
			break;

		case 2: // Retard
			if ((engineEdit->useRpmTable && absCrank > units::rpm(engineEdit->customRevLimit))
				|| (!engineEdit->useRpmTable && absCrank > units::rpm(engineEdit->rev2))) {
				_g->twoStepActive = true;
			}
			else if (absCrank > units::rpm(engineEdit->rev1) && clutchThresholdMet) {
				if (isNeutralOrAllowedGear) {
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
			break;

		case 3: // Hi Lo
			if (engineEdit->useRpmTable && absCrank > units::rpm(engineEdit->customRevLimit)) {
				m_hiLoNextRPM = engineEdit->customRevLimit - engineEdit->rev3;
			}
			else if (!engineEdit->useRpmTable && absCrank > units::rpm(engineEdit->rev2)) {
				m_hiLoNextRPM = engineEdit->rev2 - engineEdit->rev3;
			}
			if (absCrank > units::rpm(engineEdit->rev1) && clutchThresholdMet && isNeutralOrAllowedGear) {
				m_hiLoNextRPM = engineEdit->rev1 - engineEdit->rev3;
				engineUpdate->twoStepActive = true;
			}
			else {
				if (engineUpdate->clutchPosition >= engineEdit->twoStepSwitchThreshold && engineUpdate->twoStepActive) {
					m_hiLoNextRPM = 0;
				}
				engineUpdate->twoStepActive = false;
			}
			break;

		case 4: // Throttle Cut
			double maxRPM = engineEdit->rev2;
			if (engineEdit->useRpmTable) {
				maxRPM = engineEdit->customRevLimit;
			}
			if (clutchThresholdMet && isNeutralOrAllowedGear) {
				maxRPM = engineEdit->rev1;
				engineUpdate->twoStepActive = true;
			}
			else {
				engineUpdate->twoStepActive = false;
			}

			if (!_g->autoBlip && _g->cleanTps > 0.1) {
				_g->fuelCutTps = m_throttleController->AdjustThrottle(units::toRpm(absCrank), _g->cleanTps, maxRPM);
				if (_g->isRotary) {
					simFunctions->m_setThrottleRotary(_g->engineInstance, _g->fuelCutTps);
				}
				else {
					simFunctions->m_setThrottlePiston(_g->engineInstance, _g->fuelCutTps);
				}
			}
			break;
		}
	}

	if (engineEdit->idleHelper && _g->cleanTps < 0.1) {
		double idleTps = m_throttleController->AdjustIdle(units::toRpm(std::fabs(crank_v_theta)), _g->cleanTps, engineEdit->idleHelperRPM, engineEdit->idleHelperMaxTps);
		if (_g->isRotary) {
			simFunctions->m_setThrottleRotary(_g->engineInstance, idleTps);
		}
		else {
			simFunctions->m_setThrottlePiston(_g->engineInstance, idleTps);
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

	if (std::fabs(crank_v_theta) < units::rpm(m_hiLoNextRPM) || m_hiLoNextRPM < 0) {
		m_hiLoNextRPM = 0;
	}

	if (m_revLimitTimer != 0 || m_twoStepRevLimitTimer != 0 || m_speedLimiterTimer || _g->twoStepActive || m_hiLoNextRPM != 0) {
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
	m_hiLoNextRPM = 0;
	m_cutPosts = {};
	m_cycleCounter = 0;
	m_cycleTop = 1;
	m_posts = nullptr;
	m_ignitionEvents = nullptr;
	m_revLimitTimer = 0;
	m_twoStepRevLimitTimer = 0;
	m_speedLimiterTimer = 0;
	m_lastCrankshaftAngle = 0;
	m_crankShaft = 0;

	m_throttleController = new ThrottleController();

	engineUpdate = update;
	engineEdit = edit;
	simFunctions = functions;
	_g = g;
}
