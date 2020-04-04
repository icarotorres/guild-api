﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Application.ActionFilters;
using Business.Commands.Members;
using Domain.Entities.Nulls;
using Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers
{
	[ApiController]
	[Route("api/[controller]/v1")]
	public class MembersController : ControllerBase
	{
		private readonly IMediator _mediator;
		private readonly IMemberRepository _repository;

		public MembersController(IMemberRepository repository, IMediator mediator)
		{
			_repository = repository;
			_mediator = mediator;
		}

		[HttpGet("{id}", Name = "get-member")]
		[UseCache(10)]
		public async Task<IActionResult> GetAsync(Guid id)
		{
			var result = await _repository.GetByIdAsync(id, true);

			return result is NullMember ? (IActionResult) NotFound() : Ok(result);
		}

		[HttpGet(Name = "get-members")]
		[UseCache(10)]
		public async Task<IActionResult> GetAllAsync([FromQuery] MemberFilterCommand command)
		{
			var result = await _mediator.Send(command);

			return result.Errors.Any() ? (IActionResult) BadRequest(result.AsErrorOutput()) : Ok(result.Value);
		}

		[HttpPost(Name = "create-member")]
		[UseUnitOfWork]
		public async Task<IActionResult> CreateAsync([FromBody] CreateMemberCommand command)
		{
			var result = await _mediator.Send(command);

			return result.Errors.Any()
				? (IActionResult) BadRequest(result.AsErrorOutput())
				: CreatedAtAction(nameof(GetAsync), new {id = result.Value.Id}, result.Value);
		}

		[HttpPut("{id}", Name = "update-member")]
		[UseUnitOfWork]
		public async Task<IActionResult> UpdateAsync([FromBody] UpdateMemberCommand command)
		{
			var result = await _mediator.Send(command);

			return result.Errors.Any() ? (IActionResult) BadRequest(result.AsErrorOutput()) : Ok(result.Value);
		}

		[HttpPatch("{id}/promote", Name = "promote-member")]
		[UseUnitOfWork]
		public async Task<IActionResult> PromoteAsync(Guid id)
		{
			var result = await _mediator.Send(new PromoteMemberCommand(id, _repository));

			return result.Errors.Any() ? (IActionResult) BadRequest(result.AsErrorOutput()) : Ok(result.Value);
		}

		[HttpPatch("{id}/demote", Name = "demote-member")]
		[UseUnitOfWork]
		public async Task<IActionResult> DemoteAsync(Guid id)
		{
			var result = await _mediator.Send(new DemoteMemberCommand(id, _repository));

			return result.Errors.Any() ? (IActionResult) BadRequest(result.AsErrorOutput()) : Ok(result.Value);
		}

		[HttpPatch("{id}/leave", Name = "leave-guild")]
		[UseUnitOfWork]
		public async Task<IActionResult> LeaveGuildAsync(Guid id)
		{
			var result = await _mediator.Send(new LeaveGuildCommand(id, _repository));

			return result.Errors.Any() ? (IActionResult) BadRequest(result.AsErrorOutput()) : Ok(result.Value);
		}
	}
}