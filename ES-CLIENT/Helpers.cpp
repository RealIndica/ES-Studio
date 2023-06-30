#include <string>
#include <cmath>
#include <algorithm>
#include <iterator>

#include "Helpers.h"
#include "RTTI.h"
#include "units.h"

static EngineUpdate* engineUpdate;
static EngineEdit* engineEdit;
static SimFunctions* simFunctions;
static Globals* _g;

void Helpers::UpdateEngineType() {
    if (_g->engineInstance) {
        if (_g->engineInstance != _g->engineOld) {
            _g->customIgnitionNeedsUpdate = true;
            void* vtableAddress = (void*)*(uintptr_t*)_g->engineInstance;
            uintptr_t* meta = static_cast<uintptr_t*>(vtableAddress) - 1;
            CompleteObjectLocator* COL = reinterpret_cast<CompleteObjectLocator*>(*meta);
            TypeDescriptor* descriptor = COL->GetTypeDescriptor();

            std::string name = &descriptor->name;
            if (name.find("Wankel") != std::string::npos) {
                _g->isRotary = true;
            }
            else {
                _g->isRotary = false;
            }
            _g->engineOld = _g->engineInstance;
        }
    }
}

void Helpers::buildSparkList() {
    if (_g->ignitionFunctionInstance != NULL) {
        std::vector<double> ret;
        double maxSparkRPM = ((int)(engineUpdate->maxRPM + 500 - 1) / 500) * 500;
        double increment = 500;
        for (int i = 0; i <= maxSparkRPM / increment; i++) {
            double curRPM = units::rpm(increment * i);
            double adv = std::ceil((simFunctions->m_sampleTriangle(_g->ignitionFunctionInstance, curRPM) / units::deg) * 10.) / 10.;
            ret.push_back(adv);
        }
        engineUpdate->sparkTimingList.clear();
        std::copy(ret.begin(), ret.end(), std::back_inserter(engineUpdate->sparkTimingList));
    }
}

Helpers::Helpers(EngineUpdate* update, EngineEdit* edit, SimFunctions* functions, Globals* g) {
    engineUpdate = update;
    engineEdit = edit;
    simFunctions = functions;
    _g = g;
}