namespace Application.Services.Permission
{
    public interface IPermissionService
    {
        List<string> GetPermissionsByUserId(Guid userId);
        bool HasApprovedPostPermission(Guid userId);
        bool HasDeletePostPermission(Guid userId);

        bool HasViewAllCategoryPermission(Guid userId);

        bool HasViewActiveCategoryPermission(Guid userId, bool isPostActive);

        bool HasViewAllCategoriesPermission(Guid userId);
        bool HasCreateCategoryPermission(Guid userId);
        bool HasEditCategoryPermission(Guid userId);
        bool HasDeleteCategoryPermission(Guid userId);
        bool HasRoyaltyReportViewPermission(Guid userId);
        bool HasRoyaltyPayPermission(Guid userId);
    }
}
