using Application.Endpoint.Commands;
using Application.Endpoint.DTOs;
using Domain.Endpoint.Entities;
using MediatR;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebApi.Controllers
{
    public class DishesController : ApiController
    {
        private readonly IMediator mediator;
        public DishesController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost]
        public async Task<IHttpActionResult> CreateAsync([FromBody] CreateDishDto dishDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Dish dish = await mediator.Send(new CreateDishCommand(dishDto, Guid.NewGuid()));
            var url = Url.Content("~/") + "/api/dishes/" + dish.Id;
            return Created(url, dish);
        }
    }
}
