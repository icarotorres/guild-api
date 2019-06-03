﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using api.DTOs;
using api.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class GuildsController : ControllerBase
    {        
        // injected unit of work from startup.cs configure services
        private readonly IUnitOfWork _unitOfWork;
        public GuildsController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        [HttpPost] //DONE
        public ActionResult CreateGuild([FromBody] GuildForm payload)
        {
            if (!ModelState.IsValid)
                return BadRequest(ErrorMessageBuilder(string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors)
                                                                                          .Select(e => e.ErrorMessage))));
            try
            {
                var guild = _unitOfWork.Guilds.CreateGuild(payload.Id, payload.MasterId);
                if (payload.Members != null)
                {
                    foreach (var memberId in payload.Members)
                        guild.Members.Add(new User { Id = memberId });
                }
                _unitOfWork.Complete();
                return Created($"{Request.Path.ToUriComponent()}/{guild.Id}", guild);
            }
            catch (InvalidOperationException e) { return RollbackAndResult409(e); }
            catch (Exception e) { return RollbackAndResult500(e); }
        }
        
        [HttpGet("{id}")] //DONE
        public ActionResult GuildInfo(string id)
        {
            try
            {
                var guild =_unitOfWork.Guilds.Get(id);
                if (guild != null) return Ok(guild);
                else return NotFound(ErrorMessageBuilder($"Guild '{id}' not found"));
            }
            catch (Exception e) { return RollbackAndResult500(e); }
        }
        
        [HttpGet("list/{count:int=20}")] //DONE
        public ActionResult Guilds(int count)
        {
            try { return Ok(_unitOfWork.Guilds.GetNthGuilds(count)); }
            catch (Exception e) { return RollbackAndResult500(e); }
        }

        [HttpPut("{id}")] //DONE
        public ActionResult UpdateGuild(string id, [FromBody] GuildForm payload)
        {
            if (!ModelState.IsValid)
                return BadRequest(ErrorMessageBuilder(string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors)
                                                                                          .Select(e => e.ErrorMessage))));
            if (payload.Members != null && !payload.Members.Contains(payload.MasterId))
                return BadRequest(ErrorMessageBuilder($"Members must contain given MasterId value {payload.MasterId}"));

            try
            {
                var guild = _unitOfWork.Guilds.Get(id);
                if (guild != null)
                {
                    _unitOfWork.Guilds.Remove(guild);
                    _unitOfWork.Complete();

                    // re-mounting enity with new values
                    var updatedGuild = new Guild
                    {
                        Id = id,
                        MasterId = payload.MasterId,
                        Members = payload.Members?
                                         .Select(memberId => _unitOfWork.Users.Get(memberId)
                                                               ?? new User { Id = memberId }).ToHashSet()
                                                               ?? new HashSet<User>()
                    };

                    _unitOfWork.Guilds.Add(updatedGuild);
                    _unitOfWork.Complete();
                    return Ok(updatedGuild);
                }
                else return NotFound(ErrorMessageBuilder($"Guild '{id}' not found"));
            }
            catch (Exception e) { return RollbackAndResult500(e); }
        }
        
        [HttpPatch("{id}")] //DONE
        public ActionResult PatchGuild(string id, [FromBody] PatchGuildForm payload)
        {
            var messageSuffixs = new Dictionary<PatchAction, string> ()
            {
                { PatchAction.Add, $"to add member '{payload.userId}' in guild '{id}'" },
                { PatchAction.Remove, $"to remove member '{payload.userId}' in guild '{id}'" },
                { PatchAction.Transfer, $"to transfer '{id}' to member {payload.userId}" },
            };

            try
            {
                if (payload.Action == PatchAction.Add)
                    _unitOfWork.Guilds.AddMember(id, payload.userId);
                else if (payload.Action == PatchAction.Remove)
                    _unitOfWork.Guilds.RemoveMember(id, payload.userId);
                else
                    _unitOfWork.Guilds.Transfer(id, payload.userId);

                _unitOfWork.Complete();
                return Ok(true);
            }
            catch (ArgumentNullException e) { return RollbackAndResult404(e); }
            catch (InvalidOperationException e) { return RollbackAndResult409(e); }
            catch (Exception e) { return RollbackAndResult500(e); }
        }
        
        [HttpDelete("{id}")] // DONE
        public ActionResult DeleteGuild(string id)
        {
            try
            {
                var guild = _unitOfWork.Guilds.Get(id);
                if (guild != null)
                {
                    _unitOfWork.Guilds.Remove(guild);
                    _unitOfWork.Complete();
                    return NoContent();
                }
                else return NotFound(ErrorMessageBuilder($"Guild {id}' not Found"));
            }
            catch (Exception e) { return RollbackAndResult500(e); }
        }

        private string ErrorMessageBuilder(string Message = "") =>
            $"Fails on {Request.Method} " +
            $"to '{Request.Path.ToUriComponent()}'. " +
            $"Exception found: {Message}.";

        private ObjectResult RollbackAndResult404(Exception e)
        {
            _unitOfWork.Rollback();
            return NotFound(ErrorMessageBuilder(e.Message));
        }
        private ObjectResult RollbackAndResult409(Exception e)
        {
            _unitOfWork.Rollback();
            return Conflict(ErrorMessageBuilder(e.Message));
        }
        private ObjectResult RollbackAndResult500(Exception e)
        {
            _unitOfWork.Rollback();
            return StatusCode(StatusCodes.Status500InternalServerError, ErrorMessageBuilder(e.Message));
        }
    }
}
