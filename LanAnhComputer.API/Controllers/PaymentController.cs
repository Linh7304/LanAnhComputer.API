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

    public PaymentController(
        PayOSClient client,
        AppDbContext dbContext)
    {
        _client = client;
        _dbContext = dbContext;
    }

    [HttpPost("create/{orderId}")]
    public async Task<IActionResult> CreatePayment(long orderId)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(x => x.OrderId == orderId);

        if (order == null)     return NotFound();

        // tạo orderCode riêng cho PayOS
        var payOSOrderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var paymentRequest = new CreatePaymentLinkRequest
        {
            OrderCode = payOSOrderCode,

            Amount = (int)order.TotalAmount,

            Description = $"DH{order.OrderId}",

            ReturnUrl = "https://localhost:7146/checkout/success",

            CancelUrl =  "https://localhost:7146/checkout/cancel"
        };

        var paymentLink = await _client.PaymentRequests.CreateAsync(paymentRequest);

        // lưu DB
        order.PayOSOrderCode = payOSOrderCode;

        order.PaymentLinkId = paymentLink.PaymentLinkId;

        order.CheckoutUrl = paymentLink.CheckoutUrl;

        order.PaymentStatus = "PENDING";

        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            OrderId = order.OrderId,
            CheckoutUrl = paymentLink.CheckoutUrl,
            QrCode = paymentLink.QrCode,
            OrderCode = paymentLink.OrderCode,
            Amount = paymentLink.Amount
        });
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook(
        [FromBody] Webhook webhook)
    {
        var data = await _client.Webhooks.VerifyAsync(webhook);

        var orderCode = data.OrderCode;

        var order = await _dbContext.Orders.FirstOrDefaultAsync(x =>x.PayOSOrderCode == orderCode);

        if (order == null) return NotFound();

        order.PaymentStatus = "PAID";

        order.OrderStatus = "CONFIRMED";

        order.PaidAt = DateTime.UtcNow;

        order.TransactionId = data.Reference;

        await _dbContext.SaveChangesAsync();

        return Ok();
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