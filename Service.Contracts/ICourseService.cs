using LMS.Shared.DTOs.CourseDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Service.Contracts;
    public interface ICourseService
    {
        Task<IEnumerable<CourseDto>> GetAllCoursesAsync();
        Task<CourseDto?> GetCourseByIdAsync(int id);
        Task<CourseDto> CreateCourseAsync(CourseCreateDto courseCreateDto);
        Task UpdateCourseAsync(int id, CourseUpdateDto courseUpdateDto);
    }
