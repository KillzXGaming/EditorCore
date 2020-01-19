﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MarioKart.MK7
{
    public partial class STProgressBar : Form
    {
        public STProgressBar()
        {
            InitializeComponent();
        }

        public int Value
        {
            set
            {
                if (value > 100)
                    progressBar1.Value = 0;
                else
                    progressBar1.Value = value;

                if (value >= 100)
                    Close();
                progressBar1.Refresh();
            }
        }
        public string Task
        {
            set
            {
                label1.Text = value;
                label1.Refresh();
            }
        }

        private void ProgressBar_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void ProgressBarWindow_Load(object sender, EventArgs e)
        {

        }

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
        this.progressBar1 = new System.Windows.Forms.ProgressBar();
        this.label1 = new System.Windows.Forms.Label();
        this.SuspendLayout();
        // 
        // progressBar1
        // 
        this.progressBar1.Location = new System.Drawing.Point(12, 25);
        this.progressBar1.Name = "progressBar1";
        this.progressBar1.Size = new System.Drawing.Size(287, 29);
        this.progressBar1.Step = 5;
        this.progressBar1.TabIndex = 0;
        // 
        // label1
        // 
        this.label1.AutoSize = true;
        this.label1.Location = new System.Drawing.Point(114, 9);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(79, 13);
        this.label1.TabIndex = 1;
        this.label1.Text = "Loaidng File.....";
        // 
        // ProgressBar
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(311, 82);
        this.Controls.Add(this.label1);
        this.Controls.Add(this.progressBar1);
        this.Name = "ProgressBar";
        this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ProgressBar_FormClosed);
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ProgressBar progressBar1;
    public System.Windows.Forms.Label label1;
}
}
