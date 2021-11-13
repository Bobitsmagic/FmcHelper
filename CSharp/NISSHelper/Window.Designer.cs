
namespace NISSHelper
{
	partial class Window
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
			this.ScrambleTB = new System.Windows.Forms.TextBox();
			this.ScrambleLabel = new System.Windows.Forms.Label();
			this.ScrambleLable = new System.Windows.Forms.Label();
			this.ReverseScrambleLabel = new System.Windows.Forms.Label();
			this.SolutionLabel = new System.Windows.Forms.Label();
			this.TBSolution = new System.Windows.Forms.RichTextBox();
			this.SolutionReverseLabel = new System.Windows.Forms.Label();
			this.InverseLabel = new System.Windows.Forms.Label();
			this.TBInverse = new System.Windows.Forms.RichTextBox();
			this.InverseReverseLabel = new System.Windows.Forms.Label();
			this.ParseSolutionB = new System.Windows.Forms.Button();
			this.ParseInverseB = new System.Windows.Forms.Button();
			this.ParseScrambleB = new System.Windows.Forms.Button();
			this.CombinedSolutionLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// ScrambleTB
			// 
			this.ScrambleTB.Location = new System.Drawing.Point(548, 9);
			this.ScrambleTB.Name = "ScrambleTB";
			this.ScrambleTB.Size = new System.Drawing.Size(240, 20);
			this.ScrambleTB.TabIndex = 0;
			// 
			// ScrambleLabel
			// 
			this.ScrambleLabel.AutoSize = true;
			this.ScrambleLabel.Location = new System.Drawing.Point(12, 12);
			this.ScrambleLabel.Name = "ScrambleLabel";
			this.ScrambleLabel.Size = new System.Drawing.Size(51, 13);
			this.ScrambleLabel.TabIndex = 1;
			this.ScrambleLabel.Text = "Scramble";
			// 
			// ScrambleLable
			// 
			this.ScrambleLable.AutoSize = true;
			this.ScrambleLable.Location = new System.Drawing.Point(12, 35);
			this.ScrambleLable.Name = "ScrambleLable";
			this.ScrambleLable.Size = new System.Drawing.Size(47, 13);
			this.ScrambleLable.TabIndex = 2;
			this.ScrambleLable.Text = "Reverse";
			// 
			// ReverseScrambleLabel
			// 
			this.ReverseScrambleLabel.AutoSize = true;
			this.ReverseScrambleLabel.Location = new System.Drawing.Point(12, 60);
			this.ReverseScrambleLabel.Name = "ReverseScrambleLabel";
			this.ReverseScrambleLabel.Size = new System.Drawing.Size(77, 13);
			this.ReverseScrambleLabel.TabIndex = 4;
			this.ReverseScrambleLabel.Text = "ScrambleLabel";
			// 
			// SolutionLabel
			// 
			this.SolutionLabel.AutoSize = true;
			this.SolutionLabel.Location = new System.Drawing.Point(12, 119);
			this.SolutionLabel.Name = "SolutionLabel";
			this.SolutionLabel.Size = new System.Drawing.Size(45, 13);
			this.SolutionLabel.TabIndex = 5;
			this.SolutionLabel.Text = "Solution";
			// 
			// TBSolution
			// 
			this.TBSolution.Location = new System.Drawing.Point(15, 145);
			this.TBSolution.Name = "TBSolution";
			this.TBSolution.Size = new System.Drawing.Size(308, 140);
			this.TBSolution.TabIndex = 6;
			this.TBSolution.Text = "";
			// 
			// SolutionReverseLabel
			// 
			this.SolutionReverseLabel.AutoSize = true;
			this.SolutionReverseLabel.Location = new System.Drawing.Point(16, 288);
			this.SolutionReverseLabel.Name = "SolutionReverseLabel";
			this.SolutionReverseLabel.Size = new System.Drawing.Size(47, 13);
			this.SolutionReverseLabel.TabIndex = 7;
			this.SolutionReverseLabel.Text = "Reverse";
			// 
			// InverseLabel
			// 
			this.InverseLabel.AutoSize = true;
			this.InverseLabel.Location = new System.Drawing.Point(633, 119);
			this.InverseLabel.Name = "InverseLabel";
			this.InverseLabel.Size = new System.Drawing.Size(42, 13);
			this.InverseLabel.TabIndex = 8;
			this.InverseLabel.Text = "Inverse";
			// 
			// TBInverse
			// 
			this.TBInverse.Location = new System.Drawing.Point(626, 145);
			this.TBInverse.Name = "TBInverse";
			this.TBInverse.Size = new System.Drawing.Size(312, 140);
			this.TBInverse.TabIndex = 9;
			this.TBInverse.Text = "";
			// 
			// InverseReverseLabel
			// 
			this.InverseReverseLabel.AutoSize = true;
			this.InverseReverseLabel.Location = new System.Drawing.Point(628, 288);
			this.InverseReverseLabel.Name = "InverseReverseLabel";
			this.InverseReverseLabel.Size = new System.Drawing.Size(47, 13);
			this.InverseReverseLabel.TabIndex = 10;
			this.InverseReverseLabel.Text = "Reverse";
			// 
			// ParseSolutionB
			// 
			this.ParseSolutionB.Location = new System.Drawing.Point(63, 114);
			this.ParseSolutionB.Name = "ParseSolutionB";
			this.ParseSolutionB.Size = new System.Drawing.Size(75, 23);
			this.ParseSolutionB.TabIndex = 11;
			this.ParseSolutionB.Text = "Parse";
			this.ParseSolutionB.UseVisualStyleBackColor = true;
			this.ParseSolutionB.Click += new System.EventHandler(this.ParseSolutionB_Click);
			// 
			// ParseInverseB
			// 
			this.ParseInverseB.Location = new System.Drawing.Point(696, 114);
			this.ParseInverseB.Name = "ParseInverseB";
			this.ParseInverseB.Size = new System.Drawing.Size(105, 23);
			this.ParseInverseB.TabIndex = 12;
			this.ParseInverseB.Text = "Parse";
			this.ParseInverseB.UseVisualStyleBackColor = true;
			this.ParseInverseB.Click += new System.EventHandler(this.ParseInverseB_Click);
			// 
			// ParseScrambleB
			// 
			this.ParseScrambleB.Location = new System.Drawing.Point(713, 35);
			this.ParseScrambleB.Name = "ParseScrambleB";
			this.ParseScrambleB.Size = new System.Drawing.Size(75, 23);
			this.ParseScrambleB.TabIndex = 13;
			this.ParseScrambleB.Text = "Parse";
			this.ParseScrambleB.UseVisualStyleBackColor = true;
			this.ParseScrambleB.Click += new System.EventHandler(this.ParseScrambleB_Click);
			// 
			// CombinedSolutionLabel
			// 
			this.CombinedSolutionLabel.AutoSize = true;
			this.CombinedSolutionLabel.Location = new System.Drawing.Point(12, 416);
			this.CombinedSolutionLabel.Name = "CombinedSolutionLabel";
			this.CombinedSolutionLabel.Size = new System.Drawing.Size(35, 13);
			this.CombinedSolutionLabel.TabIndex = 14;
			this.CombinedSolutionLabel.Text = "label1";
			// 
			// Window
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1201, 438);
			this.Controls.Add(this.CombinedSolutionLabel);
			this.Controls.Add(this.ParseScrambleB);
			this.Controls.Add(this.ParseInverseB);
			this.Controls.Add(this.ParseSolutionB);
			this.Controls.Add(this.InverseReverseLabel);
			this.Controls.Add(this.TBInverse);
			this.Controls.Add(this.InverseLabel);
			this.Controls.Add(this.SolutionReverseLabel);
			this.Controls.Add(this.TBSolution);
			this.Controls.Add(this.SolutionLabel);
			this.Controls.Add(this.ReverseScrambleLabel);
			this.Controls.Add(this.ScrambleLable);
			this.Controls.Add(this.ScrambleLabel);
			this.Controls.Add(this.ScrambleTB);
			this.Name = "Window";
			this.Text = "KEKEKEKEK";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox ScrambleTB;
		private System.Windows.Forms.Label ScrambleLabel;
		private System.Windows.Forms.Label ScrambleLable;
		private System.Windows.Forms.Label ReverseScrambleLabel;
		private System.Windows.Forms.Label SolutionLabel;
		private System.Windows.Forms.RichTextBox TBSolution;
		private System.Windows.Forms.Label SolutionReverseLabel;
		private System.Windows.Forms.Label InverseLabel;
		private System.Windows.Forms.RichTextBox TBInverse;
		private System.Windows.Forms.Label InverseReverseLabel;
		private System.Windows.Forms.Button ParseSolutionB;
		private System.Windows.Forms.Button ParseInverseB;
		private System.Windows.Forms.Button ParseScrambleB;
		private System.Windows.Forms.Label CombinedSolutionLabel;
	}
}

