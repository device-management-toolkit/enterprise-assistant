using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace OpenAMTEnterpriseAssistant
{
    public class OpenAMTEnterpriseAssistantHttpServer
    {
        private HttpListener _listener;
        private Thread _listenerThread;
        private OpenAMTEnterpriseAssistantServer _amtServer; // Reference to the AMT server
        private JavaScriptSerializer JSON = new JavaScriptSerializer();
        private string _userName;
        private string _password;
        private string _securityKey;

        // Constructor that takes an OpenAMTEnterpriseAssistantServer instance
        public OpenAMTEnterpriseAssistantHttpServer(OpenAMTEnterpriseAssistantServer amtServer)
        {
            _amtServer = amtServer;
        }
        public bool IsServerListening
        {
            get
            {
                return _listener != null && _listener.IsListening || false;
            }
        }

        public void StartServer()
        {
            string config = null;
            // Get our assembly path
            FileInfo fi = new FileInfo(Path.Combine(Assembly.GetExecutingAssembly().Location));
            String executablePath = fi.Directory.FullName;
            try { config = File.ReadAllText("config.txt"); } catch (Exception) { }
            if (config == null) { try { config = File.ReadAllText(Path.Combine(executablePath, "config.txt")); } catch (Exception) { } }
            if (config != null)
            {
                string[] configLines = config.Replace("\r\n", "\n").Split('\n');
                foreach (string configLine in configLines)
                {
                    int i = configLine.IndexOf('=');
                    if (i > 0)
                    {
                        string key = configLine.Substring(0, i).ToLower();
                        string val = configLine.Substring(i + 1);
                        if (key == "user") { 
                            _userName = val; 
                        }
                        if (key == "pass") {
                            _password = val;
                        }
                        if (key == "securitykey") {
                            _securityKey = val; 
                        }
                    }
                }
            }
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://*:8000/");
            _listener.Start();

            _listenerThread = new Thread(HandleRequests);
            _listenerThread.Start();
        }

        private void HandleRequests()
        {
            while (_listener.IsListening)
            {
                try
                {
                    var context = _listener.GetContext();
                    ProcessRequest(context);
                }
                catch (Exception ex)
                {
                    // Check how to leverage the Log
                    Console.Error.WriteLine("An error occurred: " + ex.Message);
                }
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            var requestUrl = context.Request.Url.AbsolutePath;
            var httpMethod = context.Request.HttpMethod;

            if (httpMethod == "POST" || httpMethod == "DELETE")
            {

                if (httpMethod == "POST")
                {
                    ProcessPostRequest(context, requestUrl);
                }
                else {
                    ProcessDeleteRequest(context, requestUrl);
                }
                
            }
            else
            {
                SendResponse(context, new Dictionary<string, object> { { "status", "Not a valid endpoint" } }, 404);
                return;
            }
        }

        private bool AuthenticateRequest(HttpListenerContext context, out Guid guidFromToken)
        {
            string authHeader = context.Request.Headers["Authorization"];
            guidFromToken = Guid.Empty;

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer"))
            {
                SendResponse(context, new Dictionary<string, object> { { "status", "Unauthorized" } }, 401);
                return false;
            }

            string jwtToken = authHeader.Substring("Bearer".Length).Trim();
            if (!ValidateJwtToken(jwtToken, out guidFromToken))
            {
                SendResponse(context, new Dictionary<string, object> { { "status", "Invalid Token" } }, 401);
                return false;
            }

            return true;
        }

        private bool ValidateGuidInUrl(string requestUrl, Guid guidFromToken)
        {
            string urlGuidString = requestUrl.Substring(requestUrl.LastIndexOf('/') + 1);
            return Guid.TryParse(urlGuidString, out Guid guidFromUrl) && guidFromToken == guidFromUrl;
        }



        private void ProcessPostRequest(HttpListenerContext context, string requestUrl)
        {
            var response = new Dictionary<string, object>();
            try
            {

                if (HandleAuthenticationRequest(context, requestUrl, out response) ||
                    HandleConfigurationRequest(context, requestUrl, out response))
                {
                    SendResponse(context, response, 200);
                    return;
                }
                else
                {
                    SendResponse(context, new Dictionary<string, object> { { "status", "BadRequest" } }, 400);
                }
            }
            catch (Exception ex)
            {
                HandleException(context, ex);
            }

        }

        private bool HandleConfigurationRequest(HttpListenerContext context, string requestUrl, out Dictionary<string, object> response)
        {
            response = new Dictionary<string, object>();
           
            if (!AuthenticateRequest(context, out Guid guidFromToken))
            {
                response = new Dictionary<string, object> { { "status", "Unauthorized" } }; 
                return false;
            }

            if (!ValidateGuidInUrl(requestUrl, guidFromToken))
            {
                SendResponse(context, new Dictionary<string, object> { { "status", "GUID Mismatch" } }, 401);
                return false;
            }

            var requestBody = ReadRequestBody(context.Request);
            // Determine the request type based on the URL
            var requestType = DetermineRequestType(requestUrl);
            // Validate the request and get the model
            var requestModel = ValidateRequest(requestBody, requestType);

            return ProcessSpecificConfigureRequest(context, requestType, requestModel, out response);
        }

        private bool ProcessSpecificConfigureRequest(HttpListenerContext context, string requestType, RequestModel requestModel, out Dictionary<string, object> response)
        {
            var message = requestModel.ToDictionary(); 
            // Add default values to the dictionary. 
            message["action"] = "satellite";
            message["satelliteFlags"] = 2;

            switch (requestType)
            {
                case "profile":
                    message["subaction"] = "802.1x-ProFile-Request";
                    response = _amtServer.RequestFor8021xProfile(message, true);
                    return true;
                case "keypair":
                    message["subaction"] = "802.1x-KeyPair-Response";
                    response = _amtServer.ResponseFor8021xKeyPairGeneration(message, true);
                    return true;
                case "csr":
                    message["subaction"] = "802.1x-CSR-Response";
                    response = _amtServer.ResponseFor8021xCSR(message, true);
                    return true;
                default:
                    response = null;
                    return false;
            }
        }

        public RequestModel ValidateRequest(string requestBody, string requestType)
        {
            RequestModel requestModel = null;
            List<string> validationErrors;

            switch (requestType)
            {
                case "profile":
                    requestModel = JsonSerializer.Deserialize<ProfileRequest>(requestBody);
                    break;
                case "keypair":
                    requestModel = JsonSerializer.Deserialize<KeypairRequest>(requestBody);
                    break;
                case "csr":
                    requestModel = JsonSerializer.Deserialize<CsrRequest>(requestBody);
                    break;
                default:
                    throw new ArgumentException("Invalid request type.");
            }

            if (requestModel != null && !requestModel.IsValid(out validationErrors))
            {
                throw new ArgumentException($"Validation failed: {string.Join(", ", validationErrors)}");
            }

            return requestModel;
        }

        private string DetermineRequestType(string requestUrl)
        {
            if (requestUrl.StartsWith("/api/configure/profile", StringComparison.OrdinalIgnoreCase))
                return "profile";
            if (requestUrl.StartsWith("/api/configure/keypair", StringComparison.OrdinalIgnoreCase))
                return "keypair";
            if (requestUrl.StartsWith("/api/configure/csr", StringComparison.OrdinalIgnoreCase))
                return "csr";
            throw new ArgumentException("Invalid request type.");
        }

        private bool HandleAuthenticationRequest(HttpListenerContext context, string requestUrl, out Dictionary<string, object> response)
        {
            response = new Dictionary<string, object>();

            if (!requestUrl.StartsWith("/api/authenticate", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string guidString = requestUrl.Substring("/api/authenticate/".Length);
            if (!ValidateCredentials(context) && Guid.TryParse(guidString, out Guid guid))
            {
                SendResponse(context, new Dictionary<string, object> { { "status", "Unauthorized" } }, 401);
                return true;
            }

            var jwtToken = GenerateJwtToken(guidString, 60); 
            response = new Dictionary<string, object> { { "jwtToken", jwtToken } };
            return true;
        }

        private void HandleException(HttpListenerContext context, Exception ex)
        {
            var response = new Dictionary<string, object>
            {
                ["status"] = "Error",
                ["details"] = "Error processing request: " + ex.Message // Consider logging the full exception
            };
            SendResponse(context, response, 500);
        }
        
        private void ProcessDeleteRequest(HttpListenerContext context, string requestUrl)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();
            try
            {
                // Read the request body
                string requestBody = ReadRequestBody(context.Request);
                var message = ParseRequestBody(requestBody);
                if (requestUrl.StartsWith("/api/configure", StringComparison.OrdinalIgnoreCase))
                {
                    _amtServer.RequestFor8021xProfileRemoval(message);
                }
                // Send response back to the client...
                SendResponse(context, null, 200);
            }
            catch (Exception)
            {
                response["status"] = "Error";
                response["details"] = "Error processing request";
                SendResponse(context, response, 500);
            }

        }

        private bool ValidateCredentials(HttpListenerContext context)
        {
            var requestBody = ReadRequestBody(context.Request);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var message = JsonSerializer.Deserialize<Dictionary<string, string>>(requestBody, options);

            string username = message["username"] as string;
            string password = message["password"] as string;

            if (username == null || password == null)
            {
                return false; 
            }

            return (username == _userName && password == _password);
        }

        private bool ValidateJwtToken(string token, out Guid guid)
        {
            guid = Guid.Empty;
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("9EmRJTbIiIb4bIeSsmgcWIjrR6HyETqc")),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;

                // Extract the GUID claim
                var guidClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "deviceId");
                if (guidClaim != null && Guid.TryParse(guidClaim.Value, out guid))
                {
                    return true; // Token and GUID are valid
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        private string GenerateJwtToken(string device, int expirationMinutes)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_securityKey)); // Replace with your secret key
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim("deviceId", device),
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string ReadRequestBody(HttpListenerRequest request)
        {
            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                return reader.ReadToEnd();
            }
        }

        private Dictionary<string, object> ParseRequestBody(string requestBody)
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);
        }

        private void SendResponse(HttpListenerContext context, Dictionary<string, object> message, int statusCode)
        {
            string response = JSON.Serialize(message);
            var buffer = System.Text.Encoding.UTF8.GetBytes(response);

            context.Response.StatusCode = statusCode;
            context.Response.ContentLength64 = buffer.Length;
            var responseOutput = context.Response.OutputStream;
            responseOutput.Write(buffer, 0, buffer.Length);
            context.Response.Close();
        }

        public void StopServer()
        {
            if (_listener != null)
            {
                _listener.Stop();
                _listener.Close();
            }

            if (_listenerThread != null)
            {
                _listenerThread.Abort();
            }
        }
    }
}
