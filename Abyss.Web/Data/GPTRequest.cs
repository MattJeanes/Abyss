using FluentValidation;
using MongoDB.Bson;

namespace Abyss.Web.Data;

public struct GPTRequest
{
    public string Text;

    public ObjectId ModelId;

    public decimal Temperature;

    public decimal TopP;
}

public class PersonValidator : AbstractValidator<GPTRequest>
{
    public PersonValidator()
    {
        RuleFor(x => x.ModelId).NotNull();
        RuleFor(x => x.Temperature).InclusiveBetween(0.1m, 1m);
        RuleFor(x => x.TopP).InclusiveBetween(0m, 1m);
    }
}
