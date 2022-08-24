using System;
using System.Collections.Generic;

namespace WebService.DTO
{
    public class GetLastestJobTimingsRequest
    {
        public string jobday { get; set; }
        public List<string> users { get; set; }
    }
}
