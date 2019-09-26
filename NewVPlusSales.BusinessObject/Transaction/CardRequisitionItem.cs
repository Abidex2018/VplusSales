using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NewVPlusSales.BusinessObject.Settings;
using NewVPlusSales.Common;

namespace NewVPlusSales.BusinessObject.Transaction
{
    [Table("NewVPlusSales.CardRequisitionItem")]
  public class CardRequisitionItem
    {

        public long  CardRequisitionItemId { get; set; }

       
        public long CardRequisitionId { get; set; }


        [CheckNumber(0, ErrorMessage = "Beneficiary Id is required")]
        public int BeneficiaryId { get; set; }

        [CheckNumber(0, ErrorMessage = "Beneficiary Id is required")]
        public int CardTypeId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        public decimal Amount { get; set; }


        [CheckNumber(0, ErrorMessage = "Quantity is required")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "CommissionRate is required")]
        public decimal CommissionRate { get; set; }

        [Required(ErrorMessage = "CardCommission Id is required")]
        public int CardCommissionId { get; set; }

        [Required(ErrorMessage = "Commission Amount is required")]
        public decimal CommissionAmount { get; set; }

        [Required(ErrorMessage = "Commission Quantity is required")]
        public int CommissionQuantity { get; set; }

        [Required(ErrorMessage = "Commission Amount is required")]
        public decimal UnitPrice { get; set; }

        public int RequestedBy { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = true, ErrorMessage = "Requested Date - Time is Required")]
        [StringLength(35, ErrorMessage = "Requested Date - Time must be between 1 and 35 Character")]
        public string TimeStampRequested { get; set; }

        public int ApprovedBy { get; set; }

        public string ApproverComment { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = true, ErrorMessage = "Approved Date - Time is Required")]
        [StringLength(35, ErrorMessage = "Approved Date - Time must be between 1 and 35 Character")]
        public string TimeStampApproved { get; set; }


        [Required(ErrorMessage = "Quantity Approved is required")]
        public int QuantityApproved { get; set; }

        public CardRequisitionStatus Status { get; set; }

        public virtual Beneficiary Beneficiary { get; set; }

        public virtual CardRequisition CardRequisition { get; set; }
    }
}
