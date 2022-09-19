using AutoMapper;
using cloudpatternsapi.dto;
using cloudpatternsapi.implementation;
using cloudpatternsapi.interfaces;
using cloudpatternsapi.interfaces.services;
using cloudpatternsapi.models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cloudpatternsapi.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShowController : ControllerBase
    {
        private readonly IShowService _showService;

        public ShowController(IShowService showService)
        {
            _showService = showService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShowDto>>> GetShows([FromQuery] ShowParams showParams)
        {
            var response = await _showService.GetShows(showParams);
            Response.AddPaginationHeader(response.Item2.CurrentPage, response.Item2.PageSize, response.Item2.TotalCount, response.Item2.TotalPages);
            return Ok(response.Item1);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetShowById(int id)
        {
            var response = await _showService.GetShowById(id);
            return Ok(response);
        }

        [HttpPost("add")]
        public async Task<ActionResult<ShowDto>> Add(CreateShowDto createShowDto)
        {
            var response = await _showService.Add(createShowDto);
            return Ok(response);
        }

        [HttpGet("GetHallOfShow/{id}")]
        public async Task<ActionResult<ShowHallDto>> GetHallOfShow(int id)
        {
            var response = await _showService.GetHallOfShow(id);
            return Ok(response);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateShow([FromBody] ShowUpdateDto showUpdateDto, int id)
        {
            await _showService.UpdateShow(showUpdateDto, id);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteShow(int id)
        {
            await _showService.DeleteShow(id);
            return Ok();
        }

        [HttpPost("ChangeHallOfShow")]
        public async Task<ActionResult> ChangeHallOfShow([FromQuery] int showId, [FromQuery] int newHallId)
        {
            await _showService.ChangeHallOfShow(showId, newHallId);
            return Ok();
        }

        [HttpGet("GetSeatsOfShow")]
        public async Task<ActionResult<IEnumerable<SeatsShowDto>>> GetSeatsOfShow([FromQuery] int showId, [FromQuery] DateTime showDate)
        {
            var response = await _showService.GetSeatsOfShow(showId, showDate);
            return Ok(response);
            
        }

        [HttpGet("GetShowsForDate")]
        public async Task<ActionResult<IEnumerable<ShowDto>>> GetShowsForDate([FromQuery] DateTime dateGiven)
        {
            var response = await _showService.GetShowsForDate(dateGiven);
            return Ok(response);
        }
    }
}
