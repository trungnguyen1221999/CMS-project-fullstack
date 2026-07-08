namespace Application.Services.Permission
{
    public interface IPermissionService
    {
        List<string> GetPermissionsByUserId(Guid userId);
        bool HasApprovedPostPermission(Guid userId);
        bool HasDeletePostPermission(Guid userId);

        bool HasViewAllCategoryPermission(Guid userId);

        bool HasViewActiveCategoryPermission(Guid userId, bool isPostActive);
    }
}
