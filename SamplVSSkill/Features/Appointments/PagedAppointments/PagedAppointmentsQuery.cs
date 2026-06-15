using Dapper;
using SamplVSSkill.Domain.Common;
using SamplVSSkill.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SamplVSSkill.Features.Appointments.PagedAppointments;

public record PagedAppointmentsParams(
    int Page = 1,
    int PageSize = 10,
    string? StatusId = null,
    DateTime? Date = null);

public record PagedAppointmentItem(
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

public class PagedAppointmentsQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;

    public PagedAppointmentsQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<PaginatedResult<PagedAppointmentItem>> HandleAsync(
        PagedAppointmentsParams queryParams, string userId, CancellationToken ct)
    {
        var page     = Math.Max(1, queryParams.Page);
        var pageSize = Math.Clamp(queryParams.PageSize, 1, 100);
        var offset   = (page - 1) * pageSize;

        var conditions = new List<string> { "au.user_id = @UserId" };
        var parameters = new DynamicParameters();
        parameters.Add("UserId", userId);
        parameters.Add("PageSize", pageSize);
        parameters.Add("Offset", offset);

        if (!string.IsNullOrEmpty(queryParams.StatusId))
        {
            conditions.Add("au.status_id = @StatusId");
            parameters.Add("StatusId", queryParams.StatusId);
        }

        if (queryParams.Date.HasValue)
        {
            conditions.Add("CAST(au.appointment_date AS date) = CAST(@Date AS date)");
            parameters.Add("Date", queryParams.Date.Value.Date);
        }

        var whereClause = "WHERE " + string.Join(" AND ", conditions);

        var countSql = $"""
            SELECT COUNT(*)
            FROM appointment_users au
            {whereClause}
            """;

        var dataSql = $"""
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
            LIMIT @PageSize OFFSET @Offset
            """;

        using var connection = _connectionFactory.CreateConnection();

        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, parameters, cancellationToken: ct));

        var items = (await connection.QueryAsync<PagedAppointmentItem>(
            new CommandDefinition(dataSql, parameters, cancellationToken: ct))).ToList();

        return new PaginatedResult<PagedAppointmentItem>(items, page, pageSize, totalCount);
    }
}
