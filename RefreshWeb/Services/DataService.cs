using Microsoft.EntityFrameworkCore;
using RefreshBot.Models;
using RefreshWeb.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RefreshWeb.Services
{
    public class DataService
    {
        private readonly EntityContext _context;

        public DataService(EntityContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TargetPage>> GetActiveTargetPagesAsync()
        {
            return await _context.TargetPages.Where(t => t.IsActive).ToListAsync();
        }
    }
}
