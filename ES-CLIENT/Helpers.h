#ifndef HELPERS_H
#define HELPERS_H

#include "engineUpdate.h"
#include "engineEdit.h"
#include "SimFunctions.h"
#include "Globals.h"

class Helpers {
public:
	Helpers(EngineUpdate* update, EngineEdit* edit, SimFunctions* functions, Globals* g);

	void UpdateEngineType();
	void buildSparkList();
};

#endif
