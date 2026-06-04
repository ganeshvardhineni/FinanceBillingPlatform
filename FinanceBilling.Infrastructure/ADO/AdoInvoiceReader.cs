using Microsoft.Data.SqlClient;

namespace FinanceBilling.Infrastructure.ADO;

public class AdoInvoiceReader
{
    private readonly string _connectionString;

    public AdoInvoiceReader(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<List<InvoiceSummary>> GetInvoiceSummariesAsync()
    {
        var results = new List<InvoiceSummary>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
            SELECT
                i.InvoiceId,
                i.InvoiceNumber,
                i.TotalAmount,
                i.PaidAmount,
                (i.TotalAmount - i.PaidAmount) AS BalanceDue,
                i.Status,
                i.DueDate
            FROM Invoices i
            WHERE i.IsDeleted = 0
            ORDER BY i.DueDate ASC";

        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            results.Add(new InvoiceSummary
            {
                InvoiceId = reader.GetInt32(0),
                InvoiceNumber = reader.GetString(1),
                TotalAmount = reader.GetDecimal(2),
                PaidAmount = reader.GetDecimal(3),
                BalanceDue = reader.GetDecimal(4),
                Status = reader.GetByte(5),
                DueDate = reader.GetDateTime(6)
            });
        }

        return results;
    }
}

public class InvoiceSummary
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceDue { get; set; }
    public byte Status { get; set; }
    public DateTime DueDate { get; set; }
}