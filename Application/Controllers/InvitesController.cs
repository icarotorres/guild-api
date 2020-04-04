﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Application.ActionFilters;
using Business.Commands.Invites;
using Domain.Entities;
using Domain.Entities.Nulls;
using Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers
{
	[ApiController]
	[Route("api/[controller]/v1")]
	public class InvitesController : ControllerBase
	{
		private readonly IMediator _mediator;
		private readonly IInviteRepository _repository;

		public InvitesController(IInviteRepository repository, IMediator mediator)
		{
			_repository = repository;
			_mediator = mediator;
		}

		[HttpGet("{id}", Name = "get-invite")]
		[UseCache(20)]
		public async Task<IActionResult> GetAsync(Guid id)
		{
			var result = await _repository.GetByIdAsync(id, true);

			return result is NullInvite ? (IActionResult) NotFound() : Ok(result);
		}

		[HttpGet(Name = "get-invites")]
		[UseCache(30)]
		public async Task<IActionResult> GetAllAsync([FromQuery] InviteFilterCommand command)
		{
			var result = await _mediator.Send(command);

			return result.Errors.Any() ? (IActionResult) BadRequest(result.AsErrorOutput()) : Ok(result.Value);
		}

		[HttpPost(Name = "invite-member")]
		[UseUnitOfWork]
		public async Task<IActionResult> InviteMember([FromBody] InviteMemberCommand command)
		{
			var result = await _mediator.Send(command);

			return result.Errors.Any()
				? (IActionResult) BadRequest(result.AsErrorOutput())
				: CreatedAtAction(nameof(GetAsync), new {id = result.Value.Id}, result.Value);
		}

		[HttpPatch("{id}/accept", Name = "accept-invite")]
		[UseUnitOfWork]
		public async Task<IActionResult> AcceptAsync(Guid id)
		{
			var result = await _mediator.Send(new AcceptInviteCommand(id, _repository));

			return result.Errors.Any() ? (IActionResult) BadRequest(result.AsErrorOutput()) : Ok(result.Value);
		}

		[HttpPatch("{id}/decline", Name = "decline-invite")]
		[UseUnitOfWork]
		public async Task<IActionResult> DeclineAsync(Guid id)
		{
			var result = await _mediator.Send(new DeclineInviteCommand(id, _repository));

			return result.Errors.Any() ? (IActionResult) BadRequest(result.AsErrorOutput()) : Ok(result.Value);
		}

		[HttpPatch("{id}/cancel", Name = "cancel-invite")]
		[UseUnitOfWork]
		public async Task<IActionResult> CancelAsync(Guid id)
		{
			var result = await _mediator.Send(new CancelInviteCommand(id, _repository));

			return result.Errors.Any() ? (IActionResult) BadRequest(result.AsErrorOutput()) : Ok(result.Value);
		}
	}
}