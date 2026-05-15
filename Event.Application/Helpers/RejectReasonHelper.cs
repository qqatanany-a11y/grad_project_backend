namespace Event.Application.Helpers
{
    public static class RejectReasonHelper
    {
        public const string DefaultReason = "Rejected by admin";

        public static string Normalize(string? reason)
        {
            return string.IsNullOrWhiteSpace(reason)
                ? DefaultReason
                : reason.Trim();
        }
    }
}
