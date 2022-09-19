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
    public class HallRepository : IHallRepository
    {
        private readonly BookingContext _context;

        public HallRepository(BookingContext context)
        {
            _context = context;
        }

        public void Add(Hall hall)
        {
            _context.Halls?.Add(hall);
        }

        public async Task<PagedList<Hall>> GetAllHallsAsync(HallParams hallParams)
        {
            var query = _context.Halls?
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(hallParams.SearchTitle))
            {
                query = query?.Where(h => h.Title!.Contains(hallParams.SearchTitle));
            }

            if (hallParams.MaxCapacity > 0)
            {
                query = query?.Where(h => h.Capacity <= hallParams.MaxCapacity);
            }
            query = query?.Where(h => h.Capacity >= hallParams.MinCapacity);

            query = hallParams.OrderBy switch
            {
                "title" => query?.OrderByDescending(s => s.Title),
                _ => query?.OrderByDescending(s => s.Capacity)
            } ?? throw new Exception();

            return await PagedList<Hall>.CreateAsync(query, hallParams.PageNumber, hallParams.PageSize);
        }

        public async Task<Hall> GetHallByIdAsync(int id)
        {
            return await _context.Halls!
                .Include(hall => hall.Shows)
                .SingleOrDefaultAsync(h => h.Id == id) ?? throw new Exception();
        }

        public void Update(Hall hall)
        {
            _context.Entry(hall).State = EntityState.Modified;
        }

        public void AddShow(Hall hall, Show show)
        {
            hall.Shows?.Add(show);
        }

        public async Task<bool> Complete()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IReadOnlyList<Show>> GetShowsOfHallAsync(int id, HallParams hallParams)
        {
            var hall = await _context.Halls!
                .Include(p => p.Shows)
                .SingleOrDefaultAsync(p => p.Id == id);

            return hall!.Shows!.Where(s => s.DateEnd >= DateTime.Now).ToArray();

            //var query = hall?.Shows.AsQueryable();

            //eturn await PagedList<Show>.CreateAsync(query, hallParams.PageNumber, hallParams.PageSize);
        }

        public async Task<IReadOnlyList<Hall>> GetHallsWithoutPaginationAsync()
        {
            return await _context.Halls!
                .ToListAsync();
        }
    }
}
