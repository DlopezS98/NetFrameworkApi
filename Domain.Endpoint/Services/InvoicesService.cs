using Domain.Endpoint.Entities;
using Domain.Endpoint.Exceptions;
using Domain.Endpoint.Interfaces.Repositories;
using Domain.Endpoint.Interfaces.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Endpoint.Services
{
    public class InvoicesService : IInvoicesService
    {
        private readonly IInvoicesRepository invoicesRepository;
        private readonly IProductDetailsRepository productDetailsRepository;
        private readonly IDishesRepository dishesRepository;

        public InvoicesService(IInvoicesRepository invoicesRepository, IProductDetailsRepository productDetailsRepository, IDishesRepository dishesRepository)
        {
            this.invoicesRepository = invoicesRepository;
            this.productDetailsRepository = productDetailsRepository;
            this.dishesRepository = dishesRepository;
        }

        public async Task<Invoice> CreateAsync(Invoice invoice)
        {
            Invoice newInvoice = Clone(invoice);
            ICollection<InvoiceDetail> tempDetails = Clone(newInvoice.InvoiceDetails);
            newInvoice.ClearDetail();

            foreach (InvoiceDetail detail in tempDetails)
            {
                // Obtener el producto...
                // Validar la existencia...
                BaseItem baseItem = await GetBaseItem(detail);
                newInvoice.AddDetail(baseItem, detail.Quantity, detail.Discount);
                // Actualizar el producto - await productDetailsRepository.UpdateAsync();
            }

            await invoicesRepository.CreateAsync(newInvoice);
            return newInvoice;
        }

        public async Task<TItem> GetBaseItem<TItem>(InvoiceDetail detail) where TItem : BaseItem
        {
            ICollection<string> items = new List<string> { BaseItem.Dish, BaseItem.SingleProduct };
            if (!items.Contains(detail.ItemType)) throw new ItemTypeNotAllowedException(detail.ItemType);

            if (detail.ItemType == BaseItem.SingleProduct)
            {
                Guid productId = detail.ProductDetailId ?? throw new Exception("Invalid Entity Exception");

                ProductDetail productDetail = await productDetailsRepository.GetByIdAsync(productId);
                if (productDetail is null) throw new EntityNotFoundException(productId);

                //if (productDetail.Quantity < detail.Quantity) throw new NotEnoughQuantityException(productDetail.Quantity, detail.Quantity);
                //productDetail.Quantity -= detail.Quantity;
                //await productDetailsRepository.UpdateAsync(productDetail);

                return productDetail;
            }

            Guid dishId = detail.DishId ?? throw new Exception("Invalid Entity Exception");
            Dish dish = await dishesRepository.GetByIdAsync(dishId);
            if (dish is null) throw new EntityNotFoundException(dishId);

            BaseItem dishItem = dish.ToBaseItem();
            return dishItem;
        }

        public Task DeleteAsync(Invoice invoice)
        {
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Invoice invoice)
        {
            return Task.CompletedTask;
        }

        public static T Clone<T>(T source)
        {
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(serialized);
        }
    }
}
