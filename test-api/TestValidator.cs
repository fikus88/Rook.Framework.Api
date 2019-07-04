using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace testapi
{
    public class TestValidator : AbstractValidator<TestModel>
    {
	    public TestValidator()
	    {
		    RuleFor(m => m.Name).NotEmpty();
	    }
    }
}
