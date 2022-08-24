using System;
namespace WebService.DTO
{
    public class StationDTO
    {
       
            public string stationName { get; set; }
            public string deviceID { get; set; }
            public int stationGroup { get; set; }
            public string stationStatus { get; set; }
            public int stationNo { get; set; }
            public string dispatchStatus { get; set; }
            public int updateByJobNo { get; set; }
    }
}
