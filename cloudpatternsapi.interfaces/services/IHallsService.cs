using cloudpatternsapi.dto;
using cloudpatternsapi.models;
using PagedListForEFCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cloudpatternsapi.interfaces.services
{
    public interface IHallsService
    {
        Task<Tuple<IList<HallDto>, PagedListHeaders>> GetHalls(HallParams hallParams);
        Task<HallDto> GetHallById(int id);
        Task<IList<ShowDto>> GetShowsOfHall(int id, HallParams hallParams);
        Task<IList<HallDto>> GetWithoutPagination();
        Task AddHall(HallDto hallDto);
        Task UpdateHall(HallUpdateDto hallUpdateDto, int id);
    }
}
