namespace MiniAccountSystemDB.Models
{
    public class ChartOfAccount
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string AccountType { get; set; } = string.Empty;  // Required for INSERT

        public int? ParentId { get; set; }  // Optional for hierarchy
    }
}
