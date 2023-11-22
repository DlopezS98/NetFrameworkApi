using Application.Endpoint.Commands;
using Application.Endpoint.DTOs;
using Application.Endpoint.Queries;
using Domain.Endpoint.DTOs;
using Domain.Endpoint.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using WebApi.App_Start.Filters;

namespace WebApi.Controllers
{
    public class InvoicesController : ApiController
    {
        private readonly IMediator bus;
        public InvoicesController(IMediator mediator)
        {
            bus = mediator;
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetAll()
        {
            List<Invoice> invoices = await bus.Send(new GetAllInvoicesQuery());
            return Ok(invoices);
        }

        [HttpPost]
        [ValidateModel]
        public async Task<IHttpActionResult> CreateAsync(CreateInvoiceDto invoiceDto)
        {
            Invoice invoice = await bus.Send(new CreateInvoiceCommand(invoiceDto, Guid.NewGuid()));
            var url = Url.Content("~/") + "api/invoices/" + invoice.Id;
            return Created(url, invoice);
        }
    }
}
