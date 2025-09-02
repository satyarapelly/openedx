// <copyright file="ValidationStatus.cs" company="Microsoft">Copyright (c) Microsoft 2016. All rights reserved.</copyright>
namespace PidlTest.Diff
{
    using JsonDiff;

    public class ValidationStatus
    {
        public ValidationStatus(bool isValidated, DiffDetails validationDetails)
        {
            this.IsValidated = isValidated;
            this.ValidationDetails = validationDetails;
        }

        public ValidationStatus(bool isValidated) : this(isValidated, new DiffDetails())
        {
        }

        public bool IsValidated { get; set; }

        public DiffDetails ValidationDetails { get; set; }
    }
}
