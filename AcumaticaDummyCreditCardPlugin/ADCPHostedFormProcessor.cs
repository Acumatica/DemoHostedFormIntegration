using Acumatica.ADPCGateway;
using Acumatica.ADPCGateway.Model;
using PX.CCProcessingBase;
using PX.CCProcessingBase.Interfaces.V2;
using PX.Common;
using PX.Data.Update.ExchangeService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.UI;
using static PX.Objects.CR.OUMessage;
using static PX.SM.WikiRevision;

namespace AcumaticaDummyCreditCardPlugin
{
    public class ADCPHostedFormProcessor : ICCHostedFormProcessor
    {
        private IEnumerable<SettingsValue> settingValues;

        public ADCPHostedFormProcessor(IEnumerable<SettingsValue> settingValues)
        {
            this.settingValues = settingValues;
        }
        public HostedFormData GetDataForCreateForm(CustomerData customerData, AddressData addressData)
        {
            string hFSUrl, baseUrl, connectorFrame = string.Empty;

            if (HttpContext.Current?.Request?.UrlReferrer != null && HttpContext.Current?.Request?.Url != null)
            {
                baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
                var baseUrl2 = VirtualPathUtility.ToAbsolute("~/");
                connectorFrame = baseUrl + baseUrl2 + "/Frames/PaymentConnector.html";
            }

            string hfkey = Requests.GetHostedFormUrlKey(ADCPHelper.GetPCGredentials(settingValues));
            string serviceURL = ComposeHFServiceUrl(settingValues, hfkey);
            bool isCustomForm = settingValues.First(x => x.DetailID == ADCPConstants.ADPCUseCustomHF).Value == ADCPConstants.DefaultADPCTrue;
            
            if (isCustomForm)
            {
                hFSUrl = connectorFrame.Replace("PaymentConnector.html", "ADCPPaymentConnector.html");
            }
            else
            {
                hFSUrl = serviceURL;
            }

            Dictionary<string, string> parms = new Dictionary<string, string>()
            {
                {"Type", "CreateOnly"},
                {"HFSKey", serviceURL},
                {"CPID", customerData.CustomerProfileID},
                {"CustomerCD", customerData.CustomerCD},
                {"Width", "400" },
                {"Height", "350"},
                {"Callback", connectorFrame} 
            };

            return new HostedFormData() {
                Caption = "Create Payment Profile",
                Url = hFSUrl,
                UseGetMethod = true,
                Token = "CLTokenHostedForm",
                Parameters = parms
            };
        }

        private string ComposeHFServiceUrl(IEnumerable<SettingsValue> settingValues, string hfkey)
        { 
            string url =settingValues.First(x => x.DetailID == ADCPConstants.ADPCURL).Value;
            string tenant = settingValues.First(x => x.DetailID == ADCPConstants.ADPCTenant).Value;
            string hFSUrl = string.Format("{0}/Webhooks/{1}/{2}", url, tenant, hfkey);

            return hFSUrl;
        }

        public HostedFormData GetDataForManageForm(CustomerData customerData, CreditCardData cardData)
        {
            throw new System.NotImplementedException();
        }
    }
}