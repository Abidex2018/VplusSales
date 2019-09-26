namespace NewVPlusSale.APIObjects.Common
{
    public class APIResponseStatus
    {
        public bool IsSuccessful;
        public APIResponseMessage Message;
    }

    public class APIResponseMessage
    {
        public string FriendlyMessage;
        public string TechnicalMessage;
        public string MessageId;
        public string ShortErrorMessage;
    }
}
