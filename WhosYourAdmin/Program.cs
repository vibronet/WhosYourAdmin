using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace WhosYourAdmin
{
    class Program
    {
        static void Main(string[] args)
        {
            AuthenticationContext ac = new AuthenticationContext("https://login.microsoftonline.com/common");
            AuthenticationResult ar = ac.AcquireTokenAsync("https://graph.windows.net", "f357e8fa-87dd-44f2-84f4-40c2a53e3daf", new Uri("http://whatevs"), new PlatformParameters(PromptBehavior.Auto)).Result;

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ar.AccessToken);
            HttpResponseMessage response = client.GetAsync("https://graph.windows.net/myorganization/directoryRoles?api-version=1.6").Result;
            string rez = response.Content.ReadAsStringAsync().Result;

            JObject jo = JObject.Parse(rez);

            JObject compayAdminRole = jo["value"].Values<JObject>().Where(m => m["displayName"].Value<string>() == "Company Administrator").FirstOrDefault();
            string roleObjID = compayAdminRole["objectId"].ToString();

            response = client.GetAsync(string.Format("https://graph.windows.net/myorganization/directoryRoles/{0}/members?api-version=1.6", roleObjID)).Result;

            rez = response.Content.ReadAsStringAsync().Result;
            jo = JObject.Parse(rez);

            foreach (var adm in jo["value"])
            {
                Console.WriteLine("Admin found: {0} - {1} - {2}",adm["displayName"], adm["mail"], adm["userPrincipalName"]);
            }
            //var roleID = from c in jo["value"]
            //             where c["displayName"].ToString() == "Company Administrator"
            //             select c["objectId"];
        }
    }
}
