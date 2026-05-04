using SamplVSSkill.Features.Auth.Register;
using SamplVSSkill.Features.Auth.Login;
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
        // ── Auth ──
        RegisterEndpoint.Map(app);
        LoginEndpoint.Map(app);

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

