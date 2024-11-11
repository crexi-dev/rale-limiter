using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RateLimiter.ViewModel;
using Services.Common.Repositories;

namespace RateLimiter.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IDataRepository<User> repo) : Controller
{
    [HttpGet(Name = "GetUsers")]
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await repo.GetAllAsync().ConfigureAwait(false);
    }
    
    [HttpGet("{id}", Name = "GetUser")]
    public async Task<User> GetByIdAsync(Guid id)
    {
        return await repo.GetByIdAsync(id).ConfigureAwait(false);
    }
}