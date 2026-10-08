using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Shared.DTOs;
using LMS.Shared.DTOs.ModulesDtos;
using Service.Contracts;

namespace LMS.Services;

public class ModuleService : IModuleService
{
    private readonly IUnitOfWork _unitOfWork;

    public ModuleService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ModuleDto>> GetModulesByCourseIdAsync(int courseId)
    {
        var modules = await _unitOfWork.Modules.GetModulesByCourseIdAsync(courseId);

        return modules.Select(m => new ModuleDto
        {
            Id = m.Id,
            Name = m.Name,
            Description = m.Description,
            StartDate = m.StartDate,
            EndDate = m.EndDate,
            CourseId = m.CourseId
        });
    }

    public async Task<(bool Success, string ErrorMessage)> CreateModuleAsync(int courseId, CreateModuleDto dto)
    {
        if (dto.StartDate >= dto.EndDate)
        {
            return (false, "Modulens startdatum måste vara före dess slutdatum.");
        }

        var course = await _unitOfWork.Courses.GetByIdAsync(courseId, false);
        if (course == null)
        {
            return (false, "Den valda kursen hittades inte.");
        }

        // Module dates must be within course dates
        if (dto.StartDate < course.StartDate || dto.EndDate > course.EndDate)
        {
            return (false, $"Modulens datum måste ligga inom kursens tidsram ({course.StartDate.ToShortDateString()} - {course.EndDate.ToShortDateString()}).");
        }

        var existingModules = await _unitOfWork.Modules.GetModulesByCourseIdAsync(courseId);

        // Modules must not overlap
        // Overlap formula: (StartA <= EndB) && (EndA >= StartB)
        foreach (var existing in existingModules)
        {
            bool isOverlapping = dto.StartDate <= existing.EndDate && dto.EndDate >= existing.StartDate;
            if (isOverlapping)
            {
                return (false, $"Modulen överlappar med en befintlig modul: '{existing.Name}' ({existing.StartDate.ToShortDateString()} - {existing.EndDate.ToShortDateString()}).");
            }
        }

        var module = new Module
        {
            Name = dto.Name,
            Description = dto.Description,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            CourseId = courseId
        };

        await _unitOfWork.Modules.AddAsync(module);
        await _unitOfWork.CompleteAsync();

        return (true, string.Empty);
    }
}
