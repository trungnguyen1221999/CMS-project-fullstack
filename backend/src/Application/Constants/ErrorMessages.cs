namespace Application.Constants
{
    public static class ErrorMessages
    {
        public static class Common
        {
            public const string InvalidRequest = "INVALID_REQUEST";
            public const string InternalServerError = "INTERNAL_SERVER_ERROR";
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
            public const string PostNotFound = "POST_NOT_FOUND";
            public const string InsufficientPostPermission = "INSUFFICIENT_POST_PERMISSION";
            public const string CreatePostFailed = "CREATE_POST_FAILED";
            public const string UpdatePostFailed = "UPDATE_POST_FAILED";
            public const string ClearPostTagsFailed = "CLEAR_POST_TAGS_FAILED";
            public const string SlugAlreadyExists = "SLUG_ALREADY_EXISTS";
            public const string DeleteFailed = "DELETE_POST_FAILED";
            public const string ApproveFailed = "APPROVE_POST_FAILED";
            public const string RejectFailed = "REJECT_POST_FAILED";
            public const string SubmitForApprovalFailed = "SUBMIT_FOR_APPROVAL_FAILED";
            public const string PostNotRejected = "POST_NOT_REJECTED";
            public const string FailToGetRejectReason = "FAIL_TO_GET_REJECT_REASON";
        }

        public static class Category
        {
            public const string CategoryNotFound = "CATEGORY_NOT_FOUND";
            public const string InsufficientPermissions = "INSUFFICIENT_CATEGORY_PERMISSIONS";
            public const string SlugAlreadyExists = "CATEGORY_SLUG_ALREADY_EXISTS";
            public const string CreateFailed = "CREATE_CATEGORY_FAILED";
            public const string UpdateFailed = "UPDATE_CATEGORY_FAILED";
            public const string DeleteFailed = "DELETE_CATEGORY_FAILED";
        }

        public static class Royalty
        {
            public const string InsufficientPermissions = "INSUFFICIENT_ROYALTY_PERMISSIONS";
            public const string InvalidDateRange = "INVALID_DATE_RANGE";
            public const string NoUnpaidPosts = "NO_UNPAID_POSTS";
        }

        public static class Series
        {
            public const string SeriesNotFound = "SERIES_NOT_FOUND";
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
