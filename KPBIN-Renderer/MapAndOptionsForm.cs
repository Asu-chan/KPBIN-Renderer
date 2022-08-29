using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KPBIN_Renderer
{
    public partial class MapAndOptionsForm : Form
    {
        public MapAndOptionsForm()
        {
            InitializeComponent();
        }

        public string tilesetsExportPath = "";
        public string doodadsExportPath = "";
        public string layersExportPath = "";
        public string worldmapExportPath = "";

        public string worldmapFilename = "";

        public ImageFormat imageFormat = ImageFormat.Png;


        public string mapsFolder = "";

        private void MapAndOptionsForm_Load(object sender, EventArgs e)
        {
            mapListBox.Items.Clear();

            string[] maps = Directory.GetFiles(mapsFolder, "*.kpbin.*");
            foreach(string map in maps)
            {
                if(map.EndsWith(".kpbin") || map.EndsWith(".kpbin.LZ") || map.EndsWith(".kpbin.LH"))
                    mapListBox.Items.Add(Path.GetFileName(map));
            }

            formatComboBox.SelectedIndex = 0;
        }

        private void mapListBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = (sender as ListBox).IndexFromPoint(e.Location);

            bool exportTilesets = tilesetsCheckBox.Checked;
            bool exportDoodads = doodadsCheckBox.Checked;
            bool exportLayers = layersCheckBox.Checked;
            bool exportWorldmap = worldmapCheckBox.Checked;

            if(!exportTilesets && !exportDoodads && !exportLayers && !exportWorldmap)
            {
                MessageBox.Show("If you don't want to export anything then there's no point.");
                return;
            }

            if (exportTilesets) tilesetsExportPath = Program.askForFolder("Select where do you want tilesets to be exported");
            if (exportDoodads) doodadsExportPath = Program.askForFolder("Select where do you want doodads to be exported");
            if (exportLayers) layersExportPath = Program.askForFolder("Select where do you want layers to be exported");
            if (exportWorldmap) worldmapExportPath = Program.askForFile("Select where do you want the worldmap render to be exported");

            worldmapFilename = mapListBox.Items[index].ToString();

            imageFormat = Program.extensions.FirstOrDefault(x => x.Value == formatComboBox.Items[formatComboBox.SelectedIndex].ToString()).Key;

            this.DialogResult = DialogResult.OK;
        }
    }
}
