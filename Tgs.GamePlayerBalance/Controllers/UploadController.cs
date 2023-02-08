using System.Data;
using System.Text;
using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Tgs.GamePlayerBalance.Core.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Controllers;

[Route("[controller]")]
[ApiController]
public class UploadController : ControllerBase
{
    [HttpPost("{gameProviderCode}")]
    public ValueTask UploadAsync(
        [FromServices] ICommandService<ImportFormAddCommand> service,
        string gameProviderCode,
        IFormFile file)
    {
        if (file == null)
            throw new Exception("File is Not Received...");

        var dsexcelRecords = new DataSet();

        using (var st = file.OpenReadStream())
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var reader = file.FileName.EndsWith(".xls")
                ? ExcelReaderFactory.CreateBinaryReader(st)
                : file.FileName.EndsWith(".xlsx")
                ? ExcelReaderFactory.CreateOpenXmlReader(st)
                : ExcelReaderFactory.CreateCsvReader(st);

            dsexcelRecords = reader.AsDataSet();

            reader.Close();
        }

        var playerCodes = new List<string>();

        if (dsexcelRecords != null && dsexcelRecords.Tables.Count > 0)
        {
            DataTable dtStudentRecords = dsexcelRecords.Tables[0];
            for (int i = 0; i < dtStudentRecords.Rows.Count; i++)
            {
                var playerCode = Convert.ToString(dtStudentRecords.Rows[i][0]);
                if (!string.IsNullOrWhiteSpace(playerCode))
                    playerCodes.Add(playerCode.Trim());
            }
        }

        var command = new ImportFormAddCommand(
            GameProviderCode: gameProviderCode,
            Players: playerCodes);

        return service.ExecuteAsync(
            command: command,
            cancellationToken: HttpContext.RequestAborted);
    }
}
