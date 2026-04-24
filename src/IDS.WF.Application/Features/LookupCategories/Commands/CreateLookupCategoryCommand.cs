using FluentValidation;
using MediatR;
using Rwd.WF.Application.Common.Interfaces;
using Rwd.WF.Application.DTOs;
using Rwd.WF.Application.Mappings;
using Rwd.WF.Domain.Common;
using Rwd.WF.Domain.Entities;
using Rwd.WF.Domain.Repositories;

namespace Rwd.WF.Application.Features.LookupCategories.Commands.Create;

public record CreateLookupCategoryCommand(
    string Code,
    string NameEn,
    string NameAr,
    string? Description,
    int DisplayOrder
) : IRequest<Result<LookupCategoryDto>>;

public sealed class CreateLookupCategoryCommandValidator : AbstractValidator<CreateLookupCategoryCommand>
{
    public CreateLookupCategoryCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(50)
            .Matches("^[A-Z0-9_]+$")
            .WithMessage("Code must be uppercase alphanumeric with underscores");
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateLookupCategoryCommandHandler(
    ILookupCategoryRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    ICacheService cache,
    IPublisher publisher)
    : IRequestHandler<CreateLookupCategoryCommand, Result<LookupCategoryDto>>
{
    public async Task<Result<LookupCategoryDto>> Handle(CreateLookupCategoryCommand request, CancellationToken ct)
    {
        if (await repository.ExistsByCodeAsync(request.Code, ct: ct))
            return Result<LookupCategoryDto>.Failure($"Category with code '{request.Code}' already exists.");

        var entity = LookupCategory.Create(
            request.Code,
            request.NameEn,
            request.NameAr,
            request.Description,
            request.DisplayOrder,
            currentUser.UserId);

        await repository.AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);

        foreach (var domainEvent in entity.DomainEvents)
            await publisher.Publish(domainEvent, ct);
        entity.ClearDomainEvents();

        await cache.RemoveByPrefixAsync("lookup_categories", ct);

        return Result<LookupCategoryDto>.Success(entity.ToDto());
    }
}

