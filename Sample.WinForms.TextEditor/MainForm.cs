using System.Drawing;
using System.Windows.Forms;

namespace Sample.WinForms.TextEditor
{
    // Visual Studioのフォームデザイナで開かれるのを防ぐため
    sealed class e21b747df845491abd8b2ee8030dfc47 { }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class MainForm : Form
    {
        /// <summary>
        /// 
        /// </summary>
        public MainForm()
        {
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Text = GetType().Namespace;

            Controls.Add(new TextEditorControl { Dock = DockStyle.Fill });
        }
    }
}
