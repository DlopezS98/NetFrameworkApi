using Domain.Endpoint.Entities;
using Domain.Endpoint.Interfaces.Repositories;
using Infrastructure.Endpoint.Data.Builders;
using Infrastructure.Endpoint.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Endpoint.Data.Repositories
{
    public class InvoicesRepository : GenericRepository<Invoice>, IInvoicesRepository
    {
        public InvoicesRepository(ISqlDbConnection sqlDbConnection, ISqlCommandOperationBuilder operationBuilder) : base(sqlDbConnection, operationBuilder)
        {
        }

        public override async Task CreateAsync(Invoice entity)
        {
            SqlCommand invoiceCommand = operationBuilder.From(entity)
                .WithOperation(SqlWriteOperation.Create)
                .BuildWritter();

            List<SqlCommand> detailCommands = entity.InvoiceDetails.Select(invoiceDetail =>
            {
                return operationBuilder.From(invoiceDetail)
                 .WithOperation(SqlWriteOperation.Create)
                 .BuildWritter();
            }).ToList();

            List<SqlCommand> commands = new List<SqlCommand> { invoiceCommand };
            commands.AddRange(detailCommands);

            bool success = await sqlDbConnection.RunTransactionAsync(commands.ToArray());
        }

        public async Task<List<Invoice>> GetAsync()
        {
            DataTable dataTable = await GetDataTableAsync();
            return dataTable.AsEnumerable()
                .Select(MapEntityFromDataRow)
                .ToList();
        }

        public async Task<Invoice> GetByIdAsync(Guid id)
        {
            DataTable dataTable = await GetDataTableByIdAsync(id);
            return dataTable.AsEnumerable()
                .Select(MapEntityFromDataRow)
                .FirstOrDefault();
        }

        private Invoice MapEntityFromDataRow(DataRow row)
        {
            Invoice invoice = new Invoice
            {
                Id = sqlDbConnection.GetDataRowValue<Guid>(row, "Id"),
                Number = sqlDbConnection.GetDataRowValue<string>(row, "Number"),
                CustomerName = sqlDbConnection.GetDataRowValue<string>(row, "CustomerName"),
                Notes = sqlDbConnection.GetDataRowValue<string>(row, "Notes"),
                Quantity = sqlDbConnection.GetDataRowValue<int>(row, "Quantity"),
                Subtotal = sqlDbConnection.GetDataRowValue<decimal>(row, "Subtotal"),
                Discount = sqlDbConnection.GetDataRowValue<decimal>(row, "Discount"),
                Total = sqlDbConnection.GetDataRowValue<decimal>(row, "Total"),
                CreatedAt = sqlDbConnection.GetDataRowValue<DateTime>(row, "CreatedAt"),
                CreatedBy = sqlDbConnection.GetDataRowValue<Guid>(row, "CreatedBy"),
                UpdatedAt = sqlDbConnection.GetDataRowValue<DateTime?>(row, "UpdatedAt"),
                UpdatedBy = sqlDbConnection.GetDataRowValue<Guid?>(row, "UpdatedBy"),
            };

            return invoice;
        }
    }
}
