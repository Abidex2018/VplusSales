using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NewVPlusSales.Common;

namespace NewVPlusSales.BusinessObject.Transaction
{
    [Table("NewVPlusSales.BeneficiaryAccountTransaction")]
    public class BeneficiaryAccountTransaction
    {
        public int BeneficiaryAccountTransactionId { get; set; }

        [Required(ErrorMessage = "Beneficiary Account Id is Required")]
        public int BeneficiaryAccountId { get; set; }

        [Required(ErrorMessage = "Beneficiary Id is Required")]
        public int BeneficiaryId { get; set; }

        public TransactionType TransactionType { get; set; }

        [Required(ErrorMessage = "Amount is Required")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Previous Balance is Required")]
        public decimal PreviousBalance { get; set; }

        [Required(ErrorMessage = "New Balance is Required")]
        public decimal NewBalance { get; set; }

        public TransactionSourceType TransactionSource { get; set; }

        public int  TransactionSourceId { get; set; }

        public int RegisteredBy { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Last Transaction Date - Time is required")]
        [StringLength(35, MinimumLength = 10, ErrorMessage = "Last Transaction Date must be between 10 and 35 characters")]
        public string LastTransactionTimeStamp { get; set; }

        public string TimeStampRegistered { get; set; }

        public Status Status  { get; set; }

        public virtual BeneficiaryAccount BeneficiaryAccount { get; set; }
    }
}
