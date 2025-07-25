using FluentValidation;
using SharedKernel;
using Api.Infrastructure.Persistence;
using MediatR;
using Api.Domain.Entities;
using Api.Application.Abstractions;
using Api.Extensions;

namespace Api.Features.ResultQuotes;

public static class CreateQuote
{
    public record Command(string Author, string Content) : IRequest<Result<CreateQuoteResponse>>;
    public record CreateQuoteResponse(Quote quote);
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Author)
                .NotNull()
                .NotEmpty()
                .WithMessage("Author must be provided.");
            RuleFor(c => c.Content)
                .NotNull()
                .NotEmpty()
                .WithMessage("Content must be provided.");
        }
    }

    public class Handler(AppDbContext context) : IRequestHandler<Command, Result<CreateQuoteResponse>>
    {
        public async Task<Result<CreateQuoteResponse>> Handle(Command request, CancellationToken ct)
        {
            var quote = new Quote { Author = request.Author, Content = request.Content };
            await context.Quotes.AddAsync(quote, ct);
            await context.SaveChangesAsync(ct);

            return Result.Success(new CreateQuoteResponse(quote));

        }
    }

    public class CreateQuoteEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("results/quotes", async (ISender sender, Command createQuoteCommand, CancellationToken ct) =>
            {
                var result = await sender.Send(createQuoteCommand, ct);

                return result.Match(
                    Results.Ok,
                    CustomResults.Problem
                );
            })
            .WithTags("resultpattern");
        }
    }
}