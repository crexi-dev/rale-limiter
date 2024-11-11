using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RateLimiter.ViewModel;
using Services.Common.Repositories;

namespace RateLimiter.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropertyController(IDataRepository<Property> repo) : Controller
{
    [HttpGet(Name = "GetProperties")]
    public async Task<IEnumerable<Property>> GetAllAsync()
    {
        return await repo.GetAllAsync().ConfigureAwait(false);
    }
    
    [HttpGet("{id}", Name = "GetProperty")]
    public async Task<Property> GetByIdAsync(Guid id)
    {
        return await repo.GetByIdAsync(id).ConfigureAwait(false);
    }
}