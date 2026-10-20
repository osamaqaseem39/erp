using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{
    public class clsEmployee
    {
        public int EmployeeId { get; set; }
        public string EmployeeRefId { get; set; }
        public string FullName { get; set; }
        public string LastNameType { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public int? ReligionId { get; set; }
        public string ReligionName { get; set; }
        public string MaritalStatus { get; set; }
        public int? CountryId { get; set; }
        public string CountryName { get; set; }
        public int? CityId { get; set; }
        public string CityName { get; set; }
        public string CNIC { get; set; }
        public string Email { get; set; }
        public string PresentAddress { get; set; }
        public string PermanentAddressSame { get; set; }
        public string PermanentAddress { get; set; }
        public string ContactNo1 { get; set; }
        public string ContactNo2 { get; set; }
        public string EmgContactNo { get; set; }
        public string EmgContactName { get; set; }
        public int? RelationId { get; set; }
        public string RelationName { get; set; }
        public string AdditionalNote { get; set; }
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int? WorkNatureId { get; set; }
        public string WorkNatureName { get; set; }
        public int? ImmediateBossId { get; set; }
        public int? GrossSalary { get; set; }
        public int? DailyAllowance { get; set; }
        public string TimeRecord { get; set; }
        public string Qualification { get; set; }
        public int WorkExperienceId { get; set; }
        public string JobDescription { get; set; }
        public string Organization { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string ImmediateBossName { get; set; }
        public DateTime? Date { get; set; }
        public List<clsEmployee> DateList = new List<clsEmployee>();
        public List<clsEmployee> EmployeeList = new List<clsEmployee>();
        public int AttendanceId { get; set; }
        public DateTime? AttendanceDate { get; set; }
        public string Attendance { get; set; }
        public TimeSpan? InTime { get; set; }
        public TimeSpan? OutTime { get; set; }
        public DateTime? InTimeDateTime { get; set; }
        public DateTime? OutTimeDateTime { get; set; }
        public int? Minutes { get; set; }
        public string AttendanceDay { get; set; }
    }
}