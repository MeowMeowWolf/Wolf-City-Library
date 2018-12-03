namespace Book
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.SwitchButton = new System.Windows.Forms.Button();
            this.BookPageTable = new System.Windows.Forms.DataGridView();
            this.tBookName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tPageId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tPageName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tPageType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tCost = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tIntroduce = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Choose = new System.Windows.Forms.ComboBox();
            this.AddButton = new System.Windows.Forms.Button();
            this.AddToWhich = new System.Windows.Forms.ComboBox();
            this.AddOrRemovePanel = new System.Windows.Forms.Panel();
            this.RemoveButton = new System.Windows.Forms.Button();
            this.PageMsg = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.BookPageTable)).BeginInit();
            this.AddOrRemovePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // SwitchButton
            // 
            this.SwitchButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.SwitchButton.Location = new System.Drawing.Point(665, 3);
            this.SwitchButton.Name = "SwitchButton";
            this.SwitchButton.Size = new System.Drawing.Size(139, 23);
            this.SwitchButton.TabIndex = 0;
            this.SwitchButton.Text = "切换至公共书籍";
            this.SwitchButton.UseVisualStyleBackColor = true;
            this.SwitchButton.Click += new System.EventHandler(this.SwitchButton_Click);
            // 
            // BookPageTable
            // 
            this.BookPageTable.AllowUserToAddRows = false;
            this.BookPageTable.AllowUserToDeleteRows = false;
            this.BookPageTable.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.BookPageTable.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.BookPageTable.BackgroundColor = System.Drawing.SystemColors.Control;
            this.BookPageTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.BookPageTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.tBookName,
            this.tPageId,
            this.tPageName,
            this.tPageType,
            this.tCost,
            this.tIntroduce});
            this.BookPageTable.Location = new System.Drawing.Point(12, 45);
            this.BookPageTable.Name = "BookPageTable";
            this.BookPageTable.ReadOnly = true;
            this.BookPageTable.RowTemplate.Height = 27;
            this.BookPageTable.Size = new System.Drawing.Size(432, 487);
            this.BookPageTable.TabIndex = 2;
            this.BookPageTable.Click += new System.EventHandler(this.BookPageTable_Click);
            // 
            // tBookName
            // 
            this.tBookName.HeaderText = "书册";
            this.tBookName.Name = "tBookName";
            this.tBookName.ReadOnly = true;
            this.tBookName.Width = 66;
            // 
            // tPageId
            // 
            this.tPageId.HeaderText = "书页Id";
            this.tPageId.Name = "tPageId";
            this.tPageId.ReadOnly = true;
            this.tPageId.Visible = false;
            this.tPageId.Width = 66;
            // 
            // tPageName
            // 
            this.tPageName.HeaderText = "书页";
            this.tPageName.Name = "tPageName";
            this.tPageName.ReadOnly = true;
            this.tPageName.Width = 66;
            // 
            // tPageType
            // 
            this.tPageType.HeaderText = "书页类型";
            this.tPageType.Name = "tPageType";
            this.tPageType.ReadOnly = true;
            this.tPageType.Width = 96;
            // 
            // tCost
            // 
            this.tCost.HeaderText = "费用";
            this.tCost.Name = "tCost";
            this.tCost.ReadOnly = true;
            this.tCost.Width = 66;
            // 
            // tIntroduce
            // 
            this.tIntroduce.HeaderText = "简介";
            this.tIntroduce.Name = "tIntroduce";
            this.tIntroduce.ReadOnly = true;
            this.tIntroduce.Width = 66;
            // 
            // Choose
            // 
            this.Choose.FormattingEnabled = true;
            this.Choose.Location = new System.Drawing.Point(12, 4);
            this.Choose.Name = "Choose";
            this.Choose.Size = new System.Drawing.Size(121, 23);
            this.Choose.TabIndex = 0;
            this.Choose.Text = "选择书籍或方案";
            this.Choose.SelectionChangeCommitted += new System.EventHandler(this.ChooseBooks_SelectionChangeCommitted);
            // 
            // AddButton
            // 
            this.AddButton.Location = new System.Drawing.Point(3, 3);
            this.AddButton.Name = "AddButton";
            this.AddButton.Size = new System.Drawing.Size(75, 23);
            this.AddButton.TabIndex = 4;
            this.AddButton.Text = "添加至";
            this.AddButton.UseVisualStyleBackColor = true;
            this.AddButton.Visible = false;
            this.AddButton.Click += new System.EventHandler(this.AddButton_Click);
            // 
            // AddToWhich
            // 
            this.AddToWhich.FormattingEnabled = true;
            this.AddToWhich.Items.AddRange(new object[] {
            "方案1",
            "方案2",
            "方案3",
            "方案4",
            "方案5"});
            this.AddToWhich.Location = new System.Drawing.Point(84, 3);
            this.AddToWhich.Name = "AddToWhich";
            this.AddToWhich.Size = new System.Drawing.Size(100, 23);
            this.AddToWhich.TabIndex = 5;
            this.AddToWhich.Text = "选择方案";
            this.AddToWhich.Visible = false;
            // 
            // AddOrRemovePanel
            // 
            this.AddOrRemovePanel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.AddOrRemovePanel.Controls.Add(this.RemoveButton);
            this.AddOrRemovePanel.Controls.Add(this.AddButton);
            this.AddOrRemovePanel.Controls.Add(this.AddToWhich);
            this.AddOrRemovePanel.Location = new System.Drawing.Point(518, 453);
            this.AddOrRemovePanel.Name = "AddOrRemovePanel";
            this.AddOrRemovePanel.Size = new System.Drawing.Size(190, 30);
            this.AddOrRemovePanel.TabIndex = 6;
            // 
            // RemoveButton
            // 
            this.RemoveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RemoveButton.Location = new System.Drawing.Point(39, 3);
            this.RemoveButton.Name = "RemoveButton";
            this.RemoveButton.Size = new System.Drawing.Size(113, 23);
            this.RemoveButton.TabIndex = 7;
            this.RemoveButton.Text = "从方案中移除";
            this.RemoveButton.UseVisualStyleBackColor = true;
            this.RemoveButton.Visible = false;
            this.RemoveButton.Click += new System.EventHandler(this.RemoveButton_Click);
            // 
            // PageMsg
            // 
            this.PageMsg.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.PageMsg.BackColor = System.Drawing.SystemColors.ControlDark;
            this.PageMsg.Location = new System.Drawing.Point(472, 45);
            this.PageMsg.MaximumSize = new System.Drawing.Size(1000, 1000);
            this.PageMsg.MinimumSize = new System.Drawing.Size(300, 400);
            this.PageMsg.Name = "PageMsg";
            this.PageMsg.Size = new System.Drawing.Size(300, 400);
            this.PageMsg.TabIndex = 7;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(807, 546);
            this.Controls.Add(this.PageMsg);
            this.Controls.Add(this.AddOrRemovePanel);
            this.Controls.Add(this.SwitchButton);
            this.Controls.Add(this.Choose);
            this.Controls.Add(this.BookPageTable);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Book";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.Form_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.BookPageTable)).EndInit();
            this.AddOrRemovePanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button SwitchButton;
        private System.Windows.Forms.ComboBox Choose;
        private System.Windows.Forms.DataGridView BookPageTable;
        private System.Windows.Forms.DataGridViewTextBoxColumn tBookName;
        private System.Windows.Forms.DataGridViewTextBoxColumn tPageId;
        private System.Windows.Forms.DataGridViewTextBoxColumn tPageName;
        private System.Windows.Forms.DataGridViewTextBoxColumn tPageType;
        private System.Windows.Forms.DataGridViewTextBoxColumn tCost;
        private System.Windows.Forms.DataGridViewTextBoxColumn tIntroduce;
        private System.Windows.Forms.Button AddButton;
        private System.Windows.Forms.ComboBox AddToWhich;
        private System.Windows.Forms.Panel AddOrRemovePanel;
        private System.Windows.Forms.Button RemoveButton;
        private System.Windows.Forms.Label PageMsg;
    }
}

