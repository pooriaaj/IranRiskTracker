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
