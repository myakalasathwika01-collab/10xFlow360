using _10xFlow360.Models;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace _10xFlow360
{
    public class SAPRestService
    {
        private readonly string baseUrl;

        public SAPRestService()
        {
            // SAP Service Layer is using HTTPS/TLS 1.2
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls12;

            // TEMPORARY:
            // Your SAP certificate is expired.
            // Remove this after the SAP certificate is renewed.
            ServicePointManager.ServerCertificateValidationCallback =
                delegate
                {
                    return true;
                };

            // Prevent .NET from waiting for 100-Continue.
            // This can cause problems with Apache/SAP Service Layer.
            ServicePointManager.Expect100Continue = false;

            baseUrl =
                ConfigurationManager.AppSettings[
                    "CurrentServiceURL"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new Exception(
                    "CurrentServiceURL is missing from Web.config.");
            }
        }

        // ============================================================
        // SAP LOGIN
        // ============================================================
        public bool ValidateUserCredentials(LoginVM loginData)
        {
            try
            {
                if (loginData == null)
                {
                    throw new Exception(
                        "Login information is required.");
                }

                if (string.IsNullOrWhiteSpace(
                    loginData.UserName))
                {
                    throw new Exception(
                        "Username is required.");
                }

                if (string.IsNullOrWhiteSpace(
                    loginData.Password))
                {
                    throw new Exception(
                        "Password is required.");
                }

                string companyDb =
                    ConfigurationManager.AppSettings[
                        "CompanyDB"];

                if (string.IsNullOrWhiteSpace(companyDb))
                {
                    throw new Exception(
                        "CompanyDB is missing from Web.config.");
                }

                // ----------------------------------------------------
                // EXACT SAP LOGIN JSON
                // ----------------------------------------------------
                SAPLoginRequest loginRequest =
                    new SAPLoginRequest();

                loginRequest.CompanyDB =
                    companyDb;

                loginRequest.UserName =
                    loginData.UserName.Trim();

                loginRequest.Password =
                    loginData.Password;

                JavaScriptSerializer serializer =
                    new JavaScriptSerializer();

                string json =
                    serializer.Serialize(loginRequest);

                byte[] requestBytes =
                    Encoding.UTF8.GetBytes(json);

                // ----------------------------------------------------
                // SAP LOGIN URL
                // ----------------------------------------------------
                string loginUrl =
                    baseUrl.TrimEnd('/') +
                    "/Login";

                // ----------------------------------------------------
                // CREATE REQUEST
                // ----------------------------------------------------
                HttpWebRequest request =
                    (HttpWebRequest)
                    WebRequest.Create(loginUrl);

                request.Method = "POST";

                request.ContentType =
                    "application/json";

                request.Accept =
                    "application/json";

                request.ProtocolVersion =
                    HttpVersion.Version11;

                request.KeepAlive = true;

                request.Timeout = 120000;

                request.ReadWriteTimeout = 120000;

                request.ContentLength =
                    requestBytes.Length;

                // Do not use an IIS/system proxy
                // for the internal SAP server.
                request.Proxy = null;

                // Match normal API clients.
                request.UserAgent =
                    "PostmanRuntime/7.45.0";

                // Prevent Expect: 100-continue.
                request.ServicePoint.Expect100Continue =
                    false;

                // ----------------------------------------------------
                // WRITE JSON BODY
                // ----------------------------------------------------
                using (Stream requestStream =
                    request.GetRequestStream())
                {
                    requestStream.Write(
                        requestBytes,
                        0,
                        requestBytes.Length);
                }

                // ----------------------------------------------------
                // GET SAP RESPONSE
                // ----------------------------------------------------
                using (HttpWebResponse response =
                    (HttpWebResponse)
                    request.GetResponse())
                {
                    string responseText = "";

                    using (Stream responseStream =
                        response.GetResponseStream())
                    {
                        if (responseStream != null)
                        {
                            using (StreamReader reader =
                                new StreamReader(
                                    responseStream))
                            {
                                responseText =
                                    reader.ReadToEnd();
                            }
                        }
                    }

                    // ------------------------------------------------
                    // CHECK STATUS
                    // ------------------------------------------------
                    if (response.StatusCode !=
                        HttpStatusCode.OK)
                    {
                        throw new Exception(
                            "SAP returned HTTP " +
                            ((int)response.StatusCode) +
                            " " +
                            response.StatusDescription +
                            ". Response: " +
                            responseText);
                    }

                    if (string.IsNullOrWhiteSpace(
                        responseText))
                    {
                        throw new Exception(
                            "SAP returned an empty login response.");
                    }

                    // ------------------------------------------------
                    // DESERIALIZE SAP RESPONSE
                    // ------------------------------------------------
                    SAPLoginResponse sapResponse =
                        serializer.Deserialize
                        <SAPLoginResponse>(
                            responseText);

                    if (sapResponse == null)
                    {
                        throw new Exception(
                            "Unable to read SAP login response.");
                    }

                    if (string.IsNullOrWhiteSpace(
                        sapResponse.SessionId))
                    {
                        throw new Exception(
                            "SAP login succeeded but SessionId " +
                            "was not returned.");
                    }

                    // ------------------------------------------------
                    // GET ROUTEID
                    // ------------------------------------------------
                    string routeId = null;

                    if (response.Cookies != null)
                    {
                        foreach (Cookie cookie
                            in response.Cookies)
                        {
                            if (cookie.Name.Equals(
                                "ROUTEID",
                                StringComparison
                                    .OrdinalIgnoreCase))
                            {
                                routeId =
                                    cookie.Value;

                                break;
                            }
                        }
                    }

                    // ------------------------------------------------
                    // SAVE SAP SESSION
                    // ------------------------------------------------
                    HttpContext.Current.Session[
                        "sessionId"] =
                        sapResponse.SessionId;

                    HttpContext.Current.Session[
                        "routeId"] =
                        routeId;

                    HttpContext.Current.Session[
                        "SAP_USER"] =
                        loginData.UserName.Trim();

                    HttpContext.Current.Session[
                        "SAP_COMPANY_DB"] =
                        companyDb;

                    HttpContext.Current.Session.Remove(
                        "SAP_LOGIN_ERROR");

                    return true;
                }
            }
            catch (WebException ex)
            {
                string error =
                    GetWebExceptionMessage(ex);

                HttpContext.Current.Session[
                    "SAP_LOGIN_ERROR"] =
                    error;

                return false;
            }
            catch (Exception ex)
            {
                HttpContext.Current.Session[
                    "SAP_LOGIN_ERROR"] =
                    ex.Message;

                return false;
            }
        }

        // ============================================================
        // GET SESSION ID
        // ============================================================
        public string GetSessionId()
        {
            if (HttpContext.Current.Session == null)
            {
                return null;
            }

            object value =
                HttpContext.Current.Session[
                    "sessionId"];

            return value == null
                ? null
                : value.ToString();
        }

        // ============================================================
        // GET ROUTE ID
        // ============================================================
        public string GetRouteId()
        {
            if (HttpContext.Current.Session == null)
            {
                return null;
            }

            object value =
                HttpContext.Current.Session[
                    "routeId"];

            return value == null
                ? null
                : value.ToString();
        }

        // ============================================================
        // CHECK SAP LOGIN
        // ============================================================
        public bool IsLoggedIn()
        {
            return !string.IsNullOrWhiteSpace(
                GetSessionId());
        }

        // ============================================================
        // LOGOUT
        // ============================================================
        public void Logout()
        {
            if (HttpContext.Current.Session != null)
            {
                HttpContext.Current.Session.Remove(
                    "sessionId");

                HttpContext.Current.Session.Remove(
                    "routeId");

                HttpContext.Current.Session.Remove(
                    "SAP_USER");

                HttpContext.Current.Session.Remove(
                    "SAP_COMPANY_DB");

                HttpContext.Current.Session.Remove(
                    "SAP_LOGIN_ERROR");
            }
        }

        // ============================================================
        // GET DETAILED SAP ERROR
        // ============================================================
        private string GetWebExceptionMessage(
            WebException ex)
        {
            try
            {
                StringBuilder message =
                    new StringBuilder();

                message.Append(
                    "SAP Service Layer Login Failed. ");

                message.Append(
                    "WebException Status: ");

                message.Append(
                    ex.Status);

                message.Append(
                    ". Message: ");

                message.Append(
                    ex.Message);

                if (ex.Response != null)
                {
                    HttpWebResponse response =
                        ex.Response as HttpWebResponse;

                    if (response != null)
                    {
                        message.Append(
                            " HTTP Status: ");

                        message.Append(
                            (int)response.StatusCode);

                        message.Append(" ");

                        message.Append(
                            response.StatusDescription);

                        string responseBody = "";

                        using (Stream stream =
                            response.GetResponseStream())
                        {
                            if (stream != null)
                            {
                                using (StreamReader reader =
                                    new StreamReader(stream))
                                {
                                    responseBody =
                                        reader.ReadToEnd();
                                }
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(
                            responseBody))
                        {
                            message.Append(
                                " Response: ");

                            message.Append(
                                responseBody);
                        }
                    }
                }

                return message.ToString();
            }
            catch
            {
                return ex.Message;
            }
        }
    }

    // ================================================================
    // SAP LOGIN REQUEST
    // ================================================================
    public class SAPLoginRequest
    {
        public string CompanyDB { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }
    }

    // ================================================================
    // SAP LOGIN RESPONSE
    // ================================================================
    public class SAPLoginResponse
    {
        public string SessionId { get; set; }

        public string Version { get; set; }

        public string RouteId { get; set; }

        public int SessionTimeout { get; set; }
    }
}