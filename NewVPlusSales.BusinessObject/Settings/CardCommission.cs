using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NewVPlusSales.Common;

namespace NewVPlusSales.BusinessObject.Settings
{
    [Table("NewVPlusSales.CardCommission")]
    public class CardCommission
    {
        public int CardCommissionId { get; set; }

        [Required(ErrorMessage = "Card Type is Rrquired")]
        public int CardTypeId { get; set; }

        [Required(ErrorMessage = "Lower Amount is Rrquired")]
        public decimal LowerAmount { get; set; }

        [Required(ErrorMessage = "Upper Amount is Rrquired")]
        public decimal UpperAmount { get; set; }

        [Required(ErrorMessage = "Commission Rate is Rrquired")]
        public decimal CommissionRatee { get; set; }

        public Status Status { get; set; }

        public virtual CardType CardType { get; set; }
    }
}
