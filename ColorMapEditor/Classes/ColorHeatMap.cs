using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ColorMapEditor
{
    public class ColorHeatMap
    {
        public ColorHeatMap()
        {
            initColorsBlocks();
        }
        public ColorHeatMap(string path)
        {
            initColorsBlocks(path);
        }
        public ColorHeatMap(string[] map)
        {
            initColorsBlocks(map);
        }
        public ColorHeatMap(byte alpha)
        {
            this.Alpha = alpha;
            initColorsBlocks();
        }
        private void initColorsBlocks(string path)
        {
            List<string> allLinesText = File.ReadAllLines(path).ToList();
            string[] separatingStrings = { "   ", " ", " " };

            foreach (string str in allLinesText)
            {
                string[] colors = str.Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries);
                int R = 0, G = 0, B = 0;
                try
                {
                    int.TryParse(colors[0], out R);
                    int.TryParse(colors[1], out G);
                    int.TryParse(colors[2], out B);

                    ColorsOfMap.Add(Color.FromArgb(Alpha, R, G, B));
                }
                catch
                { }
            }

        }

        public void initColorsBlocks(string[] map)
        {
            string[] separatingStrings = { "   ", " ", " " };

            foreach (string str in map)
            {
                string[] colors = str.Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries);
                int R = 0, G = 0, B = 0;
                try
                {
                    int.TryParse(colors[0], out R);
                    int.TryParse(colors[1], out G);
                    int.TryParse(colors[2], out B);

                    ColorsOfMap.Add(Color.FromArgb(Alpha, R, G, B));
                }
                catch
                { }
            }

        }
        private void initColorsBlocks()
        {
            ColorsOfMap.Add(Color.FromArgb(Alpha, 0x00, 0x00, 0xFF));
            ColorsOfMap.Add(Color.FromArgb(Alpha, 0x00, 0xFF, 0x00));
            ColorsOfMap.Add(Color.FromArgb(Alpha, 0xFF, 0x00, 0x00));

        }
        public Color GetColorForValue(double val, double maxVal, double minVal)
        {
            try
            {
                double valPerc = (val - minVal) / (maxVal - minVal);
                double colorPerc = 1d / (ColorsOfMap.Count - 1);// % of each block of color. the last is the "100% Color"
                double blockOfColor = valPerc / colorPerc;// the integer part repersents how many block to skip
                int blockIdx = (int)Math.Truncate(blockOfColor);// Idx of 
                double valPercResidual = valPerc - (blockIdx * colorPerc);//remove the part represented of block 
                double percOfColor = valPercResidual / colorPerc;// % of color of this block that will be filled

                Color cTarget = ColorsOfMap[blockIdx];
                Color cNext = val == maxVal ? ColorsOfMap[blockIdx] : ColorsOfMap[blockIdx + 1];
                var deltaR = cNext.R - cTarget.R;
                var deltaG = cNext.G - cTarget.G;
                var deltaB = cNext.B - cTarget.B;

                var R = cTarget.R + (deltaR * percOfColor);
                var G = cTarget.G + (deltaG * percOfColor);
                var B = cTarget.B + (deltaB * percOfColor);

                Color c = ColorsOfMap[0];
                try
                {
                    c = Color.FromArgb(Alpha, (byte)R, (byte)G, (byte)B);
                }
                catch
                {
                    Console.WriteLine("Failed to Map to ARGB");
                }
                return c;
            }
            catch
            {
                Color c = ColorsOfMap[0];
                return c;
            }
        }
        public byte Alpha = 0xff;
        public List<Color> ColorsOfMap = new List<Color>();
    }
}