using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using XPLUG.WEBTOOLS;

namespace NewVPlusSales.Common
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class PropertiesMustMatchAttribute : ValidationAttribute
    {
        private const string DefaultErrorMessage = "'{0}' and '{1}' do not match.";

        private readonly object _typeId = new object();

        public PropertiesMustMatchAttribute(string originalProperty, string confirmProperty)
            : base(DefaultErrorMessage)
        {
            OriginalProperty = originalProperty;
            ConfirmProperty = confirmProperty;
        }

        public string ConfirmProperty
        {
            get;
            private set;
        }

        public string OriginalProperty
        {
            get;
            private set;
        }

        public override object TypeId
        {
            get
            {
                return _typeId;
            }
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentUICulture, ErrorMessageString,
                OriginalProperty, ConfirmProperty);
        }

        public override bool IsValid(object value)
        {
            if (value == null) { return false; }
            var properties = TypeDescriptor.GetProperties(value);
            var originalValue = properties.Find(OriginalProperty, true /* ignoreCase */).GetValue(value);
            var confirmValue = properties.Find(ConfirmProperty, true /* ignoreCase */).GetValue(value);
            return Equals(originalValue, confirmValue);
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ValidatePasswordLengthAttribute : ValidationAttribute
    {
        private const string DefaultErrorMessage = "'{0}' must be at least {1} character length.";

        private readonly int _minCharacters = 8;

        public ValidatePasswordLengthAttribute()
            : base(DefaultErrorMessage)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentUICulture, ErrorMessageString,
                name, _minCharacters);
        }

        public override bool IsValid(object value)
        {
            var valueAsString = value as string;
            return (valueAsString != null && valueAsString.Length >= _minCharacters);
        }
    }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class OldEnoughValidationAttribute : ValidationAttribute
    {
        public int LimitAge { get; set; }
        public OldEnoughValidationAttribute(int limitAge)
        {
            LimitAge = limitAge;
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            int val = (int)value;

            if (val >= LimitAge)
                return ValidationResult.Success;
            else
                return new ValidationResult(ErrorMessageString);
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CheckAttribute : ValidationAttribute
    {
        object[] ValidValues;

        public CheckAttribute(params object[] validValues)
        {
            ValidValues = validValues;
        }

        public override bool IsValid(object value)
        {
            return ValidValues.FirstOrDefault(v => v.Equals(value)) != null;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CheckPasscodeAttribute : ValidationAttribute
    {
        readonly string _compareValue;

        public CheckPasscodeAttribute()
        {

        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {

            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return new ValidationResult(ErrorMessageString);
            }
            if (value.ToString().Length != 8)
            {
                return new ValidationResult("Invalid Passcode! Passcode must 8 digits");
            }
            if (!DataCheck.IsNumeric(value.ToString()))
            {
                return new ValidationResult("Invalid Passcode! Passcode must be numeric");
            }
            if (int.Parse(value.ToString()) < 1)
            {
                return new ValidationResult("Invalid Passcode! Passcode cannot be all-zero");
            }
            return ValidationResult.Success;
        }
    }


    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CheckAgreeTermAttribute : ValidationAttribute
    {

        public CheckAgreeTermAttribute()
        {

        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return new ValidationResult("You must agree to the terms and conditions");
            }
            if (value is bool)
            {
                return bool.Parse(value.ToString()) ? ValidationResult.Success : new ValidationResult("You must agree to the terms and conditions");
            }
            return new ValidationResult("You must agree to the terms and conditions");
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CheckNumberAttribute : ValidationAttribute
    {
        readonly int _compareValue;

        public CheckNumberAttribute(int compareValue)
        {
            _compareValue = compareValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is int i)
            {
                return i > _compareValue ? ValidationResult.Success : new ValidationResult(ErrorMessageString);
            }
            if (value is long l)
            {
                return l > _compareValue ? ValidationResult.Success : new ValidationResult(ErrorMessageString);
            }
            if (value is double d)
            {
                return d > _compareValue ? ValidationResult.Success : new ValidationResult(ErrorMessageString);
            }
            return new ValidationResult(ErrorMessageString);
        }
    }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CheckNameAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var name = value as string;
            if (string.IsNullOrEmpty(name))
            {
                return new ValidationResult(ErrorMessageString);
            }
            return RegExValidation.IsNameValid(name) ? ValidationResult.Success : new ValidationResult(ErrorMessageString);

        }
    }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CheckMobileNumberAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var name = value as string;
            if (string.IsNullOrEmpty(name))
            {
                return ValidationResult.Success;
            }
            return GSMHelper.ValidateNaijaMobileNumber(name) ? ValidationResult.Success : new ValidationResult(ErrorMessageString);

        }
    }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CheckAgeAttribute : ValidationAttribute
    {
        readonly int _compareValue;

        public CheckAgeAttribute(int compareValue)
        {
            _compareValue = compareValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            try
            {
                var valueToCheck = value.ToString();
                if (string.IsNullOrEmpty(valueToCheck))
                {
                    return new ValidationResult(ErrorMessageString);
                }
                DateTime valueDateTime;
                DateTime.TryParse(valueToCheck, out valueDateTime);
                if (valueDateTime.Equals(null))
                {
                    return new ValidationResult(ErrorMessageString);
                }

                var age = DateTime.Today.Year - valueDateTime.Year;
                return age >= _compareValue ? ValidationResult.Success : new ValidationResult(ErrorMessageString);
            }
            catch
            {
                return new ValidationResult(ErrorMessageString);
            }

        }
    }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CheckAmountAttribute : ValidationAttribute
    {
        readonly float _compareValue;

        public CheckAmountAttribute(float compareValue)
        {
            _compareValue = compareValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is float)
            {
                return (float)value >= _compareValue ? ValidationResult.Success : new ValidationResult(ErrorMessageString);
            }
            if (value is double)
            {
                return (double)value >= _compareValue ? ValidationResult.Success : new ValidationResult(ErrorMessageString);
            }
            return new ValidationResult(ErrorMessageString);
        }
    }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CheckTransferCodeAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var name = value as string;
            if (string.IsNullOrEmpty(name))
            {
                return ValidationResult.Success;
            }
            return RegExValidation.IsTransferCodeValid(name) ? ValidationResult.Success : new ValidationResult(ErrorMessageString);

        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CheckCustomerCodeAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var name = value as string;
            if (string.IsNullOrEmpty(name))
            {
                return ValidationResult.Success;
            }
            return RegExValidation.IsCustomerCodeValid(name) ? ValidationResult.Success : new ValidationResult(ErrorMessageString);

        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CheckAccountNumberAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var name = value as string;
            if (string.IsNullOrEmpty(name))
            {
                return ValidationResult.Success;
            }
            return RegExValidation.IsAccountNumberValid(name) ? ValidationResult.Success : new ValidationResult(ErrorMessageString);

        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CheckEmailAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var name = value as string;
            if (string.IsNullOrEmpty(name))
            {
                return ValidationResult.Success;
            }
            return RegExValidation.IsEmailValid(name) ? ValidationResult.Success : new ValidationResult(ErrorMessageString);

        }
    }
}
