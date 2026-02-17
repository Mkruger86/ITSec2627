using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace FlatFileRepo
{
    public class PersonModel
    {
        private bool _enabled = false;
        public PersonModel(int person_Id, string? first_Name,
            string? last_Name, string address, int street_Number,
            string password, bool enabled)
        {
            Person_Id = person_Id;
            First_Name = first_Name;
            Last_Name = last_Name;
            Address = address;
            Street_Number = street_Number;
            Password = password;
            Enabled = enabled;
        }

        public int Person_Id { get; set; }
        public string? First_Name { get; set; }
        public string? Last_Name { get; set; }
        public string Address { get; set; }
        public int Street_Number { get; set; }
        public string Password { get; set; }
        public bool Enabled { get; set; }

        public void ValidateName()
        {
            if (string.IsNullOrWhiteSpace(First_Name))
            {
                throw new ArgumentException("First name cannot be empty.");
            }
            if (string.IsNullOrWhiteSpace(Last_Name))
            {
                throw new ArgumentException("Last name cannot be empty.");
            }

            if (First_Name.Length > 25 || Last_Name.Length > 25 || First_Name.Length < 1 || Last_Name.Length > 25)
            {
                throw new ArgumentException("Invalid name");
            }
        }

        public void ValidateAddress()
        {
            if (string.IsNullOrWhiteSpace(Address))
            {
                throw new ArgumentException("Address cannot be empty.");
            }
            if (Street_Number <= 0)
            {
                throw new ArgumentException("Street number must be greater than zero.");
            }
        }

        public void ValidatePassword()
        {
            if (string.IsNullOrWhiteSpace(Password))
            {
                throw new ArgumentException("Password cannot be empty.");
            }
            
            const string pattern = @"^(?=.*[A-Za-z])(?=.*\d).{6,}$";
            if (!Regex.IsMatch(Password, pattern))
            {
                 throw new ArgumentException("Password must be at least 6 characters long and contain both letters and numbers.");
            }
        }

        public void ValidateEnabled()
        {
            if (Enabled != true && Enabled != false)
            {
                throw new ArgumentException("Enabled must be either true or false.");
            }
        }

        public void ValidatePerson()
        {
            ValidateName();
            ValidateAddress();
            ValidatePassword();
            ValidateEnabled();
        }
    }
}
