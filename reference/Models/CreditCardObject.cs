using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace proCabWeb.Models
{
    public class CreditCardObject
    {
        private string _CreditCardNumber;
        private string _ExpirationDate;
        private string _CVVCode;
        private short _IntervalLength;

        private string _firstName;
        private string _lastName;
        private string _address;
        private string _city;
        private string _zip;

        private decimal _amount;


        public string CreditCardNumber
        {
            get
            {
                return _CreditCardNumber;
            }
            set
            {
                _CreditCardNumber = value;
            }
        }

        // MMYY
        public string ExpirationDate
        {
            get
            {
                return _ExpirationDate;
            }
            set
            {
                _ExpirationDate = value;
            }
        }

        public string CVVCode
        {
            get
            {
                return _CVVCode;
            }
            set
            {
                _CVVCode = value;
            }
        }

        public short IntervalLength
        {
            get
            {
                return _IntervalLength;
            }
            set
            {
                _IntervalLength = value;
            }
        }


        public string FirstName
        {
            get
            {
                return _firstName;
            }
            set
            {
                _firstName = value;
            }
        }
        public string LastName
        {
            get
            {
                return _lastName;
            }
            set
            {
                _lastName = value;
            }
        }
        public string Address
        {
            get
            {
                return _address;
            }
            set
            {
                _address = value;
            }
        }
        public string City
        {
            get
            {
                return _city;
            }
            set
            {
                _city = value;
            }
        }
        public string Zip
        {
            get
            {
                return _zip;
            }
            set
            {
                _zip = value;
            }
        }


        public decimal Amount
        {
            get
            {
                return _amount;
            }
            set
            {
                _amount = value;
            }
        }
    }
}