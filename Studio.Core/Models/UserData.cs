using System;
using System.Collections.Generic;
using System.Text;

namespace Studio.Core.Models
{
    public class UserData
    {
        public string CustomID { get; set; }
        public string Username { get; set; }
        public string Tag { get; set; }
        public string Battletag => $"{Username}#{Tag}";
        public string Email { get; set; }
        public string Avatar { get; set; }
        public int Last_update { get; set; }
        public int Times_switched { get; set; }
        public int Times_launched { get; set; }
        public RankHistory Rank_history { get; set; }

        public RankedCareer RankedCareer { get; set; }


    }

    public class RankHistory
    {
        public Dictionary<int, int> Tank { get; set; }
        public Dictionary<int, int> Damage { get; set; }
        public Dictionary<int, int> Support { get; set; }
        public _HighestRoles Highest { get; set; }
        public _CurrentRoles Current { get; set; }

    }

    public class _CurrentRoles
    {
        public int Tank { get; set; }
        public int Damage { get; set; }
        public int Support { get; set; }
    }

    public class _HighestRoles
    {
        public _Role Tank { get; set; }
        public _Role Damage { get; set; }
        public _Role Support { get; set; }
    }

    public class _Role
    {
        public int Rating { get; set; }
        public int Date { get; set; }
    }

    public class RankedCareer
    {
        public Tank Tank { get; set; }
        public Support Support { get; set; }
        public Damage Damage { get; set; }
    }

    public abstract class Role
    {
        public abstract Roles Type { get; }
        public abstract List<RankMoment> RankMoments { get; set; }
        public abstract RankMoment PeakRank { get; set; }
        public abstract Rank CurrentRank { get; set; }
    }

    public class Tank : Role
    {
        public override Roles Type => Roles.Tank;
        public override List<RankMoment> RankMoments { get; set; }
        public override RankMoment PeakRank { get; set; }
        public override Rank CurrentRank { get; set; }
    }

    public class Support : Role
    {
        public override Roles Type => Roles.Support;
        public override List<RankMoment> RankMoments { get; set; }
        public override RankMoment PeakRank { get; set; }
        public override Rank CurrentRank { get; set; }
    }

    public class Damage : Role
    {
        public override Roles Type => Roles.Damage;
        public override List<RankMoment> RankMoments { get; set; }
        public override RankMoment PeakRank { get; set; }
        public override Rank CurrentRank { get; set; }
    }



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

        public static Rank ___RankFromSR(int sr)
        {
            double flr = ((double)sr - 1000) / 500;
            int x = (int)(flr - flr % 1);
            //return $"{_divs[x]} {5 - (sr % 500) / 100}";
            Rank rank = new Rank()
            {
                SkillRating = sr,
                Division = (Division)x,
                Tier = 5 - (sr % 500) / 100,
            };

            return rank;
        }

        public static Rank RankFromSR(int sr)
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

            int tier = 5 - remainder / 100;

            return new Rank()
            {
                Division = division,
                SkillRating = sr,
                Tier = tier
            };
        }

        public static Rank RankFromDivision(string divisionString, int tier)
        {
            if (!Enum.TryParse(divisionString, out Division division))
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
}
