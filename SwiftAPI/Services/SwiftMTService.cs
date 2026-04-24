using Microsoft.AspNetCore.Mvc;
using SwiftAPI.Models;
using System.Text;
using SwiftAPI.Data;
using Microsoft.Extensions.Logging;

namespace SwiftAPI.Services
{
    public class SwiftMTService : ISwiftMTService
    {
        private readonly IMT103Repository _repository;
        private readonly ILogger<SwiftMTService> _logger;

        public SwiftMTService(IMT103Repository repository, ILogger<SwiftMTService> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        private List<string> DivideMessages(string data)
        {
            _logger.LogInformation("Dividing messages from data");
            List<string> messages = new List<string>();
            int i = 0;
            while (true)
            {
                if (i >= data.Length)
                {
                    break;
                }
                int startIndex = data.IndexOf("{1:");
                int endIndex = data.IndexOf("}{1:") + 1;
                if (startIndex != -1)
                {
                    if (endIndex == 0)
                        endIndex = data.Length;
                    string message = data.Substring(startIndex, endIndex - startIndex);
                    data = data.Remove(startIndex, endIndex - startIndex);
                    messages.Add(message);
                    i = endIndex;
                    _logger.LogInformation("Message extracted: {message}", message);
                }
                else
                    break;
            }
            return messages;
        }
        private List<string> ExtractTransactions(List<string> messages)
        {
            _logger.LogInformation("Extracting transactions from messages");
            List<string> transactions = new List<string>();
            foreach (string mssg in messages)
            {
                if (mssg.Contains(Constants.TransactionReferenceNumberTag))
                {
                    transactions.Add(mssg);
                    _logger.LogInformation("Transaction extracted: {transaction}", mssg);
                }

            }
            return transactions;
        }
       
        private List<MT103> ParseToMT103(List<string> transactions)
        {
            _logger.LogInformation("Parsing transactions to MT103 objects");
            string TransactionReferenceNumber = string.Empty;
            string BankOperationCode = string.Empty;
            string DateCurrencyAmount = string.Empty;
            string BeneficieryCustomer = string.Empty;
            string DetailsOfCharges = string.Empty;
            StringBuilder OrderingCustomer = new StringBuilder();

            List<MT103> mT103s = new List<MT103>();
            foreach (string transaction in transactions)
            {
                string[] lines = transaction.Split("\r\n");
                foreach (string line in lines)
                {
                    if (line.StartsWith(Constants.TransactionReferenceNumberTag))
                        TransactionReferenceNumber = line
                            .Substring(Constants.TransactionReferenceNumberTag.Length);

                    else if (line.StartsWith(Constants.BankOperationCodeTag))
                        BankOperationCode = line
                            .Substring(Constants.BankOperationCodeTag.Length);  

                    else if (line.StartsWith(Constants.DateCurrencyAmountTag))
                        DateCurrencyAmount = line
                            .Substring(Constants.DateCurrencyAmountTag.Length);

                    else if (line.StartsWith(Constants.OrderingCustomerTag))
                    {
                        int index = lines.IndexOf(line);
                        OrderingCustomer
                            .Append($"{line.Substring(Constants.OrderingCustomerTag.Length + 1)} " +
                            $"| {lines[index + 1]} " +
                            $"| {lines[index + 2]} " +
                            $"| {lines[index + 3]}");
                    }

                    else if (line.StartsWith(Constants.BeneficiaryCustomerTag))
                        BeneficieryCustomer = line
                            .Substring(Constants.BeneficiaryCustomerTag.Length + 1);

                    else if (line.StartsWith(Constants.DetailsOfChargesTag))
                        DetailsOfCharges = line
                            .Substring(Constants.DetailsOfChargesTag.Length);
                }

                mT103s.Add(new MT103
                {
                    TransactionReferenceNumber = TransactionReferenceNumber,
                    BankOperationCode = BankOperationCode,
                    ValueDate = DateOnly.ParseExact(DateCurrencyAmount.Substring(0, 6), "yyMMdd"),
                    Currency = DateCurrencyAmount.Substring(6, 3),
                    Amount = double.Parse(DateCurrencyAmount.Substring(9)),
                    OrderingCustomer = OrderingCustomer.ToString(),
                    BeneficiaryCustomer = BeneficieryCustomer,
                    DetailsOfCharges = DetailsOfCharges,
                    RawMessage = transaction,
                    ReceivedAtUtc = DateTime.UtcNow
                });
                _logger.LogInformation("MT103 object created: {mt103}", mT103s.Last());
            }
            return mT103s;
        }


        public async Task<IActionResult> ReadMT103Async(IFormFile file)
        {
            _logger.LogInformation("Reading MT103 messages from uploaded file: {fileName}", file.FileName);
            Stream fileStream = file.OpenReadStream();
            List<string> messages = new List<string>();
            using (var reader = new StreamReader(fileStream))
            {
                string data = await reader.ReadToEndAsync();
                messages = DivideMessages(data);
            }
            List<string> transactions = ExtractTransactions(messages);
            List<MT103> mT103s = ParseToMT103(transactions);

            foreach (var m in mT103s)
            {
                await _repository.AddAsync(m);
                _logger.LogInformation("MT103 object added to repository: {mt103}", m);
            }

            return new OkObjectResult(new { Inserted = mT103s.Count });
        }

        public async Task<MT103?> GetByIDAsync(long id)
        {
            _logger.LogInformation("Retrieving MT103 object by ID: {id}", id);
            return await _repository.GetByIdAsync(id);
        }

        public async Task<List<MT103>> GetAllAsync()
        {
            _logger.LogInformation("Retrieving all MT103 objects from repository");
            return await _repository.GetAllAsync();
        }
    }
}
