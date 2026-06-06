using System;
using System.Collections.Generic;
using System.Linq;
using IranRiskTracker.Application.DTOs;
using IranRiskTracker.Domain.Enums;

namespace IranRiskTracker.Application.Validation
{
    public static class LiveEventRequestValidator
    {
        public static IReadOnlyCollection<string> Validate(LiveEventCreateRequest? request)
        {
            var errors = new List<string>();

            if (request == null)
            {
                errors.Add("request must be provided");
                return errors;
            }

            var title = request.Title?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(title))
            {
                errors.Add("title is required");
            }
            else if (title.Length > 200)
            {
                errors.Add("title must be at most 200 characters");
            }

            var raw = request.RawContent?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(raw))
            {
                errors.Add("rawContent is required");
            }
            else if (raw.Length > 5000)
            {
                errors.Add("rawContent must be at most 5000 characters");
            }

            var sourceName = request.SourceName?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(sourceName))
            {
                errors.Add("sourceName is required");
            }
            else if (sourceName.Length > 150)
            {
                errors.Add("sourceName must be at most 150 characters");
            }

            if (!string.IsNullOrEmpty(request.SourceUrl) && request.SourceUrl.Length > 1000)
            {
                errors.Add("sourceUrl must be at most 1000 characters");
            }

            if (!string.IsNullOrEmpty(request.SourceHandle) && request.SourceHandle.Length > 100)
            {
                errors.Add("sourceHandle must be at most 100 characters");
            }

            if (!string.IsNullOrEmpty(request.OwnerNotes) && request.OwnerNotes.Length > 2000)
            {
                errors.Add("ownerNotes must be at most 2000 characters");
            }

            if (request.OccurredAt == default(DateTime))
            {
                errors.Add("occurredAt must be provided");
            }
            else
            {
                var now = DateTime.UtcNow;
                if (request.OccurredAt > now.AddDays(1))
                {
                    errors.Add("occurredAt cannot be more than 1 day in the future");
                }
            }

            if (!Enum.IsDefined(typeof(EventCategory), request.Category) || request.Category == EventCategory.Unknown)
            {
                errors.Add("category must be a defined non-Unknown EventCategory");
            }

            if (!Enum.IsDefined(typeof(UrgencyLevel), request.Urgency) /* no Unknown in UrgencyLevel */)
            {
                errors.Add("urgency must be a defined UrgencyLevel");
            }

            return errors.AsReadOnly();
        }
    }
}
