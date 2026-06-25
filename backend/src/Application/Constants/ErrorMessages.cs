namespace Application.Constants
{
    public static class ErrorMessages
    {
        public static class Common
        {
            public const string InvalidRequest = "INVALID_REQUEST";
        }

        public static class User
        {
            public const string UserNotFound = "USER_NOT_FOUND";
            public const string UsersNotFound = "USERS_NOT_FOUND";
            public const string UserAlreadyExists = "USER_ALREADY_EXISTS";
            public const string InvalidIds = "INVALID_IDS";
            public const string CreateFailed = "CREATE_FAILED";
            public const string UpdateFailed = "UPDATE_FAILED";
        }

        public static class Auth
        {
            public const string InvalidPassword = "INVALID_PASSWORD";
            public const string FailedToAssignRole = "FAILED_TO_ASSIGN_ROLE";
        }
    }
}
