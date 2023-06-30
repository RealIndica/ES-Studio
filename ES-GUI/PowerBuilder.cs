using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES_GUI
{

    public class PowerBuilder
    {
        public int Rev1;
        public int Rev2;
        public int Gain;

        private ESClient client;

        public PowerBuilder(ESClient cl) 
        {
            Rev1 = 0;
            Rev2 = 0;
            Gain = 0;
            client = cl;
        }

        private double intToRev(int rev)
        {
            int bottom = 2500;
            int incr = 500;

            if (rev == 0)
            {
                return bottom;
            }

            return bottom + (incr * rev);
        }

        private double intToCutTime(int gain)
        {
            double bottom = 0.2;
            double incr = 0.012;
            if (gain == 0)
            {
                return bottom;
            }

            return bottom - (incr * gain);
        }

        public void update()
        {
            client.edit.rev1 = intToRev(Rev2);
            client.edit.rev2 = intToRev(Rev1);
            client.edit.twoStepCutTime = intToCutTime(Gain);
            client.edit.twoStepLimiterMode = 1;
        }

    }
}
