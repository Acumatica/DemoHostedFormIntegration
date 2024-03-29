﻿using Acumatica.ADPCGateway.Api;
using Acumatica.ADPCGateway.Model;
using Acumatica.RESTClient.AuthApi;
using Acumatica.RESTClient.Auxiliary;
using Acumatica.RESTClient.Client;
using Acumatica.RESTClient.ContractBasedApi.Model;
using Acumatica.RESTClient.RootApi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Net.Security;
using System.Runtime.Serialization;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace AcumaticaDummyProcessingCenterGatewayAPI
{
    public class Requests
    {
        public string GetCreateCustomerProfileByCustomerCD(string url, string username, string password, string tenant, string customerCD, string customerName, string customerEmail)
        {
           // var authApi = new AuthApi(url);
            string result = string.Empty;
            int timeout = 100000;
            var Cookies = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = Cookies;
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
            var Client = new HttpClient(handler);
            Client.Timeout = new TimeSpan(0, 0, 0, 0, timeout);
            result = string.Empty;
            string postBody = $"{{\"name\":\"{username}\", \"password\":\"{password}\", \"tenant\": \"{tenant}\"}}";

            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = Client.PostAsync(url + "/entity/auth/login", new StringContent(postBody, Encoding.UTF8, "application/json")).GetAwaiter().GetResult();

            //if (response?.IsSuccessStatusCode == true)
            //{
            //    result = "Credentials correct. ";
            //}

            response = Client.GetAsync(url + $"/entity/ADPCGateway/1/CustomerProfile?$filter=CustomerName eq '{customerCD}'").GetAwaiter().GetResult();
            var p = response.EnsureSuccessStatusCode();
            CustomerProfile cp = ((List<CustomerProfile>)ApiClientHelpers.Deserialize<List<CustomerProfile>>(response)).FirstOrDefault();
            if (cp == null)
            {
                cp = new CustomerProfile
                {
                    CustomerName = customerCD,
                    CustomerDescription = customerName,
                    Email = customerEmail
                };

                string content = ApiClientHelpers.Serialize(cp);

                response = Client.PutAsync(url + $"/entity/ADPCGateway/1/CustomerProfile", new StringContent(content, Encoding.UTF8, "application/json")).GetAwaiter().GetResult();
                cp = (CustomerProfile)ApiClientHelpers.Deserialize<CustomerProfile>(response);
            }

            response = Client.PostAsync(url + "/entity/auth/logout", new StringContent(string.Empty, Encoding.UTF8, "application/json")).GetAwaiter().GetResult();

     
            return cp.CustomerProfileID;
        }

        public CustomerProfile GetCustomerProfileByCPID(string url, string username, string password, string tenant, string customerProfileId, bool withPaymentProfiles = false)
        {
            string result = string.Empty;
            int timeout = 100000;
            var Cookies = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = Cookies;
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
            var Client = new HttpClient(handler);
            Client.Timeout = new TimeSpan(0, 0, 0, 0, timeout);
            result = string.Empty;
            string postBody = $"{{\"name\":\"{username}\", \"password\":\"{password}\", \"tenant\": \"{tenant}\"}}";

            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = Client.PostAsync(url + "/entity/auth/login", new StringContent(postBody, Encoding.UTF8, "application/json")).GetAwaiter().GetResult();

            //if (response?.IsSuccessStatusCode == true)
            //{
            //    result = "Credentials correct. ";
            //}

            response = Client.GetAsync(url + $"/entity/ADPCGateway/1/CustomerProfile/{customerProfileId}" + (withPaymentProfiles ? "?$expand=PaymentProfiles" : "")).GetAwaiter().GetResult();
            var p = response.EnsureSuccessStatusCode();
            CustomerProfile cp = ((CustomerProfile)ApiClientHelpers.Deserialize<CustomerProfile>(response));
            response = Client.PostAsync(url + "/entity/auth/logout", new StringContent(string.Empty, Encoding.UTF8, "application/json")).GetAwaiter().GetResult();

            return cp;// new {CCPID = cp.CustomerProfileID, CustomerName = cp.CustomerDescription, CustomerCD = cp.CustomerName, Email = cp.Email };
        }

        public string GetHostedFormUrlKey(string url, string username, string password, string tenant) {
            string result = string.Empty;
            int timeout = 100000;
            var Cookies = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = Cookies;
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
            var Client = new HttpClient(handler);
            Client.Timeout = new TimeSpan(0, 0, 0, 0, timeout);
            result = string.Empty;
            string postBody = $"{{\"name\":\"{username}\", \"password\":\"{password}\", \"tenant\": \"{tenant}\"}}";

            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = Client.PostAsync(url + "/entity/auth/login", new StringContent(postBody, Encoding.UTF8, "application/json")).GetAwaiter().GetResult();

            response = Client.GetAsync(url + "/entity/ADPCGateway/1/HostedFormService?$filter=Active eq true and ImplementationClass eq 'AcumaticaDummyProcessingCenter.ADPCHostedFormWebHookHandler'").GetAwaiter().GetResult();
            var p = response.EnsureSuccessStatusCode();
            HostedFormService hfs = ((List<HostedFormService>)ApiClientHelpers.Deserialize<List<HostedFormService>>(response)).FirstOrDefault();
            response = Client.PostAsync(url + "/entity/auth/logout", new StringContent(string.Empty, Encoding.UTF8, "application/json")).GetAwaiter().GetResult();
            return hfs.webHookID.ToString();
        }

        public Transaction GetTransactionByID(string url, string username, string password, string tenant, string transactionID){
            string result = string.Empty;
            int timeout = 100000;
            var Cookies = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = Cookies;
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
            var Client = new HttpClient(handler);
            Client.Timeout = new TimeSpan(0, 0, 0, 0, timeout);
            result = string.Empty;
            string postBody = $"{{\"name\":\"{username}\", \"password\":\"{password}\", \"tenant\": \"{tenant}\"}}";

            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = Client.PostAsync(url + "/entity/auth/login", new StringContent(postBody, Encoding.UTF8, "application/json")).GetAwaiter().GetResult();

            //if (response?.IsSuccessStatusCode == true)
            //{
            //    result = "Credentials correct. ";
            //}

            response = Client.GetAsync(url + $"/entity/ADPCGateway/1/Transaction?$filter=TransactionID eq '{transactionID}'").GetAwaiter().GetResult();
            var p = response.EnsureSuccessStatusCode();
            Transaction tran = ((List<Transaction>)ApiClientHelpers.Deserialize<List<Transaction>>(response)).FirstOrDefault();
            response = Client.PostAsync(url + "/entity/auth/logout", new StringContent(string.Empty, Encoding.UTF8, "application/json")).GetAwaiter().GetResult();

            return tran;
            
        }

        public List<Transaction> GetUnsettledTransactions(string url, string username, string password, string tenant)
        {
            string result = string.Empty;
            int timeout = 100000;
            var Cookies = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = Cookies;
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
            var Client = new HttpClient(handler);
            Client.Timeout = new TimeSpan(0, 0, 0, 0, timeout);
            result = string.Empty;
            string postBody = $"{{\"name\":\"{username}\", \"password\":\"{password}\", \"tenant\": \"{tenant}\"}}";

            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = Client.PostAsync(url + "/entity/auth/login", new StringContent(postBody, Encoding.UTF8, "application/json")).GetAwaiter().GetResult();

            //if (response?.IsSuccessStatusCode == true)
            //{
            //    result = "Credentials correct. ";
            //}

            response = Client.GetAsync(url + $"/entity/ADPCGateway/1/Transaction?$filter=TransactionStatus ne 'Settled Successfully'").GetAwaiter().GetResult();
            var p = response.EnsureSuccessStatusCode();
            List<Transaction> trs = (List<Transaction>)ApiClientHelpers.Deserialize<List<Transaction>>(response);
            response = Client.PostAsync(url + "/entity/auth/logout", new StringContent(string.Empty, Encoding.UTF8, "application/json")).GetAwaiter().GetResult();

            return trs;
        }

        public string TestConnection(string SiteURL, string Username, string Password, string Tenant = null)
        {
            int timeout = 100000;
            var Cookies = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = Cookies;
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
            var Client = new HttpClient(handler);
            Client.Timeout = new TimeSpan(0, 0, 0, 0, timeout);
            string result = string.Empty;
            string postBody = $"{{\"name\":\"{Username}\", \"password\":\"{Password}\", \"tenant\": \"{Tenant}\"}}";

            Client.DefaultRequestHeaders.Accept .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = Client.PostAsync(SiteURL + "/entity/auth/login", new StringContent(postBody, Encoding.UTF8, "application/json")).GetAwaiter().GetResult();

            if (response?.IsSuccessStatusCode == true) {
                result = "Credentials correct. ";
            }

            response = Client.GetAsync(SiteURL + "/entity").GetAwaiter().GetResult();
            var p = response.EnsureSuccessStatusCode();
            if (response.Content.ReadAsStringAsync().GetAwaiter().GetResult().Contains("ADPCGateway")){
                result += "ADPC installed on the instance";
            }
            else
            {
                result += "ADPC is NOT installed on the instance";
            }

            response = Client.PostAsync(SiteURL + "/entity/auth/logout", new StringContent(string.Empty, Encoding.UTF8, "application/json")).GetAwaiter().GetResult();

            return result;
        }
    }
}