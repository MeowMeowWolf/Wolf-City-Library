using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;

namespace BookSql
{
    //sqlite数据库的建立、加/解密、连接
    public static class BookSqlDB
    {
        //新建一个带密码的sqlite数据库
        public static void Builder(string db_name, string password)
        {
            SQLiteConnectionStringBuilder new_db = new SQLiteConnectionStringBuilder();
            new_db.DataSource = db_name;
            new_db.Password = password;

            SQLiteConnection conn = new SQLiteConnection(new_db + "");
            conn.Open();
            SQLiteCommand command = new SQLiteCommand("create table Book_DB (Version INTEGER,Info TEXT); insert into Book_DB(Version,Info) values (1,\"YES\")", conn);
            command.ExecuteNonQuery();
            command.Cancel();
            conn.Close();
        }

        //更改sqlite数据库的密码
        public static void ChangePassword(string db_name, string old_password, string new_password)
        {
            SQLiteConnection db_conn = new SQLiteConnection("Data Source=" + db_name);
            db_conn.SetPassword(old_password);
            db_conn.Open();
            db_conn.ChangePassword(new_password);
            db_conn.Close();
        }

        //打开一个无密码的sqlite连接
        public static SQLiteConnection Open(string db_name)
        {
            SQLiteConnection db_conn = new SQLiteConnection("Data Source=" + db_name);
            db_conn.Open();
            if (Check(db_conn))
            {
                return db_conn;
            }
            else
            {
                throw (new Exception("Err:\"密码错误\"或\"数据库版本与预期不符\""));
            }
        }

        //打开一个带密码的sqlite连接
        public static SQLiteConnection Open(string db_name, string password)
        {
            SQLiteConnection db_conn = new SQLiteConnection("Data Source=" + db_name);
            db_conn.SetPassword(password);
            db_conn.Open();
            if (Check(db_conn))
            {
                return db_conn;
            }
            else
            {
                throw (new Exception("Err:\"密码错误\"或\"数据库版本与预期不符\""));
            }
        }

        //由于使用的SQLite3.dll版本bug，无法通过连接状态来判断密码是否正确，所以要加一个判断连接是否成功的动作
        public static bool Check(SQLiteConnection db_conn)
        {
            int rv = 0;
            SQLiteCommand command = new SQLiteCommand(db_conn);
            command.CommandText = "select count(1) from Book_DB";
            SQLiteDataReader reader = command.ExecuteReader();

            reader.Read();
            rv = Convert.ToInt32(reader["count(1)"]);
            reader.Close();
            command.Dispose();
            return (rv > 0);
        }

        //删库，谨慎执行
        public static void Delete(string DBName, string PassWord)
        {
            if (System.IO.File.Exists(DBName))
            {
                SQLiteConnection db_conn = new SQLiteConnection("Data Source=" + DBName);
                db_conn.SetPassword(PassWord);
                db_conn.Open();
                Check(db_conn);
                db_conn.Dispose();
            }
            System.IO.File.Delete(DBName);
        }
    }

    //sqlite命令执行器
    public class BookSqlCmd
    {
        SQLiteConnection Connection;
        SQLiteCommand Command;
        SQLiteDataReader Reader;

        //空构造
        public BookSqlCmd() {        }
        //构造执行器时，连接到不带密码数据库
        public BookSqlCmd(string db_name)
        {
            Connection = new SQLiteConnection("Data Source=" + db_name);
            Connection.Open();
            Command = new SQLiteCommand(Connection);
        }
        //构造执行器时，连接到带有密码数据库
        public BookSqlCmd(string db_name, string password)
        {
            Connection = new SQLiteConnection("Data Source=" + db_name);
            Connection.SetPassword(password);
            Connection.Open();
            Command = new SQLiteCommand(Connection);
        }
        //构造执行器时，借用现有的连接
        public BookSqlCmd(SQLiteConnection connection)
        {
            Connection = connection;
            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }
            Command = new SQLiteCommand(Connection);
        }

        //析构时，关闭数据库连接、命令、阅读器
        ~BookSqlCmd()
        {
            if (Reader != null)
            {
                Reader.Close();
            }
            Command.Dispose();
            Connection.Close();
        }


        //非查询命令
        public void NonQuery(string CommandText)
        {
            Command.CommandText = CommandText;
            Command.ExecuteNonQuery();
        }

        //查询命令（是否自动执行.Read()）
        public void Reading(string CommandText,Boolean isRead)
        {
            if ( Reader!=null) { Reader.Close(); }
            Command.CommandText = CommandText;
            Reader = Command.ExecuteReader();
            if (isRead) { Reader.Read(); }
        }
        //查询命令（默认自动执行.Read()）
        public void Reading(string CommandText)
        {
            if (Reader != null) { Reader.Close(); }
            Command.CommandText = CommandText;
            Reader = Command.ExecuteReader();
            Reader.Read();
        }
        //继续查询
        public Boolean Reading()
        {
            return Reader.Read();          
        }

        public bool ReadBool(string inKey) { return Convert.ToBoolean(Reader[inKey]); }
        public int ReadInt(string inKey) { return Convert.ToInt32(Reader[inKey]);}
        public double ReadDouble(string inKey) { return Convert.ToDouble(Reader[inKey]); }
        public char ReadChar(string inKey) { return Convert.ToChar(Reader[inKey]); }
        public string ReadString(string inKey) { return Convert.ToString(Reader[inKey]); }

        public BookSqlCmd Clone()
        {
            BookSqlCmd NewCmd = new BookSqlCmd();
            NewCmd.Connection = (SQLiteConnection)Connection.Clone();
            if(NewCmd.Connection.State== ConnectionState.Closed)
            {
                NewCmd.Connection.Open();
            }            
            NewCmd.Command = new SQLiteCommand(Connection);
            return NewCmd;
        }
    }
}
