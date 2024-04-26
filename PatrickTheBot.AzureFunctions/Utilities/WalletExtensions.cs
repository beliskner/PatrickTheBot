namespace PatrickTheBot.AzureFunctions.Utilities;

public static class WalletExtensions
{
    private static readonly Dictionary<string, int> AlternativeCurrencies = new()
    {
        { "Backend Millie", 1000 },
        { "Backend Barki", 100 },
        { "Backend Bankoe", 50 },
        { "Backend Jackson", 20 },
        { "Backend Joetje", 10 },
        { "Backend Fiver", 5 },
        { "Backend Deuce", 2 },
        { "Backend Buck", 1 }
    };

    public static WalletTableEntity ToTable(this Wallet wallet)
    {
        return new WalletTableEntity
        {
            PartitionKey = "WALLET",
            RowKey = wallet.HolderId,
            WalletHolder = wallet.HolderName,
            BackendBucks = wallet.BackendBucks,
            BackendPoints = wallet.BackendPoints,
            FrontendPoints = wallet.FrontendPoints
        };
    }

    public static Wallet ToWallet(this WalletTableEntity walletTableEntity)
    {
        return new Wallet
        {
            HolderId = walletTableEntity.RowKey,
            HolderName = walletTableEntity.WalletHolder,
            BackendBucks = walletTableEntity.BackendBucks,
            BackendPoints = walletTableEntity.BackendPoints,
            FrontendPoints = walletTableEntity.FrontendPoints
        };
    }

    public static string PrintAlternativeCurrencies(this WalletTableEntity walletTableEntity)
    {
        var remainder = walletTableEntity.BackendBucks;
        if (remainder < 2) return string.Empty;
        var altCurrencyDictionary = new Dictionary<string, int>();

        foreach (var currency in AlternativeCurrencies)
            (remainder, altCurrencyDictionary) = altCurrencyDictionary.TryAddAlternativeCurrencyAndReturnRemainder(currency.Key, currency.Value, remainder);

        if (!altCurrencyDictionary.Any()) return string.Empty;
        
        // Not sure if this looks better syntactically or not
        // if (altCurrencyDictionary.Count > 1)
            // altCurrencyDictionary = altCurrencyDictionary.OrderBy(di => di.Value).Reverse().ToDictionary(x => x.Key, x=> x.Value);
        
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("which would be equivalent to");
        foreach (var item in altCurrencyDictionary)
            stringBuilder.Append($"{(item.Key.Equals(altCurrencyDictionary.FirstOrDefault().Key) ? string.Empty : item.Key.Equals(altCurrencyDictionary.LastOrDefault().Key) ? " and" : ",")} {item.Value} {item.Key}");
        
        return stringBuilder.ToString();
    }

    private static (int, Dictionary<string, int>) TryAddAlternativeCurrencyAndReturnRemainder(this Dictionary<string, int> altDictionary, string currencyName, int currencyValue, int remainder)
    {
        var amount = remainder / currencyValue;
        if (amount <= 0) return (remainder, altDictionary);
        altDictionary.Add(amount is 1 ? currencyName : $"{currencyName}s", amount);

        return (remainder % currencyValue, altDictionary);
    }
}