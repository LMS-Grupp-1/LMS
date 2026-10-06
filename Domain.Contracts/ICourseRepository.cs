using Domain.Models.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Contracts;
    public interface ICourseRepository
    {
        Task<IEnumerable<Course>> GetAllAsync(bool trackChanges);
        Task<Course?> GetByIdAsync(int id, bool trackChanges);
        void CreateCourse(Course course);
        void UpdateCourse(Course course);
        void DeleteCourse(Course course);
        Task SaveAsync();
    }
