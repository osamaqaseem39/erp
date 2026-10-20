using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BlocksERP.Models;
namespace BlocksERP.Models
{
    public class clsUser
    {

        public int UserId { get; set; }
        public int DeleteRoleId { get; set; }
        public int UpdateRoleId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Type { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedByString { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public string UpdatedByString { get; set; }
        public Nullable<int> UserTypeId { get; set; }
        public string Status { get; set; }
        public string StatusStringDummyUser { get; set; }
        public int IsChangePassword { get; set; }

        public int? isSalesman { get; set; }

        public int? DocumentId { get; set; }
        public string ImagePath { get; set; }

        public bool? IsDummyUser { get; set; }

        public string emailname { get; set; }

        public string emailmail { get; set; }

        public string emailmessage { get; set; }

        public int UnReadMessagesCount { get; set; }

        public int TotalUnReadMessagesCount { get; set; }

        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }

        public List<clsUser> UsersList = new List<clsUser>();
        public List<clsDocument> DocList = new List<clsDocument>();
        public List<clsWork> WorkList = new List<clsWork>();
        public List<clsReferal> ReferalList = new List<clsReferal>();
        public List<clsUserBankAccount> BankList = new List<clsUserBankAccount>();
        


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
