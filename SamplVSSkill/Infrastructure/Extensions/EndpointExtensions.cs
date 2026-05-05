using SamplVSSkill.Features.Auth.Register;
using SamplVSSkill.Features.Auth.Login;
using SamplVSSkill.Features.Auth.CreateRole;
using SamplVSSkill.Features.Auth.ListRoles;
using SamplVSSkill.Features.Auth.DeleteRole;
using SamplVSSkill.Features.Auth.AssignRole;
using SamplVSSkill.Features.Auth.RemoveRole;
using SamplVSSkill.Features.Auth.GetUserRoles;
using SamplVSSkill.Features.Auth.ListUsers;
using SamplVSSkill.Features.Auth.GetUser;
using SamplVSSkill.Features.Auth.ToggleUserStatus;
using SamplVSSkill.Features.Auth.AssignClaim;
using SamplVSSkill.Features.Auth.RemoveClaim;
using SamplVSSkill.Features.Auth.GetUserClaims;
using SamplVSSkill.Features.MedicalCenters.PagedMedicalCenters;
using SamplVSSkill.Features.Doctors.CreateDoctor;
using SamplVSSkill.Features.Doctors.GetDoctor;
using SamplVSSkill.Features.Doctors.ListDoctors;
using SamplVSSkill.Features.Doctors.UpdateDoctor;
using SamplVSSkill.Features.Doctors.DeleteDoctor;
using SamplVSSkill.Features.MedicalCenters.CreateMedicalCenter;
using SamplVSSkill.Features.MedicalCenters.GetMedicalCenter;
using SamplVSSkill.Features.MedicalCenters.ListMedicalCenters;
using SamplVSSkill.Features.MedicalCenters.UpdateMedicalCenter;
using SamplVSSkill.Features.MedicalCenters.DeleteMedicalCenter;

namespace SamplVSSkill.Infrastructure.Extensions;

public static class EndpointExtensions
{
    /// <summary>
    /// Registers all feature Minimal API endpoints.
    /// Each feature exposes a static Map(IEndpointRouteBuilder) method — the VSA single entry point.
    /// </summary>
    public static IEndpointRouteBuilder MapFeatureEndpoints(this IEndpointRouteBuilder app)
    {
        // ── Auth (public) ──
        RegisterEndpoint.Map(app);
        LoginEndpoint.Map(app);

        // ── Roles ──
        CreateRoleEndpoint.Map(app);
        ListRolesEndpoint.Map(app);
        DeleteRoleEndpoint.Map(app);

        // ── User-Role ──
        AssignRoleEndpoint.Map(app);
        RemoveRoleEndpoint.Map(app);
        GetUserRolesEndpoint.Map(app);

        // ── Users ──
        ListUsersEndpoint.Map(app);
        GetUserEndpoint.Map(app);
        ToggleUserStatusEndpoint.Map(app);

        // ── Claims ──
        AssignClaimEndpoint.Map(app);
        RemoveClaimEndpoint.Map(app);
        GetUserClaimsEndpoint.Map(app);

        // ── Doctors ──
        CreateDoctorEndpoint.Map(app);
        GetDoctorEndpoint.Map(app);
        ListDoctorsEndpoint.Map(app);
        UpdateDoctorEndpoint.Map(app);
        DeleteDoctorEndpoint.Map(app);

        // ── Medical Centers ──
        CreateMedicalCenterEndpoint.Map(app);
        GetMedicalCenterEndpoint.Map(app);
        ListMedicalCentersEndpoint.Map(app);
        PagedMedicalCentersEndpoint.Map(app);
        UpdateMedicalCenterEndpoint.Map(app);
        DeleteMedicalCenterEndpoint.Map(app);

        return app;
    }

    /// <summary>
    /// Registers all feature Command/Query handlers in the DI container.
    /// </summary>
    public static IServiceCollection AddFeatureHandlers(this IServiceCollection services)
    {
        // ── Auth ──
        services.AddScoped<RegisterCommandHandler>();
        services.AddScoped<LoginCommandHandler>();

        // ── Roles ──
        services.AddScoped<CreateRoleCommandHandler>();
        services.AddScoped<ListRolesQueryHandler>();
        services.AddScoped<DeleteRoleCommandHandler>();

        // ── User-Role ──
        services.AddScoped<AssignRoleCommandHandler>();
        services.AddScoped<RemoveRoleCommandHandler>();
        services.AddScoped<GetUserRolesQueryHandler>();

        // ── Users ──
        services.AddScoped<ListUsersQueryHandler>();
        services.AddScoped<GetUserQueryHandler>();
        services.AddScoped<ToggleUserStatusCommandHandler>();

        // ── Claims ──
        services.AddScoped<AssignClaimCommandHandler>();
        services.AddScoped<RemoveClaimCommandHandler>();
        services.AddScoped<GetUserClaimsQueryHandler>();

        // ── Doctors ──
        services.AddScoped<CreateDoctorCommandHandler>();
        services.AddScoped<GetDoctorQueryHandler>();
        services.AddScoped<ListDoctorsQueryHandler>();
        services.AddScoped<UpdateDoctorCommandHandler>();
        services.AddScoped<DeleteDoctorCommandHandler>();

        // ── Medical Centers ──
        services.AddScoped<CreateMedicalCenterCommandHandler>();
        services.AddScoped<GetMedicalCenterQueryHandler>();
        services.AddScoped<ListMedicalCentersQueryHandler>();
        services.AddScoped<PagedMedicalCentersQueryHandler>();
        services.AddScoped<UpdateMedicalCenterCommandHandler>();
        services.AddScoped<DeleteMedicalCenterCommandHandler>();

        return services;
    }
}
