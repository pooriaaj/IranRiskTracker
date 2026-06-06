using System;
using System.Collections.Generic;
using IranRiskTracker.Application.DTOs;
using IranRiskTracker.Application.Interfaces;
using IranRiskTracker.Application.Validation;

namespace IranRiskTracker.Application.Services
{
    public class OwnerOverrideService : IOwnerOverrideService
    {
        private readonly IOwnerOverrideStore _store;

        public OwnerOverrideService(IOwnerOverrideStore store)
        {
            _store = store;
        }

        public OwnerOverrideDto Add(OwnerOverrideCreateRequest request)
        {
            var errors = OwnerOverrideValidator.Validate(request);
            if (errors != null && errors.Count > 0)
            {
                throw new ArgumentException(string.Join("; ", errors));
            }

            var dto = new OwnerOverrideDto
            {
                Id = Guid.NewGuid(),
                Title = request.Title.Trim(),
                Reasoning = request.Reasoning.Trim(),
                Category = request.Category,
                ScoreAdjustment = request.ScoreAdjustment,
                AppliedAt = request.AppliedAt,
                SourceReference = request.SourceReference
            };

            return _store.Add(dto);
        }

        public IReadOnlyCollection<OwnerOverrideDto> GetAll()
        {
            return _store.GetAll();
        }
    }
}
