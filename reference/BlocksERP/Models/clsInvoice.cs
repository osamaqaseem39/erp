using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlocksERP.Models
{
    public class clsInvoice
    {

        public string ComputerNo { get; set; }
        public string CompositKey { get; set; }
        
        public string Type { get; set; }
        public DateTime? RecordDate { get; set; }
        public DateTime? RecordDateSO { get; set; }
        public int? CashCreditType { get; set; }
        public int? DetailCode { get; set; }
        public string DetailName { get; set; }

        public int? SupplierCode { get; set; }
        public string SupplierName { get; set; }

        public int? NoOfAttachment { get; set; }

        public string PlanningPerson { get; set; }

        public string FinishedItemName { get; set; }
        public string CashCustomerName { get; set; }
        public string ReferenceNo { get; set; }
        public DateTime? ReferenceDate { get; set; }
        public string Remarks { get; set; }
        public string RemarksSO { get; set; }
        public decimal? GrossTotal { get; set; }
        public string DiscType { get; set; }
        public decimal? DiscRate { get; set; }
        public decimal? DiscAmount { get; set; }
        public decimal? Overhead { get; set; }
        public decimal? Freight { get; set; }
        public decimal? NetAmount { get; set; }
        public string Status { get; set; }
        public int? Received { get; set; }
        public int? ReceivedDetailCode { get; set; }
        public decimal? ReceivedAmount { get; set; }
        public decimal? BalanceAmount { get; set; }
        public string ReceivedRemarks { get; set; }
        public decimal? PreviousBalance { get; set; }
        public int? CreditDays { get; set; }
        public string Reason { get; set; }
        public int? ShowBalance { get; set; }
        public string RefComputerNo { get; set; }
        public int? BusinessID { get; set; }
        public string BusinessName { get; set; }
        public int? FiscalYearID { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedByString { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedByString { get; set; }
        public int? ApproveCancelByUserId { get; set; }
        public string ApproveCancelByString { get; set; }
        public string ApproveCancelRemarks { get; set; }
        public DateTime? ApproveCancelDateTime { get; set; }
        public string ApproveCancelDateTimeString { get; set; }
        public DateTime? RequireDate { get; set; }        
        public string Transporter { get; set; }
        public string BiltyNo { get; set; }
        public DateTime? BiltyDate { get; set; }
        public string BiltySize { get; set; }
        public string VehicleNo { get; set; }
        public string VehicleType { get; set; }
        public string DriverName { get; set; }
        public string DriverCell { get; set; }
        public string TermsOfSale { get; set; }
        public int? Tax1 { get; set; }
        public int? Tax2 { get; set; }
        public int? Tax1Id { get; set; }
        public string Tax1Name { get; set; }
        public string Tax1Type { get; set; }
        public decimal? Tax1Rate { get; set; }
        public decimal? Tax1Value { get; set; }
        public int? Tax2Id { get; set; }
        public string Tax2Name { get; set; }
        public string Tax2Type { get; set; }
        public decimal? Tax2Rate { get; set; }
        public decimal? Tax2Value { get; set; }
        public decimal? AmountIncTax { get; set; }
        public int? SalesmanId { get; set; }
        public string SalesmanName { get; set; }
        public string SiteName { get; set; }
        public string SiterAddress { get; set; }
        public string SiteContactPerson { get; set; }
        public string SiteContactNo { get; set; }
        
        public int DeleteRoleId { get; set; }
        public int UpdateRoleId { get; set; }
        public int ApproveCancelRoleId { get; set; }

        public int? QtyPCS { get; set; }
        public int? QtyPCSBal { get; set; }
        
        public decimal? RefQty { get; set; }
        public decimal? RejectedQty { get; set; }
        public string LineRemarksApprovalCancel { get; set; }

        public int? RouteID { get; set; }
        public string RouteName { get; set; }
        
        public int? FinishedItemID { get; set; }
        public int? FinishedUnitID { get; set; }
        public decimal? FinishedQty { get; set; }
        public decimal? FinishedRate { get; set; }
        public decimal? FinishedValue { get; set; }

        public string MaterialTypeName { get; set; }

        public int? CurrencyID { get; set; }
        public string CurrencyName { get; set; }
        public decimal? ExchangeRate { get; set; }
        public int? CreditDetailCode { get; set; }
        public string CreditDetailName { get; set; }
        public decimal? CurrencyValue { get; set; }
        

        public List<clsInvoice> invoicedetail = new List<clsInvoice>();
        public List<clsInvoice> invoicelist = new List<clsInvoice>();
        public List<clsInvoiceDetail> invoicedetaillist = new List<clsInvoiceDetail>();
        public List<clsInvoiceTerms> invoicetermslist = new List<clsInvoiceTerms>();
        public List<clsInvoiceEmployee> invoiceemployeelist = new List<clsInvoiceEmployee>();
        public List<clsInvoiceAttendance> invoiceattendancelist = new List<clsInvoiceAttendance>();

        public List<clsDocument> documentlist = new List<clsDocument>();


        public string CompanyName { get; set; }

        public string IsNewOrEdit { get; set; }
        public string WeeklyHoliday { get; set; }

        public int? SerialNo { get; set; }
        public int? ItemID { get; set; }
        public string ItemName { get; set; }
        public int? JobTypeID { get; set; }
        public string JobTypeName { get; set; }
        public int? EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public decimal? PackQty { get; set; }
        public decimal? QtyPerPack { get; set; }
        public decimal? LooseQty { get; set; }
        public decimal? Qty { get; set; }
        public decimal? Rate { get; set; }
        public decimal? NetRate { get; set; }
        public decimal? GrossValue { get; set; }
        public decimal? NetValue { get; set; }
        public string LineRemarks { get; set; }
        public string RefCompositKey { get; set; }
        public int? WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        
        public decimal? InQty { get; set; }
        public decimal? OutQty { get; set; }
        public decimal? AvgRate { get; set; }

        public string InvoiceTaxType { get; set; }
        
        public int? TermsId { get; set; }
        public string TermsIdDescription { get; set; }
        public string TermsDescription { get; set; }


        public int? UnitId { get; set; }
        public string UnitName { get; set; }
        public int? PackUnitID { get; set; }
        public string PackUnitName { get; set; }
        public string Terms { get; set; }

        public string ItemCode { get; set; }
        public string UrduName { get; set; }
        public string DataEntryName { get; set; }

        public string LineRefCompNo { get; set; }
        public string LineRefNo { get; set; }
        public DateTime? LineRefDate { get; set; }


        public string Address { get; set; }
        public string Cell { get; set; }
        public string ContactPerson { get; set; }
        public string Phone { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        public string AmountInString { get; set; }
        
        public int? NoOfRoll { get; set; }
        public int? NoOfCTN { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal? CoreWeight { get; set; }
        public decimal? NetWeight { get; set; }
        public string RateOn { get; set; }
        

        public string JobType { get; set; }
        public string DesignType { get; set; }
        public string CylinderCostType { get; set; }
        public int? MaterialCategoryID { get; set; }
        public decimal? MaterialThickness { get; set; }
        public decimal? MaterialWidth { get; set; }
        public int? LaminationCategoryID { get; set; }
        public decimal? LaminationThickness { get; set; }
        public decimal? LaminationWidth { get; set; }
        public string JobReqType { get; set; }
        public int? FinishGoodsTypeID { get; set; }
        public int? PouchTypeID { get; set; }
        public string ReelDirection { get; set; }
        public string GroundColor { get; set; }
        public int? NoOfColors { get; set; }
        public string PhotoCell { get; set; }
        public string Expiry { get; set; }
        public DateTime? DeliveryDate1 { get; set; }
        public decimal? DeliveryQty1 { get; set; }
        public DateTime? DeliveryDate2 { get; set; }
        public decimal? DeliveryQty2 { get; set; }
        public DateTime? DeliveryDate3 { get; set; }
        public decimal? DeliveryQty3 { get; set; }

        public decimal? NetBalance { get; set; }
        

        public string MaterialCategoryName { get; set; }
        public string LaminationCategoryName { get; set; }
        public string FinishGoodsTypeName { get; set; }
        public string PouchTypeName { get; set; }

        public string SaleItemName { get; set; }

        public decimal? LastPurchaseRate { get; set; }
        public int? ProcessID { get; set; }
        public string ProcessName { get; set; }
        public int? FOHTypeID { get; set; }
        public string FOHTypeName { get; set; }

        public string SubCompositKey { get; set; }
        public string ProcessCompositKey { get; set; }
        public string DetailType { get; set; }

        public decimal? TotalCost { get; set; }
        public decimal? UnitRate { get; set; }
        public decimal? MarginPers { get; set; }
        public decimal? MarginAmount { get; set; }


        public int? CategoryID { get; set; }
        public string CategoryName { get; set; }
        public int? BrandID { get; set; }
        public string BrandName { get; set; }


        public int? SeqNo { get; set; }
        public decimal? AvgStock { get; set; }
        public decimal? StockRate { get; set; }


        public int? ShowDate { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? CityID { get; set; }
        public string CityName { get; set; }
        public string ReportType { get; set; }
        public string RecordStatusType { get; set; }
        public string ReportTypeDetail { get; set; }
        
        public List<clsAccountDetail> DetailList = new List<clsAccountDetail>();
        public List<clsInvoice> InvoicesList = new List<clsInvoice>();
        public List<clsInvoice> InvoicesList2 = new List<clsInvoice>();

        public List<clsInvoiceCheckList> InvoicesCheckList = new List<clsInvoiceCheckList>();
        public List<clsInvoiceProductionTag> InvoicesProductionTag = new List<clsInvoiceProductionTag>();
        
        public decimal? NotDue { get; set; }
        public decimal? Below30 { get; set; }
        public decimal? From31To60 { get; set; }
        public decimal? From46To60 { get; set; }
        public decimal? From61To90 { get; set; }
        public decimal? Above90 { get; set; }
        public decimal? Above120 { get; set; }
        public decimal? AdjustAmount { get; set; }
        public decimal? PendingAmount { get; set; }

        public int? DueDays { get; set; }


        public decimal? SOQty { get; set; }
        public decimal? SOUnitQty { get; set; }


        public int? ShiftID { get; set; }
        public string ShiftName { get; set; }
        public int? MachineID { get; set; }
        public string MachineName { get; set; }
        public int? OperatorID { get; set; }
        public string OperatorName { get; set; }
        public int? SupervisorID { get; set; }
        public string SupervisorName { get; set; }

        public string ProcessStatus { get; set; }
        
        public int? CheckListID { get; set; }
        public string CheckListName { get; set; }
        public string CheckListStatus { get; set; }
        public string CheckListRemarks { get; set; }

        public int? NoOfTags { get; set; }
        public string TagColor { get; set; }


        public decimal? FilmWaste { get; set; }
        public decimal? JobSettingWaste { get; set; }
        public decimal? OperaterMistakeWaste { get; set; }
        public decimal? PrintedWaste { get; set; }
        public string PrintReelNo { get; set; }
        public string JobReadyTime { get; set; }


        public int? SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }



        public decimal? PETMicron { get; set; }
        public decimal? MPETMicron { get; set; }
        public decimal? LDMicron { get; set; }
        public decimal? Size { get; set; }

        public decimal? RatePerCM { get; set; }

        public string PrintSide { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string ExpiryDateString { get; set; }
        public string ExpiryDateStr { get; set; }

        public string JobPicture { get; set; }
        public decimal? Width { get; set; }
        public int? WNoOfUps { get; set; }
        public decimal? Height { get; set; }
        public int? HNoOfUps { get; set; }
        public decimal? CylLength { get; set; }
        public decimal? PrintSize { get; set; }
        public decimal? Circum { get; set; }
        public int? ColorID1 { get; set; }
        public int? ColorID2 { get; set; }
        public int? ColorID3 { get; set; }
        public int? ColorID4 { get; set; }
        public int? ColorID5 { get; set; }
        public int? ColorID6 { get; set; }
        public int? ColorID7 { get; set; }
        public int? ColorID8 { get; set; }
        public string RackName { get; set; }

        public string ColorName1 { get; set; }
        public string ColorName2 { get; set; }
        public string ColorName3 { get; set; }
        public string ColorName4 { get; set; }
        public string ColorName5 { get; set; }
        public string ColorName6 { get; set; }
        public string ColorName7 { get; set; }
        public string ColorName8 { get; set; }


        public string ProductionPlanningDone { get; set; }
        public string ProductionPlanningStatus { get; set; }
        public string ProductionDone { get; set; }
        public string DirectProductionNo { get; set; }
        public int? ProductionPlanningNoFurther { get; set; }

    }

    public class clsInvoiceCheckList
    {
        public string CompositKey { get; set; }
        public string ComputerNo { get; set; }
        public string Type { get; set; }
        public int? SerialNo { get; set; }

        public int? CheckListID { get; set; }
        public string CheckListName { get; set; }
        public string CheckListStatus { get; set; }
        public string CheckListRemarks { get; set; }

        public int? ExtraInt1 { get; set; }
        public int? ExtraInt2 { get; set; }
        public int? ExtraInt3 { get; set; }
        public decimal? ExtraDecimal1 { get; set; }
        public decimal? ExtraDecimal2 { get; set; }
        public decimal? ExtraDecimal3 { get; set; }
        public string ExtraString1 { get; set; }
        public string ExtraString2 { get; set; }
        public string ExtraString3 { get; set; }
    }

    public class clsInvoiceProductionTag
    {
        public string CompositKey { get; set; }
        public string ComputerNo { get; set; }
        public string Type { get; set; }
        public int? SerialNo { get; set; }

        public int? NoOfTags { get; set; }
        public string TagColor { get; set; }

        public int? ExtraInt1 { get; set; }
        public int? ExtraInt2 { get; set; }
        public int? ExtraInt3 { get; set; }
        public decimal? ExtraDecimal1 { get; set; }
        public decimal? ExtraDecimal2 { get; set; }
        public decimal? ExtraDecimal3 { get; set; }
        public string ExtraString1 { get; set; }
        public string ExtraString2 { get; set; }
        public string ExtraString3 { get; set; }
    }



    public class clsInvoiceDetail
    {
        public string CompositKey { get; set; }
        public string ComputerNo { get; set; }
        public string Type { get; set; }
        public int? SerialNo { get; set; }
        public int? SequenceNo { get; set; }
        public int? ItemID { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public int? WarehouseID { get; set; }
        public string WarehouseName { get; set; }

        public string SubCompositKey { get; set; }
        public string ProcessCompositKey { get; set; }

        public int? SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public int? Approved { get; set; }

        public int? UnitId { get; set; }
        public string UnitName { get; set; }
        public int? PackUnitID { get; set; }
        public string PackUnitName { get; set; }
        public int? JobTypeID { get; set; }
        public string JobTypeName { get; set; }
        public int? EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public decimal? PackQty { get; set; }
        public decimal? QtyPerPack { get; set; }
        public decimal? LooseQty { get; set; }
        public decimal? Qty { get; set; }
        public decimal? Rate { get; set; }
        public string DiscType { get; set; }
        public decimal? DiscRate { get; set; }
        public decimal? DiscAmount { get; set; }
        public decimal? NetRate { get; set; }
        public decimal? GrossValue { get; set; }
        public int? Tax1Id { get; set; }
        public string Tax1Name { get; set; }
        public string Tax1Type { get; set; }
        public decimal? Tax1Rate { get; set; }
        public decimal? Tax1Value { get; set; }
        public int? Tax2Id { get; set; }
        public string Tax2Name { get; set; }
        public string Tax2Type { get; set; }
        public decimal? Tax2Rate { get; set; }
        public decimal? Tax2Value { get; set; }
        public decimal? NetValue { get; set; }
        public string LineRemarks { get; set; }
        public string RefCompositKey { get; set; }
        public string RateOn { get; set; }
        
        public int? QtyPCS { get; set; }

        public decimal? InQty { get; set; }
        public decimal? OutQty { get; set; }
        public decimal? AvgRate { get; set; }
        public int? RequiredTest { get; set; }
        public int? RequiredInterview { get; set; }

        public int? DoneTest { get; set; }
        public int? DoneInterview { get; set; }

        public string EmployeeStatus { get; set; }
        public string EmployeeStatusRemarks { get; set; }

        public string RefComputerNo { get; set; }

        public decimal? RefQty { get; set; }
        public decimal? RejectedQty { get; set; }
        public string LineRemarksApprovalCancel { get; set; }


        public int? ExtraInt1 { get; set; }
        public int? ExtraInt2 { get; set; }
        public int? ExtraInt3 { get; set; }
        public decimal? ExtraDecimal1 { get; set; }
        public decimal? ExtraDecimal2 { get; set; }
        public decimal? ExtraDecimal3 { get; set; }
        public string ExtraString1 { get; set; }
        public string ExtraString2 { get; set; }
        public string ExtraString3 { get; set; }


        public int? NoOfRoll { get; set; }
        public int? NoOfCTN { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal? CoreWeight { get; set; }
        public decimal? NetWeight { get; set; }

        
        public int? EmpShortList { get; set; }
        public int? EmpSelected { get; set; }
        public int? EmpRejected { get; set; }
        public int? EmpPending { get; set; }
        public int? EmpDemobilize { get; set; }


        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public DateTime? JoinDate { get; set; }

        public decimal? EmployeeRate { get; set; }
        public decimal? WorkingHours { get; set; }
        public int? Medical { get; set; }
        public int? Transport { get; set; }
        public int? Insurance { get; set; }
        public int? Residence { get; set; }
        public int? Ticket { get; set; }
        public int? Food { get; set; }


        public string BrandName { get; set; }
        public string UrduName { get; set; }
        public string DataEntryName { get; set; }

        public string LineRefCompNo { get; set; }
        public string LineRefNo { get; set; }
        public DateTime? LineRefDate { get; set; }

        public string JobType { get; set; }
        public string DesignType { get; set; }
        public string CylinderCostType { get; set; }
        public int? MaterialCategoryID { get; set; }
        public decimal? MaterialThickness { get; set; }
        public decimal? MaterialWidth { get; set; }
        public int? LaminationCategoryID { get; set; }
        public decimal? LaminationThickness { get; set; }
        public decimal? LaminationWidth { get; set; }
        public string JobReqType { get; set; }
        public int? FinishGoodsTypeID { get; set; }
        public int? PouchTypeID { get; set; }
        public string ReelDirection { get; set; }
        public string GroundColor { get; set; }
        public int? NoOfColors { get; set; }
        public string PhotoCell { get; set; }
        public string Expiry { get; set; }
        public DateTime? DeliveryDate1 { get; set; }
        public decimal? DeliveryQty1 { get; set; }
        public DateTime? DeliveryDate2 { get; set; }
        public decimal? DeliveryQty2 { get; set; }
        public DateTime? DeliveryDate3 { get; set; }
        public decimal? DeliveryQty3 { get; set; }


        public string MaterialCategoryName { get; set; }
        public string LaminationCategoryName { get; set; }
        public string FinishGoodsTypeName { get; set; }
        public string PouchTypeName { get; set; }

        public string SaleItemName { get; set; }

        public decimal? LastPurchaseRate { get; set; }
        public int? ProcessID { get; set; }
        public string ProcessName { get; set; }
        public int? FOHTypeID { get; set; }
        public string FOHTypeName { get; set; }
        public string DetailType { get; set; }
        public string MaterialTypeName { get; set; }


        public decimal? RatePerCM { get; set; }

        public string PrintSide { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string ExpiryDateString { get; set; }
        public string ExpiryDateStr { get; set; }

        public string JobPicture { get; set; }
        public decimal? Width { get; set; }
        public int? WNoOfUps { get; set; }
        public decimal? Height { get; set; }
        public int? HNoOfUps { get; set; }
        public decimal? CylLength { get; set; }
        public decimal? PrintSize { get; set; }
        public decimal? Circum { get; set; }
        public int? ColorID1 { get; set; }
        public int? ColorID2 { get; set; }
        public int? ColorID3 { get; set; }
        public int? ColorID4 { get; set; }
        public int? ColorID5 { get; set; }
        public int? ColorID6 { get; set; }
        public int? ColorID7 { get; set; }
        public int? ColorID8 { get; set; }
        public string RackName { get; set; }

        public string ColorName1 { get; set; }
        public string ColorName2 { get; set; }
        public string ColorName3 { get; set; }
        public string ColorName4 { get; set; }
        public string ColorName5 { get; set; }
        public string ColorName6 { get; set; }
        public string ColorName7 { get; set; }
        public string ColorName8 { get; set; }
        
    }


    public class clsInvoiceEmployee
    {
        public string CompositKey { get; set; }
        public string ComputerNo { get; set; }
        public string Type { get; set; }
        public int? SerialNo { get; set; }
        public int? JobTypeID { get; set; }
        public string JobTypeName { get; set; }
        public int? EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public decimal? Qty { get; set; }
        public decimal? Rate { get; set; }
        public decimal? EmployeeRate { get; set; }
        public decimal? WorkingHours { get; set; }
        public string RefCompositKey { get; set; }
        public string EmployeeStatus { get; set; }
        public string EmployeeStatusRemarks { get; set; }

        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public DateTime? JoinDate { get; set; }

        public int? Medical { get; set; }
        public int? Transport { get; set; }
        public int? Insurance { get; set; }
        public int? Residence { get; set; }
        public int? Ticket { get; set; }
        public int? Food { get; set; }

        public int? RequiredTest { get; set; }
        public int? RequiredInterview { get; set; }
        public int? DoneTest { get; set; }
        public int? DoneInterview { get; set; }
        public string RefComputerNo { get; set; }


        public int? ExtraInt1 { get; set; }
        public int? ExtraInt2 { get; set; }
        public int? ExtraInt3 { get; set; }
        public decimal? ExtraDecimal1 { get; set; }
        public decimal? ExtraDecimal2 { get; set; }
        public decimal? ExtraDecimal3 { get; set; }
        public string ExtraString1 { get; set; }
        public string ExtraString2 { get; set; }
        public string ExtraString3 { get; set; }


        public int? EmpShortList { get; set; }
        public int? EmpSelected { get; set; }
        public int? EmpRejected { get; set; }
        public int? EmpPending { get; set; }

    }


    public class clsInvoiceAttendance
    {
        public int? RecordID { get; set; }
        public int? SerialNo { get; set; }
        public string ComputerNo { get; set; }
        public string CompositKey { get; set; }
        
        public int? JobTypeID { get; set; }
        public string JobTypeName { get; set; }
        public int? EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public decimal? EmployeeRate { get; set; }
        public decimal? WorkingHours { get; set; }
        public string RefCompositKey { get; set; }
        public string RefComputerNo { get; set; }

        public DateTime? AttendanceDate { get; set; }
        public string AttendanceDateStr { get; set; }
        public string Attendance { get; set; }
        public string WeeklyHoliday { get; set; }
        
        public decimal? TotalHours { get; set; }
        public decimal? TotalValue { get; set; }

        public DateTime? MonthDate { get; set; }
        
        public int? ExtraInt1 { get; set; }
        public int? ExtraInt2 { get; set; }
        public int? ExtraInt3 { get; set; }
        public decimal? ExtraDecimal1 { get; set; }
        public decimal? ExtraDecimal2 { get; set; }
        public decimal? ExtraDecimal3 { get; set; }
        public string ExtraString1 { get; set; }
        public string ExtraString2 { get; set; }
        public string ExtraString3 { get; set; }

        public List<clsInvoiceAttendance> invoiceattendancelist = new List<clsInvoiceAttendance>();
    }
    public class clsInvoiceTerms
    {
        public string CompositKey { get; set; }
        public string ComputerNo { get; set; }
        public string Type { get; set; }
        public int? SerialNo { get; set; }
        public int? TermsId { get; set; }
        public string TermsIdDescription { get; set; }
        public string TermsDescription { get; set; }
    }
}


