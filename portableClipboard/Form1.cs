using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace portableClipboard
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            initCombobox();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedText = comboBox1.SelectedItem.ToString();
        }

        public void initCombobox() {
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
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
    }
}
