using System;

namespace BlocksERP.Models
{
    public class clsTaxSetup
    {
        public int? TaxId { get; set; }
        public string TaxName { get; set; }
        public decimal? TaxRate { get; set; }
        public string TaxType { get; set; }
        public int? DetailCode { get; set; }
        public string DetailCodeName { get; set; }

        public int? Status { get; set; }
        public string StatusString { get; set; }
        public string IsNeWOrEdit { get; set; }

        public DateTime? CreatedDate { get; set; }
        public String CreatedByString { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedByString { get; set; }
        public int DeleteRoleId { get; set; }
        public int UpdateRoleId { get; set; }
    }
}