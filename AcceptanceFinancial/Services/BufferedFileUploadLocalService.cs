using System.Globalization;
using CsvHelper;
using AcceptanceFinancial.Models;

namespace AcceptanceFinancial.Services; 


public class BufferedFileUploadLocalService : IBufferedFileUploadService
{
    private readonly ILogger<BufferedFileUploadLocalService> _logger;
    private readonly IMartenService _martenService;

    public BufferedFileUploadLocalService(ILogger<BufferedFileUploadLocalService> logger, IMartenService martenService) {
        _logger = logger;
        _martenService = martenService;
    }
    public async Task<bool> UploadFile(IFormFile file)
    {
        string path = "";
        try
        {
            if (file.Length > 0)
            {
               /* path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "UploadedFiles"));
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                using (var fileStream = new FileStream(Path.Combine(path, file.FileName), FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }*/
               using (var reader = new StreamReader(file.OpenReadStream()))
               using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
               {
                   var records = csv.GetRecords<DirectMail>().ToList();
                   await _martenService.CreateDirectMails(records);
               }
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            throw new Exception("File Copy Failed", ex);
        }
    }
}