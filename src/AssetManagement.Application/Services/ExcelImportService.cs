using System.ComponentModel.DataAnnotations;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums.Assets;
using ClosedXML.Excel;

namespace AssetManagement.Application.Services;

public record ImportRowError(int Row, string Message);

public record ImportParseResult<T>(List<(int Row, T Entity)> Valid, List<ImportRowError> Errors, int TotalRows);

public class ExcelImportService
{
    private static readonly string[] AssetHeaders =
        { "assetname", "serialnumber", "type", "purchasedate", "warrantyexpirydate", "condition", "status" };

    public ImportParseResult<Asset> ParseAssets(Stream stream, string fileName)
        => Parse(stream, fileName, AssetHeaders, MapAsset);

    private static ImportParseResult<T> Parse<T>(
        Stream stream, string fileName, string[] required,
        Func<IReadOnlyDictionary<string, string>, List<string>, T?> map)
    {
        var (headers, rows) = ReadRows(stream, fileName);

        var missing = required.Where(h => !headers.Contains(h)).ToList();
        if (missing.Count > 0)
            return new ImportParseResult<T>(new(), new()
            {
                new ImportRowError(1, $"Missing required column(s): {string.Join(", ", missing)}. Expected: {string.Join(", ", required)}.")
            }, 0);

        var validRows = new List<(int Row, T Entity)>();
        var errors = new List<ImportRowError>();
        foreach (var (rowNumber, cells) in rows)
        {
            var rowErrors = new List<string>();
            var entity = map(cells, rowErrors);
            if (entity is not null && rowErrors.Count == 0)
            {
                validRows.Add((rowNumber, entity));
            }
            else
            {
                errors.Add(new ImportRowError(rowNumber, string.Join("; ", rowErrors)));
            }
        }

        if (typeof(T) == typeof(Asset))
        {
            var assetRows = validRows.Cast<(int, Asset)>().ToList();
            var duplicateRows = DetectAssetDuplicates(assetRows, errors);
            validRows.RemoveAll(r => duplicateRows.Contains(r.Row));
        }
        return new ImportParseResult<T>(validRows, errors, rows.Count);
    }

    private static (HashSet<string> Headers, List<(int Row, Dictionary<string, string> Cells)> Rows) ReadRows(Stream stream, string fileName)
    {
        if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Only .xlsx files are supported.");

        var buffer = new byte[4];
        if (stream.Read(buffer, 0, 4) < 4 || buffer[0] != 0x50 || buffer[1] != 0x4B || buffer[2] != 0x03 || buffer[3] != 0x04)
        {
            throw new ArgumentException("Invalid file format. The file is not a valid Excel file.");
        }
        stream.Position = 0;

        return ReadXlsx(stream);
    }

    private static (HashSet<string>, List<(int, Dictionary<string, string>)>) ReadXlsx(Stream stream)
    {
        using var workbook = new XLWorkbook(stream);
        var sheet = workbook.Worksheets.First();
        var headerRow = sheet.FirstRowUsed();
        if (headerRow is null) return (new(), new());

        var columns = headerRow.CellsUsed()
            .Select(cell => (Col: cell.Address.ColumnNumber, Name: cell.GetString().Trim().ToLowerInvariant()))
            .Where(x => x.Name.Length > 0)
            .ToList();

        var headers = columns.Select(x => x.Name).ToHashSet();
        var rows = new List<(int, Dictionary<string, string>)>();
        var lastRow = sheet.LastRowUsed()!.RowNumber();
        for (var r = headerRow.RowNumber() + 1; r <= lastRow; r++)
        {
            var row = sheet.Row(r);
            if (row.IsEmpty()) continue;
            var cells = columns.ToDictionary(x => x.Name, x => sheet.Cell(r, x.Col).GetString().Trim());
            rows.Add((r, cells));
        }
        return (headers, rows);
    }

    private static Asset? MapAsset(IReadOnlyDictionary<string, string> c, List<string> errors)
    {
        var asset = new Asset
        {
            AssetName = c.GetValueOrDefault("assetname", ""),
            SerialNumber = c.GetValueOrDefault("serialnumber", ""),
        };
        if (TryEnum<AssetType>(c.GetValueOrDefault("type", ""), out var type)) asset.Type = type;
        else errors.Add($"Invalid Type (allowed: {AllowedValues<AssetType>()})");

        if (TryEnum<AssetCondition>(c.GetValueOrDefault("condition", ""), out var cond)) asset.Condition = cond;
        else errors.Add($"Invalid Condition (allowed: {AllowedValues<AssetCondition>()})");

        if (TryEnum<AssetStatus>(c.GetValueOrDefault("status", ""), out var stat)) asset.Status = stat;
        else errors.Add($"Invalid Status (allowed: {AllowedValues<AssetStatus>()})");

        if (TryDate(c.GetValueOrDefault("purchasedate", ""), out var purchase)) asset.PurchaseDate = purchase;
        else errors.Add("Invalid PurchaseDate (use yyyy-MM-dd)");

        if (TryDate(c.GetValueOrDefault("warrantyexpirydate", ""), out var warranty)) asset.WarrantyExpiryDate = warranty;
        else errors.Add("Invalid WarrantyExpiryDate (use yyyy-MM-dd)");

        Validate(asset, errors);
        return asset;
    }

    private static void Validate(object entity, List<string> errors)
    {
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(entity, new ValidationContext(entity), results, validateAllProperties: true))
            errors.AddRange(results.Select(r => r.ErrorMessage ?? "Invalid value"));
    }

    private static bool TryEnum<T>(string value, out T result) where T : struct, Enum
        => Enum.TryParse(value.Replace(" ", "").Trim(), ignoreCase: true, out result) && Enum.IsDefined(result);

    private static bool TryDate(string value, out DateOnly result)
        => DateOnly.TryParse(value.Trim(), out result);

    private static string AllowedValues<T>() where T : struct, Enum
        => string.Join(", ", Enum.GetNames<T>());

    private static HashSet<int> DetectAssetDuplicates(List<(int Row, Asset Asset)> rows, List<ImportRowError> errors)
    {
        var duplicateRows = new HashSet<int>();

        var serialGroups = rows
            .GroupBy(r => r.Asset.SerialNumber.Trim().ToLowerInvariant())
            .Where(g => !string.IsNullOrWhiteSpace(g.Key))
            .Where(g => g.Count() > 1);

        foreach (var group in serialGroups)
        {
            foreach (var row in group)
            {
                duplicateRows.Add(row.Row);

                errors.Add(new ImportRowError(
                    row.Row,
                    $"Duplicate Serial Number '{row.Asset.SerialNumber}' found in uploaded file."));
            }
        }

        return duplicateRows;
    }
}
