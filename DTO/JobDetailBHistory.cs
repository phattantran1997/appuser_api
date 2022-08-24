using System;
namespace WebService.DTO
{
    public class JobDetailBHistory
    {

        public string jobno { get; set; }
        public string time_start { get; set; }
        public string time { get; set; }
        public string user_name { get; set; }
        public int station_no { get; set; }
        public string item_no { get; set; }

        public JobDetailBHistory(string jobno, string time_start, string time, string username, int stationNo, string itemNo)
        {
            this.jobno = jobno;
            this.time_start = time_start;
            this.time = time;
            this.user_name = username;
            this.station_no = stationNo;
            this.item_no = itemNo;

        }
    }
}
