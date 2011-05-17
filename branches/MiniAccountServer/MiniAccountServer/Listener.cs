using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using MiniAccountServer.Database;

using MiniAccountServer.Models;
using Newtonsoft.Json;

namespace MiniAccountServer
{
    public class Listener
    {
        private HttpListener listener;
        private DatabaseClient client;

        private string[] prefixes = {@"http://0.0.0.0:1437/Account/"};

        public Listener()
        {
            client = new DatabaseClient();
            listener = new HttpListener();

            foreach(string prefix in prefixes)
            {
                listener.Prefixes.Add(prefix);
            }
        }

        public void Start()
        {
            listener.Start();
            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                HandleRequest(context);
            }

            listener.Close();
        }

        private void HandleRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            

            // Lets figure out the request...
            switch(request.HttpMethod)
            {
                // Sanity check request
                case "GET":
                    byte[] responseString = Encoding.UTF8.GetBytes("Works!");
                    response.ContentLength64 = responseString.Length;
                    response.OutputStream.Write(responseString, 0, responseString.Length);
                    response.OutputStream.Close();
                    break;

                // Account registration request
                case "PUT":

                    // 1. Is the request good?
                    if(!request.HasEntityBody)
                    {
                        response.StatusCode = 400;
                        response.OutputStream.Close();

                        break;
                    }

                    string registrationData = new StreamReader(request.InputStream).ReadToEnd();
                    var regModel = JsonConvert.DeserializeObject<Account.RegistrationRequestModel>(registrationData);

                    if (!regModel.IsRequestValid())
                    {
                        response.StatusCode = 400;
                        response.OutputStream.Close();

                        break;
                    }

                    
                    // 2. Is the username available?
                    if (client.UsernameExists(regModel.Username))
                    {
                        response.StatusCode = 403;
                        response.OutputStream.Close();

                        break;
                    }


                    // 3. Are the credentials good?
                    if (!Account.IsValidUsername(regModel.Username) || !Account.IsValidEmail(regModel.Email))
                    {
                        response.StatusCode = 406;
                        response.OutputStream.Close();

                        break;
                    }


                    // 4. Add it to the database, and we're good to go!
                    var account = client.AccountCreate(regModel.Username, regModel.PasswordHash,
                                                       Guid.NewGuid().ToString(),
                                                       DateTime.Now, DateTime.Now, 0, regModel.Email);


                    // 5. Oh uh? Some error happened!
                    if (account == null)
                    {
                        response.StatusCode = 500;
                        response.OutputStream.Close();

                        break;
                    }


                    // Done!
                    response.StatusCode = 201;
                    response.OutputStream.Close();
                    break;

                // Account login request
                case "POST":

                    // 1. Is the request good?
                    if (!request.HasEntityBody)
                    {
                        response.StatusCode = 400;
                        response.OutputStream.Close();

                        break;
                    }

                    string loginData = new StreamReader(request.InputStream).ReadToEnd();
                    var loginModel = JsonConvert.DeserializeObject<Account.LoginRequestModel>(loginData);

                    if (!loginModel.IsRequestValid())
                    {
                        response.StatusCode = 400;
                        response.OutputStream.Close();

                        break;
                    }


                    // 2. Are the credentials good?
                    if (!client.IsAccountValid(loginModel.Username, loginModel.PasswordHash))
                    {
                        response.StatusCode = 404;
                        response.OutputStream.Close();

                        break;
                    }


                    // 3. Good to go!
                    Account a = client.AccountLogin(loginModel.Username, loginModel.PasswordHash);

                    var loginResponseModel = new Account.LoginResponseModel();
                    loginResponseModel.Username = a.Username;
                    loginResponseModel.Email = a.Email;
                    loginResponseModel.TicketId = a.SessionId;
                    loginResponseModel.DateCreated = a.DateCreated;
                    loginResponseModel.LastAccessed = a.LastAccessed;
                    loginResponseModel.Permission = a.Permission;

                    var loginResponseString = JsonConvert.SerializeObject(loginResponseModel);
                    byte[] loginResponseData = Encoding.UTF8.GetBytes(loginResponseString);
                    response.StatusCode = 200;
                    response.ContentLength64 = loginResponseData.Length;
                    response.OutputStream.Write(loginResponseData, 0, loginResponseData.Length);
                    response.OutputStream.Close();

                    break;
            }
        }
    }
}
