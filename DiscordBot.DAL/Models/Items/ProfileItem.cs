using DiscordBot.DAL.Models.Profiles;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBot.DAL.Models.Items
{
    public class ProfileItem : Entity
    {
        public int ProfileId { get; set; }
        [ForeignKey("ProfileId")]
        public Profile Profile { get; set; }
        public int ItemId { get; set; }
        [ForeignKey("ItemId")]
        public Item Item { get; set; }

        public override string ToString()
        {
            return $"ProfileId: {ProfileId}\nItemId: {ItemId}";
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.ToString() == obj.ToString();
        }
    }
}
