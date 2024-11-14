using RateLimiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Interfaces
{
    public interface IAuditService<T,F>
    {
        public Task<bool> Log(T data);
        public Task<List<T>> GetHistory(F filter);
    }
}
