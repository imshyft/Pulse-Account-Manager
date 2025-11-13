

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Studio.Models.V2
{

    #region Profile

    public class ProfileV2
    {
        public BattleTagV2 Battletag { get; set; }
        public string CustomName { get; set; }
        public string Email { get; set; }
        public string AvatarURL { get; set; }
        [JsonIgnore]
        public int LastUpdate => Snapshots.Select(s => s.Timestamp).DefaultIfEmpty(0).Max();
        public List<ProfileSnapshotV2> Snapshots { get; set; }

    }

    public class ProfileSnapshotV2
    {
        public int Timestamp { get; set; }
        public Dictionary<Roles, RoleV2> Roles { get; set; }
    }

    public class BattleTagV2
    {
        [JsonIgnore]
        public string FullTag => ToString();

        public string Username { get; set; }
        public string Tag { get; set; }

        [JsonConstructor]
        public BattleTagV2(string username, string tag)
        {
            Username = username;
            Tag = tag;
        }
        public BattleTagV2(string battletag)
        {
            string[] parts = battletag.Split('#');
            Username = parts[0];
            Tag = parts[1];
        }

        public override string ToString()
        {
            return $"{Username}#{Tag}";
        }
    }

    #endregion


    #region Roles

    public abstract class RoleV2 : INotifyPropertyChanged
    {
        private bool _isSelectedForComparison;
        [JsonIgnore]
        public bool IsSelectedForComparison
        {
            get => _isSelectedForComparison;
            set
            {
                _isSelectedForComparison = value;
                OnPropertyChanged();
            }

        }
        [JsonIgnore]
        public abstract Roles Type { get; }
        public RankV2 Rank { get; set; }
        public Dictionary<StatisticType, float> Stats { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }

    public class Tank : RoleV2
    {
        public override Roles Type => Roles.Tank;
    }

    public class Support : RoleV2
    {
        public override Roles Type => Roles.Support;
    }

    public class Damage : RoleV2
    {
        public override Roles Type => Roles.Damage;
    }

    #endregion

    #region Ranks

    public class RankV2
    {
        public int Tier { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public Division Division { get; set; }

        [JsonIgnore]
        public int SkillRating { get; set; }

        public static RankV2 RankFromSr(int sr)
        {
            if (sr < 0) sr = 0;
            if (sr >= 5000) sr = 4999;

            Division division = Division.Bronze;
            int remainder = 0;
            if (sr < 400)
                remainder = 1;
            foreach (Division div in Enum.GetValues(typeof(Division)))
            {
                if (sr < (int)div)
                    break;
                division = div;
            }

            int baseDivRank = (int)division;
            remainder = Math.Min(sr - (division == Division.Bronze ? 1000 : baseDivRank), 499);

            int tier = Math.Min(5,  5 - remainder / 100 );

            return new RankV2()
            {
                Division = division,
                SkillRating = sr,
                Tier = tier
            };
        }

        public static RankV2 RankFromDivision(string divisionString, int tier)
        {
            if (!Enum.TryParse(divisionString, true, out Division division))
                throw new ArgumentException("Division was not an accepted string");

            if (tier < 1 || tier > 5)
                throw new ArgumentException("Tier must be between 1 and 5");

            int baseSr = (int)division;
            int remainder = (5 - tier) * 100;

            int sr = baseSr + remainder;

            return new RankV2()
            {
                SkillRating = sr,
                Division = division,
                Tier = tier
            };
        }
    }
    #endregion

    public enum Roles
    {
        Tank,
        Support,
        Damage
    }

    public enum Division
    {
        Bronze = 0,
        Silver = 1500,
        Gold = 2000,
        Platinum = 2500,
        Diamond = 3000,
        Master = 3500,
        Grandmaster = 4000,
        Champion = 4500
    }

    public enum StatisticType
    {
        Damage,
        Deaths,
        Elims,
        Healing,
        TimePlayed,
        Winrate,
    }
}
