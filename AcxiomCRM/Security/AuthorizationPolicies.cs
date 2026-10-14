#nullable enable
using Microsoft.AspNetCore.Authorization;

namespace AcxiomCRM.Security;

/// <summary>
/// Defines authorization policy names and configurations used across the application.
/// </summary>
public static class AuthorizationPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string AdminOrManager = "AdminOrManager";
    public const string AllRoles = "AllRoles";

    public const string RoleAdmin = "Admin";
    public const string RoleManager = "Manager";
    public const string RoleSalesExecutive = "SalesExecutive";

    public static readonly string[] AllRoleNames = { RoleAdmin, RoleManager, RoleSalesExecutive };
}
