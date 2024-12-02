using Application.Interfaces;
using Azure.Core;
using Domain.CustomException;
using Domain.Entities;
using Domain.IRepository;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using Org.BouncyCastle.Asn1.Ocsp;

namespace Infrastructure.Email
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmail(string toEmail, string subject, string message)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration["Smtp:UserName"]));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;
            email.Body = new TextPart("plain") { Text = message };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_configuration["Smtp:Host"], int.Parse(_configuration["Smtp:Port"]), MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_configuration["Smtp:UserName"], _configuration["Smtp:Password"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        public async Task SendBookingInfo(Booking booking, double totalPrice = 0)
        {
            try
            {
                if(booking == null) throw new CustomException.DataNotFoundException("Booking is not found in server");

                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(_configuration["Smtp:UserName"]));
                email.To.Add(MailboxAddress.Parse(booking?.Account.Email));
                email.Subject = "Thông tin đặt bàn";
                email.Body = new TextPart("html") { Text = CreateBodyEmail(booking, totalPrice) };

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_configuration["Smtp:Host"], int.Parse(_configuration["Smtp:Port"]), MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_configuration["Smtp:UserName"], _configuration["Smtp:Password"]);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private string CreateBodyEmail(Booking? booking, double totalPrice)
        {
            string? barName = booking?.Bar?.BarName;
            string? address = booking?.Bar?.Address;
            booking.BookingDrinks = booking.BookingDrinks ?? new List<BookingDrink>();
            string htmlMessage = $"""
            <!DOCTYPE html>
            <html lang="vi">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>Booking BarBuddy</title>
            </head>
            <body style="font-family: Arial, sans-serif; background-color: #white; color: black; margin: 0; padding: 0;">
                <div style="width: 600px; margin: 20px auto; background-color: #white; padding: 20px; border-radius: 8px;">
                    <div style="text-align: center; margin-bottom: 20px; color: #B8860B;">
                        <h2 style="margin: 0; color: #B8860B;">{barName.ToUpper()}</h2>
                        <p style="margin: 5px 0; color: #B8860B;">{address}</p>
                        <h1 style="margin: 10px 0; color: #B8860B;">XÁC NHẬN ĐẶT BÀN THÀNH CÔNG</h1>
                        <p style="margin: 5px 0; color: #B8860B;">MÃ ĐẶT BÀN: <strong>{booking.BookingCode}</strong></p>
                    </div>
                    <div style="margin-bottom: 20px;">
                        <p style="margin: 5px 0; color: black;">BAR: {barName}</p>
                        <p style="margin: 5px 0; color: black;">ĐỊA CHỈ: {address}</p>
                        <p style="margin: 5px 0; color: black;">NGÀY ĐẶT: {booking.BookingDate.ToString("dd/MM/yyyy")}-{booking.BookingTime}</p>
                        <p style="margin: 5px 0; color: black;">SỐ BÀN: {booking.BookingTables.Count}</p>
                    </div>
                    <table style="width: 100%; border-collapse: collapse; margin-bottom: 20px; color: black;">
                        <thead>
                            <tr>
                                <th style="border: 1px solid #444; padding: 10px; background-color: #FCC434;">TÊN BÀN/ĐỒ UỐNG</th>
                                <th style="border: 1px solid #444; padding: 10px; background-color: #FCC434;">LOẠI BÀN/ĐỒ UỐNG</th>
                                <th style="border: 1px solid #444; padding: 10px; background-color: #FCC434;">ĐƠN GIÁ</th>
                                <th style="border: 1px solid #444; padding: 10px; background-color: #FCC434;">THÀNH TIỀN (VNĐ)</th>
                            </tr>
                        </thead>
                        <tbody>
                            {string.Join("", values: booking?.BookingTables?.Select(bt =>
                                {
                                    return @$"
                                <tr>
                                    <td style=""border: 1px solid #444; padding: 10px; text-align: center;"">{bt.Table?.TableName}</td>
                                    <td style=""border: 1px solid #444; padding: 10px; text-align: center;"">{bt.Table?.TableType?.TypeName}</td>
                                    <td style=""border: 1px solid #444; padding: 10px; text-align: center;"">0</td>
                                    <td style=""border: 1px solid #444; padding: 10px; text-align: center;"">0</td>
                                </tr>";
                                }
                            ))}

                            {string.Join("", values: booking?.BookingDrinks?.Select(bd =>
                                {
                                    return @$"
                                <tr>
                                    <td style=""border: 1px solid #444; padding: 10px; text-align: center;"">{bd?.Drink?.DrinkName}</td>
                                    <td style=""border: 1px solid #444; padding: 10px; text-align: center;"">{bd?.Drink?.DrinkCategory?.DrinksCategoryName}</td>
                                    <td style=""border: 1px solid #444; padding: 10px; text-align: center;"">{bd?.Drink?.Price}</td>
                                    <td style=""border: 1px solid #444; padding: 10px; text-align: center;"">{bd?.Drink?.Price}</td>
                                </tr>";
                                }
                            ))}
                        </tbody>
                    </table>
                    
                    <div style="text-align: center; margin: 30px 0;">
                        <div style="margin-bottom: 15px; color: #B8860B; font-weight: bold;">
                            MÃ QR ĐẶT BÀN
                        </div>
                        <div style="background-color: white; padding: 15px; display: inline-block; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
                            <img src="{booking.QRTicket}" 
                                 alt="QR Code" 
                                 style="width: 200px; height: 200px;" />
                        </div>
                        <div style="margin-top: 10px; font-size: 0.9em; color: #666;">
                            Vui lòng xuất trình mã QR này khi đến quán
                        </div>
                    </div>

                    <div style="text-align: right; font-weight: bold; margin-top: 10px; color: black;">
                        TỔNG TIỀN (VNĐ) - ĐÃ ÁP DỤNG CHIẾT KHẤU {booking.Bar.Discount}%: {totalPrice}
                    </div>
                    <div style="text-align: center; margin-top: 20px; font-size: 1em; color: #B8860B;">
                        Chúc Quý khách vui vẻ. Trao niềm tin nhận tài lộc !
                    </div>
                </div>
            </body>
            </html>
            """;

            return htmlMessage;
        }
    }
}
