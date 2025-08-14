using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StorySpoiler.Models
{
    public class ApiResponseDTO
    {
        [JsonPropertyName("msg")]
        public string Message { get; set; }

        [JsonPropertyName("storyId")]
        public string? storyId { get; set; }
    }
}
