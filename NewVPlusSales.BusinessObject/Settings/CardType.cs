using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NewVPlusSales.BusinessObject.CardProduction;
using NewVPlusSales.Common;

namespace NewVPlusSales.BusinessObject.Settings
{
    [Table("NewVPlusSales.CardType")]
    public class CardType
    {
        public CardType()
        {
            CardCommissions=new HashSet<CardCommission>();
            CardProductions=new HashSet<Card>();
        }
        public int CardTypeId { get; set; }

        [Column(TypeName = "varchar")]
        [Required(AllowEmptyStrings = false,ErrorMessage = "Name is Required")]
        [StringLength(150,MinimumLength = 3,ErrorMessage = "Name must be between 3 and 150 Character")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Face Value is Required")]
        public decimal FaceValue { get; set; }

        public Status Status { get; set; }

        public ICollection<Card> CardProductions { get; set; }

        public ICollection<CardCommission> CardCommissions { get; set; }

    }
}
