namespace GenericCrud.Models.Dynamic
{
    // Every input type the shared _DynamicForm partial knows how to render.
    public enum DynamicInputType
    {
        Text,
        TextArea,
        Number,
        Decimal,
        Date,
        Time,
        Dropdown,
        Radio,
        Checkbox,
        File,
        Email,
        Phone,
        Password,
        Color,
        ReadOnly,
        Hidden
    }
}
