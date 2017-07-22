namespace CIB.Exchange.BTCe.DTO
{
    internal class BTCeRootObject<T>
    {
        public bool success { get; set; }
        public T @return { get; set; }
        public string error { get; set; }
    }
}
