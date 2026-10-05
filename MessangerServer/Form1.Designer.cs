namespace MessangerServer
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            buttonStart = new Button();
            buttonStop = new Button();
            label1 = new Label();
            label2 = new Label();
            listBox1 = new ListBox();
            SuspendLayout();
            // 
            // buttonStart
            // 
            buttonStart.BackColor = Color.FromArgb(192, 255, 192);
            buttonStart.Location = new Point(12, 33);
            buttonStart.Name = "buttonStart";
            buttonStart.Size = new Size(116, 29);
            buttonStart.TabIndex = 0;
            buttonStart.Text = "Запустити";
            buttonStart.UseVisualStyleBackColor = false;
            buttonStart.Click += buttonStart_Click;
            // 
            // buttonStop
            // 
            buttonStop.BackColor = Color.FromArgb(255, 192, 192);
            buttonStop.Location = new Point(230, 33);
            buttonStop.Name = "buttonStop";
            buttonStop.Size = new Size(111, 29);
            buttonStop.TabIndex = 1;
            buttonStop.Text = "Зупинити";
            buttonStop.UseVisualStyleBackColor = false;
            buttonStop.Click += buttonStop_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe Print", 9F, FontStyle.Bold, GraphicsUnit.Point, 238);
            label1.ForeColor = Color.Navy;
            label1.Location = new Point(12, 88);
            label1.Name = "label1";
            label1.Size = new Size(82, 26);
            label1.TabIndex = 2;
            label1.Text = "Статус:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe Print", 9F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label2.ForeColor = Color.Navy;
            label2.Location = new Point(12, 162);
            label2.Name = "label2";
            label2.Size = new Size(167, 26);
            label2.TabIndex = 3;
            label2.Text = "Online користувачі:";
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.Location = new Point(12, 201);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(329, 124);
            listBox1.TabIndex = 4;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaption;
            ClientSize = new Size(353, 342);
            Controls.Add(listBox1);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(buttonStop);
            Controls.Add(buttonStart);
            MinimizeBox = false;
            Name = "Form1";
            Text = "Messanger Server";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button buttonStart;
        private Button buttonStop;
        private Label label1;
        private Label label2;
        private ListBox listBox1;
    }
}
