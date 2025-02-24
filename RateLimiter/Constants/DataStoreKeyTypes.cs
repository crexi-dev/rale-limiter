namespace RateLimiter.Constants
{
    public enum DataStoreKeyTypes
    {
        RequestsPerResource,
        RequestsPerUser,
        RequestsPerOrganization,
        RequestsPerIpAddress,
        RequestsPerRegion,
        RequestsPerUserPerResource,
        RequestsPerOrganizationUserPerResource,
        RequestsPerOrganizationPerResource,
        RequestsPerIpAddressPerResource,
        RequestsPerRegionPerResource
    }
}