using Domain.Pengumuman;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Diagnostics;

namespace API.Controllers.Pengumumans
{
    public class PengumumansController : BaseApiController
    {
        private readonly DataContext _context;

        public PengumumansController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Pengumuman>>> GetPengumumans()
        {
            return await _context.Pengumumans.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Pengumuman>> GetPengumuman(Guid id)
        {
            return await _context.Pengumumans.FindAsync(id);
        }
    }
}