using LanAnhComputer.Data;
using LanAnhComputer.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayOS;
using PayOS.Models;
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

        // ❗ nếu đã thanh toán thì không tạo lại
        if (string.Equals(order.PaymentStatus, PaymentStatuses.Paid, StringComparison.OrdinalIgnoreCase))
            return BadRequest("Order already paid");

        // ❗ nếu đã có link thì trả lại luôn
        if (!string.IsNullOrEmpty(order.PaymentLinkId))
        {
            return Ok(new
            {
                order.OrderId,
                order.CheckoutUrl
            });
        }

        // ✔ FIX: dùng OrderId làm PayOSOrderCode
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
            ReturnUrl = returnUrl,
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
            // 1. Verify webhook (chống fake)
            var data = await _client.Webhooks.VerifyAsync(webhook);

            var orderCode = data.OrderCode;

            // 2. Tìm order
            var order = await _dbContext.Orders
     .Include(x => x.OrderDetails)
     .FirstOrDefaultAsync(x => x.PayOSOrderCode == orderCode);

            // ❗ KHÔNG return NotFound trong webhook
            if (order == null)
            {
                return Ok();
            }

            // 3. CHỐNG xử lý lại nhiều lần (idempotent)
            if (string.Equals(order.PaymentStatus, PaymentStatuses.Paid, StringComparison.OrdinalIgnoreCase))
            {
                return Ok();
            }

            // 4. Update trạng thái
            order.PaymentStatus = PaymentStatuses.Paid;
            order.OrderStatus = OrderStatuses.Shipped;
            order.PaidAt = DateTime.UtcNow;
            order.TransactionId = data.Reference;

            foreach (var detail in order.OrderDetails)
            {
                var product = await _dbContext.Products
                    .FirstOrDefaultAsync(x => x.ProductId == detail.ProductId);

                if (product != null)
                {
                    product.SoldQuantity += detail.Quantity;
                }
            }
            // 3. Remove cart items
            // =========================

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

            return Ok();
        }
        catch (Exception)
        {          
            return Ok(); // luôn trả 200 để PayOS không retry lỗi
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
            .FirstOrDefaultAsync(x => x.OrderId == orderId);

        if (order == null)
            return NotFound();

        return Ok(new
        {
            paymentStatus = order.PaymentStatus
        });
    }
    }
