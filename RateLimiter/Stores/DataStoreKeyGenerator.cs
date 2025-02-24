using RateLimiter.Constants;
using RateLimiter.Exceptions;
using RateLimiter.Models;

namespace RateLimiter.Stores
{
    public class DataStoreKeyGenerator : IDataStoreKeyGenerator
    {
        private readonly DataStoreKeyTypes _ruleType;

        public DataStoreKeyGenerator(DataStoreKeyTypes ruleType)
        {
            _ruleType = ruleType;
        }

        public string GenerateKey(RequestModel request)
        {
            switch (_ruleType)
            {
                case DataStoreKeyTypes.RequestsPerResource:
                    return $"{request.RequestPath}";
                case DataStoreKeyTypes.RequestsPerUser:
                    return $"{request.UserId}";
                case DataStoreKeyTypes.RequestsPerIpAddress:
                    return $"{request.IpAddress}";
                case DataStoreKeyTypes.RequestsPerOrganization:
                    return $"{request.OrganizationId}";
                case DataStoreKeyTypes.RequestsPerUserPerResource:
                    return $"{request.UserId}:{request.RequestPath}";
                case DataStoreKeyTypes.RequestsPerOrganizationPerResource:
                    return $"{request.OrganizationId}:{request.RequestPath}";
                case DataStoreKeyTypes.RequestsPerRegionPerResource:
                    return $"{request.Region}:{request.RequestPath}";
                case DataStoreKeyTypes.RequestsPerIpAddressPerResource:
                    return $"{request.IpAddress}:{request.RequestPath}";
                case DataStoreKeyTypes.RequestsPerOrganizationUserPerResource:
                    return $"{request.UserId}:{request.OrganizationId}:{request.RequestPath}";
                default:
                    throw new DataStoreKeyTypeNotImplementedException();
            }
        }
    }
}
