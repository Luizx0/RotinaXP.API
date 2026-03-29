namespace RotinaXP.API.Common;

public static class AppConstants
{
    public static class Points
    {
        public const int TaskCompletionReward = 10;
    }

    public static class Auth
    {
        public const int MinPasswordLength = 8;
        public const int MaxPasswordLength = 128;
        public const int MinNameLength = 2;
        public const int MaxNameLength = 120;
        public const int MaxEmailLength = 254;
    }

    public static class Pagination
    {
        public const int DefaultPage = 1;
        public const int DefaultPageSize = 20;
        public const int MaxPageSize = 100;
    }

    public static class Policies
    {
        public const string ResourceOwner = "ResourceOwner";
    }

    public static class CorrelationId
    {
        public const string HeaderName = "X-Correlation-Id";
    }

    public static class Cors
    {
        public const string PolicyName = "AllowFrontend";
    }
}
