using System;

namespace BlocksERP.Models
{
    public class clsCountry
    {
        public int CountryId { get; set; }
        public string CountryName { get; set; }
        public string Code { get; set; }
        public int? Status { get; set; }
        public string StatusString { get; set; }
        public DateTime? CreatedDate { get; set; }
        public String CreatedByString { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedByString { get; set; }
        public int DeleteRoleId { get; set; }
        public int UpdateRoleId { get; set; }

    }
}