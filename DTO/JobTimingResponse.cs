using System;
namespace WebService.DTO
{
    public class JobTimingResponse
    {

        public string jobno { get; set; }
        public string operatorID { get; set; }
        public string jobday { get; set; }
        public string jobtime { get; set; }
        public int id { get; set; }
        public int stationNo { get; set; }
        public string duration { get; set; }
        public string filename { get; set; }
        public string handle { get; set; }
        public string itemno { get; set; }
    }
}
