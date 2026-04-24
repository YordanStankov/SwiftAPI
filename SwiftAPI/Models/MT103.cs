namespace SwiftAPI.Models
{
    public class MT103
    {
        public long Id { get; init; }
        //Basic Information
        public string TransactionReferenceNumber { get; init; }
        public string BankOperationCode { get; init; }
        //Value Date, Currency Code, Amount
        public DateOnly ValueDate { get; init; }
        public string Currency { get; init; }
        public double Amount { get; init; }
        //Parties
        public string OrderingCustomer { get; init; }
        public string BeneficiaryCustomer { get; init; }
        //Charges
        public string DetailsOfCharges { get; init; }

        public string RawMessage { get; init; }
        public DateTime ReceivedAtUtc { get; init; }


    }
}
