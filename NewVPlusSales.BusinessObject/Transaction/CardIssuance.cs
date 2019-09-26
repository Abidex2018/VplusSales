using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NewVPlusSales.BusinessObject.CardProduction;
using NewVPlusSales.Common;

namespace NewVPlusSales.BusinessObject.Transaction
{
    [Table("NewVPlusSales.CardIssuance")]
    public class CardIssuance
    {
        public int  CardIssuanceId { get; set; }

        [CheckNumber(0,ErrorMessage = " Card Requisition Id is required")]
        public long CardRequisitionId { get; set; }

        [CheckNumber(0, ErrorMessage = "Card Requisition Item Id is required")]
        public int CardRequisitionItemId { get; set; }

        [CheckNumber(0, ErrorMessage = "Beneficiary Id is required")]
        public int BeneficiaryId { get; set; }

        [CheckNumber(0, ErrorMessage = "CardType Id is required")]
        public int CardTypeId { get; set; }

        [CheckNumber(0, ErrorMessage = "CardItem Id is required")]
        public long CardItemId { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Batch Id is required")]
        [StringLength(300, MinimumLength = 5, ErrorMessage = "Batch Id must be 5 characters")]
        public string BatchId { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = false, ErrorMessage = " Start Batch Number is required")]
        [StringLength(300, MinimumLength = 7, ErrorMessage = " Start Batch Number must be between 7 and 12 characters")]
        public string StartBatchNumber { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = false, ErrorMessage = " Stop Batch Number is required")]
        [StringLength(300, MinimumLength = 7, ErrorMessage = " Stop Batch Number must be between 7 and 12 characters")]
        public string StopBatchNumber { get; set; }

        [CheckNumber(0,ErrorMessage = "Quantity Issued Id is required")]
        public int QuantityIssued { get; set; }

        public int IssuedBy { get; set; }


        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = true, ErrorMessage = "Issued Date - Time is Required")]
        [StringLength(35, ErrorMessage = "Issued Date - Time must be between 1 and 35 Character")]
        public string TimeStampIssued { get; set; }


        public virtual CardItem CardItem { get; set; }

        public virtual CardRequisition CardRequisition { get; set; }

        public CardRequisitionStatus Status { get; set; }
    }
}
