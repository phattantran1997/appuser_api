using System;
using WebService.DTO;

namespace WebService.Entities.response
{
    public class GetCurrentJobTimingsResponse : JobTimingResponse
    {
        public string stationName { get; set; }

    }

}
