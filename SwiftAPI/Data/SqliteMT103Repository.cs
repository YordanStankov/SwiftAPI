using Microsoft.Data.Sqlite;
using SwiftAPI.Models;
using System.Text;
using Microsoft.Extensions.Logging;
using SQLitePCL;

namespace SwiftAPI.Data
{
    public class SqliteMT103Repository : IMT103Repository
    {
        private readonly ISqliteConnectionFactory _factory;
        private readonly ILogger<SqliteMT103Repository> _logger;

        public SqliteMT103Repository(ISqliteConnectionFactory factory, ILogger<SqliteMT103Repository> logger)
        {
            _factory = factory;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            _logger.LogInformation("Initializing SQLite database and ensuring MT103 table exists.");
            await using var conn = _factory.CreateConnection();
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS MT103 (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    TransactionReferenceNumber TEXT,
    BankOperationCode TEXT,
    ValueDate TEXT,
    Currency TEXT,
    Amount REAL,
    OrderingCustomer TEXT,
    BeneficiaryCustomer TEXT,
    DetailsOfCharges TEXT,
    RawMessage TEXT,
    ReceivedAtUtc TEXT
);";
            await cmd.ExecuteNonQueryAsync();
            _logger.LogInformation("SQLite database initialized successfully.");
        }

        public async Task<long> AddAsync(MT103 mt103)
        {
            _logger.LogInformation("Adding new MT103 message with TransactionReferenceNumber: {TRN}", mt103.TransactionReferenceNumber);
            await using var conn = _factory.CreateConnection();
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO MT103
(TransactionReferenceNumber, BankOperationCode, ValueDate, Currency, Amount, OrderingCustomer, BeneficiaryCustomer, DetailsOfCharges, RawMessage, ReceivedAtUtc)
VALUES ($trn, $boc, $vd, $cur, $amt, $ord, $ben, $charges, $raw, $received);
SELECT last_insert_rowid();";

            cmd.Parameters.AddWithValue("$trn", mt103.TransactionReferenceNumber ?? string.Empty);
            cmd.Parameters.AddWithValue("$boc", mt103.BankOperationCode ?? string.Empty);
            cmd.Parameters.AddWithValue("$vd", mt103.ValueDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$cur", mt103.Currency ?? string.Empty);
            cmd.Parameters.AddWithValue("$amt", mt103.Amount);
            cmd.Parameters.AddWithValue("$ord", mt103.OrderingCustomer ?? string.Empty);
            cmd.Parameters.AddWithValue("$ben", mt103.BeneficiaryCustomer ?? string.Empty);
            cmd.Parameters.AddWithValue("$charges", mt103.DetailsOfCharges ?? string.Empty);
            cmd.Parameters.AddWithValue("$raw", mt103.RawMessage ?? string.Empty);
            cmd.Parameters.AddWithValue("$received", mt103.ReceivedAtUtc.ToString("o"));

            var result = await cmd.ExecuteScalarAsync();
            _logger.LogInformation("MT103 message added successfully with Id: {Id}", result);
            return Convert.ToInt64(result!);
        }

        public async Task<List<MT103>> GetAllAsync()
        {
            _logger.LogInformation("Retrieving all MT103 messages from the database.");
            var list = new List<MT103>();
            await using var conn = _factory.CreateConnection();
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, TransactionReferenceNumber, BankOperationCode, ValueDate, Currency, Amount, OrderingCustomer, BeneficiaryCustomer, DetailsOfCharges, RawMessage, ReceivedAtUtc FROM MT103 ORDER BY ReceivedAtUtc DESC";
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(Map(reader));
            }
            _logger.LogInformation("Retrieved {Count} MT103 messages from the database.", list.Count);
            return list;
        }

        public async Task<MT103?> GetByIdAsync(long id)
        {
            _logger.LogInformation("Retrieving MT103 message by Id: {Id}", id);
            await using var conn = _factory.CreateConnection();
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, TransactionReferenceNumber, BankOperationCode, ValueDate, Currency, Amount, OrderingCustomer, BeneficiaryCustomer, DetailsOfCharges, RawMessage, ReceivedAtUtc FROM MT103 WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", id);
            await using var reader = await cmd.ExecuteReaderAsync();
            _logger.LogInformation("MT103 message retrieval by Id: {Id} completed.", id);
            if (await reader.ReadAsync())
                return Map(reader);
            return null;
        }

        private static MT103 Map(SqliteDataReader reader)
        {
            var id = reader.GetInt64(Constants.IdColumn);
            string trn = reader.IsDBNull(Constants.TransactionReferenceNumberColumn) ? string.Empty : reader.GetString(Constants.TransactionReferenceNumberColumn);
            string boc = reader.IsDBNull(Constants.BankOperationCodeColumn) ? string.Empty : reader.GetString(Constants.BankOperationCodeColumn);
            string vd = reader.IsDBNull(Constants.ValueDateColumn) ? string.Empty : reader.GetString(Constants.ValueDateColumn);
            string cur = reader.IsDBNull(Constants.CurrencyColumn) ? string.Empty : reader.GetString(Constants.CurrencyColumn);
            double amt = reader.IsDBNull(Constants.AmountColumn) ? 0 : reader.GetDouble(Constants.AmountColumn);
            string ord = reader.IsDBNull(Constants.OrderingCustomerColumn) ? string.Empty : reader.GetString(Constants.OrderingCustomerColumn);
            string ben = reader.IsDBNull(Constants.BeneficiaryCustomerColumn) ? string.Empty : reader.GetString(Constants.BeneficiaryCustomerColumn);
            string charges = reader.IsDBNull(Constants.DetailsOfChargesColumn) ? string.Empty : reader.GetString(Constants.DetailsOfChargesColumn);
            string raw = reader.IsDBNull(Constants.RawMessageColumn) ? string.Empty : reader.GetString(Constants.RawMessageColumn);
            string received = reader.IsDBNull(Constants.RecievedAtUtcColumn) ? string.Empty : reader.GetString(Constants.RecievedAtUtcColumn);

            return new MT103
            {
                Id = id,
                TransactionReferenceNumber = trn,
                BankOperationCode = boc,
                ValueDate = string.IsNullOrEmpty(vd) ? DateOnly.MinValue : DateOnly.Parse(vd),
                Currency = cur,
                Amount = amt,
                OrderingCustomer = ord,
                BeneficiaryCustomer = ben,
                DetailsOfCharges = charges,
                RawMessage = raw,
                ReceivedAtUtc = string.IsNullOrEmpty(received) ? DateTime.MinValue : DateTime.Parse(received)
            };
        }
    }
}
