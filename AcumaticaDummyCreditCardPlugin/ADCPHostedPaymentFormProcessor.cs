using Acumatica.ADPCGateway;
using PX.CCProcessingBase;
using PX.CCProcessingBase.Interfaces.V2;
using PX.Common;
using PX.Data;
using PX.Objects.AR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProcessingInput = PX.CCProcessingBase.Interfaces.V2.ProcessingInput;

namespace AcumaticaDummyCreditCardPlugin
{
    public class ADCPHostedPaymentFormProcessor : ICCHostedPaymentFormProcessor
    {
        private IEnumerable<SettingsValue> settingValues;

        public ADCPHostedPaymentFormProcessor(IEnumerable<SettingsValue> settingValues)
        {
            this.settingValues = settingValues;
        }
        public HostedFormData GetDataForPaymentForm(ProcessingInput inputData)
        {
            string  hostedFormURL;
            hostedFormURL = PXContext.Session["CCPaymentConnectorUrl"] as string;                                       //Case when Hosted Payment Form called from Sales Orders Screen

            if (string.IsNullOrEmpty(hostedFormURL))
            {
                hostedFormURL = CCPaymentProcessingHelper.GetPaymentConnectorUrl(HttpContext.Current);                   //Case for Hosted Payment Form called fom Sales Order Screen for Acumatica ERP version >= 2022 R2
            }

            string hfkey = Requests.GetHostedFormUrlKey(ADCPHelper.GetPCGredentials(settingValues));
            string HFSUrl = ComposeHFSUrl(settingValues, hfkey);

            Dictionary<string, string> parms = new Dictionary<string, string>()
            {
                {"Type",         inputData.TranType.ToString()},
                {"HFSKey",       HFSUrl},
                {"CPID",         inputData.CustomerData.CustomerProfileID},
                {"CustomerCD",   inputData.CustomerData.CustomerCD},
                {"CustomerName", inputData.CustomerData.CustomerName},
                {"Email",        inputData.CustomerData.Email},
                {"Amount",       inputData.Amount.ToString()},
                {"Currency",     inputData.CuryID},
                {"DocType",      inputData.DocumentData.DocType},
                {"DocRefNbr",    inputData.DocumentData.DocRefNbr},
                {"TranUID",      inputData.TranUID.ToString()},                                                          //TranUid implementation for 2021 R1
                {"CallBack",     hostedFormURL}
            };
            
            return new HostedFormData()
            {
                Caption = "CLCharge",
                Url     = HFSUrl,
                UseGetMethod = true,
                Token   = "CLTokenHostedPaymentForm",
                Parameters = parms
            };
        }

        private string ComposeHFSUrl(IEnumerable<SettingsValue> settingValues, string hfkey)
        {
            string url = settingValues.First(x => x.DetailID == ADCPConstants.ADPCURL).Value;
            string tenant = settingValues.First(x => x.DetailID == ADCPConstants.ADPCTenant).Value;

            string hFSUrl = string.Format("{0}/Webhooks/{1}/{2}", url, tenant, hfkey);
            return hFSUrl;
        }

    }
}