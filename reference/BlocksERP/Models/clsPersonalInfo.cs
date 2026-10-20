using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BlocksERP.Models;
namespace BlocksERP.Models
{
    public class clsPersonalInfo
    {

        public int UserID { get; set; }
        public int PersonalInfoID { get; set; }
        public string FullName { get; set; }
        public string SocialInsuranceNumber { get; set; }
        public int? GenderID { get; set; }
        public string GenderName { get; set; }
        public int? EducationID { get; set; }
        public string EducationName { get; set; }
        public int? MaritalStatusID { get; set; }
        public string MaritalStatusName { get; set; }
        public int? CountryID { get; set; }
        public string CountryName { get; set; }
        public string ResidentialAddress { get; set; }
        public string DetailAddress { get; set; }
        public int? ResidenceCityID { get; set; }
        public string ResidenceCityName { get; set; }
        public int? DetailedCityID { get; set; }
        public string DetailedCityName { get; set; }
        public DateTime? DOB { get; set; }

    }
}
