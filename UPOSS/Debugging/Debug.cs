using System;
using System.Collections.Generic;
using System.Text;

namespace UPOSS.Debug
{
    #region Console
    /*
        System.Diagnostics.Trace.WriteLine("string");
    */
    #endregion

    #region SQLite 
    /*
     * Some syntax examples
        string path = "Data Source=SQLiteDatabase.db";

        var connection = new SQLiteConnection(path);
        connection.Open();

        //1
        //var command = new SQLiteCommand("SELECT * FROM SampleTable", connection);

        //2
        //var command = connection.CreateCommand();
        //command.CommandText = "SELECT * FROM SampleTable";

        //3
        var command = new SQLiteCommand(connection);
        command.CommandText = "SELECT * FROM SampleTable";

        // insert and update
        command.ExecuteNonQuery();

        // read data
        var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var result = reader.GetString(0);

            System.Diagnostics.Trace.WriteLine(result);
        }
    */

    /*
     * Another Setup/Structure
        public SQLiteDatabase()
        {
            SQLiteConnection sqliteConnection;
            sqliteConnection = CreateConnection();
            CreateTable(sqliteConnection);
            InsertData(sqliteConnection);
            ReadData(sqliteConnection);
        }

        static SQLiteConnection CreateConnection()
        {
            SQLiteConnection sqliteConn;
            sqliteConn = new SQLiteConnection("Data Source=SQLiteDatabase.db; Version = 3; New = True; Compress = True;");
            try
            {
                sqliteConn.Open();
            }
            catch
            {

            }
            return sqliteConn;
        }

        static void CreateTable(SQLiteConnection conn)
        {
            SQLiteCommand sqliteCommand;
            string createSQL = "CREATE TABLE [IF NOT EXISTS] products (id BIGINT PRIMARY KEY, product_no VARCHAR(100))";
            sqliteCommand = conn.CreateCommand();
            sqliteCommand.CommandText = createSQL;
            sqliteCommand.ExecuteNonQuery();
        }

        static void InsertData(SQLiteConnection conn)
        {
            SQLiteCommand sqliteCommand;
            sqliteCommand = conn.CreateCommand();
            sqliteCommand.CommandText = "INSERT INTO products (product_no) VALUES ('A0000001');";
            sqliteCommand.ExecuteNonQuery();
        }

        static void ReadData(SQLiteConnection conn)
        {
            SQLiteDataReader sqliteReader;
            SQLiteCommand sqliteCommand;
            sqliteCommand = conn.CreateCommand();
            sqliteCommand.CommandText = "SELECT * FROM products";
            sqliteReader = sqliteCommand.ExecuteReader();
            while (sqliteReader.Read())
            {
                string readerString = sqliteReader.GetString(0);
                System.Diagnostics.Trace.WriteLine(readerString);
            }
            conn.Close();
        }
    */
    #endregion

    #region End Program
    /*
        Environment.Exit(0);
    */
    #endregion
}
