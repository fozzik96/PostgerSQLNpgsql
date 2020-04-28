using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using Google.Apis.Drive.v3;
using Google.Apis.Auth.OAuth2;
using System.Threading;
using Google.Apis.Util.Store;
using Google.Apis.Services;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Xml;

namespace PostgresSQLNpgsql
{
    class ConnectionCredentials
    {

        public string ConnectionStr { get; set; }
        
        public string serverName { get; set; }

        public string dbName { get; set; }


        /// <summary>
        ///  Метод для подключения к Google Drive и создания нового Google Sheet
        /// </summary>
        /// <returns></returns>
        public Google.Apis.Drive.v3.Data.File CreateSheet()
        {
            string[] scopes = new string[] { DriveService.Scope.Drive,
                      DriveService.Scope.DriveFile,};


            var clientId = ConfigurationManager.AppSettings["clientId"]; ;      
            var clientSecret = ConfigurationManager.AppSettings["clientSecret"];  
                                                                                      // here is where we Request the user to give us access, or use the Refresh Token that was previously stored in %AppData%  
            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            }, scopes,
            Environment.UserName, CancellationToken.None, new FileDataStore("MyAppsToken")).Result;
            //Once consent is recieved, your token will be stored locally on the AppData directory, so that next time you wont be prompted for consent.   
            DriveService _service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "MyAppName",

            });
            var _parent = "1Ha0GYN6r9_KWRVgN0PHOKpduvU83-2FK";//ID of folder if you want to create spreadsheet in specific folder
            var filename = "helloworld";
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = filename,
                MimeType = "application/vnd.google-apps.spreadsheet",
                //TeamDriveId = teamDriveID, // IF you want to add to specific team drive  
            };
            FilesResource.CreateRequest request = _service.Files.Create(fileMetadata);
            request.SupportsTeamDrives = true;
            fileMetadata.Parents = new List<string> { _parent }; // Parent folder id or TeamDriveID  
            request.Fields = "id";
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            var file = request.Execute();
            Console.WriteLine("File ID: " + file.Id);
            Configuration configManager = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            KeyValueConfigurationCollection confCollection = configManager.AppSettings.Settings;
            confCollection["googleId"].Value = file.Id;
            configManager.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(configManager.AppSettings.SectionInformation.Name);

            return file;
        }

    }
}
