#ifndef SIMHOOKS_H
#define SIMHOOKS_H

#include "engineUpdate.h"
#include "engineEdit.h"
#include "SimFunctions.h"
#include "Globals.h"

class SimHooks {
public:
	SimHooks(EngineUpdate* update, EngineEdit* edit, SimFunctions* functions, Globals* g);
};

#endif