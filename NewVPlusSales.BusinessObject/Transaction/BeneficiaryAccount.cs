using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NewVPlusSales.Common;

namespace NewVPlusSales.BusinessObject.Transaction
{
    [Table("NewVPlusSales.BeneficiaryAccount")]
    public class BeneficiaryAccount
    {

        public BeneficiaryAccount()
        {
            BeneficiaryAccountTransactions= new HashSet<BeneficiaryAccountTransaction>();
        }

        public int BeneficiaryAccountId { get; set; }
        

        [Required(ErrorMessage = "Available Balance is Required")]
        public decimal AvailableBalance { get; set; }

        [Required(ErrorMessage = "Credit Limit is Required")]
        public decimal CreditLimit { get; set; }

        [Required(ErrorMessage = "Last Transaction Amount is Required")]
        public decimal LastTransactionAmount { get; set; }

        [Required(ErrorMessage = "Last Transaction Id is Required")]
        public long LastTransactionId { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Last Transaction Date - Time is required")]
        [StringLength(35, MinimumLength = 10, ErrorMessage = "Last Transaction Date must be between 10 and 35 characters")]
        public string LastTransactionTimeStamp { get; set; }

        public TransactionType LastTransactionType { get; set; }

        public Status Status { get; set; }

        public ICollection<BeneficiaryAccountTransaction> BeneficiaryAccountTransactions { get; set; }
    }
}
