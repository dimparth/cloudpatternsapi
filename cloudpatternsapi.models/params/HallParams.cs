

namespace cloudpatternsapi.models
{
    public class HallParams : PaginationParams
    {
        public string? SearchTitle { get; set; }
        public int MinCapacity { get; set; }
        public int MaxCapacity { get; set; }
        public string? OrderBy { get; set; }
    }
}