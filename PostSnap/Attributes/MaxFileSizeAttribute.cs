using System.ComponentModel.DataAnnotations;

namespace PostSnap.Attributes
{
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxFileSizeBytes;

        // Constructor: take the size in bytes (e.g., 2MB = 2 * 1024 * 1024
        public MaxFileSizeAttribute(int maxFileSizeBytes)
        {
            _maxFileSizeBytes = maxFileSizeBytes;
        }

        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                if(file.Length > _maxFileSizeBytes)
                {
                    return new ValidationResult($"Maximum allowed file size is {_maxFileSizeBytes / 1024 / 1024} MB.");
                }
            }

            return ValidationResult.Success!;
        }
    }
}
