using Dapper;
using SamplVSSkill.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SamplVSSkill.Features.Appointments.ListAppointments;

public record ListAppointmentsResponse(
    Guid Id,
    string UserId,
    DateTime? AppointmentDate,
    Guid? CenterId,
    Guid? DoctorId,
    int? SpecialtieId,
    Guid? InsurerId,
    string Description,
    string StatusId,
    DateTime CreatedAt,
    string? SpecialtyName,
    string? DoctorName,
    string? DoctorPhotoUrl,
    string? CenterName,
    string? CenterAddress,
    double? CenterLatitude,
    double? CenterLongitude);

public class ListAppointmentsQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;

    public ListAppointmentsQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<IEnumerable<ListAppointmentsResponse>> HandleAsync(
        string userId, string? statusId, DateTime? date, CancellationToken ct)
    {
        using var connection = _connectionFactory.CreateConnection();

        var conditions = new List<string> { "au.user_id = @UserId" };
        var parameters = new DynamicParameters();
        parameters.Add("UserId", userId);

        if (!string.IsNullOrEmpty(statusId))
        {
            conditions.Add("au.status_id = @StatusId");
            parameters.Add("StatusId", statusId);
        }

        if (date.HasValue)
        {
            conditions.Add("CAST(au.appointment_date AS date) = CAST(@Date AS date)");
            parameters.Add("Date", date.Value.Date);
        }

        var whereClause = "WHERE " + string.Join(" AND ", conditions);

        var sql = $"""
            SELECT au.id AS Id,
                   au.user_id AS UserId,
                   au.appointment_date AS AppointmentDate,
                   au.center_id AS CenterId,
                   au.doctor_id AS DoctorId,
                   au.specialtie_id AS SpecialtieId,
                   au.insurer_id AS InsurerId,
                   au.description AS Description,
                   au.status_id AS StatusId,
                   au.created_at AS CreatedAt,
                   s.name AS SpecialtyName,
                   CONCAT(d.name, ' ', d.last_name) AS DoctorName,
                   d.photo_url AS DoctorPhotoUrl,
                   mc.name AS CenterName,
                   mc.address AS CenterAddress,
                   mc.latitude AS CenterLatitude,
                   mc.longitude AS CenterLongitude
            FROM appointment_users au
            LEFT JOIN doctors d ON au.doctor_id = d.id
            LEFT JOIN specialties s ON au.specialtie_id = s.id
            LEFT JOIN medical_centers mc ON au.center_id = mc.id
            {whereClause}
            ORDER BY au.appointment_date DESC
            """;

        return await connection.QueryAsync<ListAppointmentsResponse>(
            new CommandDefinition(sql, parameters, cancellationToken: ct));
    }
}
