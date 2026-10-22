using Domain.Contracts;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Infrastructure.Repositories;

public abstract class BaseRepository<T> : IBaseRepository<T>
    where T : class
{
    protected readonly ApplicationDbContext _context;

    protected BaseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    protected DbSet<T> Set => _context.Set<T>();

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await Set.FindAsync(id);
    }

    public async Task AddAsync(T model)
    {
        await Set.AddAsync(model);
    }

    public void Update(T model)
    {
        Set.Update(model);
    }

    public void Delete(T model)
    {
        Set.Remove(model);
    }
}