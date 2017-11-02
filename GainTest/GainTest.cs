using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using LinuxWithin.GainWatch;

namespace GainView
{
    public partial class GainTest : Form
    {
        public GainTest()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Gain gain = new Gain(false, textBox1.Text);
        }
    }
}