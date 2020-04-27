using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Policy;
using System.Xml;
using Npgsql; // Импорт элементов для подключения к PostgreSQL базе данных

namespace PostgerSQLNpgsql
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime now = DateTime.Now;
            var googleId = ConfigurationManager.AppSettings["GoogleId"]; 
            //---- Обработка списка серверов PostgreSQL 
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
                        Console.WriteLine($"Server connection string:{childnode.InnerText}");
                        connectionCredentials.ConnectionStr = childnode.InnerText;
                    }
                    servers.Add(connectionCredentials); // Получаем имя атрибута сервера из ServerList.xml
                }
                foreach (ConnectionCredentials c in servers)
                {
                    Console.WriteLine($"{c.ConnectionStr} "); // Список всех credentials из ServerList.xml
                    using var con = new NpgsqlConnection(c.ConnectionStr); // Подключаемся к базе данных , подставляя список credentials
                    con.Open();

                    var sqlQuerryDB = "SELECT * FROM current_catalog; SELECT current_catalog ";
                    var sqlQuerry = "SELECT pg_database_size('postgres')";
                    var sqlQuerryName = "SELECT boot_val,reset_val FROM pg_settings WHERE name = 'listen_addresses'";

                    using var cmd = new NpgsqlCommand(sqlQuerry, con);
                    using var cmd2 = new NpgsqlCommand(sqlQuerryDB, con);
                    using var cmd3 = new NpgsqlCommand(sqlQuerryName, con);

                    var sizeOfDb = cmd.ExecuteScalar().ToString();
                    var dataBaseName = cmd2.ExecuteScalar().ToString();
                    var serverName = cmd3.ExecuteScalar().ToString();

                    Console.WriteLine($"Server Name: {serverName} Database Name: {dataBaseName} DB size: {sizeOfDb}");

                    var gsh = new GoogleSheetsHelper("security-details.json", googleId);
                    var nameOfServer = c.serverName;

                    var row1 = new GoogleSheetRow();
                    var row2 = new GoogleSheetRow();
                    var row3 = new GoogleSheetRow();
                    var row4 = new GoogleSheetRow();

                    var cell1 = new GoogleSheetCell() { CellValue = "Server", IsBold = true };
                    var cell2 = new GoogleSheetCell() { CellValue = "Database " };
                    var cell3 = new GoogleSheetCell() { CellValue = "SizeDB" };
                    var cell4 = new GoogleSheetCell() { CellValue = "Date" };

                    var cell5 = new GoogleSheetCell() { CellValue = $"{serverName}" };
                    var cell6 = new GoogleSheetCell() { CellValue = $"{dataBaseName}" };
                    var cell7 = new GoogleSheetCell() { CellValue = $"{sizeOfDb}" };
                    var cell8 = new GoogleSheetCell() { CellValue = $"{now}" };

                    row1.Cells.AddRange(new List<GoogleSheetCell>() { cell1, cell2, cell3, cell4 });
                    row2.Cells.AddRange(new List<GoogleSheetCell>() { cell5, cell6, cell7, cell8 });

                    var rows = new List<GoogleSheetRow>() { row1, row2, row3, row4 };

                    gsh.AddCells(new GoogleSheetParameters() { SheetName = nameOfServer, RangeColumnStart = 1, RangeRowStart = 1 }, rows);
                }
            }
        }
    }
}
