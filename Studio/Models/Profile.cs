

using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Studio.Models
{

    #region Profile

    public class Profile
    {
        public BattleTag Battletag { get; set; }
        public string BattletagString => Battletag?.ToString();
        public string CustomId { get; set; }
        public string Email { get; set; }
        public string Avatar { get; set; }
        public int LastUpdate { get; set; }
        public int TimesSwitched { get; set; }
        public int TimesLaunched { get; set; }

        public RankedCareer RankedCareer { get; set; }

    }

    public class BattleTag
    {
        public string FullTag => ToString();
        public string Username { get; set; }
        public string Tag { get; set; }

        [JsonConstructor]
        public BattleTag(string username, string tag)
        {
            Username = username;
            Tag = tag;
        }
        public BattleTag(string battletag)
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

    #region Statistics

    public class Statistic
    {
        public StatisticType Name { get; set; }
        public double Value { get; set; }
        public double ValuePer10 { get; set; }
        public double ScaledValuePer10 { get; set; }

        public void ScaleToRole(Role role) =>
            ScaledValuePer10 = role.Scalars[StatisticType.Assists] * ValuePer10;

    }
    public class StatCollection
    {

        public Dictionary<StatisticType, Statistic> Stats;
        public void ScaleToRole(Role role)
        {
            foreach (var stat in Stats)
            {
                stat.Value.ScaleToRole(role);
            }
        }

    }
    #endregion

    #region Roles
    public class RankedCareer
    {
        public Tank Tank { get; set; }
        public Support Support { get; set; }
        public Damage Damage { get; set; }
    }

    public abstract class Role : INotifyPropertyChanged
    {
        private bool _isSelectedForComparison;
        public bool IsSelectedForComparison
        {
            get => _isSelectedForComparison;
            set
            {
                _isSelectedForComparison = value;
                OnPropertyChanged();
            }

        }
        public abstract Roles Type { get; }
        public abstract List<RankMoment> RankMoments { get; set; }
        public abstract RankMoment PeakRank { get; set; }
        public abstract Rank CurrentRank { get; set; }

        [JsonIgnore]
        public abstract StatCollection StatCollection { get; set; }

        [JsonIgnore]
        public static Dictionary<StatisticType, float> StatScalars { get; set; }

        [JsonIgnore]
        public Dictionary<StatisticType, float> Scalars => ScalarCollection[Type];

        [JsonIgnore]
        public static Dictionary<Roles, Dictionary<StatisticType, float>> ScalarCollection = new()
        {
            { Roles.Tank, new Dictionary<StatisticType, float>
                {
                    { StatisticType.Damage, 0.002f },
                    { StatisticType.Assists, 0.004f },
                    { StatisticType.FinalBlows, 1.0f },
                    { StatisticType.TimePlayed, 1.0f },
                    { StatisticType.Healing, 1.0f },
                    { StatisticType.Deaths, 1.0f }
                }
            },
            { Roles.Damage, new Dictionary<StatisticType, float>
                {
                    { StatisticType.Damage, 0.004f },
                    { StatisticType.Assists, 0.004f },
                    { StatisticType.FinalBlows, 1.0f },
                    { StatisticType.TimePlayed, 1.0f },
                    { StatisticType.Healing, 0.00167f },
                    { StatisticType.Deaths, 1.0f }
                }
            },
            { Roles.Support, new Dictionary<StatisticType, float>
                {
                    { StatisticType.Damage, 0.002f },
                    { StatisticType.Assists, 0.004f },
                    { StatisticType.FinalBlows, 1.0f },
                    { StatisticType.TimePlayed, 1.0f },
                    { StatisticType.Healing, 0.00333f },
                    { StatisticType.Deaths, 1.0f }
                }
            },
        };

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }

    public class Tank : Role
    {
        public override Roles Type => Roles.Tank;
        public override List<RankMoment> RankMoments { get; set; }
        public override RankMoment PeakRank { get; set; }
        public override Rank CurrentRank { get; set; }
        public override StatCollection StatCollection { get; set; }


    }

    public class Support : Role
    {
        public override Roles Type => Roles.Support;
        public override List<RankMoment> RankMoments { get; set; }
        public override RankMoment PeakRank { get; set; }
        public override Rank CurrentRank { get; set; }
        public override StatCollection StatCollection { get; set; }
    }

    public class Damage : Role
    {
        public override Roles Type => Roles.Damage;
        public override List<RankMoment> RankMoments { get; set; }
        public override RankMoment PeakRank { get; set; }
        public override Rank CurrentRank { get; set; }
        public override StatCollection StatCollection { get; set; }
    }

    #endregion

    #region Ranks
    public class RankMoment
    {
        public Rank Rank { get; set; }
        public int Date { get; set; }
    }

    public class Rank
    {
        public int SkillRating { get; set; }
        public int Tier { get; set; }
        public Division Division { get; set; }


        public static Rank RankFromSr(int sr)
        {
            if (sr < 0) sr = 0;
            if (sr >= 5000) sr = 4999;

            Division division = Division.Bronze;
            int remainder = 0;

            foreach (Division div in Enum.GetValues(typeof(Division)))
            {
                if (sr < (int)div)
                    break;
                division = div;
            }

            int baseDivRank = (int)division;
            remainder = sr - baseDivRank;

            int tier = Math.Min(5,  5 - remainder / 100 );

            return new Rank()
            {
                Division = division,
                SkillRating = sr,
                Tier = tier
            };
        }

        public static Rank RankFromDivision(string divisionString, int tier)
        {
            if (!Enum.TryParse(divisionString, true, out Division division))
                throw new ArgumentException("Division was not an accepted string");

            if (tier < 1 || tier > 5)
                throw new ArgumentException("Tier must be between 1 and 5");

            int baseSr = (int)division;
            int remainder = (5 - tier) * 100;

            int sr = baseSr + remainder;

            return new Rank()
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
        Bronze = 1000,
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
        Assists,
        Deaths,
        TimePlayed,
        FinalBlows,
        Healing
    }
}
