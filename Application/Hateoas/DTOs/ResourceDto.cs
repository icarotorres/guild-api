﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace Application.Hateoas.DTOs
{
	public abstract class ResourceDTO
	{
		public ResourceDTO(object data)
		{
			Data = data;
		}

		[JsonProperty("data", Order = 2)] public virtual object Data { get; }
		[JsonProperty("links", Order = -2)] public virtual List<LinkDto> Links { get; } = new List<LinkDto>();
	}
}