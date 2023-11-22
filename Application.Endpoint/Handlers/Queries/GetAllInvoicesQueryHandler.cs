using Application.Endpoint.Queries;
using Domain.Endpoint.Entities;
using Domain.Endpoint.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Endpoint.Handlers.Queries
{
    public class GetAllInvoicesQueryHandler : IRequestHandler<GetAllInvoicesQuery, List<Invoice>>
    {
        private readonly IInvoicesRepository invoicesRepository;
        public GetAllInvoicesQueryHandler(IInvoicesRepository invoicesRepository)
        {
            this.invoicesRepository = invoicesRepository;
        }

        public Task<List<Invoice>> Handle(GetAllInvoicesQuery request, CancellationToken cancellationToken)
        {
            return invoicesRepository.GetAsync();
        }
    }
}
