using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BiblioRate.Domain.Entities
{
    public class SearchLog
    {
    [Key]
    public int SearchId { get; set; } // SQL: search_id 
    public int? UserId { get; set; } // SQL: user_id (Kullanıcı silinse de log kalsın diye nullable) 
    public string Query { get; set; } // SQL: query (Arama terimi) [cite: 1, 2]
    public DateTime SearchedAt { get; set; } = DateTime.Now; // SQL: searched_at

    // Navigation Property
    public User? User { get; set; }    
    }
}