namespace HumanResources
{
    public struct StringLC
    {
        private readonly string Value;
        public StringLC(string value) => this.Value = value.ToLowerInvariant();
        public static implicit operator StringLC(string value) => new StringLC(value);
        public override string ToString() => this.Value;
    }
}
