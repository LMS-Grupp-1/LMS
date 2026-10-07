using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Infrastructure.Repositories;
public class CourseRepository(ApplicationDbContext context) : ICourseRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<Course>> GetAllAsync(bool trackChanges) =>
        !trackChanges
            ? await _context.Courses.AsNoTracking().ToListAsync()
            : await _context.Courses.ToListAsync();

    public async Task<Course?> GetByIdAsync(int id, bool trackChanges) =>
        !trackChanges
            ? await _context.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id)
            : await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);

    public void CreateCourse(Course course) => _context.Courses.Add(course);

    public void UpdateCourse(Course course) => _context.Courses.Update(course);

    public void DeleteCourse(Course course) => _context.Courses.Remove(course);

    public async Task SaveAsync() => await _context.SaveChangesAsync();
}
