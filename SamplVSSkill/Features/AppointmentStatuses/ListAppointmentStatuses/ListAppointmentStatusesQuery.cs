using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SamplVSSkill.Features.AppointmentStatuses.ListAppointmentStatuses;

public record AppointmentStatusResponse(string Id, string Label);

public class ListAppointmentStatusesQueryHandler
{
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ListAppointmentStatusesQueryHandler(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IEnumerable<AppointmentStatusResponse>> HandleAsync(CancellationToken ct)
    {
        var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Domain", "Raw", "estadoscita.json");

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The appointment statuses JSON file could not be found.", filePath);
        }

        using var stream = File.OpenRead(filePath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var statuses = await JsonSerializer.DeserializeAsync<List<AppointmentStatusResponse>>(stream, options, ct);

        return statuses ?? [];
    }
}
