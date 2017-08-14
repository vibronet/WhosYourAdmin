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
    // simple snippet meant to help you find your tenant's administrators
    class Program
    {
        static void Main(string[] args)
        {
            // get a token to call the graph API
            // note: won't work if your user is a guest. In that case, substitute "common" with your tenant identifier
            AuthenticationContext ac = new AuthenticationContext("https://login.microsoftonline.com/common");
            AuthenticationResult ar = ac.AcquireTokenAsync("https://graph.windows.net", "f357e8fa-87dd-44f2-84f4-40c2a53e3daf", new Uri("http://whatevs"), new PlatformParameters(PromptBehavior.Auto)).Result;

            Console.WriteLine("Searching for admins in tenant {0}", ar.TenantId);

            // get all the roles in the directory to find out the ID of the "company administrators" in this tenant
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ar.AccessToken);
            HttpResponseMessage response = client.GetAsync("https://graph.windows.net/myorganization/directoryRoles?api-version=1.6").Result;
            string rez = response.Content.ReadAsStringAsync().Result;
            JObject jo = JObject.Parse(rez);
            JObject companyAdminRole = jo["value"].Values<JObject>().Where(m => m["displayName"].Value<string>() == "Company Administrator").FirstOrDefault();
            string roleObjID = companyAdminRole["objectId"].ToString();

            // get all the members of the role
            // note: no pagination support
            response = client.GetAsync(string.Format("https://graph.windows.net/myorganization/directoryRoles/{0}/members?api-version=1.6", roleObjID)).Result;

            rez = response.Content.ReadAsStringAsync().Result;
            jo = JObject.Parse(rez);

            // print out some contact info
            foreach (var adm in jo["value"])
            {
                // we only want people you can email, not bots
                if (adm["objectType"].ToString() == "User")
                {
                    Console.WriteLine("Admin found: {0} ", adm["displayName"]);
                    // note: UPN!= email. Also, guess users will have an odd ID here
                    Console.WriteLine("UPN: {0} ", adm["userPrincipalName"]);
                    // note: mail is not always populated (for example in case of guest users).
                    // Try getting otherMails too, for example. 
                    Console.WriteLine("Email: {0}",adm["mail"]);
                    Console.WriteLine("==========================");
                }
            }
            Console.ReadLine();
        }
    }
}
