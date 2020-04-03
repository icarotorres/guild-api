using System;
using Business.Commands.Members;
using Domain.Entities;
using Domain.Repositories;
using FluentValidation;

namespace Business.Validators.Requests.Members
{
  public class DemoteMemberValidator : AbstractValidator<DemoteMemberCommand>
  {
    public DemoteMemberValidator(IMemberRepository memberRepository, IGuildRepository guildRepository)
    {
      RuleFor(x => x.Id)
        .NotEmpty()
        .MustAsync(async (id, _) => await memberRepository.ExistsWithIdAsync(id))
        .WithMessage(x => CommonValidationMessages.ForRecordNotFound(nameof(Member), x.Id));

      RuleFor(x => x.Member.IsGuildMaster)
        .Equal(true)
        .WithMessage("Member are not a Guild Master and can not be demoted.");

      RuleFor(x => x.Member)
        .Must(x => x != null && x != new NullMember())
        .WithMessage("Member was null or empty.");

      RuleFor(x => x.Member.GuildId)
        .NotEmpty().WithMessage("Missing a guild key reference.");

      var guildNotEmptyMessage = "Members out of a guild can not be demoted.";
      RuleFor(x => x.Member.Guild)
        .NotEmpty().WithMessage(guildNotEmptyMessage)
        .NotEqual(new NullGuild()).WithMessage(guildNotEmptyMessage);
    }
  }
}