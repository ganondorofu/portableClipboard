using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace portableClipboard
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            setUSBDevices();
            initCombobox();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedText = comboBox1.SelectedItem.ToString();
        }

        public void initCombobox() {
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.Items.AddRange(new string[]
        {
        "1",
        "2",
        "3",
        "4",
        "5"
        });
            comboBox1.SelectedIndex = 0;
        }

        private void setUSBDevices()
        {
            comboBox2.Items.Clear();

            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in drives)
            {
                if (drive.DriveType == DriveType.Removable && drive.IsReady)
                {
                    string volumeLabel = string.IsNullOrEmpty(drive.VolumeLabel) ? "（ラベルなし）" : drive.VolumeLabel;
                    string drivePath = drive.Name; // 例: "F:\"

                    // 表示は「ラベル (ドライブパス)」、取得はパスだけ
                    comboBox2.Items.Add(new ComboBoxItem
                    {
                        Label = volumeLabel,
                        Path = drivePath
                    });
                }
            }

            if (comboBox2.Items.Count == 0)
            {
                comboBox2.Items.Add("デバイスが見つかりませんでした");
                comboBox2.Enabled = false;
            }
            else
            { 
                comboBox2.Enabled = true;
            }

            comboBox2.SelectedIndex = 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string slot = comboBox1.Text;

            // SelectedItemをComboBoxItemにキャストしてPathを取得
            if (comboBox2.SelectedItem is ComboBoxItem selected)
            {
                string drive = selected.Path;
                textBox1.Text = SlotFileManager.readFile(drive, slot);
            }
            else
            {
                MessageBox.Show("有効なドライブを選択してください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void button3_Click(object sender, EventArgs e)
        {

        }

        // 表示と内部値を分けるためのクラス
        public class ComboBoxItem
        {
            public string Label { get; set; }  // ボリューム名
            public string Path { get; set; }   // ドライブパス（例: "F:\"）

            public override string ToString()
            {
                return $"{Label} ({Path})";
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            setUSBDevices();
        }
    }
}
