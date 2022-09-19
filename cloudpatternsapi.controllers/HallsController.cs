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
    public class HallsController : ControllerBase
    {
        private readonly IHallsService _hallsService;

        public HallsController(IHallsService hallsService)
        {
            _hallsService = hallsService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<HallDto>>> GetHalls([FromQuery] HallParams hallParams)
        {
            var response = await _hallsService.GetHalls(hallParams);
            Response.AddPaginationHeader(response.Item2.CurrentPage, response.Item2.PageSize, response.Item2.TotalCount, response.Item2.TotalPages);
            return Ok(response.Item1);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<HallDto>> GetHallById(int id)
        {
            var response= await _hallsService.GetHallById(id);
            return Ok(response);
        }

        [HttpGet("GetShowsOfHall/{id}")]
        public async Task<ActionResult<IEnumerable<ShowDto>>> GetShowsOfHall(int id, [FromQuery] HallParams hallParams)
        {
            var response = await _hallsService.GetShowsOfHall(id, hallParams);
            return Ok(response);
        }

        [HttpGet("GetWithoutPagination")]
        public async Task<ActionResult<IEnumerable<HallDto>>> GetWithoutPagination()
        {
            var response = await _hallsService.GetWithoutPagination();
            return Ok(response);
        }

        [HttpPost("add")]
        public async Task<ActionResult> AddHall(HallDto hallDto)
        {
            await _hallsService.AddHall(hallDto);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateHall([FromBody] HallUpdateDto hallUpdateDto, int id)
        {
            await _hallsService.UpdateHall(hallUpdateDto, id);
            return Ok();
        }
    }
}
