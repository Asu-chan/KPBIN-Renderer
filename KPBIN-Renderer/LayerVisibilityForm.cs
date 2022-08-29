using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KPBIN_Renderer
{
    public partial class LayerVisibilityForm : Form
    {
        public LayerVisibilityForm()
        {
            InitializeComponent();
        }

        public int[][] layerVisibilities = new int[][]
        {
            new int[] {0, 1},
            new int[] {1, 0},
            new int[] {2, 1},
            new int[] {0, 0},
            new int[] {0, 0}
        };

        string[] layerNames = new string[] { "OBJECTS", "DOODADS", "PATHS" };

        static readonly int rowLength = 30;

        private void LayerVisibilityForm_Load(object sender, EventArgs e)
        {
            List<int> longestText = new List<int>();
            for (int dtID = 0; dtID < layerVisibilities.Length; dtID += rowLength)
                longestText.Add(0);

            List<bool> hasSecret = new List<bool>();
            for (int dtID = 0; dtID < layerVisibilities.Length; dtID += rowLength)
                hasSecret.Add(false);

            List<List<Control>> ctrl = new List<List<Control>>();
            for (int dtID = 0; dtID < layerVisibilities.Length; dtID += rowLength)
                ctrl.Add(new List<Control>()); ;

            int dataID = 0;
            for (dataID = 0; dataID < layerVisibilities.Length; dataID++)
            {
                int layerType = layerVisibilities[dataID][0];
                int visibility = layerVisibilities[dataID][1];
                int row = dataID / rowLength;

                string name = ((uint)(dataID)).ToString("X8");

                Label levelLabel = nameLabel.Clone();
                ctrl[row].Add(levelLabel);
                levelLabel.Name = name + "Label";
                if(layerType > -1)
                    levelLabel.Text = "Layer " + (dataID + 1) + " (" + layerNames[layerVisibilities[dataID][0]] + ")";
                else
                    levelLabel.Text = "Path lines && misc. nodes";
                levelLabel.Location = new Point(levelLabel.Location.X, 9 + (26 * (dataID % rowLength)));

                int labelLen = levelLabel.MeasureSize();
                if (labelLen > longestText[row]) longestText[row] = labelLen;

                CheckBox visibleCB = visibleCheckbox.Clone();
                ctrl[row].Add(visibleCB);
                visibleCB.Name = name + "VCheckBox";
                visibleCB.Location = new Point(visibleCB.Location.X, 8 + (26 * (dataID % rowLength)));
                visibleCB.Checked = (visibility > 0);
                visibleCB.CheckedChanged += new System.EventHandler(this.visibleCheckbox_CheckedChanged);
            }

            List<int> width = new List<int>();
            List<int> height = new List<int>();

            int rowCount = (int)Math.Ceiling(layerVisibilities.Length / (float)rowLength);
            Console.WriteLine("Total rows: " + rowCount);

            for (int dtID = 0; dtID < layerVisibilities.Length; dtID += rowLength)
            {
                int row = dtID / rowLength;
                int lastRowLineCount = layerVisibilities.Length % rowLength;

                int currRowLineCount = (row == (rowCount - 1)) ? lastRowLineCount : rowLength;

                int padBack = 153 - longestText[row];
                width.Add(30 + 82 + longestText[row]);
                height.Add(30 + 39 + 6 + (26 * currRowLineCount));

                //Console.WriteLine("row " + row + " -> padBack: " + padBack + " width: " + width[row] + " height: " + height[row]);

                foreach (Control ct in ctrl[row])
                    ct.Location = new Point(ct.Location.X - padBack, ct.Location.Y);
            }

            int backWidth = width[0];
            for (int dtID = rowLength; dtID < layerVisibilities.Length; dtID += rowLength)
            {
                int row = dtID / rowLength;
                //Console.WriteLine("row " + row + " forward " + backWidth);

                foreach (Control ct in ctrl[row])
                    ct.Location = new Point(ct.Location.X + backWidth, ct.Location.Y);

                backWidth += width[row];
            }

            int totalWidth = 0;
            for (int dtID = 0; dtID < layerVisibilities.Length; dtID += rowLength)
            {
                int row = dtID / rowLength;
                totalWidth += width[row];
            }

            int totalHeight = height[0];


            okButton.Location = new Point(totalWidth / 2 - (okButton.Width / 2), 8 + (26 * ((layerVisibilities.Length > rowLength) ? rowLength : layerVisibilities.Length)));
            this.Size = new Size(totalWidth, totalHeight);

            nameLabel.Visible = false;
            visibleCheckbox.Visible = false;
        }

        private void visibleCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            string name = (sender as CheckBox).Name;
            int dataID = (int)Convert.ToUInt32(name.Substring(0, 8), 16);

            int value = (sender as CheckBox).Checked ? 1 : 0;

            layerVisibilities[dataID][1] = value;

            Console.WriteLine("Value " + value + " for VValue " + dataID.ToString("D2"));
        }
    }
}
