using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// 1. Buildings.cs
namespace diplom_3.Models
{
    public class Building
    {
        public int Id { get; set; }
        public string ShortName { get; set; } = null!;   // 'А', 'Б'
        public string? Name { get; set; }

        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}