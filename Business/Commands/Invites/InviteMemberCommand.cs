﻿using System;
using Business.ResponseOutputs;
using Domain.Entities;
using MediatR;

namespace Business.Commands.Invites
{
	public class InviteMemberCommand : IRequest<ApiResponse<Invite>>
	{
		public Guid MemberId { get; set; }
		public Guid GuildId { get; set; }
	}
}