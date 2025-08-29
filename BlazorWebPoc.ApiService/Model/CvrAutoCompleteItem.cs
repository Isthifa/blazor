
namespace BlazorWebPoc.ApiService.Model
{
    public class CvrAutoCompleteItem
    {
        public long? Value { get; set; } 
        public string Text { get; set; } = string.Empty;
        public CvrSearchResult Data { get; set; } = new();

  
    }
}
