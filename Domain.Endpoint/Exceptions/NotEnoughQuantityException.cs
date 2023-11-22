using System.Runtime.Serialization;
using System;

namespace Domain.Endpoint.Exceptions
{
    [Serializable]
    public class NotEnoughQuantityException : Exception
    {
        private int quantity1;
        private int quantity2;

        public NotEnoughQuantityException()
        {
        }

        public NotEnoughQuantityException(string message) : base(message)
        {
        }

        public NotEnoughQuantityException(int quantity1, int quantity2) : this($"Not enough quantity. Quantity: {quantity1}, Required: {quantity2}")
        {
            this.quantity1 = quantity1;
            this.quantity2 = quantity2;
        }
    }
}
