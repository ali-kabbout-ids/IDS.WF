using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Rwd.WF.Application.Common.Interfaces;

namespace Rwd.WF.Application.Features.Workflow.Queries;

public record CheckMane3KanoneQuery(Guid ApplicationId) : IRequest<CheckMane3KanoneResult>;

public sealed record CheckMane3KanoneResult(bool HasMane3, string Details);

public sealed class CheckMane3KanoneQueryHandler(IAppDbContext db) : IRequestHandler<CheckMane3KanoneQuery, CheckMane3KanoneResult>
{
    public async Task<CheckMane3KanoneResult> Handle(CheckMane3KanoneQuery request, CancellationToken cancellationToken)
    {
        var app = await db.Applications.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);

        if (app is null)
            return new CheckMane3KanoneResult(false, "Application not found.");

        var haystack = app.FormData ?? string.Empty;
        var has = haystack.Contains("mane3", StringComparison.OrdinalIgnoreCase)
                  || haystack.Contains("منع", StringComparison.Ordinal);

        if (!has && !string.IsNullOrWhiteSpace(haystack))
        {
            try
            {
                using var doc = JsonDocument.Parse(haystack);
                has = ContainsRecursive(doc.RootElement, "mane3");
            }
            catch
            {
                // keep has=false
            }
        }

        return has
            ? new CheckMane3KanoneResult(true, "Indicator matched in application form data.")
            : new CheckMane3KanoneResult(false, "No mane3 indicator found in application form data.");
    }

    private static bool ContainsRecursive(JsonElement el, string token)
    {
        switch (el.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var p in el.EnumerateObject())
                {
                    if (p.Name.Contains(token, StringComparison.OrdinalIgnoreCase))
                        return true;
                    if (ContainsRecursive(p.Value, token))
                        return true;
                }

                return false;
            case JsonValueKind.Array:
                return el.EnumerateArray().Any(x => ContainsRecursive(x, token));
            case JsonValueKind.String:
                return el.GetString()?.Contains(token, StringComparison.OrdinalIgnoreCase) == true;
            default:
                return false;
        }
    }
}
