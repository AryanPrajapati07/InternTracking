using QRCoder;
using System;

public static class QrHelper
{
    public static string GenerateQrCodeBase64(string url)
    {
        if (string.IsNullOrEmpty(url))
            return null;

        using var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeBytes = qrCode.GetGraphic(20);
        return Convert.ToBase64String(qrCodeBytes);
    }
}
