using System;
using System.Diagnostics;

namespace CIB.Exchange.Model
{
    public class AccountBalance
    {
        public decimal Bitcoin { get; set; }
        public decimal Ethereum { get; set; }
        public decimal Litecoin { get; set; }
        public decimal DigitalCash { get; set; }
        public decimal Euro { get; set; }
        public decimal Dollars { get; set; }
        public decimal BitcoinCash { get; set; }

        public decimal Get(string name)
        {
            switch (name.ToUpper())
            {
                case "BTC":
                    return Bitcoin;
                case "LTC":
                    return Litecoin;
                case "ETH":
                    return Ethereum;
                case "BCH":
                    return BitcoinCash;
                case "EUR":
                    return Euro;
                case "USD":
                    return Dollars;
                case "DSH":
                    return DigitalCash;
                default:
                    Debug.Fail("Unknown balance currency");
                    throw new NotImplementedException();
            }
        }
    }
}