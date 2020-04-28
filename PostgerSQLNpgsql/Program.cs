using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Xml;
using Npgsql; // Импорт элементов для подключения к PostgreSQL базе данных

namespace PostgresSQLNpgsql
{
    class Program
    {
        static void Main(string[] args)
        {

           // ConnectionCredentials newSheet = new ConnectionCredentials(); 
           // newSheet.CreateSheet(); // Вызов метода для создания нового Google Sheet

            DateTime now = DateTime.Now;
            var googleId = ConfigurationManager.AppSettings["googleId"]; // Ссылка на URL Google Sheet из App.config
            List<ConnectionCredentials> servers = new List<ConnectionCredentials>();

            XmlDocument xDoc = new XmlDocument();
            xDoc.Load("C://Users//kirik//source//repos//PostgerSQLNpgsql//PostgerSQLNpgsql//ConfigFile//ServerList.xml");  // получим корневой элемент
            XmlElement xRoot = xDoc.DocumentElement;  // обход всех узлов в корневом элементе
            foreach (XmlNode xnode in xRoot)
            {
                ConnectionCredentials connectionCredentials = new ConnectionCredentials();

                if (xnode.Attributes.Count > 0)
                {
                    XmlNode attr = xnode.Attributes.GetNamedItem("name");
                    if (attr != null)         
                        connectionCredentials.serverName = attr.Value; // Получаем имя сервера из ServerList.xml
                        
                }
                foreach(XmlNode childnode in xnode.ChildNodes)
                {
                    if(childnode.Name =="cred")
                    {
                      //  Console.WriteLine($"Server connection string:{childnode.InnerText}");
                        connectionCredentials.ConnectionStr = childnode.InnerText;
                    }
                    if (childnode.Name == "db")
                    {
                        connectionCredentials.dbName = childnode.InnerText;
                    }
                    servers.Add(connectionCredentials); // Получаем имя атрибута сервера из ServerList.xml

                }

                foreach (ConnectionCredentials c in servers)
                {
                    //Console.WriteLine($"{c.ConnectionStr} "); // Список всех credentials из ServerList.xml
                    using var con = new NpgsqlConnection(c.ConnectionStr); // Подключаемся к базе данных , подставляя список credentials
                    con.Open();

                    var sqlQuerryDB = "SELECT * FROM current_catalog; SELECT current_catalog ";
                    //var sqlQuerry = $"SELECT pg_size_pretty (pg_database_size('{c.dbName}'))"; 
                    var sqlQuerry = $"SELECT pg_database_size('{c.dbName}')";
                    var sqlQuerryName = "SELECT boot_val,reset_val FROM pg_settings WHERE name = 'listen_addresses'"; // Достаем имя сервера

                    using var cmd = new NpgsqlCommand(sqlQuerry, con);
                    using var cmd2 = new NpgsqlCommand(sqlQuerryDB, con);
                    using var cmd3 = new NpgsqlCommand(sqlQuerryName, con);

                    var sizeOfDb = cmd.ExecuteScalar().ToString();
                    var dataBaseName = cmd2.ExecuteScalar().ToString();
                    var serverName = cmd3.ExecuteScalar().ToString();

                    long newSizeOfDb = long.Parse(sizeOfDb);
                    DriveInfo drive = new DriveInfo("C");
                    double formatDivideBy = 1;
                    double freeSpace = -1;
                    double freeSpaceDB = -1;
                    long freeSpaceNative = drive.TotalFreeSpace;
                    formatDivideBy = Math.Pow(1024, (int)3);

                    freeSpace = freeSpaceNative / formatDivideBy;
                    freeSpaceDB = newSizeOfDb / formatDivideBy;
                    

                    //Console.WriteLine($"Server Name: {serverName} Database Name: {dataBaseName} DB size: {resultSizeOfDb}");
                    /// <summary>
                    /// Заполнение данными Google Sheet из PostgreSQL 
                    /// </summary>

                    var gsh = new GoogleSheetsHelper("security-details.json", googleId);
                    var nameOfServer = c.serverName;

                    var row1 = new GoogleSheetRow();
                    var row2 = new GoogleSheetRow();
                    var row3 = new GoogleSheetRow();
                    

                    var cell1 = new GoogleSheetCell() { CellValue = "Сервер", IsBold = true };
                    var cell2 = new GoogleSheetCell() { CellValue = "База данных " };
                    var cell3 = new GoogleSheetCell() { CellValue = "Размер " };
                    var cell4 = new GoogleSheetCell() { CellValue = "Дата обновления" };

                    var cell5 = new GoogleSheetCell() { CellValue = $"{serverName}" };
                    var cell6 = new GoogleSheetCell() { CellValue = $"{dataBaseName}" };
                    var cell7 = new GoogleSheetCell() { CellValue = $"{freeSpaceDB}" };
                    var cell8 = new GoogleSheetCell() { CellValue = $"{now}" };

                    var cell9 = new GoogleSheetCell() { CellValue = $"{serverName}" };
                    var cell10 = new GoogleSheetCell() { CellValue = "Свободно" };
                    var cell11 = new GoogleSheetCell() { CellValue = $"{freeSpace} Гб" };
                    var cell12 = new GoogleSheetCell() { CellValue = $"{now}" };


                    row1.Cells.AddRange(new List<GoogleSheetCell>() { cell1, cell2, cell3, cell4 });
                    row2.Cells.AddRange(new List<GoogleSheetCell>() { cell5, cell6, cell7, cell8 });
                    row3.Cells.AddRange(new List<GoogleSheetCell>() { cell9, cell10, cell11, cell12 });

                    var rows = new List<GoogleSheetRow>() { row1, row2 };
                    var lastRow = new List<GoogleSheetRow>() { row3 };

                    gsh.AddCells(new GoogleSheetParameters() { SheetName = nameOfServer, RangeColumnStart = 1, RangeRowStart = 1 }, rows);
                    gsh.AddCells(new GoogleSheetParameters() { SheetName = nameOfServer, RangeColumnStart = 1, RangeRowStart = 20 }, lastRow);
                }
            }
        }
    }
}
