#nullable enable
using System.Security.Claims;
using AcxiomCRM.Models;
using Microsoft.AspNetCore.Identity;

namespace AcxiomCRM.Security;

/// <summary>
/// Provides record-level authorization checks.
/// Ensures users can only access records they own or are assigned to,
/// unless they have Admin or Manager role privileges.
/// </summary>
public class RecordAuthorizationService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RecordAuthorizationService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    /// <summary>
    /// Checks if the current user can access a record owned by the specified user.
    /// Admin can access all records.
    /// Manager can access all records (team scope).
    /// SalesExecutive can only access their own records.
    /// </summary>
    public async Task<bool> CanAccessRecordAsync(ClaimsPrincipal user, string? recordOwnerId)
    {
        if (user.IsInRole(AuthorizationPolicies.RoleAdmin))
            return true;

        if (user.IsInRole(AuthorizationPolicies.RoleManager))
            return true;

        var userId = _userManager.GetUserId(user);
        return userId == recordOwnerId;
    }

    /// <summary>
    /// Gets the owner filter for database queries based on user role.
    /// Returns null for Admin/Manager (no filter), or the user's ID for SalesExecutive.
    /// </summary>
    public string? GetOwnerFilter(ClaimsPrincipal user)
    {
        if (user.IsInRole(AuthorizationPolicies.RoleAdmin) ||
            user.IsInRole(AuthorizationPolicies.RoleManager))
        {
            return null; // No filter — can see all records
        }

        return _userManager.GetUserId(user);
    }

    /// <summary>
    /// Gets the current user's ID from claims.
    /// </summary>
    public string? GetUserId(ClaimsPrincipal user)
    {
        return _userManager.GetUserId(user);
    }

    /// <summary>
    /// Gets the current user's primary role name.
    /// </summary>
    public string GetUserRole(ClaimsPrincipal user)
    {
        if (user.IsInRole(AuthorizationPolicies.RoleAdmin))
            return AuthorizationPolicies.RoleAdmin;
        if (user.IsInRole(AuthorizationPolicies.RoleManager))
            return AuthorizationPolicies.RoleManager;
        return AuthorizationPolicies.RoleSalesExecutive;
    }
}
