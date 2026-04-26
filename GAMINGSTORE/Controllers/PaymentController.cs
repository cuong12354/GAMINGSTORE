using GAMINGSTORE.Data;
using GAMINGSTORE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GAMINGSTORE.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PaymentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Generate QR code for bank transfer payment
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> BankTransferQR(int orderId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var order = await _context.Orders.FindAsync(orderId);
            if (order == null || order.UserId != user.Id)
                return NotFound();

            // Bank transfer details (template)
            // In production, replace with actual bank account details
            string bankCode = "TECHBANK";
            string accountNumber = "1234567890";
            string accountName = "GAMINGSTORE CO., LTD";
            string amount = order.TotalPrice.ToString("F0");
            string description = $"DN{orderId}";

            // Format: bankCode|accountNumber|amount|accountName|description
            string qrContent = $"00020126360006970010A00000067E0108TECHBANK0123456789012501520400005303156540510.005802VN59080800000A00000067E0107D00000067E0114{description}62320618{accountNumber}63041A6D";

            // Simple format for Vietnamese bank QR
            qrContent = $"Bank Transfer\nAccount: {accountNumber}\nName: {accountName}\nAmount: {amount}₫\nDescription: {description}";

            try
            {
                // Generate QR code using QRCoder
                using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                {
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);

                    using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
                    {
                        byte[] qrImageBytes = qrCode.GetGraphic(20);
                        string qrBase64 = Convert.ToBase64String(qrImageBytes);

                        ViewBag.QRCodeImage = $"data:image/png;base64,{qrBase64}";
                        ViewBag.OrderId = orderId;
                        ViewBag.Amount = order.TotalPrice.ToString("N0");
                        ViewBag.AccountNumber = accountNumber;
                        ViewBag.AccountName = accountName;
                        ViewBag.BankCode = bankCode;
                        ViewBag.Description = description;

                        return View(order);
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi sinh mã QR: {ex.Message}");
                return View("Error");
            }
        }

        /// <summary>
        /// Confirm bank transfer payment (usually done manually in production)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ConfirmBankTransfer(int orderId, string transactionId = "")
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var order = await _context.Orders.FindAsync(orderId);
            if (order == null || order.UserId != user.Id)
                return NotFound();

            // Update order status and payment method
            order.PaymentMethod = "Bank Transfer";
            order.Status = "Pending";  // Will need admin approval for bank transfers
            order.Notes = string.IsNullOrEmpty(transactionId) 
                ? "Chờ xác nhận chuyển khoản" 
                : $"Transaction ID: {transactionId}";

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "OrderHistory", new { id = orderId });
        }

        /// <summary>
        /// Generate payment slip/invoice in text format
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> PaymentSlip(int orderId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == user.Id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        /// <summary>
        /// Validation helper to check bank account format
        /// </summary>
        private bool IsValidBankAccount(string accountNumber)
        {
            return !string.IsNullOrEmpty(accountNumber) && 
                   accountNumber.All(char.IsDigit) && 
                   accountNumber.Length >= 8 && 
                   accountNumber.Length <= 20;
        }
    }
}
