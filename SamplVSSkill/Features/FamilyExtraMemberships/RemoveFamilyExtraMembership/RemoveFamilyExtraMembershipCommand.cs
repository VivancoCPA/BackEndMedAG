using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SamplVSSkill.Infrastructure.Persistence;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SamplVSSkill.Features.FamilyExtraMemberships.RemoveFamilyExtraMembership;

// ── Response ────────────────────────────────────────────────────
public record RemoveFamilyExtraMembershipResponse(int Id, Guid FamilyGroupId, string Message);

// ── Command Handler (EF Core) ───────────────────────────────────
public class RemoveFamilyExtraMembershipCommandHandler
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public RemoveFamilyExtraMembershipCommandHandler(AppDbContext db, IWebHostEnvironment webHostEnvironment)
    {
        _db = db;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IResult> HandleAsync(Guid familyGroupId, int id, CancellationToken ct)
    {
        // 1. Verificar si el grupo familiar existe
        var groupExists = await _db.FamilyGroups.AnyAsync(g => g.Id == familyGroupId, ct);
        if (!groupExists)
            return Results.NotFound($"Grupo familiar '{familyGroupId}' no encontrado.");

        // 2. Buscar el miembro extra
        var extraMember = await _db.FamilyExtraMemberships
            .FirstOrDefaultAsync(m => m.Id == id && m.FamilyGroupId == familyGroupId, ct);

        if (extraMember is null)
            return Results.NotFound($"Miembro extra con ID '{id}' no encontrado en el grupo familiar '{familyGroupId}'.");

        var photoUrl = extraMember.PhotoUrl;

        // 3. Eliminar de la base de datos
        _db.FamilyExtraMemberships.Remove(extraMember);
        await _db.SaveChangesAsync(ct);

        // 4. Eliminar el archivo físico de foto si existía
        if (!string.IsNullOrWhiteSpace(photoUrl))
        {
            try
            {
                var physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, photoUrl.TrimStart('/'));
                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                }
            }
            catch
            {
                // No bloquear la respuesta de éxito si falla el borrado físico del archivo
            }
        }

        return Results.Ok(new RemoveFamilyExtraMembershipResponse(
            id, familyGroupId, "Miembro extra desvinculado y eliminado del grupo familiar exitosamente."));
    }
}
