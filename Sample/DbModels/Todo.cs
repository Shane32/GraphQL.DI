using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Sample.DbModels
{
    public class Todo
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Notes { get; set; }
        public bool Completed { get; set; }
        public DateTime? CompletionDate { get; set; }
        public int? CompletedByPersonId { get; set; }
        [ForeignKey("CompletedByPersonId")]
        public Person CompletedBy { get; set; }
    }
}
