namespace PaymentService.Application;

public class DuplicateException(string message, Exception innerException) : Exception(message, innerException);
