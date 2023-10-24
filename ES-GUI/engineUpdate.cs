using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES_GUI
{
    public class engineUpdate
    {
        public string Status;
        public string Name;
        public int cylinderCount;
        public double RPM;
        public double maxRPM;
        public double sparkAdvance;
        public double tps;
        public double vehicleSpeed;
        public List<double> sparkTimingList;
        public double manifoldPressure;
        public int gear;
        public double clutchPosition;
        public bool atLimiter;
        public bool twoStepActive;
        public double engineLoad;
        public SortedDictionary<double, double> calibrationTable;
    }
}
