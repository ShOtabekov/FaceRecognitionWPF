using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FaceDetectionAndRecognition.Model
{
    [Table(nameof(User))]
    public class User
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime Birthday { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
    }
}
