using LanAnhComputer.Data;
using LanAnhComputer.Constants;
using LanAnhComputer.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;

namespace LanAnhComputer.Controllers;

[ApiController]
[Route("api/payment")]
public class PaymentController : ControllerBase
{
    private readonly PayOSClient _client;
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public PaymentController(
        PayOSClient client,
        AppDbContext dbContext,
        IConfiguration configuration)
    {
        _client = client;
        _dbContext = dbContext;
        _configuration = configuration;
    }

    [HttpPost("create/{orderId}")]
    public async Task<IActionResult> CreatePayment(long orderId)
    {
        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(x => x.OrderId == orderId);

        if (order == null)
            return NotFound();

        if (string.Equals(order.PaymentStatus, PaymentStatuses.Paid, StringComparison.OrdinalIgnoreCase))
            return BadRequest("Order already paid");

        if (!string.IsNullOrEmpty(order.PaymentLinkId))
        {
            return Ok(new
            {
                order.OrderId,
                order.CheckoutUrl
            });
        }

        var payOSOrderCode = order.OrderId;

        var returnUrl = _configuration["PayOS:ReturnUrl"]
            ?? "https://localhost:7020/Checkout/Complete";

        var cancelUrl = _configuration["PayOS:CancelUrl"]
            ?? "https://localhost:7020/Checkout";

        var paymentRequest = new CreatePaymentLinkRequest
        {
            OrderCode = payOSOrderCode,
            Amount = (int)order.TotalAmount,
            Description = $"DH{order.OrderId}",
            ReturnUrl = $"{returnUrl}?orderId={order.OrderId}",
            CancelUrl = cancelUrl
        };

        var paymentLink = await _client.PaymentRequests.CreateAsync(paymentRequest);

        order.PayOSOrderCode = payOSOrderCode;
        order.PaymentLinkId = paymentLink.PaymentLinkId;
        order.CheckoutUrl = paymentLink.CheckoutUrl;
        order.PaymentStatus = PaymentStatuses.Pending;

        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            orderId = order.OrderId,
            checkoutUrl = paymentLink.CheckoutUrl,
            qrCode = paymentLink.QrCode,
            bin = paymentLink.Bin,
            accountNumber = paymentLink.AccountNumber,
            accountName = paymentLink.AccountName,
            description = paymentLink.Description,
            amount = paymentLink.Amount
        });
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook([FromBody] Webhook webhook)
    {
        try
        {
            var data = await _client.Webhooks.VerifyAsync(webhook);

            var order = await _dbContext.Orders
                .Include(x => x.OrderDetails)
                .FirstOrDefaultAsync(x => x.PayOSOrderCode == data.OrderCode);

            if (order == null)
            {
                return Ok();
            }

            await CompleteOrderPaymentAsync(order, data.Reference);
            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PayOS Webhook Error]: {ex.Message}");
            return Ok();
        }
    }

    [HttpPost("confirm-webhook")]
    public async Task<IActionResult> ConfirmWebhook()
    {
        var webhookUrl = _configuration["PayOS:WebhookUrl"];

        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            webhookUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/api/payment/webhook";
        }

        if (!Uri.TryCreate(webhookUrl, UriKind.Absolute, out _))
        {
            return BadRequest("PayOS webhook URL is invalid.");
        }

        var result = await _client.Webhooks.ConfirmAsync(webhookUrl);

        return Ok(result);
    }

    [HttpGet("status/{orderId}")]
    public async Task<IActionResult> CheckStatus(long orderId)
    {
        var order = await _dbContext.Orders
            .Include(x => x.OrderDetails)
            .FirstOrDefaultAsync(x => x.OrderId == orderId);

        if (order == null)
            return NotFound();

        if (!string.Equals(order.PaymentStatus, PaymentStatuses.Paid, StringComparison.OrdinalIgnoreCase))
        {
            await TrySyncPaymentFromPayOSAsync(order);
        }

        return Ok(new
        {
            paymentStatus = order.PaymentStatus
        });
    }

    private async Task TrySyncPaymentFromPayOSAsync(Order order)
    {
        if (!order.PayOSOrderCode.HasValue)
        {
            return;
        }

        try
        {
            var paymentLink = await _client.PaymentRequests.GetAsync(order.PayOSOrderCode.Value);
            if (!IsPayOSPaymentCompleted(paymentLink))
            {
                return;
            }

            var transactionId = paymentLink.Transactions?.LastOrDefault()?.Reference;
            await CompleteOrderPaymentAsync(order, transactionId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PayOS Sync Error] Order {order.OrderId}: {ex.Message}");
        }
    }

    private static bool IsPayOSPaymentCompleted(PaymentLink paymentLink)
    {
        var status = paymentLink.Status.ToString();
        return string.Equals(status, "PAID", StringComparison.OrdinalIgnoreCase)
               || paymentLink.AmountPaid >= paymentLink.Amount;
    }

    private async Task CompleteOrderPaymentAsync(Order order, string? transactionId)
    {
        if (string.Equals(order.PaymentStatus, PaymentStatuses.Paid, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        order.PaymentStatus = PaymentStatuses.Paid;
        order.OrderStatus = OrderStatuses.Shipped;
        order.PaidAt = DateTime.UtcNow;
        order.TransactionId = transactionId;
        order.UpdatedAt = DateTime.UtcNow;

        foreach (var detail in order.OrderDetails)
        {
            var product = await _dbContext.Products
                .FirstOrDefaultAsync(x => x.ProductId == detail.ProductId);

            if (product != null)
            {
                product.SoldQuantity += detail.Quantity;
            }
        }

        var cart = await _dbContext.Carts
            .FirstOrDefaultAsync(x => x.UserId == order.UserId);

        if (cart != null)
        {
            var cartItems = await _dbContext.CartItems
                .Where(x => x.CartId == cart.CartId)
                .ToListAsync();

            _dbContext.CartItems.RemoveRange(cartItems);
        }

        await _dbContext.SaveChangesAsync();
    }
}
