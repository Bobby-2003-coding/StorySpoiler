using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace StorySpoilerAPITests.Models
{
    internal class ApiResponseDTO
    {
        [JsonPropertyName("msg")]
        public string? Message { get; set; }

        [JsonPropertyName("storyId")]
        public string? StoryId { get; set; }
    }
}
