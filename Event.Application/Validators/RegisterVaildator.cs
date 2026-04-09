using Event.Application.Dtos;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Event.Application.Validators
{
    public class RegisterVaildator : AbstractValidator<RegisterDto>
    {
        public RegisterVaildator() {
        
            RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required.")
                .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

            RuleFor(x => x.LastName).NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");


            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.")
                .MaximumLength(200).WithMessage("Email cannot exceed 200 characters.");

            RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^\+?[1-10]\d{1,14}$").WithMessage("Invalid phone number format.")
                .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters.");

             RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 6 characters long.")
                .MaximumLength(100).WithMessage("Password cannot exceed 100 characters.");


        }
    }
}
