using Dapper;
using SamplVSSkill.Domain.Common;
using SamplVSSkill.Infrastructure.Persistence;

namespace SamplVSSkill.Features.Doctors.PagedDoctors;

public record PagedDoctorsParams(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string? SortBy = "created_at",
    bool SortDesc = false);

public class AffiliatedCenterItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? OfficeNumber { get; set; }
    public string? WorkSchedule { get; set; }
}

public class PagedDoctorItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int? SpecialtyId { get; set; }
    public string? SpecialtyName { get; set; }
    public string? Register { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? PhotoUrl { get; set; }
    public bool IsVet { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<AffiliatedCenterItem> Centers { get; set; } = new();
}

public class PagedDoctorsQueryHandler
{
    private readonly DapperConnectionFactory _connectionFactory;

    private static readonly Dictionary<string, string> AllowedSortColumns =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["name"]       = "d.name",
            ["lastname"]   = "d.last_name",
            ["email"]      = "d.email",
            ["isactive"]   = "d.is_active",
            ["created_at"] = "d.created_at",
            ["updated_at"] = "d.updated_at"
        };

    public PagedDoctorsQueryHandler(DapperConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<PaginatedResult<PagedDoctorItem>> HandleAsync(
        PagedDoctorsParams queryParams, CancellationToken ct)
    {
        var page     = Math.Max(1, queryParams.Page);
        var pageSize = Math.Clamp(queryParams.PageSize, 1, 100);
        var offset   = (page - 1) * pageSize;

        var parameters = BuildParameters(queryParams, pageSize, offset);
        var where      = BuildWhereClause(queryParams);
        var orderBy    = BuildOrderByClause(queryParams);

        var countSql = $"SELECT COUNT(*) FROM doctors d {where}";
        var dataSql  = BuildDataSql(where, orderBy, pageSize, offset);

        using var connection = _connectionFactory.CreateConnection();

        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, parameters, cancellationToken: ct));

        var doctorDict = new Dictionary<Guid, PagedDoctorItem>();

        var rawItems = await connection.QueryAsync<PagedDoctorItem, AffiliatedCenterItem, PagedDoctorItem>(
            new CommandDefinition(dataSql, parameters, cancellationToken: ct),
            (doctor, center) =>
            {
                if (!doctorDict.TryGetValue(doctor.Id, out var current))
                {
                    current = doctor;
                    doctorDict.Add(current.Id, current);
                }

                if (center != null && center.Id != Guid.Empty)
                {
                    current.Centers.Add(center);
                }
                return current;
            },
            splitOn: "Id");

        var items = doctorDict.Values.ToList();

        return new PaginatedResult<PagedDoctorItem>(items, page, pageSize, totalCount);
    }

    private static DynamicParameters BuildParameters(
        PagedDoctorsParams p, int pageSize, int offset)
    {
        var dp = new DynamicParameters();
        dp.Add("PageSize", pageSize);
        dp.Add("Offset", offset);
        if (!string.IsNullOrWhiteSpace(p.Search))
            dp.Add("Search", $"%{p.Search.Trim()}%");
        return dp;
    }

    private static string BuildWhereClause(PagedDoctorsParams p) =>
        string.IsNullOrWhiteSpace(p.Search) ? string.Empty
            : """
              WHERE d.name ILIKE @Search
                 OR d.last_name ILIKE @Search
                 OR d.email ILIKE @Search
                 OR d.register ILIKE @Search
              """;

    private static string BuildOrderByClause(PagedDoctorsParams p)
    {
        var column    = AllowedSortColumns.GetValueOrDefault(p.SortBy ?? "created_at", "created_at");
        var direction = p.SortDesc ? "DESC" : "ASC";
        return $"ORDER BY {column} {direction}";
    }

    private static string BuildDataSql(string where, string orderBy, int pageSize, int offset) => $"""
        WITH paged AS (
            SELECT d.id
            FROM doctors d
            {where}
            {orderBy}
            LIMIT {pageSize} OFFSET {offset}
        )
        SELECT d.id               AS Id,
               d.name             AS Name,
               d.last_name        AS LastName,
               d.specialty_id     AS SpecialtyId,
               s.name             AS SpecialtyName,
               d.register         AS Register,
               d.phone            AS Phone,
               d.email            AS Email,
               d.photo_url        AS PhotoUrl,
               d.is_vet           AS IsVet,
               d.is_active        AS IsActive,
               d.created_at       AS CreatedAt,
               d.updated_at       AS UpdatedAt,
               mc.id              AS Id,
               mc.name            AS Name,
               da.office_number   AS OfficeNumber,
               da.work_schedule   AS WorkSchedule
        FROM paged p
        JOIN doctors d ON p.id = d.id
        LEFT JOIN specialties s ON d.specialty_id = s.id
        LEFT JOIN doctor_affiliations da ON d.id = da.doctor_id
        LEFT JOIN medical_centers mc ON da.center_id = mc.id
        {orderBy}
        """;
}
