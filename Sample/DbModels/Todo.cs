using System.ComponentModel.DataAnnotations.Schema;

namespace Sample.DbModels;

public class Todo
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Notes { get; set; } = null!;
    public bool Completed { get; set; }
    public DateTime? CompletionDate { get; set; }
    public int? CompletedByPersonId { get; set; }
    [ForeignKey("CompletedByPersonId")]
    public Person CompletedBy { get; set; } = null!;
}
