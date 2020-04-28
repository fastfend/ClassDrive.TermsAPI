using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Google.Apis.Drive.v3.PermissionsResource;

namespace ClassDrive.TermsAPI
{

    public class UserAdder
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/drive-dotnet-quickstart.json
        static readonly string[] Scopes = { DriveService.Scope.Drive };
        static string ApplicationName = "CherryPermissions";
        private const String Folder = "###REMOVED###";

        public void AddPermission(string mail)
        {
            UserCredential credential;

            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
            try
            {
                DeletePermission(service, Folder, mail);
            }
            catch
            {

            }
            Thread.Sleep(5000);
            InsertPermission(service, Folder, mail, "user", "writer");
        }

        /// <summary>
        /// Insert a new permission.
        /// </summary>
        /// <param name="service">Drive API service instance.</param>
        /// <param name="fileId">ID of the file to insert permission for.</param>
        /// <param name="who">
        /// User or group e-mail address, domain name or null for "default" type.
        /// </param>
        /// <param name="type">The value "user", "group", "domain" or "default".</param>
        /// <param name="role">The value "owner", "writer" or "reader".</param>
        /// <returns>The inserted permission, null is returned if an API error occurred</returns>
        private Permission InsertPermission(DriveService service, string fileId, string who, string type, string role)
        {
            Permission newPermission = new Permission
            {
                EmailAddress = who,
                Type = type,
                Role = role
            };

            try
            {
                CreateRequest req = service.Permissions.Create(newPermission, fileId);
                req.SendNotificationEmail = false;
                req.Execute();
            }
            catch (Exception e)
            {
                throw new Exception("An error occurred: " + e.Message);
            }
            return null;
        }

        private Permission DeletePermission(DriveService service, string fileId, string who)
        {
            try
            {
                DeleteRequest req = service.Permissions.Delete(fileId, GetPermissionIdForEmail(service, who));
                req.Execute();
            }
            catch (Exception e)
            {
                throw new Exception("An error occurred: " + e.Message);
            }
            return null;
        }

        public static string GetPermissionIdForEmail(DriveService service, string emailAddress)
        {
            string pageToken = null;

            do
            {
                var request = service.Files.List();
                request.Q = $"'{emailAddress}' in writers or '{emailAddress}' in readers or '{emailAddress}' in owners";
                request.Spaces = "drive";
                request.Fields = "nextPageToken, files(id, name, permissions)";
                request.PageToken = pageToken;

                var result = request.Execute();

                foreach (var file in result.Files.Where(f => f.Permissions != null))
                {
                    var permission = file.Permissions.SingleOrDefault(p => string.Equals(p.EmailAddress, emailAddress, StringComparison.InvariantCultureIgnoreCase));

                    if (permission != null)
                        return permission.Id;
                }

                pageToken = result.NextPageToken;

            } while (pageToken != null);

            return null;
        }

    }
}
