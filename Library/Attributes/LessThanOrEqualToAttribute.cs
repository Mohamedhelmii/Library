using System.ComponentModel.DataAnnotations;

public class LessThanOrEqualToAttribute : ValidationAttribute
{
    private readonly string _comparisonProperty;

    // constructor اللي هيستقبل اسم الخاصية التانية اللي هتقارن بيها
    public LessThanOrEqualToAttribute(string comparisonProperty)
    {
        _comparisonProperty = comparisonProperty;
    }

    // الميثود اللي هتعمل التحقق الفعلي
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        // 1. جلب قيمة الخاصية الحالية (AvailableCopies)
        var currentValue = (int)value;

        // 2. جلب قيمة الخاصية اللي هتقارن بيها (TotalCopies)
        var comparisonValueProperty = validationContext.ObjectType.GetProperty(_comparisonProperty);
        if (comparisonValueProperty == null)
        {
            throw new ArgumentException("Comparison property not found.");
        }
        var comparisonValue = (int)comparisonValueProperty.GetValue(validationContext.ObjectInstance);

        // 3. التحقق الفعلي
        if (currentValue <= comparisonValue)
        {
            // لو الشرط تحقق، يبقى التحقق ناجح
            return ValidationResult.Success;
        }
        else
        {
            // لو الشرط فشل، نرجع رسالة الخطأ
            // {0} هو اسم الخاصية الحالية (AvailableCopies)
            // {1} هو اسم الخاصية المقارنة (TotalCopies)
            return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} must be less than or equal to {comparisonValueProperty.Name}.");
        }
    }
}