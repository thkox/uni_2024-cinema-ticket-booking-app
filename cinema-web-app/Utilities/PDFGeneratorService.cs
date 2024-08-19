using cinema_web_app.Models;
using DinkToPdf;
using DinkToPdf.Contracts;

namespace cinema_web_app.Utilities;

public class PDFGeneratorService
{
    private readonly IConverter _converter;

    public PDFGeneratorService(IConverter converter)
    {
        _converter = converter;
    }

    public async Task<byte[]> GenerateReservationPdfAsync(Reservation reservation)
    {
        // Download the images
        var movieImagePath = await DownloadImageAsync(reservation.Screening.Movie.ImageUrl);
        var qrCodeUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=150x150&data={reservation.ShortReferenceId}";
        var qrCodePath = await DownloadImageAsync(qrCodeUrl);

        var htmlContent = GenerateHtmlContent(reservation, movieImagePath, qrCodePath);

        var doc = new HtmlToPdfDocument
        {
            GlobalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4
            },
            Objects =
            {
                new ObjectSettings
                {
                    HtmlContent = htmlContent,
                    WebSettings = { DefaultEncoding = "utf-8" },
                    LoadSettings = { BlockLocalFileAccess = false }
                }
            }
        };

        var pdfBytes = _converter.Convert(doc);

        // Clean up temporary files
        File.Delete(movieImagePath);
        File.Delete(qrCodePath);

        return pdfBytes;
    }

    private async Task<string> DownloadImageAsync(string imageUrl)
    {
        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(imageUrl);
        response.EnsureSuccessStatusCode();

        var fileName = Path.GetTempFileName(); // Create a temp file
        var filePath = Path.ChangeExtension(fileName, ".jpg"); // Change extension based on image format

        await using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            await response.Content.CopyToAsync(fileStream);
        }

        return filePath;
    }

    private string GenerateHtmlContent(Reservation reservation, string movieImagePath, string qrCodePath)
    {
        var html = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; color: #333; margin: 0; padding: 0; background-color: #f7f9fc; }}
                    .container {{ width: 80%; margin: 0 auto; padding: 20px; background-color: #fff; border-radius: 10px; box-shadow: 0 4px 8px rgba(0,0,0,0.1); }}
                    .header {{ text-align: center; padding-bottom: 20px; border-bottom: 2px solid #f1f1f1; margin-bottom: 20px; }}
                    .header h1 {{ font-size: 24px; color: #333; margin: 0; }}
                    .movie-image {{ text-align: center; margin: 20px 0; }}
                    .movie-image img {{ max-width: 200px; border-radius: 10px; }}
                    .details {{ font-size: 16px; line-height: 1.6; }}
                    .details dl {{ margin: 0; padding: 0; }}
                    .details dt {{ font-weight: bold; color: #555; }}
                    .details dd {{ margin: 0 0 10px 0; }}
                    .qr-code {{ text-align: center; margin-top: 30px; }}
                    .qr-code img {{ width: 150px; height: 150px; border: 2px solid #333; border-radius: 5px; }}
                    .footer {{ text-align: center; margin-top: 40px; color: #777; font-size: 14px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Reservation Ticket</h1>
                    </div>
                    <div class='movie-image'>
                        <img src='{movieImagePath}' alt='Movie Image' />
                    </div>
                    <div class='details'>
                        <dl>
                            <dt>Movie Title:</dt>
                            <dd>{reservation.Screening.Movie.Title}</dd>

                            <dt>Description:</dt>
                            <dd>{reservation.Screening.Movie.Description}</dd>

                            <dt>Screening Date and Time:</dt>
                            <dd>{reservation.Screening.StartTime:yyyy-MM-dd HH:mm}</dd>

                            <dt>Cinema Name:</dt>
                            <dd>{reservation.Screening.ScreeningRoom.Cinema.Name}</dd>

                            <dt>Cinema Address:</dt>
                            <dd>{reservation.Screening.ScreeningRoom.Cinema.Address}</dd>

                            <dt>Screening Room:</dt>
                            <dd>{reservation.Screening.ScreeningRoom.Name}</dd>

                            <dt>Number of Booked Seats:</dt>
                            <dd>{reservation.NoOfBookedSeats}</dd>

                            <dt>Reservation ID:</dt>
                            <dd>{reservation.ShortReferenceId}</dd>
                        </dl>
                    </div>
                    <div class='qr-code'>
                        <img src='{qrCodePath}' alt='QR Code' />
                    </div>
                    <div class='footer'>
                        <p>Thank you for choosing our cinema. Enjoy your movie!</p>
                    </div>
                </div>
            </body>
            </html>";

        return html;
    }
}