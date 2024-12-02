namespace ZBar.Blazor.Dtos
{
    /// <summary>
    /// Strongly typed representaiton of ZBar's symbol object, for use with JS Interop.
    /// </summary>
    internal record class Symbol
    {
        public string TypeName { get; init; }
        public string RawValue { get; init; }
    }
}