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
        private HttpListener httpListener;
        private DatabaseClient client;

        private string[] prefixes = {@"http://0.0.0.0:1437/Account/"};

        /// <summary>
        /// Generic Constructor
        /// </summary>
        public Listener()
        {
            client = new DatabaseClient();
            httpListener = new HttpListener();

            httpListener.Prefixes.Add("http://*:1010/");
        }

        /// <summary>
        /// Starts our program, exits if any errors occur
        /// </summary>
        public void Start()
        {
            //Is this OS supported?
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("HttpListener: Not supported on current system.");
                System.Threading.Thread.Sleep(5000);
            }
            else
            {
                try
                {
                    httpListener.Start();
                    System.Threading.Thread.Sleep(1000);

                    //Are we activated?
                    if (!httpListener.IsListening)
                    {
                        Console.WriteLine("Cannot start HttpListener... Exiting.");
                        System.Threading.Thread.Sleep(5000);
                        return;
                    }

                    Console.WriteLine("Listening....");
                    while (httpListener.IsListening)
                    {
                        HttpListenerContext context = httpListener.GetContext();
                        HandleRequest(context);
                    }
                }
                catch (Exception e)
                {
                    httpListener.Close();

                    Console.WriteLine(e.ToString());
                    System.Threading.Thread.Sleep(5000);
                }
            }
        }

        private void HandleRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            Console.WriteLine("Request {0}/{1} from {2}", request.RawUrl.ToString(), request.HttpMethod, request.RemoteEndPoint);
            try
            {
                // Lets figure out the request...
                switch (request.HttpMethod)
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
                        if (!request.HasEntityBody)
                        {
                            response.StatusCode = 400;
                            response.OutputStream.Close();

                            break;
                        }

                        string registrationData = new StreamReader(request.InputStream).ReadToEnd();
                        var regModel = JsonConvert.DeserializeObject<Account.RegistrationRequestModel>(registrationData);
                        // 2. Data valid?
                        if (!regModel.IsRequestValid())
                        {
                            response.StatusCode = 400;
                            response.OutputStream.Close();

                            break;
                        }

                        // 3. Is the username available?
                        if (client.UsernameExists(regModel.Username))
                        {
                            response.StatusCode = 403;
                            response.OutputStream.Close();

                            break;
                        }

                        // 4. Are the credentials good?
                        if (!Account.IsValidUsername(regModel.Username) || !Account.IsValidEmail(regModel.Email))
                        {
                            response.StatusCode = 406;
                            response.StatusDescription = (!Account.IsValidUsername(regModel.Username) ? "Invalid Username" : "Invalid Email");
                            response.OutputStream.Close();

                            break;
                        }
                        
                        // 5. Is the email already used?
			            if (client.EmailExists(regModel.Email))
			            {
			                response.StatusCode = 409;
                            response.StatusDescription = "Email already exists.";
			                response.OutputStream.Close();
					
			                break;
		        	    }
						
                        // Add it to the database, and we're good to go!
                        var account = client.AccountCreate(regModel.Username, regModel.PasswordHash,
                                                           Guid.NewGuid().ToString(),
                                                           DateTime.Now, DateTime.Now, 0, regModel.Email);


                        // 6. Oh uh? Some error happened!
                        if (account == null)
                        {
                            response.StatusCode = 500;
                            response.StatusDescription = "Account Creation Failed.";
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
                        // 2. Is the data valid?
                        if (!loginModel.IsRequestValid())
                        {
                            response.StatusCode = 400;
                            response.OutputStream.Close();

                            break;
                        }

                        // 3. Are the credentials good?
                        if (!client.IsAccountValid(loginModel.Username, loginModel.PasswordHash))
                        {
                            response.StatusCode = 404;
                            response.OutputStream.Close();

                            break;
                        }

                        // Try logging in
                        Account a = client.AccountLogin(loginModel.Username, loginModel.PasswordHash, request.RemoteEndPoint.Address.ToString());

                        // 4. Was it successful?
                        if (a == null)
                        {
                            response.StatusCode = 400;
                            response.StatusDescription = "Account doesn't exist.";
                            response.OutputStream.Close();

                            break;
                        }

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
            catch (Exception e)
            {
                if (e.Message.Contains("SQL"))
                    Console.WriteLine("Unhandled SQL exception");
            }
        }
    }
}
