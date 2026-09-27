namespace OrderService.BLL;

public record class CreateNewOrderDTO(Guid BuyerId, decimal TotalPrice);