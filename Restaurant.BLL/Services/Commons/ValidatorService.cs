using Restaurant.BLL.Services.IServices.Commons;
using Restaurant.DAL.IRepositories.Commons;

namespace Restaurant.BLL.Services.Commons;

public class ValidatorService<T> : IValidatorService<T> where T : class
{
    private readonly IValidatorRepository<T> _repository;

    public ValidatorService(IValidatorRepository<T> repository)
    {
        _repository = repository;
    }

    public bool IsCodeExist(int id, string code)
    {
        return _repository.IsCodeExist(id, code);
    }

    public bool IsArNameExist(int id, string arName)
    {
        return _repository.IsArNameExist(id, arName);
    }

    public bool IsEnNameExist(int id, string enName)
    {
        return _repository.IsEnNameExist(id, enName);
    }

    public bool IsPhoneNumberExist(int id, string phoneNumber)
    {
        return _repository.IsPhoneNumberExist(id, phoneNumber);
    }

    public bool IsValueExist(int id, string propertyName, string value)
    {
        return _repository.IsValueExist(id, propertyName, value);
    }
}
