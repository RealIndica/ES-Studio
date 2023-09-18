using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES_GUI
{
    public class MapController
    {
        List<MapCell> cellList = new List<MapCell>();
        public Rectangle tablePoint;
        public Rectangle cellRectangle;
        public Rectangle cellOffset;

        public double maxX = 100;
        public double maxY = 100;

        public double xValue = 0;
        public double yValue = 0;

        public MapController()
        {
            tablePoint = new Rectangle(0, 0, 0, 0);
            cellRectangle = new Rectangle(0, 0, 0, 0);
            cellOffset = new Rectangle(0, 0, 0, 0);
        }

        public void CacheData(List<MapCell> sc)
        {
            cellList.Clear();
            cellList.AddRange(sc);
        }

        public double Pos2Val(bool useExact = false)
        {
            Rectangle currentPos = tablePoint;
            Point acc = new Point(currentPos.X + currentPos.Width / 2, currentPos.Y + currentPos.Height / 2);

            if (!useExact)
            {
                List<MapCell> neighbors = new List<MapCell>();

                foreach (MapCell c in cellList)
                {
                    if (Math.Abs(c.Position.X - acc.X) <= c.Position.Width && Math.Abs(c.Position.Y - acc.Y) <= c.Position.Height)
                    {
                        neighbors.Add(c);
                    }
                }

                if (neighbors.Count == 0)
                {
                    return 0;
                }

                double weightedSum = 0;
                double totalWeights = 0;

                foreach (var neighbor in neighbors)
                {
                    Point cellCenter = new Point(neighbor.Position.X + neighbor.Position.Width / 2, neighbor.Position.Y + neighbor.Position.Height / 2);
                    double distance = Math.Sqrt(Math.Pow(acc.X - cellCenter.X, 2) + Math.Pow(acc.Y - cellCenter.Y, 2));
                    double weight = 1 / (distance + 1);
                    weightedSum += weight * neighbor.Value;
                    totalWeights += weight;
                }

                double interpolatedValue = weightedSum / totalWeights;
                return interpolatedValue;
            } 
            else
            {
                foreach (MapCell c in cellList)
                {
                    if (c.Position.Contains(acc))
                    {
                        return c.Value;
                    }
                }
                return 0;
            }
        }

        public void UpdateTablePos()
        {
            float percentX = (float)((xValue * 100) / maxX);
            float p = (percentX * (cellRectangle.X - cellOffset.X)) / 100;
            tablePoint.X = (int)p + cellOffset.X;

            float percentY = (float)((yValue * 100) / maxY);
            float pY = (percentY * (cellRectangle.Y - cellOffset.Y) / 100);
            tablePoint.Y = (int)pY + cellOffset.Y;
        }
    }
}
