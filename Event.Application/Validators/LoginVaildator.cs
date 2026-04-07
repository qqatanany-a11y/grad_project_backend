using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Event.Application.Validators
{
    public class LoginVaildator : AbstractValidator<Dtos.LoginDto>
    {
        public LoginVaildator() {
        
          RuleFor(x => x.Email).NotEmpty().EmailAddress();
          RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        }
    }
}
