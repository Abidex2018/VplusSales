using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NewVPlusSales.Common;

namespace NewVPlusSales.BusinessObject.CardProduction
{
    [Table("NewVPlusSales.CardDelivery")]
   public class CardDelivery
    {
        public int CardDeliveryId { get; set; }

        [CheckNumber(0, ErrorMessage = "Card Item is Required")]
        public int CardItemId { get; set; }

        [CheckNumber(0, ErrorMessage = "Card Name is Required")]
        public int CardId { get; set; }


        [CheckNumber(0, ErrorMessage = "Card Type is Required")]
        public int CardTypeId { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = false, ErrorMessage = " Defective Batch Number is required")]
        [StringLength(500, ErrorMessage = "Defective Batch Number has maximum of 500 characters")]
        public string DefectiveBatchNumber { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Batch Id is required")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Batch Id  must be 2 characters")]
        public string BatchId { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Start Batch  is required")]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "Start Batch must be 5 characters")]
        public string StartBatchNumber { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Stop Batch  is required")]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "Stop Batch must be 5 characters")]
        public string StopBatchNumber { get; set; }


        [CheckNumber(0, ErrorMessage = "Batch Quantity is Required")]
        public int BatchQuantity { get; set; }


        [CheckNumber(0, ErrorMessage = "Defective Quantity is Required")]
        public int DefectiveQuantity { get; set; }

        [CheckNumber(0, ErrorMessage = "Missing Quantity is Required")]
        public int MissingQuantity { get; set; }

        [CheckNumber(0, ErrorMessage = "Delivered Quantity is Required")]
        public int DeliveredQuantity { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = true, ErrorMessage = "Registration Date - Time is Required")]
        [StringLength(35, MinimumLength = 10, ErrorMessage = "Registration Date - Time must be between 5 and 35 Character")]
        public string TimeStampRegisered { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = true, ErrorMessage = "Approval Comment is required")]
        [StringLength(150, ErrorMessage = "Approval Comment must be between 1 and 150 characters")]
        public string ApproverComment { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = true, ErrorMessage = "Delivered Date - Time is Required")]
        [StringLength(35, MinimumLength = 10, ErrorMessage = "Delivered Date - Time must be between 5 and 35 Character")]
        public string TimeStampDelivered { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = true, ErrorMessage = "Approved Date - Time is Required")]
        [StringLength(35, MinimumLength = 10, ErrorMessage = "Approved Date - Time must be between 5 and 35 Character")]
        public string TimeStampApproved { get; set; }

        public int ReceivedBy { get; set; }

        public int ApprovedBy { get; set; }

        public CardStatus Status { get; set; }

        public virtual CardItem CardItem { get; set; }
    }
}
