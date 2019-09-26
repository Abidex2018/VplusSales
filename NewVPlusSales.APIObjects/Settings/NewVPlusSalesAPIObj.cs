using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewVPlusSales.APIObjects.Common;
using NewVPlusSales.Common;

namespace NewVPlusSales.APIObjects.Settings
{
    #region Card Type

    public class RegCardTypeObj : AdminObj
    {
       
        [Required(AllowEmptyStrings = false, ErrorMessage = "Name is Required")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 150 Character")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Face Value is Required")]
        public decimal FaceValue { get; set; }

        public int Status { get; set; }
    }


    public class EditCardTypeObj : AdminObj
    {
        [CheckNumber(0,ErrorMessage = "Card Type Id is Required")]
        public int CardTypeId { get; set; }
        
        [Required(AllowEmptyStrings = false, ErrorMessage = "Name is Required")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 150 Character")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Face Value is Required")]
        public decimal FaceValue { get; set; }

        public int Status { get; set; }
    }

    public class DeleteCardtypeObj : AdminObj
    {
        [CheckNumber(0, ErrorMessage = "Card Type Id is Required")]
        public int CardTypeId { get; set; }
    }
    #endregion

    #region Card Commission

    public class RegCardCommissionObj : AdminObj
    {
       

        [CheckNumber(0,ErrorMessage = "Card Type is Rrquired")]
        public int CardTypeId { get; set; }

        [Required(ErrorMessage = "Lower Amount is Rrquired")]
        public decimal LowerAmount { get; set; }

        [Required(ErrorMessage = "Upper Amount is Rrquired")]
        public decimal UpperAmount { get; set; }

        [Required(ErrorMessage = "Commission Rate is Rrquired")]
        public decimal CommissionRatee { get; set; }

        public int Status { get; set; }
    }

    public class EditCardCommissionObj : AdminObj
    {
        [CheckNumber(0, ErrorMessage = "Card Type is Rrquired")]
        public int CardCommissionId { get; set; }

        [CheckNumber(0, ErrorMessage = "Card Type is Rrquired")]
        public int CardTypeId { get; set; }

        [Required(ErrorMessage = "Lower Amount is Rrquired")]
        public decimal LowerAmount { get; set; }

        [Required(ErrorMessage = "Upper Amount is Rrquired")]
        public decimal UpperAmount { get; set; }

        [Required(ErrorMessage = "Commission Rate is Rrquired")]
        public decimal CommissionRatee { get; set; }

        public int Status { get; set; }
    }

    public class DeleteCardCommission : AdminObj
    {
        [CheckNumber(0, ErrorMessage = "Card Type is Rrquired")]
        public int CardCommissionId { get; set; }
    }
    #endregion

    #region Benefciary

    public class RegBeneficiaryObj:AdminObj
    {
       
     
        [Required(AllowEmptyStrings = false, ErrorMessage = "Full Name is Required")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Full must be between 3 and 150 Character")]
        public string FulllName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Beneficiary Type is Required")]
        public int BeneficiaryType { get; set; }

       
        [Required(AllowEmptyStrings = false, ErrorMessage = "Beneficiary Mobile Number is Required")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Invaild Beneficiary Mobile Number")]
        public string MobileNumber { get; set; }

      
        [Required(AllowEmptyStrings = true, ErrorMessage = "Beneficiary Email is Required")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}", ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

       
        [Required(AllowEmptyStrings = false, ErrorMessage = "Beneficiary Address is Required")]
        [StringLength(150, MinimumLength = 5, ErrorMessage = "Beneficiary Address must be between 5 and 150 Character")]
        public string Address { get; set; }

      

        public int Status { get; set; }
    }


    public class EditBeneficiaryObj:AdminObj
    {
        [CheckNumber(0,ErrorMessage = "Beneficiary Id is Required")]
        public int BeneficiaryId { get; set; }
        
        [Required(AllowEmptyStrings = false, ErrorMessage = "Full Name is Required")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Full must be between 3 and 150 Character")]
        public string FullName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Beneficiary Type is Required")]
        public int BeneficiaryType { get; set; }


        [Required(AllowEmptyStrings = false, ErrorMessage = "Beneficiary Mobile Number is Required")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Invaild Beneficiary Mobile Number")]
        public string MobileNumber { get; set; }


        [Required(AllowEmptyStrings = true, ErrorMessage = "Beneficiary Email is Required")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}", ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }


        [Required(AllowEmptyStrings = false, ErrorMessage = "Beneficiary Address is Required")]
        [StringLength(150, MinimumLength = 5, ErrorMessage = "Beneficiary Address must be between 5 and 150 Character")]
        public string Address { get; set; }
       
        public int Status { get; set; }
    }

    public class DeleteBeneficiaryObj : AdminObj
    {
        [CheckNumber(0, ErrorMessage = "Beneficiary Id is Required")]
        public int BeneficiaryId { get; set; }
    }

    public class ApproveBeneficairyObj: AdminObj
    {
        [CheckNumber(0, ErrorMessage = "Beneficiary Id is required")]
        public long BeneficiaryId { get; set; }

        [Required(AllowEmptyStrings = true, ErrorMessage = "Approval Comment is Required")]
        [StringLength(150, ErrorMessage = "Beneficiary Address must be between 1 and 150 Character")]
        public string ApprovalComment { get; set; }
    }
    #endregion

    #region Card

    public class RegCardObj:AdminObj
    {
       
        [Required(AllowEmptyStrings = false, ErrorMessage = "Card Title is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Card Title  must be between 3 and 200 characters")]
        public string CardTitle { get; set; }

        [CheckNumber(0, ErrorMessage = "Card Type is Required")]
        public int CardTypeId { get; set; }

        [CheckNumber(0, ErrorMessage = "Total Quantity is Required")]
        public int TotalQuantity { get; set; }

       
        [Required(AllowEmptyStrings = false, ErrorMessage = "Batch key is required")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Batch key  must be 2 characters")]
        public string BatchKey { get; set; }

       
        [Required(AllowEmptyStrings = false, ErrorMessage = "Start Batch  is required")]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "Start Batch must be 5 characters")]
        public string StartBatchId { get; set; }

       
        [Required(AllowEmptyStrings = false, ErrorMessage = "Stop Batch  is required")]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "Stop Batch must be 5 characters")]
        public string StopBatchId { get; set; }

        [CheckNumber(0, ErrorMessage = "Number of Batch is Required")]
        public int NumberOfBatches { get; set; }

        [CheckNumber(0, ErrorMessage = "Quantity per batch is Required")]
        public int QuantityPerBatch { get; set; }

    }

    public class EditCardObj : AdminObj
    {
        [CheckNumber(0, ErrorMessage = "Card Type is Required")]
        public int CardId { get; set; }

        [CheckNumber(0, ErrorMessage = "Card Item Id is required")]
        public int CardItemId { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Card Title is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Card Title  must be between 3 and 200 characters")]
        public string CardTitle { get; set; }

        [CheckNumber(0, ErrorMessage = "Card Type is Required")]
        public int CardTypeId { get; set; }

        [Required(ErrorMessage = "Missing Quantity Found is required")]
        public int DefectiveQuantityRectified { get; set; }

        [Required(ErrorMessage = "Missing Quantity Found is required")]
        public int MissingQuantityFound { get; set; }

        [CheckNumber(0, ErrorMessage = "Total Quantity is Required")]
        public int TotalQuantity { get; set; }


        [Required(AllowEmptyStrings = false, ErrorMessage = "Batch key is required")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Batch key  must be 2 characters")]
        public string BatchKey { get; set; }


        [Required(AllowEmptyStrings = false, ErrorMessage = "Start Batch  is required")]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "Start Batch must be 5 characters")]
        public string StartBatchId { get; set; }


        [Required(AllowEmptyStrings = false, ErrorMessage = "Stop Batch  is required")]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "Stop Batch must be 5 characters")]
        public string StopBatchId { get; set; }

        [CheckNumber(0, ErrorMessage = "Number of Batch is Required")]
        public int NumberOfBatches { get; set; }

        [CheckNumber(0, ErrorMessage = "Quantity per batch is Required")]
        public int QuantityPerBatch { get; set; }


        public int Status { get; set; }
    }
    public class DeleteCardObj : AdminObj
    {
        [CheckNumber(0, ErrorMessage = "Card Type is Required")]
        public int CardId { get; set; }
    }
    public class LoadCardByDateObj : AdminObj
    {


        [StringLength(35, MinimumLength = 7, ErrorMessage = "Invalid Date - Time registered")]
        [DataType(DataType.DateTime)]
        public string BeginDate { get; set; }

        [StringLength(35, MinimumLength = 7, ErrorMessage = "Invalid Date - Time registered")]
        [DataType(DataType.DateTime)]
        public string EndDate { get; set; }
    }
    #endregion

    
    #region Card Delivery

    public class RegCardDeliveryObj : AdminObj
    {
        

        [CheckNumber(0, ErrorMessage = "Card Item is Required")]
        public int CardItemId { get; set; }

        [CheckNumber(0, ErrorMessage = "Card Name is Required")]
        public int CardId { get; set; }
        
        [CheckNumber(0, ErrorMessage = "Card Type is Required")]
        public int CardTypeId { get; set; }

       
        [Required(AllowEmptyStrings = false, ErrorMessage = " Defective Batch Number is required")]
        [StringLength(500, ErrorMessage = "Defective Batch Number has maximum of 500 characters")]
        public string DefectiveBatchNumber { get; set; }

       
        [Required(AllowEmptyStrings = false, ErrorMessage = "Start Batch  is required")]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "Start Batch must be 5 characters")]
        public string StartBatchNumber { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Batch Id is required")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Batch Id  must be 2 characters")]
        public string BatchId { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Stop Batch  is required")]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "Stop Batch must be 5 characters")]
        public string StopBatchNumber { get; set; }
     
        [Required(AllowEmptyStrings = true, ErrorMessage = "Approval Comment is required")]
        [StringLength(150, ErrorMessage = "Approval Comment must be between 1 and 150 characters")]
        public string ApproverComment { get; set; }

        [CheckNumber(0, ErrorMessage = "Defective Quantity is Required")]
        public int DefectiveQuantity { get; set; }

        [CheckNumber(0, ErrorMessage = "Missing Quantity is Required")]
        public int MissingQuantity { get; set; }

        [CheckNumber(0, ErrorMessage = "Delivered Quantity is Required")]
        public int DeliveredQuantity { get; set; }

        [DataType(DataType.DateTime)]
        [StringLength(35, MinimumLength = 7, ErrorMessage = "Invalid Date - Time Delivered")]
        public string TimeStampDelivered { get; set; }
        
       
    }

    public class EditCardDeliveryObj : AdminObj
    {
        [CheckNumber(0, ErrorMessage = "Card Delivery Id is Required")]
        public int CardDeliveryId { get; set; }

        [CheckNumber(0, ErrorMessage = "Card Item is Required")]
        public int CardItemId { get; set; }

        [CheckNumber(0, ErrorMessage = "Card Name is Required")]
        public int CardId { get; set; }


        [CheckNumber(0, ErrorMessage = "Card Type is Required")]
        public int CardTypeId { get; set; }

       
        [Required(AllowEmptyStrings = false, ErrorMessage = " Defective Batch Number is required")]
        [StringLength(500, ErrorMessage = "Defective Batch Number has maximum of 500 characters")]
        public string DefectiveBatchNumber { get; set; }
        
        [CheckNumber(0, ErrorMessage = "Defective Quantity is Required")]
        public int DefectiveQuantity { get; set; }

        [CheckNumber(0, ErrorMessage = "Missing Quantity is Required")]
        public int MissingQuantity { get; set; }

        [CheckNumber(0, ErrorMessage = "Delivered Quantity is Required")]
        public int DeliveredQuantity { get; set; }

        [StringLength(35, MinimumLength = 7, ErrorMessage = "Invalid Date - Time registered")]
        [DataType(DataType.DateTime)]
        public string DeliveryDate { get; set; }
    }

    public class DeleteCardDeliveryObj : AdminObj
    {
        [CheckNumber(0, ErrorMessage = "Card Delivery Id is Required")]
        public int CardDeliveryId { get; set; }

    }

    public class ApproveCardDeliveryObj : AdminObj
    {
        [CheckNumber(0, ErrorMessage = "Card Delivery Id is Required")]
        public long CardDeliveryId { get; set; }

        [Required(AllowEmptyStrings = true, ErrorMessage = "Approval Comment is required")]
        [StringLength(150, MinimumLength = 1, ErrorMessage = "Approval Comment must be between 1 and 150 characters")]
        public string ApproverComment { get; set; }

        public bool IsApproved { get; set; }

        public bool IsDenied { get; set; }

        [DataType(DataType.DateTime)]
        [StringLength(35, MinimumLength = 7, ErrorMessage = "Invalid Date - Time Approved")]
        public string TimeStampApproved { get; set; }
    }

    public class LoadCardDeliveryByDate:AdminObj
    {
        [StringLength(35, ErrorMessage = "Invalid Date - Time registered")]
        [DataType(DataType.DateTime)]
        public string BeginDate { get; set; }

        [StringLength(35, ErrorMessage = "Invalid Date - Time registered")]
        [DataType(DataType.DateTime)]
        public string EndDate { get; set; }

        public int CardId { get; set; }
    }
    #endregion

    #region Beneficiary Payment

    public class RegBeneficiaryPaymentObj: AdminObj
    {
        [CheckNumber(0, ErrorMessage = "Beneficiary Id is required")]
        public int BeneficiaryId { get; set; }

        [Required(ErrorMessage = "Amount Paid  is Required")]
        public decimal AmountPaid { get; set; }

        [Required(ErrorMessage = "Payment Source is Required")]
        public int PaySource { get; set; }

        
        [Required(AllowEmptyStrings = false, ErrorMessage = "Payment Source Name is required")]
        public string PaymentSourceName { get; set; }

       
        [StringLength(15, ErrorMessage = "Payment Reference must be max. 15 characters")]
        public string PaymentReference { get; set; }


        [DataType(DataType.DateTime)]
        [StringLength(35, MinimumLength = 7, ErrorMessage = "Invalid Date - Time registered | MM/DD/YY")]
        public string PaymentDate { get; set; }

    }

    public class LoadBeneficiaryAccountTransactionByDateObj:AdminObj
    {

        [StringLength(35, MinimumLength = 7, ErrorMessage = "Invalid Date - Time registered")]
        [DataType(DataType.DateTime)]
        public string BeginDate { get; set; }

        [StringLength(35, MinimumLength = 7, ErrorMessage = "Invalid Date - Time registered")]
        [DataType(DataType.DateTime)]
        public string EndDate { get; set; }

        public int BeneficiaryId { get; set; }
    }

    public class LoadBeneficiaryPaymentByDateObj:AdminObj
    {
        [StringLength(35, MinimumLength = 7, ErrorMessage = "Invalid Date - Time registered")]
        [DataType(DataType.DateTime)]
        public string BeginDate { get; set; }

        [StringLength(35, MinimumLength = 7, ErrorMessage = "Invalid Date - Time registered")]
        [DataType(DataType.DateTime)]
        public string EndDate { get; set; }

        public int BeneficiaryId { get; set; }
    }

    public class DeleteBeneficiaryPaymentObj:AdminObj
    {
        [CheckNumber(0, ErrorMessage = "Beneficiary Payment Id is required")]
        public int BeneficiaryPaymentId { get; set; }
    }

    public class DeleteBeneficiaryAccountTransactionObj:AdminObj
    {
        [CheckNumber(0, ErrorMessage = "Beneficiary Account Transaction Id is required")]
        public int BeneficiaryAccTransId { get; set; }
    }
    #endregion
}
