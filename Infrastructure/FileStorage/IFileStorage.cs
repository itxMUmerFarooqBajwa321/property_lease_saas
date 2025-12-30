public interface IFileStorage
{
    Task<string> SaveAsync(IFormFile file, string folder);
}
