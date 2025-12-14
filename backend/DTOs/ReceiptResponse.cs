namespace backend.DTOs;

public class ReceiptResponse
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    
    // Business Information
    public string BusinessName { get; set; } = string.Empty;
    public string? BusinessDescription { get; set; }
    public string BusinessAddress { get; set; } = string.Empty;
    public string BusinessPhone { get; set; } = string.Empty;
    public string BusinessEmail { get; set; } = string.Empty;
    
    // Order Items
    public List<ReceiptItemResponse> Items { get; set; } = new();
    
    // Totals
    public decimal SubTotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    
    // Payment Information
    public List<ReceiptPaymentResponse> Payments { get; set; } = new();
    public decimal TotalPaid { get; set; }
    public decimal RemainingBalance { get; set; }
    
    // Employee Information
    public string? CreatedByName { get; set; }
}

public class ReceiptItemResponse
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class ReceiptPaymentResponse
{
    public int PaymentId { get; set; }
    public string Method { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaidAt { get; set; }
    public string? TransactionId { get; set; }
}
