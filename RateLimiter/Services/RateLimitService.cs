using FluentValidation;
using RateLimiter.Interfaces;
using RateLimiter.Models.Apis;
using RateLimiter.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Services
{
    /// <summary>
    /// Class for rate limiter service.
    /// </summary>
    public class RateLimitService : IRateLimitService
    {
        private readonly IUserRepository _userRepository;
        private readonly IResourceRepository _resourceRepository;
        private readonly IPolicyService _policyService;
        private readonly IValidator<RateLimitRequest> _validator;

        public RateLimitService(
            IUserRepository userRepository, IResourceRepository resourceRepository, 
            IPolicyService policyService, IValidator<RateLimitRequest> validator) 
        {
            _userRepository = userRepository;
            _resourceRepository = resourceRepository;
            _policyService = policyService;
            _validator = validator;
        }

        public async Task<RateLimiteResponse> CheckRateLimitAsync(RateLimitRequest request)
        {
            // validate the request
            var (user, resource, validationErrors) = await ValidateRequest(request);
            if (validationErrors?.Any() == true)
            {
                return new RateLimiteResponse()
                {
                    Success = false,
                    Errors = validationErrors
                };
            }

            // get policies for this user to access the specified resource
            var userPolicies = await _policyService.GetRatePoliciesAsync(request.UserId, request.ResourceId);
            if (!(userPolicies?.Any() == true))
            {
                // no policy specified, assume the user cannot access the resource in any way
                return new RateLimiteResponse()
                { 
                    Success = false,
                    Errors = new List<string>()
                    {
                        $"No access policy found for user {request.UserId} and resource {request.ResourceId}"
                    }
                };
            }

            // check whether the request conform to all policies
            var result = new RateLimiteResponse()
            {
                Success = true,
                Errors = new List<string>(),
            };
            foreach (var policy in userPolicies)
            {
                var policyStatus = await policy.GetPolicyStatus(_policyService.Verifier, request);
                // AND the policy status and return its error if needed
                result.Success &= policyStatus.IsConforming;
                if (!policyStatus.IsConforming && !string.IsNullOrWhiteSpace(policyStatus.NotConformingReason))
                {
                    result.Errors.Add(policyStatus.NotConformingReason);
                }
            }

            if (result.Success)
            {
                await _userRepository.AddUserActivityAsync(request.UserId, request.ResourceId);
            }

            return result;
        }

        private async Task<(User?, Resource?, List<string>?)> ValidateRequest(RateLimitRequest request)
        {
            var validation = await _validator.ValidateAsync(request);
            if (validation == null || !validation.IsValid)
            {
                return (null, null, validation?.ToDictionary()?.Keys?.ToList());
            }

            var user = await _userRepository.GetByUserAsync(request.UserId);
            if (user == null)
            {
                return (null, null, new List<string>()
                {
                    $"Cannot find the user {request.UserId}."
                });
            }

            var resource = await _resourceRepository.GetResourceAsync(request.ResourceId);
            if (resource == null)
            {
                return (user, resource, new List<string>()
                {
                    $"Cannot find the resource {request.ResourceId}."
                });
            }

            return (user, resource, null);
        }
    }
}

