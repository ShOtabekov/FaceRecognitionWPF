using System;
using System.ComponentModel;

namespace FaceDetectionAndRecognition.ViewModels
{
    public class FoundedUserViewModel : INotifyPropertyChanged
    {
        public FoundedUserViewModel(string firstName, string lastName, DateTime brithday, string phoneNumber, string address)
        {
            FirstName = firstName;
            LastName = lastName;
            FullName = $"{lastName} {firstName}";
            Birthday = brithday;
            PhoneNumber = phoneNumber;
            Address = address;
        }
        public string FirstName
        {
            get;
        }
        public string LastName
        {
            get;
        }
        public DateTime Birthday
        {
            get;
        }
        public string PhoneNumber
        {
            get;
        }
        public string Address
        {
            get;
        }
        public string FullName { get; }

        #region Notify

        /// <summary>
        /// Property Changed event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Fire the PropertyChanged event
        /// </summary>
        /// <param name="propertyName">Name of the property that changed (defaults from CallerMemberName)</param>
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
