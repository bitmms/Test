using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MyMVVM.Speak.Model
{
    public class SpeakGroupVO // : INotifyPropertyChanged
    {
        public string groupId { get; set; }
        public string groupName { get; set; }
        public string groupOwnerNumber { get; set; }
        public string groupOwnerName { get; set; }
        public int groupMemberCount { get; set; }
        public string groupOwnerNameAndNumber { get; set; }
        public string groupOwnerNameAndCount { get; set; }

        /*public string groupId
        {
            get => _groupId;
            set
            {
                if (_groupId != value)
                {
                    _groupId = value;
                    OnPropertyChanged();
                }
            }
        }


        public string groupName
        {
            get => _groupName;
            set
            {
                if (_groupName != value)
                {
                    _groupName = value;
                    OnPropertyChanged();
                }
            }
        }


        public string groupOwnerNumber
        {
            get => _groupOwnerNumber;
            set
            {
                if (_groupOwnerNumber != value)
                {
                    _groupOwnerNumber = value;
                    OnPropertyChanged();
                }
            }
        }


        public string groupOwnerName
        {
            get => _groupOwnerName;
            set
            {
                if (_groupOwnerName != value)
                {
                    _groupOwnerName = value;
                    OnPropertyChanged();
                }
            }
        }


        public int groupMemberCount
        {
            get => _groupMemberCount;
            set
            {
                if (_groupMemberCount != value)
                {
                    _groupMemberCount = value;
                    OnPropertyChanged();
                }
            }
        }


        public string groupOwnerNameAndNumber
        {
            get => _groupOwnerNameAndNumber;
            set
            {
                if (_groupOwnerNameAndNumber != value)
                {
                    _groupOwnerNameAndNumber = value;
                    OnPropertyChanged();
                }
            }
        }


        public string groupOwnerNameAndCount
        {
            get => _groupOwnerNameAndCount;
            set
            {
                if (_groupOwnerNameAndCount != value)
                {
                    _groupOwnerNameAndCount = value;
                    OnPropertyChanged();
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }*/

    }
}
