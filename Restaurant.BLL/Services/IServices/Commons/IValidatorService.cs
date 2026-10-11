namespace Restaurant.BLL.Services.IServices.Commons;

public interface IValidatorService<T> where T : class
{
    bool IsCodeExist(int id, string code);
    bool IsArNameExist(int id, string arName);
    bool IsEnNameExist(int id, string enName);
    bool IsPhoneNumberExist(int id, string phoneNumber);
    bool IsFullNameExist(int id, string firstName, string lastName);
    bool IsValueExist(int id, string propertyName, string value);
}
