using System.Collections.Generic;

namespace CIB.Exchange.Kraken.DTO
{
    internal sealed class RootObjectDto<T>
    {
        public List<string> error { get; set; }
        public Dictionary<string, T> result { get; set; }
    }
}
