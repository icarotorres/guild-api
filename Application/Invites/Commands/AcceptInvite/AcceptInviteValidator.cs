using Application.Common.Abstractions;
using Domain.Common;
using Domain.Enums;
using Domain.Models;
using FluentValidation;
using System.Net;

namespace Application.Invites.Commands.AcceptInvite
{
    public class AcceptInviteValidator : AbstractValidator<AcceptInviteCommand>
    {
        public AcceptInviteValidator(IInviteRepository inviteRepository)
        {
            RuleFor(x => x.Id).NotEmpty().DependentRules(() =>
            {
                Invite invite = Invite.Null;

                RuleFor(x => x)
                    .MustAsync(async (x, ct) =>
                    {
                        invite = await inviteRepository.GetByIdAsync(x.Id, readOnly: true, ct);
                        return !(invite is INullObject);
                    })
                    .WithMessage(x => $"Record not found for invite with given id {x.Id}.")
                    .WithName("Id")
                    .WithErrorCode(nameof(HttpStatusCode.NotFound))

                    .Must(_ => invite.Status == InviteStatuses.Pending)
                    .WithMessage("Invite must be on pending status to be accepted.")
                    .WithName("Status")
                    .WithErrorCode(nameof(HttpStatusCode.UnprocessableEntity))

                    .Must(_ => !(invite.GetMember() is INullObject))
                    .WithMessage($"Record not found for invited member with given id {invite.MemberId}.")
                    .WithName("Member")
                    .WithErrorCode(nameof(HttpStatusCode.NotFound))

                    .Must(_ => !(invite.GetGuild() is INullObject))
                    .WithMessage($"Record not found for inviting guild with given id {invite.GuildId}.")
                    .WithName("Guild")
                    .WithErrorCode(nameof(HttpStatusCode.NotFound));
            });
        }
    }
}