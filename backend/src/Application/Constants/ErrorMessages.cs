namespace Application.Constants
{
    public static class ErrorMessages
    {
        public static class Common
        {
            public const string InvalidRequest = "INVALID_REQUEST";
            public const string NotFoundSuffix = "NOT_FOUND";
        }

        public static class User
        {
            public const string UserNotFound = "USER_NOT_FOUND";
            public const string UserAlreadyExists = "USER_ALREADY_EXISTS";
            public const string InvalidIds = "INVALID_IDS";
            public const string CreateFailed = "CREATE_FAILED";
            public const string UpdateFailed = "UPDATE_FAILED";
            public const string SetPasswordFailed = "SET_PASSWORD_FAILED";
            public const string ChangeEmailFailed = "CHANGE_EMAIL_FAILED";
            public const string AssignRolesFailed = "ASSIGN_ROLES_FAILED";

            public static class ChangePassword
            {
                public const string NewPasswordSameAsCurrent =
                    "NEW_PASSWORD_SAME_AS_CURRENT_PASSWORD";
            }
        }

        public static class Post
        {
            public const string InsufficientPostPermission = "INSUFFICIENT_POST_PERMISSION";
            public const string CreatePostFailed = "CREATE_POST_FAILED";
            public const string SlugAlreadyExists = "SLUG_ALREADY_EXISTS";
        }

        public static class Category
        {
            public const string CategoryNotFound = "CATEGORY_NOT_FOUND";
        }

        public static class Auth
        {
            public const string InvalidPassword = "INVALID_PASSWORD";
            public const string FailedToAssignRole = "FAILED_TO_ASSIGN_ROLE";
            public const string ChangePasswordFailed = "CHANGE_PASSWORD_FAILED";
            public const string CurrentPasswordIncorrect = "CURRENT_PASSWORD_INCORRECT";
            public const string ResetPasswordFailed = "RESET_PASSWORD_FAILED";
            public const string CodeInvalid = "CODE_INVALID";
        }
    }
}
