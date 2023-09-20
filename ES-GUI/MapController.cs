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
            var acc = new Point(tablePoint.X + tablePoint.Width / 2, tablePoint.Y + tablePoint.Height / 2);

            if (!useExact)
            {
                var neighbors = cellList.Where(c => Math.Abs(c.Position.X - acc.X) <= c.Position.Width && Math.Abs(c.Position.Y - acc.Y) <= c.Position.Height).ToList();

                if (!neighbors.Any()) return 0;

                double weightedSum = 0;
                double totalWeights = 0;

                foreach (var neighbor in neighbors)
                {
                    var cellCenter = new Point(neighbor.Position.X + neighbor.Position.Width / 2, neighbor.Position.Y + neighbor.Position.Height / 2);
                    double distance = Math.Sqrt(Math.Pow(acc.X - cellCenter.X, 2) + Math.Pow(acc.Y - cellCenter.Y, 2));
                    double weight = 1 / (distance + 1);
                    weightedSum += weight * neighbor.Value;
                    totalWeights += weight;
                }

                return weightedSum / totalWeights;
            }
            else
            {
                return cellList.FirstOrDefault(c => c.Position.Contains(acc))?.Value ?? 0;
            }
        }

        public void UpdateTablePos()
        {
            tablePoint.X = (int)((xValue / maxX) * (cellRectangle.X - cellOffset.X) + cellOffset.X);
            tablePoint.Y = (int)((yValue / maxY) * (cellRectangle.Y - cellOffset.Y) + cellOffset.Y);
        }
    }
}
