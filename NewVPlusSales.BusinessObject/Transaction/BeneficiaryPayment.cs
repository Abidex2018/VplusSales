using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NewVPlusSales.Common;

namespace NewVPlusSales.BusinessObject.Transaction
{
    [Table("NewVPlusSales.BeneficiaryPayment")]
   public class BeneficiaryPayment
    {

        public int BeneficiaryPaymentId { get; set; }

        [Required(ErrorMessage = "Beneficiary Account Transaction Id is Required")]
        public int BeneficiaryAccountTransactionId { get; set; }

        [Required(ErrorMessage = "Beneficiary Account Id is Required")]
        public int BeneficiaryAccountId { get; set; }

        [Required(ErrorMessage = "Beneficiary Id is Required")]
        public int BeneficiaryId { get; set; }

        [Required(ErrorMessage = "Amount Paid  is Required")]
        public decimal AmountPaid { get; set; }

       
        public PaySource PaySource { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Payment Source Name is required")]
        [StringLength(80, MinimumLength = 3, ErrorMessage = "Payment Source Name must be between 3 and 80 characters")]
        public string PaymentSourceName  { get; set; }

        [Column(TypeName = "varchar")]
        [StringLength(18, ErrorMessage = "Payment Reference must be max. 18 characters")]
        public string PaymentReference { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Payment Date - Time is required")]
        [StringLength(35, MinimumLength = 10, ErrorMessage = "Payment Date must be between 10 and 35 characters")]
        public string PaymentDate { get; set; }

        public int RegisteredBy { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Registered Date - Time is required")]
        [StringLength(35, MinimumLength = 10, ErrorMessage = "Registered Date must be between 10 and 35 characters")]
        public string TimeStampRegistered { get; set; }

        public Status Status { get; set; }

        public virtual BeneficiaryAccountTransaction BeneficiaryAccountTransaction { get; set; }
    }
}
