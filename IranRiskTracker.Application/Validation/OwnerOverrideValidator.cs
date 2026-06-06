using System;
using System.Collections.Generic;
using IranRiskTracker.Application.DTOs;
using IranRiskTracker.Domain.Enums;

namespace IranRiskTracker.Application.Validation
{
    public static class OwnerOverrideValidator
    {
        public static IReadOnlyCollection<string> Validate(OwnerOverrideCreateRequest request)
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

            var reasoning = request.Reasoning?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(reasoning))
            {
                errors.Add("reasoning is required");
            }
            else if (reasoning.Length > 5000)
            {
                errors.Add("reasoning must be at most 5000 characters");
            }

            if (!Enum.IsDefined(typeof(EventCategory), request.Category) || request.Category == EventCategory.Unknown)
            {
                errors.Add("category must be a defined non-Unknown EventCategory");
            }

            if (request.ScoreAdjustment < -25.0 || request.ScoreAdjustment > 25.0)
            {
                errors.Add("scoreAdjustment must be between -25 and 25");
            }

            if (request.AppliedAt == default(DateTime))
            {
                errors.Add("appliedAt must be provided");
            }

            if (!string.IsNullOrEmpty(request.SourceReference) && request.SourceReference.Length > 1000)
            {
                errors.Add("sourceReference must be at most 1000 characters");
            }

            return errors.AsReadOnly();
        }
    }
}
