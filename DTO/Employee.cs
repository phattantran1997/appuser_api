using System;
namespace WebService.DTO
{


    public class Employee
    {
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string Factory { get; set; }
        public int FaceID { get; set; }
        public object profilePath { get; set; }
        public string position { get; set; }
        public string Company { get; set; }
        public string Country { get; set; }
        public string Manager { get; set; }
        
    }
}
