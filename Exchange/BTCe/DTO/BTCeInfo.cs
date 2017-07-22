using System.Collections.Generic;

namespace CIB.Exchange.BTCe.DTO
{
    internal class BTCeInfo
    {
        public Dictionary<string, decimal> Funds { get; set; }
        public Dictionary<string, decimal> Rights { get; set; }
        public int TransactionCount { get; set; }
        public int OpenOrders { get; set; }
        public int ServerTime { get; set; }

        //private UserInfo() { }
        //public static UserInfo ReadFromJObject(JObject o)
        //{
        //    return new UserInfo()
        //    {
        //        Funds = Funds.ReadFromJObject(o["funds"] as JObject),
        //        Rights = Rights.ReadFromJObject(o["rights"] as JObject),
        //        TransactionCount = o.Value<int>("transaction_count"),
        //        OpenOrders = o.Value<int>("open_orders"),
        //        ServerTime = o.Value<int>("server_time")
        //    };
        //}
    }
}
