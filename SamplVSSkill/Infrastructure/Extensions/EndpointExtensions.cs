using SamplVSSkill.Features.Auth.Register;
using SamplVSSkill.Features.Auth.Login;
using SamplVSSkill.Features.Auth.RefreshToken;
using SamplVSSkill.Features.Auth.ChangePassword;
using SamplVSSkill.Features.Auth.ForgotPassword;
using SamplVSSkill.Features.Auth.ResetPassword;
using SamplVSSkill.Features.Auth.CreateRole;
using SamplVSSkill.Features.Auth.ListRoles;
using SamplVSSkill.Features.Auth.DeleteRole;
using SamplVSSkill.Features.Auth.UpdateRole;
using SamplVSSkill.Features.Auth.ToggleRoleStatus;
using SamplVSSkill.Features.Auth.AssignRole;
using SamplVSSkill.Features.Auth.RemoveRole;
using SamplVSSkill.Features.Auth.GetUserRoles;
using SamplVSSkill.Features.Auth.ListUsers;
using SamplVSSkill.Features.Auth.PagedUsers;
using SamplVSSkill.Features.Auth.GetUser;
using SamplVSSkill.Features.Auth.CreateUser;
using SamplVSSkill.Features.Auth.UpdateUser;
using SamplVSSkill.Features.Auth.ToggleUserStatus;
using SamplVSSkill.Features.Auth.AssignClaim;
using SamplVSSkill.Features.Auth.RemoveClaim;
using SamplVSSkill.Features.Auth.GetUserClaims;
using SamplVSSkill.Features.Specialties.CreateSpecialty;
using SamplVSSkill.Features.CenterTypes.CreateCenterType;
using SamplVSSkill.Features.CenterTypes.GetCenterType;
using SamplVSSkill.Features.CenterTypes.ListCenterTypes;
using SamplVSSkill.Features.CenterTypes.PagedCenterTypes;
using SamplVSSkill.Features.CenterTypes.UpdateCenterType;
using SamplVSSkill.Features.CenterTypes.LookupCenterTypes;
using SamplVSSkill.Features.CenterTypes.ToggleCenterTypeStatus;
using SamplVSSkill.Features.Specialties.GetSpecialty;
using SamplVSSkill.Features.Specialties.ListSpecialties;
using SamplVSSkill.Features.Specialties.LookupSpecialties;
using SamplVSSkill.Features.Specialties.PagedSpecialties;
using SamplVSSkill.Features.Specialties.UpdateSpecialty;
using SamplVSSkill.Features.Specialties.ToggleSpecialtyStatus;
using SamplVSSkill.Features.Insurers.CreateInsurer;
using SamplVSSkill.Features.Insurers.GetInsurer;
using SamplVSSkill.Features.Insurers.ListInsurers;
using SamplVSSkill.Features.Insurers.PagedInsurers;
using SamplVSSkill.Features.Insurers.UpdateInsurer;
using SamplVSSkill.Features.Insurers.LookupInsurers;
using SamplVSSkill.Features.Insurers.ToggleInsurerStatus;
using SamplVSSkill.Features.Doctors.CreateDoctor;
using SamplVSSkill.Features.Doctors.GetDoctor;
using SamplVSSkill.Features.Doctors.ListDoctors;
using SamplVSSkill.Features.Doctors.UpdateDoctor;
using SamplVSSkill.Features.Doctors.DeleteDoctor;
using SamplVSSkill.Features.Doctors.LookupDoctors;
using SamplVSSkill.Features.Doctors.PagedDoctors;
using SamplVSSkill.Features.Doctors.SummaryDoctors;
using SamplVSSkill.Features.Doctors.ToggleDoctorStatus;
using SamplVSSkill.Features.DoctorAffiliations.CreateDoctorAffiliation;
using SamplVSSkill.Features.DoctorAffiliations.ListDoctorAffiliations;
using SamplVSSkill.Features.DoctorAffiliations.UpdateDoctorAffiliation;
using SamplVSSkill.Features.DoctorAffiliations.DeleteDoctorAffiliation;
using SamplVSSkill.Features.MedicalCenters.CreateMedicalCenter;
using SamplVSSkill.Features.MedicalCenters.GetMedicalCenter;
using SamplVSSkill.Features.MedicalCenters.ListMedicalCenters;
using SamplVSSkill.Features.MedicalCenters.LookupMedicalCenters;
using SamplVSSkill.Features.MedicalCenters.PagedMedicalCenters;
using SamplVSSkill.Features.MedicalCenters.SummaryMedicalCenters;
using SamplVSSkill.Features.MedicalCenters.UpdateMedicalCenter;
using SamplVSSkill.Features.MedicalCenters.DeleteMedicalCenter;
using SamplVSSkill.Features.FamilyGroups.CreateFamilyGroup;
using SamplVSSkill.Features.FamilyGroups.GetFamilyGroup;
using SamplVSSkill.Features.FamilyGroups.ListFamilyGroups;
using SamplVSSkill.Features.FamilyGroups.PagedFamilyGroups;
using SamplVSSkill.Features.FamilyGroups.UpdateFamilyGroup;
using SamplVSSkill.Features.FamilyGroups.ToggleFamilyGroupStatus;
using SamplVSSkill.Features.UserInsurances.ListUserInsurances;
using SamplVSSkill.Features.UserInsurances.AssignUserInsurance;
using SamplVSSkill.Features.UserInsurances.RemoveUserInsurance;
using SamplVSSkill.Features.FamilyMemberships.AssignFamilyMembership;
using SamplVSSkill.Features.FamilyMemberships.ListFamilyMemberships;
using SamplVSSkill.Features.FamilyMemberships.RemoveFamilyMembership;

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
        ForgotPasswordEndpoint.Map(app);
        ResetPasswordEndpoint.Map(app);
        ChangePasswordEndpoint.Map(app);
        RefreshTokenEndpoint.Map(app);

        // ── Roles ──
        CreateRoleEndpoint.Map(app);
        ListRolesEndpoint.Map(app);
        DeleteRoleEndpoint.Map(app);
        UpdateRoleEndpoint.Map(app);
        ToggleRoleStatusEndpoint.Map(app);

        // ── User-Role ──
        AssignRoleEndpoint.Map(app);
        RemoveRoleEndpoint.Map(app);
        GetUserRolesEndpoint.Map(app);

        // ── Users ──
        ListUsersEndpoint.Map(app);
        PagedUsersEndpoint.Map(app);
        GetUserEndpoint.Map(app);
        CreateUserEndpoint.Map(app);
        UpdateUserEndpoint.Map(app);
        ToggleUserStatusEndpoint.Map(app);

        // ── Claims ──
        AssignClaimEndpoint.Map(app);
        RemoveClaimEndpoint.Map(app);
        GetUserClaimsEndpoint.Map(app);

        // ── Center Types ──
        CreateCenterTypeEndpoint.Map(app);
        GetCenterTypeEndpoint.Map(app);
        ListCenterTypesEndpoint.Map(app);
        LookupCenterTypesEndpoint.Map(app);
        PagedCenterTypesEndpoint.Map(app);
        UpdateCenterTypeEndpoint.Map(app);
        ToggleCenterTypeStatusEndpoint.Map(app);

        // ── Specialties ──
        CreateSpecialtyEndpoint.Map(app);
        GetSpecialtyEndpoint.Map(app);
        ListSpecialtiesEndpoint.Map(app);
        LookupSpecialtiesEndpoint.Map(app);
        PagedSpecialtiesEndpoint.Map(app);
        UpdateSpecialtyEndpoint.Map(app);
        ToggleSpecialtyStatusEndpoint.Map(app);

        // ── Insurers ──
        CreateInsurerEndpoint.Map(app);
        GetInsurerEndpoint.Map(app);
        ListInsurersEndpoint.Map(app);
        LookupInsurersEndpoint.Map(app);
        PagedInsurersEndpoint.Map(app);
        UpdateInsurerEndpoint.Map(app);
        ToggleInsurerStatusEndpoint.Map(app);

        // ── Doctors ──
        CreateDoctorEndpoint.Map(app);
        GetDoctorEndpoint.Map(app);
        ListDoctorsEndpoint.Map(app);
        LookupDoctorsEndpoint.Map(app);
        PagedDoctorsEndpoint.Map(app);
        SummaryDoctorsEndpoint.Map(app);
        UpdateDoctorEndpoint.Map(app);
        DeleteDoctorEndpoint.Map(app);
        ToggleDoctorStatusEndpoint.Map(app);

        // ── Medical Centers ──
        CreateMedicalCenterEndpoint.Map(app);
        GetMedicalCenterEndpoint.Map(app);
        ListMedicalCentersEndpoint.Map(app);
        LookupMedicalCentersEndpoint.Map(app);
        PagedMedicalCentersEndpoint.Map(app);
        SummaryMedicalCentersEndpoint.Map(app);
        UpdateMedicalCenterEndpoint.Map(app);
        DeleteMedicalCenterEndpoint.Map(app);

        // ── Doctor Affiliations ──
        CreateDoctorAffiliationEndpoint.Map(app);
        ListDoctorAffiliationsEndpoint.Map(app);
        UpdateDoctorAffiliationEndpoint.Map(app);
        DeleteDoctorAffiliationEndpoint.Map(app);

        // ── Family Groups ──
        CreateFamilyGroupEndpoint.Map(app);
        GetFamilyGroupEndpoint.Map(app);
        ListFamilyGroupsEndpoint.Map(app);
        PagedFamilyGroupsEndpoint.Map(app);
        UpdateFamilyGroupEndpoint.Map(app);
        ToggleFamilyGroupStatusEndpoint.Map(app);

        // ── User Insurances ──
        ListUserInsurancesEndpoint.Map(app);
        AssignUserInsuranceEndpoint.Map(app);
        RemoveUserInsuranceEndpoint.Map(app);

        // ── Family Memberships ──
        AssignFamilyMembershipEndpoint.Map(app);
        ListFamilyMembershipsEndpoint.Map(app);
        RemoveFamilyMembershipEndpoint.Map(app);

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
        services.AddScoped<ChangePasswordCommandHandler>();
        services.AddScoped<ForgotPasswordCommandHandler>();
        services.AddScoped<ResetPasswordCommandHandler>();
        services.AddScoped<RefreshTokenCommandHandler>();

        // ── Roles ──
        services.AddScoped<CreateRoleCommandHandler>();
        services.AddScoped<ListRolesQueryHandler>();
        services.AddScoped<DeleteRoleCommandHandler>();
        services.AddScoped<UpdateRoleCommandHandler>();
        services.AddScoped<ToggleRoleStatusCommandHandler>();

        // ── User-Role ──
        services.AddScoped<AssignRoleCommandHandler>();
        services.AddScoped<RemoveRoleCommandHandler>();
        services.AddScoped<GetUserRolesQueryHandler>();

        // ── Users ──
        services.AddScoped<ListUsersQueryHandler>();
        services.AddScoped<PagedUsersQueryHandler>();
        services.AddScoped<GetUserQueryHandler>();
        services.AddScoped<CreateUserCommandHandler>();
        services.AddScoped<UpdateUserCommandHandler>();
        services.AddScoped<ToggleUserStatusCommandHandler>();

        // ── Claims ──
        services.AddScoped<AssignClaimCommandHandler>();
        services.AddScoped<RemoveClaimCommandHandler>();
        services.AddScoped<GetUserClaimsQueryHandler>();

        // ── Specialties ──
        services.AddScoped<CreateSpecialtyCommandHandler>();
        services.AddScoped<GetSpecialtyQueryHandler>();
        services.AddScoped<ListSpecialtiesQueryHandler>();
        services.AddScoped<LookupSpecialtiesQueryHandler>();
        services.AddScoped<PagedSpecialtiesQueryHandler>();
        services.AddScoped<UpdateSpecialtyCommandHandler>();
        services.AddScoped<ToggleSpecialtyStatusCommandHandler>();

        // ── Insurers ──
        services.AddScoped<CreateInsurerCommandHandler>();
        services.AddScoped<GetInsurerQueryHandler>();
        services.AddScoped<ListInsurersQueryHandler>();
        services.AddScoped<LookupInsurersQueryHandler>();
        services.AddScoped<PagedInsurersQueryHandler>();
        services.AddScoped<UpdateInsurerCommandHandler>();
        services.AddScoped<ToggleInsurerStatusCommandHandler>();

        // ── Center Types ──
        services.AddScoped<CreateCenterTypeCommandHandler>();
        services.AddScoped<GetCenterTypeQueryHandler>();
        services.AddScoped<ListCenterTypesQueryHandler>();
        services.AddScoped<LookupCenterTypesQueryHandler>();
        services.AddScoped<PagedCenterTypesQueryHandler>();
        services.AddScoped<UpdateCenterTypeCommandHandler>();
        services.AddScoped<ToggleCenterTypeStatusCommandHandler>();

        // ── Doctors ──
        services.AddScoped<CreateDoctorCommandHandler>();
        services.AddScoped<GetDoctorQueryHandler>();
        services.AddScoped<ListDoctorsQueryHandler>();
        services.AddScoped<LookupDoctorsQueryHandler>();
        services.AddScoped<PagedDoctorsQueryHandler>();
        services.AddScoped<SummaryDoctorsQueryHandler>();
        services.AddScoped<UpdateDoctorCommandHandler>();
        services.AddScoped<DeleteDoctorCommandHandler>();
        services.AddScoped<ToggleDoctorStatusCommandHandler>();

        // ── Medical Centers ──
        services.AddScoped<CreateMedicalCenterCommandHandler>();
        services.AddScoped<GetMedicalCenterQueryHandler>();
        services.AddScoped<ListMedicalCentersQueryHandler>();
        services.AddScoped<LookupMedicalCentersQueryHandler>();
        services.AddScoped<PagedMedicalCentersQueryHandler>();
        services.AddScoped<SummaryMedicalCentersQueryHandler>();
        services.AddScoped<UpdateMedicalCenterCommandHandler>();
        services.AddScoped<DeleteMedicalCenterCommandHandler>();

        // ── Doctor Affiliations ──
        services.AddScoped<CreateDoctorAffiliationCommandHandler>();
        services.AddScoped<ListDoctorAffiliationsQueryHandler>();
        services.AddScoped<UpdateDoctorAffiliationCommandHandler>();
        services.AddScoped<DeleteDoctorAffiliationCommandHandler>();

        // ── Family Groups ──
        services.AddScoped<CreateFamilyGroupCommandHandler>();
        services.AddScoped<GetFamilyGroupQueryHandler>();
        services.AddScoped<ListFamilyGroupsQueryHandler>();
        services.AddScoped<PagedFamilyGroupsQueryHandler>();
        services.AddScoped<UpdateFamilyGroupCommandHandler>();
        services.AddScoped<ToggleFamilyGroupStatusCommandHandler>();

        // ── User Insurances ──
        services.AddScoped<ListUserInsurancesQueryHandler>();
        services.AddScoped<AssignUserInsuranceCommandHandler>();
        services.AddScoped<RemoveUserInsuranceCommandHandler>();

        // ── Family Memberships ──
        services.AddScoped<AssignFamilyMembershipCommandHandler>();
        services.AddScoped<ListFamilyMembershipsQueryHandler>();
        services.AddScoped<RemoveFamilyMembershipCommandHandler>();

        return services;
    }
}
