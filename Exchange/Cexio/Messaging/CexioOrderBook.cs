using System;
using System.Collections.Generic;

namespace CIB.Exchange.Cexio.Messaging
{
    internal class CexioOrderBook
    {
        public long id { get; set; }
        public string pair { get; set; }
        public long timestamp { get; set; }
        public List<List<decimal>> bids { get; set; }
        public List<List<decimal>> asks { get; set; }

        public void Update(CexioMarketDataUpdate marketdataUpdate)
        {
            if (marketdataUpdate.id - id == 1)
            {
                foreach (var bid in marketdataUpdate.bids)
                {
                    for (int i = 0; i < bids.Count; i++)
                    {
                        if (bids[i][0] == bid[0] && bid[1] == 0m)
                        {
                            bids.RemoveAt(i);
                            break;
                        }
                        if (bid[1] == 0m)
                            break;
                        if (bids[i][0] < bid[0])
                        {
                            bids.Insert(i, bid);
                            break;
                        }
                        if (i == bids.Count - 1)
                        {
                            bids.Add(bid);
                            break;
                        }
                    }
                }
                foreach (var ask in marketdataUpdate.asks)
                {
                    for (int i = 0; i < asks.Count; i++)
                    {
                        if (asks[i][0] == ask[0] && ask[1] == 0m)
                        {
                            asks.RemoveAt(i);
                            break;
                        }
                        if (ask[1] == 0m)
                            break;

                        if (asks[i][0] > ask[0])
                        {
                            asks.Insert(i, ask);
                            break;
                        }
                        if (i == asks.Count - 1)
                        {
                            asks.Add(ask);
                            break;
                        }
                    }
                }
                id = marketdataUpdate.id;
                return;
            }
            throw new NotImplementedException();
        }
    }
}