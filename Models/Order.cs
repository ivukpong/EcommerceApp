using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace RobustEcommerceApp.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public IdentityUser User { get; set; } = null!;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public int ShippingAddressId { get; set; }
        public ShippingAddress ShippingAddress { get; set; } = null!;
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal Price { get; set; } // Add Price property if not present
    }

    public class ShippingAddress
    {
        public int Id { get; set; }
        public string Street { get; set; } = null!;
        public string City { get; set; } = null!;
        public string PostalCode { get; set; } = null!;
        public string Country { get; set; } = null!;
    }
}
