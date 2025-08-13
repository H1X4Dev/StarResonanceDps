﻿namespace StarResonanceDpsAnalysis.Control
{
    partial class UserUidSetForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            input2 = new AntdUI.Input();
            inputNumber1 = new AntdUI.InputNumber();
            input1 = new AntdUI.Input();
            pageHeader1 = new AntdUI.PageHeader();
            TitleText = new AntdUI.Label();
            panel6 = new AntdUI.Panel();
            button1 = new AntdUI.Button();
            button2 = new AntdUI.Button();
            pageHeader1.SuspendLayout();
            panel6.SuspendLayout();
            SuspendLayout();
            // 
            // input2
            // 
            input2.Font = new Font("阿里妈妈数黑体", 8F, FontStyle.Bold);
            input2.Location = new Point(72, 203);
            input2.Name = "input2";
            input2.PrefixText = "Name：";
            input2.Radius = 3;
            input2.SelectionColor = Color.FromArgb(143, 176, 229);
            input2.Size = new Size(327, 78);
            input2.TabIndex = 6;
            // 
            // inputNumber1
            // 
            inputNumber1.Font = new Font("SAO Welcome TT", 8.999999F, FontStyle.Regular, GraphicsUnit.Point, 0);
            inputNumber1.Location = new Point(72, 104);
            inputNumber1.MaxLength = 9999999;
            inputNumber1.Name = "inputNumber1";
            inputNumber1.PrefixText = "UID：";
            inputNumber1.Radius = 3;
            inputNumber1.SelectionColor = Color.FromArgb(143, 176, 229);
            inputNumber1.SelectionStart = 1;
            inputNumber1.Size = new Size(327, 78);
            inputNumber1.TabIndex = 7;
            inputNumber1.Text = "0";
            // 
            // input1
            // 
            input1.Font = new Font("阿里妈妈数黑体", 8F, FontStyle.Bold);
            input1.Location = new Point(72, 308);
            input1.Name = "input1";
            input1.PrefixText = "职业：";
            input1.Radius = 3;
            input1.SelectionColor = Color.FromArgb(143, 176, 229);
            input1.SelectionStart = 1;
            input1.Size = new Size(327, 78);
            input1.TabIndex = 8;
            input1.Text = " ";
            // 
            // pageHeader1
            // 
            pageHeader1.BackColor = Color.FromArgb(178, 178, 178);
            pageHeader1.ColorScheme = AntdUI.TAMode.Dark;
            pageHeader1.Controls.Add(TitleText);
            pageHeader1.DividerShow = true;
            pageHeader1.DividerThickness = 2F;
            pageHeader1.Dock = DockStyle.Top;
            pageHeader1.Location = new Point(0, 0);
            pageHeader1.MaximizeBox = false;
            pageHeader1.Mode = AntdUI.TAMode.Dark;
            pageHeader1.Name = "pageHeader1";
            pageHeader1.Size = new Size(471, 52);
            pageHeader1.TabIndex = 16;
            pageHeader1.Text = "";
            // 
            // TitleText
            // 
            TitleText.BackColor = Color.Transparent;
            TitleText.ColorScheme = AntdUI.TAMode.Dark;
            TitleText.Dock = DockStyle.Fill;
            TitleText.Font = new Font("SAO Welcome TT", 12F, FontStyle.Bold);
            TitleText.Location = new Point(0, 0);
            TitleText.Name = "TitleText";
            TitleText.Size = new Size(471, 52);
            TitleText.TabIndex = 26;
            TitleText.Text = "Set Up Information";
            TitleText.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panel6
            // 
            panel6.Controls.Add(button1);
            panel6.Controls.Add(button2);
            panel6.Dock = DockStyle.Bottom;
            panel6.Location = new Point(0, 427);
            panel6.Name = "panel6";
            panel6.Shadow = 6;
            panel6.ShadowAlign = AntdUI.TAlignMini.Top;
            panel6.Size = new Size(471, 86);
            panel6.TabIndex = 31;
            panel6.Text = "panel6";
            panel6.Click += panel6_Click;
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Bottom;
            button1.Ghost = true;
            button1.Icon = Properties.Resources.cancel_normal;
            button1.IconHover = Properties.Resources.cancel_hover;
            button1.IconPosition = AntdUI.TAlignMini.None;
            button1.IconRatio = 1.5F;
            button1.Location = new Point(281, 25);
            button1.Name = "button1";
            button1.Size = new Size(57, 49);
            button1.TabIndex = 1;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Anchor = AnchorStyles.Bottom;
            button2.Ghost = true;
            button2.Icon = Properties.Resources.ok_normal;
            button2.IconHover = Properties.Resources.ok_hover;
            button2.IconPosition = AntdUI.TAlignMini.None;
            button2.IconRatio = 1.5F;
            button2.Location = new Point(126, 25);
            button2.Name = "button2";
            button2.Size = new Size(57, 49);
            button2.TabIndex = 0;
            button2.Click += button2_Click;
            // 
            // UserUidSetForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(471, 513);
            Controls.Add(panel6);
            Controls.Add(pageHeader1);
            Controls.Add(input1);
            Controls.Add(inputNumber1);
            Controls.Add(input2);
            Name = "UserUidSetForm";
            Opacity = 0.95D;
            Load += UserUidSet_Load;
            pageHeader1.ResumeLayout(false);
            panel6.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private AntdUI.Input input2;
        private AntdUI.InputNumber inputNumber1;
        private AntdUI.Input input1;
        private AntdUI.PageHeader pageHeader1;
        private AntdUI.Label TitleText;
        private AntdUI.Panel panel6;
        private AntdUI.Button button1;
        private AntdUI.Button button2;
    }
}