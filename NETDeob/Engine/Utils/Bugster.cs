/*
 * Bugster error reporter - C# client
 * Copyright (C) 2012 0xDEADDEAD (Dextrey)
 * ------------------------------------------------------
 * 
 * NOTE: If SSL is enabled you must remember that ServicePointManager.ServerCertificateValidationCallback
 * is temporarily changed during report delivery process to accept the self-signed certificate used by Bugster server.
 * It's a bad idea to run any code dealing with SSL web requests or ServicePointManager.ServerCertificateValidationCallback
 * in another thread when report is being sent by Bugster.
 */

using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace NETDeob.Core.Engine.Utils
{
    public interface IExceptionFormatter
    {
        string Format(Exception exception);
    }

    public class BugReporter
    {
        private const string ReportGenerator = "BugReporterC#";
        private const string Server = "http://bugster.dextrey.dy.fi/report.json";
        private const string SslServer = "https://bugster.dextrey.dy.fi/report.json";
        private const string SslThumbprint = "BD0F264EDED68A63B82C4802244AEB11413AB69E";

        private readonly string _apiKey;
        private readonly IExceptionFormatter _exceptionFormatter;

        /// <summary>
        /// Gets called before report is sent to the server.
        /// Return false from the callback to prevent report from being sent.
        /// </summary>
        public Func<Exception, bool> UserAuthorizationCheck { get; set; }

        /// <summary>
        /// Raised when report delivery has either succeeded or failed
        /// </summary>
        public EventHandler<ReportCompletionEventArgs> ReportCompleted;

        /// <summary>
        /// Set to choose whether SSL connection should be used
        /// </summary>
        public bool UseSSL = true;

        public BugReporter(string apiKey, IExceptionFormatter exceptionFormatter = null)
        {
            _apiKey = apiKey;
            if (exceptionFormatter == null)
            {
                _exceptionFormatter = new DefaultExceptionFormatter();
            }
            else
            {
                _exceptionFormatter = exceptionFormatter;
            }

            UserAuthorizationCheck = x => true;
        }

        public bool ManualReport(string message, string @class = "Manual")
        {
            var builder = new StringBuilder();
            builder.AppendLine("Unhandled exception - caught at " + DateTime.Now.ToString(new CultureInfo("en-US")));
            builder.AppendLine();
            builder.AppendLine(message);

            var json = BuildRequestJson(message, @class);
            try
            {
                if (!DoRequest(json))
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {

            var builder = new StringBuilder();
            builder.AppendLine("Unhandled exception - caught at " + DateTime.Now.ToString(new CultureInfo("en-US")));
            builder.AppendLine();
            builder.AppendLine(_exceptionFormatter.Format(e.ExceptionObject as Exception));

            if (IsDeliveryAuthorized(e.ExceptionObject as Exception))
            {
                DeliverReportToServer(builder.ToString(), "Unhandled exception");
            }
        }

        private bool IsDeliveryAuthorized(Exception exception)
        {
            /*TODO: more checks can be added here */
            return UserAuthorizationCheck(exception);
        }

        private void DeliverReportToServer(string message, string @class)
        {
            var json = BuildRequestJson(message, @class);
            try
            {
                if (!DoRequest(json))
                {
                    RaiseReportCompleted(false, false); // failure due to server result
                    return;
                }
            }
            catch
            {
                RaiseReportCompleted(false, true); // unexpected failure
                return;
            }
            RaiseReportCompleted(true, false); // success :P
        }

        private void RaiseReportCompleted(bool succeeded, bool failedThanksToException)
        {
            if (ReportCompleted != null)
            {
                ReportCompleted(this, new ReportCompletionEventArgs(succeeded, failedThanksToException));
            }
        }

        private bool DoRequest(string json)
        {
            var request = WebRequest.Create(UseSSL ? SslServer : Server) as HttpWebRequest;
            if (request == null)
                throw new Exception("");

            request.ServicePoint.Expect100Continue = false;
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Accept = "text/javascript";
            request.Timeout = 10 * 1000;

            var oldCallback = ServicePointManager.ServerCertificateValidationCallback;
            ServicePointManager.ServerCertificateValidationCallback = ServerCertificateValidationCallback;
            string responseString = null;
            try
            {
                using (var requestWriter = new StreamWriter(request.GetRequestStream()))
                {
                    requestWriter.Write(json);
                }

                var response = request.GetResponse() as HttpWebResponse;
                using (var responseReader = new StreamReader(response.GetResponseStream()))
                {
                    responseString = responseReader.ReadToEnd();
                }

            }
            catch
            {
                throw;
            }
            finally
            {
                ServicePointManager.ServerCertificateValidationCallback = oldCallback;
            }
            if (responseString.Trim() == "OK")
            {
                return true;
            }
            return false;
        }

        private bool ServerCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (!(certificate is X509Certificate2))
            {
                return false;
            }
            return (certificate as X509Certificate2).Thumbprint == SslThumbprint;
        }

        private string BuildRequestJson(string message, string @class)
        {
            var package = new JsonRequestPackage();
            package.ApiKey = _apiKey;
            package.Generator = ReportGenerator;
            package.Format = "plain";
            package.Content = message;
            package.Class = @class;

            var jsonSerializer = new DataContractJsonSerializer(typeof(JsonRequestPackage));
            var stream = new MemoryStream();
            jsonSerializer.WriteObject(stream, package);

            return Encoding.UTF8.GetString(stream.ToArray());
        }

        [DataContract]
        private class JsonRequestPackage
        {
            [DataMember(Name = "api_key")]
            public string ApiKey;

            [DataMember(Name = "generator")]
            public string Generator;

            [DataMember(Name = "format")]
            public string Format;

            [DataMember(Name = "report_class")]
            public string Class;

            [DataMember(Name = "content")]
            public string Content;
        }
    }

    public class ReportCompletionEventArgs : EventArgs
    {
        /// <summary>
        /// Indicates if they report was succesfully delivered or not
        /// </summary>
        public bool WasSuccesful { get; private set; }

        /// <summary>
        /// Set to true if delivery of the report failed due to exception during the delivery process
        /// False if failure of delivery was due to server refusing the request
        /// </summary>
        public bool FailedDueToException { get; private set; }

        public ReportCompletionEventArgs(bool succesful, bool failureDueToException)
        {
            WasSuccesful = succesful;
            FailedDueToException = failureDueToException;
        }
    }


    /// <summary>
    /// Provides simple exception formatting with Exception.ToString method
    /// </summary>
    internal class DefaultExceptionFormatter : IExceptionFormatter
    {
        public string Format(Exception exception)
        {
            var builder = new StringBuilder();
            builder.Append(exception.ToString());
            return builder.ToString();
        }
    }
}