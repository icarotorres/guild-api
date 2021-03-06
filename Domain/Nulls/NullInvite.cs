using Domain.Common;
using Domain.Enums;
using Domain.Models;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Domain.Nulls
{
    [ExcludeFromCodeCoverage]
    public sealed class NullInvite : Invite, INullObject
    {
        public override Membership BeAccepted(IModelFactory factory)
        {
            return Membership.Null;
        }

        public override Invite BeDenied()
        {
            return this;
        }

        public override Invite BeCanceled()
        {
            return this;
        }

        public override Guid Id { get => Guid.Empty; protected internal set { } }
        public override InviteStatuses Status { get => InviteStatuses.Canceled; protected internal set { } }
        public override Guid? MemberId { get => null; protected internal set { } }
        public override Guid? GuildId { get => null; protected internal set { } }
        public override DateTime CreatedDate { get => default; protected internal set { } }
        public override DateTime? ModifiedDate { get => default; protected internal set { } }
        public override Guild GetGuild() => Guild.Null;
        public override Member GetMember() => Member.Null;
    }
}