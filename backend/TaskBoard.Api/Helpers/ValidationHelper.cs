using System.ComponentModel.DataAnnotations;

namespace TaskBoard.Api.Helpers
{
    public static class ValidationHelper
    {
        public static IResult? Validate(object model)
        {
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(model);

            if (Validator.TryValidateObject(model, context, validationResults, validateAllProperties: true))
                return null;

            var errors = validationResults
            .GroupBy(r => r.MemberNames.FirstOrDefault() ?? string.Empty)
            .ToDictionary(
                g => g.Key,
                g => g.Select(r => r.ErrorMessage ?? string.Empty).ToArray());

            return Results.ValidationProblem(errors);
        }
    }
}
