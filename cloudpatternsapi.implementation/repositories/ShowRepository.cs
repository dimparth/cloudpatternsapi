using cloudpatternsapi.interfaces;
using cloudpatternsapi.models;
using Microsoft.EntityFrameworkCore;
using PagedListForEFCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cloudpatternsapi.implementation.repositories
{
    public class ShowRepository : IShowRepository
    {
        private readonly BookingContext _context;

        public ShowRepository(BookingContext context)
        {
            _context = context;
        }

        public void Add(Show show)
        {
            _context.Shows?.Add(show);
        }

        public async Task<bool> Complete()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<PagedList<Show>> GetAllShowsAsync(ShowParams showParams)
        {
            var query = _context.Shows?.Include(s => s.Hall).AsQueryable();

            if (!string.IsNullOrWhiteSpace(showParams.SearchTitle))
            {
                query = query?.Where(s => s.Title!.Contains(showParams.SearchTitle));
            }
            query = query?.Where(s => s.DateEnd >= DateTime.Now);

            query = showParams.OrderBy switch
            {
                "title" => query?.OrderByDescending(s => s.Title),
                _ => query?.OrderByDescending(s => s.DateStart)
            };
            return await PagedList<Show>.CreateAsync(query!, showParams.PageNumber, showParams.PageSize);
        }

        public async Task<Hall> GetHallOfShowAsync(int id)
        {
            var show = await _context.Shows!.Include(h => h.Hall)
                .SingleOrDefaultAsync(p => p.Id == id);
            return show?.Hall!;
        }

        public async Task<Show?> GetShowByIdAsync(int id)
        {
            return await _context.Shows!
                .Include(s => s.Hall)
                .SingleOrDefaultAsync(p => p.Id == id);
        }

        public void Update(Show? Show)
        {
            _context.Entry(Show!).State = EntityState.Modified;
        }

        public void Delete(Show show)
        {
            _context.Shows?.Remove(show);
        }

        public async Task<IReadOnlyList<Show>> GetShowsForSpecificDateAsync(DateTime dateGiven)
        {
            return await _context.Shows!
                .Include(s => s.Hall)
                .Where(p => dateGiven >= p.DateStart && dateGiven <= p.DateEnd)
                .ToListAsync();
        }
    }
}
