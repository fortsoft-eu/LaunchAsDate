namespace LaunchAsDate {
    partial class TestForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestForm));
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxArguments = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.dateTimePickerLaunchDate = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.dateTimePickerCurrentDate = new System.Windows.Forms.DateTimePicker();
            this.button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // textBoxArguments
            // 
            resources.ApplyResources(this.textBoxArguments, "textBoxArguments");
            this.textBoxArguments.Name = "textBoxArguments";
            this.textBoxArguments.ReadOnly = true;
            this.textBoxArguments.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxKeyDown);
            this.textBoxArguments.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TextBoxMouseDown);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // dateTimePickerLaunchDate
            // 
            resources.ApplyResources(this.dateTimePickerLaunchDate, "dateTimePickerLaunchDate");
            this.dateTimePickerLaunchDate.Name = "dateTimePickerLaunchDate";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // dateTimePickerCurrentDate
            // 
            resources.ApplyResources(this.dateTimePickerCurrentDate, "dateTimePickerCurrentDate");
            this.dateTimePickerCurrentDate.Name = "dateTimePickerCurrentDate";
            // 
            // button
            // 
            resources.ApplyResources(this.button, "button");
            this.button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button.Name = "button";
            this.button.UseVisualStyleBackColor = true;
            this.button.Click += new System.EventHandler(this.Close);
            // 
            // TestForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button;
            this.Controls.Add(this.button);
            this.Controls.Add(this.dateTimePickerCurrentDate);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.dateTimePickerLaunchDate);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxArguments);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "TestForm";
            this.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.OpenHelp);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxArguments;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker dateTimePickerLaunchDate;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DateTimePicker dateTimePickerCurrentDate;
        private System.Windows.Forms.Button button;
    }
}