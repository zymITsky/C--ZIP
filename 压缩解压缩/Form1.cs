using System;
using System.IO;
using System.Windows.Forms;

namespace 压缩解压缩 {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e) {
            if (this.radioButton1.Checked) {
                String path = this.textBox1.Text;
                if (!String.IsNullOrEmpty(path)) {
                    if (Directory.Exists(path)) {
                        try {
                            ZipHelper.ZipDirectory(path);
                        }
                        catch (Exception e1) {
                            this.label2.Text = (e1.Message.ToString());
                        }
                    } else if (File.Exists(path)) {
                        try {
                            ZipHelper.ZipFile(path);
                        }
                        catch (Exception e2) {
                            this.label2.Text = (e2.Message.ToString());
                        }
                    }
                    this.label2.Text += "\n" + "压缩成功";
                }

            }else if (this.radioButton2.Checked) {
                String path = this.textBox1.Text;
                if (!String.IsNullOrEmpty(path)) {
                    try {
                        ZipHelper.UnZip(path);
                    }
                    catch (Exception e3) {
                        this.label2.Text = e3.Message.ToString();
                    }
                }
                this.label2.Text += "\n" + "解压成功";
            }
        }
    }
}
