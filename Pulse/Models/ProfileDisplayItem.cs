using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Studio.Models
{
    public class ProfileDisplayItem
    {
        public ProfileV2 Profile { get; set; }

        public ProfileDisplayItem(ProfileV2 profile)
        {
            Profile = profile;
        }
    }
}
