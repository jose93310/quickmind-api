using QuickMind.Domain.Entities;

namespace QuickMind.Domain.Repositories;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<IEnumerable<Category>> GetByAgeGroupAsync(AgeGroup ageGroup);
    Task<IEnumerable<Category>> GetByIdsAsync(List<int> ids);
}
