using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace portableClipboard
{
    public partial class Form1 : Form
    {
        private Dictionary<string, string> slotContents = new Dictionary<string, string>(); // 各スロットの内容を保持
        private string previousSlot = ""; // 直前のスロット

        public Form1()
        {
            InitializeComponent();
            setUSBDevices();
            initCombobox();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(previousSlot))
            {
                slotContents[previousSlot] = textBox1.Text;
            }

            string selectedSlot = comboBox1.Text;
            previousSlot = selectedSlot;

            if (!slotContents.ContainsKey(selectedSlot))
            {
                if (comboBox2.SelectedItem is ComboBoxItem selected)
                {
                    string drive = selected.Path;

                    try
                    {
                        string loadedText = SlotFileManager.readFile(drive, selectedSlot);
                        slotContents[selectedSlot] = loadedText;
                        textBox1.Text = loadedText;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Picoから読み込めませんでした。\n" + ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        textBox1.Text = "";
                    }
                }
                else
                {
                    MessageBox.Show("有効なドライブを選択してください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBox1.Text = "";
                }
            }
            else
            {
                textBox1.Text = slotContents[selectedSlot];
            }

            textBox1.Modified = false;
        }

        public void initCombobox()
        {
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;

            comboBox1.Items.AddRange(new string[] { "1", "2", "3", "4", "5" });

            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
                previousSlot = comboBox1.Text;
            }
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
                    string drivePath = drive.Name;

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

            if (comboBox2.SelectedItem is ComboBoxItem selected)
            {
                string drive = selected.Path;

                if (textBox1.Modified)
                {
                    DialogResult result = MessageBox.Show(
                        "現在の内容に変更があります。読み込み直すと上書きされます。\n続行しますか？",
                        "確認",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );

                    if (result != DialogResult.Yes)
                    {
                        return;
                    }
                }

                try
                {
                    string loadedText = SlotFileManager.readFile(drive, slot);
                    textBox1.Text = loadedText;
                    textBox1.Modified = false;
                    slotContents[slot] = loadedText;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Picoから読み込めませんでした。\n" + ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("有効なドライブを選択してください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (comboBox2.SelectedItem is ComboBoxItem selected)
            {
                string slot = comboBox1.Text;
                string drive = selected.Path;
                string content = textBox1.Text;

                SlotFileManager.writeFile(drive, slot, content);
                textBox1.Modified = false;             // 変更フラグをリセット
                slotContents[slot] = content;          // キャッシュも更新
            }
            else
            {
                MessageBox.Show("有効なドライブを選択してください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            setUSBDevices();
        }

        public class ComboBoxItem
        {
            public string Label { get; set; }
            public string Path { get; set; }

            public override string ToString()
            {
                return $"{Label} ({Path})";
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (comboBox2.SelectedItem is ComboBoxItem selected)
            {
                DialogResult result = MessageBox.Show(
                    "全スロットの内容を消去します。続行しますか？",
                    "確認",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    string drive = selected.Path;
                    SlotFileManager.deleteFile(drive);
                    slotContents.Clear();
                    string currentSlot = comboBox1.Text;
                    // 各スロットを再読み込み（存在しないなら空）
                    foreach (string slot in comboBox1.Items)
                    {
                        try
                        {
                            string loadedText = SlotFileManager.readFile(drive, slot);
                            slotContents[slot] = loadedText;
                        }
                        catch
                        {
                            // 読み込めなければ空にする
                            slotContents[slot] = "";
                        }
                    }

                    // 現在選択中のスロットに対応する内容を更新
                    if (slotContents.ContainsKey(currentSlot))
                    {
                        textBox1.Text = slotContents[currentSlot];
                    }
                    else
                    {
                        textBox1.Text = "";
                    }

                    textBox1.Modified = false;
            }
            }
            else
            {
                MessageBox.Show("有効なドライブを選択してください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
