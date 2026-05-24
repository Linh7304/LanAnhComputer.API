using LanAnhComputer.Data;
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
        if (order.PaymentStatus == "PAID")
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
        order.PaymentStatus = "PENDING";

        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            order.OrderId,
            paymentLink.CheckoutUrl,
            paymentLink.QrCode
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
                .FirstOrDefaultAsync(x => x.PayOSOrderCode == orderCode);

            // ❗ KHÔNG return NotFound trong webhook
            if (order == null)
            {
                Console.WriteLine($"Order not found: {orderCode}");
                return Ok();
            }

            // 3. CHỐNG xử lý lại nhiều lần (idempotent)
            if (order.PaymentStatus == "PAID")
            {
                return Ok();
            }

            // 4. Update trạng thái
            order.PaymentStatus = "PAID";
            order.OrderStatus = "CONFIRMED";
            order.PaidAt = DateTime.UtcNow;
            order.TransactionId = data.Reference;

            await _dbContext.SaveChangesAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            // ❗ Quan trọng: webhook KHÔNG được fail
            Console.WriteLine("WEBHOOK ERROR: " + ex.Message);

            return Ok(); // luôn trả 200 để PayOS không retry lỗi
        }
    }

    [HttpPost("confirm-webhook")]
    public async Task<IActionResult> ConfirmWebhook()
    {
        var result = await _client.Webhooks.ConfirmAsync(
            "https://parking-trend-editor.ngrok-free.dev/api/payment/webhook"
        );

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