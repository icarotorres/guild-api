using Application.Common.Results;
using Application.Invites.Commands.InviteMember;
using Domain.Enums;
using Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Tests.Domain.Models.Fakes;
using Tests.Domain.Models.Proxies;
using Tests.Helpers.Builders;
using Xunit;

namespace Tests.Application.Invites.Commands.InviteMember
{
    [Trait("Application", "Handler")]
    public class InviteMemberHandlerTests
    {
        [Fact]
        public async Task Handle_Should_Succeed_With_ValidCommandAsync()
        {
            // arrange
            var guild = GuildFake.Complete().Generate();
            var member = MemberFake.WithoutGuild().Generate();
            var command = InviteMemberCommandFake.Valid(guild.Id, member.Id).Generate();
            var expectedInvite = InviteFake.ValidWithStatus(InviteStatuses.Pending, guild, member).Generate();

            var unit = UnitOfWorkMockBuilder.Create()
                .SetupMembers(x => x.GetByIdSuccess(command.MemberId, member).Build())
                .SetupGuilds(x => x.GetByIdSuccess(command.GuildId, guild).Build())
                .SetupInvites(x => x.Insert(output: expectedInvite).Build())
                .Build();
            var factory = ModelFactoryMockBuilder.Create().CreateInvite(guild, member, expectedInvite).Build();
            var sut = new InviteMemberHandler(unit, factory);

            // act
            var result = await sut.Handle(command, default);

            // assert
            result.Should().NotBeNull().And.BeOfType<SuccessCreatedResult>();
            result.Success.Should().BeTrue();
            result.Errors.Should().BeEmpty();
            result.As<SuccessCreatedResult>().StatusCode.Should().Be(StatusCodes.Status201Created);
            result.Data.Should().NotBeNull().And.BeOfType<InviteTestProxy>();
            result.Data.As<Invite>().Should().Be(expectedInvite);
        }
    }
}
