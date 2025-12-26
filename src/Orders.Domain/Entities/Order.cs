using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Domain.Entities
{
    public class Order
    {
        public Guid Id {  get; private set; }
        public string CustomerName { get; private set; } = string.Empty;
        public decimal Amount { get; private set; }
        public DateTime OrderDate { get; private set; }

        public Order()
        {
            
        }

        public Order(Guid id, string customerName, decimal amount, DateTime orderDate)
        {
            if (string.IsNullOrWhiteSpace(customerName)) 
            {
                throw new ArgumentException("CustomerName is required.", nameof(customerName));
            }

            if (customerName.Length > 200)
            {
                throw new ArgumentException("CustomerName must be <= 200 characters.", nameof (customerName));
            }

            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than zero.", nameof(amount));
            }

            Id = id == Guid.Empty ? Guid.Empty : id;
            CustomerName = customerName.Trim();
            Amount = amount;
            OrderDate = orderDate;
        }
    }
}
