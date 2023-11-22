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
            List<ProductDetail> productDetails = new List<ProductDetail>();

            // Recorrer los detalles de la factura
            foreach (InvoiceDetail detail in tempDetails)
            {
                BaseItem baseItem;
                switch (detail.ItemType)
                {
                    case BaseItem.SingleProduct:
                        {
                            // Obtener el producto y validar si existe
                            ProductDetail productDetail = await GetProductDetail(detail);
                            // Validar la cantidad de existencia
                            if (productDetail.Quantity < detail.Quantity)
                                throw new NotEnoughQuantityException(productDetail.Quantity, detail.Quantity);

                            productDetail.Quantity -= detail.Quantity;
                            // Agregar los productos a una lista temporar para posteriormente ser actualizados una vez creada la factura
                            productDetails.Add(productDetail);
                            baseItem = productDetail;
                        }
                        break;

                    case BaseItem.Dish:
                        baseItem = await GetDishFromInvoiceDetail(detail);
                        break;

                    default: throw new ItemTypeNotAllowedException(detail.ItemType);
                }

                newInvoice.AddDetail(baseItem, detail.Quantity, detail.Discount);
            }

            // Crear primero la factura...
            await invoicesRepository.CreateAsync(newInvoice);

            // Actualizar los productos (existencias)
            foreach (ProductDetail product in productDetails)
            {
                // Nota: Generar metodo que reciba multiples productos para hacer una operaciones por lote
                // en lugar de hacerlo uno a uno, el rendimiento de la app sera mucho mejor
                await productDetailsRepository.UpdateAsync(product);
            }

            return newInvoice;
        }

        private async Task<ProductDetail> GetProductDetail(InvoiceDetail detail)
        {
            Guid productId = detail.ProductDetailId ?? throw new Exception("Invalid Entity Exception");
            ProductDetail productDetail = await productDetailsRepository.GetByIdAsync(productId);
            if (productDetail is null) throw new EntityNotFoundException(productId);

            return productDetail;
        }

        private async Task<Dish> GetDishFromInvoiceDetail(InvoiceDetail detail)
        {
            Guid dishId = detail.DishId ?? throw new Exception("Invalid Entity Exception");
            Dish dish = await dishesRepository.GetByIdAsync(dishId);
            if (dish is null) throw new EntityNotFoundException(dishId);

            return dish;
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
