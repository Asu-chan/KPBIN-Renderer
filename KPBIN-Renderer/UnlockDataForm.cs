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
    public partial class UnlockDataForm : Form
    {
        public UnlockDataForm()
        {
            InitializeComponent();
        }

        public int[][] udatas = new int[][]
        {
            new int[] { 1, 1, 0, 1 },
            new int[] { 1, 2, 0, 0 },
            new int[] { 1, 3, 1, 0, 1 },
            new int[] { 255, 0, 37 },
            new int[] { 255, 1, 42 }
        };

        string[] compNames = new string[] { "total star coins", "unspent star coins" };

        static readonly int rowLength = 25;

        private void UnlockDataForm_Load(object sender, EventArgs e)
        {
            //rowLength = (int)Math.Ceiling(udatas.Length / Math.Ceiling(udatas.Length / 12.0f));

            List<int> longestText = new List<int>();
            for (int dtID = 0; dtID < udatas.Length; dtID += rowLength)
                longestText.Add(0); 

            List<bool> hasSecret = new List<bool>();
            for (int dtID = 0; dtID < udatas.Length; dtID += rowLength)
                hasSecret.Add(false); 

            List<List<Control>> ctrl = new List<List<Control>>();
            for (int dtID = 0; dtID < udatas.Length; dtID += rowLength)
                ctrl.Add(new List<Control>()); ;

            int dataID = 0;
            for (dataID = 0; dataID < udatas.Length; dataID++)
            {
                int[] udata = udatas[dataID];
                int row = dataID / rowLength;
                //Console.WriteLine("Creating for row " + row + " line " + (dataID % rowLength));
                if (udata[0] == 255)
                {
                    string name = "FF" + udata[1].ToString("X2");

                    Label compLabel = nameLabel.Clone();
                    ctrl[row].Add(compLabel);
                    compLabel.Name = name + "Label";
                    compLabel.Text = compNames[udata[1]];
                    compLabel.Location = new Point(compLabel.Location.X, 9 + (26 * (dataID % rowLength)));

                    int labelLen = compLabel.MeasureSize();
                    if (labelLen > longestText[row]) longestText[row] = labelLen;


                    NumericUpDown compNumUpDown = comparisonNumUpDown.Clone();
                    ctrl[row].Add(compNumUpDown);
                    compNumUpDown.Name = name + "NumUpDown";
                    compNumUpDown.Value = udata[2];
                    compNumUpDown.Location = new Point(compNumUpDown.Location.X, 7 + (26 * (dataID % rowLength)));
                    compNumUpDown.ValueChanged += new System.EventHandler(this.comparisonNumUpDown_ValueChanged);
                }
                else
                {
                    string name = udata[0].ToString("X2") + udata[1].ToString("X2") + udata[2].ToString("X2") + udata[3].ToString("X2");
                    if (udata[2] == 2) name += udata[4].ToString("X2");

                    if (udata[2] != 0) hasSecret[row] = true;

                    Label levelLabel = nameLabel.Clone();
                    ctrl[row].Add(levelLabel);
                    levelLabel.Name = name + "Label";
                    levelLabel.Text = (udata[0] + 1).ToString("D2") + "-" + (udata[1] + 1).ToString("D2");
                    levelLabel.Location = new Point(levelLabel.Location.X, 9 + (26 * (dataID % rowLength)));

                    int labelLen = levelLabel.MeasureSize();
                    if (labelLen > longestText[row]) longestText[row] = labelLen;

                    if (udata[2] != 1)
                    {
                        CheckBox normalCB = normalCheckbox.Clone();
                        ctrl[row].Add(normalCB);
                        normalCB.Name = name + "NCheckBox";
                        normalCB.Location = new Point(normalCB.Location.X, 8 + (26 * (dataID % rowLength)));
                        normalCB.Checked = (udata[3] == 1);
                        normalCB.CheckedChanged += new System.EventHandler(this.normalCheckbox_CheckedChanged);
                    }

                    if (udata[2] != 0)
                    {
                        CheckBox secretCB = secretCheckbox.Clone();
                        ctrl[row].Add(secretCB);
                        secretCB.Name = name + "SCheckBox";
                        secretCB.Location = new Point(secretCB.Location.X, 8 + (26 * (dataID % rowLength)));
                        secretCB.Checked = (udata[((udata[2] == 2) ? 4 : 3)] == 1);
                        secretCB.CheckedChanged += new System.EventHandler(this.secretCheckbox_CheckedChanged);
                    }
                }
            }

            List<int> width = new List<int>();
            List<int> height = new List<int>();

            int rowCount = (int)Math.Ceiling(udatas.Length / (float)rowLength);
            //Console.WriteLine("Total rows: " + rowCount);

            for (int dtID = 0; dtID < udatas.Length; dtID += rowLength)
            {
                int row = dtID / rowLength;
                int lastRowLineCount = udatas.Length % rowLength;

                int currRowLineCount = (row == (rowCount - 1)) ? lastRowLineCount : rowLength;

                int padBack = 153 - longestText[row];
                width.Add(30 + 140 + longestText[row] - ((hasSecret[row]) ? 0 : 58));
                height.Add(30 + 39 + 6 + (26 * currRowLineCount));

                //Console.WriteLine("row " + row + " -> padBack: " + padBack + " width: " + width[row] + " height: " + height[row]);

                foreach (Control ct in ctrl[row])
                    ct.Location = new Point(ct.Location.X - padBack, ct.Location.Y);
            }

            int backWidth = width[0];
            for (int dtID = rowLength; dtID < udatas.Length; dtID += rowLength)
            {
                int row = dtID / rowLength;
                //Console.WriteLine("row " + row + " forward " + backWidth);

                foreach (Control ct in ctrl[row])
                    ct.Location = new Point(ct.Location.X + backWidth, ct.Location.Y);

                backWidth += width[row];
            }

            int totalWidth = 0;
            for (int dtID = 0; dtID < udatas.Length; dtID += rowLength)
            {
                int row = dtID / rowLength;
                totalWidth += width[row];
            }

            int totalHeight = height[0];


            okButton.Location = new Point(totalWidth / 2 - (okButton.Width / 2), 8 + (26 * ((udatas.Length > rowLength) ? rowLength : udatas.Length)));
            this.Size = new Size(totalWidth, totalHeight);

            nameLabel.Visible = false;
            normalCheckbox.Visible = false;
            secretCheckbox.Visible = false;
            comparisonNumUpDown.Visible = false;
        }

        private void normalCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            string name = (sender as CheckBox).Name;
            int world = Convert.ToInt32(name.Substring(0, 2), 16);
            int level = Convert.ToInt32(name.Substring(2, 2), 16);

            int value = (sender as CheckBox).Checked ? 1 : 0;

            for(int datsID = 0; datsID < udatas.Length; datsID++) 
                if(udatas[datsID][0] == world && udatas[datsID][1] == level)
                    udatas[datsID][3] = value;

            //Console.WriteLine("Value " + value + " for NValue " + world.ToString("D2") + "-" + level.ToString("D2"));
        }

        private void secretCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            string name = (sender as CheckBox).Name;
            int world = Convert.ToInt32(name.Substring(0, 2), 16);
            int level = Convert.ToInt32(name.Substring(2, 2), 16);

            int value = (sender as CheckBox).Checked ? 1 : 0;

            for (int datsID = 0; datsID < udatas.Length; datsID++)
                if (udatas[datsID][0] == world && udatas[datsID][1] == level)
                    udatas[datsID][((udatas[datsID][2] == 2) ? 4 : 3)] = value;

            //Console.WriteLine("Value " + value + " for SValue " + world.ToString("D2") + "-" + level.ToString("D2"));
        }

        private void comparisonNumUpDown_ValueChanged(object sender, EventArgs e)
        {
            string name = (sender as NumericUpDown).Name;
            int spByte = Convert.ToInt32(name.Substring(0, 2), 16);
            int specialID = Convert.ToInt32(name.Substring(2, 2), 16);

            int value = (int)((sender as NumericUpDown).Value);

            for (int datsID = 0; datsID < udatas.Length; datsID++)
                if (udatas[datsID][0] == spByte && udatas[datsID][1] == specialID)
                    udatas[datsID][2] = value;

            //Console.WriteLine("Value " + value + " for SPValue " + specialID);
        }
    }
    public static class ControlExtensions
    {
        public static T Clone<T>(this T controlToClone)
            where T : Control
        {
            PropertyInfo[] controlProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            T instance = Activator.CreateInstance<T>();

            foreach (PropertyInfo propInfo in controlProperties)
            {
                if (propInfo.CanWrite)
                {
                    if (propInfo.Name != "WindowTarget")
                        propInfo.SetValue(instance, propInfo.GetValue(controlToClone, null), null);
                }
            }

            return instance;
        }

        public static int MeasureSize(this Label lbl)
        {
            Bitmap bmp = new Bitmap(1, 1);
            SizeF size = new SizeF(0, 0);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                size = g.MeasureString(lbl.Text, lbl.Font, 495);
            }
            bmp.Dispose();

            return (int)Math.Ceiling(size.Width);
        }
    }
}
