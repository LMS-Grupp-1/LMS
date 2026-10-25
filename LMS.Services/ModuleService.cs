using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Shared.DTOs.ModulesDtos;
using Service.Contracts;
using Module = Domain.Models.Entities.Module;

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
        var course = await _unitOfWork.Courses.GetByIdAsync(courseId,false);
        if (course == null)  
            throw new EntityNotFoundException("Kurs", courseId);

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

    public async Task CreateModuleAsync(int courseId, CreateModuleDto dto)
    {
        await ValidateModuleAsync(courseId, dto, null);

        var module = new Module
        {
            Name = dto.Name,
            Description = string.IsNullOrEmpty(dto.Description) ? "Ingen beskrivning" : dto.Description,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            CourseId = courseId
        };

        await _unitOfWork.Modules.AddAsync(module);
        await _unitOfWork.CompleteAsync();
    }

    public async Task UpdateModuleAsync(int courseId, int moduleId, CreateModuleDto dto)
    {
        // Check if the module to update actually exists
        var moduleToUpdate = await _unitOfWork.Modules.GetByIdAsync(moduleId);
        if (moduleToUpdate == null || moduleToUpdate.CourseId != courseId)
        {
            throw new EntityNotFoundException("Mudul", moduleId);
        }

        await ValidateModuleAsync(courseId, dto, moduleId);

        // Update entity properties
        moduleToUpdate.Name = dto.Name;
        moduleToUpdate.Description = string.IsNullOrEmpty(dto.Description) ? "Ingen beskrivning" : dto.Description;
        moduleToUpdate.StartDate = dto.StartDate;
        moduleToUpdate.EndDate = dto.EndDate;

        _unitOfWork.Modules.Update(moduleToUpdate);
        await _unitOfWork.CompleteAsync();
    }

    public async Task DeleteModuleAsync(int courseId, int moduleId)
    {
        var module = await _unitOfWork.Modules.GetByIdAsync(moduleId);
        if (module == null || module.CourseId != courseId)
        {
            throw new EntityNotFoundException("Mudul", moduleId);
        }

        _unitOfWork.Modules.Delete(module);
        await _unitOfWork.CompleteAsync();
    }

    private async Task ValidateModuleAsync(int courseId, CreateModuleDto dto, int? editModuleId)
    {
        if (dto.StartDate > dto.EndDate)
        {
            throw new BadRequestException("Modulens startdatum får inte vara efter slutdatum.");
        }

        var course = await _unitOfWork.Courses.GetByIdAsync(courseId, false);
        if (course == null)
        {
            throw new EntityNotFoundException("Kurs", courseId);
        }

        // Module dates must be within course dates
        if (dto.StartDate < course.StartDate || dto.EndDate > course.EndDate)
        {
            throw new BadRequestException($"Modulens datum måste ligga inom kursens tidsram ({course.StartDate.ToShortDateString()} - {course.EndDate.ToShortDateString()}).");
        }

        var existingModules = await _unitOfWork.Modules.GetModulesByCourseIdAsync(courseId);

        if (editModuleId is not null && editModuleId > 0)
        {
            existingModules = existingModules.Where(m => m.Id != editModuleId.Value);
        }

        // Modules must not overlap
        // Overlap formula: (StartA <= EndB) && (EndA >= StartB)
        foreach (var existing in existingModules)
        {
            bool isOverlapping = dto.StartDate <= existing.EndDate && dto.EndDate >= existing.StartDate;
            if (isOverlapping)
            {
                throw new BadRequestException($"Modulen överlappar med en befintlig modul: '{existing.Name}' ({existing.StartDate.ToShortDateString()} - {existing.EndDate.ToShortDateString()}).");
            }
        }
    }
}
