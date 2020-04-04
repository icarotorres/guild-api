﻿using System.Threading;
using System.Threading.Tasks;
using Business.Commands.Invites;
using Business.ResponseOutputs;
using Domain.Entities;
using MediatR;

namespace Business.Handlers.Invites
{
	public class CancelInviteHandler : IPipelineBehavior<CancelInviteCommand, ApiResponse<Invite>>
	{
		public Task<ApiResponse<Invite>> Handle(CancelInviteCommand request,
			CancellationToken cancellationToken, RequestHandlerDelegate<ApiResponse<Invite>> next)
		{
			return Task.FromResult(new ApiResponse<Invite>(request.Invite.BeCanceled()));
		}
	}
}