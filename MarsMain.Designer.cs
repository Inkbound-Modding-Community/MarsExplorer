namespace MarsExplorer
{
    partial class MarsMain
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
            this.marsCanvas1 = new MarsExplorer.MarsCanvas();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.selectedProps = new System.Windows.Forms.DataGridView();
            this.NameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Value = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.toc = new System.Windows.Forms.TreeView();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.selectedProps)).BeginInit();
            this.SuspendLayout();
            // 
            // marsCanvas1
            // 
            this.marsCanvas1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.marsCanvas1.Location = new System.Drawing.Point(423, 3);
            this.marsCanvas1.Model = null;
            this.marsCanvas1.Name = "marsCanvas1";
            this.marsCanvas1.Size = new System.Drawing.Size(1123, 1182);
            this.marsCanvas1.TabIndex = 0;
            this.marsCanvas1.Text = "marsCanvas1";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 420F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 450F));
            this.tableLayoutPanel1.Controls.Add(this.marsCanvas1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.selectedProps, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.toc, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1999, 1188);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // selectedProps
            // 
            this.selectedProps.AllowUserToAddRows = false;
            this.selectedProps.AllowUserToDeleteRows = false;
            this.selectedProps.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.selectedProps.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.NameColumn,
            this.Value});
            this.selectedProps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.selectedProps.Location = new System.Drawing.Point(1552, 3);
            this.selectedProps.Name = "selectedProps";
            this.selectedProps.ReadOnly = true;
            this.selectedProps.RowHeadersVisible = false;
            this.selectedProps.RowHeadersWidth = 62;
            this.selectedProps.RowTemplate.Height = 33;
            this.selectedProps.Size = new System.Drawing.Size(444, 1182);
            this.selectedProps.TabIndex = 1;
            // 
            // NameColumn
            // 
            this.NameColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.NameColumn.FillWeight = 1F;
            this.NameColumn.HeaderText = "Name";
            this.NameColumn.MinimumWidth = 8;
            this.NameColumn.Name = "NameColumn";
            this.NameColumn.ReadOnly = true;
            this.NameColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.NameColumn.Width = 150;
            // 
            // Value
            // 
            this.Value.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Value.HeaderText = "Value";
            this.Value.MinimumWidth = 8;
            this.Value.Name = "Value";
            this.Value.ReadOnly = true;
            // 
            // toc
            // 
            this.toc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toc.Location = new System.Drawing.Point(3, 3);
            this.toc.Name = "toc";
            this.toc.Size = new System.Drawing.Size(414, 1182);
            this.toc.TabIndex = 2;
            // 
            // MarsMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1999, 1188);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "MarsMain";
            this.Text = "Mars Explorer (Ares)";
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.selectedProps)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private MarsCanvas marsCanvas1;
        private TableLayoutPanel tableLayoutPanel1;
        private DataGridView selectedProps;
        private DataGridViewTextBoxColumn NameColumn;
        private DataGridViewTextBoxColumn Value;
        private TreeView toc;
    }
}