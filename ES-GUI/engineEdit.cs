using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ES_GUI
{
    public class engineEdit
    {
        public int sparkAdvance;
        public bool useIgnTable;
        public double customSpark;
        public bool useRpmTable;
        public double customRevLimit;
        public bool useCylinderTable;
        public bool useCylinderTableRandom;
        public int activeCylinderCount;
        public int activeCylindersRandomUpdateTime;
        public bool quickShiftEnabled;
        public double quickShiftTime;
        public double quickShiftRetardTime;
        public int quickShiftMode;
        public double quickShiftRetardDeg;
        public bool quickShiftAutoClutch;
        public bool quickShiftCutThenShift;
        public bool autoBlipEnabled;
        public double autoBlipThrottle;
        public double autoBlipTime;
        public bool dsgFarts;
        public bool twoStepEnabled;
        public bool disableRevLimit;
        public double rev1;
        public double rev2;
        public double rev3;
        public bool useCustomIgnitionModule;
        public int twoStepLimiterMode;
        public double twoStepCutTime;
        public double twoStepRetardDeg;
        public double twoStepSwitchThreshold;
        public bool allowTwoStepInGear;
        public bool idleHelper;
        public double idleHelperRPM;
        public double idleHelperMaxTps;
        public bool speedLimiter;
        public double speedLimiterSpeed;
        public int speedLimiterMode;
        public bool useAfrTable;
        public double targetAfr;
        public bool loadCalibrationMode;
        public bool doubleCamSpeed;
        public bool dfcoEnabled;
        public double dfcoExitRPM;
        public double dfcoSpark;
        public double dfcoEnterDelay;
    }
}
