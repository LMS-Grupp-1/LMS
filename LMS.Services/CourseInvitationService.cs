using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Shared.DTOs.CourseInvitationDtos;
using LMS.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace LMS.Services;

public class CourseInvitationService : ICourseInvitationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApplicationUser> _userManager;

    public CourseInvitationService(
        IUnitOfWork unitOfWork,
        IEmailSender emailSender,
        IConfiguration configuration,
        UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
        _configuration = configuration;
        _userManager = userManager;
    }

    public async Task InviteStudentAsync(string email, int courseId, CancellationToken cancellationToken = default)
    {
        var token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));

        var invitation = new CourseInvitation
        {
            Email = email,
            CourseId = courseId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsUsed = false
        };

        await _unitOfWork.CourseInvitations.AddAsync(invitation);
        await _unitOfWork.CompleteAsync();

        // Fetch configured Azure domain (test or production)
        var baseUrl = _configuration["AppUrl"] ?? "https://lms-web-prod.wonderfulsky-02f31f64.swedencentral.azurecontainerapps.io";
        var registerLink = $"{baseUrl}/register?email={Uri.EscapeDataString(email)}&token={token}";

        await _emailSender.SendEmailAsync(
            email,
            "Inbjudan till LMS Kurs",
            $"Klicka här för att registrera dig och slutföra ditt konto: <a href='{registerLink}'>Registrera konto</a>"
        );
    }

    public async Task RegisterInvitedStudentAsync(InvitedStudentRegisterDto dto, CancellationToken cancellationToken = default)
    {
        var invitation = await _unitOfWork.CourseInvitations.GetByEmailAndTokenAsync(dto.Email, dto.Token, cancellationToken);

        if (invitation == null)
        {
            throw new EntityNotFoundException("CourseInvitation", $"Email: {dto.Email}, Token: {dto.Token}");
        }
        if (invitation.IsUsed )
        {
            throw new BadRequestException("The invitation is used.");
        }
        if (invitation.ExpiresAt < DateTime.UtcNow)
        {
            throw new BadRequestException("The invitation is expired.");
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            CourseId = invitation.CourseId,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded) return createResult;

        await _userManager.AddToRoleAsync(user, UserRoles.Student);

        // Mark invitation as used
        invitation.IsUsed = true;
        _unitOfWork.CourseInvitations.Update(invitation);

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}