namespace OrderService.BLL;

public record class MakeNewOrderDTO(Guid BuyerId, decimal totalPrice);