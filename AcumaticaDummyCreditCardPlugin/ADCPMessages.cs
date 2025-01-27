using PX.Common;

namespace AcumaticaDummyCreditCardPlugin
{
    [PXLocalizable()]
    public static class ADCPMessages
    {
        public const string ADPCPluginName = "Acumatica Dummy Credit Card Plug-in";

        #region Keys for the setting descriptions
        public const string ADPCURLDesc      = "URL for Acumatica with ADPC deployed";
        public const string ADPCUserNameDesc = "Username for Acumatica with ADPC deployed";
        public const string ADPCPasswordDesc = "Password for Acumatica with ADPC deployed";
        public const string ADPCTenantDesc   = "Tenant for Acumatica with ADPC deployed";
        public const string ADPCUseCustomHostedForm   = "Use custom hosted form";
        #endregion

        #region Validation Messages
        public const string NoSetting   = "Setting is missing";
        public const string NoValue     = "No value set for {0} setting";
        #endregion
    }
}
