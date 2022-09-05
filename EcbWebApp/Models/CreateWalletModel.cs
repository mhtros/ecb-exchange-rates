using System.ComponentModel.DataAnnotations;

namespace EcbWebApp.Models;

public class CreateWalletModel
{
    public CreateWalletModel(string currencyCode, decimal balance)
    {
        CurrencyCode = currencyCode;
        Balance = balance;
    }

    [Required]
    [Display(Name = "Currency Code")]
    public string CurrencyCode { get; }

    [Required] [Display(Name = "Balance")] public decimal Balance { get; }
}