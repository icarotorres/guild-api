using Application.Common.Results;
using Application.Members.Queries.GetMember;
using Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Tests.Domain.Models.Fakes;
using Tests.Domain.Models.Proxies;
using Tests.Helpers.Builders;
using Xunit;

namespace Tests.Application.Members.Queries.GetMember
{
    [Trait("Application", "Handler")]
    public class GetMemberHandlerTests
    {
        [Fact]
        public async Task Handle_Should_Succeed_With_ValidCommand()
        {
            // arrange
            var expectedMember = MemberFake.GuildLeader().Generate();
            var command = GetMemberCommandFake.Valid(expectedMember.Id).Generate();
            var repository = MemberRepositoryMockBuilder.Create()
                .GetByIdSuccess(input: command.Id, output: expectedMember).Build();
            var sut = new GetMemberHandler(repository);

            // act
            var result = await sut.Handle(command, default);

            // assert
            result.Should().NotBeNull().And.BeOfType<SuccessResult>();
            result.Success.Should().BeTrue();
            result.Errors.Should().BeEmpty();
            result.As<SuccessResult>().StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Data.Should().NotBeNull().And.BeOfType<MemberTestProxy>();
            result.Data.As<Member>().Id.Should().Be(expectedMember.Id);
            result.Data.As<Member>().GetGuild().Id.Should().Be(expectedMember.GuildId.Value);
        }
    }
}
