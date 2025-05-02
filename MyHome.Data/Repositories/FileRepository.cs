using MyHome.Core.Interfaces;
using MyHome.Core.Models.Entities;
using MyHome.Core.Models.Exceptions;
using System.Text.Json;

namespace MyHome.Data.Repositories;

public class FileRepository<T> : IRepository<T> where T : IEntity, new()
{
    private readonly string _filePath;

    public FileRepository()
    {
        var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var appDataFolder = Path.Combine(homePath, ".myhomeapp");

        Directory.CreateDirectory(appDataFolder); 

        _filePath = Path.Combine(appDataFolder, $"{typeof(T).Name}.json");

        if (!File.Exists(_filePath))
        {
            File.Create(_filePath).Dispose();
        }
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        try
        {
            var jsonString = await File.ReadAllTextAsync(_filePath);
            if (string.IsNullOrEmpty(jsonString))
            {
                return [];
            }

            return JsonSerializer.Deserialize<IEnumerable<T>>(jsonString) ?? [];
        }
        catch (FileNotFoundException)
        {
            return [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    public async Task<T> GetByIdAsync(Guid id)
    {
        var allItems = await GetAllAsync();

        var result = allItems.FirstOrDefault(item => item.Id == id);

        result ??= new T();
        result.Id = id;

        return result;
    }

    public async Task AddAsync(T entity)
    {
        var allItems = await GetAllAsync();
        if (allItems.Any(item => item.Id == entity.Id))
        {
            throw new DuplicateException(entity.Id);
        }

        var updatedItems = allItems.Append(entity);
        await SaveToFileAsync(updatedItems);
    }

    public async Task UpdateAsync(T entity)
    {
        var allItems = await GetAllAsync();
        var existingItem = allItems.FirstOrDefault(item => item.Id == entity.Id) 
            ?? throw new NotFoundException<T>(entity.Id);
        
        var updatedItems = allItems.Where(item => item.Id != entity.Id).Append(entity);
        await SaveToFileAsync(updatedItems);
    }

    public async Task DeleteAsync(Guid id)
    {
        var allItems = await GetAllAsync();
        if (!allItems.Any(item => item.Id == id))
        {
            throw new NotFoundException<T>(id);
        }
        var updatedItems = allItems.Where(item => item.Id != id);
        await SaveToFileAsync(updatedItems);
    }

    public async Task UpsertAsync(T entity)
    {
        var allItems = await GetAllAsync();
        if (allItems.Any(item => item.Id == entity.Id))
        {
            await UpdateAsync(entity);
        }
        else
        {
            await AddAsync(entity);
        }
    }

    private async Task SaveToFileAsync(IEnumerable<T> items)
    {
        var jsonString = JsonSerializer.Serialize(items);
        await File.WriteAllTextAsync(_filePath, jsonString);
    }
}
