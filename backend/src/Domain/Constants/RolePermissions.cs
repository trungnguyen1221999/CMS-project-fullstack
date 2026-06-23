namespace Domain.Constants
{
    public static class RolePermissions
    {
        public static readonly HashSet<string> PublicReader = new()
        {
            Permissions.PostCategories.View,
            Permissions.Posts.View,
            Permissions.Series.View,
        };

        public static readonly HashSet<string> Writing = new()
        {
            Permissions.Posts.Create,
            Permissions.Posts.Edit,
            Permissions.Royalty.View,
        };

        public static readonly HashSet<string> Editing = new()
        {
            Permissions.Dashboard.View,
            Permissions.PostCategories.Create,
            Permissions.PostCategories.Edit,
            Permissions.Series.Create,
            Permissions.Series.Edit,
            Permissions.Series.Delete,
            Permissions.Posts.Delete,
            Permissions.Posts.Approve,
        };
    }
}
