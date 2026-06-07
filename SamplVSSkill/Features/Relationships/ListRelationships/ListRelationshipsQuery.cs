using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace SamplVSSkill.Features.Relationships.ListRelationships;

public record RelationshipResponse(string Id, string Label);

public class ListRelationshipsQueryHandler
{
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ListRelationshipsQueryHandler(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IEnumerable<RelationshipResponse>> HandleAsync(CancellationToken ct)
    {
        var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Domain", "Raw", "parentescos.json");

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The relationships JSON file could not be found.", filePath);
        }

        using var stream = File.OpenRead(filePath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var relationships = await JsonSerializer.DeserializeAsync<List<RelationshipResponse>>(stream, options, ct);

        return relationships ?? [];
    }
}
