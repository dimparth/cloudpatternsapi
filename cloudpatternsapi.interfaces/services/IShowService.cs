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
    public interface IShowService
    {
        Task<Tuple<IList<ShowDto>, PagedListHeaders>> GetShows(ShowParams showParams);
        Task<ShowDto> GetShowById(int id);
        Task<ShowDto> Add(CreateShowDto createShowDto);
        Task<ShowHallDto> GetHallOfShow(int id);
        Task UpdateShow(ShowUpdateDto showUpdateDto, int id);
        Task DeleteShow(int id);
        Task ChangeHallOfShow(int showId, int newHallId);
        Task<IList<SeatsShowDto>> GetSeatsOfShow(int showId, DateTime showDate);
        Task<IList<ShowDto>> GetShowsForDate(DateTime dateGiven);
    }
}
