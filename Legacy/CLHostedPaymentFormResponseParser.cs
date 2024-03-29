﻿using Newtonsoft.Json.Linq;
using PX.CCProcessingBase.Interfaces.V2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CookielessHostedForm
{
    public class CLHostedPaymentFormResponseParser : ICCHostedPaymentFormResponseParser
    {
        private IEnumerable<SettingsValue> settingValues;

        public CLHostedPaymentFormResponseParser(IEnumerable<SettingsValue> settingValues)
        {
            this.settingValues = settingValues;
        }
        public HostedFormResponse Parse(string input)
        {
            string token, trantype, amount, ccpid, docType, docRefNbr, tranUID, procStatus;
            var responseDetails = JObject.Parse(input);
            token = responseDetails["Token"].ToString();
            trantype = responseDetails["Type"].ToString();                                                
            amount = responseDetails["Amount"].ToString();
            ccpid = responseDetails["CPID"].ToString();
            docType = responseDetails["DocType"].ToString();
            docRefNbr = responseDetails["DocRefNbr"].ToString();
            tranUID = responseDetails["TranUID"].ToString();
            procStatus = responseDetails["procRes"] != null ? responseDetails["procRes"].ToString() : "0";
            return new HostedFormResponse()
            {
                TranID = String.Format("{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}", token, trantype, amount, ccpid, docType, docRefNbr, tranUID, procStatus)   
            };
        }
    }
}
