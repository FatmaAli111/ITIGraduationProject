using System;
using System.Collections.Generic;

namespace ITIGraduationProject.Application.DTOS.ShopDTOs
{
    public class CartDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
        public decimal TotalCost { get; set; }
        public int ItemCount { get; set; }
    }
}
