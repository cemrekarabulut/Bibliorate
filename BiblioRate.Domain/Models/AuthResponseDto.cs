using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace BiblioRate.Domain.Models
{
    public class AuthResponseDto
    {
    [JsonPropertyName("user_id")]
    public string UserId { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }    
    }
}