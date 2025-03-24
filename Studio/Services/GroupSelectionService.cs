using Studio.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Studio.Services
{
    public class GroupSelectionService : INotifyPropertyChanged
    {
        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set { _isEnabled = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Role> GroupRoles { get; set; } = new();

        public Range Range => new Range(MinimumSr, MaximumSr);

        public int MinimumSr
        {
            get
            {
                if (highestMemberSr == -1)
                    return 0;
                if (isChampionPresent)
                    return highestMemberSr - 300;
                else 
                    return highestMemberSr - 500;
            }
        }

        public int MaximumSr
        {
            get
            {
                if (highestMemberSr == -1)
                    return 5000;
                if (isChampionPresent)
                    return highestMemberSr + 300;
                else
                    return highestMemberSr + 500;
            }
        }

        private int highestMemberSr = -1;
        private int lowestMemberSr = -1;
        private bool isChampionPresent;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public bool Contains(Role role)
        {
            return GroupRoles.Contains(role);
        }
        public void AddMember(Role member)
        {
            GroupRoles.Add(member);
            member.IsSelectedForComparison = true;
            int sr = member.CurrentRank.SkillRating;

            if (sr < lowestMemberSr || lowestMemberSr == -1)
                lowestMemberSr = sr;

            if (sr > highestMemberSr || highestMemberSr == -1)
                highestMemberSr = sr;

            if (sr >= (int)Division.Champion && !isChampionPresent)
                isChampionPresent = true;
            OnPropertyChanged(nameof(Range));
        }

        public void RemoveMember(Role member)
        {
            if (GroupRoles.Count == 1)
            {
                RemoveAllMembers();
                return;
            }
            member.IsSelectedForComparison = false;
            GroupRoles.Remove(member);
            int minSr = 5000;
            int maxSr = 0;
            foreach (Role role in GroupRoles)
            {
                int sr = role.CurrentRank.SkillRating;
                if (sr > maxSr) maxSr = sr;
                if (sr <  minSr) minSr = sr;
                if (sr >= (int)Division.Champion) isChampionPresent = true;
            }
            lowestMemberSr = minSr;
            highestMemberSr = maxSr;
            OnPropertyChanged(nameof(Range));
        }

        public void RemoveAllMembers()
        {
            int memberCount = GroupRoles.Count;
            for (int i = 0; i < memberCount; i++)
            {
                Role role = GroupRoles[0];
                role.IsSelectedForComparison = false;
                GroupRoles.RemoveAt(0);
            }

            highestMemberSr = -1;
            lowestMemberSr = -1;
            OnPropertyChanged(nameof(Range));
        }
    }
}
