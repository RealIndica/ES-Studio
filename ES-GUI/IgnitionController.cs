using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES_GUI
{
    public class IgnitionController
    {
        List<SparkCell> sparkCellList = new List<SparkCell>();
        public Rectangle sparkTablePoint;
        public Rectangle cellRectangle;
        public Rectangle cellOffset;

        public int pressureLoadOffsetMax = 0;
        public int pressureLoadOffsetMin = 0;

        public string loadSource = "Manifold Pressure";

        public IgnitionController()
        {
            sparkTablePoint = new Rectangle(0, 0, 0, 0);
            cellRectangle = new Rectangle(0, 0, 0, 0);
            cellOffset = new Rectangle(0, 0, 0, 0);
        }

        public void CacheSparkData(List<SparkCell> sc)
        {
            sparkCellList.Clear();
            sparkCellList.AddRange(sc);
        }

        public double Pos2Spark()
        {
            Rectangle currentPos = sparkTablePoint;
            Point acc = new Point(currentPos.X + currentPos.Width / 2, currentPos.Y + currentPos.Height / 2);
            foreach (SparkCell c in sparkCellList)
            {
                if (c.Position.Contains(acc))
                {
                    return c.Advance;
                }
            }
            return 0;
        }

        public void UpdateSparkTablePos(ESClient client)
        {
            if (client.timingTable.Rows.Count > 0 && client.timingTable.Columns.Count > 0)
            {
                float percentRPM = (float)((client.update.RPM * 100) / client.update.maxRPM);
                float p = (percentRPM * (cellRectangle.X - cellOffset.X)) / 100;
                sparkTablePoint.X = (int)p + cellOffset.X;

                float maxLoad = 0;
                float percentLoad = 0;

                switch (loadSource)
                {
                    case "Manifold Pressure":
                        maxLoad = 102000;
                        maxLoad += (pressureLoadOffsetMax * -1) * maxLoad / 100;
                        percentLoad = Helpers.Clamp((float)((client.update.manifoldPressure * 100) / maxLoad), 0, 100);
                        break;
                    case "Throttle Position":
                        maxLoad = 100;
                        percentLoad = Helpers.Clamp((float)(((client.update.tps * 100) * 100) / maxLoad), 0, 100);
                        break;
                    default:
                        return;
                }

                int cellY = client.ignController.cellRectangle.Y - cellOffset.Y;

                float pLoad = (percentLoad * (cellY) / 100);
                sparkTablePoint.Y = (int)pLoad + cellOffset.Y;
            }
        }
    }
}
