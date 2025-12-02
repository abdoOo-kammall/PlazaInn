using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.DTO.Hotel;

namespace Shared.EmailSignture
{
    public static class EmailSign
    {
        public static string BuildHotelSignatureHtml(HotelDto hotel, string? logoUrl = null)
        {
            var name = System.Net.WebUtility.HtmlEncode(hotel.Name ?? "");
            var address = System.Net.WebUtility.HtmlEncode(hotel.Address ?? "");
            var phone = System.Net.WebUtility.HtmlEncode(hotel.Phone ?? "");
            var email = System.Net.WebUtility.HtmlEncode(hotel.Email ?? "");
            var city = System.Net.WebUtility.HtmlEncode(hotel.City ?? "");
            var instagram = hotel.Instagram;
            var facebook = hotel.Facebook;
            var whatsapp = hotel.WhatsApp;
            var space = hotel.Space;

            var svgInstagram = @"<svg width='18' height='18' viewBox='0 0 24 24' fill='none' xmlns='http://www.w3.org/2000/svg' role='img' aria-label='instagram'><path d='M7 2h10a5 5 0 0 1 5 5v10a5 5 0 0 1-5 5H7a5 5 0 0 1-5-5V7a5 5 0 0 1 5-5z' stroke='#666' stroke-width='1.2' stroke-linecap='round' stroke-linejoin='round'></path><path d='M12 8.5a3.5 3.5 0 1 0 0 7 3.5 3.5 0 0 0 0-7z' stroke='#666' stroke-width='1.2' stroke-linecap='round' stroke-linejoin='round'></path><path d='M17.8 6.2h.01' stroke='#666' stroke-width='1.4' stroke-linecap='round' stroke-linejoin='round'/></svg>";
            var svgFacebook = @"<svg width='18' height='18' viewBox='0 0 24 24' fill='none' xmlns='http://www.w3.org/2000/svg' role='img' aria-label='facebook'><path d='M18 2h-3a4 4 0 0 0-4 4v3H8v4h3v7h4v-7h3l1-4h-4V6a1 1 0 0 1 1-1h3V2z' stroke='#666' stroke-width='1.2' stroke-linecap='round' stroke-linejoin='round'/></svg>";
            var svgWhatsApp = @"<svg width='18' height='18' viewBox='0 0 24 24' fill='none' xmlns='http://www.w3.org/2000/svg' role='img' aria-label='whatsapp'><path d='M21 11.5A9.5 9.5 0 1 0 6 20l-1.6 4 4.2-1.1A9.5 9.5 0 0 0 21 11.5z' stroke='#666' stroke-width='1.2' stroke-linecap='round' stroke-linejoin='round'/><path d='M15.5 14.2c-.3 0-1.5-.7-1.7-.8-.2-.1-.4-.1-.6.1s-.7.8-.9 1c-.2.2-.4.2-.8.1-.4-.1-1.6-.6-2.7-1.9-1-1.2-1.6-2.7-1.8-3.1-.2-.4 0-.6.1-.8.1-.2.2-.4.4-.6.2-.1.3-.2.5-.3.2-.1.4-.1.6-.1.2 0 .4 0 .6 0 .2 0 .5-.1.8.6.3.7 1 2.2 1 2.5 0 .3-.1.4-.1.6 0 .1-.2.4-.3.7-.1.3-.2.5-.1.7.1.2.7.9 1.7 1.6 1 0 1.6.1 1.9.1.3 0 1-.4 1.2-.9.2-.5.2-.9.1-1-.2-.1-.6-.1-1.2-.1z' stroke='#666' stroke-width='0.8' stroke-linecap='round' stroke-linejoin='round'/></svg>";

            var logoImgHtml = "";
            if (!string.IsNullOrEmpty(logoUrl))
            {
                logoImgHtml = $"<td style='vertical-align:top;padding-right:12px;'><img src='{System.Net.WebUtility.HtmlEncode(logoUrl)}' alt='{name} logo' width='100' style='display:block;border-radius:6px;max-width:120px;height:auto;'></td>";
            }

            var html = $@"
<table cellpadding='0' cellspacing='0' style='font-family: Arial, Helvetica, sans-serif; font-size:14px; color:#222;'>
  <tr>
    <td>
      <table cellpadding='0' cellspacing='0' style='border-collapse:collapse;'>
        <tr>
          {logoImgHtml}
          <td style='vertical-align:top; padding-left:6px;'>
            <div style='font-size:16px; font-weight:700; color:#1f2937; margin-bottom:6px;'>{name}</div>
            <div style='font-size:13px; color:#4b5563; margin-bottom:6px;'>{city} {(!string.IsNullOrEmpty(address) ? " | " + address : "")}</div>
            <div style='font-size:13px; color:#4b5563; margin-bottom:6px;'>Phone: <a href='tel:{phone}' style='color:#2563eb; text-decoration:none;'>{phone}</a></div>
            <div style='font-size:13px; color:#4b5563; margin-bottom:8px;'>Email: <a href='mailto:{email}' style='color:#2563eb; text-decoration:none;'>{email}</a></div>

            <div style='margin-top:8px;'>
              <!-- social icons -->
              <table cellpadding='0' cellspacing='0' style='border-collapse:collapse;'>
                <tr>
                  {(string.IsNullOrEmpty(instagram) ? "" : $"<td style='padding-right:8px;'><a href='{System.Net.WebUtility.HtmlEncode(instagram)}' target='_blank' style='text-decoration:none;'>{svgInstagram}</a></td>")}
                  {(string.IsNullOrEmpty(facebook) ? "" : $"<td style='padding-right:8px;'><a href='{System.Net.WebUtility.HtmlEncode(facebook)}' target='_blank' style='text-decoration:none;'>{svgFacebook}</a></td>")}
                  {(string.IsNullOrEmpty(whatsapp) ? "" : $"<td style='padding-right:8px;'><a href='{System.Net.WebUtility.HtmlEncode(whatsapp)}' target='_blank' style='text-decoration:none;'>{svgWhatsApp}</a></td>")}
                  {(string.IsNullOrEmpty(space) ? "" : $"<td style='padding-right:8px;'><a href='{System.Net.WebUtility.HtmlEncode(space)}' target='_blank' style='text-decoration:none; color:#666; font-size:13px;'>{System.Net.WebUtility.HtmlEncode(space)}</a></td>")}
                </tr>
              </table>
            </div>

          </td>
        </tr>
      </table>

      <div style='margin-top:12px; font-size:12px; color:#6b7280;'>
        <em>Note: This message comes from {name}. If you prefer not to receive these emails, please reply with 'Unsubscribe'.</em>
      </div>
    </td>
  </tr>
</table>
";

            return html;
        }

    }
}
