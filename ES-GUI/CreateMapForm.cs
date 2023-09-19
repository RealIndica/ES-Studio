using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ES_GUI
{
    public partial class CreateMapForm : Form
    {
        private Map newMap;
        private List<Map> existingMaps;

        public bool CreateForm = false;

        public CreateMapForm(Map newMap, List<Map> existingMaps)
        {
            InitializeComponent();
            this.newMap = newMap;
            this.existingMaps = existingMaps;
        }

        private void CreateMapForm_Load(object sender, EventArgs e)
        {
            yAxis.Items.AddRange(Enum.GetNames(typeof(MapParam)));
            xAxis.Items.AddRange(Enum.GetNames(typeof(MapParam)));
            outputControl.Items.AddRange(Enum.GetNames(typeof(MapControlParam)));
            yAxis.SelectedIndex = 0;
            xAxis.SelectedIndex = 0;
            outputControl.SelectedIndex = 0;
        }

        private void createMapButton_Click(object sender, EventArgs e)
        {
            if (yAxis.SelectedIndex == xAxis.SelectedIndex)
            {
                MessageBox.Show("Y and X axis cannot have the same parameter", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (xAxis.SelectedIndex == 0)
            {
                MessageBox.Show("X axis cannot be none", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (outputControl.SelectedIndex == 0)
            {
                MessageBox.Show("You need to select an output control", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (nameTextBox.Text.Length == 0)
            {
                MessageBox.Show("You need to enter a name for your map", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MapControlParam thisControlParam = (MapControlParam)outputControl.SelectedIndex;
            foreach (Map m in existingMaps)
            {
                if (thisControlParam == m.controlParam)
                {
                    MessageBox.Show("The map \"" + m.name + "\" is already using output control \"" + thisControlParam.ToString() + "\"");
                    return;
                }
                if (nameTextBox.Text.ToLower() == m.name.ToLower())
                {
                    MessageBox.Show("The map \"" + m.name + "\" is already using this name");
                    return;
                }

                if (thisControlParam == MapControlParam.ActiveCylinders || thisControlParam == MapControlParam.ActiveCylindersRandom)
                {
                    if (m.controlParam == MapControlParam.ActiveCylinders || m.controlParam == MapControlParam.ActiveCylindersRandom) 
                    {
                        MessageBox.Show("The map \"" + m.name + "\" is already using output control \"" + MapControlParam.ActiveCylinders + "\" or \"" + MapControlParam.ActiveCylindersRandom + "\"");
                        return;
                    }
                }
            }

            newMap.Configure((MapParam)xAxis.SelectedIndex, (MapParam)yAxis.SelectedIndex, (MapControlParam)outputControl.SelectedIndex, nameTextBox.Text);
            CreateForm = true;
            this.Close();
        }
    }
}
