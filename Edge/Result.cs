namespace Edge
{
    public class Result
    {
        public int idExchLong;
        public int idExchShort;
        public decimal exposureLong;
        public decimal exposureShort;
        public decimal spreadIn;
        public decimal feesLong;
        public decimal feesShort;
        public object exchNameLong;
        public object exchNameShort;
        public decimal priceLongIn;
        public decimal priceShortIn;
        public object exitTarget;
        public decimal[][][] maxSpread;
        public decimal[][][] minSpread;

        public Result()
        {
            var numExchanges = 5;
            var pairs = 8;
            maxSpread = new decimal[numExchanges][][];
            for (int i = 0; i < numExchanges; i++)
            {
                maxSpread[i] = new decimal[numExchanges][];
                for (int j = 0; j < numExchanges; j++)
                {
                    maxSpread[i][j] = new decimal[pairs];
                    for (int k = 0; k < pairs; k++)
                        maxSpread[i][j][k] = decimal.MinValue;
                }
            }
            minSpread = new decimal[numExchanges][][];
            for (int i = 0; i < numExchanges; i++)
            {
                minSpread[i] = new decimal[numExchanges][];
                for (int j = 0; j < numExchanges; j++)
                {
                    minSpread[i][j] = new decimal[pairs];
                    for (int k = 0; k < pairs; k++)
                        minSpread[i][j][k] = decimal.MaxValue;
                }
            }
        }
    }
}