using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Studio.Core.Models
{
    public class ProfileData
    {
        public Account Account { get; set; }
        public int[] TankRankHistory;
        public int[] DamageRankHistory;
        public int[] SupportRankHistory;

        public int TankRankActive { get; set; }
        public int DamageRankActive { get; set; }

        public int SupportRankActive { get; set; }

        public string AvatarId;
        public string AvatarUrl => $"https://d15f34w2p8l1cc.cloudfront.net/overwatch/{AvatarId}.png";
    }
}
