﻿using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace InfLauncher.Helpers
{
    #region Application-level Status Codes

    /// <summary>
    /// Status codes for account registration requests.
    /// </summary>
    public enum RegistrationStatusCode
    {
        /// <summary>
        /// The account is successfully registered.
        /// </summary>
        Ok,

        /// <summary>
        /// The registration data sent is malformed.
        /// </summary>
        MalformedData,

        /// <summary>
        /// The requested username is already taken.
        /// </summary>
        UsernameTaken,

        /// <summary>
        /// The username or password combination is unsatisfactory.
        /// </summary>
        WeakCredentials,

        /// <summary>
        /// An unknown server error occured.
        /// </summary>
        ServerError,
    }

    /// <summary>
    /// Status codes for account login requests.
    /// </summary>
    public enum LoginStatusCode
    {
        /// <summary>
        /// The account is successfully logged in.
        /// </summary>
        Ok,

        /// <summary>
        /// The login data sent is malformed.
        /// </summary>
        MalformedData,

        /// <summary>
        /// The username or password are invalid.
        /// </summary>
        InvalidCredentials,

        /// <summary>
        /// An unknown server error occured.
        /// </summary>
        ServerError,
    }

    #endregion

    #region Application-level Response objects

    public class RegistrationResponse
    {
        internal RegistrationResponse(RegistrationStatusCode status)
        {
            Status = status;
        }

        public RegistrationStatusCode Status { get; private set; }
    }

    public class LoginResponse
    {
        internal LoginResponse(LoginStatusCode status, string guid)
        {
            Status = status;
            Guid = guid;
        }

        public LoginStatusCode Status { get; private set; }
        public string Guid { get; private set; }
    }

    #endregion

    /// <summary>
    /// Provides an interface to a RESTful protocol with the account server.
    /// </summary>
    public class RestConnection
    {
        /// <summary>
        /// The base URL address (including the port) of the account server.
        /// </summary>
        public static string BaseDomain = "http://localhost:52940";

        /// <summary>
        /// Relative path of the URL to create accounts.
        /// </summary>
        public static string RegisterUrl = String.Format("{0}/Account/Create", BaseDomain);

        /// <summary>
        /// Relative path of the URL to login.
        /// </summary>
        public static string LoginUrl = String.Format("{0}/Account/Login", BaseDomain);


        #region Response Delegates

        /// <summary>
        /// Called when a response has been received for a pending account registration.
        /// </summary>
        /// <param name="succeeded">True if account is successfully registered</param>
        public delegate void ReceivedRegisterAccountResponseDelegate(RegistrationResponse response);

        /// <summary>
        /// Called when a response has been received for a pending login.
        /// </summary>
        /// <param name="guid">If successful, the session id of this account; null if failed</param>
        public delegate void ReceivedLoginAccountResponseDelegate(LoginResponse response);

        /// <summary>
        /// Called when a response has been received for a pending account registration.
        /// </summary>
        public ReceivedRegisterAccountResponseDelegate OnRegisterAccountResponse;

        /// <summary>
        /// Called when a response has been received for a pending login.
        /// </summary>
        public ReceivedLoginAccountResponseDelegate OnLoginAccountResponse;

        #endregion


        #region Account Methods

        /// <summary>
        /// Starts the registration procedure with the account with the account server.
        /// 
        /// When the response (if any) is received, the listeners associated with OnRegisterAccountResponse will be notified.
        /// </summary>
        /// <param name="username">Account's username</param>
        /// <param name="password">Account's password</param>
        /// <returns>true if successfully started; false otherwise</returns>
        public bool BeginRegisterAccount(string username, string password)
        {
            if (username == null)
                throw new ArgumentNullException("username");
            if (password == null)
                throw new ArgumentNullException("password");

            // Create the registration form
            string passwordHash = CalculateHashFor(password);
            byte[] form = Encoding.UTF8.GetBytes(String.Format("username={0}&password={1}", username, passwordHash));

            // Create the request
            var request = (HttpWebRequest) WebRequest.Create(RegisterUrl);
            request.Method = "PUT";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = form.Length;

            try
            {
                var stream = request.GetRequestStream();
                stream.Write(form, 0, form.Length);
            }
            catch (WebException)
            {
                return false;
            }

            // Off it goes!
            request.BeginGetResponse(AsyncRegisterAccountResponse, request);
            return true;
        }

        /// <summary>
        /// Starts the login procedure to the account server.
        /// 
        /// When the response (if any) is received, the listeners associated with OnLoginAccountResponse will be notified.
        /// </summary>
        /// <param name="username">Account's username</param>
        /// <param name="password">Account's password</param>
        /// <returns>true if successfully started; false otherwise</returns>
        public bool BeginLoginAccount(string username, string password)
        {
            if (username == null)
                throw new ArgumentNullException("username");
            if (password == null)
                throw new ArgumentNullException("password");

            // Create the login form
            string passwordHash = CalculateHashFor(password);
            byte[] form = Encoding.UTF8.GetBytes(String.Format("username={0}&password={1}", username, passwordHash));

            // Create the request
            var request = (HttpWebRequest) WebRequest.Create(LoginUrl);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = form.Length;

            try
            {
                var stream = request.GetRequestStream();
                stream.Write(form, 0, form.Length);
            }
            catch (WebException)
            {
                return false;
            }

            // Off it goes!
            request.BeginGetResponse(AsyncLoginAccountResponse, request);
            return true;
        }

        #endregion


        #region HttpWebRequest Async Response Methods

        /// <summary>
        /// Called when a response is received for a pending account registration.
        /// </summary>
        /// <param name="result">Response data</param>
        private void AsyncRegisterAccountResponse(IAsyncResult result)
        {
            RegistrationStatusCode status = RegistrationStatusCode.ServerError;

            var request = result.AsyncState as HttpWebRequest;
            try
            {
                var response = (HttpWebResponse)request.EndGetResponse(result);

                if (HttpStatusCode.Created == response.StatusCode)
                {
                    status = RegistrationStatusCode.Ok;
                }
            }
            catch (WebException ex)
            {
                using (HttpWebResponse response = (HttpWebResponse)ex.Response)
                {
                    switch(response.StatusCode)
                    {
                        case HttpStatusCode.BadRequest:
                            status = RegistrationStatusCode.MalformedData;
                            break;

                        case HttpStatusCode.Forbidden:
                            status = RegistrationStatusCode.UsernameTaken;
                            break;

                        case HttpStatusCode.NotAcceptable:
                            status = RegistrationStatusCode.WeakCredentials;
                            break;

                        case HttpStatusCode.InternalServerError:
                            status = RegistrationStatusCode.ServerError;
                            break;
                    }
                }
            }

            OnRegisterAccountResponse(new RegistrationResponse(status));
        }

        /// <summary>
        /// Called when a response is received for a pending account login.
        /// </summary>
        /// <param name="result">Response data</param>
        private void AsyncLoginAccountResponse(IAsyncResult result)
        {
            LoginStatusCode status = LoginStatusCode.ServerError;
            string sessionId = null;

            var request = result.AsyncState as HttpWebRequest;

            try
            {
                var response = (HttpWebResponse)request.EndGetResponse(result);

                if(HttpStatusCode.OK == response.StatusCode)
                {
                    // Grab the session id.
                    var reader = new StreamReader(response.GetResponseStream());
                    var responseData = JsonConvert.DeserializeObject<LoginResponseData>(reader.ReadToEnd());

                    sessionId = responseData.SessionId.ToString();

                    status = LoginStatusCode.Ok;
                }
            }
            catch (WebException ex)
            {
                using(HttpWebResponse response = (HttpWebResponse)ex.Response)
                {
                    switch(response.StatusCode)
                    {
                        case HttpStatusCode.BadRequest:
                            status = LoginStatusCode.MalformedData;
                            break;

                        case HttpStatusCode.NotFound:
                            status = LoginStatusCode.InvalidCredentials;
                            break;

                        case HttpStatusCode.InternalServerError:
                            status = LoginStatusCode.ServerError;
                            break;
                    }
                }
            }

            OnLoginAccountResponse(new LoginResponse(status, sessionId));
        }

        #endregion


        #region Private Helper Methods

        /// <summary>
        /// Returns the SHA1 calculated hash for a given string |str|.
        /// </summary>
        /// <param name="str">The input string</param>
        /// <returns>SHA1 hashed output string</returns>
        private string CalculateHashFor(string str)
        {
            byte[] input = Encoding.ASCII.GetBytes(str);
            SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] result = sha.ComputeHash(input);

            var sb = new StringBuilder();

            foreach(byte b in result)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }

        #endregion
    }
}
