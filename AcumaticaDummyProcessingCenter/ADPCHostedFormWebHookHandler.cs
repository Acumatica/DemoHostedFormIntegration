using Newtonsoft.Json;
using PX.Api.Webhooks;
using PX.Common;
using PX.Data;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace AcumaticaDummyProcessingCenter
{
    public class ADPCHostedFormWebHookHandler : IWebhookHandler
    {

        public class HFRequest
        {
            public string Type { get; set; }
            public string CPID { get; set; }
            public string CustomerCD { get; set; }
            public string Token { get; set; }
            public string Cardtype { get; set; }
            public string Card { get; set; }
            public string ExpDate { get; set; }
            public decimal Amount { get; set; }
            public string Currency { get; set; }
            public string DocType { get; set; }
            public string DocRefNbr { get; set; }
            public Guid TranUID { get; set; }
            public string TransactionStatus { get; set; }
        }

        public class HFResponse {
            public string CPID { get; set; }
            public string PPID { get; set; }
        }
/*
        public async Task<System.Web.Http.IHttpActionResult> ProcessRequestAsync(
          HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using (var scope = GetAdminScope())
            {
            try
            {
                    string requestBody = await request.Content.ReadAsStringAsync();
                    HFRequest hFRequest = JsonConvert.DeserializeObject<HFRequest>(requestBody);
                    ADPCCustomerProfileEntry aDPCCustomerProfileEntry = PXGraph.CreateInstance<ADPCCustomerProfileEntry>();
                    ADPCCustomerProfile cp = new ADPCCustomerProfile();
                    if (!string.IsNullOrEmpty(hFRequest.CPID)) {
                        cp.CustomerProfileID = hFRequest.CPID;
                        aDPCCustomerProfileEntry.CustomerProfile.Current = cp;
                    }
                    else
                    {
                        cp.CustomerName = hFRequest.CustomerCD;
                        aDPCCustomerProfileEntry.CustomerProfile.Insert(cp);
                        aDPCCustomerProfileEntry.Save.Press();
                    }
                       
                    ADPCPaymentProfile pp = aDPCCustomerProfileEntry.PaymentProfiles.Insert();
                    pp.CustomerProfileID = aDPCCustomerProfileEntry.CustomerProfile.Current.CustomerProfileID;
                    pp.CardLastFour = hFRequest.Card.Substring(hFRequest.Card.Length - 4);
                    pp.Cardbin = hFRequest.Card.Substring(0, 6);
                    pp.CardType = hFRequest.Cardtype[0].ToString();

                    DateTime parsedExpDate;
                    pp.CardExpirationDate = DateTime.TryParseExact(hFRequest.ExpDate, "mm/yy", null, DateTimeStyles.None, out parsedExpDate) ? parsedExpDate : DateTime.Now.AddMonths(20);

                    aDPCCustomerProfileEntry.PaymentProfiles.Current = pp;
                    aDPCCustomerProfileEntry.Save.Press();

                    HFResponse hFResponse = new HFResponse();

                    hFResponse.CPID = aDPCCustomerProfileEntry.PaymentProfiles.Current.CustomerProfileID;
                    hFResponse.PPID = aDPCCustomerProfileEntry.PaymentProfiles.Current.PaymentProfileID.ToString();

                    if (!string.IsNullOrEmpty(hFRequest.Type) && hFRequest.Type !="CreateOnly")
                    {
                        var transactionMaint = PXGraph.CreateInstance<ADPCTransactionEntry>();
                        var transaction = transactionMaint.Transaction.Insert();
                        transaction.TransactionType = "A";
                        transaction.CustomerProfileID = aDPCCustomerProfileEntry.PaymentProfiles.Current.CustomerProfileID;
                        transaction.PaymentProfileID = aDPCCustomerProfileEntry.PaymentProfiles.Current.PaymentProfileID;
                        transaction.TransactionDocument = hFRequest.DocType + hFRequest.DocRefNbr;
                        transaction.TransactionAmount = hFRequest.Amount;
                        transaction.TransactionCurrency = hFRequest.Currency;
                        transaction.TransactionDate = DateTime.UtcNow;
                        transaction.Tranuid = hFRequest.TranUID;
                        transaction.TransactionStatus = string.IsNullOrEmpty(hFRequest.TransactionStatus) ? "A" : hFRequest.TransactionStatus;
                        transactionMaint.Save.Press();
                        return new TextResult(transactionMaint.Transaction.Current.TransactionID, request);
                    }

                    string text = JsonConvert.SerializeObject(hFResponse);
                    return new TextResult(text, request);

                }
            catch (Exception e) { 
                return new ExceptionResult(e, false, new DefaultContentNegotiator(), request, new[] { new JsonMediaTypeFormatter() });
            }
            }
        }
*/
        private IDisposable GetAdminScope()
        {
            var userName = "admin";
            if (PXDatabase.Companies.Length > 0)
            {
                var company = PXAccess.GetCompanyName();
                if (string.IsNullOrEmpty(company))
                {
                    company = PXDatabase.Companies[0];
                }
                userName = userName + "@" + company;
            }
            return new PXLoginScope(userName);
        }

        public async Task HandleAsync(WebhookContext context, CancellationToken cancellation)
        {
            if (context.Request.Method == "GET")
            {
                if (context.Request.Query.TryGetValue("Type", out var type))
                {
                    if (type == "CreateOnly")
                    {
                        RenderHostedForm(context);
                    }
                    else
                    {
                        RenderHostedPaymentForm(context);
                    }
                }
            }
            if (context.Request.Method == "POST")
            {
                using (var scope = GetAdminScope())
                {
                    try
                    {
                        string body = string.Empty;
                        Stream stream = context.Request.Body;
                        stream.Position = 0;
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            body = reader.ReadToEnd();
                        }

                        HFRequest hFRequest = JsonConvert.DeserializeObject<HFRequest>(body);

                        ADPCCustomerProfileEntry aDPCCustomerProfileEntry = PXGraph.CreateInstance<ADPCCustomerProfileEntry>();
                        ADPCCustomerProfile cp = new ADPCCustomerProfile();
                        if (!string.IsNullOrEmpty(hFRequest.CPID))
                        {
                            cp.CustomerProfileID = hFRequest.CPID;
                            aDPCCustomerProfileEntry.CustomerProfile.Current = cp;
                        }
                        else
                        {
                            cp = aDPCCustomerProfileEntry.CustomerProfile.Insert();
                            cp.CustomerName = hFRequest.CustomerCD;
                            aDPCCustomerProfileEntry.CustomerProfile.Update(cp);
                            aDPCCustomerProfileEntry.Save.Press();
                        }

                        ADPCPaymentProfile pp = aDPCCustomerProfileEntry.PaymentProfiles.Insert();
                        pp.CustomerProfileID = aDPCCustomerProfileEntry.CustomerProfile.Current.CustomerProfileID;
                        pp.CardLastFour = hFRequest.Card.Substring(hFRequest.Card.Length - 4);
                        pp.Cardbin = hFRequest.Card.Substring(0, 6);
                        pp.CardType = hFRequest.Cardtype[0].ToString();

                        DateTime parsedExpDate;
                        pp.CardExpirationDate = DateTime.TryParseExact(hFRequest.ExpDate, "mm/yy", null, DateTimeStyles.None, out parsedExpDate) ? parsedExpDate : DateTime.Now.AddMonths(20);

                        aDPCCustomerProfileEntry.PaymentProfiles.Current = pp;
                        aDPCCustomerProfileEntry.Save.Press();

                        HFResponse hFResponse = new HFResponse();

                        hFResponse.CPID = aDPCCustomerProfileEntry.PaymentProfiles.Current.CustomerProfileID;
                        hFResponse.PPID = aDPCCustomerProfileEntry.PaymentProfiles.Current.PaymentProfileID.ToString();

                        if (!string.IsNullOrEmpty(hFRequest.Type) && hFRequest.Type != "CreateOnly")
                        {
                            var transactionMaint = PXGraph.CreateInstance<ADPCTransactionEntry>();
                            var transaction = transactionMaint.Transaction.Insert();
                            transaction.TransactionType = "A";
                            transaction.CustomerProfileID = aDPCCustomerProfileEntry.PaymentProfiles.Current.CustomerProfileID;
                            transaction.PaymentProfileID = aDPCCustomerProfileEntry.PaymentProfiles.Current.PaymentProfileID;
                            transaction.TransactionDocument = hFRequest.DocType + hFRequest.DocRefNbr;
                            transaction.TransactionAmount = hFRequest.Amount;
                            transaction.TransactionCurrency = hFRequest.Currency;
                            transaction.TransactionDate = DateTime.UtcNow;
                            transaction.Tranuid = hFRequest.TranUID;
                            transaction.TransactionStatus = string.IsNullOrEmpty(hFRequest.TransactionStatus) ? "A" : hFRequest.TransactionStatus;
                            transactionMaint.Save.Press();
                            StreamWriter writer = new StreamWriter(context.Response.Body);
                            writer.Write(transactionMaint.Transaction.Current.TransactionID);
                            writer.Close();
                        } else {

                            string text = JsonConvert.SerializeObject(hFResponse);
                            StreamWriter writer = new StreamWriter(context.Response.Body);
                            writer.Write(text);
                            writer.Close();
                        }

                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.Message);
                    }
                }
            }
        }

        private void RenderHostedPaymentForm(WebhookContext context)
        {
            WebhookRequest request = context.Request;

            string body = string.Empty;
            Stream stream = request.Body;
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream))
            {
                body = reader.ReadToEnd();
            }

            var dict = context.Request.Query;            

            var view = $@"
            <!DOCTYPE html>
            <html xmlns=""http://www.w3.org/1999/xhtml"">
            <head>
            <title>CL Payment Communicator</title>
            <script type=""text/javascript"">

	            function SendToPC(){{
	
		            WHUrl = ""{dict["HFSKey"]}"";
		            var xmlhttp = new XMLHttpRequest();
				
		            xmlhttp.onreadystatechange = function() {{
			            if (xmlhttp.readyState == XMLHttpRequest.DONE) {{ 
			               if (xmlhttp.status == 200) {{
				               window.parent.px_callback.paymentCallback('action=transactResponse&response=' + xmlhttp.responseText);
			               }}
			               else if (xmlhttp.status == 400) {{
				              alert('There was an error 400');
			               }}
			               else {{
				               alert('something else other than 200 was returned' + xmlhttp.status);
			               }}
			            }}
		            }};

		            xmlhttp.open(""POST"", WHUrl);
		            xmlhttp.setRequestHeader(""Content-Type"", ""application/json"");
                    var data = new Object();
		            data.Cardtype = document.getElementById('cardTypeSelector').value;	
		            data.Card = document.getElementById('card').value;
		            data.ExpDate = document.getElementById('expdate').value;	
		            data.CustomerCD = ""{dict["CustomerCD"]}"";	
		            data.Token =  ""{dict["Token"]}"";	
		            data.CPID =    ""{dict["CPID"]}"";	
		            data.Type =  ""{dict["Type"]}"";	
		            data.Amount = {dict["Amount"]};	
		            data.Currency = ""{dict["Currency"]}"";	
		            data.DocType = ""{dict["DocType"]}"";	
		            data.DocRefNbr = ""{dict["DocRefNbr"]}"";	
		            data.TranUID = ""{dict["TranUID"]}"";	
		            xmlhttp.send(JSON.stringify(data));		
	            }}

	
            </script>
            </head>
            <body>
            <div id=""inRequest"">Parameters received from Acumatica: </br></br>{string.Join("</br>", dict.Select(_ => _.Key + ":" + _.Value))}<br></div>
            <div>
	            <input type=""text"" size=""40"" id=""card"" value=""4111111111111111""><br>
	            <input type=""text"" size=""40"" id=""expdate"" value=""12/25""><br>
	            <select id=""cardTypeSelector"">
		            <option selected value=""VIS"">Visa</option>
		            <option  value=""MSC"">MasterCard</option>
		            <option value=""AME"">American Express</option>
		            <option value=""UNI"">UnionPay</option>
	            </select><br>
	            <div id=""procResult"">
	            The transaction should be: 	
	            <select id=""procResultSelector"">
		            <option selected value=""0"">Approved</option>
		            <option  value=""1"">Declined</option>
		            <option value=""2"">Error</option>
		            <option value=""3"">HeldForReview</option>
		            <option value=""4"">Expired</option>
		            <option value=""5"">Unknown</option>
		            <option value=""6"">SettledSuccessfully</option>
		            <option value=""7"">Voided</option>
		            <option value=""8"">RefundSettledSuccessfully</option>
		            <option value=""9"">SettlementError</option>
		            <option value=""10"">GeneralError</option>
	            </select><br></div>
	            <input type=""button"" id=""Send"" value=""Pay"" onclick=""SendToPC();"">

            </div>
            </body>
            </html>
            ";

            StreamWriter writer = new StreamWriter(context.Response.Body);
            writer.Write(view);
            writer.Close();
        }

        private void RenderHostedForm(WebhookContext context) {
            WebhookRequest request = context.Request;

            string body = string.Empty;
            Stream stream = request.Body;
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream))
            {
                body = reader.ReadToEnd();
            }

            var dict = context.Request.Query;

            var view = $@"
            <!DOCTYPE html>
            <html xmlns=""http://www.w3.org/1999/xhtml"">
            <head>
            <title>CL Payment Communicator</title>
            <script type=""text/javascript"">

	            function SendToPC(){{
	
		            WHUrl = ""{dict["HFSKey"]}"";
		            var xmlhttp = new XMLHttpRequest();
				
		            xmlhttp.onreadystatechange = function() {{
			            if (xmlhttp.readyState == XMLHttpRequest.DONE) {{ 
			               if (xmlhttp.status == 200) {{
				               window.parent.px_callback.paymentCallback('action=transactResponse&response=' + xmlhttp.responseText);
			               }}
			               else if (xmlhttp.status == 400) {{
				              alert('There was an error 400');
			               }}
			               else {{
				               alert('something else other than 200 was returned' + xmlhttp.status);
			               }}
			            }}
		            }};

		            xmlhttp.open(""POST"", WHUrl);
		            xmlhttp.setRequestHeader(""Content-Type"", ""application/json"");
                    var data = new Object();
		            data.Cardtype = document.getElementById('cardTypeSelector').value;	
		            data.Card = document.getElementById('card').value;
		            data.ExpDate = document.getElementById('expdate').value;	
		            data.CustomerCD = ""{dict["CustomerCD"]}"";	
		            data.Token =  ""{dict["Token"]}"";	
		            data.CPID =    ""{dict["CPID"]}"";	
		            data.Type =  ""{dict["Type"]}"";		
		            xmlhttp.send(JSON.stringify(data));		
	            }}
	
            </script>
            </head>
            <body>
            <div id=""inRequest"">Parameters received from Acumatica: </br></br>{string.Join("</br>", dict.Select(_ => _.Key + ":" + _.Value))}<br></div>
            <div>
	            <input type=""text"" size=""40"" id=""card"" value=""4111111111111111""><br>
	            <input type=""text"" size=""40"" id=""expdate"" value=""12/25""><br>
	            <select id=""cardTypeSelector"">
		            <option selected value=""VIS"">Visa</option>
		            <option  value=""MSC"">MasterCard</option>
		            <option value=""AME"">American Express</option>
		            <option value=""UNI"">UnionPay</option>
	            </select><br>
	            <input type=""button"" id=""Send"" value=""Vault the Card"" onclick=""SendToPC();"">
            </div>
            </body>
            </html>
            ";

            StreamWriter writer = new StreamWriter(context.Response.Body);
            writer.Write(view);
            writer.Close();
        }
    }
}
