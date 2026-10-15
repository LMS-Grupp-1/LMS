namespace Domain.Contracts;

public interface IBaseRepository<T>
{
    Task<T?> GetByIdAsync(int id);
    Task AddAsync(T model);
    void Update(T model);
    void Delete(T model);
}