using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Book
{
    public partial class Form1 : Form
    {
        BookSqlCmd Cmd;
        Boolean BrowseStatus; // true公共书册，false个人书册
        int Choose1, Choose2;

        public Form1()
        {
            InitializeComponent();
        }

        // 窗口加载
        private void Form1_Load(object sender, EventArgs e)
        {
            ReSize();
            Cmd = new BookSqlCmd("boooook.db");
            AbiInfo.ListFromDB(Cmd);
            tBook.ListFromDB(Cmd);
            tPage.ListFromDB(Cmd);
            Choose1 = 0; Choose2 = 0;
            Switch(false);
            RefreshChoose();
            RefreshBookPageTable();
        }
        
        

        // 公共/个人切换
        private void SwitchButton_Click(object sender, EventArgs e)
        {
            Switch(BrowseStatus);
        }

        private void Switch(Boolean PP)
        {
            if (BrowseStatus)
            {
                BrowseStatus = false;
                this.SwitchButton.Text = "切换至公共书册";
                this.AddButton.Visible = false;
                this.AddToWhich.Visible = false;
                this.RemoveButton.Visible = true;
            }
            else
            {
                BrowseStatus = true;
                this.SwitchButton.Text = "切换至个人书册";
                this.AddButton.Visible = true;
                this.AddToWhich.Visible = true;
                this.RemoveButton.Visible = false;
            }
            RefreshChoose();
            RefreshBookPageTable();
        }
        
        // 刷新choose
        private void RefreshChoose()
        {
            this.Choose.Items.Clear();

            if (BrowseStatus)
            {//公共书册
                this.Choose.Items.Add("所有书册");
                foreach (int BookId in tBook.List.Keys)
                {
                    this.Choose.Items.Add(tBook.List[BookId].tName);
                }
                Choose.SelectedIndex = Choose1;
            }
            else
            {//个人书册
                this.Choose.Items.Add("选择方案");
                this.Choose.Items.Add("方案1");
                this.Choose.Items.Add("方案2");
                this.Choose.Items.Add("方案3");
                this.Choose.Items.Add("方案4");
                this.Choose.Items.Add("方案5");
                Choose.SelectedIndex = Choose2;
            }
        }

        // 刷新表格内容
        private void RefreshBookPageTable()
        {
            // 清空表格的行数据
            this.BookPageTable.Rows.Clear();

            if (BrowseStatus)
            {//公共书册
                int BookId = Choose.SelectedIndex;
                foreach (tPage Page in tPage.List.Values)
                {
                    if (BookId == 0 || Page.tBookBelong.tBookId == BookId)
                    {
                        int index = this.BookPageTable.Rows.Add();
                        this.BookPageTable.Rows[index].Cells[0].Value = Page.tBookBelong.tName;
                        this.BookPageTable.Rows[index].Cells[1].Value = Page.tPageId;
                        this.BookPageTable.Rows[index].Cells[2].Value = Page.tName;
                        this.BookPageTable.Rows[index].Cells[3].Value = BFC.Desc(Page.PageType);
                        this.BookPageTable.Rows[index].Cells[4].Value = Page.tCost;
                        this.BookPageTable.Rows[index].Cells[5].Value = Page.Introduce;
                    }
                }
            }
            else
            {//个人书册
                int MyBookid = Choose.SelectedIndex;
                Cmd.Reading($"select * from My_Book_T where My_Book = {MyBookid}", false);
                while (Cmd.Reading())
                {
                    int index = this.BookPageTable.Rows.Add();
                    int PageId = Cmd.ReadInt("Page_Id");
                    this.BookPageTable.Rows[index].Cells[0].Value = tPage.List[PageId].tBookBelong.tName;
                    this.BookPageTable.Rows[index].Cells[1].Value = PageId;
                    this.BookPageTable.Rows[index].Cells[2].Value = tPage.List[PageId].tName;
                    this.BookPageTable.Rows[index].Cells[3].Value = tPage.List[PageId].PageType.ToString();
                    this.BookPageTable.Rows[index].Cells[4].Value = tPage.List[PageId].tCost;
                    this.BookPageTable.Rows[index].Cells[5].Value = tPage.List[PageId].Introduce;
                }
            }
        }

        private void ChooseBooks_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (BrowseStatus)
            {
                Choose1 = this.Choose.SelectedIndex;
            }
            else
            {
                Choose2 = this.Choose.SelectedIndex;
            }
                RefreshBookPageTable();
        }

        // 添加进个人书册
        private void AddButton_Click(object sender, EventArgs e)
        {
            Add2MyBook(Cmd);
        }
        private void Add2MyBook(BookSqlCmd Cmd)
        {
            int MyBook = this.AddToWhich.SelectedIndex + 1;

            if (BookPageTable.SelectedRows.Count >= 1)
            {
                foreach (DataGridViewRow Row in BookPageTable.SelectedRows)
                {
                    int PageId = Convert.ToInt32(Row.Cells[1].Value);
                    Cmd.NonQuery($"Insert Into My_Book_T(My_Book,Page_Id) values({MyBook},{PageId})");
                }
            }
            else if (BookPageTable.SelectedCells.Count >= 1)
            {
                List<int> done = new List<int>();
                foreach (DataGridViewCell cell in BookPageTable.SelectedCells)
                {
                    int row = cell.RowIndex;
                    if (!done.Contains(row))
                    {
                        done.Add(row);
                        int PageId = Convert.ToInt32(this.BookPageTable.Rows[row].Cells[1].Value);
                        Cmd.NonQuery($"Insert Into My_Book_T(My_Book,Page_Id) values({MyBook},{PageId})");
                    }
                }
            }

        }
        
        // 从个人书册中删除
        private void RemoveButton_Click(object sender, EventArgs e)
        {
            RemoveFromMyBook(Cmd);
        }
        private void RemoveFromMyBook(BookSqlCmd Cmd)
        {
            int MyBook = this.Choose.SelectedIndex;

            if (BookPageTable.SelectedRows.Count >= 1)
            {
                foreach (DataGridViewRow Row in BookPageTable.SelectedRows)
                {
                    int PageId = Convert.ToInt32(Row.Cells[1].Value);
                    string CommandText = $"delete from My_Book_T where rowid in (select rowid from My_Book_T where My_Book = {MyBook} and page_id = {PageId} limit 1)";
                    Cmd.NonQuery(CommandText);
                }
            }
            else if (BookPageTable.SelectedCells.Count >= 1)
            {
                List<int> done = new List<int>();
                foreach (DataGridViewCell cell in BookPageTable.SelectedCells)
                {
                    int row = cell.RowIndex;
                    if (!done.Contains(row))
                    {
                        done.Add(row);
                        int PageId = Convert.ToInt32(this.BookPageTable.Rows[row].Cells[1].Value);
                        string CommandText = $"delete from My_Book_T where rowid in (select rowid from My_Book_T where My_Book = {MyBook} and page_id = {PageId} limit 1)";
                        Cmd.NonQuery(CommandText);
                    }
                }
            }
            RefreshBookPageTable();
        }

        // 显示卡牌信息
        private void BookPageTable_Click(object sender, EventArgs e)
        {
            if (BookPageTable.SelectedRows.Count == 1)
            {
                int PageId = (int)this.BookPageTable.SelectedRows[0].Cells[1].Value;
                this.PageMsg.Text = BFC.Msg(tPage.List[PageId]);
            }
            if (BookPageTable.SelectedCells.Count == 1)
            {
                int row = BookPageTable.SelectedCells[0].RowIndex;
                int PageId = (int)this.BookPageTable.Rows[row].Cells[1].Value;
                this.PageMsg.Text = BFC.Msg(tPage.List[PageId]);
            }
        }

        // 窗口缩放后，重新布局
        private void Form_Resize(object sender, EventArgs e)
        {
            ReSize();
        }
        // 布局控制
        private void ReSize()
        {
            int Interval = 20;
            int interval = 10;
            double ratio = 1.5;

            if (this.Width / this.Height >= ratio)
            {
                this.Width = Convert.ToInt32(this.Height * ratio);
            }
            else
            {
                this.Height = Convert.ToInt32(this.Width / ratio);
            }

            this.SwitchButton.Location = new Point(this.Width - interval * 2 - this.SwitchButton.Width
                , interval);

            this.Choose.Location = new Point(Interval, Interval);
            this.Choose.Size = new Size(100, 20);

            this.BookPageTable.Location = new Point(this.Choose.Location.X
                , this.Choose.Location.Y + this.Choose.Height + interval);
            this.BookPageTable.Width = Convert.ToInt32((this.Width - Interval * 2) * 0.7);
            this.BookPageTable.Height = this.Height - this.BookPageTable.Location.Y - Interval * 3;

            this.PageMsg.MaximumSize = new Size((this.Width - this.BookPageTable.Width - Interval * 4), this.PageMsg.Width * 3 / 2);
            this.PageMsg.MinimumSize = this.PageMsg.MaximumSize;
            this.PageMsg.Size = this.PageMsg.MaximumSize;
            this.PageMsg.Location = new Point((this.BookPageTable.Location.X + this.BookPageTable.Width + Interval)
                , this.BookPageTable.Location.Y);

            this.AddOrRemovePanel.Location = new Point((this.PageMsg.Location.X * 2 + this.PageMsg.Width) / 2 - (this.AddOrRemovePanel.Width / 2)
                , (this.Height + this.PageMsg.Location.Y + this.PageMsg.Height) / 2);

        }

    }
}
